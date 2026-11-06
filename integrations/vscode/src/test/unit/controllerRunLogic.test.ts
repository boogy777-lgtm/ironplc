import * as assert from 'assert';

import {
  ControllerRun,
  currentRun,
  doneMessage,
  NO_CONTROLLER_MESSAGE,
  runScanAction,
  runStateDetail,
  runStateText,
  runViewOf,
  SCAN_ACTIONS,
  scanActionNames,
  ScanSession,
} from '../../controllerRunLogic';
import { HotEditProtocolError, IdentityInfo, RunState } from '../../hotEditSession';
import { SUPPORTED_SESSION_PROTOCOL } from '../../sessionConstants';

const FAULT = { vCode: 'V4001', message: 'trap in task', taskId: 2, instanceId: 3 };

function identity(overrides?: Partial<IdentityInfo>): IdentityInfo {
  return {
    protocol: SUPPORTED_SESSION_PROTOCOL,
    device: { name: 'Press line 3', model: 'IronPLC SoftPLC', modification: 'vm-cli', runtimeVersion: '0.247.0' },
    application: {
      mode: 'normal', active: 1, normal: 1, candidate: null, application: 1, migration: false, rounds: 0,
    },
    ...overrides,
  };
}

/** A session that records what it was sent and answers as scripted. */
class FakeSession implements ScanSession {
  readonly sent: string[] = [];

  constructor(
    private readonly answer: RunState | Error,
    private readonly identityAnswer: IdentityInfo | Error = identity(),
  ) {}

  startScan(): Promise<RunState> {
    return this.reply('startScan');
  }

  stopScan(): Promise<RunState> {
    return this.reply('stopScan');
  }

  identity(): Promise<IdentityInfo> {
    this.sent.push('identity');
    return this.identityAnswer instanceof Error ? Promise.reject(this.identityAnswer) : Promise.resolve(this.identityAnswer);
  }

  private reply(wire: string): Promise<RunState> {
    this.sent.push(wire);
    return this.answer instanceof Error ? Promise.reject(this.answer) : Promise.resolve(this.answer);
  }
}

suite('SCAN_ACTIONS', () => {
  test('scanActionNames_when_listed_then_run_and_stop_with_their_wire_names', () => {
    assert.deepStrictEqual(scanActionNames(), ['run', 'stop']);
    assert.strictEqual(SCAN_ACTIONS.run.wire, 'startScan');
    assert.strictEqual(SCAN_ACTIONS.stop.wire, 'stopScan');
  });

  test('scanActionNames_when_listed_then_command_ids_are_distinct_and_never_the_local_simulation', () => {
    const ids = scanActionNames().map(name => SCAN_ACTIONS[name].commandId);
    assert.strictEqual(new Set(ids).size, ids.length);
    for (const id of ids) {
      assert.ok(!['ironplc.runProgram', 'ironplc.stopProgram', 'ironplc.pauseProgram', 'ironplc.stepScan'].includes(id), id);
    }
  });
});

