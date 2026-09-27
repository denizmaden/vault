using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Application.Abstractions.Identity;
using Vault.Services.Identity.Application.Models;

namespace Vault.Services.Identity.Application.Features.GenerateEmailConfirmationToken;

public sealed class GenerateEmailConfirmationTokenCommandHandler(IIdentityUserManagementService managementService)
    : ICommandHandler<GenerateEmailConfirmationTokenCommand, Result<IdentityGeneratedToken>>
{
    public async Task<Result<IdentityGeneratedToken>> Handle(
        GenerateEmailConfirmationTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await managementService.GenerateEmailConfirmationTokenAsync(command.UserId, cancellationToken);
        if (result.IsFailure)
        {
            return Result<IdentityGeneratedToken>.Failure(result.Error);
        }

        return Result<IdentityGeneratedToken>.Success(
            new IdentityGeneratedToken(command.UserId, "email_confirmation", result.Value));
    }
}
