# HA Phase 5 slice 6: engineering UI surface (HA engineering UI contract)

Date: 2026-09-21
Status: draft
Branch: feature/ha-engineering-ui

## Goal

Land the HA engineering UI milestone of the Phase 5 redundancy layer in
one PR, per the contract in `specs/design/ha-engineering-ui.md`: (1) the
backend surface — the redundancy crate's typed `HaCommand`/`HaResponse`
vocabulary + line-delimited JSON codec following the ADR-0055 pattern
(mirroring `compiler/runtime/src/commands.rs`), one `execute` dispatch,
one line in / one line out, no push channel; (2) shell composition in
`ironplcvm serve` — the composition root composes the redundancy shell,
default standalone mode answering honest standalone HA state, and a
`--ha-simulated-peer` dev/demo flag composing a loopback peer (simulator
binding) so every HA state is exercisable end-to-end through the session,
with zero runtime coupling (redundancy → runtime, one-way); (3) the VS
Code HA panel — the five Studio tabs (Pair Overview / Calibration /
Ownership & Barrier / Timing Budget / Events) rendered in the extension's
established Explorer-tree idiom over the device-panel infrastructure,
with badges (TakeoverReady, budget verdict), per-module T contributions,
the limiting device, calibration run control, and the events list;
illegal actions hidden/disabled per ADR-0064/0065 (swap offered only at
SYNC_READY, single session); (4) tests: rust serve e2e for each HA query
in both standalone and simulated-peer modes (scripted-session pattern),
redundancy-crate unit tests for the shell and the codec, vscode unit
tests for the HA panel view models, protocol parsing, and command wiring
with the established mock-transport pattern; (5) docs: the roadmap Phase
5 status line, the ha-engineering-ui.md contract amendment (third
engineer action + standalone presentation + the new V-code), the V4112
docs page. No new ADR (the contract and ADR-0062/0063/0064/0065 record
the decisions). No real EtherNet/IP binding, no real two-process UDP
pair — the loopback simulator remains the test vehicle.

## Architecture

- **`ironplc-redundancy` `commands` module — the wire vocabulary.**
  `HaCommand` (serde-tagged, camelCase: `haStatus`, `haCalibration`,
  `haBarrier`, `haIoReady`, `haTimingBudget`, `haEvents`,
  `haCommandedSwap`, `haRunCalibration`, `haSetTimingBudget{peerFailureConfirmation,
  recoveryBudget}`) and `HaResponse` (`haStatus`, `haCalibration`,
  `haBarrier`, `haIoReady`, `haTimingBudget`, `haEvents`, `ack`,
  `error`), `parse_ha_command`/`render_ha_response`, and
  `execute(command, &mut Shell)` — the whole command mapping, one
  variant → one `Shell` call, exactly like the runtime's
  `commands::execute`. Payloads follow the contract's field names:
  TermStats block {current,min,ema10,ema100,max,count} for every
  measured term; per-unit blocks for both units of the pair; the
  TakeoverReady verdict with its three inputs broken out; alarm flags
  (HA_PERFORMANCE_DEGRADED / HA_TIMING_GUARANTEE_LOST /
  REDUNDANCY_LOST); the IO_READY breakdown with the failing item; the
  budget inequality verdict with the minimum demonstrated budget.
  Errors are the crate's CSV V-codes (V4108 swap-refused, V4111
  budget-unattainable, new V4112 pair-required) in the runtime's
  error shape (`vCode`, `message`, plus `minimumDemonstrated` on a
  V4111 refusal).
- **`ironplc-redundancy` `shell` module — the composable HA state.**
  `Shell`: the redundancy shell of one served process, the state the
  commands execute against. Owns the unit's SYNC/CONTROL charts, the
  calibration engine, the pair liveness exchange over the loopback
  binding, the simulated peer (its own charts/liveness/port), the
  simulator registry fencing client, the epoch, the bounded event
  ring, and the wired generation counters. `Shell::standalone()` (no
  pair: statechart does not run, queries answer the honest standalone
  shape, actions refuse V4112) and `Shell::simulated(config, registry,
  required, budget)` with `start_up() -> AdmissionVerdict`
  (admission → sync → boot claim → commissioning calibration — reusing
  `admit`, `claim_barrier`, `ownership_barrier`, `run_calibration`),
  `tick()` (one pair exchange + chart driving + fencing observation +
  registry cadence, the session's command cadence is the simulation
  clock), `commanded_swap()` (synchronous: release side releases,
  claiming side claims through the barrier, configured roles exchange
  on completion), `run_calibration()` (synchronous commissioning run),
  `set_timing_budget()`, `on_scan_commit(ScanCommit)` (the scan-commit
  seam: epoch mint, lease renewal, output-commit stamping, scan-term
  feed), and the query surface the payloads render. Simulator-binding
  controls (`set_link_partitioned`, `fault_module`) expose the
  loopback/registry injection points so every HA state (deSYNC,
  REDUNDANCY_LOST, degraded, alarms) is reachable in tests; the wire
  contract stays clean of simulation commands. The shell never names
  the host; the composition root applies the permit verdict to the
  host latch (`permit_execution`/`revoke_execution_permit`).
