namespace Vault.Services.Ledger.Application.Models;

public sealed record LedgerAccountOpened(
    Guid LedgerAccountId,
    Guid OwnerId,
    string Currency,
    string Status);

