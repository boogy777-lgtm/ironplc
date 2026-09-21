/**
 * The VS Code side of the hot-edit engineering protocol (ADR-0052, ADR-0055):
 * a thin, unit-testable client for the newline-delimited JSON command session
 * served by `ironplcvm serve` (see `compiler/vm-cli/src/serve.rs`).
 *
 * The module also owns the ADR-0061 wire shapes: the `migration` decision map
 * on `acceptEdits` and the `pairs` list a V4010 refusal carries.
 *
 * One line carries one command; one line carries one response. This module
 * owns no process state — it speaks over an injected [`HotEditTransport`]
 * (line-buffered stdio in the extension glue), mirroring how `RunSession`
 * speaks over an injected `LanguageClientLike`. That keeps every protocol
 * decision (request framing, response matching, error shaping) unit-testable
 * without a real `ironplcvm` child process.
 */

import {
  encodeHaCommand,
  HaBarrier,
  HaCalibration,
  HaCommandName,
  HaEvents,
  HaIoReady,
  HaResponse,
  HaStatus,
  HaTimingBudget,
  parseHaResponseLine,
} from './haProtocol';

/** The commands of the hot-edit protocol, one per `command` tag on the wire. */
export type HotEditCommand
  = | 'identity'
    | 'getStatus'
    | 'acceptEdits'
    | 'testEdits'
    | 'untestEdits'
    | 'assembleEdits'
    | 'cancelEdits';

/** One engineer decision for a shared-UID type change (ADR 0061). */
export type MigrationDecision = 'init' | 'preserve';

/**
 * The wire map for `pairs`: one decision per shared
 * UID, keyed by the UID's string form, e.g. `{"1":"preserve"}`.
 */
export type MigrationDecisionMap = Record<string, MigrationDecision>;

/**
 * The session protocol version this client speaks (ADR-0063): `1` for the
 * version the engineering connection spec defines. A client refuses a
 * higher number; an older server answers `identity` with the codeless codec
 * error, which the connection state machine reads as a failed handshake.
 */
export const SUPPORTED_SESSION_PROTOCOL = 1;

/**
 * The optional `edit` identity block of `acceptEdits` (ADR-0064): the
 * advisory client labels the pending-edit record reports while the candidate
 * is staged. Both fields are optional.
 */
export interface EditIdentity {
  /** The client-supplied edit label (e.g. the source file name). */
  name?: string;
  /** The unauthenticated advisory client label (e.g. the editor name). */
  origin?: string;
}

/** A snapshot of the host's hot-edit state (the `status` response payload). */
export interface HotEditStatus {
  mode: string;
  active: number;
  normal: number;
  candidate: number | null;
  application: number;
  migration: boolean;
  rounds: number;
  /** The pending-edit record written at Accept, when a candidate is staged. */
  pendingEdit?: PendingEditRecord;
}

/** The baseline block of the pending-edit record (the normal artifact at Accept). */
export interface EditBaseline {
  normalGeneration: number;
  contentHash: readonly number[];
}

/**
 * The controller-side pending-edit record (ADR-0064): metadata written at
 * Accept beside the candidate, cleared exactly where the candidate dies
 * (Assemble, Cancel).
 */
export interface PendingEditRecord {
  name: string | null;
  origin: string | null;
  acceptedAt: number;
  baseline: EditBaseline;
}

/** The device panel block of an `identity` response (ADR-0063). */
export interface DeviceIdentity {
  name: string;
  model: string;
  modification: string;
  firmwareVersion: string;
}

/** The optional redundancy block of an `identity` response (ADR-0063). */
export interface RedundancyIdentity {
  pairId: string;
  role: string;
  epoch: number;
  sync: string;
  control: string;
}

/**
 * The payload of an `identity` response: the session protocol version, the
 * device panel fields, the live application-state snapshot (the same fields
 * `getStatus` returns), and the optional redundancy block.
 */
export interface IdentityInfo {
  protocol: number;
  device: DeviceIdentity;
  application: HotEditStatus;
  redundancy?: RedundancyIdentity;
}

