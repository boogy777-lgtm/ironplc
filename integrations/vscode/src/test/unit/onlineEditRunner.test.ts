import * as assert from 'assert';
import * as fs from 'fs';
import * as path from 'path';

import { BuildPhase } from '../../buildLogic';
import { EditView, formatStatusText, HotEditProtocolError, HotEditStatus } from '../../hotEditSession';
import { BuildFact, dialogModel, DialogItem } from '../../onlineEditLogic';
import { DialogSession, OnlineEditIo, runItem, runMessage } from '../../onlineEditRunner';

/** The runtime's edit views for the states a run walks through (the golden file). */
const GOLDEN: { state: string; view: EditView }[] = JSON.parse(fs.readFileSync(goldenPath(), 'utf-8'));

function goldenPath(): string {
  let dir = __dirname;
  for (;;) {
    const candidate = path.join(dir, 'compiler', 'runtime', 'resources', 'test', 'edit_view_golden.json');
    if (fs.existsSync(candidate)) {
      return candidate;
    }
    assert.notStrictEqual(path.dirname(dir), dir, 'the golden file was not found');
    dir = path.dirname(dir);
  }
}

function golden(state: string): EditView {
  const entry = GOLDEN.find(candidate => candidate.state === state);
  assert.ok(entry);
  return entry.view;
}

function statusOf(state: string, rounds = 0): HotEditStatus {
  return { mode: 'normal', active: 1, normal: 1, candidate: null, application: 1, migration: false, rounds, edit: golden(state) };
}

/**
 * A device that walks idle, accepted, testing and back to idle, as the runtime
 * does; `refuse` makes a command answer with a coded refusal instead.
 */
class FakeDevice implements DialogSession {
  readonly calls: string[] = [];
  state = 'NoCandidate';
  refuse = new Map<string, HotEditProtocolError>();
  statusFails = false;
  /** The state a status read shows after a command, to let a test hold the device back. */
  hold = new Set<string>();

  private move(command: string, to: string): void {
    this.calls.push(command);
    const refusal = this.refuse.get(command);
    if (refusal !== undefined) {
      throw refusal;
    }
    if (!this.hold.has(command)) {
      this.state = to;
    }
  }

  async testEdits(): Promise<void> { this.move('test', 'Testing'); }
  async untestEdits(): Promise<void> { this.move('untest', 'Accepted'); }
  async assembleEdits(): Promise<void> { this.move('assemble', 'NoCandidate'); }
  async cancelEdits(): Promise<void> { this.move('cancel', 'NoCandidate'); }

  /** What the shell's accept hook does on the device. */
  accepted(): void { this.move('accept', 'Accepted'); }

  async getStatus(): Promise<HotEditStatus> {
    this.calls.push('status');
    if (this.statusFails) {
      throw new Error('the connection is lost');
    }
    return statusOf(this.state);
  }
}

class FakeIo implements OnlineEditIo {
  buildResult = true;
  acceptResult = true;
  acknowledged: HotEditStatus[] = [];
  readonly phases: (BuildPhase | null)[] = [];

  constructor(private readonly device: FakeDevice) {}

  async build(): Promise<boolean> {
    this.device.calls.push('build');
    return this.buildResult;
  }

  async accept(): Promise<boolean> {
    if (this.acceptResult) {
      this.device.accepted();
    }
    else {
      this.device.calls.push('accept-stopped');
    }
    return this.acceptResult;
  }

  async acknowledge(status: HotEditStatus): Promise<void> {
    this.device.calls.push('acknowledge');
    this.acknowledged.push(status);
  }

  onPhase(phase: BuildPhase | null): void {
    this.phases.push(phase);
  }
}

function itemOf(state: string, build: BuildFact, kind: DialogItem['kind']): DialogItem {
  const found = dialogModel({ edit: golden(state) }, build).items.find(candidate => candidate.kind === kind);
  assert.ok(found);
  return found;
}

