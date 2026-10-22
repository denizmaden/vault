using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vault.Platform.Persistence.EntityFramework.Extensions;
using Vault.Services.Ledger.Application.Abstractions.Persistence;
using Vault.Services.Ledger.Infrastructure.Configuration;
using Vault.Services.Ledger.Infrastructure.Messaging;
using Vault.Services.Ledger.Infrastructure.Persistence;

namespace Vault.Services.Ledger.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLedgerInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LedgerDatabase")
            ?? throw new InvalidOperationException("Connection string 'LedgerDatabase' is not configured.");

        var messagingSettings = new LedgerMessagingSettings
        {
            EventsDestination = configuration[$"{LedgerMessagingSettings.SectionName}:EventsDestination"]
                ?? "vault.ledger.events"
        };

        services.AddDbContext<LedgerDbContext>(options => options.UseVaultSqlServer(connectionString));
        services.AddSingleton(messagingSettings);
        services.AddScoped<ILedgerAccountRepository, LedgerAccountRepository>();
        services.AddScoped<ILedgerReadRepository, LedgerReadRepository>();
        services.AddHostedService<LedgerOutboxDispatcher>();

        return services;
    }
}
