namespace Vault.Services.Customer.Api.Contracts;

public sealed record CreateCustomerProfileResponse(
    Guid CustomerId,
    Guid IdentityUserId,
    string OnboardingStatus);
