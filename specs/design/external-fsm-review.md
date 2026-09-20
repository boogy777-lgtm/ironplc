# Spec: External FSM Architecture Review

## Overview

The reviewed document is an externally produced architecture proposal,
"IronPLC: orthogonal controller lifecycle FSMs", v1.0 (2026-09-20), not an
audit of this repository: six orthogonal state machines — HwKey,
PlatformLifecycle (FreeRTOS/BSP), HwDiag, Runtime, ApplicationLifecycle,
OnlineEdit — plus ContractStore, a pure CapabilityResolver, CommandIngress,
a barrier coordinator, GenerationStore, and emergency hardware containment,
written in XState v5 semantic pseudocode with an N+1 audit, a devil's
advocate review, and acceptance scenarios T01–T20. Its assumed target is a
single-core FreeRTOS MCU with static RTOS objects, a physical mode key, BSP
containment, and an XState/TypeScript implementation; that target is not
ours today: IronPLC currently runs as a desktop and served VM process
(`ironplcvm serve`, `run`, `benchmark`), the runtime host is `std`
(`compiler/runtime/Cargo.toml` depends on `serde_json`; `host.rs` uses
`BTreeMap` and `SystemTime`), there is no OS supervisor, no key hardware,
no I/O stack, and no scheduler of our own. Only the execution kernel
(`ironplc-container`, `ironplc-vm`) is `no_std` (`compiler/container/src/lib.rs:1`,
ADR-0010), so the MCU-facing sections — FreeRTOS task and priority tables,
BSP reset/containment, hardware key authority — bind, if ever, only after a
target-side port, not in this architecture. Everything below is therefore
judged as an idea against decisions already committed here, with one test:
does it land on a prepared seam without reopening a decision.

## Triage

Verdict vocabulary: **ADOPT-NOW** (actionable for Phase 5 as written),
**ADOPT-LATER** (sound, but belongs to a deferred class or a later target),
**ALREADY-HAVE** (decided and/or implemented here; nothing to ingest),
**REJECT** (would add mechanism without a consumer, duplicate an authority,
or reopen a committed decision).

