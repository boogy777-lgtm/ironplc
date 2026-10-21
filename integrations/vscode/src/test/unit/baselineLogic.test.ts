import * as assert from 'assert';
import * as fs from 'fs';
import * as path from 'path';

import {
  checkBaseline,
  connectionAnchor,
  DownloadRecord,
  DownloadToRecord,
  FolderRef,
  parseDownloadRecord,
  planDownload,
  readBaseline,
  recordDownloadArgs,
  recordDownloadPathArgs,
  RunCompiler,
  writeDownloadRecord,
} from '../../baselineLogic';
import { ConnectionProfile } from '../../connectionProfiles';
import { HotEditStatus } from '../../hotEditSession';

const HASH_A = 'aa'.repeat(32);
const HASH_B = 'bb'.repeat(32);
const SOURCE_EXTENSIONS = ['.st', '.iec'];

function status(overrides?: Partial<HotEditStatus>): HotEditStatus {
  return {
    mode: 'normal',
    active: 3,
    normal: 2,
    candidate: null,
    application: 3,
    migration: false,
    rounds: 10,
    contentHash: HASH_A,
    layoutHash: 'cc'.repeat(32),
    ...overrides,
  };
}

function withoutHash(): HotEditStatus {
  const { contentHash: _contentHash, layoutHash: _layoutHash, ...rest } = status();
  return rest;
}

/** The text of a download record the compiler wrote for a container with `contentHash`. */
function recordText(contentHash: string, connection = 'My PLC', overrides?: Record<string, unknown>): string {
  return JSON.stringify({
    version: 1,
    connection,
    application: 'app',
    container: { contentHash, layoutHash: 'cc'.repeat(32), debugHash: 'dd'.repeat(32), size: 100 },
    device: { active: 3, normal: 2, application: 3 },
    time: '2026-10-20T12:00:00Z',
    ...overrides,
  });
}

const RECORD: DownloadRecord = { connection: 'My PLC', contentHash: HASH_A };

suite('parseDownloadRecord', () => {
  test('parseDownloadRecord_when_record_then_connection_and_content_hash', () => {
    assert.deepStrictEqual(parseDownloadRecord(recordText(HASH_A), 'My PLC'), RECORD);
  });

  test('parseDownloadRecord_when_not_json_then_no_record', () => {
    assert.strictEqual(parseDownloadRecord('{ not json', 'My PLC'), undefined);
    assert.strictEqual(parseDownloadRecord('[1]', 'My PLC'), undefined);
    assert.strictEqual(parseDownloadRecord('null', 'My PLC'), undefined);
  });

  test('parseDownloadRecord_when_other_version_then_no_record', () => {
    assert.strictEqual(parseDownloadRecord(recordText(HASH_A, 'My PLC', { version: 2 }), 'My PLC'), undefined);
  });

  test('parseDownloadRecord_when_record_of_another_connection_then_no_record', () => {
    // Two names that differ only in case share a file on a case-insensitive file system.
    assert.strictEqual(parseDownloadRecord(recordText(HASH_A, 'my plc'), 'My PLC'), undefined);
  });

  test('parseDownloadRecord_when_container_or_hash_malformed_then_no_record', () => {
    assert.strictEqual(parseDownloadRecord(recordText(HASH_A, 'My PLC', { container: null }), 'My PLC'), undefined);
    assert.strictEqual(parseDownloadRecord(recordText('abc'), 'My PLC'), undefined);
    assert.strictEqual(parseDownloadRecord(recordText(HASH_A.toUpperCase()), 'My PLC'), undefined);
  });

  test('parseDownloadRecord_when_unknown_extra_keys_then_still_read', () => {
    assert.deepStrictEqual(parseDownloadRecord(recordText(HASH_A, 'My PLC', { later: true }), 'My PLC'), RECORD);
  });
});

