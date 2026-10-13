using Vault.BuildingBlocks.Domain.Abstractions;

namespace Vault.Services.Customer.Domain.Events;

public sealed record CustomerOnboardingCompletedDomainEvent(Guid CustomerId, Guid IdentityUserId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
