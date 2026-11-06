/**
 * The go-online comparison as the editor renders it (ADR-0089). The comparison
 * is computed in one place, `ironplcc online-compare`, which prints it as JSON;
 * this module reads that JSON and decides nothing about it. Nothing here
 * compares a hash, a record or a device id:
 *
 * - `parseComparison` reads the JSON into the mirror types below. The mirror is
 *   tolerant on purpose: a headline row, an information row or an action that a
 *   later compiler adds is shown with the words it came with, and an action this
 *   client cannot run is shown disabled with that said;
 * - `comparisonModel` is what the QuickPick shows: the headline as the title,
 *   what the controller is and holds, what the project has, the last download,
 *   the information rows, and one item for each action with the `enabled` state
 *   and the reason the compiler gave;
 * - `ACTION_HANDLERS` is the one table from an action of the comparison to what
 *   the editor does when it is chosen. A guard test fails when an `ActionKind`
 *   has no entry, and the type does not compile without one;
 * - `acceptGate` is the answer of `hotEdit.allowed`, which replaced the E0015
 *   check of the Accept stage of Online Edit.
 *
 * The module is vscode-free: the shell (`onlineComparisonCommands`) renders the
 * model and runs the handler, and the run of the compiler is in
 * `onlineComparisonRun`.
 */

import { DOWNLOAD_ACTIONS } from './downloadLogic';
import { ONLINE_EDIT } from './onlineEditLogic';
import { HANDOFF_ACTIONS, shortDeviceId } from './vscodeCommon';

/** The command that shows the comparison of the current connection again; `checkInvariants` checks that package.json contributes it and the README names it. */
export const SHOW_ONLINE_COMPARISON = { commandId: 'ironplc.showOnlineComparison', title: 'Show Online Comparison' } as const;

/**
 * The actions the comparison offers (`ActionKind` of `ironplc_project::online_comparison`,
 * which prints them by these names), in the order the compiler lists them.
 */
export const ACTION_KINDS = [
  'monitor',
  'onlineChange',
  'download',
  'upload',
  'search',
  'chooseAnother',
  'retry',
  'resolve',
] as const;

/** An action the compiler's comparison offers that this client knows. */
export type ActionKind = typeof ACTION_KINDS[number];

/** A reason, in the words of the comparison or of the controller: a stable code and its text. */
export interface ComparisonReason {
  code: string;
  text: string;
}

/** What the controller holds. `kind` is `noApplication`, `application` or `unknown`; another kind is shown as it is. */
export interface ComparisonHolds {
  kind: string;
  contentHash?: string;
  layoutHash?: string | null;
}

/** What the controller is; absent from a comparison of an attempt that did not reach it. */
export interface ComparisonController {
  deviceId: string | null;
  runtimeVersion: string | null;
  runState: string;
  holds: ComparisonHolds;
  editStage: string | null;
}

/** What the project has. `status` is `upToDate`, `changedSinceBuild`, `neverBuilt` or `buildFails`; another is shown as it is. */
export interface ComparisonProject {
  status: string;
  application: string | null;
  contentHash: string | null;
  layoutHash: string | null;
}

/** The last download of this project to the controller. */
export interface ComparisonDownload {
  contentHash: string;
  layoutHash: string;
  time: string;
}

/** A condition beside the headline. */
export interface ComparisonInformation {
  kind: string;
  text: string;
}

/** One action of the comparison, enabled or not. `action` is an `ActionKind` or a name a later compiler adds. */
export interface ComparisonAction {
  action: string;
  label: string;
  enabled: boolean;
  reason: ComparisonReason | null;
  detail: string | null;
}

/** Whether the Build of an online change may begin. */
export interface ComparisonHotEdit {
  allowed: boolean;
  reason: ComparisonReason | null;
}

/**
 * The comparison, as `ironplcc online-compare` prints it. `row` is the headline
 * (`upToDate`, `otherApplication`, and so on); the editor never switches on it
 * to decide what an action is worth.
 */
export interface Comparison {
  row: string;
  rowNumber: number;
  summary: string;
  connected: boolean;
  atOnce: boolean;
  controller: ComparisonController | null;
  project: ComparisonProject;
  lastDownload: ComparisonDownload | null;
  information: ComparisonInformation[];
  actions: ComparisonAction[];
  hotEdit: ComparisonHotEdit;
}

