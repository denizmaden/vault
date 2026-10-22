using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Vault.Platform.Persistence.EntityFramework.Contexts;
using Vault.Services.Transaction.Domain.Entities;
using Vault.Services.Transaction.Infrastructure.Persistence.Documents;

namespace Vault.Services.Transaction.Infrastructure.Persistence;

public sealed class TransactionDbContext(DbContextOptions<TransactionDbContext> options) : VaultDbContext(options)
{
    public DbSet<TransactionRecord> Transactions => Set<TransactionRecord>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransactionDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddOutboxMessagesFromDomainEvents();
        var result = await base.SaveChangesAsync(cancellationToken);
        ClearDomainEvents();
        return result;
    }

    private void AddOutboxMessagesFromDomainEvents()
    {
        var transactions = ChangeTracker.Entries<TransactionRecord>()
            .Select(entry => entry.Entity)
            .ToArray();

        foreach (var transaction in transactions)
        {
            foreach (var domainEvent in transaction.GetDomainEvents())
            {
                OutboxMessages.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = domainEvent.GetType().Name,
                    Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    OccurredOnUtc = domainEvent.OccurredOnUtc
                });
            }
        }
    }

    private void ClearDomainEvents()
    {
        foreach (var transaction in ChangeTracker.Entries<TransactionRecord>().Select(entry => entry.Entity))
        {
            transaction.ClearDomainEvents();
        }
    }
}
