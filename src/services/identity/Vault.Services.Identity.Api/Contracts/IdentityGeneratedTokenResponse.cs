namespace Vault.Services.Identity.Api.Contracts;

public sealed record IdentityGeneratedTokenResponse(
    Guid UserId,
    string Purpose,
    string Token);
