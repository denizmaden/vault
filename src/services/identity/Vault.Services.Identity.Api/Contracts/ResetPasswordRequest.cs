namespace Vault.Services.Identity.Api.Contracts;

public sealed record ResetPasswordRequest(
    Guid UserId,
    string Token,
    string NewPassword);
