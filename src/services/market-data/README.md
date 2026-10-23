# Vault MarketData

MarketData normalizes external price feeds and exposes low-latency, symbol-filtered updates to browser and service clients.

## Data flow

1. Binance and Finnhub WebSocket clients normalize provider payloads into the `MarketPrice` domain model.
2. Normalized ticks are published to the `vault.market-data.prices` Kafka topic, keyed by symbol.
3. The `vault-market-data-projection` consumer group updates the latest-price Redis snapshot.
4. The projection publishes the committed price to Redis Pub/Sub.
5. Every API instance receives the Pub/Sub update and queues it only for local WebSocket clients subscribed to that symbol.

Kafka provides a durable, replayable integration stream. Redis provides fast latest-price reads and a scale-out backplane. A reconnecting WebSocket client receives its current snapshot from Redis, so missing an ephemeral Pub/Sub notification does not leave it without state.

## Failure handling and recovery

- Projection failures are retried 5 times with exponential backoff.
- A message that still cannot be processed is wrapped with its original payload, headers, error, and attempt count, then published to `vault.market-data.prices.dlq`.
- The Kafka source offset is committed only after processing or DLQ publication succeeds. If Kafka is temporarily unavailable during DLQ publication, the service keeps retrying instead of silently losing the message.
- `vault.market-data.prices` uses Kafka log compaction and is keyed by symbol. When the Redis snapshot is empty at startup, it is rebuilt from the Kafka topic before normal consumers and provider streams start.
- Invalid historical Kafka records are logged and skipped during rebuild, so one poison record cannot prevent service startup.
- Redis updates use an atomic Lua script. An older or duplicate `ReceivedOnUtc` value cannot overwrite a newer price, even when API replicas process messages concurrently.

## Scaling

Run one deployment with `MarketDataIngestion:Enabled=true` to own external provider connections. Additional API replicas should use `MarketDataIngestion:Enabled=false`. Projection consumers share one Kafka consumer group, while Redis Pub/Sub fans every processed update out to every API replica.

Each WebSocket connection has a bounded outbound queue. A slow client cannot block provider ingestion, Kafka projection, or other clients.

## Security and quotas

Market prices are public by default (`MarketDataSecurity:RequireAuthentication=false`), but traffic is still protected with:

- REST requests per client: 120/minute
- WebSocket handshakes per client: 20/minute
- Concurrent WebSocket connections per client: 5
- WebSocket commands per connection: 120/minute
- WebSocket origin allow-list and configurable rejection of clients without an `Origin` header
- bounded message sizes and outbound queues

For a private deployment, set `MarketDataSecurity:RequireAuthentication=true`. HTTP clients must then send a bearer token containing the `vault.marketdata.api` scope. WebSocket clients can send the bearer token as `access_token` in the query string because browser WebSocket APIs cannot set an `Authorization` header.

## WebSocket protocol

Connect to `/ws/market-prices` (or `/ws/market-prices?access_token=...` when authentication is required). The server first sends a `connected` message containing the available symbols.

Subscribe:

```json
{"type":"subscribe","symbols":["BTC-USDT","ETH-USDT"]}
```

Replace the complete subscription set:

```json
{"type":"set_subscriptions","symbols":["BTC-USDT"]}
```

Unsubscribe:

```json
{"type":"unsubscribe","symbols":["ETH-USDT"]}
```

Application heartbeat:

```json
{"type":"ping"}
```

Server message types are `connected`, `subscribed`, `snapshot`, `price_updated`, `pong`, and `error`.

## HTTP endpoints

- `GET /api/market-prices/instruments`
- `GET /api/market-prices`
- `GET /api/market-prices?symbols=BTC-USDT,ETH-USDT`
- `GET /api/market-prices/{symbol}`
- `GET /health/live`
- `GET /health/ready`
