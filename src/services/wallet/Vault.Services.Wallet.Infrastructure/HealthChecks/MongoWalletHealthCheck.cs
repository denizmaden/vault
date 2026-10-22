using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Vault.Services.Wallet.Infrastructure.HealthChecks;

public sealed class MongoWalletHealthCheck(IMongoDatabase database) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await database.RunCommandAsync((Command<BsonDocument>)"{ ping: 1 }", cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("MongoDB health check failed.", exception);
        }
    }
}

