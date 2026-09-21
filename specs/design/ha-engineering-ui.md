# Spec: HA Engineering UI Contract

## Overview

This spec defines the contract between the redundancy layer and the
engineering UI (Studio): the backend command surface the redundancy crate
exposes, the Studio tab structure that consumes it, and the mapping onto
the three existing clients (VS Code extension, MCP server, `ironplcvm
serve`). It is a contract only — no implementation, no rendering
technology, no transport beyond the established pattern.

This spec builds on:

- **[ADR-0062](../adrs/0062-measured-failover-timing-and-network-calibration.md)**:
  the timing formulas, the per-module / per-channel / ownership / event
  variable lists, and the alarms this surface carries
- **[HA Redundancy FSM](ha-redundancy-fsm.md)**: the SYNC/CONTROL
  statechart, configured roles, and the ping/pong contract whose state the
  UI renders
- **[HA Redundancy Layer Architecture](ha-redundancy-layer-architecture.md)**:
  the crate placement, the execution permit, and the runtime seams the
  backend surface sits on

## Design Goals

1. **One vocabulary, thin clients** — the backend surface is a typed
   command enum with a line-delimited JSON codec, following ADR-0055; every
   client is a serializer
2. **Every number is measured** — the UI never displays an assumed
   constant; each item traces to an ADR-0062 variable or an FSM state
3. **Alarms are data, not popups** — degradation and guarantee-loss alarms
   are readable state and timestamped events, so every client renders them
   the same way
4. **No new wire machinery** — request/response only; v1 has no push
   channel

## Backend Surface

The surface lives in the redundancy crate's `commands` module (crate
placement per
[HA Redundancy Layer Architecture](ha-redundancy-layer-architecture.md)),
following the existing pattern: a serde-tagged `Command`/`Response` enum
(`compiler/runtime/src/commands.rs:32`), one `execute` dispatch
(`compiler/runtime/src/commands.rs:284`), one line in, one line out.

All commands are queries or engineer actions against the redundancy
supervisor; none mutates the application. The query set:

### `haStatus`

The pair overview. Payload:

- Pair identity (`PairId`) and each unit's configured role and
  `ControllerId`
- Both units' chart states: SYNC substate (deSYNC / SYNCING /
  SYNC_READY) and CONTROL substate (IDLE / CLAIMING / ACTIVE /
  ACTIVE_DEGRADED / REDUNDANCY_LOST), per
  [HA Redundancy FSM](ha-redundancy-fsm.md)
- Current epoch and both generation counters (application + committed
  state generation, per the ping/pong packet fields)
- `TakeoverReady` verdict with its three inputs (`SYNC_READY`,
  `IO_READY`, `RedundancyLinkValid`) broken out, per ADR-0062
- Active alarm flags: `HA_PERFORMANCE_DEGRADED`,
  `HA_TIMING_GUARANTEE_LOST`, `REDUNDANCY_LOST`

### `haCalibration`

Per-channel calibration state, per ADR-0062's link profile:

- Per channel and direction (A→B→A, B→A→B): ping/pong round-trip latency
  as current / EMA10 / EMA100 / max / count; RTT min / max / jitter
  envelope; loss rate; max consecutive loss
- Per-side processing latency, scan time, and scan jitter
- Calibration run state (UNQUALIFIED / CALIBRATING / CALIBRATED), the
  commissioning baseline, and the last recalibration trigger

### `haBarrier`

The ownership barrier view, per ADR-0062:

- Per required I/O module: profile (preconnected / reconnect), owner
  state (from T→O status: OwnerState, OwnerControllerId, OwnerEpoch,
  OwnerConnectionAge, OwnerOutputSeq, OwnerArmed, OwnerPacketAge,
  ModuleState, FaultCode, Port1State, Port2State), and claim latency
- Each module's `T` contribution to takeover, the limiting device, and
  the worst ownership recovery

### `haIoReady`

The `IO_READY` breakdown, per ADR-0062: required inputs observable,
standby connections valid, configs match, epochs valid — each as its own
boolean with the failing item identified when false.

### `haTimingBudget`

The failover formula with live terms, per ADR-0062:

- Each term (`T_peer-detect`, `T_claim`, `T_arm`, `T_scan-safe-point`,
  `T_output-apply`) as qualification bound vs. measured current/EMA
- Configured engineer parameters: peer-failure confirmation time and
  maximum process-recovery budget
- The phase-aware `T_safepoint` inputs: current scan phase, scan
  EMA10 / EMA100 / max / jitter
- The budget inequality verdict: calculated worst case, predicted-if-now
  value, and QUALIFIED or the minimum demonstrated budget

### `haEvents`

The timestamped event ring, per ADR-0062: ForwardOpen received, owner
accepted, ARM received, output committed, last owner packet, timeout
detected, safe commanded, safe applied — plus calibration events and the
two timing-health alarms. A bounded ring, read on poll.

### Engineer actions

- `haCommandedSwap` — the commanded Primary↔Secondary swap of the FSM's
  promotion case (a), refused outside SYNC_READY
- `haSetTimingBudget` — sets the peer-failure confirmation time and the
  recovery budget; the response carries the budget-check verdict, and a
  failing budget is reported, never silently applied (ADR-0062)
- `haRunCalibration` — runs the commissioning/recalibration run
  (ADR-0062); on the demo binding the run is synchronous, so the
  acknowledgment renders after the run state it acknowledges has changed

### Standalone presentation

