import * as assert from 'assert';
import * as fs from 'fs';
import * as path from 'path';

/**
 * Drift guard for the hot-edit command surface: the command ids live in
 * `package.json` and in the extension glue, and the structural-invariant
 * check requires every contributed command to appear in a test. Pinning both
 * sides here keeps the palette contributions and their registrations from
 * diverging (the same role the CLI drift guards play for the compiler).
 */

const HOT_EDIT_COMMANDS: { id: string; title: string }[] = [
  { id: 'ironplc.startHotEditSession', title: 'Start Session' },
  { id: 'ironplc.acceptEdits', title: 'Accept Edits' },
  { id: 'ironplc.testEdits', title: 'Test Edits' },
  { id: 'ironplc.untestEdits', title: 'Untest Edits' },
  { id: 'ironplc.assembleEdits', title: 'Assemble Edits' },
  { id: 'ironplc.cancelEdits', title: 'Cancel Edits' },
  { id: 'ironplc.showHotEditStatus', title: 'Show Status' },
];

interface CommandContribution {
  command: string;
  title: string;
  category?: string;
}

function contributedCommands(): CommandContribution[] {
  const packageJson = JSON.parse(
    fs.readFileSync(path.join(__dirname, '..', '..', '..', 'package.json'), 'utf-8'),
  );
  return packageJson.contributes.commands;
}

suite('Hot edit command contributions', () => {
  test('package_json_when_hot_edit_commands_then_all_declared_with_titles', () => {
    const declared = new Map(contributedCommands().map(cmd => [cmd.command, cmd]));

    for (const expected of HOT_EDIT_COMMANDS) {
      const actual = declared.get(expected.id);
      assert.ok(actual, `${expected.id} is not contributed in package.json`);
      assert.strictEqual(actual.title, expected.title);
      assert.strictEqual(actual.category, 'IronPLC Hot Edit');
    }
  });
});
