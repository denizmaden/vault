using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Customer.Domain.Errors;

public static class CustomerErrors
{
    public static readonly Error InvalidCustomerId = new("customer.invalid_customer_id", "Customer id must not be empty.");
    public static readonly Error InvalidIdentityUserId = new("customer.invalid_identity_user_id", "Identity user id must not be empty.");
    public static readonly Error InvalidFirstName = new("customer.invalid_first_name", "First name is required and must be 100 characters or fewer.");
    public static readonly Error InvalidLastName = new("customer.invalid_last_name", "Last name is required and must be 100 characters or fewer.");
    public static readonly Error InvalidDisplayName = new("customer.invalid_display_name", "Display name is required and must be between 3 and 150 characters.");
    public static readonly Error InvalidPreferredLanguage = new("customer.invalid_preferred_language", "Preferred language must be between 2 and 10 characters.");
    public static readonly Error CustomerNotFound = new("customer.not_found", "Customer profile was not found.");
    public static readonly Error DuplicateIdentityUserId = new("customer.duplicate_identity_user_id", "A customer profile already exists for the specified identity user.");
    public static readonly Error OnboardingAlreadyCompleted = new("customer.onboarding_already_completed", "Customer onboarding is already completed.");
    public static readonly Error OnboardingRequirementsNotMet = new("customer.onboarding_requirements_not_met", "Customer onboarding requirements are not met.");
}
