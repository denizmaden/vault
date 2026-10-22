using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Ledger.Application.Models;

namespace Vault.Services.Ledger.Application.Features.GetLedgerAccountEvents;

public sealed record GetLedgerAccountEventsQuery(Guid LedgerAccountId) : IQuery<Result<IReadOnlyCollection<LedgerEventDetails>>>;

