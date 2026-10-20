import * as assert from 'assert';
import * as path from 'path';
import { CompilerSettings, compilerFlags } from '../../compilerSettingsLogic';
import { buildCompileArgs, compileArgs, outputFileNameForFolder } from '../../taskProviderLogic';

/** No setting is set: the compile has no flag of the settings. */
const NONE: CompilerSettings = { disabledWarnings: [], warningsAsErrors: [] };

/** Both lists set, to see the flags in the vector. */
const LISTED: CompilerSettings = { disabledWarnings: ['P4083'], warningsAsErrors: ['C0354', 'P4083'] };

suite('compileArgs', () => {
  test('compileArgs_when_project_input_then_compile_dot_output', () => {
    assert.deepStrictEqual(compileArgs('.', '/out/proj.iplc', NONE), ['compile', '.', '-o', '/out/proj.iplc']);
  });

  test('compileArgs_when_single_file_input_then_compile_file_output', () => {
    // The debug adapter compiles one source file to a temp container.
    assert.deepStrictEqual(
      compileArgs('/work/main.st', '/tmp/main.iplc', NONE),
      ['compile', '/work/main.st', '-o', '/tmp/main.iplc'],
    );
  });

  test('compileArgs_when_settings_listed_then_their_flags_come_before_the_input', () => {
    assert.deepStrictEqual(compileArgs('.', '/out/proj.iplc', LISTED), [
      'compile',
      '--disable-warning',
      'P4083',
      '--warning-as-error',
      'C0354',
      '--warning-as-error',
      'P4083',
      '.',
      '-o',
      '/out/proj.iplc',
    ]);
    assert.deepStrictEqual(compileArgs('.', 'o', LISTED).slice(1, 7), compilerFlags(LISTED));
  });

  test('buildCompileArgs_when_settings_listed_then_the_task_compile_has_the_flags', () => {
    const result = buildCompileArgs('/work/p', 'p.iplc', LISTED);
    assert.deepStrictEqual(result.args.slice(0, 3), ['compile', '--disable-warning', 'P4083']);
  });
});

suite('buildCompileArgs', () => {
  test('buildCompileArgs_when_workspace_folder_then_returns_compile_dot_with_output', () => {
    const result = buildCompileArgs('/home/user/myproject', 'myproject.iplc', NONE);
    assert.deepStrictEqual(result.args, [
      'compile',
      '.',
      '-o',
      path.join('/home/user/myproject', 'myproject.iplc'),
    ]);
    assert.strictEqual(result.cwd, '/home/user/myproject');
  });

  test('buildCompileArgs_when_windows_path_then_joins_correctly', () => {
    const result = buildCompileArgs('C:\\Users\\dev\\project', 'project.iplc', NONE);
    assert.strictEqual(result.args[0], 'compile');
    assert.strictEqual(result.args[1], '.');
    assert.strictEqual(result.args[2], '-o');
    assert.ok(result.args[3].endsWith('project.iplc'));
    assert.strictEqual(result.cwd, 'C:\\Users\\dev\\project');
  });
});

suite('outputFileNameForFolder', () => {
  test('outputFileNameForFolder_when_simple_name_then_appends_iplc_extension', () => {
    assert.strictEqual(outputFileNameForFolder('myproject'), 'myproject.iplc');
  });

  test('outputFileNameForFolder_when_name_with_spaces_then_preserves_spaces', () => {
    assert.strictEqual(outputFileNameForFolder('my project'), 'my project.iplc');
  });
});
