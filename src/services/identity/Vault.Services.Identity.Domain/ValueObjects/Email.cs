using System.Text.RegularExpressions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Domain.Errors;

namespace Vault.Services.Identity.Domain.ValueObjects;

public sealed partial class Email : ValueObject
{
    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Email> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<Email>.Failure(IdentityErrors.InvalidEmail);
        }

        var normalizedValue = value.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(normalizedValue))
        {
            return Result<Email>.Failure(IdentityErrors.InvalidEmail);
        }

        return Result<Email>.Success(new Email(normalizedValue));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
