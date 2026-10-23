namespace Vault.Services.MarketData.Application.Models;

public sealed record MarketPriceDto(
    string Symbol,
    string DisplayName,
    string BaseAsset,
    string QuoteAsset,
    string Kind,
    decimal Price,
    decimal? ChangePercent24h,
    string Source,
    DateTime OccurredOnUtc,
    DateTime ReceivedOnUtc);
