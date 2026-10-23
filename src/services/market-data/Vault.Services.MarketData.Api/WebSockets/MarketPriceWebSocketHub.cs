using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Vault.Services.MarketData.Application.Abstractions;
using Vault.Services.MarketData.Application.Mapping;
using Vault.Services.MarketData.Application.Models;
using Vault.Services.MarketData.Domain.Entities;
using Vault.Services.MarketData.Api.Security;

namespace Vault.Services.MarketData.Api.WebSockets;

public sealed class MarketPriceWebSocketHub(
    IMarketPriceSnapshotStore snapshotStore,
    IMarketInstrumentCatalog instrumentCatalog,
    MarketDataWebSocketSettings settings,
    MarketDataSecuritySettings securitySettings,
    ILogger<MarketPriceWebSocketHub> logger) : IMarketPriceClientNotifier
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly ConcurrentDictionary<Guid, MarketPriceWebSocketConnection> _connections = new();
    private readonly ConcurrentDictionary<string, int> _connectionsPerClient = new(StringComparer.Ordinal);

    public async Task HandleAsync(HttpContext context, CancellationToken cancellationToken)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Expected a WebSocket request.", cancellationToken);
            return;
        }

        if (!IsOriginAllowed(context))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("The WebSocket origin is not allowed.", cancellationToken);
            return;
        }

        if (_connections.Count >= settings.MaxConnections)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsync("WebSocket connection capacity has been reached.", cancellationToken);
            return;
        }

        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var clientKey = GetClientKey(context);
        var clientConnectionCount = _connectionsPerClient.AddOrUpdate(clientKey, 1, (_, count) => count + 1);
        if (clientConnectionCount > settings.MaxConnectionsPerClient)
        {
            ReleaseClientConnection(clientKey);
            await CloseAsync(
                webSocket,
                WebSocketCloseStatus.PolicyViolation,
                "Connection quota exceeded",
                cancellationToken);
            webSocket.Dispose();
            return;
        }

        using var connection = new MarketPriceWebSocketConnection(
            Guid.NewGuid(),
            webSocket,
            settings.OutboundQueueCapacity,
            settings.CommandsPerMinute);

        if (!_connections.TryAdd(connection.Id, connection))
        {
            ReleaseClientConnection(clientKey);
            await CloseAsync(webSocket, WebSocketCloseStatus.InternalServerError, "Unable to register connection", cancellationToken);
            return;
        }

        MarketDataWebSocketTelemetry.ActiveConnections.Add(1);
        logger.LogInformation("Market data WebSocket client {ConnectionId} connected.", connection.Id);

        using var connectionLifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            QueueMessage(connection, "connected", new
            {
                connectionId = connection.Id,
                protocolVersion = 1,
                availableSymbols = instrumentCatalog.Instruments
                    .OrderBy(instrument => instrument.Kind)
                    .ThenBy(instrument => instrument.Symbol)
                    .Select(instrument => new
                    {
                        instrument.Symbol,
                        instrument.DisplayName,
                        instrument.BaseAsset,
                        instrument.QuoteAsset,
                        kind = instrument.Kind.ToString()
                    })
            });

            var sendTask = SendQueuedMessagesAsync(connection, connectionLifetime.Token);
            var receiveTask = ReceiveCommandsAsync(connection, connectionLifetime.Token);

            await Task.WhenAny(sendTask, receiveTask);
            connectionLifetime.Cancel();
            connection.Complete();
            await IgnoreExpectedCompletionAsync(sendTask);
            await IgnoreExpectedCompletionAsync(receiveTask);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogDebug(
                "Market data WebSocket client {ConnectionId} disconnected during request cancellation.",
                connection.Id);
        }
        catch (WebSocketException exception)
        {
            logger.LogDebug(
                exception,
                "Market data WebSocket client {ConnectionId} disconnected unexpectedly.",
                connection.Id);
        }
        finally
        {
            _connections.TryRemove(connection.Id, out _);
            ReleaseClientConnection(clientKey);
            MarketDataWebSocketTelemetry.ActiveConnections.Add(-1);
            using var closeTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await CloseAsync(
                webSocket,
                WebSocketCloseStatus.NormalClosure,
                "Connection closed",
                closeTimeout.Token);
            logger.LogInformation(
                "Market data WebSocket client {ConnectionId} disconnected with {DroppedMessageCount} dropped messages.",
                connection.Id,
                connection.DroppedMessageCount);
        }
    }

    public Task NotifyPriceUpdatedAsync(MarketPrice price, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var payload = JsonSerializer.Serialize(
            new MarketPriceWebSocketMessage<MarketPriceDto>(
                "price_updated",
                DateTime.UtcNow,
                price.ToDto()),
            JsonSerializerOptions);

        foreach (var connection in _connections.Values)
        {
            if (connection.IsSubscribedTo(price.Instrument.Symbol))
            {
                connection.TryQueue(payload);
            }
        }

        return Task.CompletedTask;
    }

    private async Task ReceiveCommandsAsync(
        MarketPriceWebSocketConnection connection,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested
               && connection.WebSocket.State == WebSocketState.Open)
        {
            var payload = await ReceiveTextMessageAsync(connection.WebSocket, cancellationToken);
            if (payload is null)
            {
                return;
            }

            if (!connection.TryConsumeCommand())
            {
                QueueError(connection, "rate_limit_exceeded", "The WebSocket command quota has been exceeded.");
                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
                await CloseAsync(
                    connection.WebSocket,
                    WebSocketCloseStatus.PolicyViolation,
                    "Command quota exceeded",
                    cancellationToken);
                return;
            }

            MarketPriceWebSocketCommand? command;
            try
            {
                command = JsonSerializer.Deserialize<MarketPriceWebSocketCommand>(payload, JsonSerializerOptions);
            }
            catch (JsonException)
            {
                QueueError(connection, "invalid_json", "The WebSocket command is not valid JSON.");
                continue;
            }

            if (command is null || string.IsNullOrWhiteSpace(command.Type))
            {
                QueueError(connection, "invalid_command", "A command type is required.");
                continue;
            }

            await HandleCommandAsync(connection, command, cancellationToken);
        }
    }

    private async Task HandleCommandAsync(
        MarketPriceWebSocketConnection connection,
        MarketPriceWebSocketCommand command,
        CancellationToken cancellationToken)
    {
        var requestedSymbols = (command.Symbols ?? [])
            .Where(symbol => !string.IsNullOrWhiteSpace(symbol))
            .Select(symbol => symbol.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var validSymbols = requestedSymbols
            .Where(instrumentCatalog.Contains)
            .ToArray();
        var rejectedSymbols = requestedSymbols
            .Except(validSymbols, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        switch (command.Type.Trim().ToLowerInvariant())
        {
            case "subscribe":
                if (validSymbols.Length == 0)
                {
                    QueueError(connection, "invalid_symbols", "At least one known symbol is required.");
                    return;
                }

                connection.Subscribe(validSymbols);
                QueueSubscriptionAcknowledgement(connection, rejectedSymbols);
                await QueueSnapshotAsync(connection, validSymbols, cancellationToken);
                break;

            case "set_subscriptions":
                connection.Unsubscribe(connection.Subscriptions);
                connection.Subscribe(validSymbols);
                QueueSubscriptionAcknowledgement(connection, rejectedSymbols);
                await QueueSnapshotAsync(connection, validSymbols, cancellationToken);
                break;

            case "unsubscribe":
                connection.Unsubscribe(validSymbols);
                QueueSubscriptionAcknowledgement(connection, rejectedSymbols);
                break;

            case "ping":
                QueueMessage(connection, "pong", new { serverTimeUtc = DateTime.UtcNow });
                break;

            default:
                QueueError(connection, "unsupported_command", $"Unsupported command type '{command.Type}'.");
                break;
        }
    }

    private async Task QueueSnapshotAsync(
        MarketPriceWebSocketConnection connection,
        IReadOnlyCollection<string> symbols,
        CancellationToken cancellationToken)
    {
        var prices = await snapshotStore.GetSnapshotAsync(symbols, cancellationToken);
        QueueMessage(
            connection,
            "snapshot",
            new MarketPriceSnapshotDto(DateTime.UtcNow, prices.Select(price => price.ToDto()).ToArray()));
    }

    private static void QueueSubscriptionAcknowledgement(
        MarketPriceWebSocketConnection connection,
        IReadOnlyCollection<string> rejectedSymbols)
    {
        QueueMessage(connection, "subscribed", new
        {
            symbols = connection.Subscriptions.OrderBy(symbol => symbol).ToArray(),
            rejectedSymbols
        });
    }

    private static void QueueError(
        MarketPriceWebSocketConnection connection,
        string code,
        string message) =>
        QueueMessage(connection, "error", new { code, message });

    private static void QueueMessage<TPayload>(
        MarketPriceWebSocketConnection connection,
        string type,
        TPayload payload)
    {
        var message = new MarketPriceWebSocketMessage<TPayload>(type, DateTime.UtcNow, payload);
        connection.TryQueue(JsonSerializer.Serialize(message, JsonSerializerOptions));
    }

    private static async Task SendQueuedMessagesAsync(
        MarketPriceWebSocketConnection connection,
        CancellationToken cancellationToken)
    {
        await foreach (var payload in connection.OutboundMessages.ReadAllAsync(cancellationToken))
        {
            if (connection.WebSocket.State != WebSocketState.Open)
            {
                return;
            }

            await connection.WebSocket.SendAsync(
                Encoding.UTF8.GetBytes(payload),
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken);
            MarketDataWebSocketTelemetry.SentMessages.Add(1);
        }
    }

    private async Task<string?> ReceiveTextMessageAsync(
        WebSocket webSocket,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        using var stream = new MemoryStream();

        while (true)
        {
            var result = await webSocket.ReceiveAsync(buffer, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                return null;
            }

            if (result.MessageType != WebSocketMessageType.Text)
            {
                await CloseAsync(webSocket, WebSocketCloseStatus.InvalidMessageType, "Text messages are required", cancellationToken);
                return null;
            }

            if (stream.Length + result.Count > settings.MaxIncomingMessageBytes)
            {
                await CloseAsync(webSocket, WebSocketCloseStatus.MessageTooBig, "Command is too large", cancellationToken);
                return null;
            }

            stream.Write(buffer, 0, result.Count);
            if (result.EndOfMessage)
            {
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }

    private static async Task IgnoreExpectedCompletionAsync(Task task)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
        }
        catch (WebSocketException)
        {
        }
    }

    private bool IsOriginAllowed(HttpContext context)
    {
        var origin = context.Request.Headers.Origin.ToString();
        if (string.IsNullOrWhiteSpace(origin))
        {
            return securitySettings.AllowConnectionsWithoutOrigin;
        }

        var requestOrigin = $"{context.Request.Scheme}://{context.Request.Host}";
        return string.Equals(origin, requestOrigin, StringComparison.OrdinalIgnoreCase)
            || securitySettings.AllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
    }

    private static string GetClientKey(HttpContext context) =>
        context.User.FindFirst("sub")?.Value
        ?? context.Connection.RemoteIpAddress?.ToString()
        ?? "unknown";

    private void ReleaseClientConnection(string clientKey)
    {
        while (_connectionsPerClient.TryGetValue(clientKey, out var current))
        {
            if (current <= 1)
            {
                if (_connectionsPerClient.TryRemove(
                        new KeyValuePair<string, int>(clientKey, current)))
                {
                    return;
                }
            }
            else if (_connectionsPerClient.TryUpdate(clientKey, current - 1, current))
            {
                return;
            }
        }
    }

    private static async Task CloseAsync(
        WebSocket webSocket,
        WebSocketCloseStatus closeStatus,
        string statusDescription,
        CancellationToken cancellationToken)
    {
        if (webSocket.State is not (WebSocketState.Open or WebSocketState.CloseReceived))
        {
            return;
        }

        try
        {
            await webSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);
        }
        catch (WebSocketException)
        {
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }
}
