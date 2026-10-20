/**
 * The baseline check of the engineering connection (ADR-0063, "Offline
 * edits, online deploy"): the owner scenario connects, verifies the project
 * equals the controller's code, disconnects, edits offline, and reconnects
 * to deploy. Before any bytes are sent, the client compares what the device
 * runs with what this workspace last put on it; a mismatch means another
 * engineer edited the device, and the deploy refuses with E0015 StaleBaseline
 * — fail-closed, the same pattern as the V4007–V4010 staging refusals.
 *
 * What this workspace last put on the device is the *download record*
 * (ADR-0083): a file beside the application container, one per connection
 * profile, written by `ironplcc record-download`. What the device runs is the
 * `contentHash` of its identity. The comparison is by content, so a device
 * reboot, which restarts the generation counters, keeps a match.
 *
 * Two rules live in the compiler and nowhere else, so this module never
 * derives them: the file name of the record (`ironplcc record-download-path`
 * says where it is) and the format's writer (`ironplcc record-download`).
 * This module only reads the record's JSON, which is allowed, and builds the
 * arguments of the two commands.
 *
 * The module is vscode-free: the decisions are pure and the two compiler
 * commands and the file read are injected, so they unit-test without a
 * compiler or a file system.
 */

import * as path from 'path';

import { programKind } from './debugAdapterLogic';
import { ConnectionProfile } from './connectionProfiles';
import { HotEditStatus } from './hotEditSession';
import { ExecFileFn } from './syncUidsLogic';
import { outputFileNameForFolder } from './taskProviderLogic';

export type BaselineState = 'none' | 'match' | 'mismatch';

/** The version of the download record this client reads (the compiler's `RecordKind::version`). */
const DOWNLOAD_RECORD_VERSION = 1;

/** What the check reads of a download record. */
export interface DownloadRecord {
  /** The name of the connection profile the record is for. */
  connection: string;
  /** The `contentHash` of the container the device received, 64 lowercase hexadecimal digits. */
  contentHash: string;
}

/**
 * Reads the download record of `connection` from its file text. A text that is
 * not a record of this version for this connection is no record — as the
 * compiler's reader treats it (fail-soft). The connection is compared because
 * two names that differ only in letter case share a file on a file system that
 * ignores case.
 */
export function parseDownloadRecord(text: string, connection: string): DownloadRecord | undefined {
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
  const container = record.container;
  if (record.version !== DOWNLOAD_RECORD_VERSION || record.connection !== connection) {
    return undefined;
  }
  if (typeof container !== 'object' || container === null) {
    return undefined;
  }
  const contentHash = (container as Record<string, unknown>).contentHash;
  if (typeof contentHash !== 'string' || !/^[0-9a-f]{64}$/.test(contentHash)) {
    return undefined;
  }
  return { connection, contentHash };
}

/**
 * Compares the device's current application snapshot with the download record.
 *
 * - no record: `none` (a first deploy, or a connection never downloaded to);
 * - a record and the same `contentHash`: `match`;
 * - a record and another `contentHash`, or a device that reports none (it
 *   predates the field, so what it runs cannot be known): `mismatch`, which
 *   fails closed into E0015.
 */
export function checkBaseline(record: DownloadRecord | undefined, application: HotEditStatus): BaselineState {
  if (record === undefined) {
    return 'none';
  }
  return application.contentHash === record.contentHash ? 'match' : 'mismatch';
}

/** A workspace folder, as the anchor needs it. */
export interface FolderRef {
  name: string;
  path: string;
}

/** The application container of a connection, or why it has none. */
export type Anchor = { anchor: string } | { reason: string };

/**
 * The application container a connection's download record is named after
 * and sits beside (the compiler names the file from it).
 *
 * - A stdio profile whose `program` is a container: that container, which the
 *   session serves.
 * - A stdio profile whose `program` is a source file: none. The session compiles
 *   it to a container of its own that the engineer has no file for.
 * - A tcp profile: the container the build task writes in the first workspace
 *   folder, `<folder>/<folder>.iplc`. The first folder is a fixed choice, so a
 *   record is found again whatever file is open.
 */
