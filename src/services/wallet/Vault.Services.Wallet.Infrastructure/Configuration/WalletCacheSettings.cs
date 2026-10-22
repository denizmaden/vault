namespace Vault.Services.Wallet.Infrastructure.Configuration;

public sealed class WalletCacheSettings
{
    public const string SectionName = "WalletCache";

    public int WalletDetailsTtlSeconds { get; init; } = 300;

    public TimeSpan WalletDetailsTtl =>
        TimeSpan.FromSeconds(WalletDetailsTtlSeconds > 0 ? WalletDetailsTtlSeconds : 300);
}
