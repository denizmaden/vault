using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Identity.Application.Features.AssignClaim;

public sealed record AssignClaimCommand(
    Guid UserId,
    string ClaimType,
    string ClaimValue) : ICommand<Result>;
