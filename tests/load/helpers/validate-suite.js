// Offline structural validation, using Node built-ins only. No services or secrets required.
import assert from 'node:assert/strict';
import { readFileSync, readdirSync, existsSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { resolve, dirname, relative } from 'node:path';
import { spawnSync } from 'node:child_process';
import { endpoints, canonicalEndpoint } from '../config/endpoints.js';
import { workloads, scenarioOptions, executionPlan, selectedEndpoints, successfulEndpointExclusions } from '../config/workloads.js';
import { thresholds } from '../config/thresholds.js';
import { profiles, profileName } from '../config/profiles.js';
import { suite, suites, hosts } from '../config/environments.js';

if (process.argv.includes('--all')) {
  for (const profile of Object.keys(profiles)) for (const selectedSuite of suites) {
    const child = spawnSync(process.execPath, [fileURLToPath(import.meta.url)], { env: { ...process.env, PROFILE: profile, SUITE: selectedSuite }, encoding: 'utf8' });
    process.stdout.write(child.stdout); process.stderr.write(child.stderr);
    if (child.status !== 0) process.exit(child.status || 1);
  }
  process.exit(0);
}

const root = fileURLToPath(new URL('../', import.meta.url));
const repository = resolve(root, '../..');
const read = path => readFileSync(path, 'utf8');
function files(path) {
  return readdirSync(path, { withFileTypes: true }).flatMap(item => ['node_modules','results','bin','obj'].includes(item.name) ? []
    : item.isDirectory() ? files(resolve(path, item.name)) : [resolve(path, item.name)]);
}
const normalize = path => path.replace(/\{[^}]+\}/g, ':id').replace(/:[A-Za-z][\w]*/g, ':id').replace(/\/+$/, '');
const sourceRoutes = new Set();
for (const file of Object.keys(hosts).flatMap(service => files(resolve(repository, 'src/services', service))).filter(x => /[\\/]Controllers[\\/].*\.cs$/.test(x))) {
  const source = read(file), base = source.match(/Route\("([^"]+)"\)/)?.[1];
  if (!base) continue;
  for (const match of source.matchAll(/\[Http(Get|Post|Put|Delete|Patch)(?:\("([^"]*)"\))?\]/g)) {
    sourceRoutes.add(`${match[1].toUpperCase()} ${normalize('/' + base + (match[2] ? '/' + match[2] : ''))}`);
  }
}
const catalog = new Set(endpoints.map(x => normalize(x.endpoint)));
assert.equal(catalog.size, endpoints.length, 'Duplicate canonical endpoints');
for (const route of sourceRoutes) assert(catalog.has(route), `Business API endpoint missing from load inventory: ${route}`);
for (const route of catalog) assert(sourceRoutes.has(route) || route === 'POST /connect/token', `Stale inventory route: ${route}`);
const excluded = new Set(successfulEndpointExclusions.map(x => normalize(x.endpoint)));
assert.equal(excluded.size, successfulEndpointExclusions.length, 'Duplicate endpoint exclusion');
for (const { endpoint, reason } of successfulEndpointExclusions) {
  assert(catalog.has(normalize(endpoint)) && reason, `Invalid endpoint exclusion: ${endpoint}`);
  assert(!Object.values(workloads).some(x => x.endpoints.includes(endpoint)), `Excluded endpoint still declared as successful load: ${endpoint}`);
}
for (const endpoint of catalog) assert(excluded.has(endpoint) || Object.values(workloads).some(x => x.endpoints.some(y => normalize(y) === endpoint)), `No workflow or explicit exclusion covers ${endpoint}`);
for (const [name, spec] of Object.entries(workloads)) {
  assert(spec.rate > 0 && spec.budgetSeconds > 0, `Invalid workload ${name}`);
  for (const route of spec.endpoints) assert(catalog.has(normalize(route)), `Unknown workload endpoint ${route}`);
}
const entrypoint = read(resolve(root, 'all.js'));
for (const [name, spec] of Object.entries(scenarioOptions())) assert(new RegExp(`(?:function ${spec.exec}\\(|\\{ ${spec.exec} \\})`).test(entrypoint), `Missing k6 export ${name}`);
const allSources = files(root).filter(x => x.endsWith('.js'));
const importGraph = new Map();
for (const file of allSources) {
  const imports = [...read(file).matchAll(/(?:from\s*|import\s*)['"](\.[^'"]+)['"]/g)].map(x => resolve(dirname(file), x[1]));
  for (const target of imports) assert(existsSync(target), `Broken relative import in ${relative(root, file)}: ${relative(root, target)}`);
  importGraph.set(file, imports);
}
// Node tools must never enter the k6 import graph.
const visited = new Set();
function visit(file) {
  if (visited.has(file)) return;
  visited.add(file);
  assert(!/from ['"]k6\/(?:websockets|ws)['"]/.test(read(file)), `WebSocket load module remains: ${relative(root, file)}`);
  assert(!/from ['"]node:/.test(read(file)), `Node-only tool imported into k6: ${relative(root, file)}`);
  for (const target of importGraph.get(file) || []) visit(target);
}
for (const name of ['all.js','helpers/readiness.js']) visit(resolve(root, name));
const gates = thresholds();
for (const { endpoint } of selectedEndpoints) assert(gates[`endpoint_successes{phase:measure,endpoint:${endpoint}}`], `Missing sample gate ${endpoint}`);
assert.equal(canonicalEndpoint('POST /api/transactions/:id/fail [terminal]'), 'POST /api/transactions/:id/fail');
assert.equal(Object.keys(profiles).length, 5);
console.log(JSON.stringify({ valid: true, profile: profileName, suite, businessHttpEndpoints: catalog.size, sourceHttpRoutes: sourceRoutes.size,
  selectedSuccessfulEndpoints: selectedEndpoints.length, excludedSuccessfulEndpoints: successfulEndpointExclusions.length, workflows: executionPlan.workflows.length, thresholdGroups: Object.keys(gates).length, k6Modules: visited.size }));
