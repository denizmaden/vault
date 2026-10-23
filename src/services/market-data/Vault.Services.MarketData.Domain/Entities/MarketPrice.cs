using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.MarketData.Domain.Enums;
using Vault.Services.MarketData.Domain.Errors;
using Vault.Services.MarketData.Domain.ValueObjects;

namespace Vault.Services.MarketData.Domain.Entities;

public sealed class MarketPrice
{
    private MarketPrice(
        MarketInstrument instrument,
        decimal price,
        decimal? changePercent24h,
        MarketPriceSource source,
        DateTime occurredOnUtc,
        DateTime receivedOnUtc)
    {
        Instrument = instrument;
        Price = decimal.Round(price, 8, MidpointRounding.ToZero);
        ChangePercent24h = changePercent24h is null
            ? null
            : decimal.Round(changePercent24h.Value, 4, MidpointRounding.ToZero);
        Source = source;
        OccurredOnUtc = occurredOnUtc;
        ReceivedOnUtc = receivedOnUtc;
    }

    public MarketInstrument Instrument { get; }

    public decimal Price { get; }

    public decimal? ChangePercent24h { get; }

    public MarketPriceSource Source { get; }

    public DateTime OccurredOnUtc { get; }

    public DateTime ReceivedOnUtc { get; }

    public static Result<MarketPrice> Create(
        MarketInstrument instrument,
        decimal price,
        decimal? changePercent24h,
        MarketPriceSource source,
        DateTime occurredOnUtc,
        DateTime receivedOnUtc)
    {
        ArgumentNullException.ThrowIfNull(instrument);

        if (price <= 0)
        {
            return Result<MarketPrice>.Failure(MarketDataErrors.InvalidPrice);
        }

        return Result<MarketPrice>.Success(
            new MarketPrice(
                instrument,
                price,
                changePercent24h,
                source,
                occurredOnUtc,
                receivedOnUtc));
    }
}
