using Microsoft.Extensions.DependencyInjection;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Application.Execution;

namespace Vault.BuildingBlocks.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBuildingBlocksApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        return services;
    }
}
