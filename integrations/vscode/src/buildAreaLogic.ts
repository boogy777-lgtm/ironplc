/**
 * The Build message area in VS Code. The compiler keeps its messages in areas
 * (`compiler/dsl/src/diagnostic/area.rs`), as the CODESYS Messages view does,
 * and the editor shows each area where it belongs: the Precompile area in the
 * Problems panel (the language server fills it), the Build area in one Output
 * channel (this module). The Output panel's channel drop-down plays the role
 * of the CODESYS area drop-down.
 *
 * Every build the extension runs writes the one shared channel, and takes it
 * as a parameter instead of creating its own: the channel is cleared when a
 * build starts (CODESYS clears the Build list at each build) and then shows
 * everything the compiler wrote, also when the compile failed. A non-zero exit
 * is a normal outcome of a build — the errors are the messages, and the last
 * line says that no download is possible — so it is not a problem of the
 * extension. Only a compiler that cannot be started is.
 *
 * This module has no `vscode` import; the glue passes the channel in.
 */

import { execFile } from 'child_process';
import * as path from 'path';

import { ProblemCode } from './problems';
import { containerOutputPath } from './debugAdapterLogic';
import { CompilerSettings } from './compilerSettingsLogic';
import { buildCompileArgs, compileArgs, outputFileNameForFolder } from './taskProviderLogic';

/**
 * The name of the shared channel. It is the `source` of `Area::Build` in
 * `compiler/dsl/src/diagnostic/area.rs` ("IronPLC Build"), so the editor's two
 * areas read alike: the Problems panel shows "IronPLC Precompile", the Output
 * panel offers "IronPLC Build".
 */
export const BUILD_AREA_NAME = 'IronPLC Build';

/** What the area needs of an Output channel; `vscode.OutputChannel` satisfies it. */
export interface BuildAreaChannel {
  append(text: string): void;
  clear(): void;
  show(preserveFocus?: boolean): void;
}

/** What one compiler run wrote and how it ended. */
export interface CompilerRun {
  stdout: string;
  stderr: string;
  exitCode: number;
}

/**
 * Runs the compiler once. It resolves with the exit code whatever it is, and
 * rejects only when the process could not be started (not found, not
 * executable). Tests inject a mock.
 */
export type RunCompilerFn = (file: string, args: string[], cwd?: string) => Promise<CompilerRun>;

/** The output of a compile is a few lines per message; this leaves room for a very large project. */
const MAX_OUTPUT_BYTES = 64 * 1024 * 1024;

/** The `RunCompilerFn` on `child_process.execFile`. */
export const execCompiler: RunCompilerFn = (file, args, cwd) => {
  return new Promise((resolve, reject) => {
    execFile(file, args, { cwd, encoding: 'utf8', maxBuffer: MAX_OUTPUT_BYTES }, (error, stdout, stderr) => {
      if (!error) {
        resolve({ stdout, stderr, exitCode: 0 });
        return;
      }
      // A non-zero exit carries a numeric `code`; a start failure carries a
      // string (`ENOENT`) and a process ended by a signal carries none.
      const code = (error as { code?: unknown }).code;
      if (typeof code === 'number') {
        resolve({ stdout, stderr, exitCode: code });
        return;
      }
      reject(error);
    });
  });
};

/** How a build ended for the area: the compiler ran (any exit code), or it could not be started. */
export type BuildOutcome
  = | { kind: 'finished'; exitCode: number }
    | { kind: 'not-started'; reason: string };

/** A build starts: the area is cleared and shown without taking the focus. */
export function startBuild(channel: BuildAreaChannel): void {
  channel.clear();
  channel.show(true);
}

/** The text with a line end at its end, so the next write starts a line. */
function endedLine(text: string): string {
  return text.endsWith('\n') ? text : `${text}\n`;
}

/**
 * Runs one compile into the Build area: clears and shows it, runs the
 * compiler and writes everything it wrote (its messages are on standard
 * error), whatever the exit code.
 */
