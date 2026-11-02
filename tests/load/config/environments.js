// Only the isolated Compose network is supported. No arbitrary production URL override.
export const env = typeof __ENV === 'undefined' ? (typeof process === 'undefined' ? {} : process.env) : __ENV;
export function positiveInt(value, fallback, name) {
  const number = value === undefined || value === '' ? fallback : Number(value);
  if (!Number.isSafeInteger(number) || number <= 0) throw new Error(`${name} must be a positive integer`);
  return number;
}
export const suite = env.SUITE || 'all';
export const suites = ['all', 'reads', 'writes', 'standalone'];
if (!suites.includes(suite)) throw new Error(`Unknown suite: ${suite}`);
export const runId = env.RUN_ID || '';
export const hosts = Object.fromEntries(
  ['identity', 'customer', 'ledger', 'wallet', 'transaction', 'reporting']
    .map(service => [service, `http://${service}-api:8080`]),
);
export const settings = {
  clientId: env.CLIENT_ID || 'vault-load', clientSecret: env.CLIENT_SECRET,
  password: env.TEST_PASSWORD, requestTimeout: '10s', pollTimeoutMs: 20000,
};
export function assertIsolatedEnvironment() {
  if (!/^[a-zA-Z0-9-]{6,64}$/.test(runId)) throw new Error('RUN_ID is required (6-64 alphanumeric/hyphen characters)');
  if (env.LOAD_ACKNOWLEDGE !== 'isolated-vault-load') throw new Error('Use the isolated load-test runner');
  if (!settings.clientSecret || !settings.password) throw new Error('CLIENT_SECRET and TEST_PASSWORD are required');
}
