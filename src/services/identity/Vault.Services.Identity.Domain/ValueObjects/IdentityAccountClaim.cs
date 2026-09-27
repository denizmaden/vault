using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Domain.Errors;

namespace Vault.Services.Identity.Domain.ValueObjects;

public sealed class IdentityAccountClaim : ValueObject
{
    private IdentityAccountClaim(string type, string value)
    {
        Type = type;
        Value = value;
    }

    public string Type { get; }

    public string Value { get; }

    public static Result<IdentityAccountClaim> Create(string? type, string? value)
    {
        if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value))
        {
            return Result<IdentityAccountClaim>.Failure(IdentityErrors.InvalidClaim);
        }

        return Result<IdentityAccountClaim>.Success(new IdentityAccountClaim(type.Trim(), value.Trim()));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return Value;
    }
}
