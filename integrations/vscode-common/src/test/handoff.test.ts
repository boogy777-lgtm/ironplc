import * as assert from 'assert';
import {
  CommandSources,
  HANDOFF_ACTIONS,
  contributedCommandIds,
  handoffNames,
  isControllerArgument,
  isHandoffAvailable,
  runHandoff,
} from '../handoff';

suite('HANDOFF_ACTIONS', () => {
  test('HANDOFF_ACTIONS_when_choose_then_names_the_ironplc_command', () => {
    assert.strictEqual(HANDOFF_ACTIONS.choose.command, 'ironplc.chooseController');
  });

  test('HANDOFF_ACTIONS_when_connect_then_names_the_ironplc_command', () => {
    assert.strictEqual(HANDOFF_ACTIONS.connect.command, 'ironplc.connectController');
  });

  test('handoffNames_when_listed_then_each_member_declares_command_and_reason', () => {
    const names = handoffNames();
    assert.deepStrictEqual(names, ['choose', 'connect']);
    for (const name of names) {
      const declaration = HANDOFF_ACTIONS[name];
      assert.ok(declaration.command.startsWith('ironplc.'), `${name}: command is an IronPLC command`);
      assert.ok(declaration.unavailableReason.length > 0, `${name}: has a reason`);
    }
  });

  test('handoffNames_when_listed_then_command_ids_are_unique', () => {
    const commands = handoffNames().map(name => HANDOFF_ACTIONS[name].command);
    assert.strictEqual(new Set(commands).size, commands.length);
  });
});

function sources(registered: string[], contributed: string[]): CommandSources {
  return { registered: new Set(registered), contributed: new Set(contributed) };
}

suite('isHandoffAvailable', () => {
  test('isHandoffAvailable_when_the_command_is_only_contributed_then_true', () => {
    assert.strictEqual(isHandoffAvailable('choose', sources([], ['ironplc.chooseController', 'other'])), true);
  });

  test('isHandoffAvailable_when_the_command_is_only_registered_then_true', () => {
    assert.strictEqual(isHandoffAvailable('choose', sources(['ironplc.chooseController', 'other'], [])), true);
  });

  test('isHandoffAvailable_when_the_command_is_both_contributed_and_registered_then_true', () => {
    assert.strictEqual(isHandoffAvailable('choose', sources(['ironplc.chooseController'], ['ironplc.chooseController'])), true);
  });

  test('isHandoffAvailable_when_only_the_other_action_is_available_then_false', () => {
    assert.strictEqual(isHandoffAvailable('connect', sources(['ironplc.chooseController'], ['ironplc.chooseController'])), false);
    assert.strictEqual(isHandoffAvailable('connect', sources([], ['ironplc.connectController'])), true);
    assert.strictEqual(isHandoffAvailable('connect', sources(['ironplc.connectController'], [])), true);
  });

  test('isHandoffAvailable_when_neither_contributed_nor_registered_then_false', () => {
    assert.strictEqual(isHandoffAvailable('choose', sources(['ironplc.runProgram'], ['ironplc.buildApplication'])), false);
    assert.strictEqual(isHandoffAvailable('choose', sources([], [])), false);
  });
});

suite('contributedCommandIds', () => {
  test('contributedCommandIds_when_manifests_contribute_commands_then_collects_every_id', () => {
    const ids = contributedCommandIds([
      { contributes: { commands: [{ command: 'a.one' }, { command: 'a.two' }] } },
      { contributes: { commands: [{ command: 'b.one' }, { command: 'a.one' }] } },
    ]);
    assert.deepStrictEqual([...ids].sort(), ['a.one', 'a.two', 'b.one']);
  });

  test('contributedCommandIds_when_a_manifest_has_no_or_malformed_commands_then_it_contributes_nothing', () => {
    const ids = contributedCommandIds([
      undefined,
      null,
      'text',
      {},
      { contributes: {} },
      { contributes: { commands: 'not a list' } },
      { contributes: { commands: [null, 5, {}, { command: 7 }, { command: 'ok.one' }] } },
    ]);
    assert.deepStrictEqual([...ids], ['ok.one']);
  });

  test('contributedCommandIds_when_no_manifests_then_empty', () => {
    assert.strictEqual(contributedCommandIds([]).size, 0);
  });
});

suite('runHandoff', () => {
  test('runHandoff_when_choose_then_executes_the_command_with_the_argument', async () => {
    const calls: unknown[][] = [];
    const argument = { deviceId: '00112233445566778899aabbccddeeff', address: '192.168.1.50', sessionPort: 49152 };
    await runHandoff('choose', argument, async (command, ...args) => { calls.push([command, ...args]); });
    assert.deepStrictEqual(calls, [['ironplc.chooseController', argument]]);
  });

  test('runHandoff_when_connect_then_executes_the_command_with_the_argument', async () => {
    const calls: unknown[][] = [];
    const argument = { deviceId: '00112233445566778899aabbccddeeff', address: '192.168.1.50', sessionPort: 49152 };
    await runHandoff('connect', argument, async (command, ...args) => { calls.push([command, ...args]); });
    assert.deepStrictEqual(calls, [['ironplc.connectController', argument]]);
  });

  test('runHandoff_when_the_command_fails_then_the_failure_reaches_the_caller', async () => {
    await assert.rejects(
      runHandoff('choose', { deviceId: 'x', address: 'y', sessionPort: 1 }, async () => { throw new Error('boom'); }),
      /boom/,
    );
  });
});

suite('isControllerArgument', () => {
  const valid = { deviceId: '00112233445566778899aabbccddeeff', address: '192.168.1.50', sessionPort: 49152 };

  test('isControllerArgument_when_the_three_fields_are_valid_then_true', () => {
    assert.strictEqual(isControllerArgument(valid), true);
  });

  test('isControllerArgument_when_not_an_object_then_false', () => {
    for (const value of [undefined, null, 'x', 5, []]) {
      assert.strictEqual(isControllerArgument(value), false, String(value));
    }
  });

  test('isControllerArgument_when_a_field_is_wrong_then_false', () => {
    const wrong: unknown[] = [
      { ...valid, deviceId: '0011' },
      { ...valid, deviceId: valid.deviceId.toUpperCase() },
      { ...valid, deviceId: 5 },
      { ...valid, address: '' },
      { ...valid, address: 5 },
      { ...valid, sessionPort: 0 },
      { ...valid, sessionPort: 65536 },
      { ...valid, sessionPort: 1.5 },
      { ...valid, sessionPort: '49152' },
      { address: valid.address, sessionPort: valid.sessionPort },
    ];
    for (const value of wrong) {
      assert.strictEqual(isControllerArgument(value), false, JSON.stringify(value));
    }
  });
});
