using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Identity.Application.Features.CreateRole;

public sealed record CreateRoleCommand(string RoleName) : ICommand<Result>;
