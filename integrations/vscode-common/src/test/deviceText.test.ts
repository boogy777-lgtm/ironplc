import * as assert from 'assert';
import { formatAge, shortDeviceId, socketAddress } from '../deviceText';

suite('device text', () => {
  test('formatAge_when_seconds_minutes_hours_days_then_the_unit_changes', () => {
    const now = 10_000_000_000;
    assert.strictEqual(formatAge(now, now), 'just now');
    assert.strictEqual(formatAge(now - 12_000, now), '12 s ago');
    assert.strictEqual(formatAge(now - 3 * 60_000, now), '3 min ago');
    assert.strictEqual(formatAge(now - 2 * 3_600_000, now), '2 h ago');
    assert.strictEqual(formatAge(now - 4 * 86_400_000, now), '4 d ago');
  });

  test('formatAge_when_seen_in_the_future_then_just_now', () => {
    assert.strictEqual(formatAge(2000, 1000), 'just now');
  });

  test('socketAddress_when_ipv6_then_brackets_the_address', () => {
    assert.strictEqual(socketAddress('192.168.1.5', 49152), '192.168.1.5:49152');
    assert.strictEqual(socketAddress('fe80::1', 49152), '[fe80::1]:49152');
  });

  test('shortDeviceId_when_a_full_id_then_the_first_eight_digits', () => {
    assert.strictEqual(shortDeviceId('00112233445566778899aabbccddeeff'), '00112233');
  });
});
