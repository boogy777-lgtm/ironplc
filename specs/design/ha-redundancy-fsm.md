# Spec: HA Redundancy FSM

## Overview

This spec defines the per-controller redundancy model for a redundant
controller pair: how a controller boots, synchronizes with its peer, claims
output ownership, and how redundancy is lost when fencing cannot be proven.

The model has two levels:

1. **Configured role (UI, static).** `Primary` / `Secondary` is a PLC role
   set from the engineering UI — not a runtime state. It pins boot-time
   rights: only a Primary-configured unit may claim I/O ownership at boot;
   a Secondary-configured unit may claim only through the promotion paths.
2. **Redundancy statechart (runtime, hierarchical).** Orthogonal to the
   configured role, every unit runs one statechart with two superstates: a
   SYNC sub-chart tracking pair synchronization (`deSYNC` → `SYNCING` →
   `SYNC_READY`) and a CONTROL chart tracking output ownership (`IDLE` →
   `CLAIMING` → `ACTIVE`, plus `ACTIVE_DEGRADED` and `REDUNDANCY_LOST`).
   CONTROL transitions are guarded by the SYNC substate and by the
   configured role.

Before either level runs the application, the redundancy layer passes
admission: it discovers the neighbor, decides standalone vs. redundant,
and only then grants the application permission to start.

It follows established industry practice for redundant controller pairs and
maps that model onto the decided no-hardware-module architecture:
redundancy is a software layer above the runtime, with no add-on redundancy
module and no hardware arbiter.

This spec builds on:

- **[Roadmap, Phase 5 — Redundancy / HA](../roadmap.md)**: the binding
  decisions — EtherNet/IP, daisy-chain media, port map, and the five-layer
  quorum (two-channel observation, logical epochs, fencing at the target,
  all-or-nothing ownership barrier, readiness re-establishment)
- **HA Architecture Handoff** (internal reference document): HA coordinate
  model (`DomainId`, `Epoch`, `Generation`), the rule that epoch dominates
  generation, and the separation of state synchronization from physical
  output publication
- **Redundancy Architecture Summary** (internal reference document):
  passive secondary, state replication, readiness policies, no automatic
  failback
- **[Runtime Execution Model](runtime-execution-model.md)**: the scan cycle
  and process image this statechart gates output publication onto

## Design Goals

1. **No functional split-brain** — two controllers must never command the
   same physical output; partial I/O ownership never means `ACTIVE`
2. **Deterministic failover, honestly not bumpless** — takeover completes in
   a bounded, computable time; it is not a zero-transfer failover
3. **Fail-safe on ambiguity** — any situation the statechart cannot resolve
   drops to `REDUNDANCY_LOST` and asks a human; it never auto-fails-over on
   ambiguous evidence
4. **No zombie ACTIVE** — a restarted, pair-lost, or uncleanly stopped
   controller can never re-enter `ACTIVE` directly
5. **ACTIVE realtime isolation** — a controller in `ACTIVE` never misses
   its control deadline because the peer or the redundancy link became
   unhealthy; no peer-ACK sits in the output path

## Configured Role and Identity

`Primary` and `Secondary` are configured roles, set per unit from the
engineering UI: the engineer flashes PLC 1 as Primary with its IP address;
the UI registers PLC 2's IP address with the Secondary role. They are
static configuration, not runtime states: a unit's configured role changes
only when an engineer changes it in the UI — including the commanded role
swap (below), which exchanges the pair's roles.

The configured role pins boot-time rights:

- only a Primary-configured unit may claim I/O ownership at boot (with
  fencing applied at boot);
- a Secondary-configured unit may claim I/O ownership only through the
  promotion paths (`IDLE` → `CLAIMING`, guarded by `SYNC_READY`): the
  takeover barrier or the commanded swap.

The project stores `REDUNDANCY_ENABLED` plus redundancy configuration,
including each unit's configured role. One application serves the pair;
the application is identical on both units regardless of role.

Each physical controller has a permanent identity (`PairId`,
`ControllerId`, hardware identity). Identity never changes. Everything
dynamic — whether the unit is synchronized, whether it commands outputs —
is expressed by the redundancy statechart below, never by the configured
role.

## Admission — Decided Before the Application Starts

A PLC does not know a priori whether it runs standalone or as one unit of
a redundant pair. The redundancy layer decides **before the application
starts** and grants the application permission to start. Boot flow:

1. **Discover the neighbor** on the pair link (port 1).
2. **No pair configured** → standalone admission: the application starts
   as a single controller; the redundancy statechart does not run.