// ---------------------------------------------------------------- reading

type Json = Record<string, unknown>;

function asObject(value: unknown, what: string): Json {
  if (typeof value !== 'object' || value === null || Array.isArray(value)) {
    throw new Error(`the comparison has no ${what}`);
  }
  return value as Json;
}

function text(record: Json, key: string, what: string): string {
  const value = record[key];
  if (typeof value !== 'string') {
    throw new Error(`the comparison has no ${what}`);
  }
  return value;
}

function flag(record: Json, key: string, what: string): boolean {
  const value = record[key];
  if (typeof value !== 'boolean') {
    throw new Error(`the comparison has no ${what}`);
  }
  return value;
}

function optionalText(record: Json, key: string): string | null {
  const value = record[key];
  return typeof value === 'string' ? value : null;
}

function reasonOf(value: unknown): ComparisonReason | null {
  if (typeof value !== 'object' || value === null) {
    return null;
  }
  const record = value as Json;
  return typeof record.code === 'string' && typeof record.text === 'string'
    ? { code: record.code, text: record.text }
    : null;
}

function holdsOf(value: unknown): ComparisonHolds {
  const record = asObject(value, 'description of what the controller holds');
  const holds: ComparisonHolds = { kind: text(record, 'kind', 'kind of what the controller holds') };
  if (typeof record.contentHash === 'string') {
    holds.contentHash = record.contentHash;
  }
  if (typeof record.layoutHash === 'string' || record.layoutHash === null) {
    holds.layoutHash = record.layoutHash;
  }
  return holds;
}

function controllerOf(value: unknown): ComparisonController | null {
  if (value === null || value === undefined) {
    return null;
  }
  const record = asObject(value, 'controller');
  return {
    deviceId: optionalText(record, 'deviceId'),
    runtimeVersion: optionalText(record, 'runtimeVersion'),
    runState: text(record, 'runState', 'run state of the controller'),
    holds: holdsOf(record.holds),
    editStage: optionalText(record, 'editStage'),
  };
}

function projectOf(value: unknown): ComparisonProject {
  const record = asObject(value, 'project');
  return {
    status: text(record, 'status', 'status of the project'),
    application: optionalText(record, 'application'),
    contentHash: optionalText(record, 'contentHash'),
    layoutHash: optionalText(record, 'layoutHash'),
  };
}

function downloadOf(value: unknown): ComparisonDownload | null {
  if (value === null || value === undefined) {
    return null;
  }
  const record = asObject(value, 'last download');
  return {
    contentHash: text(record, 'contentHash', 'content hash of the last download'),
    layoutHash: text(record, 'layoutHash', 'layout hash of the last download'),
    time: text(record, 'time', 'time of the last download'),
  };
}

function informationOf(value: unknown): ComparisonInformation[] {
  if (!Array.isArray(value)) {
    throw new Error('the comparison has no information rows');
  }
  return value.map((entry) => {
    const record = asObject(entry, 'information row');
    return { kind: text(record, 'kind', 'kind of an information row'), text: text(record, 'text', 'text of an information row') };
  });
}

function actionsOf(value: unknown): ComparisonAction[] {
  if (!Array.isArray(value)) {
    throw new Error('the comparison has no actions');
  }
  return value.map((entry) => {
    const record = asObject(entry, 'action');
    const action = text(record, 'action', 'name of an action');
    return {
      action,
      label: optionalText(record, 'label') ?? action,
      enabled: flag(record, 'enabled', 'enabled state of an action'),
      reason: reasonOf(record.reason),
      detail: optionalText(record, 'detail'),
    };
  });
}

/**
 * Reads the JSON `ironplcc online-compare` printed. Rejects (with a message that
 * says what is missing) text that is not a comparison; fields it does not know
 * are ignored, and rows, notes and actions are kept by their names whatever they
 * are.
 */