/**
 * One out-of-policy storage-class change a V4010 error names (ADR 0061):
 * what a client renders as a decision-list row.
 */
export interface TypeChangePair {
  /** The entity's stable UID, the key of the decision map. */
  uid: number;
  /** The entity's debug name, when the candidate names it. */
  name: string | null;
  /** The active artifact's storage class (e.g. `"I32"`). */
  from: string;
  /** The candidate's storage class (e.g. `"U32"`). */
  to: string;
  /** Whether a `preserve` decision is legal for this pair (storage sizes equal). */
  sizeEqual: boolean;
}

/**
 * Why a command failed: the stable V-code when the host supplied one
 * (`null` for transport-level codec errors), plus the host's own message.
 * The user-facing form is always `V#### - message`, or just the message
 * when there is no V-code. A V4010 refusal also carries the `pairs` list
 * of type changes to decide (ADR 0061); every other error carries none.
 */
export class HotEditProtocolError extends Error {
  constructor(
    readonly vCode: string | null,
    message: string,
    readonly pairs: readonly TypeChangePair[] = [],
  ) {
    super(message);
    this.name = 'HotEditProtocolError';
  }

  toString(): string {
    return this.vCode ? `${this.vCode} - ${this.message}` : this.message;
  }
}

/**
 * Renders one command as a single line of JSON, without a trailing newline.
 * The `acceptEdits` line carries the program bytes and, when non-empty, the
 * ADR-0061 migration decision map and the ADR-0064 edit identity.
 */
export function encodeRequest(
  command: HotEditCommand,
  program?: Uint8Array,
  migration?: MigrationDecisionMap,
  edit?: EditIdentity,
): string {
  if (command === 'acceptEdits') {
    const request: Record<string, unknown> = { command, program: Array.from(program ?? []) };
    if (edit && (edit.name !== undefined || edit.origin !== undefined)) {
      request.edit = edit;
    }
    if (migration && Object.keys(migration).length > 0) {
      request.migration = migration;
    }
    return JSON.stringify(request);
  }
  return JSON.stringify({ command });
}

type StatusResponse = { kind: 'status'; status: HotEditStatus };
type IdentityResponse = { kind: 'identity'; identity: IdentityInfo };
type AckResponse = { kind: 'ack' };
type ErrorResponse = { kind: 'error'; error: HotEditProtocolError };

/** The parsed answer to one command, before command-specific matching. */
export type HotEditResponse = StatusResponse | IdentityResponse | AckResponse | ErrorResponse;

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
  if (record.response === 'identity') {
    return { kind: 'identity', identity: parseIdentity(record) };
  }
  if (record.response === 'status') {
    return { kind: 'status', status: parseStatus(record) };
  }
  if (record.response === 'ack') {
    return { kind: 'ack' };
  }
  if (record.response === 'error') {
    const vCode = typeof record.vCode === 'string' ? record.vCode : null;
    const message = typeof record.message === 'string' ? record.message : 'unknown error';
    return { kind: 'error', error: new HotEditProtocolError(vCode, message, parsePairs(record.pairs)) };
  }
  throw new HotEditProtocolError(null, `invalid response line: ${line}`);
}

/**
 * Builds the [`TypeChangePair`] list from a parsed error response's optional
 * `pairs` field. Absent or null means "no details"; anything else must match
 * the wire shape, so a malformed payload surfaces as a protocol error rather
 * than a misread decision list.
 */
function parsePairs(value: unknown): TypeChangePair[] {
  if (value === undefined || value === null) {
    return [];
  }
  if (!Array.isArray(value)) {
    throw new HotEditProtocolError(null, 'error response carries a malformed pairs list');
  }
  return value.map(parsePair);
}

