# Online Change Performed by the Runtime Host

status: accepted
date: 2026-09-15

## Context and Problem Statement

The container format specifies a `layout_hash` over everything that determines
variable memory and an online change protocol that replaces bytecode at a scan
boundary while variable, FB instance and process-image memory stay intact
(`specs/design/bytecode-container-format.md`, "Layout Hash and Online Change").
The hot edit P0 work implements the protocol end to end.

The VM is a pure execution kernel. `Vm::load` borrows a container and a
caller-owned `VmBuffers`, `VmReady::resume` continues a session without
re-running init, and the `VmRunning` typestate borrow makes "a scan is in
flight" a lifetime fact ([ADR-0009](0009-typestate-vm-lifecycle.md)). Nothing
in the VM owns a second code image, and nothing in the workspace owned two
containers plus their buffers, so the swap had no home. Where should online
change live: inside the VM as a code-pointer swap, or around it in a host?

## Decision Drivers

* The VM must stay a pure execution kernel: no runtime allocation, no second
  code image, and no awareness of hot edit ([ADR-0010](0010-no-std-vm-for-embedded-targets.md)).
* A change must be atomic with respect to a scan: the program never executes a
  mix of old and new code within one scan.
* Layout compatibility is all-or-nothing. The container spec's deterministic
  ordering means a declaration change produces a different `layout_hash`, and
  the correct response is a full stop-load-start, never a partial migration.
* The baseline invariant: reverting a test changes code, never process state.
* P0 has no user-facing runtime problem codes; the engineering CLI surface that
  would assign them comes later.

## Considered Options

* **VM-internal code-pointer swap.** The VM owns the active and candidate code
  sections and swaps a code pointer at the scan boundary.
* **Host-level reload (chosen).** A runtime host owns both containers and the
  buffers, ends the `VmRunning` borrow at the boundary, and reloads the VM from
  the candidate on the host's buffers.
* **Cold restart with state export/import.** Stop the VM, load the candidate
  cold, and copy state back. Rejected for P0: it needs a second state-transfer
  mechanism that the buffer reload already provides, and it breaks the
  boundary model the container spec describes.

## Decision Outcome

Chosen option: **host-level reload**, because it keeps the VM a pure execution
kernel and makes the two-image protocol expressible in safe Rust: the
`VmRunning` borrow that makes a scan in flight is the same borrow that must end
before the code can change.

The `ironplc-runtime` crate implements the host:

* **The swap is host-level.** `RuntimeHost` owns the normal container, the
  staged candidate, and `VmBuffers`. At a scan boundary it ends the
  `VmRunning` borrow and loads the VM from the destination container with
  `resume(rounds)` on the host's buffers. The VM never holds two images and
  never learns that a change happened.
* **Validation tuple.** A candidate is accepted only when its `layout_hash`,
  `num_variables`, header `flags`, input/output/memory image sizes, and task
  table all equal the active application's. Violations are typed
  `OnlineChangeError` variants (`LayoutIncompatible`, `IoIncompatible`,
  `ScheduleIncompatible`); no V-codes exist yet — user-facing codes arrive with
  the P0.5 engineering CLI.
* **FSM.** `stage` is the controller's accept: validate and hold. `test` and
  `untest` record a pending swap that takes effect at the next scan boundary;
  `untest` switches back to the normal artifact and never restores variable or
  data-region bytes (the baseline invariant). `assemble` promotes the candidate
  to the normal artifact. `cancel` discards a staged candidate while the normal
  artifact is active. A candidate never executes before `test`.
* **Scheduler state.** Every swap reloads from the destination container,
  which re-arms every scheduler field (`tasks`, `programs`) exactly as a cold
  load does. Task-table validation protects schedule identity; what is re-armed
  is execution history. This is a P0 caveat: per-task scan counts, watchdog
  maxima and overrun counts reset at test and untest.
* **Generations.** `LogicGeneration` versions one compiled artifact; staging
  assigns the next value. `ApplicationGeneration` versions the active manifest
  and advances on `assemble` only, so test and untest move the executing code
  without committing a new application generation.
* **Hash contract.** `header.layout_hash` is meaningful only for a serialized
  container: `write_to` computes it. A header that was built but never written
  carries zeros and must not be compared. Buffers are rebuilt from the
  destination container so a candidate that grows the data region is
  accommodated outside the scan; the persistent prefix of the variable table
  and data region is copied over, and transient buffers are left fresh because
  nothing in them outlives a scan boundary.

### Consequences

* Good, because the VM is unchanged by hot edit: it keeps its typestate,
  caller-owned-buffer design, and online change is testable at the host seam.
* Good, because the swap is a plain reload of caller-owned memory: no `unsafe`,
  no second code image, no lifetime escape from the `VmRunning` borrow.
* Good, because a declaration edit is rejected before it can run, and the
  running application produces the same values after the rejection.
* Bad, because the host keeps two containers and copies the persistent buffer
  prefix at each swap, so the working set is larger than a code-pointer swap
  would need.
* Bad, because scheduler history is re-armed at every swap; per-task counters
  restart even on untest (the P0 caveat above).
* Neutral, because user-facing error codes are deferred: the host returns
  typed errors and the CLI surface will translate them.

## More Information

* `specs/design/bytecode-container-format.md`, "Layout Hash and Online Change"
  and "Variable Table" (type section).
* [ADR-0009](0009-typestate-vm-lifecycle.md) — the typestate borrow the host
  ends before swapping.
* [ADR-0010](0010-no-std-vm-for-embedded-targets.md) — why the buffers stay
  caller-owned.
* `compiler/runtime/src/host.rs`, `online_change.rs`, `generation.rs`,
  `error.rs`; acceptance tests in `compiler/runtime/tests/acceptance.rs`.
