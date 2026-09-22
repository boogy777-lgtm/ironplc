# Spec: Controller OS Portability Review — Zephyr / RT-Thread

Devil's-advocate review (task §24 discipline: a concrete counterexample per
finding, not a style assessment) answering the owner's question: *does the
Linux controller architecture hold if the OS is Zephyr or RT-Thread instead
of Linux?* The design under review is
[Linux Controller Layer Architecture](linux-controller-architecture.md);
the requirements source is the DCS firmware task v2.2
(`docs/reference/dcs-firmware-task-v2.2-ru.md`, §8 ports/execution
profile/capability composition, §14.4 update units, §19.2/19.4 platform
matrix, §23 N+1 same-class table) and
[ADR-0066](../adrs/0066-linux-execution-platform-and-global-epoch.md).
External facts below are from the official Zephyr and RT-Thread
documentation and repositories, accessed 2026-09-22 (sources in §7).

## 1. Verdict

**The layer/contract/port design holds for an OS swap — verified, not
assumed. The bindings, the Rust toolchain, and the isolation guarantees do
not carry over, and one of the three (toolchain) is a P0 owner decision
that can end the swap before it starts.**

Three pillars, each checked against evidence rather than intent:

1. **The port shapes are OS-neutral by construction.** Every task §8.2
   port in the design's §3 table is a narrow consumer-need trait
   (task D26): monotonic ticks, bounded queues, atomic publish/durability,
   arm/feed/status, bounded submit/receive, reason-coded reset. No port
   signature names a Linux API. The task anticipated exactly this shape:
   an OS swap "may not change domain rules for `pthread`/FreeRTOS APIs,
   but it does not forbid changing these ports' implementations and
   repeating the tests" (task §19.3).
2. **The domain code is already OS-free.** The design's §5 list is real:
   both statecharts (`compiler/ironplc-redundancy/src/statechart.rs:41,332`),
   the permit latch and all four runtime seams
   (`compiler/runtime/src/host.rs:166,204-221,605-636,419-437`), the
   epoch/lease/crossload codec in abstract ticks
   (`epoch.rs`, `lease.rs:21-28`, `crossload.rs:61-108`), and the `no_std`
   execution kernel (`compiler/container/src/lib.rs:1`). None of these
   imports `std` in its logic.
3. **The task's own matrix names the swap as the central case.** Task
   §19.4: ports/BSP, composition, execution profile, and qualification
   change; domain rules, state semantics, contracts, and invariants
   survive. Task §23: FreeRTOS → Linux and "add a VxWorks provider" are
   same-class under exactly this division of labor.

What breaks, stated plainly:

- **Bindings.** The existing seam *implementations* are the Linux answers:
  `UdpPort` over `std::net::UdpSocket` (`compiler/ironplc-redundancy/src/udp.rs:39-109`),
  the session server over `std::net::TcpListener`
  (`compiler/vm-cli/src/tcp.rs:40-53`), the `SlotStore` over `std::fs`
  (`compiler/vm-cli/src/slot_store.rs:1-33`), the host loop over `std`
  threads. None of these compiles on an RTOS; all six ports must be
  re-developed behind their (unchanged) seams.
- **Toolchain.** The platform is Rust end to end. Zephyr's official Rust
  support is an optional module with self-declared "rather minimalistic"
  bindings and a short target list [S7][S8]; RT-Thread has no official
  Rust support at all [S12][S13]. Finding F1 (P0).
- **Isolation.** `cgroups`/`isolcpus`/process separation map to MPU
  memory domains or to nothing (F2). The qualified execution profile
  changes from "qualified Linux (PREEMPT_RT) profile" to "deterministic
  RTOS profile" — and per task D11/§8.3, timing bounds are requalified,
  never inherited, in both directions.

## 2. Port Table per OS

Flags: ✅ direct mechanism · ⚠ mechanism change, design holds · ❌ no
direct equivalent (guarantee changes). Linux column repeats the design's
§3 mapping; the design has seven task ports — the six the owner named
plus `BoundedSignalPort`, kept here for completeness.