| External idea (source section) | Verdict | One-line rationale | Our evidence |
|---|---|---|---|
| Orthogonal FSMs, own state only, typed contracts, no foreign enum or state read (§1, §13, I1) | ALREADY-HAVE | Separation is physical, not conventional: the runtime crate names no role, pair, or epoch, and the redundancy layer sits above it with a one-way dependency. | [HA Redundancy Layer Architecture](ha-redundancy-layer-architecture.md), Design Goals and Crate Placement; [HA Redundancy FSM](ha-redundancy-fsm.md), configured role vs. runtime state |
| Dependency audit table: owns / reads / publishes / must-not-know (§13) | ALREADY-HAVE | It reaches our n+1 verdict and module decomposition; the audit method adds no decision. | [HA Redundancy Layer Architecture](ha-redundancy-layer-architecture.md), "Is the Architecture Modular?"; [HA Architecture Readiness](ha-architecture-readiness.md), readiness conditions 1–3 |
| ContractStore: per-producer snapshots, revisions, `boot_id`, TTL, dedup, schema version (§1, §3.1–3.2) | REJECT | A new state authority with no consumer here: the host already answers authoritative pull snapshots, and the peer contract it would serve is the ping/pong packet already specified. | `StatusPayload` / `identity` pull snapshot (`compiler/runtime/src/commands.rs:159`, `:251`); ping/pong fields `pair_id`/`epoch`/`generations`/sequences/`crc` ([HA Redundancy FSM](ha-redundancy-fsm.md), Ping/Pong Liveness); ADR-0055 adds no protocol state |
| Pure CapabilityResolver composing facts into permits, versioned policy (§1, §3.3) | REJECT | A general policy engine for a single gate; the decided shape is a policy/mechanism split where the host enforces one permit latch, not a facts engine. | [HA Redundancy Layer Architecture](ha-redundancy-layer-architecture.md), "Shell, Not Runtime +1" and "Minimal Seams" 1 |
| Permit TTL / `valid_until` freshness (§3.1–3.2) | REJECT | Time-based validity needs a monotonic clock and measured producer cadence our targets do not have; freshness is the boundary commit itself. | Boundary commit (`compiler/runtime/src/host.rs:383`, `:430`); ADR-0062 (measured terms, not assumed constants) |
| Re-validate permits at commit: a permit revoked after admission cancels the commit (§3.1, §10.2 step 6, I10) | ADOPT-NOW | The one habit worth keeping; it lands on the already-designed execution-permit seam and needs no new layer. | [HA Redundancy Layer Architecture](ha-redundancy-layer-architecture.md), "Minimal Seams" 1 (permit latch); the host boundary is the single re-check point |
| Facts, not enums: consumers see capabilities/contracts, never foreign state enums (§2, §13, I4–I5) | ALREADY-HAVE | The runtime stores no foreign enum; HA observability is an additive block a server composes, never a control input. | `RedundancyIdentity` optional block, "shape only today" (`compiler/runtime/src/commands.rs:231`); dependency direction redundancy → runtime |
| RuntimeFSM `PROGRAM`/`RUN`; `TEST` belongs to OnlineEditFSM; committed mode changes only at safe point (§2, §7) | REJECT (mode) / ALREADY-HAVE (commit discipline) | Our host is driven by rounds, not a mode owner; `PROGRAM`/`RUN` would add a second execution authority and reopen ADR-0052/0055, while the prescribed safe-point commit is exactly our pending-swap latch. | `HostMode` is Normal/Testing (`compiler/runtime/src/host.rs:51`); pending swap applied at the boundary (`:146`, `:430`); ADR-0064 keeps edit, delivery, selection, and commit separate |
| OnlineEditFSM lifecycle: `IDLE`/`PREPARING`/`STAGED`/`SWITCH_PENDING`/`TESTING`/`FINALIZING`/`DISCARDING` (§9) | ALREADY-HAVE | Maps one-to-one onto Accepted → Testing with Untest/Cancel/Assemble, assemble-requires-Test, and no state rollback; the extra state is receipt reconciliation only. | ADR-0064 decisions 1–4; host `stage`/`test`/`untest`/`assemble`/`cancel` (`compiler/runtime/src/host.rs:209`, `:260`, `:280`, `:305`, `:332`); V4017 pending |
| `SWITCH_PENDING` + `BindingCommand`/`BindingReceipt`; pending until reconciliation after a lost receipt (§9, §10.2, E2) | ADOPT-LATER (pair) | On one unit the serving shell drives the boundary round before the ack, so a pending-binding state would be protocol state ADR-0055 refuses; the pair transaction is where receipt loss is real. | Boundary round before ack (`compiler/vm-cli/src/serve.rs:195`, `:416`); ADR-0064(h) and its amendment item 5 (durable-pending substate named open) |
| Two code banks ping-pong; state arena; exact-match reuse by `StableStateId` and semantic type (§9.1) | ALREADY-HAVE | Normal/candidate plus caller-owned `VmBuffers` and the persistent-prefix carry are the same model; exact match is the layout-equality path, migration the schema-change path. | `compiler/runtime/src/host.rs:133`, `:209`; `compiler/runtime/src/online_change.rs:159`; `compiler/vm/src/buffers.rs:18`; ADR-0053, ADR-0059 |
| Untest returns the current state, never the pre-Test state; no untest after a schema change (§9.1, E5) | ALREADY-HAVE | The host refuses untest on a migration candidate and untest never restores bytes; state never rolls back. | `compiler/runtime/src/host.rs:280` (`UntestUnsupported`); ADR-0052 baseline invariant; V4011 |
| Pinning, generation lifetime, release proof before free, no TTL free, no force-free (§9.1, §16, T12) | ALREADY-HAVE (stronger) | Rust ownership plus the VM typestate borrow makes free-while-in-flight unrepresentable — stronger than a refcount/lease; flash durability has a never-both-invalid invariant. | ADR-0009 and ADR-0052 (typestate borrow); ADR-0064 amendment, durability invariant and crash table |
| `MigrationArena` + `MigrationPlan`, NEW state gets defaults before the offer; explicit conversion policy (§9.1) | ALREADY-HAVE | Buffers are rebuilt from the candidate init image and the plan applies admitted conversions or engineer decisions at the boundary. | `compiler/runtime/src/host.rs:462` (`apply_migration_swap`); ADR-0054, ADR-0060, ADR-0061 |
| Online structural migration needs a consistent checkpoint + journal + bounded catch-up, and must refuse when residual exceeds the budget (§9.1, §16, T10) | ADOPT-LATER | Our migration is synchronous inside the boundary, so no shadow goes stale and no replay exists; the bound it demands is a pause measurement we owe, while journal/catch-up is deferred multi-task work. | `compiler/runtime/src/host.rs:462`; roadmap "Deferred" (multi-task ExecutionFrontier, external-protocol replay); ADR-0062 measurement philosophy |
| Safe point defined as: no new jobs, jobs finished, no descriptor references, state writes done, output frame epoch, DMA excluded (§10.1) | ALREADY-HAVE (single-task) / ADOPT-LATER (multi-task) | One borrowed VM session plus a boundary swap gives the single-task safe point; a domain rendezvous is exactly the deferred multi-task work. | `run_session`/`apply_pending_swap` (`compiler/runtime/src/host.rs:399`, `:430`); roadmap "Deferred" multi-task |
| One mode/binding operation per barrier; priority emergency → fault → stop → PROGRAM → RUN → edit; equal priority by ingress order (§10.2–10.3, T20) | ALREADY-HAVE (latch) / ADOPT-LATER (table) | One pending-swap slot plus fail-closed refusals makes concurrent operations impossible today; an explicit priority table has no RUN/STOP owner yet and belongs with the HA stop/handover path. | `pending: Option<PendingSwap>` (`compiler/runtime/src/host.rs:146`); V4015; [HA Redundancy FSM](ha-redundancy-fsm.md) invariants (no automatic RUN) |
| Command receipts with `request_id`, deadlines, dedup, backpressure, reconciliation after a lost receipt (§3.1, §10.2, R6, T11, T14) | REJECT (standalone) / ADOPT-LATER (pair) | Reliable ordered transports plus the authoritative status snapshot already reconcile; request ids and journals are protocol state ADR-0055 deliberately does not have. The pair transaction is the one place a lost receipt matters. | ADR-0063 (stdio/TCP one protocol); ADR-0055 (no layer state); reconciliation via `identity`/`getStatus` ([Engineering Connection](engineering-connection.md), Trial state); ADR-0064(h) |
| Output ownership epoch; execution and output as separate permits; publish checks sink, generation, epoch (§3.2–3.3, §15, I15) | ALREADY-HAVE (design) | Epoch dominates generation, ownership is the fencing truth, and publishing requires ACTIVE plus all required I/O owned; a separate `OutputPermit` type adds nothing to the decided statechart. | ADR-0062 (epoch anti-stale only, OwnerLease at scan commit); [HA Redundancy FSM](ha-redundancy-fsm.md) invariants (`CAN_EXECUTE_OUTPUTS`, partial ownership != ACTIVE); roadmap quorum layer 3 |
| Emergency hardware containment independent of software quiescence (§10.4, §16, I14) | REJECT now / ADOPT-LATER target-side | No hardware exists in the served-VM target; the decided equivalent is the I/O module's autonomous watchdog and safe-state policy, which must never depend on PLC software. | Roadmap I/O firmware profiles (IRONPLC_HA_IO: autonomous owner watchdog, `SAFE_VALUE`/`HOLD_LAST`/`RAMP_TO_SAFE`) |
| Timeout != rollback; no fabricated committed; recovery never auto-RUNs (§7, §16, R6, I9–I11, E3) | ALREADY-HAVE | V4011 forbids untest after a schema change, boot adoption boots only verified bytes, and the HA FSM has no automatic RUN or takeover from silence. | `compiler/runtime/src/host.rs:280`; ADR-0064 amendment boot adoption; [HA Redundancy FSM](ha-redundancy-fsm.md) invariants |
| N+1 audit; redundancy is a new mechanism class, ownership/replication a new responsibility (§15) | ALREADY-HAVE | Same conclusion, proven earlier: a new crate above the runtime, no role enum inside it. | [HA Redundancy Layer Architecture](ha-redundancy-layer-architecture.md), n+1 verdict and module list |
| Truth table: `!valid(L1) && !valid(L2) → no automatic takeover`; one lost channel is degraded transport (§15) | ALREADY-HAVE | Our detection case table reaches the same result by construction: silence makes a claimant, the I/O target's arbitration is the final fence, failure lands in REDUNDANCY_LOST. | [HA Redundancy FSM](ha-redundancy-fsm.md), Detection Case Table and "Silence makes a claimant, never an owner" |
| Acceptance scenarios T01–T20 (§17) | MIXED | Eight are worth adding to the Phase 5 matrix, six are already covered, six are target-side or deferred by roadmap; mapping below. | Acceptance mapping below |
| XState v5 semantic model as the working model (§12) | REJECT | Not our stack; it duplicates the normative tables the document itself declares authoritative (DRY) and would add an actor runtime and a new execution abstraction. | ADR-0052; our FSMs are Rust enums/typestates with table-driven tests; workspace doctrine on new abstractions |
| FreeRTOS task/priority tables, static allocation, primitives, timing formulas (§11) | REJECT now / ADOPT-LATER at an MCU port | Target-side scheduling with no consumer in a served-VM deployment; our timing commitment is already measured, not tabulated. | ADR-0062; `std` runtime host (`compiler/runtime/Cargo.toml`, `compiler/runtime/src/host.rs:38`); no_std is container/vm only (`compiler/container/src/lib.rs:1`, ADR-0010) |
| HwKeyFSM states and the position-grant table (§4) | REJECT now | No key hardware or requirement; roles are configured, and authentication/identity are explicitly deferred — a key profile would be a new class without an owner. | [HA Redundancy FSM](ha-redundancy-fsm.md), configured role; ADR-0063 (auth deferred); ADR-0065 (no engineer identity) |
| PlatformLifecycleFSM and HwDiagFSM with fault records and latches (§5–6) | REJECT now / ADOPT-LATER at an MCU port | BSP/RTOS/hardware diagnostics are outside every current target; the only lifecycle we own — boot adoption of verified artifacts — exists. | ADR-0064 amendment boot adoption; `compiler/vm-cli/src/slot_store.rs` |
| ApplicationLifecycleFSM with `GenerationOffer`/availability separate from runtime (§8, A1–A5) | ALREADY-HAVE (substance) | The host plus the A/B store already own load, validate, commit, and adopt; a second lifecycle model for one application would duplicate it. | Host stage/assemble (`compiler/runtime/src/host.rs:209`, `:305`); `compiler/vm-cli/src/slot_store.rs:89`; ADR-0064 amendment |
| Contract envelope `boot_id`/`sequence`/`valid_until` for peer facts and producer restart (§3.1, T13) | ALREADY-HAVE (design) | The pair contract is the ping/pong packet (pair id, epoch, generations, per-channel sequences, crc), and boot-epoch anti-stale is the decided epoch rule. | [HA Redundancy FSM](ha-redundancy-fsm.md), Ping/Pong Liveness; ADR-0062 (epoch rejects stale, never arbitrates) |

