import * as assert from 'assert';

import { ConnectionProfile } from '../../connectionProfiles';
import {
  backoffDelayMs,
  ConnectionManager,
  ConnectionPolicy,
  ConnectionState,
  OpenedConnection,
} from '../../connectionState';
import {
  HotEditStatus,
  HotEditTransport,
  IdentityInfo,
} from '../../hotEditSession';
import { ProblemCode } from '../../problems';

/** An in-memory transport: the responder answers commands synchronously. */
class MockTransport implements HotEditTransport {
  readonly sent: string[] = [];
  handler: ((line: string) => void) | undefined;
  private readonly lineListeners: ((line: string) => void)[] = [];
  private readonly exitListeners: (() => void)[] = [];

  sendLine(line: string): void {
    this.sent.push(line);
    this.handler?.(line);
  }

  onLine(listener: (line: string) => void): void {
    this.lineListeners.push(listener);
  }

  onExit(listener: () => void): void {
    this.exitListeners.push(listener);
  }

  emitLine(line: string): void {
    for (const listener of this.lineListeners) {
      listener(line);
    }
  }

  emitExit(): void {
    for (const listener of this.exitListeners) {
      listener();
    }
  }
}

class MockOpened implements OpenedConnection {
  readonly transport = new MockTransport();
  closed = false;

  close(): void {
    this.closed = true;
  }
}

const DEVICE = { name: 'ironplcvm', model: 'IronPLC SoftPLC', modification: 'vm-cli', firmwareVersion: '0.13.0' };
const APPLICATION = { mode: 'normal', active: 1, normal: 1, candidate: null, application: 1, migration: false, rounds: 0 };

function identityLine(protocol = 1): string {
  return JSON.stringify({ response: 'identity', protocol, device: DEVICE, application: APPLICATION });
}

const STATUS_LINE = JSON.stringify({ response: 'status', ...APPLICATION, rounds: 7 });

interface RecordedError {
  code: ProblemCode;
  context: string;
}

interface Setup {
  manager: ConnectionManager;
  opened: MockOpened[];
  states: ConnectionState[];
  errors: RecordedError[];
  identities: IdentityInfo[];
  statuses: HotEditStatus[];
  sleeps: number[];
  heartbeat: { tick: () => void; starts: number };
}

interface SetupOptions {
  /** Overrides the transport factory; the default answers identity and getStatus. */
  open?: (profile: ConnectionProfile, opened: MockOpened[]) => Promise<OpenedConnection>;
  policy?: Partial<ConnectionPolicy>;
}

function tcpProfile(): ConnectionProfile {
  return { name: 'Cell 1', transport: 'tcp', address: '192.168.1.10', port: 49152 };
}

function stdioProfile(): ConnectionProfile {
  return { name: 'Local VM', transport: 'stdio', program: 'main.st' };
}

function setup(options?: SetupOptions): Setup {
  const opened: MockOpened[] = [];
  const states: ConnectionState[] = [];
  const errors: RecordedError[] = [];
  const identities: IdentityInfo[] = [];
  const statuses: HotEditStatus[] = [];
  const sleeps: number[] = [];
  const heartbeat = { tick: () => {}, starts: 0 };

  const defaultOpen = (profile: ConnectionProfile): Promise<OpenedConnection> => {
    const target = new MockOpened();
    target.transport.handler = (line: string) => {
      const command = JSON.parse(line).command as string;
      if (command === 'identity') {
        target.transport.emitLine(identityLine());
      }
      else if (command === 'getStatus') {
        target.transport.emitLine(STATUS_LINE);
      }
    };
    opened.push(target);
    return Promise.resolve(target);
  };

  const manager = new ConnectionManager({
    open: options?.open
      ? profile => options.open!(profile, opened)
      : defaultOpen,
    events: {
      onState: (state) => {
        states.push(state);
      },
      onIdentity: (identity) => {
        identities.push(identity);
      },
      onStatus: (status) => {
        statuses.push(status);
      },
      report: (code, context) => {
        errors.push({ code, context });
      },
    },
    clock: {
      sleep: (ms: number) => {
        sleeps.push(ms);
        return Promise.resolve();
      },
      random: () => 0.5,
      startHeartbeat: (tick: () => void) => {
        heartbeat.starts++;
        heartbeat.tick = tick;
        return () => {};
      },
    },
    policy: options?.policy,
  });

  return { manager, opened, states, errors, identities, statuses, sleeps, heartbeat };
}

