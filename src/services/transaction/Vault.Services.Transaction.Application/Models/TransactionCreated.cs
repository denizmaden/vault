namespace Vault.Services.Transaction.Application.Models;

public sealed record TransactionCreated(Guid TransactionId, string Status);
