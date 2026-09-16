# Spec: HA Redundancy FSM

## Overview

This spec defines the per-controller redundancy model for a redundant
controller pair: how a controller boots, synchronizes with its peer, claims
output ownership, and how redundancy is lost when fencing cannot be proven.

The model has two levels:

1. **Configured role (UI, static).** `DUTY` / `RESERVE` is a PLC role set
   from the engineering UI — not a runtime state. It pins boot-time rights:
   only a DUTY-configured unit may claim I/O ownership at boot; a
   RESERVE-configured unit may claim only through the takeover path.
2. **Redundancy statechart (runtime, hierarchical).** Orthogonal to the
   configured role, every unit runs one statechart with two superstates: a
   SYNC sub-chart tracking pair synchronization (`deSYNC` → `SYNCING` →
   `SYNC_READY`) and a CONTROL chart tracking output ownership (`IDLE` →
   `CLAIMING` → `ACTIVE`, plus `DUTY_DEGRADED` and `REDUNDANCY_LOST`).
   CONTROL transitions are guarded by the SYNC substate and by the
   configured role.

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
  passive reserve, state replication, readiness policies, no automatic
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

`DUTY` and `RESERVE` are configured roles, set per unit from the
engineering UI. They are static configuration, not runtime states: a
unit's role never changes at runtime, only when an engineer changes it in
the UI.

The configured role pins boot-time rights:

- only a DUTY-configured unit may claim I/O ownership at boot (with
  fencing applied at boot);
- a RESERVE-configured unit may claim I/O ownership only through the
  takeover path (`IDLE` → `CLAIMING`, guarded by `SYNC_READY`).

The project stores `REDUNDANCY_ENABLED` plus redundancy configuration,
including each unit's configured role. One application serves the pair;
the application is identical on both units regardless of role.

Each physical controller has a permanent identity (`PairId`,
`ControllerId`, hardware identity). Identity never changes. Everything
dynamic — whether the unit is synchronized, whether it commands outputs —
is expressed by the redundancy statechart below, never by the configured
role.

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
| SYNC_READY | State replication complete; state and epoch aligned with the peer. The unit executes no standard application logic; it receives state replication and observes I/O. Takeover-eligible: the CONTROL chart may enter CLAIMING per the guard table. |

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
│ IDLE  │     DUTY-configured: at boot, fencing applied at boot
└───┬───┘     RESERVE-configured: SYNC in SYNC_READY, !P && !I
    │
    ▼
┌────────────┐  barrier passed: epoch bumped,   ┌────────┐
│  CLAIMING  │─► owns ALL required I/O ────────►│ ACTIVE │
└─────┬──────┘                                  └───┬────┘
      │                                             │
      │ any acquisition or                partial   │ full
      │ verification fails:               I/O loss  │ fencing
      │ release-all                       (policy)  │ loss
      │                                             ▼    │
      │                                    ┌───────────────┐  │
      │                                    │ DUTY_DEGRADED │  │
      │                                    └───────┬───────┘  │
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
| CLAIMING | OWNERSHIP_BARRIER in progress: evidence says the peer is gone and self is ready, but the unit must prove fencing by acquiring Exclusive Owner on **all** required outputs, in a fixed configured order, verifying each. Not yet output-controlling. |
| ACTIVE | Barrier passed, epoch bumped, owns all required I/O — CAN_EXECUTE_OUTPUTS. Executes the application and commands outputs under the current epoch. |
| DUTY_DEGRADED | An ACTIVE unit that lost a policy-defined subset of I/O ownership. Continues executing with the degraded policy; escalation to full fencing loss ends in REDUNDANCY_LOST. |
| REDUNDANCY_LOST | Terminal until manual repair. Alarm raised; both units drop all I/O ownership. Entered on any failed barrier acquisition and on full fencing loss. Never auto-recovered; on manual repair/requalification the unit returns to IDLE with the SYNC chart at deSYNC. |

| From | To | Trigger | Action |
|------|----|---------|--------|
| (boot) | IDLE | Boot complete | Own no outputs; begin observing |
| IDLE | CLAIMING | Guard table: boot barrier (DUTY-configured, boot fencing succeeds) or takeover barrier (RESERVE-configured, SYNC in SYNC_READY and `!P && !I`) | Boot barrier: fence outputs at boot, establish epoch. Both: start ordered fencing acquisition |
| CLAIMING | ACTIVE | Exclusive Owner acquired and verified on ALL required outputs | Bump epoch; execute application from the last committed HA resume point |
| CLAIMING | REDUNDANCY_LOST | Any acquisition or verification fails | Release everything acquired; raise alarm |
| ACTIVE | DUTY_DEGRADED | Partial I/O ownership loss within policy | Apply degraded-output policy subset |
| DUTY_DEGRADED | REDUNDANCY_LOST | Loss grows to full fencing loss | Release all; raise alarm |
| ACTIVE | REDUNDANCY_LOST | Full fencing loss | Release all; raise alarm |
| REDUNDANCY_LOST | IDLE | Manual repair / requalification | SYNC chart to deSYNC; re-establish readiness per policy |

### Guard table — configured role × chart transitions

The only role-dependent transitions in the whole statechart are the two
ways into CLAIMING; everything else is role-independent.

