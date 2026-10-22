using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Vault.Services.Transaction.Infrastructure.Persistence.Extensions;

public static class TransactionDatabaseExtensions
{
    public static async Task MigrateTransactionDatabaseAsync(this IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
