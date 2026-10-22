namespace Vault.Services.Transaction.Api.Contracts;

public sealed record CreateTransferTransactionRequest(
    Guid TransactionId,
    Guid InitiatingCustomerId,
    Guid SourceWalletId,
    Guid DestinationWalletId,
    string Currency,
    decimal Amount,
    string? Reference);
