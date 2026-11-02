import { actor, token } from '../helpers/auth.js';
import { request, uuid, invariant, poll, sagaLatency } from '../helpers/client.js';
import { transferBody } from '../helpers/test-data.js';

export function transferProcessing(data) {
  const user = actor(data), bearer = token(data), id = uuid();
  const body = transferBody(user, id), started = Date.now();
  request('transaction', 'POST', '/api/transactions/transfers', body, bearer, 'POST /api/transactions/transfers', [201]);
  // Duplicate ID must be rejected without creating a second saga.
  request('transaction', 'POST', '/api/transactions/transfers', body, bearer, 'POST /api/transactions/transfers [duplicate]', [409]);
  const result = poll('transaction', `/api/transactions/${id}`, bearer, 'GET /api/transactions/:id [saga]', value => ['Completed','Failed'].includes(value.status));
  sagaLatency.add(Date.now() - started);
  invariant(result.data.status === 'Completed' && Boolean(result.data.fundsReservedOnUtc) && Boolean(result.data.completedOnUtc) && !result.data.failedOnUtc, 'RabbitMQ saga reaches Completed with ordered timestamps');
  invariant(Date.parse(result.data.completedOnUtc) >= Date.parse(result.data.fundsReservedOnUtc), 'Saga completion follows reservation');
  // This implementation changes transaction state only; it does NOT transfer ledger funds.
  request('transaction', 'POST', `/api/transactions/${id}/reserve-funds`, null, bearer, 'POST /api/transactions/:id/reserve-funds [terminal]', [400]);
  request('transaction', 'POST', `/api/transactions/${id}/complete`, null, bearer, 'POST /api/transactions/:id/complete [terminal]', [400]);
  request('transaction', 'POST', `/api/transactions/${id}/fail`, { failureReason: 'load-terminal-check' }, bearer, 'POST /api/transactions/:id/fail [terminal]', [400]);
}
