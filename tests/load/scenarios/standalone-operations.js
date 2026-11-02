import { actor, token } from '../helpers/auth.js';
import { request, json, uuid, invariant } from '../helpers/client.js';

export function projectionMaintenance(data) {
  const bearer = token(data), user = actor(data), id = uuid(), occurredOnUtc = new Date().toISOString();
  request('wallet', 'POST', '/api/wallet-projections/ledger-events/opened', { walletId: id, ownerId: user.customerId, currency: 'USD', version: 1, occurredOnUtc }, bearer, 'POST /api/wallet-projections/ledger-events/opened', [204]);
  // Prime the detail cache before updates to exercise invalidation.
  request('wallet', 'GET', `/api/wallets/${id}`, null, bearer, 'GET /api/wallets/:id [cache-prime]');
  request('wallet', 'POST', '/api/wallet-projections/ledger-events/deposited', { walletId: id, amount: 20, version: 2, occurredOnUtc }, bearer, 'POST /api/wallet-projections/ledger-events/deposited', [204]);
  request('wallet', 'POST', '/api/wallet-projections/ledger-events/withdrawn', { walletId: id, amount: 5, version: 3, occurredOnUtc }, bearer, 'POST /api/wallet-projections/ledger-events/withdrawn', [204]);
  const result = json(request('wallet', 'GET', `/api/wallets/${id}`, null, bearer, 'GET /api/wallets/:id [cache-invalidated]'));
  invariant(result?.balance === 15 && result.version === 3, 'Manual projection invalidates cached wallet');
}
