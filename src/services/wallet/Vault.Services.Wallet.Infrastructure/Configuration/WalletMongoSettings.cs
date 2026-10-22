namespace Vault.Services.Wallet.Infrastructure.Configuration;

public sealed class WalletMongoSettings
{
    public const string SectionName = "Mongo";

    public string ConnectionString { get; init; } = null!;

    public string DatabaseName { get; init; } = null!;

    public string WalletsCollectionName { get; init; } = "wallets";
}

