using System.Text.Json;
using StackExchange.Redis;
using Vault.Services.MarketData.Application.Abstractions;
using Vault.Services.MarketData.Domain.Entities;
using Vault.Services.MarketData.Infrastructure.Configuration;
using Vault.Services.MarketData.Infrastructure.Messaging;

namespace Vault.Services.MarketData.Infrastructure.Persistence;

public sealed class RedisMarketPriceSnapshotStore(
    IConnectionMultiplexer connectionMultiplexer,
    MarketDataRedisSettings settings) : IMarketPriceSnapshotStore
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);
    private const string UpsertIfNewerScript = """
        local currentTimestamp = redis.call('HGET', KEYS[2], ARGV[1])
        if currentTimestamp and tonumber(currentTimestamp) >= tonumber(ARGV[2]) then
            return 0
        end

        redis.call('HSET', KEYS[1], ARGV[1], ARGV[3])
        redis.call('HSET', KEYS[2], ARGV[1], ARGV[2])
        return 1
        """;

    public async Task<bool> UpsertAsync(MarketPrice price, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(price);
        cancellationToken.ThrowIfCancellationRequested();

        var payload = JsonSerializer.Serialize(price.ToIntegrationEvent(), JsonSerializerOptions);
        var receivedOnUtc = price.ReceivedOnUtc.Kind == DateTimeKind.Utc
            ? price.ReceivedOnUtc
            : price.ReceivedOnUtc.ToUniversalTime();
        var timestamp = new DateTimeOffset(receivedOnUtc).ToUnixTimeMilliseconds();
        var result = await connectionMultiplexer.GetDatabase().ScriptEvaluateAsync(
            UpsertIfNewerScript,
            [settings.SnapshotHashKey, settings.SnapshotTimestampHashKey],
            [price.Instrument.Symbol, timestamp, payload]);

        return (long)result == 1;
    }

    public async Task<MarketPrice?> GetBySymbolAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        cancellationToken.ThrowIfCancellationRequested();

        var value = await connectionMultiplexer
            .GetDatabase()
            .HashGetAsync(settings.SnapshotHashKey, symbol.Trim().ToUpperInvariant());

        return Deserialize(value);
    }

    public async Task<IReadOnlyCollection<MarketPrice>> GetSnapshotAsync(
        IReadOnlyCollection<string>? symbols = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var database = connectionMultiplexer.GetDatabase();

        IEnumerable<RedisValue> values;
        if (symbols is { Count: > 0 })
        {
            var fields = symbols
                .Where(symbol => !string.IsNullOrWhiteSpace(symbol))
                .Select(symbol => (RedisValue)symbol.Trim().ToUpperInvariant())
                .Distinct()
                .ToArray();
            values = await database.HashGetAsync(settings.SnapshotHashKey, fields);
        }
        else
        {
            values = (await database.HashGetAllAsync(settings.SnapshotHashKey))
                .Select(entry => entry.Value);
        }

        return values
            .Select(Deserialize)
            .Where(price => price is not null)
            .Cast<MarketPrice>()
            .OrderBy(price => price.Instrument.Kind)
            .ThenBy(price => price.Instrument.Symbol)
            .ToArray();
    }

    private static MarketPrice? Deserialize(RedisValue value)
    {
        if (value.IsNullOrEmpty)
        {
            return null;
        }

        var @event = JsonSerializer.Deserialize<MarketPriceUpdatedIntegrationEvent>(
            value.ToString(),
            JsonSerializerOptions);
        return @event?.ToDomain();
    }
}
