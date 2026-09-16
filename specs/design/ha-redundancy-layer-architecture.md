# Spec: HA Redundancy Layer Architecture

## Overview

This spec designs the Phase 5 redundancy layer: where it lives, how it
decomposes, which existing architectural patterns it reuses, and the
minimal seams it needs from `ironplc-runtime`. It answers two questions
directly:

1. **Should the redundancy layer be an n+1 module?** Yes — and this spec
   proves the workspace actually supports that move, with evidence.
2. **Where may new abstractions appear?** Only where no architectural
   groundwork for redundancy exists; each such gap is stated with the
   reason no existing call covers it.

This spec builds on:

- **[HA Architecture Readiness](ha-architecture-readiness.md)**: the
  readiness verdict and the reuse/gap inventory this design deepens
- **[HA Redundancy FSM](ha-redundancy-fsm.md)**: the SYNC/CONTROL
  statechart, admission, and the ping/pong liveness contract this layer
  implements
- **[ADR-0062](../adrs/0062-measured-failover-timing-and-network-calibration.md)**:
  measured timing, calibration-gated readiness, and the OwnerLease minting
  rule the seams below serve
- **[Roadmap, Phase 5](../roadmap.md)**: the binding media, port-map, and
  quorum decisions

## Design Goals

1. **The runtime stays redundancy-free** — `ironplc-runtime` never names a
   pair, a role, an epoch, or a network; the dependency direction is
   one-way, redundancy → runtime
2. **Reuse before invention** — every mechanism that already has an
   architectural home is called, not wrapped; new abstractions exist only
   where the gap list proves no home exists
3. **A standalone controller ships zero redundancy** — the layer is a
   separate crate a deployment includes or omits
4. **Measurement from day one** — calibration and EMA tracking are part of
   the first runnable increment, per ADR-0062, not a retrofit

## Is the Architecture Modular? The n+1 Verdict

**Yes.** The workspace is genuinely modular, and the redundancy layer is
the same kind of addition `ironplc-runtime` already was: an n+1 crate
above the existing core, consumed by thin clients. Evidence:

- **Crate-per-concern workspace.** `compiler/Cargo.toml` lists 22 members,
   each owning one concern: `dsl`, `parser`, `analyzer`, `codegen`,
   `container`, `vm`, `plc2plc`, `project`, `runtime`, `mcp`,
   `cli-support`, `vm-cli`, plus tooling crates. No crate absorbs a
   neighbor's responsibility.
- **The runtime was itself added as n+1.** `compiler/runtime/Cargo.toml`
   depends on `ironplc-container`, `ironplc-vm`, and serde only: the
   online-change host was layered above the VM without modifying the VM's
   execution kernel (ADR-0052). The move this spec makes is the same move,
   one level higher.
- **Thin clients share one core.** `compiler/vm-cli/Cargo.toml:26-29`
   consumes `ironplc-vm` + `ironplc-container` + `ironplc-runtime`;
   `compiler/mcp/Cargo.toml:21` consumes the same `ironplc-runtime`; the
   VS Code extension drives the same command layer. Three clients, one
   protocol authority — the pattern a redundancy engineering surface
   joins.
- **Feature surfaces stay aligned by construction.**
   `compiler/mcp/src/feature_flag_conformance.rs:295` fails the build when
   a feature flag lacks a fixture or fails to gate its example — the
   workspace polices surface drift with conformance tests, not
   conventions.
- **Curated crate boundaries.** `compiler/runtime/src/lib.rs:19-34` is
   private modules plus a curated `pub use` — the shape the new crate
   adopts.

## Pattern Reuse Map

Each entry names the pattern, proves where it lives, and states what the
redundancy layer reuses it for. These are pattern reuses — the new crate
follows them; it does not wrap the existing code.

| Existing pattern | Proof | Reused for |
|---|---|---|
| Typed command enums + line-delimited JSON codec | `compiler/runtime/src/commands.rs:32`, `compiler/runtime/src/commands.rs:284` | The engineering-facing redundancy commands (pair status, commanded swap): one serde-tagged vocabulary, thin transports, no per-client dialects |
| V-codes from CSV via build.rs codegen | `compiler/runtime/build.rs:1` (mirrors the vm-cli `io_codes` build); `compiler/runtime/resources/problem-codes.csv` | HA runtime codes: a new crate-local CSV generating constants the same way, leaving the VM's trap table and the runtime's hot-edit codes untouched |
| State-gated single-threaded session loop | `compiler/vm-cli/src/dap/server.rs:1` (requests gated through a legality table), framing in `compiler/vm-cli/src/dap/framing.rs:1` | The pair-link session: a framed transport driving a statechart-gated loop; the ping/pong exchange and admission are gated by SYNC/CONTROL state, not by transport callbacks |
| Versioned, fail-soft JSON persistence | `compiler/project/src/sidecar.rs:1`; auto-load wiring `compiler/project/src/project.rs:281` | Engineering-side pair configuration (`REDUNDANCY_ENABLED`, configured role, pair addressing): deterministic file, missing/malformed loads as empty, explicit update flows |
| Newtype generation counters | `compiler/runtime/src/generation.rs:15`, `compiler/runtime/src/generation.rs:37` | `Epoch`: the ownership/fencing counter follows the same opaque-newtype shape; no framework, one type |
| Caller-owned state buffers | `compiler/vm/src/buffers.rs:18` | The crossload payload: `VmBuffers`' persistent regions are the replication unit's memory image, enumerable and sized from the container |
| Append-only sub-tables with tolerant readers | ADR-0053/ADR-0059; `compiler/container/src/type_section.rs:189`, `compiler/container/src/type_section.rs:209` | The rule that any future container-carried HA metadata is a new sub-table a pre-existing reader tolerates; v1 keeps redundancy configuration project-side and does not touch the container |
| Clap subcommand conventions | `compiler/vm-cli/src/main.rs:43` | Any redundancy CLI growth is a new subcommand on the existing binaries, not a new binary |
| REQ traceability from specs to tests | `compiler/vm-cli/build.rs:74` (`spec_requirements_gen::generate`), `#[spec_test]` via `spec_test_macro` | The new crate's spec-conformance machinery: requirements live in this and later HA specs, enforced by the same generator |

