using Microsoft.Extensions.DependencyInjection;
using Vault.Platform.Caching.Abstractions;
using Vault.Platform.Caching.Redis.Configuration;

namespace Vault.Platform.Caching.Redis;

public static class DependencyInjection
{
    public static IServiceCollection AddVaultRedisCaching(
        this IServiceCollection services,
        RedisCacheSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            throw new InvalidOperationException("Redis connection string is missing.");
        }

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = settings.ConnectionString;
            options.InstanceName = settings.InstanceName;
        });
        services.AddSingleton<ICacheStore, RedisCacheStore>();

        return services;
    }
}
