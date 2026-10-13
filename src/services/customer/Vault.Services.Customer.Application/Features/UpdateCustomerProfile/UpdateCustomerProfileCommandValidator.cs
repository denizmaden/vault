using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Customer.Domain.Errors;

namespace Vault.Services.Customer.Application.Features.UpdateCustomerProfile;

public sealed class UpdateCustomerProfileCommandValidator : IRequestValidator<UpdateCustomerProfileCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        UpdateCustomerProfileCommand request,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();

        if (request.CustomerId == Guid.Empty)
        {
            errors.Add(CustomerErrors.InvalidCustomerId);
        }

        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            errors.Add(CustomerErrors.InvalidFirstName);
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            errors.Add(CustomerErrors.InvalidLastName);
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            errors.Add(CustomerErrors.InvalidDisplayName);
        }

        if (string.IsNullOrWhiteSpace(request.PreferredLanguage))
        {
            errors.Add(CustomerErrors.InvalidPreferredLanguage);
        }

        return Task.FromResult<IReadOnlyCollection<Error>>(errors);
    }
}
