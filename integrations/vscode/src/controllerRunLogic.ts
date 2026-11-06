/**
 * Run and Stop of a connected controller (ADR-0086, stage 3c). The commands
 * change whether the device executes scan rounds and nothing else: they send
 * `startScan` or `stopScan` over the session of the engineering connection and
 * compile nothing. They are a mechanism apart from Build, and apart from the
 * local simulation (`ironplc.runProgram` and its siblings), which compiles the
 * editor's source in the language server and never reaches a device.
 *
 * The run state shown, and whether a command succeeded, are the device's
 * answers (`runState` of the answer, `runState` and `fault` of the identity);
 * this module keeps no state of its own. The device decides every refusal: a
 * start of a faulted device is V6019, a host without a scan loop is V4022.
 * Vscode-free.
 *
 * The commands are the members of one table, `SCAN_ACTIONS`: a later command of
 * the class (reset) is one more entry here and one more wire name in the
 * session client's mirror.
 */

import { DeviceFault, HotEditProtocolError, IdentityInfo, RunState, runStateOf } from './hotEditSession';

/** The commands that change whether the connected controller scans. */
export type ScanAction = 'run' | 'stop';

/** The session commands a scan action sends: the ones of `HotEditSession` that answer a run state. */
export type ScanWireName = 'startScan' | 'stopScan';

/** The part of the session that Run and Stop use; `HotEditSession` satisfies it. */
export type ScanSession = { [W in ScanWireName]: () => Promise<RunState> } & {
  identity(): Promise<IdentityInfo>;
};

/** One member of the class: its command, its wire name and how it is named to the person. */
export interface ScanActionDef {
  /** The id of the contributed command. */
  commandId: string;
  /** The title of the contributed command (category `IronPLC`). */
  title: string;
  /** The session command it sends. */
  wire: ScanWireName;
}

/** The table of the class. A member is checked for its contribution by `checkInvariants`. */
export const SCAN_ACTIONS: { readonly [A in ScanAction]: ScanActionDef } = {
  run: { commandId: 'ironplc.runController', title: 'Run Controller', wire: 'startScan' },
  stop: { commandId: 'ironplc.stopController', title: 'Stop Controller', wire: 'stopScan' },
};

/** The members of the class, in the table's order. */
export function scanActionNames(): ScanAction[] {
  return Object.keys(SCAN_ACTIONS) as ScanAction[];
}

/**
 * What the device last said about scanning: whether it runs and, while it is
 * faulted, the fault. A faulted device reports `running` beside its fault, so
 * the fault, when there is one, is what the person is told first.
 */
export interface ControllerRun {
  runState: RunState;
  fault?: DeviceFault;
}

/** The run view an identity answer carries (an absent `runState` is `running`). */
export function runViewOf(identity: IdentityInfo): ControllerRun {
  return {
    runState: runStateOf(identity),
    ...(identity.fault !== undefined ? { fault: identity.fault } : {}),
  };
}

/** The run view of the model: the last answer to Run or Stop, else the identity of the connection; none before either. */
export function currentRun(model: {
  run?: ControllerRun | null;
  identity: IdentityInfo | null;
}): ControllerRun | null {
  return model.run ?? (model.identity === null ? null : runViewOf(model.identity));
}

/** The word for the status item: `running`, `stopped`, or `faulted V4001`. */
export function runStateText(run: ControllerRun): string {
  return run.fault === undefined ? run.runState : `faulted ${run.fault.vCode}`;
}

/** The lines of the status tooltip about scanning; the fault is described in full. */
export function runStateDetail(run: ControllerRun): string[] {
  const lines = [`Run state: ${run.runState}`];
  if (run.fault !== undefined) {
    const fault = run.fault;
    lines.push(`Fault: ${fault.vCode} - ${fault.message} (task ${fault.taskId}, instance ${fault.instanceId})`);
  }
  return lines;
}

/**
 * What a Run or Stop came to:
 * - `noSession`: no controller is connected, nothing was sent;
 * - `done`: the device answered with the state it left, and `run` is what to
 *   show from now on;
 * - `refused`: the device refused or the transport failed. `vCode` is the code
 *   of the refusal (null for a transport error) and `text` starts with it.
 *   `run` is the state the device reported afterwards, when it could be asked.
 */
export type ScanOutcome
  = | { kind: 'noSession'; message: string }
    | { kind: 'done'; run: ControllerRun; message: string }
    | { kind: 'refused'; vCode: string | null; text: string; run?: ControllerRun };

/** The message when no controller is connected (the one the build commands show). */
export const NO_CONTROLLER_MESSAGE = 'No device is connected. Run "IronPLC: Connect to Device" first.';

/** The sentence that says what a successful command left. */
export function doneMessage(run: ControllerRun): string {
  if (run.fault !== undefined) {
    return `IronPLC: the controller is ${run.runState}; the fault ${run.fault.vCode} stays reported.`;
  }
  return `IronPLC: the controller is ${run.runState}.`;
}

/**
 * Sends the action's command over `session` and reports what came of it. Sends
 * nothing without a session. After an answer or a coded refusal the identity is
 * asked for, because the answer to a command does not carry the fault (a stop
 * of a faulted device leaves the fault reported; a refused start is a fault);
 * when that fails the answer alone is the state, with no fault known.
 */
export async function runScanAction(
  session: ScanSession | undefined,
  action: ScanAction,
): Promise<ScanOutcome> {
  if (session === undefined) {
    return { kind: 'noSession', message: NO_CONTROLLER_MESSAGE };
  }
  let answered: RunState;
  try {
    answered = await session[SCAN_ACTIONS[action].wire]();
  }
  catch (err) {
    if (err instanceof HotEditProtocolError) {
      const run = err.vCode === null ? undefined : await askRun(session);
      return { kind: 'refused', vCode: err.vCode, text: err.toString(), ...(run !== undefined ? { run } : {}) };
    }
    return { kind: 'refused', vCode: null, text: err instanceof Error ? err.message : String(err) };
  }
  const run = (await askRun(session)) ?? { runState: answered };
  return { kind: 'done', run, message: doneMessage(run) };
}

/** The run view the device reports now; undefined when it cannot be asked. */
async function askRun(session: ScanSession): Promise<ControllerRun | undefined> {
  try {
    return runViewOf(await session.identity());
  }
  catch {
    return undefined;
  }
}
