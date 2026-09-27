using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Domain.Errors;
using Vault.Services.Identity.Domain.Policies;

namespace Vault.Services.Identity.Application.Features.ResetPassword;

public sealed class ResetPasswordCommandValidator : IRequestValidator<ResetPasswordCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        ResetPasswordCommand request,
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

        var passwordResult = PasswordPolicy.Validate(request.NewPassword);
        if (passwordResult.IsFailure)
        {
            errors.Add(passwordResult.Error);
        }

        return Task.FromResult<IReadOnlyCollection<Error>>(errors);
    }
}
