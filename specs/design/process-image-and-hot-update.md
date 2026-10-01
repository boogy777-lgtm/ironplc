# Design: Process Image and Hot Update

status: approved
date: 2026-09-29

## Overview

This design specifies how IronPLC realizes IEC 61131-3 directly-represented
(located) variables — `%I` inputs, `%Q` outputs, `%M` memory — and how the
process image behaves across online change, hot edit (Accept/Test/Untest/
Assemble), and HA crossload. It is the realization of
[ADR-0071](../adrs/0071-iocycle-owned-process-image-with-effect-gated-output-flush.md):
the IoCycle owner owns the images, the VM borrows scan-scoped views, output
publication is admitted by the EffectGate, and `%M` joins the persistent swap,
migration and crossload state.

It adapts an upstream compiler-side design (commit `93a40ce4e`, branch
`claude/direct-variables-process-image-design`): the resolver, region offset
formulas, located opcodes, verifier bounds and test matrix are reused; region
ownership, flush authority, located identity and the migration classes are
redesigned.

The design builds on:

- [ADR-0071](../adrs/0071-iocycle-owned-process-image-with-effect-gated-output-flush.md)
  — the decision this document realizes
- [Runtime Execution Model](runtime-execution-model.md) — the authoritative
  spec for the `%I`/`%Q`/`%M` regions, byte-offset formulas, the
  `INPUT_FREEZE`/`OUTPUT_FLUSH` phases, and input-snapshot / output-staging
  semantics
- [Bytecode Container Format](bytecode-container-format.md) — the reserved
  image header fields, the task-table image offsets, and the online-change
  promise that process-image memory stays intact
- [Bytecode Verifier Rules](bytecode-verifier-rules.md) — R0600 region bounds
- [ADR-0033](../adrs/0033-opcode-encoding-by-class-and-type.md) — opcode
  encoding for the located-access family
- [ADR-0053](../adrs/0053-stable-variable-ids-for-declaration-level-hot-edit.md),
  [ADR-0054](../adrs/0054-state-migration-across-declaration-level-edits.md)
  — stable UIDs and migration, which `%M` joins
- [DCS PLC Production Platform v3.0](dcs-plc-production-platform-spec-ru.md),
  §7.2 and §8 — IoCycle ownership, phase-scoped working views, effect
  admission
- [FMI Co-Simulation Support](fmi-co-simulation-support.md) — the first
  process-image driver

## 1. Upstream Origin and Current State

The upstream commit is the origin of the compiler-side design; its ADR number
is not reused here (it collides with an existing decision), so this design
cites it by commit hash. It supplies:

- one resolver,
  `resolve_located_access(&AddressAssignment) -> (region, byte_offset, width, signedness)`,
  shared by the named and anonymous forms (closing the `Variable::Direct`
  gap);
- region sizing and container header / task-offset population;
- the located-access opcode set under ADR-0033;
- analyzer type seeding and consistency checks;
- R0600 verifier bounds against the declared region sizes;
- a test matrix (prefix × size-prefix, frozen `%I`, staged `%Q`, `%M`
  round-trip, anonymous / `REF` / FB-output forms, bounds).

Facts in the current tree the work starts from:

- the image header fields are reserved and zeroed
  (`compiler/container/src/header.rs:97-99`);
- the task-table image offsets are hardcoded `0`
  (`compiler/container/src/builder.rs:423-424`);
- the scan phases are `// Stub` no-ops (`compiler/vm/src/vm.rs:390,443`);
- anonymous located access returns `NotImplemented`
  (`compiler/codegen/src/compile_expr.rs:956`);
- named located variables get ordinary variable-table slots, so they compile
  as scratch memory with no input source and no output sink.

## 2. Mechanism Anatomy: Reuse and Redesign

Every row carries a verdict: **reuse** (upstream design kept), **redesign**
(this repository's shape replaces it), or **extend** (upstream base plus a
new obligation).

