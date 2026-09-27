using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Application.Abstractions.Identity;

namespace Vault.Services.Identity.Application.Features.CreateRole;

public sealed class CreateRoleCommandHandler(IIdentityUserManagementService managementService)
    : ICommandHandler<CreateRoleCommand, Result>
{
    public Task<Result> Handle(
        CreateRoleCommand command,
        CancellationToken cancellationToken = default) =>
        managementService.CreateRoleAsync(command.RoleName, cancellationToken);
}
