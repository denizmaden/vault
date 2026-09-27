using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Domain.Errors;

namespace Vault.Services.Identity.Application.Features.GetIdentityUser;

public sealed class GetIdentityUserQueryValidator : IRequestValidator<GetIdentityUserQuery>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        GetIdentityUserQuery request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Error> errors = request.UserId == Guid.Empty
            ? [IdentityErrors.UserNotFound]
            : [];

        return Task.FromResult(errors);
    }
}
