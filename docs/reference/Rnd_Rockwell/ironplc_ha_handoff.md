# IronPLC / RegulPLC — HA Architecture Handoff

## Purpose

This document is the authoritative handoff for continuing the IronPLC/RegulPLC architecture in a new chat.

The goal is **not** a minimal hot-standby implementation. The target is a **Rockwell ControlLogix-class high-availability architecture**, including qualification, state crossload, synchronization points, preemption semantics, distributed hot change, I/O ownership/fencing, deterministic takeover, and fault-injection validation.

The implementation should preserve the architecture and idioms of IronPLC wherever possible instead of wrapping the VM in a large external framework.

---

# 1. Non-negotiable project principles

## 1.1 IronPLC direction

Use IronPLC as the basis of the PLC execution system.

- `.iplc` is the deployment artifact.
- The controller runs compiled `.iplc`; the compiler/toolchain does not belong on the controller.
- `ironplc-vm` remains a small execution kernel.
- Libraries, dependency resolution, IDE integration, protocol configuration, and compilation are engineering-time concerns.
- Do not turn the runtime into a monolith.

## 1.2 IEC language compatibility

Stay within IEC 61131-3 semantics.

Do **not** introduce custom variable attributes/properties or language syntax merely to support runtime features if that makes the ST language vendor-specific.

Runtime metadata may exist in project/container metadata, but the IEC language model itself should remain standard-compatible.

## 1.3 No Service Manager architecture

`Service Manager` is not part of the architecture and must not be reintroduced.

Use explicit architectural planes instead:

- Execution Plane
- Replicated State Plane
- I/O Plane
- Redundancy Plane
- Protocol / External Service Plane

These planes may have dedicated Rust components, but there is no generic central `ServiceManager` registry controlling everything.

## 1.4 Rust style

Prefer IronPLC/Rust idioms already visible in the repository:

- caller-owned memory;
- borrowed slices;
- explicit lifetimes;
- newtypes for identifiers;
- enums for state machines;
- compile-time/static layouts;
- no heap allocation in scan-critical paths;
- no `HashMap<ServiceId, Box<dyn Service>>` in the VM;
- avoid premature abstraction;
- keep `unsafe` outside IronPLC VM; hardware access belongs behind HAL/runtime boundaries.

---

# 2. Runtime size / embedded direction

The important runtime is `ironplc-vm`, not the desktop CLI/compiler.

Current architectural observation:

```text
ironplc-vm-cli
      |
      v
 ironplc-vm
      |
      v
ironplc-container
```

`ironplc-container` already has a `no_std` zero-copy representation (`ContainerRef`). The embedded direction is therefore:

```text
.iplc in Flash
    |
    v
ContainerRef
    |
    v
IronPLC VM core
```

The embedded port should converge toward:

- `ContainerRef` instead of owned desktop `Container` where possible;
- caller-supplied buffers instead of `Vec`;
- external monotonic/cluster clock;
- no filesystem requirement;
- no CLI/logger dependencies;
- eventual `no_std` VM core.

Target runtime size remains an engineering target, not a measured fact yet. A rough goal is a few hundred KB of Flash for the execution core, with the real decision made only after actual cross-build/size measurement.

---

# 3. Protocol / network direction

The VM must remain protocol-neutral.

Do not put Modbus, MQTT, OPC UA, EtherNet/IP, TSN, etc. inside the interpreter.

External protocols belong outside the VM and interact through deterministic runtime boundaries.

Important latest project direction: **do not assume EtherCAT as the final controller/network architecture**. The current RegulPLC direction is toward **EtherNet/IP and TSN**, with standard Ethernet hardware where possible. Earlier EtherCAT discussion should be treated as a reference for I/O/process-image and hot-standby semantics, not as a mandatory final fieldbus decision.

Protocol-facing IEC FBs may exist, but asynchronous network work must never block the PLC scan.

---

# 4. Core execution architecture

The stable top-level split is:

```text
                 Regul Runtime

  +---------------------------------------+
  | Redundancy Plane                      |
  | qualification / fencing / ownership   |
  +---------------------------------------+
  | Scheduler / HA execution coordination |
  +---------------------------------------+
  | IronPLC VM                            |
  | IEC execution only                    |
  +---------------------------------------+
  | Replicated State                      |
  +---------------------------------------+
  | Process Image / I/O Plane             |
  +---------------------------------------+
  | HAL / network / device drivers         |
  +---------------------------------------+
```

