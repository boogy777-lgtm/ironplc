# IronPLC Fork — Hot Edit + Redundancy Architecture Baseline

Status: **Accepted architectural baseline**  
Project direction: **IronPLC fork / IEC 61131-3 / Rust / bytecode VM / Studio 5000-like online edit semantics**

---

## 1. Core principles

The project follows IEC 61131-3 semantics and uses IronPLC as the compiler/runtime foundation.

The runtime architecture is deliberately split into independent concerns:

```text
IEC SOURCE
   ↓
IronPLC compiler
   ↓
.iplc container
   ↓
Runtime Core
   ├── ExecutableImage
   ├── RuntimeState
   ├── Scheduler
   ├── VM
   └── HotEditEngine

Redundancy Layer
   ├── Role Manager
   ├── State Replication
   ├── Generation Replication
   ├── Edit Transaction Replication
   ├── Qualification / Synchronization
   └── Takeover Policy
```

**Redundancy is a separate layer above Runtime Core.**

The VM, scheduler and HotEditEngine must remain usable on a standalone PLC without any redundancy subsystem present.

---

## 2. Non-negotiable architecture boundaries

### 2.1 Runtime Core owns execution semantics

Runtime Core owns:

- bytecode execution;
- task scheduling;
- process image handling;
- persistent application state;
- initialization;
- hot edit FSM;
- executable generation switching;
- hot-edit compatibility checks.

Runtime Core must not know whether the controller is:

- standalone;
- Primary;
- Secondary;
- synchronized;
- degraded;
- disqualified.

Those concepts belong to the Redundancy Layer.

### 2.2 Redundancy Layer owns pair semantics

Redundancy Layer owns:

- Primary / Secondary roles;
- qualification;
- synchronization;
- redundancy transport state;
- takeover eligibility;
- state replication;
- executable generation replication;
- hot-edit transaction replication;
- switchover behavior during Test Edits;
- pair epoch / primary epoch;
- stale-message rejection.

Redundancy Layer does **not** execute IEC bytecode itself.

---

## 3. POU model

POU remains the IEC semantic unit.

```text
POU
├── PROGRAM
├── FUNCTION_BLOCK
└── FUNCTION
```

A POU is not a runtime memory blob.

Conceptually:

```text
PouType
├── InterfaceSchema
├── StateSchema
└── Logic
```

For a function block:

```text
FUNCTION_BLOCK Motor
        │
        ├── InterfaceSchema
        ├── StateSchema
        └── Logic
```

Instances are separate from the POU type:

```text
Motor1 : Motor
Motor2 : Motor
Motor3 : Motor
```

All instances share executable logic but have independent state.

```text
                 Motor Logic
                     ↑
          ┌──────────┼──────────┐
          │          │          │
       Motor1     Motor2     Motor3
          │          │          │
        State      State      State
```

---

## 4. `.iplc` role

`.iplc` is a **versioned binary artifact container**, not the object model itself.

For P0 it remains a full application container.

```text
.iplc
├── header
├── bytecode/functions
├── constants
├── state layout metadata
├── init image metadata
├── task/schedule metadata
├── I/O metadata
├── debug metadata
└── compatibility hashes
```

Do **not** split the project into physical per-POU `.iplc` files in P0.

Per-Logic/per-POU artifacts may be introduced later as an optimization.

---

## 5. Runtime memory model

Runtime memory is split into three categories.

### 5.1 Executable code

Immutable for the duration of one scheduling round.

```text
ExecutableImage
├── bytecode
├── constant pool
├── function table
├── execution metadata
├── schedule description
└── debug map
```

### 5.2 Persistent runtime state

Must survive HOT-1 edits.

```text
RuntimeState
├── global variables
├── PROGRAM state
├── FB instance state
├── timer/counter state
├── RETAIN/PERSISTENT data
├── input image
├── output image
├── memory image
└── hardware/session state where applicable
```

### 5.3 Transient execution scratch

Does not survive a safe point and does not need migration.

```text
ExecutionScratch
├── operand stack
├── call frames
├── temporary values
├── instruction pointer
└── current execution frame
```

