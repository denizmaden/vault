namespace Vault.Services.Identity.Application.Models;

public sealed record IdentityUserDetails(
    Guid UserId,
    string Email,
    bool EmailConfirmed,
    DateTime CreatedOnUtc,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<IdentityUserClaimDetails> Claims);
