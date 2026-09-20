/**
 * The client-side connection state machine of the engineering connection
 * (ADR-0063, Mechanism 2): Disconnected → Connecting → Connected →
 * Reconnecting, owned per profile by the one manager the extension glue
 * hosts — one active connection per workspace (ADR-0065). The machine adds
 * no state to the server; it composes the existing `HotEditSession` over an
 * injected transport factory and drives the spec's transitions:
 *
 * - Connecting: transport open (bounded by the connect timeout), then the
 *   `identity` handshake; any open failure, timeout, or identity error lands
 *   in Disconnected with E0013 ConnectFailed, carrying the server's V-code
 *   when the refusal carried one.
 * - Connected: the idle heartbeat (`getStatus`, the existing command) runs
 *   at its cadence; the configured tolerance of consecutive misses is a
 *   transport fault.
 * - Reconnecting: TCP faults retry bounded times under full jitter
 *   `random(0, min(500 ms * 2^(n-1), 8 s))`; a coded identity refusal never
 *   retries; exhaustion lands in Disconnected with E0014 ConnectionLost.
 *   stdio faults (the spawned child exiting) go straight to E0014 — the
 *   child is never respawned.
 * - User disconnect from any state lands in Disconnected and cancels
 *   everything else (a generation counter invalidates in-flight attempts).
 *
 * The module is vscode-free: the clock, the transport factory, and the
 * events are injected, so every transition is unit-testable with mock
 * transports — no real `ironplcvm` process, socket, or timer needed.
 */

import { ConnectionProfile } from './connectionProfiles';
import { ProblemCode } from './problems';
import {
  HotEditProtocolError,
  HotEditSession,
  HotEditStatus,
  HotEditTransport,
  IdentityInfo,
} from './hotEditSession';

/** The four states of the connection state machine. */
export type ConnectionState = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

/** The v1 defaults of the spec's timeout and retry policy. */
export const CONNECT_TIMEOUT_MS = 5000;
export const RESPONSE_TIMEOUT_MS = 30000;
export const HEARTBEAT_INTERVAL_MS = 5000;
export const HEARTBEAT_TOLERANCE = 2;
export const MAX_RECONNECT_ATTEMPTS = 5;

const BACKOFF_BASE_MS = 500;
const BACKOFF_CAP_MS = 8000;

/**
 * The delay before `attempt` (numbered from 1, no delay before the first) of
 * a reconnect sequence: full jitter over exponential growth, capped at 8 s —
 * `random(0, min(500 ms * 2^(n-1), 8 s))`.
 */
export function backoffDelayMs(attempt: number, random: () => number = Math.random): number {
  const ceiling = Math.min(BACKOFF_BASE_MS * 2 ** (attempt - 1), BACKOFF_CAP_MS);
  return Math.floor(random() * ceiling);
}

/** What one successful transport open hands the manager: the channel and how to close it. */
export interface OpenedConnection {
  transport: HotEditTransport;
  close(): void;
}

/** The data the UI renders for the current state. */
export interface ConnectionSnapshot {
  profileName: string | null;
  identity: IdentityInfo | null;
  lastStatus: HotEditStatus | null;
}

/** State/error/identity notifications the glue renders (status bar, panel, problems). */
export interface ConnectionEvents {
  onState(state: ConnectionState, snapshot: ConnectionSnapshot): void;
  onIdentity(identity: IdentityInfo): void;
  onStatus(status: HotEditStatus): void;
  report(code: ProblemCode, context: string): void;
}

/** Injectable time sources; the glue binds timers, tests bind fakes. */
export interface ConnectionClock {
  sleep(ms: number): Promise<void>;
  random(): number;
  /** Starts the heartbeat cadence; returns the stop function. */
  startHeartbeat(tick: () => void, intervalMs: number): () => void;
}

/** The numbers behind the timeout/retry policy, injectable for tests. */
export interface ConnectionPolicy {
  connectTimeoutMs: number;
  responseTimeoutMs: number;
  heartbeatIntervalMs: number;
  heartbeatTolerance: number;
  maxReconnectAttempts: number;
}

export const DEFAULT_CONNECTION_POLICY: ConnectionPolicy = {
  connectTimeoutMs: CONNECT_TIMEOUT_MS,
  responseTimeoutMs: RESPONSE_TIMEOUT_MS,
  heartbeatIntervalMs: HEARTBEAT_INTERVAL_MS,
  heartbeatTolerance: HEARTBEAT_TOLERANCE,
  maxReconnectAttempts: MAX_RECONNECT_ATTEMPTS,
};

