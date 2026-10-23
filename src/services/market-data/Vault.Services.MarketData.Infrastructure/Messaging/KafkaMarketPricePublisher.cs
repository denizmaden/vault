using System.Text.Json;
using Vault.Platform.Messaging.Abstractions;
using Vault.Services.MarketData.Application.Abstractions;
using Vault.Services.MarketData.Domain.Entities;
using Vault.Services.MarketData.Infrastructure.Configuration;

namespace Vault.Services.MarketData.Infrastructure.Messaging;

public sealed class KafkaMarketPricePublisher(
    IMessagePublisher publisher,
    MarketDataMessagingSettings settings) : IMarketPricePublisher
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public Task PublishAsync(MarketPrice price, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(price);

        var @event = price.ToIntegrationEvent();
        var envelope = new MessageEnvelope(
            settings.PricesDestination,
            price.Instrument.Symbol,
            nameof(MarketPriceUpdatedIntegrationEvent),
            JsonSerializer.Serialize(@event, JsonSerializerOptions),
            price.OccurredOnUtc,
            new Dictionary<string, string>
            {
                ["schema-version"] = "1",
                ["market-source"] = price.Source.ToString()
            });

        return publisher.PublishAsync(envelope, cancellationToken);
    }
}
