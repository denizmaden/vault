using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Vault.Platform.Persistence.Mongo;
using Vault.Services.Wallet.Application.Abstractions.Caching;
using Vault.Services.Wallet.Application.Abstractions.Persistence;
using Vault.Services.Wallet.Infrastructure.Caching;
using Vault.Services.Wallet.Infrastructure.Configuration;
using Vault.Services.Wallet.Infrastructure.HealthChecks;
using Vault.Services.Wallet.Infrastructure.Messaging;
using Vault.Services.Wallet.Infrastructure.Persistence;
using Vault.Services.Wallet.Infrastructure.Persistence.Documents;

namespace Vault.Services.Wallet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddWalletInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var mongoSettings = new WalletMongoSettings
        {
            ConnectionString = configuration[$"{WalletMongoSettings.SectionName}:ConnectionString"]
                ?? throw new InvalidOperationException("Mongo connection string for Wallet is missing."),
            DatabaseName = configuration[$"{WalletMongoSettings.SectionName}:DatabaseName"]
                ?? throw new InvalidOperationException("Mongo database name for Wallet is missing."),
            WalletsCollectionName = configuration[$"{WalletMongoSettings.SectionName}:WalletsCollectionName"] ?? "wallets"
        };
        var messagingSettings = new WalletMessagingSettings
        {
            LedgerEventsDestination = configuration[$"{WalletMessagingSettings.SectionName}:LedgerEventsDestination"]
                ?? "vault.ledger.events",
            ConsumerGroupId = configuration[$"{WalletMessagingSettings.SectionName}:ConsumerGroupId"]
                ?? "vault-wallet-projection"
        };
        var cacheSettings = new WalletCacheSettings
        {
            WalletDetailsTtlSeconds = int.TryParse(
                configuration[$"{WalletCacheSettings.SectionName}:WalletDetailsTtlSeconds"],
                out var walletDetailsTtlSeconds)
                ? walletDetailsTtlSeconds
                : 300
        };

        services.AddSingleton(mongoSettings);
        services.AddSingleton(messagingSettings);
        services.AddSingleton(cacheSettings);
        services.AddVaultMongoClient(mongoSettings.ConnectionString);

        services.AddSingleton(serviceProvider =>
        {
            var client = serviceProvider.GetRequiredService<IMongoClient>();
            return client.GetDatabase(mongoSettings.DatabaseName);
        });
        services.AddSingleton(serviceProvider =>
        {
            var database = serviceProvider.GetRequiredService<IMongoDatabase>();
            var collection = database.GetCollection<WalletDocument>(mongoSettings.WalletsCollectionName);

            var ownerIndex = Builders<WalletDocument>.IndexKeys.Ascending(x => x.OwnerId);
            collection.Indexes.CreateOne(new CreateIndexModel<WalletDocument>(ownerIndex));

            return collection;
        });

        services.AddScoped<IWalletProjectionRepository, WalletProjectionRepository>();
        services.AddSingleton<IWalletProjectionCache, WalletProjectionCache>();
        services.AddSingleton<MongoWalletHealthCheck>();
        services.AddHostedService<LedgerEventsConsumer>();

        return services;
    }
}