export function parseComparison(printed: string): Comparison {
  let value: unknown;
  try {
    value = JSON.parse(printed);
  }
  catch {
    throw new Error('the comparison is not JSON');
  }
  const record = asObject(value, 'object');
  const hotEdit = asObject(record.hotEdit, 'hot edit answer');
  return {
    row: text(record, 'row', 'headline row'),
    rowNumber: typeof record.rowNumber === 'number' ? record.rowNumber : 0,
    summary: text(record, 'summary', 'summary'),
    connected: flag(record, 'connected', 'connected state'),
    atOnce: flag(record, 'atOnce', 'at-once state'),
    controller: controllerOf(record.controller),
    project: projectOf(record.project),
    lastDownload: downloadOf(record.lastDownload),
    information: informationOf(record.information),
    actions: actionsOf(record.actions),
    hotEdit: { allowed: flag(hotEdit, 'allowed', 'hot edit allowed state'), reason: reasonOf(hotEdit.reason) },
  };
}

// ------------------------------------------------------------ the actions

/** What the editor does when an action is chosen. The shell has one case for each kind. */
export type ActionRun
  /** Nothing more: the connection stays as it is. */
  = | { kind: 'close' }
  /** Runs a command of the editor. */
    | { kind: 'command'; command: string }
  /** Searches the network for controllers (IronLink). */
    | { kind: 'search' }
  /** Connects again, as the last attempt did. */
    | { kind: 'retry' }
  /** The comparison never enables it yet: there is nothing to run. */
    | { kind: 'none' };

/**
 * The one table from an action of the comparison to what the editor does. The
 * enablement and the reason of the action are the comparison's; this table says
 * only what running it means. A new action is one entry here and one name in
 * `ACTION_KINDS`; Upload is one entry (`command`) the day the compiler enables it.
 */
export const ACTION_HANDLERS: { readonly [A in ActionKind]: ActionRun } = {
  monitor: { kind: 'close' },
  onlineChange: { kind: 'command', command: ONLINE_EDIT.commandId },
  download: { kind: 'command', command: DOWNLOAD_ACTIONS.download.commandId },
  upload: { kind: 'none' },
  search: { kind: 'search' },
  chooseAnother: { kind: 'command', command: HANDOFF_ACTIONS.choose.command },
  retry: { kind: 'retry' },
  resolve: { kind: 'command', command: ONLINE_EDIT.commandId },
};

/** The handler of an action by its name; undefined for a name this client does not know. */
export function handlerOf(action: string): ActionRun | undefined {
  return Object.prototype.hasOwnProperty.call(ACTION_HANDLERS, action) ? ACTION_HANDLERS[action as ActionKind] : undefined;
}

// -------------------------------------------------------------- the model

/** One line of what the comparison says, beside the actions. */
export interface ModelRow {
  kind: 'controller' | 'project' | 'lastDownload' | 'information';
  label: string;
  text: string;
}

/** One action as the QuickPick shows it. */
export interface ModelItem {
  /** The name of the action in the comparison. */
  action: string;
  label: string;
  /** The code of the reason when the action is disabled; empty when it is enabled. */
  description: string;
  /** The words under the label: the detail of an enabled action, the text of the reason of a disabled one. */
  detail: string;
  /** Whether choosing it does something: the comparison enables it and this client can run it. */
  enabled: boolean;
  /** What choosing it does; undefined for an action this client does not know. */
  run: ActionRun | undefined;
}

/** What the QuickPick shows. */
export interface ComparisonModel {
  /** The headline, in the one line the status bar shows too. */
  title: string;
  rows: ModelRow[];
  items: ModelItem[];
}

const SHORT_HASH_LENGTH = 12;

function shortHash(hash: string): string {
  return hash.length > SHORT_HASH_LENGTH ? hash.slice(0, SHORT_HASH_LENGTH) : hash;
}

/** How the project status reads; a status this table does not know is shown as the compiler names it. */
const PROJECT_STATUS_TEXT: Readonly<Record<string, string>> = {
  upToDate: 'the sources are those of the last build',
  changedSinceBuild: 'the sources changed since the last build',
  neverBuilt: 'never built',
  buildFails: 'does not build',
};

function holdsText(holds: ComparisonHolds): string {
  switch (holds.kind) {
    case 'noApplication':
      return 'no application';
    case 'application':
      return holds.contentHash === undefined ? 'an application' : `application ${shortHash(holds.contentHash)}`;
    case 'unknown':
      return 'content not reported';
    default:
      return holds.kind;
  }
}

