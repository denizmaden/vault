using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Customer.Domain.Errors;

namespace Vault.Services.Customer.Domain.ValueObjects;

public sealed class PreferredLanguage : ValueObject
{
    private PreferredLanguage()
    {
        Value = string.Empty;
    }

    private PreferredLanguage(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PreferredLanguage> Create(string value)
    {
        var normalizedValue = value?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedValue) || normalizedValue.Length is < 2 or > 10)
        {
            return Result<PreferredLanguage>.Failure(CustomerErrors.InvalidPreferredLanguage);
        }

        return Result<PreferredLanguage>.Success(new PreferredLanguage(normalizedValue));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
