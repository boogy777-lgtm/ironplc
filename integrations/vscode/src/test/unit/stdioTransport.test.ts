import * as assert from 'assert';
import { vmPathBeside } from '../../stdioTransport';

suite('vmPathBeside', () => {
  test('vmPathBeside_when_win32_then_uses_backslashes_and_exe', () => {
    assert.strictEqual(
      vmPathBeside('C:\\Tools\\IronPLC\\ironplcc.exe', 'win32'),
      'C:\\Tools\\IronPLC\\ironplcvm.exe',
    );
  });

  test('vmPathBeside_when_linux_then_uses_forward_slashes_and_no_extension', () => {
    assert.strictEqual(vmPathBeside('/opt/ironplc/ironplcc', 'linux'), '/opt/ironplc/ironplcvm');
  });
});
