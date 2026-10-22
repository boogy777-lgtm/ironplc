import * as assert from 'assert';
import * as path from 'path';

import {
  applicationAnchor,
  buildPickEntries,
  ChosenController,
  decidePick,
  describeIronLinkFailure,
  IronLinkState,
  parsePathRecord,
  PickEntry,
  readChosen,
  recordPathArgs,
  recordPathPathArgs,
  SEARCH_LABEL,
  writeChoice,
} from '../../chooseControllerLogic';
import { BrowseSnapshot, ControllerArgument, IronLinkRoute } from '../../vscodeCommon';

const PRESS = '00112233445566778899aabbccddeeff';
const MIXER = 'a0a1a2a3a4a5a6a7a8a9aaabacadaeaf';
const PACKER = '0f0e0d0c0b0a09080706050403020100';
const NOW = 1761136020000;

const CONTROLLER: ControllerArgument = { deviceId: PRESS, address: '192.168.1.50', sessionPort: 49152 };

function recordText(overrides: Record<string, unknown> = {}): string {
  return JSON.stringify({
    version: 1,
    deviceId: PRESS,
    routeHint: { address: '192.168.1.50', sessionPort: 49152 },
    chosenAt: NOW - 3 * 86_400_000,
    ...overrides,
  });
}

const CHOSEN: ChosenController = {
  deviceId: PRESS,
  routeHint: { address: '192.168.1.50', sessionPort: 49152 },
  chosenAt: NOW - 3 * 86_400_000,
};

function route(deviceId: string, address: string, verifiedAt = NOW - 60_000): IronLinkRoute {
  return { deviceId, address, sessionPort: 49152, driver: 'addressList', verifiedAt };
}

const BROWSE: BrowseSnapshot = {
  browsedAt: NOW - 120_000,
  devices: [
    { deviceId: MIXER, name: 'Mixer', address: '192.168.1.71', sessionPort: 49152, protected: false, seenAt: NOW - 125_000 },
    { deviceId: PACKER, name: 'Packer', address: '192.168.1.60', sessionPort: 49152, protected: true, seenAt: NOW - 130_000 },
    { deviceId: PRESS, name: 'Press line 3', address: '192.168.1.50', sessionPort: 49152, protected: false, seenAt: NOW - 135_000 },
  ],
};

function ready(routes: IronLinkRoute[], lastBrowse: BrowseSnapshot | undefined = undefined): IronLinkState {
  return { state: 'ready', routes, lastBrowse };
}

function kinds(entries: PickEntry[]): string[] {
  return entries.map(entry => entry.kind === 'separator' ? `# ${entry.label}` : entry.kind === 'device' ? `${entry.group}:${entry.label}` : entry.kind);
}

suite('parsePathRecord', () => {
  test('parsePathRecord_when_a_record_with_a_hint_then_reads_the_controller', () => {
    assert.deepStrictEqual(parsePathRecord(recordText()), CHOSEN);
  });

  test('parsePathRecord_when_the_hint_is_null_then_the_controller_has_no_hint', () => {
    assert.deepStrictEqual(
      parsePathRecord(recordText({ routeHint: null })),
      { deviceId: PRESS, chosenAt: CHOSEN.chosenAt },
    );
  });

  test('parsePathRecord_when_not_a_record_of_this_version_then_none', () => {
    const wrong = [
      'not json',
      '5',
      'null',
      recordText({ version: 2 }),
      recordText({ deviceId: 'abc' }),
      recordText({ deviceId: PRESS.toUpperCase() }),
      recordText({ chosenAt: 'yesterday' }),
      recordText({ chosenAt: -1 }),
      recordText({ routeHint: 'x' }),
      recordText({ routeHint: { address: '', sessionPort: 49152 } }),
      recordText({ routeHint: { address: '10.0.0.1', sessionPort: 0 } }),
      recordText({ routeHint: { address: '10.0.0.1', sessionPort: 70000 } }),
      JSON.stringify({ version: 1, deviceId: PRESS, chosenAt: 1 }),
    ];
    for (const text of wrong) {
      assert.strictEqual(parsePathRecord(text), undefined, text);
    }
  });
});

