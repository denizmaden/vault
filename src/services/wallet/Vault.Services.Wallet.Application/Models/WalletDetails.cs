namespace Vault.Services.Wallet.Application.Models;

public sealed record WalletDetails(
    Guid WalletId,
    Guid OwnerId,
    string Currency,
    decimal Balance,
    string Status,
    int Version,
    DateTime OpenedOnUtc,
    DateTime LastSyncedOnUtc);

