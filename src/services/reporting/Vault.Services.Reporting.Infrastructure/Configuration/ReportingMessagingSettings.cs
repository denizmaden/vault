namespace Vault.Services.Reporting.Infrastructure.Configuration;

public sealed class ReportingMessagingSettings
{
    public const string SectionName = "ReportingMessaging";

    public string LedgerEventsDestination { get; init; } = "vault.ledger.events";

    public string ConsumerGroupId { get; init; } = "vault-reporting-projection";
}
