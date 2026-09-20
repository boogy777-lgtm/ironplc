/**
 * The model behind the Redundant Pair Dashboard webview, ported from the
 * HA engineering contract (`specs/design/ha-engineering-ui.md`): the channel
 * truth table, the takeover barrier, death/resurrection with the zombie
 * rule, and the derived view model for the five contract sections.
 *
 * The module is vscode-free (unit-testable, like `syncUidsLogic`): the
 * webview provider owns the panel and the message loop, and this module
 * owns every decision. Transitions are synchronous — the demo shows the
 * resulting state and the event ledger, not an animation.
 *
 * ## The demo seam
 *
 * The dashboard normally renders the real data source. Until the HA runtime
 * exposes a pair surface, the provider serves [`emptyPairState`] ("no
 * redundant pair connected") and an explicitly marked DEMO MODE backed by
 * [`demoPairState`] + [`applyAction`]. Removing the demo when the runtime
 * surface lands deletes this module's demo functions and one branch in the
 * provider; nothing else references them.
 */

/** The two unit identities of a pair. */
export type UnitId = 'PLC-1' | 'PLC-2';
/** All units, in display order. */
export const UNITS: readonly UnitId[] = ['PLC-1', 'PLC-2'];

/** The SYNC sub-chart substates. */
export type SyncState = 'deSYNC' | 'SYNCING' | 'SYNC_READY';
/** The CONTROL chart substates, plus DEAD for a unit that is not running. */
export type ControlState = 'IDLE' | 'CLAIMING' | 'ACTIVE' | 'ACTIVE_DEGRADED' | 'REDUNDANCY_LOST' | 'DEAD';

/** Per-module ownership state; mirrors the roadmap's I/O firmware contract. */
export type ModuleOwnerState = 'ARMED' | 'CLAIMED_DISARMED' | 'SAFE' | 'FAILSAFE';

/** One required output module. */
export interface ModuleState {
  id: string;
  profile: 'preconnected' | 'reconnect';
  owner: UnitId;
  ownerState: ModuleOwnerState;
  armed: boolean;
  claimMs: number;
  claimMaxMs: number;
  packetAgeUs: number;
}

/** One timestamped entry of the event ledger. */
export interface PairEvent {
  at: string;
  severity: 'info' | 'warn' | 'alarm';
  message: string;
}

/** One channel's ping/pong liveness counters (+1 per exchange, +1000 per miss). */
export interface LivenessCounters {
  ping: number;
  pong: number;
  missing: number;
  penalty: number;
}

/** The full demo/real pair state the dashboard renders. */
export interface PairDashboardState {
  demo: boolean;
  connected: boolean;
  mode: 'redundant' | 'standalone';
  roles: Record<UnitId, 'Primary' | 'Secondary'>;
  admission: 'ADMITTED' | 'CALIBRATING' | 'UNQUALIFIED' | 'STANDALONE';
  permitOwner: UnitId | null;
  epoch: number;
  applicationGeneration: number;
  stateGeneration: number;
  sync: Record<UnitId, SyncState>;
  control: Record<UnitId, ControlState>;
  dead: Record<UnitId, boolean>;
  deadRole: Record<UnitId, 'owner' | 'standby' | null>;
  ownerUnreachable: boolean;
  redundancyLost: boolean;
  channels: { P: boolean; I: boolean };
  degraded: boolean;
  timingLost: boolean;
  calibration: 'CALIBRATED' | 'CALIBRATING' | 'UNQUALIFIED';
  counters: { ch1: LivenessCounters; ch2: LivenessCounters };
  modules: ModuleState[];
  confirmationMs: number;
  budgetMs: number;
  events: PairEvent[];
}

/** Every action the dashboard can request. */
export type DashboardAction
  = | 'swap'
    | 'primary-death'
    | 'secondary-death'
    | 'resurrect-primary'
    | 'resurrect-secondary'
    | 'kill-p'
    | 'kill-i'
    | 'restore-channels'
    | 'partition'
    | 'calibrate'
    | 'invalidate'
    | 'degrade'
    | 'reset';

/** The command that opens the dashboard (pinned by the unit tests). */
export const OPEN_DASHBOARD_COMMAND = 'ironplc.openRedundantPairDashboard';

