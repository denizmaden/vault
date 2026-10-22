namespace Vault.Platform.Caching.Redis.Configuration;

public sealed class RedisCacheSettings
{
    public string ConnectionString { get; init; } = null!;

    public string InstanceName { get; init; } = "vault:";
}
