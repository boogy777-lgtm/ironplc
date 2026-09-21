# Spec: DCS Firmware Task v2 — Audit Against Project State

## Overview

This document audits the owner-provided reference task
`docs/reference/dcs-firmware-task-v2-ru.md` (v2.0, 2026-09-21; Part A defects
D01–D24, Part B requirements §1–§21, Part C checks T01–T28 and release gates
G01–G10) against the actual IronPLC project state — its committed specs, ADRs,
and the code they describe. It is analysis only: it changes no architecture,
introduces no requirements, and assigns dispositions for the gaps it finds.

The task document is preserved verbatim as a reference and explicitly declares
itself "not a testimony of product production readiness" [Task, header]. This
audit holds the project to the same standard: every claim below is verified
against a file and line, and no production readiness is claimed anywhere.

This audit builds on:

- **HA Redundancy Layer Architecture**
  (`specs/design/ha-redundancy-layer-architecture.md`) and **HA Redundancy
  FSM** (`specs/design/ha-redundancy-fsm.md`) — the redundancy layer whose
  mechanisms carry much of the task's Part B §13
- **External FSM Architecture Review**
  (`specs/design/external-fsm-review.md`) — the triage verdicts for the task
  document's architectural predecessor
- **HA Architecture Readiness** (`specs/design/ha-architecture-readiness.md`)
  — the reuse/gap inventory this audit re-verifies rather than repeats
- **ADR-0052 through ADR-0065** — the online-change, transport, timing,
  pair-edit, and session decisions the task's requirements intersect

## 1. Verdict and Method

**Verdict: the task document is a sound, largely compatible requirements
source for the controller-facing phases the project has not reached yet; the
project's committed decisions already satisfy the task's *logical* core
(ownership separation, barrier commit, measured timing, two-channel rule,
sequence-vs-statistics honesty) but the task's *physical* core (HW_KEY,
HW_DIAG, OS ports, output enforcement on hardware, watchdog containment,
firmware update, security) is absent by scope, and two committed decisions
genuinely conflict with task MUSTs and need an owner ruling.**

Method: the task document was read in full. Every Part A defect D01–D24 and
every major Part B section was mapped to project state and verified against
code and spec lines, not against other documents' claims alone. Where a design
document and the code could disagree, the code was treated as authoritative;
the roadmap's "delivered" Phase 5 slices 1–6 of 2026-09-21 all check out in
code (permit latch, pair link, crossload, fencing, calibration, engineering
UI — verified at the citations in §2).

Scope caveat, stated plainly: the task targets firmware for a full DCS/BPCS
controller with FreeRTOS and qualified-Linux ports, a physical mode key,
hardware diagnostics, and physical output enforcement [Task §1, §7–§9, §12,
§19]. IronPLC today is a VM-centered compiler/runtime: the execution kernel is
served and run as desktop processes, the redundancy layer's fencing and
transport run on a loopback simulator binding, and there is no I/O stack, no
RTOS, and no hardware target
[specs/design/external-fsm-review.md:13-23],
[compiler/vm-cli/src/serve.rs:87],
[specs/roadmap.md:200-210]. The audit therefore uses
OUT-OF-SCOPE-BY-DECISION as a first-class verdict, matching the project's own
triage vocabulary [specs/design/external-fsm-review.md:29-33].

## 2. Coverage Matrix

Verdict vocabulary: **ALIGNED** (mechanism exists, cited), **PARTIAL**
(something exists, cited, with the missing part named), **ABSENT** (nothing
exists), **CONFLICT** (a committed decision contradicts the task), **OOS-DEC**
(out of scope by committed decision).

### 2.1 Part A defects D01–D24

