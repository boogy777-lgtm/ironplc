/**
 * Connecting to the controller of the project (ADR-0084). The connection goes
 * through the table of connection methods like any other: a controller is
 * reached by an unsaved `tcp` profile built from its route, so there is no
 * second way to connect. This module holds the decisions around it and nothing
 * that needs the editor:
 *
 * - which route the chosen controller is reached by (the workstation route of
 *   IronLink first, then the route hint of the project, else a reason);
 * - the profile that route makes;
 * - the information lines when the controller that answers is not the one that
 *   was meant or chosen. Connect never refuses on identity (ADR-0084): the lines
 *   are information.
 */

import { ChosenController } from './chooseControllerLogic';
import { ConnectionProfile } from './connectionProfiles';
import { ControllerArgument, IronLinkRoute, shortDeviceId, socketAddress } from './vscodeCommon';

/** Where the route to the chosen controller came from. */
export type RouteSource = 'workstation' | 'project';

/** The route to the chosen controller, or why there is none. */
export type ChosenRoute =
  | { route: ControllerArgument; source: RouteSource }
  | { reason: string };

/** The words that say where a route came from, for the connection output. */
export const ROUTE_SOURCE_TEXT: { readonly [S in RouteSource]: string } = {
  workstation: 'the route stored on this PC',
  project: 'the route hint of the project',
};

/**
 * The route by which the chosen controller is reached, in this order:
 *
 * 1. the workstation route for its device id (IronLink), when IronLink is
 *    present (`workstationRoutes` is undefined when it is not);
 * 2. the route hint of the path file;
 * 3. otherwise a reason.
 */
export function resolveChosenRoute(
  chosen: ChosenController | undefined,
  workstationRoutes: readonly IronLinkRoute[] | undefined,
): ChosenRoute {
  if (chosen === undefined) {
    return { reason: 'No controller is chosen for this project.' };
  }
  const stored = workstationRoutes?.find(route => route.deviceId === chosen.deviceId);
  if (stored !== undefined) {
    return {
      route: { deviceId: chosen.deviceId, address: stored.address, sessionPort: stored.sessionPort },
      source: 'workstation',
    };
  }
  if (chosen.routeHint !== undefined) {
    return {
      route: { deviceId: chosen.deviceId, address: chosen.routeHint.address, sessionPort: chosen.routeHint.sessionPort },
      source: 'project',
    };
  }
  return {
    reason: `No route to the chosen controller ${shortDeviceId(chosen.deviceId)} is known: this PC stores none`
      + `${workstationRoutes === undefined ? ' (the IronLink extension is not available)' : ''}, and the project has no address for it.`,
  };
}

/**
 * The unsaved `tcp` profile that reaches `controller`: named by the device name
 * when it is known, else the short device id, with the route's address and the
 * session port. It is validated like a saved profile, by `validateProfiles`.
 */
export function controllerProfile(controller: ControllerArgument, name?: string): ConnectionProfile {
  return {
    name: name !== undefined && name.trim().length > 0 ? name : shortDeviceId(controller.deviceId),
    transport: 'tcp',
    address: controller.address,
    port: controller.sessionPort,
  };
}

/** What the connected controller is compared with. */
export interface IdentityComparison {
  /** The device id the connected controller reports; undefined when it reports none. */
  connected: string | undefined;
  /** The device id of the route that was connected to; undefined for a saved profile. */
  intended: string | undefined;
  /** The device id of the controller chosen for the project; undefined when none is. */
  chosen: string | undefined;
}

/**
 * The information lines for a connected controller: none when it is the one
 * that was meant and chosen, and a line for each difference otherwise. A
 * controller that reports no device id is not compared (nothing is known of it).
 */
export function identityNotes(comparison: IdentityComparison): string[] {
  const { connected, intended, chosen } = comparison;
  if (connected === undefined) {
    return [];
  }
  const notes: string[] = [];
  if (intended !== undefined && connected !== intended) {
    notes.push(
      `The controller that answered (${shortDeviceId(connected)}) is not the one expected at this address (${shortDeviceId(intended)}).`,
    );
  }
  if (chosen !== undefined && connected !== chosen) {
    notes.push(
      `The connected controller (${shortDeviceId(connected)}) is not the controller chosen for this project (${shortDeviceId(chosen)}).`,
    );
  }
  return notes;
}

/** The line for the connection output that says what is being connected to. */
export function connectingText(profile: ConnectionProfile, source: string): string {
  return `Connecting to ${profile.name} at ${socketAddress(profile.address ?? '', profile.port ?? 0)} by ${source}.`;
}
