using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Ledger.Application.Models;

namespace Vault.Services.Ledger.Application.Features.GetLedgerAccount;

public sealed record GetLedgerAccountQuery(Guid LedgerAccountId) : IQuery<Result<LedgerAccountDetails>>;

