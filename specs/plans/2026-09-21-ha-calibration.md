# HA Phase 5 slice 5: calibration engine (ADR-0062)

Date: 2026-09-21
Status: draft
Branch: feature/ha-calibration

## Goal

Land the calibration milestone of the Phase 5 redundancy layer in one
PR, per ADR-0062's measured-reality paradigm (measure → calibrate →
qualify → configure a budget → continuously verify reality):
(1) the timing model — typed T-contributions tracked as current /
EMA10 / EMA100 / max / count: peer-detection (measured from the
ping/pong exchange), claim (per module), arm, scan safe-point, and
output apply (per module, firmware-reported), recorded per simulated
module/device where applicable; (2) the calibration run — a
deterministic commissioning procedure over the loopback/simulator pair
that measures each contribution in the abstract tick domain (repeated
samples → representative values per the ADR's method: EMA estimates,
max qualification bounds), identifies the limiting device, computes
the takeover budget, and emits the TakeoverReady verdict exactly per
the ADR's formulas; (3) guarantee monitoring — live operation leaving
the calibrated envelope raises HA_PERFORMANCE_DEGRADED; a recovery
budget that can no longer be met raises HA_TIMING_GUARANTEE_LOST, via
the new V4110/V4111 codes with rst pages; (4) the backend surface — a
queryable `CalibrationStatus` carrying the values ha-engineering-ui.md
renders (contributions, budget, limiting device, readiness, counters),
internal API + tests, no transport exposure; (5) tests: budget math
against hand-computed ADR vectors, limiting-device selection,
calibration-run reproducibility on the simulator, degradation crossing
→ alarm codes, re-calibration invalidating the previous guarantee;
(6) docs: the roadmap Phase 5 status line. No vscode UI, no new ADR
(ADR-0062 records the decision), no EtherNet/IP binding, no guard-table
rewiring (the shell composition slice wires the verdict into promotion
policy).

## Architecture

- **`timing` module — the typed T-contributions.** `TermStats`: one
  term's tracker — current / EMA10 / EMA100 / max / count (ADR-0062,
  "Decision": each term is tracked exactly so). EMA in fixed-point
  milli-units (alpha = 2/(N+1)), seeded with the first sample:
  deterministic, hand-computable in tests, no floats. `DirectionProfile`:
  the per-direction link profile (ADR-0062's HA link profile) — RTT
  `TermStats` plus min (jitter envelope min..=max), pings sent, pongs
  received, current/max consecutive loss; loss rate derived.
  `ModuleContribution`: per required I/O module — claim, arm, and
  output-apply `TermStats` (the per-module T contributions of the
  ownership-barrier view).
- **ADR-0062 formulas, implemented literally.** `T_claim =
  sum_i(T_claim,i)` (sequential v1); estimate `sum_i(EMA_claim,i)`,
  qualification bound `sum_i(MaxQualified_claim,i)`. `T_claim-start =
  max(T_plc-peer-detection, T_io-owner-lease-expiry)` — both
  independent proofs. `T_arm = max_i(arm_i)` and `T_output-apply =
  max_i(output_apply_i)`: the barrier arms all modules in one
  all-or-nothing participation, so the parallel terms take the slowest
  device. `T_safepoint` bounded `0..=T_scan,max`; the worst case pays
  `T_scan,max`, the predicted-if-now estimate pays the phase-aware
  remainder. `T_recovery = T_peer-detect + T_claim + T_arm +
  T_scan-safe-point + T_output-apply`. Budget check exactly the ADR's
  inequality `T_detect + T_claim,max + T_scan,max + T_output,max <=
  T_recovery-budget`; on failure the verdict reports the minimum
  demonstrated budget (the calculated worst-case recovery). The
  budget's `T_detect` is the claim-start (max of peer-detection and
  lease-expiry): uncontrolled failover starts at both proofs agreeing.
  `TakeoverReady = SYNC_READY && IO_READY && RedundancyLinkValid`
  (ADR-0062); the engine consumes the SYNC/IO booleans and owns the
  link-validity gate.
- **`calibration` module — the engine.** `CalibrationState`:
  UNQUALIFIED → CALIBRATING → CALIBRATED (the ADR's
  calibration-gated readiness chain); `begin()` invalidates any
  previous guarantee (link invalid until `complete()`), `invalidate(reason)`
  models the ADR's significant-change list (link/topology/protocol
  change → requalification required). Recording APIs feed samples both
  during calibration and live: `record_rtt`, `record_exchange_loss`,
  `record_peer_detect`, `record_scan`, `record_module_timing`. The
  commissioning envelope (RTT max) freezes at `complete()`.
  `TimingAlarm` is the coded alarm vocabulary: `PerformanceDegraded`
  (V4110 — reality left the calibrated envelope: a current sample or
  the EMA10 above the frozen envelope on either direction) and
  `TimingGuaranteeLost` (V4111 — the worst case recomputed from the
  live term maxima exceeds the configured budget); two independent
  latched flags (haStatus renders both), cleared only by
  recalibration — thresholds are never automatically retuned
  (ADR-0062). Evaluation runs inside the record calls (the engine is
  the one authority; no caller-must-check convention).
  `CalibrationStatus` is the queryable backend surface: state,
  link_valid, takeover_ready, per-term stats, per-module
  contributions, the limiting device (the module with the greatest
  qualification-bound claim contribution), the budget with its
  verdict, the alarm flags, and the counters (samples, exchanges,
  recalibrations, last invalidation).