3. **Live Primary neighbor found** → this unit is the Secondary: it runs
   the sync pipeline — manual or automatic per the readiness policy — and
   only then may the application start, in **monitor mode** (no output
   control: the unit receives state replication and observes I/O). A
   Primary-configured unit that finds a live Primary takes this path too;
   configuration does not override a living owner.
4. **No live Primary neighbor, Primary-configured** → initial-owner
   admission: the boot barrier below (fence outputs at boot, establish
   epoch), then the application starts with I/O control.
5. **No live Primary neighbor, Secondary-configured** → no admission to
   control I/O: the unit stays in deSYNC and keeps discovering. A
   Secondary gains I/O control only through the two promotion cases.

Admission is also the zombie fence: a revived ex-Primary repeats this boot
lifecycle, discovers the live Primary, and joins as the Secondary — it
never re-enters as Primary.

## Redundancy Statechart

One runtime statechart per unit, orthogonal to the configured role, in two
superstates:

- **SYNC** — is this unit's state aligned with its peer?
- **CONTROL** — does this unit command outputs?

Both superstates run concurrently: at any moment a unit is in exactly one
SYNC substate and one CONTROL substate. CONTROL transitions are guarded by
the SYNC substate and by the configured role (guard table below); SYNC
transitions are role-independent.

### SYNC superstate

```text
              boot / restart / pair loss / unclean shutdown
                                │
                                ▼
┌───────────┐  paired  ┌──────────┐  state replication  ┌────────────┐
│  deSYNC   │─────────►│ SYNCING  │────────────────────►│ SYNC_READY │
└───────────┘          └──────────┘  complete + peer    └────────────┘
     ▲                     │         epoch agreed             │
     │                     │                                  │
     └─────────────────────┴──────────────────────────────────┘
          sync loss / epoch discontinuity (any substate →
          deSYNC); SYNC_READY lost ⇒ deSYNC, re-enter via SYNCING
```

| Substate | Description |
|----------|-------------|
| deSYNC | Not synchronized. Covers both the never-synced and the sync-lapsed conditions; a reason is recorded for diagnostics. The unit executes no application logic and owns no I/O. Readiness is re-established per the readiness policy. |
| SYNCING | Obtains the peer's epoch, then replicates application state, runtime state, and I/O configuration from the peer. Never takeover-eligible. |
| SYNC_READY | State replication complete; state and epoch aligned with the peer. The unit executes no standard application logic (monitor mode); it receives state replication and observes I/O. Takeover-eligible: the CONTROL chart may enter CLAIMING per the guard table. |

| From | To | Trigger | Action |
|------|----|---------|--------|
| (boot) | deSYNC | Boot complete — any unit; any restart, pair loss, or unclean shutdown | Record reason; start readiness synchronization per policy |
| deSYNC | SYNCING | Peer reachable (paired) and readiness policy permits | Fetch peer epoch; begin state replication |
| SYNCING | SYNC_READY | State replication complete, peer epoch agreed | Begin observing; report ready |
| SYNCING | deSYNC | Sync loss or epoch discontinuity | Record reason; re-establish readiness per policy |
| SYNC_READY | deSYNC | Sync loss or epoch discontinuity | Record reason; re-establish readiness per policy |

### CONTROL superstate

```text
┌───────┐   CLAIMING guard (see guard table):
│ IDLE  │     Primary-configured: at boot, fencing applied at boot
└───┬───┘     Secondary-configured: SYNC in SYNC_READY,
    │           (!P && !I) or commanded swap
    │         (commanded swap also moves ACTIVE → IDLE on the peer)
    ▼
┌────────────┐  barrier passed: epoch bumped,   ┌────────┐
│  CLAIMING  │─► owns ALL required I/O ────────►│ ACTIVE │
└─────┬──────┘                                  └───┬────┘
      │                                             │
      │ any acquisition or                partial   │ full
      │ verification fails:               I/O loss  │ fencing
      │ release-all                       (policy)  │ loss
      │                                             ▼    │
      │                                 ┌─────────────────┐  │
      │                                 │ ACTIVE_DEGRADED │  │
      │                                 └─────────┬───────┘  │
      │                                            │ loss     │
      │                                            │ grows to │
      │                                            │ full     │
      ▼                                            ▼           ▼
┌───────────────────────────────────────────────────────────────┐
│                       REDUNDANCY_LOST                         │
│  alarm; both units drop all I/O ownership; recovery is        │
│  manual repair                                                │
└───────────────────────────────┬───────────────────────────────┘
                                │ manual repair / requalification
                                ▼
                     CONTROL → IDLE, SYNC → deSYNC
```