| Task port (§8.2) | Linux (ADR-0066 profile) | Zephyr | RT-Thread | Flag |
|---|---|---|---|---|
| `MonotonicClockPort` | `clock_gettime(CLOCK_MONOTONIC)` | `k_uptime_ticks()` / `k_cycle_get_64()` over the hardware timer, 64-bit accumulation, per-boot identity | `rt_tick_get()` (OS tick, typically 100 Hz–1 kHz, config-dependent) + HWTIMER device for µs resolution | ⚠ both — tick granularity and wrap must be re-specified in the port's resolution/drift fields (task §8.2) |
| `CyclicExecutionPort` | `SCHED_FIFO` scan thread, `isolcpus`, IRQ affinity, timer-driven release | Fixed-priority preemptive threads (lower numeric = higher priority); cooperative range for driver-class work; `k_timer` release; per-thread CPU masks (`CONFIG_SCHED_CPU_MASK`, `k_thread_cpu_mask_*`), and a CPU can be reserved at build time (`CONFIG_MP_MAX_NUM_CPUS`) — the closest analog to `isolcpus` [S1][S6] | Preemptive fixed-priority scheduler, up to 256 levels, time-slice round-robin within a level [S9]; SMP scheduler exists in source (`scheduler_mp.c`, `cpu_mp.c`) but the programming manual documents single-core semantics and exposes no CPU-affinity or IRQ-affinity API [S9][S13] | ⚠ Zephyr (priority mapping + CPU pinning exists); ❌→ RT-Thread: priorities ✅, **CPU isolation has no direct equivalent** on small SMP |
| `BoundedSignalPort` | Bounded `mpsc` + `eventfd` | `k_msgq`/`k_fifo` (statically sized) + `k_poll` | `rt_mb`/`rt_mq` + `select`/`poll` via DFS | ✅ both |
| `StoragePort` | `pwrite` + `fdatasync`; tmp + rename + marker | littlefs over the VFS (`fs_sync`), or raw flash-map partitions + NVS/FCB; erase-before-write, erase-block alignment, wear budget (NVS documents an erase-cycle lifetime formula) [S4][S5] | DFS: elmfat default, jffs2 for raw NOR, littlefs via the package ecosystem; POSIX `fsync` exists over DFS [S11][S12] | ⚠ both — **no `O_DIRECT`/page-cache semantics**; durability = flash program/erase completion; slot store better re-bound as partition-per-slot than files (F3) |
| `WatchdogPort` | `/dev/watchdog`, magic close, `WDIOC_SETTIMEOUT` | `wdt_install_timeout()` with window min/max + early-feed callback, `wdt_feed()`, per-board options (`WDT_OPT_PAUSE_IN_SLEEP`); some boards cannot be disabled (`-EPERM`) [S3] | `rt_device` control: `RT_DEVICE_CTRL_WDT_SET_TIMEOUT` / `GET_TIMELEFT` / `KEEPALIVE` / `START` / `STOP` [S10] | ✅ both APIs exist; window/reset/output-state limits remain board-bound (task §12.3, §26 blocker — unchanged); see F5 for the policy trap |
| `NetworkTransportPort` | UDP sockets, `SO_PRIORITY`, `AF_PACKET` for the sync link | Native IP stack, BSD-sockets subset (UDP/TCP, blocking/non-blocking), `AF_PACKET` packet sockets — since v4.2 the `SOCK_RAW`/`IPPROTO_RAW` combination was removed, use `AF_PACKET`/`SOCK_DGRAM`/`ETH_P_ALL`; traffic classification as the priority analog; PTP/gPTP available [S4][S5] | SAL: BSD sockets over lwIP (also AT Socket/WIZnet); `SOCK_RAW` listed but **no `AF_PACKET` / link-layer raw path in SAL** [S11] | ⚠ Zephyr (raw link feasible, binding details change); ❌ RT-Thread: engineering TCP ✅, **dual-fiber raw link must bind below SAL to the eth driver/netdev** — the design's "ADOPT sockets" row silently becomes "DEVELOP" (F4) |
| `PlatformPowerPort` | `reboot(2)` / orderly poweroff; watchdog as last resort | `sys_reboot()` + PM subsystem | `rt_hw_cpu_reset()` / `rt_hw_cpu_shutdown()` BSP hooks + pm component | ✅ both |
| *Isolation* (middleware row, not a §8.2 port) | `cgroups`/cpuset, affinity, `mlockall`, process boundary | Optional MPU-backed user mode: threads at reduced privilege, memory domains, hardware stack guards — "designed for devices with MPU hardware" [S2] | Standard RT-Thread: flat kernel address space, no userspace/MPU partitioning (RT-Thread Smart is a separate µkernel product for Cortex-A) [S12][S13] | ❌ **no direct equivalent for process isolation** — fault-containment guarantee changes per task §19.2 (F2) |

