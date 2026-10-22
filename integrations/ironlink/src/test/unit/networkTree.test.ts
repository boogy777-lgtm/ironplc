import * as assert from 'assert';

import { parseResponseLine } from '../../ironlinkProtocol';
import {
  NetworkNode,
  buildNetworkTree,
  driverContextValue,
  driverLabel,
} from '../../networkTree';
import { shortDeviceId } from '../../vscodeCommon';
import {
  EMPTY_BROWSE_LINE,
  FIXTURE_NOW,
  MIXER_ID,
  PACKER_ID,
  PRESS_LINE_ID,
  SILENT_ID,
  browseFixture,
} from './browseFixture';

function tree(): NetworkNode {
  const roots = buildNetworkTree(browseFixture(), FIXTURE_NOW);
  assert.strictEqual(roots.length, 1);
  return roots[0];
}

function driver(root: NetworkNode, kind: string): NetworkNode {
  const found = root.children.find(child => child.id === `driver:${kind}`);
  assert.ok(found, `driver ${kind}`);
  return found;
}

function devices(node: NetworkNode): NetworkNode[] {
  return node.children.filter(child => child.kind === 'device');
}

// REQ-IL-ironlink-vscode-085
suite('buildNetworkTree structure', () => {
  test('buildNetworkTree_when_browse_then_workstation_is_the_root_with_a_row_per_driver', () => {
    const root = tree();
    assert.strictEqual(root.kind, 'workstation');
    assert.strictEqual(root.label, 'PC-ONE');
    assert.deepStrictEqual(
      root.children.filter(child => child.kind === 'driver').map(child => child.label),
      ['Broadcast', 'Address list'],
    );
  });

  test('buildNetworkTree_when_driver_then_description_holds_its_targets', () => {
    const root = tree();
    assert.strictEqual(driver(root, 'broadcast').description, '192.168.1.20 -> 255.255.255.255:49152');
    assert.strictEqual(driver(root, 'addressList').description, '10.0.4.7:49152, 192.168.1.50:49152');
  });

  test('buildNetworkTree_when_driver_then_context_value_names_the_driver_kind', () => {
    const root = tree();
    assert.strictEqual(driver(root, 'addressList').contextValue, 'driver.addressList');
    assert.strictEqual(driver(root, 'addressList').contextValue, driverContextValue('addressList'));
  });

  test('buildNetworkTree_when_driver_heard_nothing_then_one_info_row_says_so', () => {
    const empty = parseResponseLine(EMPTY_BROWSE_LINE) as ReturnType<typeof browseFixture>;
    const [root] = buildNetworkTree(empty, FIXTURE_NOW);
    const [row, ...rest] = root.children[0].children;
    assert.strictEqual(rest.length, 0);
    assert.strictEqual(row.kind, 'info');
    assert.strictEqual(row.label, 'No controller answered');
  });

  test('buildNetworkTree_when_no_routes_are_unseen_then_there_is_no_routes_not_seen_node', () => {
    const empty = parseResponseLine(EMPTY_BROWSE_LINE) as ReturnType<typeof browseFixture>;
    const [root] = buildNetworkTree(empty, FIXTURE_NOW);
    assert.ok(!root.children.some(child => child.kind === 'notSeenGroup'));
  });

  test('buildNetworkTree_when_built_twice_then_ids_are_stable_and_unique', () => {
    const ids = (node: NetworkNode): string[] => [node.id, ...node.children.flatMap(ids)];
    const first = ids(tree());
    const second = ids(tree());
    assert.deepStrictEqual(first, second);
    assert.strictEqual(new Set(first).size, first.length);
  });
});

// REQ-IL-ironlink-vscode-086
suite('buildNetworkTree device rows', () => {
  test('buildNetworkTree_when_device_then_label_is_the_name_and_description_has_address_id_and_age', () => {
    const rows = devices(driver(tree(), 'addressList'));
    assert.strictEqual(rows.length, 1);
    assert.strictEqual(rows[0].label, 'Press line 3');
    assert.strictEqual(rows[0].description, '192.168.1.50:49152  00112233  just now');
  });

  test('buildNetworkTree_when_route_is_the_stored_route_then_description_has_the_route_marker', () => {
    const packer = devices(driver(tree(), 'broadcast')).find(row => row.device?.deviceId === PACKER_ID);
    assert.ok(packer);
    assert.strictEqual(packer.description, '192.168.1.60:49152  0f0e0d0c  route  protected  1 s ago');
    assert.strictEqual(packer.device?.routed, true);
    assert.strictEqual(packer.device?.protected, true);
  });

  test('buildNetworkTree_when_route_is_not_stored_then_no_route_marker', () => {
    const rows = devices(driver(tree(), 'broadcast'));
    const press = rows.find(row => row.device?.deviceId === PRESS_LINE_ID);
    assert.ok(press);
    assert.ok(!press.description.split('  ').includes('route'));
    assert.ok(!press.description.includes('protected'));
  });

  test('buildNetworkTree_when_device_then_the_row_carries_the_facts_the_actions_need', () => {
    const press = devices(driver(tree(), 'broadcast')).find(row => row.device?.deviceId === PRESS_LINE_ID);
    assert.deepStrictEqual(press?.device, {
      deviceId: PRESS_LINE_ID,
      name: 'Press line 3',
      address: '192.168.1.50',
      sessionPort: 49152,
      conflict: false,
      protected: false,
      routed: false,
    });
    assert.strictEqual(press?.contextValue, 'device');
  });

  test('buildNetworkTree_when_device_then_tooltip_shows_the_full_observation', () => {
    const press = devices(driver(tree(), 'broadcast')).find(row => row.device?.deviceId === PRESS_LINE_ID);
    const tooltip = press?.tooltip ?? '';
    for (const expected of [
      'Press line 3',
      'Model: IronPLC SoftPLC',
      'Runtime: 0.247.0',
      'Session protocol: 1',
      `Device id: ${PRESS_LINE_ID}`,
      'Credentials required: no',
      'Route: 192.168.1.50:49152 by Broadcast (tcp)',
      'Seen: 2025-10-22T',
      'Stored route: not this route',
    ]) {
      assert.ok(tooltip.includes(expected), `tooltip has "${expected}": ${tooltip}`);
    }
  });
});

