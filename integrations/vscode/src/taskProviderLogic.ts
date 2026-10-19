import { execFile } from 'child_process';
import * as os from 'os';
import * as path from 'path';

import { CompilerSettings, compilerFlags } from './compilerSettingsLogic';
import { containerOutputPath, firstLine } from './debugAdapterLogic';
import { ExecFileFn } from './syncUidsLogic';

/**
 * Builds the `ironplcc compile` argument vector for compiling `input` to the
 * container at `output`. Shared by the build task (which compiles the whole
 * project via `.`) and the debug adapter (which compiles a single source file).
 *
 * The compiler settings of the editor are an argument and not an option: every
 * compile the extension starts is built here, so a site that forgets them does
 * not type-check (`compilerSettingsLogic.ts`).
 */
export function compileArgs(input: string, output: string, settings: CompilerSettings): string[] {
  return ['compile', ...compilerFlags(settings), input, '-o', output];
}

/**
 * Builds the arguments for the ironplcc compile command.
 */
export function buildCompileArgs(
  workspaceFolderPath: string,
  outputFileName: string,
  settings: CompilerSettings,
): { args: string[]; cwd: string } {
  const outputPath = path.join(workspaceFolderPath, outputFileName);
  return {
    args: compileArgs('.', outputPath, settings),
    cwd: workspaceFolderPath,
  };
}

/**
 * Derives the output file name from a workspace folder name.
 */
export function outputFileNameForFolder(folderName: string): string {
  return `${folderName}.iplc`;
}

/**
 * Runs one compiler invocation, echoing the command and its output to `log`;
 * rejects with the first diagnostic line on failure. The single transport
 * every compiler-spawning command shares: the hot-edit session, the sync
 * command, and the connection/build glue. `exec` is injectable for tests.
 */
export function runCompiler(
  compiler: string,
  args: string[],
  log: (text: string) => void,
  exec: ExecFileFn = defaultExecFile,
): Promise<{ stdout: string; stderr: string }> {
  log(`$ ${compiler} ${args.join(' ')}\n`);
  return exec(compiler, args).then(
    (result) => {
      if (result.stdout) {
        log(result.stdout);
      }
      if (result.stderr) {
        log(result.stderr);
      }
      return result;
    },
    (err: unknown) => {
      const error = err instanceof Error ? err : new Error(String(err));
      log(error.message);
      throw new Error(firstLine(error.message) || String(error));
    },
  );
}

/** The glue-side `ExecFileFn` on `child_process.execFile`. */
function defaultExecFile(file: string, args: string[]): Promise<{ stdout: string; stderr: string }> {
  return new Promise((resolve, reject) => {
    execFile(file, args, (error, stdout, stderr) => {
      if (error) {
        const detail = firstLine(stderr) || firstLine(stdout) || String(error);
        reject(new Error(detail));
        return;
      }
      resolve({ stdout, stderr });
    });
  });
}

/**
 * The serve session loads a compiled container, so a source program must be
 * compiled first — the same single-file compile the debug launch uses.
 * Resolves with the container path.
 */
export function compileSourceToContainer(
  compiler: string,
  source: string,
  log: (text: string) => void,
  settings: CompilerSettings,
): Promise<string> {
  const output = containerOutputPath(source, os.tmpdir());
  const args = compileArgs(source, output, settings);
  return runCompiler(compiler, args, log).then(() => output);
}
