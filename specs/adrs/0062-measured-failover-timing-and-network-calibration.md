# Measured Failover Timing and Network Calibration

status: accepted
date: 2026-09-16

## Context

The redundancy layer (see `specs/design/ha-redundancy-fsm.md`) needs a
failover-time story the engineer can trust. A constant promise is
unattainable: takeover time depends on heartbeat transport, per-module
claim/ARM behavior, scan position, and output apply, all of which vary per
installation and per device. The honest contract is a formula whose terms
are measured, not a marketing number.

## Paradigm change

This decision replaces the classical supervision paradigm — a pre-set magic
threshold ("declare the peer dead after N missed heartbeats") — with a
measured-reality paradigm: **measure → calibrate → qualify → configure a
budget → continuously verify reality**.

- The pair's timing model is established by a commissioning calibration
  under realistic worst load (PLC application running, I/O running,
  crossload running), not on an idle controller: a heartbeat response
  delayed by a heavy scan is otherwise indistinguishable from a peer
  failure, so calibration without load is meaningless.
- Calibration measures the whole chain, in both directions (A→B→A and
  B→A→B): HA task, driver, NIC/PHY, medium, peer NIC/PHY, driver, peer HA
  task, and the reply. Scheduler behavior, IRQ affinity, NIC queues, and
  CPU load differ per direction, so a single-direction number is not
  evidence.