export function connectionAnchor(
  profile: ConnectionProfile,
  folders: readonly FolderRef[],
  sourceExtensions: readonly string[],
): Anchor {
  if (profile.transport === 'stdio') {
    const program = profile.program ?? '';
    if (programKind(program, sourceExtensions) === 'container') {
      return { anchor: path.resolve(program) };
    }
    return { reason: `the connection "${profile.name}" runs ${program}, a program that the session compiles itself: there is no application container to name a record after` };
  }
  const folder = folders[0];
  if (folder === undefined) {
    return { reason: 'no workspace folder is open, so there is no application container to name a record after' };
  }
  return { anchor: path.join(folder.path, outputFileNameForFolder(folder.name)) };
}

/** The compiler, as the record commands see it: arguments in, output out. */
export type RunCompiler = (args: string[]) => ReturnType<ExecFileFn>;

/** One download to record, resolved. */
export interface DownloadToRecord {
  anchor: string;
  /** The container file that was sent. */
  container: string;
  connection: string;
  /** The device's status after the download: the counters and, when reported, the content hash. */
  status: HotEditStatus;
}

/** What to record for a download, or why nothing is recorded. */
export type DownloadPlan = { download: DownloadToRecord } | { reason: string };

/** A download that happened on the connection of `profile`. */
export interface DownloadRequest {
  profile: ConnectionProfile;
  folders: readonly FolderRef[];
  sourceExtensions: readonly string[];
  /**
   * The container file that was sent, when it is not the application container
   * (a container compiled for the occasion); the application container itself
   * when absent.
   */
  sentContainer?: string;
  /** The device's status after the download. */
  status: HotEditStatus;
  /**
   * Whether the device must report its content hash. A session started with a
   * container does not say what the device holds (a device that kept a
   * committed application boots that, not the file), so only the device's own
   * hash shows that it runs the file.
   */
  requireDeviceHash: boolean;
}

/**
 * Plans the record of a download. The one place that decides whether and how a
 * download is recorded, for every download path.
 */
export function planDownload(request: DownloadRequest): DownloadPlan {
  const anchor = connectionAnchor(request.profile, request.folders, request.sourceExtensions);
  if ('reason' in anchor) {
    return anchor;
  }
  if (request.requireDeviceHash && request.status.contentHash === undefined) {
    return { reason: 'the device does not report the content hash of its application, so it cannot be shown that it runs this container' };
  }
  return {
    download: {
      anchor: anchor.anchor,
      container: request.sentContainer === undefined ? anchor.anchor : path.resolve(request.sentContainer),
      connection: request.profile.name,
      status: request.status,
    },
  };
}

/** The arguments of `ironplcc record-download` for a download. */
export function recordDownloadArgs(download: DownloadToRecord): string[] {
  const { status } = download;
  const args = ['record-download', download.anchor, '--connection', download.connection];
  if (download.container !== download.anchor) {
    args.push('--container', download.container);
  }
  // With the hash, the compiler writes the record only if the device runs the
  // container: the record says what the device received.
  if (status.contentHash !== undefined) {
    args.push('--device-content-hash', status.contentHash);
  }
  args.push(
    '--active', String(status.active),
    '--normal', String(status.normal),
    '--application', String(status.application),
  );
  return args;
}

/** The arguments of `ironplcc record-download-path`. */
export function recordDownloadPathArgs(anchor: string, connection: string): string[] {
  return ['record-download-path', anchor, '--connection', connection];
}

/** Writes the download record, through the compiler: the one writer of the format. */
export async function writeDownloadRecord(run: RunCompiler, download: DownloadToRecord): Promise<void> {
  await run(recordDownloadArgs(download));
}

/**
 * Compares the device with the download record of `connection` for `anchor`.
 * The record's file is the one the compiler names; a file that is not there is
 * no record. `readText` resolves `undefined` for a file that does not exist
 * and rejects for any other failure.
 */
export async function readBaseline(
  run: RunCompiler,
  readText: (file: string) => Promise<string | undefined>,
  anchor: string,
  connection: string,
  application: HotEditStatus,
): Promise<BaselineState> {
  const printed = (await run(recordDownloadPathArgs(anchor, connection))).stdout;
  const file = printed.replace(/\r?\n$/, '');
  if (file.length === 0) {
    throw new Error('the compiler did not say where the download record is');
  }
  const text = await readText(file);
  const record = text === undefined ? undefined : parseDownloadRecord(text, connection);
  return checkBaseline(record, application);
}