The VM must not know about sockets, network protocols, redundant Ethernet links, hardware registers, DMA, or external services.

---

# 5. Process Image model

IronPLC already points toward:

```text
INPUT_FREEZE
    ->
EXECUTE
    ->
OUTPUT_FLUSH / COMMIT
```

This is the correct basis for normal I/O.

## 5.1 I/O is state exchange, not a service call

Examples that belong to Process Image:

- `%I`
- `%Q`
- `%M`
- local digital/analog I/O
- remote cyclic I/O
- cyclic industrial Ethernet I/O image

External asynchronous operations such as MQTT publish, OPC UA method call, generic Modbus client request, HTTP, etc. belong to the external protocol plane.

## 5.2 Stable snapshot rule

The VM never reads a live DMA/bus buffer directly.

Use stable generations / ping-pong buffers:

```text
bus/DMA
  -> RX buffer A/B
  -> publish completed generation
  -> INPUT_FREEZE
  -> VM
```

Outputs similarly use staged/committed buffers.

## 5.3 Fault semantics

Partial execution must not leak half-written physical outputs.

```text
VM writes pending output
        |
        v
successful execution + valid HA commit
        |
        v
I/O COMMIT
        |
        v
physical outputs
```

If execution traps/faults before the commit point, physical outputs retain the last-known-good committed image.

---

# 6. Four distinct boundaries

Do not collapse these concepts into one generic sync point.

## 6.1 `ExecutionSafePoint`

A point where the local executor may safely yield/preempt/debug-pause.

## 6.2 `StateSyncPoint`

A point where replicated logical state may be crossloaded to the secondary.

Typical example: end of a program, similar to Rockwell redundancy synchronization semantics.

## 6.3 `IoCommitPoint`

A point where a certified logical output state becomes eligible for physical publication.

State synchronization and physical output publication are separate concerns.

## 6.4 `HotChangeBarrier`

A global quiescent point at which code can be replaced safely.

No active continuation may still point into the old code image.

This becomes essential once execution is preemptive/resumable.

---

# 7. HA coordinate model

Use strongly typed identifiers.

Conceptually:

```rust
struct HaPosition {
    domain: DomainId,
    epoch: Epoch,
    generation: Generation,
    task: TaskId,
    sync_point: SyncPointId,
}
```

Meaning:

- `DomainId` — identity of one redundant control domain/pair.
- `Epoch` — ownership/fencing generation; changes on takeover.
- `Generation` — committed logical state generation; monotonically advances.
- `SyncPointId` — logical HA synchronization boundary.
- task/program identifiers describe deterministic resume semantics.

**Epoch dominates Generation.**

A controller with an older epoch may never control physical outputs, even if it claims a numerically larger generation.

---

# 8. Primary / Secondary execution model

Follow the Rockwell model for standard logic:

- Primary executes standard application logic.
- Secondary does not independently run the same standard application in lockstep.
- Primary crossloads logical state and execution position at synchronization boundaries.
- Secondary remains prepared to take over from the latest committed HA resume point.

Do not duplicate ordinary logic execution on both controllers merely for HA.

A separate future safety profile may use dual execution/cross-compare, but **HA is not functional safety**.

---

# 9. Replicated vs local vs external state

## 9.1 Replicated state

Must survive takeover:

- PLC global/program variables;
- persistent FB instance state;
- arrays/strings/struct persistent data;
- `%M` logical memory;
- task scheduling/checkpoint state;
- forces;
- committed output state;
- application/configuration generation;
- logical resume positions;
- side-effect journal metadata where required.

## 9.2 Local execution state

Never crossload:

- operand stack;
- VM call frames;
- instruction PC;
- function-local temporary variables;
- temp allocator/buffers;
- native pointer state;
- local DMA state;
- debugger implementation state.

Secondary resumes from a logical HA boundary, not from a copied machine/bytecode PC.

## 9.3 External state

Cannot be memcpy-replicated:

- TCP sessions;
- MQTT transport sessions;
- OPC UA sessions;
- physical I/O ownership;
- remote side effects;
- filesystem/network transport internals.

These require explicit HA-aware protocol semantics.

---

# 10. Preemptive / resumable executor redesign

Current IronPLC cooperative execution is insufficient for full Rockwell-like semantics.

The future executor should be resumable and preemptive.

Conceptually:

```rust
enum ExecutionYield {
    ProgramComplete,
    TaskComplete,
    Preempted,
    SyncBoundary,
    DebugPaused,
    Fault,
}
```

Important rule:

> Local preemption position and HA resume position are different things.

The Primary may resume a suspended task from an exact local continuation.

The Secondary resumes only from the last committed logical `ResumePoint`, typically beginning of the program segment after the last HA sync.

## 10.1 Per-task execution contexts

Current IronPLC has shared VM execution buffers. True preemption requires per-active-task execution contexts for:

- operand stack;
- frame stack;
- temp state;
- frame-local function variables.

## 10.2 Function locals must leave global scratch storage

Current function/FB call machinery uses shared variable-table/scratch regions. That is unsafe under preemption because a higher-priority task can overwrite a suspended lower-priority task's locals.

Move function-local/call-local scratch to frame/task-local execution storage.

Persistent FB instance state remains replicated; execution scratch is local.

## 10.3 Long opcodes

Instruction-count quanta alone do not bound preemption latency if one opcode can perform a very large copy/string/array operation.

Long operations must either:

- become resumable micro-operations; or
- be explicitly bounded non-preemptible critical operations.

Preemption latency must be measurable and bounded.

---

# 11. Scheduler semantics

Target behavior is Rockwell-like priority/preemption semantics.

Each task has explicit runtime state, e.g.:

```text
Idle
Ready
Running
Suspended
```

A higher-priority ready task may preempt a lower-priority running task.

Do not create multiple concurrent activations of the same task. If a periodic/event activation occurs while the previous activation remains running/suspended, increment an overlap counter and apply the defined overlap policy rather than allocating another task instance.

Scheduler checkpoint state is itself replicated state.

Watchdog state must survive switchover.

---

# 12. Cluster time

Local MCU uptime is insufficient for HA.

Use an HA monotonic cluster-time domain.

Scheduler releases and watchdog deadlines are expressed against cluster time, not controller-local uptime.

Qualification must include bounded clock offset/skew. A Secondary whose cluster-clock error exceeds policy must become non-promotable.

Watchdog deadline does not reset during takeover.

---

# 13. Replicated State Store

All persistent writes must pass through mutation gates that perform dirty tracking.

Do not expose unrestricted `&mut [u8]` access to replicated persistent memory.

Desired architecture:

```text
ReplicatedState
  |- Slots
  |- DataRegion
  |- SchedulerState
  |- ForceState
  |- CommittedOutputs
  `- SideEffectJournal
```

## 13.1 Dirty semantics

A write marks replicated state dirty even when the new value equals the old value.

Do not implement dirty tracking as `if old != new`.

This intentionally matches the useful Rockwell-style write/crossload semantics.

## 13.2 Replication segments, not whole logical objects

Do not make a huge array/UDT one indivisible crossload unit.

Use compiler-defined `ReplicationSegment`s of bounded size.

```text
State Object
   -> Replication Segment 0
   -> Replication Segment 1
   -> ...
```

Dirty tracking operates on segments; **Generation** provides state atomicity.

Segment size is a compile/deployment optimization parameter and must have a bounded worst-case crossload cost.

## 13.3 Deterministic delta order

Use bitmaps/static descriptors, not heap maps/sets.

Dirty segments are streamed in deterministic order.

---

# 14. HA commit model

A simple `delta -> ACK` protocol is insufficient.

The true authority must be represented by a **Commit Certificate** issued/accepted by the independent arbitration plane.

Conceptually:

```rust
struct CommitCertificate {
    domain: DomainId,
    epoch: Epoch,
    generation: Generation,
    state_digest: StateDigest,
    application: ApplicationHash,
    quorum_proof: QuorumProof,
}
```

`PREPARED` is never takeover-authoritative.

Only a certified committed generation is eligible for promotion/output publication.

---

# 15. Arbitration / fencing

Do not rely on two-controller heartbeat alone to prevent split brain.

There must be an independent arbitration/fencing plane.

Use quorum semantics rather than trusting a single software heartbeat.

Conceptual goal:

```text
Controller A
Controller B
Independent arbitration/witness domain

