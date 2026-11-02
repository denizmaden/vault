import { profileName, minEndpointSamples } from '../config/profiles.js';
import { runId, suite } from '../config/environments.js';
import { selectedEndpoints, executionPlan, successfulEndpointExclusions } from '../config/workloads.js';

export function handleSummary(data) {
  const failures = [];
  for (const [metric, value] of Object.entries(data.metrics)) {
    for (const [threshold, outcome] of Object.entries(value.thresholds || {})) {
      if (!outcome.ok) failures.push(`${metric}: ${threshold}`);
    }
  }
  const metric = (name, endpoint) => data.metrics[`${name}{phase:measure,endpoint:${endpoint}}`]?.values || {};
  const coverage = selectedEndpoints.map(({ service, endpoint }) => {
    const samples = metric('endpoint_successes', endpoint).count || 0;
    return { service, endpoint, successes: samples, required: minEndpointSamples, covered: samples >= minEndpointSamples,
      errors: metric('endpoint_errors', endpoint).rate ?? null, latencyMs: metric('endpoint_latency_ms', endpoint),
      percentileEvidence: samples >= 100 ? 'at least 100 samples; evaluate against the required statistical confidence' : 'sparse samples; p95/p99 are indicative only' };
  });
  for (const row of coverage) if (!row.covered) failures.push(`Successful endpoint coverage missing: ${row.endpoint} (${row.successes}/${row.required})`);
  const metadata = {
    runId, profile: profileName, suite, finishedAt: new Date().toISOString(),
    passed: failures.length === 0, failedThresholds: failures, endpointCoverage: coverage,
    scope: 'Identity, Customer, Ledger, Wallet, Reporting and Transaction HTTP workflows, including password/refresh token requests. Successful manual transaction commands are excluded.',
    successfulEndpointExclusions,
    executionPlan,
    limitations: ['Local container benchmark, not a production capacity claim.', 'Transaction saga does not debit/credit ledger balances.', 'Service latency SLOs are initial engineering budgets, not business-approved SLAs.'],
  };
  return {
    '/results/summary.json': JSON.stringify(data, null, 2),
    '/results/coverage.json': JSON.stringify(coverage, null, 2),
    '/results/execution-plan.json': JSON.stringify(executionPlan, null, 2),
    '/results/result.json': JSON.stringify(metadata, null, 2),
    stdout: `\nVault ${profileName}/${suite}: ${metadata.passed ? 'PASS' : 'FAIL'}\n${failures.join('\n')}\nArtifacts: summary.json, result.json; dashboard.html when duration permits\n`,
  };
}
