# Spec: Linux Controller Layer Architecture

## Overview

This spec answers the owner's question: *if the controller bases on Linux
tomorrow, what must be prepared architecturally?* Three lines up front:

1. **Layer boundaries run where the contracts already run.** Every boundary
   in the task's §4.5 layer view lands on an existing seam — `NicPort`
   (`compiler/ironplc-redundancy/src/hal.rs:17-29`), the fencing client
   (`compiler/ironplc-redundancy/src/fencing.rs`), the clock/scan-commit
   injection (`compiler/runtime/src/host.rs:605-636`), the A/B slot store
   (`compiler/vm-cli/src/slot_store.rs:1-33`), and the execution permit
   latch (`compiler/runtime/src/host.rs:204-221`) — so Linux changes the
   bindings behind the seams, not the layer shapes.
2. **Middleware is mostly adopted, little is developed.** Linux supplies
   the scheduler, sockets, storage, watchdog, and isolation; we develop
   only the dual-fiber redundancy transport (the ADR-0066 N+1 driver), the
   EtherNet/IP adapter behind the fencing seam, the engineering session
   server (exists), the diagnostics provider, and the evidence/contract
   types of the task's §5.
3. **OS linkage is six narrow ports, not an `OsPort`.** Each task §8.2 port
   maps to one Linux mechanism (§3); what userspace cannot guarantee — the
   hardware inhibit path of task T02/G04 — is marked a blocker, honestly.

Nothing here claims qualification. The CPU/board is undecided (task §26),
so every board-dependent cell is marked; this spec names the target profile
and the checklist (ADR-0066), not a completed port.

This spec builds on:

- **DCS Firmware Task v2.2**
  (`docs/reference/dcs-firmware-task-v2.2-ru.md`, owner-provided)
  — the requirements source: §4.4–4.9 (layer view), §4.10–4.16 (S01–S12,
  State Inventory), §8 (ports, execution profile), §12 (I/O enforcement),
  §14.4 (update units), §16 (timing budgets), §19.3/19.4/19.5 (Linux
  profile, platform matrix, three views), §26 (qualification blockers)
- **[ADR-0066](../adrs/0066-linux-execution-platform-and-global-epoch.md)** —
  the Linux platform decision, the pair-wide epoch, the N+1 driver ruling
- **[ADR-0062](../adrs/0062-measured-failover-timing-and-network-calibration.md)**
  and **[ADR-0064](../adrs/0064-online-change-on-a-redundant-pair.md)** —
  measured timing and the pair online-change pipeline the mechanisms serve
- **[HA Redundancy Layer Architecture](ha-redundancy-layer-architecture.md)**
  and **[HA Redundancy FSM](ha-redundancy-fsm.md)** — the seams, the
  statechart, and Protocol Portability this design ports
- **[DCS Firmware Task Audit](dcs-firmware-task-audit.md)** — the verdicts
  this spec re-classifies in its v2.2 addendum

## 1. Layer Boundary Placement (Task §4.5 Adapted to Linux)

The task's layer view is a **responsibility map, not a stack of managers**
(task D25): layer membership grants no state machine, no authority, and no
update unit by itself (task §14.4). Below, each boundary names what crosses
it (contracts only), which existing component sits there, and what stays
board-dependent. "Board-dependent" marks a task §26 blocker: the CPU/board,
watchdog/reset topology, and I/O adapters are undecided, so no claim below
depends on a specific board.

