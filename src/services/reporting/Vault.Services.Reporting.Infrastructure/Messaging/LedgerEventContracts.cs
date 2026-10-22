namespace Vault.Services.Reporting.Infrastructure.Messaging;

public sealed record LedgerAccountOpenedIntegrationEvent(
    Guid LedgerAccountId,
    Guid OwnerId,
    string Currency,
    DateTime OccurredOnUtc);

public sealed record FundsDepositedIntegrationEvent(
    Guid LedgerAccountId,
    decimal Amount,
    string Reference,
    DateTime OccurredOnUtc);

public sealed record FundsWithdrawnIntegrationEvent(
    Guid LedgerAccountId,
    decimal Amount,
    string Reference,
    DateTime OccurredOnUtc);
