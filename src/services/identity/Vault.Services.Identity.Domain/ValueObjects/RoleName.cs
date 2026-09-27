using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Domain.Errors;

namespace Vault.Services.Identity.Domain.ValueObjects;

public sealed class RoleName : ValueObject
{
    private RoleName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<RoleName> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<RoleName>.Failure(IdentityErrors.InvalidRoleName);
        }

        return Result<RoleName>.Success(new RoleName(value.Trim()));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
