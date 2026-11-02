import http from 'k6/http';
import { check, fail, sleep } from 'k6';
import exec from 'k6/execution';
import { Counter, Trend, Rate } from 'k6/metrics';
import { hosts, runId, settings, assertIsolatedEnvironment } from '../config/environments.js';
import { canonicalEndpoint } from '../config/endpoints.js';
assertIsolatedEnvironment();

export const contractFailures = new Counter('contract_failures');
export const correctnessFailures = new Counter('correctness_failures');
export const projectionLatency = new Trend('projection_latency_ms', true);
export const sagaLatency = new Trend('saga_latency_ms', true);
const endpointRequests = new Counter('endpoint_requests');
const endpointSuccesses = new Counter('endpoint_successes');
const endpointErrors = new Rate('endpoint_errors');
const endpointLatency = new Trend('endpoint_latency_ms', true);
const started = new Counter('workflows_started');
const completed = new Counter('workflows_completed');
const failures = new Rate('workflow_failures');
const duration = new Trend('workflow_duration_ms', true);

export function runWorkflow(name, action, data) {
  const tags = { workflow: name }, since = Date.now();
  started.add(1, tags);
  let succeeded = false;
  try { action(data); succeeded = true; completed.add(1, tags); }
  finally { failures.add(!succeeded, tags); duration.add(Date.now() - since, tags); }
}
export function recordEndpoint(response, service, name, expected) {
  const tags = { service, endpoint: canonicalEndpoint(name) };
  endpointRequests.add(1, tags);
  endpointErrors.add(!expected, tags);
  const success = response.status >= 200 && response.status < 300;
  endpointSuccesses.add(success ? 1 : 0, tags);
  if (success) endpointLatency.add(response.timings.duration, tags);
}

export function uuid() {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
    const r = Math.floor(Math.random() * 16); return (c === 'x' ? r : (r & 3) | 8).toString(16);
  });
}
export function json(response) {
  try { return response.json(); } catch (_) { return null; }
}
export function invariant(condition, label) {
  correctnessFailures.add(condition ? 0 : 1);
  check(condition, { [label]: value => value });
  if (!condition) { exec.test.fail(`Correctness failure: ${label}`); fail(label); }
}
export function request(service, method, path, body, token, name, statuses = [200], extra = {}) {
  const response = http.request(method, `${hosts[service]}${path}`, body === null ? null : JSON.stringify(body), {
    headers: { 'Content-Type': 'application/json', 'X-Load-Test-Run': runId, ...(token ? { Authorization: `Bearer ${token}` } : {}) },
    timeout: settings.requestTimeout, redirects: 0,
    tags: { service, name, endpoint: canonicalEndpoint(name), ...extra }, responseCallback: http.expectedStatuses(...statuses),
  });
  const ok = statuses.includes(response.status);
  recordEndpoint(response, service, name, ok);
  contractFailures.add(ok ? 0 : 1, { service, name });
  check(response, { [name]: r => statuses.includes(r.status) }, { service });
  if (!ok) {
    // Do not log bodies: identity responses can contain passwords or tokens.
    console.error(`${service} ${name}: expected ${statuses.join('/')} received ${response.status}`);
    fail(`${name}: unexpected HTTP ${response.status}`);
  }
  return response;
}
export function poll(service, path, bearer, name, predicate, timeout = settings.pollTimeoutMs) {
  const started = Date.now();
  let delay = 0.1;
  while (Date.now() - started < timeout) {
    const response = request(service, 'GET', path, null, bearer, name, [200, 404]);
    const data = response.status === 200 ? json(response) : null;
    if (data && predicate(data)) return { data, elapsed: Date.now() - started };
    sleep(delay);
    delay = Math.min(delay * 1.5, 0.75);
  }
  invariant(false, `${name}: converged within ${timeout}ms`);
}
