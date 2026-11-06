import * as assert from 'assert';
import * as fs from 'fs';
import * as path from 'path';

import { ConnectionProfile } from '../../connectionProfiles';
import { ConnectionFailure } from '../../connectionState';
import { IdentityInfo } from '../../hotEditSession';
import { Comparison, ComparisonModel } from '../../onlineComparisonLogic';
import {
  ATTEMPTS,
  Attempt,
  attemptOfFailure,
  compareOnConnect,
  ComparisonEnv,
  comparisonAnchor,
  comparisonArgs,
  comparisonSources,
  identityJson,
  NO_APPLICATION_CONTAINER,
  runComparison,
} from '../../onlineComparisonRun';
import { SUPPORTED_SESSION_PROTOCOL } from '../../sessionConstants';
import { ProblemCode } from '../../vscodeCommon';

const FIXTURES = path.join(__dirname, '..', '..', '..', 'src', 'test', 'fixtures', 'online-comparison');

function fixtureText(name: string): string {
  return fs.readFileSync(path.join(FIXTURES, `${name}.json`), 'utf-8');
}

const FOLDERS = [{ name: 'plant', path: path.join('work', 'plant') }];
const TCP: ConnectionProfile = { name: 'Press', transport: 'tcp', address: '10.0.0.5', port: 49152 };
const STDIO_CONTAINER: ConnectionProfile = { name: 'Local', transport: 'stdio', program: path.join('work', 'plant', 'app.iplc') };
const STDIO_SOURCE: ConnectionProfile = { name: 'Local', transport: 'stdio', program: path.join('work', 'plant', 'main.st') };
const STORAGE = path.join('storage', 'ironplc');

const WIRE = {
  response: 'identity',
  protocol: SUPPORTED_SESSION_PROTOCOL,
  device: { deviceId: '0123456789abcdef0123456789abcdef', name: 'press', model: 'm', modification: 'v', runtimeVersion: '0.247.0' },
  application: { mode: 'normal', active: 1, normal: 1, candidate: null, application: 1, migration: false, rounds: 0 },
  persistedContentHash: 'ab'.repeat(32),
  containerFormat: 9,
};

const IDENTITY: IdentityInfo = {
  protocol: SUPPORTED_SESSION_PROTOCOL,
  device: { deviceId: '0123456789abcdef0123456789abcdef', name: 'press', model: 'm', modification: 'v', runtimeVersion: '0.247.0' },
  application: { mode: 'normal', active: 1, normal: 1, candidate: null, application: 1, migration: false, rounds: 0 },
  wire: WIRE,
};

/** One call of the fake compiler. */
interface Call {
  args: string[];
  input: string | undefined;
}

interface Harness {
  env: ComparisonEnv;
  calls: Call[];
  said: string[];
}

/** An environment whose compiler answers with the fixture `answer(attempt)` and records the calls. */
function harness(answer: (attempt: string) => string | Error, overrides?: Partial<ComparisonEnv>): Harness {
  const calls: Call[] = [];
  const said: string[] = [];
  const env: ComparisonEnv = {
    run: async (args, input) => {
      calls.push({ args, input });
      const reply = answer(args[args.indexOf('--attempt') + 1]);
      if (reply instanceof Error) {
        throw reply;
      }
      return { stdout: reply, stderr: '' };
    },
    folders: FOLDERS,
    sourceExtensions: ['.st'],
    options: () => ['--disable-warning', 'P4001'],
    storageDir: STORAGE,
    say: line => said.push(line),
    ...overrides,
  };
  return { env, calls, said };
}

/** The view that Connect shows: what it was told and whether a dialog opened. */
function view(): { current: Comparison[]; dialogs: ComparisonModel[]; ui: Parameters<typeof compareOnConnect>[2] } {
  const current: Comparison[] = [];
  const dialogs: ComparisonModel[] = [];
  return {
    current,
    dialogs,
    ui: { current: comparison => current.push(comparison), dialog: model => dialogs.push(model) },
  };
}

