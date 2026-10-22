namespace Vault.Platform.Caching.Configuration;

public sealed class VaultCachingOptions
{
    public const string SectionName = "Caching";

    public bool Enabled { get; init; } = true;

    public string Provider { get; init; } = "Redis";

    public VaultRedisOptions Redis { get; init; } = new();
}

public sealed class VaultRedisOptions
{
    public string ConnectionString { get; init; } = null!;

    public string InstanceName { get; init; } = "vault:";
}
