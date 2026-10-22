using Vault.Services.Wallet.Application.Models;
using WalletProjection = Vault.Services.Wallet.Domain.Entities.Wallet;

namespace Vault.Services.Wallet.Application.Abstractions.Persistence;

public interface IWalletProjectionRepository
{
    Task<bool> ExistsAsync(Guid walletId, CancellationToken cancellationToken = default);

    Task<WalletProjection?> GetAsync(Guid walletId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<WalletDetails>> GetByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);

    Task UpsertAsync(WalletProjection wallet, CancellationToken cancellationToken = default);
}

