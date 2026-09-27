namespace Vault.Services.Identity.Api.Contracts;

public sealed record AssignClaimRequest(
    string ClaimType,
    string ClaimValue);
