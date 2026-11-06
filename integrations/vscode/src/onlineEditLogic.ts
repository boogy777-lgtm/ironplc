/**
 * The online-edit dialog as a model: what the QuickPick shows for the edit view
 * of the device and the one fact the client holds about Build. The shell sets the widget properties from the model and
 * runs the command of the item the person selects; nothing here imports `vscode`.
 *
 * The invariants of every rendering of the edit view:
 * - the labels, the enablement and the reasons of the items are the device's
 *   edit view (ADR-0087), copied and never recomputed; the only other inputs are
 *   `candidateMode` (what kind of candidate is staged, for a notice) and the
 *   Build fact;
 * - the client holds exactly one fact: a build of the current sources succeeded
 *   in this dialog. Build is not a device command, so the view knows nothing of
 *   it; Next is Build while the device is idle and the fact is missing, and
 *   everything else follows from the view;
 * - the dialog sequences commands only through the steps the view names:
 *   `finalizeAll` (the client runs them, with Build in front when the fact is
 *   missing) and the two-step Cancel under Test (the device runs the steps, the
 *   client sends one command);
 * - a device without the view gets an honest, disabled dialog.
 *
 * The stage marks come from one table, `STAGES`: a new stage is one entry.
 * A guard test reads this source and fails when it reads a status field other
 * than the edit view and `candidateMode`.
 */

import { BuildPhase } from './buildLogic';
import { CandidateMode, EditCommandView, EditView, HotEditStatus } from './hotEditSession';

/** The command that opens the dialog; `checkInvariants` checks that package.json contributes it and the README names it. */
export const ONLINE_EDIT = { commandId: 'ironplc.onlineEdit', title: 'Online Edit' } as const;

/** The one fact about Build: whether a build of the current sources succeeded in this dialog. */
export type BuildFact = 'notBuilt' | 'built';

/** What the dialog reads of a status: the edit view and the kind of the candidate. */
export type DialogStatus = Pick<HotEditStatus, 'edit' | 'candidateMode'>;

/** The wire name the dialog gives to Build, which is not a command of the device. */
export const BUILD_STEP = 'build';

/** One stage of an online edit, in the order the dialog marks them. */
export interface StageDef {
  /** The wire name of the device command that performs the stage, or `build`. */
  command: string;
  /** The label of a stage the device does not name (Build); the others use the label of the view. */
  label?: string;
  /** The client-side phase the device panel renders while the stage runs. */
  phase: BuildPhase;
}

/** The stages, in order. A device command that is a new stage of the edit is one entry here. */
export const STAGES: readonly StageDef[] = [
  { command: BUILD_STEP, label: 'Build', phase: 'compiling' },
  { command: 'accept', phase: 'uploading' },
  { command: 'test', phase: 'verifying' },
  { command: 'assemble', phase: 'running' },
];

/**
 * The stage in progress while a swap waits for its scan boundary: the view
 * offers no next command then, and the swap is a test in the usual case.
 */
const SWAP_WAIT_STAGE = 'test';

const MARK_DONE = '✔';
const MARK_CURRENT = '●';
const MARK_TODO = '○';

/** How the title names an edit stage; a stage this table does not know is named as the device names it. */
const EDIT_STAGE_NAMES: Readonly<Record<string, string>> = {
  idle: 'No candidate',
  accepted: 'Accepted',
  testing: 'Testing',
  testingMigration: 'Testing (migration)',
  testingReplace: 'Testing (download)',
  swapPending: 'Waiting for the scan boundary',
};

/** What a notice says about the kind of a staged candidate. */
const CANDIDATE_NOTICES: Readonly<Partial<Record<CandidateMode, string>>> = {
  replace: 'This candidate is a Download: assembling it replaces the application and starts its variables again.',
  migrate: 'This candidate migrates variables whose type changed: it cannot be reverted once it runs.',
};

/** The kind of an item: what it does, whatever the label the view gives it. */
export type ItemKind = 'next' | 'finalizeAll' | 'cancel';