## 3. Findings

### F1 — Rust toolchain on RTOS is the P0 — **P0**

- **Counterexample.** Take the composition root the design reuses
  (`compiler/vm-cli/src/serve.rs:110-125`, `ha_pair.rs:50-72`) and
  cross-compile it for a Zephyr Rust target (`thumbv7m-none-eabi`, the
  official sample's target class) or RT-Thread. Compilation fails at the
  first `std` import — `std::net`, `std::fs`, `std::thread` — before any
  of the six ports is reached. The "six narrow ports" claim is true of
  the traits and false of every existing binding.
- **Verified facts.** Zephyr: official optional module
  (`CONFIG_RUST`, west `project-filter`, CMake `rust_cargo_application()`,
  Cargo `staticlib`, mandatory `#![no_std]`, `zephyr` crate 0.1.0);
  "Only a few targets currently support Rust"; the bindings are "under
  development, and are currently rather minimalistic" (official README)
  [S7][S8]. RT-Thread: C-first toolchain story (scons/Env; MDK/IAR/GCC;
  documented environments are POSIX/CMSIS/C++), no Rust build
  integration or bindings in the official repository [S12][S13].
- **Violated task property.** Task §19.4 itself: a platform's "Rust
  target/FFI is not deemed available or verified until separately
  implemented." Plus D11 (a port is not automatic equivalence) and the
  repository's own fence: workspace-denied `unsafe` with exemptions only
  for build/test tooling (repo `AGENTS.md`, lint fences) — and every
  RTOS escape route from this finding crosses `unsafe` or `no_std`
  rewrites measured in person-months, not lines.
- **Options, quantified.** (a) *Rust core + C FFI shim to RTOS services*:
  every OS call crosses an `unsafe` boundary; requires an owner-approved
  amendment of the no-unsafe fence for a sanctioned platform-FFI crate
  with its own audit regime — the policy cost is that the platform's
  universal "safe Rust" guarantee ends at every seam, and the trust model
  at the boundary becomes C's. (b) *Rust core on bare metal with the
  RTOS as HAL only*: the container/VM is `no_std`-capable, but the
  runtime host, HA shell, and session server are `std`-based (large
  rewrite), and networking/storage still call the RTOS's C APIs — still
  FFI; this is effectively a different architecture, not this design's
  port. (c) *Stay Linux-class for the controller profile*: ADR-0066
  stands; RTOS targets are deferred to a future MCU-class product whose
  middleware estate is C-first by nature.
- **Minimal change.** A decision record only — no code on any branch
  until the owner rules. If (a) is ever chosen: add
  `ironplc-platform-<rtos>` with a documented, reason-carrying
  `#![allow(unsafe_code)]`, keep all domain crates 100% safe, and list
  the shim's toolchain identity in the release manifest (task §14.4).
- **Residual risk.** Zephyr Rust is a moving target (minimalistic
  bindings, few targets — we would extend the bindings ourselves); for
  RT-Thread we would own the entire FFI surface unmaintained by anyone
  else.

### F2 — Flat memory: no process isolation — **P1**

- **Counterexample.** The EtherNet/IP adapter — or the lwIP stack inside
  RT-Thread's SAL, third-party C code that the RTOS profile makes
  *more* likely in-process — suffers a heap overflow. On the Linux
  profile the engineering/diagnostics planes are cgroup-bounded and the
  adapter can be a separate address space; on the RTOS profile the
  corrupted memory may belong to the epoch/lease producer or the permit
  latch. HA arbitration and output admission then execute on state with
  no integrity, and the INV03/INV04 endpoint checks certify nothing.
  The T02 scenario widens too: one thread spinning with interrupts
  masked starves everything; no lower scheduler bounds it.
- **Violated task property.** Task §19.2: "logical contracts by
  themselves do not provide spatial isolation; if MPU/privilege
  separation is absent, the fault model must reflect that shared-memory
  corruption can affect neighboring state owners"; §19.4 isolation row
  ("only hardware/port mechanisms actually provided"); INV11 (the
  software reaction path may itself be gone); T02.
- **Minimal change.** Re-declare the RTOS execution profile *without*
  process isolation; write into the fault model that software-fault
  containment = whole-image watchdog reset with outputs held by the
  hardware inhibit (T02/G04 — the board blocker, unchanged and still
  mandatory). On Zephyr, optionally fence third-party C stacks with MPU
  memory domains [S2]. Replace the cgroup containment of the engineering
  plane with priority separation + bounded queues (its loss must still
  never cancel admitted control — task §4.8).
- **Residual risk.** A whole-image reset makes every software fault a
  cold-restart event for HA state; the pair must tolerate that failover
  frequency, and task §15.2's production default (no spontaneous
  resumption of physical control after an unknown/corruption fault) must
  gate RUN re-entry on every such reset.
