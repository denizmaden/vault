using Vault.BuildingBlocks.Domain.Abstractions;

namespace Vault.Services.Transaction.Domain.Events;

public sealed record TransactionCompletedDomainEvent(Guid TransactionId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
