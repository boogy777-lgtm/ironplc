import * as assert from 'assert';
import { IRONLINK_API_MEMBERS, IRONLINK_API_VERSION, IRONLINK_EXTENSION_ID } from '../ironlinkApi';

suite('IronLink API declaration', () => {
  test('IRONLINK_API_MEMBERS_when_listed_then_names_every_member_the_consumers_use', () => {
    assert.deepStrictEqual(
      Object.keys(IRONLINK_API_MEMBERS).sort(),
      ['apiVersion', 'browse', 'lastBrowse', 'revealNetwork', 'routeList', 'setRoute'],
    );
  });

  test('IRONLINK_API_VERSION_when_read_then_is_a_positive_integer', () => {
    assert.ok(Number.isInteger(IRONLINK_API_VERSION) && IRONLINK_API_VERSION >= 1);
  });

  test('IRONLINK_EXTENSION_ID_when_read_then_is_the_single_id_of_the_extension', () => {
    assert.strictEqual(IRONLINK_EXTENSION_ID, 'ironplc.ironlink');
  });
});
