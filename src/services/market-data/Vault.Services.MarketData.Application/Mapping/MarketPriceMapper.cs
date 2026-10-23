using Vault.Services.MarketData.Application.Models;
using Vault.Services.MarketData.Domain.Entities;

namespace Vault.Services.MarketData.Application.Mapping;

public static class MarketPriceMapper
{
    public static MarketPriceDto ToDto(this MarketPrice price) =>
        new(
            price.Instrument.Symbol,
            price.Instrument.DisplayName,
            price.Instrument.BaseAsset,
            price.Instrument.QuoteAsset,
            price.Instrument.Kind.ToString(),
            price.Price,
            price.ChangePercent24h,
            price.Source.ToString(),
            price.OccurredOnUtc,
            price.ReceivedOnUtc);
}
