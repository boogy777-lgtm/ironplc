import * as assert from 'assert';
import {
  candidatesOf,
  describeMapping,
  ExecFileFn,
  formatKey,
  formatSyncSummary,
  MapUidError,
  mapUidArgs,
  parseSyncReport,
  SidecarKey,
  SyncCandidate,
  SyncReport,
  SyncResolutionUi,
  SyncUidsError,
  syncUidsArgs,
  syncVariableIds,
} from '../../syncUidsLogic';

/**
 * A mocked compiler transport: tests script the responses per invocation and
 * record every argument vector, so no real `ironplcc` child process is
 * needed to exercise the sync and resolution flows.
 */
class MockCli {
  readonly calls: string[][] = [];
  private readonly script: ((args: string[]) => { stdout: string; stderr: string }) | undefined;
  private queue: { stdout: string; stderr: string }[] = [];
  private failure: Error | undefined;

  constructor(script?: (args: string[]) => { stdout: string; stderr: string }) {
    this.script = script;
  }

  /** Queues responses returned in order for each non-scripted invocation. */
  respond(...responses: { stdout: string; stderr: string }[]): MockCli {
    this.queue.push(...responses);
    return this;
  }

  /** Makes every invocation fail with `error`. */
  failWith(error: Error): MockCli {
    this.failure = error;
    return this;
  }

  execFile: ExecFileFn = async (_file: string, args: string[]) => {
    this.calls.push(args);
    if (this.failure) {
      throw this.failure;
    }
    if (this.script) {
      return this.script(args);
    }
    // Only sync invocations consume the queued report; map-uid and any other
    // command succeed silently.
    if (args[1] === 'sync-uids') {
      return this.queue.shift() ?? { stdout: '', stderr: '' };
    }
    return { stdout: '', stderr: '' };
  };
}

/** A UI that always picks the first candidate and confirms; records its calls. */
function acceptingUi(overrides?: Partial<SyncResolutionUi>) {
  const picked: SyncCandidate[][] = [];
  const confirmed: string[] = [];
  const ui: SyncResolutionUi = {
    async pickCandidate(candidates: SyncCandidate[]) {
      picked.push(candidates);
      return candidates[0];
    },
    async confirmMapping(mapping: string) {
      confirmed.push(mapping);
      return true;
    },
    ...overrides,
  };
  return { ui, picked, confirmed };
}

const CLEAN_REPORT = 'preserved: 1\n'
  + '  main.x (uid 1)\n'
  + 'assigned: 0\n'
  + 'removed: 0\n'
  + 'rename candidates: 0\n'
  + 'swap candidates: 0\n';

const RENAME_CANDIDATE_REPORT = 'preserved: 0\n'
  + 'assigned: 1\n'
  + '  main.y (uid 2)\n'
  + 'removed: 1\n'
  + '  main.x (uid 1)\n'
  + 'rename candidates: 1\n'
  + '  main.x -> main.y\n'
  + 'swap candidates: 0\n';

const SWAP_CANDIDATE_REPORT = 'preserved: 0\n'
  + 'assigned: 2\n'
  + '  main.c (uid 3)\n'
  + '  main.d (uid 4)\n'
  + 'removed: 2\n'
  + '  main.a (uid 1)\n'
  + '  main.b (uid 2)\n'
  + 'rename candidates: 0\n'
  + 'swap candidates: 1\n'
  + '  (main.a, main.b) -> (main.c, main.d)\n';

const CLEAN_PARSED: SyncReport = {
  preserved: [{ key: { scope: 'main', name: 'x' }, uid: 1 }],
  assigned: [],
  removed: [],
  renameCandidates: [],
  swapCandidates: [],
};

function key(scope: string, name: string): SidecarKey {
  return { scope, name };
}

