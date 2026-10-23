import { useEffect, useState } from 'react';

const defaultSymbols = ['USD-TRY', 'EUR-TRY', 'BTC-USDT', 'ETH-USDT'];

const formatters = {
  TRY: new Intl.NumberFormat('tr-TR', {
    style: 'currency',
    currency: 'TRY',
    maximumFractionDigits: 4
  }),
  USD: new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    maximumFractionDigits: 2
  }),
  USDT: new Intl.NumberFormat('en-US', {
    maximumFractionDigits: 2
  })
};

function buildWebSocketUrl() {
  const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
  return `${protocol}//${window.location.host}/ws/market-prices`;
}

function formatPrice(price) {
  const formatter = formatters[price.quoteAsset] ?? new Intl.NumberFormat('en-US', {
    maximumFractionDigits: 8
  });

  const formatted = formatter.format(price.price);
  return price.quoteAsset === 'USDT' ? `${formatted} USDT` : formatted;
}

function formatTime(value) {
  if (!value) {
    return 'Waiting';
  }

  return new Intl.DateTimeFormat('en-US', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  }).format(new Date(value));
}

function mergePrice(current, incoming) {
  return {
    ...incoming,
    direction: current && incoming.price !== current.price
      ? incoming.price > current.price ? 'up' : 'down'
      : current?.direction ?? 'flat'
  };
}

export function App() {
  const [connectionState, setConnectionState] = useState('connecting');
  const [prices, setPrices] = useState({});
  const [events, setEvents] = useState([]);
  const [lastMessageAt, setLastMessageAt] = useState(null);

  useEffect(() => {
    let reconnectTimer;
    let heartbeatTimer;
    let socket;
    let closedByEffect = false;

    const connect = () => {
      setConnectionState('connecting');
      socket = new WebSocket(buildWebSocketUrl());

      socket.addEventListener('open', () => {
        socket.send(JSON.stringify({
          type: 'subscribe',
          symbols: defaultSymbols
        }));
        heartbeatTimer = window.setInterval(() => {
          if (socket.readyState === WebSocket.OPEN) {
            socket.send(JSON.stringify({ type: 'ping' }));
          }
        }, 15000);
      });

      socket.addEventListener('message', (event) => {
        const message = JSON.parse(event.data);
        setLastMessageAt(message.sentOnUtc);

        if (message.type === 'subscribed') {
          setConnectionState('live');
          return;
        }

        if (message.type === 'error') {
          setConnectionState('degraded');
          return;
        }

        if (message.type === 'snapshot') {
          setPrices((current) => {
            const next = { ...current };
            for (const price of message.payload.prices) {
              next[price.symbol] = mergePrice(current[price.symbol], price);
            }
            return next;
          });
          return;
        }

        if (message.type === 'price_updated') {
          setPrices((current) => ({
            ...current,
            [message.payload.symbol]: mergePrice(current[message.payload.symbol], message.payload)
          }));

          setEvents((current) => [
            {
              symbol: message.payload.symbol,
              price: message.payload.price,
              quoteAsset: message.payload.quoteAsset,
              receivedOnUtc: message.payload.receivedOnUtc,
              source: message.payload.source
            },
            ...current
          ].slice(0, 8));
        }
      });

      socket.addEventListener('close', () => {
        window.clearInterval(heartbeatTimer);
        if (!closedByEffect) {
          setConnectionState('reconnecting');
          reconnectTimer = window.setTimeout(connect, 2000);
        }
      });

      socket.addEventListener('error', () => {
        setConnectionState('degraded');
      });
    };

    connect();

    return () => {
      closedByEffect = true;
      window.clearTimeout(reconnectTimer);
      window.clearInterval(heartbeatTimer);
      socket?.close();
    };
  }, []);

  const orderedPrices = defaultSymbols.map((symbol) => prices[symbol] ?? {
    symbol,
    displayName: symbol,
    baseAsset: symbol.split('-')[0],
    quoteAsset: symbol.split('-')[1],
    kind: symbol.includes('TRY') ? 'Forex' : 'Crypto',
    price: 0,
    source: 'Pending'
  });

  return (
    <main className="shell">
      <section className="workspace">
        <header className="topbar">
          <div>
            <p className="eyebrow">Vault MarketData</p>
            <h1>Live market stream</h1>
          </div>
          <div className={`status status-${connectionState}`}>
            <span />
            {connectionState}
          </div>
        </header>

        <section className="ticker-grid" aria-label="Live prices">
          {orderedPrices.map((price) => (
            <article className={`ticker-card direction-${price.direction ?? 'flat'}`} key={price.symbol}>
              <div className="ticker-card-header">
                <div>
                  <p>{price.kind}</p>
                  <h2>{price.displayName}</h2>
                </div>
                <strong>{price.symbol}</strong>
              </div>
              <div className="price-row">
                <span>{price.price > 0 ? formatPrice(price) : 'Waiting'}</span>
              </div>
              <div className="meta-row">
                <span>{price.source}</span>
                <span>{formatTime(price.receivedOnUtc)}</span>
              </div>
              {price.changePercent24h !== null && price.changePercent24h !== undefined && (
                <div className="change-row">
                  24h {Number(price.changePercent24h).toFixed(2)}%
                </div>
              )}
            </article>
          ))}
        </section>

        <section className="lower-grid">
          <div className="stream-panel">
            <div className="panel-title">
              <h2>Recent ticks</h2>
              <span>{formatTime(lastMessageAt)}</span>
            </div>
            <div className="event-list">
              {events.length === 0 ? (
                <p className="empty-state">Waiting for market ticks.</p>
              ) : events.map((event, index) => (
                <div className="event-row" key={`${event.symbol}-${event.receivedOnUtc}-${index}`}>
                  <span>{event.symbol}</span>
                  <strong>{formatPrice(event)}</strong>
                  <em>{event.source}</em>
                </div>
              ))}
            </div>
          </div>

          <aside className="info-panel">
            <h2>Stream contract</h2>
            <dl>
              <div>
                <dt>Client socket</dt>
                <dd>/ws/market-prices</dd>
              </div>
              <div>
                <dt>Durable stream</dt>
                <dd>Kafka price events</dd>
              </div>
              <div>
                <dt>Snapshot & scale-out</dt>
                <dd>Redis</dd>
              </div>
              <div>
                <dt>Forex source</dt>
                <dd>Finnhub WebSocket</dd>
              </div>
              <div>
                <dt>Crypto source</dt>
                <dd>Binance Spot WebSocket</dd>
              </div>
            </dl>
          </aside>
        </section>
      </section>
    </main>
  );
}
