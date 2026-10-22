import * as assert from 'assert';

import { ApiClient, ApiDependencies, BrowseMemory, createIronLinkApi, devicesHeard } from '../../api';
import { IronLinkCommandError, IronLinkProblem } from '../../failures';
import { BrowseOptions, BrowseResponse, RouteListResponse, RouteSetResponse } from '../../ironlinkProtocol';
import { IRONLINK_API_MEMBERS, IRONLINK_API_VERSION, IronLinkApi, ProblemCode } from '../../vscodeCommon';
import { MIXER_ID, PACKER_ID, PRESS_LINE_ID, SILENT_ID, browseFixture } from './browseFixture';

const NOW = 1761136020000;

/** A fake program that records what the API asked and answers what it was given. */
class FakeClient implements ApiClient {
  readonly calls: string[] = [];
  routeSetError: unknown;
  routes: RouteListResponse = { response: 'routeList', routes: [] };

  async browse(options: BrowseOptions = {}): Promise<BrowseResponse> {
    this.calls.push(`browse ${JSON.stringify(options)}`);
    return browseFixture();
  }

  async routeSet(deviceId: string, address: string, options: BrowseOptions = {}): Promise<RouteSetResponse> {
    this.calls.push(`routeSet ${deviceId} ${address} ${JSON.stringify(options)}`);
    if (this.routeSetError !== undefined) {
      throw this.routeSetError;
    }
    return {
      response: 'routeSet',
      deviceId,
      route: { address: '192.168.1.50', driver: 'addressList', sessionPort: 49152, verifiedAt: NOW - 5 },
      replaced: true,
    };
  }

  async routeList(): Promise<RouteListResponse> {
    this.calls.push('routeList');
    return this.routes;
  }
}

function build(client = new FakeClient(), overrides: Partial<ApiDependencies> = {}) {
  const memory = new BrowseMemory();
  const shown: BrowseResponse[] = [];
  let revealed = 0;
  const api = createIronLinkApi({
    client,
    memory,
    now: () => NOW,
    browseOptions: () => ({ attempts: 2 }),
    show: response => shown.push(response),
    revealNetwork: async () => { revealed++; },
    ...overrides,
  });
  return { api, client, memory, shown, revealed: () => revealed };
}

// REQ-IL-ironlink-vscode-096
suite('IronLinkApi conformance', () => {
  test('createIronLinkApi_when_built_then_has_exactly_the_members_of_the_shared_type', () => {
    const { api } = build();
    const typed: IronLinkApi = api;
    assert.deepStrictEqual(Object.keys(typed).sort(), Object.keys(IRONLINK_API_MEMBERS).sort());
  });

  test('createIronLinkApi_when_built_then_every_function_member_is_a_function', () => {
    const { api } = build();
    for (const member of Object.keys(IRONLINK_API_MEMBERS) as Array<keyof IronLinkApi>) {
      assert.strictEqual(typeof api[member], member === 'apiVersion' ? 'number' : 'function', member);
    }
  });

  test('createIronLinkApi_when_built_then_reports_the_api_version_of_the_shared_package', () => {
    assert.strictEqual(build().api.apiVersion, IRONLINK_API_VERSION);
  });
});