/** Awaits a promise expected to reject, returning the rejection reason. */
async function rejectionOf(promise: Promise<unknown>): Promise<unknown> {
  try {
    await promise;
  }
  catch (err) {
    return err;
  }
  throw new Error('expected the promise to reject');
}

suite('sync argument builders', () => {
  test('syncUidsArgs_then_refactor_sync_uids_with_project', () => {
    assert.deepStrictEqual(syncUidsArgs('proj'), ['refactor', 'sync-uids', 'proj']);
  });

  test('mapUidArgs_then_refactor_map_uid_with_keys_in_order', () => {
    assert.deepStrictEqual(
      mapUidArgs('proj', key('main', 'x'), key('global', 'y')),
      ['refactor', 'map-uid', 'proj', 'main', 'x', 'global', 'y'],
    );
  });
});

suite('parseSyncReport', () => {
  test('parseSyncReport_when_clean_report_then_every_section', () => {
    assert.deepStrictEqual(parseSyncReport(CLEAN_REPORT), CLEAN_PARSED);
  });

  test('parseSyncReport_when_rename_candidate_then_old_and_new_keys', () => {
    const report = parseSyncReport(RENAME_CANDIDATE_REPORT);

    assert.deepStrictEqual(report.assigned, [{ key: key('main', 'y'), uid: 2 }]);
    assert.deepStrictEqual(report.removed, [{ key: key('main', 'x'), uid: 1 }]);
    assert.deepStrictEqual(report.renameCandidates, [{ old: key('main', 'x'), new: key('main', 'y') }]);
    assert.deepStrictEqual(report.swapCandidates, []);
  });

  test('parseSyncReport_when_swap_candidate_then_both_pairs', () => {
    const report = parseSyncReport(SWAP_CANDIDATE_REPORT);

    assert.deepStrictEqual(report.swapCandidates, [{
      removed: [key('main', 'a'), key('main', 'b')],
      added: [key('main', 'c'), key('main', 'd')],
    }]);
    assert.deepStrictEqual(report.renameCandidates, []);
  });

  test('parseSyncReport_when_crlf_line_endings_then_parses', () => {
    const report = parseSyncReport(CLEAN_REPORT.replace(/\n/g, '\r\n'));

    assert.deepStrictEqual(report, CLEAN_PARSED);
  });

  test('parseSyncReport_when_entry_without_section_then_throws', () => {
    assert.throws(() => parseSyncReport('  main.x (uid 1)\n'), SyncUidsError);
  });

  test('parseSyncReport_when_line_matches_wrong_section_then_throws', () => {
    const text = 'preserved: 1\n  main.x -> main.y\n';

    assert.throws(() => parseSyncReport(text), SyncUidsError);
  });

  test('parseSyncReport_when_count_mismatch_then_throws', () => {
    const text = 'preserved: 2\n  main.x (uid 1)\nassigned: 0\nremoved: 0\n'
      + 'rename candidates: 0\nswap candidates: 0\n';

    assert.throws(() => parseSyncReport(text), /header promises 2/);
  });

  test('parseSyncReport_when_missing_section_then_throws', () => {
    assert.throws(() => parseSyncReport('preserved: 0\nassigned: 0\n'), /missing the removed section/);
  });

  test('parseSyncReport_when_key_without_dot_then_throws', () => {
    const text = 'preserved: 1\n  mainx (uid 1)\nassigned: 0\nremoved: 0\n'
      + 'rename candidates: 0\nswap candidates: 0\n';

    assert.throws(() => parseSyncReport(text), SyncUidsError);
  });
});

