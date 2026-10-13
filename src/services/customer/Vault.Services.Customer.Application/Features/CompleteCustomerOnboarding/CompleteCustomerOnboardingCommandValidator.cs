using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Customer.Domain.Errors;

namespace Vault.Services.Customer.Application.Features.CompleteCustomerOnboarding;

public sealed class CompleteCustomerOnboardingCommandValidator : IRequestValidator<CompleteCustomerOnboardingCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        CompleteCustomerOnboardingCommand request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Error> errors = request.CustomerId == Guid.Empty
            ? [CustomerErrors.InvalidCustomerId]
            : [];

        return Task.FromResult(errors);
    }
}
