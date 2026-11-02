import { runId, settings } from '../config/environments.js';
import { token, passwordGrant } from '../helpers/auth.js';
import { request, json, uuid, invariant } from '../helpers/client.js';
import { register } from '../helpers/test-data.js';

export function accountManagement(data) {
  const bearer = token(data), user = register();
  const roleName = `load-${uuid()}`;
  request('identity', 'POST', '/api/identity-roles', { roleName }, bearer, 'POST /api/identity-roles', [201]);
  request('identity', 'POST', `/api/identity-users/${user.userId}/roles`, { roleName }, bearer, 'POST /api/identity-users/:id/roles', [204]);
  request('identity', 'POST', `/api/identity-users/${user.userId}/claims`, { claimType: 'load-test', claimValue: runId }, bearer, 'POST /api/identity-users/:id/claims', [204]);
  const confirmation = json(request('identity', 'POST', `/api/identity-users/${user.userId}/email-confirmation-token`, null, bearer, 'POST /api/identity-users/:id/email-confirmation-token'));
  request('identity', 'POST', `/api/identity-users/${user.userId}/confirm-email`, { token: confirmation?.token }, bearer, 'POST /api/identity-users/:id/confirm-email', [204]);
  const nextPassword = `${settings.password}X`;
  request('identity', 'POST', `/api/identity-users/${user.userId}/change-password`, { currentPassword: settings.password, newPassword: nextPassword }, bearer, 'POST /api/identity-users/:id/change-password', [204]);
  const reset = json(request('identity', 'POST', '/api/identity-users/password-reset-token', { email: user.email }, null, 'POST /api/identity-users/password-reset-token'));
  request('identity', 'POST', '/api/identity-users/reset-password', { userId: user.userId, token: reset?.token, newPassword: settings.password }, null, 'POST /api/identity-users/reset-password', [204]);
  const auth = passwordGrant(user.email);
  const detail = json(request('identity', 'GET', `/api/identity-users/${user.userId}`, null, auth.access_token, 'GET /api/identity-users/:id [lifecycle]'));
  invariant(detail?.emailConfirmed && detail.roles.includes(roleName) && detail.claims.some(claim => claim.type === 'load-test' && claim.value === runId), 'Identity lifecycle persists email confirmation, role and claim');
}
