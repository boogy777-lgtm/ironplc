import * as assert from 'assert';
import * as fs from 'fs';
import * as path from 'path';

import {
  ACTION_HANDLERS,
  ACTION_KINDS,
  acceptGate,
  Comparison,
  comparisonModel,
  handlerOf,
  ModelItem,
  NO_COMPARISON_TEXT,
  parseComparison,
  replacesOtherApplication,
  SHOW_ONLINE_COMPARISON,
  statusSummary,
} from '../../onlineComparisonLogic';

/**
 * The fixtures are the output of the real `ironplcc online-compare` (see
 * `scripts/generate-comparison-fixtures.js`), one for each row of ADR-0089, so the
 * mirror is tested against what the compiler prints.
 */
const FIXTURES = path.join(__dirname, '..', '..', '..', 'src', 'test', 'fixtures', 'online-comparison');

function fixtureText(name: string): string {
  return fs.readFileSync(path.join(FIXTURES, `${name}.json`), 'utf-8');
}

function fixture(name: string): Comparison {
  return parseComparison(fixtureText(name));
}

function fixtureNames(): string[] {
  return fs.readdirSync(FIXTURES).filter(file => file.endsWith('.json')).map(file => file.slice(0, -'.json'.length)).sort();
}

function item(items: readonly ModelItem[], action: string): ModelItem {
  const found = items.find(candidate => candidate.action === action);
  assert.ok(found, `no item for the action ${action}`);
  return found;
}

/** What ADR-0089 says of each row: the headline, the actions that are enabled and why the others are not. */
interface RowExpectation {
  fixture: string;
  row: string;
  rowNumber: number;
  atOnce: boolean;
  enabled: string[];
  /** The action and the code of its refusal, for the disabled ones the row is about. */
  refused: Record<string, string>;
  information?: string[];
}

