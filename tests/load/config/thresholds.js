import { minEndpointSamples } from './profiles.js';
import { selectedWorkloads, selectedEndpoints, schedule } from './workloads.js';

export function thresholds() {
  const values = {
    checks: ['rate==1'], contract_failures: ['count==0'], correctness_failures: ['count==0'], dropped_iterations: ['count==0'],
    'http_req_failed{phase:measure}': ['rate<0.01', { threshold: 'rate<0.5', abortOnFail: true, delayAbortEval: '30s' }],
    'http_req_duration{phase:measure}': ['p(95)<750', 'p(99)<2000'], 'http_reqs{phase:measure}': ['count>0'],
  };
  for (const [name, spec] of selectedWorkloads) {
    const { minimumCompleted } = schedule(spec);
    values[`workflows_completed{workflow:${name}}`] = [`count>=${minimumCompleted}`];
    values[`workflow_failures{workflow:${name}}`] = ['rate==0'];
    values[`workflow_duration_ms{workflow:${name}}`] = [`p(95)<${spec.budgetSeconds * 1000}`];
  }
  for (const { endpoint } of selectedEndpoints) {
    values[`endpoint_successes{phase:measure,endpoint:${endpoint}}`] = [`count>=${minEndpointSamples}`];
    values[`endpoint_errors{phase:measure,endpoint:${endpoint}}`] = ['rate==0'];
    // Only 2xx samples enter this trend. A quick 400/404 cannot satisfy a successful-path SLO.
    values[`endpoint_latency_ms{phase:measure,endpoint:${endpoint}}`] = ['p(95)<1500', 'p(99)<3000'];
  }
  for (const service of new Set(selectedEndpoints.map(x => x.service))) {
    values[`http_req_duration{phase:measure,service:${service}}`] = ['p(95)<1000', 'p(99)<3000'];
  }
  if (selectedWorkloads.some(([name]) => name === 'walletActivity')) values.projection_latency_ms = ['p(95)<10000', 'p(99)<20000'];
  if (selectedWorkloads.some(([name]) => name === 'transferProcessing')) values.saga_latency_ms = ['p(95)<10000', 'p(99)<20000'];
  return values;
}
