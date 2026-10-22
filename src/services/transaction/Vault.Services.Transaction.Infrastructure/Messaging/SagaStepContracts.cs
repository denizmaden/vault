namespace Vault.Services.Transaction.Infrastructure.Messaging;

public sealed record TransferTransactionRequestedMessage(
    Guid TransactionId,
    Guid InitiatingCustomerId,
    Guid SourceWalletId,
    Guid DestinationWalletId,
    string Currency,
    decimal Amount,
    string Reference,
    DateTime OccurredOnUtc);

public sealed record TransactionFundsReservedMessage(
    Guid TransactionId,
    DateTime OccurredOnUtc);

public sealed record TransactionCompletedMessage(
    Guid TransactionId,
    DateTime OccurredOnUtc);

public sealed record TransactionFailedMessage(
    Guid TransactionId,
    string FailureReason,
    DateTime OccurredOnUtc);