const ROWS: RowExpectation[] = [
  { fixture: 'unreachable', row: 'unreachable', rowNumber: 1, atOnce: false, enabled: ['search', 'chooseAnother', 'retry'], refused: { monitor: 'notConnected', download: 'notConnected', onlineChange: 'notConnected' } },
  { fixture: 'busy', row: 'busy', rowNumber: 2, atOnce: false, enabled: ['search', 'chooseAnother', 'retry'], refused: { monitor: 'notConnected' } },
  { fixture: 'protected', row: 'protected', rowNumber: 3, atOnce: false, enabled: ['search', 'chooseAnother'], refused: { retry: 'protected', monitor: 'notConnected' } },
  { fixture: 'no-application', row: 'noApplication', rowNumber: 4, atOnce: false, enabled: ['monitor', 'download', 'search', 'chooseAnother'], refused: { onlineChange: 'noApplicationToChange' } },
  { fixture: 'up-to-date', row: 'upToDate', rowNumber: 5, atOnce: true, enabled: ['monitor', 'onlineChange', 'download', 'search', 'chooseAnother'], refused: { retry: 'alreadyConnected', upload: 'notAvailable' } },
  { fixture: 'sources-changed', row: 'sourcesChanged', rowNumber: 6, atOnce: false, enabled: ['monitor', 'onlineChange', 'download', 'search', 'chooseAnother'], refused: {} },
  { fixture: 'previous-download', row: 'previousDownload', rowNumber: 7, atOnce: false, enabled: ['monitor', 'onlineChange', 'download', 'search', 'chooseAnother'], refused: {} },
  { fixture: 'previous-download-other-layout', row: 'previousDownloadOtherLayout', rowNumber: 8, atOnce: false, enabled: ['monitor', 'onlineChange', 'download', 'search', 'chooseAnother'], refused: {} },
  { fixture: 'other-application', row: 'otherApplication', rowNumber: 9, atOnce: false, enabled: ['monitor', 'download', 'search', 'chooseAnother'], refused: { onlineChange: 'otherApplication' } },
  { fixture: 'other-application-no-record', row: 'otherApplication', rowNumber: 9, atOnce: false, enabled: ['monitor', 'download', 'search', 'chooseAnother'], refused: { onlineChange: 'otherApplication' } },
  { fixture: 'edit-in-progress', row: 'upToDate', rowNumber: 5, atOnce: true, enabled: ['monitor', 'resolve', 'search', 'chooseAnother'], refused: { onlineChange: 'V4004', download: 'V4004' }, information: ['editInProgress'] },
  { fixture: 'build-fails', row: 'sourcesChanged', rowNumber: 6, atOnce: false, enabled: ['monitor', 'search', 'chooseAnother'], refused: { onlineChange: 'projectDoesNotBuild', download: 'projectDoesNotBuild' }, information: ['buildFails'] },
  { fixture: 'other-controller', row: 'upToDate', rowNumber: 5, atOnce: true, enabled: ['monitor', 'onlineChange', 'download', 'search', 'chooseAnother'], refused: {}, information: ['otherController'] },
  { fixture: 'connection-lost', row: 'connectionLost', rowNumber: 13, atOnce: false, enabled: ['search', 'chooseAnother', 'retry'], refused: { monitor: 'notConnected' } },
  { fixture: 'not-persisted', row: 'upToDate', rowNumber: 5, atOnce: true, enabled: ['monitor', 'onlineChange', 'download', 'search', 'chooseAnother'], refused: {}, information: ['notPersisted'] },
  { fixture: 'candidate-lost', row: 'upToDate', rowNumber: 5, atOnce: true, enabled: ['monitor', 'onlineChange', 'download', 'search', 'chooseAnother'], refused: {}, information: ['candidateLost'] },
  { fixture: 'retained-discarded', row: 'upToDate', rowNumber: 5, atOnce: true, enabled: ['monitor', 'onlineChange', 'download', 'search', 'chooseAnother'], refused: {}, information: ['retainedDiscarded'] },
  { fixture: 'container-format-refused', row: 'upToDate', rowNumber: 5, atOnce: true, enabled: ['monitor', 'search', 'chooseAnother'], refused: { onlineChange: 'containerFormat', download: 'containerFormat' }, information: ['containerFormatRefused'] },
  { fixture: 'stopped', row: 'upToDate', rowNumber: 5, atOnce: true, enabled: ['monitor', 'onlineChange', 'download', 'search', 'chooseAnother'], refused: {}, information: ['stopped'] },
  { fixture: 'faulted', row: 'upToDate', rowNumber: 5, atOnce: true, enabled: ['monitor', 'onlineChange', 'download', 'search', 'chooseAnother'], refused: {}, information: ['faulted'] },
  { fixture: 'no-device-id', row: 'upToDate', rowNumber: 5, atOnce: true, enabled: ['monitor', 'onlineChange', 'download', 'search', 'chooseAnother'], refused: {}, information: ['noDeviceId'] },
  { fixture: 'content-not-reported', row: 'otherApplication', rowNumber: 9, atOnce: false, enabled: ['monitor', 'download', 'search', 'chooseAnother'], refused: { onlineChange: 'otherApplication' }, information: ['contentNotReported'] },
  { fixture: 'never-built', row: 'otherApplication', rowNumber: 9, atOnce: false, enabled: ['monitor', 'download', 'search', 'chooseAnother'], refused: { onlineChange: 'otherApplication' } },
];

