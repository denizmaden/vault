namespace Vault.Services.Identity.Api.Contracts;

public sealed record IdentityUserClaimResponse(
    string Type,
    string Value);