| Substate | Description |
|----------|-------------|
| IDLE | No output control. The unit owns no outputs; it receives state replication (as the SYNC chart advances) and observes I/O over Input Only connections. |
| CLAIMING | OWNERSHIP_BARRIER in progress: the way is clear (peer gone, or ownership released in a commanded swap) and self is ready, but the unit must prove fencing by acquiring Exclusive Owner on **all** required outputs, in a fixed configured order, verifying each. Not yet output-controlling. |
| ACTIVE | Barrier passed, epoch bumped, owns all required I/O — CAN_EXECUTE_OUTPUTS. Executes the application and commands outputs under the current epoch. |
| ACTIVE_DEGRADED | An ACTIVE unit that lost a policy-defined subset of I/O ownership. Continues executing with the degraded policy; escalation to full fencing loss ends in REDUNDANCY_LOST. |
| REDUNDANCY_LOST | Terminal until manual repair. Alarm raised; both units drop all I/O ownership. Entered on any failed barrier acquisition and on full fencing loss. Never auto-recovered; on manual repair/requalification the unit returns to IDLE with the SYNC chart at deSYNC. |

| From | To | Trigger | Action |
|------|----|---------|--------|
| (boot) | IDLE | Boot complete | Own no outputs; begin observing |
| IDLE | CLAIMING | Guard table: boot barrier (Primary-configured, boot fencing succeeds), takeover barrier (Secondary-configured, SYNC in SYNC_READY and `!P && !I`), or commanded-swap barrier (Secondary-configured, pair in SYNC_READY, swap commanded) | Boot barrier: fence outputs at boot, establish epoch. All: start ordered fencing acquisition |
| CLAIMING | ACTIVE | Exclusive Owner acquired and verified on ALL required outputs | Bump epoch; execute application from the last committed HA resume point |
| CLAIMING | REDUNDANCY_LOST | Any acquisition or verification fails | Release everything acquired; raise alarm |
| ACTIVE | IDLE | Commanded role swap, pair in SYNC_READY (promotion case a) | Release all I/O ownership; stop executing; SYNC chart to deSYNC, re-sync as the new Secondary |
| ACTIVE | ACTIVE_DEGRADED | Partial I/O ownership loss within policy | Apply degraded-output policy subset |
| ACTIVE_DEGRADED | REDUNDANCY_LOST | Loss grows to full fencing loss | Release all; raise alarm |
| ACTIVE | REDUNDANCY_LOST | Full fencing loss | Release all; raise alarm |
| REDUNDANCY_LOST | IDLE | Manual repair / requalification | SYNC chart to deSYNC; re-establish readiness per policy |

### Guard table — configured role × chart transitions

The only role-dependent transitions in the whole statechart are the three
ways into CLAIMING; everything else is role-independent.

| CONTROL transition | Condition | Primary-configured | Secondary-configured |
|--------------------|-----------|--------------------|----------------------|
| IDLE → CLAIMING (boot barrier) | Clean boot; fencing applied at boot | allowed | forbidden |
| IDLE → CLAIMING (takeover barrier) | SYNC in SYNC_READY and `!P && !I` (case table) | forbidden | allowed |
| IDLE → CLAIMING (commanded-swap barrier) | Engineer commands a role swap; pair in SYNC_READY | release side (ACTIVE → IDLE) | allowed |
| All other CONTROL transitions | See the CONTROL transition table | role-independent | role-independent |
| All SYNC transitions | See the SYNC transition table | role-independent | role-independent |

### Promotion — exactly two cases

A Secondary's right to control I/O arises in exactly two cases. In all
other cases a Secondary never controls I/O. (The Primary-configured
unit's initial claim at boot — no live Primary neighbor, fencing applied
at boot — is admission, not promotion.)

**(a) Manual commanded swap — Primary ↔ Secondary while SYNC_READY.** The
engineer commands the swap from the UI; the configured roles exchange.
Ordered steps:

1. Precondition: the pair is in SYNC_READY.
2. Old Primary: release all I/O ownership, ACTIVE → IDLE, stop executing
   the application.
3. New Primary (the old Secondary, SYNC in SYNC_READY): IDLE → CLAIMING;
   run the OWNERSHIP_BARRIER — Exclusive Owner on all required outputs,
   in the fixed configured order, verifying each.
