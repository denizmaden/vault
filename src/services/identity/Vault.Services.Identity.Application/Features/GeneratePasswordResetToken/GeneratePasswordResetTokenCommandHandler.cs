using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Application.Abstractions.Identity;
using Vault.Services.Identity.Application.Models;

namespace Vault.Services.Identity.Application.Features.GeneratePasswordResetToken;

public sealed class GeneratePasswordResetTokenCommandHandler(IIdentityUserManagementService managementService)
    : ICommandHandler<GeneratePasswordResetTokenCommand, Result<IdentityGeneratedToken>>
{
    public async Task<Result<IdentityGeneratedToken>> Handle(
        GeneratePasswordResetTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await managementService.GeneratePasswordResetTokenAsync(command.Email, cancellationToken);
        if (result.IsFailure)
        {
            return Result<IdentityGeneratedToken>.Failure(result.Error);
        }

        return Result<IdentityGeneratedToken>.Success(result.Value);
    }
}
