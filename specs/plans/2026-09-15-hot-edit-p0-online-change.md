# Hot Edit P0: Variable Table, Layout Hash, Online Change Host

## Goal

Deliver the online change protocol already specified in
`specs/design/bytecode-container-format.md` (Layout Hash and Online Change,
:641) as a working, tested POC:

- codegen emits a real Variable Table and a real `layout_hash`;
- a runtime host stages a candidate container, validates it, and atomically
  swaps code at a scan boundary while all IEC state survives.

Acceptance is the P0 acceptance test from the POC handoff
(`docs/reference/Rnd_Rockwell/ironplc_hot_edit_poc_handoff.md:597`) plus the
four P0 tests from the accepted baseline
(`docs/reference/Rnd_Rockwell/ironplc_hot_edit_redundancy_architecture.md:1277`):

1. Program variable survives a logic-only edit (Counter=12537; `+1` becomes
   `+10`; next scan yields 12547, not 0).
2. FB instance state survives a logic-only edit.
3. A `StateAbi`-changing edit is rejected without disturbing the running
   application.
4. Untest reverts code, not process state (Counter=100, test `+10` for three
   scans =130, untest, next value is 131, not 101).

## Background

The container format spec already defines everything this plan implements:

- Deterministic ordering and the exact `layout_hash` formula
  (`specs/design/bytecode-container-format.md:615-663`).
- The Variable Table the hash is defined over, marked "planned, not emitted"
  (`:232`) because the ADR-0006 verifier does not exist yet.
- The online change protocol: compare hashes, swap code at the end of the
  scan cycle, keep variable/FB/process-image memory intact (`:668-686`).

