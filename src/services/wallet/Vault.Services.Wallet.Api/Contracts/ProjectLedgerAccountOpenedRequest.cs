namespace Vault.Services.Wallet.Api.Contracts;

public sealed record ProjectLedgerAccountOpenedRequest(
    Guid WalletId,
    Guid OwnerId,
    string Currency,
    int Version,
    DateTime OccurredOnUtc);

