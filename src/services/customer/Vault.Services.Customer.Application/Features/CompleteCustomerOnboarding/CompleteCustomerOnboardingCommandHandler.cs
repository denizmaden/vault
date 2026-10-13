using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Customer.Application.Abstractions.Persistence;
using Vault.Services.Customer.Domain.Errors;

namespace Vault.Services.Customer.Application.Features.CompleteCustomerOnboarding;

public sealed class CompleteCustomerOnboardingCommandHandler(ICustomerProfileRepository customerProfileRepository)
    : ICommandHandler<CompleteCustomerOnboardingCommand, Result>
{
    public async Task<Result> Handle(
        CompleteCustomerOnboardingCommand command,
        CancellationToken cancellationToken = default)
    {
        var profile = await customerProfileRepository.GetByIdAsync(command.CustomerId, cancellationToken);

        if (profile is null)
        {
            return Result.Failure(CustomerErrors.CustomerNotFound);
        }

        var completionResult = profile.CompleteOnboarding(DateTime.UtcNow);
        if (completionResult.IsFailure)
        {
            return completionResult;
        }

        await customerProfileRepository.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