function controllerText(controller: ComparisonController): string {
  const parts: string[] = [];
  if (controller.deviceId !== null) {
    parts.push(shortDeviceId(controller.deviceId));
  }
  if (controller.runtimeVersion !== null) {
    parts.push(`runtime ${controller.runtimeVersion}`);
  }
  parts.push(controller.runState, `holds ${holdsText(controller.holds)}`);
  if (controller.editStage !== null) {
    parts.push(`edit ${controller.editStage}`);
  }
  return parts.join(' · ');
}

function projectText(project: ComparisonProject): string {
  const parts = [PROJECT_STATUS_TEXT[project.status] ?? project.status];
  if (project.application !== null) {
    parts.push(`application ${project.application}`);
  }
  if (project.contentHash !== null) {
    parts.push(`build ${shortHash(project.contentHash)}`);
  }
  return parts.join(' · ');
}

function itemOf(offer: ComparisonAction): ModelItem {
  const run = handlerOf(offer.action);
  if (!offer.enabled) {
    return {
      action: offer.action,
      label: offer.label,
      description: offer.reason?.code ?? 'unavailable',
      detail: offer.reason?.text ?? 'The comparison does not offer this action now.',
      enabled: false,
      run,
    };
  }
  if (run === undefined) {
    return {
      action: offer.action,
      label: offer.label,
      description: 'unsupported',
      detail: 'This version of the IronPLC extension cannot run this action; update the extension.',
      enabled: false,
      run,
    };
  }
  return { action: offer.action, label: offer.label, description: '', detail: offer.detail ?? '', enabled: true, run };
}

/**
 * The QuickPick for a comparison: the headline as the title, the lines that say
 * what the controller is and holds, what the project has, the last download and
 * the information rows, then one item for each action in the order the
 * comparison gives. Nothing is added, hidden or reordered.
 */
export function comparisonModel(comparison: Comparison): ComparisonModel {
  const rows: ModelRow[] = [];
  if (comparison.controller !== null) {
    rows.push({ kind: 'controller', label: 'Controller', text: controllerText(comparison.controller) });
  }
  rows.push({ kind: 'project', label: 'Project', text: projectText(comparison.project) });
  if (comparison.lastDownload !== null) {
    rows.push({
      kind: 'lastDownload',
      label: 'Last download',
      text: `${comparison.lastDownload.time} · ${shortHash(comparison.lastDownload.contentHash)}`,
    });
  }
  for (const note of comparison.information) {
    rows.push({ kind: 'information', label: 'Information', text: note.text });
  }
  return { title: comparison.summary, rows, items: comparison.actions.map(itemOf) };
}

/** The line the status bar shows for a comparison; undefined while there is none. */
export function statusSummary(comparison: Comparison | null | undefined): string | undefined {
  return comparison === null || comparison === undefined ? undefined : comparison.summary;
}

// ------------------------------------------------------ the Accept stage

/**
 * Whether the Accept stage of Online Edit may begin, from the comparison made
 * just before it. `hotEdit.allowed` is the answer of the `onlineChange` action,
 * so the dialog and the action cannot disagree. A refusal carries the words of
 * the comparison and says whether Download is the way in (the comparison
 * enables it). A comparison that could not be made refuses: nothing else says
 * what the controller holds.
 */
export type AcceptGate
  = | { allowed: true }
    | { allowed: false; code: string | undefined; text: string; offerDownload: boolean };

export const NO_COMPARISON_TEXT
  = 'The comparison of the controller with the project could not be made, so it is not known what the controller holds. See the "IronPLC Connection" output.';

export function acceptGate(comparison: Comparison | undefined): AcceptGate {
  if (comparison === undefined) {
    return { allowed: false, code: undefined, text: NO_COMPARISON_TEXT, offerDownload: false };
  }
  if (comparison.hotEdit.allowed) {
    return { allowed: true };
  }
  const download = comparison.actions.find(offer => offer.action === 'download');
  return {
    allowed: false,
    code: comparison.hotEdit.reason?.code,
    text: comparison.hotEdit.reason?.text ?? comparison.summary,
    offerDownload: download?.enabled === true,
  };
}

/**
 * Whether the controller holds an application that is not the last download,
 * where a download is on record: a Download then replaces what someone else
 * put there, and the confirmation says so. The comparison says it by its
 * headline (`otherApplication`) and its record.
 */
export function replacesOtherApplication(comparison: Comparison | undefined): boolean {
  return comparison !== undefined && comparison.row === 'otherApplication' && comparison.lastDownload !== null;
}
