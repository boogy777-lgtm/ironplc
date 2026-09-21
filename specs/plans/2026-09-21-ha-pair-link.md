# HA Phase 5 slice 2: pair link + SYNC FSM

Date: 2026-09-21
Status: draft
Branch: feature/ha-pair-link

## Goal

Land the pair-link milestone of the Phase 5 redundancy layer in one PR:
(1) runtime seam 2 — the scan-commit callback on `RuntimeHost::run` (the
ADR-0062 mint point for epochs and the future OwnerLease), with `serve`
composing a logging callback; (2) the pair-link transport seam in
`ironplc-redundancy` — the design doc's `NicPort` interface plus the
loopback simulator binding as a first-class deliverable; (3) ping/pong
liveness — per-channel +1/+1000 penalty counter, missing-increment silence
detection, missed-threshold peer death as an FSM input; (4) configured
roles + epoch — epoch minted at the scan-commit callback, exchanged on the
pair link, anti-stale only; (5) the SYNC subchart (deSYNC → SYNCING →
SYNC_READY) with the FSM doc's guards, the crossload-readiness typed
placeholder seam, and the zombie rule enforced at admission; (6) admission
↔ permit interplay via the existing `permit_for` policy and the crate-local
V41xx CSV (V4101 only). Fencing/claim/barrier/ARM, crossload payload,
calibration, promotion mechanics, engineering surface: explicitly out of
scope.

## Architecture

- **Scan-commit callback (mechanism inside `RuntimeHost`):** one
  notification per completed scan round, carrying the boundary identity
  (`rounds`, `mode`, active `LogicGeneration`, `ApplicationGeneration`).
  `run` keeps its signature and delegates with a no-op; `run_with_commit`
  carries the callback as one parameter on the existing loop (the design's
  "one parameter, not a framework"). The callback fires after the round is
  committed, inside `run_session`, without holding the VM borrow across a
  host mutation.
- **Transport seam (`hal`):** the `NicPort` trait exactly as declared in
  the architecture doc (`capabilities` / `counters` / `send` / `poll`),
  with `PortCapabilities`, `PhyCounters`, `IngressTimestamp`, `PortError`.
  The **loopback binding** (`loopback`) is two in-process ports over shared
  queues, with frame counters and a partition switch for loss/death
  injection — the simulator the two-instance scenarios run over.
- **Liveness:** one logical ping/pong packet (the FSM doc's field list:
  pair id, role, epoch, generation, ping/pong seqs, reserved
  `io_owner_state`, crc32) encoded as a fixed 38-byte frame; codec lives
  inside `liveness`. `ping_seq` +1 per PING, `pong_seq` +1 per PONG
  (answered on the next send). Silence is a missing expected increment: an
  unconfirmed ping beyond the confirmation window records one miss
  (+1000; success is +1) and a consecutive-miss count; at the configured
  missed threshold the exchange emits `PeerDied` once. The wire `role`
  field is the sender's pair role as admitted (Primary iff the sender's
  admission verdict is Primary): this slice's fold of the doc's
  "configured role + chart state"; the codec is unchanged when CONTROL
  lands.
- **Epoch:** `Epoch` newtype mirroring the generation-counter shape;
  `adopt(peer)` is strictly anti-stale (Adopted / Aligned / Stale), never
  arbitration (ADR-0062). Minting is the supervisor's job at scan commit:
  the owning unit's `run_with_commit` closure bumps the epoch.
- **SYNC subchart (`statechart`):** a pure table-driven 3-state chart with
  the FSM doc's transition table (paired → SYNCING; replication complete
  and peer epoch agreed → SYNC_READY; sync loss / epoch discontinuity /
  peer death → deSYNC with a recorded reason). `CrossloadReadiness`
  (`InProgress`/`Complete`) is the typed placeholder seam the future
  crossload module feeds; the driver emits `ReplicationComplete` only when
  readiness is `Complete` and the epoch is agreed.
- **Admission (zombie fence in code):** discovery classifies the neighbor
  from validated packets (`classify`: pair identity first — a foreign
  pair id is the V4101 refusal, never a neighbor); `admit` maps
  (configured role × discovery) to the verdict. A live Primary neighbor
  makes any unit — including a Primary-configured one — the Secondary:
  configuration does not override a living owner. `permit_for` (slice 1)
  stays the only verdict→permit mapping.
