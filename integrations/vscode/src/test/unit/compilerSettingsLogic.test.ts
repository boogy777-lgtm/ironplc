import * as assert from 'assert';
import * as fs from 'fs';
import * as path from 'path';
import {
  COMPILER_SETTINGS,
  CompilerSettings,
  SETTING_NAMES,
  compilerFlags,
  readCompilerSettings,
  serverInitializationOptions,
  stringList,
} from '../../compilerSettingsLogic';

const EMPTY: CompilerSettings = { disabledWarnings: [], warningsAsErrors: [] };

/** The text of a file of the repository, from the root of the extension. */
function repositoryFile(...parts: string[]): string {
  return fs.readFileSync(path.join(__dirname, '..', '..', '..', ...parts), 'utf8');
}

/** `disabled_warnings` -> `disabledWarnings`, the key the language server reads. */
function camelCase(snake: string): string {
  return snake.replace(/_([a-z])/g, (_, letter: string) => letter.toUpperCase());
}

suite('compilerSettingsLogic', () => {
  test('serverInitializationOptions_when_lists_set_then_server_keys_carry_them', () => {
    const options = serverInitializationOptions(
      { disabledWarnings: ['P4083'], warningsAsErrors: ['C0354'] },
      'codesys',
    );

    assert.deepStrictEqual(options, {
      dialect: 'codesys',
      disabledWarnings: ['P4083'],
      warningsAsErrors: ['C0354'],
    });
  });

  test('readCompilerSettings_when_unset_then_empty_lists', () => {
    assert.deepStrictEqual(readCompilerSettings(() => undefined), EMPTY);
    assert.deepStrictEqual(readCompilerSettings(() => null), EMPTY);
  });

  test('readCompilerSettings_when_set_then_each_setting_read_by_its_name', () => {
    const asked: string[] = [];
    const settings = readCompilerSettings((name) => {
      asked.push(name);
      return name === 'disabledWarnings' ? ['P4083'] : ['C0354'];
    });

    assert.deepStrictEqual(asked, SETTING_NAMES);
    assert.deepStrictEqual(settings, { disabledWarnings: ['P4083'], warningsAsErrors: ['C0354'] });
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

  test('compilerFlags_when_nothing_set_then_no_flag', () => {
    assert.deepStrictEqual(compilerFlags(EMPTY), []);
  });

  test('compilerFlags_when_lists_set_then_one_flag_for_each_entry_in_declaration_order', () => {
    assert.deepStrictEqual(
      compilerFlags({ disabledWarnings: ['P4083', 'C0001'], warningsAsErrors: ['C0354'] }),
      ['--disable-warning', 'P4083', '--disable-warning', 'C0001', '--warning-as-error', 'C0354'],
    );
  });

  test('settings_when_each_declared_then_in_both_projections', () => {
    // A value for every declared setting: it must appear in the options of the
    // language server under its name and in the command line under its flag.
    const settings = readCompilerSettings(name => [`entry-of-${name}`]);
    const server = serverInitializationOptions(settings, 'iec61131-3-ed2') as unknown as Record<string, unknown>;
    const flags = compilerFlags(settings);

    for (const name of SETTING_NAMES) {
      assert.deepStrictEqual(server[name], [`entry-of-${name}`], `${name} missing from the server options`);
      const at = flags.indexOf(COMPILER_SETTINGS[name].flag);
      assert.ok(at >= 0, `${name}: ${COMPILER_SETTINGS[name].flag} missing from the command line`);
      assert.strictEqual(flags[at + 1], `entry-of-${name}`, name);
    }
  });

  test('settings_when_declared_in_package_json_then_arrays_of_strings_without_default_entries', () => {
    const manifest = JSON.parse(repositoryFile('package.json'));
    const properties = manifest.contributes.configuration.properties;

    for (const name of SETTING_NAMES) {
      const property = properties[`ironplc.${name}`];
      assert.ok(property, `ironplc.${name} is not declared in package.json`);
      assert.strictEqual(property.type, 'array', name);
      assert.strictEqual(property.items.type, 'string', name);
      assert.deepStrictEqual(property.default, [], name);
    }
  });

  test('settings_when_compared_with_the_compiler_then_names_and_flags_are_the_ones_it_declares', () => {
    // `WarningList` declares the option key and the flag of each list; the
    // language server reads the lowerCamelCase form of the key.
    const source = repositoryFile('..', '..', 'compiler', 'dsl', 'src', 'diagnostic', 'settings.rs');
    const keys = [...source.matchAll(/WarningList::\w+ => "([a-z_]+)",/g)].map(match => match[1]);
    const flags = [...source.matchAll(/WarningList::\w+ => "(--[a-z-]+)",/g)].map(match => match[1]);

    assert.deepStrictEqual(keys.map(camelCase), SETTING_NAMES);
    assert.deepStrictEqual(flags, SETTING_NAMES.map(name => COMPILER_SETTINGS[name].flag));
  });
});
