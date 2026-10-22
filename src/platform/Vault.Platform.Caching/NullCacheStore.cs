using Vault.Platform.Caching.Abstractions;

namespace Vault.Platform.Caching;

internal sealed class NullCacheStore : ICacheStore
{
    public Task<TValue?> GetAsync<TValue>(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<TValue?>(default);
    }

    public Task SetAsync<TValue>(
        string key,
        TValue value,
        TimeSpan absoluteExpirationRelativeToNow,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
