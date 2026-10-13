namespace Vault.Services.Customer.Api.Contracts;

public sealed record UpdateCustomerProfileRequest(
    string FirstName,
    string LastName,
    string DisplayName,
    string PreferredLanguage);
