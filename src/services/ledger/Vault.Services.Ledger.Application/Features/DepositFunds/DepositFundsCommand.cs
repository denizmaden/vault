using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Ledger.Application.Features.DepositFunds;

public sealed record DepositFundsCommand(
    Guid LedgerAccountId,
    decimal Amount,
    string Reference) : ICommand<Result>;

