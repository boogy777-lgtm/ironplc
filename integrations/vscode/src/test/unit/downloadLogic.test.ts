import * as assert from 'assert';

import {
  DOWNLOAD_ACTIONS,
  downloadActionNames,
  DownloadIo,
  DownloadOutcome,
  DownloadSession,
  formatReplaceConfirmation,
  outcomeMessage,
  REINIT_REASON_TEXT,
  reinitReasonText,
  ReplaceConfirmation,
  runDownload,
  syncIdsStep,
} from '../../downloadLogic';
import { BuildPhase } from '../../buildLogic';
import {
  EditCommandView,
  EditView,
  formatStatusText,
  HotEditProtocolError,
  HotEditStatus,
  Reinitialised,
  ReplaceNotSupportedError,
} from '../../hotEditSession';
import { SyncReport } from '../../syncUidsLogic';
import { rejectWith } from './testHelpers';

const REFUSED_CANCEL = 'untest and cancel are not supported after a schema-changing test; assemble instead';

function command(label: string, enabled: boolean, code?: string, text?: string, steps?: string[]): EditCommandView {
  return {
    enabled,
    label,
    ...(code !== undefined && text !== undefined ? { reason: { code, text } } : {}),
    ...(steps !== undefined ? { steps } : {}),
  };
}

/** The edit view of a device in `stage`, as the device reports it for a replace (the golden file of the runtime). */
function view(stage: 'idle' | 'accepted' | 'testingReplace' | 'swapPending'): EditView {
  const staged = command('Accept', false, 'V4013', 'a candidate is already staged');
  return {
    stage,
    commands: {
      accept: stage === 'idle' ? command('Accept', true) : staged,
      test: stage === 'accepted'
        ? command('Test', true)
        : command('Test', false, stage === 'idle' ? 'V4012' : 'V4015', 'not now'),
      untest: command('Untest', false, 'V4011', REFUSED_CANCEL),
      assemble: stage === 'testingReplace' ? command('Assemble', true) : command('Assemble', false, 'V4017', 'assemble requires the candidate to have executed under Test'),
      cancel: stage === 'accepted' ? command('Cancel', true) : command('Cancel', false, 'V4011', REFUSED_CANCEL),
      finalizeAll: command('Finalize all', stage === 'idle', 'V4013', 'a candidate is already staged', ['accept', 'test', 'assemble']),
    },
    ...(stage === 'idle'
      ? { next: { command: 'accept', label: 'Accept' } }
      : stage === 'accepted'
        ? { next: { command: 'test', label: 'Test' } }
        : stage === 'testingReplace'
          ? { next: { command: 'assemble', label: 'Assemble' } }
          : {}),
  };
}

function statusOf(edit: EditView | undefined, overrides?: Partial<HotEditStatus>): HotEditStatus {
  return {
    mode: 'normal',
    active: 2,
    normal: 2,
    candidate: null,
    application: 2,
    migration: false,
    rounds: 7,
    ...(edit !== undefined ? { edit } : {}),
    ...overrides,
  };
}

type Stage = 'idle' | 'accepted' | 'testingReplace' | 'swapPending';

/** A device that walks the replace stages and records every command it is sent. */
class FakeDevice implements DownloadSession {
  readonly calls: string[] = [];
  stage: Stage = 'idle';
  /** What `getStatus` answers instead of the walk, when set. */
  override: HotEditStatus | undefined;
  failOn: Partial<Record<string, Error>> = {};
  /** The stage after the assemble; `idle` unless a test loses the candidate. */
  afterTest: Stage = 'testingReplace';
  reinitialised: Reinitialised[] = [];

  async acceptReplace(program: Uint8Array): Promise<Reinitialised[]> {
    this.calls.push(`acceptReplace:${program.length}`);
    this.throwIfFailing('accept');
    this.stage = 'accepted';
    return this.reinitialised;
  }

  async testEdits(): Promise<void> {
    this.calls.push('test');
    this.throwIfFailing('test');
    this.stage = this.afterTest;
  }

  async assembleEdits(): Promise<void> {
    this.calls.push('assemble');
    this.throwIfFailing('assemble');
    this.stage = 'idle';
  }

  async cancelEdits(): Promise<void> {
    this.calls.push('cancel');
    this.throwIfFailing('cancel');
    this.stage = 'idle';
  }

