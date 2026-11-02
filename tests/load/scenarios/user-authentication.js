import { actor, passwordGrant, grant } from '../helpers/auth.js';
import { request, json, invariant } from '../helpers/client.js';

export function userAuthentication(data) {
  const user = actor(data);
  const auth = passwordGrant(user.email);
  const refresh = grant({ grant_type: 'refresh_token', refresh_token: auth.refresh_token }, 'POST /connect/token [refresh]');
  invariant(Boolean(refresh.refresh_token) && refresh.refresh_token !== auth.refresh_token, 'Refresh token rotates successfully');
  const me = json(request('identity', 'GET', '/api/auth/me', null, refresh.access_token, 'GET /api/auth/me [refreshed]'));
  invariant(me?.subject === user.userId, 'Authenticated subject matches test user');
  const detail = json(request('identity', 'GET', `/api/identity-users/${user.userId}`, null, refresh.access_token, 'GET /api/identity-users/:id'));
  invariant(detail?.email === user.email, 'Identity read returns correct email');
}
