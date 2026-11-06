/**
 * Download to the connected controller (ADR-0088, stage 5d): the editor action
 * that replaces the application on the controller with the one compiled from
 * the project. It is the `replace` mode of the same pipeline every edit takes
 * (accept, test, assemble), so the device decides what survives: the `PERSISTENT`
 * variables whose declaration is equal keep their values and every other
 * variable starts again. The action only sequences, and shows the person
 * what the device said before the replacement runs.
 *
 * The invariants of every member of the class:
 * - the container is compiled once, after the stable variable IDs are synced,
 *   because the IDs are what lets a `PERSISTENT` variable be matched;
 * - the container is sent in the replace mode over the open engineering
 *   connection, and the person sees every `PERSISTENT` variable the device will
 *   re-initialise before the replacement runs;
 * - the steps after the accept are the ones the device's edit view names
 *   (`next`, bounded by the steps of `finalizeAll`); the client decides no
 *   enablement, and a command the view disables is shown with the view's reason
 *   and not sent;
 * - the download record is written only after the device acknowledged the
 *   assemble (`DownloadIo.acknowledge`).
 *
 * The actions are the members of one table, `DOWNLOAD_ACTIONS`: a later one (a
 * Download that keeps the application running, chosen in the go-online
 * comparison) is one more entry here and one more wire mode in the session
 * client's mirror.
 *
 * The module is vscode-free: the session is injected as the minimal command
 * surface and the rest (the sync of the IDs, the compile, the confirmation, the
 * record) as callbacks, so unit tests record the exact call order with a fake.
 */

import {
  AcceptMode,
  EditIdentity,
  EditView,
  HotEditProtocolError,
  HotEditSession,
  HotEditStatus,
  Reinitialised,
  ReinitReason,
  ReplaceNotSupportedError,
} from './hotEditSession';
import { BuildPhase } from './buildLogic';
import { candidatesOf, SyncReport } from './syncUidsLogic';

/** The editor actions that deploy a build by replacing the application. */
export type DownloadAction = 'download';

/** One member of the class: its command, how it is named to the person, and the mode it sends. */
export interface DownloadActionDef {
  /** The id of the contributed command. */
  commandId: string;
  /** The title of the contributed command (category `IronPLC`). */
  title: string;
  /** The `mode` of the accept it sends. */
  mode: Extract<AcceptMode, 'replace'>;
}

/** The table of the class. A member is checked for its contribution by `checkInvariants`. */
export const DOWNLOAD_ACTIONS: { readonly [A in DownloadAction]: DownloadActionDef } = {
  download: { commandId: 'ironplc.download', title: 'Download to Controller', mode: 'replace' },
};

/** The members of the class, in the table's order. */
export function downloadActionNames(): DownloadAction[] {
  return Object.keys(DOWNLOAD_ACTIONS) as DownloadAction[];
}

/** The part of the session a Download uses; `HotEditSession` satisfies it. */
export type DownloadSession = Pick<
  HotEditSession,
  'acceptReplace' | 'testEdits' | 'assembleEdits' | 'cancelEdits' | 'getStatus'
>;

/** What the compile step produced: at least the bytes to send. */
export interface SentBytes {
  bytes: Uint8Array;
}

/** What the person is asked to confirm before the replacement runs. */
export interface ReplaceConfirmation {
  /** The name of the program that is downloaded (the edit label). */
  program: string | undefined;
  /** The `PERSISTENT` variables the device will re-initialise. */
  reinitialised: readonly Reinitialised[];
  /**
   * Whether the application on the device is not the one this project last
   * downloaded to it (the check of the download record): a Download replaces
   * it all the same, and the person is told what is lost.
   */
  deviceChanged: boolean;
}

