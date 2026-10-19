import * as assert from 'assert';
import * as fs from 'fs';
import * as path from 'path';

import {
  BUILD_AREA_NAME,
  BuildAreaChannel,
  buildContainer,
  buildFolder,
  chooseFolder,
  CompilerRun,
  execCompiler,
  FolderRef,
  problemOfOutcome,
  runBuild,
  RunCompilerFn,
  startBuild,
} from '../../buildAreaLogic';
import { ProblemCode } from '../../problems';
import { rejectWith } from './testHelpers';

/** An Output channel that records what it shows: the text since the last clear, and every call in order. */
class MockChannel implements BuildAreaChannel {
  text = '';
  readonly calls: string[] = [];

  append(text: string): void {
    this.calls.push('append');
    this.text += text;
  }

  clear(): void {
    this.calls.push('clear');
    this.text = '';
  }

  show(preserveFocus?: boolean): void {
    this.calls.push(`show:${preserveFocus}`);
  }
}

/** A compiler that answers every run with `run`, and records the invocations. */
function compilerThatRuns(run: CompilerRun): { exec: RunCompilerFn; invocations: { file: string; args: string[]; cwd?: string }[] } {
  const invocations: { file: string; args: string[]; cwd?: string }[] = [];
  const exec: RunCompilerFn = async (file, args, cwd) => {
    invocations.push({ file, args, cwd });
    return run;
  };
  return { exec, invocations };
}

const WARNING_RUN: CompilerRun = {
  stdout: '',
  stderr: '------ Build started: Application: areas -------\nWarning[P4015]: Unused variable\nBuild complete -- 0 errors, 1 warnings : Ready for download\n',
  exitCode: 0,
};

const ERROR_RUN: CompilerRun = {
  stdout: '',
  stderr: '------ Build started: Application: areas -------\nError[P4007]: Variable not defined before used\nBuild complete -- 1 errors, 0 warnings : No download possible\n',
  exitCode: 1,
};

suite('Build area', () => {
  test('build_area_name_when_read_then_is_the_source_of_the_build_area_of_the_compiler', () => {
    const area = fs.readFileSync(
      path.join(__dirname, '..', '..', '..', '..', '..', 'compiler', 'dsl', 'src', 'diagnostic', 'area.rs'),
      'utf-8',
    );

    assert.strictEqual(BUILD_AREA_NAME, 'IronPLC Build');
    assert.ok(area.includes(`"${BUILD_AREA_NAME}"`), 'area.rs does not name the source of the Build area');
  });

  test('package_json_when_build_application_command_then_declared_in_category_ironplc', () => {
    const contributes = JSON.parse(
      fs.readFileSync(path.join(__dirname, '..', '..', '..', 'package.json'), 'utf-8'),
    ).contributes as { commands: { command: string; title: string; category?: string }[] };

    const command = contributes.commands.find(cmd => cmd.command === 'ironplc.buildApplication');

    assert.ok(command, 'ironplc.buildApplication is not contributed in package.json');
    assert.strictEqual(command.title, 'Build');
    assert.strictEqual(command.category, 'IronPLC');
  });
});

