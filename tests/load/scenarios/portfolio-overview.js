import { actor, token } from '../helpers/auth.js';
import { request, json, invariant } from '../helpers/client.js';

// A user opens the portfolio and sees coherent data across the business services.
export function portfolioOverview(data) {
  const user = actor(data), bearer = token(data);
  const customer = json(request('customer', 'GET', `/api/customer-profiles/${user.customerId}`, null, bearer, 'GET /api/customer-profiles/:id'));
  invariant(customer?.identityUserId === user.userId && customer.onboardingStatus === 'Completed', 'Customer read returns seeded profile');

  const account = json(request('ledger', 'GET', `/api/ledger-accounts/${user.sourceId}`, null, bearer, 'GET /api/ledger-accounts/:id'));
  invariant(account?.balance === 100 && account.version === 2, 'Seed ledger balance and version remain correct');
  const events = json(request('ledger', 'GET', `/api/ledger-accounts/${user.sourceId}/events`, null, bearer, 'GET /api/ledger-accounts/:id/events'));
  invariant(Array.isArray(events) && events.length === 2, 'Ledger returns complete seeded event stream');

  const wallet = json(request('wallet', 'GET', `/api/wallets/${user.sourceId}`, null, bearer, 'GET /api/wallets/:id'));
  invariant(wallet?.balance === 100 && wallet.ownerId === user.customerId, 'Wallet cache returns correct seeded balance and owner');
  const wallets = json(request('wallet', 'GET', `/api/wallets/owners/${user.customerId}`, null, bearer, 'GET /api/wallets/owners/:id'));
  invariant(Array.isArray(wallets) && wallets.some(item => item.walletId === user.destinationId), 'Owner wallet listing includes destination');

  const report = json(request('reporting', 'GET', `/api/reports/wallets/${user.sourceId}`, null, bearer, 'GET /api/reports/wallets/:id'));
  invariant(report?.currentBalance === 100 && report.totalDeposited === 100, 'Report totals match ledger');
  const reports = json(request('reporting', 'GET', `/api/reports/owners/${user.customerId}/wallets`, null, bearer, 'GET /api/reports/owners/:id/wallets'));
  invariant(Array.isArray(reports) && reports.some(item => item.walletId === user.sourceId), 'Owner report listing includes source');

  const transaction = json(request('transaction', 'GET', `/api/transactions/${user.transactionId}`, null, bearer, 'GET /api/transactions/:id'));
  invariant(transaction?.status === 'Completed' && transaction.amount === 1, 'Transaction query returns completed saga');
  const transactions = json(request('transaction', 'GET', `/api/transactions/customers/${user.customerId}`, null, bearer, 'GET /api/transactions/customers/:id'));
  invariant(Array.isArray(transactions) && transactions.some(item => item.transactionId === user.transactionId), 'Customer transaction listing contains seeded saga');
}
