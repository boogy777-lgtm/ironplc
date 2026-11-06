/**
 * Asking the compiler for the go-online comparison (ADR-0089), and what Connect
 * does with the answer. The comparison is `ironplcc online-compare`; this module
 * builds its command line in one place (`comparisonArgs`), says which attempt a
 * failed connection was, runs the compiler through an injected runner and hands
 * the result to the person through an injected interface. It reads no record and
 * compares nothing.
 *
 * Connect (`compareOnConnect`):
 *
 * - the session opened: the identity the device answered goes to the compiler on
 *   standard input with `--attempt ok`;
 * - the connection failed: the same command with the attempt that failed
 *   (`unreachable`, `busy`, `protected` or `connection-lost`), so the same view
 *   explains it;
 * - `atOnce` (the application is up to date): the connection is online with no
 *   dialog, and the conditions beside the headline are said in the connection
 *   channel; every other row is shown to the person. The status line is always
 *   the row's summary.
 *
 * The module is vscode-free: the compiler and the view are injected.
 */

import * as path from 'path';

import { ConnectionFailure } from './connectionState';
import { connectionAnchor, FolderRef } from './downloadRecordLogic';
import { ConnectionProfile } from './connectionProfiles';
import { IdentityInfo } from './hotEditSession';
import {
  Comparison,
  ComparisonModel,
  comparisonModel,
  parseComparison,
} from './onlineComparisonLogic';
import { ProblemCode } from './vscodeCommon';

/** How the attempt to connect went: the values of `--attempt` of `ironplcc online-compare`. */
export const ATTEMPTS = ['ok', 'unreachable', 'busy', 'protected', 'connection-lost'] as const;

/** One attempt. */
export type Attempt = typeof ATTEMPTS[number];

/** The code of the refusal of a device that already serves a session. */
const BUSY_V_CODE = 'V6014';

/**
 * The attempt a failed connection was. A device that refused with V6014 is busy;
 * a connection that was open and ended is lost; a controller that the network
 * search marked as protected is protected (it wants credentials, which are not
 * supported yet); every other failure is a controller that did not answer.
 */
export function attemptOfFailure(failure: ConnectionFailure | undefined, controllerProtected: boolean): Attempt {
  if (failure?.vCode === BUSY_V_CODE) {
    return 'busy';
  }
  if (controllerProtected) {
    return 'protected';
  }
  return failure?.code === ProblemCode.ConnectionLost ? 'connection-lost' : 'unreachable';
}

/**
 * The command line of `ironplcc online-compare`, the one place it is built.
 * `anchor` is the application container the records sit beside, `sources` the
 * files and folders of the project (as `compile` takes them), `options` the
 * compiler flags of the editor settings. The identity of the controller is read
 * from standard input, and only with the attempt `ok`, which is what the
 * command accepts.
 */
export function comparisonArgs(
  anchor: string,
  sources: readonly string[],
  options: readonly string[],
  attempt: Attempt,
): string[] {
  const args = ['online-compare', ...options, ...sources, '-o', anchor, '--attempt', attempt];
  if (attempt === 'ok') {
    args.push('--identity', '-');
  }
  return args;
}

/** The text to write to standard input for the attempt `ok`: the answer of the device to `identity`, as it sent it. */
export function identityJson(identity: IdentityInfo): string {
  return JSON.stringify(identity.wire ?? identity);
}

/** The file name of the container that no application has, beside which no record is ever found. */
export const NO_APPLICATION_CONTAINER = 'no-application.iplc';

/**
 * The application container the records are looked up beside. A connection that
 * has one (`connectionAnchor`) uses it. A connection without one (a stdio
 * session to a source program, which compiles it to a container of its own) is
 * compared against a container in the extension's storage directory that does
 * not exist and has no records beside it: the command treats a missing record
 * as none, so the comparison is made with no build record, no download record
 * and no path record, and says so by what it finds.
 */