suite('comparisonArgs', () => {
  test('comparisonArgs_when_attempt_ok_then_the_identity_is_read_from_standard_input', () => {
    assert.deepStrictEqual(
      comparisonArgs('plant.iplc', ['plant'], ['--disable-warning', 'P4001'], 'ok'),
      ['online-compare', '--disable-warning', 'P4001', 'plant', '-o', 'plant.iplc', '--attempt', 'ok', '--identity', '-'],
    );
  });

  test('comparisonArgs_when_attempt_did_not_reach_the_controller_then_no_identity', () => {
    for (const attempt of ATTEMPTS.filter(candidate => candidate !== 'ok')) {
      const args = comparisonArgs('plant.iplc', ['plant'], [], attempt);

      assert.deepStrictEqual(args, ['online-compare', 'plant', '-o', 'plant.iplc', '--attempt', attempt]);
    }
  });

  test('comparisonArgs_when_no_sources_then_only_the_anchor_names_the_project', () => {
    assert.deepStrictEqual(comparisonArgs('x.iplc', [], [], 'busy'), ['online-compare', '-o', 'x.iplc', '--attempt', 'busy']);
  });
});

suite('attemptOfFailure', () => {
  const failure = (overrides: Partial<ConnectionFailure>): ConnectionFailure => ({
    code: ProblemCode.ConnectFailed,
    vCode: null,
    message: 'x',
    ...overrides,
  });

  test('attemptOfFailure_when_the_device_refused_with_v6014_then_busy', () => {
    assert.strictEqual(attemptOfFailure(failure({ vCode: 'V6014' }), false), 'busy');
  });

  test('attemptOfFailure_when_the_device_refused_with_another_code_then_unreachable', () => {
    assert.strictEqual(attemptOfFailure(failure({ vCode: 'V4007' }), false), 'unreachable');
  });

  test('attemptOfFailure_when_nothing_opened_then_unreachable', () => {
    assert.strictEqual(attemptOfFailure(failure({}), false), 'unreachable');
    assert.strictEqual(attemptOfFailure(undefined, false), 'unreachable');
  });

  test('attemptOfFailure_when_an_open_connection_ended_then_connection_lost', () => {
    assert.strictEqual(attemptOfFailure(failure({ code: ProblemCode.ConnectionLost }), false), 'connection-lost');
  });

  test('attemptOfFailure_when_the_search_marked_the_controller_protected_then_protected', () => {
    assert.strictEqual(attemptOfFailure(failure({}), true), 'protected');
  });

  test('attemptOfFailure_when_every_outcome_then_a_value_the_command_accepts', () => {
    const outcomes: Attempt[] = [
      attemptOfFailure(failure({ vCode: 'V6014' }), false),
      attemptOfFailure(failure({}), false),
      attemptOfFailure(failure({}), true),
      attemptOfFailure(failure({ code: ProblemCode.ConnectionLost }), false),
    ];
    assert.deepStrictEqual([...outcomes].sort(), ['busy', 'connection-lost', 'protected', 'unreachable']);
    for (const outcome of outcomes) {
      assert.ok(ATTEMPTS.includes(outcome));
    }
  });
});

suite('comparisonAnchor', () => {
  test('comparisonAnchor_when_tcp_then_the_container_of_the_first_folder', () => {
    assert.strictEqual(comparisonAnchor(TCP, FOLDERS, ['.st'], STORAGE), path.join('work', 'plant', 'plant.iplc'));
  });

  test('comparisonAnchor_when_stdio_container_then_that_container', () => {
    assert.strictEqual(comparisonAnchor(STDIO_CONTAINER, FOLDERS, ['.st'], STORAGE), path.resolve(path.join('work', 'plant', 'app.iplc')));
  });

  test('comparisonAnchor_when_stdio_source_then_a_container_in_the_storage_directory_that_has_no_records', () => {
    assert.strictEqual(comparisonAnchor(STDIO_SOURCE, FOLDERS, ['.st'], STORAGE), path.join(STORAGE, NO_APPLICATION_CONTAINER));
  });

  test('comparisonAnchor_when_no_folder_is_open_then_a_container_in_the_storage_directory', () => {
    assert.strictEqual(comparisonAnchor(TCP, [], ['.st'], STORAGE), path.join(STORAGE, NO_APPLICATION_CONTAINER));
  });
});

