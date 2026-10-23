using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Vault.Services.MarketData.Infrastructure.HealthChecks;

public sealed class RedisMarketDataHealthCheck(IConnectionMultiplexer connectionMultiplexer) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var latency = await connectionMultiplexer.GetDatabase().PingAsync();
            return HealthCheckResult.Healthy($"Redis responded in {latency.TotalMilliseconds:0.##} ms.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Redis market data store is unavailable.", exception);
        }
    }
}
