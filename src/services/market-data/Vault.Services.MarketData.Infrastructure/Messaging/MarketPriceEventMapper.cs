using Vault.Services.MarketData.Domain.Entities;
using Vault.Services.MarketData.Domain.Enums;
using Vault.Services.MarketData.Domain.ValueObjects;

namespace Vault.Services.MarketData.Infrastructure.Messaging;

internal static class MarketPriceEventMapper
{
    public static MarketPriceUpdatedIntegrationEvent ToIntegrationEvent(this MarketPrice price) =>
        new(
            price.Instrument.Symbol,
            price.Instrument.DisplayName,
            price.Instrument.BaseAsset,
            price.Instrument.QuoteAsset,
            price.Instrument.Kind.ToString(),
            price.Instrument.ProviderSymbol,
            price.Price,
            price.ChangePercent24h,
            price.Source.ToString(),
            price.OccurredOnUtc,
            price.ReceivedOnUtc);

    public static MarketPrice ToDomain(this MarketPriceUpdatedIntegrationEvent message)
    {
        if (!Enum.TryParse<MarketInstrumentKind>(message.Kind, ignoreCase: true, out var kind))
        {
            throw new InvalidOperationException($"Unknown market instrument kind '{message.Kind}'.");
        }

        if (!Enum.TryParse<MarketPriceSource>(message.Source, ignoreCase: true, out var source))
        {
            throw new InvalidOperationException($"Unknown market price source '{message.Source}'.");
        }

        var instrument = MarketInstrument.Create(
            message.Symbol,
            message.DisplayName,
            message.BaseAsset,
            message.QuoteAsset,
            kind,
            message.ProviderSymbol);

        if (instrument.IsFailure)
        {
            throw new InvalidOperationException(instrument.Error.Description);
        }

        var price = MarketPrice.Create(
            instrument.Value,
            message.Price,
            message.ChangePercent24h,
            source,
            message.OccurredOnUtc,
            message.ReceivedOnUtc);

        return price.IsSuccess
            ? price.Value
            : throw new InvalidOperationException(price.Error.Description);
    }
}
