import * as assert from 'assert';
import * as fs from 'fs';
import * as path from 'path';
import {
  ACTIONS_CONTAINER_ID,
  ACTIONS_VIEW_ID,
  actionItems,
  buildActionGroups,
} from '../../iplcViewLogic';

/**
 * The manifest is the source of truth the model must not drift from: the
 * tests read the real `package.json` rather than a restated copy, so a
 * command rename or a removed contribution fails here.
 */
interface Manifest {
  contributes: {
    commands: { command: string }[];
    viewsContainers: { activitybar: { id: string; icon: string }[] };
    views: Record<string, { id: string }[]>;
  };
}

function readManifest(): Manifest {
  const manifestPath = path.join(__dirname, '..', '..', '..', 'package.json');
  return JSON.parse(fs.readFileSync(manifestPath, 'utf-8')) as Manifest;
}

suite('action model', () => {
  test('buildActionGroups_then_all_group_ids_unique_and_labels_set', () => {
    const groups = buildActionGroups();
    const ids = groups.map(group => group.id);
    assert.strictEqual(new Set(ids).size, ids.length);
    assert.ok(groups.length > 0);
    for (const group of groups) {
      assert.ok(group.label.length > 0);
      assert.ok(group.items.length > 0);
    }
  });

  test('buildActionGroups_then_all_item_ids_unique_and_labels_and_icons_set', () => {
    const items = actionItems(buildActionGroups());
    const ids = items.map(item => item.id);
    assert.strictEqual(new Set(ids).size, ids.length);
    for (const item of items) {
      assert.ok(item.label.length > 0);
      assert.ok(item.icon.length > 0);
    }
  });

  test('buildActionGroups_then_every_command_is_declared_in_the_manifest', () => {
    const declared = new Set(readManifest().contributes.commands.map(c => c.command));
    for (const item of actionItems(buildActionGroups())) {
      assert.ok(declared.has(item.command), `undeclared command in actions view: ${item.command}`);
    }
  });

  test('buildActionGroups_then_hot_edit_group_exposes_every_hot_edit_command', () => {
    const hotEditCommands = actionItems(buildActionGroups())
      .map(item => item.command)
      .filter(command => command.startsWith('ironplc.') && command !== 'ironplc.runProgram'
        && command !== 'ironplc.stopProgram' && command !== 'ironplc.pauseProgram'
        && command !== 'ironplc.stepScan');
    assert.deepStrictEqual(hotEditCommands, [
      'ironplc.startHotEditSession',
      'ironplc.acceptEdits',
      'ironplc.testEdits',
      'ironplc.untestEdits',
      'ironplc.assembleEdits',
      'ironplc.cancelEdits',
      'ironplc.showHotEditStatus',
      'ironplc.syncVariableIds',
    ]);
  });

  test('actionsView_then_view_id_and_container_are_declared_in_the_manifest', () => {
    const manifest = readManifest();
    const viewIds = (manifest.contributes.views[ACTIONS_CONTAINER_ID] ?? []).map(view => view.id);
    assert.ok(viewIds.includes(ACTIONS_VIEW_ID));
    const containerIds = manifest.contributes.viewsContainers.activitybar.map(container => container.id);
    assert.ok(containerIds.includes(ACTIONS_CONTAINER_ID));
  });

  test('actionsIcon_then_manifest_points_at_an_existing_single_color_svg', () => {
    const manifest = readManifest();
    const container = manifest.contributes.viewsContainers.activitybar
      .find(candidate => candidate.id === ACTIONS_CONTAINER_ID);
    assert.ok(container);
    const iconPath = path.join(__dirname, '..', '..', '..', container.icon);
    const svg = fs.readFileSync(iconPath, 'utf-8');
    assert.ok(svg.includes('currentColor'), 'activity bar icon must use currentColor');
    assert.ok(!svg.includes('<image'), 'activity bar icon must be a single-color vector');
  });
});
