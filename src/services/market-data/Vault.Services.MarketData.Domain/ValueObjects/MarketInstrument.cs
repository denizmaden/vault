using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.MarketData.Domain.Enums;
using Vault.Services.MarketData.Domain.Errors;

namespace Vault.Services.MarketData.Domain.ValueObjects;

public sealed class MarketInstrument : ValueObject
{
    private MarketInstrument(
        string symbol,
        string displayName,
        string baseAsset,
        string quoteAsset,
        MarketInstrumentKind kind,
        string providerSymbol)
    {
        Symbol = symbol;
        DisplayName = displayName;
        BaseAsset = baseAsset;
        QuoteAsset = quoteAsset;
        Kind = kind;
        ProviderSymbol = providerSymbol;
    }

    public string Symbol { get; }

    public string DisplayName { get; }

    public string BaseAsset { get; }

    public string QuoteAsset { get; }

    public MarketInstrumentKind Kind { get; }

    public string ProviderSymbol { get; }

    public static Result<MarketInstrument> Create(
        string? symbol,
        string? displayName,
        string? baseAsset,
        string? quoteAsset,
        MarketInstrumentKind kind,
        string? providerSymbol)
    {
        if (string.IsNullOrWhiteSpace(symbol)
            || string.IsNullOrWhiteSpace(displayName)
            || string.IsNullOrWhiteSpace(baseAsset)
            || string.IsNullOrWhiteSpace(quoteAsset)
            || string.IsNullOrWhiteSpace(providerSymbol))
        {
            return Result<MarketInstrument>.Failure(MarketDataErrors.InvalidInstrument);
        }

        return Result<MarketInstrument>.Success(
            new MarketInstrument(
                symbol.Trim().ToUpperInvariant(),
                displayName.Trim(),
                baseAsset.Trim().ToUpperInvariant(),
                quoteAsset.Trim().ToUpperInvariant(),
                kind,
                providerSymbol.Trim()));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Symbol;
        yield return ProviderSymbol;
    }
}