## Acceptance Scenario Mapping (T01–T20)

"COVERED" means an existing decision or test already answers the scenario;
"ADD" means it is worth an explicit Phase 5 (HA) acceptance test, per the
takeaways below; "N/A" and "DEFERRED" mean the subsystem does not exist.

| Scenario (§17) | Status | Anchor / reason |
|---|---|---|
| T01 cold boot with no application | COVERED | Boot adoption boots nothing verifiable as an honest empty device (ADR-0064 amendment item 3d); slot-store boot tests (`compiler/vm-cli/src/slot_store.rs`) |
| T02 key bounces between codes | N/A | No key subsystem; roles are configured ([HA Redundancy FSM](ha-redundancy-fsm.md)) |
| T03 scheduler starts, a required task does not ACK | DEFERRED | No scheduler ACK; multi-task ExecutionFrontier is roadmap-deferred |
| T04 optional PHY lost | DEFERRED | No NIC/HAL; Phase 5 gap list ([HA Architecture Readiness](ha-architecture-readiness.md)) |
| T05 remote RUN + key PROGRAM + HW inhibit in one cut | ADD | Permit latch plus priority; lands with the permit seam ([HA Redundancy Layer Architecture](ha-redundancy-layer-architecture.md), "Minimal Seams" 1) |
| T06 hardware fatal mid-job | N/A | Target-side; I/O module autonomous watchdog and safe-state policy (roadmap I/O firmware profiles) |
| T07 permit expires after admission before the barrier | ADD | Commit-time re-validation on the permit seam; takeaway 1 |
| T08 second job still running, root must not switch | DEFERRED | Multi-task; the single-task boundary case is covered by host tests |
| T09 exact-match Test/Untest switches code root, state root stays | ADD | ADR-0064(d) "no state copy"; add the pair acceptance test |
| T10 structural migration journal does not finish | ADD | Measure the synchronous migration pause and refuse when out of budget; takeaway 3 |
| T11 binding receipt lost after commit | ADD | Pair assemble transaction reconciled from authoritative status; ADR-0064(h), amendment item 5 |
| T12 unload timeout with a live reference | COVERED | The borrow/ownership model makes force-free unrepresentable (ADR-0009, ADR-0052); pair generation pinning arrives with crossload |
| T13 producer restart with `sequence=0` | ADD | Epoch anti-stale (ADR-0062); peer restart test |
| T14 command flood and a full diagnostics queue | REJECT now | No queues or TTL machinery; would need the rejected ContractStore; revisit only with an event stream |
| T15 fault clear / REMOTE after PROGRAM does not auto-RUN | COVERED (invariant) | [HA Redundancy FSM](ha-redundancy-fsm.md) invariants; make it an explicit test when admission lands |
| T16 simulation RUN publishes nothing physical | DEFERRED | No simulation sink yet; once outputs exist the gate is `CAN_EXECUTE_OUTPUTS` |
| T17 a second application crosses output ownership | DEFERRED | One application today; admission/ownership arbitration when multi-app arrives |
| T18 L1=0, L2=0 on the Secondary: no automatic takeover | COVERED | Detection Case Table: `!P && !I && S` enters CLAIMING, the target fence decides, failure ends in REDUNDANCY_LOST |
| T19 finalize interrupted by power loss | ADD | Amendment crash table covers one unit; extend to the half-persisted pair (amendment item 5) |
| T20 STOP and TEST on one barrier | ADD | One-op-per-barrier is structural (single pending slot); the pair version tests stop priority against a staged test |

