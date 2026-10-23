using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vault.Services.MarketData.Application.Abstractions;
using Vault.Services.MarketData.Domain.Entities;
using Vault.Services.MarketData.Domain.Enums;
using Vault.Services.MarketData.Domain.ValueObjects;
using Vault.Services.MarketData.Infrastructure.Configuration;

namespace Vault.Services.MarketData.Infrastructure.Streaming;

public sealed class BinanceCryptoWebSocketClient(
    BinanceMarketDataSettings settings,
    IMarketPricePublisher publisher,
    ILogger<BinanceCryptoWebSocketClient> logger) : BackgroundService
{
    private readonly IReadOnlyDictionary<string, MarketInstrument> _instruments = settings.Instruments
        .Select(CreateInstrument)
        .Where(instrument => instrument is not null)
        .ToDictionary(instrument => instrument!.ProviderSymbol, instrument => instrument!, StringComparer.OrdinalIgnoreCase);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!settings.Enabled)
        {
            logger.LogInformation("Binance crypto stream is disabled.");
            return;
        }

        if (_instruments.Count == 0)
        {
            logger.LogWarning("Binance crypto stream has no valid instruments configured.");
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
                logger.LogWarning(exception, "Binance crypto WebSocket connection failed.");
            }

            await DelayBeforeReconnectAsync(stoppingToken);
        }
    }

    private async Task RunConnectionAsync(CancellationToken cancellationToken)
    {
        using var webSocket = new ClientWebSocket();
        var streamPath = string.Join("/", settings.Instruments.Select(x => x.Stream.Trim().ToLowerInvariant()));
        var uri = new Uri($"{settings.WebSocketUrl}?streams={streamPath}");

        logger.LogInformation("Connecting to Binance crypto stream for {InstrumentCount} instruments.", _instruments.Count);
        await webSocket.ConnectAsync(uri, cancellationToken);

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

    private async Task HandleMessageAsync(string message, CancellationToken cancellationToken)
    {
        using var document = JsonDocument.Parse(message);
        var root = document.RootElement;
        var payload = root.TryGetProperty("data", out var dataElement) ? dataElement : root;

        if (!payload.TryGetProperty("s", out var symbolElement)
            || string.IsNullOrWhiteSpace(symbolElement.GetString())
            || !_instruments.TryGetValue(symbolElement.GetString()!, out var instrument)
            || !payload.TryGetProperty("c", out var priceElement)
            || !priceElement.TryGetDecimalValue(out var price))
        {
            return;
        }

        decimal? changePercent = null;
        if (payload.TryGetProperty("P", out var changeElement)
            && changeElement.TryGetDecimalValue(out var parsedChangePercent))
        {
            changePercent = parsedChangePercent;
        }

        var occurredOnUtc = payload.TryGetProperty("E", out var timestampElement)
            ? timestampElement.ReadUnixMillisecondsAsUtc()
            : DateTime.UtcNow;

        var marketPrice = MarketPrice.Create(
            instrument,
            price,
            changePercent,
            MarketPriceSource.Binance,
            occurredOnUtc,
            DateTime.UtcNow);

        if (marketPrice.IsSuccess)
        {
            await publisher.PublishAsync(marketPrice.Value, cancellationToken);
        }
    }

    private async Task DelayBeforeReconnectAsync(CancellationToken cancellationToken)
    {
        var delay = TimeSpan.FromSeconds(settings.ReconnectDelaySeconds > 0 ? settings.ReconnectDelaySeconds : 5);
        await Task.Delay(delay, cancellationToken);
    }

    private static MarketInstrument? CreateInstrument(BinanceInstrumentSettings instrument)
    {
        var result = MarketInstrument.Create(
            instrument.Symbol,
            instrument.DisplayName,
            instrument.BaseAsset,
            instrument.QuoteAsset,
            MarketInstrumentKind.Crypto,
            instrument.ProviderSymbol);

        return result.IsSuccess ? result.Value : null;
    }
}
