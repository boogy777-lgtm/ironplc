/**
 * The HA panel's rendering of the redundancy engineering surface
 * (`specs/design/ha-engineering-ui.md`): the five Studio tabs (Pair
 * Overview / Calibration / Ownership & Barrier / Timing Budget / Events)
 * as tree sections and rows over the wire payloads the session queries,
 * plus the legality predicates that hide or refuse engineer actions per
 * ADR-0064/0065 (a swap is offered only at SYNC_READY, and only on the
 * one active connection). The module is vscode-free: `haPanel.ts` maps
 * these rows to tree items and owns no HA decision.
 */

import { PanelRow } from './devicePanelLogic';
import {
  HaBarrier,
  HaCalibration,
  HaEvents,
  HaIoReady,
  HaStatus,
  HaTermStats,
  HaTimingBudget,
} from './haProtocol';

/** The command ids the HA panel registers and the glue wires. */
export const HA_REFRESH_COMMAND = 'ironplc.haRefresh';
export const HA_COMMANDED_SWAP_COMMAND = 'ironplc.haCommandedSwap';
export const HA_RUN_CALIBRATION_COMMAND = 'ironplc.haRunCalibration';
export const HA_SET_TIMING_BUDGET_COMMAND = 'ironplc.haSetTimingBudget';

/** Everything the panel renders, in the vocabulary of the wire payloads. */
export interface HaPanelModel {
  /** Whether the engineering connection is live (one session, ADR-0065). */
  connected: boolean;
  /** False when the server predates the HA surface (a codeless refusal). */
  supported: boolean;
  /** Whether the connected unit runs standalone (no pair configured). */
  standalone: boolean;
  /** True when a refresh failed transiently; the data is the last good. */
  stale: boolean;
  status: HaStatus | null;
  calibration: HaCalibration | null;
  barrier: HaBarrier | null;
  ioReady: HaIoReady | null;
  budget: HaTimingBudget | null;
  events: HaEvents | null;
}

/** The disconnected model: one honest row, no fabricated data. */
export function disconnectedHaModel(): HaPanelModel {
  return {
    connected: false,
    supported: true,
    standalone: false,
    stale: false,
    status: null,
    calibration: null,
    barrier: null,
    ioReady: null,
    budget: null,
    events: null,
  };
}

/** One collapsible tab of the panel. */
export interface HaSection {
  id: string;
  title: string;
  /** The badge the tab headline carries (TakeoverReady, budget verdict). */
  badge?: string;
  rows: PanelRow[];
}

/** The five Studio tabs from the model, in the contract's order. */
export function haSections(model: HaPanelModel): HaSection[] {
  if (!model.connected) {
    return [
      {
        id: 'connection',
        title: 'Pair Overview',
        rows: [
          {
            id: 'connection.hint',
            label: 'Engineering connection',
            description: 'not connected',
            tooltip: 'Connect to a device (IronPLC: Connect to Device); the HA surface reads the same single session.',
          },
        ],
      },
    ];
  }
  if (!model.supported) {
    return [
      {
        id: 'unsupported',
        title: 'Pair Overview',
        rows: [
          {
            id: 'unsupported.hint',
            label: 'HA surface',
            description: 'unavailable on this device',
            tooltip: 'The device answered the HA query with a codec error: its server predates the HA engineering surface.',
          },
        ],
      },
    ];
  }
  return [
    pairOverviewSection(model),
    calibrationSection(model),
    barrierSection(model),
    timingBudgetSection(model),
    eventsSection(model),
  ];
}

/**
 * Whether the commanded swap may be offered: only on a live pair whose
 * local unit is at SYNC_READY (the swap is refused anywhere else, so the
 * UI does not offer it — ADR-0064/0065).
 */
export function canCommandSwap(model: HaPanelModel): boolean {
  return model.connected
    && model.supported
    && !model.standalone
    && model.status?.local.sync === 'syncReady';
}

/** Whether a calibration run may be offered: a live pair, not standalone. */
export function canRunCalibration(model: HaPanelModel): boolean {
  return model.connected && model.supported && !model.standalone;
}

/** Whether the timing-budget edit may be offered: a live pair, not standalone. */
export function canSetTimingBudget(model: HaPanelModel): boolean {
  return model.connected && model.supported && !model.standalone;
}