suite('comparisonSources', () => {
  test('comparisonSources_when_folders_then_the_first_one', () => {
    assert.deepStrictEqual(comparisonSources([{ name: 'a', path: 'A' }, { name: 'b', path: 'B' }]), ['A']);
  });

  test('comparisonSources_when_no_folder_then_none', () => {
    assert.deepStrictEqual(comparisonSources([]), []);
  });
});

suite('identityJson', () => {
  test('identityJson_when_the_answer_was_kept_then_it_is_what_the_device_sent', () => {
    assert.deepStrictEqual(JSON.parse(identityJson(IDENTITY)), WIRE);
  });

  test('identityJson_when_the_answer_was_not_kept_then_the_identity_as_read', () => {
    const { wire: _wire, ...withoutWire } = IDENTITY;

    assert.deepStrictEqual(JSON.parse(identityJson(withoutWire)), withoutWire);
  });
});

suite('runComparison', () => {
  test('runComparison_when_session_opened_then_the_compiler_gets_the_identity_on_standard_input', async () => {
    const { env, calls } = harness(() => fixtureText('up-to-date'));

    const comparison = await runComparison(env, { profile: TCP, attempt: 'ok', identity: IDENTITY });

    assert.strictEqual(comparison?.row, 'upToDate');
    assert.strictEqual(calls.length, 1);
    assert.deepStrictEqual(calls[0].args, [
      'online-compare', '--disable-warning', 'P4001', path.join('work', 'plant'),
      '-o', path.join('work', 'plant', 'plant.iplc'), '--attempt', 'ok', '--identity', '-',
    ]);
    assert.deepStrictEqual(JSON.parse(calls[0].input!), WIRE);
  });

  test('runComparison_when_a_stdio_session_runs_a_source_then_a_temporary_anchor_finds_no_records', async () => {
    const { env, calls } = harness(() => fixtureText('other-application-no-record'));

    await runComparison(env, { profile: STDIO_SOURCE, attempt: 'ok', identity: IDENTITY });

    const output = calls[0].args[calls[0].args.indexOf('-o') + 1];
    assert.strictEqual(output, path.join(STORAGE, NO_APPLICATION_CONTAINER));
  });

  test('runComparison_when_connection_failed_then_the_attempt_is_passed_and_nothing_is_written_to_standard_input', async () => {
    const { env, calls } = harness(() => fixtureText('busy'));

    const comparison = await runComparison(env, { profile: TCP, attempt: 'busy' });

    assert.strictEqual(comparison?.row, 'busy');
    assert.ok(calls[0].args.join(' ').endsWith('--attempt busy'));
    assert.strictEqual(calls[0].input, undefined);
  });

  test('runComparison_when_the_compiler_is_not_installed_then_none_and_the_reason_is_said', async () => {
    const { env, said } = harness(() => '', { run: undefined });

    assert.strictEqual(await runComparison(env, { profile: TCP, attempt: 'ok', identity: IDENTITY }), undefined);
    assert.ok(said[0].includes('not installed'));
  });

  test('runComparison_when_attempt_ok_without_an_identity_then_none', async () => {
    const { env, calls, said } = harness(() => fixtureText('up-to-date'));

    assert.strictEqual(await runComparison(env, { profile: TCP, attempt: 'ok' }), undefined);
    assert.strictEqual(calls.length, 0);
    assert.ok(said[0].includes('no identity'));
  });

  test('runComparison_when_the_compiler_fails_then_none_and_the_failure_is_said', async () => {
    const { env, said } = harness(() => new Error('the identity is not JSON'));

    assert.strictEqual(await runComparison(env, { profile: TCP, attempt: 'ok', identity: IDENTITY }), undefined);
    assert.ok(said[0].includes('the identity is not JSON'));
  });

  test('runComparison_when_the_compiler_prints_what_is_not_a_comparison_then_none_and_the_reason_is_said', async () => {
    const { env, said } = harness(() => 'Build started');

    assert.strictEqual(await runComparison(env, { profile: TCP, attempt: 'ok', identity: IDENTITY }), undefined);
    assert.ok(said[0].includes('not JSON'));
  });
});

