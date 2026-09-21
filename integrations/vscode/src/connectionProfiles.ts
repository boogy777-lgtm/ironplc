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

import { ProblemCode } from './problems';

/** The transport a profile speaks: a spawned child or a remote socket. */
export type ConnectionTransport = 'stdio' | 'tcp';

/**
 * One connection profile as persisted in the `ironplc.connections` setting.
 * Per-transport fields are required by validation, not by the type system:
 * `program` for `stdio`, `address` (and optionally `port`) for `tcp`.
 */
export interface ConnectionProfile {
  name: string;
  transport: ConnectionTransport;
  /** The source file or `.iplc` container a stdio session loads (`serve <FILE>`). */
  program?: string;
  /** The IPv4/IPv6/hostname address of a tcp device. */
  address?: string;
  /** The tcp port, 1-65535; defaults to 49152 when omitted. */
  port?: number;
  /** The secret-store key the host resolves a username/password pair from. */
  credentialsKey?: string;
}

/** One validation failure: the E-code, the offending profile, and the detail. */
export interface ProfileValidationError {
  code: ProblemCode;
  /** The profile name, or `#<index + 1>` when the profile has no usable name. */
  profile: string;
  detail: string;
}

/** The tcp port a profile assumes when none is configured (first IANA dynamic port). */
export const DEFAULT_TCP_PORT = 49152;

const MAX_PORT = 65535;
const MAX_HOSTNAME_LENGTH = 253;

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

    if (record.transport !== 'stdio' && record.transport !== 'tcp') {
      errors.push(error(ProblemCode.ConnectionProfileInvalid, name, `the transport must be "stdio" or "tcp", not ${JSON.stringify(record.transport)}`));
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

    if (transport === 'stdio') {
      if (program === undefined || program.trim().length === 0) {
        errors.push(error(ProblemCode.ConnectionProfileInvalid, name, 'a stdio profile needs the program the spawned session loads'));
        return;
      }
      if (address !== undefined) {
        errors.push(error(ProblemCode.ConnectionAddressInvalid, name, 'a stdio profile spawns a local session and must not set an address'));
        return;
      }
      if (port !== undefined) {
        errors.push(error(ProblemCode.ConnectionPortInvalid, name, 'a stdio profile spawns a local session and must not set a port'));
        return;
      }
    }
    else {
      if (address === undefined || address.trim().length === 0) {
        errors.push(error(ProblemCode.ConnectionProfileInvalid, name, 'a tcp profile needs the device address'));
        return;
      }
      if (!isValidAddress(address)) {
        errors.push(error(ProblemCode.ConnectionAddressInvalid, name, `the address "${address}" is not an IPv4/IPv6 literal or RFC 1123 hostname`));
        return;
      }
    }

    profiles.push({
      name,
      transport,
      program,
      address,
      port: transport === 'tcp' ? port ?? DEFAULT_TCP_PORT : undefined,
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

/**
 * An address is an IPv4 dotted-quad, an IPv6 literal, or an RFC 1123
 * hostname: letters, digits, and hyphens, one-to-63-character labels that do
 * not start or end with a hyphen, at most 253 characters overall. A dotted
 * name whose labels are all digits is treated as a mistyped IPv4 literal and
 * rejected when an octet is out of range (so '999.1.1.1' is not a hostname).
 */
export function isValidAddress(address: string): boolean {
  const host = address.trim();
  if (host.length === 0 || host.length > MAX_HOSTNAME_LENGTH) {
    return false;
  }
  if (isIpv4Address(host) || isIpv6Address(host)) {
    return true;
  }
  const labels = host.split('.');
  // An all-numeric dotted name is a mistyped IPv4 literal, not a hostname:
  // '999.1.1.1' must be rejected rather than pass as an RFC 1123 hostname
  // whose labels happen to be all digits.
  if (labels.every(label => /^[0-9]+$/.test(label))) {
    return false;
  }
  return labels.every(label => /^[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?$/.test(label));
}

/** Four decimal octets in 0-255. */
function isIpv4Address(host: string): boolean {
  const parts = host.split('.');
  return parts.length === 4
    && parts.every(part => /^\d{1,3}$/.test(part) && Number(part) <= 255);
}

/** Hex groups and `::` compression, loosely — enough to tell a literal from a hostname. */
function isIpv6Address(host: string): boolean {
  return host.includes(':') && /^[0-9A-Fa-f:]+$/.test(host);
}
