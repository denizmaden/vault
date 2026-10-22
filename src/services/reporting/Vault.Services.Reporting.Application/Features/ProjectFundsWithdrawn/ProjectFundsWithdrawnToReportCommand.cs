using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Reporting.Application.Features.ProjectFundsWithdrawn;

public sealed record ProjectFundsWithdrawnToReportCommand(
    Guid WalletId,
    decimal Amount,
    string Reference,
    int Version,
    DateTime OccurredOnUtc) : ICommand<Result>;
