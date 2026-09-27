using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Application.Abstractions.Identity;

namespace Vault.Services.Identity.Application.Features.ConfirmEmail;

public sealed class ConfirmEmailCommandHandler(IIdentityUserManagementService managementService)
    : ICommandHandler<ConfirmEmailCommand, Result>
{
    public Task<Result> Handle(
        ConfirmEmailCommand command,
        CancellationToken cancellationToken = default) =>
        managementService.ConfirmEmailAsync(command.UserId, command.Token, cancellationToken);
}