/** Builds one [`TypeChangePair`] from a parsed `pairs` entry. */
function parsePair(value: unknown): TypeChangePair {
  if (typeof value !== 'object' || value === null) {
    throw new HotEditProtocolError(null, 'error response carries a malformed pair');
  }
  const { uid, name, from, to, sizeEqual } = value as Record<string, unknown>;
  if (
    typeof uid !== 'number'
    || !Number.isInteger(uid)
    || uid < 0
    || (typeof name !== 'string' && name !== null && name !== undefined)
    || typeof from !== 'string'
    || typeof to !== 'string'
    || typeof sizeEqual !== 'boolean'
  ) {
    throw new HotEditProtocolError(null, 'error response carries a malformed pair');
  }
  return { uid, name: name ?? null, from, to, sizeEqual };
}

/** Builds a [`HotEditStatus`] from a parsed `status` response object. */
function parseStatus(record: Record<string, unknown>): HotEditStatus {
  if (typeof record.mode !== 'string') {
    throw new HotEditProtocolError(null, 'status response is missing the mode');
  }
  const pendingEdit = parsePendingEdit(record.pendingEdit);
  return {
    mode: record.mode,
    active: numberField(record, 'active'),
    normal: numberField(record, 'normal'),
    candidate: record.candidate === null || record.candidate === undefined ? null : numberField(record, 'candidate'),
    application: numberField(record, 'application'),
    migration: record.migration === true,
    rounds: numberField(record, 'rounds'),
    ...(pendingEdit !== undefined ? { pendingEdit } : {}),
  };
}

/** Reads a numeric field, refusing a malformed status payload. */
export function numberField(record: Record<string, unknown>, key: string): number {
  if (typeof record[key] !== 'number') {
    throw new HotEditProtocolError(null, `status response is missing ${key}`);
  }
  return record[key] as number;
}

/**
 * Builds an [`IdentityInfo`] from a parsed `identity` response object: the
 * protocol version (a higher number than this client speaks is refused, the
 * version negotiation the engineering connection specifies), the device
 * panel block, the application snapshot (the [`parseStatus`] vocabulary),
 * and the optional redundancy block. Unknown top-level blocks are ignored —
 * the field set grows by adding optional fields.
 */
function parseIdentity(record: Record<string, unknown>): IdentityInfo {
  if (typeof record.protocol !== 'number' || !Number.isInteger(record.protocol)) {
    throw new HotEditProtocolError(null, 'identity response is missing the protocol version');
  }
  if (record.protocol > SUPPORTED_SESSION_PROTOCOL) {
    throw new HotEditProtocolError(
      null,
      `device speaks session protocol ${record.protocol}, this client supports up to ${SUPPORTED_SESSION_PROTOCOL}`,
    );
  }
  const device = record.device;
  if (typeof device !== 'object' || device === null) {
    throw new HotEditProtocolError(null, 'identity response is missing the device block');
  }
  const deviceRecord = device as Record<string, unknown>;
  const application = record.application;
  if (typeof application !== 'object' || application === null) {
    throw new HotEditProtocolError(null, 'identity response is missing the application block');
  }
  return {
    protocol: record.protocol,
    device: {
      name: stringField(deviceRecord, 'name'),
      model: stringField(deviceRecord, 'model'),
      modification: stringField(deviceRecord, 'modification'),
      firmwareVersion: stringField(deviceRecord, 'firmwareVersion'),
    },
    application: parseStatus(application as Record<string, unknown>),
    redundancy: parseRedundancy(record.redundancy),
  };
}

/** Reads a string field, refusing a malformed block. */
export function stringField(record: Record<string, unknown>, key: string): string {
  if (typeof record[key] !== 'string') {
    throw new HotEditProtocolError(null, `response block is missing ${key}`);
  }
  return record[key] as string;
}

/** Builds the optional redundancy block; absent means standalone. */
function parseRedundancy(value: unknown): RedundancyIdentity | undefined {
  if (value === undefined || value === null) {
    return undefined;
  }
  if (typeof value !== 'object') {
    throw new HotEditProtocolError(null, 'identity response carries a malformed redundancy block');
  }
  const record = value as Record<string, unknown>;
  return {
    pairId: stringField(record, 'pairId'),
    role: stringField(record, 'role'),
    epoch: numberField(record, 'epoch'),
    sync: stringField(record, 'sync'),
    control: stringField(record, 'control'),
  };
}

