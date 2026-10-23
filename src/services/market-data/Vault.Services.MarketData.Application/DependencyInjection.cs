using Microsoft.Extensions.DependencyInjection;

namespace Vault.Services.MarketData.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddMarketDataApplication(this IServiceCollection services)
    {
        return services;
    }
}
