namespace Vault.Services.MarketData.Api.WebSockets;

public sealed class MarketDataWebSocketSettings
{
    public const string SectionName = "MarketDataWebSockets";

    public int MaxConnections { get; init; } = 5000;

    public int MaxConnectionsPerClient { get; init; } = 5;

    public int OutboundQueueCapacity { get; init; } = 256;

    public int MaxIncomingMessageBytes { get; init; } = 16 * 1024;

    public int CommandsPerMinute { get; init; } = 120;
}