/** Builds the optional pending-edit record of a `status` payload. */
function parsePendingEdit(value: unknown): PendingEditRecord | undefined {
  if (value === undefined || value === null) {
    return undefined;
  }
  if (typeof value !== 'object') {
    throw new HotEditProtocolError(null, 'status response carries a malformed pendingEdit record');
  }
  const record = value as Record<string, unknown>;
  const name = record.name === undefined ? null : stringField(record, 'name');
  const origin = record.origin === undefined ? null : stringField(record, 'origin');
  const baseline = record.baseline;
  if (typeof baseline !== 'object' || baseline === null) {
    throw new HotEditProtocolError(null, 'pendingEdit record is missing the baseline block');
  }
  const baselineRecord = baseline as Record<string, unknown>;
  const contentHash = baselineRecord.contentHash;
  if (!Array.isArray(contentHash) || !contentHash.every(item => typeof item === 'number')) {
    throw new HotEditProtocolError(null, 'pendingEdit baseline carries a malformed content hash');
  }
  return {
    name,
    origin,
    acceptedAt: numberField(record, 'acceptedAt'),
    baseline: {
      normalGeneration: numberField(baselineRecord, 'normalGeneration'),
      contentHash,
    },
  };
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
  resolve(line: string): void;
  reject(reason: Error): void;
}

