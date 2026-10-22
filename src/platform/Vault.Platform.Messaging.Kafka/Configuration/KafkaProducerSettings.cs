namespace Vault.Platform.Messaging.Kafka.Configuration;

public sealed class KafkaProducerSettings
{
    public string BootstrapServers { get; init; } = null!;

    public string ClientId { get; init; } = null!;
}

