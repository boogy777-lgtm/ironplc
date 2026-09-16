# Spec: HA Architecture Readiness

## Overview

This spec records an architecture-readiness analysis for the Phase 5
redundancy layer (see [Roadmap](../roadmap.md)): whether the existing
compiler, container, VM, and runtime host form a foundation the redundancy
layer can grow on, what already exists that maps onto redundancy needs,
what does not exist at all, and where the new layer should live.

It is an analysis, not a component design: it changes no architecture and
introduces no requirements. Its inputs are the committed specs and ADRs and
the repository sources they describe.

This spec builds on:

- **[Roadmap, Phase 5 — Redundancy / HA](../roadmap.md)**: the binding
  decisions — EtherNet/IP, daisy-chain media, port map, five-layer quorum,
  admission before application start, commanded swap, and the three I/O
  firmware profiles
- **[HA Redundancy FSM](ha-redundancy-fsm.md)**: the SYNC/CONTROL
  statechart, admission, the OWNERSHIP_BARRIER, and the ping/pong contract
  the foundation must serve
- **[ADR-0062](../adrs/0062-measured-failover-timing-and-network-calibration.md)**:
  measured failover timing, the OwnerLease minting rule, and the
  `TakeoverReady` formula
- **[ADR-0052](../adrs/0052-online-change-performed-by-the-runtime-host.md)**
  through
  **[ADR-0061](../adrs/0061-engineer-decided-migration-for-out-of-policy-type-changes.md)**:
  the online-change stack whose seams this analysis reuses

## Readiness Verdict

**The architecture is ready to grow the redundancy layer on its
foundation.** The properties the redundancy statechart needs from below —
one immutable artifact per generation, caller-owned state separate from
code, an atomic commit point at a scan boundary, a typed engineering
protocol surface, and load-time integrity verification — all exist and are
enforced today. Everything the statechart needs from *above and beside*
(the network, the fencing authority, epoch persistence) does not exist and
was deliberately kept out of the runtime's scope, so the layer can be
built without reopening any committed runtime decision.

Three conditions keep the verdict true; all are already project rules:

1. The VM stays a pure execution kernel (ADR-0010); nothing about pairing,
   networking, or fencing enters `compiler/vm`.
2. The redundancy layer sits above `ironplc-runtime` and drives it;
   `ironplc-runtime` never depends on the redundancy implementation
   (dependency direction is one-way).
3. An ACTIVE unit never waits on a peer inside its scan — the host's scan
   loop keeps no peer-ACK in the output path, matching the realtime
   isolation goal of [HA Redundancy FSM](ha-redundancy-fsm.md).

## Reuse Map

Each row names an existing asset, proves where it lives, and states what it
gives the redundancy layer and what it still lacks.

### Runtime host FSM and the scan-boundary barrier

`compiler/runtime/src/host.rs:88` (`RuntimeHost`), the controller FSM at
`compiler/runtime/src/host.rs:9`, and the boundary application at
`compiler/runtime/src/host.rs:289` and `compiler/runtime/src/host.rs:336`.

*Gives:* the atomic-commit discipline the OWNERSHIP_BARRIER needs below it:
a change is staged and validated first, then applied at a scan boundary,
never mid-scan. The CLAIMING state is structurally the same shape as the
host's staged-candidate state — present, validated, not yet active. The
migration swap (`compiler/runtime/src/host.rs:368`) already rebuilds state
from an init image plus a per-entity plan, which is the local half of
readiness re-establishment.

*Lacks:* the host starts executing the moment it is constructed
(`compiler/runtime/src/host.rs:108`); there is no admission hook, no
monitor mode (execute nothing, receive replicated state), and no execution
permit the redundancy layer could grant or withhold.

### Command layer and the V-code registry

`compiler/runtime/src/commands.rs:32` (`Command`),
`compiler/runtime/src/commands.rs:284` (`execute`), and
`compiler/runtime/resources/problem-codes.csv` (V4007–V4016).

