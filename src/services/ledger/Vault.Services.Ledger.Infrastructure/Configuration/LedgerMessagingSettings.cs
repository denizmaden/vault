namespace Vault.Services.Ledger.Infrastructure.Configuration;

public sealed class LedgerMessagingSettings
{
    public const string SectionName = "LedgerMessaging";

    public string EventsDestination { get; init; } = "vault.ledger.events";
}