- **Side effect that simplifies.** Update units collapse to whole-image
  (task §14.4 already makes BSP/kernel/core-OS updates platform-restart
  events); the D28 "runtime replacement" question disappears — the whole
  product is one image.

### F3 — Storage semantics: files to flash transactions — **P1**

- **Counterexample.** Two, one per OS. (1) RT-Thread DFS + elmfat on raw
  SPI flash: elmfat has no journaling; a power loss between directory
  update and FAT flush yields cross-linked clusters — the crash table
  heals a *missing marker*, it cannot heal a half-renamed file that
  parses as valid. (2) Zephyr on internal flash: an erase/program of the
  storage partition stalls the XIP bus, so the durability sync — invoked
  on the update path — can blow the scan deadline; Zephyr's own NVS docs
  note the MPU reconfiguration flash writes require [S4].
- **Violated task property.** Task §5.2 durability evidence; §14.2 (a
  power loss on any durable step yields the previous or the new whole
  version, never a mixed one); INV12; T44.
- **Minimal change.** Re-bind `SlotStore` from files to
  partition-per-slot over the flash map (or a raw-block DFS binding):
  slot write = erase + program with a per-slot monotonic counter + CRC;
  keep the seam and the committed-wire latch (`host.rs:411-417`); move
  durability writes off the scan path (deferred to the HA/diagnostics
  thread with its own deadline). Re-derive durability evidence as
  "flash program completed, erase budget intact" instead of `fdatasync`.
