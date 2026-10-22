using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Vault.Platform.Persistence.Mongo;
using Vault.Services.Reporting.Application.Abstractions.Persistence;
using Vault.Services.Reporting.Infrastructure.Configuration;
using Vault.Services.Reporting.Infrastructure.HealthChecks;
using Vault.Services.Reporting.Infrastructure.Messaging;
using Vault.Services.Reporting.Infrastructure.Persistence;
using Vault.Services.Reporting.Infrastructure.Persistence.Documents;

namespace Vault.Services.Reporting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddReportingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var mongoSettings = new ReportingMongoSettings
        {
            ConnectionString = configuration[$"{ReportingMongoSettings.SectionName}:ConnectionString"]
                ?? throw new InvalidOperationException("Mongo connection string for Reporting is missing."),
            DatabaseName = configuration[$"{ReportingMongoSettings.SectionName}:DatabaseName"]
                ?? throw new InvalidOperationException("Mongo database name for Reporting is missing."),
            ReportsCollectionName = configuration[$"{ReportingMongoSettings.SectionName}:ReportsCollectionName"] ?? "wallet_activity_reports"
        };
        var messagingSettings = new ReportingMessagingSettings
        {
            LedgerEventsDestination = configuration[$"{ReportingMessagingSettings.SectionName}:LedgerEventsDestination"]
                ?? "vault.ledger.events",
            ConsumerGroupId = configuration[$"{ReportingMessagingSettings.SectionName}:ConsumerGroupId"]
                ?? "vault-reporting-projection"
        };

        services.AddSingleton(mongoSettings);
        services.AddSingleton(messagingSettings);
        services.AddVaultMongoClient(mongoSettings.ConnectionString);
        services.AddSingleton(serviceProvider =>
        {
            var client = serviceProvider.GetRequiredService<IMongoClient>();
            return client.GetDatabase(mongoSettings.DatabaseName);
        });
        services.AddSingleton(serviceProvider =>
        {
            var database = serviceProvider.GetRequiredService<IMongoDatabase>();
            var collection = database.GetCollection<WalletActivityReportDocument>(mongoSettings.ReportsCollectionName);

            var ownerIndex = Builders<WalletActivityReportDocument>.IndexKeys.Ascending(x => x.OwnerId);
            collection.Indexes.CreateOne(new CreateIndexModel<WalletActivityReportDocument>(ownerIndex));

            return collection;
        });

        services.AddScoped<IWalletActivityReportRepository, WalletActivityReportRepository>();
        services.AddSingleton<MongoReportingHealthCheck>();
        services.AddHostedService<LedgerEventsReportingConsumer>();

        return services;
    }
}