Today `layout_hash` is written as zeros; this is tracked as
`REQ-CF-codegen-025` (`specs/design/bytecode-container-format.md:118`,
ADR-0007, issue #1583).

The VM already has the seam the host needs:

- `Vm::load(container: &Container, bufs: &mut VmBuffers)`
  (`compiler/vm/src/vm.rs:50`) borrows code and keeps every mutable byte in
  caller-owned buffers (`compiler/vm/src/buffers.rs:18`).
- `VmReady::resume(scan_count)` resumes without running init functions
  (`compiler/vm/src/vm.rs:241`).
- `run_round(uptime_us)` is the scan boundary (`compiler/vm/src/vm.rs:363`);
  `vm-cli` already drives `loop { run_round }`
  (`compiler/vm-cli/src/cli.rs:59`).

Nothing in the workspace owns two containers plus their buffers, so the
swap is host-level work, not VM work.

## Architecture

### 1. Variable Table (container format)

Emit the planned sub-table into the type section, fourth and last after user
FB descriptors (`specs/design/bytecode-container-format.md:232-253`):

```
count: u16                 // must equal header.num_variables
entries: [VarEntry; count] // 4 bytes each
VarEntry = var_type u8 || flags u8 || extra u16 (LE)
```

- `flags` bit 0 = is array; `extra` = STRING/WSTRING max length, FB_INSTANCE
  fb_type_id, arrays: array descriptor index (`:245-251`).
- Update `REQ-CF-container-018` ("three sub-tables in this order", `:167`)
  to four, and the section status note (`:234`).
- `FORMAT_VERSION` 3 -> 4 (`compiler/container/src/header.rs:10`); the
  Versioning rules (`:699`) call a section-semantics change a major bump.
- Codegen collects `VarEntry` rows for every index `0..num_variables`
  (including hidden/scratch slots) while those indices are assigned; a gap
  is a codegen bug, not an SLOT guess. Audit the allocation sites:
  `assign_variables` (`compiler/codegen/src/compile_setup.rs:38`), function
  params/locals (`compile_fn.rs`), FB field regions and FB_INSTANCE entries
  (`compile.rs:863-939`), arrays/strings/structs (`compile_array*.rs`,
  `compile_string.rs`, `compile_struct.rs`), and
  `allocate_scratch_variable` (`compile.rs:1457`).

### 2. layout_hash

- Compute BLAKE3 exactly per the formula at `:650-663`: num_variables, then
  VarEntry rows in index order, then FB type descriptors (num_fields and
  per-field entries), then array descriptors. Nothing else — no offsets, no
  task table, no data-region sizes.
- Public `Container::compute_layout_hash()`; `write_to` stores it in
  `header.layout_hash` (`compiler/container/src/header.rs:51`).
  `content_hash`/`debug_hash` stay zero (issue #1583); split
  `REQ-CF-codegen-025` text so layout_hash is no longer in its scope.
- Determinism tests: same source twice -> identical hash; `Counter + 1` ->
  `Counter + 10` -> identical hash; add/remove/retype a variable, change an
  array bound, change an FB field -> different hash.

### 3. Runtime host (new crate `runtime`, package `ironplc-runtime`)

- Owns the active `Container`, staged candidates, `VmBuffers`, and two
  generations per the baseline: `LogicGeneration` (artifact) and
  `ApplicationGeneration` (the active manifest) —
  `docs/reference/Rnd_Rockwell/ironplc_hot_edit_redundancy_architecture.md:507-553`.
- Minimal Rockwell FSM for P0 (`:410-468`): `accept` (stage + validate),
  `test` (activate candidate at the next scan boundary), `untest` (revert to
  the original at the next boundary), `assemble` (candidate becomes normal),
  `cancel` (discard a staged candidate while the original is active),
  `status`.
- Swap primitive at the round boundary: end the `VmRunning` borrow, build
  candidate buffers, carry persistent state over, then
  `Vm::new().load(&candidate, &mut bufs).resume(scan_count)`.
- Buffer policy: when candidate header resource sizes exceed the active
  ones (`stack`, `temp_buf`, `data_region`, `frames`), reallocate at stage
  time and copy the persistent prefix of `vars` and `data_region`. The
  layout hash guarantees indices and persistent data offsets; transient
  bytes are not copied. This happens outside the scan, so no scan-critical
  allocation is introduced.
- Validation: `candidate.layout_hash == active.layout_hash` and task-table
  equality (a schedule change stays a cold start in P0). Rejections are a
  typed `OnlineChangeError` in the runtime crate; user-facing V-codes are
  deferred to the P0.5 CLI surface.

### 4. Acceptance tests (in the runtime crate)

1. code body edit (`+1` -> `+10`) preserves Counter (12537 -> 12547).
2. FB instance state preserves across a body edit.
3. declaration change (add/retype a variable) is rejected; the running
   application produces the same values before and after the attempt.
4. untest reverts code but not state (100 -> test +10 x3 = 130 -> untest ->
   131).
5. adding a string temp (data region growth) resizes buffers and preserves
   state.
6. scan count continues across a test; no restart, no init re-run.

## Prefactoring

None required before starting. If the per-index `VarEntry` collection pushes
`compile.rs` past 1000 lines, extract a `compile_var_table.rs` module
(entry point collects rows; allocation sites push into it).

## File Map

- `specs/design/bytecode-container-format.md` — Variable Table status,
  `REQ-CF-container-018`, `REQ-CF-codegen-025`, version history.
- `compiler/container/src/type_section.rs` — Variable Table struct,
  (de)serialize, section size.
- `compiler/container/src/container.rs`, `container_ref.rs` — parse/write
  the new sub-table on both the owned and zero-copy paths.
- `compiler/container/src/builder.rs` — variable table input API.
- `compiler/container/src/header.rs` — `FORMAT_VERSION` 4; hash storage
  already exists.
- `compiler/codegen/src/compile*.rs` — `VarEntry` collection; layout hash
  wiring; determinism tests.
- `compiler/codegen/src/spec_conformance_container_format.rs` — updated and
  new REQ tests.
- `compiler/runtime/` — new crate: `host.rs`, `online_change.rs`,
  `generation.rs`, `lib.rs`, `error.rs`; acceptance tests under `tests/`.
- `compiler/Cargo.toml` — workspace member.
- `specs/adrs/0052-*.md` — runtime host and online change protocol.

## Tasks

- [ ] T1 container: Variable Table sub-table, format v4, REQ-CF-container-018
      update, roundtrip + ContainerRef tests.
- [ ] T2 codegen: per-index `VarEntry` collection at every allocation site,
      builder wiring, entries.len() == num_variables guard test.
- [ ] T3 layout hash: computation, header write, REQ-CF-codegen-025 split,
      determinism and sensitivity tests.
- [ ] T4 runtime crate: host loop, FSM, swap and resize, acceptance tests
      1-6.
- [ ] T5 ADR 0052 + spec text updates; `cd compiler && just` full gate.

## Not in Scope

- `content_hash` / `debug_hash` (issue #1583) and signature verification.
- The verifier that consumes the Variable Table (ADR-0006).
- Engineering protocol commands over a network and the VS Code surface
  (P0.5).
- Redundancy, HA, distributed commit.
- Variable declaration edits — that is the Stage 2 plan
  (`2026-09-15-hot-edit-state-migration.md`).