- **Residual risk.** Flash wear becomes a product-lifetime parameter
  (the NVS documentation's erase-cycle formula [S4] is the model);
  mixed-version protection now rests on our slot-counter logic rather
  than a battle-tested VFS.

### F4 — Networking: sockets adoptable, raw link is not (RT-Thread) — **P1**

- **Counterexample.** The dual-fiber `NicPort` (the ADR-0066 N+1 driver)
  moves raw L2 frames per link with PHY-counter evidence. Zephyr: fine —
  `AF_PACKET` exists; but since v4.2 the `SOCK_RAW`/`IPPROTO_RAW`
  combination was removed and the binding must use
  `AF_PACKET`/`SOCK_DGRAM`/`ETH_P_ALL` [S5]. RT-Thread: SAL accepts
  `AF_INET`/`AF_INET6` only; there is no `AF_PACKET` and no link-layer
  raw path in the abstraction [S11]. Below SAL the only interfaces are
  lwIP netif callbacks / eth driver ops — so the sync-link binding must
  live *inside* the RTOS network layer, and the design's "ADOPT sockets"
  middleware row silently becomes "DEVELOP a driver" for that link.
- **Violated task property.** Task §19.4 (storage/network services are
  "explicitly chosen and jointly qualified"); D26 (the port contract is
  bounded submit/receive + link evidence — driver internals must not
  leak into the domain); T33 (driver reset, stale completion, session
  identity).
- **Minimal change.** Keep `NicPort` unchanged; write a Zephyr
  `AF_PACKET` binding and an RT-Thread binding over the eth
  driver/netdev ops behind it; declare in the platform profile that on
  RT-Thread the sync-link driver is a developed component with
  drain/quiescence semantics (task §14.4), not an adopted socket.
- **Residual risk.** A driver-level binding shares the kernel
  networking fault domain (see F2) and requalifies with every lwIP/
  netdev change (T34).

### F5 — Watchdog: mechanism portable, vendor docs push the wrong policy — **P2**

- **Counterexample.** Both vendors' own documentation steers a porter to
  the forbidden policy: RT-Thread's manual demonstrates feeding the
  watchdog from the idle hook — a periodic alive-tick, exactly what task
  §16.3 rejects [S10]. A port done "by the book" ships the wrong safety
  policy. Zephyr adds window semantics: `wdt_install_timeout()` takes a
  min/max window (feeding too early resets), plus per-board options like
  `WDT_OPT_PAUSE_IN_SLEEP`, and some boards refuse `wdt_disable()`
  (`-EPERM`) [S3].
- **Violated task property.** Task §16.3 (feeding on evidence of
  progress — scan-commit counter + permit state — never a periodic
  tick); §8.2 (`WatchdogPort` carries the hardware's actual limits);
  §12.3/T02/G04 (independent enforcement path).
- **Minimal change.** None to the design — the feeding policy is OS-free
  and maps to `wdt_feed` / `RT_DEVICE_CTRL_WDT_KEEPALIVE`; add the
  window minimum to the port's status reporting; keep the hardware
  inhibit as the independent path (board blocker unchanged).
- **Residual risk.** Same as Linux: window/reset/output-state properties
  are board-bound; the OS swap moves none of the task §26 blocker rows.

### F6 — Engineering host tooling independence — **P2** (verified, not a gap)

- **Counterexample attempt.** D27 asks whether host tooling is a hidden
  dependency of continuous control. The session server's only OS
  touchpoints are the listener socket and the line codec; the typed
  command enums and codec (`compiler/vm-cli/src/serve.rs:1-62`) are
  pure. No counterexample found within scope: on condition that
  `tcp.rs` is re-bound to the target stack's sockets (Zephyr BSD subset
  has TCP with build-time socket count [S4]; RT-Thread SAL has TCP over
  lwIP [S11]) and ADR-0065's single-session refusal is preserved, host
  tools are unaffected — the protocol bytes do not change.
- **Violated task property.** None if rebound; D27/T29 otherwise.
- **Minimal change.** Re-bind `tcp.rs` behind the session seam.
- **Residual risk.** On Zephyr, TCP socket count and net-buffer sizing
  are Kconfig-time constants: the engineering session now competes with
  the HA pair link for buffers, so the resource catalog (task §8.4) must
  budget both explicitly.

## 4. Same-class or new-class? (task §23)

The task's test: what may change (ports/BSP, composition, execution
profile, qualification) versus what must survive (domain rules, state
semantics, contracts, invariants), plus the closing rule: *if an existing
contract cannot express a new fundamental property, revise the contract
explicitly — do not hide a necessary change inside an adapter while
formally claiming "nothing changed."*

- **At the port-binding layer: same-class, unconditionally.** This is
  the "add a VxWorks provider" row: port implementations, BSP,
  packaging, and qualification change; the semantics of the existing
  domain contracts and behavior models survive. The §8.2 traits were
  built for exactly this (D26), and §5 shows every mechanism surviving
  the swap.
