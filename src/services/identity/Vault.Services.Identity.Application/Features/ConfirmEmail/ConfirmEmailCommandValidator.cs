using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Domain.Errors;

namespace Vault.Services.Identity.Application.Features.ConfirmEmail;

public sealed class ConfirmEmailCommandValidator : IRequestValidator<ConfirmEmailCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        ConfirmEmailCommand request,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();
        if (request.UserId == Guid.Empty)
        {
            errors.Add(IdentityErrors.UserNotFound);
        }

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            errors.Add(IdentityErrors.InvalidToken);
        }

        return Task.FromResult<IReadOnlyCollection<Error>>(errors);
    }
}
