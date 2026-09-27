using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Application.Abstractions.Identity;

namespace Vault.Services.Identity.Application.Features.AssignRole;

public sealed class AssignRoleCommandHandler(IIdentityUserManagementService managementService)
    : ICommandHandler<AssignRoleCommand, Result>
{
    public Task<Result> Handle(
        AssignRoleCommand command,
        CancellationToken cancellationToken = default) =>
        managementService.AssignRoleAsync(command.UserId, command.RoleName, cancellationToken);
}
