/**
 * The texts the extension shows for the answers of `ironlink` that are not
 * failures: the reply to an identify, the outcome of an address change and the
 * summary of a search. Vscode-free, with the clock and the time format
 * injected, so each text is unit-tested.
 */

import {
  AddressAddResponse,
  AddressRemoveResponse,
  BrowseResponse,
  IdentifyResponse,
} from './ironlinkProtocol';
import { shortDeviceId, socketAddress } from './networkTree';
import { RouteSet } from './vscodeCommon';

/** A text and how loudly to show it. */
export interface Notice {
  severity: 'information' | 'warning';
  text: string;
}

/**
 * The reply to an identify with its time. An identify that nothing answers is
 * a normal answer of the program, shown as a warning and not as an error.
 */
export function describeIdentify(response: IdentifyResponse, formatTime: (ms: number) => string): Notice {
  switch (response.answer) {
    case 'device': {
      const seen = response.observation;
      const where = socketAddress(seen.route.address, seen.sessionPort);
      const protectedText = seen.protected ? ', credentials required' : '';
      return {
        severity: 'information',
        text: `${seen.name} (${seen.model}, runtime ${seen.runtimeVersion}, id ${shortDeviceId(seen.deviceId)}`
          + `${protectedText}) answered from ${where} at ${formatTime(seen.seenAt)}.`,
      };
    }
    case 'otherVersion': {
      const other = response.otherVersion;
      return {
        severity: 'warning',
        text: `${other.address} answered with discovery version ${other.wireVersion}, `
          + `which this IronLink cannot read, at ${formatTime(other.seenAt)}. Update IronLink.`,
      };
    }
    case 'none':
      return {
        severity: 'warning',
        text: `No answer from ${response.address} within ${response.timeoutMs} ms.`,
      };
  }
}

/** The outcome of adding an address. */
export function describeAddressAdd(response: AddressAddResponse): string {
  return response.added
    ? `Added ${response.address} to the address list.`
    : `${response.address} is in the address list already.`;
}

/** The outcome of removing an address. */
export function describeAddressRemove(response: AddressRemoveResponse): string {
  return response.removed
    ? `Removed ${response.address} from the address list.`
    : `${response.address} was not in the address list.`;
}

/** The outcome of setting a route: the route verified and stored for a device. */
export function describeRouteSet(set: RouteSet): string {
  const route = set.route;
  return `Route to ${shortDeviceId(route.deviceId)} is ${socketAddress(route.address, route.sessionPort)}`
    + `${set.replaced ? ' (replaced the earlier route).' : '.'}`;
}

/** One line for the log: what a search found. */
export function describeBrowse(response: BrowseResponse): string {
  const routes = response.drivers.reduce(
    (total, driver) => total + driver.devices.reduce(
      (count, device) => count + device.variants.reduce((n, variant) => n + variant.routes.length, 0),
      0,
    ),
    0,
  );
  const drivers = response.drivers.map(driver => driver.kind).join(', ') || 'no driver';
  return `Search found ${routes} ${routes === 1 ? 'route' : 'routes'} (${drivers}); `
    + `${response.routesNotSeen.length} stored ${response.routesNotSeen.length === 1 ? 'route' : 'routes'} not seen.`;
}
