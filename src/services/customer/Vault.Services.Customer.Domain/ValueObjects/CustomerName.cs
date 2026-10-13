using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Customer.Domain.Errors;

namespace Vault.Services.Customer.Domain.ValueObjects;

public sealed class CustomerName : ValueObject
{
    private CustomerName()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    private CustomerName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public string FirstName { get; }

    public string LastName { get; }

    public string FullName => $"{FirstName} {LastName}";

    public static Result<CustomerName> Create(string firstName, string lastName)
    {
        var normalizedFirstName = firstName?.Trim() ?? string.Empty;
        var normalizedLastName = lastName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedFirstName) || normalizedFirstName.Length > 100)
        {
            return Result<CustomerName>.Failure(CustomerErrors.InvalidFirstName);
        }

        if (string.IsNullOrWhiteSpace(normalizedLastName) || normalizedLastName.Length > 100)
        {
            return Result<CustomerName>.Failure(CustomerErrors.InvalidLastName);
        }

        return Result<CustomerName>.Success(new CustomerName(normalizedFirstName, normalizedLastName));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }
}
