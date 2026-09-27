using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Identity.Application.Features.RegisterIdentityUser;

public sealed record RegisterIdentityUserCommand(
    string Email,
    string Password) : ICommand<Result<RegisterIdentityUserResponse>>;
