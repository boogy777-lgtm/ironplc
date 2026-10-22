/**
 * Choosing the controller of a project (ADR-0084): the engineer picks a
 * controller, this workstation remembers the route to it (IronLink's data) and
 * the project remembers the choice in its path file `<app>.path.iplcinfo`
 * (IronPLC's data, the record kind `Path` of ADR-0083).
 *
 * Two rules live in the compiler and nowhere else, so this module never derives
 * them: the file name of the record (`ironplcc record-path-path` says where it
 * is) and the format's one writer (`ironplcc record-path`, which also refuses a
 * loopback address as the route hint). This module reads the record's JSON,
 * which is allowed, builds the arguments of the two commands, and builds what
 * the picker shows and decides what picking an entry does.
 *
 * The module is vscode-free: the compiler, the file read and the clock are
 * injected, so every decision unit-tests without a compiler, a file system or an
 * editor.
 */

import { Anchor, connectionAnchor, FolderRef, RunCompiler } from './baselineLogic';
import {
  BrowseSnapshot,
  ControllerArgument,
  formatAge,
  IronLinkRoute,
  shortDeviceId,
  socketAddress,
} from './vscodeCommon';

/** The version of the path record this client reads (the compiler's `RecordKind::version`). */
const PATH_RECORD_VERSION = 1;

/** The controller a project last chose, as its path record holds it. */
export interface ChosenController {
  /** The device id, 32 lowercase hexadecimal digits. */
  deviceId: string;
  /** The last route that is not loopback, when there was one. */
  routeHint?: { address: string; sessionPort: number };
  /** When the controller was chosen, in milliseconds since the Unix epoch. */
  chosenAt: number;
}

/**
 * Reads a path record from its file text. A text that is not a record of this
 * version, as the compiler's reader treats it, is no record (fail-soft).
 */
export function parsePathRecord(text: string): ChosenController | undefined {
  let value: unknown;
  try {
    value = JSON.parse(text);
  }
  catch {
    return undefined;
  }
  if (typeof value !== 'object' || value === null) {
    return undefined;
  }
  const record = value as Record<string, unknown>;
  if (record.version !== PATH_RECORD_VERSION
    || typeof record.deviceId !== 'string' || !/^[0-9a-f]{32}$/.test(record.deviceId)
    || typeof record.chosenAt !== 'number' || !Number.isInteger(record.chosenAt) || record.chosenAt < 0) {
    return undefined;
  }
  if (record.routeHint === null) {
    return { deviceId: record.deviceId, chosenAt: record.chosenAt };
  }
  const hint = record.routeHint;
  if (typeof hint !== 'object') {
    return undefined;
  }
  const { address, sessionPort } = hint as Record<string, unknown>;
  if (typeof address !== 'string' || address.length === 0
    || typeof sessionPort !== 'number' || !Number.isInteger(sessionPort) || sessionPort < 1 || sessionPort > 65535) {
    return undefined;
  }
  return { deviceId: record.deviceId, routeHint: { address, sessionPort }, chosenAt: record.chosenAt };
}

/** The arguments of `ironplcc record-path-path`. */
export function recordPathPathArgs(anchor: string): string[] {
  return ['record-path-path', anchor];
}

/** The arguments of `ironplcc record-path` for a chosen controller. */
export function recordPathArgs(anchor: string, controller: ControllerArgument): string[] {
  return [
    'record-path', anchor,
    '--device-id', controller.deviceId,
    '--address', controller.address,
    '--port', String(controller.sessionPort),
  ];
}

/**
 * The application container a project's path record is named after and sits
 * beside: the container of the application the connection method `tcp` anchors
 * its download record to. Asking that method keeps one rule for where the records
 * of a project are, and the download record and the path record of an
 * application sit beside one another. The method decides which container that is
 * (the first workspace folder's), or why there is none.
 */
export function applicationAnchor(folders: readonly FolderRef[], sourceExtensions: readonly string[]): Anchor {
  return connectionAnchor({ name: 'application', transport: 'tcp' }, folders, sourceExtensions);
}

/** Writes the path record, through the compiler: the one writer of the format. */
export async function writeChoice(run: RunCompiler, anchor: string, controller: ControllerArgument): Promise<void> {
  await run(recordPathArgs(anchor, controller));
}

