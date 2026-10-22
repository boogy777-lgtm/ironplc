import * as assert from 'assert';
import * as fs from 'fs';
import * as path from 'path';

import { DEVICE_ACTION_COMMANDS } from '../../deviceActions';

// out/test/unit -> the extension root
const manifest = JSON.parse(fs.readFileSync(path.join(__dirname, '..', '..', '..', 'package.json'), 'utf-8'));

interface Command {
  command: string;
  title: string;
  category: string;
}

const commands: Command[] = manifest.contributes.commands;

suite('the manifest', () => {
  test('commands_when_declared_then_are_the_view_commands_and_the_device_actions_in_the_ironlink_category', () => {
    assert.deepStrictEqual(
      commands.map(command => command.command).sort(),
      [
        'ironlink.browse',
        'ironlink.addAddress',
        'ironlink.removeAddress',
        'ironlink.identify',
        'ironlink.copyDeviceId',
        'ironlink.choose',
      ].sort(),
    );
    assert.deepStrictEqual(DEVICE_ACTION_COMMANDS, {
      identify: 'ironlink.identify',
      copyDeviceId: 'ironlink.copyDeviceId',
      choose: 'ironlink.choose',
    });
    for (const command of commands) {
      assert.strictEqual(command.category, 'IronLink', command.command);
    }
  });

  test('commands_when_declared_then_the_titles_are_the_ones_the_view_shows', () => {
    const titles = Object.fromEntries(commands.map(command => [command.command, command.title]));
    assert.strictEqual(titles['ironlink.browse'], 'Search network');
    assert.strictEqual(titles['ironlink.addAddress'], 'Add address…');
    assert.strictEqual(titles['ironlink.removeAddress'], 'Remove address');
    assert.strictEqual(titles[DEVICE_ACTION_COMMANDS.identify], 'Identify');
    assert.strictEqual(titles[DEVICE_ACTION_COMMANDS.copyDeviceId], 'Copy device id');
    assert.strictEqual(titles[DEVICE_ACTION_COMMANDS.choose], 'Choose');
  });

  test('views_when_declared_then_the_ironlink_container_holds_the_network_view', () => {
    assert.strictEqual(manifest.contributes.viewsContainers.activitybar[0].id, 'ironlink');
    assert.strictEqual(manifest.contributes.viewsContainers.activitybar[0].title, 'IronLink');
    assert.deepStrictEqual(manifest.contributes.views.ironlink.map((view: { id: string }) => view.id), ['ironlink.network']);
  });

  test('view_title_when_declared_then_offers_search_network_and_add_address', () => {
    const inTitle = manifest.contributes.menus['view/title'].map((item: { command: string }) => item.command);
    assert.deepStrictEqual(inTitle, ['ironlink.browse', 'ironlink.addAddress']);
  });

  test('address_list_driver_when_declared_then_offers_add_and_remove_address_on_its_row', () => {
    const onDriver = manifest.contributes.menus['view/item/context']
      .filter((item: { when: string }) => item.when.includes('viewItem == driver.addressList'))
      .map((item: { command: string }) => item.command);
    assert.deepStrictEqual(onDriver, ['ironlink.addAddress', 'ironlink.removeAddress']);
  });

  test('settings_when_declared_then_are_the_path_and_the_two_browse_defaults', () => {
    assert.deepStrictEqual(
      Object.keys(manifest.contributes.configuration.properties).sort(),
      ['ironlink.browse.attempts', 'ironlink.browse.timeoutMs', 'ironlink.path'],
    );
  });

  test('identity_when_declared_then_is_the_ironlink_extension_of_the_ironplc_publisher', () => {
    assert.strictEqual(manifest.name, 'ironlink');
    assert.strictEqual(manifest.publisher, 'ironplc');
  });
});