## Readiness Verdict

**The architecture is ready for the good parts of this document.** Every
ADOPT-NOW item lands on a seam that already exists or is already decided,
and no triage verdict requires reopening a committed decision. The prepared
seams:

1. **Execution permit latch** — a designed extension of `HostMode`, not a
   new layer; standalone binaries grant it at startup, the redundancy shell
   grants it on an admission verdict, and `run()` refuses without it. Policy
   outside, enforcement inside ([HA Redundancy Layer Architecture](ha-redundancy-layer-architecture.md),
   "Shell, Not Runtime +1", "Minimal Seams" 1).
2. **Scan-commit notification** — the OwnerLease producer seam ADR-0062
   requires; `serve` already drives single rounds from outside, so the
   callback is one parameter on an existing loop (`compiler/vm-cli/src/serve.rs:416`).
3. **State snapshot accessors** — `read_variable` and `data_region` exist
   (`compiler/runtime/src/host.rs:484`, `:494`); the crossload write path
   mirrors `apply_migration_swap` (`:462`).
4. **Capability descriptor with the per-protocol binding** — a protocol swap
   changes a binding and the recorded guarantee level, never the FSM
   ([HA Redundancy Layer Architecture](ha-redundancy-layer-architecture.md),
   "Protocol Portability").
5. **Epoch** — the newtype shape exists (`compiler/runtime/src/generation.rs:15`,
   `:37`); `EpochStore` is the smallest port that keeps NV policy open.
