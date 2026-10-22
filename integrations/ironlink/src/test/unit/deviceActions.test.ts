import * as assert from 'assert';

import {
  DEVICE_ACTION_COMMANDS,
  DeviceAction,
  NO_DEVICE_ROW_REASON,
  chooseArgument,
  decideDeviceAction,
} from '../../deviceActions';
import { DeviceRowFacts } from '../../networkTree';
import { HANDOFF_ACTIONS } from '../../vscodeCommon';
import { PRESS_LINE_ID } from './browseFixture';

const ROW: DeviceRowFacts = {
  deviceId: PRESS_LINE_ID,
  name: 'Press line 3',
  address: '192.168.1.50',
  sessionPort: 49152,
  conflict: false,
  protected: false,
  routed: false,
};

const ALL_ACTIONS = Object.keys(DEVICE_ACTION_COMMANDS) as DeviceAction[];
const NOTHING = new Set<string>();
const WITHOUT_IRONPLC = { registeredCommands: new Set(['workbench.action.quickOpen']), contributedCommands: NOTHING };
const WITH_IRONPLC = {
  registeredCommands: new Set([HANDOFF_ACTIONS.choose.command, 'workbench.action.quickOpen']),
  contributedCommands: NOTHING,
};
// IronPLC is installed but has not activated: it registered nothing, and its manifest contributes the command.
const INACTIVE_IRONPLC = { registeredCommands: NOTHING, contributedCommands: new Set([HANDOFF_ACTIONS.choose.command]) };

// REQ-IL-ironlink-vscode-091
suite('decideDeviceAction', () => {
  test('decideDeviceAction_when_no_row_then_every_action_is_unavailable_with_the_same_reason', () => {
    for (const action of ALL_ACTIONS) {
      assert.deepStrictEqual(
        decideDeviceAction(action, undefined, WITH_IRONPLC),
        { enabled: false, reason: NO_DEVICE_ROW_REASON },
        action,
      );
    }
  });

  test('decideDeviceAction_when_identify_or_copy_on_a_row_then_enabled_without_ironplc', () => {
    assert.deepStrictEqual(decideDeviceAction('identify', ROW, WITHOUT_IRONPLC), { enabled: true });
    assert.deepStrictEqual(decideDeviceAction('copyDeviceId', ROW, WITHOUT_IRONPLC), { enabled: true });
  });

  test('decideDeviceAction_when_a_conflicting_or_protected_row_then_every_action_with_the_command_stays_enabled', () => {
    const row = { ...ROW, conflict: true, protected: true };
    assert.deepStrictEqual(decideDeviceAction('identify', row, WITHOUT_IRONPLC), { enabled: true });
    assert.deepStrictEqual(decideDeviceAction('copyDeviceId', row, WITHOUT_IRONPLC), { enabled: true });
    assert.deepStrictEqual(decideDeviceAction('choose', row, WITH_IRONPLC), { enabled: true });
  });
});

// REQ-IL-ironlink-vscode-092
suite('decideDeviceAction choose', () => {
  test('decideDeviceAction_when_choose_and_the_ironplc_command_is_missing_then_unavailable_with_its_reason', () => {
    assert.deepStrictEqual(
      decideDeviceAction('choose', ROW, WITHOUT_IRONPLC),
      { enabled: false, reason: HANDOFF_ACTIONS.choose.unavailableReason },
    );
  });

  test('decideDeviceAction_when_choose_and_the_ironplc_command_exists_then_enabled', () => {
    assert.deepStrictEqual(decideDeviceAction('choose', ROW, WITH_IRONPLC), { enabled: true });
  });

  test('decideDeviceAction_when_choose_and_ironplc_is_installed_but_not_active_then_enabled', () => {
    assert.deepStrictEqual(decideDeviceAction('choose', ROW, INACTIVE_IRONPLC), { enabled: true });
  });

  test('decideDeviceAction_when_choose_and_only_another_command_is_contributed_then_unavailable_with_its_reason', () => {
    const other = { registeredCommands: NOTHING, contributedCommands: new Set(['ironplc.buildApplication']) };
    assert.deepStrictEqual(
      decideDeviceAction('choose', ROW, other),
      { enabled: false, reason: HANDOFF_ACTIONS.choose.unavailableReason },
    );
  });

  test('decideDeviceAction_when_choose_then_depends_on_the_command_id_and_not_on_an_extension_id', () => {
    // Only registered command ids are in the context; no extension id is asked for.
    const onlyCommand = { registeredCommands: new Set(['ironplc.chooseController']), contributedCommands: NOTHING };
    assert.deepStrictEqual(decideDeviceAction('choose', ROW, onlyCommand), { enabled: true });
    const lookalike = { registeredCommands: new Set(['ironplc.ironplc', 'ironplc.ironplc-vscode']), contributedCommands: NOTHING };
    assert.strictEqual(decideDeviceAction('choose', ROW, lookalike).enabled, false);
  });
});

// REQ-IL-ironlink-vscode-093
suite('chooseArgument', () => {
  test('chooseArgument_when_row_then_has_exactly_device_id_address_and_session_port', () => {
    assert.deepStrictEqual(chooseArgument(ROW), {
      deviceId: PRESS_LINE_ID,
      address: '192.168.1.50',
      sessionPort: 49152,
    });
  });
});