4. Barrier passed → bump epoch → ACTIVE; execute the application from
   the last committed HA resume point.
5. Old Primary: SYNC chart to deSYNC → SYNCING → SYNC_READY; it re-syncs
   as the new Secondary.
6. Rollback on barrier failure: any acquisition or verification failure
   releases everything acquired and the pair lands in REDUNDANCY_LOST
   (alarm; manual repair). The swap is never half-done — partial
   ownership never means ACTIVE.

**(b) Proven death of the Primary.** The Primary is silent on both
ping/pong channels with no I/O evidence (`!P && !I`, case table) and the
Secondary is SYNC_READY: IDLE → CLAIMING, OWNERSHIP_BARRIER passed →
ACTIVE. This is the takeover barrier of the guard table.

A revived ex-Primary repeats the boot lifecycle: admission discovers the
live Primary (its successor), so it joins the Secondary path — deSYNC →
SYNCING → SYNC_READY, monitor mode. It never re-enters as Primary; there
is no automatic failback.

Four rules sit above the tables:

- **Zombie re-entry is forbidden.** Restart, pair loss, epoch
  discontinuity, or unclean shutdown always lands the SYNC chart in
  deSYNC. A unit that returns repeats admission, rejoins through SYNCING,
  and becomes takeover-eligible only at SYNC_READY; it claims ownership
  only per the guard table. There is no automatic failback.
- **Promotion is a career ladder with exactly two rungs.** A Secondary
  gains I/O control only through the commanded swap or proven death of
  the Primary (above). In all other cases a Secondary never controls
  I/O.
- **Silence makes a claimant, never an owner.** Losing the peer on both
  ping/pong channels only moves a unit from IDLE to CLAIMING. The I/O
  target is the last fence.
- **The barrier is all-or-nothing.** CLAIMING acquires Exclusive Owner
  connections in a fixed, configured order and verifies each. One failure
  releases everything and ends in REDUNDANCY_LOST — including inside a
  commanded swap. Partial ownership never means ACTIVE.

## Detection Case Table

Signals, from the non-ACTIVE unit's viewpoint, per quorum layers 1 and 3:

- **P** — Peer ping/pong observed: the same logical ping/pong packet
  arrives on port 1 (pair link) or on port 2 (traversing the I/O
  daisy-chain). This proves peer runtime and Ethernet-stack life, not
  mere PHY link.
- **I** — I/O evidence of a live peer: input data keeps changing on the
  unit's Input Only connections in a way attributable to the peer's
  ownership epoch.
- **S** — Self is ready and healthy: the SYNC chart is in SYNC_READY,
  scan watchdog sound, own I/O path traversable.

| P | I | S | Meaning | Action |
|---|---|---|---------|--------|
| 1 | — | 1 | Normal | Stay in SYNC_READY; keep state replication current |
| 1 | — | 0 | Peer alive, own sync broken | SYNC chart to deSYNC; re-establish readiness per policy |
| 0 | 1 | 1 | Ping/pong lost but peer demonstrably owns I/O (pair-link failure) | Stay in IDLE; raise degraded-channel alarm; never claim |
| 0 | 0 | 1 | No ping/pong, no I/O evidence, self ready | Enter CLAIMING; run the OWNERSHIP_BARRIER |
| 0 | — | 0 | Ambiguous and self not ready | SYNC chart to deSYNC; claim forbidden |
| 0 | 0 | 1, acquisition fails | Peer alive behind a partition | REDUNDANCY_LOST (all-or-nothing OWNERSHIP_BARRIER) |

The `!P && !I && S` case is deliberately a candidacy, not a promotion: if
the peer is in fact alive behind a network partition it still owns the
outputs, the acquisition fails, and redundancy is lost with an alarm
rather than risk two owners. Loss of only one ping/pong channel is
degraded transport, not redundancy loss.

## Invariants

```text
CAN_EXECUTE_OUTPUTS = (CONTROL in ACTIVE) && owns_all_required_io

partial ownership != ACTIVE
epoch dominates generation
no verified OWNERSHIP_BARRIER  => no promotion to ACTIVE
Secondary I/O control          => commanded swap or proven Primary death only
restart | pair loss | epoch discontinuity | unclean shutdown
                               => deSYNC (never ACTIVE)
REDUNDANCY_LOST                => terminal; manual repair only
```

A controller holding an older epoch never commands physical outputs, even
if it claims a numerically larger state generation. A controller in
ACTIVE never waits on peer acknowledgement inside its scan.

