import { sleep } from 'k6';
import exec from 'k6/execution';
import { hosts } from './config/environments.js';
import { scenarioOptions } from './config/workloads.js';
import { thresholds } from './config/thresholds.js';
import { request, invariant, runWorkflow } from './helpers/client.js';
import { setupFixtures, verifyFixtures } from './helpers/test-data.js';
import { userAuthentication as authenticate } from './scenarios/user-authentication.js';
import { customerOnboarding as onboard } from './scenarios/customer-onboarding.js';
import { accountManagement as manage } from './scenarios/account-management.js';
import { walletActivity as transact } from './scenarios/wallet-activity.js';
import { transferProcessing as transfer } from './scenarios/transfer-processing.js';
import { portfolioOverview as overview } from './scenarios/portfolio-overview.js';
import { projectionMaintenance as project } from './scenarios/standalone-operations.js';
export function userAuthentication(data) { runWorkflow('userAuthentication', authenticate, data); }
export function customerOnboarding(data) { runWorkflow('customerOnboarding', onboard, data); }
export function accountManagement(data) { runWorkflow('accountManagement', manage, data); }
export function walletActivity(data) { runWorkflow('walletActivity', transact, data); }
export function transferProcessing(data) { runWorkflow('transferProcessing', transfer, data); }
export function portfolioOverview(data) { runWorkflow('portfolioOverview', overview, data); }
export function projectionMaintenance(data) { runWorkflow('projectionMaintenance', project, data); }
export { warmup } from './helpers/test-data.js';
export { handleSummary } from './helpers/summary.js';

export const options = {
  scenarios: scenarioOptions(), thresholds: thresholds(),
  setupTimeout: '5m', teardownTimeout: '2m',
  summaryTrendStats: ['avg','min','med','max','p(95)','p(99)'],
  // Exclude URL (contains unique IDs) and error text from metrics cardinality.
  systemTags: ['status','method','name','scenario','expected_response','check','group','proto','subproto','tls_version'],
};
export function setup() {
  exec.vu.metrics.tags.phase = 'setup';
  for (const service of Object.keys(hosts)) {
    let ready = false;
    for (let attempt = 0; attempt < 60; attempt++) {
      try {
        const response = request(service, 'GET', '/health/ready', null, null, 'GET /health/ready', [200,503]);
        ready = response.status === 200;
      } catch (_) { /* Startup may briefly refuse connections. Never hide final failure. */ }
      if (ready) break;
      sleep(1);
    }
    invariant(ready, `${service} readiness`);
  }
  const data = setupFixtures();
  // Prove authentication remains enabled in the capacity environment.
  request('customer', 'GET', `/api/customer-profiles/${data.actors[0].customerId}`, null, null, 'GET customer [unauthenticated]', [401]);
  console.log(`Prepared ${data.actors.length} isolated actors; all ${Object.keys(hosts).length} services are ready.`);
  return data;
}
export function teardown(data) {
  exec.vu.metrics.tags.phase = 'teardown';
  if (data?.actors) verifyFixtures(data);
}