=> quorum determines Owner/Epoch/CommittedGeneration
```

The exact hardware implementation is still to be designed, but the architectural semantics are mandatory.

## 15.1 Ownership lease

Physical I/O acceptance must require a valid ownership/fencing token, e.g.:

```rust
struct OwnershipLease {
    domain: DomainId,
    owner: ControllerId,
    epoch: Epoch,
    valid_until: ClusterTime,
    nonce: ...,
    quorum_proof: ...,
}
```

Outputs are accepted only when Domain, Owner, Epoch, lease validity and committed Generation are valid.

---

# 16. Critical redundancy-link rule

Redundancy transport uses two optical links, `L1` and `L2`.

This rule is absolute:

```text
L1=1, L2=1  -> redundancy active
L1=1, L2=0  -> redundancy active, degraded path
L1=0, L2=1  -> redundancy active, degraded path
L1=0, L2=0  -> NO valid redundancy link
```

If **both redundancy links are lost** (`L1=0 && L2=0`):

- Secondary is immediately disqualified.
- Secondary loses `SynchronizedSecondary` / `TakeoverReady` status.
- Secondary has **no right to automatic takeover**, regardless of heartbeat timeout or subsequent apparent Primary failure.
- Current Primary continues executing the application.

The governing rule is:

> **No valid redundancy link => no automatic takeover.**

Loss of only one link is degraded transport, not redundancy loss.

Do not “solve” two-link loss by promoting both sides based on timeout.

---

# 17. I/O ownership and commit

State synchronization and physical output publication are separate.

Outputs are:

```text
PLC logic
  -> pending logical output
  -> HA-certified state generation
  -> IoCommitPoint
  -> valid ownership Epoch/Lease
  -> physical publication
```

A stale controller cannot command outputs even if it continues executing.

Critical outputs should have statically known ownership/commit groups. Avoid two independent tasks racing to own the same physical output. Prefer compile-time rejection or explicit arbitration logic.

---

# 18. Hot change / online edit

Hot change is a distributed transaction in HA mode.

Both controllers must stage and validate the new artifact before commit.

Required checks include at minimum:

- artifact signature/integrity;
- runtime/container ABI;
- state-layout compatibility;
- I/O-layout compatibility;
- replication-layout compatibility;
- required resources;
- protocol/service ABI requirements;
- HA policy compatibility.

Do not hot-swap merely at an old cooperative `scan boundary` once preemption exists.

A `HotChangeBarrier` must guarantee no suspended continuation still points into the old code image.

Keep old/new code images transactionally available until the distributed update is safely committed/recoverable.

---

# 19. Artifact identity / hashes

A single generic hash is insufficient for diagnostics and HA qualification.

Prefer distinct identities such as:

```text
CodeHash
StateLayoutHash
ExecutionConfigHash
IoLayoutHash
ReplicationLayoutHash
ServiceAbiHash
```

Then derive an overall `ApplicationIdentity` as needed.

This lets qualification explain *what* differs rather than returning only "hash mismatch".

All execution-relevant metadata, including task/sync policy and I/O mapping, must be covered by integrity/signature protection.

---

# 20. External protocol side effects

The external protocol plane must not mutate live VM state asynchronously.

Reads use snapshots; writes/commands enter at deterministic boundaries through mailboxes/queues.

Network FBs must not block the PLC executor.

## 20.1 Replay semantics

Not every external operation can be exactly-once after takeover.

Classify operations explicitly, e.g.:

```rust
enum ReplaySemantics {
    State,
    Idempotent,
    Deduplicated,
    AtLeastOnce,
    NonReplayable,
}
```

A transactional outbox/journal helps, but it cannot magically make an arbitrary remote RPC exactly-once.

`NonReplayable` operations require an explicit recovery/operator policy.

---

# 21. Libraries

Library management is engineering-time functionality.

The controller does not need a dynamic library loader.

Desired flow:

```text
Application
 + IEC libraries
 + dependency resolver / lock file
        |
        v
