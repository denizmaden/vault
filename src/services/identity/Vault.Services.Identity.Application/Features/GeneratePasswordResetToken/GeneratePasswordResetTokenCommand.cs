using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Application.Models;

namespace Vault.Services.Identity.Application.Features.GeneratePasswordResetToken;

public sealed record GeneratePasswordResetTokenCommand(string Email)
    : ICommand<Result<IdentityGeneratedToken>>;