suite('checkBaseline', () => {
  test('checkBaseline_when_no_record_then_none', () => {
    assert.strictEqual(checkBaseline(undefined, status()), 'none');
  });

  test('checkBaseline_when_content_hash_equal_then_match', () => {
    assert.strictEqual(checkBaseline(RECORD, status()), 'match');
  });

  test('checkBaseline_when_device_rebooted_and_counters_restarted_then_still_match', () => {
    // A reboot restarts the session generations; the content hash is the same.
    assert.strictEqual(checkBaseline(RECORD, status({ active: 1, normal: 1, application: 1 })), 'match');
  });

  test('checkBaseline_when_counters_differ_but_content_equal_then_match', () => {
    assert.strictEqual(checkBaseline(RECORD, status({ active: 9, application: 9 })), 'match');
  });

  test('checkBaseline_when_other_container_then_mismatch', () => {
    assert.strictEqual(checkBaseline(RECORD, status({ contentHash: HASH_B })), 'mismatch');
  });

  test('checkBaseline_when_device_reports_no_hash_and_record_then_mismatch', () => {
    // An older device: what it runs cannot be known, so it fails closed.
    assert.strictEqual(checkBaseline(RECORD, withoutHash()), 'mismatch');
  });

  test('checkBaseline_when_device_reports_no_hash_and_no_record_then_none', () => {
    assert.strictEqual(checkBaseline(undefined, withoutHash()), 'none');
  });
});

const STDIO_CONTAINER: ConnectionProfile = { name: 'local', transport: 'stdio', program: path.join('plant', 'app.iplc') };
const STDIO_SOURCE: ConnectionProfile = { name: 'local', transport: 'stdio', program: path.join('plant', 'main.st') };
const TCP: ConnectionProfile = { name: 'My PLC', transport: 'tcp', address: '10.0.0.5' };
const FOLDERS: FolderRef[] = [
  { name: 'plant', path: path.join('work', 'plant') },
  { name: 'other', path: path.join('work', 'other') },
];

suite('connectionAnchor', () => {
  test('connectionAnchor_when_stdio_container_then_that_container', () => {
    assert.deepStrictEqual(
      connectionAnchor(STDIO_CONTAINER, FOLDERS, SOURCE_EXTENSIONS),
      { anchor: path.resolve(path.join('plant', 'app.iplc')) },
    );
  });

  test('connectionAnchor_when_stdio_source_then_no_anchor_and_reason', () => {
    const anchor = connectionAnchor(STDIO_SOURCE, FOLDERS, SOURCE_EXTENSIONS);

    assert.ok('reason' in anchor);
    assert.ok(anchor.reason.includes('compiles itself'));
  });

  test('connectionAnchor_when_tcp_then_container_of_first_folder', () => {
    assert.deepStrictEqual(
      connectionAnchor(TCP, FOLDERS, SOURCE_EXTENSIONS),
      { anchor: path.join('work', 'plant', 'plant.iplc') },
    );
  });

  test('connectionAnchor_when_tcp_and_no_folder_then_no_anchor', () => {
    assert.ok('reason' in connectionAnchor(TCP, [], SOURCE_EXTENSIONS));
  });
});

suite('planDownload', () => {
  const base = { folders: FOLDERS, sourceExtensions: SOURCE_EXTENSIONS, requireDeviceHash: false };

  test('planDownload_when_assemble_of_another_file_then_anchor_and_sent_container', () => {
    const plan = planDownload({ ...base, profile: TCP, sentContainer: path.join('tmp', 'main.iplc'), status: status() });

    assert.deepStrictEqual(plan, {
      download: {
        anchor: path.join('work', 'plant', 'plant.iplc'),
        container: path.resolve(path.join('tmp', 'main.iplc')),
        connection: 'My PLC',
        status: status(),
      },
    });
  });

  test('planDownload_when_session_start_then_the_application_container_is_the_sent_one', () => {
    const plan = planDownload({ ...base, profile: STDIO_CONTAINER, status: status(), requireDeviceHash: true });

    assert.ok('download' in plan);
    assert.strictEqual(plan.download.container, plan.download.anchor);
    assert.strictEqual(plan.download.connection, 'local');
  });

  test('planDownload_when_session_start_and_device_reports_no_hash_then_nothing_recorded', () => {
    const plan = planDownload({ ...base, profile: STDIO_CONTAINER, status: withoutHash(), requireDeviceHash: true });

    assert.ok('reason' in plan);
    assert.ok(plan.reason.includes('content hash'));
  });

  test('planDownload_when_assemble_and_device_reports_no_hash_then_still_recorded', () => {
    const plan = planDownload({ ...base, profile: TCP, sentContainer: 'x.iplc', status: withoutHash() });

    assert.ok('download' in plan);
  });

  test('planDownload_when_stdio_source_then_nothing_recorded_with_reason', () => {
    const plan = planDownload({ ...base, profile: STDIO_SOURCE, sentContainer: 'x.iplc', status: status() });

    assert.ok('reason' in plan);
  });
});

