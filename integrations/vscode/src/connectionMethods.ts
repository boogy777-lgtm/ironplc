/**
 * The connection methods of the engineering connection (ADR-0063, ADR-0084):
 * how IronPLC talks to a controller. One declaration per method holds every
 * decision the extension takes by method; the code that validates a profile,
 * lists it, opens it, reconnects it and anchors its download record asks the
 * method, and never compares the method name.
 *
 * - Class: a connection method (a way to reach one controller). Members:
 *   `stdio` (a spawned local `ironplcvm serve`) and `tcp` (a socket to a
 *   device). A near case outside the class: the discovery driver, which only
 *   finds controllers (`compiler/ironlink`).
 * - Invariant: the table below is the one list of methods. The method names
 *   (`ConnectionTransport`) are its keys, so a name without a declaration, or
 *   a declaration without a name, cannot exist; each member is checked
 *   against `ConnectionMethod` by `satisfies`. `checkInvariants` also checks
 *   that the `transport` enum of the `ironplc.connections` setting (a second
 *   list, in `package.json`) equals the keys, and that every module in
 *   `connectionMethods/` is in the table.
 * - Next case (a USB method): one module in `connectionMethods/` and one line
 *   here, plus its value in the `package.json` enum, which the check above
 *   fails the build without. Profile validation, the connect command, the
 *   QuickPick, the reconnect policy and the baseline code are not edited.
 *
 * The module is vscode-free on purpose, like the logic modules that use it:
 * a member that needs the editor (the output channel, the settings) receives
 * it in the {@link OpenContext}.
 */

import type { Anchor, FolderRef } from './baselineLogic';
import type { CompilerSettings } from './compilerSettingsLogic';
import type { ConnectionProfileFields } from './connectionProfiles';
import type { OpenedConnection } from './connectionState';
import type { ProblemCode } from './problems';
import { stdioMethod } from './connectionMethods/stdio';
import { tcpMethod } from './connectionMethods/tcp';

/** One violated rule of a method's profile fields: the E-code and the detail. */
export interface FieldFault {
  code: ProblemCode;
  detail: string;
}

/** What opening a connection needs from the editor and its settings. */
export interface OpenContext {
  /** The compiler, or undefined when it is not installed (a method that needs it says so). */
  compilerPath: string | undefined;
  sourceExtensions: readonly string[];
  /** The connection output channel. */
  output: {
    append(text: string): void;
    appendLine(text: string): void;
  };
  /** The compiler settings in effect now. */
  compilerSettings(): CompilerSettings;
}

/** Every decision the extension takes by connection method. */
export interface ConnectionMethod {
  /**
   * The method's rules for the profile fields, after the checks every method
   * shares (names, field types, port range): the fields it requires and the
   * ones it forbids. Returns the first violated rule.
   */
  checkFields(fields: ConnectionProfileFields): FieldFault | undefined;
  /** The port a profile gets when it sets none, for a method that has one. */
  readonly defaultPort?: number;
  /** Whether the compiler and the VM must be installed before connecting. */
  readonly needsLocalTools: boolean;
  /** The second line of the profile's QuickPick entry. */
  detail(fields: ConnectionProfileFields): string | undefined;
  /** Opens the channel to the controller. */
  open(fields: ConnectionProfileFields, context: OpenContext): Promise<OpenedConnection>;
  /** Whether a lost connection is retried; otherwise the loss is final. */
  readonly reconnects: boolean;
  /** Whether starting a session is itself a download, so it is recorded. */
  readonly recordsSessionStart: boolean;
  /** The application container the connection's download record is named after, or why there is none. */
  anchor(
    fields: ConnectionProfileFields,
    folders: readonly FolderRef[],
    sourceExtensions: readonly string[],
  ): Anchor;
}

/** The one table of connection methods, keyed by the name a profile's `transport` carries. */
export const CONNECTION_METHODS = {
  stdio: stdioMethod,
  tcp: tcpMethod,
} as const satisfies Record<string, ConnectionMethod>;

/** The name of a connection method: a key of the table. */
export type ConnectionTransport = keyof typeof CONNECTION_METHODS;

/** The names of the connection methods, in the table's order. */
export function connectionTransports(): ConnectionTransport[] {
  return Object.keys(CONNECTION_METHODS) as ConnectionTransport[];
}

/** Whether `value` names a connection method. */
export function isConnectionTransport(value: unknown): value is ConnectionTransport {
  return typeof value === 'string' && Object.prototype.hasOwnProperty.call(CONNECTION_METHODS, value);
}

/** The method a profile uses. */
export function connectionMethod(profile: { transport: ConnectionTransport }): ConnectionMethod {
  return CONNECTION_METHODS[profile.transport];
}

/** The method names as a quoted alternative: `"stdio" or "tcp"`, `"a", "b" or "c"`. */
export function describeTransports(): string {
  const names = connectionTransports().map(name => JSON.stringify(name));
  const last = names.pop();
  return names.length === 0 ? last ?? '' : `${names.join(', ')} or ${last}`;
}
