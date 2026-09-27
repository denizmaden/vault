namespace Vault.Services.Identity.Api.Contracts;

public sealed record IdentityUserResponse(
    Guid UserId,
    string Email,
    bool EmailConfirmed,
    DateTime CreatedOnUtc,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<IdentityUserClaimResponse> Claims);
