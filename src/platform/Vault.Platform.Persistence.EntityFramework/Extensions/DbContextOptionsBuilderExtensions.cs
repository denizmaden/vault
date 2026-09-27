using Microsoft.EntityFrameworkCore;

namespace Vault.Platform.Persistence.EntityFramework.Extensions;

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseVaultSqlServer(
        this DbContextOptionsBuilder builder,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        builder.UseSqlServer(
            connectionString,
            sqlServer =>
            {
                sqlServer.EnableRetryOnFailure();
            });

        return builder;
    }
}