*Gives:* one typed, serde-serializable request/response protocol with a
line-delimited codec that every client shares (ADR-0055), and a proven
pattern for registering new user-facing runtime codes. Redundancy commands
(pair status, commanded swap) are additive variants of the same enum; the
statechart's REDUNDANCY_LOST alarm has an established surfacing path.

*Lacks:* the vocabulary is hot-edit only; the layer is request/response
  with no event or stream surface for ping/pong traffic, metrics, or alarms; the
payload encoding (container bytes as a JSON array) does not suit cyclic
replication traffic.

### Logic and application generations

`compiler/runtime/src/generation.rs:15` (`LogicGeneration`) and
`compiler/runtime/src/generation.rs:37` (`ApplicationGeneration`).

*Gives:* the local source for the ping/pong packet's `generations` field
([HA Redundancy FSM](ha-redundancy-fsm.md), "Ping/Pong Liveness"): one compiled
artifact and one active manifest are already versioned, ordered, and
comparable, so "does the peer hold my application generation" is answerable
without new runtime state.

*Lacks:* no HA `Epoch` type exists anywhere; generations are host-local
counters with no pair-wide agreement, no non-volatile persistence, and no
stale-rejection rule.

### Caller-owned VM buffers as the replication payload

`compiler/vm/src/buffers.rs:18` (`VmBuffers`) and the persistent-prefix
carry-over at `compiler/runtime/src/online_change.rs:168`.

*Gives:* the entire persistent IEC state — variable table plus data region
— lives in one caller-owned struct outside the VM, sized from the container
header. The state replication payload and its worst-case size are already
enumerable from the artifact, and `swap_buffers` already demonstrates
moving that state between buffer sets at a boundary.

*Lacks:* no dirty tracking, no bounded replication segments, no mutation
gates on writes, and no serialization for transport; the buffers are a
memory layout, not a replicable store.

### Container hashes and the load-time verifier

`compiler/container/src/container.rs:150` (hash population on write),
`compiler/container/src/load_verify.rs:288` (`verify_load`), wired into
`Container::read_from` per ADR-0058.

*Gives:* the config-signature analogue for pair qualification: BLAKE3
`content_hash` / `layout_hash` / `debug_hash` with fail-closed load
verification means a candidate replicated to the peer can be proven
byte-identical and internally consistent before it is stored — the
"both controllers possess the same validated candidate" barrier has its
integrity machinery already.