- **No new abstraction layers:** the per-unit driver that pumps packets,
  advances the chart, and mints the epoch lives in the integration tests
  as test support; the real shell replaces it when a binary embeds the
  layer (the architecture doc's module list deliberately has no driver
  module — the shell is the binary's composition root).

## File map

- `compiler/runtime/src/host.rs` — `ScanCommit`, `run_with_commit`,
  `run` delegating; callback fired per committed round in `run_session`.
- `compiler/runtime/src/lib.rs` — export `ScanCommit`.
- `compiler/runtime/tests/acceptance.rs` — scan-commit callback tests.
- `compiler/vm-cli/src/serve.rs` — `drive_scan_round` composes a debug
  logging callback via `run_with_commit`.
- `compiler/ironplc-redundancy/src/config.rs` — `ConfiguredRole`,
  `PairId`, `RedundancyConfig` (standalone/pair, link thresholds).
- `compiler/ironplc-redundancy/src/admission.rs` — `Discovery`,
  `Neighbor`, `classify`, `admit` (Result with the V4101 refusal),
  existing `AdmissionVerdict`/`permit_for` kept.
- `compiler/ironplc-redundancy/src/hal.rs` — `NicPort` + port types.
- `compiler/ironplc-redundancy/src/loopback.rs` — simulator binding.
- `compiler/ironplc-redundancy/src/liveness.rs` — packet codec (crc32),
  `PairRole`, `Liveness` exchange, `LivenessEvent`.
- `compiler/ironplc-redundancy/src/epoch.rs` — `Epoch`, `EpochAdoption`.
- `compiler/ironplc-redundancy/src/statechart.rs` — `SyncChart`,
  `SyncState`, `SyncEvent`, `DeSyncReason`, `CrossloadReadiness`.
- `compiler/ironplc-redundancy/{build.rs,resources/problem-codes.csv}` —
  crate-local V41xx codegen mirroring the runtime crate.
- `compiler/ironplc-redundancy/Cargo.toml` — `csv` build-dependency.
- `compiler/ironplc-redundancy/tests/pair_link.rs` — two-instance
  loopback scenarios (converge; +1/+1000; peer death → deSYNC; zombie
  rejoin; stale epoch rejection; permit held until admission).
- `docs/reference/runtime/problems/V4101.rst` + register the crate CSV in
  `docs/extensions/ironplc_problemcode.py` (docs lifecycle of the V41xx
  block per `ha-engineering-ui.md`).
- `specs/roadmap.md` — Phase 5 delivered line.

## Tasks

1. Runtime seam 2: `ScanCommit` + `run_with_commit` + tests (per-round
   identity; `run` unchanged behavior).
2. `serve` composes the logging callback.
3. Redundancy modules: config, hal + loopback, liveness (codec +
   exchange), epoch, statechart, admission extension.
4. Crate CSV + build.rs codegen + V4101 (foreign pair at discovery) with
   docs page and extension registration.
5. Integration scenarios over the loopback binding, incl. epoch minting
   through a real `RuntimeHost` scan-commit callback and the permit held
   until admission.
6. Docs: roadmap line; full gates (`cd compiler && just`, specs gates);
   remove this plan before merge.

## Authorities

- `specs/design/ha-redundancy-layer-architecture.md` (Minimal Seams 2,
  hal/liveness/epoch/statechart module decomposition, NicPort interface,
  simulator binding mandate)
- `specs/design/ha-redundancy-fsm.md` (SYNC subchart, admission, zombie
  rule, ping/pong packet and +1/+1000 penalty, promotion exactly 2 cases)
- `specs/design/external-fsm-review.md` (scan-commit notification seam;
  T07 — already implemented in slice 1, no delta needed)
- ADR-0062 (epoch anti-stale only, minted by the supervisor at scan
  commit), ADR-0064/0065 (pair staging context; single engineering
  session — the pair link is PLC-to-PLC)
- `specs/design/ha-engineering-ui.md` (V41xx block assignment + docs
  lifecycle)
