namespace Vault.Services.Ledger.Api.Contracts;

public sealed record LedgerAccountResponse(
    Guid LedgerAccountId,
    Guid OwnerId,
    string Currency,
    decimal Balance,
    string Status,
    int Version,
    DateTime OpenedOnUtc);

