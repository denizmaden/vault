using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Reporting.Domain.Errors;

public static class ReportingErrors
{
    public static readonly Error InvalidWalletId = new("reporting.invalid_wallet_id", "Wallet id is required.");
    public static readonly Error InvalidOwnerId = new("reporting.invalid_owner_id", "Owner id is required.");
    public static readonly Error InvalidCurrency = new("reporting.invalid_currency", "Currency is invalid.");
    public static readonly Error InvalidAmount = new("reporting.invalid_amount", "Amount must be greater than zero.");
    public static readonly Error InvalidVersion = new("reporting.invalid_version", "Version must be greater than zero.");
    public static readonly Error VersionConflict = new("reporting.version_conflict", "Projection version is out of sequence.");
    public static readonly Error DuplicateReport = new("reporting.duplicate_report", "Wallet activity report already exists.");
    public static readonly Error ReportNotFound = new("reporting.report_not_found", "Wallet activity report was not found.");
    public static readonly Error InvalidReference = new("reporting.invalid_reference", "Reference is invalid.");
    public static readonly Error NegativeProjectedBalance = new("reporting.negative_projected_balance", "Projected balance cannot be negative.");
}
