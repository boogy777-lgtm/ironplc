import * as assert from 'assert';
import * as fs from 'fs';
import * as os from 'os';
import * as path from 'path';

import { accessIronLink, IRONLINK_NOT_INSTALLED } from '../../ironlinkAccess';
import { readTextIfPresent } from '../../readText';
import { IRONLINK_API_VERSION, IRONLINK_EXTENSION_ID } from '../../vscodeCommon';

suite('accessIronLink', () => {
  test('accessIronLink_when_not_installed_then_unavailable_with_the_reason_to_install', async () => {
    const asked: string[] = [];
    const access = await accessIronLink(id => { asked.push(id); return undefined; });
    assert.deepStrictEqual(access, { available: false, reason: IRONLINK_NOT_INSTALLED });
    assert.deepStrictEqual(asked, [IRONLINK_EXTENSION_ID]);
  });

  test('accessIronLink_when_installed_then_activates_it_and_returns_its_exports', async () => {
    const api = { apiVersion: IRONLINK_API_VERSION };
    const access = await accessIronLink(() => ({ activate: async () => api }));
    assert.ok(access.available);
    assert.strictEqual(access.api, api as unknown);
  });

  test('accessIronLink_when_activation_fails_then_unavailable_with_the_failure', async () => {
    const access = await accessIronLink(() => ({ activate: async () => { throw new Error('boom'); } }));
    assert.deepStrictEqual(access, { available: false, reason: 'The IronLink extension did not start: boom' });
  });

  test('accessIronLink_when_the_exports_are_of_another_version_then_unavailable_with_an_update_hint', async () => {
    for (const exported of [undefined, null, {}, { apiVersion: IRONLINK_API_VERSION + 1 }, 'x']) {
      const access = await accessIronLink(() => ({ activate: async () => exported }));
      assert.ok(!access.available, String(exported));
      assert.ok(access.reason.includes(`version ${IRONLINK_API_VERSION}`), access.reason);
    }
  });
});

suite('readTextIfPresent', () => {
  test('readTextIfPresent_when_the_file_exists_then_its_text', async () => {
    const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'ironplc-readtext-'));
    try {
      const file = path.join(dir, 'a.txt');
      fs.writeFileSync(file, 'hello');
      assert.strictEqual(await readTextIfPresent(file), 'hello');
    }
    finally {
      fs.rmSync(dir, { recursive: true, force: true });
    }
  });

  test('readTextIfPresent_when_there_is_no_file_then_undefined', async () => {
    assert.strictEqual(await readTextIfPresent(path.join(os.tmpdir(), 'ironplc-no-such-dir', 'a.txt')), undefined);
  });

  test('readTextIfPresent_when_the_path_is_a_directory_then_the_failure_reaches_the_caller', async () => {
    await assert.rejects(readTextIfPresent(os.tmpdir()));
  });
});
