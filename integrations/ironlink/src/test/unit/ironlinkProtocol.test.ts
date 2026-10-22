import * as assert from 'assert';

import { encodeCommand, parseResponseLine } from '../../ironlinkProtocol';
import { browseOptionsFromSettings } from '../../settings';
import { BROWSE_LINE } from './browseFixture';

// REQ-IL-ironlink-vscode-081
suite('encodeCommand', () => {
  test('encodeCommand_when_command_then_one_line_without_a_newline', () => {
    const line = encodeCommand({ command: 'addressAdd', address: '10.0.4.7:49152' });
    assert.strictEqual(line, '{"command":"addressAdd","address":"10.0.4.7:49152"}');
    assert.ok(!line.includes('\n'));
  });

  test('encodeCommand_when_the_address_has_special_characters_then_the_line_stays_one_line', () => {
    const line = encodeCommand({ command: 'addressAdd', address: 'a"b\nc' });
    assert.ok(!line.includes('\n'));
    assert.strictEqual(JSON.parse(line).address, 'a"b\nc');
  });
});

suite('parseResponseLine', () => {
  test('parseResponseLine_when_browse_line_then_has_the_tag_and_the_tree', () => {
    const parsed = parseResponseLine(BROWSE_LINE);
    assert.strictEqual(parsed.response, 'browse');
  });

  test('parseResponseLine_when_not_json_then_throws_naming_the_problem', () => {
    assert.throws(() => parseResponseLine('hello'), /not JSON/);
  });

  test('parseResponseLine_when_json_without_a_response_tag_then_throws', () => {
    assert.throws(() => parseResponseLine('{"drivers":[]}'), /"response" tag/);
    assert.throws(() => parseResponseLine('[1,2]'), /"response" tag/);
    assert.throws(() => parseResponseLine('null'), /"response" tag/);
  });
});

// REQ-IL-ironlink-vscode-089
suite('browseOptionsFromSettings', () => {
  test('browseOptionsFromSettings_when_both_unset_then_no_options', () => {
    assert.deepStrictEqual(browseOptionsFromSettings(null, undefined), {});
  });

  test('browseOptionsFromSettings_when_set_then_passes_the_numbers_through_unchecked', () => {
    assert.deepStrictEqual(browseOptionsFromSettings(2500, 3), { timeoutMs: 2500, attempts: 3 });
    // The range belongs to the program (L6004), not to the extension.
    assert.deepStrictEqual(browseOptionsFromSettings(999999, 0), { timeoutMs: 999999, attempts: 0 });
  });

  test('browseOptionsFromSettings_when_not_a_number_then_ignored', () => {
    assert.deepStrictEqual(browseOptionsFromSettings('1000', {}), {});
  });
});
