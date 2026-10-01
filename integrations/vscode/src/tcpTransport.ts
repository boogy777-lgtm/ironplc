/**
 * The TCP transport of the engineering connection (ADR-0063): the
 * `HotEditTransport` line interface implemented over a socket, one
 * 4-byte-little-endian-length-prefixed frame per message instead of one
 * newline-delimited line. The frame replaces only the delimiter — the JSON
 * inside is byte-identical to the stdio line — so `HotEditSession` serves
 * either transport behind the same interface.
 *
 * The module is vscode-free on purpose: like `hotEditSession.ts`, it is
 * unit-testable against a real local listener without an `ironplcvm`
 * process. Exported for the Mechanism-1 connection-profile wiring; nothing
 * references it yet.
 */

import { Socket, createConnection } from 'net';

import { HotEditTransport } from './hotEditSession';

/**
 * The largest frame accepted, mirroring the server (16 MiB, the
 * container-upload headroom). A larger declared length means the peer is not
 * speaking the session protocol — the transport faults (exit), exactly what
 * the server does with V6013.
 */
export const MAX_FRAME_LENGTH = 16 * 1024 * 1024;

/**
 * Frame-buffers a socket into {@link HotEditTransport}: one frame in, one
 * line out; one line written, one frame out. Mirrors the stdio
 * `StdioLineTransport`'s shape (listener arrays, single exit) so the session
 * glue treats both transports alike.
 */
export class TcpLineTransport implements HotEditTransport {
  private readonly lineListeners: ((line: string) => void)[] = [];
  private readonly exitListeners: (() => void)[] = [];
  private buffer: Buffer = Buffer.alloc(0);
  private exited = false;

  constructor(private readonly socket: Socket) {
    socket.on('data', (chunk: Buffer) => this.handleData(chunk));
    // A lost peer surfaces as 'error' and/or 'close'; either way the channel
    // is gone, so report it once.
    socket.on('error', () => this.emitExit());
    socket.on('close', () => this.emitExit());
  }

  /** Writes one line (without the trailing newline) as one frame. */
  sendLine(line: string): void {
    const payload = Buffer.from(line, 'utf8');
    const header = Buffer.alloc(4);
    header.writeUInt32LE(payload.length, 0);
    this.socket.write(Buffer.concat([header, payload]));
  }

  onLine(listener: (line: string) => void): void {
    this.lineListeners.push(listener);
  }

  onExit(listener: () => void): void {
    this.exitListeners.push(listener);
  }

  /** Tears the transport down; the peer sees the socket close. */
  dispose(): void {
    this.socket.destroy();
  }

  private handleData(chunk: Buffer): void {
    this.buffer = this.buffer.length === 0 ? chunk : Buffer.concat([this.buffer, chunk]);
    for (;;) {
      if (this.buffer.length < 4) {
        return;
      }
      const length = this.buffer.readUInt32LE(0);
      if (length > MAX_FRAME_LENGTH) {
        this.dispose();
        this.emitExit();
        return;
      }
      if (this.buffer.length < 4 + length) {
        return;
      }
      const line = this.buffer.subarray(4, 4 + length).toString('utf8');
      this.buffer = this.buffer.subarray(4 + length);
      if (line.length > 0) {
        this.emitLine(line);
      }
    }
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

/**
 * Opens a TCP transport to `host:port`, resolving once the socket connects.
 * The connect timeout defaults to the 5 s the engineering connection's
 * timeout policy sets for opening a transport.
 */
export function connectTcpLineTransport(
  host: string,
  port: number,
  timeoutMs = 5000,
): Promise<TcpLineTransport> {
  return new Promise((resolve, reject) => {
    const timer = setTimeout(() => {
      socket.destroy();
      reject(new Error(`timed out connecting to ${host}:${port} after ${timeoutMs} ms`));
    }, timeoutMs);
    const socket = createConnection({ host, port }, () => {
      clearTimeout(timer);
      resolve(new TcpLineTransport(socket));
    });
    socket.on('error', (err) => {
      clearTimeout(timer);
      reject(err);
    });
  });
}
