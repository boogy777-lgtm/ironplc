# State Migration Across Declaration-Level Edits

status: accepted
date: 2026-09-15

## Context and Problem Statement

The runtime host performs online change by reloading the VM from a candidate
container on the host's buffers
([ADR-0052](0052-online-change-performed-by-the-runtime-host.md)): a candidate
whose `layout_hash` matches keeps the running state, and a candidate whose
hash differs is rejected. [ADR-0053](0053-stable-variable-ids-for-declaration-level-hot-edit.md)
removed the reason every declaration edit had to look like a layout change —
persistent variables carry a stable UID that survives renames, so the active
and candidate containers can be diffed per entity. The runtime still has to
decide, for every UID, whether the value may move, where its bytes live, and
what happens when the two sides disagree. The container spec's "Layout Hash
and Online Change" section names this successor feature but leaves the rules
open. How does the runtime classify a UID diff, what may it copy, and how does
a migration candidate behave under Test and Untest?

## Decision Drivers

* Fail closed. Copying a value onto the wrong entity is silent data
  corruption; an edit the planner cannot justify must be rejected with a
  typed error, and a rejected candidate must leave the running application
  untouched.
* A rename or reorder moves no data: the UID, not the index, decides where a
  value continues.
* Migration is all-or-nothing at the scan boundary: a scan never sees a mix
  of the two layouts or partially copied state.
* The VM stays a pure execution kernel
  ([ADR-0010](0010-no-std-vm-for-embedded-targets.md)); the diff, the copies
  and the arena adoption all live in the host layer.
* The baseline invariant — reverting a test changes code, never process
  state — has no reverse mapping across a schema change, and the host must
  say so rather than reset state silently.
* Function-block internals are not UID-covered yet; the POC keeps the safe
  rejection and defers field-level migration.

## Considered Options

* **Reject every declaration edit.** Stage 1's behavior; safe, but it makes a
  rename a full stop-load-start even though the entity's value was never in
  question.
* **Partial migration.** Copy the UIDs the planner can justify and reject only
  the rest. Rejected: a half-migrated candidate is exactly the silent
  corruption the design exists to prevent, and the operator cannot see which
  values were reset.
* **UID-diff planner with a fail-closed classification (chosen).** The
  planner compares the two containers' stable variable ID tables and rejects
  the whole candidate on any shared UID whose entry differs.
* **Full per-field FB migration now.** Rejected for this stage: field values
  are addressed through the type section's FB descriptors, not through UIDs,
  so per-field identity needs a format change of its own. The planner instead
  requires the FB tail layout to be identical and rejects otherwise.

## Decision Outcome

Chosen option: **a UID-diff migration planner in the runtime host**, because
it turns the declaration-level rejection into a per-entity decision without
weakening the fail-closed rule. `StateMigrationPlan` performs the diff at
stage time; the host applies it at the scan boundary.

* **UID diff classification.** For every stable variable ID entry in the
  candidate: a UID the two containers share is copied only when its
  `VarEntry` is identical (variable type, flags, and `extra`); a UID only in
  the candidate has no action because the candidate's init image already
  initialised it; a UID only in the base has no action because the value
  belongs to a dropped entity and goes away with the old buffers; a shared
  UID whose entry differs rejects the candidate with
  `MigrationError::IncompatibleEntry`. The rejection is fail-closed: `stage`
  returns the error and the host keeps the original artifact and buffers.
* **Data-region payloads.** A STRING/WSTRING variable copies its slot and its
  data-region region — the string header and the characters — sized from the
  type data as header plus `max_length ×` character width; before any byte
  moves the copy checks the active value's current length against the
  candidate's maximum length and refuses with `MigrationError::StringShrink`
  when it does not fit. An array or structure copies its region only when the
  array descriptor at the entry's `extra` index exists and is equal in both
  containers (`MigrationError::ArrayDescriptorMismatch` otherwise); the
  region size comes from the candidate descriptor. Structures are flat arrays
  of slots and take the array path. The shared entry already fixes the
  region's size, so a request that changes a maximum length or a descriptor is
  rejected by the classification above rather than quietly resized.
* **FB safety rule (POC).** An FB instance's slot points at a field region
  whose values are addressed through the type section's FB descriptors, and
  those fields have no UIDs. A migration in which either side's stable table
  names an `FB_INSTANCE` variable is therefore accepted only when the layout
  after the program prefix is identical: the program prefix size
  (`task_table.shared_globals_size`), the user FB descriptors, and the FB
  type descriptors (compared per type ID, independent of emission order).
  Any difference rejects with `MigrationError::FbLayoutUnsupported` rather
  than risk a slot that points at another instance's fields. This is
  deliberately conservative — it also rejects an add or remove of an FB
  instance when the tail layout moves — and FB field-level migration is a
  documented follow-up.
* **Untest policy.** A migration candidate may be tested, but not untested.
  Writes made under the candidate's layout have no defined reverse mapping for
  variables the candidate added or removed, so `untest` returns
  `OnlineChangeError::UntestUnsupported` — a typed error, not a silent state
  reset — and leaves the candidate active and the state unchanged. The
  allowed exits are `assemble`, which promotes the candidate and advances the
  application generation, or `cancel` while the original artifact is still
  active. The logic-only path keeps its untest behavior from ADR-0052.
* **All-or-nothing at the scan boundary.** The plan is built outside the scan
  while the original artifact runs. At the boundary the host builds a fresh
  buffer set from the candidate container, runs the candidate's init image
  once on it, applies the plan's copies to those fresh buffers, and adopts
  the buffer set as a whole. The active buffers are never modified in place,
  so a candidate refused at staging leaves the executing state byte-for-byte
  untouched, and the candidate's own declared initial values are what
  candidate-only entities start from.
* **Error surface.** Failures are typed: `MigrationError` variants behind
  `OnlineChangeError::MigrationUnsupported`, and `UntestUnsupported` for the
  untest refusal. As in ADR-0052, no user-facing runtime problem codes exist
  yet; the CLI surface assigns them later.

The candidate must also keep the header flags, the process-image sizes, and
the task identity, and both containers must carry stable variable IDs; those
checks stay in the host's candidate validation. A candidate with no
migration to justify is still rejected as layout-incompatible.

### Consequences

* Good, because a rename, name swap, reorder, add or remove migrates by UID:
  surviving values stay with their entities and no data moves for a rename.
* Good, because the planner either justifies every shared UID or rejects the
  whole candidate; there is no partial state that silently resets values.
* Good, because the commit is a wholesale buffer adoption at the boundary, so
  the running application can never observe a half-migrated state.
* Bad, because FB instances only migrate under a byte-identical tail layout;
  a field-level FB edit still requires a full stop-load-start.
* Bad, because a migration candidate cannot be untested: state written under
  the candidate layout is committed or discarded, never reversed.
* Neutral, because containers without stable variable IDs (or without a
  candidate-side table) keep the stage 1 rejection; migration is opt-in
  through the engineering side's UID allocation.

## More Information

* `specs/design/bytecode-container-format.md`, "Stable Variable IDs" and
  "Layout Hash and Online Change".
* [ADR-0052](0052-online-change-performed-by-the-runtime-host.md) — the
  host-level online change this planner extends.
* [ADR-0053](0053-stable-variable-ids-for-declaration-level-hot-edit.md) —
  the UID table the planner diffs.
* `compiler/runtime/src/migration.rs`, `host.rs`, `online_change.rs`,
  `error.rs`; acceptance tests in `compiler/runtime/tests/migration_acceptance.rs`.