suite('record commands', () => {
  const download: DownloadToRecord = {
    anchor: path.join('work', 'plant', 'plant.iplc'),
    container: path.join('work', 'plant', 'plant.iplc'),
    connection: 'My PLC',
    status: status(),
  };

  test('recordDownloadArgs_when_container_is_the_anchor_then_no_container_flag', () => {
    assert.deepStrictEqual(recordDownloadArgs(download), [
      'record-download', download.anchor, '--connection', 'My PLC',
      '--device-content-hash', HASH_A,
      '--active', '3', '--normal', '2', '--application', '3',
    ]);
  });

  test('recordDownloadArgs_when_other_container_then_container_flag', () => {
    const args = recordDownloadArgs({ ...download, container: path.join('tmp', 'main.iplc') });

    assert.deepStrictEqual(args.slice(0, 6), [
      'record-download', download.anchor, '--connection', 'My PLC',
      '--container', path.join('tmp', 'main.iplc'),
    ]);
  });

  test('recordDownloadArgs_when_device_reports_no_hash_then_no_hash_flag', () => {
    const args = recordDownloadArgs({ ...download, status: withoutHash() });

    assert.ok(!args.includes('--device-content-hash'));
  });

  test('recordDownloadPathArgs_when_anchor_and_connection_then_path_command', () => {
    assert.deepStrictEqual(recordDownloadPathArgs('a.iplc', 'My PLC'), [
      'record-download-path', 'a.iplc', '--connection', 'My PLC',
    ]);
  });

  test('writeDownloadRecord_when_download_then_runs_the_writer_once', async () => {
    const calls: string[][] = [];
    const run: RunCompiler = async (args) => {
      calls.push(args);
      return { stdout: '', stderr: '' };
    };

    await writeDownloadRecord(run, download);

    assert.deepStrictEqual(calls, [recordDownloadArgs(download)]);
  });

  test('writeDownloadRecord_when_writer_fails_then_rejects', async () => {
    const run: RunCompiler = async () => {
      throw new Error('the device did not receive this container');
    };

    await assert.rejects(writeDownloadRecord(run, download), /did not receive/);
  });
});