| ID | Verdict | Evidence and missing part |
|---|---|---|
| D01 Narrow public contracts, no foreign enums | ALIGNED | One-way dependency redundancy → runtime; the runtime names no role, pair, or epoch [specs/design/ha-redundancy-layer-architecture.md:31-34]; typed command enums are the only surface [compiler/runtime/src/commands.rs:28]. |
| D02 Logical ownership ≠ failure domain; independent containment | PARTIAL | Ownership is physically separated by crate; physical containment is delegated to the future I/O firmware's autonomous watchdog — a roadmap audit item, not code [specs/roadmap.md:215-230]. No independent hardware path exists. |
| D03 `RUNNING → os.ready` not instantaneous; bounded reaction | PARTIAL | The fail-closed analogue exists: a permit revoked between request and boundary cancels the pending swap terminally [compiler/runtime/src/host.rs:701-707]. An OS context/FSM and a stale-freshness model do not exist at all. |
| D04 HW_DIAG publishes facts; owner decides suitability | ABSENT | No diagnostics subsystem. Nearest echo: the `IO_READY` decomposition with the failing item identified [specs/design/ha-engineering-ui.md:88-93]. |
| D05 Permission ≠ intent ≠ start ≠ restart policy | PARTIAL | Permit grant/revoke is separate from admission's start decision [compiler/runtime/src/host.rs:204-221], [specs/design/ha-redundancy-fsm.md:90-110]. Restart-policy variety (cold/warm/watchdog/fault) is undefined — one boot path exists. |
| D06 Single authoritative output-write boundary | PARTIAL | Designed and simulated: `CAN_EXECUTE_OUTPUTS = ACTIVE && owns_all_required_io`, fencing enforced at the I/O target [specs/design/ha-redundancy-fsm.md:310-313], exercised via the simulator binding [compiler/ironplc-redundancy/src/simulator.rs:21]. No physical boundary, no protocol-FB/force bypass coverage yet. |
| D07 No boolean soup: versions, boot identity, freshness | PARTIAL | Epoch anti-stale + epoch-stamped OwnerLease + CRC-guarded frames + generation newtypes [compiler/ironplc-redundancy/src/lease.rs:13-19]. The task's `Evidence<T>`/`Stale` validity model as such is not adopted — refuted as a new ContractStore layer in the predecessor review [specs/design/external-fsm-review.md:39-41]; fail-closed refusals carry the load instead. |
| D08 Re-check rights at commit; revocation wins | ALIGNED | Boundary commit is the single re-check point; a revoked permit cancels the swap terminally [compiler/runtime/src/host.rs:701-707]; slice-1 acceptance [specs/roadmap.md:142-148]. |
| D09 Composition root allowed; no ServiceManager | ALIGNED | Composition root at the binary entry point; "no registry, no event bus, no plugin point" [specs/design/ha-redundancy-layer-architecture.md:121-130,218-219]. |
| D10 Independent watchdog / output timeout / hardware inhibit | ABSENT | Design intent only: the IRONPLC_HA_IO profile's autonomous owner watchdog and SAFE_VALUE/HOLD_LAST/RAMP_TO_SAFE policies [specs/roadmap.md:217-230]. No mechanism, no hardware, no failure-coverage proof. |
| D11 RTOS→Linux port is not automatic real-time equivalence | OOS-DEC | No RTOS/Linux port work exists; the host is `std`, only the container/VM kernel is `no_std` [compiler/container/src/lib.rs:1]; MCU-facing sections bind only after a target-side port [specs/design/external-fsm-review.md:62]. The task's requalification principle is accepted for whenever a port happens. |
| D12 Candidate load must not break working Original | ALIGNED | Staging validates first; the normal artifact keeps executing; rejection leaves the application untouched [compiler/runtime/src/host.rs:251-299]; the crossload refusal path never pretends redundancy-ready [compiler/ironplc-redundancy/src/crossload.rs:104-120]. |
| D13 TEST mode ≠ Test Edits | PARTIAL | `HostMode::Testing` is Test Edits (execution-selector switch) [compiler/runtime/src/host.rs:54-59]. The task's TEST mode (IEC logic runs, application-driven physical effects suppressed, protocol-FB bypass closed) has no mechanism; the decided future gate is `CAN_EXECUTE_OUTPUTS` [specs/design/external-fsm-review.md:91]. |
| D14 StableStateId match ≠ no process jump | PARTIAL | Honest scoping exists: failover is "deterministic but honestly not bumpless" [specs/design/ha-redundancy-fsm.md:52-53]; state preservation is documented as state-only (ADR-0064 decision (d)). A recorded *behaviour-change risk + engineering acceptance* artifact (the task's counterexample discipline) does not exist. |
| D15 O(1) switch hides barrier/quiescence cost | PARTIAL | The swap is synchronous inside the boundary [compiler/runtime/src/host.rs:701-729]; the project already refuses the O(1) myth and owes a measured pause bound [specs/design/external-fsm-review.md:175-179]. Separate `T_barrier`/`T_commit` budgets are not applicable yet (single-task), and the migration-pause measurement is still an open action. |
| D16 Sync readiness vs role vs fencing vs write, separated | ALIGNED | Four axes are distinct mechanisms: SYNC subchart, CONTROL subchart, P/I/S detection case table, fencing client [compiler/ironplc-redundancy/src/statechart.rs:594-618], [specs/design/ha-redundancy-fsm.md:511-550]. |
| D17 Two-lost-links rule vs takeover-at-any-failure | ALIGNED | `!P && !I && S` is candidacy, not promotion; a partitioned peer still owns outputs, acquisition fails, REDUNDANCY_LOST [specs/design/ha-redundancy-fsm.md:295-308]. The same irreducible trade-off the task states is implicit here and should be made explicit in the availability model (§6, action 5). |
| D18 Protocol sequence sequential; +1000 only as legacy UI projection | ALIGNED | Wire `ping_seq`/`pong_seq` advance strictly +1 [compiler/ironplc-redundancy/src/liveness.rs:255-270]; the +1/+1000 counter is a separate `penalty` field, documented "diagnostics input on the engineering HMI, never the arbiter of takeover" [compiler/ironplc-redundancy/src/liveness.rs:18-30,273-276]. Exactly the task's fix. |
| D19 EMA is trend, not worst-case bound | ALIGNED | `TermStats` = current/min/EMA10/EMA100/max/count [compiler/ironplc-redundancy/src/timing.rs:27-40]; the budget check consumes `max` (the qualification bound) [compiler/ironplc-redundancy/src/timing.rs:111-115], [compiler/ironplc-redundancy/src/calibration.rs:184-220]; EMA terms feed only the predicted-if-now estimate with a bounded safepoint [compiler/ironplc-redundancy/src/calibration.rs:604-617]; detection time is the engineer-configured confirmation time validated by calibration-run drills [compiler/ironplc-redundancy/src/calibration_run.rs:1]. |
| D20 Hold-last-100 ms is not universal; per-group policy | PARTIAL | Per-module safe-state policies are a decided audit item (SAFE_VALUE/HOLD_LAST/RAMP_TO_SAFE per module) [specs/roadmap.md:217-230]; no output groups and no per-reason policy table (the PROGRAM/TEST/fault/loss/HA/force rows of [Task §12.2]) exist. |
| D21 Firmware update ≠ application online change | ABSENT | No firmware-update transaction exists at all. The application-level analogue (A/B slots, single commit point, crash table) is implemented design (ADR-0064 amendment); a firmware maintenance transaction, version-pair policy, and bootloader concerns are untouched. |
| D22 Keys/permissions ≠ authentication | CONFLICT | The task makes authentication/authorization a MUST [Task §17]; the project defers it: no engineer identity, one unauthenticated engineering session, wire authentication "a later decision" (ADR-0065 decision 2; ADR-0063 deferred item). Needs an owner ruling (§3). |
| D23 Same-class test; new class may add a mechanism | ALIGNED | Workspace doctrine; the predecessor review reaches the same N+1 verdict and classifies HA as a new mechanism class [specs/design/external-fsm-review.md:58]; the layer adds a crate, not runtime machinery [specs/design/ha-redundancy-layer-architecture.md:42-71]. |
| D24 Measurable release gates, not pretty FSMs | PARTIAL | The *discipline* exists and is build-enforced (compile, coverage ≥ 85 %, clippy, fmt, duplicate checks, requirement→test traceability with an empty-UNTESTED meta-test). The task's *product* gates (fault injection, HIL, FAT/SAT) do not — see §5. |

Tally for D01–D24: **9 ALIGNED, 10 PARTIAL, 3 ABSENT, 1 CONFLICT, 1 OOS-DEC.**

### 2.2 Part B major sections

| Section | Verdict | Evidence and missing part |
|---|---|---|
| §2 Constraints 1–7 (no ServiceManager; IEC 61131-3; immutable active code + two banks; runtime as final executor; LiveStateStore exact match; MigrationPlan; PRIMARY/SECONDARY) | ALIGNED (one naming deviation) | Immutable containers + normal/candidate in RAM + flash A/B slots [compiler/runtime/src/host.rs:150-167], (ADR-0064 amendment); host-side validation the IDE cannot bypass (ADR-0052); same-buffers no-copy exact match (ADR-0064 decision (d)); `StateMigrationPlan` [compiler/runtime/src/host.rs:738-757]. Deviation: role names are `Primary`/`Secondary` (configured, capitalized) — cosmetic against the task's `PRIMARY`/`SECONDARY` [specs/design/ha-redundancy-fsm.md:63-70]. |
| §2.8 / §13.2 Topology: two dedicated optical sync links L1/L2 | CONFLICT | Decided topology: port 1 = pair link, port 2 = the same logical ping/pong routed through the whole I/O daisy-chain [specs/roadmap.md:74-79], [specs/design/ha-redundancy-fsm.md:328-334]. Not dedicated, not optical, and port 2 shares fate with I/O traffic — deliberately (it proves chain traversability), but it contradicts the task's mandated topology. Owner ruling needed (§3). |
| §2.9/§2.10 Two-channel loss rule; EtherNet/IP without assumed fencing | ALIGNED | Detection case table and 0/0 ⇒ no-auto-promotion [specs/design/ha-redundancy-fsm.md:295-308]; per-protocol fencing bindings with a recorded capability descriptor and guarantee levels [specs/design/ha-redundancy-layer-architecture.md:249-296]. |
| §3 N+1 rule | ALIGNED | See D23. |
| §4 Contexts (HW_KEY / OS / HW_DIAG / RUNTIME / APPLICATION) + additional boundaries | PARTIAL | RUNTIME ≈ `RuntimeHost` [compiler/runtime/src/host.rs:150-167]; APPLICATION ≈ container + host staging; redundancy boundary = the `ironplc-redundancy` crate with module decomposition [specs/design/ha-redundancy-layer-architecture.md:191-219]. HW_KEY, OS, HW_DIAG contexts: ABSENT (see §7–§9 below). Deployment-transaction and fencing boundaries exist as design + simulation; the watchdog-enforcement boundary is ABSENT. |
| §5 Contract specification discipline (freshness, validity, revocation order, expiry) | PARTIAL | Typed request/response + status snapshots exist [compiler/runtime/src/commands.rs:28,157]; epoch anti-stale + lease expiry covers the lifetime questions at the pair boundary [compiler/ironplc-redundancy/src/lease.rs:1-28]. No per-contract spec in the task's §5.2 field list; no consumer-computed `Stale`; no revocation-priority matrix beyond the single permit latch. |
| §6 Deterministic execution, bounded queues, panic policy | PARTIAL | Single-threaded host, no queues in the control path, `unsafe` denied workspace-wide, panics denied by lint fences; the VM never panics on arbitrary bytecode (pinned by test). WCET/backlog/overload specs are not applicable to the served-VM target; multi-task scheduling is roadmap-deferred. |
| §7 HW_KEY | ABSENT (OOS-DEC) | No key hardware or requirement; roles are configured from the UI; a key profile would be a new class without an owner — the predecessor review's REJECT-now verdict stands [specs/design/external-fsm-review.md:63], [specs/design/ha-redundancy-fsm.md:63-88]. |
| §8 OS / Platform FSM and ports | ABSENT (OOS-DEC) | No OS lifecycle model, no platform ports; the host is `std` on a desktop OS [specs/design/external-fsm-review.md:13-23,62]. The qualified-profile idea is accepted for a future target. |
| §9 HW_DIAG | ABSENT | No diagnostic records, startup/background test budgets, or resource health model. Nearest: fencing module-state views and the HA alarm flags [specs/design/ha-engineering-ui.md:79-93]. |
| §10 RUNTIME modes/phases/faults (ModeSelection, ExecutionPhase, fault lifecycle) | CONFLICT (gap) | The runtime has **no operating modes**: the host is always cyclic once permitted; `HostMode` is an execution-selector label, not a mode authority [compiler/runtime/src/host.rs:51-59,605-636]. No STOPPED/STARTING/QUIESCING/FAULTED phases, no fault detect/latch/ack/clear lifecycle (a trap aborts the round with an error), no controlled-stop or quiescence policy. A committed decision rejects a runtime RUN/PROGRAM mode authority as reopening ADR-0052/0055 [specs/design/external-fsm-review.md:44]. Owner ruling needed (§3). |
| §11 Application deployment (artifact lifecycle, single binding owner, online-change contract, state preservation, barrier/migration) | ALIGNED | Strongest overlap. Per-artifact staging with validation, one active binding owner, and the task's stage/test/untest/finalize/discard table maps onto accept/test/untest/assemble/cancel (ADR-0064 decisions 1–4; [compiler/runtime/src/host.rs:228-387]); exact preservation via stable-UID + semantic-type matching, `NOT_PROVEN` = fail-closed refusal with per-UID decisions (ADR-0060, ADR-0061); untest never restores bytes [compiler/runtime/src/host.rs:313-334]; one candidate only (V4013); synchronous migration with a plan, no third bank, no arbitrary structural migration promised [specs/design/external-fsm-review.md:175-179]. Gaps: pair-assemble crash recovery is design-only (half-persisted pair is an open item, ADR-0064 amendment item 5); journal/catch-up structural migration is deferred multi-task work. |
| §12 I/O enforcement (single write boundary, per-group policy, CPU/OS failure path, data semantics) | PARTIAL | Single-boundary principle and the all-or-nothing ownership barrier: designed and simulated [specs/design/ha-redundancy-fsm.md:193-199,310-326], [compiler/ironplc-redundancy/src/fencing.rs:1]. Per-group policy table, force semantics, hardware inhibit, output-frame identity/invalidation, input quality/age: ABSENT — no physical I/O exists. |
| §13 Redundancy (axes, two-channel rule, split-brain, replication, supervision/calibration) | ALIGNED (one design difference) | Axes, 0/0 rule, fencing-at-target, no zombie ACTIVE, no auto-failback: [specs/design/ha-redundancy-fsm.md:116-326], [compiler/ironplc-redundancy/src/statechart.rs:594-618]. Supervision: D18/D19 above. Replication design difference: see §3(f) — snapshot+offer vs journal-by-write-fact. EtherNet/IP ownership/reconnect proof: open (real binding pending) [specs/roadmap.md:200-210]. |
| §14 Firmware update | ABSENT | See D21. |
| §15 Boot / restart / shutdown | PARTIAL | Boot adoption of verified artifacts with a marker/crash table and never-both-invalid invariant (ADR-0064 amendment items 2–3); a reboot honestly restores the last committed generation. Missing: staged OS boot, restart-policy taxonomy, shutdown procedure, power-fail-safe retain flush (directory durability is a noted residual risk). Note: standalone shells grant the permit unconditionally at startup [specs/design/ha-redundancy-layer-architecture.md:135-138] — acceptable for a served VM, but it *is* "spontaneous resumption of control" in task terms once physical outputs exist. |
| §16 Timing/resource guarantees | PARTIAL | Measured-terms model, budget validation, limiting-device view, calibration-gated readiness (ADR-0062; [compiler/ironplc-redundancy/src/calibration.rs:184-262]); the takeover inequality is the ADR's budget check. Missing: WCET/scheduling analysis, watchdog-feeding progress proof, starvation budgets for TLS/upload/discovery (no such loads exist), and memory/queue bounds that are structural (fixed frames, bounded ring) rather than analyzed. |
| §17 Cybersecurity | CONFLICT (deferred) | No threat model, no authentication/authorization, no SBOM, no fuzzing program. Bounded parsing exists (fixed-size frames, CRC, length-checked codec [compiler/ironplc-redundancy/src/liveness.rs:108-138]) but is not a security posture. Authentication is deferred by committed decision — owner ruling needed (§3). |
| §18 Observability, events, audit, configuration/retain | PARTIAL | Structured read-only projections (five HA tabs; `haStatus`; stale-baseline handling) [specs/design/ha-engineering-ui.md:159-274], [integrations/vscode/src/haPanelLogic.ts:30]; a timestamped event ring exists but is in-memory and volatile [compiler/ironplc-redundancy/src/shell/views.rs:13-31]. Missing: a durable audit trail, fault-ack vs alarm-ack vs logging separation, retain storage with schema identity/wear budget, and backup/restore and CPU-replacement procedures. |
| §19 FreeRTOS / Linux ports | ABSENT (OOS-DEC) | See §8 and D11. |
| §20 Formal properties / TLA+ | PARTIAL | Invariants are documented and table-tested (`CAN_EXECUTE_OUTPUTS`, epoch-dominates-generation, no-zombie, the detection case table) [specs/design/ha-redundancy-fsm.md:310-326], [compiler/ironplc-redundancy/src/statechart.rs:640]; requirement→test traceability is build-enforced. No TLA+/PlusCal models, no model-checked liveness. |

## 3. Conflicts Requiring Owner Decision

Six seeded hypotheses were verified. Two dissolve as non-conflicts; four stand.

**(a) D18 +1000 penalty counter — RESOLVED, ALIGNED.** The task fears the
+1000 step contaminating the protocol sequence [Task §A.2 D18, §13.5]. In code
the wire sequences advance strictly +1 per PING/PONG
[compiler/ironplc-redundancy/src/liveness.rs:255-270]; the +1/+1000 counter is
a separate in-memory diagnostic field, exposed as a penalty indicator and
explicitly barred from timeout, freshness, and takeover arbitration
[compiler/ironplc-redundancy/src/liveness.rs:18-30,273-276]. This is exactly
the task's "legacy UI projection" rule. No decision needed.

**(b) D19 EMA as bound — RESOLVED, ALIGNED.** The task fears EMA10/EMA100
being read as worst-case bounds [Task §A.2 D19]. In code the trackers carry
min/max beside the EMAs, the budget check consumes only the running maximum as
the qualification bound, and EMA terms feed only the predicted-if-now estimate
whose safepoint term is separately bounded
[compiler/ironplc-redundancy/src/timing.rs:27-40,111-115],
[compiler/ironplc-redundancy/src/calibration.rs:184-220,604-617]. Detection
time is an engineer-configured confirmation time validated by calibration
drills under load, not an EMA
[compiler/ironplc-redundancy/src/calibration_run.rs:1]. No decision needed.

**(c) Topology: dedicated optical sync links vs decided port map — CONFLICT.**
The task fixes "two dedicated optical sync links, separate from engineering,
SCADA, and process I/O" [Task §2.8] and reasons over `L1`/`L2` as independent
channels [Task §13.2]. The project's committed topology is one pair link
(port 1) plus the *same* logical packet routed through the I/O daisy-chain
(port 2) — chosen deliberately so channel 2 proves chain traversability
[specs/roadmap.md:74-79], [specs/design/ha-redundancy-fsm.md:328-334]. Port 2
is neither dedicated nor optical and shares fate with I/O traffic; the task's
failure-independence argument for L1/L2 does not hold for it, while the
project's traversability argument does not exist in the task's topology. This
is a genuine engineering fork with availability-model consequences (the 0/0
case means different physical failures in the two designs). **Decision
needed:** adopt the task's dedicated-link topology (reopen the port map), or
record a documented deviation with the availability trade-off the task demands
[Task §13.2].

**(d) Operating modes — CONFLICT (committed decision vs task MUST).** The task
mandates the semantic separation `ModeSelection = PROGRAM | TEST | RUN` and
`ExecutionPhase = STOPPED | STARTING | CYCLIC | QUIESCING | FAULTED` with a
fault lifecycle [Task §10.1] and declares the separation semantically
obligatory. The project's runtime has no modes: it is cyclic whenever the
permit is granted, and a committed review rejected a runtime mode authority as
reopening ADR-0052/0055 and adding a second execution authority
[specs/design/external-fsm-review.md:44],
[compiler/runtime/src/host.rs:51-59]. The task's strongest arguments (output
ownership keyed to RUN, TEST output suppression, quiescence deadlines, fault
ack/clear ≠ start) currently have no physical subject in this project — but
they will the moment physical outputs land. **Decision needed:** either adopt
the task's §10 axis model when the I/O enforcement boundary is built (the
permit latch is the prepared seam for exactly this), or record a permanent
deviation with an equivalent mechanism mapping.

