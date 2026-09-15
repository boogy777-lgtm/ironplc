# Hot Edit Stage 2: Variable Migration via Stable IDs (A/B/C Arenas)

## Goal

Extend the Stage 1 online change
(`2026-09-15-hot-edit-p0-online-change.md`) from logic-only edits to
declaration edits whose semantic identity is preserved:

- rename, including a name swap between two variables, keeps values;
- add/remove variables migrates every other variable value;
- type/layout-changing edits reject with a typed error;
- candidate state is staged in separate arenas (A active, B candidate,
  C undo) and committed atomically at the scan boundary.

## Background

Stage 1 follows the container spec's all-or-nothing rule: any structural
change invalidates `layout_hash` and is rejected
(`specs/design/bytecode-container-format.md:631`). The spec names this stage
as its intended successor: per-variable migration "can be added later if
needed by extending the type section with optional name metadata" (`:697`).

Rename cannot be inferred from source text alone:

- a same-type name swap is textually identical to a declaration reorder
  (values stay with names vs values swap entities are both valid readings);
- a diff-type name swap is textually identical to two retypes.

Identity therefore must be an explicit model property: an entity UID
assigned when the declaration is created and surviving renames; the name is
only a binding. The user-visible ambiguity (which edit intent was meant) is
resolved by the engineering environment during the Pending phase, not by the
runtime.

The runtime has the mechanism: `VmBuffers` are caller-owned
(`compiler/vm/src/buffers.rs:18`), so multiple state arenas can be alive;
swapping arenas is a host decision between rounds
(`compiler/vm/src/vm.rs:50`, `:241`).

## Architecture

### 1. StableVarId (UID) in the container

- New optional type-section sub-table `stable_vars`: for each persistent
  variable (globals, program variables, FB instance fields): var_index u16
  and uid u64. Transient slots (function locals, scratch) get no UID.
- UID source: the engineering project model, created on declaration
  creation, updated by explicit rename/swap refactor operations. For this
  stage the compiler accepts an optional UID table input (name -> uid) on
  the codegen/project API; IDE sidecar storage and the refactor commands
  come later.
- UIDs are intentionally excluded from `layout_hash`: a rename must not look
  like a layout change. Migration compatibility is decided by the planner
  below, not by a single hash.
- Decide in T1 (ADR 0053): separate sub-table vs extending `VarEntry`;
  UID width/format (u64 counter vs UUID) including merge behavior.

### 2. Migration planner (`runtime/src/migration.rs`)

Inputs: base container + candidate container, both with `stable_vars`.
Outputs a `StateMigrationPlan`:

| Case | Action |
|---|---|
| UID in both, identical `var_type`/`extra`/layout | Copy value (slot and data-region bytes) |
| UID only in candidate | Initialise from the candidate's init image |
| UID only in base | Drop (recorded for the undo arena) |
| UID in both, different type/layout | Reject |

- Strings: copy header + payload; reject when the candidate max length is
  smaller than the current length.
- FB instances: identity is the instance UID; field migration uses the FB
  type descriptor. An FB type layout change is rejected unless identical
  (growth by appending fields is a follow-up, not this stage).
- Arrays: copy the raw element range when `element_type`, `total_elements`
  and `extra` are identical; otherwise reject.

### 3. A/B/C arenas and commit

- A = active buffers, B = candidate buffers built by the planner at stage
  time (outside the scan), C = spare/undo.
- At the scan boundary the host swaps to B; A is retained as the undo
  arena for Untest; after Assemble, A is released back to C.
- Untest policy (ADR 0053): a schema-changing Test may not Untest, because
  writes made under the candidate layout have no defined reverse mapping
  for added/removed variables. Allowed exits are Assemble or Cancel while
  the original is active. This is a typed error, not a silent state reset.
- Initial values for new variables are applied from the candidate's init
  image at migration time; the running application's init is not re-run.

### 4. Semantics to pin with tests

1. same-type name swap via UID rebinding: values swap bindings, zero data
   movement, no layout change;
2. diff-type name swap: types stay with entities, zero data movement, code
   type-checks against the new bindings;
3. reorder declarations: values follow UIDs, not positions;
4. add a variable: existing values preserved, new variable initialised;
5. remove a variable: remaining values preserved, dropped value not leaked
   into another slot;
6. delete B + rename A->B: UID2 removed, UID1 renamed, values follow the
   rules above;
7. reject: retype, array bound change, FB field change, string shrink.

## Prefactoring

Stage 1 already introduces the Variable Table and per-index type rows; this
stage reuses that collection path for the UID table. No separate prefactor
is planned. If `migration.rs` grows past 1000 lines, split the per-type
walkers (string/FB/array) into focused modules.

## File Map

- `specs/design/bytecode-container-format.md` — `stable_vars` sub-table,
  migration compatibility rules, per-variable migration status.
- `compiler/container/src/type_section.rs`, `builder.rs`,
  `container_ref.rs` — UID sub-table (de)serialize.
- `compiler/codegen/src/compile*.rs` — UID table input plumbing and
  emission alongside the Variable Table.
- `compiler/project/src/compile.rs` — optional UID table parameter.
- `compiler/runtime/src/migration.rs`, `arenas.rs`, `online_change.rs` —
  planner, A/B/C commit, Untest policy.
- Tests in `compiler/runtime/tests/` for the seven cases above.
- `specs/adrs/0053-*.md` — stable IDs, migration policy, schema-edit
  Untest policy.

## Tasks

- [ ] T1 spec + container UID sub-table + roundtrip tests + ADR 0053
      (representation decision).
- [ ] T2 codegen/project UID plumbing and emission; UIDs identical for two
      recompiles with the same table, renames keep UIDs.
- [ ] T3 planner + arenas + acceptance tests 1-7; Untest policy error.
- [ ] T4 spec/status updates; `cd compiler && just` full gate.

## Not in Scope

- IDE rename/swap refactor commands and the Pending-phase confirmation UX
  for ambiguous raw-text edits.
- Type-changing migration (INT -> DINT and similar) - rejected for now.
- FB type evolution beyond byte-identical layouts.
- Redundancy: generations and UIDs are the prerequisite for the HA
  crossload model, but distributed commit stays out of this stage.

## Open Decisions

- UID representation and storage (ADR 0053, T1).
- Whether new-variable initialisation reads the init-image constant pool or
  runs a filtered candidate init function (T3).