## Crate Placement and Dependency Direction

The redundancy layer is a **new workspace crate, `ironplc-redundancy`**,
with the dependency chain:

```text
ironplc-redundancy → ironplc-runtime → ironplc-vm / ironplc-container
```

- It is not a module of `ironplc-runtime`: the runtime is the hot-edit
  protocol authority present on every standalone controller, and it must
  never name pair semantics. Folding redundancy in would break the
  dependency-direction rule that keeps the runtime core deployable alone.
- It is not in the VM: excluded by ADR-0010 and by the readiness
  verdict's conditions.
- The engineering command vocabulary for redundancy lives in the new
  crate, following the ADR-0055 pattern (typed enums, line codec, CSV
  V-codes). This refines the placement suggested in
  [HA Architecture Readiness](ha-architecture-readiness.md): the readiness
  doc proposed extending the runtime's `Command` enum, but that would put
  redundancy vocabulary into the crate that must stay redundancy-free.
  Each layer keeps its own typed surface; `ironplcvm serve` and the MCP
  server compose both, staying thin.

## Module Decomposition

`ironplc-redundancy` follows the workspace conventions: private modules,
curated re-exports, each module under 1000 lines with one responsibility.

```text
ironplc-redundancy
├── statechart    — the SYNC and CONTROL superstates, guards, case table
├── admission     — neighbor discovery outcome → standalone / secondary /
│                   initial-owner decision before the application starts
├── liveness      — ping/pong codec, per-channel sequences, two-channel
│                   cross-check, peer-failure confirmation timing
├── crossload     — state replication: segmentation of the host's
│                   persistent buffers, stable-UID addressing, peer apply
├── fencing       — I/O fencing authority client: ordered claim,
│                   CLAIMED_DISARMED verification, ARM, barrier rollback
├── calibration   — EMA10/EMA100/max/count trackers, link profile,
│                   budget validation, degradation alarms (ADR-0062)
├── epoch         — the Epoch newtype and the EpochStore persistence port
│                   (volatile backend first, NV backend per target)
├── lease         — OwnerLease minting, fed by the host's scan-commit seam
│                   (never by the network task — ADR-0062)
├── hal           — the NIC port abstraction (below); drivers live outside
└── commands      — the typed engineering vocabulary + line codec + CSV
                    codes, following the ADR-0055 pattern
```

The statechart module owns the only FSMs; every other module is a service
it drives. There is no registry, no event bus, no plugin point.

## Interfaces

The interfaces below are the only new abstractions this design introduces.
Each exists because the gap list shows nothing in the workspace covers it.

```rust
/// One network port as the redundancy layer sees it. Implementations are
/// target-side drivers; the layer never touches registers.
trait NicPort {
    fn capabilities(&self) -> PortCapabilities; // timestamp/IRQ/DMA per port
    fn counters(&self) -> PhyCounters;          // uniform PHY/error counters
    fn send(&mut self, frame: &[u8]) -> Result<(), PortError>;
    fn poll(&mut self) -> Option<(IngressTimestamp, Frame)>;
}

/// Epoch persistence across power loss. Volatile backend first; an NV
/// backend (FRAM/journalled flash) plugs in without architecture change.
trait EpochStore {
    fn load(&self) -> Option<Epoch>;
    fn store(&mut self, epoch: Epoch) -> Result<(), PersistError>;
}
```

Everything else is composition over existing APIs: the crossload payload
is `VmBuffers`; addressing is the container's stable UID tables;
engineering transport is the line-codec pattern; generations are the
existing newtypes.

## Minimal Seams in ironplc-runtime

The supervisor drives the existing host API; exactly three small
extensions are needed, each an extension of a call that exists today — no
new abstraction layer inside the runtime.

