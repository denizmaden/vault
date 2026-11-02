// Explicit business HTTP contract inventory. Framework, health, Swagger and UI routes are excluded.
// Stable templates keep GUIDs, tokens, email addresses and query strings out of metric labels.
const inventory = {
  identity: [
    'POST /connect/token',
    'GET /api/auth/me',
    'POST /api/identity-users',
    'GET /api/identity-users/:id',
    'POST /api/identity-roles',
    'POST /api/identity-users/:id/roles',
    'POST /api/identity-users/:id/claims',
    'POST /api/identity-users/:id/email-confirmation-token',
    'POST /api/identity-users/:id/confirm-email',
    'POST /api/identity-users/:id/change-password',
    'POST /api/identity-users/password-reset-token',
    'POST /api/identity-users/reset-password',
  ],
  customer: [
    'POST /api/customer-profiles',
    'GET /api/customer-profiles/:id',
    'PUT /api/customer-profiles/:id',
    'POST /api/customer-profiles/:id/complete-onboarding',
  ],
  ledger: [
    'POST /api/ledger-accounts',
    'POST /api/ledger-accounts/:id/deposit',
    'POST /api/ledger-accounts/:id/withdraw',
    'GET /api/ledger-accounts/:id',
    'GET /api/ledger-accounts/:id/events',
  ],
  wallet: [
    'GET /api/wallets/:id',
    'GET /api/wallets/owners/:id',
    'POST /api/wallet-projections/ledger-events/opened',
    'POST /api/wallet-projections/ledger-events/deposited',
    'POST /api/wallet-projections/ledger-events/withdrawn',
  ],
  reporting: [
    'GET /api/reports/wallets/:id',
    'GET /api/reports/owners/:id/wallets',
  ],
  transaction: [
    'POST /api/transactions/transfers',
    'GET /api/transactions/:id',
    'GET /api/transactions/customers/:id',
    'POST /api/transactions/:id/reserve-funds',
    'POST /api/transactions/:id/complete',
    'POST /api/transactions/:id/fail',
  ],
};
export const endpoints = Object.entries(inventory).flatMap(([service, routes]) => routes.map(endpoint => ({ service, endpoint })));
export const canonicalEndpoint = name => name.replace(/ \[[^\]]+\]$/, '');
