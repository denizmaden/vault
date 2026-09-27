using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Application.Abstractions.Identity;
using Vault.Services.Identity.Application.Models;
using Vault.Services.Identity.Domain.Entities;
using Vault.Services.Identity.Domain.Errors;
using Vault.Services.Identity.Domain.Policies;
using Vault.Services.Identity.Domain.ValueObjects;

namespace Vault.Services.Identity.Infrastructure.Identity;

public sealed class IdentityUserManagementService(
    UserManager<VaultIdentityUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager)
    : IIdentityUserManagementService
{
    public async Task<Result<IdentityUserDetails>> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result<IdentityUserDetails>.Failure(IdentityErrors.UserNotFound);
        }

        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);
        var account = CreateAccount(user, roles, claims);

        return Result<IdentityUserDetails>.Success(
            new IdentityUserDetails(
                account.Id,
                account.Email.Value,
                account.EmailConfirmed,
                account.CreatedOnUtc,
                account.Roles.Select(role => role.Value).ToArray(),
                account.Claims.Select(claim => new IdentityUserClaimDetails(claim.Type, claim.Value)).ToArray()));
    }

    public async Task<Result<string>> GenerateEmailConfirmationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result<string>.Failure(IdentityErrors.UserNotFound);
        }

        if (user.EmailConfirmed)
        {
            return Result<string>.Failure(IdentityErrors.EmailAlreadyConfirmed);
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        return Result<string>.Success(token);
    }

    public async Task<Result> ConfirmEmailAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(IdentityErrors.UserNotFound);
        }

        var emailResult = Email.Create(user.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure(emailResult.Error);
        }

        var account = IdentityAccount.Rehydrate(
            user.Id,
            emailResult.Value,
            user.EmailConfirmed,
            user.CreatedOnUtc,
            [],
            []);

        var confirmResult = account.ConfirmEmail();
        if (confirmResult.IsFailure)
        {
            return confirmResult;
        }

        var result = await userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(IdentityErrors.InvalidToken);
    }

    public async Task<Result> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(IdentityErrors.UserNotFound);
        }

        var passwordResult = PasswordPolicy.Validate(newPassword);
        if (passwordResult.IsFailure)
        {
            return passwordResult;
        }

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (result.Succeeded)
        {
            return Result.Success();
        }

        return result.Errors.Any(error => error.Code.Contains("Password", StringComparison.OrdinalIgnoreCase))
            ? Result.Failure(IdentityErrors.InvalidPassword)
            : Result.Failure(IdentityErrors.InvalidToken);
    }

    public async Task<Result<IdentityGeneratedToken>> GeneratePasswordResetTokenAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result<IdentityGeneratedToken>.Failure(IdentityErrors.UserNotFound);
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        return Result<IdentityGeneratedToken>.Success(
            new IdentityGeneratedToken(user.Id, "password_reset", token));
    }

    public async Task<Result> ResetPasswordAsync(
        Guid userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(IdentityErrors.UserNotFound);
        }

        var passwordResult = PasswordPolicy.Validate(newPassword);
        if (passwordResult.IsFailure)
        {
            return passwordResult;
        }

        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        if (result.Succeeded)
        {
            return Result.Success();
        }

        return result.Errors.Any(error => error.Code.Contains("Password", StringComparison.OrdinalIgnoreCase))
            ? Result.Failure(IdentityErrors.InvalidPassword)
            : Result.Failure(IdentityErrors.InvalidToken);
    }

    public async Task<Result> CreateRoleAsync(
        string roleName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return Result.Failure(IdentityErrors.InvalidRoleName);
        }

        var roleNameResult = RoleName.Create(roleName);
        if (roleNameResult.IsFailure)
        {
            return Result.Failure(roleNameResult.Error);
        }

        var existingRole = await roleManager.FindByNameAsync(roleNameResult.Value.Value);
        if (existingRole is not null)
        {
            return Result.Failure(IdentityErrors.DuplicateRole);
        }

        var result = await roleManager.CreateAsync(new IdentityRole<Guid>(roleNameResult.Value.Value));
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(IdentityErrors.DuplicateRole);
    }

    public async Task<Result> AssignRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(IdentityErrors.UserNotFound);
        }

        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            return Result.Failure(IdentityErrors.RoleNotFound);
        }

        var account = CreateAccount(user, await userManager.GetRolesAsync(user), await userManager.GetClaimsAsync(user));

        var roleNameResult = RoleName.Create(roleName);
        if (roleNameResult.IsFailure)
        {
            return Result.Failure(roleNameResult.Error);
        }

        var assignRoleResult = account.AssignRole(roleNameResult.Value);
        if (assignRoleResult.IsFailure)
        {
            return assignRoleResult;
        }

        var result = await userManager.AddToRoleAsync(user, roleNameResult.Value.Value);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(IdentityErrors.RoleAlreadyAssigned);
    }

    public async Task<Result> AssignClaimAsync(
        Guid userId,
        string claimType,
        string claimValue,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(claimType) || string.IsNullOrWhiteSpace(claimValue))
        {
            return Result.Failure(IdentityErrors.InvalidClaim);
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(IdentityErrors.UserNotFound);
        }

        var claimResult = IdentityAccountClaim.Create(claimType, claimValue);
        if (claimResult.IsFailure)
        {
            return Result.Failure(claimResult.Error);
        }

        var account = CreateAccount(user, await userManager.GetRolesAsync(user), await userManager.GetClaimsAsync(user));
        var assignClaimResult = account.AssignClaim(claimResult.Value);
        if (assignClaimResult.IsFailure)
        {
            return assignClaimResult;
        }

        var result = await userManager.AddClaimAsync(user, new Claim(claimResult.Value.Type, claimResult.Value.Value));
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(IdentityErrors.InvalidClaim);
    }

    private static IdentityAccount CreateAccount(
        VaultIdentityUser user,
        IEnumerable<string> roles,
        IEnumerable<Claim> claims)
    {
        var email = Email.Create(user.Email).Value;
        var roleNames = roles.Select(role => RoleName.Create(role).Value).ToArray();
        var identityClaims = claims.Select(claim => IdentityAccountClaim.Create(claim.Type, claim.Value).Value).ToArray();

        return IdentityAccount.Rehydrate(
            user.Id,
            email,
            user.EmailConfirmed,
            user.CreatedOnUtc,
            roleNames,
            identityClaims);
    }
}
