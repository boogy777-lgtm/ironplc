/**
 * The actions of a device row, and the one function that decides whether each
 * is available.
 *
 * Whether an action works is IronLink's and IronPLC's to say, not the view's:
 * `decideDeviceAction` maps the row and the context (which IronPLC commands
 * exist) to enabled, or to unavailable with a reason, and every place that
 * offers an action asks it. The decision is shown when an action is invoked:
 * the commands are always on the row, and an unavailable one reports its
 * reason instead of running. A `when` clause could only see the row's
 * `contextValue`, which would make a second, weaker copy of the rule.
 *
 * Vscode-free, so the whole table of decisions is unit-tested.
 */

import { DeviceRowFacts } from './networkTree';
import {
  ControllerArgument,
  HANDOFF_ACTIONS,
  HandoffName,
  isHandoffAvailable,
} from './vscodeCommon';

/** An action on a device row. */
export type DeviceAction = 'identify' | 'copyDeviceId' | 'choose' | 'connect';

/** The command id of each action; the invariants check keeps `package.json` in step with it. */
export const DEVICE_ACTION_COMMANDS: { readonly [A in DeviceAction]: string } = {
  identify: 'ironlink.identify',
  copyDeviceId: 'ironlink.copyDeviceId',
  choose: 'ironlink.choose',
  connect: 'ironlink.connect',
};

/** What the decision depends on besides the row. */
export interface ActionContext {
  /** The ids of the commands registered in the editor (`getCommands(true)`). */
  registeredCommands: ReadonlySet<string>;
  /**
   * The ids of the commands that installed extensions contribute in their
   * manifests: an extension that is installed but not yet active has registered
   * nothing, and running a contributed command activates it.
   */
  contributedCommands: ReadonlySet<string>;
}

/** The decision for one action on one row. */
export type ActionDecision =
  | { enabled: true }
  | { enabled: false; reason: string };

/** The reason shown when an action is invoked without a device row. */
export const NO_DEVICE_ROW_REASON = 'Select a controller in the Network view.';

/**
 * The handoff action each device action performs, if it is one: its
 * availability is that of the IronPLC command it hands off to.
 */
function handoffOf(action: DeviceAction): HandoffName | undefined {
  switch (action) {
    case 'choose':
      return 'choose';
    case 'connect':
      return 'connect';
    case 'identify':
    case 'copyDeviceId':
      return undefined;
    default:
      return assertNever(action);
  }
}

function assertNever(value: never): never {
  throw new Error(`unhandled device action: ${String(value)}`);
}

/**
 * Whether `action` is available on `row`. `row` is undefined when the command
 * ran without a device row (from the command palette).
 */
export function decideDeviceAction(
  action: DeviceAction,
  row: DeviceRowFacts | undefined,
  context: ActionContext,
): ActionDecision {
  if (row === undefined) {
    return { enabled: false, reason: NO_DEVICE_ROW_REASON };
  }
  const handoff = handoffOf(action);
  if (handoff !== undefined && !isHandoffAvailable(handoff, { registered: context.registeredCommands, contributed: context.contributedCommands })) {
    return { enabled: false, reason: HANDOFF_ACTIONS[handoff].unavailableReason };
  }
  return { enabled: true };
}

/** The argument of a handoff for a row: exactly the device id, address and session port. */
export function controllerArgument(row: DeviceRowFacts): ControllerArgument {
  return { deviceId: row.deviceId, address: row.address, sessionPort: row.sessionPort };
}
