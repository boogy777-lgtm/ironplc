/**
 * Runs the item the person selected in the online-edit dialog (stage 6 of the
 * online model). The model (`onlineEditLogic`) says what an item sends; this
 * module sends it, and decides nothing else.
 *
 * The invariants of every run:
 * - the commands are the item's: one command, or the steps the view names for
 *   Finalize all. The two-step Cancel under Test is one command, `cancelEdits`:
 *   the device runs its steps (ADR-0087);
 * - before every device command of a sequence after the first, the view is read
 *   again and the command is sent only when that view enables it; a command it
 *   disables is shown with the view's reason and not sent;
 * - whatever the outcome, the view is read again at the end, so the dialog is
 *   rendered from the device's state and every failure ends in a state whose
 *   exits are shown;
 * - the acknowledged assemble is handed to `OnlineEditIo.acknowledge` once, the
 *   one place that records the download.
 *
 * A command the client can send is one entry of `EDIT_SENDERS`. The module is
 * vscode-free: the session is the minimal command surface and Build, Accept and
 * the record are injected, so unit tests record the exact call order with a fake.
 */

import { BuildPhase } from './buildLogic';
import { OutcomeMessage } from './downloadLogic';
import { EditView, HotEditProtocolError, HotEditSession, HotEditStatus } from './hotEditSession';
import { BUILD_STEP, DialogItem, phaseOf, stageLabel } from './onlineEditLogic';

/** The part of the session the dialog uses; `HotEditSession` satisfies it. */
export type DialogSession = Pick<
  HotEditSession,
  'testEdits' | 'untestEdits' | 'assembleEdits' | 'cancelEdits' | 'getStatus'
>;

/** The steps of a run that are not plain session commands, injected by the shell. */
export interface OnlineEditIo {
  /** Compiles the program; true when the build succeeded. A failure is already shown to the person. */
  build(): Promise<boolean>;
  /**
   * Stages the container of the build in the change mode. True when it is
   * staged; false when the step stopped before anything was sent and has told
   * the person why (a stale baseline, a refused decision). A refusal of the
   * device is thrown as the coded error.
   */
  accept(): Promise<boolean>;
  /** The device acknowledged the assemble: the one place that records the download. */
  acknowledge(status: HotEditStatus): Promise<void>;
  /** The client-side phase, rendered by the device panel; null when the run ends. */
  onPhase(phase: BuildPhase | null): void;
}

/** The edit commands the client sends with the session alone, by the wire name the view gives them. */
const EDIT_SENDERS: Readonly<Record<string, (session: DialogSession) => Promise<void>>> = {
  test: session => session.testEdits(),
  untest: session => session.untestEdits(),
  assemble: session => session.assembleEdits(),
  cancel: session => session.cancelEdits(),
};

/**
 * What a run came to:
 * - `done`: every step ran; `assembled` says whether the device acknowledged an assemble;
 * - `stopped`: `step` stopped before it sent anything and has told the person;
 * - `refused`: the device (or its view) refused `step`, with `error`;
 * - `failed`: `step` failed for a reason that is not the device's refusal (the transport, the disk).
 */
export type RunOutcome
  = | { kind: 'done'; assembled: boolean }
    | { kind: 'stopped'; step: string }
    | { kind: 'refused'; step: string; error: HotEditProtocolError }
    | { kind: 'failed'; step: string; error: Error };

/** An outcome and the state the device reported at the end, when it could be read. */
export interface RunResult {
  outcome: RunOutcome;
  status?: HotEditStatus;
}

/** The refusal the view gives for a command that is not enabled. */
function refusalOf(view: EditView | undefined, command: string): HotEditProtocolError {
  const reason = view?.commands[command]?.reason;
  return reason === undefined
    ? new HotEditProtocolError(null, `the device does not allow ${command} now`)
    : new HotEditProtocolError(reason.code, reason.text);
}

/** The commands an item sends, in order. */
function stepsOf(item: DialogItem): readonly string[] {
  if (!item.enabled) {
    return [];
  }
  return item.steps ?? (item.command === undefined ? [] : [item.command]);
}

/** Sends one step; true when it ran, false when it stopped and told the person. */
async function send(session: DialogSession, io: OnlineEditIo, step: string): Promise<boolean> {
  if (step === BUILD_STEP) {
    return io.build();
  }
  if (step === 'accept') {
    return io.accept();
  }
  const sender = EDIT_SENDERS[step];
  if (sender === undefined) {
    throw new HotEditProtocolError(null, `this client cannot send ${step}`);
  }
  await sender(session);
  return true;
}

/** Reads the status, or undefined when the device cannot be read: the outcome is what is reported then. */
async function readQuietly(session: DialogSession): Promise<HotEditStatus | undefined> {
  try {
    return await session.getStatus();
  }
  catch {
    return undefined;
  }
}

/**
 * Runs the steps of `item`. A step after a device command is sent only when
 * the view, read again, enables it.
 */
async function runSteps(session: DialogSession, io: OnlineEditIo, steps: readonly string[]): Promise<RunOutcome> {
  let sentToDevice = false;
  let assembled = false;
  for (const step of steps) {
    try {
      if (sentToDevice && step !== BUILD_STEP) {
        const view = (await session.getStatus()).edit;
        if (view?.commands[step]?.enabled !== true) {
          return { kind: 'refused', step, error: refusalOf(view, step) };
        }
      }
      const phase = phaseOf(step);
      if (phase !== undefined) {
        io.onPhase(phase);
      }
      if (!(await send(session, io, step))) {
        return { kind: 'stopped', step };
      }
      sentToDevice = sentToDevice || step !== BUILD_STEP;
      if (step === 'assemble') {
        assembled = true;
        await io.acknowledge(await session.getStatus());
      }
    }
    catch (err) {
      return err instanceof HotEditProtocolError
        ? { kind: 'refused', step, error: err }
        : { kind: 'failed', step, error: err instanceof Error ? err : new Error(String(err)) };
    }
  }
  return { kind: 'done', assembled };
}

/**
 * Runs `item` and reads the view again. Never throws: a failure is the outcome.
 */
export async function runItem(session: DialogSession, io: OnlineEditIo, item: DialogItem): Promise<RunResult> {
  const outcome = await runSteps(session, io, stepsOf(item));
  io.onPhase(null);
  const status = await readQuietly(session);
  return { outcome, ...(status !== undefined ? { status } : {}) };
}

/**
 * The message for a run, or undefined when the dialog's own state says it all:
 * a run that stopped has told the person already, and a step that is not an
 * assemble needs no popup because the dialog shows the next stage.
 */
export function runMessage(result: RunResult, formatStatus: (status: HotEditStatus) => string): OutcomeMessage | undefined {
  const { outcome } = result;
  const view = result.status?.edit;
  switch (outcome.kind) {
    case 'done':
      return outcome.assembled && result.status !== undefined
        ? { level: 'info', text: `IronPLC Online Edit: the candidate is assembled (${formatStatus(result.status)}).` }
        : undefined;
    case 'stopped':
      return undefined;
    case 'refused': {
      const { vCode } = outcome.error;
      const at = stageLabel(outcome.step, view);
      // A coded text starts with its code, as the link to the page of the code needs.
      return vCode === null
        ? { level: 'error', text: `IronPLC Online Edit: ${outcome.error.toString()} (at ${at}).` }
        : { level: 'error', text: `${outcome.error.toString()}. IronPLC Online Edit stopped at ${at}.`, vCode };
    }
    case 'failed':
      return { level: 'error', text: `IronPLC Online Edit: ${outcome.error.message} (at ${stageLabel(outcome.step, view)}).` };
  }
}