- **At the controller-profile layer: conditional — and this is the
  devil's point.** §23's "new board with the same capabilities" row is
  same-class only "if the declared failure/timing profile has not
  changed." The swap as casually proposed changes it twice: the
  isolation guarantee (process + cgroups → MPU-or-nothing, F2) and the
  toolchain guarantee (tier-1 Rust on Linux → experimental bindings or
  none, F1). The same table classifies *new* isolation guarantees as
  new-class ("bare metal → partitioned/hypervisor profile"); a swap that
  silently *drops* a declared guarantee cannot be same-class either, or
  the rule means nothing. By the task's own closing rule, dropping the
  guarantee without re-declaring it would be hiding a necessary change
  in an adapter.
- **Classification verdict.** Adapters + requalification = same-class
  (the VxWorks-row pattern). The unqualified swap = new-class at the
  platform layer, gated entirely on owner decisions 1–3 in §5 — the
  classification is an owner ruling, not an engineering conclusion.

Timing note (task §8.3): neither Zephyr nor RT-Thread needs a
PREEMPT_RT-class patch — determinism comes from fixed-priority
preemption, tick configuration, and static allocation [S1][S9] — but the
task's own caveat cuts both ways: an RTOS, like PREEMPT_RT, "is not
proof of a concrete delay on a chosen board" [S14]. D11 stands in both
directions: every timing bound is requalified per board.

## 5. Owner decision list

1. **Rust-on-RTOS strategy.** (a) Rust core + C FFI shim — requires an
   owner-approved amendment of the workspace no-`unsafe` fence and ends
   the universal safe-Rust guarantee at every seam; (b) bare-metal Rust
   core with the RTOS as HAL — a large `std`→`no_std` rewrite of the
   runtime host/HA shell/session server *and* still FFI for
   networking/storage, i.e., a different architecture; (c) stay
   Linux-class for the controller profile and treat RTOS targets as a
   future, separately-profiled product. **Recommendation: (c) now.**
   Revisit (a) only against a Zephyr LTS whose Rust module covers the
   chosen target, with the fence amendment recorded in an ADR. F1 makes
   this a decision, not a detail: every other finding assumes it answered.
2. **Isolation requirement.** Does the product need process isolation,
   or is task/MPU-level containment enough per the failure model (task
   §19.2)? If an MCU-class profile is declared, the honest answer is:
   software-fault containment = whole-image reset, outputs held by the
   hardware inhibit (T02/G04 unchanged); MPU memory domains [S2] become
   *optional* fencing for third-party C stacks, not a product guarantee.
   **Recommendation: for the RTOS profile, declare task-level containment
   + hardware inhibit; require MPU domains only when a C protocol stack
   lands in-process; write the weakened guarantee into the profile
   explicitly (task §23 closing rule).**
3. **Target class.** Zephyr/RT-Thread imply MCU-class hardware (Cortex-M
   primary; small SMP for Zephyr, scheduler-only SMP for RT-Thread
   [S6][S9][S13]); there is no PREEMPT_RT story there, and the timing
   narrative changes from "qualified Linux profile" (ADR-0066) to
   "deterministic RTOS profile" with every §8.3/§16 budget measured anew
   (T26). **Recommendation: do not swap the controller profile of
   ADR-0066; if MCU-class nodes are wanted (remote I/O, edge gateways),
   run this review's port table as their own profile with decisions 1–2
   answered for that product.** Input to the open ADR-0066 transport
   item: F4 affects the dual-fiber feasibility ranking per OS — it does
   not decide it.

No new ADR is recorded here: these are decision items. An ADR follows
the owner's ruling (a new-class platform profile, a fence amendment, or
rejection of the swap).

## 6. What must NOT change — and the requalification checklist

**Must not change (cite — all OS-free by construction):**

- Domain contracts and state semantics: both statecharts
  (`statechart.rs:41,332`), the edit FSM (`host.rs:9-15`), the epoch/
  lease producer separation (ADR-0062/0066; `epoch.rs:19-25`,
  `lease.rs:36-69`), the permit latch (`host.rs:204-221,701-707`),
  snapshot/crossload (`host.rs:419-513`, `crossload.rs:61-108`).
- The HA design: SYNC/CONTROL charts, the fencing seam and its
  capability descriptor (`fencing.rs:199-218`), liveness and calibration
  in abstract ticks.