export async function runBuild(
  channel: BuildAreaChannel,
  compiler: string,
  args: string[],
  cwd: string | undefined,
  run: RunCompilerFn = execCompiler,
): Promise<BuildOutcome> {
  startBuild(channel);
  let result: CompilerRun;
  try {
    result = await run(compiler, args, cwd);
  }
  catch (err) {
    const reason = err instanceof Error ? err.message : String(err);
    channel.append(endedLine(reason));
    return { kind: 'not-started', reason };
  }
  for (const text of [result.stderr, result.stdout]) {
    if (text) {
      channel.append(endedLine(text));
    }
  }
  if (result.exitCode !== 0 && !result.stderr && !result.stdout) {
    channel.append(`The compiler ended with exit code ${result.exitCode} and wrote no messages.\n`);
  }
  return { kind: 'finished', exitCode: result.exitCode };
}

/** A workspace folder, as the logic needs it. */
export interface FolderRef {
  name: string;
  path: string;
}

/** Which folder a build is for. */
export type FolderChoice
  = | { kind: 'none' }
    | { kind: 'folder'; folder: FolderRef }
    | { kind: 'ask'; folders: FolderRef[] };

/** What is shown when no workspace folder is open and so nothing can run. */
export const NO_FOLDER_MESSAGE = 'IronPLC Build needs a workspace folder. Open a folder that holds the project first.';

/** Whether `file` is inside `folder`. */
function isInside(folder: string, file: string): boolean {
  const relative = path.relative(folder, file);
  return relative !== '' && !relative.startsWith('..') && !path.isAbsolute(relative);
}

/**
 * The folder a build is for: the only folder; else the folder of the active
 * editor's file (the innermost, when folders nest); else the person is asked.
 */
export function chooseFolder(folders: readonly FolderRef[], activeFile?: string): FolderChoice {
  if (folders.length === 0) {
    return { kind: 'none' };
  }
  if (folders.length === 1) {
    return { kind: 'folder', folder: folders[0] };
  }
  if (activeFile) {
    const holding = folders
      .filter(folder => isInside(folder.path, activeFile))
      .sort((a, b) => b.path.length - a.path.length);
    if (holding.length > 0) {
      return { kind: 'folder', folder: holding[0] };
    }
  }
  return { kind: 'ask', folders: [...folders] };
}

/** Builds the project of a folder as the `ironplc: compile` task does: the output is `<folder>.iplc` in the folder. */
export function buildFolder(
  channel: BuildAreaChannel,
  compiler: string,
  folder: FolderRef,
  settings: CompilerSettings,
  run: RunCompilerFn = execCompiler,
): Promise<BuildOutcome> {
  const details = buildCompileArgs(folder.path, outputFileNameForFolder(folder.name), settings);
  return runBuild(channel, compiler, details.args, details.cwd, run);
}

/**
 * Compiles one source to a container in `outputDir` into the Build area, for a
 * flow that needs the container (the device build). Resolves with the
 * container path; rejects when the compiler cannot be started or does not end
 * with exit code 0 — the messages are in the area.
 */
export async function buildContainer(
  channel: BuildAreaChannel,
  compiler: string,
  source: string,
  outputDir: string,
  settings: CompilerSettings,
  run: RunCompilerFn = execCompiler,
): Promise<string> {
  const output = containerOutputPath(source, outputDir);
  const outcome = await runBuild(channel, compiler, compileArgs(source, output, settings), undefined, run);
  if (outcome.kind === 'not-started') {
    throw new Error(outcome.reason);
  }
  if (outcome.exitCode !== 0) {
    throw new Error(`the compiler ended with exit code ${outcome.exitCode}`);
  }
  return output;
}

/**
 * The problem of the extension that a build outcome is, if any. A compiler
 * that ran is not one, whatever its exit code; one that could not be started
 * is.
 */
export function problemOfOutcome(
  compiler: string,
  outcome: BuildOutcome,
): { code: ProblemCode; context: string } | undefined {
  if (outcome.kind === 'finished') {
    return undefined;
  }
  return {
    code: ProblemCode.NoCompiler,
    context: `IronPLC could not start the compiler at "${compiler}": ${outcome.reason}`,
  };
}