- The engineer configures process-reaction times, never protocol
  counters: a peer-failure confirmation time ("declare peer failed after
  T ms") and a maximum process-recovery budget. The runtime translates
  these into supervision protocol parameters; the UI never exposes
  ping periods or missed-ping counts.
- A secondary has no right to be takeover-ready until the pair has proven
  its timing model. Readiness is calibration-gated (see Decision).

## Decision

- **Failover/takeover time is NEVER promised as a constant.** It is computed
  from a formula whose terms come from (a) qualification bounds and (b)
  continuous runtime measurements of the actual installation: per-channel
  heartbeat latencies, per-module claim/ARM latencies, scan safe-point, and
  output apply. Each term is tracked as current / EMA10 / EMA100 / max /
  count. I/O firmware instruments its own delays and reports them; the PLC
  measures peer-detection. The engineering UI (Studio) shows per-device
  contributions and the limiting device.
- **TakeoverReady** = `SYNC_READY && IO_READY && RedundancyLinkValid`;
  **IO_READY** = all required inputs observable && standby connections valid
  && configs match && epochs valid.
- **Controlled (manual) switchover** targets zero old-owner-timeout via an
  explicit `RELEASE_OWNER`; **uncontrolled failover** starts at
  `max(PLC peer-detection, I/O owner-lease expiry)`.
- **Epoch is anti-stale protection ONLY** — it rejects stale packets,
  images, and claims; it is never the arbiter of who may take over (this
  prevents partition bidding wars). **OwnerLease is generated exclusively by
  the HA supervisor at scan commit** — the network task must never mint it.
- **Calibration-gated readiness.** The readiness chain is UNQUALIFIED →
  CALIBRATING → CALIBRATED → SYNCING → SYNC_READY → TAKEOVER_READY: a
  unit cannot be `TakeoverReady` until the pair's commissioning
  calibration has produced a valid link profile. A significant change
  (NIC or medium replaced, link speed changed, topology changed, runtime
  or HA protocol version changed) invalidates the qualification and
  requires a new calibration.
- **Budget validation, not silent acceptance.** The engineering UI checks
  the configured recovery budget against the calibrated worst case and
  refuses to silently accept a setting it cannot guarantee; it reports the
  minimum demonstrated budget instead.
- **Continuous verification.** After the commissioning baseline, runtime
  EMA/jitter/max tracking (online adaptation) watches for reality leaving
  the calibrated envelope: a degraded channel raises
  `HA_PERFORMANCE_DEGRADED`; a recovery budget that can no longer be met
  raises `HA_TIMING_GUARANTEE_LOST`. Failover thresholds are not
  automatically retuned.

## Consequences

- Studio can display an honest, per-installation takeover estimate and name
  the device that dominates it, instead of quoting a fixed bound.
- Manual swap avoids the old-owner timeout entirely; uncontrolled failover
  pays detection plus lease expiry, and both terms are measured.
- Epoch and OwnerLease have disjoint producers and purposes, so a network
  fault cannot fabricate ownership and a partition cannot escalate epoch.
- The pair ships no unexplained magic number: every threshold an engineer
  sees is either measured or validated against measurements, and a
  configuration the installation cannot honor is rejected at engineering
  time rather than discovered at failover time.
- Timing regressions after commissioning (cable aging, added adapters,
  load growth) surface as degradation alarms while redundancy still works,
  instead of silently eroding the failover guarantee.

## Timing formulas

- Process recovery: `T_recovery = T_peer-detect + T_claim + T_arm +
  T_scan-safe-point + T_output-apply`.
- Claim start: `T_claim-start = max(T_plc-peer-detection,
  T_io-owner-lease-expiry)` — both independent proofs must agree.
- Sequential claim (v1): `T_claim = sum_i(T_claim,i)`; estimate =
  `sum_i(EMA_claim,i)`; qualification bound =
  `sum_i(MaxQualified_claim,i)`.
- Future parallel Forward_Open: `T_claim ~= max_i(T_claim,i)` — no firmware
  change required.
- Single-exclusive profile: `T_claim^single = T_owner-release +
  T_ForwardOpen + T_verify`.
- Preconnected profile: `T_claim^redundant = T_owner-failure-confirm +
  T_arbitration + T_ARM` (no new connection on the takeover path).
- Controlled (manual) switchover: `T_old-owner-timeout = 0` via explicit
  `RELEASE_OWNER`.
- Gates: `TakeoverPermission = PeerFailureConfirmed &&
  OldOwnerAuthorityExpired && SyncReady && IoReady`; `TakeoverReady =
  SYNC_READY && IO_READY && RedundancyLinkValid`.
- Scan safe-point is phase-aware, not a flat scan time: `T_safepoint =
  T_next-commit - t_takeover`, bounded `0 <= T_safepoint <= T_scan,max`.
  The online estimator therefore shows two numbers: predicted recovery if
  the failover happened now, and the calibrated worst case.
- Budget check: `T_detect + T_claim,max + T_scan,max + T_output,max <=
  T_recovery-budget`; when the inequality fails, the UI reports the minimum
  demonstrated budget and does not apply the configuration.
- Detection and resume terms are measured like every other term:
  `T_recovery` decomposes as detection (the configured confirmation time),
  HA-FSM decision, claim, execution-context resume, scan safe-point, and
  output apply; none is assumed zero.

## Variables for the engineering UI (Studio tabs)

- Per I/O module (each as current / EMA10 / EMA100 / max / count):
  ClaimLatency, ArmToOutputLatency, OwnerLossDetectionLatency,
  FaultApplyLatency, InputSampleJitter, OutputCommitJitter.
- Per pair channel: heartbeat latency, peer-detection time, owner-A
  connection quality, owner-B connection quality (continuously measured
  pre-fault).
- Ownership status view (from T->O status): OwnerState, OwnerControllerId,
  OwnerEpoch, OwnerConnectionAge, OwnerOutputSeq, OwnerArmed,
  OwnerPacketAge, ModuleState, FaultCode, Port1State, Port2State.
- Timestamped events: ForwardOpen received, owner accepted, ARM received,
  output committed, last owner packet, timeout detected, safe commanded,
  safe applied.
- Ownership barrier view: per-module profile (preconnected/reconnect) and
  its T contribution, the limiting device, worst ownership recovery.
- IO_READY breakdown: required inputs observable, standby connections
  valid, configs match, epochs valid.
- HA link profile (commissioning calibration, both directions): RTT fast
  EMA / slow EMA, RTT min / max / jitter envelope, loss rate, max
  consecutive loss, per-side processing latency, per-side scan time and
  scan jitter. Recalibration events and the calibration state of each
  direction are visible.
- Per-controller scan statistics feeding the safe-point term: scan EMA10 /
  EMA100 / max / jitter envelope, plus current scan phase for the
  predicted-if-now estimate.
- Configured engineer parameters and their validation: peer-failure
  confirmation time, maximum process-recovery budget, calculated worst
  case, predicted-if-now value, and the budget-check result (qualified or
  minimum demonstrated budget).
- Timing health alarms: `HA_PERFORMANCE_DEGRADED` (reality left the
  calibrated envelope) and `HA_TIMING_GUARANTEE_LOST` (recovery budget no
  longer attainable).

Firmware MUST timestamp each stage and compute the per-module metrics
locally, reporting them upward (per the philosophy of this ADR: measure
reality, never assume constants).
