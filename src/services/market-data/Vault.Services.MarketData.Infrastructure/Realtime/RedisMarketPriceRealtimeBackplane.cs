using System.Text.Json;
using System.Threading.Channels;
using StackExchange.Redis;
using Vault.Services.MarketData.Application.Abstractions;
using Vault.Services.MarketData.Domain.Entities;
using Vault.Services.MarketData.Infrastructure.Configuration;
using Vault.Services.MarketData.Infrastructure.Messaging;

namespace Vault.Services.MarketData.Infrastructure.Realtime;

public sealed class RedisMarketPriceRealtimeBackplane(
    IConnectionMultiplexer connectionMultiplexer,
    MarketDataRedisSettings settings) : IMarketPriceRealtimeBackplane
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task PublishAsync(MarketPrice price, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(price);
        cancellationToken.ThrowIfCancellationRequested();

        var payload = JsonSerializer.Serialize(price.ToIntegrationEvent(), JsonSerializerOptions);
        await connectionMultiplexer
            .GetSubscriber()
            .PublishAsync(RedisChannel.Literal(settings.RealtimeChannel), payload);
    }

    public async Task SubscribeAsync(
        Func<MarketPrice, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var messages = Channel.CreateBounded<RedisValue>(new BoundedChannelOptions(4096)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropOldest
        });
        var channel = RedisChannel.Literal(settings.RealtimeChannel);
        var subscriber = connectionMultiplexer.GetSubscriber();

        await subscriber.SubscribeAsync(channel, (_, value) => messages.Writer.TryWrite(value));

        try
        {
            await foreach (var payload in messages.Reader.ReadAllAsync(cancellationToken))
            {
                var @event = JsonSerializer.Deserialize<MarketPriceUpdatedIntegrationEvent>(
                    payload.ToString(),
                    JsonSerializerOptions);

                if (@event is not null)
                {
                    await handler(@event.ToDomain(), cancellationToken);
                }
            }
        }
        finally
        {
            messages.Writer.TryComplete();
            await subscriber.UnsubscribeAsync(channel);
        }
    }
}
