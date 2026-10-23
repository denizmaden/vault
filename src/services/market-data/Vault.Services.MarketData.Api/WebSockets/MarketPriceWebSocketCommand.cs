namespace Vault.Services.MarketData.Api.WebSockets;

public sealed record MarketPriceWebSocketCommand(
    string Type,
    IReadOnlyCollection<string>? Symbols = null);