suite('sync report helpers', () => {
  test('candidatesOf_when_rename_and_swap_then_in_report_order', () => {
    const report = parseSyncReport(SWAP_CANDIDATE_REPORT);

    assert.deepStrictEqual(candidatesOf(report), [{
      kind: 'swap',
      removed: [key('main', 'a'), key('main', 'b')],
      added: [key('main', 'c'), key('main', 'd')],
    }]);
  });

  test('describeMapping_when_rename_then_arrow_form', () => {
    const candidate: SyncCandidate = { kind: 'rename', old: key('main', 'x'), new: key('main', 'y') };

    assert.strictEqual(describeMapping(candidate), 'main.x -> main.y');
  });

  test('describeMapping_when_swap_then_parenthesized_pairs', () => {
    const candidate: SyncCandidate = {
      kind: 'swap',
      removed: [key('main', 'a'), key('main', 'b')],
      added: [key('main', 'c'), key('main', 'd')],
    };

    assert.strictEqual(describeMapping(candidate), '(main.a, main.b) -> (main.c, main.d)');
  });

  test('formatKey_then_scope_dot_name', () => {
    assert.strictEqual(formatKey(key('global', 'g')), 'global.g');
  });

  test('formatSyncSummary_then_counts_in_order', () => {
    const report = parseSyncReport(RENAME_CANDIDATE_REPORT);

    assert.strictEqual(formatSyncSummary(report), '0 preserved, 1 assigned, 1 removed');
  });
});

