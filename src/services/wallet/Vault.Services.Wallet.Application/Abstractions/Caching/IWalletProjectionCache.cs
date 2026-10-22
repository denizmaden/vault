using Vault.Services.Wallet.Application.Models;

namespace Vault.Services.Wallet.Application.Abstractions.Caching;

public interface IWalletProjectionCache
{
    Task<WalletDetails?> GetAsync(Guid walletId, CancellationToken cancellationToken = default);

    Task SetAsync(WalletDetails wallet, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid walletId, CancellationToken cancellationToken = default);
}

