using Microsoft.AspNetCore.Identity;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Application.Abstractions.Identity;
using Vault.Services.Identity.Domain.Entities;
using Vault.Services.Identity.Domain.Errors;
using Vault.Services.Identity.Domain.Policies;
using Vault.Services.Identity.Domain.ValueObjects;

namespace Vault.Services.Identity.Infrastructure.Identity;

public sealed class IdentityRegistrationService(UserManager<VaultIdentityUser> userManager)
    : IIdentityUserRegistrationService
{
    public async Task<Result<Guid>> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
        {
            return Result<Guid>.Failure(emailResult.Error);
        }

        var passwordResult = PasswordPolicy.Validate(password);
        if (passwordResult.IsFailure)
        {
            return Result<Guid>.Failure(passwordResult.Error);
        }

        var account = IdentityAccount.Register(Guid.NewGuid(), emailResult.Value, DateTime.UtcNow);

        var existingUser = await userManager.FindByEmailAsync(account.Email.Value);
        if (existingUser is not null)
        {
            return Result<Guid>.Failure(IdentityErrors.DuplicateEmail);
        }

        var user = new VaultIdentityUser
        {
            Id = account.Id,
            UserName = account.Email.Value,
            Email = account.Email.Value,
            NormalizedUserName = account.Email.Value.ToUpperInvariant(),
            NormalizedEmail = account.Email.Value.ToUpperInvariant(),
            EmailConfirmed = account.EmailConfirmed,
            CreatedOnUtc = account.CreatedOnUtc
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (createResult.Succeeded)
        {
            return Result<Guid>.Success(user.Id);
        }

        if (createResult.Errors.Any(error => error.Code.Contains("DuplicateEmail", StringComparison.OrdinalIgnoreCase)))
        {
            return Result<Guid>.Failure(IdentityErrors.DuplicateEmail);
        }

        return Result<Guid>.Failure(IdentityErrors.InvalidPassword);
    }
}
