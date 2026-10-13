using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Customer.Domain.Enums;
using Vault.Services.Customer.Domain.Errors;
using Vault.Services.Customer.Domain.Events;
using Vault.Services.Customer.Domain.Specifications;
using Vault.Services.Customer.Domain.ValueObjects;

namespace Vault.Services.Customer.Domain.Entities;

public sealed class CustomerProfile : AggregateRoot<Guid>
{
    private CustomerProfile(
        Guid id,
        Guid identityUserId,
        CustomerName legalName,
        CustomerDisplayName displayName,
        PreferredLanguage preferredLanguage,
        DateTime createdOnUtc)
        : base(id)
    {
        IdentityUserId = identityUserId;
        LegalName = legalName;
        DisplayName = displayName;
        PreferredLanguage = preferredLanguage;
        CreatedOnUtc = createdOnUtc;
        OnboardingStatus = CustomerOnboardingStatus.Pending;
    }

    private CustomerProfile()
        : base(Guid.Empty)
    {
        LegalName = null!;
        DisplayName = null!;
        PreferredLanguage = null!;
    }

    public Guid IdentityUserId { get; private set; }

    public CustomerName LegalName { get; private set; }

    public CustomerDisplayName DisplayName { get; private set; }

    public PreferredLanguage PreferredLanguage { get; private set; }

    public CustomerOnboardingStatus OnboardingStatus { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public DateTime? OnboardingCompletedOnUtc { get; private set; }

    public static Result<CustomerProfile> Create(
        Guid customerId,
        Guid identityUserId,
        CustomerName legalName,
        CustomerDisplayName displayName,
        PreferredLanguage preferredLanguage,
        DateTime createdOnUtc)
    {
        if (customerId == Guid.Empty)
        {
            return Result<CustomerProfile>.Failure(CustomerErrors.InvalidCustomerId);
        }

        if (identityUserId == Guid.Empty)
        {
            return Result<CustomerProfile>.Failure(CustomerErrors.InvalidIdentityUserId);
        }

        var profile = new CustomerProfile(
            customerId,
            identityUserId,
            legalName,
            displayName,
            preferredLanguage,
            createdOnUtc);

        profile.RaiseDomainEvent(new CustomerProfileCreatedDomainEvent(profile.Id, profile.IdentityUserId));

        return Result<CustomerProfile>.Success(profile);
    }

    public void UpdateProfile(
        CustomerName legalName,
        CustomerDisplayName displayName,
        PreferredLanguage preferredLanguage)
    {
        LegalName = legalName;
        DisplayName = displayName;
        PreferredLanguage = preferredLanguage;

        RaiseDomainEvent(new CustomerProfileUpdatedDomainEvent(Id));
    }

    public Result CompleteOnboarding(DateTime completedOnUtc)
    {
        if (OnboardingStatus == CustomerOnboardingStatus.Completed)
        {
            return Result.Failure(CustomerErrors.OnboardingAlreadyCompleted);
        }

        var specification = new CustomerProfileCompletenessSpecification();

        if (!specification.IsSatisfiedBy(this))
        {
            return Result.Failure(CustomerErrors.OnboardingRequirementsNotMet);
        }

        OnboardingStatus = CustomerOnboardingStatus.Completed;
        OnboardingCompletedOnUtc = completedOnUtc;
        RaiseDomainEvent(new CustomerOnboardingCompletedDomainEvent(Id, IdentityUserId));

        return Result.Success();
    }
}
