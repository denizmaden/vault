using RabbitMQ.Client;
using Vault.Platform.Messaging.RabbitMQ.Abstractions;
using Vault.Platform.Messaging.RabbitMQ.Configuration;

namespace Vault.Platform.Messaging.RabbitMQ;

public sealed class RabbitMqConnectionProvider : IRabbitMqConnectionProvider, IDisposable
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private readonly object _lock = new();

    public RabbitMqConnectionProvider(RabbitMqConnectionSettings settings)
    {
        _factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password,
            VirtualHost = settings.VirtualHost,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
        };
    }

    public IConnection GetConnection()
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        lock (_lock)
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            _connection?.Dispose();
            _connection = _factory.CreateConnection();
            return _connection;
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
