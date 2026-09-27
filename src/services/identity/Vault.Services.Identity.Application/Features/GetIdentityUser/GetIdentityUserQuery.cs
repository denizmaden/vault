using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Application.Models;

namespace Vault.Services.Identity.Application.Features.GetIdentityUser;

public sealed record GetIdentityUserQuery(Guid UserId) : IQuery<Result<IdentityUserDetails>>;
