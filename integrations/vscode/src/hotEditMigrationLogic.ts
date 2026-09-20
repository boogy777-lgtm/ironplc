/**
 * The VS Code side of the ADR-0061 migration decision flow: an `acceptEdits`
 * refusal (V4010) names every out-of-policy storage-class change; the
 * engineer decides per variable between `init` (the fail-closed default) and
 * `preserve` (same-size pairs only), and the identical payload is resubmitted
 * once with the resulting `migration` map (see `hotEditSession`).
 *
 * The flow owns no process or UI state — the caller injects a
 * decision-selection callback, mirroring how `syncUidsLogic` injects its
 * resolution UI, so every planning decision (all-init default, preserve only
 * for size-equal rows, cancel re-surfaces the original error) is
 * unit-testable without a real `ironplcvm` child process.
 */

import {
  EditIdentity,
  HotEditProtocolError,
  HotEditSession,
  MigrationDecisionMap,
  TypeChangePair,
} from './hotEditSession';

/** UI decisions the migration flow needs, injected so the flow is unit-testable. */
export interface MigrationDecisionUi {
  /**
   * Presents one checkbox row per pending type change and returns the UIDs to
   * preserve (the checked rows); `undefined` cancels the edit. Only
   * size-equal rows are checkable (see [`preservablePairs`]); every other
   * row is fixed to init.
   */
  choosePreserved(pairs: readonly TypeChangePair[]): Promise<readonly number[] | undefined>;
}

/** The pending rows a `preserve` decision is legal for (the checkable ones). */
export function preservablePairs(pairs: readonly TypeChangePair[]): TypeChangePair[] {
  return pairs.filter(pair => pair.sizeEqual);
}

/**
 * The wire map for `pairs`: every row initializes (the fail-closed default),
 * except the `preserved` UIDs when their row is size-equal — so a stale or
 * malformed selection can never produce an illegal preserve.
 */
export function migrationPlan(
  pairs: readonly TypeChangePair[],
  preserved: readonly number[],
): MigrationDecisionMap {
  const keep = new Set(preserved);
  const plan: MigrationDecisionMap = {};
  for (const pair of pairs) {
    plan[String(pair.uid)] = pair.sizeEqual && keep.has(pair.uid) ? 'preserve' : 'init';
  }
  return plan;
}

/** The warning shown before applying an engineer-decided migration (ADR 0061). */
export function formatMigrationWarning(
  pairs: readonly TypeChangePair[],
  preserved: readonly number[],
): string {
  const parts = ['Applying the migration is irreversible; values never roll back.'];
  if (preserved.length > 0) {
    parts.push(
      'Preserve keeps the raw storage bits reinterpreted under the new type,'
      + ' so the value may no longer be valid.',
    );
  }
  const keep = new Set(preserved);
  const reinitialized = pairs.filter(pair => !keep.has(pair.uid));
  if (reinitialized.length > 0) {
    // The warning is the only place the reinitialized variables are named
    // when no row was checkable (no quick pick is shown then).
    parts.push(
      `${reinitialized.length} variable(s) are reinitialized (the default decision): `
      + `${reinitialized.map(pair => pair.name ?? `uid ${pair.uid}`).join(', ')}.`,
    );
  }
  return parts.join(' ');
}

/**
 * Stages `program` as the edit candidate, driving the decision flow on a
 * V4010 refusal that carries pairs: ask which variables preserve, build the
 * map (unchecked = init), and resubmit the identical payload once. The
 * optional `edit` identity (ADR-0064) labels the pending-edit record on both
 * attempts. A refusal without pairs, an engineer cancel, or any resubmit
 * failure propagates to the caller, which surfaces the coded protocol error
 * as before.
 */
export async function acceptEditsWithDecisions(
  client: Pick<HotEditSession, 'acceptEdits'>,
  program: Uint8Array,
  ui: MigrationDecisionUi,
  edit?: EditIdentity,
): Promise<void> {
  try {
    await client.acceptEdits(program, undefined, edit);
    return;
  }
  catch (err) {
    const pairs = err instanceof HotEditProtocolError ? err.pairs : [];
    if (pairs.length === 0) {
      throw err;
    }
    const preserved = await ui.choosePreserved(pairs);
    if (preserved === undefined) {
      throw err;
    }
    await client.acceptEdits(program, migrationPlan(pairs, preserved), edit);
  }
}
