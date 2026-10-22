namespace Vault.Services.Wallet.Api.Contracts;

public sealed record ProjectMoneyMovementRequest(
    Guid WalletId,
    decimal Amount,
    int Version,
    DateTime OccurredOnUtc);