suite('parseComparison', () => {
  test('parseComparison_when_the_compiler_printed_it_then_every_field_is_read', () => {
    const comparison = fixture('previous-download');

    assert.strictEqual(comparison.row, 'previousDownload');
    assert.strictEqual(comparison.rowNumber, 7);
    assert.strictEqual(comparison.connected, true);
    assert.strictEqual(comparison.controller?.deviceId, '0123456789abcdef0123456789abcdef');
    assert.strictEqual(comparison.controller?.runtimeVersion, '0.247.0');
    assert.strictEqual(comparison.controller?.runState, 'running');
    assert.strictEqual(comparison.controller?.holds.kind, 'application');
    assert.strictEqual(comparison.controller?.holds.contentHash?.length, 64);
    assert.strictEqual(comparison.project.status, 'upToDate');
    assert.strictEqual(comparison.project.application, 'main');
    assert.strictEqual(comparison.lastDownload?.time, '2026-01-01T00:00:00Z');
    assert.deepStrictEqual(comparison.actions.map(offer => offer.action), [...ACTION_KINDS]);
    assert.strictEqual(comparison.hotEdit.allowed, true);
  });

  test('parseComparison_when_the_attempt_did_not_reach_the_controller_then_there_is_no_controller', () => {
    const comparison = fixture('unreachable');

    assert.strictEqual(comparison.controller, null);
    assert.strictEqual(comparison.connected, false);
    assert.strictEqual(comparison.lastDownload, null);
  });

  test('parseComparison_when_the_controller_holds_nothing_or_does_not_say_then_the_kind_is_kept', () => {
    assert.strictEqual(fixture('no-application').controller?.holds.kind, 'noApplication');
    assert.strictEqual(fixture('content-not-reported').controller?.holds.kind, 'unknown');
  });

  test('parseComparison_when_the_compiler_lists_the_actions_then_they_are_the_actions_of_the_mirror_in_order', () => {
    // The mirror's list is the compiler's `ActionKind::ALL`: a drift fails here.
    for (const name of fixtureNames()) {
      assert.deepStrictEqual(fixture(name).actions.map(offer => offer.action), [...ACTION_KINDS], name);
    }
  });

  test('parseComparison_when_not_json_then_rejects', () => {
    assert.throws(() => parseComparison('Build started'), /not JSON/);
  });

  test('parseComparison_when_a_required_field_is_missing_then_rejects_and_says_which', () => {
    const value = JSON.parse(fixtureText('up-to-date')) as Record<string, unknown>;
    for (const [key, expected] of [['summary', /summary/], ['actions', /actions/], ['hotEdit', /hot edit/], ['project', /project/], ['information', /information/]] as const) {
      const damaged = { ...value };
      delete damaged[key];
      assert.throws(() => parseComparison(JSON.stringify(damaged)), expected, key);
    }
    assert.throws(() => parseComparison('[1]'), /no object/);
  });

  test('parseComparison_when_the_compiler_adds_fields_then_they_are_ignored', () => {
    const value = JSON.parse(fixtureText('up-to-date')) as Record<string, unknown>;

    const comparison = parseComparison(JSON.stringify({ ...value, later: { anything: 1 } }));

    assert.strictEqual(comparison.row, 'upToDate');
  });
});

