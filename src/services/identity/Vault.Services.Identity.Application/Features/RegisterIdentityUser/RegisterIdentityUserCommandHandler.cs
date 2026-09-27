using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Application.Abstractions.Identity;

namespace Vault.Services.Identity.Application.Features.RegisterIdentityUser;

public sealed class RegisterIdentityUserCommandHandler(IIdentityUserRegistrationService registrationService)
    : ICommandHandler<RegisterIdentityUserCommand, Result<RegisterIdentityUserResponse>>
{
    public async Task<Result<RegisterIdentityUserResponse>> Handle(
        RegisterIdentityUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var registrationResult = await registrationService.RegisterAsync(
            command.Email,
            command.Password,
            cancellationToken);

        if (registrationResult.IsFailure)
        {
            return Result<RegisterIdentityUserResponse>.Failure(registrationResult.Error);
        }

        return Result<RegisterIdentityUserResponse>.Success(
            new RegisterIdentityUserResponse(registrationResult.Value, command.Email.Trim().ToLowerInvariant()));
    }
}
