# IoCycle-Owned Process Image with Effect-Gated Output Flush

status: accepted
date: 2026-09-29

## Context and Problem Statement

IEC 61131-3 directly-represented (located) variables — `%I` inputs, `%Q`
outputs, `%M` memory — are specified but unbuilt. The
[Runtime Execution Model](../design/runtime-execution-model.md) defines the
three regions, the per-size-prefix byte-offset formulas, the
`INPUT_FREEZE → EXECUTE → OUTPUT_FLUSH → IDLE` scan cycle, and the
located-access opcodes; the container header reserves `input_image_bytes` /
`output_image_bytes` / `memory_image_bytes`
(`compiler/container/src/header.rs:97-99`), all zeroed; the scan phases are
`// Stub` no-ops (`compiler/vm/src/vm.rs:390,443`); and anonymous located
access still fails with a `NotImplemented` diagnostic
(`compiler/codegen/src/compile_expr.rs:956`). The
[FMI co-simulation design](../design/fmi-co-simulation-support.md) records the
decision to build the process image as its phase 2.

An upstream, docs-only compiler-side design (commit `93a40ce4e`, branch
`claude/direct-variables-process-image-design`) already specifies that work
faithfully: one resolver for the named and anonymous forms, region sizing and
container population, the located-access opcode set under
[ADR-0033](0033-opcode-encoding-by-class-and-type.md), verifier bounds under
R0600, and a test matrix. Its context cites the same runtime model, header
fields, and verifier rules this repository is committed to. (Its ADR number
collides with an ADR already present here, so the commit is cited by hash and
this decision takes the next free number, 0071.)

Its runtime shape, however, contradicts our architecture:

* The upstream VM **owns** the regions and performs an **unconditional
  `OUTPUT_FLUSH`** at the end of every round. The DCS platform norm places the
  process images with the **IoCycle owner** (the process images belong to
  IoCycle; [v3 spec](../design/dcs-plc-production-platform-spec-ru.md), §7.2)
  and gives the RuntimeHost only a phase-scoped working view, returned before
  seal. VM ownership would reopen the W12/EffectGate work.
* An unconditional flush cannot express trial-run safety. Trial execution
  (`HostMode::Testing`) must refuse live application effects **by
  construction**. A boolean the VM checks at the end of a round is a
  discipline, not a mechanism: any path that forgets the check can reach the
  sink.
* The upstream design is **silent on online change**: it does not say how the
  process image relates to the existing persistent state (`swap_buffers`,
  `apply_migration_swap`, `state_snapshot`, HA crossload), yet `%M` is
  application memory that must survive a hot edit.
* It is also **silent on located-variable identity**: named located variables
  stay ordinary slots with no stable ID, so a declaration-level edit could not
  migrate their state or keep their binding under
  [ADR-0053](0053-stable-variable-ids-for-declaration-level-hot-edit.md) and
  W07/W16.

How should located variables be implemented here: adopt the upstream runtime
shape, rewrite the design from scratch, or adapt it?

## Decision Drivers

* **Safety by construction** ([ADR-0005](0005-safety-first-design-principle.md))
  — a trial run must not reach a physical sink, and the refusal must be a
  type/mode gate no call site can forget.
* **One authority per right** — IoCycle owns the images, the gate admits, the
  sink accepts, the host commits, the VM executes.
* **Hot-update bit stability** — located offsets derive from hardware
  addresses, not declaration order, so a logic-only edit must not move `%M`.
* **One migration authority** — region growth must be a class inside the
  existing migration planner, not a second planner.
* **Reuse over rewrite** — the resolver, opcode, sizing, verifier and test
  design is correct; only the ownership must change.
* **Repo standards** — requirement IDs carry crate slugs
  ([ADR-0037](0037-mandatory-crate-slug-in-requirement-ids.md)); the upstream
  `REQ-DRV-…` form is not valid here.

## Considered Options

* **Adopt upstream as-is** — the VM owns `%I`/`%Q`/`%M`, `OUTPUT_FLUSH` runs at
  the end of every round, any caller may be the I/O driver.
* **Own path from scratch** — re-derive the whole located-variable design in
  this repository, ignoring the upstream commit.
* **Adapt upstream (chosen)** — keep its compiler-side design; make the
  IoCycle owner the image owner, gate the seal, make `%M` persistent, and give
  located variables stable identity.

## Decision Outcome

Chosen: **adapt the upstream compiler-side design to IoCycle ownership with an
effect-gated output flush.** Point by point:

1. **Owner.** The IoCycle owner owns the `%I`/`%Q`/`%M` images, the output
   staging, and the input quality/age metadata. The VM extends the
   caller-owned-buffers pattern (`compiler/vm/src/buffers.rs:18-27`): it
   borrows an immutable input view and an exclusive output working view for
   the EXECUTE phase only, filling the freeze/flush seams
   (`compiler/vm/src/vm.rs:390,443`). The borrow ends before seal; the VM
   owns no region and flushes nothing.
2. **Seal.** After a successful execution unit the working view returns to
   the IoCycle owner, which hands the sealed batch to the EffectGate. The gate
   admits an `EffectTicket` (binding, mode, generation, sequence), and only
   the admitted batch reaches the sink. `HostMode::Testing` refuses by
   construction: the testing path cannot mint a ticket, so a trial run
   publishes nothing.
