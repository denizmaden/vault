namespace Vault.Services.Transaction.Api.Contracts;

public sealed record TransactionResponse(
    Guid TransactionId,
    Guid InitiatingCustomerId,
    Guid SourceWalletId,
    Guid DestinationWalletId,
    string Currency,
    decimal Amount,
    string Reference,
    string Type,
    string Status,
    string? FailureReason,
    DateTime CreatedOnUtc,
    DateTime LastUpdatedOnUtc,
    DateTime? FundsReservedOnUtc,
    DateTime? CompletedOnUtc,
    DateTime? FailedOnUtc);
