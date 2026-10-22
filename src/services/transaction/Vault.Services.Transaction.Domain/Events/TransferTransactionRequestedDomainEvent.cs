using Vault.BuildingBlocks.Domain.Abstractions;

namespace Vault.Services.Transaction.Domain.Events;

public sealed record TransferTransactionRequestedDomainEvent(
    Guid TransactionId,
    Guid InitiatingCustomerId,
    Guid SourceWalletId,
    Guid DestinationWalletId,
    string Currency,
    decimal Amount,
    string Reference) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
