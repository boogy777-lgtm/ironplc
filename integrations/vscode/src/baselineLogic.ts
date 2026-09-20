/**
 * The baseline check of the engineering connection (ADR-0063, "Offline
 * edits, online deploy"): the owner scenario connects, verifies the project
 * equals the controller's code, disconnects, edits offline, and reconnects
 * to deploy. Before any bytes are sent, the client compares the device's
 * application snapshot against the baseline captured at the last
 * verified-equal moment (our own assemble); a mismatch means another
 * engineer edited the device or it rebooted into different state, and the
 * deploy refuses with E0015 StaleBaseline — fail-closed, the same pattern as
 * the V4007–V4010 staging refusals. No stored baseline (initial deploy)
 * means no check.
 *
 * The module is vscode-free: capture and compare are pure decisions over
 * the `HotEditStatus` vocabulary, unit-testable for the match, mismatch,
 * and reboot-reset cases. Persistence in `workspaceState` is glue-side.
 */

import { createHash } from 'crypto';

import { HotEditStatus } from './hotEditSession';

/**
 * The last verified-equal state: the generation counters from the device's
 * application snapshot plus the SHA-256 of the container bytes the client
 * itself compiled and assembled — the artifact identity the client hashes
 * from its own compile.
 */
export interface BaselineSnapshot {
  active: number;
  application: number;
  containerHash: string;
}

export type BaselineState = 'none' | 'match' | 'mismatch';

/** Hashes the compiled container bytes (the artifact identity of one build). */
export function hashContainer(bytes: Uint8Array): string {
  return createHash('sha256').update(bytes).digest('hex');
}

/**
 * Captures the baseline at the verified-equal moment: right after our own
 * `assembleEdits` acknowledged, from the post-assemble status and the exact
 * container bytes that were uploaded.
 */
export function captureBaseline(status: HotEditStatus, containerBytes: Uint8Array): BaselineSnapshot {
  return {
    active: status.active,
    application: status.application,
    containerHash: hashContainer(containerBytes),
  };
}

/**
 * Compares the device's current application snapshot against the stored
 * baseline. Session generations restart on a device reboot while the
 * container hash is recomputed and verified at every load, so counter
 * equality is read conservatively: anything other than an exact counter
 * match (or a missing baseline) is a mismatch and fails closed into E0015.
 */
export function checkBaseline(baseline: BaselineSnapshot | undefined, application: HotEditStatus): BaselineState {
  if (baseline === undefined) {
    return 'none';
  }
  return baseline.active === application.active && baseline.application === application.application
    ? 'match'
    : 'mismatch';
}