suite('Build area: running a build', () => {
  test('start_build_when_called_then_clears_and_shows_without_taking_focus', () => {
    const channel = new MockChannel();

    startBuild(channel);

    assert.deepStrictEqual(channel.calls, ['clear', 'show:true']);
  });

  test('run_build_when_compiler_exits_zero_then_channel_shows_everything_it_wrote', async () => {
    const channel = new MockChannel();
    const { exec } = compilerThatRuns(WARNING_RUN);

    const outcome = await runBuild(channel, 'ironplcc', ['compile'], undefined, exec);

    assert.deepStrictEqual(outcome, { kind: 'finished', exitCode: 0 });
    assert.strictEqual(channel.text, WARNING_RUN.stderr);
    assert.strictEqual(problemOfOutcome('ironplcc', outcome), undefined);
  });

  test('run_build_when_compiler_exits_nonzero_then_channel_shows_the_messages_and_no_problem_is_reported', async () => {
    const channel = new MockChannel();
    const { exec } = compilerThatRuns(ERROR_RUN);

    const outcome = await runBuild(channel, 'ironplcc', ['compile'], undefined, exec);

    assert.deepStrictEqual(outcome, { kind: 'finished', exitCode: 1 });
    assert.ok(channel.text.includes('Error[P4007]'), channel.text);
    assert.ok(channel.text.endsWith('No download possible\n'), channel.text);
    assert.strictEqual(problemOfOutcome('ironplcc', outcome), undefined);
  });

  test('run_build_when_compiler_wrote_stdout_and_stderr_then_both_are_shown_each_ending_a_line', async () => {
    const channel = new MockChannel();
    const { exec } = compilerThatRuns({ stdout: 'out', stderr: 'err', exitCode: 2 });

    await runBuild(channel, 'ironplcc', [], undefined, exec);

    assert.strictEqual(channel.text, 'err\nout\n');
  });

  test('run_build_when_nonzero_exit_without_any_output_then_a_line_names_the_exit_code', async () => {
    const channel = new MockChannel();
    const { exec } = compilerThatRuns({ stdout: '', stderr: '', exitCode: 3 });

    await runBuild(channel, 'ironplcc', [], undefined, exec);

    assert.strictEqual(channel.text, 'The compiler ended with exit code 3 and wrote no messages.\n');
  });

  test('run_build_when_compiler_cannot_be_started_then_reason_is_shown_and_outcome_is_not_started', async () => {
    const channel = new MockChannel();
    const exec: RunCompilerFn = async () => {
      throw new Error('spawn ironplcc ENOENT');
    };

    const outcome = await runBuild(channel, 'ironplcc', [], undefined, exec);

    assert.deepStrictEqual(outcome, { kind: 'not-started', reason: 'spawn ironplcc ENOENT' });
    assert.strictEqual(channel.text, 'spawn ironplcc ENOENT\n');
  });

  test('problem_of_outcome_when_not_started_then_no_compiler_problem_names_the_path_and_reason', () => {
    const problem = problemOfOutcome('C:\\bin\\ironplcc.exe', { kind: 'not-started', reason: 'spawn EACCES' });

    assert.ok(problem);
    assert.strictEqual(problem.code, ProblemCode.NoCompiler);
    assert.ok(problem.context.includes('C:\\bin\\ironplcc.exe'), problem.context);
    assert.ok(problem.context.includes('spawn EACCES'), problem.context);
  });

  test('run_build_when_second_build_starts_then_channel_holds_only_the_second_build', async () => {
    const channel = new MockChannel();

    await runBuild(channel, 'ironplcc', [], undefined, compilerThatRuns(ERROR_RUN).exec);
    await runBuild(channel, 'ironplcc', [], undefined, compilerThatRuns(WARNING_RUN).exec);

    assert.strictEqual(channel.text, WARNING_RUN.stderr);
    assert.deepStrictEqual(channel.calls.filter(call => call === 'clear').length, 2);
  });

  test('run_build_when_build_runs_then_area_is_cleared_and_shown_before_anything_is_written', async () => {
    const channel = new MockChannel();

    await runBuild(channel, 'ironplcc', [], undefined, compilerThatRuns(WARNING_RUN).exec);

    assert.deepStrictEqual(channel.calls, ['clear', 'show:true', 'append']);
  });
});

suite('Build area: the callers of a build', () => {
  test('build_folder_when_folder_then_compiles_as_the_task_does_in_the_folder', async () => {
    const channel = new MockChannel();
    const { exec, invocations } = compilerThatRuns(WARNING_RUN);
    const folder: FolderRef = { name: 'areas', path: path.join(path.sep, 'work', 'areas') };

    await buildFolder(channel, 'ironplcc', folder, exec);

    assert.deepStrictEqual(invocations, [{
      file: 'ironplcc',
      args: ['compile', '.', '-o', path.join(folder.path, 'areas.iplc')],
      cwd: folder.path,
    }]);
  });

  test('build_container_when_compile_succeeds_then_resolves_with_the_container_in_the_output_directory', async () => {
    const channel = new MockChannel();
    const { exec, invocations } = compilerThatRuns(WARNING_RUN);
    const dir = path.join(path.sep, 'tmp');

    const container = await buildContainer(channel, 'ironplcc', path.join(path.sep, 'src', 'main.st'), dir, exec);

    assert.strictEqual(container, path.join(dir, 'main.iplc'));
    assert.deepStrictEqual(invocations[0].args, ['compile', path.join(path.sep, 'src', 'main.st'), '-o', container]);
    assert.strictEqual(channel.text, WARNING_RUN.stderr);
  });

  test('build_container_when_compile_fails_then_rejects_and_channel_shows_all_messages', async () => {
    const channel = new MockChannel();
    const { exec } = compilerThatRuns(ERROR_RUN);

    const err = await rejectWith(buildContainer(channel, 'ironplcc', 'main.st', 'tmp', exec));

    assert.ok(err instanceof Error);
    assert.ok(err.message.includes('exit code 1'), err.message);
    assert.strictEqual(channel.text, ERROR_RUN.stderr);
  });

  test('build_container_when_compiler_cannot_be_started_then_rejects_with_the_reason', async () => {
    const channel = new MockChannel();
    const exec: RunCompilerFn = async () => {
      throw new Error('spawn ironplcc ENOENT');
    };

    const err = await rejectWith(buildContainer(channel, 'ironplcc', 'main.st', 'tmp', exec));

    assert.ok(err instanceof Error);
    assert.strictEqual(err.message, 'spawn ironplcc ENOENT');
  });
});

