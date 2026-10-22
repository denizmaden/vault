using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Reporting.Domain.Errors;

namespace Vault.Services.Reporting.Application.Features.ProjectFundsDeposited;

public sealed class ProjectFundsDepositedToReportCommandValidator : IRequestValidator<ProjectFundsDepositedToReportCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(ProjectFundsDepositedToReportCommand request, CancellationToken cancellationToken = default)
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
