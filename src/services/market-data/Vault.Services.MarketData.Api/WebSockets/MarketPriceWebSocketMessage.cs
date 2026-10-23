namespace Vault.Services.MarketData.Api.WebSockets;

public sealed record MarketPriceWebSocketMessage<TPayload>(
    string Type,
    DateTime SentOnUtc,
    TPayload Payload);