  async getStatus(): Promise<HotEditStatus> {
    this.calls.push('getStatus');
    return this.override ?? statusOf(view(this.stage));
  }

  private throwIfFailing(name: string): void {
    const error = this.failOn[name];
    if (error !== undefined) {
      throw error;
    }
  }
}

interface Sent {
  bytes: Uint8Array;
  path: string;
}

/** The injected steps, recording the order against the device's calls. */
function makeIo(device: FakeDevice, overrides?: Partial<DownloadIo<Sent>>): { io: DownloadIo<Sent>; log: string[]; phases: BuildPhase[]; confirmations: ReplaceConfirmation[] } {
  const log: string[] = [];
  const phases: BuildPhase[] = [];
  const confirmations: ReplaceConfirmation[] = [];
  const mark = (entry: string): void => {
    log.push(entry);
    device.calls.push(entry);
  };
  const io: DownloadIo<Sent> = {
    syncIds: async () => {
      mark('sync');
      return true;
    },
    compile: async () => {
      mark('compile');
      return { bytes: new Uint8Array([1, 2, 3]), path: 'main.iplc' };
    },
    confirm: async (request) => {
      mark('confirm');
      confirmations.push(request);
      return true;
    },
    acknowledge: async (sent, status) => {
      mark(`acknowledge:${sent.path}:gen${status.active}`);
    },
    onPhase: phase => phases.push(phase),
    ...overrides,
  };
  return { io, log, phases, confirmations };
}

const EDIT = { name: 'main.st', origin: 'ironplc-vscode' };

suite('DOWNLOAD_ACTIONS', () => {
  test('DOWNLOAD_ACTIONS_when_listed_then_each_member_names_a_command_of_the_replace_mode', () => {
    assert.deepStrictEqual(downloadActionNames(), ['download']);
    for (const name of downloadActionNames()) {
      assert.ok(DOWNLOAD_ACTIONS[name].commandId.startsWith('ironplc.'));
      assert.strictEqual(DOWNLOAD_ACTIONS[name].mode, 'replace');
    }
    assert.strictEqual(DOWNLOAD_ACTIONS.download.commandId, 'ironplc.download');
    assert.strictEqual(DOWNLOAD_ACTIONS.download.title, 'Download to Controller');
  });
});

