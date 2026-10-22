namespace Vault.Platform.Messaging.Configuration;

public sealed class VaultMessagingOptions
{
    public const string SectionName = "Messaging";

    public string Provider { get; init; } = "Kafka";

    public VaultKafkaOptions Kafka { get; init; } = new();
}

public sealed class VaultKafkaOptions
{
    public string BootstrapServers { get; init; } = null!;
}