suite('readBaseline', () => {
  /** A compiler that says the record is `file`, recording what it was asked. */
  function compilerSaying(file: string, asked: string[][] = []): RunCompiler {
    return async (args) => {
      asked.push(args);
      return { stdout: `${file}\r\n`, stderr: '' };
    };
  }

  const anchor = path.join('work', 'plant', 'plant.iplc');

  test('readBaseline_when_record_has_the_devices_hash_then_match_and_path_comes_from_the_compiler', async () => {
    const asked: string[][] = [];
    const read: string[] = [];

    const state = await readBaseline(
      compilerSaying('F:\\work\\plant\\the-file-the-compiler-named', asked),
      async (file) => {
        read.push(file);
        return recordText(HASH_A);
      },
      anchor,
      'My PLC',
      status(),
    );

    assert.strictEqual(state, 'match');
    assert.deepStrictEqual(asked, [recordDownloadPathArgs(anchor, 'My PLC')]);
    assert.deepStrictEqual(read, ['F:\\work\\plant\\the-file-the-compiler-named']);
  });

  test('readBaseline_when_device_rebooted_then_match', async () => {
    const state = await readBaseline(
      compilerSaying('r'),
      async () => recordText(HASH_A),
      anchor,
      'My PLC',
      status({ active: 1, normal: 1, application: 1, rounds: 0 }),
    );

    assert.strictEqual(state, 'match');
  });

  test('readBaseline_when_device_runs_another_container_then_mismatch', async () => {
    const state = await readBaseline(
      compilerSaying('r'),
      async () => recordText(HASH_A),
      anchor,
      'My PLC',
      status({ contentHash: HASH_B }),
    );

    assert.strictEqual(state, 'mismatch');
  });

  test('readBaseline_when_no_record_file_then_none', async () => {
    const state = await readBaseline(compilerSaying('r'), async () => undefined, anchor, 'My PLC', status());

    assert.strictEqual(state, 'none');
  });

  test('readBaseline_when_record_malformed_then_none', async () => {
    const state = await readBaseline(compilerSaying('r'), async () => '{ broken', anchor, 'My PLC', status());

    assert.strictEqual(state, 'none');
  });

  test('readBaseline_when_compiler_names_no_file_then_rejects', async () => {
    await assert.rejects(
      readBaseline(async () => ({ stdout: '\n', stderr: '' }), async () => undefined, anchor, 'My PLC', status()),
      /did not say where/,
    );
  });

  test('readBaseline_when_compiler_fails_then_rejects', async () => {
    const run: RunCompiler = async () => {
      throw new Error('unknown subcommand');
    };

    await assert.rejects(readBaseline(run, async () => undefined, anchor, 'My PLC', status()), /unknown subcommand/);
  });

  test('readBaseline_when_file_cannot_be_read_then_rejects', async () => {
    await assert.rejects(
      readBaseline(compilerSaying('r'), async () => { throw new Error('EACCES'); }, anchor, 'My PLC', status()),
      /EACCES/,
    );
  });
});

/** The non-test sources of the extension, with their text. */
function extensionSources(): { file: string; text: string }[] {
  const root = path.join(__dirname, '..', '..', '..', 'src');
  const found: { file: string; text: string }[] = [];
  const visit = (dir: string): void => {
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
      const full = path.join(dir, entry.name);
      if (entry.isDirectory()) {
        if (entry.name !== 'test') {
          visit(full);
        }
      }
      else if (entry.name.endsWith('.ts')) {
        found.push({ file: path.relative(root, full), text: fs.readFileSync(full, 'utf-8') });
      }
    }
  };
  visit(root);
  return found;
}

/** The files of the extension whose text contains `needle`. */
function filesContaining(needle: string): string[] {
  return extensionSources().filter(source => source.text.includes(needle)).map(source => source.file);
}

suite('download record guards', () => {
  test('sources_when_scanned_then_the_workspace_state_baseline_is_not_read_or_written', () => {
    // The key is assembled so that this file does not contain it.
    const key = ['ironplc', 'baseline'].join('.');

    assert.deepStrictEqual(filesContaining(key), []);
    assert.deepStrictEqual(filesContaining('BASELINE_STATE_KEY'), []);
    assert.deepStrictEqual(filesContaining('captureVerifiedBaseline'), []);
    assert.deepStrictEqual(filesContaining('hashContainer'), []);
  });

  test('sources_when_scanned_then_the_writer_command_is_built_in_one_module', () => {
    assert.deepStrictEqual(filesContaining('\'record-download\''), ['baselineLogic.ts']);
  });

  test('sources_when_scanned_then_the_record_file_name_rule_is_not_derived_here', () => {
    // The suffix is assembled so that this file does not contain it.
    const suffix = ['download', 'iplcinfo'].join('.');

    assert.deepStrictEqual(filesContaining(suffix), []);
  });

  test('buildCommands_when_scanned_then_the_assemble_acknowledgement_has_one_handler', () => {
    const source = extensionSources().find(candidate => candidate.file === 'buildCommands.ts');

    assert.ok(source);
    // Defined once; the commit path and the trial's assemble call it; it is the
    // only place that reaches the connection's recorder.
    assert.strictEqual(source.text.split('function acknowledgeAssemble(').length - 1, 1);
    assert.strictEqual(source.text.split('await acknowledgeAssemble(').length - 1, 2);
    assert.strictEqual(source.text.split('connection.recordDownload(').length - 1, 1);
  });
});