/** The webview view type; one panel per editor area. */
export const DASHBOARD_VIEW_TYPE = 'ironplc.redundantPairDashboard';

const MODULES: Omit<ModuleState, 'owner' | 'ownerState' | 'armed' | 'packetAgeUs'>[] = [
  { id: 'DO-01', profile: 'preconnected', claimMs: 0.72, claimMaxMs: 1.74 },
  { id: 'DO-02', profile: 'preconnected', claimMs: 0.68, claimMaxMs: 1.68 },
  { id: 'DO-03', profile: 'preconnected', claimMs: 0.81, claimMaxMs: 1.91 },
  { id: 'DO-04', profile: 'preconnected', claimMs: 0.74, claimMaxMs: 1.83 },
  { id: 'DO-05', profile: 'preconnected', claimMs: 0.69, claimMaxMs: 1.70 },
  { id: 'DO-06', profile: 'preconnected', claimMs: 0.77, claimMaxMs: 1.79 },
  { id: 'DO-07', profile: 'reconnect', claimMs: 1.41, claimMaxMs: 8.40 },
  { id: 'DO-08', profile: 'preconnected', claimMs: 0.70, claimMaxMs: 1.72 },
];

const SCAN_MAX_MS = 11.80;
const SCAN_EMA_MS = 7.40;
const ARM_EMA_MS = 0.42;
const SCAN_PHASE = 0.62;

function counters(): LivenessCounters {
  return { ping: 0, pong: 0, missing: 0, penalty: 0 };
}

/** The honest real-mode state: no pair surface exists yet. */
export function emptyPairState(): PairDashboardState {
  return {
    demo: false,
    connected: false,
    mode: 'redundant',
    roles: { 'PLC-1': 'Primary', 'PLC-2': 'Secondary' },
    admission: 'ADMITTED',
    permitOwner: null,
    epoch: 0,
    applicationGeneration: 0,
    stateGeneration: 0,
    sync: { 'PLC-1': 'deSYNC', 'PLC-2': 'deSYNC' },
    control: { 'PLC-1': 'IDLE', 'PLC-2': 'IDLE' },
    dead: { 'PLC-1': false, 'PLC-2': false },
    deadRole: { 'PLC-1': null, 'PLC-2': null },
    ownerUnreachable: false,
    redundancyLost: false,
    channels: { P: false, I: false },
    degraded: false,
    timingLost: false,
    calibration: 'UNQUALIFIED',
    counters: { ch1: counters(), ch2: counters() },
    modules: [],
    confirmationMs: 5.0,
    budgetMs: 40.0,
    events: [],
  };
}

/** The commissioning baseline the demo starts from. */
export function demoPairState(): PairDashboardState {
  return {
    demo: true,
    connected: true,
    mode: 'redundant',
    roles: { 'PLC-1': 'Primary', 'PLC-2': 'Secondary' },
    admission: 'ADMITTED',
    permitOwner: 'PLC-1',
    epoch: 41,
    applicationGeneration: 105,
    stateGeneration: 1842,
    sync: { 'PLC-1': 'SYNC_READY', 'PLC-2': 'SYNC_READY' },
    control: { 'PLC-1': 'ACTIVE', 'PLC-2': 'IDLE' },
    dead: { 'PLC-1': false, 'PLC-2': false },
    deadRole: { 'PLC-1': null, 'PLC-2': null },
    ownerUnreachable: false,
    redundancyLost: false,
    channels: { P: true, I: true },
    degraded: false,
    timingLost: false,
    calibration: 'CALIBRATED',
    counters: { ch1: counters(), ch2: counters() },
    modules: MODULES.map((module, index) => ({
      ...module,
      owner: 'PLC-1',
      ownerState: 'ARMED',
      armed: true,
      packetAgeUs: 180 + index * 9,
    })),
    confirmationMs: 5.0,
    budgetMs: 40.0,
    events: [
      event('info', 'admission complete — execution permit granted to PLC-1'),
      event('info', 'commissioning calibration complete — link profile QUALIFIED'),
      event('info', 'claim/ARM epoch 41 — PLC-1 ACTIVE, 8/8 required modules ARMED'),
    ],
  };
}