suite('path record commands', () => {
  test('recordPathArgs_when_a_controller_then_has_the_anchor_device_id_address_and_port', () => {
    assert.deepStrictEqual(recordPathArgs('/p/app.iplc', CONTROLLER), [
      'record-path', '/p/app.iplc', '--device-id', PRESS, '--address', '192.168.1.50', '--port', '49152',
    ]);
  });

  test('recordPathPathArgs_when_an_anchor_then_asks_the_compiler_where_the_record_is', () => {
    assert.deepStrictEqual(recordPathPathArgs('/p/app.iplc'), ['record-path-path', '/p/app.iplc']);
  });

  test('writeChoice_when_a_controller_then_runs_record_path_once', async () => {
    const calls: string[][] = [];
    await writeChoice(async args => { calls.push(args); return { stdout: '', stderr: '' }; }, '/p/app.iplc', CONTROLLER);
    assert.deepStrictEqual(calls, [recordPathArgs('/p/app.iplc', CONTROLLER)]);
  });

  test('writeChoice_when_the_compiler_fails_then_the_failure_reaches_the_caller', async () => {
    await assert.rejects(
      writeChoice(async () => { throw new Error('bad port'); }, '/p/app.iplc', CONTROLLER),
      /bad port/,
    );
  });
});

suite('readChosen', () => {
  const run = async (args: string[]) => {
    assert.deepStrictEqual(args, recordPathPathArgs('/p/app.iplc'));
    return { stdout: '/p/app.path.iplcinfo\n', stderr: '' };
  };

  test('readChosen_when_the_file_has_a_record_then_returns_the_controller', async () => {
    let read = '';
    const chosen = await readChosen(run, async file => { read = file; return recordText(); }, '/p/app.iplc');
    assert.strictEqual(read, '/p/app.path.iplcinfo');
    assert.deepStrictEqual(chosen, CHOSEN);
  });

  test('readChosen_when_there_is_no_file_then_none', async () => {
    assert.strictEqual(await readChosen(run, async () => undefined, '/p/app.iplc'), undefined);
  });

  test('readChosen_when_the_file_is_not_a_record_then_none', async () => {
    assert.strictEqual(await readChosen(run, async () => '{}', '/p/app.iplc'), undefined);
  });

  test('readChosen_when_the_compiler_prints_no_path_then_fails', async () => {
    await assert.rejects(
      readChosen(async () => ({ stdout: '\n', stderr: '' }), async () => undefined, '/p/app.iplc'),
      /did not say where/,
    );
  });

  test('readChosen_when_the_read_fails_then_the_failure_reaches_the_caller', async () => {
    await assert.rejects(readChosen(run, async () => { throw new Error('denied'); }, '/p/app.iplc'), /denied/);
  });
});

suite('applicationAnchor', () => {
  test('applicationAnchor_when_a_folder_is_open_then_the_container_the_download_record_sits_beside', () => {
    const anchor = applicationAnchor([{ name: 'plant', path: path.join('/w', 'plant') }, { name: 'other', path: '/w/other' }], ['.st']);
    assert.deepStrictEqual(anchor, { anchor: path.join('/w', 'plant', 'plant.iplc') });
  });

  test('applicationAnchor_when_no_folder_is_open_then_a_reason', () => {
    const anchor = applicationAnchor([], ['.st']);
    assert.ok('reason' in anchor && anchor.reason.includes('no workspace folder'));
  });
});

