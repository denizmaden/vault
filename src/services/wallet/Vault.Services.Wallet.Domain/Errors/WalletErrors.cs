using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Wallet.Domain.Errors;

public static class WalletErrors
{
    public static readonly Error InvalidWalletId = new("wallet.invalid_wallet_id", "Wallet id is invalid.");
    public static readonly Error InvalidOwnerId = new("wallet.invalid_owner_id", "Wallet owner id is invalid.");
    public static readonly Error InvalidCurrency = new("wallet.invalid_currency", "Wallet currency is invalid.");
    public static readonly Error InvalidAmount = new("wallet.invalid_amount", "Wallet amount must be greater than zero.");
    public static readonly Error WalletAlreadyExists = new("wallet.duplicate_wallet", "Wallet already exists.");
    public static readonly Error WalletNotFound = new("wallet.not_found", "Wallet was not found.");
    public static readonly Error WalletVersionConflict = new("wallet.version_conflict", "Wallet projection version is out of sequence.");
    public static readonly Error InsufficientProjectedBalance = new("wallet.insufficient_balance", "Wallet projection balance is insufficient.");
}

