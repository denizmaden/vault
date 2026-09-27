using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Domain.Errors;
using Vault.Services.Identity.Domain.ValueObjects;

namespace Vault.Services.Identity.Application.Features.AssignClaim;

public sealed class AssignClaimCommandValidator : IRequestValidator<AssignClaimCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        AssignClaimCommand request,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();
        if (request.UserId == Guid.Empty)
        {
            errors.Add(IdentityErrors.UserNotFound);
        }

        var claimResult = IdentityAccountClaim.Create(request.ClaimType, request.ClaimValue);
        if (claimResult.IsFailure)
        {
            errors.Add(claimResult.Error);
        }

        return Task.FromResult<IReadOnlyCollection<Error>>(errors);
    }
}