/** Lets the manager's promise chains settle: each turn is one macrotask. */
async function flush(times = 30): Promise<void> {
  for (let i = 0; i < times; i++) {
    await new Promise<void>(resolve => setImmediate(resolve));
  }
}

const TEST_POLICY: Partial<ConnectionPolicy> = {
  connectTimeoutMs: 100,
  responseTimeoutMs: 40,
  heartbeatIntervalMs: 1000,
  heartbeatTolerance: 2,
  maxReconnectAttempts: 5,
};

suite('backoffDelayMs', () => {
  test('backoffDelayMs_when_attempts_then_full_jitter_over_exponential_growth', () => {
    assert.strictEqual(backoffDelayMs(1, () => 0.5), 250);
    assert.strictEqual(backoffDelayMs(2, () => 0.5), 500);
    assert.strictEqual(backoffDelayMs(3, () => 0.5), 1000);
    assert.strictEqual(backoffDelayMs(4, () => 0.5), 2000);
    assert.strictEqual(backoffDelayMs(5, () => 0.5), 4000);
  });

  test('backoffDelayMs_when_growth_exceeds_cap_then_8s_ceiling', () => {
    assert.strictEqual(backoffDelayMs(6, () => 0.5), 4000);
    assert.strictEqual(backoffDelayMs(12, () => 1), 8000);
  });

  test('backoffDelayMs_when_random_zero_then_no_delay', () => {
    assert.strictEqual(backoffDelayMs(5, () => 0), 0);
  });
});

