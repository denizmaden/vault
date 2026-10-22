using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vault.Platform.Messaging.Configuration;
using Vault.Platform.Messaging.Kafka;
using Vault.Platform.Messaging.RabbitMQ;
using Vault.Platform.Messaging.RabbitMQ.Configuration;

namespace Vault.Platform.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddVaultMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        string clientId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);

        var options = configuration
            .GetSection(VaultMessagingOptions.SectionName)
            .Get<VaultMessagingOptions>()
            ?? new VaultMessagingOptions();

        if (string.IsNullOrWhiteSpace(options.Provider))
        {
            throw new InvalidOperationException(
                "Messaging provider is missing. Supported providers: Kafka, RabbitMQ.");
        }

        switch (options.Provider.Trim().ToUpperInvariant())
        {
            case "KAFKA":
                if (string.IsNullOrWhiteSpace(options.Kafka.BootstrapServers))
                {
                    throw new InvalidOperationException(
                        "Messaging:Kafka:BootstrapServers must be configured for the Kafka provider.");
                }

                services.AddVaultKafkaMessaging(options.Kafka.BootstrapServers, clientId);
                break;

            case "RABBITMQ":
                if (string.IsNullOrWhiteSpace(options.RabbitMq.HostName))
                {
                    throw new InvalidOperationException(
                        "Messaging:RabbitMq:HostName must be configured for the RabbitMQ provider.");
                }

                services.AddVaultRabbitMqMessaging(new RabbitMqConnectionSettings
                {
                    HostName = options.RabbitMq.HostName,
                    Port = options.RabbitMq.Port,
                    UserName = options.RabbitMq.UserName,
                    Password = options.RabbitMq.Password,
                    VirtualHost = options.RabbitMq.VirtualHost
                });
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported messaging provider '{options.Provider}'. Supported providers: Kafka, RabbitMQ.");
        }

        return services;
    }
}
