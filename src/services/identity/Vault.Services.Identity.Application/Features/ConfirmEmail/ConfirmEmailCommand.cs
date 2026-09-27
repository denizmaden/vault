using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Identity.Application.Features.ConfirmEmail;

public sealed record ConfirmEmailCommand(Guid UserId, string Token) : ICommand<Result>;