compiler/linker
        |
        v
single application .iplc
        |
        v
ironplc-vm
```

Use versioned dependencies and reproducible lock information on the engineering side.

Runtime sees only the final compiled artifact and required runtime ABI capabilities.

---

# 22. Resource model

Do not make an MCU choice based only on the small size of `ironplc-vm`.

Full HA memory includes:

```text
replicated state
+ task execution contexts
+ frame-local storage
+ operand stacks
+ process images
+ dirty bitmaps
+ prepared/undo crossload state
+ redundancy transport buffers
+ network/I/O stack
+ protocol services
+ RTOS/HAL
+ hot-change staging
+ diagnostics
+ safety reserve
```

Before freezing the final controller MCU, implement a static worst-case resource calculator.

For an HA controller, preserve a large RAM reserve (target roughly 25-30% after worst-case sizing).

External ECC RAM may be justified for HA/hot-change staging even if execution-critical state remains in internal SRAM.

H743 remains a candidate, not yet proven adequate for the **full** Rockwell-class HA controller.

---

# 23. What has already survived the devil review

The following decisions are considered strong and should not be casually reopened:

1. IronPLC VM remains a pure IEC execution kernel.
2. Normal I/O uses Process Image, not generic services.
3. External protocols remain outside the VM.
4. Primary executes standard application; Secondary mirrors state.
5. Replicated state is separate from local execution state.
6. HA resume position is not local bytecode PC.
7. Preemption requires per-task contexts and frame-local scratch.
8. All persistent writes pass through dirty-tracked mutation gates.
9. Crossload uses bounded replication segments.
10. `Epoch` is the I/O fencing/ownership generation.
11. `Generation` is the logical committed-state generation.
12. `PREPARED` is not takeover-authoritative.
13. Commit authority is represented by an independently verifiable certificate/quorum state.
14. `ExecutionSafePoint`, `StateSyncPoint`, `IoCommitPoint`, and `HotChangeBarrier` are distinct concepts.
15. Hot change is a distributed transaction.
16. HA is not functional safety.
17. No valid redundancy link means no automatic takeover.

---

# 24. Known open problems

These are intentionally unresolved and are the next design work, not forgotten issues:

1. Exact Arbitration Quorum hardware topology.
2. Exact Commit Certificate / quorum protocol.
3. HA cluster-clock implementation and skew limits.
4. Physical fencing implementation for I/O ownership.
5. Final redundant network/I/O technology and takeover semantics under the current EtherNet/IP/TSN direction.
6. Exact `ReplicationSegment` sizing/layout algorithm.
7. Resumable long-op implementation in IronPLC VM.
8. Static memory/resource calculator and final CPU/RAM choice.
9. Precise hot-change state-migration rules when layouts differ.
10. External protocol HA capability model.
11. Security model for RLink/redundancy traffic: authentication, integrity, anti-replay, key lifecycle.
12. Fault-injection test matrix.

---

# 25. Immediate next step

Do **not** start RLink packet design yet.

The next task is:

> **Design `Arbitration Quorum + Commit Certificate + Fencing` formally.**

It must define:

- quorum participants;
- authority source for `Epoch`;
- authority source for committed `Generation`;
- ownership lease lifecycle;
- behavior for every combination of controller/link/witness failure;
- dual-link (`L1/L2`) degradation rules;
- explicit rule that `L1=0 && L2=0` disqualifies Secondary and forbids automatic takeover;
- commit-certificate creation/validation;
- stale-primary fencing;
- recovery after reboot;
- cluster-clock authority;
- security/authentication requirements;
- exact invariants that must never be violated.

Only after this is proven internally consistent should RLink wire format, redundant I/O ownership, and distributed hot change be finalized.

---

# 26. Mandatory design-review question

Every future subsystem must answer:

> **What happens if the current Primary disappears at this exact point?**

The answer must identify:

- authoritative Epoch;
- authoritative committed Generation;
- Secondary eligibility;
- logical ResumePoint;
- state that may be replayed;
- state that must not be replayed;
- I/O ownership;
- external side-effect status;
- whether automatic takeover is permitted.

If any of these is ambiguous, the design is not complete.

