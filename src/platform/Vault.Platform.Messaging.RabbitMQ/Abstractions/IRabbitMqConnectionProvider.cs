using RabbitMQ.Client;

namespace Vault.Platform.Messaging.RabbitMQ.Abstractions;

public interface IRabbitMqConnectionProvider
{
    IConnection GetConnection();
}