suite('comparisonModel for every row of the go-online table', () => {
  for (const expected of ROWS) {
    test(`comparisonModel_when_${expected.fixture.replace(/-/g, '_')}_then_row_${expected.rowNumber}_${expected.row}_with_its_actions`, () => {
      const comparison = fixture(expected.fixture);
      const model = comparisonModel(comparison);

      assert.strictEqual(comparison.row, expected.row);
      assert.strictEqual(comparison.rowNumber, expected.rowNumber);
      assert.strictEqual(comparison.atOnce, expected.atOnce);
      assert.strictEqual(model.title, comparison.summary);
      assert.deepStrictEqual(model.items.filter(entry => entry.enabled).map(entry => entry.action).sort(), [...expected.enabled].sort());
      for (const [action, code] of Object.entries(expected.refused)) {
        const refused = item(model.items, action);
        assert.strictEqual(refused.enabled, false, action);
        assert.strictEqual(refused.description, code, action);
        assert.ok(refused.detail.length > 0, action);
      }
      const noted = model.rows.filter(row => row.kind === 'information').length;
      assert.strictEqual(noted, expected.information?.length ?? 0);
      assert.deepStrictEqual(comparison.information.map(note => note.kind), expected.information ?? []);
    });
  }

  test('comparisonModel_when_every_fixture_then_the_items_are_the_actions_of_the_comparison_in_its_order', () => {
    for (const name of fixtureNames()) {
      const comparison = fixture(name);
      const model = comparisonModel(comparison);

      assert.deepStrictEqual(model.items.map(entry => entry.action), comparison.actions.map(offer => offer.action), name);
      assert.deepStrictEqual(model.items.map(entry => entry.label), comparison.actions.map(offer => offer.label), name);
      for (const [index, entry] of model.items.entries()) {
        // The enablement is the compiler's, and a refusal carries the compiler's code and text.
        assert.strictEqual(entry.enabled, comparison.actions[index].enabled, `${name} ${entry.action}`);
        if (!entry.enabled) {
          assert.strictEqual(entry.description, comparison.actions[index].reason?.code, `${name} ${entry.action}`);
          assert.strictEqual(entry.detail, comparison.actions[index].reason?.text, `${name} ${entry.action}`);
        }
      }
    }
  });

  test('comparisonModel_when_every_fixture_then_every_information_row_is_shown_with_its_text', () => {
    for (const name of fixtureNames()) {
      const comparison = fixture(name);
      const shown = comparisonModel(comparison).rows.filter(row => row.kind === 'information').map(row => row.text);

      assert.deepStrictEqual(shown, comparison.information.map(note => note.text), name);
    }
  });

  test('comparisonModel_when_enabled_action_has_a_detail_then_the_detail_is_shown_under_it', () => {
    const model = comparisonModel(fixture('other-application'));

    assert.ok(item(model.items, 'download').detail.startsWith('Download replaces the application'));
    assert.strictEqual(item(model.items, 'download').description, '');
  });

  test('comparisonModel_when_controller_reached_then_lines_for_the_controller_the_project_and_the_last_download', () => {
    const model = comparisonModel(fixture('previous-download'));

    assert.deepStrictEqual(model.rows.map(row => row.kind), ['controller', 'project', 'lastDownload']);
    assert.ok(model.rows[0].text.includes('01234567'), model.rows[0].text);
    assert.ok(model.rows[0].text.includes('runtime 0.247.0'), model.rows[0].text);
    assert.ok(model.rows[0].text.includes('running'), model.rows[0].text);
    assert.ok(model.rows[0].text.includes('holds application'), model.rows[0].text);
    assert.ok(model.rows[1].text.includes('application main'), model.rows[1].text);
    assert.ok(model.rows[2].text.startsWith('2026-01-01T00:00:00Z'), model.rows[2].text);
  });

  test('comparisonModel_when_attempt_did_not_reach_the_controller_then_no_controller_line', () => {
    const model = comparisonModel(fixture('busy'));

    assert.deepStrictEqual(model.rows.map(row => row.kind), ['project']);
  });

  test('comparisonModel_when_controller_holds_nothing_or_does_not_say_then_the_line_says_so', () => {
    assert.ok(comparisonModel(fixture('no-application')).rows[0].text.includes('holds no application'));
    assert.ok(comparisonModel(fixture('content-not-reported')).rows[0].text.includes('content not reported'));
  });

  test('comparisonModel_when_an_edit_is_unfinished_then_the_controller_line_names_the_stage', () => {
    assert.ok(comparisonModel(fixture('edit-in-progress')).rows[0].text.includes('edit testing'));
  });

  test('comparisonModel_when_project_never_built_then_the_project_line_says_so', () => {
    assert.ok(comparisonModel(fixture('never-built')).rows.find(row => row.kind === 'project')?.text.startsWith('never built'));
  });

  test('comparisonModel_when_row_9_then_download_is_enabled_and_online_change_is_disabled_with_its_reason', () => {
    const model = comparisonModel(fixture('other-application-no-record'));

    assert.strictEqual(item(model.items, 'download').enabled, true);
    const online = item(model.items, 'onlineChange');
    assert.strictEqual(online.enabled, false);
    assert.strictEqual(online.description, 'otherApplication');
    assert.ok(online.detail.includes('Download replaces it'), online.detail);
  });
});

