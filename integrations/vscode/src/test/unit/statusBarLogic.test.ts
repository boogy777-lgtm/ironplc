import * as assert from 'assert';

import { DevicePanelModel } from '../../devicePanelLogic';
import { HotEditStatus, IdentityInfo } from '../../hotEditSession';
import { Comparison } from '../../onlineComparisonLogic';
import { SUPPORTED_SESSION_PROTOCOL } from '../../sessionConstants';
import { statusCommand, statusText, statusTooltip } from '../../statusBarLogic';

const STATUS: HotEditStatus = {
  mode: 'normal',
  active: 1,
  normal: 1,
  candidate: null,
  application: 1,
  migration: false,
  rounds: 0,
};

const FAULT = { vCode: 'V4001', message: 'trap in task', taskId: 2, instanceId: 3 };

const IDENTITY: IdentityInfo = {
  protocol: SUPPORTED_SESSION_PROTOCOL,
  device: { name: 'Press line 3', model: 'IronPLC SoftPLC', modification: 'vm-cli', runtimeVersion: '0.247.0' },
  application: STATUS,
};

const COMPARISON: Comparison = {
  row: 'otherApplication',
  rowNumber: 9,
  summary: 'The controller holds an application that this workstation did not send.',
  connected: true,
  atOnce: false,
  controller: null,
  project: { status: 'upToDate', application: null, contentHash: null, layoutHash: null },
  lastDownload: null,
  information: [],
  actions: [],
  hotEdit: { allowed: false, reason: null },
};

function model(overrides?: Partial<DevicePanelModel>): DevicePanelModel {
  return {
    state: 'disconnected',
    profileName: null,
    identity: null,
    lastStatus: null,
    buildPhase: null,
    ...overrides,
  };
}

suite('statusText', () => {
  test('statusText_when_disconnected_and_nothing_chosen_then_not_connected', () => {
    assert.strictEqual(statusText(model(), undefined), '$(debug-disconnect) IronPLC: Not connected');
  });

  test('statusText_when_disconnected_and_a_controller_is_chosen_then_it_is_named_and_not_connected', () => {
    assert.strictEqual(statusText(model(), 'Press line 3'), '$(debug-disconnect) IronPLC: Press line 3 (not connected)');
    assert.strictEqual(statusText(model(), '00112233'), '$(debug-disconnect) IronPLC: 00112233 (not connected)');
  });

  test('statusText_when_connecting_or_reconnecting_then_unchanged_by_the_chosen_controller', () => {
    assert.strictEqual(statusText(model({ state: 'connecting' }), 'Press line 3'), '$(sync~spin) IronPLC: Connecting…');
    assert.strictEqual(statusText(model({ state: 'reconnecting' }), 'Press line 3'), '$(sync~spin) IronPLC: Reconnecting…');
  });

  test('statusText_when_connected_then_the_device_name_whatever_is_chosen', () => {
    const connected = model({ state: 'connected', profileName: 'Press', identity: IDENTITY });
    assert.strictEqual(statusText(connected, undefined), '$(plug) IronPLC: Press line 3 (running)');
    assert.strictEqual(statusText(connected, '00ffeedd'), '$(plug) IronPLC: Press line 3 (running)');
  });

  test('statusText_when_connected_with_a_comparison_then_the_summary_of_its_row_follows_the_name', () => {
    const connected = model({ state: 'connected', identity: IDENTITY, comparison: COMPARISON });
    assert.strictEqual(
      statusText(connected, undefined),
      `$(plug) IronPLC: Press line 3 (running) — ${COMPARISON.summary}`,
    );
  });

  test('statusText_when_connected_without_a_comparison_then_no_summary', () => {
    assert.strictEqual(statusText(model({ state: 'connected', identity: IDENTITY, comparison: null }), undefined), '$(plug) IronPLC: Press line 3 (running)');
  });

  test('statusText_when_connected_then_the_run_state_the_device_reports', () => {
    const stopped = model({ state: 'connected', identity: { ...IDENTITY, runState: 'stopped' } });
    assert.strictEqual(statusText(stopped, undefined), '$(plug) IronPLC: Press line 3 (stopped)');
    const faulted = model({ state: 'connected', identity: { ...IDENTITY, runState: 'running', fault: FAULT } });
    assert.strictEqual(statusText(faulted, undefined), '$(plug) IronPLC: Press line 3 (faulted V4001)');
  });

  test('statusText_when_connected_and_run_or_stop_answered_then_the_answer_wins_over_the_identity', () => {
    const connected = model({
      state: 'connected',
      identity: { ...IDENTITY, runState: 'running' },
      run: { runState: 'stopped' },
    });
    assert.strictEqual(statusText(connected, undefined), '$(plug) IronPLC: Press line 3 (stopped)');
  });

  test('statusText_when_connected_without_an_identity_then_the_profile_name_else_connected', () => {
    assert.strictEqual(statusText(model({ state: 'connected', profileName: 'Press' }), undefined), '$(plug) IronPLC: Press');
    assert.strictEqual(statusText(model({ state: 'connected' }), undefined), '$(plug) IronPLC: Connected');
  });

  test('statusText_when_disconnected_then_no_equality_or_run_state_placeholders', () => {
    const text = statusText(model(), 'Press line 3');
    assert.ok(!/EQUAL|DIFFERENT|Run|Stop/i.test(text), text);
  });
});

