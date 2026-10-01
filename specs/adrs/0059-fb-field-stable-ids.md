# FB Field-Level Stable IDs for Instance Migration

status: accepted
date: 2026-09-15

## Context and Problem Statement

ADR-0054's migration planner decides per persistent variable, but an FB
instance's *fields* are not persistent variables: they are addressed through
the type section's user FB descriptors, and no UID covered them. The planner
therefore treated every FB instance as opaque and required the layout after
the program prefix (program prefix size, user FB descriptors, FB type
descriptors) to be byte-identical before it would carry even the instance's
slot. A field insert, remove, or reorder — the edits an FB type undergoes
most — forced a full stop-load-start even though the surviving fields' values
were never in question. The container format had no place to record field
identity, and the engineering-side UID store (the sidecar, ADR-0053) keyed
only program-scope declarations.

## Decision Drivers

* Fail closed, as in ADR-0054: a value copied onto the wrong field is silent
  data corruption, so a field the planner cannot identify must reject the
  candidate, not guess.
* Identity is not position: inserting a field shifts the ordinals of the
  following fields, so ordinals alone cannot carry values across the edit.
* The UID sidecar's format and sync/map-uid flows already track `(scope,
  name)` keys; FB fields should reuse them rather than grow a second store.
* Standard-library FBs (TON, TOF, ...) have fixed, VM-owned layouts; their
  fields are not engineering entities and need no UIDs.
* The container carries no names (they belong in the debug section), so field
  identity on the wire must be numeric.

## Considered Options

* **Keep the POC rejection.** Safe, but a field insert in a widely used FB
  type still stops the PLC for every instance of the type.
* **Per-FB-type field UIDs in the type section (chosen).** A sixth type-
  section sub-table maps `(fb_type_id, field_index)` to the field's
  engineering UID; the migration planner matches a shared instance's fields
  by UID and copies, initialises, or drops each field individually.
* **Match fields by name in the container.** Rejected: names do not belong in
  the type section, and a stripped debug section would leave nothing to match
  on.
* **Bump the sidecar format for a field table.** Rejected: the existing
  `(scope, name)` key already expresses `(qualified FB type name, field)`
  with no format change; a second table would duplicate the sync/rename
  machinery.

## Decision Outcome

Chosen option: **per-FB-type field UIDs**, extending the ADR-0053 pattern one
level inward. `FORMAT_VERSION` bumps to 6.

* **Container (format v6).** The type section gains a sixth and last
  sub-table of FB field UIDs: a u16 count followed by 11-byte entries
  (`fb_type_id` u16 LE, `field_index` u8, `uid` u64 LE) in ascending
  `(fb_type_id, field_index)` order. Only user-defined FB types carry
  entries. The load-time verifier (ADR-0006) checks that each entry names an
  existing user FB descriptor and a field ordinal within its `num_fields`,
  and that entries ascend; like the stable variable IDs, the table is
  identity rather than layout and is excluded from `layout_hash`. A type
  section ending before the sub-table (a pre-v6 container) reads as zero
  entries, so v5 containers keep loading and simply carry no field UIDs.
* **Keying.** The sidecar format is unchanged. An FB field's key is
  `(scope = qualified FB type name, name = field)`; `declared_var_keys` now
  collects FB fields alongside program variables, so the existing `sync` and
  `map-uid` flows cover them with no command changes. At compile time the
  project splits the keyed table with the library's declarations: scopes that
  name FB types become codegen's `(fb_type, field)` UID table, the rest stay
  the name-only stable variable table codegen matches by name (ADR-0053's
  first-match rule is unchanged).
* **Codegen.** The FB pre-scan already enumerates the type's fields in
  (VAR_INPUT, VAR_OUTPUT, VAR) order; it now records an `FbFieldUidEntry` for
  each field the engineering table names, and the collection pass sorts the
  table and rejects a duplicate `(fb_type_id, field_index)` as an internal
  error. Fields the table does not name carry no entry.
* **Migration.** For a shared-UID instance of a user FB type (a user FB
  descriptor exists on both sides under the instance's type ID): when the
  descriptors are identical, the plan copies the slot and the whole field
  region (`num_fields * 8` bytes); when they differ, the plan matches the
  type's fields by UID — a field UID present on both sides copies its 8-byte
  slot when the field's variable-table entries are identical
  (`MigrationError::IncompatibleEntry` otherwise), a candidate-only field UID
  is initialised by the candidate's init image, and a base-only field UID is
  dropped with the old buffers. A candidate field whose UID is unknown — no
  entry, or the reserved UID 0 — while the layout differs rejects the whole
  candidate with `MigrationError::FbLayoutUnsupported`: the value's identity
  is unprovable, so the planner fails closed. Standard-library FB instances,
  and any instance only one side has, keep ADR-0054's global tail rule as the
  fallback: with any such instance in either stable table, the post-prefix
  layout must be identical.
* **Goldens.** The vm-cli golden containers were regenerated for the format
  bump, as the version check requires on every `FORMAT_VERSION` change.

### Consequences

* Good, because inserting, removing, or reordering fields of a user FB type
  now migrates per field: surviving values follow their UIDs to their new
  ordinals, and only genuinely new fields start from the candidate's init
  image.
* Good, because the fail-closed rule is preserved exactly where identity runs
  out: an unknown field UID with a changed layout rejects the candidate
  rather than risking a copy onto the wrong field.
* Good, because v5 containers degrade gracefully — no field UIDs means the
  planner falls back to the ADR-0054 rejection for layout-changing edits, so
  there is no silent behavior change for artifacts already in the field.
* Good, because the sidecar needed no format version bump: the sync, rename,
  and swap heuristics work on FB field keys unchanged.
* Bad, because field values inside a migrated instance are copied as raw
  8-byte slots, matching the VM's copy-in/copy-out; a field whose type
  changed is rejected (`IncompatibleEntry`) rather than converted — type-
  changing conversion policies are the follow-up (ADR-0060's table applies to
  variables first).
* Neutral, because UID 0 remains reserved and unassigned; an entry carrying
  it is read as "no UID", which the fail-closed rule then treats like any
  other unknown identity.

## More Information

* [ADR-0054](0054-state-migration-across-declaration-level-edits.md) — the
  migration planner and the POC FB safety rule this decision replaces for
  user-defined FB types.
* [ADR-0053](0053-stable-variable-ids-for-declaration-level-hot-edit.md) —
  the UID pattern (container sub-table, sidecar persistence) this extends to
  FB fields.
* `specs/design/bytecode-container-format.md`, "FB Field UIDs" and
  REQ-CF-container-018/030; the load-time checks in
  `compiler/container/src/load_verify.rs`; codegen emission in
  `compiler/codegen/src/compile_var_table.rs`; the planner in
  `compiler/runtime/src/migration.rs`; acceptance tests in
  `compiler/runtime/tests/migration_acceptance.rs`.