suite('comparisonModel tolerates what a later compiler adds', () => {
  function changed(edit: (value: Record<string, any>) => void): Comparison {
    const value = JSON.parse(fixtureText('other-application')) as Record<string, any>;
    edit(value);
    return parseComparison(JSON.stringify(value));
  }

  test('comparisonModel_when_a_new_headline_row_then_its_summary_is_the_title', () => {
    const comparison = changed((value) => {
      value.row = 'brandNewRow';
      value.summary = 'The controller is in a state this editor has never seen.';
    });

    assert.strictEqual(comparisonModel(comparison).title, 'The controller is in a state this editor has never seen.');
    assert.strictEqual(statusSummary(comparison), 'The controller is in a state this editor has never seen.');
  });

  test('comparisonModel_when_a_new_information_row_then_it_is_shown_with_its_text', () => {
    const comparison = changed((value) => {
      value.information.push({ kind: 'somethingNew', text: 'The controller reports something new.' });
    });

    const rows = comparisonModel(comparison).rows.filter(row => row.kind === 'information');

    assert.deepStrictEqual(rows.map(row => row.text), ['The controller reports something new.']);
  });

  test('comparisonModel_when_a_new_action_then_it_is_shown_disabled_and_says_the_extension_cannot_run_it', () => {
    const comparison = changed((value) => {
      value.actions.push({ action: 'rewind', label: 'Rewind', enabled: true, reason: null, detail: null });
    });

    const rewind = item(comparisonModel(comparison).items, 'rewind');

    assert.strictEqual(rewind.enabled, false);
    assert.strictEqual(rewind.run, undefined);
    assert.strictEqual(rewind.description, 'unsupported');
    assert.ok(rewind.detail.includes('update the extension'));
  });

  test('comparisonModel_when_a_new_action_is_disabled_then_its_own_reason_is_shown', () => {
    const comparison = changed((value) => {
      value.actions.push({ action: 'rewind', label: 'Rewind', enabled: false, reason: { code: 'noHistory', text: 'There is no history.' }, detail: null });
    });

    const rewind = item(comparisonModel(comparison).items, 'rewind');

    assert.strictEqual(rewind.description, 'noHistory');
    assert.strictEqual(rewind.detail, 'There is no history.');
  });

  test('comparisonModel_when_new_kinds_of_holds_and_project_status_then_shown_as_the_compiler_names_them', () => {
    const comparison = changed((value) => {
      value.controller.holds = { kind: 'encrypted' };
      value.project.status = 'archived';
    });

    const rows = comparisonModel(comparison).rows;

    assert.ok(rows[0].text.includes('holds encrypted'), rows[0].text);
    assert.ok(rows[1].text.startsWith('archived'), rows[1].text);
  });

  test('comparisonModel_when_a_disabled_action_has_no_reason_then_it_is_still_shown_disabled', () => {
    const comparison = changed((value) => {
      value.actions[0].enabled = false;
      value.actions[0].reason = null;
    });

    const monitor = item(comparisonModel(comparison).items, 'monitor');

    assert.strictEqual(monitor.enabled, false);
    assert.strictEqual(monitor.description, 'unavailable');
  });
});

