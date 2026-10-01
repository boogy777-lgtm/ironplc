import * as assert from 'assert';

import {
  connectionStateLabel,
  devicePanelRows,
  buildPhaseLabel,
  DevicePanelModel,
} from '../../devicePanelLogic';
import { HotEditStatus, IdentityInfo } from '../../hotEditSession';

const DEVICE = {
  name: 'ironplcvm',
  model: 'IronPLC SoftPLC',
  modification: 'vm-cli',
  firmwareVersion: '0.13.0',
};

function identity(overrides?: Partial<IdentityInfo>): IdentityInfo {
  return {
    protocol: 1,
    device: DEVICE,
    application: status(),
    ...overrides,
  };
}

function status(overrides?: Partial<HotEditStatus>): HotEditStatus {
  return {
    mode: 'normal',
    active: 1,
    normal: 1,
    candidate: null,
    application: 1,
    migration: false,
    rounds: 0,
    ...overrides,
  };
}

function model(overrides?: Partial<DevicePanelModel>): DevicePanelModel {
  return {
    state: 'disconnected',
    profileName: null,
    identity: null,
    lastStatus: null,
    baseline: 'none',
    buildPhase: null,
    ...overrides,
  };
}

function descriptions(rows: { id: string; description?: string }[]): Map<string, string | undefined> {
  return new Map(rows.map(row => [row.id, row.description]));
}

suite('connectionStateLabel', () => {
  test('connectionStateLabel_when_states_then_ux_vocabulary', () => {
    assert.strictEqual(connectionStateLabel('disconnected'), 'Not connected');
    assert.strictEqual(connectionStateLabel('connecting'), 'Connecting…');
    assert.strictEqual(connectionStateLabel('connected'), 'Connected');
    assert.strictEqual(connectionStateLabel('reconnecting'), 'Reconnecting…');
  });
});

suite('buildPhaseLabel', () => {
  test('buildPhaseLabel_when_phases_then_panel_vocabulary', () => {
    assert.strictEqual(buildPhaseLabel('compiling'), 'Compiling…');
    assert.strictEqual(buildPhaseLabel('uploading'), 'Uploading…');
    assert.strictEqual(buildPhaseLabel('verifying'), 'Verifying…');
    assert.strictEqual(buildPhaseLabel('running'), 'Running…');
  });
});

suite('devicePanelRows', () => {
  test('devicePanelRows_when_disconnected_then_single_honest_row', () => {
    const rows = devicePanelRows(model());
    assert.deepStrictEqual(rows, [{ id: 'state', label: 'Connection', description: 'Not connected' }]);
  });

  test('devicePanelRows_when_connected_then_device_identity_and_application', () => {
    const rows = devicePanelRows(model({
      state: 'connected',
      profileName: 'Cell 1',
      identity: identity(),
      lastStatus: status(),
    }));
    const byId = descriptions(rows);
    assert.strictEqual(byId.get('state'), 'Connected');
    assert.strictEqual(byId.get('profile'), 'Cell 1');
    assert.strictEqual(byId.get('device.name'), 'ironplcvm');
    assert.strictEqual(byId.get('device.model'), 'IronPLC SoftPLC');
    assert.strictEqual(byId.get('device.modification'), 'vm-cli');
    assert.strictEqual(byId.get('device.firmware'), '0.13.0');
    assert.strictEqual(byId.get('application'), 'Normal (gen 1)');
  });

  test('devicePanelRows_when_redundancy_block_present_then_pair_rows', () => {
    const rows = devicePanelRows(model({
      state: 'connected',
      identity: identity({ redundancy: { pairId: '7f3a9c', role: 'primary', epoch: 12, sync: 'syncReady', control: 'active' } }),
      lastStatus: status(),
    }));
    const byId = descriptions(rows);
    assert.strictEqual(byId.get('redundancy.pair'), '7f3a9c');
    assert.strictEqual(byId.get('redundancy.role'), 'primary');
    assert.strictEqual(byId.get('redundancy.epoch'), '12');
    assert.strictEqual(byId.get('redundancy.sync'), 'syncReady');
    assert.strictEqual(byId.get('redundancy.control'), 'active');
  });

  test('devicePanelRows_when_pending_edit_record_then_row_with_name_and_origin', () => {
    const rows = devicePanelRows(model({
      state: 'connected',
      identity: identity(),
      lastStatus: status({
        candidate: 2,
        pendingEdit: {
          name: 'main.st',
          origin: 'ironplc-vscode',
          acceptedAt: 1720000000000,
          baseline: { normalGeneration: 1, contentHash: [1, 2, 3] },
        },
      }),
    }));
    const row = rows.find(candidate => candidate.id === 'pendingEdit');
    assert.ok(row);
    assert.strictEqual(row.description, 'main.st');
    assert.ok(row.tooltip!.includes('ironplc-vscode'));
  });

  test('devicePanelRows_when_reconnecting_then_last_data_marked_stale', () => {
    const rows = devicePanelRows(model({
      state: 'reconnecting',
      profileName: 'Cell 1',
      identity: identity(),
      lastStatus: status(),
    }));
    const byId = descriptions(rows);
    assert.strictEqual(byId.get('state'), 'Reconnecting…');
    assert.strictEqual(byId.get('device.name'), 'ironplcvm');
    assert.strictEqual(byId.get('stale'), 'stale — reconnecting');
  });

  test('devicePanelRows_when_baseline_mismatch_then_resync_guidance', () => {
    const rows = devicePanelRows(model({
      state: 'connected',
      identity: identity(),
      lastStatus: status(),
      baseline: 'mismatch',
    }));
    const row = rows.find(candidate => candidate.id === 'baseline');
    assert.ok(row);
    assert.ok(row.description!.includes('E0015'));
    assert.ok(row.description!.includes('re-sync'));
  });

  test('devicePanelRows_when_build_phase_then_phase_row', () => {
    const rows = devicePanelRows(model({
      state: 'connected',
      identity: identity(),
      lastStatus: status(),
      buildPhase: 'uploading',
    }));
    const byId = descriptions(rows);
    assert.strictEqual(byId.get('build'), 'Uploading…');
  });

  test('devicePanelRows_when_migration_candidate_then_application_tooltip_warns_no_revert', () => {
    const rows = devicePanelRows(model({
      state: 'connected',
      identity: identity(),
      lastStatus: status({ migration: true }),
    }));
    const row = rows.find(candidate => candidate.id === 'application');
    assert.ok(row);
    assert.ok(row.tooltip!.includes('untest'));
  });
});