At a hot-edit safe point, execution scratch must be empty/resettable.

---

## 6. Fundamental runtime invariant

> **One scheduling round executes against exactly one immutable ExecutableGeneration.**

Forbidden:

```text
Round N
├── Program A → generation 10
└── Program B → generation 11
```

Required:

```text
Round N      → generation 10
SAFE POINT
Round N + 1  → generation 11
```

This invariant is the foundation of deterministic hot edit.

---

## 7. Cold load vs hot load

### 7.1 Cold start

```text
.iplc
  ↓
parse
  ↓
build ExecutableImage
  ↓
derive StateLayout
  ↓
allocate RuntimeState
  ↓
apply InitImage
  ↓
RUN
```

### 7.2 Hot stage

```text
new .iplc
   ↓
parse
   ↓
build candidate ExecutableImage
   ↓
derive compatibility metadata
   ↓
compare StateAbi / ScheduleAbi / IoAbi
   ↓
if compatible → stage
```

Hot stage must **not**:

- allocate replacement application state;
- reinitialize runtime state;
- reset FB instances;
- reset timers/counters;
- restart scheduler timing;
- stop the PLC.

---

## 8. Compatibility model

P0 uses strict compatibility.

```text
CompatibilityManifest
├── RuntimeAbiHash
├── StateAbiHash
├── ScheduleAbiHash
├── IoAbiHash
├── InitImageHash
└── ExecutableHash
```

### RuntimeAbiHash

Determines whether this bytecode format/runtime contract can be executed by the current runtime.

### StateAbiHash

Describes persistent application state layout.

Includes canonical information such as:

- semantic owner/path;
- storage class;
- variable name for P0;
- IEC type;
- nested type layout;
- array dimensions;
- offset;
- size;
- alignment.

HOT-1 requires an exact match.

### ScheduleAbiHash

Describes task topology/scheduling configuration.

For P0 any schedule change requires cold download.

### IoAbiHash

Describes process image/I/O layout.

For P0 any I/O layout change requires cold download.

### InitImageHash

Initial values are **not part of StateAbi**.

Example:

```iecst
Counter : DINT := 100;
```

changed to:

```iecst
Counter : DINT := 200;
```

must not modify the current online value during hot edit.

Initializers apply only during cold initialization.

---

## 9. P0 Hot Edit scope

Supported:

- full-project compilation;
- full ExecutableImage replacement;
- logic-only changes;
- state preservation;
- Test / Untest / Assemble semantics;
- cancel of accepted edits;
- exact compatibility checks;
- code revert without state rollback.

Not supported in P0:

- state schema migration;
- persistent variable rename online;
- task topology changes online;
- I/O topology changes online;
- per-POU patching;
- incremental compiler;
- stable FieldId migration;
- arbitrary object graph modifications.

---

## 10. Rockwell-like Hot Edit FSM

The user-visible hot-edit semantics follow Studio 5000 / Logix.

There are two related FSMs.

### 10.1 Engineering FSM

`PENDING` exists only on the engineering workstation.

```text
CLEAN
  │ Start Edit
  ▼
PENDING
  ├── Cancel Pending ───────→ CLEAN
  └── Accept Pending ───────→ controller ACCEPTED
```

The PLC does not know about Pending edits.

### 10.2 Controller Hot Edit FSM

```text
NORMAL(A)
   │ Accept B
   ▼
ACCEPTED(A,B)
   ├── Cancel Accepted ─────→ NORMAL(A)
   │
   └── Test
        ▼
     TESTING(A,B)
        ├── Untest ─────────→ ACCEPTED(A,B)
        └── Assemble ───────→ NORMAL(B)
```

Semantics:

#### Accept

```text
A executes
B is present and validated in controller
```

#### Test

At the next safe point:

```text
A active → B active
```

A remains available as Original.

#### Untest

At the next safe point:

```text
B active → A active
```

Current runtime state is preserved.

#### Assemble

```text
B becomes NORMAL
A loses its Original/fallback semantic role
```

#### Cancel Accepted

Allowed when Original A is active.

