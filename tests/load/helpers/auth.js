import http from 'k6/http';
import { check, fail } from 'k6';
import exec from 'k6/execution';
import { hosts, settings } from '../config/environments.js';
import { canonicalEndpoint } from '../config/endpoints.js';
import { json, contractFailures, recordEndpoint } from './client.js';
let tokenCache;
export const scope = 'openid profile offline_access IdentityServerApi vault.identity.api vault.customer.api vault.ledger.api vault.wallet.api vault.transaction.api vault.reporting.api';
export function grant(fields, name = 'POST /connect/token [password]') {
  const response = http.post(`${hosts.identity}/connect/token`, {
    client_id: settings.clientId, client_secret: settings.clientSecret, ...fields,
  }, { tags: { service: 'identity', name, endpoint: canonicalEndpoint(name) }, timeout: settings.requestTimeout, responseCallback: http.expectedStatuses(200) });
  const data = json(response);
  const ok = response.status === 200 && Boolean(data?.access_token);
  recordEndpoint(response, 'identity', name, ok);
  contractFailures.add(ok ? 0 : 1, { service: 'identity', name });
  check(ok, { [name]: value => value });
  if (!ok) fail(`${name}: token request failed (${response.status})`);
  return data;
}
export function passwordGrant(email, password = settings.password) {
  return grant({ grant_type: 'password', username: email, password, scope });
}
export function actor(data) { return data.actors[(exec.vu.idInTest - 1) % data.actors.length]; }
export function token(data) {
  const user = actor(data);
  if (!tokenCache || tokenCache.email !== user.email || tokenCache.expires < Date.now() + 30000) {
    const auth = passwordGrant(user.email);
    tokenCache = { email: user.email, value: auth.access_token, expires: Date.now() + auth.expires_in * 1000 };
  }
  return tokenCache.value;
}
