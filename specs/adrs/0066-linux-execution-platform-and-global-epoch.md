# Linux Execution Platform and Global Epoch

status: accepted
date: 2026-09-22

## Context and Problem Statement

The DCS firmware task audit
([DCS Firmware Task Audit](https://github.com/boogy777-lgtm/ironplc/blob/8a7f6d0d09b00436daebfd669babd39f2e0f2e13/specs/design/dcs-firmware-task-audit.md)) recorded
D11 (an RTOS→Linux port is not automatic real-time equivalence) as
out-of-scope-by-decision, listed the task's topology mandate (§2.8: two
dedicated optical sync links) as a conflict with the decided port map
(audit §3(c)), and left the task's §13.3 epoch questions (fencing issuer,
reboot identity, replay protection) to the pending arbitration/quorum
design. On 2026-09-22 the owner ruled on five items: the controller
profile's platform, the epoch's semantics, the candidate redundancy
transports, the place of the network driver work, and the claim that an
I/O abstraction already exists. Which of these are decisions, and what do
they commit the project to before any port or target work starts?

## Decision Drivers

* The task's §8.3 demands a qualified execution profile, not
  `realtime_available: bool`: kernel/config, scheduling policy,
  thread/IRQ priorities, memory locking/prefault, CPU/power policy, I/O
  interference, and no unqualified virtualization. PREEMPT_RT changes
  preemption/locking/interrupt handling; it is not proof of a concrete
  delay on a chosen board.
* The task's §23 (N+1) classifies FreeRTOS → Linux as same-class: domain
  FSMs and semantic contracts survive; timing bounds do not inherit — they
  are requalified per platform.
* ADR-0062 already fixes the epoch's producer rule (only the HA supervisor
  mints, and only at scan commit); the owner's "one global epoch" names
  the scope that rule actually implements — the audit addendum verified it
  against the code.
* The network driver work must be justifiable as N+1-same-class before it
  starts, or it is a new mechanism without a consumer.
* Owner decisions gate: the audit's conflicts cannot be closed by
  engineering judgment alone, and a transport recommendation without a
  ruling would smuggle a decision past the owner.

## Considered Options