/** The steps of a Download that are not session commands, injected by the shell. */
export interface DownloadIo<S extends SentBytes> {
  /**
   * Syncs the stable variable IDs of the project. False when the person left
   * rename or swap decisions open: the IDs are not fully synced and the Download
   * stops. A program that is a compiled container has no sources to sync.
   */
  syncIds(): Promise<boolean>;
  /** Compiles the program (or reads the container), once. */
  compile(): Promise<S>;
  /** Shows what the replacement will do; true when the person continues. */
  confirm(request: ReplaceConfirmation): Promise<boolean>;
  /** The device acknowledged the assemble: the one place that records the download. */
  acknowledge(sent: S, status: HotEditStatus): Promise<void>;
  /** The client-side phase, rendered by the device panel. */
  onPhase(phase: BuildPhase): void;
}

/**
 * The sync step of a Download: true when the stable variable IDs are fully
 * synced, so the container carries them. A program that is already a container
 * has no sources to sync. A report that still lists rename or swap candidates
 * means the person left decisions open, and the Download stops.
 */
export async function syncIdsStep(isContainer: boolean, sync: () => Promise<SyncReport>): Promise<boolean> {
  if (isContainer) {
    return true;
  }
  return candidatesOf(await sync()).length === 0;
}

/** The edit commands this client sends, by the wire name the view gives them. */
const SENDERS: Readonly<Record<string, (session: DownloadSession) => Promise<void>>> = {
  test: session => session.testEdits(),
  assemble: session => session.assembleEdits(),
};

/** Where in the sequence a refusal happened: the wire name of the command, or `accept`. */
export type DownloadStep = string;

/**
 * What a Download came to:
 * - `syncOpen`: rename or swap decisions of the ID sync were left open, nothing
 *   was sent;
 * - `cancelled`: the person did not continue after seeing the list; the
 *   candidate was discarded when the device allowed it;
 * - `done`: the device acknowledged the assemble and `status` is what it
 *   reported, `reinitialised` what it re-initialised;
 * - `refused`: the device refused `step` (or does not support the mode) with
 *   `error`; `status` is the state the device reported afterwards, and
 *   `discarded` says whether the candidate was cancelled;
 * - `stalled`: the device's view offers no way on (a device without the view, a
 *   swap that waits for a boundary, a command this client does not send); `status`
 *   is what it reported.
 */
export type DownloadOutcome
  = | { kind: 'syncOpen' }
    | { kind: 'cancelled'; discarded: boolean; status?: HotEditStatus }
    | { kind: 'done'; status: HotEditStatus; reinitialised: readonly Reinitialised[] }
    | { kind: 'refused'; step: DownloadStep; error: HotEditProtocolError; discarded: boolean; status?: HotEditStatus }
    | { kind: 'stalled'; status: HotEditStatus };

/** Words for the reasons a `PERSISTENT` variable is re-initialised; a member is required for each reason. */
export const REINIT_REASON_TEXT: { readonly [R in ReinitReason]: string } = {
  noStableId: 'it has no stable variable ID, so it cannot be matched with the new application',
  notInCandidate: 'it is not in the new application',
  qualifierChanged: 'it is no longer declared PERSISTENT in the new application',
  typeChanged: 'its type changed',
  unsupportedType: 'its type cannot be carried over yet',
};

/** The sentence for a reason, also for a reason this client does not know (a later device's). */
export function reinitReasonText(reason: string): string {
  return (REINIT_REASON_TEXT as Readonly<Record<string, string | undefined>>)[reason] ?? `reason: ${reason}`;
}

/** How a re-initialised variable is named: its name, else its ID, else that it has neither. */
function variableLabel(entry: Reinitialised): string {
  if (entry.name !== undefined) {
    return entry.name;
  }
  return entry.uid === undefined ? 'a variable without a name or an ID' : `variable ${entry.uid}`;
}

