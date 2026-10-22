namespace Vault.Services.Transaction.Domain.Enums;

public enum TransactionStatus
{
    Requested = 1,
    FundsReserved = 2,
    Completed = 3,
    Failed = 4
}