/**
 * The controller last chosen for `anchor`, or `undefined` when none was or the
 * file is not a record. The file is the one the compiler names. `readText`
 * resolves `undefined` for a file that does not exist and rejects for any other
 * failure.
 */
export async function readChosen(
  run: RunCompiler,
  readText: (file: string) => Promise<string | undefined>,
  anchor: string,
): Promise<ChosenController | undefined> {
  const printed = (await run(recordPathPathArgs(anchor))).stdout;
  const file = printed.replace(/\r?\n$/, '');
  if (file.length === 0) {
    throw new Error('the compiler did not say where the path record is');
  }
  const text = await readText(file);
  return text === undefined ? undefined : parsePathRecord(text);
}

/** What the picker knows of IronLink. */
export type IronLinkState =
  | { state: 'unavailable'; reason: string }
  | { state: 'failed'; message: string }
  | { state: 'ready'; routes: readonly IronLinkRoute[]; lastBrowse: BrowseSnapshot | undefined };

/** What the picker is built from. */
export interface PickInputs {
  /** The controller last chosen for the project, when there is one. */
  chosen: ChosenController | undefined;
  ironlink: IronLinkState;
  /** Milliseconds since the Unix epoch. */
  nowMs: number;
}

/** The group a controller is listed in. */
export type PickGroup = 'last' | 'route' | 'observed';

/** The heading of each group, in the order the groups are shown. */
export const PICK_GROUPS: ReadonlyArray<{ group: PickGroup; heading: string }> = [
  { group: 'last', heading: 'Last chosen controller' },
  { group: 'route', heading: 'Controllers with a route on this PC' },
  { group: 'observed', heading: 'Recently observed' },
];

/** The label of the entry that searches the network. */
export const SEARCH_LABEL = 'Search network…';

/** One row of the picker. */
export type PickEntry =
  | { kind: 'separator'; label: string }
  | {
    kind: 'device';
    group: PickGroup;
    deviceId: string;
    label: string;
    description: string;
    detail: string;
    /** The route to verify and record when the entry is picked, or why there is none. */
    target: ControllerArgument | { unavailable: string };
  }
  | { kind: 'search'; label: string; detail: string }
  | { kind: 'ironlinkNeeded'; label: string; detail: string }
  | { kind: 'ironlinkFailed'; label: string; detail: string };

/** What picking an entry does. */
export type PickDecision =
  | { action: 'choose'; controller: ControllerArgument }
  | { action: 'search' }
  | { action: 'explain'; message: string }
  | { action: 'nothing' };

const IRONLINK_NEEDED_LABEL = 'Searching the network needs the IronLink extension';

/** The name a controller is listed under: the name it last answered with, else its short id. */
function nameOf(deviceId: string, lastBrowse: BrowseSnapshot | undefined): string {
  return lastBrowse?.devices.find(device => device.deviceId === deviceId)?.name ?? `Controller ${shortDeviceId(deviceId)}`;
}

function sortedRoutes(routes: readonly IronLinkRoute[]): IronLinkRoute[] {
  return [...routes].sort((a, b) => a.deviceId.localeCompare(b.deviceId));
}

/**
 * The rows of the picker, in this order: the controller last chosen, the
 * controllers with a route on this PC, the controllers recently observed, and
 * last the way to search the network. A controller is listed once, in the first
 * group it belongs to (a device id heard at several addresses is a row for each
 * address); a group with nothing in it has no heading. Without IronLink
 * there are only the controller last chosen and one row that says what searching
 * needs; picking that row does nothing.
 */
