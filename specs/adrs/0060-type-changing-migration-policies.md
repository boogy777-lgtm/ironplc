# Type-Changing Migration Policies for Online Change

status: accepted
date: 2026-09-15

## Context and Problem Statement

ADR-0054's migration planner carries a persistent variable's value across a
declaration-level edit only when the variable table entries bound to the
shared stable UID are identical: any entry difference rejected the whole
candidate with `MigrationError::IncompatibleEntry`, and ADR-0059 kept the
same rule for FB instance fields. A type change — the most common
declaration edit after a rename — therefore forced a full stop-load-start
even when the conversion is safe and value-preserving in practice, such as
`DINT` -> `LINT` or `DINT` -> `REAL`.

## Decision Drivers

* The value, not the name, is what the edit must preserve: a widening
  conversion (`INT` -> `DINT`, `DINT` -> `REAL`) keeps the running value
  meaningful, so rejecting it costs the operator a restart that buys
  nothing.
* Fail closed, as in ADR-0054 and ADR-0059: a conversion that can lose or
  redefine a value (narrowing, signedness changes) must reject the
  candidate, never guess.
* The decision must be provable from the container alone, at stage time —
  the same evidence the per-variable copy decisions use.
* No container format change: the variable table's storage-class entries
  (ADR-0054's input) already carry everything the policy needs.

## Considered Options

* **Keep rejecting every retype (status quo).** Safe, but a widening edit
  stops the PLC for a conversion the runtime could have proven sound.
* **Policy table of admitted conversions (chosen).** A fixed table of
  `(base, candidate)` storage-class pairs — widening and same-family only.
  Pairs outside the table reject at stage time with the pair named.
* **Full IEC 61131-3 conversion semantics.** Rejected: implicit-conversion
  rules admit narrowing and signedness changes whose semantics (overflow,
  rounding) are policy decisions this runtime has not taken; the table
  admits only what it can justify.

## Decision Outcome

Chosen option: **policy table**, evaluated where ADR-0054's planner already
classifies a shared UID whose entries differ, with the conversion executed
at the scan boundary.

* **The table** (storage-class granularity, because the variable table
  cannot distinguish `SINT` from `DINT`): `I32` -> `I64` and `U32` -> `U64`
  (integer widening within the signed and unsigned families),
  `I32` -> `F32`, `I32` -> `F64`, and `I64` -> `F64` (signed integer to
  real), and `F32` -> `F64` (real widening). Within-class widenings such as
  `INT` -> `DINT` change no `VarEntry` at all, so the layout hash stays
  equal and no migration is involved — the ordinary online change carries
  the sign-extended slot.
* **Classification.** A shared UID whose entries differ only in `var_type`
  is looked up in the table. An admitted scalar plans a `Convert` action;
  an admitted array element pair plans a `ConvertArray` action, but only
  when the descriptors' element counts and per-element sizes are equal.
  Strings migrate only within the same width and maximum length (a
  same-size copy); a width or length change rejects. Everything else —
  narrowing, signedness changes, `TIME`, strings of a different size, FB
  instances — rejects the whole candidate at stage time with
  `MigrationError::TypeChangeUnsupported` naming the pair, surfaced through
  the existing `MigrationUnsupported` (V4010) path.
* **Execution.** The conversion runs where every migration copy runs: in
  the planner's `apply`, at the scan boundary, reading the base slot
  through the accessor matching the base VM's storage convention and
  storing the target the candidate's VM will use. The policy table and the
  conversions live beside the planner in the runtime crate
  (`compiler/runtime/src/conversion.rs`); no new dependencies.
* **FB fields.** A shared field UID whose entry differs still rejects, as
  ADR-0059 specified; applying the policy table to fields is follow-up.

### Consequences

* Good, because the common widening edits (`DINT` -> `LINT`,
  `DINT` -> `REAL`, `INT` -> `DINT`) now migrate their running values
  instead of forcing a restart.
* Good, because the policy is a closed, enumerable table: a pair it does
  not name is rejected with the pair named, so the fail-closed guarantee
  of ADR-0054 is preserved exactly where the policy runs out.
* Bad, because narrowing edits (for example `LREAL` -> `REAL`) still
  require a stop-load-start; the policy could be extended, but each new
  pair is a rounding/overflow decision that deserves its own scrutiny.
* Neutral, because FB instance field retypes keep the ADR-0059 rejection:
  the table applies to variables first, and the per-field path extends the
  same way when needed.

## More Information

* [ADR-0054](0054-state-migration-across-declaration-level-edits.md) — the
  migration planner this decision extends; its copy/init/drop
  classification and V4010 rejection path are unchanged.
* [ADR-0059](0059-fb-field-stable-ids.md) — the per-field FB instance
  migration whose retyped-field rejection this decision leaves in place.
* `specs/design/bytecode-container-format.md`, "Layout Hash and Online
  Change" — the container-side contract for type-changing migration; the
  policy table in `compiler/runtime/src/conversion.rs`; the planner in
  `compiler/runtime/src/migration.rs`; acceptance tests in
  `compiler/runtime/tests/migration_acceptance.rs`.
