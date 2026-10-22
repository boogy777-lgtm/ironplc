/**
 * The wire shapes of `ironlink serve`: the JSON-lines command protocol of the
 * `ironlink` program (specs/design/ironlink.md, "The shell"). One command is
 * one line, and each command line is answered by one response line, in order.
 *
 * The program decides everything; these types only describe what it says, so
 * the extension can render it. The module is vscode-free so the client and the
 * tree builder can be unit-tested directly.
 */

/** How a route to a device is carried; today the session over TCP. */
export type RouteMethod = string;

/** One route by which a device variant answered a browse. */
export interface SeenRoute {
  /** The discovery driver that found it: `broadcast` or `addressList`. */
  driver: string;
  /** The address the reply came from (an IP address, no port). */
  address: string;
  sessionPort: number;
  method: RouteMethod;
  /** Milliseconds since the Unix epoch at which the reply was received. */
  seenAt: number;
  /** Whether the stored route record of this workstation holds exactly this route. */
  routed: boolean;
}

/** The replies of one device id that said the same identity. */
export interface DeviceVariant {
  name: string;
  model: string;
  runtimeVersion: string;
  protocol: number;
  protected: boolean;
  routes: SeenRoute[];
}

/** One device id as heard by a driver; several variants are an identity conflict. */
export interface DeviceEntry {
  /** 32 lowercase hexadecimal digits. */
  deviceId: string;
  conflict: boolean;
  variants: DeviceVariant[];
}

/** A datagram that is a discovery reply of another wire version. */
export interface OtherVersion {
  address: string;
  wireVersion: number;
  seenAt: number;
}

/** What one discovery driver found. */
export interface DriverReport {
  /** The driver: `broadcast` or `addressList`. */
  kind: string;
  /** What the driver aimed at. */
  targets: string[];
  devices: DeviceEntry[];
  otherVersions: OtherVersion[];
  /** How many datagrams were not discovery replies. */
  undecodable: number;
}

/** A route of the stored route record: how this workstation reaches a device id. */
export interface StoredRoute {
  deviceId: string;
  address: string;
  driver: string;
  sessionPort: number;
  /** Milliseconds since the Unix epoch at which the route was verified. */
  verifiedAt: number;
}

/** The answer to `browse`: the tree of the workstation and its drivers. */
export interface BrowseResponse {
  response: 'browse';
  workstation: { name: string };
  drivers: DriverReport[];
  /** Stored routes of devices that no driver heard in this browse. */
  routesNotSeen: StoredRoute[];
}

/** What one discovery reply said, with the route and the time it was seen. */
export interface Observation {
  deviceId: string;
  name: string;
  model: string;
  runtimeVersion: string;
  protocol: number;
  sessionPort: number;
  protected: boolean;
  route: { driver: string; address: string; sessionPort: number; method: RouteMethod };
  seenAt: number;
}

/** The answer to `identify`: a device, another wire version, or nothing. */
export type IdentifyResponse = {
  response: 'identify';
  /** The address asked, with its port. */
  address: string;
  timeoutMs: number;
} & (
  | { answer: 'device'; observation: Observation }
  | { answer: 'otherVersion'; otherVersion: OtherVersion }
  | { answer: 'none' }
);

/** The answer to `addressAdd`. */
export interface AddressAddResponse {
  response: 'addressAdd';
  address: string;
  /** False when the address was in the list already. */
  added: boolean;
}

/** The answer to `addressRemove`. */
export interface AddressRemoveResponse {
  response: 'addressRemove';
  address: string;
  /** False when the address was not in the list. */
  removed: boolean;
}

/** The answer to `addressList`. */
export interface AddressListResponse {
  response: 'addressList';
  /** `IP:PORT`, unique and sorted. */
  addresses: string[];
}

/** The answer to `routeSet`: the route that was verified and stored. */
export interface RouteSetResponse {
  response: 'routeSet';
  deviceId: string;
  route: {
    address: string;
    driver: string;
    sessionPort: number;
    /** Milliseconds since the Unix epoch at which the device answered. */
    verifiedAt: number;
  };
  /** Whether the device had a route, which this one replaced. */
  replaced: boolean;
}

/** The answer to `routeList`: the routes of this workstation, ordered by device id. */
export interface RouteListResponse {
  response: 'routeList';
  routes: StoredRoute[];
}

/** The answer to `version`. */
export interface VersionResponse {
  response: 'version';
  ironlink: string;
  discoveryWireVersion: number;
}

/** A failed command, or a line that was not a command (`lCode` is `null`). */
export interface ErrorResponse {
  response: 'error';
  lCode: string | null;
  message: string;
}

/** Every response line of `ironlink serve` this extension reads. */
export type IronLinkResponse =
  | BrowseResponse
  | IdentifyResponse
  | AddressAddResponse
  | AddressRemoveResponse
  | AddressListResponse
  | RouteSetResponse
  | RouteListResponse
  | VersionResponse
  | ErrorResponse;

/** The tag of a response. */
export type ResponseName = IronLinkResponse['response'];

/** The response type with the tag `K`. */
export type ResponseOf<K extends ResponseName> = Extract<IronLinkResponse, { response: K }>;

/** The options of a browse; the program applies its own default to a missing one. */
export interface BrowseOptions {
  timeoutMs?: number;
  attempts?: number;
}

/** The commands this extension sends, one `command` tag each. */
export type IronLinkCommand =
  | ({ command: 'browse' } & BrowseOptions)
  | ({ command: 'identify'; address: string } & BrowseOptions)
  | { command: 'addressAdd'; address: string }
  | { command: 'addressRemove'; address: string }
  | { command: 'addressList' }
  | ({ command: 'routeSet'; deviceId: string; address: string } & BrowseOptions)
  | { command: 'routeList' }
  | { command: 'version' };

/** The command line (without its newline) for a command. */
export function encodeCommand(command: IronLinkCommand): string {
  return JSON.stringify(command);
}

/**
 * Parses one response line. Throws a plain `Error` that names what is wrong
 * when the line is not a JSON object with a string `response` tag; the client
 * turns that into the protocol problem.
 */
export function parseResponseLine(line: string): IronLinkResponse {
  let parsed: unknown;
  try {
    parsed = JSON.parse(line);
  }
  catch (err) {
    throw new Error(`the line is not JSON (${err instanceof Error ? err.message : String(err)})`);
  }
  if (typeof parsed !== 'object' || parsed === null || typeof (parsed as { response?: unknown }).response !== 'string') {
    throw new Error('the line is not a response object with a "response" tag');
  }
  return parsed as IronLinkResponse;
}
