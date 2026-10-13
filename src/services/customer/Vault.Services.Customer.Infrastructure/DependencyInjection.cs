using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vault.Platform.Persistence.EntityFramework.Extensions;
using Vault.Services.Customer.Application.Abstractions.Persistence;
using Vault.Services.Customer.Infrastructure.Persistence;

namespace Vault.Services.Customer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomerInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CustomerDatabase")
            ?? throw new InvalidOperationException("Connection string 'CustomerDatabase' is not configured.");

        services.AddDbContext<CustomerDbContext>(options => options.UseVaultSqlServer(connectionString));
        services.AddScoped<ICustomerProfileRepository, CustomerProfileRepository>();

        return services;
    }
}
