using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Customer.Domain.Errors;

namespace Vault.Services.Customer.Domain.ValueObjects;

public sealed class CustomerDisplayName : ValueObject
{
    private CustomerDisplayName()
    {
        Value = string.Empty;
    }

    private CustomerDisplayName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<CustomerDisplayName> Create(string value)
    {
        var normalizedValue = value?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedValue) || normalizedValue.Length is < 3 or > 150)
        {
            return Result<CustomerDisplayName>.Failure(CustomerErrors.InvalidDisplayName);
        }

        return Result<CustomerDisplayName>.Success(new CustomerDisplayName(normalizedValue));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
