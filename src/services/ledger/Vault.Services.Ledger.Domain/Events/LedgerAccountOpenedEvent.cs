namespace Vault.Services.Ledger.Domain.Events;

public sealed record LedgerAccountOpenedEvent(
    Guid LedgerAccountId,
    Guid OwnerId,
    string Currency,
    DateTime OccurredOnUtc) : LedgerAccountEvent(LedgerAccountId, OccurredOnUtc);

