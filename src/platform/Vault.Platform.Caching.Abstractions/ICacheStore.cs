namespace Vault.Platform.Caching.Abstractions;

public interface ICacheStore
{
    Task<TValue?> GetAsync<TValue>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<TValue>(
        string key,
        TValue value,
        TimeSpan absoluteExpirationRelativeToNow,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
