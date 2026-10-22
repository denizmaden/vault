using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Wallet.Application.Features.ProjectFundsDeposited;

public sealed class ProjectFundsDepositedCommandValidator : IRequestValidator<ProjectFundsDepositedCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(ProjectFundsDepositedCommand request, CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();

        if (request.WalletId == Guid.Empty)
        {
            errors.Add(new Error("wallet.validation.wallet_id_required", "Wallet id is required."));
        }

        if (request.Amount <= 0)
        {
            errors.Add(new Error("wallet.validation.amount_required", "Amount must be greater than zero."));
        }

        if (request.Version < 1)
        {
            errors.Add(new Error("wallet.validation.version_required", "Projection version must be positive."));
        }

        return Task.FromResult<IReadOnlyCollection<Error>>(errors);
    }
}

