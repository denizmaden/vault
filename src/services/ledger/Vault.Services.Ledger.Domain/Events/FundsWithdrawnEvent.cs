namespace Vault.Services.Ledger.Domain.Events;

public sealed record FundsWithdrawnEvent(
    Guid LedgerAccountId,
    decimal Amount,
    string Reference,
    DateTime OccurredOnUtc) : LedgerAccountEvent(LedgerAccountId, OccurredOnUtc);

