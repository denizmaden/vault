namespace Vault.Services.Wallet.Api.Contracts;

public sealed record WalletResponse(
    Guid WalletId,
    Guid OwnerId,
    string Currency,
    decimal Balance,
    string Status,
    int Version,
    DateTime OpenedOnUtc,
    DateTime LastSyncedOnUtc);

