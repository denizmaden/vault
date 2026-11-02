import { suite } from './environments.js';
import { profile, profileName, minEndpointSamples } from './profiles.js';
import { endpoints } from './endpoints.js';

// Rates are business workflow starts/minute, not HTTP requests/second.
// A shared endpoint receives the sum of traffic from every workflow that uses it.
export const workloads = {
  userAuthentication: { rate: 12, budgetSeconds: 4, kind: 'write', endpoints: endpoints.filter(x => x.endpoint === 'POST /connect/token' || x.endpoint === 'GET /api/auth/me' || x.endpoint === 'GET /api/identity-users/:id').map(x => x.endpoint) },
  customerOnboarding: { rate: 6, budgetSeconds: 6, kind: 'write', endpoints: ['POST /api/identity-users', 'POST /api/customer-profiles', 'PUT /api/customer-profiles/:id', 'POST /api/customer-profiles/:id/complete-onboarding', 'GET /api/customer-profiles/:id'] },
  accountManagement: { rate: 2, budgetSeconds: 8, kind: 'write', endpoints: endpoints.filter(x => x.service === 'identity' && x.endpoint !== 'GET /api/auth/me').map(x => x.endpoint) },
  walletActivity: { rate: 12, budgetSeconds: 25, kind: 'write', endpoints: ['POST /api/ledger-accounts', 'POST /api/ledger-accounts/:id/deposit', 'POST /api/ledger-accounts/:id/withdraw', 'GET /api/ledger-accounts/:id', 'GET /api/ledger-accounts/:id/events', 'GET /api/wallets/:id', 'GET /api/reports/wallets/:id'] },
  transferProcessing: { rate: 12, budgetSeconds: 25, kind: 'write', endpoints: ['POST /api/transactions/transfers', 'GET /api/transactions/:id'] },
  portfolioOverview: { rate: 120, budgetSeconds: 5, kind: 'read', endpoints: endpoints.filter(x => ['customer','ledger','wallet','reporting','transaction'].includes(x.service) && x.endpoint.startsWith('GET ')).map(x => x.endpoint) },
  projectionMaintenance: { rate: 2, budgetSeconds: 4, kind: 'standalone', endpoints: ['POST /api/wallet-projections/ledger-events/opened', 'POST /api/wallet-projections/ledger-events/deposited', 'POST /api/wallet-projections/ledger-events/withdrawn', 'GET /api/wallets/:id'] },
};
// Inventory is complete; successful HTTP load intentionally follows normal business flows.
// These commands retain terminal-state rejection checks in transferProcessing, without SQL fixtures.
export const successfulEndpointExclusions = [
  'POST /api/transactions/:id/reserve-funds',
  'POST /api/transactions/:id/complete',
  'POST /api/transactions/:id/fail',
].map(endpoint => ({ endpoint, reason: 'Direct successful state-command load is excluded; normal saga and terminal-state rejection checks are retained.' }));
export const selectedWorkloads = Object.entries(workloads).filter(([, spec]) => suite === 'all'
  || (suite === 'reads' && spec.kind === 'read')
  || (suite === 'writes' && ['write','standalone'].includes(spec.kind))
  || (suite === 'standalone' && spec.kind === 'standalone'));
export function schedule(spec) {
  const target = Math.max(profileName === 'smoke' ? 6 : 1, Math.round(spec.rate * profile.factor));
  const startRate = Math.max(1, Math.round(target / 5));
  const plannedStarts = (profile.ramp * (startRate + target) / 2 + profile.hold * target + profile.down * target / 2) / 60;
  return { target, startRate, plannedStarts, minimumCompleted: Math.max(1, Math.floor(plannedStarts * 0.95)) };
}
export const selectedEndpoints = endpoints.filter(x => selectedWorkloads.some(([, spec]) => spec.endpoints.includes(x.endpoint)));
export const executionPlan = {
  profile: profileName, suite, minEndpointSamples,
  measurementSeconds: profile.ramp + profile.hold + profile.down,
  workflows: selectedWorkloads.map(([name, spec]) => ({ name, ...schedule(spec), budgetSeconds: spec.budgetSeconds, endpoints: spec.endpoints })),
  successfulEndpointExclusions,
};
export function scenarioOptions() {
  const scenarios = {};
  for (const [name, spec] of selectedWorkloads) {
    const { target, startRate } = schedule(spec);
    scenarios[name] = {
      executor: 'ramping-arrival-rate', exec: name, timeUnit: '1m', startRate,
      preAllocatedVUs: Math.max(2, Math.ceil(target / 60 * spec.budgetSeconds) + 2),
      startTime: `${profile.warmup}s`, gracefulStop: '45s',
      stages: [{ duration: `${profile.ramp}s`, target }, { duration: `${profile.hold}s`, target }, { duration: `${profile.down}s`, target: 0 }],
      tags: { phase: 'measure', workflow: name },
    };
  }
  scenarios.warmup = { executor: 'constant-vus', vus: 1, duration: `${profile.warmup}s`, gracefulStop: '5s', exec: 'warmup', tags: { phase: 'warmup' } };
  return scenarios;
}
