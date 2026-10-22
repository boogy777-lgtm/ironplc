/**
 * The stdio transport of the engineering connection (ADR-0063) and of the
 * hot-edit session: the `ironplcvm serve` child's stdin/stdout as a
 * `HotEditTransport`, one JSON command per line. Like `tcpTransport.ts`, the
 * module is vscode-free so the connection methods can use it and unit tests
 * can load it.
 */

import { ChildProcessWithoutNullStreams } from 'child_process';
import { pathForPlatform } from './platformPath';
import { HotEditTransport } from './hotEditSession';

/** The VM executable name on `platform` (`.exe` on Windows). */
export function vmFileName(platform: string): string {
  return platform === 'win32' ? 'ironplcvm.exe' : 'ironplcvm';
}

/** The VM executable that sits next to the compiler at `compilerPath`. */
export function vmPathBeside(compilerPath: string, platform: string): string {
  const platformPath = pathForPlatform(platform);
  return platformPath.join(platformPath.dirname(compilerPath), vmFileName(platform));
}

/**
 * Line-buffers the child process's stdio into [`HotEditTransport`]: stdout is
 * the protocol channel (split into lines, carriage returns trimmed), stdin
 * carries one JSON command per line. The hot-edit commands and the
 * engineering connection's stdio method both spawn the same `ironplcvm serve`
 * session over this framing.
 */
export class StdioLineTransport implements HotEditTransport {
  private readonly lineListeners: ((line: string) => void)[] = [];
  private readonly exitListeners: (() => void)[] = [];
  private exited = false;

  constructor(private readonly child: ChildProcessWithoutNullStreams) {
    let buffer = '';
    child.stdout.on('data', (chunk: Buffer) => {
      buffer += chunk.toString();
      for (;;) {
        const newline = buffer.indexOf('\n');
        if (newline < 0) {
          break;
        }
        let line = buffer.slice(0, newline);
        buffer = buffer.slice(newline + 1);
        if (line.endsWith('\r')) {
          line = line.slice(0, -1);
        }
        if (line.length > 0) {
          this.emitLine(line);
        }
      }
    });
    // A spawn failure surfaces as 'error' (and may not be followed by
    // 'exit'); either way the channel is gone, so report it once.
    child.on('error', () => this.emitExit());
    child.on('exit', () => this.emitExit());
  }

  sendLine(line: string): void {
    this.child.stdin.write(line + '\n');
  }

  onLine(listener: (line: string) => void): void {
    this.lineListeners.push(listener);
  }

  onExit(listener: () => void): void {
    this.exitListeners.push(listener);
  }

  private emitLine(line: string): void {
    for (const listener of this.lineListeners) {
      listener(line);
    }
  }

  private emitExit(): void {
    if (this.exited) {
      return;
    }
    this.exited = true;
    for (const listener of this.exitListeners) {
      listener();
    }
  }
}
