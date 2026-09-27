using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Domain.Errors;

namespace Vault.Services.Identity.Application.Features.GenerateEmailConfirmationToken;

public sealed class GenerateEmailConfirmationTokenCommandValidator : IRequestValidator<GenerateEmailConfirmationTokenCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        GenerateEmailConfirmationTokenCommand request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Error> errors = request.UserId == Guid.Empty
            ? [IdentityErrors.UserNotFound]
            : [];

        return Task.FromResult(errors);
    }
}