suite('ACTION_HANDLERS', () => {
  test('ACTION_HANDLERS_when_checked_then_every_action_kind_of_the_mirror_has_exactly_one_entry', () => {
    assert.deepStrictEqual(Object.keys(ACTION_HANDLERS).sort(), [...ACTION_KINDS].sort());
    for (const kind of ACTION_KINDS) {
      assert.notStrictEqual(handlerOf(kind), undefined, kind);
    }
  });

  test('ACTION_HANDLERS_when_every_fixture_then_every_action_the_compiler_offers_has_a_handler', () => {
    for (const name of fixtureNames()) {
      for (const offer of fixture(name).actions) {
        assert.notStrictEqual(handlerOf(offer.action), undefined, `${name} ${offer.action}`);
      }
    }
  });

  test('ACTION_HANDLERS_when_command_handlers_then_each_runs_a_command_the_extension_contributes', () => {
    const manifest = JSON.parse(fs.readFileSync(path.join(__dirname, '..', '..', '..', 'package.json'), 'utf-8')) as {
      contributes: { commands: { command: string }[] };
    };
    const contributed = new Set(manifest.contributes.commands.map(entry => entry.command));

    for (const kind of ACTION_KINDS) {
      const handler = ACTION_HANDLERS[kind];
      if (handler.kind === 'command') {
        assert.ok(contributed.has(handler.command), `${kind} runs ${handler.command}, which package.json does not contribute`);
      }
    }
  });

  test('ACTION_HANDLERS_when_looked_up_then_each_action_means_one_editor_action', () => {
    assert.deepStrictEqual(ACTION_HANDLERS.monitor, { kind: 'close' });
    assert.deepStrictEqual(ACTION_HANDLERS.onlineChange, { kind: 'command', command: 'ironplc.onlineEdit' });
    assert.deepStrictEqual(ACTION_HANDLERS.resolve, { kind: 'command', command: 'ironplc.onlineEdit' });
    assert.deepStrictEqual(ACTION_HANDLERS.download, { kind: 'command', command: 'ironplc.download' });
    assert.deepStrictEqual(ACTION_HANDLERS.chooseAnother, { kind: 'command', command: 'ironplc.chooseController' });
    assert.deepStrictEqual(ACTION_HANDLERS.search, { kind: 'search' });
    assert.deepStrictEqual(ACTION_HANDLERS.retry, { kind: 'retry' });
    assert.deepStrictEqual(ACTION_HANDLERS.upload, { kind: 'none' });
  });

  test('handlerOf_when_an_inherited_property_name_then_no_handler', () => {
    assert.strictEqual(handlerOf('toString'), undefined);
    assert.strictEqual(handlerOf('constructor'), undefined);
  });

  test('comparisonModel_when_every_fixture_then_an_enabled_item_always_has_a_handler', () => {
    for (const name of fixtureNames()) {
      for (const entry of comparisonModel(fixture(name)).items) {
        assert.strictEqual(entry.enabled, entry.run !== undefined && fixture(name).actions.find(offer => offer.action === entry.action)?.enabled === true, `${name} ${entry.action}`);
      }
    }
  });
});

suite('statusSummary', () => {
  test('statusSummary_when_a_comparison_then_the_summary_of_its_row', () => {
    assert.strictEqual(statusSummary(fixture('up-to-date')), 'Up to date: the controller runs the last build of this project.');
    assert.strictEqual(statusSummary(fixture('unreachable')), 'The controller cannot be reached.');
  });

  test('statusSummary_when_no_comparison_then_nothing', () => {
    assert.strictEqual(statusSummary(undefined), undefined);
    assert.strictEqual(statusSummary(null), undefined);
  });

  test('statusSummary_when_every_fixture_then_the_summary_is_one_line', () => {
    for (const name of fixtureNames()) {
      const summary = statusSummary(fixture(name));
      assert.ok(summary !== undefined && summary.length > 0 && !summary.includes('\n'), name);
    }
  });
});

