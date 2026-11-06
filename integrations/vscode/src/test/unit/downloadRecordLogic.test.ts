import * as assert from 'assert';
import * as fs from 'fs';
import * as path from 'path';

import {
  connectionAnchor,
  DownloadToRecord,
  FolderRef,
  planDownload,
  recordDownloadArgs,
  RunCompiler,
  writeDownloadRecord,
} from '../../downloadRecordLogic';
import { ConnectionProfile } from '../../connectionProfiles';
import { HotEditStatus } from '../../hotEditSession';

const HASH_A = 'aa'.repeat(32);
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

const DEVICE_ID = '0123456789abcdef0123456789abcdef';

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
  const base = {
    folders: FOLDERS,
    sourceExtensions: SOURCE_EXTENSIONS,
    requireDeviceHash: false,
    deviceId: DEVICE_ID as string | undefined,
  };

  test('planDownload_when_assemble_of_another_file_then_anchor_and_sent_container', () => {
    const plan = planDownload({ ...base, profile: TCP, sentContainer: path.join('tmp', 'main.iplc'), status: status() });

    assert.deepStrictEqual(plan, {
      download: {
        anchor: path.join('work', 'plant', 'plant.iplc'),
        container: path.resolve(path.join('tmp', 'main.iplc')),
        deviceId: DEVICE_ID,
        status: status(),
        keepContainer: false,
      },
    });
  });

  test('planDownload_when_keep_container_requested_then_the_plan_keeps_it', () => {
    const plan = planDownload({ ...base, profile: TCP, sentContainer: 'x.iplc', status: status(), keepContainer: true });

    assert.ok('download' in plan);
    assert.strictEqual(plan.download.keepContainer, true);
  });

  test('planDownload_when_session_start_then_the_application_container_is_the_sent_one', () => {
    const plan = planDownload({ ...base, profile: STDIO_CONTAINER, status: status(), requireDeviceHash: true });

    assert.ok('download' in plan);
    assert.strictEqual(plan.download.container, plan.download.anchor);
    assert.strictEqual(plan.download.deviceId, DEVICE_ID);
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

  test('planDownload_when_device_reports_no_id_then_nothing_recorded_with_reason', () => {
    const plan = planDownload({ ...base, profile: TCP, deviceId: undefined, status: status() });

    assert.ok('reason' in plan);
    assert.ok(plan.reason.includes('device id'));
  });

  test('planDownload_when_profile_is_renamed_then_the_plan_is_the_same', () => {
    const renamed: ConnectionProfile = { ...TCP, name: 'Another name' };

    assert.deepStrictEqual(
      planDownload({ ...base, profile: renamed, status: status() }),
      planDownload({ ...base, profile: TCP, status: status() }),
    );
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
    deviceId: DEVICE_ID,
    status: status(),
    keepContainer: false,
  };

  test('recordDownloadArgs_when_keep_container_then_the_flag_follows_the_container', () => {
    const args = recordDownloadArgs({ ...download, container: path.join('tmp', 'main.iplc'), keepContainer: true });

    assert.deepStrictEqual(args.slice(0, 7), [
      'record-download', download.anchor, '--device-id', DEVICE_ID,
      '--container', path.join('tmp', 'main.iplc'), '--keep-container',
    ]);
  });

  test('recordDownloadArgs_when_not_keep_container_then_no_keep_flag', () => {
    assert.ok(!recordDownloadArgs(download).includes('--keep-container'));
  });

  test('recordDownloadArgs_when_container_is_the_anchor_then_no_container_flag', () => {
    assert.deepStrictEqual(recordDownloadArgs(download), [
      'record-download', download.anchor, '--device-id', DEVICE_ID,
      '--device-content-hash', HASH_A,
      '--active', '3', '--normal', '2', '--application', '3',
    ]);
  });

  test('recordDownloadArgs_when_other_container_then_container_flag', () => {
    const args = recordDownloadArgs({ ...download, container: path.join('tmp', 'main.iplc') });

    assert.deepStrictEqual(args.slice(0, 6), [
      'record-download', download.anchor, '--device-id', DEVICE_ID,
      '--container', path.join('tmp', 'main.iplc'),
    ]);
  });

  test('recordDownloadArgs_when_device_reports_no_hash_then_no_hash_flag', () => {
    const args = recordDownloadArgs({ ...download, status: withoutHash() });

    assert.ok(!args.includes('--device-content-hash'));
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
    assert.deepStrictEqual(filesContaining('\'record-download\''), ['downloadRecordLogic.ts']);
  });

  test('sources_when_scanned_then_the_extension_compares_nothing_with_the_download_record', () => {
    // The go-online comparison is computed by the compiler (ADR-0089); the
    // extension reads its JSON. None of the removed client-side pieces is back.
    for (const removed of ['checkBaseline', 'readBaseline', 'parseDownloadRecord', 'BaselineState', 'StaleBaseline', 'recordDownloadPathArgs']) {
      assert.deepStrictEqual(filesContaining(removed), [], removed);
    }
  });

  test('sources_when_scanned_then_the_comparison_command_is_built_in_one_module', () => {
    assert.deepStrictEqual(filesContaining('\'online-compare\''), ['onlineComparisonRun.ts']);
  });

  test('sources_when_scanned_then_the_record_file_name_rule_is_not_derived_here', () => {
    // The suffix is assembled so that this file does not contain it.
    const suffix = ['download', 'iplcinfo'].join('.');

    assert.deepStrictEqual(filesContaining(suffix), []);
  });

  test('buildCommands_when_scanned_then_the_assemble_acknowledgement_has_one_handler', () => {
    const source = extensionSources().find(candidate => candidate.file === 'buildCommands.ts');

    assert.ok(source);
    // Defined once; the Download's assemble calls it and the dialog's assemble
    // reaches it through the build support; it is the only place that reaches
    // the connection's recorder.
    assert.strictEqual(source.text.split('function acknowledgeAssemble(').length - 1, 1);
    assert.strictEqual(source.text.split('await acknowledgeAssemble(').length - 1, 1);
    assert.strictEqual(source.text.split('connection.recordDownload(').length - 1, 1);
  });

  test('onlineEditCommands_when_scanned_then_it_compiles_and_records_through_the_build_support_only', () => {
    const source = extensionSources().find(candidate => candidate.file === 'onlineEditCommands.ts');

    assert.ok(source);
    // The compile and the recorder exist once, in buildCommands.ts; the dialog calls them.
    assert.strictEqual(source.text.split('support.compile(').length - 1, 1);
    assert.strictEqual(source.text.split('support.acknowledgeAssemble(').length - 1, 1);
    for (const own of ['recordDownload(', 'buildContainer(', 'runCompiler(', 'compileSourceToContainer(']) {
      assert.ok(!source.text.includes(own), `onlineEditCommands.ts calls ${own}`);
    }
  });

  test('buildCommands_when_scanned_then_only_the_download_keeps_the_container_and_registers_its_table_command', () => {
    const source = extensionSources().find(candidate => candidate.file === 'buildCommands.ts');

    assert.ok(source);
    // The dialog records without a copy; the Download, which replaced the
    // application, keeps the container that was sent beside the record.
    assert.strictEqual(source.text.split('acknowledgeAssemble(sent.path, status, true)').length - 1, 1);
    assert.strictEqual(source.text.split('await acknowledgeAssemble(').length - 1, 1);
    assert.ok(source.text.includes('registerCommand(DOWNLOAD_ACTIONS.download.commandId, download)'));
  });
});