suite('buildPickEntries', () => {
  test('buildPickEntries_when_all_groups_have_members_then_last_routes_observed_and_search_in_that_order', () => {
    const entries = buildPickEntries({
      chosen: CHOSEN,
      ironlink: ready([route(MIXER, '192.168.1.71')], BROWSE),
      nowMs: NOW,
    });
    assert.deepStrictEqual(kinds(entries), [
      '# Last chosen controller',
      'last:Press line 3',
      '# Controllers with a route on this PC',
      'route:Mixer',
      '# Recently observed',
      'observed:Packer',
      'search',
    ]);
  });

  test('buildPickEntries_when_a_controller_is_in_two_groups_then_it_is_listed_once_in_the_first', () => {
    const entries = buildPickEntries({
      chosen: CHOSEN,
      ironlink: ready([route(PRESS, '192.168.1.50'), route(MIXER, '192.168.1.71')], BROWSE),
      nowMs: NOW,
    });
    const devices = entries.filter(entry => entry.kind === 'device').map(entry => `${entry.group}:${entry.deviceId}`);
    assert.deepStrictEqual(devices, [`last:${PRESS}`, `route:${MIXER}`, `observed:${PACKER}`]);
  });

  test('buildPickEntries_when_a_group_is_empty_then_it_has_no_heading', () => {
    const entries = buildPickEntries({ chosen: undefined, ironlink: ready([], BROWSE), nowMs: NOW });
    assert.deepStrictEqual(kinds(entries), [
      '# Recently observed',
      'observed:Mixer',
      'observed:Packer',
      'observed:Press line 3',
      'search',
    ]);
  });

  test('buildPickEntries_when_nothing_is_known_then_only_the_search', () => {
    const entries = buildPickEntries({ chosen: undefined, ironlink: ready([]), nowMs: NOW });
    assert.deepStrictEqual(entries, [{ kind: 'search', label: SEARCH_LABEL, detail: 'Search the network for controllers' }]);
  });

  test('buildPickEntries_when_a_device_id_was_heard_at_two_addresses_then_a_row_for_each', () => {
    const copy = { ...BROWSE.devices[0], name: 'Mixer (copy)', address: '192.168.1.72' };
    const entries = buildPickEntries({
      chosen: undefined,
      ironlink: ready([], { ...BROWSE, devices: [BROWSE.devices[0], copy] }),
      nowMs: NOW,
    });
    assert.deepStrictEqual(kinds(entries), ['# Recently observed', 'observed:Mixer', 'observed:Mixer (copy)', 'search']);
  });

  test('buildPickEntries_when_observed_then_the_age_of_the_observation_is_shown_and_protected_is_marked', () => {
    const entries = buildPickEntries({ chosen: undefined, ironlink: ready([], BROWSE), nowMs: NOW });
    const packer = entries.find(entry => entry.kind === 'device' && entry.deviceId === PACKER);
    assert.ok(packer?.kind === 'device');
    assert.strictEqual(packer.description, '192.168.1.60:49152');
    assert.strictEqual(packer.detail, 'Observed 2 min ago, protected; device id 0f0e0d0c');
  });

  test('buildPickEntries_when_a_route_then_shows_the_address_and_the_age_of_the_verification', () => {
    const entries = buildPickEntries({ chosen: undefined, ironlink: ready([route(MIXER, 'fe80::1', NOW - 3_600_000)]), nowMs: NOW });
    const mixer = entries.find(entry => entry.kind === 'device');
    assert.ok(mixer?.kind === 'device');
    assert.strictEqual(mixer.label, 'Controller a0a1a2a3');
    assert.strictEqual(mixer.description, '[fe80::1]:49152');
    assert.strictEqual(mixer.detail, 'Route verified 1 h ago; device id a0a1a2a3');
  });

  test('buildPickEntries_when_the_chosen_controller_has_a_route_then_the_route_is_its_target_before_the_hint', () => {
    const entries = buildPickEntries({
      chosen: CHOSEN,
      ironlink: ready([{ ...route(PRESS, '10.9.9.9'), sessionPort: 50000 }]),
      nowMs: NOW,
    });
    const last = entries.find(entry => entry.kind === 'device');
    assert.ok(last?.kind === 'device');
    assert.deepStrictEqual(last.target, { deviceId: PRESS, address: '10.9.9.9', sessionPort: 50000 });
    assert.strictEqual(last.detail, 'Chosen 3 d ago; device id 00112233');
  });

  test('buildPickEntries_when_the_chosen_controller_has_only_a_hint_then_the_hint_is_its_target', () => {
    const entries = buildPickEntries({ chosen: CHOSEN, ironlink: ready([]), nowMs: NOW });
    const last = entries.find(entry => entry.kind === 'device');
    assert.ok(last?.kind === 'device');
    assert.deepStrictEqual(last.target, CONTROLLER);
    assert.strictEqual(last.description, '192.168.1.50:49152');
  });

  test('buildPickEntries_when_the_chosen_controller_has_no_address_then_it_says_to_search', () => {
    const entries = buildPickEntries({
      chosen: { deviceId: PRESS, chosenAt: NOW },
      ironlink: ready([]),
      nowMs: NOW,
    });
    const last = entries.find(entry => entry.kind === 'device');
    assert.ok(last?.kind === 'device');
    assert.strictEqual(last.description, 'no address is known');
    assert.ok('unavailable' in last.target && last.target.unavailable.includes('Search the network'));
  });
});

