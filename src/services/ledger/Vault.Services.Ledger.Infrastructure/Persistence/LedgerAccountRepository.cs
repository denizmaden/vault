using Microsoft.EntityFrameworkCore;
using Vault.Services.Ledger.Application.Abstractions.Persistence;
using Vault.Services.Ledger.Domain.Entities;
using Vault.Services.Ledger.Infrastructure.Persistence.Documents;

namespace Vault.Services.Ledger.Infrastructure.Persistence;

public sealed class LedgerAccountRepository(LedgerDbContext dbContext) : ILedgerAccountRepository
{
    public Task<bool> ExistsAsync(Guid ledgerAccountId, CancellationToken cancellationToken = default)
    {
        return dbContext.LedgerAccounts.AnyAsync(x => x.LedgerAccountId == ledgerAccountId, cancellationToken);
    }

    public async Task<LedgerAccount?> LoadAsync(Guid ledgerAccountId, CancellationToken cancellationToken = default)
    {
        var records = await dbContext.LedgerEvents
            .Where(x => x.LedgerAccountId == ledgerAccountId)
            .OrderBy(x => x.Version)
            .ToListAsync(cancellationToken);

        if (records.Count == 0)
        {
            return null;
        }

        var events = records
            .Select(record => LedgerEventSerializer.Deserialize(record.EventType, record.Data))
            .ToArray();

        var rehydrated = LedgerAccount.Rehydrate(ledgerAccountId, events);

        if (rehydrated.IsFailure)
        {
            throw new InvalidOperationException(rehydrated.Error.Description);
        }

        return rehydrated.Value;
    }

    public async Task SaveAsync(LedgerAccount ledgerAccount, CancellationToken cancellationToken = default)
    {
        var pendingEvents = ledgerAccount.GetUncommittedEvents().ToArray();

        if (pendingEvents.Length == 0)
        {
            return;
        }

        var persistedVersion = ledgerAccount.Version - pendingEvents.Length;
        var projection = await dbContext.LedgerAccounts.SingleOrDefaultAsync(x => x.LedgerAccountId == ledgerAccount.Id, cancellationToken);

        for (var index = 0; index < pendingEvents.Length; index++)
        {
            var ledgerEvent = pendingEvents[index];
            var version = persistedVersion + index + 1;
            var payload = LedgerEventSerializer.Serialize(ledgerEvent);
            var eventType = LedgerEventSerializer.GetEventType(ledgerEvent);

            dbContext.LedgerEvents.Add(new LedgerEventRecord
            {
                Id = Guid.NewGuid(),
                LedgerAccountId = ledgerAccount.Id,
                Version = version,
                EventType = eventType,
                Data = payload,
                OccurredOnUtc = ledgerEvent.OccurredOnUtc
            });

            dbContext.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = eventType,
                Payload = payload,
                OccurredOnUtc = ledgerEvent.OccurredOnUtc
            });
        }

        if (projection is null)
        {
            dbContext.LedgerAccounts.Add(new LedgerAccountProjection
            {
                LedgerAccountId = ledgerAccount.Id,
                OwnerId = ledgerAccount.OwnerId,
                Currency = ledgerAccount.Currency.Code,
                Balance = ledgerAccount.Balance,
                Status = ledgerAccount.Status.ToString(),
                Version = ledgerAccount.Version,
                OpenedOnUtc = ledgerAccount.OpenedOnUtc,
                LastUpdatedOnUtc = pendingEvents[^1].OccurredOnUtc
            });
        }
        else
        {
            projection.OwnerId = ledgerAccount.OwnerId;
            projection.Currency = ledgerAccount.Currency.Code;
            projection.Balance = ledgerAccount.Balance;
            projection.Status = ledgerAccount.Status.ToString();
            projection.Version = ledgerAccount.Version;
            projection.LastUpdatedOnUtc = pendingEvents[^1].OccurredOnUtc;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        ledgerAccount.ClearUncommittedEvents();
    }
}

