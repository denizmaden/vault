namespace Vault.Services.Reporting.Api.Contracts;

public sealed record WalletActivityReportResponse(
    Guid WalletId,
    Guid OwnerId,
    string Currency,
    decimal CurrentBalance,
    decimal TotalDeposited,
    decimal TotalWithdrawn,
    int Version,
    DateTime OpenedOnUtc,
    DateTime LastUpdatedOnUtc,
    IReadOnlyCollection<ActivityEntryResponse> Entries);
