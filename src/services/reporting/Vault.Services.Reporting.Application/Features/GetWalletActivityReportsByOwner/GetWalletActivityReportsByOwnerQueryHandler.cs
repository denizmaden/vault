using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Reporting.Application.Abstractions.Persistence;
using Vault.Services.Reporting.Application.Models;

namespace Vault.Services.Reporting.Application.Features.GetWalletActivityReportsByOwner;

public sealed class GetWalletActivityReportsByOwnerQueryHandler(IWalletActivityReportRepository repository)
    : IQueryHandler<GetWalletActivityReportsByOwnerQuery, Result<IReadOnlyCollection<WalletActivityReportDetails>>>
{
    public async Task<Result<IReadOnlyCollection<WalletActivityReportDetails>>> Handle(
        GetWalletActivityReportsByOwnerQuery query,
        CancellationToken cancellationToken = default)
    {
        var reports = await repository.GetByOwnerIdAsync(query.OwnerId, cancellationToken);

        return Result<IReadOnlyCollection<WalletActivityReportDetails>>.Success(
            reports
                .OrderBy(x => x.Id)
                .Select(Features.GetWalletActivityReport.GetWalletActivityReportQueryHandler.Map)
                .ToArray());
    }
}
