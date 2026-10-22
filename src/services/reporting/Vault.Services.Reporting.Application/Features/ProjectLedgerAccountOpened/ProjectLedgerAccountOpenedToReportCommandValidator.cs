using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Reporting.Domain.Errors;

namespace Vault.Services.Reporting.Application.Features.ProjectLedgerAccountOpened;

public sealed class ProjectLedgerAccountOpenedToReportCommandValidator : IRequestValidator<ProjectLedgerAccountOpenedToReportCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(ProjectLedgerAccountOpenedToReportCommand request, CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();

        if (request.WalletId == Guid.Empty)
        {
            errors.Add(ReportingErrors.InvalidWalletId);
        }

        if (request.OwnerId == Guid.Empty)
        {
            errors.Add(ReportingErrors.InvalidOwnerId);
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            errors.Add(ReportingErrors.InvalidCurrency);
        }

        if (request.Version < 1)
        {
            errors.Add(ReportingErrors.InvalidVersion);
        }

        return Task.FromResult<IReadOnlyCollection<Error>>(errors);
    }
}