| # | Layer | Responsibility in our product | What crosses the boundary (contracts only) | Existing component (evidence) | Board-dependent |
|---|-------|-------------------------------|--------------------------------------------|-------------------------------|-----------------|
| 1 | Hardware | CPU/SoC, memory, PHYs, watchdog, power, I/O | Documented electrical properties and limits only | — none (by definition) | **Everything — blocker (task §26)** |
| 2 | BSP / hardware adaptation | Board identity, pin/clock/reset config, IRQ/DMA resources, boot handoff | Device tree, boot handoff, documented device interfaces | Nothing of ours yet; lands in the future `ironplc-platform-linux` crate's board-config part | **Yes — blocker**: U-Boot/device tree per board |
| 3 | Kernel / execution substrate | Native scheduling, interrupt/exception handling, synchronization, clock/memory primitives | The §8.2 ports: `MonotonicClockPort`, `CyclicExecutionPort`, `BoundedSignalPort` | Nothing in-kernel (we adopt; no kernel modules planned) | Kernel/config per board — PREEMPT_RT is a board qualification item (task §8.3) |
| 4 | Core OS / device infrastructure | Device APIs, driver lifecycle, filesystem/block infrastructure, `/dev/watchdog` | Device files with lifetime/completion/bounded-error semantics: `WatchdogPort`, `StoragePort` | Adopted; the future platform crate wraps `/dev/watchdog` and block devices | Watchdog hardware properties (task §12.3) |
| 5 | OS Services | Network transport, time services, storage services, instrumentation | `NetworkTransportPort`, time correlation, atomic publish/durability evidence | `UdpPort` over `std::net::UdpSocket` (`compiler/ironplc-redundancy/src/udp.rs:39-109`); the A/B `SlotStore` over OS files (`compiler/vm-cli/src/slot_store.rs:1-33`) | NIC choice (optical PHYs per the transport ruling) |
| 6 | Middleware | Protocol/security/serialization libraries, industrial communication, engineering transport adapters | Protocol-neutral domain requests/results and I/O evidence | Pair-link codec (`liveness.rs`, `crossload.rs:61-108`), engineering session protocol (`compiler/vm-cli/src/serve.rs:1-62`, `tcp.rs:40-53`) | EtherNet/IP stack selection (adopted library vs own adapter) |
| 7 | DCS platform / PLC Runtime | HW_KEY/HW_DIAG policy and evidence processing, Runtime, application lifecycle, deployment, HA, output admission | Public domain contracts, execution binding, process-image/effect interfaces | `ironplc-runtime` (`compiler/runtime/src/host.rs`), `ironplc-redundancy` (statechart, fencing client, calibration), composition roots (`serve.rs:110-125`, `ha_pair.rs:50-72`) | HW_KEY/HW_DIAG hardware; the output-enforcement target |
| 8 | IEC application | POU/FB/programs, declared tasks, state schema | The verified executable contract (`Container`) and the allowed effect ports | Built at engineering time by the compiler pipeline (dsl → parser → analyzer → codegen → `container`); loaded by `Container::read_from` (`crossload.rs:7-8`) | No (portable bytecode) |

Three boundary rules the task demands, and how the project already honors
them:

- **D25 — layers are not FSM hierarchies.** The only state machines in the
  product are the runtime's edit FSM (`compiler/runtime/src/host.rs:9-15`)
  and the HA SYNC/CONTROL charts
  (`compiler/ironplc-redundancy/src/statechart.rs:41,332`) — both justified
  lifecycles per S03. No layer has a `*_READY` mode; services publish
  capability/evidence, consumers derive readiness (task §4.6). The layer
  table above adds no state machine anywhere.
- **D26 — no mega-`OsPort`.** Ports are narrow and chosen by consumer need:
  the transport port (`hal.rs:17-29`) knows nothing of clocks or storage;
  the clock arrives as a composition-root injection
  (`host.rs:605-636`); epoch persistence is a declared port, not a grab-bag
  (`epoch.rs:13-15`). §3 adds the remaining ports in the same shape — one
  trait per consumer need.
- **D09 / §8.4 — a composition root, never a Service Manager.** Composition
  lives at binary entry points (`serve.rs:110-125`, `ha_pair.rs:50-72`);
  the architecture record is explicit: "no registry, no event bus, no
  plugin point" (ha-redundancy-layer-architecture.md:218-219). There is no
  global `OS_READY`: each binding records its own capabilities
  (`hal.rs:34-41`, `fencing.rs:199-218`), and the session dispatches over a
  concrete two-variant enum, not a registry (`serve.rs:85-90`).

## 2. Middleware Inventory — Develop vs Adopt

Rule: adopt the Linux mechanism wherever it satisfies the contract;
develop only what carries our domain semantics. Each developed item names
its mechanism, boundary, and update unit per task §14.4.

