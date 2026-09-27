using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Vault.Services.Identity.Infrastructure.Persistence.DesignTime;

public sealed class PersistedGrantDbContextFactory : IDesignTimeDbContextFactory<PersistedGrantDbContext>
{
    public PersistedGrantDbContext CreateDbContext(string[] args)
    {
        const string connectionString =
            "Server=localhost,14333;Database=VaultIdentity;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<PersistedGrantDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            sql => sql.MigrationsAssembly(typeof(IdentityDbContext).Assembly.GetName().Name));

        return new PersistedGrantDbContext(optionsBuilder.Options);
    }
}
