using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Ledger.Application.Abstractions.Persistence;
using Vault.Services.Ledger.Application.Models;

namespace Vault.Services.Ledger.Application.Features.GetLedgerAccountEvents;

public sealed class GetLedgerAccountEventsQueryHandler(
    ILedgerReadRepository readRepository) : IQueryHandler<GetLedgerAccountEventsQuery, Result<IReadOnlyCollection<LedgerEventDetails>>>
{
    public async Task<Result<IReadOnlyCollection<LedgerEventDetails>>> Handle(
        GetLedgerAccountEventsQuery query,
        CancellationToken cancellationToken = default)
    {
        var events = await readRepository.GetEventsAsync(query.LedgerAccountId, cancellationToken);
        return Result<IReadOnlyCollection<LedgerEventDetails>>.Success(events);
    }
}