| Topic | Upstream (commit `93a40ce4e`) | Ours (ADR-0071) | Verdict |
|---|---|---|---|
| Named + anonymous resolution | One `resolve_located_access(&AddressAssignment)` returning `(region, byte_offset, width, signedness)` | Same resolver, same signature | reuse |
| Region offset formulas | `X`→bit, `B`→byte, `W`×2, `D`×4, `L`×8; LSB-first bit order | Same formulas from the runtime model | reuse |
| Region sizing | Compiler computes `input_image_bytes` / `output_image_bytes` / `memory_image_bytes`; task offsets populated | Same sizing and header population | reuse |
| Opcodes | `LOAD_INPUT` / `STORE_OUTPUT` / `LOAD_MEMORY` / `STORE_MEMORY` per ADR-0033 | Same operation set; the op-class family shape is fixed in S2 under ADR-0033's one-free-slot amendment | reuse (shape open) |
| Verifier bounds | R0600 checks each access against the header region size | Same | reuse |
| Test matrix | `LocationPrefix × SizePrefix`, frozen `%I`, staged `%Q`, `%M` round-trip, anonymous / `REF` / FB-output, bounds | Same tests plus the hot-update and Trial additions in §8 | reuse + extend |
| Region ownership | The VM allocates and owns the regions and the staging buffer | The IoCycle owner owns the images, staging and quality; the VM borrows phase-scoped views (`compiler/vm/src/buffers.rs:18-27`) | redesign |
| Input freeze | The VM's `INPUT_FREEZE` copies provider inputs into its own image | IoCycle freezes at the declared boundary; provider ingress never mutates the frozen image; the VM receives an immutable input view | redesign |
| Output flush | The VM runs an unconditional `OUTPUT_FLUSH` at the end of each round | The working view returns to IoCycle at seal; the EffectGate admits the batch (EffectTicket) before the sink; `HostMode::Testing` refuses by construction | redesign |
| Online change | Silent | Input snapshot and staging are scan-transient; `%M` is persistent and joins swap, migration, snapshot and crossload | extend |
| Located identity | Silent; named variables stay slots | Named located variables keep stable UIDs (ADR-0053); the binding is recorded in `StateAddressMap`; address is layout, UID is identity | redesign |
| Region growth | Silent | One new class inside the existing `StateMigrationPlan`: size-equal carries bytes, prefix-preserving `%M` growth carries the prefix and zero-fills the tail | extend |
| Requirement IDs | `REQ-DRV-001…` (no crate slug; invalid per ADR-0037) | `REQ-PI-<crate-slug>-NNN`; registration through W04 when each slice is wired | redesign |

## 3. Ownership Boundaries

### 3.1 IoCycle Owner

The IoCycle owner is the single writer of the input snapshot, the output
staging, the image plan, and the channel quality/age metadata:

- provider ingress writes only its ingress buffer; it never mutates the
  frozen image;
- freeze produces one consistent envelope per consistency group: value,
  quality, age and generation come from the same sample version (W11-A01,
  T62/T71);
- the working output view is handed out for the EXECUTE phase only and
  returns before seal; the module replacement path invalidates quality and
  requires requalification before the next effect (W11-A02).

The image bytes are storage in the caller-owned buffer set (one allocation
site), but the lifecycle authority — when an image becomes an input and when
an output may be published — is the IoCycle owner's. This keeps the
RuntimeHost's single swap/migration/capture authority over `%M` without a
second copy of the image.

### 3.2 RuntimeHost

The RuntimeHost remains the single mutable execution owner:

- it borrows the input view and the exclusive output working view for the
  EXECUTE phase and returns them at seal; the `&mut self` idle boundary of
  the existing swap surface (`apply_state_snapshot`) stays the borrow-checker
  guarantee that no scan session is live across an activation;
- `%M` is persistent and carried beside `vars` and `data_region` in
  `swap_buffers` (`compiler/runtime/src/host.rs:561-566`), migration
  (`apply_migration_swap`, `compiler/runtime/src/host.rs:745-765`), and the
  HA snapshot pair (`state_snapshot` / `apply_state_snapshot`,
  `compiler/runtime/src/host.rs:425-438`);
- a trap during EXECUTE cancels the working batch; the previous sink state
  holds (W11-A03).

### 3.3 EffectGate and Sink

The seal path is the platform's single output admission point:

- after a successful execution unit, the IoCycle owner seals the batch and
  asks the gate to admit it;
- the gate mints an `EffectTicket` (binding, mode, generation, sequence) only
  when the current binding, mode and permits allow a live application effect;
- the sink accepts only a batch accompanied by an admitted ticket;
- `HostMode::Testing` refuses **by construction**: the testing path cannot
  produce a ticket, so no call site can flush by forgetting a check. This is
  the type-level enforcement of the v3 spec §8.4 rule that TEST forbids
  application-driven live effects.

Until the full W12 gate lands, the S3 seam uses a pass-through receiver in
Normal mode and a refusal in Testing; the ticket type is introduced with the
seam so the refusal is structural from the start.

### 3.4 VM

The VM is a pure execution kernel for located access:

- located reads execute against the borrowed immutable input view (`%I`) and
  the working `%M` view; located writes execute against the working view
  (`%Q` staged, `%M` direct);
- the VM owns no image buffer and performs no freeze or flush;
- the `INPUT_FREEZE` and `OUTPUT_FLUSH` stubs are replaced by the view
  plumbing they describe, not by region logic.

### 3.5 ExternalData Owner

Subscriptions, queues and outbound requests stay with the ExternalData owner.
Import happens at the declared boundary and never writes IEC RAM
asynchronously; outbound side effects pass the same admission contract as
`%Q` (W11 scope; full enforcement in W12).

## 4. Scan Ordering: Freeze → EXECUTE → Seal

```
IoCycle                    RuntimeHost/VM                 EffectGate        Sink
   │                             │                             │              │
   │ provider ingress            │                             │              │
   │ ── freeze (consistent       │                             │              │
   │    value+quality+age) ──────► immutable input view         │              │
   │                             │ EXECUTE (%M direct,         │              │
   │                             │  %Q → working view)         │              │
   │                             │                             │              │
   │ ◄──── working view returns at seal (success only)          │              │
   │ ──────────── sealed batch ────────────────────────────────►│              │
   │                             │                 admit?      │ ticket ─────►│
   │                             │                             │ (refuse in   │
   │                             │                             │  Testing)    │
```

1. **Freeze.** IoCycle fixes the input snapshot for the consistency group and
   the qualified ExternalData samples. Value and quality/age belong to one
   version. The RuntimeHost receives the immutable input view.
2. **EXECUTE.** The VM runs the due tasks with owned state and the exclusive
   output working view. `LOAD_INPUT` reads the frozen snapshot,
   `STORE_OUTPUT` writes the working staging, `LOAD_MEMORY`/`STORE_MEMORY`
   access `%M` directly. Published values are invisible to the driver until
   the unit completes.
3. **Trap / fault.** A trap cancels the working batch; nothing is published;
   the gate/reaction policy applies; `%M` writes already made during the unit
   follow the declared all-or-nothing unit (task group / scan) and are not
   rolled back by the sink refusal.
4. **Seal.** On success the working view returns to IoCycle, which seals the
   output batch and passes it to the EffectGate. In Normal mode the gate
   admits a ticket and the sink receives the batch; in Testing mode the gate
   refuses and the candidate run publishes nothing.
5. **Boundary.** The next swap/activation may apply only at this quiescent
   boundary (W16). The frozen input and the working staging are discarded
   across it; `%M` is not.

### Trial-run guarantee

Trial execution is a first-class host mode (`HostMode::Testing`), not a flag
on the flush path. The seal call requires an `EffectTicket` value; the gate is
the only producer, and it produces none for Testing. Assembly
(`HostMode::Normal` after Assemble) resumes publication on the next round.
This is what lets W12's later refinements (force overlay, receiver
freshness/fallback, per-output-group policy) extend the gate without touching
the VM.

## 5. `%M` Persistence, Retention and HA Crossload

- **Within a scan:** `%M` is directly addressed; no double buffering.
- **Across scans:** `%M` persists; it is application memory, not physical
  I/O, and not sourced from the freeze.
- **Across a hot-edit swap (Test/Untest):** `%M` bytes join the persistent
  carry in `swap_buffers` and `apply_migration_swap`, beside `vars` and
  `data_region`. Untest keeps the `%M` values (the code reverts, process state
  does not); only the scan-transient views are discarded.
- **Across a declaration-level migration:** `%M` migrates per stable UID like
  any other persistent entity (ADR-0054).
- **Across HA crossload:** `state_snapshot` / `apply_state_snapshot` carry
  `%M` byte-for-byte with `vars` and `data_region`, so takeover during
  Testing executes the candidate over the migrated `%M` state (ADR-0064).
- **Across a restart / A/B boot:** `%M` values survive only under the
  declared RETAIN/PERSISTENT policy (W21); a cold start re-zeroes what the
  retention contract does not declare. Live crossload is not retention.

## 6. Migration Class for Region Growth

The migration planner (`StateMigrationPlan`) stays the single migration
authority. One class is added for process-image regions:

| Active → candidate | Action |
|---|---|
| Region size equal | Carry the region bytes |
| `%M` size change with an identical prefix layout (append or truncate) | Carry `min(active_len, candidate_len)` prefix bytes; zero-fill the grown tail |
| Any other `%I`/`%Q`/`%M` size change, or an offset remap | Refuse with `IoIncompatible` |