function event(severity: PairEvent['severity'], message: string, at = timestamp()): PairEvent {
  return { at, severity, message };
}

/** Local wall-clock stamp for the ledger; callers pass a fixed `at` in tests. */
export function timestamp(): string {
  const now = new Date();
  return `${pad(now.getHours())}:${pad(now.getMinutes())}:${pad(now.getSeconds())}`;
}

function pad(value: number): string {
  return value.toString().padStart(2, '0');
}

function clone(state: PairDashboardState): PairDashboardState {
  return {
    ...state,
    roles: { ...state.roles },
    sync: { ...state.sync },
    control: { ...state.control },
    dead: { ...state.dead },
    deadRole: { ...state.deadRole },
    channels: { ...state.channels },
    counters: {
      ch1: { ...state.counters.ch1 },
      ch2: { ...state.counters.ch2 },
    },
    modules: state.modules.map(module => ({ ...module })),
    events: state.events.slice(),
  };
}

function alive(state: PairDashboardState, unit: UnitId): boolean {
  return !state.dead[unit];
}

/** The unit that currently commands outputs, if any. */
export function ownerOf(state: PairDashboardState): UnitId | null {
  for (const unit of UNITS) {
    const control = state.control[unit];
    if ((control === 'ACTIVE' || control === 'ACTIVE_DEGRADED') && alive(state, unit)) {
      return unit;
    }
  }
  return null;
}

/** The non-owning unit of the pair, if there is one. */
export function standbyOf(state: PairDashboardState): UnitId | null {
  const owner = ownerOf(state);
  if (owner === null) {
    return null;
  }
  return owner === 'PLC-1' ? 'PLC-2' : 'PLC-1';
}

function singleChannelOk(state: PairDashboardState): boolean {
  return state.channels.P && state.channels.I;
}

function pushEvent(state: PairDashboardState, severity: PairEvent['severity'], message: string, at?: string): void {
  state.events.unshift(event(severity, message, at));
  if (state.events.length > 200) {
    state.events.length = 200;
  }
}

function bumpLiveness(state: PairDashboardState): void {
  if (state.channels.P) {
    state.counters.ch1.ping += 1;
    state.counters.ch1.pong += 1;
  }
  if (state.channels.I) {
    state.counters.ch2.ping += 1;
    state.counters.ch2.pong += 1;
  }
}

function claimAll(state: PairDashboardState, unit: UnitId): void {
  for (const module of state.modules) {
    module.owner = unit;
    module.ownerState = 'ARMED';
    module.armed = true;
  }
}

/** All dashboard actions, in display order. */
export const DASHBOARD_ACTIONS: readonly DashboardAction[] = [
  'swap',
  'primary-death',
  'secondary-death',
  'resurrect-primary',
  'resurrect-secondary',
  'kill-p',
  'kill-i',
  'restore-channels',
  'partition',
  'calibrate',
  'invalidate',
  'degrade',
  'reset',
];

/** The enablement of every action, so the webview can disable buttons without duplicating guards. */
export function enabledActions(state: PairDashboardState): Record<DashboardAction, boolean> {
  const enabled = {} as Record<DashboardAction, boolean>;
  for (const action of DASHBOARD_ACTIONS) {
    enabled[action] = canApply(state, action);
  }
  return enabled;
}

/** Whether an action is legal in the current state (the demo's guards). */
export function canApply(state: PairDashboardState, action: DashboardAction): boolean {
  if (!state.demo) {
    return false;
  }
  const owner = ownerOf(state);
  const standby = standbyOf(state);
  const free = !state.redundancyLost;
  switch (action) {
    case 'swap':
      return free && owner !== null && standby !== null && alive(state, standby) && singleChannelOk(state)
        && state.sync[owner] === 'SYNC_READY' && state.sync[standby] === 'SYNC_READY'
        && state.calibration === 'CALIBRATED';
    case 'primary-death':
      return free && owner !== null;
    case 'secondary-death':
      return free && owner !== null && standby !== null && alive(state, standby);
    case 'resurrect-primary':
      return (state.dead['PLC-1'] && state.deadRole['PLC-1'] === 'owner')
        || (state.dead['PLC-2'] && state.deadRole['PLC-2'] === 'owner');
    case 'resurrect-secondary':
      return (state.dead['PLC-1'] && state.deadRole['PLC-1'] === 'standby')
        || (state.dead['PLC-2'] && state.deadRole['PLC-2'] === 'standby');
    case 'kill-p':
      return free && state.channels.P;
    case 'kill-i':
      return free && state.channels.I;
    case 'restore-channels':
      return !state.channels.P || !state.channels.I;
    case 'partition':
      return free && owner !== null && standby !== null && alive(state, standby) && !state.channels.P && !state.channels.I
        && state.sync[standby] === 'SYNC_READY' && state.calibration === 'CALIBRATED';
    case 'calibrate':
    case 'invalidate':
    case 'degrade':
    case 'reset':
      return true;
    default:
      return false;
  }
}

