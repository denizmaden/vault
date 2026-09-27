namespace Vault.Services.Identity.Api.Contracts;

public sealed record RegisterIdentityUserRequest(
    string Email,
    string Password);
