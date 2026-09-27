using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Application.Models;

namespace Vault.Services.Identity.Application.Abstractions.Identity;

public interface IIdentityUserManagementService
{
    Task<Result<IdentityUserDetails>> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<string>> GenerateEmailConfirmationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result> ConfirmEmailAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken = default);

    Task<Result> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<Result<IdentityGeneratedToken>> GeneratePasswordResetTokenAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<Result> ResetPasswordAsync(
        Guid userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<Result> CreateRoleAsync(
        string roleName,
        CancellationToken cancellationToken = default);

    Task<Result> AssignRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default);

    Task<Result> AssignClaimAsync(
        Guid userId,
        string claimType,
        string claimValue,
        CancellationToken cancellationToken = default);
}
