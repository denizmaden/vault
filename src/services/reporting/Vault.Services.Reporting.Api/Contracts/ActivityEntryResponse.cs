namespace Vault.Services.Reporting.Api.Contracts;

public sealed record ActivityEntryResponse(
    string Type,
    decimal Amount,
    string Reference,
    int Version,
    DateTime OccurredOnUtc);
