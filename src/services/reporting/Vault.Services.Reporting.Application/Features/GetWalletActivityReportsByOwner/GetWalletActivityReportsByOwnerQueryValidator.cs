using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Reporting.Domain.Errors;

namespace Vault.Services.Reporting.Application.Features.GetWalletActivityReportsByOwner;

public sealed class GetWalletActivityReportsByOwnerQueryValidator : IRequestValidator<GetWalletActivityReportsByOwnerQuery>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(GetWalletActivityReportsByOwnerQuery request, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Error> errors = request.OwnerId == Guid.Empty
            ? [ReportingErrors.InvalidOwnerId]
            : Array.Empty<Error>();

        return Task.FromResult(errors);
    }
}
