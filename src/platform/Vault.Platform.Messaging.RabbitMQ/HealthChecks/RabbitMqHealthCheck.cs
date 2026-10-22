using Microsoft.Extensions.Diagnostics.HealthChecks;
using Vault.Platform.Messaging.RabbitMQ.Abstractions;

namespace Vault.Platform.Messaging.RabbitMQ.HealthChecks;

public sealed class RabbitMqHealthCheck(IRabbitMqConnectionProvider connectionProvider) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = connectionProvider.GetConnection();
            return Task.FromResult(connection.IsOpen
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("RabbitMQ connection is not open."));
        }
        catch (Exception exception)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ connection probe failed.", exception));
        }
    }
}
