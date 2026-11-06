import * as assert from 'assert';
import * as fs from 'fs';
import * as path from 'path';

import {
  BUILD_STEP,
  BuildFact,
  dialogModel,
  DialogItem,
  phaseOf,
  reopenItem,
  STAGES,
  stageLabel,
} from '../../onlineEditLogic';
import { CandidateMode, EditView } from '../../hotEditSession';

/** One entry of the golden file: the runtime's own edit view for a state of the edit. */
interface GoldenEntry {
  state: string;
  view: EditView;
}

const GOLDEN_RELATIVE = ['compiler', 'runtime', 'resources', 'test', 'edit_view_golden.json'];

/**
 * The golden file of the runtime, found by walking up from this test until a
 * directory holds it: the depth of `out/test/unit` below the extension and of
 * the extension below the repository is then not written down.
 */
function goldenPath(): string {
  let dir = __dirname;
  for (;;) {
    const candidate = path.join(dir, ...GOLDEN_RELATIVE);
    if (fs.existsSync(candidate)) {
      return candidate;
    }
    const parent = path.dirname(dir);
    assert.notStrictEqual(parent, dir, `${GOLDEN_RELATIVE.join('/')} was not found above ${__dirname}`);
    dir = parent;
  }
}

const GOLDEN: GoldenEntry[] = JSON.parse(fs.readFileSync(goldenPath(), 'utf-8'));

function golden(state: string): EditView {
  const entry = GOLDEN.find(candidate => candidate.state === state);
  assert.ok(entry, `the golden file has no state ${state}`);
  return entry.view;
}

function item(items: readonly DialogItem[], kind: DialogItem['kind']): DialogItem {
  const found = items.find(candidate => candidate.kind === kind);
  assert.ok(found, `no ${kind} item`);
  return found;
}

function reasonOf(view: EditView, command: string): string {
  const reason = view.commands[command].reason;
  assert.ok(reason, `${command} has no reason`);
  return `${reason.code}: ${reason.text}`;
}

const BUILDS: BuildFact[] = ['notBuilt', 'built'];

suite('online edit dialog against the golden edit views', () => {
  test('golden_when_read_then_it_has_the_states_of_every_stage_the_dialog_names', () => {
    assert.ok(GOLDEN.length >= 11);
    const stages = new Set(GOLDEN.map(entry => entry.view.stage));
    for (const stage of ['idle', 'accepted', 'testing', 'testingMigration', 'testingReplace', 'swapPending']) {
      assert.ok(stages.has(stage), `the golden file has no ${stage}`);
    }
  });

  for (const entry of GOLDEN) {
    for (const build of BUILDS) {
      test(`dialogModel_when_${entry.state}_and_${build}_then_the_items_are_the_view`, () => {
        const model = dialogModel({ edit: entry.view }, build);
        const { view } = entry;
        const idleWithoutBuild = view.stage === 'idle' && build === 'notBuilt';
        assert.deepStrictEqual(model.items.map(candidate => candidate.kind), ['next', 'finalizeAll', 'cancel']);

        const next = item(model.items, 'next');
        if (idleWithoutBuild) {
          assert.strictEqual(next.label, 'Build');
          assert.strictEqual(next.command, BUILD_STEP);
          assert.strictEqual(next.enabled, true);
        }
        else if (view.next !== undefined) {
          assert.strictEqual(next.label, view.next.label);
          assert.strictEqual(next.command, view.next.command);
          assert.strictEqual(next.enabled, view.commands[view.next.command].enabled);
          // The device promises that its next command is allowed.
          assert.strictEqual(next.enabled, true);
        }
        else {
          assert.strictEqual(next.enabled, false);
          assert.strictEqual(next.command, undefined);
        }

        const finalize = item(model.items, 'finalizeAll');
        const finalizeView = view.commands.finalizeAll;
        assert.strictEqual(finalize.label, finalizeView.label);
        assert.strictEqual(finalize.enabled, finalizeView.enabled);
        if (finalizeView.enabled) {
          assert.deepStrictEqual(finalize.steps, idleWithoutBuild ? [BUILD_STEP, ...finalizeView.steps!] : finalizeView.steps);
        }
        else {
          assert.strictEqual(finalize.description, reasonOf(view, 'finalizeAll'));
          assert.strictEqual(finalize.steps, undefined);
        }

        const cancel = item(model.items, 'cancel');
        const cancelView = view.commands.cancel;
        assert.strictEqual(cancel.label, cancelView.label);
        assert.strictEqual(cancel.enabled, cancelView.enabled);
        if (cancelView.enabled) {
          assert.strictEqual(cancel.command, 'cancel');
          assert.deepStrictEqual(cancel.deviceSteps, cancelView.steps);
        }
        else {
          assert.strictEqual(cancel.description, reasonOf(view, 'cancel'));
          assert.strictEqual(cancel.command, undefined);
        }

        // A disabled item with a view reason shows exactly that reason.
        for (const shown of model.items) {
          if (!shown.enabled && shown.kind !== 'next') {
            assert.match(shown.description, /^V\d{4}: /);
          }
        }
      });
    }

    test(`dialogModel_when_${entry.state}_then_the_title_names_the_stage_and_the_warning_is_shown`, () => {
      const model = dialogModel({ edit: entry.view }, 'built');
      assert.ok(model.title.startsWith('Online Edit: '));
      // A stage the title table does not name would be shown by its wire name.
      assert.notStrictEqual(model.title, `Online Edit: ${entry.view.stage}`);
      assert.strictEqual(model.notices.includes(entry.view.warning ?? '\u0000'), entry.view.warning !== undefined);
    });
  }

  test('stages_when_compared_with_the_golden_commands_then_every_next_and_every_step_has_an_entry', () => {
    const known = new Set(STAGES.map(stage => stage.command));
    for (const entry of GOLDEN) {
      if (entry.view.next !== undefined) {
        assert.ok(known.has(entry.view.next.command), `${entry.view.next.command} is not in STAGES`);
      }
      for (const steps of [entry.view.commands.finalizeAll.steps ?? []]) {
        for (const step of steps) {
          assert.ok(known.has(step), `${step} is not in STAGES`);
        }
      }
    }
  });
});

