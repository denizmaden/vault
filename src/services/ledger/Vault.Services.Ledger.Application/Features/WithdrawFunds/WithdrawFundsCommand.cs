using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Ledger.Application.Features.WithdrawFunds;

public sealed record WithdrawFundsCommand(
    Guid LedgerAccountId,
    decimal Amount,
    string Reference) : ICommand<Result>;

