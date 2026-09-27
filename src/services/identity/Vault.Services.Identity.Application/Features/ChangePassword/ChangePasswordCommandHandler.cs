using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Application.Abstractions.Identity;

namespace Vault.Services.Identity.Application.Features.ChangePassword;

public sealed class ChangePasswordCommandHandler(IIdentityUserManagementService managementService)
    : ICommandHandler<ChangePasswordCommand, Result>
{
    public Task<Result> Handle(
        ChangePasswordCommand command,
        CancellationToken cancellationToken = default) =>
        managementService.ChangePasswordAsync(
            command.UserId,
            command.CurrentPassword,
            command.NewPassword,
            cancellationToken);
}