suite('buildPickEntries without IronLink', () => {
  const reason = 'Install the IronLink extension.';

  test('buildPickEntries_when_ironlink_is_not_available_then_the_last_chosen_and_one_row_that_explains', () => {
    const entries = buildPickEntries({ chosen: CHOSEN, ironlink: { state: 'unavailable', reason }, nowMs: NOW });
    assert.deepStrictEqual(kinds(entries), ['# Last chosen controller', 'last:Controller 00112233', 'ironlinkNeeded']);
    const needed = entries[entries.length - 1];
    assert.ok(needed.kind === 'ironlinkNeeded');
    assert.strictEqual(needed.detail, reason);
  });

  test('buildPickEntries_when_ironlink_is_not_available_and_nothing_was_chosen_then_only_the_explaining_row', () => {
    const entries = buildPickEntries({ chosen: undefined, ironlink: { state: 'unavailable', reason }, nowMs: NOW });
    assert.deepStrictEqual(kinds(entries), ['ironlinkNeeded']);
  });

  test('buildPickEntries_when_ironlink_failed_then_the_row_carries_its_message', () => {
    const entries = buildPickEntries({
      chosen: undefined,
      ironlink: { state: 'failed', message: 'E0016 - IronLink program (ironlink) not found' },
      nowMs: NOW,
    });
    assert.deepStrictEqual(entries, [{
      kind: 'ironlinkFailed',
      label: 'IronLink did not answer',
      detail: 'E0016 - IronLink program (ironlink) not found',
    }]);
  });
});

suite('decidePick', () => {
  const entries = buildPickEntries({
    chosen: undefined,
    ironlink: ready([route(MIXER, '192.168.1.71')], BROWSE),
    nowMs: NOW,
  });

  test('decidePick_when_a_device_with_a_target_then_choose_it', () => {
    const device = entries.find(entry => entry.kind === 'device');
    assert.deepStrictEqual(decidePick(device!), {
      action: 'choose',
      controller: { deviceId: MIXER, address: '192.168.1.71', sessionPort: 49152 },
    });
  });

  test('decidePick_when_a_device_without_a_target_then_explain_why', () => {
    const [, last] = buildPickEntries({
      chosen: { deviceId: PRESS, chosenAt: NOW },
      ironlink: ready([]),
      nowMs: NOW,
    });
    assert.strictEqual(decidePick(last).action, 'explain');
  });

  test('decidePick_when_search_then_search', () => {
    assert.deepStrictEqual(decidePick(entries[entries.length - 1]), { action: 'search' });
  });

  test('decidePick_when_the_ironlink_row_or_a_heading_then_nothing', () => {
    assert.deepStrictEqual(decidePick({ kind: 'ironlinkNeeded', label: 'x', detail: 'y' }), { action: 'nothing' });
    assert.deepStrictEqual(decidePick({ kind: 'ironlinkFailed', label: 'x', detail: 'y' }), { action: 'nothing' });
    assert.deepStrictEqual(decidePick({ kind: 'separator', label: 'x' }), { action: 'nothing' });
  });
});

suite('describeIronLinkFailure', () => {
  test('describeIronLinkFailure_when_the_message_starts_with_an_l_code_then_the_code_and_text', () => {
    assert.deepStrictEqual(describeIronLinkFailure(new Error('L6003 - no answer')), { code: 'L6003', text: 'L6003 - no answer' });
  });

  test('describeIronLinkFailure_when_the_message_starts_with_an_e_code_then_the_code_and_text', () => {
    assert.deepStrictEqual(describeIronLinkFailure(new Error('E0018 - exited')), { code: 'E0018', text: 'E0018 - exited' });
  });

  test('describeIronLinkFailure_when_there_is_no_code_then_none', () => {
    assert.strictEqual(describeIronLinkFailure(new Error('boom')), undefined);
    assert.strictEqual(describeIronLinkFailure('L6003 - not an error'), undefined);
  });
});
