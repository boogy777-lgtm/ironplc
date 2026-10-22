import * as assert from 'assert';

import { ChosenController } from '../../chooseControllerLogic';
import { CONNECTION_METHODS } from '../../connectionMethods';
import {
  connectingText,
  controllerProfile,
  identityNotes,
  resolveChosenRoute,
  ROUTE_SOURCE_TEXT,
} from '../../connectControllerLogic';
import { validateProfiles } from '../../connectionProfiles';
import { DEFAULT_SESSION_PORT } from '../../sessionConstants';
import { IronLinkRoute } from '../../vscodeCommon';

const PRESS = '00112233445566778899aabbccddeeff';
const MIXER = 'a0a1a2a3a4a5a6a7a8a9aaabacadaeaf';

const CHOSEN: ChosenController = {
  deviceId: PRESS,
  routeHint: { address: '192.168.1.50', sessionPort: 49152 },
  chosenAt: 1,
};

const WORKSTATION_ROUTE: IronLinkRoute = {
  deviceId: PRESS,
  address: '10.9.9.9',
  sessionPort: 50000,
  driver: 'addressList',
  verifiedAt: 2,
};

suite('resolveChosenRoute', () => {
  test('resolveChosenRoute_when_the_workstation_has_a_route_then_it_wins_over_the_hint', () => {
    assert.deepStrictEqual(resolveChosenRoute(CHOSEN, [WORKSTATION_ROUTE]), {
      route: { deviceId: PRESS, address: '10.9.9.9', sessionPort: 50000 },
      source: 'workstation',
    });
  });

  test('resolveChosenRoute_when_the_workstation_route_is_for_another_device_then_the_hint_is_used', () => {
    assert.deepStrictEqual(resolveChosenRoute(CHOSEN, [{ ...WORKSTATION_ROUTE, deviceId: MIXER }]), {
      route: { deviceId: PRESS, address: '192.168.1.50', sessionPort: 49152 },
      source: 'project',
    });
  });

  test('resolveChosenRoute_when_ironlink_is_not_present_then_the_hint_is_used', () => {
    assert.deepStrictEqual(resolveChosenRoute(CHOSEN, undefined), {
      route: { deviceId: PRESS, address: '192.168.1.50', sessionPort: 49152 },
      source: 'project',
    });
  });

  test('resolveChosenRoute_when_ironlink_has_no_routes_then_the_hint_is_used', () => {
    assert.strictEqual('route' in resolveChosenRoute(CHOSEN, []), true);
    assert.strictEqual((resolveChosenRoute(CHOSEN, []) as { source: string }).source, 'project');
  });

  test('resolveChosenRoute_when_no_route_and_no_hint_then_a_reason', () => {
    const resolved = resolveChosenRoute({ deviceId: PRESS, chosenAt: 1 }, []);
    assert.ok('reason' in resolved);
    assert.ok(resolved.reason.includes('00112233'));
    assert.ok(!resolved.reason.includes('not available'));
  });

  test('resolveChosenRoute_when_no_hint_and_no_ironlink_then_the_reason_says_so', () => {
    const resolved = resolveChosenRoute({ deviceId: PRESS, chosenAt: 1 }, undefined);
    assert.ok('reason' in resolved);
    assert.ok(resolved.reason.includes('IronLink extension is not available'));
  });

  test('resolveChosenRoute_when_nothing_is_chosen_then_a_reason', () => {
    assert.deepStrictEqual(resolveChosenRoute(undefined, [WORKSTATION_ROUTE]), {
      reason: 'No controller is chosen for this project.',
    });
  });

  test('ROUTE_SOURCE_TEXT_when_each_source_then_has_words', () => {
    assert.strictEqual(ROUTE_SOURCE_TEXT.workstation, 'the route stored on this PC');
    assert.strictEqual(ROUTE_SOURCE_TEXT.project, 'the route hint of the project');
  });
});