- **Prefactors of slice-5 code (small, additive).** `TermStats` gains
  `min` (the jitter envelopes the contract renders; lets
  `DirectionProfile` delegate its `rtt_min`/`jitter_envelope` instead
  of duplicating the tracking). `DirectionProfile` gains a per-side
  `processing` tracker (ADR-0062's "per-side processing latency"),
  fed by the commissioning run. `Calibration` gains `set_budget`
  (re-evaluates the guarantee) and `CalibrationStatus` exposes the
  frozen commissioning envelope (the Calibration tab's baseline).
  `ModuleRegistry` gains `is_online` (the IO_READY / failing-item
  input). No behavior changes elsewhere.
- **`vm-cli` composition.** `Serve` gains `--ha-simulated-peer`.
  `serve_session` grows one `Option<&mut Shell>` parameter: each
  command line first tries the runtime `parse_command`, then the HA
  `parse_ha_command` (disjoint vocabularies; a line failing both is
  the codeless codec error exactly as today), executes against host
  or shell, and in simulated mode ticks the shell and reconciles the
  permit latch per the shell's verdict before the response renders.
  `drive_scan_round`'s commit closure calls `shell.on_scan_commit`
  when a shell is composed (the standalone debug-log composition
  stays). `tcp::serve_tcp` passes the same flag through — one
  session protocol, both transports. The demo binding's device
  properties (module timings, connection timeout, demo owner
  identities) live in `serve.rs`, the composition root.
- **VS Code extension.** `haProtocol.ts` (vscode-free): the HA wire
  types and strict parsers (the `parseStatus`/`parseIdentity` idiom,
  sharing the exported field helpers). `hotEditSession.ts`: the
  pending-request FIFO now resolves raw lines and parses per caller;
  `haStatus()`/`haCalibration()`/`haBarrier()`/`haIoReady()`/
  `haTimingBudget()`/`haEvents()`/`haCommandedSwap()`/
  `haRunCalibration()`/`haSetTimingBudget()` methods. `haPanelLogic.ts`
  (vscode-free): the five-tab view model as tree sections + rows over
  `PanelRow`, the badge strings, and the legality predicates
  (`canCommandSwap` = connected pair at SYNC_READY, etc.) keyed by the
  command-id constants. `haPanel.ts` (glue): the `ironplc.haPanel`
  Explorer view registered like the device panel, refreshed on the
  connection feature's model event (the heartbeat cadence — the
  contract's slow poll, no new timer) and after its own actions; the
  four commands (`ironplc.haRefresh`, `ironplc.haCommandedSwap`,
  `ironplc.haRunCalibration`, `ironplc.haSetTimingBudget`) wired to
  the manager's single active session (ADR-0065) with guards that
  hide/ refuse illegal actions. `package.json`: the view, the
  commands, the activation event. No new E-codes (server V-codes
  render as-is, as hot edit does).

## File map

- `compiler/ironplc-redundancy/src/commands.rs` — new: wire
  vocabulary, payloads, codec, `execute`, tests.
- `compiler/ironplc-redundancy/src/shell.rs` — new: `Shell`
  (standalone + simulated pair), bring-up/tick/swap/queries/events,
  tests.
- `compiler/ironplc-redundancy/src/timing.rs` — `TermStats.min`;
  `DirectionProfile.processing`; delegation of rtt_min/jitter.
- `compiler/ironplc-redundancy/src/calibration.rs` — `set_budget`,
  envelope exposure on `CalibrationStatus`.
- `compiler/ironplc-redundancy/src/simulator.rs` — `is_online`.
- `compiler/ironplc-redundancy/src/lib.rs` — modules, re-exports,
  slice-6 doc paragraph.
- `compiler/ironplc-redundancy/resources/problem-codes.csv` +
  `docs/reference/runtime/problems/V4112.rst` — the pair-required
  code.
- `compiler/vm-cli/src/serve.rs` — HA composition, session wiring;
  tests move to `compiler/vm-cli/src/serve_tests.rs` (module-size
  split) + HA e2e tests.
- `compiler/vm-cli/src/{main.rs,tcp.rs,Cargo.toml}` — the flag, the
  pass-through, the dependency.
- `integrations/vscode/src/{haProtocol.ts,haPanelLogic.ts,haPanel.ts}`
  — new; `hotEditSession.ts`, `extension.ts`, `package.json` —
  additive.
- `integrations/vscode/src/test/unit/{haProtocol.test.ts,
  haPanelLogic.test.ts,hotEditSession.test.ts}` — tests.
- `specs/design/ha-engineering-ui.md` — the contract amendment
  (haRunCalibration, standalone presentation, V4112); `specs/roadmap.md`
  — the Phase 5 slice-6 line.

## Tasks

1. Prefactors (TermStats.min, DirectionProfile.processing,
   Calibration::set_budget + envelope, registry is_online) + their
   unit tests.
2. `shell.rs`: standalone + simulated bring-up, tick, commanded swap,
   calibration run, timing budget, events ring, injection hooks;
   unit tests covering every HA state (SYNC_READY happy path, swap
   + V4108 refusal, V4111 budget refusal, partition → deSYNC,
   module fault → REDUNDANCY_LOST, alarm transitions).
3. `commands.rs`: vocabulary, payloads, codec, execute mapping;
   wire-shape tests per command.
4. vm-cli: `--ha-simulated-peer`, session composition, permit
   reconciliation, scan-commit wiring, tcp pass-through; serve e2e
   for each HA query in both modes; module-size split of serve
   tests.
5. Vscode: haProtocol + session methods + haPanelLogic + haPanel
   glue + package.json; unit tests (protocol round-trips, view
   models, legality, command wiring over the mock transport);
   eslint on changed files.
6. Docs: V4112 page, CSV row, ha-engineering-ui.md amendment,
   roadmap Phase 5 line.
7. Gates: `cd compiler && just`; specs gates via Git Bash; vscode
   `npm run compile`/`test:unit`/lint; iterate to green; remove this
   plan before merge.
