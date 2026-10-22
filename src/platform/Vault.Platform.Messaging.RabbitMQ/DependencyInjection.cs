using Microsoft.Extensions.DependencyInjection;
using Vault.Platform.Messaging.Abstractions;
using Vault.Platform.Messaging.RabbitMQ.Abstractions;
using Vault.Platform.Messaging.RabbitMQ.Configuration;
using Vault.Platform.Messaging.RabbitMQ.HealthChecks;

namespace Vault.Platform.Messaging.RabbitMQ;

public static class DependencyInjection
{
    public static IServiceCollection AddVaultRabbitMqMessaging(
        this IServiceCollection services,
        RabbitMqConnectionSettings settings)
    {
        services.AddSingleton(settings);
        services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
        services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();
        services.AddSingleton<IMessageSubscriber, RabbitMqMessageSubscriber>();
        services.AddHealthChecks()
            .AddCheck<RabbitMqHealthCheck>("messaging-rabbitmq", tags: ["ready"]);

        return services;
    }
}
