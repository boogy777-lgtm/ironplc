/**
 * The device-panel rendering of the engineering connection: pure rows from
 * the connection state machine's snapshot, following the online-editing-ux
 * vocabulary — the panel answers "is the controller running my edits?" with
 * the same words the ADR-0064 lifecycle uses. The module is vscode-free:
 * the tree provider in `devicePanel.ts` renders these rows and nothing else.
 */

import { ConnectionState } from './connectionState';
import { formatStatusText, HotEditStatus, IdentityInfo } from './hotEditSession';
import { BaselineState } from './baselineLogic';
import { BuildPhase } from './buildLogic';

/** One row of the device panel. */
export interface PanelRow {
  id: string;
  label: string;
  /** The trailing annotation (the value half of the row). */
  description?: string;
  /** Hover detail, when the value alone is not enough. */
  tooltip?: string;
}

/** Everything the panel renders, in the vocabulary of the specs. */
export interface DevicePanelModel {
  state: ConnectionState;
  profileName: string | null;
  identity: IdentityInfo | null;
  lastStatus: HotEditStatus | null;
  baseline: BaselineState;
  buildPhase: BuildPhase | null;
}

/** The state row label, per the online-editing-ux status vocabulary. */
export function connectionStateLabel(state: ConnectionState): string {
  switch (state) {
    case 'disconnected':
      return 'Not connected';
    case 'connecting':
      return 'Connecting…';
    case 'reconnecting':
      return 'Reconnecting…';
    case 'connected':
      return 'Connected';
  }
}

/**
 * Builds the panel rows for the current model. Disconnected renders the
 * single honest row; Reconnecting keeps the last identity data, marked
 * stale, exactly like the spec's Reconnecting state description.
 */
export function devicePanelRows(model: DevicePanelModel): PanelRow[] {
  const rows: PanelRow[] = [
    { id: 'state', label: 'Connection', description: connectionStateLabel(model.state) },
  ];

  if (model.state === 'disconnected') {
    return rows;
  }

  if (model.profileName !== null) {
    rows.push({ id: 'profile', label: 'Profile', description: model.profileName });
  }

  const identity = model.identity;
  if (identity !== null) {
    rows.push({ id: 'device.name', label: 'Device', description: identity.device.name });
    rows.push({ id: 'device.model', label: 'Model', description: identity.device.model });
    rows.push({ id: 'device.modification', label: 'Modification', description: identity.device.modification });
    rows.push({
      id: 'device.firmware',
      label: 'Firmware version',
      description: identity.device.firmwareVersion,
    });
    if (identity.redundancy !== undefined) {
      const redundancy = identity.redundancy;
      rows.push({ id: 'redundancy.pair', label: 'Pair', description: redundancy.pairId });
      rows.push({ id: 'redundancy.role', label: 'Role', description: redundancy.role });
      rows.push({ id: 'redundancy.epoch', label: 'Epoch', description: String(redundancy.epoch) });
      rows.push({ id: 'redundancy.sync', label: 'Sync', description: redundancy.sync });
      rows.push({ id: 'redundancy.control', label: 'Control', description: redundancy.control });
    }
  }

  const status = model.lastStatus ?? identity?.application ?? null;
  if (status !== null) {
    rows.push({
      id: 'application',
      label: 'Application',
      description: formatStatusText(status),
      tooltip: status.migration
        ? 'The staged candidate changes the schema: revert (untest) is unavailable, commit or cancel are the only exits.'
        : undefined,
    });
    if (status.pendingEdit !== undefined) {
      const pending = status.pendingEdit;
      rows.push({
        id: 'pendingEdit',
        label: 'Pending edit',
        description: pending.name ?? '(unnamed)',
        tooltip: `Origin: ${pending.origin ?? 'unknown'} · accepted ${new Date(pending.acceptedAt).toLocaleString()}`,
      });
    }
  }

  if (model.state === 'reconnecting') {
    rows.push({ id: 'stale', label: 'Last device data', description: 'stale — reconnecting' });
  }

  if (model.baseline === 'mismatch') {
    rows.push({
      id: 'baseline',
      label: 'Baseline',
      description: 'stale (E0015) — re-sync from the device, rebuild, then connect',
    });
  }

  if (model.buildPhase !== null) {
    rows.push({ id: 'build', label: 'Build', description: buildPhaseLabel(model.buildPhase) });
  }

  return rows;
}

/** The panel's phase label for one build step, per the Mechanism-3 phase list. */
export function buildPhaseLabel(phase: BuildPhase): string {
  switch (phase) {
    case 'compiling':
      return 'Compiling…';
    case 'uploading':
      return 'Uploading…';
    case 'verifying':
      return 'Verifying…';
    case 'running':
      return 'Running…';
  }
}
