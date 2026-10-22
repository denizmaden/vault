namespace Vault.Platform.Messaging.Configuration;

public sealed class VaultMessagingOptions
{
    public const string SectionName = "Messaging";

    public string Provider { get; init; } = "Kafka";

    public VaultKafkaOptions Kafka { get; init; } = new();

    public VaultRabbitMqOptions RabbitMq { get; init; } = new();
}

public sealed class VaultRabbitMqOptions
{
    public string HostName { get; init; } = null!;

    public int Port { get; init; } = 5672;

    public string UserName { get; init; } = "guest";

    public string Password { get; init; } = "guest";

    public string VirtualHost { get; init; } = "/";
}

public sealed class VaultKafkaOptions
{
    public string BootstrapServers { get; init; } = null!;
}
