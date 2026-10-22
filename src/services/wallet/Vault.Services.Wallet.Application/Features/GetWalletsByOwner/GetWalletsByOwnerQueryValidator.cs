using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Wallet.Application.Features.GetWalletsByOwner;

public sealed class GetWalletsByOwnerQueryValidator : IRequestValidator<GetWalletsByOwnerQuery>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(GetWalletsByOwnerQuery request, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Error> errors = request.OwnerId == Guid.Empty
            ? [new Error("wallet.validation.owner_id_required", "Owner id is required.")]
            : [];

        return Task.FromResult(errors);
    }
}