suite('runDownload', () => {
  test('runDownload_when_device_follows_the_view_then_sync_compile_accept_confirm_test_assemble_acknowledge', async () => {
    const device = new FakeDevice();
    device.reinitialised = [{ uid: 4, name: 'Count', reason: 'typeChanged' }];
    const { io, log, phases } = makeIo(device);

    const outcome = await runDownload(device, io, EDIT, false);

    assert.deepStrictEqual(log, ['sync', 'compile', 'confirm', 'acknowledge:main.iplc:gen2']);
    assert.deepStrictEqual(
      device.calls.filter(call => !call.startsWith('getStatus')),
      ['sync', 'compile', 'acceptReplace:3', 'confirm', 'test', 'assemble', 'acknowledge:main.iplc:gen2'],
    );
    assert.deepStrictEqual(phases, ['compiling', 'uploading', 'verifying', 'running']);
    assert.strictEqual(outcome.kind, 'done');
    if (outcome.kind === 'done') {
      assert.deepStrictEqual(outcome.reinitialised, [{ uid: 4, name: 'Count', reason: 'typeChanged' }]);
      assert.strictEqual(outcome.status.edit?.stage, 'idle');
    }
  });

  test('runDownload_when_each_step_runs_then_the_status_is_read_again_after_it', async () => {
    const device = new FakeDevice();
    const { io } = makeIo(device);

    await runDownload(device, io, EDIT, false);

    const afterAccept = device.calls.indexOf('acceptReplace:3');
    assert.deepStrictEqual(
      device.calls.slice(afterAccept).filter(call => call !== 'confirm'),
      ['acceptReplace:3', 'getStatus', 'test', 'getStatus', 'assemble', 'getStatus', 'acknowledge:main.iplc:gen2'],
    );
  });

  test('runDownload_when_confirming_then_the_request_carries_the_list_the_program_and_the_baseline', async () => {
    const device = new FakeDevice();
    device.reinitialised = [{ reason: 'noStableId' }];
    const { io, confirmations } = makeIo(device);

    await runDownload(device, io, EDIT, true);

    assert.deepStrictEqual(confirmations, [
      { program: 'main.st', reinitialised: [{ reason: 'noStableId' }], deviceChanged: true },
    ]);
  });

  test('runDownload_when_sync_is_left_open_then_nothing_is_compiled_or_sent', async () => {
    const device = new FakeDevice();
    const { io, log } = makeIo(device, { syncIds: async () => false });

    const outcome = await runDownload(device, io, EDIT, false);

    assert.deepStrictEqual(outcome, { kind: 'syncOpen' });
    assert.deepStrictEqual(log, []);
    assert.deepStrictEqual(device.calls, []);
  });

  test('runDownload_when_compile_fails_then_the_error_propagates_and_nothing_is_sent', async () => {
    const device = new FakeDevice();
    const { io } = makeIo(device, { compile: async () => Promise.reject(new Error('P0001')) });

    const err = await rejectWith(runDownload(device, io, EDIT, false));

    assert.strictEqual((err as Error).message, 'P0001');
    assert.deepStrictEqual(device.calls, ['sync']);
  });

  test('runDownload_when_person_declines_then_candidate_is_cancelled_and_nothing_runs', async () => {
    const device = new FakeDevice();
    const { io, log } = makeIo(device, { confirm: async () => false });

    const outcome = await runDownload(device, io, EDIT, false);

    assert.strictEqual(outcome.kind, 'cancelled');
    assert.ok(outcome.kind === 'cancelled' && outcome.discarded);
    assert.deepStrictEqual(device.calls.filter(call => call === 'cancel'), ['cancel']);
    assert.ok(!device.calls.includes('test'));
    assert.ok(!device.calls.includes('assemble'));
    assert.ok(!log.some(entry => entry.startsWith('acknowledge')));
    assert.strictEqual(device.stage, 'idle');
  });

  test('runDownload_when_accept_is_refused_then_refused_at_accept_and_nothing_else_is_sent', async () => {
    const device = new FakeDevice();
    device.failOn.accept = new HotEditProtocolError('V4013', 'a candidate is already staged');
    const { io, log } = makeIo(device);

    const outcome = await runDownload(device, io, EDIT, false);

    assert.strictEqual(outcome.kind, 'refused');
    if (outcome.kind === 'refused') {
      assert.strictEqual(outcome.step, 'accept');
      assert.strictEqual(outcome.error.vCode, 'V4013');
      assert.strictEqual(outcome.discarded, false);
    }
    assert.deepStrictEqual(log, ['sync', 'compile']);
    assert.ok(!device.calls.includes('cancel'));
  });

  test('runDownload_when_device_does_not_know_the_mode_then_candidate_is_cancelled_and_refused', async () => {
    const device = new FakeDevice();
    device.acceptReplace = async () => {
      device.calls.push('acceptReplace:3');
      device.stage = 'accepted';
      throw new ReplaceNotSupportedError();
    };
    const { io, log } = makeIo(device);

    const outcome = await runDownload(device, io, EDIT, false);

    assert.strictEqual(outcome.kind, 'refused');
    if (outcome.kind === 'refused') {
      assert.strictEqual(outcome.step, 'accept');
      assert.ok(outcome.error instanceof ReplaceNotSupportedError);
      assert.strictEqual(outcome.discarded, true);
    }
    assert.ok(device.calls.includes('cancel'));
    assert.ok(!log.includes('confirm'));
    assert.strictEqual(device.stage, 'idle');
  });

  test('runDownload_when_test_is_refused_then_candidate_is_cancelled_as_the_view_allows_and_nothing_is_recorded', async () => {
    const device = new FakeDevice();
    device.failOn.test = new HotEditProtocolError('V4001', 'the candidate traps');
    const { io, log } = makeIo(device);

    const outcome = await runDownload(device, io, EDIT, false);

    assert.strictEqual(outcome.kind, 'refused');
    if (outcome.kind === 'refused') {
      assert.strictEqual(outcome.step, 'test');
      assert.strictEqual(outcome.error.vCode, 'V4001');
      assert.strictEqual(outcome.discarded, true);
      assert.strictEqual(outcome.status?.edit?.stage, 'idle');
    }
    assert.ok(!device.calls.includes('assemble'));
    assert.ok(!log.some(entry => entry.startsWith('acknowledge')));
  });

  test('runDownload_when_assemble_is_refused_under_test_then_cancel_is_not_sent_because_the_view_disallows_it', async () => {
    const device = new FakeDevice();
    device.failOn.assemble = new HotEditProtocolError('V4020', 'assemble failed');
    const { io, log } = makeIo(device);

    const outcome = await runDownload(device, io, EDIT, false);

    assert.strictEqual(outcome.kind, 'refused');
    if (outcome.kind === 'refused') {
      assert.strictEqual(outcome.step, 'assemble');
      assert.strictEqual(outcome.discarded, false);
      assert.strictEqual(outcome.status?.edit?.stage, 'testingReplace');
    }
    assert.ok(!device.calls.includes('cancel'));
    assert.ok(!log.some(entry => entry.startsWith('acknowledge')));
  });

  test('runDownload_when_the_view_disables_the_next_command_then_it_is_not_sent_and_the_reason_is_the_views', async () => {
    const device = new FakeDevice();
    const disabled = view('accepted');
    device.override = statusOf({
      ...disabled,
      commands: { ...disabled.commands, test: command('Test', false, 'V4015', 'operation not allowed in the current hot-edit phase') },
    });
    const { io } = makeIo(device);

    const outcome = await runDownload(device, io, EDIT, false);

    assert.strictEqual(outcome.kind, 'refused');
    if (outcome.kind === 'refused') {
      assert.strictEqual(outcome.step, 'test');
      assert.strictEqual(outcome.error.vCode, 'V4015');
      assert.strictEqual(outcome.error.message, 'operation not allowed in the current hot-edit phase');
    }
    assert.ok(!device.calls.includes('test'));
  });

  test('runDownload_when_the_device_reports_no_view_then_stalled_and_nothing_is_sent_after_the_accept', async () => {
    const device = new FakeDevice();
    device.override = statusOf(undefined);
    const { io, log } = makeIo(device);

    const outcome = await runDownload(device, io, EDIT, false);

    assert.strictEqual(outcome.kind, 'stalled');
    assert.ok(!device.calls.includes('test'));
    assert.ok(!log.some(entry => entry.startsWith('acknowledge')));
  });

  test('runDownload_when_a_swap_waits_for_a_boundary_then_stalled', async () => {
    const device = new FakeDevice();
    device.override = statusOf(view('swapPending'));
    const { io } = makeIo(device);

    const outcome = await runDownload(device, io, EDIT, false);

    assert.strictEqual(outcome.kind, 'stalled');
    assert.ok(!device.calls.includes('test'));
  });

  test('runDownload_when_the_candidate_is_gone_before_the_first_step_then_stalled_not_done', async () => {
    const device = new FakeDevice();
    device.override = statusOf(view('idle'));
    const { io, log } = makeIo(device);

    const outcome = await runDownload(device, io, EDIT, false);

    assert.strictEqual(outcome.kind, 'stalled');
    assert.ok(!log.some(entry => entry.startsWith('acknowledge')));
  });

  test('runDownload_when_the_candidate_is_lost_after_the_test_then_stalled_and_nothing_is_recorded', async () => {
    const device = new FakeDevice();
    device.afterTest = 'idle';
    const { io, log } = makeIo(device);

    const outcome = await runDownload(device, io, EDIT, false);

    assert.strictEqual(outcome.kind, 'stalled');
    assert.ok(!device.calls.includes('assemble'));
    assert.ok(!log.some(entry => entry.startsWith('acknowledge')));
  });

  test('runDownload_when_the_device_never_reaches_idle_then_the_steps_are_bounded_by_finalize_all', async () => {
    const device = new FakeDevice();
    // A device that stays under Test whatever it is sent.
    device.assembleEdits = async () => {
      device.calls.push('assemble');
    };
    const { io, log } = makeIo(device);

    const outcome = await runDownload(device, io, EDIT, false);

    assert.strictEqual(outcome.kind, 'stalled');
    assert.ok(device.calls.filter(call => call === 'assemble').length <= 3);
    assert.ok(!log.some(entry => entry.startsWith('acknowledge')));
  });

  test('runDownload_when_the_transport_fails_during_a_step_then_the_error_propagates', async () => {
    const device = new FakeDevice();
    device.failOn.test = new Error('socket closed');
    const { io } = makeIo(device);

    const err = await rejectWith(runDownload(device, io, EDIT, false));

    assert.strictEqual((err as Error).message, 'socket closed');
  });

  test('runDownload_when_the_cleanup_cancel_fails_then_the_refusal_is_still_reported', async () => {
    const device = new FakeDevice();
    device.failOn.test = new HotEditProtocolError('V4001', 'the candidate traps');
    device.failOn.cancel = new Error('socket closed');
    const { io } = makeIo(device);

    const outcome = await runDownload(device, io, EDIT, false);

    assert.strictEqual(outcome.kind, 'refused');
    assert.ok(outcome.kind === 'refused' && !outcome.discarded);
  });
});

