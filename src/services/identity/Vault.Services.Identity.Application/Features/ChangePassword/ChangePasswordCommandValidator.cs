using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Domain.Errors;
using Vault.Services.Identity.Domain.Policies;

namespace Vault.Services.Identity.Application.Features.ChangePassword;

public sealed class ChangePasswordCommandValidator : IRequestValidator<ChangePasswordCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        ChangePasswordCommand request,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();
        if (request.UserId == Guid.Empty)
        {
            errors.Add(IdentityErrors.UserNotFound);
        }

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            errors.Add(IdentityErrors.InvalidPassword);
        }

        var passwordResult = PasswordPolicy.Validate(request.NewPassword);
        if (passwordResult.IsFailure)
        {
            errors.Add(passwordResult.Error);
        }

        return Task.FromResult<IReadOnlyCollection<Error>>(errors);
    }
}
