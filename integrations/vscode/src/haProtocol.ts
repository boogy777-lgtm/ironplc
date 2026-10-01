/**
 * The HA engineering protocol of the redundancy shell
 * (`specs/design/ha-engineering-ui.md`): the wire types of the six
 * queries (`haStatus`, `haCalibration`, `haBarrier`, `haIoReady`,
 * `haTimingBudget`, `haEvents`) and the three engineer actions
 * (`haCommandedSwap`, `haRunCalibration`, `haSetTimingBudget`), with
 * strict parsers following the `hotEditSession` parsing idiom — a
 * malformed payload throws `HotEditProtocolError` (no V-code) rather
 * than misrendering. The module is vscode-free and unit-testable; the
 * `HotEditSession` HA methods send these lines over the same
 * one-line-in/one-line-out transport the hot-edit commands use.
 */

import { HotEditProtocolError, numberField, stringField } from './hotEditSession';

/** The commands of the HA engineering protocol, one per `command` tag. */
export type HaCommandName
  = | 'haStatus'
    | 'haCalibration'
    | 'haBarrier'
    | 'haIoReady'
    | 'haTimingBudget'
    | 'haEvents'
    | 'haCommandedSwap'
    | 'haRunCalibration'
    | 'haSetTimingBudget';

/**
 * Renders one HA command as a single line of JSON, without a trailing
 * newline. `haSetTimingBudget` is the only command with parameters; the
 * others are tag-only lines.
 */
export function encodeHaCommand(
  command: HaCommandName,
  params?: { peerFailureConfirmation: number; recoveryBudget: number },
): string {
  if (command === 'haSetTimingBudget') {
    return JSON.stringify({
      command,
      peerFailureConfirmation: params?.peerFailureConfirmation ?? 0,
      recoveryBudget: params?.recoveryBudget ?? 0,
    });
  }
  return JSON.stringify({ command });
}

/** One measured term: current / min / EMA10 / EMA100 / max / count. */
export interface HaTermStats {
  current: number;
  min: number;
  ema10: number;
  ema100: number;
  max: number;
  count: number;
}

/** One unit's block of the pair overview. */
export interface HaUnit {
  /** The configured role (`primary` / `secondary`); absent standalone. */
  role?: string;
  /** The permanent ControllerId. */
  controllerId: number;
  /** The SYNC substate; absent standalone. */
  sync?: string;
  /** The guard-relevant reason of the last deSYNC entry. */
  syncReason?: string;
  /** The CONTROL substate; absent standalone. */
  control?: string;
  /** The latched CONTROL alarm, if any. */
  alarm?: string;
  /** The unit's current epoch. */
  epoch: number;
}

/** The active alarm flags of the pair. */
export interface HaAlarmFlags {
  performanceDegraded: boolean;
  timingGuaranteeLost: boolean;
  redundancyLost: boolean;
}

/** The `haStatus` payload: the pair overview. */
export interface HaStatus {
  standalone: boolean;
  pairId?: string;
  local: HaUnit;
  peer?: HaUnit;
  epoch?: number;
  applicationGeneration: number;
  stateGeneration: number;
  takeoverReady: boolean;
  syncReady: boolean;
  ioReady: boolean;
  linkValid: boolean;
  alarms: HaAlarmFlags;
}

/** The per-direction link profile of the calibration view. */
export interface HaDirection {
  rtt: HaTermStats;
  processing: HaTermStats;
  lossRatePercent: number;
  maxConsecutiveLoss: number;
  pingsSent: number;
  pongsReceived: number;
}

/** The `haCalibration` payload. */
export interface HaCalibration {
  state: string;
  linkValid: boolean;
  takeoverReady: boolean;
  ab: HaDirection;
  ba: HaDirection;
  peerDetect: HaTermStats;
  leaseExpiry: number;
  claimStart: number;
  scan: HaTermStats;
  baselineRttMax?: number;
  recalibrations: number;
  lastInvalidation?: string;
}

/** One module's row of the barrier view. */
export interface HaModuleBarrier {
  module: number;
  profile: string;
  online: boolean;
  ownerState: string;
  owner?: number;
  ownerEpoch?: number;
  ownerArmed: boolean;
  claim: HaTermStats;
  arm: HaTermStats;
  outputApply: HaTermStats;
}

/** The `haBarrier` payload. */
export interface HaBarrier {
  modules: HaModuleBarrier[];
  limitingDevice?: number;
  worstOwnershipRecovery?: number;
}

/** The `haIoReady` payload: the IO_READY breakdown. */
export interface HaIoReady {
  requiredInputsObservable: boolean;
  standbyConnectionsValid: boolean;
  configsMatch: boolean;
  epochsValid: boolean;
  failingItem?: string;
}