| Item | Develop / Adopt | Mechanism | Boundary | Update unit (task §14.4) | Size |
|------|-----------------|-----------|----------|--------------------------|------|
| Scheduler (incl. PREEMPT_RT patch) | **ADOPT** | Linux scheduling classes; IEC task release/order stays in the runtime — the port maps it to OS contexts (task §4.5) | Kernel → OS Services | Kernel is part of the platform image; update ⇒ platform restart | — |
| Sockets (UDP/TCP) + `AF_PACKET`/raw Ethernet | **ADOPT** | `std`/libc sockets today (`udp.rs:39-109`); raw Ethernet for the dedicated sync link later | OS Services | Platform image | — |
| Storage durability (`pwrite`/`fsync`) | **ADOPT** | VFS/page cache + `fdatasync`; atomic publish is ours (tmp + rename + marker) | Core OS → OS Services | Platform image | — |
| `/dev/watchdog` | **ADOPT** | Char device, magic-close; feeding policy is ours (§3) | Core OS | Platform image | — |
| Isolation (`cgroups`, `isolcpus`, affinity, `mlockall`) | **ADOPT** | cpuset/cgroup for the engineering and diagnostics planes; FIFO priorities for RT threads | Kernel → OS Services | Platform image | — |
| Time services (`CLOCK_MONOTONIC`, optional PTP) | **ADOPT** | `clock_gettime`; PTP only for UTC correlation, never for timeouts (task §5.4) | OS Services | Platform image | — |
| libc / Rust `std` runtime | **ADOPT** | — | all | toolchain | — |
| Redundancy transport driver over dual fiber | **DEVELOP** | Per-port `NicPort` instances (the ADR-0066 N+1 driver) + the existing pair-link codec; `AF_PACKET` binding, `SO_PRIORITY` | OS Services ↔ Middleware (driver); Middleware → DCS platform (`NicPort`) | A versioned driver component of the platform image with declared dependencies; replacement needs drain/quiescence semantics | L |
| EtherNet/IP adapter behind the fencing seam | **DEVELOP** | A `FencingClient` binding recording its guarantee level in `FencingCapabilities` (`fencing.rs:199-218`) | Middleware ↔ DCS platform | Adapter component; runtime replacement only with stated recovery semantics | L |
| Engineering session server | **DEVELOP (exists)** | Typed command enums + line codec (`serve.rs:1-62`), TCP listener with single-session refusal (`tcp.rs:40-53`, ADR-0065) | Middleware → DCS platform | Part of the controller daemon image; session-drainable | S |
| Diagnostics provider | **DEVELOP** | Per-resource health records (PHY counters exist, `hal.rs:47-54`); durable bounded event/audit ring (today's `HaEvent` ring is volatile, `compiler/ironplc-redundancy/src/shell/views.rs:13-31`) | DCS platform | Platform image component | M |
| Evidence/contract types (task §5, S01–S12) | **DEVELOP** | `Evidence`/`EvidenceStamp`-shaped validity at contract boundaries; the State Inventory as a registry artifact (§4.2) | DCS platform (compile-time types) | The owning crate's image — no separate runtime update | M |
| Userspace output-enforcement boundary | **DEVELOP** | Pure admission + one stateful write boundary (task §12.1); the permit-latch pattern is the local analogue (`host.rs:166,204-221,701-707`) | DCS platform → effect ports | The runtime image; the immutable application bank gives no right to hot-swap the runtime (task §14.4) | L |

Counts: **7 adopted, 6 developed** (one of the six already exists).

## 3. OS Linkage — Port Table (Task §8.2 → Linux)

Each task §8.2 port mapped to its Linux mechanism and our existing seam.
Domain traces stay identical across ports; timing bounds are requalified,
never inherited (task §8.3, D11).

| Task port | Linux mechanism | Our seam today (evidence) | Semantics and honesty notes |
|-----------|-----------------|---------------------------|------------------------------|
| `MonotonicClockPort` | `clock_gettime(CLOCK_MONOTONIC)` | Clock injected by the composition root: `run(rounds, clock)` (`host.rs:605-636`); abstract ticks in the lease (`lease.rs:21-28`) | Monotonic per boot; wall-clock jumps never affect timeouts. `CLOCK_REALTIME` is used only to stamp the pending-edit record (`host.rs:778-783`) and audit correlation. Clock identity per boot; never compare raw monotonic timestamps across controllers (task §5.4) |
| `CyclicExecutionPort` | `SCHED_FIFO` scan thread (`SCHED_DEADLINE` optional), `isolcpus`, IRQ affinity, timer-driven release | The single-threaded `run_with_commit` loop (`host.rs:619-689`); the soft-device pump cadence (`ha_pair.rs:41-45`) | The runtime defines IEC task release/order and deadlines; the port maps them to qualified OS contexts — no second scheduler inside the runtime (task §4.5). Wakeup latency, interference, and load envelope are measured per board (task §16.3) |
| `BoundedSignalPort` | Bounded `mpsc` ring + `eventfd` | Direct calls in the single-threaded host; the pair pump's channel (`ha_pair.rs:79`) | Bounded backlog; overload ⇒ coded refusal, never unbounded growth (task §6.1) |
| `StoragePort` | `pwrite` + `fdatasync`, tmp + rename + marker | A/B `SlotStore` (`slot_store.rs:1-33`); committed-wire latch (`host.rs:411-417`) | Atomic publish = verified tmp, fsync, rename into the inactive slot, marker flip; the crash table heals a missing marker (ADR-0064 amendment). Durability evidence per task §5.2 |
| `WatchdogPort` | `/dev/watchdog` (magic close, `WDIOC_SETTIMEOUT`) | None — a named gap; feeding policy defined here | Fed on **evidence of progress**: the scan-commit counter advancing (`host.rs:677-682`) plus permit state — never a periodic alive tick (task §16.3). Hardware timeout, reset-state outputs, and stop semantics must be verified on the chosen driver/board (task §12.3) — **board blocker** |
| `NetworkTransportPort` | UDP sockets; `SO_PRIORITY` for the pair link; `AF_PACKET`/raw Ethernet for the dedicated sync link | `NicPort` + `UdpPort` (`hal.rs:17-29`, `udp.rs:39-109`); per-channel sequences and PHY counters as link evidence (`liveness.rs:94,255-270`, `hal.rs:47-54`) | Bounded submit/receive (one datagram per `poll`); loss/duplication stay UDP-honest and are arbitrated by the liveness exchange, not hidden by the port |
| `PlatformPowerPort` | `reboot(2)` / orderly poweroff via sysfs/systemd; last-resort watchdog reset | None — a named gap | Reason-coded shutdown/reset request; escalation deadlines per task §15.3 |

**What userspace cannot guarantee (task T02/G04, stated plainly):** when
the CPU/OS stalls, software owners get no CPU, so the output policy must be
enforced by a path independent of the runtime — a hardware inhibit, remote
I/O watchdog, or reset-state output circuitry. Linux userspace can feed
`/dev/watchdog` honestly (progress-evidence feeding above), but the
electrical state of the outputs after a reset is a **board-design property
and a blocker** (task §12.3, G04). No software layer claim substitutes for
it.

## 4. Mechanisms Filling Each Layer

### 4.1 Process and Thread Model

One real-time process (the controller daemon; today's `ironplcvm serve`
pair composition is its soft-device prototype):

- **Scan thread** — `SCHED_FIFO`, highest priority. Drives
  `run_with_commit` (`host.rs:619-636`); the scan-commit callback mints the
  epoch/lease at the boundary (the existing seam, `shell/mod.rs:660-685`).
  `mlockall` + prefault; no allocation on the scan path (buffers are
  container-sized by construction, `compiler/vm/src/buffers.rs:18-63` —
  that is the `Mem_active`/`Mem_state` bound of task §16.1).
- **HA thread** — `SCHED_FIFO`, lower priority. Pumps `PairLink::tick`
  (the pair codec, `compiler/ironplc-redundancy/src/pair_link.rs`) over the
  two sync-link ports; never sits in the scan path (ACTIVE realtime
  isolation, HA Redundancy FSM goal 5).
- **Engineering thread** — `SCHED_OTHER`, cgroup-bounded CPU and memory.
  Serves the typed command session (`serve.rs:1-62`, `tcp.rs`); its loss
  never cancels admitted control (task §4.8).
- **Diagnostics thread** — `SCHED_OTHER`, budgeted background tests (task
  §9 budgets); publishes health records, never RUN/STOP decisions.

Concurrency model: shared memory between these threads is the existing
host API; the scan boundary is the synchronization point, exactly as the
single-threaded host already enforces by borrow (the VM borrow cannot
overlap a buffer mutation, `host.rs:32-36`). Candidate and migration arenas
(`Mem_candidate`, `Mem_migration`) are rebuilt at the boundary today
(`host.rs:738-757`); on Linux they get explicit `mlockall` bounds plus
cgroup limits.

### 4.2 State Inventory Mapping (Task §4.14)

The task's registry fields, mapped onto mechanisms that already exist —
the registry formalizes them; it creates no runtime manager (task §4.16).

| State kind | Identity / owner | Representation | Lifetime / recovery (task §4.14 rows) | Evidence |
|------------|------------------|----------------|----------------------------------------|----------|
| IEC semantic state (FB/POU data) | Runtime — `VmBuffers` vars + data region (`buffers.rs:18-27`) | Typed slots; stable-UID-addressed (`host.rs:419-437`) | Hot edit: exact-match reuse, no copy (swap at boundary, `host.rs:701-729`); structural: `StateMigrationPlan` (`host.rs:738-757`); HA: whole-image crossload (`crossload.rs:83-88`) | `state_snapshot`/`apply_state_snapshot` (`host.rs:419-513`) |
| Execution binding / mode | Runtime — single owner (`host.rs:150-167`) | `HostMode` + one pending swap (`host.rs:51-66`) | Survives reboot as the committed generation (boot adoption, ADR-0064 amendment) | `HostStatus` projection (`host.rs:390-409`) |
| Output authority | HA supervisor — pair-wide epoch (`epoch.rs:19-25`), `OwnerLease` (`lease.rs:36-69`), fencing `ModuleState` (`fencing.rs:92-111`) | Newtype counter + expiry + per-module ownership truth | New right only through fencing; a restored role enum grants nothing (task §4.14) | Anti-stale adoption (`epoch.rs:55-64`); barrier verification (`fencing.rs:286-296`) |
| Transactions (hot edit, deployment) | Runtime — candidate lifecycle FSM (`host.rs:9-15`); a justified EFSM per S03 | `PendingEditRecord` (RAM-only, `host.rs:93-109`) + committed-wire latch | Candidate dies on reboot; canonical bytes persist via A/B slots (`slot_store.rs:1-33`) | Coded refusals (V-codes) |
| Derived status / caches | Nobody — projections only | `HostStatus`, HA views (`views.rs`) | Re-derived from facts after any restart/failover; a cache never extends authority: the boundary re-checks the permit (`host.rs:701-707`) — INV15/S06 | Stale-baseline refusal on the client (E0015) |

### 4.3 Enforcement Boundary Design for Linux

Task §12 demands one authoritative write boundary; split pure admission
from stateful enforcement per S09:

1. **Admission (pure):** `CAN_EXECUTE_OUTPUTS = (CONTROL in ACTIVE) &&
   owns_all_required_io` (HA Redundancy FSM invariants) — evaluated at the
   scan boundary, where the permit latch is re-checked today
   (`host.rs:701-707`). This is the decision/commit discipline of S09 in
   miniature: a right revoked between request and boundary cancels the
   operation.
2. **The one write boundary (stateful):** the I/O adapter/driver endpoint
   admits or rejects output frames under epoch + owner + freshness; the
   fencing client is the seam behind which the per-protocol binding lands
   (`fencing.rs:1-32`). Per-group policies (PROGRAM/TEST/fault/loss/HA/
   force of task §12.2) attach here, keyed by output group.
3. **What still needs the EtherNet/IP endpoint proof (task §13.3):**
   target-enforced exclusivity on real modules, epoch-stamped ownership in
   the cyclic input, reconnect/timeout semantics, and the measured time to
   the first valid new-owner output frame. Until the real binding exists
   (roadmap Phase 5 open item), fencing is proven only against the
   simulator registry (`fencing.rs:426-439`) — split-brain containment on
   a real wire is **open**, and the production HA profile stays unaccepted
   by the task's own rule.

## 5. Prepared-for-Linux Checklist

**Already portable today (cite):**

- Domain rules and both statecharts (`statechart.rs:41,332`) — pure
  table-driven, no OS calls.
- The permit latch and all four runtime seams (`host.rs:166,204-221,`
  `605-636,419-437,469-513`).
- The transport seam with a real socket binding (`hal.rs:17-29`,
  `udp.rs:39-109`) — the two-process pair already runs over UDP
  (`ha_pair.rs:50-72`).
- The fencing seam with its capability descriptor (`fencing.rs:199-218`).
- The crossload/offer codec, epoch, lease, and calibration trackers
  (`crossload.rs:61-108`, `epoch.rs`, `lease.rs`, `timing.rs:33`) — all in
  abstract ticks, awaiting `CLOCK_MONOTONIC` at the composition root.
- The `no_std` execution kernel (`compiler/container/src/lib.rs:1`) and the
  A/B slot store (`slot_store.rs:1-33`).
- Composition roots that already select bindings per deployment
  (`serve.rs:110-125`, `ha_pair.rs:50-72`).

**Must be built next (ordered):**

1. **(S)** `ironplc-platform-linux`: the §8.2 port adapters —
   `MonotonicClockPort` (`clock_gettime`), `WatchdogPort` (`/dev/watchdog`),
   `StoragePort` (`pwrite`/`fsync`), `PlatformPowerPort` (`reboot`) —
   behind the existing composition seams.
2. **(S)** Controller daemon composition root: one RT process, the §4.1
   thread set, `mlockall` + prefault, replacing the 5 ms command-cadence
   pump (`ha_pair.rs:41-45`) with a timer-driven cadence.
3. **(M)** Evidence/stamp types and the State Inventory registry artifact
   (task §5.2, S07) at the contract boundaries.
4. **(M)** Diagnostics provider: per-resource health records + durable
   bounded event/audit ring (extending the volatile `HaEvent` ring,
   `views.rs:13-31`).
5. **(L)** Dual-fiber pair-link drivers: two `NicPort` instances over
   `AF_PACKET`, after the transport ruling (ADR-0066 open item).
6. **(L)** EtherNet/IP binding behind the fencing seam, with the §13.3
   endpoint proof.
7. **(L)** Userspace output-enforcement boundary (§4.3) — blocked on the
   I/O firmware contract audit.

**Blocked on hardware decisions (task §26 rows):** CPU/board, memory,
watchdog/clock/reset topology (isolation, memory budget); exact kernel/
config (execution profile, timing); output groups, fallback, hold times;
real I/O adapters and fencing support (exclusive writer, handover time);
test loads, environmental envelope, acceptance thresholds.

## 6. v2.2 Delta Note

Against the v2.0-audit baseline, the v2.2 task adds D25–D36, the normative
state model S01–S12 (§4.10–4.16), the three views (§4.4–4.9, §19.5), the
registry (§4.14), T29–T44, and INV15–INV18. What changes for us:

- **D25** (layers ≠ FSM hierarchy): ALIGNED — never violated here; the
  only FSMs are justified lifecycles (S03).
- **D26** (no mega-`OsPort`): partially aligned through the existing
  narrow seams — the audit's §8 row moves ABSENT → PARTIAL (addendum).
- **D27** (no hidden host dependency): partially aligned — autonomous
  execution exists by construction (the pair pump runs with no client,
  `ha_pair.rs:41-80`); engineering-plane budgets and the absent-service
  policy (§4.8/§8.4) are undefined.
- **D28** (layer ≠ update unit): partially aligned — update-unit
  discipline exists for the application layer (A/B slots, one commit);
  platform-image membership/update units are undefined.
- **S01–S12:** largely ALIGNED via existing mechanisms (single-owner
  latches, epoch/lease producer separation, snapshot/crossload state
  scope, typed fail-closed refusals); the registry artifact and §5.2
  per-contract field lists remain to be written (§4.2 maps them).
- **T29–T44:** mostly target-side or already covered by the permit/epoch/
  fencing test suites; the acceptance-matrix adoption stands (audit §6,
  action 4). T41's bounded-execution claim holds by construction today
  (fixed 38-byte frames, container-sized buffers) and becomes a measured
  property with the daemon.

The audit document carries the formal re-classifications in its dated
addendum; only rows whose verdict actually changed are touched.

## 7. Proposed Roadmap Lines

In the roadmap's own voice, one line each:

- *"Linux platform adapters: MonotonicClockPort/WatchdogPort/StoragePort/PlatformPowerPort over clock_gettime, /dev/watchdog, pwrite/fsync, reboot(2) in a small ironplc-platform-linux crate behind the existing composition seams — requirements per the DCS task §8.2; size S."*
- *"Controller daemon composition root: one RT process (SCHED_FIFO scan + HA threads, engineering/diagnostics threads, mlockall+prefault) replacing the soft-device command-cadence pump — the DCS task §8.3 execution profile; size S."*
- *"State Inventory and evidence types: the registry artifact and Evidence/stamp types at the contract boundaries per the DCS task §4.14/§5.2 — formalizes what the permit/epoch/snapshot seams already enforce; size M."*
- *"Diagnostics provider: per-resource health records and a durable bounded event/audit ring (the HA event ring is volatile today) — requirements per the DCS task §9/§18.2; size M."*
- *"Dual-fiber pair-link drivers: two NicPort instances over AF_PACKET on the optical sync links, following the owner transport ruling (ADR-0066 open item) — the sanctioned N+1 network-driver work; size L."*
- *"Userspace output-enforcement boundary: pure admission + per-group output policy + the single write path behind the fencing seam — requirements per the DCS task §12; blocked on the I/O firmware contract audit and a hardware target; size L."*

## Out of Scope

- Any claim of qualification: PREEMPT_RT timing, watchdog properties, and
  storage endurance are measured per board after the §26 blockers clear.
- Kernel modules, a hypervisor/partitioned profile, and the VxWorks
  adapter — contract-feasibility reference only (task §19.4).
- Security posture (task §17): unchanged, still deferred by ADR-0063/0065
  and still conflicting; the owner ruling gates it.
