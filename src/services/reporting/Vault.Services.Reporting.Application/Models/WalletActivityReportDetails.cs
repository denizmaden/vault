namespace Vault.Services.Reporting.Application.Models;

public sealed record WalletActivityReportDetails(
    Guid WalletId,
    Guid OwnerId,
    string Currency,
    decimal CurrentBalance,
    decimal TotalDeposited,
    decimal TotalWithdrawn,
    int Version,
    DateTime OpenedOnUtc,
    DateTime LastUpdatedOnUtc,
    IReadOnlyCollection<ActivityEntryDetails> Entries);
