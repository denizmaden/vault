import { runId } from '../config/environments.js';
import { actor, token } from '../helpers/auth.js';
import { request, json, uuid, invariant, poll, projectionLatency } from '../helpers/client.js';
import { openAccount } from '../helpers/test-data.js';

export function walletActivity(data) {
  const user = actor(data), bearer = token(data), id = uuid();
  const started = Date.now();
  openAccount(id, user.customerId, bearer);
  request('ledger', 'POST', `/api/ledger-accounts/${id}/withdraw`, { amount: 25, reference: `load-${runId}` }, bearer, 'POST /api/ledger-accounts/:id/withdraw', [204]);
  const authoritative = json(request('ledger', 'GET', `/api/ledger-accounts/${id}`, null, bearer, 'GET /api/ledger-accounts/:id [written]'));
  invariant(authoritative?.balance === 75 && authoritative.version === 3, 'Ledger deposit minus withdrawal equals 75 at version 3');
  const events = json(request('ledger', 'GET', `/api/ledger-accounts/${id}/events`, null, bearer, 'GET /api/ledger-accounts/:id/events [written]'));
  invariant(Array.isArray(events) && events.length === 3, 'Written ledger contains open, deposit and withdrawal events');
  // Independent account per iteration prevents accidental optimistic concurrency conflicts.
  const wallet = poll('wallet', `/api/wallets/${id}`, bearer, 'GET /api/wallets/:id [convergence]', value => value.version >= 3);
  const report = poll('reporting', `/api/reports/wallets/${id}`, bearer, 'GET /api/reports/wallets/:id [convergence]', value => value.version >= 3);
  projectionLatency.add(Date.now() - started);
  invariant(wallet.data.balance === 75 && wallet.data.version === 3, 'Kafka wallet projection matches written ledger');
  invariant(report.data.currentBalance === 75 && report.data.totalDeposited === 100 && report.data.totalWithdrawn === 25 && report.data.version === 3 && report.data.entries.length === 3, 'Kafka report balances, totals, entry count and version match ledger');
}
