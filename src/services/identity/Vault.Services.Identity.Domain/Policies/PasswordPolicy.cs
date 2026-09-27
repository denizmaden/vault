using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Domain.Errors;

namespace Vault.Services.Identity.Domain.Policies;

public static class PasswordPolicy
{
    public static Result Validate(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            return Result.Failure(IdentityErrors.InvalidPassword);
        }

        return Result.Success();
    }
}