/** Applies one dashboard action, returning the next state (the input is not mutated). */
export function applyAction(
  state: PairDashboardState,
  action: DashboardAction,
  at = timestamp(),
): PairDashboardState {
  if (!canApply(state, action)) {
    return state;
  }
  const next = clone(state);
  bumpLiveness(next);
  switch (action) {
    case 'swap':
      applySwap(next, at);
      break;
    case 'primary-death':
      applyPrimaryDeath(next, at);
      break;
    case 'secondary-death':
      applySecondaryDeath(next, at);
      break;
    case 'resurrect-primary':
      applyResurrection(next, 'owner', at);
      break;
    case 'resurrect-secondary':
      applyResurrection(next, 'standby', at);
      break;
    case 'kill-p':
      next.channels.P = false;
      next.counters.ch1.missing += 1;
      next.counters.ch1.penalty += 1000;
      pushEvent(next, 'alarm', 'pair link (P) killed — expected +1 missing on channel P', at);
      applyTruth(next, at);
      break;
    case 'kill-i':
      next.channels.I = false;
      next.counters.ch2.missing += 1;
      next.counters.ch2.penalty += 1000;
      pushEvent(next, 'alarm', 'I/O path (I) killed — expected +1 missing on channel I', at);
      applyTruth(next, at);
      break;
    case 'restore-channels':
      next.channels.P = true;
      next.channels.I = true;
      next.counters.ch1.missing = 0;
      next.counters.ch1.penalty = 0;
      next.counters.ch2.missing = 0;
      next.counters.ch2.penalty = 0;
      if (!next.redundancyLost) {
        const owner = ownerOf(next);
        if (owner !== null) {
          next.control[owner] = 'ACTIVE';
        }
        const standby = standbyOf(next);
        if (standby !== null && next.control[standby] === 'REDUNDANCY_LOST') {
          next.control[standby] = 'IDLE';
        }
      }
      pushEvent(next, 'info', 'channels restored — P=1, I=1', at);
      break;
    case 'partition':
      applyPartition(next, at);
      break;
    case 'calibrate':
      next.calibration = 'CALIBRATED';
      next.admission = 'ADMITTED';
      pushEvent(next, 'info', 'calibration complete — link profile accepted (QUALIFIED)', at);
      break;
    case 'invalidate':
      next.calibration = 'UNQUALIFIED';
      next.admission = 'UNQUALIFIED';
      pushEvent(next, 'warn', 'qualification invalidated — recalibration required before takeover', at);
      break;
    case 'degrade':
      next.degraded = true;
      next.timingLost = !buildBudget(next).ok;
      pushEvent(next, 'warn', 'HA_PERFORMANCE_DEGRADED — channel 2 EMA left the calibrated envelope', at);
      if (next.timingLost) {
        pushEvent(next, 'alarm', `HA_TIMING_GUARANTEE_LOST — worst case ${buildBudget(next).worstMs.toFixed(2)} ms exceeds budget`, at);
      }
      break;
    case 'reset':
      return { ...demoPairState(), events: [event('info', 'demo reset to the commissioning baseline', at)] };
    default:
      break;
  }
  return next;
}