export interface ConnectionManagerDeps {
  /** Opens the transport a profile names (spawned stdio child or TCP socket). */
  open(profile: ConnectionProfile): Promise<OpenedConnection>;
  events: ConnectionEvents;
  clock: ConnectionClock;
  policy?: Partial<ConnectionPolicy>;
}

/** What `connect` resolved to; a failure is reported through `events.report`. */
export type ConnectResult = 'connected' | 'failed' | 'already-connected';

type AttemptOutcome = 'connected' | 'refused' | 'fault';

/**
 * Owns one connection per extension host. All async entry points and
 * continuations carry a generation counter: `disconnect` bumps it, and every
 * stale continuation that observes a newer generation exits without acting —
 * the mechanism that makes "user disconnect cancels everything else" hold
 * without per-path flags.
 */
export class ConnectionManager {
  private readonly policy: ConnectionPolicy;
  private state: ConnectionState = 'disconnected';
  private generation = 0;
  private profile: ConnectionProfile | undefined;
  private opened: OpenedConnection | undefined;
  private session: HotEditSession | undefined;
  private identity: IdentityInfo | undefined;
  private lastStatus: HotEditStatus | undefined;
  private heartbeatMisses = 0;
  private stopHeartbeat: (() => void) | undefined;
  private lastError = '';

  constructor(private readonly deps: ConnectionManagerDeps) {
    this.policy = { ...DEFAULT_CONNECTION_POLICY, ...deps.policy };
  }

  get currentState(): ConnectionState {
    return this.state;
  }

  /** The live session, only while Connected — the only state that may send commands. */
  get activeSession(): HotEditSession | undefined {
    return this.state === 'connected' ? this.session : undefined;
  }

  get currentProfile(): ConnectionProfile | undefined {
    return this.profile;
  }

  snapshot(): ConnectionSnapshot {
    return {
      profileName: this.profile?.name ?? null,
      identity: this.identity ?? null,
      lastStatus: this.lastStatus ?? null,
    };
  }

  /**
   * Connects the profile: opens its transport and runs the `identity`
   * handshake. Refused (with E0013 reported) on any open failure, connect
   * timeout, or identity error; refuses without traffic when a connection is
   * already open — one active connection per workspace.
   */
  async connect(profile: ConnectionProfile): Promise<ConnectResult> {
    if (this.state !== 'disconnected') {
      return 'already-connected';
    }
    this.generation++;
    const gen = this.generation;
    this.profile = profile;
    this.setState('connecting');
    const outcome = await this.runAttempt(gen);
    if (outcome === 'connected') {
      return 'connected';
    }
    if (gen !== this.generation) {
      return 'failed';
    }
    this.landDisconnected(ProblemCode.ConnectFailed, this.lastError);
    return 'failed';
  }

  /** User disconnect: from any state, lands in Disconnected and cancels everything else. */
  disconnect(): void {
    this.generation++;
    this.stopHeartbeat?.();
    this.stopHeartbeat = undefined;
    this.teardownSession();
    this.profile = undefined;
    this.identity = undefined;
    this.lastStatus = undefined;
    this.heartbeatMisses = 0;
    this.setState('disconnected');
  }

  private async runAttempt(gen: number): Promise<AttemptOutcome> {
    let opened: OpenedConnection | undefined;
    try {
      const profile = this.profile!;
      opened = await withTimeout(
        this.deps.open(profile),
        this.policy.connectTimeoutMs,
        `timed out opening the ${profile.transport} transport after ${this.policy.connectTimeoutMs} ms`,
      );
      const session = new HotEditSession(opened.transport, () => this.onTransportExit(gen));
      const identity = await withTimeout(
        session.identity(),
        this.policy.connectTimeoutMs,
        `the identity handshake timed out after ${this.policy.connectTimeoutMs} ms`,
      );
      if (gen !== this.generation) {
        // The user disconnected while the handshake was in flight: the
        // result is stale and must not commit.
        opened.close();
        return 'fault';
      }
      this.opened = opened;
      this.session = session;
      this.identity = identity;
      this.lastStatus = identity.application;
      this.heartbeatMisses = 0;
      this.setState('connected');
      this.deps.events.onIdentity(identity);
      this.startHeartbeat(gen);
      return 'connected';
    }
    catch (err) {
      opened?.close();
      this.lastError = describeError(err);
      return isCodedRefusal(err) ? 'refused' : 'fault';
    }
  }

