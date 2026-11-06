/**
 * The download record of the engineering connection (ADR-0083): what this
 * workspace puts on a device, written after the device acknowledged the
 * assemble. The record is a file beside the application container, one per
 * device, keyed by the device's id (`deviceId` of its identity) and written by
 * `ironplcc record-download`. What the record says about the device is read by
 * the go-online comparison (`ironplcc online-compare`, ADR-0089), never here.
 *
 * Two rules live in the compiler and nowhere else, so this module never
 * derives them: the file name of the record (`ironplcc record-download-path`
 * says where it is) and the format's writer (`ironplcc record-download`). This
 * module plans a download (which container the record is named after, and
 * whether there is anything to record) and builds the arguments of the writer.
 *
 * The module is vscode-free: the decisions are pure and the compiler command
 * is injected, so they unit-test without a compiler or a file system.
 */

import * as path from 'path';

import { connectionMethod } from './connectionMethods';
import { ConnectionProfile } from './connectionProfiles';
import { HotEditStatus } from './hotEditSession';
import { ExecFileFn } from './syncUidsLogic';

/** A workspace folder, as the anchor needs it. */
export interface FolderRef {
  name: string;
  path: string;
}

/** The application container of a connection, or why it has none. */
export type Anchor = { anchor: string } | { reason: string };

/**
 * The application container a connection's download record is named after
 * and sits beside (the compiler names the file from it; the device's id is the
 * rest of the name). Which container that
 * is, or that there is none, is the connection method's rule
 * (`ConnectionMethod.anchor`).
 */
export function connectionAnchor(
  profile: ConnectionProfile,
  folders: readonly FolderRef[],
  sourceExtensions: readonly string[],
): Anchor {
  return connectionMethod(profile).anchor(profile, folders, sourceExtensions);
}

/**
 * The compiler, as the commands that call it see it: arguments in (and, for a
 * command that reads standard input, the text to write to it), output out.
 */
export type RunCompiler = (args: string[], input?: string) => ReturnType<ExecFileFn>;

/** One download to record, resolved. */
export interface DownloadToRecord {
  anchor: string;
  /** The container file that was sent. */
  container: string;
  /** The id of the connected device, which keys the record. */
  deviceId: string;
  /** The device's status after the download: the counters and, when reported, the content hash. */
  status: HotEditStatus;
  /**
   * Whether the compiler also keeps a copy of the container that was sent beside
   * the record (a Download, ADR-0088). The copy's name is the compiler's rule,
   * like the record's.
   */
  keepContainer: boolean;
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
  /**
   * The id of the connected device, as its identity reports it. A device whose
   * identity carries none (one that predates the field, or a host that has
   * none) has no record.
   */
  deviceId: string | undefined;
  /** The device's status after the download. */
  status: HotEditStatus;
  /** Whether the container that was sent is also kept beside the record. Not kept when absent. */
  keepContainer?: boolean;
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
  if (request.deviceId === undefined) {
    return { reason: 'the device does not report a device id, which keys the record' };
  }
  if (request.requireDeviceHash && request.status.contentHash === undefined) {
    return { reason: 'the device does not report the content hash of its application, so it cannot be shown that it runs this container' };
  }
  return {
    download: {
      anchor: anchor.anchor,
      container: request.sentContainer === undefined ? anchor.anchor : path.resolve(request.sentContainer),
      deviceId: request.deviceId,
      status: request.status,
      keepContainer: request.keepContainer ?? false,
    },
  };
}

/** The arguments of `ironplcc record-download` for a download. */
export function recordDownloadArgs(download: DownloadToRecord): string[] {
  const { status } = download;
  const args = ['record-download', download.anchor, '--device-id', download.deviceId];
  if (download.container !== download.anchor) {
    args.push('--container', download.container);
  }
  if (download.keepContainer) {
    args.push('--keep-container');
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

/** Writes the download record, through the compiler: the one writer of the format. */
export async function writeDownloadRecord(run: RunCompiler, download: DownloadToRecord): Promise<void> {
  await run(recordDownloadArgs(download));
}
