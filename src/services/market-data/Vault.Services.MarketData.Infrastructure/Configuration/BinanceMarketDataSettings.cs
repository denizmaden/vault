namespace Vault.Services.MarketData.Infrastructure.Configuration;

public sealed class BinanceMarketDataSettings
{
    public const string SectionName = "Binance";

    public bool Enabled { get; init; } = true;

    public string WebSocketUrl { get; init; } = "wss://stream.binance.com:9443/stream";

    public int ReconnectDelaySeconds { get; init; } = 5;

    public IReadOnlyCollection<BinanceInstrumentSettings> Instruments { get; init; } = [];
}
