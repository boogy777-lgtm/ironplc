/**
 * What "Choose" does on a device row, in order: first the route of this
 * workstation, then the handoff to IronPLC.
 *
 * The route is IronLink's own data and is verified by the program (an identify
 * that must return the device id). The project path file is IronPLC's, so the
 * handoff comes only after the route is set: a route that cannot be verified
 * leaves everything as it was and nothing is handed off. Vscode-free; the two
 * steps are injected.
 */

import { controllerArgument } from './deviceActions';
import { DeviceRowFacts } from './networkTree';
import { ChooseControllerArgument, RouteFailure, RouteOutcome, RouteSet, socketAddress } from './vscodeCommon';

/** The outcome of Choose. */
export type ChooseOutcome =
  | { handedOff: true; route: RouteSet }
  | { handedOff: false; failure: RouteFailure };

/**
 * Sets the route of `row`, and only when that succeeds hands the row off. A
 * failure of the handoff itself reaches the caller as it is.
 */
export async function chooseDevice(
  row: DeviceRowFacts,
  setRoute: (deviceId: string, address: string) => Promise<RouteOutcome>,
  handoff: (argument: ChooseControllerArgument) => Promise<void>,
): Promise<ChooseOutcome> {
  const route = await setRoute(row.deviceId, socketAddress(row.address, row.sessionPort));
  if (!route.ok) {
    return { handedOff: false, failure: route };
  }
  await handoff(controllerArgument(row));
  return { handedOff: true, route };
}
