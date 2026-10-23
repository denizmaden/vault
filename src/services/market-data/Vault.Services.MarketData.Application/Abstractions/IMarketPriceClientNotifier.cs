using Vault.Services.MarketData.Domain.Entities;

namespace Vault.Services.MarketData.Application.Abstractions;

public interface IMarketPriceClientNotifier
{
    Task NotifyPriceUpdatedAsync(MarketPrice price, CancellationToken cancellationToken = default);
}