```text
B discarded
A remains active
```

---

## 11. Code revert is not state rollback

This is a hard invariant.

Example:

```text
A:
Counter := Counter + 1

Counter = 100

TEST B:
Counter := Counter + 10
```

After three scans:

```text
Counter = 130
```

UNTEST back to A:

```text
next value = 131
```

Not 101.

Therefore:

> **Untest/Revert switches executable logic only. RuntimeState always remains the current process state.**

---

## 12. Runtime structures reserved now

Conceptual Rust model:

```rust
struct PlcRuntime {
    active: ExecutableImage,
    state: RuntimeState,
    scheduler_state: SchedulerState,
    scratch: ExecutionScratch,
    hot_edit: HotEditEngine,
}
```

```rust
struct ExecutableGeneration {
    id: GenerationId,
    image_hash: ImageHash,
    state_abi: StateAbiHash,
}
```

```rust
struct EditTransaction {
    id: EditTxnId,
    base: GenerationId,
    candidate: GenerationId,
    phase: EditPhase,
    switchover_policy: TestSwitchoverPolicy,
}
```

```rust
enum EditPhase {
    Accepted,
    Testing,
}
```

```rust
enum TestSwitchoverPolicy {
    RevertToOriginal,
    RetainTest,
}
```

The exact Rust data layout is implementation-defined; these are semantic contracts.

---

# Part II — Redundancy Architecture

## 13. Redundancy is a separate runtime layer

Hard architectural rule:

> **Redundancy must not be implemented inside the VM, scheduler, bytecode executor or core HotEdit FSM.**

Instead:

```text
┌──────────────────────────────┐
│      Redundancy Layer        │
│                              │
│ RoleManager                  │
│ StateReplicator              │
│ GenerationReplicator         │
│ EditReplicator               │
│ QualificationManager         │
│ TakeoverManager              │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│        Runtime Core          │
│                              │
│ VM                           │
│ Scheduler                    │
│ RuntimeState                 │
│ HotEditEngine                │
│ ExecutableGeneration Store   │
└──────────────────────────────┘
```

Standalone runtime remains fully functional with no Redundancy Layer loaded.

---

## 14. Two orthogonal FSMs

Do not combine Hot Edit and Redundancy into one large state machine.

### Hot Edit FSM

```text
NORMAL
ACCEPTED
TESTING
```

### Redundancy FSM

Conceptually:

```text
Primary
Secondary
Synchronizing
SynchronizedSecondary
Degraded
Disqualified
TakeoverReady
```

The two FSMs communicate through guards/barriers.

Never create a Cartesian explosion such as:

```text
PrimaryTestingSynchronized
PrimaryTestingDegraded
SecondaryAcceptedSynchronized
...
```

---

## 15. Redundancy transport rule

For the project, the following rule is hard:

```text
L1=1, L2=1  → redundancy active
L1=1, L2=0  → redundancy active, degraded
L1=0, L2=1  → redundancy active, degraded
L1=0, L2=0  → no valid redundancy link
```

When both redundancy links are lost:

```text
Secondary immediately loses:
- SynchronizedSecondary
- TakeoverReady
- right to automatic takeover
```

Primary continues executing the application.

Hard formula:

> **No valid redundancy link => no automatic takeover.**

Heartbeat timeout must never override this rule.

---

## 16. Replication channels are logically separate

Redundancy traffic is separated into at least three logical domains.

### 16.1 State replication

Replicates current process/application state.

```text
StateReplicator
├── runtime state changes
├── retain state
├── process image state as required
└── sequence information
```

### 16.2 Generation replication

Replicates executable generations / `.iplc` candidate data.

```text
GenerationReplicator
├── image chunks
├── image hash
├── signature
├── StateAbiHash
└── compatibility metadata
```

### 16.3 Edit transaction replication

Replicates semantic hot-edit phase.

```text
EditReplicator
├── EditTxnId
├── base generation
├── candidate generation
├── edit phase
└── switchover policy
```

Do not mix these into one opaque replication stream.

---

## 17. Redundant ACCEPT semantics

On standalone PLC:

