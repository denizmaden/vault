using Vault.Services.MarketData.Application.Abstractions;
using Vault.Services.MarketData.Domain.Enums;
using Vault.Services.MarketData.Domain.ValueObjects;

namespace Vault.Services.MarketData.Infrastructure.Configuration;

public sealed class MarketInstrumentCatalog : IMarketInstrumentCatalog
{
    private readonly IReadOnlyDictionary<string, MarketInstrument> _instruments;

    public MarketInstrumentCatalog(
        BinanceMarketDataSettings binanceSettings,
        FinnhubMarketDataSettings finnhubSettings)
    {
        _instruments = binanceSettings.Instruments
            .Select(instrument => Create(
                instrument.Symbol,
                instrument.DisplayName,
                instrument.BaseAsset,
                instrument.QuoteAsset,
                MarketInstrumentKind.Crypto,
                instrument.ProviderSymbol))
            .Concat(finnhubSettings.Instruments.Select(instrument => Create(
                instrument.Symbol,
                instrument.DisplayName,
                instrument.BaseAsset,
                instrument.QuoteAsset,
                MarketInstrumentKind.Forex,
                instrument.ProviderSymbol)))
            .Where(instrument => instrument is not null)
            .Cast<MarketInstrument>()
            .GroupBy(instrument => instrument.Symbol, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToDictionary(instrument => instrument.Symbol, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<MarketInstrument> Instruments => _instruments.Values.ToArray();

    public bool Contains(string symbol) =>
        !string.IsNullOrWhiteSpace(symbol) && _instruments.ContainsKey(symbol.Trim());

    private static MarketInstrument? Create(
        string symbol,
        string displayName,
        string baseAsset,
        string quoteAsset,
        MarketInstrumentKind kind,
        string providerSymbol)
    {
        var result = MarketInstrument.Create(symbol, displayName, baseAsset, quoteAsset, kind, providerSymbol);
        return result.IsSuccess ? result.Value : null;
    }
}
