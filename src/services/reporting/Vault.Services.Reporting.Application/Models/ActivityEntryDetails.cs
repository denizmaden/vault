namespace Vault.Services.Reporting.Application.Models;

public sealed record ActivityEntryDetails(
    string Type,
    decimal Amount,
    string Reference,
    int Version,
    DateTime OccurredOnUtc);