```text
ACCEPT B
→ A active
→ B staged
```

On a redundant pair, `ACCEPTED` means more:

> **Both controllers possess the same validated candidate generation B.**

Flow:

```text
Engineering
    │
    ▼
Primary receives B
    │
    ├── validate B
    │
    ├── verify compatibility
    │
    ▼
replicate B to Secondary
    │
    ▼
Secondary
    ├── verify image hash
    ├── verify RuntimeAbi
    ├── verify StateAbi
    ├── verify ScheduleAbi
    └── store candidate
    │
    ▼
ACK CandidateReady(B)
    │
    ▼
Pair enters ACCEPTED(A,B)
```

If Secondary does not confirm candidate readiness, the pair has not reached the redundancy ACCEPT barrier.

---

## 18. Redundant TEST semantics

Once both controllers hold A and B:

```text
Primary
A = Original
B = Candidate

Secondary
A = Original
B = Candidate
```

`TEST` does not need to transfer the full image again.

Primary moves to:

```text
execution_generation = B
```

Secondary takeover behavior depends on policy.

### Default policy

```text
TestSwitchoverPolicy::RevertToOriginal
```

During test:

```text
Primary executes B
Secondary takeover target = A
```

State produced while B is running is still replicated to Secondary.

If takeover occurs:

```text
CODE  = A
STATE = latest replicated current state
```

This means code reverts but process state does not.

### Optional policy

```text
TestSwitchoverPolicy::RetainTest
```

During test:

```text
Primary executes B
Secondary takeover target = B
```

This is an advanced option and should not be the default.

---

## 19. Secondary distinguishes stored generation from takeover generation

Do not model Secondary with a generic `active_generation` only.

Important concepts:

```text
Primary:
    execution_generation

Secondary:
    takeover_generation
```

During default Test mode:

```text
Primary.execution_generation = B
Secondary.takeover_generation = A
```

During RetainTest:

```text
Primary.execution_generation = B
Secondary.takeover_generation = B
```

This distinction must exist in the Redundancy Layer, not the VM.

---

## 20. Redundant UNTEST

```text
Primary TESTING B
Secondary stores A + B
```

UNTEST:

```text
Primary:
execution_generation B → A at safe point

Secondary:
takeover_generation = A

B remains accepted/staged on both controllers
```

Runtime state is not rolled back.

---

## 21. Redundant ASSEMBLE

ASSEMBLE is stronger than TEST and requires a redundancy barrier.

Flow:

```text
Primary
   │
   │ PREPARE_ASSEMBLE(EditTxnId, B)
   ▼
Secondary
   │
   ├── verify B present
   ├── verify matching hash
   ├── verify matching edit transaction
   └── ACK ReadyToAssemble
   │
   ▼
Primary safe point
   │
   ▼
B becomes NORMAL
   │
   ▼
COMMIT_ASSEMBLE
   │
   ▼
Secondary
B becomes normal takeover generation
```

Only after successful pair commit may Original A lose its semantic fallback role.

Physical deletion of A may be delayed for housekeeping/recovery, but user-visible FSM is already NORMAL(B).

---

## 22. Redundant CANCEL ACCEPTED

If A is active and B is only accepted:

```text
Primary: discard B
Secondary: discard B
Pair returns to NORMAL(A)
```

Cancellation itself must be replicated so both controllers return to the same edit transaction state.

If B is actively being tested, the required sequence is:

```text
UNTEST
↓
ACCEPTED(A,B)
↓
CANCEL ACCEPTED
↓
NORMAL(A)
```

---

## 23. EditTransaction identity

Introduce this now, before redundancy implementation.

```text
EditTxnId
```

Example semantic record:

```text
EditTransaction #71
├── BaseGeneration       = A
├── CandidateGeneration  = B
├── Phase                = TESTING
├── StateAbiHash         = ...
├── CandidateImageHash   = ...
└── SwitchoverPolicy     = RevertToOriginal
```

The same logical transaction must be visible on both members of a synchronized pair.

---

## 24. Epochs and stale message protection

Reserve protocol fields now:

