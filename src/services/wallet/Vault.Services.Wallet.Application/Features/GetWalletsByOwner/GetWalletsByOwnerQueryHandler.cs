using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Wallet.Application.Abstractions.Persistence;
using Vault.Services.Wallet.Application.Models;

namespace Vault.Services.Wallet.Application.Features.GetWalletsByOwner;

public sealed class GetWalletsByOwnerQueryHandler(
    IWalletProjectionRepository repository) : IQueryHandler<GetWalletsByOwnerQuery, Result<IReadOnlyCollection<WalletDetails>>>
{
    public async Task<Result<IReadOnlyCollection<WalletDetails>>> Handle(GetWalletsByOwnerQuery query, CancellationToken cancellationToken = default)
    {
        var wallets = await repository.GetByOwnerAsync(query.OwnerId, cancellationToken);
        return Result<IReadOnlyCollection<WalletDetails>>.Success(wallets);
    }
}