/** One failover-formula term: the qualification bound vs. measured. */
export interface HaTerm {
  bound: number;
  current: number;
  ema10: number;
}

/** The phase-aware safe-point term (the scan statistics + phase). */
export interface HaScanTerm {
  bound: number;
  current: number;
  ema10: number;
  ema100: number;
  min: number;
  max: number;
  phase: number;
}

/** The budget-check verdict. */
export interface HaBudgetVerdict {
  qualified: boolean;
  minimumDemonstrated?: number;
}

/** The `haTimingBudget` payload: the honest failover estimate. */
export interface HaTimingBudget {
  peerFailureConfirmation: number;
  recoveryBudget: number;
  peerDetect: HaTerm;
  claim: HaTerm;
  arm: HaTerm;
  scan: HaScanTerm;
  outputApply: HaTerm;
  calculatedWorstCase?: number;
  predictedIfNow?: number;
  verdict?: HaBudgetVerdict;
}

/** One timestamped event of the ring. */
export interface HaEvent {
  tick: number;
  kind: string;
  detail?: string;
}

/** The `haEvents` payload: the bounded ring plus the ever-recorded count. */
export interface HaEvents {
  count: number;
  events: HaEvent[];
}

/** Why an HA command failed, as the wire carries it. */
export interface HaError {
  vCode: string | null;
  message: string;
  /** The minimum demonstrated budget a V4111 refusal carries. */
  minimumDemonstrated?: number;
}

/** The parsed answer to one HA command, before command-specific matching. */
export type HaResponse
  = | { kind: 'haStatus'; status: HaStatus }
    | { kind: 'haCalibration'; calibration: HaCalibration }
    | { kind: 'haBarrier'; barrier: HaBarrier }
    | { kind: 'haIoReady'; ioReady: HaIoReady }
    | { kind: 'haTimingBudget'; budget: HaTimingBudget }
    | { kind: 'haEvents'; events: HaEvents }
    | { kind: 'ack' }
    | { kind: 'error'; error: HaError };

/**
 * Parses one HA response line (a single JSON value without its trailing
 * newline). Throws `HotEditProtocolError` when the line is not a
 * well-formed HA response — the session answers every command line with
 * exactly one response line, so a malformed one means the wire desynced
 * or the server predates the HA surface.
 */
export function parseHaResponseLine(line: string): HaResponse {
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
  switch (record.response) {
    case 'haStatus':
      return { kind: 'haStatus', status: parseStatus(record) };
    case 'haCalibration':
      return { kind: 'haCalibration', calibration: parseCalibration(record) };
    case 'haBarrier':
      return { kind: 'haBarrier', barrier: parseBarrier(record) };
    case 'haIoReady':
      return { kind: 'haIoReady', ioReady: parseIoReady(record) };
    case 'haTimingBudget':
      return { kind: 'haTimingBudget', budget: parseTimingBudget(record) };
    case 'haEvents':
      return { kind: 'haEvents', events: parseEvents(record) };
    case 'ack':
      return { kind: 'ack' };
    case 'error':
      return { kind: 'error', error: parseError(record) };
    default:
      throw new HotEditProtocolError(null, `invalid response line: ${line}`);
  }
}

/** Builds one measured-term block, refusing a malformed payload. */
function parseTermStats(value: unknown, what: string): HaTermStats {
  if (typeof value !== 'object' || value === null) {
    throw new HotEditProtocolError(null, `${what} is missing the term statistics`);
  }
  const record = value as Record<string, unknown>;
  return {
    current: numberField(record, 'current'),
    min: numberField(record, 'min'),
    ema10: numberField(record, 'ema10'),
    ema100: numberField(record, 'ema100'),
    max: numberField(record, 'max'),
    count: numberField(record, 'count'),
  };
}

/** Builds one unit block of the pair overview. */
function parseUnit(value: unknown, what: string): HaUnit {
  if (typeof value !== 'object' || value === null) {
    throw new HotEditProtocolError(null, `${what} is missing the unit block`);
  }
  const record = value as Record<string, unknown>;
  const unit: HaUnit = {
    controllerId: numberField(record, 'controllerId'),
    epoch: numberField(record, 'epoch'),
  };
  if (record.role !== undefined) {
    unit.role = stringField(record, 'role');
  }
  if (record.sync !== undefined) {
    unit.sync = stringField(record, 'sync');
    if (record.syncReason !== undefined) {
      unit.syncReason = stringField(record, 'syncReason');
    }
  }
  if (record.control !== undefined) {
    unit.control = stringField(record, 'control');
  }
  if (record.alarm !== undefined) {
    unit.alarm = stringField(record, 'alarm');
  }
  return unit;
}

