using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Application.Abstractions.Identity;

namespace Vault.Services.Identity.Application.Features.AssignClaim;

public sealed class AssignClaimCommandHandler(IIdentityUserManagementService managementService)
    : ICommandHandler<AssignClaimCommand, Result>
{
    public Task<Result> Handle(
        AssignClaimCommand command,
        CancellationToken cancellationToken = default) =>
        managementService.AssignClaimAsync(
            command.UserId,
            command.ClaimType,
            command.ClaimValue,
            cancellationToken);
}
