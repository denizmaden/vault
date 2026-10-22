using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Vault.Platform.Caching.Abstractions;

namespace Vault.Platform.Caching.Redis;

public sealed class RedisCacheStore(IDistributedCache distributedCache) : ICacheStore
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<TValue?> GetAsync<TValue>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var payload = await distributedCache.GetStringAsync(key, cancellationToken);
        return string.IsNullOrWhiteSpace(payload)
            ? default
            : JsonSerializer.Deserialize<TValue>(payload, JsonSerializerOptions);
    }

    public Task SetAsync<TValue>(
        string key,
        TValue value,
        TimeSpan absoluteExpirationRelativeToNow,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        if (absoluteExpirationRelativeToNow <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(absoluteExpirationRelativeToNow),
                "Cache expiration must be greater than zero.");
        }

        var payload = JsonSerializer.Serialize(value, JsonSerializerOptions);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
        };

        return distributedCache.SetStringAsync(key, payload, options, cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return distributedCache.RemoveAsync(key, cancellationToken);
    }
}
