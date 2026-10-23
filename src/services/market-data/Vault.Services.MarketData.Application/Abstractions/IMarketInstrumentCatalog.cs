using Vault.Services.MarketData.Domain.ValueObjects;

namespace Vault.Services.MarketData.Application.Abstractions;

public interface IMarketInstrumentCatalog
{
    IReadOnlyCollection<MarketInstrument> Instruments { get; }

    bool Contains(string symbol);
}
