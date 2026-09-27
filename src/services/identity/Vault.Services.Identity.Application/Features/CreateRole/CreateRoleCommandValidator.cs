using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Domain.ValueObjects;

namespace Vault.Services.Identity.Application.Features.CreateRole;

public sealed class CreateRoleCommandValidator : IRequestValidator<CreateRoleCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        CreateRoleCommand request,
        CancellationToken cancellationToken = default)
    {
        var roleResult = RoleName.Create(request.RoleName);
        IReadOnlyCollection<Error> errors = roleResult.IsFailure
            ? [roleResult.Error]
            : [];

        return Task.FromResult(errors);
    }
}
