namespace Vault.Services.Ledger.Application.Models;

public sealed record LedgerEventDetails(
    Guid EventId,
    string EventType,
    int Version,
    DateTime OccurredOnUtc,
    string Data);