suite('online edit dialog model', () => {
  test('dialogModel_when_idle_and_nothing_built_then_next_is_build_and_finalize_all_starts_with_build', () => {
    const model = dialogModel({ edit: golden('NoCandidate') }, 'notBuilt');

    assert.strictEqual(model.items[0].label, 'Build');
    assert.strictEqual(model.items[0].command, 'build');
    assert.deepStrictEqual(model.items[1].steps, ['build', 'accept', 'test', 'assemble']);
    assert.strictEqual(model.items[1].description, 'Build → Accept → Test → Assemble');
    assert.strictEqual(model.prompt, '● Build  ○ Accept  ○ Test  ○ Assemble');
    assert.strictEqual(model.title, 'Online Edit: No candidate');
    assert.strictEqual(model.step, 1);
    assert.strictEqual(model.totalSteps, 4);
  });

  test('dialogModel_when_idle_and_built_then_next_is_the_views_accept_and_build_is_marked_done', () => {
    const model = dialogModel({ edit: golden('NoCandidate') }, 'built');

    assert.strictEqual(model.items[0].label, 'Accept');
    assert.strictEqual(model.items[0].command, 'accept');
    assert.deepStrictEqual(model.items[1].steps, ['accept', 'test', 'assemble']);
    assert.strictEqual(model.prompt, '✔ Build  ● Accept  ○ Test  ○ Assemble');
    assert.strictEqual(model.title, 'Online Edit: Built');
    assert.strictEqual(model.step, 2);
  });

  test('dialogModel_when_a_candidate_exists_then_the_build_fact_does_not_change_next', () => {
    for (const build of BUILDS) {
      const model = dialogModel({ edit: golden('Accepted') }, build);

      assert.strictEqual(model.items[0].label, 'Test');
      assert.strictEqual(model.prompt, '✔ Build  ✔ Accept  ● Test  ○ Assemble');
      assert.strictEqual(model.step, 3);
      assert.strictEqual(model.title, 'Online Edit: Accepted');
    }
  });

  test('dialogModel_when_testing_then_assemble_is_current_and_cancel_is_the_two_step_exit', () => {
    const model = dialogModel({ edit: golden('Testing') }, 'built');
    const cancel = item(model.items, 'cancel');

    assert.strictEqual(model.items[0].label, 'Assemble');
    assert.strictEqual(model.prompt, '✔ Build  ✔ Accept  ✔ Test  ● Assemble');
    assert.strictEqual(model.step, 4);
    assert.strictEqual(cancel.enabled, true);
    assert.strictEqual(cancel.command, 'cancel');
    assert.deepStrictEqual(cancel.deviceSteps, ['untest', 'cancel']);
    assert.strictEqual(cancel.description, 'Untest, then Cancel');
    assert.strictEqual(cancel.steps, undefined);
  });

  test('dialogModel_when_testing_a_migration_or_a_replace_then_cancel_is_disabled_with_the_v4011_reason', () => {
    for (const state of ['TestingMigration', 'TestingReplace']) {
      const cancel = item(dialogModel({ edit: golden(state) }, 'built').items, 'cancel');

      assert.strictEqual(cancel.enabled, false);
      assert.strictEqual(cancel.command, undefined);
      assert.match(cancel.description, /^V4011: /);
    }
  });

  test('dialogModel_when_a_swap_is_pending_then_next_is_disabled_and_the_wait_is_marked', () => {
    const model = dialogModel({ edit: golden('TestPending') }, 'built');

    assert.strictEqual(model.items[0].label, 'Next');
    assert.strictEqual(model.items[0].enabled, false);
    assert.strictEqual(model.title, 'Online Edit: Waiting for the scan boundary');
    assert.strictEqual(model.prompt, '✔ Build  ✔ Accept  ● Test  ○ Assemble');
    assert.ok(model.items.every(shown => !shown.enabled));
  });

  test('dialogModel_when_running_then_busy_and_the_prompt_says_what_runs', () => {
    const idle = dialogModel({ edit: golden('Accepted') }, 'built');
    const busy = dialogModel({ edit: golden('Accepted') }, 'built', 'Test');

    assert.strictEqual(idle.busy, false);
    assert.strictEqual(busy.busy, true);
    assert.strictEqual(busy.prompt, `${idle.prompt}  —  Test…`);
  });

  test('dialogModel_when_a_candidate_is_staged_then_the_warning_of_the_view_is_a_notice', () => {
    const model = dialogModel({ edit: golden('Accepted') }, 'built');

    assert.deepStrictEqual(model.notices, ['a restart of the device loses this candidate']);
    assert.deepStrictEqual(dialogModel({ edit: golden('NoCandidate') }, 'built').notices, []);
  });

  test('dialogModel_when_the_candidate_is_a_download_or_a_migration_then_a_notice_says_so', () => {
    const modes: [CandidateMode, RegExp][] = [['replace', /Download/], ['migrate', /migrates/]];
    for (const [mode, expected] of modes) {
      const model = dialogModel({ edit: golden('Accepted'), candidateMode: mode }, 'built');

      assert.match(model.notices[0], expected);
    }
    assert.deepStrictEqual(dialogModel({ edit: golden('Accepted'), candidateMode: 'change' }, 'built').notices.length, 1);
    // No candidate, no notice about its kind.
    assert.deepStrictEqual(dialogModel({ edit: golden('NoCandidate'), candidateMode: 'replace' }, 'built').notices, []);
  });

  test('dialogModel_when_a_replace_candidate_is_staged_then_it_is_shown_as_the_ordinary_stages', () => {
    const accepted = dialogModel({ edit: golden('AcceptedReplace'), candidateMode: 'replace' }, 'notBuilt');
    const testing = dialogModel({ edit: golden('TestingReplace'), candidateMode: 'replace' }, 'notBuilt');

    assert.strictEqual(accepted.items[0].label, 'Test');
    assert.strictEqual(accepted.items[0].enabled, true);
    assert.strictEqual(testing.title, 'Online Edit: Testing (download)');
    assert.strictEqual(testing.items[0].label, 'Assemble');
    assert.strictEqual(testing.items[2].enabled, false);
  });

  test('dialogModel_when_the_device_has_no_edit_view_then_only_build_is_offered', () => {
    const notBuilt = dialogModel(undefined, 'notBuilt');
    const built = dialogModel({}, 'built');

    assert.deepStrictEqual(notBuilt.items.map(shown => shown.enabled), [true, false, false]);
    assert.strictEqual(notBuilt.items[0].label, 'Build');
    assert.deepStrictEqual(built.items.map(shown => shown.enabled), [false, false, false]);
    assert.strictEqual(built.title, 'Online Edit');
    assert.strictEqual(built.step, undefined);
    assert.strictEqual(built.prompt, '');
    assert.strictEqual(notBuilt.notices.length, 1);
  });

  test('dialogModel_when_a_stage_is_unknown_to_the_client_then_the_view_is_followed_without_marks', () => {
    const view: EditView = {
      stage: 'staging2',
      commands: {
        verify: { enabled: true, label: 'Verify' },
        finalizeAll: { enabled: false, label: 'Finalize all', reason: { code: 'V4013', text: 'busy' }, steps: ['verify'] },
        cancel: { enabled: true, label: 'Cancel' },
      },
      next: { command: 'verify', label: 'Verify' },
    };
    const model = dialogModel({ edit: view }, 'built');

    assert.strictEqual(model.title, 'Online Edit: staging2');
    assert.strictEqual(model.step, undefined);
    assert.strictEqual(model.prompt, '');
    assert.strictEqual(model.items[0].label, 'Verify');
    assert.strictEqual(model.items[0].command, 'verify');
    assert.strictEqual(model.items[2].description, '');
  });

  test('stageLabel_when_a_command_is_not_in_the_view_then_the_wire_name_is_capitalised', () => {
    assert.strictEqual(stageLabel('build', undefined), 'Build');
    assert.strictEqual(stageLabel('test', golden('Accepted')), 'Test');
    assert.strictEqual(stageLabel('test', undefined), 'Test');
    assert.strictEqual(stageLabel('verify', undefined), 'Verify');
  });

  test('phaseOf_when_a_stage_runs_then_the_device_panel_phase_follows_the_table', () => {
    assert.deepStrictEqual(STAGES.map(stage => phaseOf(stage.command)), ['compiling', 'uploading', 'verifying', 'running']);
    assert.strictEqual(phaseOf('cancel'), undefined);
  });

  test('reopenItem_when_a_candidate_exists_then_the_item_names_the_stage_and_carries_the_warning', () => {
    const shown = reopenItem({ edit: golden('Testing') });

    assert.ok(shown);
    assert.strictEqual(shown.text, '$(edit) Online Edit: Testing');
    assert.match(shown.tooltip, /a restart of the device loses this candidate/);
  });

  test('reopenItem_when_no_candidate_or_no_view_then_there_is_no_item', () => {
    assert.strictEqual(reopenItem({ edit: golden('NoCandidate') }), undefined);
    assert.strictEqual(reopenItem({}), undefined);
    assert.strictEqual(reopenItem(undefined), undefined);
  });
});

