namespace Vault.Services.Wallet.Infrastructure.Configuration;

public sealed class WalletMessagingSettings
{
    public const string SectionName = "WalletMessaging";

    public string LedgerEventsDestination { get; init; } = "vault.ledger.events";

    public string ConsumerGroupId { get; init; } = "vault-wallet-projection";
}
