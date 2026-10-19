import * as assert from 'assert';
import * as fs from 'fs';
import * as path from 'path';
import { serverInitializationOptions, stringList } from '../../serverOptionsLogic';

suite('serverOptionsLogic', () => {
  test('serverInitializationOptions_when_lists_set_then_server_keys_carry_them', () => {
    const options = serverInitializationOptions({
      dialect: 'codesys',
      disabledWarnings: ['P4083'],
      warningsAsErrors: ['C0354'],
    });

    assert.deepStrictEqual(options, {
      dialect: 'codesys',
      disabledWarnings: ['P4083'],
      warningsAsErrors: ['C0354'],
    });
  });

  test('serverInitializationOptions_when_lists_unset_then_empty_lists', () => {
    const options = serverInitializationOptions({
      dialect: 'iec61131-3-ed2',
      disabledWarnings: undefined,
      warningsAsErrors: null,
    });

    assert.deepStrictEqual(options.disabledWarnings, []);
    assert.deepStrictEqual(options.warningsAsErrors, []);
  });

  test('stringList_when_entries_are_not_strings_then_they_are_dropped_and_order_kept', () => {
    assert.deepStrictEqual(stringList(['P4083', 7, null, 'C0354', { code: 'P4083' }]), [
      'P4083',
      'C0354',
    ]);
  });

  test('stringList_when_value_is_not_an_array_then_empty', () => {
    assert.deepStrictEqual(stringList('P4083'), []);
  });

  test('stringList_when_code_has_other_case_then_kept_as_written', () => {
    // Case is exact in the compiler; the extension does not normalise it.
    assert.deepStrictEqual(stringList(['p4083']), ['p4083']);
  });

  test('settings_when_declared_in_package_json_then_arrays_of_strings_without_default_entries', () => {
    const manifest = JSON.parse(
      fs.readFileSync(path.join(__dirname, '..', '..', '..', 'package.json'), 'utf8')
    );
    const properties = manifest.contributes.configuration.properties;

    for (const key of ['ironplc.disabledWarnings', 'ironplc.warningsAsErrors']) {
      assert.strictEqual(properties[key].type, 'array', key);
      assert.strictEqual(properties[key].items.type, 'string', key);
      assert.deepStrictEqual(properties[key].default, [], key);
    }
  });
});