| CONTROL transition | Condition | DUTY-configured | RESERVE-configured |
|--------------------|-----------|-----------------|--------------------|
| IDLE → CLAIMING (boot barrier) | Clean boot; fencing applied at boot | allowed | forbidden |
| IDLE → CLAIMING (takeover barrier) | SYNC in SYNC_READY and `!P && !I` (case table) | forbidden | allowed |
| All other CONTROL transitions | See the CONTROL transition table | role-independent | role-independent |
| All SYNC transitions | See the SYNC transition table | role-independent | role-independent |

Three rules sit above the tables:

- **Zombie re-entry is forbidden.** Restart, pair loss, epoch
  discontinuity, or unclean shutdown always lands the SYNC chart in
  deSYNC. A unit that returns rejoins through SYNCING and becomes
  takeover-eligible only at SYNC_READY; it claims ownership only per the
  guard table. There is no automatic failback.
- **Silence makes a claimant, never an owner.** Losing the peer on both
  heartbeat channels only moves a unit from IDLE to CLAIMING. The I/O
  target is the last fence.
- **The barrier is all-or-nothing.** CLAIMING acquires Exclusive Owner
  connections in a fixed, configured order and verifies each. One failure
  releases everything and ends in REDUNDANCY_LOST. Partial ownership never
  means ACTIVE.

## Detection Case Table

Signals, from the non-ACTIVE unit's viewpoint, per quorum layers 1 and 3:

- **P** — Peer heartbeat observed: the same logical heartbeat arrives on
  port 1 (pair link) or on port 2 (traversing the I/O daisy-chain). This
  proves peer runtime and Ethernet-stack life, not mere PHY link.
- **I** — I/O evidence of a live peer: input data keeps changing on the
  unit's Input Only connections in a way attributable to the peer's
  ownership epoch.
- **S** — Self is ready and healthy: the SYNC chart is in SYNC_READY,
  scan watchdog sound, own I/O path traversable.

| P | I | S | Meaning | Action |
|---|---|---|---------|--------|
| 1 | — | 1 | Normal | Stay in SYNC_READY; keep state replication current |
| 1 | — | 0 | Peer alive, own sync broken | SYNC chart to deSYNC; re-establish readiness per policy |
| 0 | 1 | 1 | Heartbeat lost but peer demonstrably owns I/O (pair-link failure) | Stay in IDLE; raise degraded-channel alarm; never claim |
| 0 | 0 | 1 | No heartbeat, no I/O evidence, self ready | Enter CLAIMING; run the OWNERSHIP_BARRIER |
| 0 | — | 0 | Ambiguous and self not ready | SYNC chart to deSYNC; claim forbidden |
| 0 | 0 | 1, acquisition fails | Peer alive behind a partition | REDUNDANCY_LOST (all-or-nothing OWNERSHIP_BARRIER) |

The `!P && !I && S` case is deliberately a candidacy, not a promotion: if
the peer is in fact alive behind a network partition it still owns the
outputs, the acquisition fails, and redundancy is lost with an alarm
rather than risk two owners. Loss of only one heartbeat channel is
degraded transport, not redundancy loss.

## Invariants

```text
CAN_EXECUTE_OUTPUTS = (CONTROL in ACTIVE) && owns_all_required_io

partial ownership != ACTIVE
epoch dominates generation
no verified OWNERSHIP_BARRIER  => no promotion to ACTIVE
restart | pair loss | epoch discontinuity | unclean shutdown
                              => deSYNC (never ACTIVE)
REDUNDANCY_LOST              => terminal; manual repair only
```

A controller holding an older epoch never commands physical outputs, even
if it claims a numerically larger state generation. A controller in
ACTIVE never waits on peer acknowledgement inside its scan.

## Heartbeat

One logical heartbeat packet is sent on **both** channels: port 1 (pair
link, alongside state replication) and port 2 (routed through the whole
I/O daisy-chain, proving chain traversability). Fields:

```text
┌──────────────────────────────────────────────────────────┐
│ pair_id          │ redundant pair (domain) identity      │
│ role             │ sender's configured role + chart state│
│ epoch            │ ownership/fencing epoch               │
│ generations      │ application + committed state gen     │
│ seqs             │ per-channel sequence numbers          │
│ io_owner_state   │ sender's view of required-IO ownership│
│ crc              │ integrity over all fields             │
└──────────────────────────────────────────────────────────┘
```

Both channels carry the same logical content so a receiver can cross-check
them; per-channel sequence numbers detect a silently repeating link.
Periods and timeouts are open parameters (below).

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

Redundant duty/reserve controller pairs are long-established industrial
practice, and the concepts this statechart uses are common across vendors:
one application per pair; a configured duty/reserve role per unit; a
passive reserve that replicates state from the active controller; a
readiness lifecycle (unsynced → syncing → ready) governed by a readiness
policy (off / on-link-change / continuous); and program/task
synchronization points. One known approach to ownership arbitration is a
dedicated hardware module in the chassis that manages the pair and runs
state replication in hardware; this design instead does both in software —
the OWNERSHIP_BARRIER at the I/O target plus the two-channel heartbeat.

What we honestly lose versus hardware-module arbitration: failover is
deterministic but **not bumpless**. Takeover time is

```text
T = detection + old-connection timeout + Forward_Open + validation
    + scan boundary
```

and each term is an open parameter below. A hardware module does state
replication and failover in dedicated hardware; we do both in software on
the control network. What we keep: one application per pair, configured
duty/reserve roles, passive reserve, state replication, program/task
synchronization points, the unsynced → syncing → ready lifecycle, and the
three-mode readiness policy.

## Open Parameters

All values in this section are **open** (roadmap: still open); the
statechart is parameterized over them and none are decided here:

- Heartbeat period and timeout, per channel
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
