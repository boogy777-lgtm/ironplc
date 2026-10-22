/**
 * The `tcp` connection method: open a socket to the device at the profile's
 * `address` and `port` (the session port when none is set). A lost
 * connection is retried; the device is not started by IronPLC, so there is no
 * session start to record.
 */

import * as path from 'path';

import type { Anchor } from '../baselineLogic';
import { ProblemCode } from '../vscodeCommon';
import { DEFAULT_SESSION_PORT } from '../sessionConstants';
import { outputFileNameForFolder } from '../taskProviderLogic';
import { connectTcpLineTransport } from '../tcpTransport';
import type { ConnectionMethod } from '../connectionMethods';

const MAX_HOSTNAME_LENGTH = 253;

export const tcpMethod: ConnectionMethod = {
  checkFields(fields) {
    if (fields.address === undefined || fields.address.trim().length === 0) {
      return { code: ProblemCode.ConnectionProfileInvalid, detail: 'a tcp profile needs the device address' };
    }
    if (!isValidAddress(fields.address)) {
      return { code: ProblemCode.ConnectionAddressInvalid, detail: `the address "${fields.address}" is not an IPv4/IPv6 literal or RFC 1123 hostname` };
    }
    return undefined;
  },

  defaultPort: DEFAULT_SESSION_PORT,

  needsLocalTools: false,

  detail: fields => `${fields.address}:${fields.port}`,

  async open(fields) {
    const transport = await connectTcpLineTransport(fields.address!, fields.port!);
    return { transport, close: () => transport.dispose() };
  },

  reconnects: true,

  recordsSessionStart: false,

  /**
   * The container the build task writes in the first workspace folder,
   * `<folder>/<folder>.iplc`. The first folder is a fixed choice, so a record
   * is found again whatever file is open.
   */
  anchor(_fields, folders): Anchor {
    const folder = folders[0];
    if (folder === undefined) {
      return { reason: 'no workspace folder is open, so there is no application container to name a record after' };
    }
    return { anchor: path.join(folder.path, outputFileNameForFolder(folder.name)) };
  },
};

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