suite('syncIdsStep', () => {
  const clean: SyncReport = { preserved: [], assigned: [], removed: [], renameCandidates: [], swapCandidates: [] };
  const open: SyncReport = {
    ...clean,
    renameCandidates: [{ old: { scope: 'main', name: 'a' }, new: { scope: 'main', name: 'b' } }],
  };

  test('syncIdsStep_when_container_then_true_without_syncing', async () => {
    let synced = false;

    const result = await syncIdsStep(true, async () => {
      synced = true;
      return clean;
    });

    assert.strictEqual(result, true);
    assert.strictEqual(synced, false);
  });

  test('syncIdsStep_when_report_has_no_candidates_then_true', async () => {
    assert.strictEqual(await syncIdsStep(false, async () => clean), true);
  });

  test('syncIdsStep_when_report_has_open_candidates_then_false', async () => {
    assert.strictEqual(await syncIdsStep(false, async () => open), false);
  });

  test('syncIdsStep_when_sync_fails_then_the_error_propagates', async () => {
    const err = await rejectWith(syncIdsStep(false, async () => Promise.reject(new Error('no sidecar'))));

    assert.strictEqual((err as Error).message, 'no sidecar');
  });
});

suite('formatReplaceConfirmation', () => {
  const base: ReplaceConfirmation = { program: 'main.st', reinitialised: [], deviceChanged: false };

  test('formatReplaceConfirmation_when_nothing_is_reinitialised_then_says_all_persistent_variables_are_kept', () => {
    const { message, detail } = formatReplaceConfirmation(base);

    assert.ok(message.includes('replaces the application'));
    assert.ok(detail.includes('main.st'));
    assert.ok(detail.includes('Only PERSISTENT variables whose declaration did not change keep their values'));
    assert.ok(detail.includes('RETAIN variables included'));
    assert.ok(detail.includes('No PERSISTENT variable is re-initialised'));
    assert.ok(detail.includes('cannot be undone'));
    assert.ok(!detail.includes('last downloaded'));
  });

  test('formatReplaceConfirmation_when_entries_then_each_is_listed_with_its_name_or_id_and_the_reason', () => {
    const { detail } = formatReplaceConfirmation({
      ...base,
      reinitialised: [
        { uid: 2, name: 'Retyped', reason: 'typeChanged' },
        { uid: 3, reason: 'notInCandidate' },
        { reason: 'noStableId' },
      ],
    });

    assert.ok(detail.includes('These PERSISTENT variables are re-initialised:'));
    assert.ok(detail.includes('  - Retyped: its type changed'));
    assert.ok(detail.includes('  - variable 3: it is not in the new application'));
    assert.ok(detail.includes('  - a variable without a name or an ID: it has no stable variable ID'));
    assert.ok(!detail.includes('No PERSISTENT variable is re-initialised'));
  });

  test('formatReplaceConfirmation_when_device_changed_then_says_what_was_changed_there_is_lost', () => {
    const { detail } = formatReplaceConfirmation({ ...base, deviceChanged: true });

    assert.ok(detail.includes('not the one this project last downloaded'));
  });

  test('formatReplaceConfirmation_when_no_program_name_then_names_the_project', () => {
    const { detail } = formatReplaceConfirmation({ ...base, program: undefined });

    assert.ok(detail.includes('replaced by the project'));
  });

  test('REINIT_REASON_TEXT_when_listed_then_every_reason_of_the_device_has_a_sentence', () => {
    assert.deepStrictEqual(
      Object.keys(REINIT_REASON_TEXT).sort(),
      ['noStableId', 'notInCandidate', 'qualifierChanged', 'typeChanged', 'unsupportedType'],
    );
    for (const sentence of Object.values(REINIT_REASON_TEXT)) {
      assert.ok(sentence.length > 0);
    }
  });

  test('reinitReasonText_when_reason_is_a_later_devices_then_it_is_shown_as_it_is', () => {
    assert.strictEqual(reinitReasonText('qualifierChanged'), REINIT_REASON_TEXT.qualifierChanged);
    assert.strictEqual(reinitReasonText('somethingNew'), 'reason: somethingNew');
  });
});