/** The text of the confirmation before the replacement runs: a headline and the detail. */
export function formatReplaceConfirmation(request: ReplaceConfirmation): { message: string; detail: string } {
  const program = request.program === undefined ? 'the project' : request.program;
  const paragraphs = [
    `The application on the controller is replaced by ${program} and starts again with the initial values of its variables.`
    + ' Only PERSISTENT variables whose declaration did not change keep their values.'
    + ' All other variables, RETAIN variables included, start again.',
  ];
  if (request.reinitialised.length === 0) {
    paragraphs.push('No PERSISTENT variable is re-initialised: all of them keep their values.');
  }
  else {
    paragraphs.push(
      'These PERSISTENT variables are re-initialised:\n'
      + request.reinitialised.map(entry => `  - ${variableLabel(entry)}: ${reinitReasonText(entry.reason)}`).join('\n'),
    );
  }
  if (request.deviceChanged) {
    paragraphs.push(
      'The application on the controller is not the one this project last downloaded to it. Whatever was changed there is lost.',
    );
  }
  paragraphs.push('The replacement cannot be undone once it runs.');
  return { message: 'Download replaces the application on the controller.', detail: paragraphs.join('\n\n') };
}

/**
 * Tries to discard a candidate that is staged: sends cancel when, and only when,
 * the device's own view allows it. Never throws: it runs while another outcome is
 * being reported, and the state the device reports afterwards is what is shown.
 */
async function discardCandidate(session: DownloadSession): Promise<{ discarded: boolean; status?: HotEditStatus }> {
  let status: HotEditStatus | undefined;
  try {
    status = await session.getStatus();
    if (status.edit?.commands.cancel?.enabled !== true) {
      return { discarded: false, status };
    }
    await session.cancelEdits();
    return { discarded: true, status: await session.getStatus() };
  }
  catch {
    return { discarded: false, ...(status !== undefined ? { status } : {}) };
  }
}

/** The refusal the view gives for a command that is not enabled. */
function refusalOf(view: EditView, command: string): HotEditProtocolError {
  const reason = view.commands[command]?.reason;
  return reason === undefined
    ? new HotEditProtocolError(null, `the device does not allow ${command} now`)
    : new HotEditProtocolError(reason.code, reason.text);
}

/**
 * Runs the steps after the accept as the device's edit view gives them: while a
 * candidate is staged, send the view's `next` command, then read the status
 * again. The count of steps is bounded by the steps of `finalizeAll`, so a device
 * that never reaches `idle` cannot loop the client.
 */
async function followEdit(
  session: DownloadSession,
  first: HotEditStatus,
  reinitialised: readonly Reinitialised[],
  onStep: (command: string) => void,
): Promise<DownloadOutcome> {
  let status = first;
  let last: string | undefined;
  const bound = status.edit?.commands.finalizeAll?.steps?.length ?? 0;
  for (let taken = 0; taken <= bound; taken++) {
    const view = status.edit;
    if (view === undefined) {
      return { kind: 'stalled', status };
    }
    if (view.stage === 'idle') {
      // The edit is over. It ended in the assemble only if that is what was
      // sent last: a candidate the device lost on the way is not a download.
      return last === 'assemble' ? { kind: 'done', status, reinitialised } : { kind: 'stalled', status };
    }
    const command = view.next?.command;
    const send = command === undefined ? undefined : SENDERS[command];
    if (command === undefined || send === undefined || taken === bound) {
      return { kind: 'stalled', status };
    }
    if (view.commands[command]?.enabled !== true) {
      const { discarded, status: after } = await discardCandidate(session);
      return { kind: 'refused', step: command, error: refusalOf(view, command), discarded, status: after ?? status };
    }
    onStep(command);
    last = command;
    try {
      await send(session);
    }
    catch (err) {
      if (!(err instanceof HotEditProtocolError)) {
        throw err;
      }
      const { discarded, status: after } = await discardCandidate(session);
      return { kind: 'refused', step: command, error: err, discarded, ...(after !== undefined ? { status: after } : {}) };
    }
    status = await session.getStatus();
  }
  return { kind: 'stalled', status };
}

/**
 * Downloads the program: sync the IDs, compile, accept in the replace mode, ask
 * the person to confirm what will be re-initialised, then run the steps the
 * device's view names (test, assemble) and hand the acknowledged assemble to
 * `io.acknowledge`. Errors that are not the device's coded refusals (the compile
 * failing, the transport) are thrown to the caller.
 */
