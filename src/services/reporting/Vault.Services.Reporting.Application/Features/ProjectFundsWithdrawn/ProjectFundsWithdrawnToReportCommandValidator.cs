using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Reporting.Domain.Errors;

namespace Vault.Services.Reporting.Application.Features.ProjectFundsWithdrawn;

public sealed class ProjectFundsWithdrawnToReportCommandValidator : IRequestValidator<ProjectFundsWithdrawnToReportCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(ProjectFundsWithdrawnToReportCommand request, CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();

        if (request.WalletId == Guid.Empty)
        {
            errors.Add(ReportingErrors.InvalidWalletId);
        }

        if (request.Amount <= 0)
        {
            errors.Add(ReportingErrors.InvalidAmount);
        }

        if (request.Version < 1)
        {
            errors.Add(ReportingErrors.InvalidVersion);
        }

        return Task.FromResult<IReadOnlyCollection<Error>>(errors);
    }
}
