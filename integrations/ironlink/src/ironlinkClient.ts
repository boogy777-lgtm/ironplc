/**
 * The client of `ironlink serve`: one command per line out, one response per
 * command in, in order. The program decides; the client sends a command, waits
 * for its response, and turns an error response into a typed failure.
 *
 * The process is started lazily on the first request and stopped by `dispose`.
 * When it exits, the requests in flight fail with the `IronLinkExited` problem
 * and the next request starts a new process. The module is vscode-free: the
 * process comes from an injected launcher, so a fake child exercises every
 * rule here (the in-order matching is `LineRequestChannel` of the shared
 * package).
 */

import {
  AddressAddResponse,
  AddressListResponse,
  AddressRemoveResponse,
  BrowseOptions,
  BrowseResponse,
  IdentifyResponse,
  IronLinkCommand,
  RouteListResponse,
  RouteSetResponse,
  ResponseName,
  ResponseOf,
  VersionResponse,
  encodeCommand,
  parseResponseLine,
} from './ironlinkProtocol';
import { IronLinkCommandError, IronLinkProblem } from './failures';
import { LineRequestChannel, LineTransport, ProblemCode } from './vscodeCommon';

/** A started `ironlink serve` process, as the client needs it. */
export interface IronLinkProcess {
  transport: LineTransport;
  /** Ends the process; its exit is reported through the transport. */
  stop(): void;
}

/**
 * Starts `ironlink serve`. It throws an `IronLinkProblem` when the program is
 * missing (`IronLinkNotFound`) or cannot be started (`IronLinkStartFailed`).
 */
export type IronLinkLauncher = () => Promise<IronLinkProcess>;

interface Session {
  process: IronLinkProcess;
  channel: LineRequestChannel;
}

export class IronLinkClient {
  /** The running session, once started. */
  private current: Session | undefined;
  /** The start in progress, so that concurrent first requests share one process. */
  private starting: Promise<Session> | undefined;
  private disposed = false;

  constructor(
    private readonly launcher: IronLinkLauncher,
    private readonly onStarted: (process: IronLinkProcess) => void = () => {},
  ) {}

  /** Finds controllers with every discovery driver. */
  browse(options: BrowseOptions = {}): Promise<BrowseResponse> {
    return this.send({ command: 'browse', ...definedOptions(options) }, 'browse');
  }

  /** Asks one route (`IP` or `IP:PORT`) for a fresh observation. */
  identify(address: string, options: BrowseOptions = {}): Promise<IdentifyResponse> {
    return this.send({ command: 'identify', address, ...definedOptions(options) }, 'identify');
  }

  /** Adds an address (`IP` or `IP:PORT`) to the stored address list. */
  addressAdd(address: string): Promise<AddressAddResponse> {
    return this.send({ command: 'addressAdd', address }, 'addressAdd');
  }

  /** Removes an address from the stored address list. */
  addressRemove(address: string): Promise<AddressRemoveResponse> {
    return this.send({ command: 'addressRemove', address }, 'addressRemove');
  }

  /** The stored address list. */
  addressList(): Promise<AddressListResponse> {
    return this.send({ command: 'addressList' }, 'addressList');
  }

  /**
   * Verifies the route to `deviceId` at `address` (`IP` or `IP:PORT`) with a
   * fresh identify and stores it on this workstation.
   */
  routeSet(deviceId: string, address: string, options: BrowseOptions = {}): Promise<RouteSetResponse> {
    return this.send({ command: 'routeSet', deviceId, address, ...definedOptions(options) }, 'routeSet');
  }

  /** The routes stored on this workstation. */
  routeList(): Promise<RouteListResponse> {
    return this.send({ command: 'routeList' }, 'routeList');
  }

  /** The versions of the program. */
  version(): Promise<VersionResponse> {
    return this.send({ command: 'version' }, 'version');
  }

  /** Stops the process, if one runs; later requests fail with `IronLinkExited`. */
  dispose(): void {
    this.disposed = true;
    this.current?.process.stop();
    this.current = undefined;
  }

  private async send<K extends ResponseName>(command: IronLinkCommand, expected: K): Promise<ResponseOf<K>> {
    const session = await this.ensureSession();
    const line = await session.channel.request(encodeCommand(command));
    let response;
    try {
      response = parseResponseLine(line);
    }
    catch (err) {
      // The wire desynced: no pending request can be matched reliably, so the
      // process is replaced by the next request.
      throw this.abandon(session, new IronLinkProblem(
        ProblemCode.IronLinkProtocolError,
        err instanceof Error ? err.message : String(err),
      ));
    }
    if (response.response === 'error') {
      if (response.lCode === null) {
        // A line the program could not read as a command: a bug of the client
        // or a program of another version, not a failure of the command.
        throw new IronLinkProblem(ProblemCode.IronLinkProtocolError, response.message);
      }
      throw new IronLinkCommandError(response.lCode, response.message);
    }
    if (response.response !== expected) {
      throw this.abandon(session, new IronLinkProblem(
        ProblemCode.IronLinkProtocolError,
        `a "${response.response}" response answered the "${command.command}" command`,
      ));
    }
    return response as ResponseOf<K>;
  }

  private async ensureSession(): Promise<Session> {
    if (this.disposed) {
      throw exited('the extension stopped the program');
    }
    if (this.current !== undefined) {
      return this.current;
    }
    if (this.starting === undefined) {
      // A launch that fails is not remembered: the next request tries again.
      this.starting = this.start().finally(() => {
        this.starting = undefined;
      });
    }
    return this.starting;
  }

  private async start(): Promise<Session> {
    const process = await this.launcher();
    const session: Session = {
      process,
      channel: new LineRequestChannel(
        process.transport,
        {
          ended: () => exited('the program was stopped'),
          exited: () => exited('the program ended before it answered'),
        },
        () => {
          // This process is gone; the next request starts another.
          if (this.current === session) {
            this.current = undefined;
          }
        },
      ),
    };
    if (this.disposed) {
      process.stop();
      throw exited('the extension stopped the program');
    }
    if (session.channel.isActive) {
      this.current = session;
    }
    this.onStarted(process);
    return session;
  }

  /** Fails what is pending with `reason`, drops the session and ends its process. */
  private abandon(session: Session, reason: IronLinkProblem): IronLinkProblem {
    session.channel.rejectPending(reason);
    if (this.current === session) {
      this.current = undefined;
    }
    session.process.stop();
    return reason;
  }
}

function exited(detail: string): IronLinkProblem {
  return new IronLinkProblem(ProblemCode.IronLinkExited, detail);
}

/** The options the caller set; an unset option is left to the program's default. */
function definedOptions(options: BrowseOptions): BrowseOptions {
  const defined: BrowseOptions = {};
  if (options.timeoutMs !== undefined) {
    defined.timeoutMs = options.timeoutMs;
  }
  if (options.attempts !== undefined) {
    defined.attempts = options.attempts;
  }
  return defined;
}
