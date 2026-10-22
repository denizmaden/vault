using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Wallet.Application.Features.GetWallet;

public sealed class GetWalletQueryValidator : IRequestValidator<GetWalletQuery>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(GetWalletQuery request, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Error> errors = request.WalletId == Guid.Empty
            ? [new Error("wallet.validation.wallet_id_required", "Wallet id is required.")]
            : [];

        return Task.FromResult(errors);
    }
}