/** Pair Overview: is the pair healthy, and who may command outputs now. */
function pairOverviewSection(model: HaPanelModel): HaSection {
  const status = model.status;
  if (status === null) {
    return emptySection('pair', 'Pair Overview');
  }
  if (status.standalone) {
    return {
      id: 'pair',
      title: 'Pair Overview',
      badge: 'Standalone',
      rows: [
        {
          id: 'pair.standalone',
          label: 'Redundancy',
          description: 'standalone — no pair configured',
          tooltip: 'This unit runs without a redundant pair; the HA statechart does not run and every HA action is refused.',
        },
      ],
    };
  }
  const rows: PanelRow[] = [
    unitRows('pair.local', 'Local unit', status.local, status),
    unitRows('pair.peer', 'Peer unit', status.peer, status),
    {
      id: 'pair.epoch',
      label: 'Epoch',
      description: status.epoch === undefined ? '—' : String(status.epoch),
    },
    {
      id: 'pair.generations',
      label: 'Generations',
      description: `app ${status.applicationGeneration} · state ${status.stateGeneration}`,
    },
    {
      id: 'pair.takeoverReady',
      label: 'TakeoverReady',
      description: status.takeoverReady ? 'ready' : 'not ready',
      tooltip: 'TakeoverReady = SYNC_READY && IO_READY && RedundancyLinkValid (ADR-0062).',
    },
    {
      id: 'pair.inputs',
      label: 'Verdict inputs',
      description: `sync ${yesNo(status.syncReady)} · io ${yesNo(status.ioReady)} · link ${yesNo(status.linkValid)}`,
    },
    {
      id: 'pair.alarms',
      label: 'Alarms',
      description: alarmSummary(status),
    },
  ].flat();
  return {
    id: 'pair',
    title: 'Pair Overview',
    badge: status.takeoverReady ? 'TakeoverReady' : 'Not ready',
    rows,
  };
}

function unitRows(id: string, label: string, unit: HaStatus['local'] | undefined, status: HaStatus): PanelRow[] {
  if (unit === undefined) {
    return [{ id, label, description: '—' }];
  }
  const sync = unit.sync === undefined ? 'standalone' : unit.sync;
  const control = unit.control === undefined ? '—' : unit.control;
  const row: PanelRow = {
    id,
    label,
    description: `${unit.role ?? '—'} · ${sync} · ${control}`,
    tooltip: `ControllerId ${unit.controllerId} · epoch ${unit.epoch}`
      + (unit.syncReason !== undefined ? ` · deSYNC: ${unit.syncReason}` : '')
      + (unit.alarm !== undefined ? ` · alarm: ${unit.alarm}` : ''),
  };
  return [row];
}

/** Calibration: is the pair's timing model measured, current, and valid. */
function calibrationSection(model: HaPanelModel): HaSection {
  const calibration = model.calibration;
  if (calibration === null) {
    return emptySection('calibration', 'Calibration');
  }
  const rows: PanelRow[] = [
    {
      id: 'calibration.state',
      label: 'Run state',
      description: calibration.state,
    },
    {
      id: 'calibration.baseline',
      label: 'Commissioning baseline',
      description: calibration.baselineRttMax === undefined ? '—' : `${calibration.baselineRttMax} ticks RTT max`,
    },
    {
      id: 'calibration.recalibrations',
      label: 'Recalibrations',
      description: String(calibration.recalibrations),
    },
  ];
  if (calibration.lastInvalidation !== undefined) {
    rows.push({
      id: 'calibration.lastInvalidation',
      label: 'Last recalibration trigger',
      description: calibration.lastInvalidation,
    });
  }
  rows.push(
    ...directionRows('calibration.ab', 'A→B→A', calibration.ab),
    ...directionRows('calibration.ba', 'B→A→B', calibration.ba),
    {
      id: 'calibration.scan',
      label: 'Scan time',
      description: termSummary(calibration.scan),
      tooltip: `Jitter envelope ${calibration.scan.min}..=${calibration.scan.max} ticks.`,
    },
    {
      id: 'calibration.peerDetect',
      label: 'Peer detection',
      description: termSummary(calibration.peerDetect),
      tooltip: `Lease expiry ${calibration.leaseExpiry} ticks · claim start ${calibration.claimStart} ticks.`,
    },
  );
  return {
    id: 'calibration',
    title: 'Calibration',
    badge: calibration.state === 'calibrated' ? 'Calibrated' : calibration.state,
    rows: rows.flat(),
  };
}

function directionRows(id: string, label: string, direction: HaCalibration['ab']): PanelRow[] {
  return [
    {
      id: `${id}.rtt`,
      label: `${label} RTT`,
      description: termSummary(direction.rtt),
      tooltip: `Jitter envelope ${direction.rtt.min}..=${direction.rtt.max} ticks · loss ${direction.lossRatePercent}% (max streak ${direction.maxConsecutiveLoss}).`,
    },
    {
      id: `${id}.processing`,
      label: `${label} processing`,
      description: termSummary(direction.processing),
    },
  ];
}

/** Ownership & Barrier: the fencing state and the cost of the barrier. */
function barrierSection(model: HaPanelModel): HaSection {
  const barrier = model.barrier;
  if (barrier === null) {
    return emptySection('barrier', 'Ownership & Barrier');
  }
  const rows: PanelRow[] = barrier.modules.map((module, index) => ({
    id: `barrier.module.${module.module}`,
    label: `Module ${index}`,
    description: moduleSummary(module),
    tooltip: `Claim ${termSummary(module.claim)} · ARM ${termSummary(module.arm)} · output apply ${termSummary(module.outputApply)}.`,
  }));
  rows.push(
    {
      id: 'barrier.limiting',
      label: 'Limiting device',
      description: barrier.limitingDevice === undefined ? '—' : `module ${barrier.limitingDevice}`,
      tooltip: 'The module whose qualification-bound claim dominates the barrier — the device the takeover estimate names (ADR-0062).',
    },
    {
      id: 'barrier.worstRecovery',
      label: 'Worst ownership recovery',
      description: barrier.worstOwnershipRecovery === undefined ? '—' : `${barrier.worstOwnershipRecovery} ticks`,
    },
  );
  return {
    id: 'barrier',
    title: 'Ownership & Barrier',
    rows,
  };
}