/** Builds the pair overview from a parsed `haStatus` record. */
function parseStatus(record: Record<string, unknown>): HaStatus {
  if (record.standalone !== true && record.standalone !== false) {
    throw new HotEditProtocolError(null, 'haStatus response is missing the standalone flag');
  }
  const status: HaStatus = {
    standalone: record.standalone,
    local: parseUnit(record.local, 'haStatus response'),
    applicationGeneration: numberField(record, 'applicationGeneration'),
    stateGeneration: numberField(record, 'stateGeneration'),
    takeoverReady: record.takeoverReady === true,
    syncReady: record.syncReady === true,
    ioReady: record.ioReady === true,
    linkValid: record.linkValid === true,
    alarms: parseAlarms(record.alarms),
  };
  if (record.pairId !== undefined) {
    status.pairId = stringField(record, 'pairId');
  }
  if (record.peer !== undefined) {
    status.peer = parseUnit(record.peer, 'haStatus response');
  }
  if (record.epoch !== undefined) {
    status.epoch = numberField(record, 'epoch');
  }
  return status;
}

function parseAlarms(value: unknown): HaAlarmFlags {
  if (typeof value !== 'object' || value === null) {
    throw new HotEditProtocolError(null, 'haStatus response is missing the alarms block');
  }
  const record = value as Record<string, unknown>;
  return {
    performanceDegraded: record.performanceDegraded === true,
    timingGuaranteeLost: record.timingGuaranteeLost === true,
    redundancyLost: record.redundancyLost === true,
  };
}

/** Builds one per-direction link profile. */
function parseDirection(value: unknown, what: string): HaDirection {
  if (typeof value !== 'object' || value === null) {
    throw new HotEditProtocolError(null, `${what} is missing the direction block`);
  }
  const record = value as Record<string, unknown>;
  return {
    rtt: parseTermStats(record.rtt, what),
    processing: parseTermStats(record.processing, what),
    lossRatePercent: numberField(record, 'lossRatePercent'),
    maxConsecutiveLoss: numberField(record, 'maxConsecutiveLoss'),
    pingsSent: numberField(record, 'pingsSent'),
    pongsReceived: numberField(record, 'pongsReceived'),
  };
}

/** Builds the calibration payload from a parsed `haCalibration` record. */
function parseCalibration(record: Record<string, unknown>): HaCalibration {
  const calibration: HaCalibration = {
    state: stringField(record, 'state'),
    linkValid: record.linkValid === true,
    takeoverReady: record.takeoverReady === true,
    ab: parseDirection(record.ab, 'haCalibration response'),
    ba: parseDirection(record.ba, 'haCalibration response'),
    peerDetect: parseTermStats(record.peerDetect, 'haCalibration response'),
    leaseExpiry: numberField(record, 'leaseExpiry'),
    claimStart: numberField(record, 'claimStart'),
    scan: parseTermStats(record.scan, 'haCalibration response'),
    recalibrations: numberField(record, 'recalibrations'),
  };
  if (record.baselineRttMax !== undefined) {
    calibration.baselineRttMax = numberField(record, 'baselineRttMax');
  }
  if (record.lastInvalidation !== undefined) {
    calibration.lastInvalidation = stringField(record, 'lastInvalidation');
  }
  return calibration;
}

/** Builds the barrier payload from a parsed `haBarrier` record. */
function parseBarrier(record: Record<string, unknown>): HaBarrier {
  if (!Array.isArray(record.modules)) {
    throw new HotEditProtocolError(null, 'haBarrier response is missing the modules list');
  }
  const barrier: HaBarrier = {
    modules: record.modules.map(parseModuleBarrier),
  };
  if (record.limitingDevice !== undefined) {
    barrier.limitingDevice = numberField(record, 'limitingDevice');
  }
  if (record.worstOwnershipRecovery !== undefined) {
    barrier.worstOwnershipRecovery = numberField(record, 'worstOwnershipRecovery');
  }
  return barrier;
}

function parseModuleBarrier(value: unknown): HaModuleBarrier {
  if (typeof value !== 'object' || value === null) {
    throw new HotEditProtocolError(null, 'haBarrier response carries a malformed module row');
  }
  const record = value as Record<string, unknown>;
  const row: HaModuleBarrier = {
    module: numberField(record, 'module'),
    profile: stringField(record, 'profile'),
    online: record.online === true,
    ownerState: stringField(record, 'ownerState'),
    ownerArmed: record.ownerArmed === true,
    claim: parseTermStats(record.claim, 'haBarrier module row'),
    arm: parseTermStats(record.arm, 'haBarrier module row'),
    outputApply: parseTermStats(record.outputApply, 'haBarrier module row'),
  };
  if (record.owner !== undefined) {
    row.owner = numberField(record, 'owner');
  }
  if (record.ownerEpoch !== undefined) {
    row.ownerEpoch = numberField(record, 'ownerEpoch');
  }
  return row;
}

