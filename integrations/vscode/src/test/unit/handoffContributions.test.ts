import * as assert from 'assert';
import * as fs from 'fs';
import * as path from 'path';

import { HANDOFF_ACTIONS, handoffNames } from '../../vscodeCommon';

/**
 * The handoff commands that the IronLink extension runs are declared once, in the
 * shared table. This extension registers each of them (`handoffCommands.ts`, one
 * handler for each member, checked by the compiler) and contributes each in
 * `package.json`, so it shows in the command palette. This keeps the two
 * together.
 */

interface CommandContribution {
  command: string;
  title: string;
  category?: string;
}

function contributedCommands(): Map<string, CommandContribution> {
  const packageJson = JSON.parse(
    fs.readFileSync(path.join(__dirname, '..', '..', '..', 'package.json'), 'utf-8'),
  ) as { contributes: { commands: CommandContribution[] } };
  return new Map(packageJson.contributes.commands.map(cmd => [cmd.command, cmd]));
}

suite('Handoff command contributions', () => {
  test('package_json_when_a_handoff_action_then_its_command_is_contributed', () => {
    const declared = contributedCommands();
    for (const name of handoffNames()) {
      assert.ok(declared.has(HANDOFF_ACTIONS[name].command), `${HANDOFF_ACTIONS[name].command} is not contributed in package.json`);
    }
  });

  test('package_json_when_choose_controller_then_titled_and_in_the_network_category', () => {
    const choose = contributedCommands().get('ironplc.chooseController');
    assert.ok(choose);
    assert.strictEqual(choose.title, 'Choose Controller…');
    assert.strictEqual(choose.category, 'IronPLC: Network');
  });

  test('handoff_actions_when_listed_then_the_choose_command_is_the_one_the_extension_contributes', () => {
    assert.strictEqual(HANDOFF_ACTIONS.choose.command, 'ironplc.chooseController');
  });
});
