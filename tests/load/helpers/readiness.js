import http from 'k6/http';
import { check } from 'k6';
import { hosts } from '../config/environments.js';
export const options = { vus: 1, iterations: 1, thresholds: { checks: ['rate==1'] } };
export default function () {
  for (const [service, address] of Object.entries(hosts)) {
    const response = http.get(`${address}/health/ready`, { timeout: '2s', tags: { name: `ready-${service}` } });
    check(response, { [`${service} ready`]: value => value.status === 200 });
  }
}
