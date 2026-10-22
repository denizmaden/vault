using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Reporting.Application.Features.ProjectFundsDeposited;

public sealed record ProjectFundsDepositedToReportCommand(
    Guid WalletId,
    decimal Amount,
    string Reference,
    int Version,
    DateTime OccurredOnUtc) : ICommand<Result>;
