using Microsoft.Extensions.Logging;
using Vault.Platform.Caching.Abstractions;
using Vault.Services.Wallet.Application.Abstractions.Caching;
using Vault.Services.Wallet.Application.Models;
using Vault.Services.Wallet.Infrastructure.Configuration;

namespace Vault.Services.Wallet.Infrastructure.Caching;

public sealed class WalletProjectionCache(
    ICacheStore cacheStore,
    WalletCacheSettings settings,
    ILogger<WalletProjectionCache> logger) : IWalletProjectionCache
{
    public async Task<WalletDetails?> GetAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await cacheStore.GetAsync<WalletDetails>(BuildWalletDetailsKey(walletId), cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Wallet cache read failed for wallet {WalletId}.", walletId);
            return null;
        }
    }

    public async Task SetAsync(WalletDetails wallet, CancellationToken cancellationToken = default)
    {
        try
        {
            await cacheStore.SetAsync(
                BuildWalletDetailsKey(wallet.WalletId),
                wallet,
                settings.WalletDetailsTtl,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Wallet cache write failed for wallet {WalletId}.", wallet.WalletId);
        }
    }

    public async Task RemoveAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        try
        {
            await cacheStore.RemoveAsync(BuildWalletDetailsKey(walletId), cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Wallet cache invalidation failed for wallet {WalletId}.", walletId);
        }
    }

    private static string BuildWalletDetailsKey(Guid walletId) => $"wallets:{walletId:D}:details";
}
