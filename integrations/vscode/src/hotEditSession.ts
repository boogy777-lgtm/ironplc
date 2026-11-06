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
import { SUPPORTED_SESSION_PROTOCOL } from './sessionConstants';
import { LineRequestChannel, LineTransport } from './vscodeCommon';

/** The commands of the hot-edit protocol, one per `command` tag on the wire. */
export type HotEditCommand
  = | 'identity'
    | 'getStatus'
    | 'acceptEdits'
    | 'testEdits'
    | 'untestEdits'
    | 'assembleEdits'
    | 'cancelEdits'
    | 'startScan'
    | 'stopScan';

/**
 * Whether the device executes scan rounds (ADR-0086, Run and Stop). A device
 * that faults reports `running` with a [`DeviceFault`]: the fault is reported
 * apart from the run state.
 */
export type RunState = 'running' | 'stopped';

/** The fault block of an `identity` response: why the device stopped scanning by itself. */
export interface DeviceFault {
  vCode: string;
  message: string;
  taskId: number;
  instanceId: number;
}

/** One engineer decision for a shared-UID type change (ADR 0061). */
export type MigrationDecision = 'init' | 'preserve';

/**
 * The wire map for `pairs`: one decision per shared
 * UID, keyed by the UID's string form, e.g. `{"1":"preserve"}`.
 */
export type MigrationDecisionMap = Record<string, MigrationDecision>;

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

/**
 * How an accepted candidate takes over the running application (ADR-0088): the
 * optional `mode` of `acceptEdits`. `change` is the default and is absent from
 * the line; `replace` is a Download. `migrate` is not a mode a client chooses:
 * the device selects it inside `change`.
 */
export type AcceptMode = 'change' | 'replace';

/** How the staged candidate takes over, as the status reports it (`candidateMode`). */
export type CandidateMode = AcceptMode | 'migrate';

/**
 * Why a `PERSISTENT` variable is re-initialised by a replace (ADR-0088), the
 * `reason` of an entry of the `accepted` answer.
 */
export type ReinitReason
  = | 'noStableId'
    | 'notInCandidate'
    | 'qualifierChanged'
    | 'typeChanged'
    | 'unsupportedType';

/** One `PERSISTENT` variable that a replace re-initialises: which one, and why. */
export interface Reinitialised {
  /** The stable variable ID, when the running application has one. */
  uid?: number;
  /** The variable's name, when the device knows it. */
  name?: string;
  reason: ReinitReason;
}

/** A refusal reason of the edit view (ADR-0087): the V-code and its text. */
export interface EditReason {
  code: string;
  text: string;
}

/** One command of the edit view: whether the device accepts it now, and why not. */
export interface EditCommandView {
  enabled: boolean;
  label: string;
  /** Present when `enabled` is false. */
  reason?: EditReason;
  /**
   * The commands that make this one up: what the device runs for a command
   * (Cancel under Test), or what the client runs for `finalizeAll`.
   */
  steps?: readonly string[];
}

/**
 * The edit view of a status (ADR-0087): which edit commands the device allows
 * now, decided by the device from its own transition table. A client shows it
 * and computes no enablement. The keys of `commands` are the wire names of the
 * edit commands and `finalizeAll`; a command a later device adds is one more key.
 */
export interface EditView {
  /**
   * Where the edit stands: `idle`, `accepted`, `testing`, `testingMigration`,
   * `testingReplace` or `swapPending`. A stage a client does not know is read
   * through `commands` and `next`, which are the contract.
   */
  stage: string;
  commands: Readonly<Record<string, EditCommandView>>;
  /** The command that moves the edit one stage on; absent while a swap is pending. */
  next?: { command: string; label: string };
  /** What the engineer should know about the edit as it stands. */
  warning?: string;
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
  /**
   * The `content_hash` of the header of the normal (committed) artifact, as 64
   * lowercase hexadecimal digits: what the device runs, by content. Optional
   * on the wire; a device that predates it omits it. Unlike the generation
   * counters it survives a device restart.
   */
  contentHash?: string;
  /** The `layout_hash` of the normal artifact, under the same rules as `contentHash`. */
  layoutHash?: string;
  /**
   * How the staged candidate takes over the state (ADR-0088). Absent when no
   * candidate is staged and from a device that predates the field.
   */
  candidateMode?: CandidateMode;
  /**
   * Which edit commands the device allows now (ADR-0087). Absent from a device
   * that predates the field: a client then has no view to follow.
   */
  edit?: EditView;
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
  /**
   * The device's unique id: 32 lowercase hexadecimal digits that the device
   * generates once and keeps. Absent from a device that has none and from a
   * device that predates the field.
   */
  deviceId?: string;
  name: string;
  model: string;
  modification: string;
  runtimeVersion: string;
}

