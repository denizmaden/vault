import { passwordGrant } from '../helpers/auth.js';
import { request, json, invariant } from '../helpers/client.js';
import { register, customerBody } from '../helpers/test-data.js';

export function customerOnboarding(data) {
  const user = register();
  const bearer = passwordGrant(user.email).access_token;
  const created = json(request('customer', 'POST', '/api/customer-profiles', customerBody(user.userId), bearer, 'POST /api/customer-profiles', [201]));
  const id = created?.customerId;
  invariant(Boolean(id), 'Onboarding creates a customer ID');
  request('customer', 'PUT', `/api/customer-profiles/${id}`, { ...customerBody(user.userId), displayName: 'Updated Tester' }, bearer, 'PUT /api/customer-profiles/:id', [204]);
  request('customer', 'POST', `/api/customer-profiles/${id}/complete-onboarding`, null, bearer, 'POST /api/customer-profiles/:id/complete-onboarding', [204]);
  const profile = json(request('customer', 'GET', `/api/customer-profiles/${id}`, null, bearer, 'GET /api/customer-profiles/:id [updated]'));
  invariant(profile?.displayName === 'Updated Tester' && profile.onboardingStatus === 'Completed', 'Updated onboarding state is persisted');
}
