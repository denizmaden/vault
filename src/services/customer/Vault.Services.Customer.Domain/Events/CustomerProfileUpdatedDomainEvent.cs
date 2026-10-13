using Vault.BuildingBlocks.Domain.Abstractions;

namespace Vault.Services.Customer.Domain.Events;

public sealed record CustomerProfileUpdatedDomainEvent(Guid CustomerId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