Rules that keep the mechanism count at one:

- a candidate whose image sizes differ goes through the migration path even
  when `layout_hash` matches (anonymous located addresses can grow a region
  without changing the variable table), so the single class owns the carry
  and the zero-fill;
- `%I`/`%Q` growth is refused on the hot-edit path: the image size is part of
  the driver's copy contract and a binding change is a W10 requalification,
  not a logic-only edit;
- a declaration whose stable UID survives but whose located address moves is
  an offset change, not a rename: refuse, never rebind silently;
- the offsets of untouched located variables never move, because they derive
  from hardware addresses, not declaration order. This is the bit-stability
  guarantee behind logic-only hot edits.

## 7. Located Identity Model

- **Named located variables** (`sensor AT %IW0 : INT;`) are declaration
  entities with stable UIDs (ADR-0053), assigned and persisted by the
  engineering side. The address/offset is layout; the UID is identity.
- **`StateAddressMap`** (W07) records `UID → (region, byte_offset, width,
  signedness)` for a generation; W16 consumes it as part of exact reuse. The
  map records the binding; it never substitutes for it.
- **Anonymous located addresses** (`x := %IW0;`) are layout only: no UID, no
  state, nothing to migrate.
- A shared UID whose binding changed is refused (§6), so a value can never
  land at a different hardware address through a rename-like edit.
- `%M` values move with their UID through swap, migration, snapshot and
  crossload; addresses remain a decode-time property of each container.

## 8. Evidence Test Matrix

The upstream matrix is kept; the rows marked "ours" are added. Each row names
the owning crate that will carry the `#[spec_test]` annotation when the slice
is wired (W04).

| # | Claim | Origin | Owner |
|---|---|---|---|
| 1 | Every `LocationPrefix × SizePrefix` resolves to the correct `(region, offset, width, signedness)` | upstream | codegen |
| 2 | Anonymous `%IW0` and named `AT %IW0` share one resolver | upstream | codegen |
| 3 | Frozen `%I`: a provider write during EXECUTE is not observed until the next freeze | upstream | vm |
| 4 | Staged `%Q`: a `STORE_OUTPUT` is invisible to the sink before seal | upstream | vm |
| 5 | `%M` round-trip within a scan and across scans | upstream | vm |
| 6 | Anonymous / `REF()` / FB-output located targets compile and run | upstream | codegen |
| 7 | R0600: an access beyond the declared region size is rejected at load | upstream | container |
| 8 | Candidate under Test: an EXECUTE that writes `%Q` publishes nothing (no ticket) | ours | runtime |
| 9 | Assemble → outputs flow: after Assemble the next round's batch reaches the sink | ours | runtime |
| 10 | Untest keeps `%M`: Test→Untest preserves `%M` bytes; snapshot/staging are discarded | ours | runtime |
| 11 | `%M` survives A/B via RETAIN policy only: a cold boot keeps `%M` only per W21; crossload carries it regardless | ours | runtime |
| 12 | `IoIncompatible` on a non-prefix-preserving size or offset edit | ours | runtime |
| 13 | Migration append class: prefix-preserving `%M` growth carries the prefix and zero-fills the tail | ours | runtime |
| 14 | HA crossload: `state_snapshot`/`apply_state_snapshot` carry `%M` byte-for-byte across takeover | ours | runtime |

## 9. Implementation Slices S1–S5

Each slice is independently testable and lands as its own scoped change with
its own PR. Module placement follows the
[DCS modular framework](../implementation/dcs-platform/02-modular-framework.md)
("Controller owners": `compiler/controller`, modules first); the VM seam stays
in `compiler/vm`.

