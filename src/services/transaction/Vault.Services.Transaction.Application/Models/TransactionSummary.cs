namespace Vault.Services.Transaction.Application.Models;

public sealed record TransactionSummary(
    Guid TransactionId,
    Guid InitiatingCustomerId,
    Guid SourceWalletId,
    Guid DestinationWalletId,
    string Currency,
    decimal Amount,
    string Status,
    DateTime CreatedOnUtc,
    DateTime LastUpdatedOnUtc);