// REQ-IL-ironlink-vscode-097
suite('IronLinkApi reads', () => {
  test('routeList_when_the_program_has_routes_then_returns_them_in_the_shared_shape', async () => {
    const client = new FakeClient();
    client.routes = {
      response: 'routeList',
      routes: [{ deviceId: SILENT_ID, address: '10.0.4.7', driver: 'addressList', sessionPort: 49152, verifiedAt: 7 }],
    };
    assert.deepStrictEqual(await build(client).api.routeList(), [
      { deviceId: SILENT_ID, address: '10.0.4.7', sessionPort: 49152, driver: 'addressList', verifiedAt: 7 },
    ]);
  });

  test('lastBrowse_when_nothing_was_browsed_then_undefined', () => {
    assert.strictEqual(build().api.lastBrowse(), undefined);
  });

  test('browse_when_called_then_sends_the_set_options_shows_the_result_and_remembers_it_with_its_time', async () => {
    const { api, client, shown, memory } = build();
    const snapshot = await api.browse();
    assert.deepStrictEqual(client.calls, ['browse {"attempts":2}']);
    assert.strictEqual(shown.length, 1);
    assert.strictEqual(memory.last()?.browsedAt, NOW);
    assert.strictEqual(snapshot.browsedAt, NOW);
    assert.deepStrictEqual(api.lastBrowse(), snapshot);
  });

  test('devicesHeard_when_fixture_then_one_entry_for_each_address_newest_first', () => {
    const devices = devicesHeard(browseFixture());
    assert.deepStrictEqual(
      devices.map(device => [device.name, device.address, device.seenAt]),
      [
        ['Press line 3', '192.168.1.50', 1761136009900],
        ['Packer', '192.168.1.60', 1761136009000],
        ['Mixer', '192.168.1.71', 1761136008000],
        ['Mixer (copy)', '192.168.1.72', 1761136007500],
      ],
    );
    assert.deepStrictEqual(devices.map(device => device.deviceId), [PRESS_LINE_ID, PACKER_ID, MIXER_ID, MIXER_ID]);
  });

  test('devicesHeard_when_a_device_was_heard_by_two_drivers_then_keeps_the_later_observation', () => {
    const press = devicesHeard(browseFixture()).filter(device => device.deviceId === PRESS_LINE_ID);
    assert.strictEqual(press.length, 1);
    assert.strictEqual(press[0].seenAt, 1761136009900);
  });

  test('devicesHeard_when_a_device_is_protected_then_says_so', () => {
    const packer = devicesHeard(browseFixture()).find(device => device.deviceId === PACKER_ID);
    assert.strictEqual(packer?.protected, true);
  });

  test('devicesHeard_when_nothing_was_heard_then_empty', () => {
    const response = browseFixture();
    response.drivers.forEach(driver => { driver.devices = []; });
    assert.deepStrictEqual(devicesHeard(response), []);
  });
});

// REQ-IL-ironlink-vscode-098
suite('IronLinkApi setRoute', () => {
  test('setRoute_when_the_program_verifies_then_returns_the_route_and_sends_the_options', async () => {
    const { api, client } = build();
    const outcome = await api.setRoute(PRESS_LINE_ID, '192.168.1.50:49152');
    assert.deepStrictEqual(outcome, {
      ok: true,
      route: { deviceId: PRESS_LINE_ID, address: '192.168.1.50', sessionPort: 49152, driver: 'addressList', verifiedAt: NOW - 5 },
      replaced: true,
    });
    assert.deepStrictEqual(client.calls, [`routeSet ${PRESS_LINE_ID} 192.168.1.50:49152 {"attempts":2}`]);
  });

  test('setRoute_when_the_program_refuses_then_returns_the_l_code_and_text_and_does_not_throw', async () => {
    const client = new FakeClient();
    client.routeSetError = new IronLinkCommandError('L6008', 'another device answered');
    assert.deepStrictEqual(await build(client).api.setRoute(PRESS_LINE_ID, '192.168.1.50'), {
      ok: false,
      code: 'L6008',
      message: 'L6008 - another device answered',
    });
  });

  test('setRoute_when_the_program_cannot_run_then_returns_the_e_code', async () => {
    const client = new FakeClient();
    client.routeSetError = new IronLinkProblem(ProblemCode.IronLinkNotFound);
    const outcome = await build(client).api.setRoute(PRESS_LINE_ID, '192.168.1.50');
    assert.strictEqual(outcome.ok, false);
    if (!outcome.ok) {
      assert.strictEqual(outcome.code, 'E0016');
      assert.ok(outcome.message.startsWith('E0016 - '), outcome.message);
    }
  });

  test('setRoute_when_a_defect_of_the_extension_then_rethrows_it', async () => {
    const client = new FakeClient();
    client.routeSetError = new TypeError('defect');
    await assert.rejects(build(client).api.setRoute(PRESS_LINE_ID, '192.168.1.50'), /defect/);
  });
});

// REQ-IL-ironlink-vscode-099
suite('IronLinkApi revealNetwork', () => {
  test('revealNetwork_when_called_then_asks_the_editor_to_focus_the_view_once', async () => {
    const { api, revealed } = build();
    await api.revealNetwork();
    assert.strictEqual(revealed(), 1);
  });
});
