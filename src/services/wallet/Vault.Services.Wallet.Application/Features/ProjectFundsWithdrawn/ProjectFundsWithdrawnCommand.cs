using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Wallet.Application.Features.ProjectFundsWithdrawn;

public sealed record ProjectFundsWithdrawnCommand(
    Guid WalletId,
    decimal Amount,
    int Version,
    DateTime OccurredOnUtc) : ICommand<Result>;