/** The truth table: who may promote, who must not, and what each loss means. */
function applyTruth(state: PairDashboardState, at: string): void {
  if (state.channels.P && state.channels.I) {
    return;
  }
  const owner = ownerOf(state);
  if (owner === null || !alive(state, owner)) {
    return;
  }
  const standby = standbyOf(state);
  if (!state.channels.P && state.channels.I) {
    if (standby !== null && alive(state, standby)) {
      state.control[standby] = 'REDUNDANCY_LOST';
      pushEvent(state, 'alarm', `pair link lost, ${owner} alive via I/O path — REDUNDANCY_LOST on observer; NO auto promotion`, at);
    }
  }
  else if (state.channels.P && !state.channels.I) {
    state.control[owner] = 'ACTIVE_DEGRADED';
    pushEvent(state, 'alarm', 'I/O path degraded — owner continues ACTIVE_DEGRADED; NO auto promotion', at);
  }
  else {
    state.control[owner] = 'ACTIVE_DEGRADED';
    if (standby !== null && alive(state, standby)) {
      state.control[standby] = 'IDLE';
    }
    pushEvent(state, 'warn', 'P=false && I=false with a live owner — promotion path only; a claim must be rejected (partition)', at);
  }
}

function applySwap(state: PairDashboardState, at: string): void {
  const owner = ownerOf(state)!;
  const standby = standbyOf(state)!;
  pushEvent(state, 'info', `commanded swap — release-owner epoch ${state.epoch} (${owner})`, at);
  state.control[owner] = 'IDLE';
  state.permitOwner = null;
  pushEvent(state, 'info', `all modules CLAIMED_DISARMED by ${standby}`, at);
  state.epoch += 1;
  claimAll(state, standby);
  state.control[standby] = 'ACTIVE';
  state.permitOwner = standby;
  pushEvent(state, 'info', `ARM epoch ${state.epoch} — ${standby} ACTIVE`, at);
  state.roles[owner] = 'Secondary';
  state.roles[standby] = 'Primary';
  state.sync[owner] = 'SYNC_READY';
  pushEvent(state, 'info', `${owner} rejoined as Secondary — SYNC_READY (no automatic failback)`, at);
  pushEvent(state, 'info', 'no ARM was ever sent before the barrier passed; partial ownership never means ACTIVE', at);
}

function applyPrimaryDeath(state: PairDashboardState, at: string): void {
  const owner = ownerOf(state)!;
  const standby = standbyOf(state)!;
  state.dead[owner] = true;
  state.deadRole[owner] = 'owner';
  state.ownerUnreachable = true;
  state.channels.P = false;
  state.channels.I = false;
  state.counters.ch1.missing += 1;
  state.counters.ch1.penalty += 1000;
  state.counters.ch2.missing += 1;
  state.counters.ch2.penalty += 1000;
  state.sync[owner] = 'deSYNC';
  state.control[owner] = 'DEAD';
  for (const module of state.modules) {
    module.ownerState = 'FAILSAFE';
    module.armed = false;
  }
  pushEvent(state, 'alarm', `${owner} DEAD — both channels silent (P=false, I=false)`, at);
  pushEvent(state, 'warn', 'owner lease expired on every module — fencing releases ownership', at);
  pushEvent(state, 'info', `${standby} takes the promotion path: claim → barrier → ARM`, at);
  state.epoch += 1;
  claimAll(state, standby);
  state.control[standby] = 'ACTIVE';
  state.permitOwner = standby;
  pushEvent(state, 'info', `takeover complete: ${standby} ACTIVE epoch ${state.epoch} — worst recovery ${buildBudget(state).worstMs.toFixed(2)} ms`, at);
}

function applySecondaryDeath(state: PairDashboardState, at: string): void {
  const standby = standbyOf(state)!;
  state.dead[standby] = true;
  state.deadRole[standby] = 'standby';
  state.sync[standby] = 'deSYNC';
  state.control[standby] = 'DEAD';
  pushEvent(state, 'alarm', `${standby} DEAD — no takeover target; owner continues ACTIVE, pair degraded`, at);
}

