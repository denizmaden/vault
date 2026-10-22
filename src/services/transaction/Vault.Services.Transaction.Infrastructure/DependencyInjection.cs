using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vault.Platform.Persistence.EntityFramework.Extensions;
using Vault.Services.Transaction.Application.Abstractions.Persistence;
using Vault.Services.Transaction.Infrastructure.Configuration;
using Vault.Services.Transaction.Infrastructure.Messaging;
using Vault.Services.Transaction.Infrastructure.Persistence;

namespace Vault.Services.Transaction.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTransactionInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TransactionDatabase")
            ?? throw new InvalidOperationException("Connection string 'TransactionDatabase' is not configured.");
        var messagingSettings = new TransactionMessagingSettings
        {
            RequestedQueue = configuration[$"{TransactionMessagingSettings.SectionName}:RequestedQueue"]
                ?? "vault.transaction.transfer.requested",
            ReservedQueue = configuration[$"{TransactionMessagingSettings.SectionName}:ReservedQueue"]
                ?? "vault.transaction.transfer.funds-reserved",
            CompletedQueue = configuration[$"{TransactionMessagingSettings.SectionName}:CompletedQueue"]
                ?? "vault.transaction.transfer.completed",
            FailedQueue = configuration[$"{TransactionMessagingSettings.SectionName}:FailedQueue"]
                ?? "vault.transaction.transfer.failed"
        };

        services.AddDbContext<TransactionDbContext>(options => options.UseVaultSqlServer(connectionString));
        services.AddSingleton(messagingSettings);
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransactionReadRepository, TransactionReadRepository>();
        services.AddHostedService<TransactionOutboxDispatcher>();
        services.AddHostedService<TransferRequestedSagaConsumer>();
        services.AddHostedService<FundsReservedSagaConsumer>();

        return services;
    }
}
