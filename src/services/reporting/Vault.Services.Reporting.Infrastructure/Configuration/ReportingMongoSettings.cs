namespace Vault.Services.Reporting.Infrastructure.Configuration;

public sealed class ReportingMongoSettings
{
    public const string SectionName = "Mongo";

    public string ConnectionString { get; init; } = null!;

    public string DatabaseName { get; init; } = null!;

    public string ReportsCollectionName { get; init; } = "wallet_activity_reports";
}