export function buildPickEntries(inputs: PickInputs): PickEntry[] {
  const { chosen, ironlink, nowMs } = inputs;
  const routes = ironlink.state === 'ready' ? sortedRoutes(ironlink.routes) : [];
  const lastBrowse = ironlink.state === 'ready' ? ironlink.lastBrowse : undefined;
  const listed = new Set<string>();
  const byGroup = new Map<PickGroup, PickEntry[]>();
  const add = (group: PickGroup, entry: PickEntry & { kind: 'device' }) => {
    byGroup.set(group, [...(byGroup.get(group) ?? []), entry]);
  };

  if (chosen !== undefined) {
    listed.add(chosen.deviceId);
    const route = routes.find(candidate => candidate.deviceId === chosen.deviceId);
    const where = route !== undefined
      ? { address: route.address, sessionPort: route.sessionPort }
      : chosen.routeHint;
    const known = where === undefined ? 'no address is known' : socketAddress(where.address, where.sessionPort);
    let target: ControllerArgument | { unavailable: string };
    if (ironlink.state !== 'ready') {
      target = { unavailable: 'This controller is already chosen.' };
    }
    else if (where === undefined) {
      target = { unavailable: 'No address is known for this controller. Search the network, then choose it from the result.' };
    }
    else {
      target = { deviceId: chosen.deviceId, address: where.address, sessionPort: where.sessionPort };
    }
    add('last', {
      kind: 'device',
      group: 'last',
      deviceId: chosen.deviceId,
      label: nameOf(chosen.deviceId, lastBrowse),
      description: known,
      detail: `Chosen ${formatAge(chosen.chosenAt, nowMs)}; device id ${shortDeviceId(chosen.deviceId)}`,
      target,
    });
  }

  for (const route of routes) {
    if (listed.has(route.deviceId)) {
      continue;
    }
    listed.add(route.deviceId);
    add('route', {
      kind: 'device',
      group: 'route',
      deviceId: route.deviceId,
      label: nameOf(route.deviceId, lastBrowse),
      description: socketAddress(route.address, route.sessionPort),
      detail: `Route verified ${formatAge(route.verifiedAt, nowMs)}; device id ${shortDeviceId(route.deviceId)}`,
      target: { deviceId: route.deviceId, address: route.address, sessionPort: route.sessionPort },
    });
  }

  for (const device of lastBrowse?.devices ?? []) {
    if (listed.has(device.deviceId)) {
      continue;
    }
    add('observed', {
      kind: 'device',
      group: 'observed',
      deviceId: device.deviceId,
      label: device.name,
      description: socketAddress(device.address, device.sessionPort),
      detail: `Observed ${formatAge(device.seenAt, nowMs)}${device.protected ? ', protected' : ''}; device id ${shortDeviceId(device.deviceId)}`,
      target: { deviceId: device.deviceId, address: device.address, sessionPort: device.sessionPort },
    });
  }

  const entries: PickEntry[] = [];
  for (const { group, heading } of PICK_GROUPS) {
    const rows = byGroup.get(group);
    if (rows !== undefined && rows.length > 0) {
      entries.push({ kind: 'separator', label: heading }, ...rows);
    }
  }
  switch (ironlink.state) {
    case 'ready':
      entries.push({
        kind: 'search',
        label: SEARCH_LABEL,
        detail: lastBrowse === undefined
          ? 'Search the network for controllers'
          : `Last search ${formatAge(lastBrowse.browsedAt, nowMs)}`,
      });
      break;
    case 'failed':
      entries.push({ kind: 'ironlinkFailed', label: 'IronLink did not answer', detail: ironlink.message });
      break;
    case 'unavailable':
      entries.push({ kind: 'ironlinkNeeded', label: IRONLINK_NEEDED_LABEL, detail: ironlink.reason });
      break;
  }
  return entries;
}

/** What picking `entry` does. */
export function decidePick(entry: PickEntry): PickDecision {
  switch (entry.kind) {
    case 'device':
      return 'unavailable' in entry.target
        ? { action: 'explain', message: entry.target.unavailable }
        : { action: 'choose', controller: entry.target };
    case 'search':
      return { action: 'search' };
    case 'separator':
    case 'ironlinkNeeded':
    case 'ironlinkFailed':
      return { action: 'nothing' };
  }
}

/** A failure of IronLink as it reaches this extension: its code and the whole text to show. */
export interface IronLinkFailure {
  /** The `L####` code of IronLink or the `E####` code of the shared catalog. */
  code: string;
  text: string;
}

/**
 * The failure an error thrown by the IronLink API carries. IronLink's own error
 * classes do not cross the extension boundary, but every message of them starts
 * with the code (`L6003 - ...`, `E0016 - ...`). An error without a code is a
 * defect and has no failure: the caller rethrows it.
 */
export function describeIronLinkFailure(error: unknown): IronLinkFailure | undefined {
  if (!(error instanceof Error)) {
    return undefined;
  }
  const code = /^([LE]\d{4}) - /.exec(error.message)?.[1];
  return code === undefined ? undefined : { code, text: error.message };
}
