using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Reporting.Domain.Errors;

namespace Vault.Services.Reporting.Application.Features.GetWalletActivityReport;

public sealed class GetWalletActivityReportQueryValidator : IRequestValidator<GetWalletActivityReportQuery>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(GetWalletActivityReportQuery request, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Error> errors = request.WalletId == Guid.Empty
            ? [ReportingErrors.InvalidWalletId]
            : Array.Empty<Error>();

        return Task.FromResult(errors);
    }
}
