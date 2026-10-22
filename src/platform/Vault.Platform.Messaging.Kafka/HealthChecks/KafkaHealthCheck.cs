using Confluent.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Vault.Platform.Messaging.Kafka.Configuration;

namespace Vault.Platform.Messaging.Kafka.HealthChecks;

public sealed class KafkaHealthCheck(KafkaProducerSettings settings) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var adminClient = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = settings.BootstrapServers,
                ClientId = $"{settings.ClientId}-health"
            }).Build();

            adminClient.GetMetadata(TimeSpan.FromSeconds(5));
            return Task.FromResult(HealthCheckResult.Healthy());
        }
        catch (Exception exception)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Kafka metadata probe failed.", exception));
        }
    }
}