suite('acceptGate', () => {
  test('acceptGate_when_hot_edit_is_allowed_then_allowed', () => {
    assert.deepStrictEqual(acceptGate(fixture('up-to-date')), { allowed: true });
    assert.deepStrictEqual(acceptGate(fixture('previous-download')), { allowed: true });
  });

  test('acceptGate_when_controller_holds_another_application_then_refused_with_its_reason_and_download_offered', () => {
    const gate = acceptGate(fixture('other-application-no-record'));

    assert.ok(!gate.allowed);
    assert.strictEqual(gate.code, 'otherApplication');
    assert.ok(gate.text.includes('Download replaces it'));
    assert.strictEqual(gate.offerDownload, true);
  });

  test('acceptGate_when_no_application_on_the_controller_then_refused_and_download_offered', () => {
    const gate = acceptGate(fixture('no-application'));

    assert.ok(!gate.allowed);
    assert.strictEqual(gate.code, 'noApplicationToChange');
    assert.strictEqual(gate.offerDownload, true);
  });

  test('acceptGate_when_project_does_not_build_then_refused_and_download_not_offered', () => {
    const gate = acceptGate(fixture('build-fails'));

    assert.ok(!gate.allowed);
    assert.strictEqual(gate.code, 'projectDoesNotBuild');
    assert.strictEqual(gate.offerDownload, false);
  });

  test('acceptGate_when_controller_refuses_an_edit_then_its_own_code_and_text_are_kept', () => {
    const gate = acceptGate(fixture('edit-in-progress'));

    assert.ok(!gate.allowed);
    assert.strictEqual(gate.code, 'V4004');
    assert.strictEqual(gate.text, 'a candidate is staged');
    assert.strictEqual(gate.offerDownload, false);
  });

  test('acceptGate_when_container_format_is_refused_then_refused_and_download_not_offered', () => {
    const gate = acceptGate(fixture('container-format-refused'));

    assert.ok(!gate.allowed);
    assert.strictEqual(gate.code, 'containerFormat');
    assert.strictEqual(gate.offerDownload, false);
  });

  test('acceptGate_when_not_connected_then_refused', () => {
    const gate = acceptGate(fixture('unreachable'));

    assert.ok(!gate.allowed);
    assert.strictEqual(gate.code, 'notConnected');
  });

  test('acceptGate_when_hot_edit_says_allowed_then_allowed_whatever_the_action_list_says', () => {
    // The gate reads `hotEdit.allowed`, the answer the compiler evaluated once.
    const comparison = fixture('other-application');
    comparison.hotEdit = { allowed: true, reason: null };

    assert.deepStrictEqual(acceptGate(comparison), { allowed: true });
  });

  test('acceptGate_when_every_fixture_then_it_agrees_with_hot_edit_allowed', () => {
    for (const name of fixtureNames()) {
      const comparison = fixture(name);
      assert.strictEqual(acceptGate(comparison).allowed, comparison.hotEdit.allowed, name);
    }
  });

  test('acceptGate_when_no_comparison_could_be_made_then_refused_and_says_so', () => {
    const gate = acceptGate(undefined);

    assert.ok(!gate.allowed);
    assert.strictEqual(gate.text, NO_COMPARISON_TEXT);
    assert.strictEqual(gate.offerDownload, false);
  });

  test('acceptGate_when_refused_without_a_reason_then_the_summary_is_the_text', () => {
    const comparison = fixture('other-application');
    comparison.hotEdit = { allowed: false, reason: null };

    const gate = acceptGate(comparison);

    assert.ok(!gate.allowed);
    assert.strictEqual(gate.text, comparison.summary);
  });
});

suite('replacesOtherApplication', () => {
  test('replacesOtherApplication_when_another_application_and_a_download_on_record_then_true', () => {
    assert.strictEqual(replacesOtherApplication(fixture('other-application')), true);
  });

  test('replacesOtherApplication_when_another_application_and_no_record_then_false', () => {
    assert.strictEqual(replacesOtherApplication(fixture('other-application-no-record')), false);
  });

  test('replacesOtherApplication_when_the_controller_holds_the_build_or_nothing_is_known_then_false', () => {
    assert.strictEqual(replacesOtherApplication(fixture('up-to-date')), false);
    assert.strictEqual(replacesOtherApplication(fixture('previous-download')), false);
    assert.strictEqual(replacesOtherApplication(undefined), false);
  });
});

suite('the Show Online Comparison command', () => {
  test('SHOW_ONLINE_COMPARISON_when_checked_then_it_is_contributed_with_its_title', () => {
    const manifest = JSON.parse(fs.readFileSync(path.join(__dirname, '..', '..', '..', 'package.json'), 'utf-8')) as {
      contributes: { commands: { command: string; title: string; category?: string }[] };
    };

    const contributed = manifest.contributes.commands.find(entry => entry.command === SHOW_ONLINE_COMPARISON.commandId);

    assert.strictEqual(contributed?.title, SHOW_ONLINE_COMPARISON.title);
    assert.strictEqual(contributed?.category, 'IronPLC');
  });
});
