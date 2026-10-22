using Microsoft.Extensions.Caching.Memory;
using Vault.Platform.Caching.Abstractions;

namespace Vault.Platform.Caching.InMemory;

public sealed class InMemoryCacheStore(IMemoryCache memoryCache) : ICacheStore
{
    public Task<TValue?> GetAsync<TValue>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(memoryCache.TryGetValue<TValue>(key, out var value) ? value : default);
    }

    public Task SetAsync<TValue>(
        string key,
        TValue value,
        TimeSpan absoluteExpirationRelativeToNow,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);
        cancellationToken.ThrowIfCancellationRequested();

        if (absoluteExpirationRelativeToNow <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(absoluteExpirationRelativeToNow),
                "Cache expiration must be greater than zero.");
        }

        memoryCache.Set(key, value, absoluteExpirationRelativeToNow);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();
        memoryCache.Remove(key);
        return Task.CompletedTask;
    }
}
