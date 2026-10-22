using Vault.Services.Reporting.Domain.Entities;

namespace Vault.Services.Reporting.Application.Abstractions.Persistence;

public interface IWalletActivityReportRepository
{
    Task<bool> ExistsAsync(Guid walletId, CancellationToken cancellationToken = default);

    Task<WalletActivityReport?> GetByIdAsync(Guid walletId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<WalletActivityReport>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);

    Task AddAsync(WalletActivityReport report, CancellationToken cancellationToken = default);

    Task UpdateAsync(WalletActivityReport report, CancellationToken cancellationToken = default);
}
