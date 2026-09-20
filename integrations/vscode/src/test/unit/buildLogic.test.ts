import * as assert from 'assert';

import {
  BuildPhase,
  BuildSession,
  resolveTrial,
  runBuildCommit,
  runBuildTrial,
} from '../../buildLogic';
import {
  EditIdentity,
  HotEditProtocolError,
  HotEditStatus,
  TypeChangePair,
} from '../../hotEditSession';
import {
  MigrationDecisionUi,
  migrationPlan,
} from '../../hotEditMigrationLogic';

/** Records every call in order; the answers queue responses per command. */
class MockSession implements BuildSession {
  readonly calls: string[] = [];
  readonly editArgs: (EditIdentity | undefined)[] = [];
  private readonly acceptQueue: Array<() => Promise<void>> = [];
  migration?: Record<string, 'init' | 'preserve'>;

  constructor(private readonly status: HotEditStatus = postStatus()) {}

  queueAcceptFailure(err: Error): void {
    this.acceptQueue.push(() => Promise.reject(err));
  }

  async acceptEdits(_program: Uint8Array, migration?: Record<string, 'init' | 'preserve'>, edit?: EditIdentity): Promise<void> {
    this.calls.push('accept');
    this.editArgs.push(edit);
    this.migration = migration;
    const next = this.acceptQueue.shift();
    if (next) {
      return next();
    }
    return Promise.resolve();
  }

  async testEdits(): Promise<void> {
    this.calls.push('test');
    return Promise.resolve();
  }

  async untestEdits(): Promise<void> {
    this.calls.push('untest');
    return Promise.resolve();
  }

  async assembleEdits(): Promise<void> {
    this.calls.push('assemble');
    return Promise.resolve();
  }

  async cancelEdits(): Promise<void> {
    this.calls.push('cancel');
    return Promise.resolve();
  }

  async getStatus(): Promise<HotEditStatus> {
    this.calls.push('status');
    return this.status;
  }
}

function postStatus(): HotEditStatus {
  return {
    mode: 'normal',
    active: 2,
    normal: 2,
    candidate: null,
    application: 2,
    migration: false,
    rounds: 3,
  };
}

const BYTES = new Uint8Array([1, 2, 3]);
const EDIT: EditIdentity = { name: 'main.st', origin: 'ironplc-vscode' };

/** A decisions UI that records and answers "init for all" (no preserves). */
function decisionsUi(): MigrationDecisionUi & { asked: TypeChangePair[][] } {
  const asked: TypeChangePair[][] = [];
  return {
    asked,
    async choosePreserved(pairs: readonly TypeChangePair[]): Promise<readonly number[]> {
      asked.push([...pairs]);
      return [];
    },
  };
}

function phases(): { phases: BuildPhase[]; onPhase: (phase: BuildPhase) => void } {
  const seen: BuildPhase[] = [];
  return { phases: seen, onPhase: (phase: BuildPhase) => seen.push(phase) };
}

suite('runBuildCommit', () => {
  test('runBuildCommit_when_connected_then_accept_test_assemble_in_order', async () => {
    const session = new MockSession();
    const ui = decisionsUi();
    const recorder = phases();

    const status = await runBuildCommit(session, BYTES, EDIT, ui, recorder.onPhase);

    assert.deepStrictEqual(session.calls, ['accept', 'test', 'assemble', 'status']);
    assert.deepStrictEqual(session.editArgs, [EDIT]);
    assert.strictEqual(status.active, 2);
  });

  test('runBuildCommit_when_phases_then_uploading_verifying_running', async () => {
    const session = new MockSession();
    const recorder = phases();

    await runBuildCommit(session, BYTES, EDIT, decisionsUi(), recorder.onPhase);

    assert.deepStrictEqual(recorder.phases, ['uploading', 'verifying', 'running']);
  });

  test('runBuildCommit_when_v4010_with_pairs_then_decisions_flow_and_resubmit_with_edit', async () => {
    const session = new MockSession();
    const pairs: TypeChangePair[] = [{ uid: 7, name: 'counter', from: 'I32', to: 'U32', sizeEqual: true }];
    session.queueAcceptFailure(new HotEditProtocolError('V4010', 'migration decisions required', pairs));
    const ui = decisionsUi();

    await runBuildCommit(session, BYTES, EDIT, ui, phases().onPhase);

    assert.deepStrictEqual(session.calls, ['accept', 'accept', 'test', 'assemble', 'status']);
    assert.strictEqual(ui.asked.length, 1);
    assert.deepStrictEqual(session.migration, migrationPlan(pairs, []));
    assert.deepStrictEqual(session.editArgs, [EDIT, EDIT], 'the edit identity rides both attempts');
  });

  test('runBuildCommit_when_host_refuses_then_vcode_propagates_and_sequence_stops', async () => {
    const session = new MockSession();
    session.queueAcceptFailure(new HotEditProtocolError('V4007', 'layout hash mismatch'));

    await assert.rejects(
      runBuildCommit(session, BYTES, EDIT, decisionsUi(), phases().onPhase),
      (err: unknown) => err instanceof HotEditProtocolError && err.vCode === 'V4007',
    );
    assert.deepStrictEqual(session.calls, ['accept']);
  });
});

suite('runBuildTrial', () => {
  test('runBuildTrial_when_connected_then_stops_after_test_without_assembling', async () => {
    const session = new MockSession();
    const recorder = phases();

    const status = await runBuildTrial(session, BYTES, EDIT, decisionsUi(), recorder.onPhase);

    assert.deepStrictEqual(session.calls, ['accept', 'test', 'status']);
    assert.deepStrictEqual(recorder.phases, ['uploading', 'verifying', 'running']);
    assert.strictEqual(status.active, 2);
  });
});

suite('resolveTrial', () => {
  test('resolveTrial_when_assemble_then_promotes_and_returns_status', async () => {
    const session = new MockSession();

    await resolveTrial(session, 'assemble');

    assert.deepStrictEqual(session.calls, ['assemble', 'status']);
  });

  test('resolveTrial_when_untest_then_reverts_and_returns_status', async () => {
    const session = new MockSession();

    await resolveTrial(session, 'untest');

    assert.deepStrictEqual(session.calls, ['untest', 'status']);
  });

  test('resolveTrial_when_cancel_then_discards_and_returns_status', async () => {
    const session = new MockSession();

    await resolveTrial(session, 'cancel');

    assert.deepStrictEqual(session.calls, ['cancel', 'status']);
  });
});
