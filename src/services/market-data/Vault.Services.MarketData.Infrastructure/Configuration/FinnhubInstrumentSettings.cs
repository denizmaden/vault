namespace Vault.Services.MarketData.Infrastructure.Configuration;

public sealed class FinnhubInstrumentSettings
{
    public string ProviderSymbol { get; init; } = null!;

    public string Symbol { get; init; } = null!;

    public string DisplayName { get; init; } = null!;

    public string BaseAsset { get; init; } = null!;

    public string QuoteAsset { get; init; } = null!;
}
