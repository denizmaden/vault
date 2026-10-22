namespace Vault.Services.Ledger.Application.Models;

public sealed record LedgerAccountDetails(
    Guid LedgerAccountId,
    Guid OwnerId,
    string Currency,
    decimal Balance,
    string Status,
    int Version,
    DateTime OpenedOnUtc);

