import * as assert from 'assert';
import { createHash } from 'crypto';

import {
  BaselineSnapshot,
  captureBaseline,
  checkBaseline,
  hashContainer,
} from '../../baselineLogic';
import { HotEditStatus } from '../../hotEditSession';

function status(overrides?: Partial<HotEditStatus>): HotEditStatus {
  return {
    mode: 'normal',
    active: 3,
    normal: 2,
    candidate: null,
    application: 3,
    migration: false,
    rounds: 10,
    ...overrides,
  };
}

suite('hashContainer', () => {
  test('hashContainer_when_bytes_then_sha256_hex', () => {
    const bytes = new Uint8Array([1, 2, 255]);
    const expected = createHash('sha256').update(bytes).digest('hex');
    assert.strictEqual(hashContainer(bytes), expected);
    assert.strictEqual(hashContainer(bytes).length, 64);
  });
});

suite('captureBaseline', () => {
  test('captureBaseline_when_assemble_acknowledged_then_counters_and_hash', () => {
    const bytes = new Uint8Array([9, 8, 7]);
    const snapshot = captureBaseline(status(), bytes);
    assert.strictEqual(snapshot.active, 3);
    assert.strictEqual(snapshot.application, 3);
    assert.strictEqual(snapshot.containerHash, hashContainer(bytes));
  });
});

suite('checkBaseline', () => {
  const BASELINE: BaselineSnapshot = { active: 3, application: 3, containerHash: 'abc' };

  test('checkBaseline_when_no_baseline_then_none', () => {
    assert.strictEqual(checkBaseline(undefined, status()), 'none');
  });

  test('checkBaseline_when_counters_equal_then_match', () => {
    assert.strictEqual(checkBaseline(BASELINE, status()), 'match');
  });

  test('checkBaseline_when_active_counter_differs_then_mismatch', () => {
    assert.strictEqual(checkBaseline(BASELINE, status({ active: 4 })), 'mismatch');
  });

  test('checkBaseline_when_application_counter_differs_then_mismatch', () => {
    assert.strictEqual(checkBaseline(BASELINE, status({ application: 4 })), 'mismatch');
  });

  test('checkBaseline_when_device_rebooted_and_counters_restarted_then_mismatch', () => {
    // A reboot restarts the session generations: anything other than the
    // captured counters fails closed, even counter values the device has
    // reached before.
    assert.strictEqual(checkBaseline(BASELINE, status({ active: 1, application: 1 })), 'mismatch');
  });
});