3. **Activation.** The input snapshot and the output staging are
   scan-transient and never cross a swap. `%M` is persistent application
   memory and joins the persistent bytes beside `vars` and `data_region` in
   `swap_buffers`, `apply_migration_swap`, `state_snapshot`
   (`compiler/runtime/src/host.rs:425-438,561-566,745-765`) and the HA
   crossload, subject to validation in
   `compiler/runtime/src/online_change.rs:37-42,86-91`.
4. **Hot-update guarantee.** Located offsets derive from hardware addresses,
   not declaration order, so a logic-only edit keeps every offset bit-stable.
   A size or offset change remains rejected by `IoIncompatible`; region growth
   becomes exactly **one** new migration-plan class inside the existing
   planner: size-equal carries the region bytes, a prefix-preserving `%M`
   growth carries the prefix and zero-fills the tail. The number of migration
   mechanisms stays one. The container format already promises this: an
   online change keeps "variable, FB instance, and process image memory
   intact"
   ([container format](../design/bytecode-container-format.md#online-change-protocol)).
5. **Identity.** Named located variables keep stable UIDs (ADR-0053): the
   address/offset is layout, the UID is identity. `StateAddressMap` (W07/W16)
   records the `UID → (region, offset, width, signedness)` binding, and `%M`
   migrates per UID like every other persistent entity.
6. **Reused from upstream, unchanged.** The resolver
   `resolve_located_access(&AddressAssignment) -> (region, byte_offset,
   width, signedness)` shared by the anonymous and named forms; the
   located-access opcode set encoded per ADR-0033; the region offset formulas;
   the R0600 verifier bounds; and the origin's test matrix (every
   `LocationPrefix × SizePrefix`, frozen `%I`, staged `%Q`, `%M` round-trip,
   anonymous / `REF` / FB-output forms, bounds). Requirement IDs are re-minted
   with crate slugs (ADR-0037) under the area code `PI` when the design is
   wired.
7. **Redesigned, deliberately.** Region ownership (IoCycle, not the VM),
   flush authority (EffectGate, not the VM), location identity (stable UIDs),
   migration of region growth (inside the existing planner), and the missing
   online-change story.

The upstream commit is credited as the origin of the compiler-side design;
[Process Image and Hot Update](../design/process-image-and-hot-update.md)
records the topic-by-topic anatomy of what is reused and what is redesigned.

## Consequences

* Good, because trial-run safety is structural: no ticket, no publication.
* Good, because logic-only hot edits keep `%M` offsets and bytes stable, and
  `%M` participates in the same swap/migration/snapshot authority as every
  other persistent region.
* Good, because region growth does not add a second migration mechanism: it
  is one class in `StateMigrationPlan`.
* Good, because the FMI co-simulation driver binds at the IoCycle provider
  boundary, not inside the VM.
* Neutral, because the container-format surface (header image fields,
  opcodes) is shared with upstream; only ownership and activation differ.
* Bad, because the upstream VM implementation cannot be merged as written:
  its region allocation, staging buffer, and unconditional flush are replaced
  by borrowed views and a gated seal.
* Bad, because the IoCycle owner, the EffectGate, and the phase-scoped view
  contract are a real interface that must exist before located variables run
  end to end.

## Pros and Cons of the Options

### Adopt upstream as-is

* Good, because no design work is repeated and the diff is the upstream
  compiler patch plus a VM region implementation.
* Bad, because it reopens W12/EffectGate: Test safety would rest on a flag
  inside the VM's flush path rather than on admission.
* Bad, because it leaves online change and located identity unspecified, so
  the first hot edit touching `%M` would regress into a restart.

### Own path from scratch

* Good, because no upstream constraint is inherited.
* Bad, because the resolver, opcode set, sizing formulas and test matrix are
  already correct and shared; re-deriving them adds no safety and loses the
  compatible format surface.

### Adapt upstream (chosen)

* Good, because the correct compiler-side work is kept and the runtime shape
  is made compatible with the platform norm.
* Good, because ownership, seal authority, identity and migration become one
  coherent story with the existing W07/W11/W12/W16 boundaries.
* Bad, because the design is split across this repository and the upstream
  commit, so the anatomy table in the design document must be kept honest.

## More Information

* [Process Image and Hot Update](../design/process-image-and-hot-update.md) —
  the adapted design: ownership boundaries, scan ordering, `%M` persistence,
  migration class, identity model, reuse/redesign matrix, evidence tests, and
  the S1–S5 implementation slices.
* [Runtime Execution Model](../design/runtime-execution-model.md) — the
  authoritative process-image, scan-phase and located-opcode spec this
  decision implements.
* [ADR-0033](0033-opcode-encoding-by-class-and-type.md) — encoding of the
  located-access opcodes; its 2026-08-31 amendment leaves one op-class slot
  free, so the family shape is an implementation decision (see the design's
  open questions).
* [ADR-0053](0053-stable-variable-ids-for-declaration-level-hot-edit.md),
  [ADR-0054](0054-state-migration-across-declaration-level-edits.md) — the
  identity and migration machinery `%M` joins.
* DCS platform dossiers:
  [W07](../implementation/dcs-platform/tasks/W07-semantic-schema.md)
  (semantic schema / `StateAddressMap`),
  [W11](../implementation/dcs-platform/tasks/W11-process-image.md)
  (process-image work package),
  [W12](../implementation/dcs-platform/tasks/W12-effects.md) (EffectGate),
  [W15](../implementation/dcs-platform/tasks/W15-single-slice.md) (end-to-end
  acceptance),
  [W16](../implementation/dcs-platform/tasks/W16-exact-activation.md)
  (exact reuse).
* [FMI Co-Simulation Support](../design/fmi-co-simulation-support.md) — the
  first driver for the process image.
