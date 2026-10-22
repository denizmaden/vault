using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vault.Platform.Caching.Abstractions;
using Vault.Platform.Caching.Configuration;
using Vault.Platform.Caching.InMemory;
using Vault.Platform.Caching.Redis;
using Vault.Platform.Caching.Redis.Configuration;

namespace Vault.Platform.Caching;

public static class DependencyInjection
{
    public static IServiceCollection AddVaultCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(VaultCachingOptions.SectionName)
            .Get<VaultCachingOptions>()
            ?? new VaultCachingOptions();

        if (!options.Enabled)
        {
            services.AddSingleton<ICacheStore, NullCacheStore>();
            return services;
        }

        if (string.IsNullOrWhiteSpace(options.Provider))
        {
            throw new InvalidOperationException(
                "Caching provider is missing. Supported providers: Redis, InMemory.");
        }

        switch (options.Provider.Trim().ToUpperInvariant())
        {
            case "REDIS":
                services.AddVaultRedisCaching(new RedisCacheSettings
                {
                    ConnectionString = options.Redis.ConnectionString,
                    InstanceName = options.Redis.InstanceName
                });
                break;

            case "INMEMORY":
                services.AddVaultInMemoryCaching();
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported caching provider '{options.Provider}'. Supported providers: Redis, InMemory.");
        }

        return services;
    }
}
