import * as assert from 'assert';

import { IdentifyResponse } from '../../ironlinkProtocol';
import {
  describeAddressAdd,
  describeAddressRemove,
  describeBrowse,
  describeIdentify,
} from '../../messages';
import { PRESS_LINE_ID, browseFixture } from './browseFixture';

const at = (ms: number) => `T${ms}`;

const DEVICE_REPLY: IdentifyResponse = {
  response: 'identify',
  address: '192.168.1.50:49152',
  timeoutMs: 1000,
  answer: 'device',
  observation: {
    deviceId: PRESS_LINE_ID,
    name: 'Press line 3',
    model: 'IronPLC SoftPLC',
    runtimeVersion: '0.247.0',
    protocol: 1,
    sessionPort: 49152,
    protected: true,
    route: { driver: 'addressList', address: '192.168.1.50', sessionPort: 49152, method: 'tcp' },
    seenAt: 1761136000123,
  },
};

// REQ-IL-ironlink-vscode-094
suite('describeIdentify', () => {
  test('describeIdentify_when_a_device_answered_then_shows_the_reply_and_its_time', () => {
    const notice = describeIdentify(DEVICE_REPLY, at);
    assert.strictEqual(notice.severity, 'information');
    assert.strictEqual(
      notice.text,
      'Press line 3 (IronPLC SoftPLC, runtime 0.247.0, id 00112233, credentials required) '
      + 'answered from 192.168.1.50:49152 at T1761136000123.',
    );
  });

  test('describeIdentify_when_nothing_answered_then_a_warning_and_not_an_error', () => {
    const notice = describeIdentify(
      { response: 'identify', address: '10.0.4.7:49152', timeoutMs: 1000, answer: 'none' },
      at,
    );
    assert.strictEqual(notice.severity, 'warning');
    assert.strictEqual(notice.text, 'No answer from 10.0.4.7:49152 within 1000 ms.');
  });

  test('describeIdentify_when_another_wire_version_answered_then_says_to_update_ironlink', () => {
    const notice = describeIdentify(
      {
        response: 'identify',
        address: '10.0.4.7:49152',
        timeoutMs: 1000,
        answer: 'otherVersion',
        otherVersion: { address: '10.0.4.7', wireVersion: 2, seenAt: 5 },
      },
      at,
    );
    assert.strictEqual(notice.severity, 'warning');
    assert.ok(notice.text.includes('discovery version 2'));
    assert.ok(notice.text.includes('T5'));
    assert.ok(notice.text.endsWith('Update IronLink.'));
  });
});

// REQ-IL-ironlink-vscode-090
suite('address change messages', () => {
  test('describeAddressAdd_when_added_or_present_then_says_which', () => {
    assert.strictEqual(
      describeAddressAdd({ response: 'addressAdd', address: '10.0.4.7:49152', added: true }),
      'Added 10.0.4.7:49152 to the address list.',
    );
    assert.strictEqual(
      describeAddressAdd({ response: 'addressAdd', address: '10.0.4.7:49152', added: false }),
      '10.0.4.7:49152 is in the address list already.',
    );
  });

  test('describeAddressRemove_when_removed_or_absent_then_says_which', () => {
    assert.strictEqual(
      describeAddressRemove({ response: 'addressRemove', address: '10.0.4.7:49152', removed: true }),
      'Removed 10.0.4.7:49152 from the address list.',
    );
    assert.strictEqual(
      describeAddressRemove({ response: 'addressRemove', address: '10.0.4.7:49152', removed: false }),
      '10.0.4.7:49152 was not in the address list.',
    );
  });
});

suite('describeBrowse', () => {
  test('describeBrowse_when_fixture_then_counts_routes_and_unseen_routes', () => {
    assert.strictEqual(
      describeBrowse(browseFixture()),
      'Search found 5 routes (broadcast, addressList); 1 stored route not seen.',
    );
  });
});
