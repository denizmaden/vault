using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace Vault.Services.Reporting.Infrastructure.HealthChecks;

public sealed class MongoReportingHealthCheck(IMongoDatabase database) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await database.RunCommandAsync((Command<MongoDB.Bson.BsonDocument>)"{ ping: 1 }", cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Reporting Mongo health check failed.", exception);
        }
    }
}
