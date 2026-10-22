import * as assert from 'assert';
import { HANDOFF_ACTIONS, handoffNames, isHandoffAvailable, runHandoff } from '../handoff';

suite('HANDOFF_ACTIONS', () => {
  test('HANDOFF_ACTIONS_when_choose_then_names_the_ironplc_command', () => {
    assert.strictEqual(HANDOFF_ACTIONS.choose.command, 'ironplc.chooseController');
  });

  test('handoffNames_when_listed_then_each_member_declares_command_and_reason', () => {
    const names = handoffNames();
    assert.deepStrictEqual(names, ['choose']);
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

suite('isHandoffAvailable', () => {
  test('isHandoffAvailable_when_the_command_is_registered_then_true', () => {
    assert.strictEqual(isHandoffAvailable('choose', new Set(['ironplc.chooseController', 'other'])), true);
  });

  test('isHandoffAvailable_when_the_command_is_not_registered_then_false', () => {
    assert.strictEqual(isHandoffAvailable('choose', new Set(['ironplc.runProgram'])), false);
  });
});

suite('runHandoff', () => {
  test('runHandoff_when_choose_then_executes_the_command_with_the_argument', async () => {
    const calls: unknown[][] = [];
    const argument = { deviceId: '00112233445566778899aabbccddeeff', address: '192.168.1.50', sessionPort: 49152 };
    await runHandoff('choose', argument, async (command, ...args) => { calls.push([command, ...args]); });
    assert.deepStrictEqual(calls, [['ironplc.chooseController', argument]]);
  });

  test('runHandoff_when_the_command_fails_then_the_failure_reaches_the_caller', async () => {
    await assert.rejects(
      runHandoff('choose', { deviceId: 'x', address: 'y', sessionPort: 1 }, async () => { throw new Error('boom'); }),
      /boom/,
    );
  });
});
