using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Customer.Application.Abstractions.Persistence;
using Vault.Services.Customer.Application.Models;
using Vault.Services.Customer.Domain.Entities;
using Vault.Services.Customer.Domain.Errors;
using Vault.Services.Customer.Domain.ValueObjects;

namespace Vault.Services.Customer.Application.Features.CreateCustomerProfile;

public sealed class CreateCustomerProfileCommandHandler(ICustomerProfileRepository customerProfileRepository)
    : ICommandHandler<CreateCustomerProfileCommand, Result<CustomerProfileCreated>>
{
    public async Task<Result<CustomerProfileCreated>> Handle(
        CreateCustomerProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        if (await customerProfileRepository.ExistsForIdentityUserAsync(command.IdentityUserId, cancellationToken))
        {
            return Result<CustomerProfileCreated>.Failure(CustomerErrors.DuplicateIdentityUserId);
        }

        var legalNameResult = CustomerName.Create(command.FirstName, command.LastName);
        if (legalNameResult.IsFailure)
        {
            return Result<CustomerProfileCreated>.Failure(legalNameResult.Error);
        }

        var displayNameResult = CustomerDisplayName.Create(command.DisplayName);
        if (displayNameResult.IsFailure)
        {
            return Result<CustomerProfileCreated>.Failure(displayNameResult.Error);
        }

        var preferredLanguageResult = PreferredLanguage.Create(command.PreferredLanguage);
        if (preferredLanguageResult.IsFailure)
        {
            return Result<CustomerProfileCreated>.Failure(preferredLanguageResult.Error);
        }

        var profileResult = CustomerProfile.Create(
            Guid.NewGuid(),
            command.IdentityUserId,
            legalNameResult.Value,
            displayNameResult.Value,
            preferredLanguageResult.Value,
            DateTime.UtcNow);

        if (profileResult.IsFailure)
        {
            return Result<CustomerProfileCreated>.Failure(profileResult.Error);
        }

        await customerProfileRepository.AddAsync(profileResult.Value, cancellationToken);
        await customerProfileRepository.SaveChangesAsync(cancellationToken);

        return Result<CustomerProfileCreated>.Success(
            new CustomerProfileCreated(
                profileResult.Value.Id,
                profileResult.Value.IdentityUserId,
                profileResult.Value.OnboardingStatus));
    }
}