function applyResurrection(state: PairDashboardState, role: 'owner' | 'standby', at: string): void {
  const unit = UNITS.find(candidate => state.dead[candidate] && state.deadRole[candidate] === role)!;
  state.dead[unit] = false;
  state.deadRole[unit] = null;
  state.control[unit] = 'IDLE';
  state.sync[unit] = 'SYNC_READY';
  pushEvent(state, 'info', `${unit} boots deSYNC (UNQUALIFIED) — a live primary exists: rejoins as SECONDARY (zombie rule: never Primary)`, at);
  pushEvent(state, 'info', `${unit} SYNCING → SYNC_READY — monitor mode`, at);
  if (role === 'owner') {
    state.ownerUnreachable = false;
    state.channels.P = true;
    state.channels.I = true;
    state.counters.ch1.missing = 0;
    state.counters.ch1.penalty = 0;
    state.counters.ch2.missing = 0;
    state.counters.ch2.penalty = 0;
    const owner = ownerOf(state);
    if (owner !== null) {
      state.control[owner] = 'ACTIVE';
    }
    pushEvent(state, 'info', `channels restored — ${unit} is Secondary to the promoted owner`, at);
  }
}

function applyPartition(state: PairDashboardState, at: string): void {
  const owner = ownerOf(state)!;
  const standby = standbyOf(state)!;
  pushEvent(state, 'warn', `${standby} sees P=false && I=false — takes the promotion path; owner ${owner} is alive`, at);
  pushEvent(state, 'alarm', `claim DO-01 REJECTED: OWNERSHIP_CONFLICT — ${owner} holds Exclusive Owner epoch ${state.epoch}`, at);
  pushEvent(state, 'alarm', 'release-all — the claimant never armed anything; no partial ownership', at);
  for (const module of state.modules) {
    module.ownerState = 'SAFE';
    module.armed = false;
  }
  state.redundancyLost = true;
  state.control[owner] = 'REDUNDANCY_LOST';
  state.control[standby] = 'REDUNDANCY_LOST';
  pushEvent(state, 'alarm', 'REDUNDANCY_LOST — both units dropped ownership; manual repair required (Reset)', at);
}

/** The formula terms, as the Timing Budget section renders them. */
export interface BudgetView {
  worstMs: number;
  predictedMs: number;
  ok: boolean;
  confirmationMs: number;
  budgetMs: number;
}

function buildBudget(state: PairDashboardState): BudgetView {
  const claimMax = state.degraded ? 24.10 : 20.77;
  const claimEma = state.degraded ? 7.90 : 6.52;
  const outputMax = state.degraded ? 0.90 : 0.60;
  const outputEma = state.degraded ? 0.36 : 0.31;
  const worstMs = state.confirmationMs + claimMax + SCAN_MAX_MS + outputMax;
  const predictedMs = state.confirmationMs + claimEma + ARM_EMA_MS + (1 - SCAN_PHASE) * SCAN_EMA_MS + outputEma;
  return { worstMs, predictedMs, ok: worstMs <= state.budgetMs, confirmationMs: state.confirmationMs, budgetMs: state.budgetMs };
}

/** One module row of the Ownership & Barrier section. */
export interface ModuleView extends ModuleState {
  limiting: boolean;
}

/** The serializable view model the webview renders. */
export interface DashboardViewModel {
  demo: boolean;
  connected: boolean;
  emptyMessage: string | null;
  pair: {
    mode: string;
    epoch: number;
    applicationGeneration: number;
    stateGeneration: number;
    admission: string;
    permitOwner: UnitId | null;
  };
  units: {
    id: UnitId;
    role: 'Primary' | 'Secondary';
    sync: SyncState;
    control: ControlState;
    dead: boolean;
    permit: boolean;
    takeoverReady: boolean;
  }[];
  ioReady: { standbyReady: boolean; linkValid: boolean; calibrationValid: boolean };
  takeoverReady: boolean;
  budget: BudgetView;
  calibration: {
    state: string;
    degraded: boolean;
    jitterMs: number;
    lossPercent: number;
    directions: { direction: string; currentMs: number; ema10Ms: number; ema100Ms: number; maxMs: number; count: number }[];
  };
  channels: {
    p: boolean;
    i: boolean;
    ch1: LivenessCounters;
    ch2: LivenessCounters;
  };
  barrier: {
    claimed: number;
    total: number;
    passed: boolean;
    limitingDevice: string;
    worstClaimMs: number;
    modules: ModuleView[];
  };
  alarms: {
    performanceDegraded: boolean;
    timingLost: boolean;
    redundancyLost: boolean;
    deadUnits: UnitId[];
  };
  events: PairEvent[];
}

