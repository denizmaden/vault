using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Application.Abstractions.Identity;
using Vault.Services.Identity.Application.Models;

namespace Vault.Services.Identity.Application.Features.GetIdentityUser;

public sealed class GetIdentityUserQueryHandler(IIdentityUserManagementService managementService)
    : IQueryHandler<GetIdentityUserQuery, Result<IdentityUserDetails>>
{
    public Task<Result<IdentityUserDetails>> Handle(
        GetIdentityUserQuery query,
        CancellationToken cancellationToken = default) =>
        managementService.GetByIdAsync(query.UserId, cancellationToken);
}