When no pair is configured, the queries answer the honest standalone
shape — `haStatus` carries `standalone: true` with the chart-state
fields absent, and the other payloads carry empty profiles and zero
counters — and the engineer actions refuse with V4112 (`PairRequired`)
instead of acknowledging a no-op. `ironplcvm serve` composes this
standalone shell by default; `--ha-simulated-peer` composes the
loopback peer (the simulator binding) so every HA state is exercisable
end-to-end through the session, with the session's command cadence as
the simulation clock.

### Error codes

HA failures are stable V-codes from the redundancy crate's own
`resources/problem-codes.csv`, generated by the same build.rs CSV
convention (`compiler/runtime/build.rs:1`, mirroring
`compiler/runtime/resources/problem-codes.csv`). A V41xx block is
proposed; allocation and the docs lifecycle follow
`specs/steering/problem-code-management.md`. A codec error (a line that
does not parse) keeps no code, matching the existing layer. The
registered surface so far: V4101–V4111 from the FSM, fencing, and
calibration slices, plus V4112 (`PairRequired`) for the standalone
refusal above.

### Refresh model

The codec is request/response; there is no push channel in v1. Clients
poll queries; an action response is written only after the state it
acknowledges has actually changed, matching the `serve` convention
([vm-cli.md](vm-cli.md), REQ-VC-vm-cli-023). "On-change" below means: the
client re-queries after its own action, and watches the alarm flags in
`haStatus` between polls.

## Studio Tab Structure

Five tabs. Each lists its purpose, its exact data items, the command that
feeds it, and how it refreshes.

### Pair Overview

*Purpose:* is the pair healthy, and who may command outputs right now.

*Data:* both units' SYNC/CONTROL substates with the guard-relevant
reasons; configured roles; epoch; generations; `TakeoverReady` with its
three inputs; alarm flags.

*Fed by:* `haStatus`. *Refresh:* polled on a slow cadence; immediate
re-query on any alarm flag.

### Calibration

*Purpose:* prove the pair's timing model is measured, current, and valid
— the commissioning baseline and its drift.

*Data:* per-channel RTT current / EMA10 / EMA100 / max / count, jitter
envelope, loss rate, max consecutive loss; per-side processing and scan
statistics; calibration run state; last recalibration trigger and the
invalidation reason when requalification is required.

*Fed by:* `haCalibration`. *Refresh:* polled while a calibration run is
active; on-change otherwise.

### Ownership & Barrier

*Purpose:* the fencing state of every required I/O module and the cost of
the barrier, device by device.

*Data:* per-module profile, owner state block (OwnerState through
Port2State), claim latency; per-module `T` contribution; the limiting
device; worst ownership recovery.

*Fed by:* `haBarrier`. *Refresh:* polled; immediate re-query around any
CLAIMING transition observed in `haStatus`.

### Timing Budget

*Purpose:* the honest failover estimate — every formula term as a
measured number, the configured budget, and the verdict.

*Data:* per-term qualification bound vs. measured; confirmation time and
recovery budget; calculated worst case; predicted-if-now; the
budget-check verdict; scan phase and scan statistics behind
`T_safepoint`.

*Fed by:* `haTimingBudget`. *Refresh:* on-change (configuration edits,
qualification changes); polled for the live predicted-if-now value.

### Events

*Purpose:* the timestamped ledger a post-incident review reads.

*Data:* the ADR-0062 event list, calibration events, and alarm
transitions, each with its timestamp.

*Fed by:* `haEvents`. *Refresh:* polled; appended, never rewritten.

## Client Mapping

### VS Code extension

Thin client per `specs/steering/extension-standards.md`: all logic in
unit-testable, vscode-free modules; UI strings through the generated
`formatProblem` helper with `E####` codes, never hardcoded.

- A status bar item fed by `haStatus` (the slow poll): pair state at a
  glance, alarm flags as the attention signal.
- The five tabs live in a custom editor panel (the extension's existing
  custom-editor capability is the pattern); the panel is a renderer over
  the command responses and holds no HA logic.
- Engineer actions (`haCommandedSwap`, `haSetTimingBudget`) are
  registered commands, each shipping with its test per the extension
  testing gates.

### MCP server

AI-assisted HA diagnostics as `ha_*` tools, one tool per command,
following `compiler/mcp/src/tools/hot_edit.rs:1-9`: the tools own no
protocol logic, every call maps onto one command, every refusal returns
the stable V-code plus the message; the server only serializes. Session
state follows the one-session-per-process convention (ADR-0056).

### `ironplcvm serve`

The serve session accepts the new commands on the same one-line-in /
one-line-out protocol ([vm-cli.md](vm-cli.md), REQ-VC-vm-cli-019), so a
scripted client drives HA diagnostics exactly as it drives hot edit
today. The session composes the runtime's command layer and the
redundancy crate's layer; neither crate depends on the other.

## Non-Goals

- No implementation of any of this surface.
- No web framework, charting library, or rendering technology choices —
  the tabs are data contracts, not widgets.
- No push/streaming transport: v1 is request/response; a notification
  channel is a follow-up decision if polling proves insufficient.
- No editor monitoring/edit-mode gating while connected: that gate is
  owned by [Online Editing UX](online-editing-ux.md); this contract only
  supplies the HA state the monitoring overlays render.
- No new authentication/authorization model for the engineering surface.
- No changes to the runtime's hot-edit command vocabulary or V-codes.

## Out of Scope

- The redundancy supervisor internals that produce these values (owned by
  [HA Redundancy Layer Architecture](ha-redundancy-layer-architecture.md)
  and the FSM spec).
- I/O firmware telemetry formats behind the per-module values (the I/O
  firmware contract is a roadmap audit item).
