using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Wallet.Application.Features.ProjectFundsDeposited;

public sealed record ProjectFundsDepositedCommand(
    Guid WalletId,
    decimal Amount,
    int Version,
    DateTime OccurredOnUtc) : ICommand<Result>;

