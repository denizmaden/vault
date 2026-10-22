using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Reporting.Application.Abstractions.Persistence;
using Vault.Services.Reporting.Domain.Errors;
using Vault.Services.Reporting.Domain.ValueObjects;

namespace Vault.Services.Reporting.Application.Features.ProjectFundsWithdrawn;

public sealed class ProjectFundsWithdrawnToReportCommandHandler(IWalletActivityReportRepository repository)
    : ICommandHandler<ProjectFundsWithdrawnToReportCommand, Result>
{
    public async Task<Result> Handle(ProjectFundsWithdrawnToReportCommand command, CancellationToken cancellationToken = default)
    {
        var report = await repository.GetByIdAsync(command.WalletId, cancellationToken);
        if (report is null)
        {
            return Result.Failure(ReportingErrors.ReportNotFound);
        }

        var amountResult = ReportingAmount.Create(command.Amount);
        if (amountResult.IsFailure)
        {
            return Result.Failure(amountResult.Error);
        }

        var applyResult = report.ApplyWithdrawal(amountResult.Value, command.Reference, command.Version, command.OccurredOnUtc);
        if (applyResult.IsFailure)
        {
            return applyResult;
        }

        await repository.UpdateAsync(report, cancellationToken);
        return Result.Success();
    }
}
