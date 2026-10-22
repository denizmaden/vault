namespace Vault.Services.Ledger.Api.Contracts;

public sealed record LedgerEventResponse(
    Guid EventId,
    string EventType,
    int Version,
    DateTime OccurredOnUtc,
    string Data);