function moduleSummary(module: HaBarrier['modules'][number]): string {
  const state = module.online ? module.ownerState : 'faulted';
  const owner = module.owner === undefined ? '' : ` by ${module.owner}`;
  const armed = module.ownerArmed ? ' · armed' : '';
  return `${state}${owner}${armed} · ${module.profile}`;
}

/** Timing Budget: the honest failover estimate and the verdict. */
function timingBudgetSection(model: HaPanelModel): HaSection {
  const budget = model.budget;
  if (budget === null) {
    return emptySection('budget', 'Timing Budget');
  }
  const verdict = budget.verdict;
  const rows: PanelRow[] = [
    {
      id: 'budget.configured',
      label: 'Configured',
      description: `confirm ${budget.peerFailureConfirmation} · budget ${budget.recoveryBudget} ticks`,
    },
    {
      id: 'budget.peerDetect',
      label: 'T_peer-detect',
      description: termBoundSummary(budget.peerDetect.bound, budget.peerDetect.current, budget.peerDetect.ema10),
    },
    {
      id: 'budget.claim',
      label: 'T_claim',
      description: termBoundSummary(budget.claim.bound, budget.claim.current, budget.claim.ema10),
    },
    {
      id: 'budget.arm',
      label: 'T_arm',
      description: termBoundSummary(budget.arm.bound, budget.arm.current, budget.arm.ema10),
    },
    {
      id: 'budget.scan',
      label: 'T_scan-safe-point',
      description: `${budget.scan.bound} bound · ${budget.scan.current} current · phase ${budget.scan.phase}`,
      tooltip: `Scan EMA10 ${budget.scan.ema10} · EMA100 ${budget.scan.ema100} · jitter ${budget.scan.min}..=${budget.scan.max}.`,
    },
    {
      id: 'budget.outputApply',
      label: 'T_output-apply',
      description: termBoundSummary(budget.outputApply.bound, budget.outputApply.current, budget.outputApply.ema10),
    },
    {
      id: 'budget.worstCase',
      label: 'Calculated worst case',
      description: budget.calculatedWorstCase === undefined ? '—' : `${budget.calculatedWorstCase} ticks`,
    },
    {
      id: 'budget.predicted',
      label: 'Predicted if now',
      description: budget.predictedIfNow === undefined ? '—' : `${budget.predictedIfNow} ticks`,
      tooltip: 'The online estimator: the recovery if the failover happened at the current scan phase (ADR-0062).',
    },
  ];
  return {
    id: 'budget',
    title: 'Timing Budget',
    badge: verdictBadge(verdict?.qualified, verdict?.minimumDemonstrated),
    rows,
  };
}

/** Events: the timestamped ledger, appended never rewritten. */
function eventsSection(model: HaPanelModel): HaSection {
  const events = model.events;
  if (events === null) {
    return emptySection('events', 'Events');
  }
  const rows: PanelRow[] = events.events.map((event, index) => ({
    id: `events.${index}`,
    label: event.kind,
    description: `tick ${event.tick}`,
    tooltip: event.detail,
  }));
  if (rows.length === 0) {
    rows.push({ id: 'events.empty', label: 'No events recorded', description: '' });
  }
  return {
    id: 'events',
    title: 'Events',
    badge: String(events.count),
    rows,
  };
}

function emptySection(id: string, title: string): HaSection {
  return { id, title, rows: [{ id: `${id}.empty`, label: 'No data', description: '' }] };
}

function termSummary(term: HaTermStats): string {
  return `${term.current} current · ${term.ema10} EMA10 · ${term.ema100} EMA100 · ${term.max} max · n=${term.count}`;
}

function termBoundSummary(bound: number, current: number, ema10: number): string {
  return `${bound} bound · ${current} current · ${ema10} EMA10`;
}

function yesNo(value: boolean): string {
  return value ? 'yes' : 'no';
}

function alarmSummary(status: HaStatus): string {
  const alarms: string[] = [];
  if (status.alarms.performanceDegraded) {
    alarms.push('HA_PERFORMANCE_DEGRADED (V4110)');
  }
  if (status.alarms.timingGuaranteeLost) {
    alarms.push('HA_TIMING_GUARANTEE_LOST (V4111)');
  }
  if (status.alarms.redundancyLost) {
    alarms.push('REDUNDANCY_LOST');
  }
  return alarms.length === 0 ? 'none' : alarms.join(' · ');
}

function verdictBadge(qualified: boolean | undefined, minimumDemonstrated: number | undefined): string | undefined {
  if (qualified === undefined) {
    return undefined;
  }
  return qualified ? 'Qualified' : `Min demonstrated ${minimumDemonstrated ?? '—'}`;
}