suite('compareOnConnect', () => {
  test('compareOnConnect_when_the_controller_runs_the_last_build_then_online_at_once_with_no_dialog', async () => {
    const { env } = harness(() => fixtureText('up-to-date'));
    const shown = view();

    const comparison = await compareOnConnect(env, { profile: TCP, attempt: 'ok', identity: IDENTITY }, shown.ui, true);

    assert.strictEqual(comparison?.atOnce, true);
    assert.deepStrictEqual(shown.current.map(entry => entry.row), ['upToDate']);
    assert.deepStrictEqual(shown.dialogs, []);
  });

  test('compareOnConnect_when_row_9_then_the_dialog_enables_download_and_disables_online_change_with_its_reason', async () => {
    const { env } = harness(() => fixtureText('other-application-no-record'));
    const shown = view();

    await compareOnConnect(env, { profile: TCP, attempt: 'ok', identity: IDENTITY }, shown.ui, true);

    assert.strictEqual(shown.dialogs.length, 1);
    const items = shown.dialogs[0].items;
    const download = items.find(entry => entry.action === 'download');
    const online = items.find(entry => entry.action === 'onlineChange');
    assert.strictEqual(download?.enabled, true);
    assert.deepStrictEqual(download?.run, { kind: 'command', command: 'ironplc.download' });
    assert.strictEqual(online?.enabled, false);
    assert.strictEqual(online?.description, 'otherApplication');
    assert.ok(online?.detail.includes('Download replaces it'));
    assert.strictEqual(shown.dialogs[0].title, 'The controller holds an application that this workstation did not send.');
  });

  test('compareOnConnect_when_the_comparison_is_only_renewed_then_the_status_follows_and_no_dialog_opens', async () => {
    const { env } = harness(() => fixtureText('other-application'));
    const shown = view();

    await compareOnConnect(env, { profile: TCP, attempt: 'ok', identity: IDENTITY }, shown.ui, false);

    assert.deepStrictEqual(shown.current.map(entry => entry.row), ['otherApplication']);
    assert.deepStrictEqual(shown.dialogs, []);
  });

  test('compareOnConnect_when_a_connection_failed_then_the_same_dialog_explains_it', async () => {
    const attempts: Array<[Attempt, string, string]> = [
      ['unreachable', 'unreachable', 'unreachable'],
      ['busy', 'busy', 'busy'],
      ['protected', 'protected', 'protected'],
      ['connection-lost', 'connection-lost', 'connectionLost'],
    ];
    for (const [attempt, name, row] of attempts) {
      const { env, calls } = harness(() => fixtureText(name));
      const shown = view();

      const comparison = await compareOnConnect(env, { profile: TCP, attempt }, shown.ui, true);

      assert.strictEqual(comparison?.row, row, attempt);
      assert.ok(calls[0].args.includes(attempt), attempt);
      assert.strictEqual(shown.dialogs.length, 1, attempt);
      assert.strictEqual(shown.dialogs[0].items.find(entry => entry.action === 'monitor')?.enabled, false, attempt);
    }
  });

  test('compareOnConnect_when_the_comparison_cannot_be_made_then_the_view_is_not_touched', async () => {
    const { env } = harness(() => new Error('boom'));
    const shown = view();

    const comparison = await compareOnConnect(env, { profile: TCP, attempt: 'ok', identity: IDENTITY }, shown.ui, true);

    assert.strictEqual(comparison, undefined);
    assert.deepStrictEqual(shown.current, []);
    assert.deepStrictEqual(shown.dialogs, []);
  });

  test('compareOnConnect_when_the_controller_is_not_the_one_last_chosen_then_it_is_information_and_everything_is_offered', async () => {
    const { env, said } = harness(() => fixtureText('other-controller'));
    const shown = view();

    const comparison = await compareOnConnect(env, { profile: TCP, attempt: 'ok', identity: IDENTITY }, shown.ui, true);

    // Connect never refuses: the row is up to date and goes online at once, with the information beside it.
    assert.strictEqual(comparison?.atOnce, true);
    assert.deepStrictEqual(comparison?.information.map(note => note.kind), ['otherController']);
    assert.deepStrictEqual(shown.dialogs, []);
    assert.deepStrictEqual(said, comparison?.information.map(note => note.text));
  });
});
