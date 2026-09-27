using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Vault.Platform.Persistence.EntityFramework.Extensions;
using Vault.Services.Identity.Application.Abstractions.Identity;
using Vault.Services.Identity.Infrastructure.Identity;
using Vault.Services.Identity.Infrastructure.Persistence;

namespace Vault.Services.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IdentityDatabase")
            ?? throw new InvalidOperationException("Connection string 'IdentityDatabase' is not configured.");

        services.AddDbContext<IdentityDbContext>(options => options.UseVaultSqlServer(connectionString));
        services
            .AddIdentity<VaultIdentityUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IIdentityUserRegistrationService, IdentityRegistrationService>();
        services.AddScoped<IIdentityUserManagementService, IdentityUserManagementService>();

        return services;
    }
}