```text
PairEpoch
PrimaryEpoch
SequenceNumber
EditTxnId
GenerationId
```

Each redundancy control message must be attributable to a current primary epoch.

Example:

```text
PrimaryEpoch = 47
EditTxnId     = 71
Command       = TEST
Generation    = B
```

After switchover:

```text
PrimaryEpoch = 48
```

Any delayed message from epoch 47 is stale and must be rejected.

No full distributed-consensus protocol is required for the intended two-controller deterministic Primary/Secondary model.

---

## 25. TakeoverReady is a derived safety state

Secondary may expose `TakeoverReady = true` only if all required conditions are satisfied.

At minimum:

```text
valid redundancy transport
AND role qualification valid
AND required generation(s) present
AND required generation hash verified
AND StateAbi compatible
AND EditTxn state synchronized
AND takeover policy resolved
AND state replication sufficiently current
```

Example during Testing:

```text
Primary:
  execution_generation = B
  EditTxn = 71

Secondary:
  has A
  has B
  EditTxn = 71
  takeover_generation = A
  current state mirror valid
```

Only then may Secondary be takeover-ready under default Original-on-switchover policy.

---

## 26. TakeoverConsistencyToken

A useful architectural concept for fast consistency checks:

```text
TakeoverConsistencyToken {
    normal_generation,
    candidate_generation,
    edit_txn,
    edit_phase,
    takeover_generation,
    state_sequence,
    state_abi,
}
```

This does not need to be a public API type yet, but the data relationships should be preserved in protocol design.

---

## 27. Link loss during ACCEPT

Example:

```text
Primary has complete B
Secondary has received only 60% of B
L1=0
L2=0
```

Required behavior:

```text
Primary:
  continues A
  may keep B locally staged

Secondary:
  immediately Disqualified
  TakeoverReady = false
  partial B is not considered valid
```

The distributed ACCEPT barrier was never reached.

After transport restoration:

```text
re-establish link
↓
validate/re-transfer B
↓
verify hash
↓
resynchronize state
↓
qualify Secondary
↓
restore TakeoverReady
```

---

## 28. Link loss during TEST

Example:

```text
Primary executes B
Secondary has A+B
```

Then:

```text
L1=0
L2=0
```

Required behavior:

```text
Secondary:
  immediately Disqualified
  TakeoverReady = false

Primary:
  continues executing B
```

No heartbeat timeout may promote the disconnected Secondary automatically.

After reconnection Secondary must resynchronize:

- edit transaction;
- required code generations;
- switchover policy;
- current runtime state;
- pair epoch/primary epoch;

before becoming `SynchronizedSecondary` and `TakeoverReady` again.

---

## 29. Hot edit and qualification interlock

Do not allow arbitrary simultaneous structural controller mutations.

Introduce a logical mutation fence:

```text
ControllerMutationFence
├── Idle
├── HotEditTransaction
├── RedundancyQualification
├── SystemUpdate
└── FirmwareUpdate
```

For early versions these modes are mutually exclusive.

Example:

```text
ASSEMBLE
```

must not race with:

```text
qualify replacement Secondary
```

unless a future protocol explicitly defines how to coordinate them.

---

## 30. Observer boundary for future redundancy

The HotEditEngine should not directly depend on redundancy.

Reserve an observer/coordinator boundary.

Conceptually:

```rust
trait HotEditObserver {
    fn candidate_ready(...);
    fn test_requested(...);
    fn untest_requested(...);
    fn assemble_requested(...);
    fn cancel_requested(...);
}
```

Standalone controller:

```text
LocalHotEditObserver
```

Redundant controller:

```text
ReplicatedHotEditCoordinator
```

The exact trait shape may change, but dependency direction must remain:

```text
Redundancy Layer
      ↓ uses
HotEditEngine
```

Never:

```text
HotEditEngine
      ↓ depends on
Redundancy implementation
```

---

## 31. Recommended crate/module boundary

Long-term conceptual Rust layout:

```text
ironplc-runtime-core
├── vm
├── scheduler
├── executable_image
├── runtime_state
├── compatibility
└── hot_edit

ironplc-redundancy
├── role
├── transport
├── qualification
├── synchronization
├── state_replication
├── generation_replication
├── edit_replication
├── takeover
└── epochs

ironplc-engineering-protocol
├── status
├── accept_edits
├── test_edits
├── untest_edits
├── assemble_edits
└── cancel_edits
```

Names are provisional; separation of responsibility is not.

---

## 32. Implementation roadmap

### P0 — Standalone Hot Edit

Implement:

```text
RuntimeState separated from ExecutableImage
CompatibilityManifest
full .iplc rebuild
full ExecutableImage stage
safe-point Test/Untest
Assemble
Cancel Accepted
state preservation
```

Acceptance tests:

1. PROGRAM variable survives edit.
2. FB instance state survives edit.
3. StateAbi change is rejected.
4. Untest reverts code but not current state.

### P0.5 — Engineering protocol

Implement controller commands:

```text
GET_STATUS
ACCEPT_EDITS
TEST_EDITS
UNTEST_EDITS
ASSEMBLE_EDITS
CANCEL_EDITS
```

VS Code remains a thin client.

### P1 — Redundancy architecture implementation

Implement separate layer:

```text
RoleManager
StateReplicator
GenerationReplicator
EditReplicator
QualificationManager
TakeoverManager
```

Preserve the same HotEditEngine.

### P2 — Granular code artifacts

Optional optimization:

```text
whole ExecutableImage
       ↓
per Logic / per Routine artifact
```

No user-visible FSM change.

### P3 — State-schema compatible hot change

Introduce:

```text
Stable FieldId
State migration
HOT-2
```

Again, no need to redesign the hot-edit FSM.

---

## 33. Architectural decisions frozen by this document

The following decisions are considered accepted unless explicitly reopened:

1. **IronPLC is the chosen base path.**
2. **IEC 61131-3 POU semantics are preserved.**
3. **Rust is the primary implementation language for compiler/runtime core.**
4. **`.iplc` is a container, not the semantic object model.**
5. **Code and persistent state have independent lifetimes.**
6. **One scheduling round uses one immutable executable generation.**
7. **P0 may rebuild the entire project on the engineering PC.**
8. **P0 hot change swaps the whole ExecutableImage.**
9. **P0 requires exact StateAbi/ScheduleAbi/IoAbi compatibility.**
10. **Initial values are separate from StateAbi and are not reapplied on hot edit.**
11. **Hot Edit FSM follows Rockwell semantics: Pending → Accepted → Test → Assemble, with Untest and Cancel.**
12. **Pending exists only in the engineering environment.**
13. **Untest/revert switches code only; it never rolls back process state.**
14. **Redundancy is a separate layer above Runtime Core.**
15. **Hot Edit FSM and Redundancy FSM remain orthogonal.**
16. **Redundant ACCEPT requires both controllers to possess and validate the candidate generation.**
17. **Default Test switchover policy is Original-on-switchover.**
18. **Retain Test on switchover is optional advanced behavior.**
19. **Secondary takeover generation is distinct from Primary execution generation.**
20. **No valid redundancy link means no automatic takeover.**
21. **Loss of both redundancy links immediately disqualifies Secondary.**
22. **Loss of one redundancy link only degrades transport if the other link is valid.**
23. **Primary continues application execution after total redundancy-link loss.**
24. **State replication, generation replication and edit replication are logically separate.**
25. **EditTxnId, GenerationId, PairEpoch and PrimaryEpoch are reserved in the architecture now.**
26. **TakeoverReady is derived from explicit consistency conditions, not heartbeat timeout alone.**
27. **Runtime core must remain usable without redundancy.**
28. **Redundancy must depend on runtime interfaces; runtime must not depend on redundancy implementation.**

---

## 34. Next design target

The next design work should focus on **P0 crate/API boundaries**, specifically:

```text
ExecutableImage
RuntimeState
ExecutionScratch
CompatibilityManifest
HotEditEngine
GenerationStore
Scheduler safe-point hook
```

Then implement the four P0 acceptance tests before adding networking, redundancy transport or VS Code UI behavior.

