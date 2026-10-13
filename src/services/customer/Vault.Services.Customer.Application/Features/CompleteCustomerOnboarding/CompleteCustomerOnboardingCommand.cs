using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Customer.Application.Features.CompleteCustomerOnboarding;

public sealed record CompleteCustomerOnboardingCommand(Guid CustomerId) : ICommand<Result>;
