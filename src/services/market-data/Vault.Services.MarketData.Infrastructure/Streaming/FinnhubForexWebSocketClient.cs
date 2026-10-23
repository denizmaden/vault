using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vault.Services.MarketData.Application.Abstractions;
using Vault.Services.MarketData.Domain.Entities;
using Vault.Services.MarketData.Domain.Enums;
using Vault.Services.MarketData.Domain.ValueObjects;
using Vault.Services.MarketData.Infrastructure.Configuration;

namespace Vault.Services.MarketData.Infrastructure.Streaming;

public sealed class FinnhubForexWebSocketClient(
    FinnhubMarketDataSettings settings,
    IMarketPricePublisher publisher,
    ILogger<FinnhubForexWebSocketClient> logger) : BackgroundService
{
    private readonly IReadOnlyDictionary<string, MarketInstrument> _instruments = settings.Instruments
        .Select(CreateInstrument)
        .Where(instrument => instrument is not null)
        .ToDictionary(instrument => instrument!.ProviderSymbol, instrument => instrument!, StringComparer.OrdinalIgnoreCase);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!settings.Enabled)
        {
            logger.LogInformation("Finnhub forex stream is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            logger.LogWarning("Finnhub API key is missing. Forex market data stream will not start.");
            return;
        }

        if (_instruments.Count == 0)
        {
            logger.LogWarning("Finnhub forex stream has no valid instruments configured.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunConnectionAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Finnhub forex WebSocket connection failed.");
            }

            await DelayBeforeReconnectAsync(stoppingToken);
        }
    }

    private async Task RunConnectionAsync(CancellationToken cancellationToken)
    {
        using var webSocket = new ClientWebSocket();
        var uri = new Uri($"{settings.WebSocketUrl}?token={Uri.EscapeDataString(settings.ApiKey!)}");

        logger.LogInformation("Connecting to Finnhub forex stream for {InstrumentCount} instruments.", _instruments.Count);
        await webSocket.ConnectAsync(uri, cancellationToken);

        foreach (var instrument in _instruments.Values)
        {
            await SubscribeAsync(webSocket, instrument.ProviderSymbol, cancellationToken);
        }

        while (!cancellationToken.IsCancellationRequested && webSocket.State == WebSocketState.Open)
        {
            var message = await WebSocketReceive.ReceiveTextAsync(webSocket, cancellationToken);
            if (message is null)
            {
                break;
            }

            await HandleMessageAsync(message, cancellationToken);
        }
    }

    private async Task SubscribeAsync(
        ClientWebSocket webSocket,
        string providerSymbol,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new
        {
            type = "subscribe",
            symbol = providerSymbol
        });

        await webSocket.SendAsync(
            Encoding.UTF8.GetBytes(payload),
            WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken);
    }

    private async Task HandleMessageAsync(string message, CancellationToken cancellationToken)
    {
        using var document = JsonDocument.Parse(message);
        var root = document.RootElement;

        if (!root.TryGetProperty("type", out var typeElement)
            || !string.Equals(typeElement.GetString(), "trade", StringComparison.OrdinalIgnoreCase)
            || !root.TryGetProperty("data", out var dataElement)
            || dataElement.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var trade in dataElement.EnumerateArray())
        {
            if (!trade.TryGetProperty("s", out var symbolElement)
                || string.IsNullOrWhiteSpace(symbolElement.GetString())
                || !trade.TryGetProperty("p", out var priceElement)
                || !priceElement.TryGetDecimalValue(out var price)
                || !_instruments.TryGetValue(symbolElement.GetString()!, out var instrument))
            {
                continue;
            }

            var occurredOnUtc = trade.TryGetProperty("t", out var timestampElement)
                ? timestampElement.ReadUnixMillisecondsAsUtc()
                : DateTime.UtcNow;

            var marketPrice = MarketPrice.Create(
                instrument,
                price,
                changePercent24h: null,
                MarketPriceSource.Finnhub,
                occurredOnUtc,
                DateTime.UtcNow);

            if (marketPrice.IsSuccess)
            {
                await publisher.PublishAsync(marketPrice.Value, cancellationToken);
            }
        }
    }

    private async Task DelayBeforeReconnectAsync(CancellationToken cancellationToken)
    {
        var delay = TimeSpan.FromSeconds(settings.ReconnectDelaySeconds > 0 ? settings.ReconnectDelaySeconds : 5);
        await Task.Delay(delay, cancellationToken);
    }

    private static MarketInstrument? CreateInstrument(FinnhubInstrumentSettings instrument)
    {
        var result = MarketInstrument.Create(
            instrument.Symbol,
            instrument.DisplayName,
            instrument.BaseAsset,
            instrument.QuoteAsset,
            MarketInstrumentKind.Forex,
            instrument.ProviderSymbol);

        return result.IsSuccess ? result.Value : null;
    }
}