suite('statusTooltip', () => {
  test('statusTooltip_when_disconnected_and_nothing_chosen_then_offers_the_connect_command', () => {
    assert.strictEqual(statusTooltip(model(), undefined), 'Connect to a device (IronPLC: Connect to Device)');
  });

  test('statusTooltip_when_disconnected_and_chosen_then_names_the_controller_and_the_command', () => {
    const tooltip = statusTooltip(model(), 'Press line 3');
    assert.ok(tooltip.includes('Press line 3'));
    assert.ok(tooltip.includes('Connect to Chosen Controller'));
  });

  test('statusTooltip_when_connected_then_profile_status_and_the_comparison_hint', () => {
    const tooltip = statusTooltip(model({ state: 'connected', profileName: 'Press', lastStatus: STATUS }), undefined);
    assert.ok(tooltip.startsWith('Profile: Press'));
    assert.ok(tooltip.endsWith('Click for the go-online comparison (Show Online Comparison).'));
  });

  test('statusTooltip_when_connected_with_a_comparison_then_its_summary_follows_the_profile', () => {
    const tooltip = statusTooltip(model({ state: 'connected', profileName: 'Press', comparison: COMPARISON }), undefined);
    assert.strictEqual(tooltip.split('\n')[1], COMPARISON.summary);
  });

  test('statusTooltip_when_disconnected_after_a_failed_attempt_then_the_summary_of_the_attempt_comes_first', () => {
    const failed = { ...COMPARISON, row: 'unreachable', summary: 'The controller cannot be reached.', connected: false };
    const tooltip = statusTooltip(model({ comparison: failed }), undefined);
    assert.strictEqual(tooltip, 'The controller cannot be reached.\nConnect to a device (IronPLC: Connect to Device)');
  });

  test('statusTooltip_when_connected_then_the_run_state_and_the_fault_in_full', () => {
    const faulted = model({
      state: 'connected',
      profileName: 'Press',
      identity: { ...IDENTITY, fault: FAULT },
      run: { runState: 'stopped', fault: FAULT },
    });
    const tooltip = statusTooltip(faulted, undefined);
    assert.ok(tooltip.includes('Run state: stopped'), tooltip);
    assert.ok(tooltip.includes('Fault: V4001 - trap in task (task 2, instance 3)'), tooltip);
  });

  test('statusTooltip_when_connected_without_an_identity_or_answer_then_no_run_state_line', () => {
    const tooltip = statusTooltip(model({ state: 'connected', profileName: 'Press' }), undefined);
    assert.ok(!tooltip.includes('Run state'), tooltip);
  });

  test('statusTooltip_when_connecting_or_reconnecting_then_says_what_is_happening', () => {
    assert.ok(statusTooltip(model({ state: 'connecting' }), undefined).includes('identity handshake'));
    assert.ok(statusTooltip(model({ state: 'reconnecting' }), undefined).includes('retrying'));
  });
});

suite('statusCommand', () => {
  test('statusCommand_when_disconnected_and_a_controller_is_chosen_then_connects_to_it', () => {
    assert.strictEqual(statusCommand('disconnected', 'Press line 3'), 'ironplc.connectController');
  });

  test('statusCommand_when_disconnected_and_nothing_is_chosen_then_the_connect_command_as_before', () => {
    assert.strictEqual(statusCommand('disconnected', undefined), 'ironplc.connect');
  });

  test('statusCommand_when_connected_then_the_comparison_is_shown_again_whatever_is_chosen', () => {
    assert.strictEqual(statusCommand('connected', 'Press line 3'), 'ironplc.showOnlineComparison');
  });

  test('statusCommand_when_connecting_or_reconnecting_then_the_device_panel', () => {
    for (const state of ['connecting', 'reconnecting'] as const) {
      assert.strictEqual(statusCommand(state, 'Press line 3'), 'ironplc.devicePanel.focus', state);
    }
  });
});
