using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Domain.Errors;
using Vault.Services.Identity.Domain.ValueObjects;

namespace Vault.Services.Identity.Application.Features.AssignRole;

public sealed class AssignRoleCommandValidator : IRequestValidator<AssignRoleCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        AssignRoleCommand request,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();
        if (request.UserId == Guid.Empty)
        {
            errors.Add(IdentityErrors.UserNotFound);
        }

        var roleResult = RoleName.Create(request.RoleName);
        if (roleResult.IsFailure)
        {
            errors.Add(roleResult.Error);
        }

        return Task.FromResult<IReadOnlyCollection<Error>>(errors);
    }
}
