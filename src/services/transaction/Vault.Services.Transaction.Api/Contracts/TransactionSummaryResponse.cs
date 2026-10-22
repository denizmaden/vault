namespace Vault.Services.Transaction.Api.Contracts;

public sealed record TransactionSummaryResponse(
    Guid TransactionId,
    Guid InitiatingCustomerId,
    Guid SourceWalletId,
    Guid DestinationWalletId,
    string Currency,
    decimal Amount,
    string Status,
    DateTime CreatedOnUtc,
    DateTime LastUpdatedOnUtc);
