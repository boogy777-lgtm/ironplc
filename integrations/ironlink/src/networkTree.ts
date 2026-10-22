/**
 * The "Network" tree, built from the browse response by a pure function.
 *
 * The rows are text rows, as RSWho shows them: the workstation, under it one
 * row per discovery driver, and under each driver one row per route to a
 * device. A device row's label is the device name; its description holds the
 * address, a short device id, the text markers `route`, `protected` and
 * `conflict`, and the age of the observation. Every decision a marker shows
 * (`routed`, `conflict`, `protected`) is the program's answer in the response;
 * nothing is computed here but wording. Vscode-free, so the whole tree is
 * unit-tested against browse responses.
 */

import {
  BrowseResponse,
  DeviceEntry,
  DeviceVariant,
  DriverReport,
  OtherVersion,
  SeenRoute,
  StoredRoute,
} from './ironlinkProtocol';
import { formatAge, shortDeviceId, socketAddress } from './vscodeCommon';

/** The kind of a row; a row's `contextValue` and the `when` clauses of its actions derive from it. */
export type NodeKind = 'workstation' | 'driver' | 'device' | 'otherVersion' | 'notSeenGroup' | 'notSeenRoute' | 'info';

/** What the actions on a device row need to know about it. */
export interface DeviceRowFacts {
  deviceId: string;
  name: string;
  /** The address the controller answered from (an IP address, no port). */
  address: string;
  sessionPort: number;
  /** More than one identity answers under this device id. */
  conflict: boolean;
  protected: boolean;
  /** The stored route of this workstation is exactly this route. */
  routed: boolean;
}

/** One row of the tree. */
export interface NetworkNode {
  /** Stable across refreshes, so VS Code keeps the expansion state. */
  id: string;
  kind: NodeKind;
  label: string;
  description: string;
  /** The full observation, one fact per line. */
  tooltip: string;
  /** The `viewItem` value that the `when` clauses of the actions test. */
  contextValue: string;
  /** Present on a `device` row. */
  device?: DeviceRowFacts;
  children: NetworkNode[];
}

/** The `contextValue` of a driver row: `driver.<kind>`, so each driver can have its own actions. */
export function driverContextValue(kind: string): string {
  return `driver.${kind}`;
}

/** The `contextValue` of every row kind that is not a driver. */
export const CONTEXT_VALUES = {
  workstation: 'workstation',
  device: 'device',
  otherVersion: 'otherVersion',
  notSeenGroup: 'notSeenGroup',
  notSeenRoute: 'notSeenRoute',
  info: 'info',
} as const;

/** "Address list" for `addressList`: the words of a driver kind, so a new driver needs no table. */
export function driverLabel(kind: string): string {
  const words = kind.replace(/([a-z0-9])([A-Z])/g, '$1 $2').toLowerCase();
  return words.charAt(0).toUpperCase() + words.slice(1);
}

/** The time of an observation in the form the tooltip shows. */
function formatTime(ms: number): string {
  return new Date(ms).toISOString();
}

function plural(count: number, one: string, many: string): string {
  return count === 1 ? one : many;
}

/** Builds the roots of the tree: the workstation row. */
export function buildNetworkTree(browse: BrowseResponse, nowMs: number): NetworkNode[] {
  const children = browse.drivers.map(driver => driverNode(driver, nowMs));
  if (browse.routesNotSeen.length > 0) {
    children.push(notSeenNode(browse.routesNotSeen, nowMs));
  }
  return [{
    id: 'workstation',
    kind: 'workstation',
    label: browse.workstation.name,
    description: 'this workstation',
    tooltip: `Workstation ${browse.workstation.name}`,
    contextValue: CONTEXT_VALUES.workstation,
    children,
  }];
}

function driverNode(driver: DriverReport, nowMs: number): NetworkNode {
  const rows: NetworkNode[] = [];
  for (const device of driver.devices) {
    device.variants.forEach((variant, index) => {
      for (const route of variant.routes) {
        rows.push(deviceNode(driver.kind, device, variant, index, route, nowMs));
      }
    });
  }
  for (const other of driver.otherVersions) {
    rows.push(otherVersionNode(driver.kind, other, nowMs));
  }
  if (rows.length === 0) {
    rows.push({
      id: `${driverId(driver.kind)}:none`,
      kind: 'info',
      label: 'No controller answered',
      description: '',
      tooltip: 'No controller answered this driver.',
      contextValue: CONTEXT_VALUES.info,
      children: [],
    });
  }
  const tooltip = [
    `Driver: ${driverLabel(driver.kind)}`,
    ...driver.targets.map(target => `Target: ${target}`),
    `Datagrams that were not discovery replies: ${driver.undecodable}`,
  ].join('\n');
  return {
    id: driverId(driver.kind),
    kind: 'driver',
    label: driverLabel(driver.kind),
    description: driver.targets.join(', '),
    tooltip,
    contextValue: driverContextValue(driver.kind),
    children: rows,
  };
}