/**
 * A client session over one `ironplcvm serve` process. Requests are matched
 * to responses in order: the serve session answers every command line with
 * exactly one response line, so a FIFO of pending requests is sufficient and
 * pipelined callers stay correct. The FIFO resolves the raw response line
 * and each caller parses it — the hot-edit commands through
 * `parseResponseLine`, the HA engineering commands through
 * `parseHaResponseLine` — so both vocabularies ride one session without a
 * per-layer dispatch.
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

  /**
   * The engineering-connection handshake (ADR-0063): the first command on
   * every (re)opened transport. Fills the device panel and carries the
   * application-state snapshot the baseline check compares.
   */
  async identity(): Promise<IdentityInfo> {
    const response = await this.request('identity');
    if (response.kind !== 'identity') {
      throw new HotEditProtocolError(null, `unexpected ${response.kind} response to identity`);
    }
    return response.identity;
  }

  /** Asks the host for its hot-edit status. */
  async getStatus(): Promise<HotEditStatus> {
    const response = await this.request('getStatus');
    if (response.kind !== 'status') {
      throw new HotEditProtocolError(null, `unexpected ${response.kind} response to getStatus`);
    }
    return response.status;
  }

  /**
   * Stages the compiled container `program` as the edit candidate, resolving
   * out-of-policy type changes with the engineer's `migration` decisions
   * (ADR 0061). Without decisions the host refuses with V4010 and a `pairs`
   * list on the thrown [`HotEditProtocolError`]. The optional `edit`
   * identity (ADR-0064) labels the pending-edit record the status reports.
   */
  async acceptEdits(program: Uint8Array, migration?: MigrationDecisionMap, edit?: EditIdentity): Promise<void> {
    await this.request('acceptEdits', program, migration, edit);
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

  /** The HA pair overview (haStatus): both units' chart states, the
   * TakeoverReady verdict with its inputs, and the active alarm flags. */
  async haStatus(): Promise<HaStatus> {
    const response = await this.haRequest('haStatus', 'haStatus');
    return response.status;
  }

  /** The HA calibration state (haCalibration). */
  async haCalibration(): Promise<HaCalibration> {
    const response = await this.haRequest('haCalibration', 'haCalibration');
    return response.calibration;
  }

  /** The HA ownership barrier view (haBarrier). */
  async haBarrier(): Promise<HaBarrier> {
    const response = await this.haRequest('haBarrier', 'haBarrier');
    return response.barrier;
  }

  /** The HA IO_READY breakdown (haIoReady). */
  async haIoReady(): Promise<HaIoReady> {
    const response = await this.haRequest('haIoReady', 'haIoReady');
    return response.ioReady;
  }

  /** The HA timing budget (haTimingBudget). */
  async haTimingBudget(): Promise<HaTimingBudget> {
    const response = await this.haRequest('haTimingBudget', 'haTimingBudget');
    return response.budget;
  }

  /** The HA event ring (haEvents). */
  async haEvents(): Promise<HaEvents> {
    const response = await this.haRequest('haEvents', 'haEvents');
    return response.events;
  }

  /**
   * The commanded Primary↔Secondary swap (haCommandedSwap). The host
   * refuses outside SYNC_READY with V4108; the refusal (and the V4107
   * barrier failure) surfaces as a coded `HotEditProtocolError`.
   */
  async haCommandedSwap(): Promise<void> {
    await this.haRequest('haCommandedSwap', 'ack');
  }

  /** Runs a calibration run (haRunCalibration). */
  async haRunCalibration(): Promise<void> {
    await this.haRequest('haRunCalibration', 'ack');
  }

  /**
   * Sets the peer-failure confirmation time and the recovery budget
   * (haSetTimingBudget). A budget the installation cannot honor is
   * refused with V4111; the refusal message names the minimum
   * demonstrated budget the installation can honor.
   */
  async haSetTimingBudget(peerFailureConfirmation: number, recoveryBudget: number): Promise<void> {
    await this.haRequest('haSetTimingBudget', 'ack', { peerFailureConfirmation, recoveryBudget });
  }

  /** Rejects pending requests; the transport itself is owned by the glue. */
  dispose(): void {
    this.rejectPending(new HotEditProtocolError(null, 'the hot edit session has ended.'));
  }

  /**
   * Sends one HA command and matches the answer's kind: a coded refusal
   * throws `HotEditProtocolError` with the server's V-code, and an
   * answer of the wrong kind is the same "unexpected response" error the
   * hot-edit methods raise.
   */
  private async haRequest<K extends HaResponse['kind']>(
    command: HaCommandName,
    kind: K,
    params?: { peerFailureConfirmation: number; recoveryBudget: number },
  ): Promise<Extract<HaResponse, { kind: K }>> {
    const line = await this.requestRaw(encodeHaCommand(command, params));
    let response: HaResponse;
    try {
      response = parseHaResponseLine(line);
    }
    catch (err) {
      const error = err instanceof Error ? err : new Error(String(err));
      this.rejectPending(error);
      throw error;
    }
    if (response.kind === 'error') {
      throw new HotEditProtocolError(response.error.vCode, response.error.message);
    }
    if (response.kind !== kind) {
      throw new HotEditProtocolError(null, `unexpected ${response.kind} response to ${command}`);
    }
    return response as Extract<HaResponse, { kind: K }>;
  }

  private async request(
    command: HotEditCommand,
    program?: Uint8Array,
    migration?: MigrationDecisionMap,
    edit?: EditIdentity,
  ): Promise<HotEditResponse> {
    const line = await this.requestRaw(encodeRequest(command, program, migration, edit));
    let parsed: HotEditResponse;
    try {
      parsed = parseResponseLine(line);
    }
    catch (err) {
      // The wire desynced: no pending request can be matched reliably.
      const error = err instanceof Error ? err : new Error(String(err));
      this.rejectPending(error);
      throw error;
    }
    if (parsed.kind === 'error') {
      throw parsed.error;
    }
    return parsed;
  }

  /** Sends one line and resolves with the raw response line, in order. */
  private async requestRaw(line: string): Promise<string> {
    if (this.exited) {
      throw new HotEditProtocolError(null, 'the hot edit session has ended.');
    }
    const response = new Promise<string>((resolve, reject) => {
      this.pending.push({ resolve, reject });
    });
    this.transport.sendLine(line);
    return response;
  }

  private handleLine(line: string): void {
    const pending = this.pending.shift();
    if (!pending) {
      // The serve session sends nothing unasked; drop the line defensively.
      return;
    }
    pending.resolve(line);
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
