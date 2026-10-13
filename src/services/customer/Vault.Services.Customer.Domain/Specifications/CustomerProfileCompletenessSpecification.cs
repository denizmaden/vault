using Vault.Services.Customer.Domain.Entities;

namespace Vault.Services.Customer.Domain.Specifications;

public sealed class CustomerProfileCompletenessSpecification
{
    public bool IsSatisfiedBy(CustomerProfile profile)
    {
        return !string.IsNullOrWhiteSpace(profile.DisplayName.Value)
            && !string.IsNullOrWhiteSpace(profile.LegalName.FirstName)
            && !string.IsNullOrWhiteSpace(profile.LegalName.LastName)
            && !string.IsNullOrWhiteSpace(profile.PreferredLanguage.Value);
    }
}
