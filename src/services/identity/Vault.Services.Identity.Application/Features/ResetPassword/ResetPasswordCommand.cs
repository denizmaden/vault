using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Identity.Application.Features.ResetPassword;

public sealed record ResetPasswordCommand(
    Guid UserId,
    string Token,
    string NewPassword) : ICommand<Result>;