  private onTransportExit(gen: number): void {
    // Only a live Connected session's loss starts Reconnecting; teardown and
    // in-attempt exits are handled by the awaiting attempt itself.
    if (gen !== this.generation || this.state !== 'connected' || !this.session) {
      return;
    }
    this.lastError = `the ${this.profile!.transport} transport closed`;
    this.enterReconnecting(gen);
  }

  private async heartbeatTick(gen: number): Promise<void> {
    if (gen !== this.generation || this.state !== 'connected' || !this.session) {
      return;
    }
    try {
      const status = await withTimeout(
        this.session.getStatus(),
        this.policy.responseTimeoutMs,
        'the heartbeat answer timed out',
      );
      if (gen !== this.generation || this.state !== 'connected') {
        return;
      }
      this.heartbeatMisses = 0;
      this.lastStatus = status;
      this.deps.events.onStatus(status);
    }
    catch {
      if (gen !== this.generation || this.state !== 'connected') {
        return;
      }
      this.heartbeatMisses++;
      if (this.heartbeatMisses >= this.policy.heartbeatTolerance) {
        this.lastError = `the heartbeat missed ${this.policy.heartbeatTolerance} times in a row`;
        this.enterReconnecting(gen);
      }
    }
  }

  /** Connected → Reconnecting: stop the heartbeat, drop the session, schedule the retries. */
  private enterReconnecting(gen: number): void {
    this.stopHeartbeat?.();
    this.stopHeartbeat = undefined;
    this.teardownSession();
    this.setState('reconnecting');
    void this.runReconnects(gen);
  }

  private async runReconnects(gen: number): Promise<void> {
    // stdio children are never respawned: a fault goes straight to E0014.
    const attempts = this.profile?.transport === 'tcp' ? this.policy.maxReconnectAttempts : 0;
    for (let attempt = 1; attempt <= attempts; attempt++) {
      if (gen !== this.generation) {
        return;
      }
      if (attempt > 1) {
        await this.deps.clock.sleep(backoffDelayMs(attempt, this.deps.clock.random));
      }
      if (gen !== this.generation) {
        return;
      }
      const outcome = await this.runAttempt(gen);
      if (outcome === 'connected') {
        return;
      }
      if (outcome === 'refused') {
        // A coded identity refusal never triggers another reconnect.
        break;
      }
    }
    if (gen !== this.generation) {
      return;
    }
    this.landDisconnected(ProblemCode.ConnectionLost, this.lastError);
  }

  private landDisconnected(code: ProblemCode, context: string): void {
    this.teardownSession();
    this.profile = undefined;
    this.identity = undefined;
    this.lastStatus = undefined;
    this.heartbeatMisses = 0;
    this.setState('disconnected');
    this.deps.events.report(code, context);
  }

  private setState(state: ConnectionState): void {
    this.state = state;
    this.deps.events.onState(state, this.snapshot());
  }

  private startHeartbeat(gen: number): void {
    this.stopHeartbeat?.();
    this.stopHeartbeat = this.deps.clock.startHeartbeat(
      () => {
        void this.heartbeatTick(gen);
      },
      this.policy.heartbeatIntervalMs,
    );
  }

  private teardownSession(): void {
    this.session?.dispose();
    this.session = undefined;
    this.opened?.close();
    this.opened = undefined;
  }
}

/** True when the device answered with a coded refusal — never a reconnect trigger. */
function isCodedRefusal(err: unknown): boolean {
  return err instanceof HotEditProtocolError && err.vCode !== null;
}

/** Renders an error for the E-code context: the coded refusal form or the message. */
function describeError(err: unknown): string {
  if (err instanceof HotEditProtocolError) {
    return err.toString();
  }
  return err instanceof Error ? err.message : String(err);
}

/** Rejects after `ms` when `promise` has not settled; the policy's one timeout shape. */
function withTimeout<T>(promise: Promise<T>, ms: number, message: string): Promise<T> {
  return new Promise<T>((resolve, reject) => {
    const timer = setTimeout(() => reject(new Error(message)), ms);
    promise.then(
      (value) => {
        clearTimeout(timer);
        resolve(value);
      },
      (err: unknown) => {
        clearTimeout(timer);
        reject(err instanceof Error ? err : new Error(String(err)));
      },
    );
  });
}
