using System.Diagnostics.Metrics;

namespace Vault.Services.MarketData.Api.WebSockets;

internal static class MarketDataWebSocketTelemetry
{
    private static readonly Meter Meter = new("Vault.MarketData");

    public static readonly UpDownCounter<long> ActiveConnections =
        Meter.CreateUpDownCounter<long>("vault.market_data.websocket.active_connections");

    public static readonly Counter<long> SentMessages =
        Meter.CreateCounter<long>("vault.market_data.websocket.messages.sent");

    public static readonly Counter<long> DroppedMessages =
        Meter.CreateCounter<long>("vault.market_data.websocket.messages.dropped");
}