* **Defer the platform decision until a hardware target exists.**
  Rejected: the owner chose now. A named platform turns the deferred
  §8/§19 port rows and D11 into an in-scope qualification task with a
  concrete checklist (the task's §8.3), instead of an open scope question
  blocking every target-side row.
* **Read the epoch as a local per-unit counter.** Rejected by code
  verification: the epoch is pair-wide today — the peer adopts the owner's
  mint under the anti-stale rule, `SYNC_READY` requires the peer epoch
  agreed, and `IO_READY` requires the pair's epochs aligned
  (`compiler/ironplc-redundancy/src/epoch.rs:19,55-64`;
  `compiler/ironplc-redundancy/src/shell/mod.rs:395-397,655-683,818-835`).
* **Let the network task — or each unit independently — mint the epoch.**
  Rejected: ADR-0062's producer rule is unchanged; two minting authorities
  would reopen partition bidding wars, which the anti-stale-only epoch
  exists to prevent.
* **Decide the redundancy transport (dual fiber vs single Ethernet) in
  this ADR.** Rejected for this record: the evaluation belongs to the
  audit addendum ("Re-audit with owner decisions (2026-09-22)"), which
  recommends dual fiber; the ruling is a separate owner decision feeding
  audit §3(c).

## Decision Outcome

Chosen option: **Linux execution platform for the controller profile, one
global pair-wide epoch minted exclusively by the HA supervisor, and the
per-port network driver work as the N+1-sanctioned growth of the existing
seam.**

1. **Platform = Linux.** The controller profile targets Linux; N+1
   protocol testing happens on Linux. Qualification per the task's §8.3
   remains a release-gate obligation and is not implied by the choice:
   identical domain traces, independently proven timing bounds, no
   unqualified virtualization. No port code exists today
   ([External FSM Review](https://github.com/boogy777-lgtm/ironplc/blob/8a7f6d0d09b00436daebfd669babd39f2e0f2e13/specs/design/external-fsm-review.md): the host is
   `std`, no OS supervisor); this decision names the target and the
   checklist, not a completed port. D11 moves from out-of-scope-by-decision
   to PARTIAL (platform chosen, qualification pending).
2. **Global epoch semantics.** One epoch per redundant pair — not per
   unit, not system-wide — owned and minted exclusively by the HA
   supervisor at exactly two authority points: the scan-commit callback
   (one epoch per committed round) and the promotion barrier (epoch bump,
   barrier run in that epoch, `OwnerLease` minted under it) — never by the
   network task (ADR-0062;
   `compiler/ironplc-redundancy/src/epoch.rs:1-15`;
   `compiler/ironplc-redundancy/src/lease.rs:1-28`;
   `compiler/ironplc-redundancy/src/shell/mod.rs:655-683,809-835`). The
   peer adopts the owner's mint; the value is volatile in this slice and
   `EpochStore`'s NV backend stays a roadmap open parameter. Against the
   task's §13.3: the intent (epoch-stamped fencing at the target, replay
   rejection via sequences + CRC + anti-stale adoption, release-all on
   partial acquire) is satisfied in the simulated pair; the persistent
   reboot identity and the real-binding fencing authority land in the
   pending arbitration/quorum design (audit addendum §5).
3. **The network driver is the N+1 driver.** Per-port driver instances
   behind the existing `NicPort` seam
   (`compiler/ironplc-redundancy/src/hal.rs:17-54`) are the task's §23
   "one → N network interfaces" same-class extension — resource instances
   and routing/config change; the domain contracts of identified resources
   survive. This holds only while no new network guarantee class is
   claimed; a binding that claims one ends the same-class test by the
   task's own rule.

**Open item — redundancy transport:** dual fiber vs single Ethernet vs
the current pair-link + I/O-chain port map is NOT decided by this ADR. The
evaluation (dual fiber recommended: it alone satisfies the task's L1/L2
mandate; single Ethernet provides no two independent sync channels) lives
in the audit addendum
([DCS Firmware Task Audit](https://github.com/boogy777-lgtm/ironplc/blob/8a7f6d0d09b00436daebfd669babd39f2e0f2e13/specs/design/dcs-firmware-task-audit.md),
"Re-audit with owner decisions (2026-09-22)", §2); the owner ruling gates
the availability model and the EtherNet/IP binding.

### Consequences

* Good, because G03 gains a concrete target profile: the §8.3 checklist
  defines what qualification must measure instead of debating whether a
  target exists.
* Good, because the epoch decision changes no code: it records, with
  citations, the semantics the implementation already has — one minting
  authority, two authority points, pair-wide adoption — so the audit's
  "global epoch" has a verified meaning, not a hoped one.
* Good, because the driver work is bounded before it starts: instances
  behind a seam, same-class while no new guarantee class is claimed — the
  N+1 audit of the driver work is pre-answered and the seam it lands on
  already exists (`hal.rs`, the loopback binding).
* Neutral, because D11 and the §8/§19 rows move from "out of scope" to
  "in scope, unbuilt": platform chosen, qualification pending, no port
  mechanism exists yet.
* Bad, because the transport conflict stays open: the EtherNet/IP binding
  and the 0/0 availability model remain blocked on the owner's ruling
  recorded as the open item above.

## More Information

* [DCS Firmware Task Audit](https://github.com/boogy777-lgtm/ironplc/blob/8a7f6d0d09b00436daebfd669babd39f2e0f2e13/specs/design/dcs-firmware-task-audit.md) — the
  first audit; this ADR's addendum ("Re-audit with owner decisions
  (2026-09-22)") carries the evidence, the topology evaluation, and the
  updated conflict list.
* [ADR-0062](0062-measured-failover-timing-and-network-calibration.md) —
  the epoch producer rule and OwnerLease minting this decision leaves
  unchanged.
* [HA Redundancy FSM](../design/ha-redundancy-fsm.md) and
  [HA Redundancy Layer Architecture](../design/ha-redundancy-layer-architecture.md)
  — the pair-wide epoch, the `NicPort` seam, and the fencing capability
  descriptor the addendum verifies.
* [Roadmap](../roadmap.md), Phase 5 — the real EtherNet/IP binding and
  target-side per-port drivers remain the named open items.
* Owner task reference: [historical owner task](https://github.com/boogy777-lgtm/ironplc/blob/8a7f6d0d09b00436daebfd669babd39f2e0f2e13/docs/reference/dcs-firmware-task-v2-ru.md)
  (§2.8 topology, §8.3 execution profile, §13.3 split-brain/epoch, §23
  N+1 acceptance).

