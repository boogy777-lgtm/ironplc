import * as assert from 'assert';
import * as path from 'path';
import { pathForPlatform } from '../platformPath';

suite('pathForPlatform', () => {
  test('pathForPlatform_when_win32_then_joins_with_backslashes', () => {
    assert.strictEqual(pathForPlatform('win32').join('C:/tools', 'ironplcvm.exe'), 'C:\\tools\\ironplcvm.exe');
  });

  test('pathForPlatform_when_linux_then_joins_with_forward_slashes', () => {
    assert.strictEqual(pathForPlatform('linux').join('/opt/ironplc', 'ironplcvm'), '/opt/ironplc/ironplcvm');
  });

  test('pathForPlatform_when_darwin_then_joins_with_forward_slashes', () => {
    assert.strictEqual(pathForPlatform('darwin').join('/opt/homebrew/bin', 'ironplcc'), '/opt/homebrew/bin/ironplcc');
  });

  test('pathForPlatform_when_platform_is_a_pure_module_then_is_independent_of_host', () => {
    assert.strictEqual(pathForPlatform('win32'), path.win32);
    assert.strictEqual(pathForPlatform('linux'), path.posix);
  });
});