suite('Build area: the folder of a build', () => {
  const root = path.join(path.sep, 'work');
  const first: FolderRef = { name: 'first', path: path.join(root, 'first') };
  const second: FolderRef = { name: 'second', path: path.join(root, 'second') };

  test('choose_folder_when_no_folder_then_none', () => {
    assert.deepStrictEqual(chooseFolder([], path.join(first.path, 'a.st')), { kind: 'none' });
  });

  test('choose_folder_when_only_one_folder_then_that_folder_even_if_the_editor_is_elsewhere', () => {
    assert.deepStrictEqual(
      chooseFolder([first], path.join(root, 'elsewhere', 'a.st')),
      { kind: 'folder', folder: first },
    );
  });

  test('choose_folder_when_several_folders_and_active_file_in_one_then_that_folder', () => {
    assert.deepStrictEqual(
      chooseFolder([first, second], path.join(second.path, 'src', 'a.st')),
      { kind: 'folder', folder: second },
    );
  });

  test('choose_folder_when_folders_nest_then_the_innermost_folder_of_the_active_file', () => {
    const inner: FolderRef = { name: 'inner', path: path.join(first.path, 'inner') };

    assert.deepStrictEqual(
      chooseFolder([first, inner], path.join(inner.path, 'a.st')),
      { kind: 'folder', folder: inner },
    );
  });

  test('choose_folder_when_several_folders_and_no_active_file_then_ask', () => {
    assert.deepStrictEqual(chooseFolder([first, second]), { kind: 'ask', folders: [first, second] });
  });

  test('choose_folder_when_active_file_outside_every_folder_then_ask', () => {
    assert.deepStrictEqual(
      chooseFolder([first, second], path.join(root, 'third', 'a.st')),
      { kind: 'ask', folders: [first, second] },
    );
  });

  test('choose_folder_when_active_file_in_a_folder_with_a_common_name_prefix_then_not_that_folder', () => {
    const near: FolderRef = { name: 'first2', path: path.join(root, 'first2') };

    assert.deepStrictEqual(
      chooseFolder([first, second], path.join(near.path, 'a.st')),
      { kind: 'ask', folders: [first, second] },
    );
  });
});

suite('Build area: the process of a build', () => {
  test('exec_compiler_when_process_exits_zero_then_resolves_with_output_and_code_zero', async () => {
    const run = await execCompiler(process.execPath, ['-e', 'console.log("out"); console.error("err")']);

    assert.strictEqual(run.exitCode, 0);
    assert.strictEqual(run.stdout.trim(), 'out');
    assert.strictEqual(run.stderr.trim(), 'err');
  });

  test('exec_compiler_when_process_exits_nonzero_then_resolves_with_code_and_all_output', async () => {
    const run = await execCompiler(process.execPath, ['-e', 'console.error("line one\\nline two"); process.exit(3)']);

    assert.strictEqual(run.exitCode, 3);
    assert.strictEqual(run.stderr.trim(), 'line one\nline two');
  });

  test('exec_compiler_when_cwd_given_then_process_runs_there', async () => {
    const dir = fs.realpathSync(__dirname);

    const run = await execCompiler(process.execPath, ['-e', 'console.log(process.cwd())'], dir);

    assert.strictEqual(fs.realpathSync(run.stdout.trim()), dir);
  });

  test('exec_compiler_when_program_does_not_exist_then_rejects', async () => {
    const err = await rejectWith(execCompiler(path.join(__dirname, 'no-such-compiler-here'), []));

    assert.ok(err instanceof Error);
  });
});
