using Microsoft.EntityFrameworkCore;
using Vault.Services.Ledger.Application.Abstractions.Persistence;
using Vault.Services.Ledger.Application.Models;

namespace Vault.Services.Ledger.Infrastructure.Persistence;

public sealed class LedgerReadRepository(LedgerDbContext dbContext) : ILedgerReadRepository
{
    public async Task<LedgerAccountDetails?> GetAccountAsync(Guid ledgerAccountId, CancellationToken cancellationToken = default)
    {
        return await dbContext.LedgerAccounts
            .Where(x => x.LedgerAccountId == ledgerAccountId)
            .Select(x => new LedgerAccountDetails(
                x.LedgerAccountId,
                x.OwnerId,
                x.Currency,
                x.Balance,
                x.Status,
                x.Version,
                x.OpenedOnUtc))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<LedgerEventDetails>> GetEventsAsync(Guid ledgerAccountId, CancellationToken cancellationToken = default)
    {
        return await dbContext.LedgerEvents
            .Where(x => x.LedgerAccountId == ledgerAccountId)
            .OrderBy(x => x.Version)
            .Select(x => new LedgerEventDetails(
                x.Id,
                x.EventType,
                x.Version,
                x.OccurredOnUtc,
                x.Data))
            .ToArrayAsync(cancellationToken);
    }
}