function driverId(kind: string): string {
  return `driver:${kind}`;
}

function deviceNode(
  driverKind: string,
  device: DeviceEntry,
  variant: DeviceVariant,
  variantIndex: number,
  route: SeenRoute,
  nowMs: number,
): NetworkNode {
  const where = socketAddress(route.address, route.sessionPort);
  const markers = [
    route.routed ? 'route' : undefined,
    variant.protected ? 'protected' : undefined,
    device.conflict ? 'conflict' : undefined,
  ].filter((marker): marker is string => marker !== undefined);
  const description = [where, shortDeviceId(device.deviceId), ...markers, formatAge(route.seenAt, nowMs)].join('  ');
  const tooltip = [
    `${variant.name}`,
    `Model: ${variant.model}`,
    `Runtime: ${variant.runtimeVersion}`,
    `Session protocol: ${variant.protocol}`,
    `Device id: ${device.deviceId}`,
    `Credentials required: ${variant.protected ? 'yes' : 'no'}`,
    `Route: ${where} by ${driverLabel(route.driver)} (${route.method})`,
    `Seen: ${formatTime(route.seenAt)} (${formatAge(route.seenAt, nowMs)})`,
    `Stored route: ${route.routed ? 'this route' : 'not this route'}`,
    ...(device.conflict
      ? [`Identity conflict: ${device.variants.length} different answers under this device id`]
      : []),
  ].join('\n');
  return {
    id: `device:${driverKind}:${device.deviceId}:${variantIndex}:${where}`,
    kind: 'device',
    label: variant.name,
    description,
    tooltip,
    contextValue: CONTEXT_VALUES.device,
    device: {
      deviceId: device.deviceId,
      name: variant.name,
      address: route.address,
      sessionPort: route.sessionPort,
      conflict: device.conflict,
      protected: variant.protected,
      routed: route.routed,
    },
    children: [],
  };
}

function otherVersionNode(driverKind: string, other: OtherVersion, nowMs: number): NetworkNode {
  return {
    id: `other:${driverKind}:${other.address}:${other.wireVersion}`,
    kind: 'otherVersion',
    label: `Discovery version ${other.wireVersion}`,
    description: [other.address, 'update IronLink', formatAge(other.seenAt, nowMs)].join('  '),
    tooltip: [
      `A device at ${other.address} answered with discovery version ${other.wireVersion},`,
      'which this IronLink cannot read. Update IronLink.',
      `Seen: ${formatTime(other.seenAt)}`,
    ].join('\n'),
    contextValue: CONTEXT_VALUES.otherVersion,
    children: [],
  };
}

function notSeenNode(routes: StoredRoute[], nowMs: number): NetworkNode {
  return {
    id: 'routesNotSeen',
    kind: 'notSeenGroup',
    label: 'Routes not seen',
    description: `${routes.length} ${plural(routes.length, 'route', 'routes')}`,
    tooltip: 'Stored routes of devices that no driver heard in this search. They are not observations.',
    contextValue: CONTEXT_VALUES.notSeenGroup,
    children: routes.map(route => ({
      id: `notSeen:${route.deviceId}`,
      kind: 'notSeenRoute' as const,
      label: shortDeviceId(route.deviceId),
      description: [
        socketAddress(route.address, route.sessionPort),
        `verified ${formatAge(route.verifiedAt, nowMs)}`,
      ].join('  '),
      tooltip: [
        `Device id: ${route.deviceId}`,
        `Stored route: ${socketAddress(route.address, route.sessionPort)} by ${driverLabel(route.driver)}`,
        `Verified: ${formatTime(route.verifiedAt)}`,
        'No driver heard this device in this search.',
      ].join('\n'),
      contextValue: CONTEXT_VALUES.notSeenRoute,
      children: [],
    })),
  };
}
