import * as assert from 'assert';
import {
  acceptEditsWithDecisions,
  formatMigrationWarning,
  migrationPlan,
  MigrationDecisionUi,
  preservablePairs,
} from '../../hotEditMigrationLogic';
import {
  HotEditProtocolError,
  MigrationDecisionMap,
  TypeChangePair,
} from '../../hotEditSession';

/**
 * A mocked accept transport: tests script one outcome per `acceptEdits`
 * invocation and record the decision map of every call, so no real
 * `ironplcvm` child process is needed to exercise the flow.
 */
class MockAcceptClient {
  private readonly outcomes: (HotEditProtocolError | undefined)[] = [];
  readonly migrations: (MigrationDecisionMap | undefined)[] = [];

  /** Queues the outcome (an error, or success when omitted) per call, in order. */
  respond(...outcomes: (HotEditProtocolError | undefined)[]): MockAcceptClient {
    this.outcomes.push(...outcomes);
    return this;
  }

  acceptEdits = async (_program: Uint8Array, migration?: MigrationDecisionMap): Promise<void> => {
    this.migrations.push(migration);
    const outcome = this.outcomes[this.migrations.length - 1];
    if (outcome) {
      throw outcome;
    }
  };
}

/** A decision UI that returns a scripted selection and records the pairs it saw. */
function scriptedUi(selection: readonly number[] | undefined) {
  const seen: TypeChangePair[][] = [];
  const ui: MigrationDecisionUi = {
    async choosePreserved(pairs: readonly TypeChangePair[]) {
      seen.push([...pairs]);
      return selection;
    },
  };
  return { ui, seen };
}

function pair(overrides?: Partial<TypeChangePair>): TypeChangePair {
  return { uid: 1, name: 'Counter', from: 'I32', to: 'U32', sizeEqual: true, ...overrides };
}

function refusal(pairs: TypeChangePair[]): HotEditProtocolError {
  return new HotEditProtocolError('V4010', 'the edit changes variable types', pairs);
}

const PROGRAM = new Uint8Array([1, 2, 3]);

suite('preservablePairs', () => {
  test('preservablePairs_when_mixed_then_only_size_equal_rows', () => {
    const equal = pair({ uid: 1, sizeEqual: true });
    const mismatch = pair({ uid: 2, sizeEqual: false });

    assert.deepStrictEqual(preservablePairs([equal, mismatch]), [equal]);
  });
});

suite('migrationPlan', () => {
  test('migrationPlan_when_nothing_selected_then_every_uid_init', () => {
    const plan = migrationPlan([pair({ uid: 1 }), pair({ uid: 2 })], []);

    assert.deepStrictEqual(plan, { [1]: 'init', [2]: 'init' });
  });

  test('migrationPlan_when_size_equal_selected_then_preserve_and_rest_init', () => {
    const plan = migrationPlan([pair({ uid: 1 }), pair({ uid: 2 })], [1]);

    assert.deepStrictEqual(plan, { [1]: 'preserve', [2]: 'init' });
  });

  test('migrationPlan_when_size_mismatch_selected_then_downgraded_to_init', () => {
    const plan = migrationPlan([pair({ uid: 7, sizeEqual: false })], [7]);

    assert.deepStrictEqual(plan, { [7]: 'init' });
  });
});

suite('formatMigrationWarning', () => {
  test('formatMigrationWarning_when_preserving_then_states_irreversible_and_invalid_value', () => {
    const warning = formatMigrationWarning(2, 1);

    assert.ok(warning.includes('irreversible'));
    assert.ok(warning.includes('may no longer be valid'));
    assert.ok(warning.includes('1 variable(s) are reinitialized'));
  });

  test('formatMigrationWarning_when_all_reinitialized_then_no_preserve_claim', () => {
    const warning = formatMigrationWarning(2, 0);

    assert.ok(!warning.includes('may no longer be valid'));
    assert.ok(warning.includes('2 variable(s) are reinitialized'));
  });
});

suite('acceptEditsWithDecisions', () => {
  test('acceptEditsWithDecisions_when_accepted_then_no_ui_and_one_call', async () => {
    const client = new MockAcceptClient();
    const { ui, seen } = scriptedUi(undefined);

    await acceptEditsWithDecisions(client, PROGRAM, ui);

    assert.strictEqual(seen.length, 0);
    assert.deepStrictEqual(client.migrations, [undefined]);
  });

  test('acceptEditsWithDecisions_when_error_without_pairs_then_rethrows_and_no_ui', async () => {
    const error = new HotEditProtocolError('V4013', 'a candidate is already staged');
    const client = new MockAcceptClient().respond(error);
    const { ui, seen } = scriptedUi([]);

    const err = await rejectWith(acceptEditsWithDecisions(client, PROGRAM, ui));

    assert.strictEqual(err, error);
    assert.strictEqual(seen.length, 0);
    assert.deepStrictEqual(client.migrations, [undefined]);
  });

  test('acceptEditsWithDecisions_when_pairs_and_nothing_selected_then_resubmits_all_init', async () => {
    const pairs = [pair({ uid: 1 }), pair({ uid: 2, sizeEqual: false })];
    const client = new MockAcceptClient().respond(refusal(pairs));
    const { ui, seen } = scriptedUi([]);

    await acceptEditsWithDecisions(client, PROGRAM, ui);

    assert.deepStrictEqual(seen, [pairs]);
    assert.deepStrictEqual(client.migrations, [undefined, { [1]: 'init', [2]: 'init' }]);
  });

  test('acceptEditsWithDecisions_when_preserve_selected_then_resubmits_preserve_map', async () => {
    const pairs = [pair({ uid: 1 }), pair({ uid: 2, from: 'I32', to: 'F32' })];
    const client = new MockAcceptClient().respond(refusal(pairs));
    const { ui } = scriptedUi([2]);

    await acceptEditsWithDecisions(client, PROGRAM, ui);

    assert.deepStrictEqual(client.migrations, [undefined, { [1]: 'init', [2]: 'preserve' }]);
  });

  test('acceptEditsWithDecisions_when_cancelled_then_rethrows_original_refusal', async () => {
    const error = refusal([pair({ uid: 1 })]);
    const client = new MockAcceptClient().respond(error);
    const { ui } = scriptedUi(undefined);

    const err = await rejectWith(acceptEditsWithDecisions(client, PROGRAM, ui));

    assert.strictEqual(err, error);
    assert.deepStrictEqual(client.migrations, [undefined]);
  });

  test('acceptEditsWithDecisions_when_resubmit_fails_then_propagates_the_new_error', async () => {
    const first = refusal([pair({ uid: 1 })]);
    const second = new HotEditProtocolError('V4010', 'unknown decision uid');
    const client = new MockAcceptClient().respond(first, second);
    const { ui } = scriptedUi([]);

    const err = await rejectWith(acceptEditsWithDecisions(client, PROGRAM, ui));

    assert.strictEqual(err, second);
    assert.strictEqual(client.migrations.length, 2);
  });
});

/** Awaits a promise expected to reject, returning the rejection reason. */
async function rejectWith(promise: Promise<unknown>): Promise<unknown> {
  try {
    await promise;
  }
  catch (err) {
    return err;
  }
  throw new Error('expected the promise to reject');
}