/** One item of the dialog. */
export interface DialogItem {
  kind: ItemKind;
  label: string;
  /** The reason (`V####: text`) when the item is disabled, else what the item does. */
  description: string;
  enabled: boolean;
  /** The one command an enabled item sends: a device command, or `build`. */
  command?: string;
  /** The commands the client runs in order, with the view re-read between them (Finalize all). */
  steps?: readonly string[];
  /** The steps the device runs for the one command (Cancel under Test); shown, never sent. */
  deviceSteps?: readonly string[];
}

/** What the QuickPick shows. */
export interface DialogModel {
  /** `Online Edit: ` and the edit stage. */
  title: string;
  /** The position of the stage in progress among `totalSteps`; absent for a stage this client does not know. */
  step?: number;
  totalSteps?: number;
  /** True while a command runs. */
  busy: boolean;
  /** The stage marks, and what runs while busy. */
  prompt: string;
  /** Lines the person should read: the device's warning, the kind of the candidate. */
  notices: readonly string[];
  items: readonly DialogItem[];
}

/** The text of a stage: Build is the client's own, every other stage is named by the view. */
export function stageLabel(command: string, view: EditView | undefined): string {
  const def = STAGES.find(stage => stage.command === command);
  if (def?.label !== undefined) {
    return def.label;
  }
  return view?.commands[command]?.label ?? command.charAt(0).toUpperCase() + command.slice(1);
}

/** The client-side phase of a step, for the device panel; undefined for a command that is no stage. */
export function phaseOf(command: string): BuildPhase | undefined {
  return STAGES.find(stage => stage.command === command)?.phase;
}

/** The reason of a disabled command as the description of an item. */
function reasonText(command: EditCommandView): string {
  return command.reason === undefined ? '' : `${command.reason.code}: ${command.reason.text}`;
}

function titleOf(view: EditView | undefined, build: BuildFact): string {
  if (view === undefined) {
    return 'Online Edit';
  }
  const named = view.stage === 'idle' && build === 'built' ? 'Built' : EDIT_STAGE_NAMES[view.stage] ?? view.stage;
  return `Online Edit: ${named}`;
}

/** The stage the next click performs: the view's next, else Build while idle without a build. */
function currentStage(view: EditView | undefined, build: BuildFact): string | undefined {
  if (view === undefined) {
    return build === 'notBuilt' ? BUILD_STEP : undefined;
  }
  if (view.stage === 'idle' && build === 'notBuilt') {
    return BUILD_STEP;
  }
  return view.next?.command ?? (view.stage === 'swapPending' ? SWAP_WAIT_STAGE : undefined);
}

/** The marks line, and the position of `current`; both empty for a stage the table does not know. */
function marks(current: string | undefined, view: EditView | undefined): { prompt: string; step?: number } {
  const at = STAGES.findIndex(stage => stage.command === current);
  if (at < 0) {
    return { prompt: '' };
  }
  const prompt = STAGES
    .map((stage, index) => `${index < at ? MARK_DONE : index === at ? MARK_CURRENT : MARK_TODO} ${stageLabel(stage.command, view)}`)
    .join('  ');
  return { prompt, step: at + 1 };
}

function nextItem(view: EditView | undefined, build: BuildFact): DialogItem {
  if (build === 'notBuilt' && (view === undefined || view.stage === 'idle')) {
    return { kind: 'next', label: stageLabel(BUILD_STEP, view), description: 'compile the project', enabled: true, command: BUILD_STEP };
  }
  if (view === undefined) {
    return { kind: 'next', label: 'Next', description: 'the device does not report which edit commands it allows', enabled: false };
  }
  const next = view.next;
  if (next === undefined) {
    return { kind: 'next', label: 'Next', description: `${EDIT_STAGE_NAMES[view.stage] ?? view.stage}: no command is offered now`, enabled: false };
  }
  const command = view.commands[next.command];
  if (command === undefined) {
    return { kind: 'next', label: next.label, description: `the device does not list ${next.command}`, enabled: false };
  }
  return {
    kind: 'next',
    label: next.label,
    description: command.enabled ? '' : reasonText(command),
    enabled: command.enabled,
    command: next.command,
  };
}