| Slice | Scope | Owner paths | Gate |
|---|---|---|---|
| **S1** Resolver + codegen | `resolve_located_access` for both forms; analyzer type seeding and prefix/type consistency; region sizing and header emission; task image offsets; plc2plc `%…` round-trip. Closes the `Variable::Direct` gap at `compile_expr.rs:956`. No runtime change. | `compiler/analyzer/src/xform_resolve_expr_types.rs`, `compiler/analyzer/src/xform_resolve_late_bound_expr_kind.rs`; `compiler/codegen/src/compile_expr.rs`, `compiler/codegen/src/compile_fn.rs`; `compiler/container/src/{header,builder}.rs`; `compiler/plc2plc` | `cargo test -p ironplc-codegen -p ironplc-analyzer -p ironplc-container -p ironplc-plc2plc` |
| **S2** Regions + loaned views | IoCycle module: image storage sized from the container header, freeze, staging, quality; located opcodes against borrowed views; `vm.rs:390,443` seams filled. `%M` storage joins the persistent buffer set. | `compiler/controller/src/io/*` (new module); `compiler/vm/src/{vm,buffers}.rs`; `compiler/container/src/opcode.rs` | `cargo test -p ironplc-vm -p ironplc-container -p ironplc-controller` |
| **S3** EffectGate seal seam | Sealed-batch contract; `EffectTicket` value; `HostMode::Testing` refusal by construction; pass-through receiver in Normal mode until W12. | `compiler/controller/src/io/gate.rs`; `compiler/runtime/src/host.rs` | `cargo test -p ironplc-runtime -p ironplc-controller` |
| **S4** `%M` persistence + migration + crossload | `%M` beside `vars`/`data_region` in `swap_buffers`, `apply_migration_swap`, `state_snapshot`/`apply_state_snapshot`; the one region-growth class in `StateMigrationPlan`; validation admits the class and keeps `IoIncompatible` otherwise. | `compiler/runtime/src/{host,online_change,migration}.rs` | `cargo test -p ironplc-runtime` |
| **S5** End-to-end matrix + W15 slice | Every row of §8 as an executable test, including the HA pair crossload; the W15 acceptance slice (source → verified artifact → input → scan → accepted output → fault fallback). | `compiler/vm/tests/*`, `compiler/vm-cli/tests/ha_pair.rs`, `tests/e2e/*` | `cd compiler && just` |

Every slice runs `cd compiler && just format` and its focused test command
while in progress; the full `cd compiler && just` gate runs before the slice's
PR. S2 owns the ADR-0033 op-class decision (its amendment leaves one free
slot, so the four located operations fold into one encountered family shape;
`encoding_when_op_class_census_taken_then_one_slot_free` pins the census).

Requirement registration: this document is not yet listed by any `build.rs`.
Each slice registers its `REQ-PI-<slug>-NNN` markers through W04 — the
generator panics on an unslugged or unclaimed marker, so registration and the
first `#[spec_test]` annotation land together.

## 10. Open Questions

1. **Origin Q1 — size prefix vs declared type** (`AT %IW0 : BOOL` against a
   `W` prefix). Relevance: active in S1. Adopt the upstream recommendation:
   the analyzer validates consistency and rejects a mismatch; the declared
   type never silently wins.
2. **Origin Q2 — `%M` aliasing/overlap** (`%MW0` and `%MB1`). Relevance:
   active in S1. Adopt the upstream recommendation: overlap is a documented
   IEC feature; the verifier bounds accesses against region size only.
3. **Origin Q3 — anonymous address without a size prefix** (`%I0`).
   Relevance: active in S1. Adopt the upstream recommendation: a size prefix
   is required for anonymous inline use; error otherwise.
4. **Origin Q4 — bit-addressed `%Q`/`%M` writes.** Relevance: active in
   S1/S2. Adopt the upstream recommendation: reuse the existing bit-access
   read-modify-write machinery.
5. **Origin Q5 — verifier region-size source.** Relevance: resolved by
   adoption — the verifier reads the header fields S1 populates.
6. **Op-class family shape.** ADR-0033's amendment leaves one free op class;
   four independent classes exceed the budget. S2 folds the located family
   behind one class with a sub-opcode, or gains a reviewed census change —
   the census test forces the choice into the open.
7. **`%I`/`%Q` growth policy.** Refused on the logic-only path in this
   design (binding change). Whether W10's requalification ever admits an
   explicit rebind with a grown image is left to W10.
8. **REQ-ID renumbering.** The upstream `REQ-DRV-…` IDs are invalid here
   (ADR-0037). This design uses `REQ-PI-<slug>-NNN`; the slices register them
   through W04. Numbers are never reused.
9. **FMI scheduling.** The FMI shim is the first driver at the S2 provider
   boundary; its implementation order follows the FMI design's phase 2 and
   does not gate S1.

## More Information

- [ADR-0071](../adrs/0071-iocycle-owned-process-image-with-effect-gated-output-flush.md)
  — the decision this design realizes.
- DCS work packages:
  [W07](../implementation/dcs-platform/tasks/W07-semantic-schema.md),
  [W11](../implementation/dcs-platform/tasks/W11-process-image.md),
  [W12](../implementation/dcs-platform/tasks/W12-effects.md),
  [W15](../implementation/dcs-platform/tasks/W15-single-slice.md),
  [W16](../implementation/dcs-platform/tasks/W16-exact-activation.md).
