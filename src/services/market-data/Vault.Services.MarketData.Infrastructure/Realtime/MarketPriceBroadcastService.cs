using Microsoft.Extensions.Hosting;
using Vault.Services.MarketData.Application.Abstractions;
using Vault.Services.MarketData.Domain.Entities;

namespace Vault.Services.MarketData.Infrastructure.Realtime;

public sealed class MarketPriceBroadcastService(
    IMarketPriceRealtimeBackplane backplane,
    IEnumerable<IMarketPriceClientNotifier> clientNotifiers) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        backplane.SubscribeAsync(NotifyClientsAsync, stoppingToken);

    private async Task NotifyClientsAsync(MarketPrice price, CancellationToken cancellationToken)
    {
        foreach (var notifier in clientNotifiers)
        {
            await notifier.NotifyPriceUpdatedAsync(price, cancellationToken);
        }
    }
}
