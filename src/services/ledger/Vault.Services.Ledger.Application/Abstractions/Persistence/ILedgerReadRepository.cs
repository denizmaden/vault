using Vault.Services.Ledger.Application.Models;

namespace Vault.Services.Ledger.Application.Abstractions.Persistence;

public interface ILedgerReadRepository
{
    Task<LedgerAccountDetails?> GetAccountAsync(Guid ledgerAccountId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<LedgerEventDetails>> GetEventsAsync(Guid ledgerAccountId, CancellationToken cancellationToken = default);
}