6. **Additive status surface** — `StatusPayload` grows optional blocks
   (`pendingEdit`, `compiler/runtime/src/commands.rs:159`), `identity`
   carries the optional `redundancy` block (`:231`, `:251`), and clients
   ignore unknown fields ([Engineering Connection](engineering-connection.md),
   "Extensibility is structural").
7. **V-code registries** — per-crate CSVs with generated constants: runtime
   V4007–V4017 (`compiler/runtime/resources/problem-codes.csv`), vm-cli
   V6011–V6014, and the proposed HA V41xx block
   ([HA Engineering UI Contract](ha-engineering-ui.md)).
8. **A/B slot store and boot adoption** — `SlotStore`
   (`compiler/vm-cli/src/slot_store.rs:89`) with the crash table and
   durability invariant of the ADR-0064 amendment; the pair persist path
   reuses it rather than inventing one.
9. **A `no_std` execution kernel for a future MCU target** —
   `ironplc-container` is `#![no_std]` (`compiler/container/src/lib.rs:1`)
   and `ironplc-vm` follows ADR-0010; the *host* is `std` today, so MCU
   constraints bind only after a target-side host port, which is not an
   architecture reopen.
10. **Structural single-owner latches** — one pending swap
    (`compiler/runtime/src/host.rs:146`), one candidate (V4013), one
    engineering session (V6014, ADR-0065): the reasons the rejected
    machinery of T14/T20 is unnecessary for one unit.

