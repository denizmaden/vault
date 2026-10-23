namespace Vault.Services.MarketData.Infrastructure.Configuration;

public sealed class MarketDataMessagingSettings
{
    public const string SectionName = "MarketDataMessaging";

    public string PricesDestination { get; init; } = "vault.market-data.prices";

    public string DeadLetterDestination { get; init; } = "vault.market-data.prices.dlq";

    public string ProjectionConsumerGroupId { get; init; } = "vault-market-data-projection";

    public int MaxProcessingAttempts { get; init; } = 5;

    public int RetryInitialDelayMilliseconds { get; init; } = 200;

    public int DeadLetterPublishRetrySeconds { get; init; } = 2;

    public bool RebuildSnapshotWhenEmpty { get; init; } = true;

    public int RebuildTimeoutSeconds { get; init; } = 60;
}
