namespace Vault.Services.MarketData.Infrastructure.Configuration;

public sealed class FinnhubMarketDataSettings
{
    public const string SectionName = "Finnhub";

    public bool Enabled { get; init; } = true;

    public string? ApiKey { get; init; }

    public string WebSocketUrl { get; init; } = "wss://ws.finnhub.io";

    public int ReconnectDelaySeconds { get; init; } = 5;

    public IReadOnlyCollection<FinnhubInstrumentSettings> Instruments { get; init; } = [];
}
