using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Ledger.Application.Features.WithdrawFunds;

public sealed class WithdrawFundsCommandValidator : IRequestValidator<WithdrawFundsCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        WithdrawFundsCommand request,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();

        if (request.LedgerAccountId == Guid.Empty)
        {
            errors.Add(new Error("ledger.validation.account_id_required", "Ledger account id is required."));
        }

        if (request.Amount <= 0)
        {
            errors.Add(new Error("ledger.validation.amount_required", "Amount must be greater than zero."));
        }

        return Task.FromResult<IReadOnlyCollection<Error>>(errors);
    }
}

