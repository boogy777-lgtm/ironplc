/**
 * The VS Code side of the hot-edit engineering protocol (ADR-0052, ADR-0055):
 * a thin, unit-testable client for the newline-delimited JSON command session
 * served by `ironplcvm serve` (see `compiler/vm-cli/src/serve.rs`).
 *
 * One line carries one command; one line carries one response. This module
 * owns no process state — it speaks over an injected [`HotEditTransport`]
 * (line-buffered stdio in the extension glue), mirroring how `RunSession`
 * speaks over an injected `LanguageClientLike`. That keeps every protocol
 * decision (request framing, response matching, error shaping) unit-testable
 * without a real `ironplcvm` child process.
 */

/** The commands of the hot-edit protocol, one per `command` tag on the wire. */
export type HotEditCommand
  = | 'getStatus'
    | 'acceptEdits'
    | 'testEdits'
    | 'untestEdits'
    | 'assembleEdits'
    | 'cancelEdits';

/** A snapshot of the host's hot-edit state (the `status` response payload). */
export interface HotEditStatus {
  mode: string;
  active: number;
  normal: number;
  candidate: number | null;
  application: number;
  migration: boolean;
  rounds: number;
}

/**
 * Why a command failed: the stable V-code when the host supplied one
 * (`null` for transport-level codec errors), plus the host's own message.
 * The user-facing form is always `V#### - message`, or just the message
 * when there is no V-code.
 */
export class HotEditProtocolError extends Error {
  constructor(
    readonly vCode: string | null,
    message: string,
  ) {
    super(message);
    this.name = 'HotEditProtocolError';
  }

  toString(): string {
    return this.vCode ? `${this.vCode} - ${this.message}` : this.message;
  }
}

/** Renders one command as a single line of JSON, without a trailing newline. */
export function encodeRequest(command: HotEditCommand, program?: Uint8Array): string {
  if (command === 'acceptEdits') {
    return JSON.stringify({ command, program: Array.from(program ?? []) });
  }
  return JSON.stringify({ command });
}

type StatusResponse = { kind: 'status'; status: HotEditStatus };
type AckResponse = { kind: 'ack' };
type ErrorResponse = { kind: 'error'; error: HotEditProtocolError };

/** The parsed answer to one command, before command-specific matching. */
export type HotEditResponse = StatusResponse | AckResponse | ErrorResponse;

/**
 * Parses one response line (a single JSON value without its trailing
 * newline). Throws [`HotEditProtocolError`] when the line is not a
 * well-formed response, which means the wire desynced — the serve session
 * answers every command line with exactly one response line.
 */
export function parseResponseLine(line: string): HotEditResponse {
  let value: unknown;
  try {
    value = JSON.parse(line);
  }
  catch {
    throw new HotEditProtocolError(null, `invalid response line: ${line}`);
  }
  if (typeof value !== 'object' || value === null) {
    throw new HotEditProtocolError(null, `invalid response line: ${line}`);
  }

  const record = value as Record<string, unknown>;
  if (record.response === 'status') {
    return { kind: 'status', status: parseStatus(record) };
  }
  if (record.response === 'ack') {
    return { kind: 'ack' };
  }
  if (record.response === 'error') {
    const vCode = typeof record.vCode === 'string' ? record.vCode : null;
    const message = typeof record.message === 'string' ? record.message : 'unknown error';
    return { kind: 'error', error: new HotEditProtocolError(vCode, message) };
  }
  throw new HotEditProtocolError(null, `invalid response line: ${line}`);
}

/** Builds a [`HotEditStatus`] from a parsed `status` response object. */
function parseStatus(record: Record<string, unknown>): HotEditStatus {
  if (typeof record.mode !== 'string') {
    throw new HotEditProtocolError(null, 'status response is missing the mode');
  }
  return {
    mode: record.mode,
    active: numberField(record, 'active'),
    normal: numberField(record, 'normal'),
    candidate: record.candidate === null || record.candidate === undefined ? null : numberField(record, 'candidate'),
    application: numberField(record, 'application'),
    migration: record.migration === true,
    rounds: numberField(record, 'rounds'),
  };
}

/** Reads a numeric field, refusing a malformed status payload. */
function numberField(record: Record<string, unknown>, key: string): number {
  if (typeof record[key] !== 'number') {
    throw new HotEditProtocolError(null, `status response is missing ${key}`);
  }
  return record[key] as number;
}

/** "Normal", "Testing" — the wire mode with a display-ready capital. */
export function displayMode(mode: string): string {
  return mode.length === 0 ? mode : mode.charAt(0).toUpperCase() + mode.slice(1);
}

