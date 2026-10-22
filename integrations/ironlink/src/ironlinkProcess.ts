/**
 * Starting `ironlink serve` as a child process, like a language server: no
 * daemon and no port, stdin and stdout carry the protocol and stderr is the
 * log. Vscode-free, so the discovery order and the failures are unit-tested
 * (the spawn itself against a real child, `node`, that answers like the
 * program).
 */

import { spawn } from 'child_process';

import { IronLinkLauncher, IronLinkProcess } from './ironlinkClient';
import { IronLinkProblem } from './failures';
import { ProblemCode, StdioLineTransport, ToolchainDiscoveryResult, ToolchainEnvironment, findToolchainExecutable } from './vscodeCommon';

/** The base name of the program; the platform adds `.exe` on Windows. */
export const IRONLINK_EXECUTABLE = 'ironlink';

/**
 * Finds the `ironlink` program by the toolchain discovery shared with the
 * IronPLC extension. The `path` setting of `env` is this extension's own
 * `ironlink.path`.
 */
export function locateIronLink(env: ToolchainEnvironment): ToolchainDiscoveryResult | undefined {
  return findToolchainExecutable(env, IRONLINK_EXECUTABLE);
}

/**
 * Spawns `command` with `args` and frames its stdio as lines. Resolves once
 * the operating system has started the process; rejects with
 * `IronLinkStartFailed`, carrying the reason, when it cannot (a program built
 * for another platform, a missing execute permission).
 */
export function spawnProcess(
  command: string,
  args: string[],
  onStderr: (text: string) => void,
): Promise<IronLinkProcess> {
  return new Promise((resolve, reject) => {
    const child = spawn(command, args);
    child.stderr.on('data', (chunk: Buffer) => onStderr(chunk.toString()));
    // A write to a child that already ended raises 'error' on stdin; the
    // transport reports the exit, so the write error needs no second report.
    child.stdin.on('error', () => {});
    const failed = (err: Error) => {
      reject(new IronLinkProblem(
        ProblemCode.IronLinkStartFailed,
        `${command}: ${err.message}`,
      ));
    };
    child.once('error', failed);
    child.once('spawn', () => {
      child.removeListener('error', failed);
      resolve({
        transport: new StdioLineTransport(child),
        stop: () => {
          child.kill();
        },
      });
    });
  });
}

/**
 * The launcher of the extension: locates the program on every start (so a
 * changed setting applies to the next process) and spawns `ironlink serve`.
 * `IronLinkNotFound` when no trial finds the program.
 */
export function ironLinkLauncher(
  env: () => ToolchainEnvironment,
  onStderr: (text: string) => void,
  onCommand: (commandLine: string) => void = () => {},
): IronLinkLauncher {
  return async () => {
    const found = locateIronLink(env());
    if (found === undefined) {
      throw new IronLinkProblem(
        ProblemCode.IronLinkNotFound,
        'Set the ironlink.path setting to the directory that contains ironlink.',
      );
    }
    const args = ['serve'];
    onCommand(`$ ${found.path} ${args.join(' ')}  (found by: ${found.source})`);
    return spawnProcess(found.path, args, onStderr);
  };
}