/** Builds the view model for the five contract sections. */
export function buildDashboardViewModel(state: PairDashboardState): DashboardViewModel {
  if (!state.connected) {
    return {
      demo: false,
      connected: false,
      emptyMessage: 'No redundant pair connected — the controller has not exposed an HA runtime surface yet.',
      pair: { mode: state.mode, epoch: 0, applicationGeneration: 0, stateGeneration: 0, admission: '—', permitOwner: null },
      units: [],
      ioReady: { standbyReady: false, linkValid: false, calibrationValid: false },
      takeoverReady: false,
      budget: buildBudget(state),
      calibration: { state: '—', degraded: false, jitterMs: 0, lossPercent: 0, directions: [] },
      channels: { p: false, i: false, ch1: state.counters.ch1, ch2: state.counters.ch2 },
      barrier: { claimed: 0, total: 0, passed: false, limitingDevice: '—', worstClaimMs: 0, modules: [] },
      alarms: { performanceDegraded: false, timingLost: false, redundancyLost: false, deadUnits: [] },
      events: state.events,
    };
  }

  const standby = standbyOf(state);
  const owner = ownerOf(state);
  const linkValid = singleChannelOk(state) || state.ownerUnreachable;
  const takeoverReady = standby !== null && alive(state, standby) && state.sync[standby] === 'SYNC_READY'
    && state.calibration === 'CALIBRATED' && !state.redundancyLost && linkValid;
  const claimed = state.modules.filter(module => module.ownerState === 'ARMED' || module.ownerState === 'CLAIMED_DISARMED').length;
  const limiting = state.modules.reduce(
    (worst, module) => (module.claimMaxMs > worst.claimMaxMs ? module : worst),
    state.modules[0],
  );
  const worstClaimMs = state.modules.reduce((sum, module) => sum + module.claimMaxMs, 0);
  const degraded = state.degraded;

  return {
    demo: state.demo,
    connected: state.connected,
    emptyMessage: null,
    pair: {
      mode: state.mode,
      epoch: state.epoch,
      applicationGeneration: state.applicationGeneration,
      stateGeneration: state.stateGeneration,
      admission: state.admission,
      permitOwner: state.permitOwner,
    },
    units: UNITS.map(unit => ({
      id: unit,
      role: state.roles[unit],
      sync: state.sync[unit],
      control: state.control[unit],
      dead: state.dead[unit],
      permit: state.permitOwner === unit,
      takeoverReady: takeoverReady && unit === standby,
    })),
    ioReady: {
      standbyReady: standby !== null && state.sync[standby] === 'SYNC_READY',
      linkValid,
      calibrationValid: state.calibration === 'CALIBRATED',
    },
    takeoverReady,
    budget: buildBudget(state),
    calibration: {
      state: state.calibration,
      degraded,
      jitterMs: degraded ? 0.54 : 0.18,
      lossPercent: degraded ? 0.21 : 0.003,
      directions: [
        { direction: 'A→B→A', currentMs: 0.31, ema10Ms: 0.34, ema100Ms: 0.30, maxMs: 0.82, count: state.counters.ch1.ping },
        {
          direction: 'B→A→B',
          currentMs: degraded ? 1.42 : 0.33,
          ema10Ms: degraded ? 1.18 : 0.34,
          ema100Ms: degraded ? 0.62 : 0.31,
          maxMs: degraded ? 2.06 : 0.88,
          count: state.counters.ch2.ping,
        },
      ],
    },
    channels: {
      p: state.channels.P,
      i: state.channels.I,
      ch1: state.counters.ch1,
      ch2: state.counters.ch2,
    },
    barrier: {
      claimed,
      total: state.modules.length,
      passed: claimed === state.modules.length && state.modules.every(module => module.ownerState === 'ARMED'),
      limitingDevice: limiting ? limiting.id : '—',
      worstClaimMs,
      modules: state.modules.map(module => ({ ...module, limiting: module.id === (limiting ? limiting.id : '') })),
    },
    alarms: {
      performanceDegraded: state.degraded,
      timingLost: state.timingLost,
      redundancyLost: state.redundancyLost,
      deadUnits: UNITS.filter(unit => state.dead[unit]),
    },
    events: state.events,
  };
}
