using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Ledger.Domain.Errors;

public static class LedgerErrors
{
    public static readonly Error InvalidLedgerAccountId = new("ledger.invalid_account_id", "Ledger account id is invalid.");
    public static readonly Error InvalidOwnerId = new("ledger.invalid_owner_id", "Ledger account owner id is invalid.");
    public static readonly Error InvalidCurrency = new("ledger.invalid_currency", "Currency is invalid.");
    public static readonly Error InvalidAmount = new("ledger.invalid_amount", "Amount must be greater than zero.");
    public static readonly Error LedgerAccountNotFound = new("ledger.not_found", "Ledger account was not found.");
    public static readonly Error DuplicateLedgerAccount = new("ledger.duplicate_account", "Ledger account already exists.");
    public static readonly Error InsufficientFunds = new("ledger.insufficient_funds", "Ledger account has insufficient funds.");
    public static readonly Error UnsupportedEventType = new("ledger.unsupported_event_type", "Ledger event type is not supported.");
    public static readonly Error InvalidEventStream = new("ledger.invalid_event_stream", "Ledger event stream is invalid.");
}

