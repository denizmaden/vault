import { env, positiveInt } from './environments.js';

// Starting engineering workloads, not production capacity or business-approved SLAs.
export const profiles = {
  smoke: { factor: 0.2, warmup: 5, ramp: 5, hold: 30, down: 5, actors: 4, minSamples: 3 },
  load: { factor: 1, warmup: 20, ramp: 30, hold: 180, down: 30, actors: 12, minSamples: 5 },
  stress: { factor: 4, warmup: 30, ramp: 60, hold: 180, down: 30, actors: 24, minSamples: 20 },
  spike: { factor: 6, warmup: 20, ramp: 5, hold: 60, down: 10, actors: 24, minSamples: 5 },
  soak: { factor: 1, warmup: 30, ramp: 60, hold: 1800, down: 30, actors: 12, minSamples: 30 },
};
export const profileName = env.PROFILE || 'smoke';
if (!profiles[profileName]) throw new Error(`Unknown profile: ${profileName}`);
export const profile = profiles[profileName];
export const minEndpointSamples = positiveInt(env.MIN_ENDPOINT_SAMPLES, profile.minSamples, 'MIN_ENDPOINT_SAMPLES');
