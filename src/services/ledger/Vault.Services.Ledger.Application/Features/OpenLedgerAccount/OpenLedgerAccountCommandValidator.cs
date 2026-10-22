using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Ledger.Application.Features.OpenLedgerAccount;

public sealed class OpenLedgerAccountCommandValidator : IRequestValidator<OpenLedgerAccountCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        OpenLedgerAccountCommand request,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();

        if (request.LedgerAccountId == Guid.Empty)
        {
            errors.Add(new Error("ledger.validation.account_id_required", "Ledger account id is required."));
        }

        if (request.OwnerId == Guid.Empty)
        {
            errors.Add(new Error("ledger.validation.owner_id_required", "Owner id is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            errors.Add(new Error("ledger.validation.currency_required", "Currency is required."));
        }

        return Task.FromResult<IReadOnlyCollection<Error>>(errors);
    }
}