- Hot edit: candidate FSM, committed-wire latch, and the A/B slot
  *semantics* (ADR-0064) — F3 changes the binding, not the transaction
  protocol or INV07–INV09.
- The evidence model: task §5.2/S01–S12 field lists at contract
  boundaries and the State Inventory mapping (design §4.2) — types only.
- The composition discipline: no global `OS_READY`, per-capability
  evidence, no registry (task D09/D25/D26) — mechanism, not OS policy.

**Requalification checklist (task §19.4, T26/T30/T34, §8.5):**

- **T26** — identical input traces ⇒ identical domain results; platform
  timing tests pass independently (wakeup latency, interference, load
  envelope measured per board, task §16.3).
- **T30** — new board/BSP: clock/reset identities, device lifetimes,
  memory alignment and **cache coherency on SMP** (cache-incoherent
  architectures exist and Zephyr handles them via
  `CONFIG_KERNEL_COHERENCE` [S6]), boot defaults, IRQ/DMA lifetimes,
  I/O reset behavior within the used functions (task §8.5).
- **T34** — any OS/middleware component replaced with a compatible API
  but different ABI/timing (kernel config, lwIP/netdev, littlefs, Rust
  toolchain) ⇒ the release manifest flags restart + requalification; the
  toolchain identity is part of the manifest (task §14.4).
- Plus the standing rows the swap does not retire: watchdog
  window/reset-state/output-state measurement (§12.3), storage
  power-loss-at-every-step and endurance (T44/§14.2), network driver
  reset/stale-completion (T33), and the hardware inhibit proof (T02/G04)
  as a board property.

## 7. Sources (all accessed 2026-09-22)

- [S1] Zephyr — Scheduling: https://docs.zephyrproject.org/latest/kernel/services/scheduling/index.html
- [S2] Zephyr — User Mode (MPU-backed, memory domains): https://docs.zephyrproject.org/latest/kernel/usermode/index.html
- [S3] Zephyr — Watchdog driver API: https://docs.zephyrproject.org/latest/doxygen/html/group__watchdog__interface.html
- [S4] Zephyr — Networking overview; Storage index; NVS: https://docs.zephyrproject.org/latest/services/connectivity/networking/overview.html , https://docs.zephyrproject.org/latest/services/storage/index.html , https://docs.zephyrproject.org/latest/services/storage/nvs/nvs.html
- [S5] Zephyr — VFS file systems; v4.2 migration guide (AF_PACKET change): https://docs.zephyrproject.org/latest/services/storage/file_system/index.html , https://docs.zephyrproject.org/latest/releases/migration-guide-4.2.html
- [S6] Zephyr — SMP (CPU masks, `CONFIG_MP_MAX_NUM_CPUS`, coherence): https://docs.zephyrproject.org/latest/kernel/services/smp/smp.html
- [S7] Zephyr — Rust language support: https://docs.zephyrproject.org/latest/develop/languages/rust/index.html
- [S8] zephyr-lang-rust module repository (README; maturity): https://github.com/zephyrproject-rtos/zephyr-lang-rust
- [S9] RT-Thread — Thread management (preemptive priorities, time slices): https://www.rt-thread.io/document/site/programming-manual/thread/thread/
- [S10] RT-Thread — WATCHDOG device: https://www.rt-thread.io/document/site/programming-manual/device/watchdog/watchdog/
- [S11] RT-Thread — Socket Abstraction Layer: https://www.rt-thread.io/document/site/programming-manual/sal/sal/
- [S12] RT-Thread — Virtual file system; project README (footprint, C-first): https://www.rt-thread.io/document/site/programming-manual/filesystem/filesystem/ , https://github.com/RT-Thread/rt-thread
- [S13] RT-Thread — kernel source tree (scheduler_up/scheduler_mp, cpu_up/cpu_mp — SMP present, undocumented in the manual): https://github.com/RT-Thread/rt-thread/tree/master/src
- [S14] Linux PREEMPT_RT theory of operation (cited by task §8.3): https://cdn.kernel.org/doc/html/latest/core-api/real-time/theory.html