suite('runScanAction', () => {
  test('runScanAction_when_no_session_then_no_session_and_nothing_sent', async () => {
    assert.deepStrictEqual(await runScanAction(undefined, 'run'), { kind: 'noSession', message: NO_CONTROLLER_MESSAGE });
    assert.deepStrictEqual(await runScanAction(undefined, 'stop'), { kind: 'noSession', message: NO_CONTROLLER_MESSAGE });
  });

  test('runScanAction_when_run_and_the_device_runs_then_done_running', async () => {
    const session = new FakeSession('running', identity({ runState: 'running' }));

    const outcome = await runScanAction(session, 'run');

    assert.deepStrictEqual(outcome, {
      kind: 'done',
      run: { runState: 'running' },
      message: 'IronPLC: the controller is running.',
    });
    assert.deepStrictEqual(session.sent, ['startScan', 'identity']);
  });

  test('runScanAction_when_stop_and_the_device_stops_then_done_stopped_and_only_stop_scan_is_sent', async () => {
    const session = new FakeSession('stopped', identity({ runState: 'stopped' }));

    const outcome = await runScanAction(session, 'stop');

    assert.deepStrictEqual(outcome, {
      kind: 'done',
      run: { runState: 'stopped' },
      message: 'IronPLC: the controller is stopped.',
    });
    assert.deepStrictEqual(session.sent, ['stopScan', 'identity']);
  });

  test('runScanAction_when_stop_of_a_faulted_device_then_done_stopped_and_the_fault_stays_reported', async () => {
    const session = new FakeSession('stopped', identity({ runState: 'stopped', fault: FAULT }));

    const outcome = await runScanAction(session, 'stop');

    assert.deepStrictEqual(outcome, {
      kind: 'done',
      run: { runState: 'stopped', fault: FAULT },
      message: 'IronPLC: the controller is stopped; the fault V4001 stays reported.',
    });
  });

  test('runScanAction_when_the_identity_cannot_be_asked_after_an_answer_then_the_answer_is_the_state', async () => {
    const session = new FakeSession('stopped', new Error('socket closed'));

    const outcome = await runScanAction(session, 'stop');

    assert.deepStrictEqual(outcome, {
      kind: 'done',
      run: { runState: 'stopped' },
      message: 'IronPLC: the controller is stopped.',
    });
  });

  test('runScanAction_when_start_of_a_faulted_device_is_refused_then_refused_v6019_with_the_fault_from_the_identity', async () => {
    const refusal = new HotEditProtocolError('V6019', 'the device is faulted and cannot start');
    const session = new FakeSession(refusal, identity({ runState: 'running', fault: FAULT }));

    const outcome = await runScanAction(session, 'run');

    assert.deepStrictEqual(outcome, {
      kind: 'refused',
      vCode: 'V6019',
      text: 'V6019 - the device is faulted and cannot start',
      run: { runState: 'running', fault: FAULT },
    });
  });

  test('runScanAction_when_the_host_has_no_scan_loop_then_refused_v4022', async () => {
    const refusal = new HotEditProtocolError('V4022', 'this host has no scan loop');
    const session = new FakeSession(refusal, identity());

    const outcome = await runScanAction(session, 'stop');

    assert.strictEqual(outcome.kind, 'refused');
    if (outcome.kind === 'refused') {
      assert.strictEqual(outcome.vCode, 'V4022');
      assert.strictEqual(outcome.text, 'V4022 - this host has no scan loop');
    }
  });

  test('runScanAction_when_the_transport_fails_then_refused_without_a_code_and_no_identity_asked', async () => {
    const session = new FakeSession(new HotEditProtocolError(null, 'the ironplcvm process exited.'));

    const outcome = await runScanAction(session, 'run');

    assert.deepStrictEqual(outcome, { kind: 'refused', vCode: null, text: 'the ironplcvm process exited.' });
    assert.deepStrictEqual(session.sent, ['startScan']);
  });

  test('runScanAction_when_the_error_is_not_a_protocol_error_then_refused_with_its_message', async () => {
    const session = new FakeSession(new Error('write EPIPE'));

    const outcome = await runScanAction(session, 'stop');

    assert.deepStrictEqual(outcome, { kind: 'refused', vCode: null, text: 'write EPIPE' });
  });

  test('runScanAction_when_refused_and_the_identity_cannot_be_asked_then_no_run_view', async () => {
    const session = new FakeSession(new HotEditProtocolError('V6019', 'faulted'), new Error('gone'));

    const outcome = await runScanAction(session, 'run');

    assert.deepStrictEqual(outcome, { kind: 'refused', vCode: 'V6019', text: 'V6019 - faulted' });
  });
});

suite('the run view', () => {
  test('runViewOf_when_the_identity_lacks_run_state_and_fault_then_running_without_a_fault', () => {
    const view = runViewOf(identity());

    assert.deepStrictEqual(view, { runState: 'running' });
    assert.ok(!('fault' in view));
  });

  test('runViewOf_when_the_identity_has_both_then_both', () => {
    assert.deepStrictEqual(
      runViewOf(identity({ runState: 'stopped', fault: FAULT })),
      { runState: 'stopped', fault: FAULT },
    );
  });

  test('currentRun_when_an_answer_exists_then_it_wins_else_the_identity_else_none', () => {
    const answer: ControllerRun = { runState: 'stopped' };

    assert.deepStrictEqual(currentRun({ run: answer, identity: identity() }), answer);
    assert.deepStrictEqual(currentRun({ run: null, identity: identity({ runState: 'stopped' }) }), { runState: 'stopped' });
    assert.deepStrictEqual(currentRun({ identity: identity() }), { runState: 'running' });
    assert.strictEqual(currentRun({ identity: null }), null);
  });

  test('runStateText_when_running_stopped_or_faulted_then_the_word_with_the_code_for_a_fault', () => {
    assert.strictEqual(runStateText({ runState: 'running' }), 'running');
    assert.strictEqual(runStateText({ runState: 'stopped' }), 'stopped');
    assert.strictEqual(runStateText({ runState: 'running', fault: FAULT }), 'faulted V4001');
    assert.strictEqual(runStateText({ runState: 'stopped', fault: FAULT }), 'faulted V4001');
  });

  test('runStateDetail_when_faulted_then_the_state_and_the_fault_in_full', () => {
    assert.deepStrictEqual(runStateDetail({ runState: 'stopped' }), ['Run state: stopped']);
    assert.deepStrictEqual(runStateDetail({ runState: 'running', fault: FAULT }), [
      'Run state: running',
      'Fault: V4001 - trap in task (task 2, instance 3)',
    ]);
  });

  test('doneMessage_when_running_then_the_plain_sentence', () => {
    assert.strictEqual(doneMessage({ runState: 'running' }), 'IronPLC: the controller is running.');
  });
});
