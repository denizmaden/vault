namespace Vault.Services.MarketData.Application.Models;

public sealed record MarketPriceSnapshotDto(
    DateTime SentOnUtc,
    IReadOnlyCollection<MarketPriceDto> Prices);