suite('outcomeMessage', () => {
  const message = (outcome: DownloadOutcome) => outcomeMessage(outcome, 'Download', formatStatusText);

  test('outcomeMessage_when_sync_open_then_warning_that_nothing_was_sent', () => {
    const result = message({ kind: 'syncOpen' });

    assert.strictEqual(result.level, 'warning');
    assert.ok(result.text.includes('nothing was sent'));
  });

  test('outcomeMessage_when_cancelled_and_discarded_then_unchanged', () => {
    const result = message({ kind: 'cancelled', discarded: true });

    assert.strictEqual(result.level, 'info');
    assert.ok(result.text.includes('unchanged'));
  });

  test('outcomeMessage_when_cancelled_and_not_discarded_then_says_the_device_kept_it_and_the_stage', () => {
    const result = message({ kind: 'cancelled', discarded: false, status: statusOf(view('accepted')) });

    assert.ok(result.text.includes('kept the candidate'));
    assert.ok(result.text.includes('stage accepted'));
    assert.ok(result.text.includes('Test'));
  });

  test('outcomeMessage_when_done_then_info_with_the_count_of_reinitialised_variables', () => {
    const status = statusOf(view('idle'));

    assert.ok(message({ kind: 'done', status, reinitialised: [] }).text.includes('All PERSISTENT variables kept their values'));
    assert.ok(message({ kind: 'done', status, reinitialised: [{ reason: 'noStableId' }] }).text.includes('1 PERSISTENT variable was re-initialised'));
    assert.ok(message({ kind: 'done', status, reinitialised: [{ reason: 'noStableId' }, { reason: 'typeChanged' }] }).text.includes('2 PERSISTENT variables were re-initialised'));
  });

  test('outcomeMessage_when_refused_with_a_code_then_the_text_starts_with_the_code_and_carries_it', () => {
    const result = message({
      kind: 'refused',
      step: 'assemble',
      error: new HotEditProtocolError('V4011', REFUSED_CANCEL),
      discarded: false,
      status: statusOf(view('testingReplace')),
    });

    assert.strictEqual(result.level, 'error');
    assert.strictEqual(result.vCode, 'V4011');
    assert.ok(result.text.startsWith('V4011 - '));
    assert.ok(result.text.includes('stopped at assemble'));
    assert.ok(result.text.includes('stage testingReplace'));
    assert.ok(result.text.includes('next step the device offers is Assemble'));
  });

  test('outcomeMessage_when_refused_and_discarded_then_the_application_is_unchanged', () => {
    const result = message({
      kind: 'refused',
      step: 'test',
      error: new HotEditProtocolError('V4001', 'the candidate traps'),
      discarded: true,
    });

    assert.ok(result.text.includes('discarded'));
    assert.ok(result.text.includes('unchanged'));
  });

  test('outcomeMessage_when_refused_without_a_code_then_no_code_is_carried', () => {
    const result = message({
      kind: 'refused',
      step: 'accept',
      error: new ReplaceNotSupportedError(),
      discarded: true,
    });

    assert.strictEqual(result.vCode, undefined);
    assert.ok(result.text.startsWith('IronPLC Download: '));
    assert.ok(result.text.includes('does not support a Download'));
  });

  test('outcomeMessage_when_stalled_then_warning_with_the_stage', () => {
    const result = message({ kind: 'stalled', status: statusOf(view('swapPending')) });

    assert.strictEqual(result.level, 'warning');
    assert.ok(result.text.includes('stage swapPending'));
  });

  test('outcomeMessage_when_stalled_without_a_view_then_no_stage_is_claimed', () => {
    const result = message({ kind: 'stalled', status: statusOf(undefined) });

    assert.ok(!result.text.includes('in the stage'));
  });
});
