using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Wallet.Application.Abstractions.Caching;
using Vault.Services.Wallet.Application.Abstractions.Persistence;
using Vault.Services.Wallet.Application.Models;
using Vault.Services.Wallet.Domain.Errors;
using WalletProjection = Vault.Services.Wallet.Domain.Entities.Wallet;

namespace Vault.Services.Wallet.Application.Features.GetWallet;

public sealed class GetWalletQueryHandler(
    IWalletProjectionRepository repository,
    IWalletProjectionCache cache) : IQueryHandler<GetWalletQuery, Result<WalletDetails>>
{
    public async Task<Result<WalletDetails>> Handle(GetWalletQuery query, CancellationToken cancellationToken = default)
    {
        var cachedWallet = await cache.GetAsync(query.WalletId, cancellationToken);
        if (cachedWallet is not null)
        {
            return Result<WalletDetails>.Success(cachedWallet);
        }

        var wallet = await repository.GetAsync(query.WalletId, cancellationToken);

        if (wallet is null)
        {
            return Result<WalletDetails>.Failure(WalletErrors.WalletNotFound);
        }

        var details = Map(wallet);
        await cache.SetAsync(details, cancellationToken);

        return Result<WalletDetails>.Success(details);
    }

    private static WalletDetails Map(WalletProjection wallet) =>
        new(
            wallet.Id,
            wallet.OwnerId,
            wallet.Currency.Code,
            wallet.Balance,
            wallet.Status.ToString(),
            wallet.Version,
            wallet.OpenedOnUtc,
            wallet.LastSyncedOnUtc);
}

