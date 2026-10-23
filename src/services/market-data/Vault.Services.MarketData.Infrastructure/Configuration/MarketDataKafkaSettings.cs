namespace Vault.Services.MarketData.Infrastructure.Configuration;

public sealed class MarketDataKafkaSettings
{
    public string BootstrapServers { get; init; } = null!;
}
