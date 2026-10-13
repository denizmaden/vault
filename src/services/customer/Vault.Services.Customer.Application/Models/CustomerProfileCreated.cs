using Vault.Services.Customer.Domain.Enums;

namespace Vault.Services.Customer.Application.Models;

public sealed record CustomerProfileCreated(
    Guid CustomerId,
    Guid IdentityUserId,
    CustomerOnboardingStatus OnboardingStatus);
