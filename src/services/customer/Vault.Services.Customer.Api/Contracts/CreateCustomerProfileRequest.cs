namespace Vault.Services.Customer.Api.Contracts;

public sealed record CreateCustomerProfileRequest(
    Guid IdentityUserId,
    string FirstName,
    string LastName,
    string DisplayName,
    string PreferredLanguage);