suite('ConnectionManager', () => {
  test('connect_when_tcp_transport_opens_and_identity_answers_then_connected', async () => {
    const { manager, opened, states, identities, heartbeat } = setup({ policy: TEST_POLICY });

    const result = await manager.connect(tcpProfile());

    assert.strictEqual(result, 'connected');
    assert.deepStrictEqual(states, ['connecting', 'connected']);
    assert.strictEqual(opened.length, 1);
    assert.strictEqual(heartbeat.starts, 1);
    assert.strictEqual(manager.activeSession !== undefined, true);
    assert.strictEqual(identities.length, 1);
    assert.strictEqual(identities[0].device.name, 'ironplcvm');
  });

  test('connect_when_transport_open_fails_then_e0013_and_disconnected', async () => {
    const { manager, states, errors } = setup({
      policy: TEST_POLICY,
      open: () => Promise.reject(new Error('connection refused')),
    });

    const result = await manager.connect(tcpProfile());

    assert.strictEqual(result, 'failed');
    assert.deepStrictEqual(states, ['connecting', 'disconnected']);
    assert.strictEqual(errors.length, 1);
    assert.strictEqual(errors[0].code, ProblemCode.ConnectFailed);
    assert.ok(errors[0].context.includes('connection refused'));
    assert.strictEqual(manager.activeSession, undefined);
  });

  test('connect_when_open_times_out_then_e0013', async () => {
    const { manager, errors } = setup({
      policy: TEST_POLICY,
      open: () => new Promise<OpenedConnection>(() => {}),
    });

    const result = await manager.connect(tcpProfile());

    assert.strictEqual(result, 'failed');
    assert.strictEqual(errors.length, 1);
    assert.strictEqual(errors[0].code, ProblemCode.ConnectFailed);
    assert.ok(errors[0].context.includes('timed out'));
  });

  test('connect_when_identity_refused_with_vcode_then_e0013_carries_vcode', async () => {
    const { manager, errors } = setup({
      policy: TEST_POLICY,
      open: (profile, openedList) => {
        const target = new MockOpened();
        target.transport.handler = () => {
          target.transport.emitLine(JSON.stringify({ response: 'error', vCode: 'V6014', message: 'one engineering session' }));
        };
        openedList.push(target);
        return Promise.resolve(target);
      },
    });

    const result = await manager.connect(tcpProfile());

    assert.strictEqual(result, 'failed');
    assert.strictEqual(errors.length, 1);
    assert.strictEqual(errors[0].code, ProblemCode.ConnectFailed);
    assert.ok(errors[0].context.includes('V6014'));
    assert.strictEqual(manager.currentState, 'disconnected');
  });

  test('connect_when_identity_answers_higher_protocol_then_e0013', async () => {
    const { manager, errors } = setup({
      policy: TEST_POLICY,
      open: (profile, openedList) => {
        const target = new MockOpened();
        target.transport.handler = () => {
          target.transport.emitLine(identityLine(2));
        };
        openedList.push(target);
        return Promise.resolve(target);
      },
    });

    const result = await manager.connect(tcpProfile());

    assert.strictEqual(result, 'failed');
    assert.strictEqual(errors.length, 1);
    assert.strictEqual(errors[0].code, ProblemCode.ConnectFailed);
    assert.ok(errors[0].context.includes('session protocol 2'));
  });

  test('connect_when_already_connected_then_refuses_without_opening_a_second_transport', async () => {
    const { manager, opened } = setup({ policy: TEST_POLICY });
    await manager.connect(tcpProfile());

    const result = await manager.connect(tcpProfile());

    assert.strictEqual(result, 'already-connected');
    assert.strictEqual(opened.length, 1);
  });

  test('disconnect_when_connected_then_closes_transport_and_disconnected', async () => {
    const { manager, opened, states, errors } = setup({ policy: TEST_POLICY });
    await manager.connect(tcpProfile());

    manager.disconnect();

    assert.strictEqual(manager.currentState, 'disconnected');
    assert.strictEqual(opened[0].closed, true);
    assert.deepStrictEqual(states, ['connecting', 'connected', 'disconnected']);
    assert.deepStrictEqual(errors, []);
    assert.strictEqual(manager.activeSession, undefined);
  });

  test('transportFault_when_tcp_then_reconnects_and_recovers', async () => {
    const { manager, opened, states, errors } = setup({ policy: TEST_POLICY });
    await manager.connect(tcpProfile());

    opened[0].transport.emitExit();
    assert.strictEqual(manager.currentState, 'reconnecting');
    await flush();

    assert.strictEqual(manager.currentState, 'connected');
    assert.strictEqual(opened.length, 2);
    assert.deepStrictEqual(states, ['connecting', 'connected', 'reconnecting', 'connected']);
    assert.deepStrictEqual(errors, []);
  });

  test('transportFault_when_tcp_and_attempts_exhaust_then_e0014_and_disconnected', async () => {
    let opens = 0;
    const { manager, opened, states, errors, sleeps } = setup({
      policy: TEST_POLICY,
      open: (profile, openedList) => {
        opens++;
        if (opens === 1) {
          const first = new MockOpened();
          first.transport.handler = (line: string) => {
            first.transport.emitLine(identityLine());
          };
          openedList.push(first);
          return Promise.resolve(first);
        }
        return Promise.reject(new Error('device is down'));
      },
    });
    await manager.connect(tcpProfile());
    await flush();

    opened[0].transport.emitExit();
    await flush(60);

    assert.strictEqual(manager.currentState, 'disconnected');
    assert.strictEqual(opens, 6, 'one connect plus five reconnect attempts');
    assert.deepStrictEqual(sleeps, [500, 1000, 2000, 4000], 'full jitter at 0.5 over attempts 2-5');
    assert.strictEqual(errors.length, 1);
    assert.strictEqual(errors[0].code, ProblemCode.ConnectionLost);
    assert.ok(errors[0].context.includes('device is down'));
    assert.deepStrictEqual(states, ['connecting', 'connected', 'reconnecting', 'disconnected']);
  });

  test('transportFault_when_reconnect_refused_with_vcode_then_no_further_attempts', async () => {
    const opened: MockOpened[] = [];
    let opens = 0;
    const { manager, errors, sleeps } = setup({
      policy: TEST_POLICY,
      open: () => {
        opens++;
        const target = new MockOpened();
        target.transport.handler = () => {
          if (opens === 1) {
            target.transport.emitLine(identityLine());
          }
          else {
            target.transport.emitLine(JSON.stringify({ response: 'error', vCode: 'V6014', message: 'taken over' }));
          }
        };
        opened.push(target);
        return Promise.resolve(target);
      },
    });
    await manager.connect(tcpProfile());

    opened[0].transport.emitExit();
    await flush();

    assert.strictEqual(manager.currentState, 'disconnected');
    assert.strictEqual(opens, 2, 'the coded refusal ends the sequence after the first attempt');
    assert.deepStrictEqual(sleeps, [], 'a refusal never waits another backoff');
    assert.strictEqual(errors.length, 1);
    assert.strictEqual(errors[0].code, ProblemCode.ConnectionLost);
    assert.ok(errors[0].context.includes('V6014'));
  });

  test('transportFault_when_stdio_child_exits_then_e0014_without_respawn', async () => {
    const { manager, opened, states, errors } = setup({ policy: TEST_POLICY });
    await manager.connect(stdioProfile());

    opened[0].transport.emitExit();
    await flush();

    assert.strictEqual(manager.currentState, 'disconnected');
    assert.strictEqual(opened.length, 1, 'the stdio child is never respawned');
    assert.strictEqual(errors.length, 1);
    assert.strictEqual(errors[0].code, ProblemCode.ConnectionLost);
    assert.deepStrictEqual(states, ['connecting', 'connected', 'disconnected']);
  });

  test('userDisconnect_during_reconnecting_then_abandons_retries_without_e0014', async () => {
    let opens = 0;
    const { manager, opened, states, errors } = setup({
      policy: TEST_POLICY,
      open: (profile, openedList) => {
        opens++;
        if (opens === 1) {
          const first = new MockOpened();
          first.transport.handler = () => {
            first.transport.emitLine(identityLine());
          };
          openedList.push(first);
          return Promise.resolve(first);
        }
        // The first reconnect attempt hangs until its connect timeout fires.
        return new Promise<OpenedConnection>(() => {});
      },
    });
    await manager.connect(tcpProfile());

    opened[0].transport.emitExit();
    await flush(2);
    assert.strictEqual(manager.currentState, 'reconnecting');

    manager.disconnect();
    await new Promise<void>(resolve => setTimeout(resolve, 150));
    await flush();

    assert.strictEqual(manager.currentState, 'disconnected');
    assert.strictEqual(opens, 2, 'no further attempt after the user disconnect');
    assert.deepStrictEqual(errors, []);
    assert.deepStrictEqual(states, ['connecting', 'connected', 'reconnecting', 'disconnected']);
  });

  test('heartbeat_when_two_consecutive_misses_then_reconnecting', async () => {
    let opens = 0;
    const { manager, heartbeat, states, errors } = setup({
      policy: TEST_POLICY,
      open: (profile, openedList) => {
        opens++;
        const target = new MockOpened();
        // Answer identity but never getStatus: every heartbeat misses.
        target.transport.handler = (line: string) => {
          if (JSON.parse(line).command === 'identity') {
            target.transport.emitLine(identityLine());
          }
        };
        openedList.push(target);
        return Promise.resolve(target);
      },
    });
    await manager.connect(tcpProfile());

    heartbeat.tick();
    await new Promise<void>(resolve => setTimeout(resolve, 80));
    heartbeat.tick();
    await new Promise<void>(resolve => setTimeout(resolve, 80));
    await flush();

    assert.strictEqual(manager.currentState, 'connected', 'the reconnect after the misses recovered');
    assert.deepStrictEqual(states, ['connecting', 'connected', 'reconnecting', 'connected']);
    assert.deepStrictEqual(errors, []);
    assert.strictEqual(opens, 2);
  });

  test('heartbeat_when_answer_arrives_then_status_event_and_no_reconnect', async () => {
    const { manager, heartbeat, statuses } = setup({ policy: TEST_POLICY });
    await manager.connect(tcpProfile());

    heartbeat.tick();
    await flush();

    assert.strictEqual(manager.currentState, 'connected');
    assert.strictEqual(statuses.length, 1);
    assert.strictEqual(statuses[0].rounds, 7);
  });

  test('session_when_coded_error_response_arrives_unasked_then_state_unchanged', async () => {
    const { manager, opened, errors } = setup({ policy: TEST_POLICY });
    await manager.connect(tcpProfile());

    opened[0].transport.emitLine(JSON.stringify({ response: 'error', vCode: 'V4007', message: 'layout mismatch' }));
    await flush();

    assert.strictEqual(manager.currentState, 'connected');
    assert.deepStrictEqual(errors, [], 'a coded refusal is a protocol answer, never a reconnect trigger');
  });
});
