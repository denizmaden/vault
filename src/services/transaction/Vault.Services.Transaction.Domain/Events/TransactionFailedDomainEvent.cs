using Vault.BuildingBlocks.Domain.Abstractions;

namespace Vault.Services.Transaction.Domain.Events;

public sealed record TransactionFailedDomainEvent(Guid TransactionId, string FailureReason) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
