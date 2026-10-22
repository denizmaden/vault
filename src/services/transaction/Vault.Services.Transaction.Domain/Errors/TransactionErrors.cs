using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Transaction.Domain.Errors;

public static class TransactionErrors
{
    public static readonly Error InvalidTransactionId = new("transaction.invalid_transaction_id", "Transaction id must not be empty.");
    public static readonly Error InvalidCustomerId = new("transaction.invalid_customer_id", "Initiating customer id must not be empty.");
    public static readonly Error InvalidWalletId = new("transaction.invalid_wallet_id", "Wallet id must not be empty.");
    public static readonly Error DuplicateWallets = new("transaction.duplicate_wallets", "Source and destination wallets must be different.");
    public static readonly Error InvalidCurrency = new("transaction.invalid_currency", "Transaction currency is invalid.");
    public static readonly Error InvalidAmount = new("transaction.invalid_amount", "Transaction amount must be greater than zero.");
    public static readonly Error TransactionNotFound = new("transaction.not_found", "Transaction was not found.");
    public static readonly Error DuplicateTransaction = new("transaction.duplicate_transaction", "Transaction already exists.");
    public static readonly Error InvalidReference = new("transaction.invalid_reference", "Reference must be 150 characters or fewer.");
    public static readonly Error InvalidFailureReason = new("transaction.invalid_failure_reason", "Failure reason must be 250 characters or fewer.");
    public static readonly Error InvalidStatusTransition = new("transaction.invalid_status_transition", "Transaction status transition is not allowed.");
}