/** The names of the fields of `HotEditStatus`, read from its declaration. */
function statusFieldNames(): string[] {
  const text = fs.readFileSync(path.join(__dirname, '..', '..', '..', 'src', 'hotEditSession.ts'), 'utf-8');
  const body = /export interface HotEditStatus \{([\s\S]*?)\n\}/.exec(text);
  assert.ok(body, 'the declaration of HotEditStatus was not found');
  const names = [...body[1].matchAll(/^ {2}(\w+)\??:/gm)].map(match => match[1]);
  assert.ok(names.includes('edit') && names.includes('mode'), 'the declaration of HotEditStatus was not read');
  return names;
}

/** The code of a source without its comments and its string and template literals. */
function codeOf(text: string): string {
  return text
    .replace(/\/\*[\s\S]*?\*\//g, '')
    .replace(/\/\/.*$/gm, '')
    .replace(/'(?:[^'\\\n]|\\.)*'/g, '\'\'')
    .replace(/`(?:[^`\\]|\\.)*`/g, '``');
}

/** The modules that decide what the dialog shows or sends: they read the edit view and nothing else of a status. */
const DIALOG_MODULES = ['onlineEditLogic.ts', 'onlineEditRunner.ts'];

suite('online edit dialog guards', () => {
  test('dialog_modules_when_scanned_then_they_read_no_status_field_but_edit_and_candidate_mode', () => {
    const allowed = new Set(['edit', 'candidateMode']);
    const forbidden = statusFieldNames().filter(name => !allowed.has(name));
    assert.ok(forbidden.length >= 8);

    for (const file of DIALOG_MODULES) {
      const code = codeOf(fs.readFileSync(path.join(__dirname, '..', '..', '..', 'src', file), 'utf-8'));
      for (const name of forbidden) {
        assert.ok(!new RegExp(`\\.${name}\\b`).test(code), `${file} reads the status field ${name}`);
        assert.ok(!new RegExp(`\\{[^}]*\\b${name}\\b[^}]*\\}\\s*=`).test(code), `${file} destructures the status field ${name}`);
      }
    }
  });

  test('dialog_modules_when_scanned_then_the_guard_would_catch_a_read_of_another_field', () => {
    const code = codeOf('const m = status.mode; // comment: status.active\nconst s = \'status.rounds\';');

    assert.ok(/\.mode\b/.test(code));
    assert.ok(!/\.active\b/.test(code));
    assert.ok(!/\.rounds\b/.test(code));
  });

  test('dialog_modules_when_scanned_then_they_import_no_vscode', () => {
    for (const file of DIALOG_MODULES) {
      const text = fs.readFileSync(path.join(__dirname, '..', '..', '..', 'src', file), 'utf-8');
      assert.ok(!/from 'vscode'/.test(text), `${file} imports vscode`);
    }
  });
});
