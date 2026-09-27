using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Identity.Application.Features.ChangePassword;

public sealed record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword) : ICommand<Result>;
