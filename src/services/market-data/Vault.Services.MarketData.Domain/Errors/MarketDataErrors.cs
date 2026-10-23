using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.MarketData.Domain.Errors;

public static class MarketDataErrors
{
    public static readonly Error InvalidInstrument = new("market_data.invalid_instrument", "Market instrument is invalid.");

    public static readonly Error InvalidPrice = new("market_data.invalid_price", "Market price must be greater than zero.");
}