suite('online edit runner', () => {
  test('runItem_when_finalize_all_then_the_steps_run_in_order_and_the_view_is_read_between_them', async () => {
    const device = new FakeDevice();
    const io = new FakeIo(device);

    const result = await runItem(device, io, itemOf('NoCandidate', 'built', 'finalizeAll'));

    assert.deepStrictEqual(device.calls, [
      'accept', 'status', 'test', 'status', 'assemble',
      // The acknowledged assemble reads the status it hands to the record, then the run ends with a read.
      'status', 'acknowledge', 'status',
    ]);
    assert.deepStrictEqual(result.outcome, { kind: 'done', assembled: true });
    assert.strictEqual(result.status?.edit?.stage, 'idle');
    assert.strictEqual(io.acknowledged.length, 1);
  });

  test('runItem_when_finalize_all_without_a_build_then_build_runs_first_and_the_phases_follow_the_stages', async () => {
    const device = new FakeDevice();
    const io = new FakeIo(device);

    await runItem(device, io, itemOf('NoCandidate', 'notBuilt', 'finalizeAll'));

    assert.deepStrictEqual(device.calls.filter(call => call !== 'status'), ['build', 'accept', 'test', 'assemble', 'acknowledge']);
    assert.deepStrictEqual(io.phases, ['compiling', 'uploading', 'verifying', 'running', null]);
  });

  test('runItem_when_the_build_fails_then_nothing_is_sent_and_the_view_is_read', async () => {
    const device = new FakeDevice();
    const io = new FakeIo(device);
    io.buildResult = false;

    const result = await runItem(device, io, itemOf('NoCandidate', 'notBuilt', 'next'));

    assert.deepStrictEqual(device.calls, ['build', 'status']);
    assert.deepStrictEqual(result.outcome, { kind: 'stopped', step: 'build' });
    assert.strictEqual(result.status?.edit?.stage, 'idle');
    assert.strictEqual(runMessage(result, formatStatusText), undefined);
  });

  test('runItem_when_the_accept_is_stopped_before_sending_then_the_rest_of_finalize_all_does_not_run', async () => {
    const device = new FakeDevice();
    const io = new FakeIo(device);
    io.acceptResult = false;

    const result = await runItem(device, io, itemOf('NoCandidate', 'built', 'finalizeAll'));

    assert.deepStrictEqual(device.calls, ['accept-stopped', 'status']);
    assert.deepStrictEqual(result.outcome, { kind: 'stopped', step: 'accept' });
  });

  test('runItem_when_the_device_refuses_a_step_then_the_rest_is_not_sent_and_the_view_is_read_and_shown', async () => {
    const device = new FakeDevice();
    device.refuse.set('test', new HotEditProtocolError('V4015', 'a swap is pending'));
    const io = new FakeIo(device);

    const result = await runItem(device, io, itemOf('NoCandidate', 'built', 'finalizeAll'));

    assert.deepStrictEqual(device.calls, ['accept', 'status', 'test', 'status']);
    assert.strictEqual(result.outcome.kind, 'refused');
    assert.strictEqual(io.acknowledged.length, 0);
    // The re-read view is the device's: a candidate is staged and its exits are enabled.
    assert.strictEqual(result.status?.edit?.stage, 'accepted');
    const message = runMessage(result, formatStatusText);
    assert.strictEqual(message?.level, 'error');
    assert.strictEqual(message?.vCode, 'V4015');
    assert.match(message?.text ?? '', /^V4015 - a swap is pending\. IronPLC Online Edit stopped at Test\./);
  });

  test('runItem_when_the_view_disables_the_next_step_then_it_is_not_sent_and_the_views_reason_is_shown', async () => {
    const device = new FakeDevice();
    // The device acknowledges the test, but the swap has not been applied: the view says assemble is refused.
    device.hold.add('test');
    const io = new FakeIo(device);
    device.state = 'Accepted';

    const result = await runItem(device, io, { kind: 'finalizeAll', label: 'Finalize all', description: '', enabled: true, steps: ['test', 'assemble'] });

    assert.deepStrictEqual(device.calls, ['test', 'status', 'status']);
    assert.strictEqual(result.outcome.kind, 'refused');
    if (result.outcome.kind === 'refused') {
      assert.strictEqual(result.outcome.step, 'assemble');
      assert.strictEqual(result.outcome.error.vCode, 'V4017');
    }
  });

  test('runItem_when_next_is_a_device_command_then_it_is_sent_once_without_a_prior_read', async () => {
    const device = new FakeDevice();
    device.state = 'Accepted';
    const io = new FakeIo(device);

    const result = await runItem(device, io, itemOf('Accepted', 'built', 'next'));

    assert.deepStrictEqual(device.calls, ['test', 'status']);
    assert.deepStrictEqual(result.outcome, { kind: 'done', assembled: false });
    assert.strictEqual(result.status?.edit?.stage, 'testing');
    assert.strictEqual(runMessage(result, formatStatusText), undefined);
  });

  test('runItem_when_cancel_under_test_then_cancel_is_sent_once_and_the_device_runs_the_steps', async () => {
    const device = new FakeDevice();
    device.state = 'Testing';
    const io = new FakeIo(device);

    const result = await runItem(device, io, itemOf('Testing', 'built', 'cancel'));

    assert.deepStrictEqual(device.calls, ['cancel', 'status']);
    assert.strictEqual(result.status?.edit?.stage, 'idle');
  });

  test('runItem_when_assemble_is_the_next_step_then_the_acknowledged_assemble_is_recorded_once', async () => {
    const device = new FakeDevice();
    device.state = 'Testing';
    const io = new FakeIo(device);

    const result = await runItem(device, io, itemOf('Testing', 'built', 'next'));

    assert.deepStrictEqual(device.calls, ['assemble', 'status', 'acknowledge', 'status']);
    assert.strictEqual(io.acknowledged.length, 1);
    const message = runMessage(result, formatStatusText);
    assert.strictEqual(message?.level, 'info');
    assert.match(message?.text ?? '', /the candidate is assembled/);
  });

  test('runItem_when_the_assemble_is_refused_then_nothing_is_recorded', async () => {
    const device = new FakeDevice();
    device.state = 'Testing';
    device.refuse.set('assemble', new HotEditProtocolError('V4017', 'assemble requires the candidate to have executed under Test'));
    const io = new FakeIo(device);

    const result = await runItem(device, io, itemOf('Testing', 'built', 'next'));

    assert.deepStrictEqual(device.calls, ['assemble', 'status']);
    assert.strictEqual(io.acknowledged.length, 0);
    assert.strictEqual(result.outcome.kind, 'refused');
  });

  test('runItem_when_the_accept_is_refused_by_the_device_then_the_run_ends_with_a_read_of_the_view', async () => {
    const device = new FakeDevice();
    const io = new FakeIo(device);
    io.accept = async () => {
      device.calls.push('accept');
      throw new HotEditProtocolError('V4013', 'a candidate is already staged');
    };

    const result = await runItem(device, io, itemOf('NoCandidate', 'built', 'next'));

    assert.deepStrictEqual(device.calls, ['accept', 'status']);
    assert.strictEqual(result.outcome.kind, 'refused');
  });

  test('runItem_when_a_step_fails_for_a_reason_that_is_not_a_refusal_then_the_outcome_is_failed_and_the_view_is_read', async () => {
    const device = new FakeDevice();
    device.state = 'Accepted';
    device.testEdits = async () => {
      device.calls.push('test');
      throw new Error('the ironplcvm process exited.');
    };
    const io = new FakeIo(device);

    const result = await runItem(device, io, itemOf('Accepted', 'built', 'next'));

    assert.strictEqual(result.outcome.kind, 'failed');
    assert.deepStrictEqual(device.calls, ['test', 'status']);
    assert.match(runMessage(result, formatStatusText)?.text ?? '', /exited\. \(at Test\)\./);
  });

  test('runItem_when_the_view_cannot_be_read_at_the_end_then_the_outcome_stands_without_a_status', async () => {
    const device = new FakeDevice();
    device.state = 'Accepted';
    device.statusFails = true;
    const io = new FakeIo(device);

    const result = await runItem(device, io, itemOf('Accepted', 'built', 'next'));

    assert.deepStrictEqual(result.outcome, { kind: 'done', assembled: false });
    assert.strictEqual(result.status, undefined);
  });

  test('runItem_when_a_step_is_not_one_this_client_sends_then_it_is_refused_with_no_code', async () => {
    const device = new FakeDevice();
    const io = new FakeIo(device);

    const result = await runItem(device, io, { kind: 'next', label: 'Verify', description: '', enabled: true, command: 'verify' });

    assert.strictEqual(result.outcome.kind, 'refused');
    if (result.outcome.kind === 'refused') {
      assert.strictEqual(result.outcome.error.vCode, null);
      assert.match(result.outcome.error.message, /cannot send verify/);
    }
    assert.deepStrictEqual(device.calls, ['status']);
  });

  test('runItem_when_the_item_is_disabled_then_nothing_runs_but_the_view_is_read', async () => {
    const device = new FakeDevice();
    const io = new FakeIo(device);

    const result = await runItem(device, io, itemOf('NoCandidate', 'built', 'cancel'));

    assert.deepStrictEqual(device.calls, ['status']);
    assert.deepStrictEqual(result.outcome, { kind: 'done', assembled: false });
  });

  test('runItem_when_the_edit_view_has_a_replace_candidate_then_the_ordinary_steps_run', async () => {
    const device = new FakeDevice();
    device.state = 'Accepted';
    const io = new FakeIo(device);

    const next = itemOf('AcceptedReplace', 'notBuilt', 'next');
    const result = await runItem(device, io, next);

    assert.strictEqual(next.command, 'test');
    assert.deepStrictEqual(result.outcome, { kind: 'done', assembled: false });
  });
});
