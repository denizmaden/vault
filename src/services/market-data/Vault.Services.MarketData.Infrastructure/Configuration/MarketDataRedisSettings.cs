namespace Vault.Services.MarketData.Infrastructure.Configuration;

public sealed class MarketDataRedisSettings
{
    public const string SectionName = "MarketDataRedis";

    public string ConnectionString { get; init; } = "localhost:6381,abortConnect=false";

    public string SnapshotHashKey { get; init; } = "vault:market-data:{prices:v1}:values";

    public string SnapshotTimestampHashKey { get; init; } = "vault:market-data:{prices:v1}:timestamps";

    public string RealtimeChannel { get; init; } = "vault:market-data:realtime:v1";
}
