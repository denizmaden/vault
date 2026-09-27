using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Vault.Platform.Persistence.EntityFramework.Extensions;

namespace Vault.Services.Identity.Infrastructure.Persistence.DesignTime;

public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        const string connectionString =
            "Server=localhost,14333;Database=VaultIdentity;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        optionsBuilder.UseVaultSqlServer(connectionString);

        return new IdentityDbContext(optionsBuilder.Options);
    }
}
