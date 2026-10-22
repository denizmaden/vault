namespace Vault.Services.Ledger.Domain.Events;

public sealed record FundsDepositedEvent(
    Guid LedgerAccountId,
    decimal Amount,
    string Reference,
    DateTime OccurredOnUtc) : LedgerAccountEvent(LedgerAccountId, OccurredOnUtc);