*Lacks:* signature sections remain zero (issue #1583); there is no
pair/configuration identity hash (a `PairId`/config binding) and no
replication-layout hash covering what the crossload channel carries.

### Stable variable UIDs as replication addressing

`compiler/container/src/type_section.rs:189` (`StableVarEntry`) and
`compiler/container/src/type_section.rs:209` (`FbFieldUidEntry`);
ADR-0053 and ADR-0059.

*Gives:* persistent state already has identity independent of memory
position. A replication channel can address state by stable UID rather than
by offset, and the load-time verifier already checks these tables, so a
replicated state segment is self-describing against the container the peer
verified.

*Lacks:* UIDs cover variables and FB fields only — not scheduler state,
checkpoint state, or the committed output image; there is no wire protocol
that consumes them.

### Engineering backend surface: serve session, MCP tools, V-codes

`compiler/vm-cli/src/serve.rs:31` (`serve`, specified by
[vm-cli.md](vm-cli.md) REQ-VC-vm-cli-018 through REQ-VC-vm-cli-023) and
`compiler/mcp/src/tools/hot_edit.rs:1` (the six `hot_edit_*` tools).

*Gives:* the engineering backend surface the future engineering UI drives:
a served one-line-in/one-line-out session over the command layer, plus
stateful MCP tools that keep one host session per server process
(ADR-0056). Pair status, SYNC/CONTROL state, ownership-barrier, and
takeover-readiness tabs are new commands and tools on a proven pattern,
not a new transport.

*Lacks:* the session model is single-host; `serve` drives scan rounds at a
constant zero uptime (the acceptance-test convention), so it is a scripted
engineering session, not the embedded runtime loop a controller runs.

### UID sidecar as the configuration-persistence analogue

`compiler/project/src/sidecar.rs:1` and the auto-load wiring at
`compiler/project/src/project.rs:281`; ADR-0057.

*Gives:* the persistence pattern for redundancy configuration:
deterministic, versioned JSON; fail-soft load of a missing or malformed
file; explicit rename/swap flows; engineering-side storage that leaves the
IEC sources untouched. `REDUNDANCY_ENABLED`, the configured role, and the
pair's registered addresses are project data of exactly this shape.

*Lacks:* the sidecar is engineering-workstation storage; the controller's
own non-volatile persistence (epoch and OwnerLease state surviving power
loss) is a different, target-side store that does not exist.

### Project/codegen pipeline as the pair's artifact path

`project::compile` orchestration into codegen, which emits the variable
table, stable UIDs, and hashes into the container (ADR-0053, ADR-0058).

*Gives:* "one application serves the pair" is already the unit of
deployment: one compiled `.iplc`, integrity-hashed, carrying the state
identity tables the replication channel and the migration planner both
consume. Distributed hot change (deferred follow-up) starts from an
artifact both units can verify identically.

*Lacks:* redundancy configuration is not yet project data — no
`REDUNDANCY_ENABLED`, no role, no pair addressing, no I/O configuration
model for the port map.

## Gap List

The following do not exist at all in the repository today. Ordering is not
priority; see the sequencing recommendation below.

1. **Network transport / EtherNet/IP stack.** No socket, CIP, Class 1
   connection, Forward_Open, or cyclic I/O code exists anywhere; the
   runtime crate's dependencies are the VM, the container, and serde.
2. **NIC HAL abstraction.** No per-port driver abstraction, capability
   advertisement (timestamp/IRQ/DMA), or uniform PHY counters — the
   roadmap's open "network HAL abstraction" item.
3. **Pair discovery and admission.** Nothing discovers a neighbor on the
   pair link or decides standalone vs. redundant before the application
   starts; `RuntimeHost::new` runs init and is ready to scan immediately.
4. **Crossload channel.** No state replication transport, segmentation,
   dirty tracking, delta ordering, or peer-side apply path; no monitor
   mode to apply into.
5. **OwnerLease and epoch non-volatile persistence.** No epoch type, no
   lease minted by the host at scan commit (ADR-0062 forbids the network
   task minting it), no NV store — volatile or otherwise.
6. **I/O fencing authority.** No I/O driver model at all (explicitly out of
   scope in [Runtime Execution Model](runtime-execution-model.md)); no
   Exclusive Owner / Input Only connection management, no
   CLAIMED_DISARMED / ARM staging, no ownership-status consumption, no
   all-or-nothing barrier implementation.
7. **Calibration metrics pipeline.** No current/EMA10/EMA100/max/count
   collection, no per-module latency ingestion, no engineering surface for
   the ADR-0062 variables; `ironplcvm benchmark` (REQ-VC-vm-cli-013) is
   the nearest existing measurement surface and is offline-only.
8. **Witness-free quorum machinery.** No ping/pong encode/decode, no
   two-channel cross-check, no detection case table evaluation, no
   `TakeoverPermission` gate (ADR-0062); the five-layer quorum is decided
   on paper only.

Deferred by the roadmap and therefore deliberately absent from this list:
preemptive/resumable execution, multi-task execution frontiers, cluster
time, and external-protocol side-effect/replay semantics.

## Growth Recommendation

### Crate placement: a new crate above `ironplc-runtime`

The redundancy layer should be a **new crate** (working name
`ironplc-redundancy`) that depends on `ironplc-runtime` — never the
reverse.

- *Why not a module in `ironplc-runtime`:* the runtime crate is the
  hot-edit protocol authority and ships on every standalone controller.
  Pair semantics — roles, quorum, replication, fencing — are a separate
  responsibility with a separate dependency surface (network, NV storage,
  HAL). Folding them in would make the standalone runtime carry a
  redundancy it must never know about, violating the rule that the runtime
  core stays usable with no redundancy layer present.
- *Why not inside the VM:* excluded by ADR-0010 and by the readiness
  verdict's condition 1.
- *Dependency direction:* `ironplc-redundancy` → `ironplc-runtime` →
  `ironplc-vm` / `ironplc-container`. This is the layering the reference
  architecture freezes, and it matches how the command layer already
  exposes the host without the host knowing its clients (ADR-0055).

### How the existing `RuntimeHost` API is driven

The redundancy layer is the host's client and gatekeeper:

- **Admission.** Before `RuntimeHost::new` is allowed to scan, the
  redundancy layer runs discovery and decides standalone vs. redundant
  ([HA Redundancy FSM](ha-redundancy-fsm.md), "Admission"). Standalone
  admission constructs and runs the host exactly as today.
- **Monitor mode.** A Secondary constructs the host but does not drive
  `run`; the crossload apply path writes replicated state into the host's
  buffers. This is the one genuine host API addition the layer needs —
  admission gating and monitor mode are new entry points, not changes to
  the swap machinery.
- **CLAIMING → ACTIVE.** The CONTROL chart drives ordered fencing
  acquisition through the I/O fencing authority; on barrier pass it bumps
  the epoch and starts driving `run`. The host's scan-boundary discipline
  is reused unchanged; no peer-ACK enters the scan (condition 3).
- **Commanded swap and status.** New variants on the existing `Command`
  enum and new V-codes in the runtime's CSV pattern keep the engineering
  surface single-sourced; the CLI serve session and MCP tools grow
  matching thin wrappers, as they did for hot edit.

### Phase-5 implementation sequencing

Consistent with the roadmap ("design first: arbitration quorum + commit
certificate + fencing, then the redundancy layer") and with
[HA Redundancy FSM](ha-redundancy-fsm.md) leaving quorum internals to the
preceding design:

1. **Arbitration, fencing, and epoch/OwnerLease design** (specs + ADRs):
   quorum participants, epoch authority, lease lifecycle, and the failure
   matrix. Everything below consumes these decisions; ADR-0062 fixes the
   minting rule (the host mints OwnerLease at scan commit; the network
   task never does).
2. **NIC HAL abstraction and the two-channel ping/pong exchange.** Detection feeds
   every later gate, and the calibration philosophy of ADR-0062 requires
   measurement from day one — per-port metrics are part of the HAL, not a
   retrofit.
3. **Epoch and OwnerLease non-volatile persistence policy** — an open
   roadmap parameter that blocks CLAIMING correctness across power loss.
4. **Crossload channel: state replication from `VmBuffers` with stable-UID
   addressing**, bringing the SYNC sub-chart (deSYNC → SYNCING →
   SYNC_READY) up on the new crate.
5. **I/O fencing authority client and the OWNERSHIP_BARRIER**, bringing up
   the CONTROL sub-chart (IDLE → CLAIMING → ACTIVE, REDUNDANCY_LOST),
   sequentially per the v1 claim model.
6. **Calibration metrics pipeline**: EMA collection per channel and per
   module, feeding `IO_READY` / `TakeoverReady` exactly as ADR-0062's
   formulas require.
7. **Engineering surface**: redundancy commands, V-codes, and the
   engineering UI tabs (pair status, SYNC/CONTROL state, per-channel EMA
   metrics, ownership barrier, takeover readiness).

Steps 1–3 are design and target-side primitives; steps 4–7 build the layer
itself, in an order where each step's inputs already exist.

## Out of Scope

- Quorum/commit-certificate wire formats and protocol internals (step 1
  above is their home, and [HA Redundancy FSM](ha-redundancy-fsm.md)
  already excludes them).
- I/O firmware internals behind the three profiles (GENERIC,
  REDUNDANT_OWNER, IRONPLC_HA_IO); this spec consumes the profiles as
  roadmap decisions.
- Distributed hot change, edit replication, and external-protocol replay
  semantics (roadmap deferred follow-ups).
- Any change to production code; this document is analysis only.