/** One-line summary for the status bar, e.g. "Normal (gen 1)". */
export function formatStatusText(status: HotEditStatus): string {
  return `${displayMode(status.mode)} (gen ${status.active})`;
}

/** Full detail for the status-bar tooltip and the Show Status notification. */
export function formatStatusDetail(status: HotEditStatus): string {
  const lines = [
    `Mode: ${displayMode(status.mode)}`,
    `Active generation: ${status.active}`,
    `Normal generation: ${status.normal}`,
    `Application generation: ${status.application}`,
    `Rounds: ${status.rounds}`,
  ];
  if (status.candidate !== null) {
    lines.push(`Candidate generation: ${status.candidate}`);
  }
  if (status.migration) {
    lines.push('Candidate changes the schema (state migration)');
  }
  return lines.join('\n');
}

/**
 * The line-oriented channel the session speaks over. The extension glue
 * implements this on the child process's stdio; unit tests inject a mock, so
 * no real child process is needed to exercise the protocol.
 */
export interface HotEditTransport {
  /** Writes one line (without the trailing newline) to the peer. */
  sendLine(line: string): void;
  /** Registers a listener for each complete line received from the peer. */
  onLine(listener: (line: string) => void): void;
  /** Registers a listener for when the peer closes the channel. */
  onExit(listener: () => void): void;
}

interface PendingRequest {
  resolve(response: HotEditResponse): void;
  reject(reason: Error): void;
}

/**
 * A client session over one `ironplcvm serve` process. Requests are matched
 * to responses in order: the serve session answers every command line with
 * exactly one response line, so a FIFO of pending requests is sufficient and
 * pipelined callers stay correct.
 */
export class HotEditSession {
  private readonly pending: PendingRequest[] = [];
  private exited = false;

  constructor(
    private readonly transport: HotEditTransport,
    private readonly onSessionExit: () => void = () => {},
  ) {
    transport.onLine(line => this.handleLine(line));
    transport.onExit(() => this.handleExit());
  }

  /** False once the peer has closed the channel. */
  get isActive(): boolean {
    return !this.exited;
  }

  /** Asks the host for its hot-edit status. */
  async getStatus(): Promise<HotEditStatus> {
    const response = await this.request('getStatus');
    if (response.kind !== 'status') {
      throw new HotEditProtocolError(null, `unexpected ${response.kind} response to getStatus`);
    }
    return response.status;
  }

  /** Stages the compiled container `program` as the edit candidate. */
  async acceptEdits(program: Uint8Array): Promise<void> {
    await this.request('acceptEdits', program);
  }

  /** Activates the staged candidate at the next scan boundary. */
  async testEdits(): Promise<void> {
    await this.request('testEdits');
  }

  /** Reverts to the original artifact at the next scan boundary. */
  async untestEdits(): Promise<void> {
    await this.request('untestEdits');
  }

  /** Promotes the staged candidate to the running application. */
  async assembleEdits(): Promise<void> {
    await this.request('assembleEdits');
  }

  /** Discards the staged candidate. */
  async cancelEdits(): Promise<void> {
    await this.request('cancelEdits');
  }

  /** Rejects pending requests; the transport itself is owned by the glue. */
  dispose(): void {
    this.rejectPending(new HotEditProtocolError(null, 'the hot edit session has ended.'));
  }

  private async request(command: HotEditCommand, program?: Uint8Array): Promise<HotEditResponse> {
    if (this.exited) {
      throw new HotEditProtocolError(null, 'the hot edit session has ended.');
    }
    const response = new Promise<HotEditResponse>((resolve, reject) => {
      this.pending.push({ resolve, reject });
    });
    this.transport.sendLine(encodeRequest(command, program));
    const result = await response;
    if (result.kind === 'error') {
      throw result.error;
    }
    return result;
  }

  private handleLine(line: string): void {
    let response: HotEditResponse;
    try {
      response = parseResponseLine(line);
    }
    catch (err) {
      // The wire desynced: no pending request can be matched reliably.
      this.rejectPending(err instanceof Error ? err : new Error(String(err)));
      return;
    }
    const pending = this.pending.shift();
    if (!pending) {
      // The serve session sends nothing unasked; drop the line defensively.
      return;
    }
    pending.resolve(response);
  }

  private handleExit(): void {
    if (this.exited) {
      return;
    }
    this.exited = true;
    this.rejectPending(new HotEditProtocolError(null, 'the ironplcvm process exited.'));
    this.onSessionExit();
  }

  private rejectPending(reason: Error): void {
    const pending = this.pending.splice(0, this.pending.length);
    for (const request of pending) {
      request.reject(reason);
    }
  }
}
