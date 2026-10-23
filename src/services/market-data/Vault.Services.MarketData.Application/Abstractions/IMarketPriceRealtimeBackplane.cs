using Vault.Services.MarketData.Domain.Entities;

namespace Vault.Services.MarketData.Application.Abstractions;

public interface IMarketPriceRealtimeBackplane
{
    Task PublishAsync(MarketPrice price, CancellationToken cancellationToken = default);

    Task SubscribeAsync(
        Func<MarketPrice, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default);
}
