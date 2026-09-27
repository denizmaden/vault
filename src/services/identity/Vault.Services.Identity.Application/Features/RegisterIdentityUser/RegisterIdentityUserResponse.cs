namespace Vault.Services.Identity.Application.Features.RegisterIdentityUser;

public sealed record RegisterIdentityUserResponse(
    Guid IdentityUserId,
    string Email);
