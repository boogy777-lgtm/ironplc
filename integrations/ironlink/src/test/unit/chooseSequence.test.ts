import * as assert from 'assert';

import { chooseDevice } from '../../chooseSequence';
import { DeviceRowFacts } from '../../networkTree';
import { ChooseControllerArgument, RouteOutcome } from '../../vscodeCommon';
import { PRESS_LINE_ID } from './browseFixture';

const ROW: DeviceRowFacts = {
  deviceId: PRESS_LINE_ID,
  name: 'Press line 3',
  address: '192.168.1.50',
  sessionPort: 49152,
  conflict: false,
  protected: false,
  routed: false,
};

const ROUTE_OK: RouteOutcome = {
  ok: true,
  route: { deviceId: PRESS_LINE_ID, address: '192.168.1.50', sessionPort: 49152, driver: 'addressList', verifiedAt: 1 },
  replaced: false,
};

// REQ-IL-ironlink-vscode-100
suite('chooseDevice', () => {
  test('chooseDevice_when_the_route_is_set_then_the_route_comes_first_and_the_handoff_second_with_the_row', async () => {
    const steps: string[] = [];
    const outcome = await chooseDevice(
      ROW,
      async (deviceId, address) => { steps.push(`route ${deviceId} ${address}`); return ROUTE_OK; },
      async (argument: ChooseControllerArgument) => { steps.push(`handoff ${JSON.stringify(argument)}`); },
    );
    assert.deepStrictEqual(steps, [
      `route ${PRESS_LINE_ID} 192.168.1.50:49152`,
      `handoff ${JSON.stringify({ deviceId: PRESS_LINE_ID, address: '192.168.1.50', sessionPort: 49152 })}`,
    ]);
    assert.deepStrictEqual(outcome, { handedOff: true, route: ROUTE_OK });
  });

  test('chooseDevice_when_the_route_fails_then_nothing_is_handed_off_and_the_failure_is_returned', async () => {
    const failure = { ok: false as const, code: 'L6003', message: 'L6003 - no answer' };
    let handedOff = 0;
    const outcome = await chooseDevice(ROW, async () => failure, async () => { handedOff++; });
    assert.strictEqual(handedOff, 0);
    assert.deepStrictEqual(outcome, { handedOff: false, failure });
  });

  test('chooseDevice_when_an_ipv6_address_then_the_route_address_is_bracketed', async () => {
    let asked = '';
    await chooseDevice({ ...ROW, address: 'fe80::1' }, async (_id, address) => { asked = address; return ROUTE_OK; }, async () => {});
    assert.strictEqual(asked, '[fe80::1]:49152');
  });

  test('chooseDevice_when_the_handoff_fails_then_the_failure_reaches_the_caller', async () => {
    await assert.rejects(
      chooseDevice(ROW, async () => ROUTE_OK, async () => { throw new Error('boom'); }),
      /boom/,
    );
  });
});
