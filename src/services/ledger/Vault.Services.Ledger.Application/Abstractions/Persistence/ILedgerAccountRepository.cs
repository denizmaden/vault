using Vault.Services.Ledger.Domain.Entities;

namespace Vault.Services.Ledger.Application.Abstractions.Persistence;

public interface ILedgerAccountRepository
{
    Task<bool> ExistsAsync(Guid ledgerAccountId, CancellationToken cancellationToken = default);

    Task<LedgerAccount?> LoadAsync(Guid ledgerAccountId, CancellationToken cancellationToken = default);

    Task SaveAsync(LedgerAccount ledgerAccount, CancellationToken cancellationToken = default);
}