/** The optional redundancy block of an `identity` response (ADR-0063). */
export interface RedundancyIdentity {
  pairId: string;
  role: string;
  /** The 64-bit ownership epoch as a JSON number, exact up to 2^53 (ADR-0062). */
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
  /**
   * Whether the device scans. Absent from a device that predates the field,
   * which runs; read it through `runStateOf`, which says so once.
   */
  runState?: RunState;
  /** Present while the device is faulted. */
  fault?: DeviceFault;
}

/** The run state of an identity: an absent field is `running`, what a device that predates it does. */
export function runStateOf(identity: IdentityInfo): RunState {
  return identity.runState ?? 'running';
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
 * A device answered an accept in the `replace` mode as it answers an ordinary
 * accept: it does not know the mode, and the candidate it staged is a change.
 * The caller discards that candidate.
 */
export class ReplaceNotSupportedError extends HotEditProtocolError {
  constructor() {
    super(null, 'the device does not support a Download: it answered the accept as an ordinary edit');
    this.name = 'ReplaceNotSupportedError';
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
  mode: AcceptMode = 'change',
): string {
  if (command === 'acceptEdits') {
    const request: Record<string, unknown> = { command, program: Array.from(program ?? []) };
    // The default mode is absent from the line, as the device writes it.
    if (mode !== 'change') {
      request.mode = mode;
    }
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
type AcceptedResponse = { kind: 'accepted'; reinitialised: Reinitialised[] };
type RunStateResponse = { kind: 'runState'; runState: RunState };
type ErrorResponse = { kind: 'error'; error: HotEditProtocolError };

/** The parsed answer to one command, before command-specific matching. */
export type HotEditResponse
  = | StatusResponse
    | IdentityResponse
    | AckResponse
    | AcceptedResponse
    | RunStateResponse
    | ErrorResponse;

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
  if (record.response === 'accepted') {
    return { kind: 'accepted', reinitialised: parseReinitialised(record.reinitialised) };
  }
  if (record.response === 'runState') {
    const runState = parseRunState(record.runState);
    if (runState === undefined) {
      throw new HotEditProtocolError(null, 'runState response is missing the run state');
    }
    return { kind: 'runState', runState };
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

/**
 * Builds the list of an `accepted` answer. The list is always present on the
 * wire (empty when every variable is kept); a reason this client does not know
 * is kept as it is, so a later device's reason is shown and not hidden.
 */
function parseReinitialised(value: unknown): Reinitialised[] {
  if (!Array.isArray(value)) {
    throw new HotEditProtocolError(null, 'accepted response is missing the reinitialised list');
  }
  return value.map((entry: unknown): Reinitialised => {
    if (typeof entry !== 'object' || entry === null) {
      throw new HotEditProtocolError(null, 'accepted response carries a malformed reinitialised entry');
    }
    const { uid, name, reason } = entry as Record<string, unknown>;
    if (
      typeof reason !== 'string'
      || (uid !== undefined && (typeof uid !== 'number' || !Number.isInteger(uid) || uid < 0))
      || (name !== undefined && typeof name !== 'string')
    ) {
      throw new HotEditProtocolError(null, 'accepted response carries a malformed reinitialised entry');
    }
    return {
      ...(uid !== undefined ? { uid: uid as number } : {}),
      ...(name !== undefined ? { name: name as string } : {}),
      reason: reason as ReinitReason,
    };
  });
}

/** Reads the optional `candidateMode` of a status payload; a mode this client does not know is refused. */
function parseCandidateMode(value: unknown): CandidateMode | undefined {
  if (value === undefined || value === null) {
    return undefined;
  }
  if (value !== 'change' && value !== 'migrate' && value !== 'replace') {
    throw new HotEditProtocolError(null, 'status response carries a malformed candidateMode');
  }
  return value;
}

/** Reads one command of the edit view. */
function parseEditCommand(value: unknown): EditCommandView {
  if (typeof value !== 'object' || value === null) {
    throw new HotEditProtocolError(null, 'status response carries a malformed edit command');
  }
  const { enabled, label, reason, steps } = value as Record<string, unknown>;
  if (typeof enabled !== 'boolean' || typeof label !== 'string') {
    throw new HotEditProtocolError(null, 'status response carries a malformed edit command');
  }
  let parsedReason: EditReason | undefined;
  if (reason !== undefined && reason !== null) {
    const { code, text } = reason as Record<string, unknown>;
    if (typeof code !== 'string' || typeof text !== 'string') {
      throw new HotEditProtocolError(null, 'status response carries a malformed edit reason');
    }
    parsedReason = { code, text };
  }
  if (steps !== undefined && (!Array.isArray(steps) || !steps.every(step => typeof step === 'string'))) {
    throw new HotEditProtocolError(null, 'status response carries a malformed edit command');
  }
  return {
    enabled,
    label,
    ...(parsedReason !== undefined ? { reason: parsedReason } : {}),
    ...(steps !== undefined ? { steps: steps as string[] } : {}),
  };
}

/**
 * Reads the optional `edit` view of a status payload (ADR-0087). Absent means
 * a device that predates it. The stage and the command names are not checked
 * against a list: the view grows by a row, and a client reads what is there.
 */
function parseEditView(value: unknown): EditView | undefined {
  if (value === undefined || value === null) {
    return undefined;
  }
  if (typeof value !== 'object') {
    throw new HotEditProtocolError(null, 'status response carries a malformed edit view');
  }
  const { stage, commands, next, warning } = value as Record<string, unknown>;
  if (typeof stage !== 'string' || typeof commands !== 'object' || commands === null) {
    throw new HotEditProtocolError(null, 'status response carries a malformed edit view');
  }
  const parsed: Record<string, EditCommandView> = {};
  for (const [name, command] of Object.entries(commands)) {
    parsed[name] = parseEditCommand(command);
  }
  let parsedNext: { command: string; label: string } | undefined;
  if (next !== undefined && next !== null) {
    const { command, label } = next as Record<string, unknown>;
    if (typeof command !== 'string' || typeof label !== 'string') {
      throw new HotEditProtocolError(null, 'status response carries a malformed edit view');
    }
    parsedNext = { command, label };
  }
  return {
    stage,
    commands: parsed,
    ...(parsedNext !== undefined ? { next: parsedNext } : {}),
    ...(typeof warning === 'string' ? { warning } : {}),
  };
}

/** Builds a [`HotEditStatus`] from a parsed `status` response object. */
function parseStatus(record: Record<string, unknown>): HotEditStatus {
  if (typeof record.mode !== 'string') {
    throw new HotEditProtocolError(null, 'status response is missing the mode');
  }
  const pendingEdit = parsePendingEdit(record.pendingEdit);
  const candidateMode = parseCandidateMode(record.candidateMode);
  const editView = parseEditView(record.edit);
  const contentHash = parseHashField(record, 'contentHash');
  const layoutHash = parseHashField(record, 'layoutHash');
  return {
    mode: record.mode,
    active: numberField(record, 'active'),
    normal: numberField(record, 'normal'),
    candidate: record.candidate === null || record.candidate === undefined ? null : numberField(record, 'candidate'),
    application: numberField(record, 'application'),
    migration: record.migration === true,
    rounds: numberField(record, 'rounds'),
    ...(pendingEdit !== undefined ? { pendingEdit } : {}),
    ...(contentHash !== undefined ? { contentHash } : {}),
    ...(layoutHash !== undefined ? { layoutHash } : {}),
    ...(candidateMode !== undefined ? { candidateMode } : {}),
    ...(editView !== undefined ? { edit: editView } : {}),
  };
}

/**
 * Reads an optional header hash of a status payload: absent means a device
 * that does not report it; present, it is 64 lowercase hexadecimal digits.
 */
function parseHashField(record: Record<string, unknown>, key: string): string | undefined {
  const value = record[key];
  if (value === undefined || value === null) {
    return undefined;
  }
  if (typeof value !== 'string' || !/^[0-9a-f]{64}$/.test(value)) {
    throw new HotEditProtocolError(null, `status response carries a malformed ${key}`);
  }
  return value;
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
 * protocol version (a higher number than `SUPPORTED_SESSION_PROTOCOL` is
 * refused, the version negotiation the engineering connection specifies; the
 * constant is generated from the file the runtime reads as
 * `SESSION_PROTOCOL_VERSION`, ADR-0063), the device
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
  const runState = parseRunState(record.runState);
  const fault = parseFault(record.fault);
  return {
    protocol: record.protocol,
    device: {
      ...parseDeviceId(deviceRecord),
      name: stringField(deviceRecord, 'name'),
      model: stringField(deviceRecord, 'model'),
      modification: stringField(deviceRecord, 'modification'),
      runtimeVersion: stringField(deviceRecord, 'runtimeVersion'),
    },
    application: parseStatus(application as Record<string, unknown>),
    redundancy: parseRedundancy(record.redundancy),
    ...(runState !== undefined ? { runState } : {}),
    ...(fault !== undefined ? { fault } : {}),
  };
}

/**
 * Reads an optional run state: absent means a device that does not report it,
 * so the result is undefined; present, it is one of the two states.
 */
function parseRunState(value: unknown): RunState | undefined {
  if (value === undefined || value === null) {
    return undefined;
  }
  if (value !== 'running' && value !== 'stopped') {
    throw new HotEditProtocolError(null, 'response carries a malformed runState');
  }
  return value;
}

/** Reads the optional fault block of an `identity` response; absent means no fault. */
function parseFault(value: unknown): DeviceFault | undefined {
  if (value === undefined || value === null) {
    return undefined;
  }
  if (typeof value !== 'object') {
    throw new HotEditProtocolError(null, 'identity response carries a malformed fault block');
  }
  const record = value as Record<string, unknown>;
  const { vCode, message, taskId, instanceId } = record;
  if (
    typeof vCode !== 'string'
    || typeof message !== 'string'
    || typeof taskId !== 'number'
    || typeof instanceId !== 'number'
  ) {
    throw new HotEditProtocolError(null, 'identity response carries a malformed fault block');
  }
  return { vCode, message, taskId, instanceId };
}

/**
 * Reads the optional `deviceId` of a device block: absent means a device that
 * reports none, so the key stays absent from the result; present, it is 32
 * lowercase hexadecimal digits.
 */
function parseDeviceId(device: Record<string, unknown>): { deviceId?: string } {
  const value = device.deviceId;
  if (value === undefined || value === null) {
    return {};
  }
  if (typeof value !== 'string' || !/^[0-9a-f]{32}$/.test(value)) {
    throw new HotEditProtocolError(null, 'identity response carries a malformed deviceId');
  }
  return { deviceId: value };
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
export type HotEditTransport = LineTransport;

/**
 * A client session over one `ironplcvm serve` process. Requests are matched
 * to responses in order by a `LineRequestChannel`: the serve session answers
 * every command line with exactly one response line, so pipelined callers stay
 * correct. The channel resolves the raw response line
 * and each caller parses it — the hot-edit commands through
 * `parseResponseLine`, the HA engineering commands through
 * `parseHaResponseLine` — so both vocabularies ride one session without a
 * per-layer dispatch.
 */
export class HotEditSession {
  private readonly channel: LineRequestChannel;

  constructor(
    transport: HotEditTransport,
    onSessionExit: () => void = () => {},
  ) {
    this.channel = new LineRequestChannel(transport, {
      ended: () => new HotEditProtocolError(null, 'the hot edit session has ended.'),
      exited: () => new HotEditProtocolError(null, 'the ironplcvm process exited.'),
    }, onSessionExit);
  }

  /** False once the peer has closed the channel. */
  get isActive(): boolean {
    return this.channel.isActive;
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

  /**
   * Stages the compiled container `program` in the `replace` mode (ADR-0088): a
   * Download. The answer lists the `PERSISTENT` variables that will be
   * re-initialised, empty when every one is kept. A device that does not know
   * the mode answers like an ordinary accept and has staged a change candidate
   * then, so the answer is [`ReplaceNotSupportedError`] and the caller discards
   * the candidate.
   */
  async acceptReplace(program: Uint8Array, edit?: EditIdentity): Promise<Reinitialised[]> {
    const response = await this.request('acceptEdits', program, undefined, edit, 'replace');
    if (response.kind !== 'accepted') {
      throw new ReplaceNotSupportedError();
    }
    return response.reinitialised;
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

  /**
   * Starts the scan of the device (ADR-0086). Answers the run state the
   * command left; a faulted device refuses with V6019 and a host without a
   * scan loop with V4022, as a coded `HotEditProtocolError`.
   */
  async startScan(): Promise<RunState> {
    return this.scanCommand('startScan');
  }

  /** Stops the scan of the device; answers the run state the command left. */
  async stopScan(): Promise<RunState> {
    return this.scanCommand('stopScan');
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
    this.channel.end();
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
    const line = await this.channel.request(encodeHaCommand(command, params));
    let response: HaResponse;
    try {
      response = parseHaResponseLine(line);
    }
    catch (err) {
      const error = err instanceof Error ? err : new Error(String(err));
      this.channel.rejectPending(error);
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

  /** Sends one scan command and returns the run state of its `runState` answer. */
  private async scanCommand(command: 'startScan' | 'stopScan'): Promise<RunState> {
    const response = await this.request(command);
    if (response.kind !== 'runState') {
      throw new HotEditProtocolError(null, `unexpected ${response.kind} response to ${command}`);
    }
    return response.runState;
  }

  private async request(
    command: HotEditCommand,
    program?: Uint8Array,
    migration?: MigrationDecisionMap,
    edit?: EditIdentity,
    mode?: AcceptMode,
  ): Promise<HotEditResponse> {
    const line = await this.channel.request(encodeRequest(command, program, migration, edit, mode));
    let parsed: HotEditResponse;
    try {
      parsed = parseResponseLine(line);
    }
    catch (err) {
      // The wire desynced: no pending request can be matched reliably.
      const error = err instanceof Error ? err : new Error(String(err));
      this.channel.rejectPending(error);
      throw error;
    }
    if (parsed.kind === 'error') {
      throw parsed.error;
    }
    return parsed;
  }
}
