using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Vault.Services.Customer.Infrastructure.Persistence.Extensions;

public static class CustomerDatabaseExtensions
{
    public static async Task MigrateCustomerDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
