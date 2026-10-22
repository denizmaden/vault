using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Reporting.Application.Abstractions.Persistence;
using Vault.Services.Reporting.Application.Models;
using Vault.Services.Reporting.Domain.Entities;
using Vault.Services.Reporting.Domain.Errors;

namespace Vault.Services.Reporting.Application.Features.GetWalletActivityReport;

public sealed class GetWalletActivityReportQueryHandler(IWalletActivityReportRepository repository)
    : IQueryHandler<GetWalletActivityReportQuery, Result<WalletActivityReportDetails>>
{
    public async Task<Result<WalletActivityReportDetails>> Handle(GetWalletActivityReportQuery query, CancellationToken cancellationToken = default)
    {
        var report = await repository.GetByIdAsync(query.WalletId, cancellationToken);
        if (report is null)
        {
            return Result<WalletActivityReportDetails>.Failure(ReportingErrors.ReportNotFound);
        }

        return Result<WalletActivityReportDetails>.Success(Map(report));
    }

    internal static WalletActivityReportDetails Map(WalletActivityReport report) =>
        new(
            report.Id,
            report.OwnerId,
            report.Currency.Code,
            report.CurrentBalance,
            report.TotalDeposited,
            report.TotalWithdrawn,
            report.Version,
            report.OpenedOnUtc,
            report.LastUpdatedOnUtc,
            report.Entries
                .OrderBy(x => x.Version)
                .Select(x => new ActivityEntryDetails(
                    x.Type.ToString(),
                    x.Amount,
                    x.Reference,
                    x.Version,
                    x.OccurredOnUtc))
                .ToArray());
}
