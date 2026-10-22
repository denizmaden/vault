using Microsoft.EntityFrameworkCore;
using Vault.Platform.Persistence.EntityFramework.Contexts;
using Vault.Services.Ledger.Infrastructure.Persistence.Documents;

namespace Vault.Services.Ledger.Infrastructure.Persistence;

public sealed class LedgerDbContext(DbContextOptions<LedgerDbContext> options) : VaultDbContext(options)
{
    public DbSet<LedgerEventRecord> LedgerEvents => Set<LedgerEventRecord>();

    public DbSet<LedgerAccountProjection> LedgerAccounts => Set<LedgerAccountProjection>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LedgerDbContext).Assembly);
    }
}

