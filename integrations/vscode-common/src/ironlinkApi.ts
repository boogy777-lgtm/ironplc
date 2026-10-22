/**
 * The API that the IronLink extension exposes to other extensions as the
 * `exports` of its `activate`, the way the Git extension exposes its API.
 *
 * IronLink finds controllers and keeps the data of the workstation (the route
 * record and the address list). What another extension needs of that is here:
 * the stored routes, the last browse with its time, a new search, setting a
 * route, and showing the Network view. The IronLink extension implements
 * {@link IronLinkApi}; the IronPLC extension reads it. The types are written
 * once, here, and are free of `vscode`, so both sides and the tests share them.
 *
 * The IronLink extension is found by {@link IRONLINK_EXTENSION_ID} (it has a
 * single id) and is optional: a consumer works without it and says why a
 * function that needs it is unavailable.
 */

/** The id of the IronLink extension, as `vscode.extensions.getExtension` takes it. */
export const IRONLINK_EXTENSION_ID = 'ironplc.ironlink';

/** The version of {@link IronLinkApi}; it grows when a member changes in a way a consumer can notice. */
export const IRONLINK_API_VERSION = 1;

/** A route that this workstation stored for a device, verified when it was set. */
export interface IronLinkRoute {
  /** The device id, 32 lowercase hexadecimal digits. */
  deviceId: string;
  /** The address the device answered from (an IP address, no port). */
  address: string;
  /** The TCP port of the device's session. */
  sessionPort: number;
  /** The discovery driver by which the device was verified. */
  driver: string;
  /** Milliseconds since the Unix epoch at which the device answered. */
  verifiedAt: number;
}

/** A device that a search heard, at one address. */
export interface ObservedDevice {
  deviceId: string;
  name: string;
  /** The address the device answered from (an IP address, no port). */
  address: string;
  sessionPort: number;
  protected: boolean;
  /** Milliseconds since the Unix epoch at which the reply was received. */
  seenAt: number;
}

/** The result of the last browse the IronLink extension ran. */
export interface BrowseSnapshot {
  /** Milliseconds since the Unix epoch at which the search finished. */
  browsedAt: number;
  /** Every device heard, once for each address, newest observation first. */
  devices: ObservedDevice[];
}

/** A route that was set. */
export interface RouteSet {
  ok: true;
  route: IronLinkRoute;
  /** Whether the device had a route, which this one replaced. */
  replaced: boolean;
}

/**
 * A route that could not be set. `code` is the `L####` code of IronLink (nothing
 * answered, another device answered, the address is not valid) or, when the
 * program could not be run at all, the `E####` code of the shared catalog.
 * `message` is the whole text to show, starting with the code.
 */
export interface RouteFailure {
  ok: false;
  code: string;
  message: string;
}

/** The outcome of {@link IronLinkApi.setRoute}. */
export type RouteOutcome = RouteSet | RouteFailure;

/** The API of the IronLink extension. */
export interface IronLinkApi {
  /** {@link IRONLINK_API_VERSION} of the extension that provides this API. */
  readonly apiVersion: number;
  /** The routes stored on this workstation, ordered by device id. */
  routeList(): Promise<IronLinkRoute[]>;
  /** The last browse of the extension (also one run from the Network view), or `undefined` before any. */
  lastBrowse(): BrowseSnapshot | undefined;
  /** Searches the network, shows the result in the Network view and remembers it as the last browse. */
  browse(): Promise<BrowseSnapshot>;
  /**
   * Verifies the route to `deviceId` at `address` (`IP` or `IP:PORT`, as the
   * program takes it) with a fresh identify and stores it. Nothing is stored,
   * and a {@link RouteFailure} is returned, when the verification fails. A
   * failure of the program is a value; this does not throw for it.
   */
  setRoute(deviceId: string, address: string): Promise<RouteOutcome>;
  /** Shows the Network view. */
  revealNetwork(): Promise<void>;
}

/**
 * Every member of {@link IronLinkApi}. The mapped type needs one entry for each
 * member, so a member added to the interface without an entry here does not
 * compile, and a conformance test compares an implementation against the list.
 */
export const IRONLINK_API_MEMBERS: { readonly [K in keyof IronLinkApi]: true } = {
  apiVersion: true,
  routeList: true,
  lastBrowse: true,
  browse: true,
  setRoute: true,
  revealNetwork: true,
};
