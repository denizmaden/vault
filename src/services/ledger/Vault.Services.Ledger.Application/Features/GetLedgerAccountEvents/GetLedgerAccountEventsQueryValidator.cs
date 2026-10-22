using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Ledger.Application.Features.GetLedgerAccountEvents;

public sealed class GetLedgerAccountEventsQueryValidator : IRequestValidator<GetLedgerAccountEventsQuery>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        GetLedgerAccountEventsQuery request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Error> errors = request.LedgerAccountId == Guid.Empty
            ? [new Error("ledger.validation.account_id_required", "Ledger account id is required.")]
            : [];

        return Task.FromResult(errors);
    }
}

