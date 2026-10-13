using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Customer.Application.Abstractions.Persistence;
using Vault.Services.Customer.Application.Models;
using Vault.Services.Customer.Domain.Errors;

namespace Vault.Services.Customer.Application.Features.GetCustomerProfile;

public sealed class GetCustomerProfileQueryHandler(ICustomerProfileRepository customerProfileRepository)
    : IQueryHandler<GetCustomerProfileQuery, Result<CustomerProfileDetails>>
{
    public async Task<Result<CustomerProfileDetails>> Handle(
        GetCustomerProfileQuery query,
        CancellationToken cancellationToken = default)
    {
        var profile = await customerProfileRepository.GetByIdAsync(query.CustomerId, cancellationToken);

        if (profile is null)
        {
            return Result<CustomerProfileDetails>.Failure(CustomerErrors.CustomerNotFound);
        }

        return Result<CustomerProfileDetails>.Success(
            new CustomerProfileDetails(
                profile.Id,
                profile.IdentityUserId,
                profile.LegalName.FirstName,
                profile.LegalName.LastName,
                profile.DisplayName.Value,
                profile.PreferredLanguage.Value,
                profile.OnboardingStatus,
                profile.CreatedOnUtc,
                profile.OnboardingCompletedOnUtc));
    }
}
