namespace Vault.Services.Transaction.Api.Contracts;

public sealed record CreateTransferTransactionResponse(Guid TransactionId, string Status);