1. **Scan-commit notification.** `RuntimeHost::run`
   (`compiler/runtime/src/host.rs:289`) applies pending swaps at round
   boundaries; `compiler/vm-cli/src/serve.rs:12` already demonstrates
   driving single rounds from outside. The supervisor needs one commit
   notification per completed round to mint the OwnerLease and take the
   crossload snapshot at the commit point (ADR-0062: the lease is born at
   scan commit, never in the network task). Seam: an optional per-round
   callback on the run loop — one parameter, not a framework.
2. **State snapshot read access.** The host owns the buffers and exposes
   `data_region()` (`compiler/runtime/src/host.rs:400`) and per-index
   `read_variable` (`compiler/runtime/src/host.rs:390`). The crossload
   source needs a bulk read of the persistent regions (`vars` +
   `data_region`). Seam: one read accessor beside the existing ones.
3. **Crossload apply while not scanning.** Admission and the SYNC chart
   require a unit that holds the application and applies replicated state
   without executing — monitor mode. The host today is either running
   rounds or idle with private buffers. Seam: an apply path that writes a
   replicated snapshot into the buffers while the host drives no rounds,
   mirroring what `apply_migration_swap`
   (`compiler/runtime/src/host.rs:368`) already does from a migration
   plan — same buffers, different source.

No other runtime change: the swap machinery, validation tuple, and command
layer are reused unchanged.

## Gaps With No Existing Groundwork

Each gap below requires new code because nothing in the workspace performs
the function at all (the readiness doc's gap list, now with the design
consequence):

1. **NIC HAL.** No networking exists anywhere; the runtime's dependencies
   are the VM, the container, and serde. The roadmap requires per-port
   drivers with advertised capabilities and uniform counters — hence
   `NicPort`, the smallest trait that carries the decided requirements.
2. **Pair discovery and admission.** No code discovers a neighbor or
   decides standalone vs. redundant; `RuntimeHost::new` is ready to scan
   immediately. Admission is a new module driving the host, gated by
   discovery.
3. **Crossload channel.** `VmBuffers` is the payload, not a transport; no
   segmentation, dirty tracking, or peer apply exists. New module; the
   payload and addressing reuse what exists.
4. **OwnerLease minting at scan commit.** No epoch type, no lease, no
   commit-time producer. ADR-0062 fixes the producer (the supervisor, at
   scan commit) — seam 1 above is its only runtime requirement.
5. **I/O fencing client.** The runtime has no I/O driver model (explicitly
   out of scope in the execution model); ordered claim, CLAIMED_DISARMED
   verification, ARM, and barrier rollback are new, sitting in `fencing`.
6. **Calibration metrics pipeline and Studio surface.** No EMA collection,
   no link profile, no budget check. `ironplcvm benchmark` is offline
   measurement only. New module; the engineering surface follows the
   command-layer pattern.
7. **Epoch NV persistence.** The sidecar is engineering-workstation
   storage; controller-side persistence across power loss has no home —
   hence `EpochStore`, the smallest port that keeps the policy open.

## Deliberately Not Built

- No generic service framework, registry, or event bus — modules call
  each other directly; the statechart drives services.
- No VM changes, no preemptive executor, no per-task contexts, no cluster
  time — roadmap-deferred.
- No new wire framework: the pair link reuses the typed-enum + thin-codec
  pattern; the binary cyclic encoding is a codec detail inside
  `liveness`/`crossload`, not a framework.
- No container format change in v1: redundancy configuration is project
  data (sidecar pattern); the container stays an execution artifact.
- No dual-owner output modules, no scheduled ARM, no witness hardware —
  out of v1 scope per the FSM spec and roadmap.
- No automatic retuning of failover thresholds (ADR-0062: alarms, not
  silent adjustment).

## Implementation Sequencing

The phase-5 order, mapped onto the modules above (consistent with the
readiness doc's sequence; design steps precede crate work):

1. **Arbitration / fencing / epoch design** (specs + ADRs first): quorum
   participants, epoch authority, lease lifecycle, failure matrix.
2. **`hal` + `liveness` + `calibration` trackers** — the two-channel
   ping/pong exchange with per-port measurement from day one, because
   ADR-0062 makes measurement the foundation, not a retrofit.
3. **`epoch` + `lease` + the runtime seams** — the commit callback is the
   only change inside `ironplc-runtime`, and it gates OwnerLease minting.
4. **`admission` + `crossload` + the SYNC chart** — a Secondary that
   syncs to SYNC_READY in monitor mode.
5. **`fencing` + the CONTROL chart** — the OWNERSHIP_BARRIER and
   REDUNDANCY_LOST.
6. **Calibration pipeline completion** — link profile, budget validation,
   degradation alarms feeding `IO_READY` / `TakeoverReady`.
7. **Engineering surface** — the redundancy command vocabulary, CSV
   V-codes, and thin growth of `ironplcvm serve` and the MCP tools;
   Studio tabs consume them.

## Out of Scope

- Quorum/commit-certificate wire formats and protocol internals (step 1
  above is their home).
- I/O firmware internals behind the three roadmap profiles; this spec is
  the client of the fencing authority, not its implementation.
- Distributed hot change, edit replication, and external-protocol replay
  semantics (roadmap deferred follow-ups).
- Driver implementations for any specific NIC or NV device.