/** Builds the IO_READY breakdown from a parsed `haIoReady` record. */
function parseIoReady(record: Record<string, unknown>): HaIoReady {
  const ioReady: HaIoReady = {
    requiredInputsObservable: record.requiredInputsObservable === true,
    standbyConnectionsValid: record.standbyConnectionsValid === true,
    configsMatch: record.configsMatch === true,
    epochsValid: record.epochsValid === true,
  };
  if (record.failingItem !== undefined) {
    ioReady.failingItem = stringField(record, 'failingItem');
  }
  return ioReady;
}

/** Builds one failover-formula term. */
function parseTerm(value: unknown, what: string): HaTerm {
  if (typeof value !== 'object' || value === null) {
    throw new HotEditProtocolError(null, `${what} is missing the term block`);
  }
  const record = value as Record<string, unknown>;
  return {
    bound: numberField(record, 'bound'),
    current: numberField(record, 'current'),
    ema10: numberField(record, 'ema10'),
  };
}

/** Builds the safe-point term from a parsed `haTimingBudget` record. */
function parseScanTerm(value: unknown, what: string): HaScanTerm {
  if (typeof value !== 'object' || value === null) {
    throw new HotEditProtocolError(null, `${what} is missing the scan term block`);
  }
  const record = value as Record<string, unknown>;
  return {
    bound: numberField(record, 'bound'),
    current: numberField(record, 'current'),
    ema10: numberField(record, 'ema10'),
    ema100: numberField(record, 'ema100'),
    min: numberField(record, 'min'),
    max: numberField(record, 'max'),
    phase: numberField(record, 'phase'),
  };
}

/** Builds the timing-budget payload from a parsed `haTimingBudget` record. */
function parseTimingBudget(record: Record<string, unknown>): HaTimingBudget {
  const budget: HaTimingBudget = {
    peerFailureConfirmation: numberField(record, 'peerFailureConfirmation'),
    recoveryBudget: numberField(record, 'recoveryBudget'),
    peerDetect: parseTerm(record.peerDetect, 'haTimingBudget response'),
    claim: parseTerm(record.claim, 'haTimingBudget response'),
    arm: parseTerm(record.arm, 'haTimingBudget response'),
    scan: parseScanTerm(record.scan, 'haTimingBudget response'),
    outputApply: parseTerm(record.outputApply, 'haTimingBudget response'),
  };
  if (record.calculatedWorstCase !== undefined) {
    budget.calculatedWorstCase = numberField(record, 'calculatedWorstCase');
  }
  if (record.predictedIfNow !== undefined) {
    budget.predictedIfNow = numberField(record, 'predictedIfNow');
  }
  if (record.verdict !== undefined) {
    budget.verdict = parseVerdict(record.verdict);
  }
  return budget;
}

function parseVerdict(value: unknown): HaBudgetVerdict {
  if (typeof value !== 'object' || value === null) {
    throw new HotEditProtocolError(null, 'haTimingBudget response carries a malformed verdict');
  }
  const record = value as Record<string, unknown>;
  const verdict: HaBudgetVerdict = { qualified: record.qualified === true };
  if (record.minimumDemonstrated !== undefined) {
    verdict.minimumDemonstrated = numberField(record, 'minimumDemonstrated');
  }
  return verdict;
}

/** Builds the events payload from a parsed `haEvents` record. */
function parseEvents(record: Record<string, unknown>): HaEvents {
  if (!Array.isArray(record.events)) {
    throw new HotEditProtocolError(null, 'haEvents response is missing the events list');
  }
  return {
    count: numberField(record, 'count'),
    events: record.events.map(parseEvent),
  };
}

function parseEvent(value: unknown): HaEvent {
  if (typeof value !== 'object' || value === null) {
    throw new HotEditProtocolError(null, 'haEvents response carries a malformed event');
  }
  const record = value as Record<string, unknown>;
  const event: HaEvent = { tick: numberField(record, 'tick'), kind: stringField(record, 'kind') };
  if (record.detail !== undefined) {
    event.detail = stringField(record, 'detail');
  }
  return event;
}

/** Builds the coded error, mirroring the hot-edit error shape plus the
 * HA-only `minimumDemonstrated` detail a V4111 refusal carries. */
function parseError(record: Record<string, unknown>): HaError {
  const error: HaError = {
    vCode: typeof record.vCode === 'string' ? record.vCode : null,
    message: typeof record.message === 'string' ? record.message : 'unknown error',
  };
  if (record.minimumDemonstrated !== undefined) {
    error.minimumDemonstrated = numberField(record, 'minimumDemonstrated');
  }
  return error;
}
