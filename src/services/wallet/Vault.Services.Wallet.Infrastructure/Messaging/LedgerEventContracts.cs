namespace Vault.Services.Wallet.Infrastructure.Messaging;

internal sealed record LedgerAccountOpenedIntegrationEvent(
    Guid LedgerAccountId,
    Guid OwnerId,
    string Currency,
    DateTime OccurredOnUtc);

internal sealed record FundsDepositedIntegrationEvent(
    Guid LedgerAccountId,
    decimal Amount,
    string Reference,
    DateTime OccurredOnUtc);

internal sealed record FundsWithdrawnIntegrationEvent(
    Guid LedgerAccountId,
    decimal Amount,
    string Reference,
    DateTime OccurredOnUtc);

