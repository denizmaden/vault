using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Vault.Platform.Persistence.Mongo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVaultMongoClient(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));

        return services;
    }
}
