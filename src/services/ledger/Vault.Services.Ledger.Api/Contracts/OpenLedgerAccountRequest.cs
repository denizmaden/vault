namespace Vault.Services.Ledger.Api.Contracts;

public sealed record OpenLedgerAccountRequest(
    Guid LedgerAccountId,
    Guid OwnerId,
    string Currency);