Append-only extension points, already the house pattern: new commands are
new enum variants plus CSV rows and thin client wrappers (ADR-0055,
[HA Engineering UI Contract](ha-engineering-ui.md)); new container metadata
is a new tolerant sub-table (ADR-0053, ADR-0059); redundancy configuration
is project-side data ([HA Architecture Readiness](ha-architecture-readiness.md));
the redundancy crate adds modules, not runtime changes.

Where the document demands reopening a decision, the answer is REJECT:
a runtime RUN/PROGRAM mode authority (reopens ADR-0052 and dilutes ADR-0064);
`request_id`/deadline/journal protocol state (reopens ADR-0055 and
ADR-0065); a general ContractStore/CapabilityResolver (a new abstraction
layer with one consumer, forbidden by workspace doctrine until a gap is
shown); an XState actor runtime (new dependency and execution abstraction);
a safety execution domain (a new mechanism class with no ADR or
requirement). One item is a genuine open question, not a reopen: the pair
assemble transaction's receipt/epoch durability — the ADR-0064 amendment
already names "whether the epoch needs a durable-pending substate" as open
for the Phase 5 arbitration/fencing/epoch design.

## Concrete Takeaways

1. **Land the execution-permit latch with commit-time re-validation.**
   Implement the decided seam ([HA Redundancy Layer Architecture](ha-redundancy-layer-architecture.md),
   "Minimal Seams" 1) and make the boundary commit the single re-check
   point: a permit revoked between Accept and the boundary cancels the
   operation with a terminal result (scenario T07).
2. **Add the pair acceptance tests this review endorses:** T09 (exact-match
   test/untest, unchanged state root), T11 (lost binding receipt reconciled
   from authoritative status), T13 (peer restart anti-stale), T19
   (half-persisted pair after power loss), T20 (stop vs. test on one
   barrier). Anchors: ADR-0064(d)(h), the amendment crash table, ADR-0062.
3. **Bound the structural-migration pause.** Measure
   `apply_migration_swap` at the boundary (`compiler/runtime/src/host.rs:462`)
   and refuse or defer an edit whose pause cannot fit the process budget;
   promise no O(1) structural migration, and keep journal/catch-up deferred
   with the multi-task class (scenario T10).
4. **Record the quiescence proof in the HA commit path.** Mint the
   OwnerLease at the scan-commit callback with the boundary identity the
   host already has (the `rounds` counter, `compiler/runtime/src/host.rs:141`),
   so "the committed mode changed at a safe point" is auditable and ADR-0062's
   sole-minting-authority rule is enforced in code.
5. **Adopt the truth-table and no-auto-run scenarios as explicit HA
   acceptance tests:** T15 (no automatic RUN after recovery), T18
   (`!P && !I` makes a claimant, never an owner), T05 (permit and inhibit
   in one cut). Anchors: [HA Redundancy FSM](ha-redundancy-fsm.md)
   Detection Case Table and invariants; ADR-0062.

## Open Questions

1. **Pair epoch durability.** Does the pair's assemble transaction need a
   durable-pending epoch substate, and who reconciles a half-persisted pair
   from which authority? Decide in the Phase 5 arbitration/fencing/epoch
   design (the ADR-0064 amendment leaves this open).
2. **MCU/no_std host port.** If a controller-target port happens, which of
   the document's Platform/HwDiag contracts become mandatory, and do they
   live in a target shell rather than in the runtime?
3. **Hardware key and safety-mode requirement.** Is a key/safety profile
   ever expected as a product requirement? If yes, it is a new ADR and a new
   mechanism class — not absorption of this document.
