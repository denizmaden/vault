using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Reporting.Application.Abstractions.Persistence;
using Vault.Services.Reporting.Domain.Entities;
using Vault.Services.Reporting.Domain.Errors;
using Vault.Services.Reporting.Domain.ValueObjects;

namespace Vault.Services.Reporting.Application.Features.ProjectLedgerAccountOpened;

public sealed class ProjectLedgerAccountOpenedToReportCommandHandler(IWalletActivityReportRepository repository)
    : ICommandHandler<ProjectLedgerAccountOpenedToReportCommand, Result>
{
    public async Task<Result> Handle(ProjectLedgerAccountOpenedToReportCommand command, CancellationToken cancellationToken = default)
    {
        if (await repository.ExistsAsync(command.WalletId, cancellationToken))
        {
            return Result.Failure(ReportingErrors.DuplicateReport);
        }

        var currencyResult = ReportingCurrency.Create(command.Currency);
        if (currencyResult.IsFailure)
        {
            return Result.Failure(currencyResult.Error);
        }

        var reportResult = WalletActivityReport.Open(
            command.WalletId,
            command.OwnerId,
            currencyResult.Value,
            command.Version,
            command.OccurredOnUtc);

        if (reportResult.IsFailure)
        {
            return Result.Failure(reportResult.Error);
        }

        await repository.AddAsync(reportResult.Value, cancellationToken);
        return Result.Success();
    }
}
