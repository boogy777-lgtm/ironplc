import { ChildProcessWithoutNullStreams } from 'child_process';

/**
 * A line-oriented channel to a peer: one JSON command per line out, one
 * response per line in. Protocol clients speak over this interface so they
 * can be unit-tested with a fake and run over any framing (a child's stdio, a
 * socket) without knowing which.
 */
export interface LineTransport {
  /** Writes one line (without the trailing newline) to the peer. */
  sendLine(line: string): void;
  /** Registers a listener for each complete line received from the peer. */
  onLine(listener: (line: string) => void): void;
  /** Registers a listener for when the peer closes the channel. */
  onExit(listener: () => void): void;
}

/**
 * Line-buffers a child process's stdio into a {@link LineTransport}: stdout is
 * the protocol channel (split into lines, carriage returns trimmed, empty
 * lines dropped), stdin carries one command per line. The process is spawned
 * by the caller, which also owns its lifetime and its stderr.
 */
export class StdioLineTransport implements LineTransport {
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
