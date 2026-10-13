using Microsoft.EntityFrameworkCore;
using Vault.Services.Customer.Domain.Entities;

namespace Vault.Services.Customer.Infrastructure.Persistence;

public sealed class CustomerDbContext(DbContextOptions<CustomerDbContext> options) : DbContext(options)
{
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomerDbContext).Assembly);
    }
}