suite('controllerProfile', () => {
  const controller = { deviceId: PRESS, address: '192.168.1.50', sessionPort: 49152 };

  test('controllerProfile_when_named_then_a_tcp_profile_with_the_route', () => {
    assert.deepStrictEqual(controllerProfile(controller, 'Press line 3'), {
      name: 'Press line 3',
      transport: 'tcp',
      address: '192.168.1.50',
      port: 49152,
    });
  });

  test('controllerProfile_when_no_name_or_a_blank_one_then_the_short_device_id', () => {
    assert.strictEqual(controllerProfile(controller).name, '00112233');
    assert.strictEqual(controllerProfile(controller, '  ').name, '00112233');
  });

  test('controllerProfile_when_validated_then_passes_the_tcp_member_and_the_profile_validation', () => {
    const profile = controllerProfile(controller, 'Press line 3');
    assert.strictEqual(CONNECTION_METHODS.tcp.checkFields(profile), undefined);
    const { profiles, errors } = validateProfiles([profile]);
    assert.deepStrictEqual(errors, []);
    assert.strictEqual(profiles.length, 1);
    assert.strictEqual(profiles[0].transport, 'tcp');
    assert.strictEqual(profiles[0].port, 49152);
  });

  test('controllerProfile_when_a_host_name_or_ipv6_or_the_default_port_then_passes_the_validation', () => {
    for (const address of ['plc-3.example.com', 'fe80::1']) {
      const profile = controllerProfile({ ...controller, address, sessionPort: DEFAULT_SESSION_PORT });
      assert.deepStrictEqual(validateProfiles([profile]).errors, [], address);
    }
  });

  test('controllerProfile_when_a_device_name_is_long_or_has_spaces_then_still_valid', () => {
    const profile = controllerProfile(controller, 'Press line 3 (west hall, cell 2)');
    assert.deepStrictEqual(validateProfiles([profile]).errors, []);
  });
});

suite('identityNotes', () => {
  test('identityNotes_when_the_connected_is_the_intended_and_the_chosen_then_no_lines', () => {
    assert.deepStrictEqual(identityNotes({ connected: PRESS, intended: PRESS, chosen: PRESS }), []);
  });

  test('identityNotes_when_a_saved_profile_and_nothing_chosen_then_no_lines', () => {
    assert.deepStrictEqual(identityNotes({ connected: PRESS, intended: undefined, chosen: undefined }), []);
  });

  test('identityNotes_when_another_controller_answers_at_the_route_then_a_line_names_both', () => {
    assert.deepStrictEqual(identityNotes({ connected: MIXER, intended: PRESS, chosen: PRESS }), [
      'The controller that answered (a0a1a2a3) is not the one expected at this address (00112233).',
      'The connected controller (a0a1a2a3) is not the controller chosen for this project (00112233).',
    ]);
  });

  test('identityNotes_when_the_intended_is_connected_but_is_not_the_chosen_then_one_line_about_the_chosen', () => {
    assert.deepStrictEqual(identityNotes({ connected: MIXER, intended: MIXER, chosen: PRESS }), [
      'The connected controller (a0a1a2a3) is not the controller chosen for this project (00112233).',
    ]);
  });

  test('identityNotes_when_a_saved_profile_reaches_another_than_the_chosen_then_one_line', () => {
    assert.deepStrictEqual(identityNotes({ connected: MIXER, intended: undefined, chosen: PRESS }), [
      'The connected controller (a0a1a2a3) is not the controller chosen for this project (00112233).',
    ]);
  });

  test('identityNotes_when_the_controller_reports_no_device_id_then_no_lines', () => {
    assert.deepStrictEqual(identityNotes({ connected: undefined, intended: PRESS, chosen: PRESS }), []);
  });
});

suite('connectingText', () => {
  test('connectingText_when_a_profile_then_names_it_the_address_and_the_source', () => {
    assert.strictEqual(
      connectingText(controllerProfile({ deviceId: PRESS, address: '192.168.1.50', sessionPort: 49152 }, 'Press'), 'the route stored on this PC'),
      'Connecting to Press at 192.168.1.50:49152 by the route stored on this PC.',
    );
  });
});