- **`calibration_run` module — the deterministic commissioning
  procedure.** `run_calibration(config, budget, registry)` drives a
  loopback pair of `Liveness` exchanges plus an empty `RuntimeHost`
  through the scan-commit seam: phase 1 samples ping/pong RTT in both
  directions over the loopback (stamped per `ping_seq`, recorded on
  the confirming pong increment) and the scan cadence from the commit
  timestamps; phase 2 repeats the detection drill (partition, count
  ticks to `PeerDied`, restore, re-converge) to measure
  `T_peer-detect`; phase 3 samples the firmware-reported per-module
  delays from the registry (ADR-0062: "I/O firmware instruments its
  own delays and reports them; the PLC measures peer-detection") and
  the target's owner-lease expiry (the registry's old-connection
  timeout, falling back to the configured lease TTL). Everything is
  integer ticks — the procedure is reproducible: two runs over equal
  inputs produce identical statuses (pinned by test).
- **`simulator` module — firmware-reported delays.** `ModuleTiming`
  (claim / arm / output-apply ticks) as a per-module device property,
  `with_module_timing` builder, `module_timing` query, and a
  `connection_timeout` getter: the registry plays the I/O firmware
  role of instrumenting and reporting its own delays.
- **V-codes.** V4110 PerformanceDegraded, V4111 TimingGuaranteeLost —
  CSV rows (build.rs codegen, the Slice-2 mechanism) + rst pages
  mirroring V4101's template; the docs problemcode extension already
  registers the crate CSV, so the pages are the registration.
- **Guard table unchanged.** The verdict is computed and exposed; the
  shell composition slice wires it into the promotion guard. The
  fencing slice's guard table and tests are untouched.

## File map

- `compiler/ironplc-redundancy/src/timing.rs` — `TermStats`,
  `DirectionProfile`, `ModuleContribution`.
- `compiler/ironplc-redundancy/src/calibration.rs` — `Calibration`,
  `CalibrationState`, `CalibrationStatus`, `TakeoverBudget`,
  `BudgetVerdict`, `TimingAlarm`, `InvalidationReason`.
- `compiler/ironplc-redundancy/src/calibration_run.rs` —
  `run_calibration` (the deterministic commissioning procedure).
- `compiler/ironplc-redundancy/src/simulator.rs` — `ModuleTiming`,
  `with_module_timing`, `module_timing`, `connection_timeout`.
- `compiler/ironplc-redundancy/src/lib.rs` — modules, curated exports,
  crate-doc slice-5 paragraph.
- `compiler/ironplc-redundancy/resources/problem-codes.csv` —
  V4110–V4111.
- `docs/reference/runtime/problems/V4110.rst`, `V4111.rst`.
- `compiler/ironplc-redundancy/tests/calibration.rs` — budget math
  vectors, limiting-device selection, run reproducibility, degradation
  → alarm codes, recalibration invalidation.
- `specs/roadmap.md` — the Phase 5 slice-5 delivered line + the open
  item trimmed (per-port drivers remain; the calibration engine is
  delivered).

## Tasks

1. `timing` module: TermStats EMA math (hand-computed vectors),
   DirectionProfile, ModuleContribution + unit tests.
2. `simulator` module: ModuleTiming + queries + unit tests.
3. `calibration` module: engine, budget math (ADR inequality +
   recovery formula), verdict, alarms, invalidation + unit tests.
4. `calibration_run` module: the deterministic procedure + unit test.
5. V4110–V4111: CSV rows, alarm v-code mapping, rst pages.
6. `tests/calibration.rs`: hand-computed budget vectors (claim sum,
   parallel arm/output maxima, claim-start max, scan bound, the ADR
   inequality at/above/below the boundary, minimum demonstrated
   budget); limiting-device selection; two runs → identical status;
   post-calibration RTT spike → V4110 latched, link still valid;
   post-calibration scan growth past the budget → V4111 latched,
   takeover_ready false; begin() after Calibrated → guarantee
   invalid until complete; invalidate(reason) → Unqualified with the
   reason recorded.
7. Docs: roadmap line; full gates (`cd compiler && just`; specs gates
   via Git Bash); remove this plan before merge.

## Authorities

- `specs/adrs/0062-measured-failover-timing-and-network-calibration.md`
  — THE contract: the T terms, the formulas, the calibration method,
  the budget check, the readiness chain, the two timing-health
  alarms, the link-profile and per-module variable lists.
- `specs/design/external-fsm-review.md` — freshest structural
  authority; the measured-terms philosophy (no tabulated constants)
  and the seam list this slice composes.
- `specs/design/ha-redundancy-fsm.md` — ping/pong EMA/degraded
  thresholds context, the detection vocabulary the detection term
  measures.
- `specs/design/ha-redundancy-layer-architecture.md` — the
  `calibration` module's decomposition (EMA10/EMA100/max/count
  trackers, link profile, budget validation, degradation alarms), the
  simulator-binding-first rule, the measured-terms philosophy.
- `specs/design/ha-engineering-ui.md` — the CONSUMER vocabulary this
  backend implements: haCalibration/haTimingBudget fields,
  TakeoverReady/budget verdicts, per-module T contributions, limiting
  device, alarm flags.
- ADR-0064/0065 — unchanged by this slice (pair pipeline, session
  exclusivity).
- Slice 1–4 code state (`liveness.rs` exchange counting, `loopback.rs`
  binding, `simulator.rs` registry, `lease.rs` TTL, `statechart.rs`
  alarm-latch precedent, `tests/fencing.rs` node precedent).
