import * as assert from 'assert';

import { DevicePanelModel } from '../../devicePanelLogic';
import { HotEditStatus, IdentityInfo } from '../../hotEditSession';
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

const IDENTITY: IdentityInfo = {
  protocol: SUPPORTED_SESSION_PROTOCOL,
  device: { name: 'Press line 3', model: 'IronPLC SoftPLC', modification: 'vm-cli', runtimeVersion: '0.247.0' },
  application: STATUS,
};

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
    assert.strictEqual(statusText(connected, undefined), '$(plug) IronPLC: Press line 3');
    assert.strictEqual(statusText(connected, '00ffeedd'), '$(plug) IronPLC: Press line 3');
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

  test('statusTooltip_when_connected_then_profile_status_and_the_panel_hint', () => {
    const tooltip = statusTooltip(model({ state: 'connected', profileName: 'Press', lastStatus: STATUS }), undefined);
    assert.ok(tooltip.startsWith('Profile: Press'));
    assert.ok(tooltip.endsWith('Click for the IronPLC Device panel.'));
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

  test('statusCommand_when_not_disconnected_then_the_device_panel_whatever_is_chosen', () => {
    for (const state of ['connecting', 'connected', 'reconnecting'] as const) {
      assert.strictEqual(statusCommand(state, 'Press line 3'), 'ironplc.devicePanel.focus', state);
    }
  });
});
