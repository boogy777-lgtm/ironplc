/**
 * The connection profile model of the engineering connection (ADR-0063,
 * Mechanism 1): one profile describes one target — a `stdio` profile spawns
 * a local `ironplcvm serve` child on the `program` it names, a `tcp`
 * profile opens a socket to the device's `address`/`port`. Profiles persist
 * in the `ironplc.connections` setting; `credentialsKey` names an entry in
 * the host secret store (VS Code `SecretStorage`) and never carries the
 * credential itself.
 *
 * The module is vscode-free on purpose: validation is pure decision logic
 * (the reference UI's error toast on bad settings) and unit-tests as a
 * matrix. Every failure is an E-code from the generated registry, reported
 * before any transport opens — a bad profile fails without network traffic.
 */

import {
  connectionMethod,
  ConnectionTransport,
  describeTransports,
  isConnectionTransport,
} from './connectionMethods';
import { ProblemCode } from './problems';

/**
 * The fields of a connection profile that do not name its connection method.
 * Per-method fields are required or forbidden by the method's own rules
 * (`ConnectionMethod.checkFields`), not by the type system: `program` for
 * `stdio`, `address` (and optionally `port`) for `tcp`.
 */
export interface ConnectionProfileFields {
  name: string;
  /** The source file or `.iplc` container a stdio session loads (`serve <FILE>`). */
  program?: string;
  /** The IPv4/IPv6/hostname address of a tcp device. */
  address?: string;
  /** The tcp port, 1-65535; defaults to the session port when omitted. */
  port?: number;
  /** The secret-store key the host resolves a username/password pair from. */
  credentialsKey?: string;
}

/**
 * One connection profile as persisted in the `ironplc.connections` setting:
 * the connection method it speaks and the fields that method reads.
 */
export interface ConnectionProfile extends ConnectionProfileFields {
  transport: ConnectionTransport;
}

/** One validation failure: the E-code, the offending profile, and the detail. */
export interface ProfileValidationError {
  code: ProblemCode;
  /** The profile name, or `#<index + 1>` when the profile has no usable name. */
  profile: string;
  detail: string;
}

const MAX_PORT = 65535;

/**
 * Validates the raw `ironplc.connections` setting value and returns the
 * usable profiles (defaults applied) plus every failure found. The rules are
 * the engineering-connection spec's table: E0010 for a missing/duplicate
 * name, an unknown transport, or a missing transport-required field; E0011
 * for a malformed address or an address on a stdio profile; E0012 for a
 * port outside 1-65535 or a port on a stdio profile.
 */
export function validateProfiles(raw: unknown): {
  profiles: ConnectionProfile[];
  errors: ProfileValidationError[];
} {
  const profiles: ConnectionProfile[] = [];
  const errors: ProfileValidationError[] = [];
  if (!Array.isArray(raw)) {
    return { profiles, errors };
  }

  const seen = new Set<string>();
  raw.forEach((entry, index) => {
    const label = profileLabel(entry, index);
    if (typeof entry !== 'object' || entry === null) {
      errors.push(error(ProblemCode.ConnectionProfileInvalid, label, 'the profile is not an object'));
      return;
    }
    const record = entry as Record<string, unknown>;

    const name = record.name;
    if (typeof name !== 'string' || name.trim().length === 0) {
      errors.push(error(ProblemCode.ConnectionProfileInvalid, label, 'the profile is missing a name'));
      return;
    }
    if (seen.has(name)) {
      errors.push(error(ProblemCode.ConnectionProfileInvalid, name, `the profile name "${name}" is used more than once`));
      return;
    }

    if (!isConnectionTransport(record.transport)) {
      errors.push(error(ProblemCode.ConnectionProfileInvalid, name, `the transport must be ${describeTransports()}, not ${JSON.stringify(record.transport)}`));
      return;
    }
    const transport: ConnectionTransport = record.transport;

    const program = optionalString(record.program);
    const address = optionalString(record.address);
    const credentialsKey = optionalString(record.credentialsKey);
    if (record.program !== undefined && program === undefined) {
      errors.push(error(ProblemCode.ConnectionProfileInvalid, name, 'the program must be a string'));
      return;
    }
    if (record.address !== undefined && address === undefined) {
      errors.push(error(ProblemCode.ConnectionProfileInvalid, name, 'the address must be a string'));
      return;
    }
    if (record.credentialsKey !== undefined && credentialsKey === undefined) {
      errors.push(error(ProblemCode.ConnectionProfileInvalid, name, 'the credentialsKey must be a string'));
      return;
    }

    const port = record.port;
    if (port !== undefined && (typeof port !== 'number' || !Number.isInteger(port) || port < 1 || port > MAX_PORT)) {
      errors.push(error(ProblemCode.ConnectionPortInvalid, name, `the port must be an integer in 1-${MAX_PORT}, not ${JSON.stringify(port)}`));
      return;
    }

    const method = connectionMethod({ transport });
    const fault = method.checkFields({ name, program, address, port, credentialsKey });
    if (fault !== undefined) {
      errors.push(error(fault.code, name, fault.detail));
      return;
    }

    profiles.push({
      name,
      transport,
      program,
      address,
      port: port ?? method.defaultPort,
      credentialsKey,
    });
    // Only accepted profiles claim their name: an invalid profile must not
    // poison duplicate detection for a later valid one of the same name.
    seen.add(name);
  });
  return { profiles, errors };
}

function error(code: ProblemCode, profile: string, detail: string): ProfileValidationError {
  return { code, profile, detail };
}

function profileLabel(entry: unknown, index: number): string {
  if (typeof entry === 'object' && entry !== null && typeof (entry as Record<string, unknown>).name === 'string') {
    return (entry as Record<string, unknown>).name as string;
  }
  return `#${index + 1}`;
}

function optionalString(value: unknown): string | undefined {
  return typeof value === 'string' ? value : undefined;
}
