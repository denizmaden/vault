import { sleep } from 'k6';
import { profile } from '../config/profiles.js';
import { runId, settings } from '../config/environments.js';
import { request, json, uuid, invariant, poll } from './client.js';
import { passwordGrant, token, actor } from './auth.js';

export function customerBody(identityUserId) {
  return { identityUserId, firstName: 'Load', lastName: 'Tester', displayName: 'Load Tester', preferredLanguage: 'en-US' };
}
export function register() {
  const email = `load-${runId}-${uuid().replace(/-/g, '')}@example.test`;
  const result = json(request('identity', 'POST', '/api/identity-users', { email, password: settings.password }, null, 'POST /api/identity-users', [201]));
  invariant(Boolean(result?.identityUserId), 'Registered identity has an ID');
  return { email, userId: result.identityUserId };
}
export function openAccount(id, ownerId, bearer, deposit = 100) {
  request('ledger', 'POST', '/api/ledger-accounts', { ledgerAccountId: id, ownerId, currency: 'USD' }, bearer, 'POST /api/ledger-accounts', [201]);
  request('ledger', 'POST', `/api/ledger-accounts/${id}/deposit`, { amount: deposit, reference: `load-${runId}` }, bearer, 'POST /api/ledger-accounts/:id/deposit', [204]);
}
export function transferBody(user, transactionId) {
  return { transactionId, initiatingCustomerId: user.customerId, sourceWalletId: user.sourceId, destinationWalletId: user.destinationId, currency: 'USD', amount: 1, reference: `load-${runId}` };
}
export function setupFixtures() {
  const actors = [];
  for (let i = 0; i < profile.actors; i++) {
    const user = register();
    const auth = passwordGrant(user.email);
    const bearer = auth.access_token;
    const customer = json(request('customer', 'POST', '/api/customer-profiles', customerBody(user.userId), bearer, 'POST /api/customer-profiles', [201]));
    invariant(Boolean(customer?.customerId), 'Fixture customer ID exists');
    Object.assign(user, { customerId: customer.customerId, sourceId: uuid(), destinationId: uuid(), transactionId: uuid() });
    request('customer', 'POST', `/api/customer-profiles/${user.customerId}/complete-onboarding`, null, bearer, 'POST /api/customer-profiles/:id/complete-onboarding', [204]);
    openAccount(user.sourceId, user.customerId, bearer);
    openAccount(user.destinationId, user.customerId, bearer, 50);
    request('transaction', 'POST', '/api/transactions/transfers', transferBody(user, user.transactionId), bearer, 'POST /api/transactions/transfers', [201]);
    actors.push(user);
  }
  for (const user of actors) {
    const bearer = passwordGrant(user.email).access_token;
    poll('wallet', `/api/wallets/${user.sourceId}`, bearer, 'GET /api/wallets/:id [seed]', value => value.version === 2 && value.balance === 100, 120000);
    poll('reporting', `/api/reports/wallets/${user.sourceId}`, bearer, 'GET /api/reports/wallets/:id [seed]', value => value.version === 2 && value.currentBalance === 100, 120000);
    poll('transaction', `/api/transactions/${user.transactionId}`, bearer, 'GET /api/transactions/:id [seed]', value => value.status === 'Completed', 120000);
  }
  return { actors, runId };
}
export function verifyFixtures(data) {
  for (const user of data.actors) {
    const bearer = passwordGrant(user.email).access_token;
    const ledger = json(request('ledger', 'GET', `/api/ledger-accounts/${user.sourceId}`, null, bearer, 'GET /api/ledger-accounts/:id [final]'));
    const wallet = json(request('wallet', 'GET', `/api/wallets/${user.sourceId}`, null, bearer, 'GET /api/wallets/:id [final]'));
    const report = json(request('reporting', 'GET', `/api/reports/wallets/${user.sourceId}`, null, bearer, 'GET /api/reports/wallets/:id [final]'));
    invariant(ledger?.balance === 100 && wallet?.balance === 100 && report?.currentBalance === 100, 'Final seeded balances agree across all projections');
  }
}
export function warmup(data) {
  const bearer = token(data);
  const user = actor(data);
  request('wallet', 'GET', `/api/wallets/${user.sourceId}`, null, bearer, 'GET /api/wallets/:id [warmup]');
  request('customer', 'GET', `/api/customer-profiles/${user.customerId}`, null, bearer, 'GET /api/customer-profiles/:id [warmup]');
  sleep(0.5);
}
