using Microsoft.Extensions.DependencyInjection;
using Vault.Platform.Caching.Abstractions;

namespace Vault.Platform.Caching.InMemory;

public static class DependencyInjection
{
    public static IServiceCollection AddVaultInMemoryCaching(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheStore, InMemoryCacheStore>();
        return services;
    }
}