## Ping/Pong Liveness

Pair liveness is a ping/pong exchange owned by the redundancy layer; the
I/O firmware plays no part in it. One logical ping/pong packet is sent on
**both** channels: port 1 (pair link, alongside state replication) and
port 2 (routed through the whole I/O daisy-chain, proving chain
traversability). Fields:

```text
┌──────────────────────────────────────────────────────────┐
│ pair_id          │ redundant pair (domain) identity      │
│ role             │ sender's configured role + chart state│
│ epoch            │ ownership/fencing epoch               │
│ generations      │ application + committed state gen     │
│ ping_seq         │ per-channel PING sequence, +1 per PING│
│ pong_seq         │ per-channel PONG sequence, +1 per PONG│
│ io_owner_state   │ sender's view of required-IO ownership│
│ crc              │ integrity over all fields             │
└──────────────────────────────────────────────────────────┘
```

Both channels carry the same logical content so a receiver can cross-check
them; the per-channel sequence counters detect a silently repeating link.

Silence detection is a **missing expected increment**, not a bare packet
timeout: a peer is silent when the expected +1 (PING sent, PONG received)
fails to arrive within the configured peer-failure confirmation time (see
[ADR-0062](../adrs/0062-measured-failover-timing-and-network-calibration.md)).

The owner dialogue additionally defines an operational penalty counter on
top of the exchange: a successful exchange adds +1, a missing exchange
adds +1000, so silence dominates the indicator immediately. The dialogue
assigns this counter a diagnostics role on the engineering HMI — it is
not the arbiter of takeover (the detection case table and the fencing
chain are). Whether the +1000 step also appears on the wire — for example
as a sequence jump proving traversal or processing on the second channel
— is not defined in the dialogue; **owner to confirm**.

Periods and the confirmation time are open parameters (below).

## Connection Roles (EtherNet/IP)

| Controller | Inputs | Outputs |
|------------|--------|---------|
| Unit in ACTIVE | Input Only | Exclusive Owner |
| Unit not in ACTIVE | Input Only (observer) | Input Only (observer) |

A non-ACTIVE unit never uses Listen Only: a Listen Only connection depends
on an existing owner and would mask peer loss, defeating the `I` signal.
Input Only stands alone. v1 uses standard Exclusive Owner + Input Only;
some vendors offer output modules arbitrating between two owner
connections; out of v1 scope.

## Established industry practice

Redundant primary/secondary controller pairs are long-established
industrial practice, and the concepts this statechart uses are common
across vendors: one application per pair; a configured primary/secondary
role per unit; a passive secondary that replicates state from the active
controller; a readiness lifecycle (unsynced → syncing → ready) governed
by a readiness policy (off / on-link-change / continuous); and
program/task synchronization points. One known approach to ownership
arbitration is a dedicated hardware module in the chassis that manages the
pair and runs state replication in hardware; this design instead does both
in software — the OWNERSHIP_BARRIER at the I/O target plus the two-channel
ping/pong exchange.

What we honestly lose versus hardware-module arbitration: failover is
deterministic but **not bumpless**. Takeover time is

```text
T = detection + old-connection timeout + Forward_Open + validation
    + scan boundary
```

and each term is an open parameter below. A hardware module does state
replication and failover in dedicated hardware; we do both in software on
the control network. What we keep: one application per pair, configured
primary/secondary roles, passive secondary, state replication,
program/task synchronization points, the unsynced → syncing → ready
lifecycle, and the three-mode readiness policy.

## Open Parameters

All values in this section are **open** (roadmap: still open); the
statechart is parameterized over them and none are decided here:

- Ping/pong period and peer-failure confirmation time, per channel
- Detection time budget across N adapters on the daisy-chain
- Readiness policy default (off / on-link-change / continuous)
- State replication sizing: segment layout, bandwidth budget on port 1
- Epoch persistence in NV storage (surviving power loss vs. deliberate
  readiness re-establishment)
- Verification that target I/O firmware allows multiple concurrent Input
  Only originators
- Mid-chain break policy: the ACTIVE unit continues degraded with partial
  I/O; peer fencing then fails and redundancy is lost

## Out of Scope

- Commit Certificate / quorum protocol internals, ownership lease wire
  formats (the arbitration/quorum design precedes this layer)
- Generation replication, edit replication, and distributed hot change
- External-protocol side effects and replay semantics
- DLR ring operation details; the ring is optional media, not logic
- Dual-owner arbitrating output modules (v2)
