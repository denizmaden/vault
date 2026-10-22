using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Ledger.Application.Features.GetLedgerAccount;

public sealed class GetLedgerAccountQueryValidator : IRequestValidator<GetLedgerAccountQuery>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        GetLedgerAccountQuery request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Error> errors = request.LedgerAccountId == Guid.Empty
            ? [new Error("ledger.validation.account_id_required", "Ledger account id is required.")]
            : [];

        return Task.FromResult(errors);
    }
}

