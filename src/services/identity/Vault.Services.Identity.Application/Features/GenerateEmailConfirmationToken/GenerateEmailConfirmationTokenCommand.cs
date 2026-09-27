using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Application.Models;

namespace Vault.Services.Identity.Application.Features.GenerateEmailConfirmationToken;

public sealed record GenerateEmailConfirmationTokenCommand(Guid UserId)
    : ICommand<Result<IdentityGeneratedToken>>;
