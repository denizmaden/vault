using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Vault.Services.Identity.Infrastructure.Persistence.Extensions;

public static class IdentityDatabaseExtensions
{
    public static async Task MigrateIdentityDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var persistedGrantDbContext = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
        await dbContext.Database.MigrateAsync();
        await persistedGrantDbContext.Database.MigrateAsync();
    }
}
