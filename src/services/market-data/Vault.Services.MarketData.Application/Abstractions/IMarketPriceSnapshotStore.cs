using Vault.Services.MarketData.Domain.Entities;

namespace Vault.Services.MarketData.Application.Abstractions;

public interface IMarketPriceSnapshotStore
{
    Task<bool> UpsertAsync(MarketPrice price, CancellationToken cancellationToken = default);

    Task<MarketPrice?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MarketPrice>> GetSnapshotAsync(
        IReadOnlyCollection<string>? symbols = null,
        CancellationToken cancellationToken = default);
}
