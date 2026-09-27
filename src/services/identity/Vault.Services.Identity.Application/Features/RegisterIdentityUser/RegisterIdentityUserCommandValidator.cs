using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Domain.Policies;
using Vault.Services.Identity.Domain.ValueObjects;

namespace Vault.Services.Identity.Application.Features.RegisterIdentityUser;

public sealed class RegisterIdentityUserCommandValidator : IRequestValidator<RegisterIdentityUserCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        RegisterIdentityUserCommand request,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            errors.Add(emailResult.Error);
        }

        var passwordResult = PasswordPolicy.Validate(request.Password);
        if (passwordResult.IsFailure)
        {
            errors.Add(passwordResult.Error);
        }

        return Task.FromResult<IReadOnlyCollection<Error>>(errors);
    }
}
