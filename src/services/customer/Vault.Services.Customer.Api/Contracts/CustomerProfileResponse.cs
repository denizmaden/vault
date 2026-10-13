namespace Vault.Services.Customer.Api.Contracts;

public sealed record CustomerProfileResponse(
    Guid CustomerId,
    Guid IdentityUserId,
    string FirstName,
    string LastName,
    string DisplayName,
    string PreferredLanguage,
    string OnboardingStatus,
    DateTime CreatedOnUtc,
    DateTime? OnboardingCompletedOnUtc);
