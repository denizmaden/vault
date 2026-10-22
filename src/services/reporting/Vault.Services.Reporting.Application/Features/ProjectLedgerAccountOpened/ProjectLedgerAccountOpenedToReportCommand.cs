using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Reporting.Application.Features.ProjectLedgerAccountOpened;

public sealed record ProjectLedgerAccountOpenedToReportCommand(
    Guid WalletId,
    Guid OwnerId,
    string Currency,
    int Version,
    DateTime OccurredOnUtc) : ICommand<Result>;