export async function runDownload<S extends SentBytes>(
  session: DownloadSession,
  io: DownloadIo<S>,
  edit: EditIdentity,
  deviceChanged: boolean,
): Promise<DownloadOutcome> {
  if (!(await io.syncIds())) {
    return { kind: 'syncOpen' };
  }
  io.onPhase('compiling');
  const sent = await io.compile();

  io.onPhase('uploading');
  let reinitialised: Reinitialised[];
  try {
    reinitialised = await session.acceptReplace(sent.bytes, edit);
  }
  catch (err) {
    if (err instanceof ReplaceNotSupportedError) {
      // The device staged the container as an ordinary edit: do not leave it.
      const { discarded, status } = await discardCandidate(session);
      return { kind: 'refused', step: 'accept', error: err, discarded, ...(status !== undefined ? { status } : {}) };
    }
    if (err instanceof HotEditProtocolError) {
      return { kind: 'refused', step: 'accept', error: err, discarded: false };
    }
    throw err;
  }

  if (!(await io.confirm({ program: edit.name, reinitialised, deviceChanged }))) {
    const { discarded, status } = await discardCandidate(session);
    return { kind: 'cancelled', discarded, ...(status !== undefined ? { status } : {}) };
  }

  const outcome = await followEdit(session, await session.getStatus(), reinitialised, (command) => {
    io.onPhase(command === 'assemble' ? 'running' : 'verifying');
  });
  if (outcome.kind === 'done') {
    await io.acknowledge(sent, outcome.status);
  }
  return outcome;
}

/** How the shell shows an outcome: a level, the text, and the code of a coded refusal. */
export interface OutcomeMessage {
  level: 'info' | 'warning' | 'error';
  text: string;
  /** The V-code of a refusal; the text then starts with it. */
  vCode?: string;
}

/** The sentence about the edit view's state, when the device reported one. */
function stateSentence(status: HotEditStatus | undefined): string {
  const view = status?.edit;
  if (view === undefined) {
    return '';
  }
  const next = view.next === undefined ? '' : ` The next step the device offers is ${view.next.label}.`;
  return ` The edit is in the stage ${view.stage}.${next}`;
}

/** The message for an outcome, with the device's own reason and the state it reported. */
export function outcomeMessage(outcome: DownloadOutcome, title: string, formatStatus: (status: HotEditStatus) => string): OutcomeMessage {
  switch (outcome.kind) {
    case 'syncOpen':
      return {
        level: 'warning',
        text: `IronPLC ${title}: the variable IDs are not fully synced, so nothing was sent. Run the command again to resolve the open rename and swap candidates.`,
      };
    case 'cancelled':
      return {
        level: 'info',
        text: outcome.discarded
          ? `IronPLC ${title}: cancelled. The application on the controller is unchanged.`
          : `IronPLC ${title}: cancelled, but the device kept the candidate.${stateSentence(outcome.status)}`,
      };
    case 'done': {
      const reinit = outcome.reinitialised.length;
      const kept = reinit === 0
        ? 'All PERSISTENT variables kept their values.'
        : `${reinit} PERSISTENT variable${reinit === 1 ? ' was' : 's were'} re-initialised.`;
      return { level: 'info', text: `IronPLC ${title}: the application is replaced (${formatStatus(outcome.status)}). ${kept}` };
    }
    case 'refused': {
      const fate = outcome.discarded
        ? ' The candidate was discarded; the application on the controller is unchanged.'
        : ` The candidate was not discarded.${stateSentence(outcome.status)}`;
      const { vCode } = outcome.error;
      // A coded text starts with its code, as the link to the page of the code needs.
      const text = vCode === null
        ? `IronPLC ${title}: ${outcome.error.toString()} (at ${outcome.step}).${fate}`
        : `${outcome.error.toString()}. IronPLC ${title} stopped at ${outcome.step}.${fate}`;
      return { level: 'error', text, ...(vCode !== null ? { vCode } : {}) };
    }
    case 'stalled':
      return {
        level: 'warning',
        text: `IronPLC ${title}: stopped, because the device's edit view offers no next step.${stateSentence(outcome.status)} Nothing was recorded.`,
      };
  }
}
