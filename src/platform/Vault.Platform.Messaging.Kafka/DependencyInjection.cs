using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Vault.Platform.Messaging.Abstractions;
using Vault.Platform.Messaging.Kafka.Abstractions;
using Vault.Platform.Messaging.Kafka.Configuration;
using Vault.Platform.Messaging.Kafka.HealthChecks;

namespace Vault.Platform.Messaging.Kafka;

public static class DependencyInjection
{
    public static IServiceCollection AddVaultKafkaMessaging(
        this IServiceCollection services,
        string bootstrapServers,
        string clientId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bootstrapServers);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);

        services.AddSingleton(new KafkaProducerSettings
        {
            BootstrapServers = bootstrapServers,
            ClientId = clientId
        });

        services.AddSingleton<IProducer<string, string>>(_ =>
        {
            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                ClientId = clientId,
                AllowAutoCreateTopics = false,
                EnableIdempotence = true,
                Acks = Acks.All,
                MessageTimeoutMs = 10_000
            };

            return new ProducerBuilder<string, string>(config).Build();
        });

        services.AddSingleton<IMessagePublisher, KafkaMessagePublisher>();
        services.AddSingleton<IKafkaConsumerFactory, KafkaConsumerFactory>();
        services.AddHealthChecks()
            .AddCheck<KafkaHealthCheck>("messaging-kafka", tags: ["ready"]);

        return services;
    }
}
