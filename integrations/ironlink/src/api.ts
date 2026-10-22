/**
 * The API that this extension exposes to other extensions (the type
 * `IronLinkApi` of the shared package), built from the client of the program.
 *
 * The program decides; this module only shapes its answers for a consumer that
 * is not IronLink: the last browse is flattened to the devices heard, a route
 * is the program's stored route, and a failure of the program is a value
 * (`RouteFailure`) that carries its code, so a consumer needs neither the
 * wire types nor this extension's classes. The module is vscode-free: the
 * clock, the client and the editor's effects are injected.
 */

import { IronLinkCommandError, IronLinkProblem } from './failures';
import { BrowseOptions, BrowseResponse, RouteListResponse, RouteSetResponse, StoredRoute } from './ironlinkProtocol';
import {
  BrowseSnapshot,
  IRONLINK_API_VERSION,
  IronLinkApi,
  IronLinkRoute,
  ObservedDevice,
  RouteOutcome,
} from './vscodeCommon';

/** The commands of the program the API uses; `IronLinkClient` has this shape. */
export interface ApiClient {
  browse(options?: BrowseOptions): Promise<BrowseResponse>;
  routeSet(deviceId: string, address: string, options?: BrowseOptions): Promise<RouteSetResponse>;
  routeList(): Promise<RouteListResponse>;
}

/** The last browse the extension ran, with the time it finished. */
export interface RememberedBrowse {
  response: BrowseResponse;
  browsedAt: number;
}

/** Holds the last browse, for the Network view and for the API. */
export class BrowseMemory {
  private current: RememberedBrowse | undefined;

  remember(response: BrowseResponse, browsedAt: number): void {
    this.current = { response, browsedAt };
  }

  last(): RememberedBrowse | undefined {
    return this.current;
  }
}

/** What the API needs from the extension besides the program. */
export interface ApiDependencies {
  client: ApiClient;
  memory: BrowseMemory;
  /** Milliseconds since the Unix epoch. */
  now: () => number;
  /** The browse options the user set. */
  browseOptions: () => BrowseOptions;
  /** Shows a finished browse in the Network view and in the log. */
  show: (response: BrowseResponse) => void;
  /** Focuses the Network view. */
  revealNetwork: () => Promise<void>;
}

/**
 * The devices a browse heard, once for each address and port, newest first. A
 * device that two drivers heard at one address is one entry with the later
 * observation; a conflicting device id is an entry for each address.
 */
export function devicesHeard(response: BrowseResponse): ObservedDevice[] {
  const byRoute = new Map<string, ObservedDevice>();
  for (const driver of response.drivers) {
    for (const device of driver.devices) {
      for (const variant of device.variants) {
        for (const route of variant.routes) {
          const key = `${device.deviceId}@${route.address}:${route.sessionPort}`;
          const known = byRoute.get(key);
          if (known === undefined || known.seenAt < route.seenAt) {
            byRoute.set(key, {
              deviceId: device.deviceId,
              name: variant.name,
              address: route.address,
              sessionPort: route.sessionPort,
              protected: variant.protected,
              seenAt: route.seenAt,
            });
          }
        }
      }
    }
  }
  return [...byRoute.values()].sort((a, b) => b.seenAt - a.seenAt);
}

function routeOf(stored: StoredRoute): IronLinkRoute {
  return {
    deviceId: stored.deviceId,
    address: stored.address,
    sessionPort: stored.sessionPort,
    driver: stored.driver,
    verifiedAt: stored.verifiedAt,
  };
}

function snapshotOf(remembered: RememberedBrowse): BrowseSnapshot {
  return { browsedAt: remembered.browsedAt, devices: devicesHeard(remembered.response) };
}

/** The API of the extension, over the program and the memory of the last browse. */
export function createIronLinkApi(deps: ApiDependencies): IronLinkApi {
  return {
    apiVersion: IRONLINK_API_VERSION,

    async routeList() {
      return (await deps.client.routeList()).routes.map(routeOf);
    },

    lastBrowse() {
      const remembered = deps.memory.last();
      return remembered === undefined ? undefined : snapshotOf(remembered);
    },

    async browse() {
      const response = await deps.client.browse(deps.browseOptions());
      const remembered: RememberedBrowse = { response, browsedAt: deps.now() };
      deps.memory.remember(response, remembered.browsedAt);
      deps.show(response);
      return snapshotOf(remembered);
    },

    async setRoute(deviceId, address): Promise<RouteOutcome> {
      try {
        const set = await deps.client.routeSet(deviceId, address, deps.browseOptions());
        return {
          ok: true,
          route: routeOf({ deviceId: set.deviceId, ...set.route }),
          replaced: set.replaced,
        };
      }
      catch (error) {
        if (error instanceof IronLinkCommandError) {
          return { ok: false, code: error.lCode, message: error.message };
        }
        if (error instanceof IronLinkProblem) {
          return { ok: false, code: error.problem, message: error.message };
        }
        // Not a failure of the program: a defect of the extension, which has no code.
        throw error;
      }
    },

    revealNetwork: () => deps.revealNetwork(),
  };
}
