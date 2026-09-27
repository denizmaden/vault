using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Identity.Application.Features.AssignRole;

public sealed record AssignRoleCommand(Guid UserId, string RoleName) : ICommand<Result>;
