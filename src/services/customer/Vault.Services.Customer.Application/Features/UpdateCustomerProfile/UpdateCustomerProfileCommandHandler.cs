using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Customer.Application.Abstractions.Persistence;
using Vault.Services.Customer.Domain.Errors;
using Vault.Services.Customer.Domain.ValueObjects;

namespace Vault.Services.Customer.Application.Features.UpdateCustomerProfile;

public sealed class UpdateCustomerProfileCommandHandler(ICustomerProfileRepository customerProfileRepository)
    : ICommandHandler<UpdateCustomerProfileCommand, Result>
{
    public async Task<Result> Handle(
        UpdateCustomerProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var profile = await customerProfileRepository.GetByIdAsync(command.CustomerId, cancellationToken);

        if (profile is null)
        {
            return Result.Failure(CustomerErrors.CustomerNotFound);
        }

        var legalNameResult = CustomerName.Create(command.FirstName, command.LastName);
        if (legalNameResult.IsFailure)
        {
            return Result.Failure(legalNameResult.Error);
        }

        var displayNameResult = CustomerDisplayName.Create(command.DisplayName);
        if (displayNameResult.IsFailure)
        {
            return Result.Failure(displayNameResult.Error);
        }

        var preferredLanguageResult = PreferredLanguage.Create(command.PreferredLanguage);
        if (preferredLanguageResult.IsFailure)
        {
            return Result.Failure(preferredLanguageResult.Error);
        }

        profile.UpdateProfile(legalNameResult.Value, displayNameResult.Value, preferredLanguageResult.Value);
        await customerProfileRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
