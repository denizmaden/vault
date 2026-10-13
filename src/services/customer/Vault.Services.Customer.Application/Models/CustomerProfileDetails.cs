using Vault.Services.Customer.Domain.Enums;

namespace Vault.Services.Customer.Application.Models;

public sealed record CustomerProfileDetails(
    Guid CustomerId,
    Guid IdentityUserId,
    string FirstName,
    string LastName,
    string DisplayName,
    string PreferredLanguage,
    CustomerOnboardingStatus OnboardingStatus,
    DateTime CreatedOnUtc,
    DateTime? OnboardingCompletedOnUtc);