suite('syncVariableIds', () => {
  test('syncVariableIds_when_clean_sync_then_one_invocation_and_no_picks', async () => {
    const cli = new MockCli().respond({ stdout: CLEAN_REPORT, stderr: '' });
    const { ui, picked } = acceptingUi({
      async pickCandidate() {
        throw new Error('pickCandidate must not be called without candidates');
      },
    });

    const report = await syncVariableIds('ironplcc', 'proj', cli.execFile, ui);

    assert.deepStrictEqual(cli.calls, [syncUidsArgs('proj')]);
    assert.deepStrictEqual(report, CLEAN_PARSED);
    assert.strictEqual(picked.length, 0);
  });

  test('syncVariableIds_when_rename_candidate_then_maps_and_resyncs', async () => {
    const cli = new MockCli().respond(
      { stdout: RENAME_CANDIDATE_REPORT, stderr: '' },
      { stdout: CLEAN_REPORT, stderr: '' },
    );
    const { ui, picked, confirmed } = acceptingUi();

    const report = await syncVariableIds('ironplcc', 'proj', cli.execFile, ui);

    assert.deepStrictEqual(cli.calls, [
      syncUidsArgs('proj'),
      mapUidArgs('proj', key('main', 'x'), key('main', 'y')),
      syncUidsArgs('proj'),
    ]);
    assert.strictEqual(picked.length, 1);
    assert.deepStrictEqual(picked[0], [{
      kind: 'rename',
      old: key('main', 'x'),
      new: key('main', 'y'),
    }]);
    assert.deepStrictEqual(confirmed, ['main.x -> main.y']);
    assert.deepStrictEqual(report, CLEAN_PARSED);
  });

  test('syncVariableIds_when_swap_candidate_then_maps_both_pairs_and_resyncs', async () => {
    const cli = new MockCli().respond(
      { stdout: SWAP_CANDIDATE_REPORT, stderr: '' },
      { stdout: CLEAN_REPORT, stderr: '' },
    );
    const { ui, confirmed } = acceptingUi();

    await syncVariableIds('ironplcc', 'proj', cli.execFile, ui);

    assert.deepStrictEqual(cli.calls, [
      syncUidsArgs('proj'),
      mapUidArgs('proj', key('main', 'a'), key('main', 'c')),
      mapUidArgs('proj', key('main', 'b'), key('main', 'd')),
      syncUidsArgs('proj'),
    ]);
    assert.deepStrictEqual(confirmed, ['(main.a, main.b) -> (main.c, main.d)']);
  });

  test('syncVariableIds_when_user_skips_pick_then_no_mapping_and_first_report_returned', async () => {
    const cli = new MockCli().respond({ stdout: RENAME_CANDIDATE_REPORT, stderr: '' });
    const { ui, confirmed } = acceptingUi({
      async pickCandidate() {
        return undefined;
      },
    });

    const report = await syncVariableIds('ironplcc', 'proj', cli.execFile, ui);

    assert.deepStrictEqual(cli.calls, [syncUidsArgs('proj')]);
    assert.strictEqual(confirmed.length, 0);
    assert.deepStrictEqual(report.renameCandidates, [{ old: key('main', 'x'), new: key('main', 'y') }]);
  });

  test('syncVariableIds_when_user_cancels_mapping_then_no_mapping_and_first_report_returned', async () => {
    const cli = new MockCli().respond({ stdout: RENAME_CANDIDATE_REPORT, stderr: '' });
    const { ui } = acceptingUi({
      async confirmMapping() {
        return false;
      },
    });

    const report = await syncVariableIds('ironplcc', 'proj', cli.execFile, ui);

    assert.deepStrictEqual(cli.calls, [syncUidsArgs('proj')]);
    assert.strictEqual(report.renameCandidates.length, 1);
  });

  test('syncVariableIds_when_sync_fails_then_rejects_with_sync_error', async () => {
    const cli = new MockCli().failWith(new Error('main.st:2: undeclared identifier'));
    const { ui } = acceptingUi();

    const err = await rejectionOf(syncVariableIds('ironplcc', 'proj', cli.execFile, ui));

    assert.ok(err instanceof SyncUidsError);
    assert.match(err.message, /undeclared identifier/);
    assert.deepStrictEqual(cli.calls, [syncUidsArgs('proj')]);
  });

  test('syncVariableIds_when_map_uid_fails_then_rejects_with_map_error_and_stops', async () => {
    const cli = new MockCli((args) => {
      if (args[1] === 'map-uid') {
        throw new Error('the old key has no recorded stable variable ID');
      }
      return { stdout: RENAME_CANDIDATE_REPORT, stderr: '' };
    });
    const { ui } = acceptingUi();

    const err = await rejectionOf(syncVariableIds('ironplcc', 'proj', cli.execFile, ui));

    assert.ok(err instanceof MapUidError);
    assert.match(err.message, /no recorded stable variable ID/);
    assert.deepStrictEqual(cli.calls, [syncUidsArgs('proj'), mapUidArgs('proj', key('main', 'x'), key('main', 'y'))]);
  });

  test('syncVariableIds_when_resync_reports_another_candidate_then_offers_again', async () => {
    // Models a re-sync that surfaces a further candidate after the first
    // mapping; the flow must offer it and keep resolving until the report
    // is clean.
    const second = 'preserved: 1\n  main.y (uid 1)\n'
      + 'assigned: 0\nremoved: 1\n  main.b (uid 2)\n'
      + 'rename candidates: 1\n  main.b -> main.d\nswap candidates: 0\n';
    const cli = new MockCli().respond(
      { stdout: RENAME_CANDIDATE_REPORT, stderr: '' },
      { stdout: second, stderr: '' },
      { stdout: CLEAN_REPORT, stderr: '' },
    );
    const { ui, picked } = acceptingUi();

    const report = await syncVariableIds('ironplcc', 'proj', cli.execFile, ui);

    assert.strictEqual(picked.length, 2);
    assert.deepStrictEqual(cli.calls, [
      syncUidsArgs('proj'),
      mapUidArgs('proj', key('main', 'x'), key('main', 'y')),
      syncUidsArgs('proj'),
      mapUidArgs('proj', key('main', 'b'), key('main', 'd')),
      syncUidsArgs('proj'),
    ]);
    assert.deepStrictEqual(report, CLEAN_PARSED);
  });

  test('syncVariableIds_when_report_garbage_then_rejects_with_sync_error', async () => {
    const cli = new MockCli().respond({ stdout: 'not a sync report', stderr: '' });
    const { ui } = acceptingUi();

    await assert.rejects(syncVariableIds('ironplcc', 'proj', cli.execFile, ui), SyncUidsError);
  });
});
