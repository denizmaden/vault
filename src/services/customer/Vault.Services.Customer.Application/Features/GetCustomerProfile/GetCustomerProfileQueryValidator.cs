using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Customer.Domain.Errors;

namespace Vault.Services.Customer.Application.Features.GetCustomerProfile;

public sealed class GetCustomerProfileQueryValidator : IRequestValidator<GetCustomerProfileQuery>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        GetCustomerProfileQuery request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Error> errors = request.CustomerId == Guid.Empty
            ? [CustomerErrors.InvalidCustomerId]
            : [];

        return Task.FromResult(errors);
    }
}
