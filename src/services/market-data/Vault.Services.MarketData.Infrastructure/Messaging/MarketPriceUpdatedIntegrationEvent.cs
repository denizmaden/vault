namespace Vault.Services.MarketData.Infrastructure.Messaging;

public sealed record MarketPriceUpdatedIntegrationEvent(
    string Symbol,
    string DisplayName,
    string BaseAsset,
    string QuoteAsset,
    string Kind,
    string ProviderSymbol,
    decimal Price,
    decimal? ChangePercent24h,
    string Source,
    DateTime OccurredOnUtc,
    DateTime ReceivedOnUtc);
