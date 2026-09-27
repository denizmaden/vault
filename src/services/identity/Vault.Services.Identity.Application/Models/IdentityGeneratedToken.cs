namespace Vault.Services.Identity.Application.Models;

public sealed record IdentityGeneratedToken(
    Guid UserId,
    string Purpose,
    string Token);
