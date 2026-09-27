using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Application.Abstractions.Identity;

namespace Vault.Services.Identity.Application.Features.ResetPassword;

public sealed class ResetPasswordCommandHandler(IIdentityUserManagementService managementService)
    : ICommandHandler<ResetPasswordCommand, Result>
{
    public Task<Result> Handle(
        ResetPasswordCommand command,
        CancellationToken cancellationToken = default) =>
        managementService.ResetPasswordAsync(
            command.UserId,
            command.Token,
            command.NewPassword,
            cancellationToken);
}
