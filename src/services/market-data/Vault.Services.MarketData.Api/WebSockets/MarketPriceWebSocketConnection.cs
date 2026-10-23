using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Channels;

namespace Vault.Services.MarketData.Api.WebSockets;

public sealed class MarketPriceWebSocketConnection : IDisposable
{
    private readonly ConcurrentDictionary<string, byte> _subscriptions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Channel<string> _outboundMessages;
    private readonly int _commandsPerMinute;
    private readonly object _commandRateLock = new();
    private long _droppedMessageCount;
    private long _commandWindowStartedAt = Environment.TickCount64;
    private int _commandsInCurrentWindow;

    public MarketPriceWebSocketConnection(
        Guid id,
        WebSocket webSocket,
        int outboundQueueCapacity,
        int commandsPerMinute)
    {
        Id = id;
        WebSocket = webSocket;
        _commandsPerMinute = commandsPerMinute;
        _outboundMessages = Channel.CreateBounded<string>(new BoundedChannelOptions(outboundQueueCapacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    public Guid Id { get; }

    public WebSocket WebSocket { get; }

    public ChannelReader<string> OutboundMessages => _outboundMessages.Reader;

    public IReadOnlyCollection<string> Subscriptions => _subscriptions.Keys.ToArray();

    public long DroppedMessageCount => Interlocked.Read(ref _droppedMessageCount);

    public void Subscribe(IEnumerable<string> symbols)
    {
        foreach (var symbol in symbols)
        {
            _subscriptions.TryAdd(symbol, 0);
        }
    }

    public void Unsubscribe(IEnumerable<string> symbols)
    {
        foreach (var symbol in symbols)
        {
            _subscriptions.TryRemove(symbol, out _);
        }
    }

    public bool IsSubscribedTo(string symbol) => _subscriptions.ContainsKey(symbol);

    public bool TryConsumeCommand()
    {
        lock (_commandRateLock)
        {
            var now = Environment.TickCount64;
            if (now - _commandWindowStartedAt >= 60_000)
            {
                _commandWindowStartedAt = now;
                _commandsInCurrentWindow = 0;
            }

            if (_commandsInCurrentWindow >= _commandsPerMinute)
            {
                return false;
            }

            _commandsInCurrentWindow++;
            return true;
        }
    }

    public bool TryQueue(string payload)
    {
        if (_outboundMessages.Writer.TryWrite(payload))
        {
            return true;
        }

        Interlocked.Increment(ref _droppedMessageCount);
        MarketDataWebSocketTelemetry.DroppedMessages.Add(1);
        return false;
    }

    public void Complete() => _outboundMessages.Writer.TryComplete();

    public void Dispose()
    {
        Complete();
        WebSocket.Dispose();
    }
}
