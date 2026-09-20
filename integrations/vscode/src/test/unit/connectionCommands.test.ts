import * as assert from 'assert';
import * as fs from 'fs';
import * as path from 'path';

/**
 * Drift guard for the engineering-connection command surface: the command
 * ids and the device-panel view id live in `package.json` and in the
 * extension glue, and the structural-invariant check requires every
 * contributed command to appear in a test. Pinning both sides here keeps
 * the palette contributions and their registrations from diverging (the
 * same role the hot-edit command drift guard plays).
 */

const CONNECTION_COMMANDS: { id: string; title: string }[] = [
  { id: 'ironplc.connect', title: 'Connect to Device' },
  { id: 'ironplc.disconnect', title: 'Disconnect from Device' },
  { id: 'ironplc.build', title: 'Build & Commit' },
  { id: 'ironplc.buildTrial', title: 'Build & Trial' },
];

const DEVICE_PANEL_VIEW = 'ironplc.devicePanel';

interface CommandContribution {
  command: string;
  title: string;
  category?: string;
}

function packageJson(): {
  contributes: {
    commands: CommandContribution[];
    views?: Record<string, { id: string; name: string }[]>;
  };
} {
  return JSON.parse(
    fs.readFileSync(path.join(__dirname, '..', '..', '..', 'package.json'), 'utf-8'),
  );
}

suite('Engineering connection contributions', () => {
  test('package_json_when_connection_commands_then_all_declared_with_titles', () => {
    const declared = new Map(packageJson().contributes.commands.map(cmd => [cmd.command, cmd]));

    for (const expected of CONNECTION_COMMANDS) {
      const actual = declared.get(expected.id);
      assert.ok(actual, `${expected.id} is not contributed in package.json`);
      assert.strictEqual(actual.title, expected.title);
      assert.strictEqual(actual.category, 'IronPLC');
    }
  });

  test('package_json_when_device_panel_view_then_declared_in_explorer_container', () => {
    const views = packageJson().contributes.views ?? {};
    const explorerViews = views.explorer ?? [];
    const panel = explorerViews.find(view => view.id === DEVICE_PANEL_VIEW);
    assert.ok(panel, `${DEVICE_PANEL_VIEW} is not contributed in the explorer container`);
    assert.strictEqual(panel.name, 'IronPLC Device');
  });
});