// REQ-IL-ironlink-vscode-087
suite('buildNetworkTree conflicts and shared devices', () => {
  test('buildNetworkTree_when_device_is_seen_by_two_drivers_then_it_is_under_both', () => {
    const root = tree();
    const underBroadcast = devices(driver(root, 'broadcast')).filter(row => row.device?.deviceId === PRESS_LINE_ID);
    const underList = devices(driver(root, 'addressList')).filter(row => row.device?.deviceId === PRESS_LINE_ID);
    assert.strictEqual(underBroadcast.length, 1);
    assert.strictEqual(underList.length, 1);
    assert.notStrictEqual(underBroadcast[0].id, underList[0].id);
  });

  test('buildNetworkTree_when_identity_conflict_then_each_route_row_has_the_conflict_marker', () => {
    const mixers = devices(driver(tree(), 'broadcast')).filter(row => row.device?.deviceId === MIXER_ID);
    assert.deepStrictEqual(mixers.map(row => row.label), ['Mixer', 'Mixer (copy)']);
    for (const row of mixers) {
      assert.ok(row.description.split('  ').includes('conflict'), row.description);
      assert.strictEqual(row.device?.conflict, true);
      assert.ok(row.tooltip.includes('Identity conflict: 2 different answers under this device id'));
    }
    assert.deepStrictEqual(mixers.map(row => row.device?.address), ['192.168.1.71', '192.168.1.72']);
  });

  test('buildNetworkTree_when_no_conflict_then_no_row_has_the_conflict_marker', () => {
    const rows = devices(driver(tree(), 'addressList'));
    assert.ok(rows.every(row => !row.description.includes('conflict')));
  });

  test('buildNetworkTree_when_a_device_has_two_routes_then_each_route_is_its_own_row', () => {
    const browse = browseFixture();
    const press = browse.drivers[0].devices.find(device => device.deviceId === PRESS_LINE_ID)!;
    press.variants[0].routes.push({ ...press.variants[0].routes[0], address: '10.1.1.5' });
    const [root] = buildNetworkTree(browse, FIXTURE_NOW);
    const rows = devices(driver(root, 'broadcast')).filter(row => row.device?.deviceId === PRESS_LINE_ID);
    assert.deepStrictEqual(rows.map(row => row.device?.address), ['192.168.1.50', '10.1.1.5']);
  });

  test('buildNetworkTree_when_another_wire_version_answered_then_a_row_says_to_update_ironlink', () => {
    const rows = driver(tree(), 'broadcast').children.filter(child => child.kind === 'otherVersion');
    assert.strictEqual(rows.length, 1);
    assert.strictEqual(rows[0].label, 'Discovery version 2');
    assert.ok(rows[0].description.includes('update IronLink'));
    assert.ok(rows[0].description.includes('192.168.1.90'));
  });
});

// REQ-IL-ironlink-vscode-088
suite('buildNetworkTree routes not seen', () => {
  test('buildNetworkTree_when_routes_not_seen_then_a_node_lists_them_after_the_drivers', () => {
    const root = tree();
    const last = root.children[root.children.length - 1];
    assert.strictEqual(last.kind, 'notSeenGroup');
    assert.strictEqual(last.label, 'Routes not seen');
    assert.strictEqual(last.description, '1 route');
    assert.strictEqual(last.children.length, 1);
    assert.strictEqual(last.children[0].label, shortDeviceId(SILENT_ID));
    assert.ok(last.children[0].description.startsWith('10.0.4.7:49152  verified '));
    assert.ok(last.children[0].tooltip.includes(`Device id: ${SILENT_ID}`));
  });
});

suite('tree wording helpers', () => {
  test('driverLabel_when_camel_case_kind_then_words_with_a_capital', () => {
    assert.strictEqual(driverLabel('broadcast'), 'Broadcast');
    assert.strictEqual(driverLabel('addressList'), 'Address list');
    assert.strictEqual(driverLabel('usbEnumeration'), 'Usb enumeration');
  });
});