**(e) D13 TEST mode vs Test Edits — CONFLICT, same ruling as (d).** Our
`HostMode::Testing` is the Rockwell Test Edits selector switch
[compiler/runtime/src/host.rs:54-59]; the task's TEST mode (IEC logic
executes, all application-driven physical effects suppressed, protocol-FB
bypass closed, real-input observation only in an explicit profile
[Task §10.1]) has no mechanism here. The decided future gate —
`CAN_EXECUTE_OUTPUTS` [specs/design/external-fsm-review.md:91] — covers "a
non-ACTIVE unit writes nothing" but not "an ACTIVE unit in TEST computes
without affecting the process." Same owner ruling as (d).

**(f) Crossload transport: journal-by-write-fact vs snapshot+offer —
CONFLICT (scope of the guarantee).** The task mandates state crossload "by
write-fact": a journal containing every mutating write (including same-value
writes) with checkpoint boundary, sequence, ACK, commit marker, integrity,
and skip recovery; the SECONDARY applies whole checkpoints only
[Task §13.4]. The project's crossload is image-based: one whole-state snapshot
in the Accept offer plus steady-state `StateUpdate` whole images
[compiler/ironplc-redundancy/src/crossload.rs:57-102] — no write journal, no
per-image sequence/ACK/commit marker (the offer itself has Accepted/Refused;
the steady-state images are one-way). Honest comparison: image replication
applies whole checkpoints (satisfying the task's atomic-apply rule) with a
progress-loss bound of one update interval and no journal-overflow or
unknown-schema failure modes; the journal gives finer loss granularity and gap
detection but adds exactly the overflow, ordering, and catch-up machinery the
project defers with multi-task execution — and on a passive-secondary
whole-checkpoint model its extra fidelity buys little until
impulse/transactional outputs exist [Task §13.4]. **Decision needed:** record
snapshot+offer as the v1 transport with the task's checkpoint protocol
(sequence/ACK/commit marker) as the requirements source for the real
EtherNet/IP binding, or adopt the journal now.

## 4. Gap List with Proposed Disposition

Every ABSENT/PARTIAL from §2, with a disposition: **ADOPT** (into the roadmap
as a one-line entry), **DEFER** (with rationale and the task's own blocker
logic), **REJECT** (with rationale).

| Gap | Disposition | Rationale and proposed roadmap entry |
|---|---|---|
| HW_KEY (§7) | REJECT (now) | No key hardware or product requirement; roles are configured [specs/design/ha-redundancy-fsm.md:63-88]; the predecessor review's REJECT-now verdict already covers this [specs/design/external-fsm-review.md:63]. Revisit only if a key/safety profile becomes a requirement — then it is a new ADR and mechanism class, per the task's own same-class rule. The task's blocker table supports waiting: restart/retain/force policies are an owner decision [Task §26]. |
| HW_DIAG (§9) | DEFER | No diagnostic consumers exist on a served VM; the record model (per-`ResourceId` facts, quality, latch/recovery) is a sound requirements source for the MCU port named in D11. Blocker per the task: CPU/board/watchdog topology undecided [Task §26]. |
| OS/platform ports (§8, §19) | DEFER | Same target-side blocker; port contracts and requalification rules should be lifted from the task when a port starts — building ports without a target would be mechanism without a consumer. |
| I/O output enforcement + output groups + watchdog/hardware inhibit (§12, D06/D10/D20) | ADOPT | The largest real P0 cluster. The roadmap already carries the seed (I/O firmware contract audit, three profiles, autonomous watchdog [specs/roadmap.md:215-230]); the task's §12.2 per-reason policy table and §12.3 independence criteria are the right requirements source. Proposed roadmap entry: *"I/O output enforcement: single authoritative write boundary with per-group output policy (PROGRAM/TEST/fault/loss/HA/force), output-frame identity and invalidation, and an autonomous module watchdog (SAFE_VALUE/HOLD_LAST/RAMP_TO_SAFE) independent of the CPU/OS path — requirements per the DCS task §12; blocked on the I/O firmware contract audit and a hardware target."* |
| RUN/PROGRAM/TEST modes + fault lifecycle (§10, D13) | DEFER pending owner ruling (c–e) | The permit latch is the prepared enforcement seam [specs/design/ha-redundancy-layer-architecture.md:132-148]; if the owner adopts the task's §10 model, it lands there without reopening the swap machinery. Until the ruling: no action. Proposed roadmap entry: *"Operating-mode axis ruling: adopt or formally deviate from the DCS task's ModeSelection/ExecutionPhase model (§10) — gates the I/O enforcement build."* |
| Security/authentication (§17, D22) | DEFER pending owner ruling | Committed deferral (ADR-0063, ADR-0065) contradicts task MUSTs; only the owner can move this. Bounded parsing/CRC/length checks already exist and should be kept. Proposed roadmap entry: *"Security profile ruling: threat model, authentication/authorization for engineering and update channels, SBOM — owner ruling required; currently deferred by ADR-0063/0065 in contradiction with the DCS task §17 MUSTs."* |
| TLA+/PlusCal models (§20) | ADOPT (small) | One small model for the still-pending arbitration/fencing protocol (start/revoke race, promotion fencing, message loss) is cheap, and the protocol is unimplemented design; the task's assumptions-and-bounds discipline applies. Proposed roadmap entry: *"Formal model: a TLA+/PlusCal model of the HA ownership protocol (start/revoke race, generation switch, promotion fencing, message loss) alongside the arbitration/fencing design."* |
| Retain/checkpoint storage semantics (§15.2, §18.3) | ADOPT | Retain data today lives only in RAM; the slot store persists committed code, not process state. Schema identity, integrity, wear budget, and recovery policy are the task's words and should seed the design. Proposed roadmap entry: *"Retain/checkpoint persistence: schema-identified, integrity-guarded retain store with wear budget and recovery policy — controller target; requirements per the DCS task §18.3."* |
| Event/audit journal durability (§18.2) | ADOPT (spec now; persistence with the retain store) | The event ring exists but is volatile [compiler/ironplc-redundancy/src/shell/views.rs:13-31]; reason codes are already stable V-codes, so the vocabulary half is done. Specify the durability contract (what survives reboot and power loss) inside the retain-store entry rather than as a second mechanism. |
| Firmware update transaction (§14, D21) | DEFER | No field-update requirement or bootloader exists; the task's own blocker table puts supported version pairs and update authority with release engineering [Task §26]. The application A/B slot machinery is the seed; lift §14 as the requirements source when a device target appears. Proposed roadmap entry: *"Firmware maintenance transaction: staged image, trial boot, rollback, mixed-version pair policy — requirements per the DCS task §14; blocked on a hardware target and the security ruling."* |
| Request-id dedup / durable journal (§6.2) | REJECT (standalone) / ADOPT-LATER (pair) | Already triaged: protocol state ADR-0055 deliberately does not have; the one place it matters — the pair assemble transaction — is a named open item (ADR-0064 amendment item 5), [specs/design/external-fsm-review.md:54]. No new entry needed. |
| Behaviour-change risk record (D14) | ADOPT (into the pending-edit record) | The controller-side `PendingEditRecord` exists [compiler/runtime/src/host.rs:93-109]; adding a state-compatibility/behaviour-change field (layout-equality vs migration) is a one-field schema growth when the next edit-lifecycle slice lands. Covered by the existing Phase 6 debt entry; extend its closure scope. |
| Topology ruling (§2.8) | ADOPT (decision entry) | See §3(c). Proposed roadmap entry: *"HA topology ruling: dedicated optical sync links (task §2.8) vs the decided pair-link + I/O-chain port map — owner decision; gates the availability model and the EtherNet/IP binding."* |
| Crossload checkpoint protocol (§13.4) | ADOPT (as binding requirement) | See §3(f). Proposed roadmap entry: *"Crossload hardening for the real binding: per-image sequence/ACK/commit markers and skip recovery per the DCS task §13.4 — folds into the EtherNet/IP binding item."* |

## 5. Release Gates G01–G10 — Current Evidence State

Brutally honest, per the task's own warning that readiness is claimed for a
*specific* build/hardware/OS/application/failure-model combination [Task §25]
— none of which exists as a shippable controller profile today.

| Gate | State | Why |
|---|---|---|
| G01 Scope / hazards | **UNMET** | No hazard analysis, no output groups, no process-reaction budgets, no operational envelope. The failure model is crash-fault pair semantics in simulation only. |
| G02 Architecture | **PARTIAL** | Ownership matrix, full transition tables, typed contracts, and coded refusals exist and are traceable to tests. Open: the arbitration/quorum/commit-certificate design the roadmap itself sequences first [specs/roadmap.md:69-73]; epoch fencing authority and replay protection undecided (the task's §13.3 questions); the conflicts in §3 unresolved. |
| G03 Time / resources | **PARTIAL** | The measured-terms model, budget validation, and calibration gating exist and are tested — over abstract ticks on a loopback binding [compiler/ironplc-redundancy/src/calibration.rs:184-262]. No target measurements, no scheduling analysis, no worst-case load envelope. |
| G04 Physical containment | **UNMET** | Nothing independent of the software path exists: no watchdog, no hardware inhibit, no HIL. The task's disqualifier — protection depending only on a live Runtime task — is precisely the current state; the mitigation is a roadmap audit item, not a mechanism [specs/roadmap.md:215-230]. |
| G05 Deployment / persistence | **PARTIAL** | A/B slot store with a four-crash-point table, boot adoption, byte-equality tests, and the V6012-on-wire answer are implemented (ADR-0064 amendment items 2–4). Power-fail consistency is *simulated* (crashes are stop-between-steps; true power-cut testing is unavailable); half-persisted pair recovery is design-only; the structural-migration pause is unmeasured (external-fsm-review takeaway 3, open). |
| G06 HA profile | **PARTIAL** | Fencing, the CONTROL chart, the 0/0 rule, and REDUNDANCY_LOST are proven against the simulator binding [specs/roadmap.md:167-176]; the real EtherNet/IP binding, the real two-process pair, and fault-coverage evidence are open [specs/roadmap.md:200-210]. The 0/0 availability trade-off is enforced but not yet recorded in an owner-accepted availability model. |
| G07 Security | **UNMET** | Deferred by committed decision; no threat model, no access tests, no fuzzing program, no SBOM, no patch procedure. Bounded parsing exists but is not a security posture. |
| G08 Verification | **PARTIAL** | Requirement→test traceability is build-enforced (generated spec tests, empty-UNTESTED meta-test), coverage ≥ 85 %, table-driven statechart tests, lint fences. Missing: the task's fault-injection matrix T01–T28 as an explicit acceptance suite (parts are covered by existing host/statechart tests; see §6), model results, HIL evidence. |
| G09 Operations | **UNMET** | Engineering-side diagnostics and HA tabs exist; backup/restore of a controller, CPU replacement, stale-restore refusal, incident procedures, and operator (non-engineering) diagnostics do not. |
| G10 Release / site | **UNMET** | No hardware revision, no compatibility matrix, no FAT/SAT, no environmental/EMC/endurance validation. Consistent with the project's honest posture: no production claim is made. |

## 6. Recommendations

**Top five prioritized actions:**

1. **Rule on the owner-level conflicts (§3): topology (c), operating modes
   (d/e), crossload transport (f), security (D22/§17).** All four gate
   roadmap entries; none can be closed by engineering judgment alone.
2. **Land the arbitration/quorum + epoch-authority design** the roadmap
   already sequences first [specs/roadmap.md:69-73], using the task's §13.3
   as its requirements source (fencing issuer, replay protection,
   partial-acquire recovery, per-module EtherNet/IP ownership proof).
3. **Commission the I/O firmware contract audit into an implementation
   plan** for the output-enforcement boundary, per-group policies, and the
   autonomous watchdog — the task's P0 cluster D06/D10/D20 and gate G04.
4. **Adopt the task's Part C T01–T28 matrix as the Phase 5/6 acceptance-test
   backlog:** map each scenario onto existing tests (T04/T05/T07/T09/T13 and
   the T14/T18 analogues are already covered by host, statechart, and shell
   tests), write the missing pair-level ones (the T16/T17/T19-style fencing
   and recovery races) against the simulator binding, and mark target-side
   scenarios (T01/T02/T06/T26-style) blocked on hardware rather than absent.
5. **Write the availability model for the 0/0 trade-off** (the task's §13.2
   demand) as part of the HA profile: what the pair guarantees, what it
   deliberately does not, and which plant scenarios the owner accepts —
   before any failover-time number is quoted to a user.

**Proposed roadmap additions** (one line each, in the file's own voice):

- *"HA topology ruling: two dedicated optical sync links (DCS task §2.8) vs the decided pair-link + I/O-chain port map — owner decision; gates the availability model and the EtherNet/IP binding."*
- *"Operating-mode axis ruling: adopt or formally deviate from the DCS task's ModeSelection/ExecutionPhase model (§10) — gates the I/O enforcement build; the execution permit is the prepared seam."*
- *"Security profile ruling: threat model, authentication/authorization, SBOM per the DCS task §17 — currently deferred by ADR-0063/0065; the owner must reconcile the contradiction with the task's MUSTs before any production-profile language."*
- *"I/O output enforcement: single authoritative write boundary, per-group output policy, autonomous module watchdog — requirements per the DCS task §12; blocked on the I/O firmware contract audit and a hardware target."*
- *"Retain/checkpoint persistence: schema-identified, integrity-guarded retain store (wear budget, recovery policy) plus a durable audit trail for critical actions — requirements per the DCS task §18.2–18.3."*
- *"Crossload hardening for the real binding: per-image sequence/ACK/commit markers and skip recovery per the DCS task §13.4."*
- *"HA acceptance matrix: the DCS task's Part C T01–T28 mapped onto spec-conformance tests; target-side scenarios explicitly blocked on hardware, not silently dropped."*
- *"Formal model: TLA+/PlusCal for the HA ownership protocol (start/revoke race, promotion fencing, message loss) alongside the arbitration/fencing design."*

**Sections to adopt as requirements sources for future phases:** Part B §13
(redundancy — the pending arbitration/fencing design), Part C (T01–T28 as the
acceptance matrix; G01–G10 as the release-gate checklist), §12 (I/O
enforcement, when the boundary is built), §14 (firmware update, at device
target), §16.2 (the takeover inequalities — already matched by ADR-0062's
budget check and worth citing side by side), and §5.2 (the contract-field
checklist for the quorum wire formats). Part B §7–§9 and §19 bind only at a
target-side port, per D11.

## Out of Scope

- Any production code, spec, or ADR change; this document is analysis only.
- Reopening any committed decision; the conflicts in §3 are recorded for the
  owner, not resolved here.
- The reference corpus in `docs/reference/Rnd_Rockwell/`, which this audit
  does not touch.
