namespace Vault.Services.Ledger.Api.Contracts;

public sealed record OpenLedgerAccountResponse(
    Guid LedgerAccountId,
    Guid OwnerId,
    string Currency,
    string Status);

