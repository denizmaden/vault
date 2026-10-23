namespace Vault.Services.MarketData.Infrastructure.Configuration;

public sealed class MarketDataIngestionSettings
{
    public const string SectionName = "MarketDataIngestion";

    public bool Enabled { get; init; } = true;
}