export function comparisonAnchor(
  profile: ConnectionProfile,
  folders: readonly FolderRef[],
  sourceExtensions: readonly string[],
  storageDir: string,
): string {
  const anchor = connectionAnchor(profile, folders, sourceExtensions);
  return 'anchor' in anchor ? anchor.anchor : path.join(storageDir, NO_APPLICATION_CONTAINER);
}

/** The sources of the project: its workspace folder, the one the build task compiles and the download record is named after. */
export function comparisonSources(folders: readonly FolderRef[]): string[] {
  return folders.length === 0 ? [] : [folders[0].path];
}

/** What a comparison needs from the editor. */
export interface ComparisonEnv {
  /** Runs `ironplcc` with the arguments and the text for its standard input; undefined when the compiler is not installed. */
  run: ((args: string[], input?: string) => Promise<{ stdout: string; stderr: string }>) | undefined;
  folders: readonly FolderRef[];
  sourceExtensions: readonly string[];
  /** The compiler flags of the editor settings now. */
  options(): string[];
  /** The extension's storage directory, for a connection that has no application container. */
  storageDir: string;
  /** Says why a comparison could not be made (the connection channel). */
  say(line: string): void;
}

/** What is compared: the connection and how the attempt went. */
export interface ComparisonRequest {
  profile: ConnectionProfile;
  attempt: Attempt;
  /** The identity the device answered; required with the attempt `ok`. */
  identity?: IdentityInfo;
}

/**
 * Asks the compiler for the comparison. Resolves undefined, with the reason in
 * the connection channel, when it cannot be made (no compiler, a failed run, an
 * answer that is not a comparison): Connect never refuses, so the connection
 * stays as it is without the view.
 */
export async function runComparison(env: ComparisonEnv, request: ComparisonRequest): Promise<Comparison | undefined> {
  if (env.run === undefined) {
    env.say('The go-online comparison needs the IronPLC compiler, which is not installed.');
    return undefined;
  }
  if (request.attempt === 'ok' && request.identity === undefined) {
    env.say('The go-online comparison was not made: the controller gave no identity.');
    return undefined;
  }
  const anchor = comparisonAnchor(request.profile, env.folders, env.sourceExtensions, env.storageDir);
  const args = comparisonArgs(anchor, comparisonSources(env.folders), env.options(), request.attempt);
  try {
    const printed = await env.run(args, request.attempt === 'ok' && request.identity !== undefined ? identityJson(request.identity) : undefined);
    return parseComparison(printed.stdout);
  }
  catch (err) {
    env.say(`The go-online comparison was not made: ${err instanceof Error ? err.message : String(err)}`);
    return undefined;
  }
}

/** What Connect shows. */
export interface ComparisonUi {
  /** The comparison is the current one: the status line follows it. */
  current(comparison: Comparison): void;
  /** Shows the dialog of a comparison that is not at once. */
  dialog(model: ComparisonModel, comparison: Comparison): void;
}

/**
 * Compares the controller with the project and shows the result: the status
 * line always, the dialog unless the application is up to date. `announce` is
 * false for a comparison that is only renewed (the controller reconnected, a
 * download finished): the status line follows and nothing opens.
 */
export async function compareOnConnect(
  env: ComparisonEnv,
  request: ComparisonRequest,
  ui: ComparisonUi,
  announce: boolean,
): Promise<Comparison | undefined> {
  const comparison = await runComparison(env, request);
  if (comparison === undefined) {
    return undefined;
  }
  ui.current(comparison);
  if (announce && !comparison.atOnce) {
    ui.dialog(comparisonModel(comparison), comparison);
  }
  else if (announce) {
    // Online at once: the conditions beside the headline (another controller than
    // the one last chosen, a stopped controller) open no dialog, so they are said here.
    for (const note of comparison.information) {
      env.say(note.text);
    }
  }
  return comparison;
}
