namespace Vault.Platform.Messaging.RabbitMQ.Configuration;

public sealed class RabbitMqConnectionSettings
{
    public string HostName { get; init; } = null!;

    public int Port { get; init; } = 5672;

    public string UserName { get; init; } = "guest";

    public string Password { get; init; } = "guest";

    public string VirtualHost { get; init; } = "/";
}
