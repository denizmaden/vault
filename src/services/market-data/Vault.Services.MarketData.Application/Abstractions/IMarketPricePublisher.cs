using Vault.Services.MarketData.Domain.Entities;

namespace Vault.Services.MarketData.Application.Abstractions;

public interface IMarketPricePublisher
{
    Task PublishAsync(MarketPrice price, CancellationToken cancellationToken = default);
}
