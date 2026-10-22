using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Wallet.Application.Features.ProjectLedgerAccountOpened;

public sealed class ProjectLedgerAccountOpenedCommandValidator : IRequestValidator<ProjectLedgerAccountOpenedCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(ProjectLedgerAccountOpenedCommand request, CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();

        if (request.WalletId == Guid.Empty)
        {
            errors.Add(new Error("wallet.validation.wallet_id_required", "Wallet id is required."));
        }

        if (request.OwnerId == Guid.Empty)
        {
            errors.Add(new Error("wallet.validation.owner_id_required", "Owner id is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            errors.Add(new Error("wallet.validation.currency_required", "Currency is required."));
        }

        if (request.Version < 1)
        {
            errors.Add(new Error("wallet.validation.version_required", "Projection version must be at least 1."));
        }

        return Task.FromResult<IReadOnlyCollection<Error>>(errors);
    }
}