function finalizeAllItem(view: EditView | undefined, build: BuildFact): DialogItem {
  const command = view?.commands.finalizeAll;
  if (view === undefined || command === undefined) {
    return { kind: 'finalizeAll', label: 'Finalize all', description: 'the device does not offer it', enabled: false };
  }
  const named = command.steps;
  if (named === undefined || named.length === 0) {
    return { kind: 'finalizeAll', label: command.label, description: 'the device names no steps for it', enabled: false };
  }
  // Build is the client's: it goes first only when the device is idle and nothing was built.
  const steps = view.stage === 'idle' && build === 'notBuilt' ? [BUILD_STEP, ...named] : named;
  return {
    kind: 'finalizeAll',
    label: command.label,
    description: command.enabled ? steps.map(step => stageLabel(step, view)).join(' → ') : reasonText(command),
    enabled: command.enabled,
    ...(command.enabled ? { steps } : {}),
  };
}

function cancelItem(view: EditView | undefined): DialogItem {
  const command = view?.commands.cancel;
  if (view === undefined || command === undefined) {
    return { kind: 'cancel', label: 'Cancel', description: 'the device does not offer it', enabled: false };
  }
  const deviceSteps = command.steps;
  return {
    kind: 'cancel',
    label: command.label,
    description: command.enabled
      ? (deviceSteps === undefined ? '' : deviceSteps.map(step => stageLabel(step, view)).join(', then '))
      : reasonText(command),
    enabled: command.enabled,
    ...(command.enabled ? { command: 'cancel' } : {}),
    ...(command.enabled && deviceSteps !== undefined ? { deviceSteps } : {}),
  };
}

/**
 * The dialog for `status` (undefined: nothing is known yet) and the Build fact.
 * `running` is the label of what runs now; the dialog is busy while it is set.
 */
export function dialogModel(status: DialogStatus | undefined, build: BuildFact, running?: string): DialogModel {
  const view = status?.edit;
  const { prompt: markLine, step } = marks(currentStage(view, build), view);
  const notices: string[] = [];
  if (view === undefined) {
    notices.push('The device does not report which edit commands it allows.');
  }
  else {
    const kind = status?.candidateMode === undefined || view.stage === 'idle' ? undefined : CANDIDATE_NOTICES[status.candidateMode];
    if (kind !== undefined) {
      notices.push(kind);
    }
    if (view.warning !== undefined) {
      notices.push(view.warning);
    }
  }
  return {
    title: titleOf(view, build),
    ...(step !== undefined ? { step, totalSteps: STAGES.length } : {}),
    busy: running !== undefined,
    prompt: running === undefined ? markLine : `${markLine}${markLine === '' ? '' : '  —  '}${running}…`,
    notices,
    items: [nextItem(view, build), finalizeAllItem(view, build), cancelItem(view)],
  };
}

/**
 * What the status bar item that reopens the dialog shows, or undefined when no
 * candidate exists (the item is hidden then). The warning of the view, when it
 * has one, is in the tooltip.
 */
export function reopenItem(status: DialogStatus | undefined): { text: string; tooltip: string } | undefined {
  const view = status?.edit;
  if (view === undefined || view.stage === 'idle') {
    return undefined;
  }
  const named = EDIT_STAGE_NAMES[view.stage] ?? view.stage;
  const lines = [`An online edit is in progress (${named.toLowerCase()}).`];
  if (view.warning !== undefined) {
    lines.push(view.warning);
  }
  lines.push('Click to open the online edit dialog.');
  return { text: `$(edit) Online Edit: ${named}`, tooltip: lines.join('\n') };
}
