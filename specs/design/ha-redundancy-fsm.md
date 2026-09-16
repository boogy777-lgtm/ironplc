# Spec: HA Redundancy FSM

## Overview

This spec defines the per-controller role/state machine for a redundant
controller pair: how a controller boots, synchronizes as a reserve, is
promoted to duty, and how redundancy is lost when fencing cannot be proven.
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
  and process image this FSM gates output publication onto

## Design Goals

1. **No functional split-brain** — two controllers must never command the
   same physical output; partial I/O ownership never means duty
2. **Deterministic failover, honestly not bumpless** — takeover completes in
   a bounded, computable time; it is not a zero-transfer failover
3. **Fail-safe on ambiguity** — any situation the FSM cannot resolve drops
   to REDUNDANCY_LOST and asks a human; the FSM never auto-fails-over on
   ambiguous evidence
4. **No zombie duty** — a restarted, pair-lost, or uncleanly stopped
   controller can never resume the duty role directly
5. **Duty realtime isolation** — a healthy duty controller never misses its
   control deadline because the reserve or the redundancy link became
   unhealthy; no reserve-ACK sits in the output path

## Roles and Identity

`DUTY` and `RESERVE` are runtime roles, not configuration. The project
stores `REDUNDANCY_ENABLED` plus redundancy configuration; it never stores
"controller A is duty". One application serves the pair.

Each physical controller has a permanent identity (`PairId`, `ControllerId`,
hardware identity). Roles change after failover; identity does not. One
unit of the pair is the **designated duty**: the only unit permitted to
enter `DUTY` directly from boot, and only with fencing applied at boot.

## State Machine

```
                            ┌────────┐
                            │  BOOT  │
                            └───┬────┘
                                │
               designated unit  │  any other unit / any restart,
               + fencing ok     │  pair loss, unclean shutdown
                                ▼
         ┌──────────────┐   ┌──────────────────┐       ┌──────────────┐
         │     DUTY     │   │ RESERVE_UNSYNCED │◄──────│ RESERVE_SYNC │
         └──┬────────┬──┘   └────────┬─────────┘ sync  └──────┬───────┘
            │        │               ▲     ▲ lost / epoch      │ state
  partial   │ full   │               │     │ discontinuity     │ replication
  I/O loss  │ fencing│               │     └──────────────────┤ complete,
  (policy)  │ loss   │               │                        │ epoch aligned
            ▼        ▼               │                         ▼
    ┌───────────────┐ ┌──────────────┴┐  resync  ┌────────────────────┐
    │ DUTY_DEGRADED │ │               │◄─────────│    RESERVE_READY   │
    └──────┬────────┘ │               │          └─────────┬──────────┘
           │ full     │               │                    │ !P && !I && S
           │ fencing  │               │                    ▼
           │ loss     │               │  OWNERSHIP_   ┌──────────────────┐
           └──────────►│ REDUNDANCY_   │◄─ BARRIER ───│ RESERVE_CLAIMING │
                      │ LOST          │  any failed   └───────┬──────────┘
                      │ (terminal,    │  acquisition:         │ ordered
                      │  manual       │  release all          │ acquisition of
                      │  repair)      │                       │ Exclusive Owner
                      └───────────────┘                       │ on ALL required
                            ▲                                 │ outputs, verify,
                            │                                 │ epoch bump
                            └─────────────────────────────────┘
                                promotion verified ──► DUTY
```

| State | Description |
|-------|-------------|
| BOOT | Power-on or restart. Loads the application, validates identity and redundancy configuration, then leaves to RESERVE_UNSYNCED (or, designated unit only, to DUTY under boot fencing). |
| RESERVE_UNSYNCED | The controller executes no application logic and owns no I/O. Covers both the never-synced and the sync-lapsed conditions; a reason is recorded for diagnostics. Readiness is re-established per the readiness policy. |
| RESERVE_SYNC | Obtains the peer's epoch, then replicates application state, runtime state, and I/O configuration from the duty controller. Never takeover-ready. |
| RESERVE_READY | State replication complete; state and epoch aligned with the duty controller. The reserve executes no standard application logic; it receives state replication and observes I/O. Takeover-eligible only through RESERVE_CLAIMING. |
| RESERVE_CLAIMING | Evidence says the duty controller is gone and self is ready. Not yet duty: the controller must prove fencing by acquiring Exclusive Owner on **all** required outputs. |
| DUTY | Executes the application and commands outputs. Holds Exclusive Owner on all required outputs under the current epoch. |
| DUTY_DEGRADED | A DUTY that lost a policy-defined subset of I/O ownership. Continues executing with the degraded policy; escalation to full fencing loss ends in REDUNDANCY_LOST. |
| REDUNDANCY_LOST | Terminal. Alarm raised; both controllers drop all I/O ownership; recovery is manual repair. Entered on any failed promotion acquisition and on full fencing loss. Never auto-recovered. |

### Transition Rules

| From | To | Trigger | Action |
|------|----|---------|--------|
| BOOT | RESERVE_UNSYNCED | Boot complete (any non-designated unit; any restart, pair loss, or unclean shutdown) | Start readiness synchronization per policy |
| BOOT | DUTY | Designated unit, boot fencing succeeds | Fence outputs at boot, establish epoch, acquire Exclusive Owner on all required outputs, begin scanning |
| RESERVE_UNSYNCED | RESERVE_SYNC | Peer reachable and readiness policy permits | Fetch peer epoch; begin state replication |
| RESERVE_SYNC | RESERVE_READY | State replication complete, epochs aligned | Begin observing; report ready |
| RESERVE_READY | RESERVE_UNSYNCED | Sync lost or epoch discontinuity | Record reason; re-establish readiness per policy |
| RESERVE_READY | RESERVE_CLAIMING | `!P && !I && S` (see case table) | Start ordered fencing acquisition |
| RESERVE_CLAIMING | DUTY | Exclusive Owner acquired and verified on ALL required outputs | Bump epoch; execute application from the last committed HA resume point |
| RESERVE_CLAIMING | REDUNDANCY_LOST | Any acquisition or verification fails | Release everything acquired; raise alarm |
| DUTY | DUTY_DEGRADED | Partial I/O ownership loss within policy | Apply degraded-output policy subset |
| DUTY_DEGRADED | REDUNDANCY_LOST | Loss grows to full fencing loss | Release all; raise alarm |
| DUTY | REDUNDANCY_LOST | Full fencing loss | Release all; raise alarm |

Three rules sit above the table:

- **Zombie-duty re-entry is forbidden.** Restart, pair loss, epoch
  discontinuity, or unclean shutdown always lands in RESERVE_UNSYNCED. A
  former duty controller that returns rejoins through readiness
  synchronization and becomes a reserve; there is no automatic failback.
- **Silence makes a claimant, never a duty.** Losing the duty controller
  on both heartbeat channels only moves the reserve to RESERVE_CLAIMING.
  The I/O target is the last fence.
- **The barrier is all-or-nothing.** RESERVE_CLAIMING acquires Exclusive
  Owner connections in a fixed, configured order and verifies each. One
  failure releases everything and ends in REDUNDANCY_LOST. Partial
  ownership is never a duty.

## Detection Case Table

Signals, from the reserve's viewpoint, per quorum layers 1 and 3:

- **P** — Duty heartbeat observed: the same logical heartbeat arrives on
  port 1 (pair link) or on port 2 (traversing the I/O daisy-chain). This
  proves peer runtime and Ethernet-stack life, not mere PHY link.
- **I** — I/O evidence of a live duty controller: input data keeps
  changing on the reserve's Input Only connections in a way attributable
  to the duty controller's ownership epoch.
- **S** — Self is ready and healthy: state synchronized, scan watchdog
  sound, own I/O path traversable.

| P | I | S | Meaning | Reserve action |
|---|---|---|---------|----------------|
| 1 | — | 1 | Normal | Stay RESERVE_READY; keep state replication current |
| 1 | — | 0 | Peer alive, own sync broken | RESERVE_UNSYNCED; re-establish readiness per policy |
| 0 | 1 | 1 | Heartbeat lost but duty demonstrably owns I/O (pair-link failure) | Stay reserve; raise degraded-channel alarm; never promote |
| 0 | 0 | 1 | No heartbeat, no I/O evidence, self ready | RESERVE_CLAIMING; run the OWNERSHIP_BARRIER |
| 0 | — | 0 | Ambiguous and self not ready | RESERVE_UNSYNCED; promotion forbidden |
| 0 | 0 | 1, acquisition fails | Duty alive behind a partition | REDUNDANCY_LOST (all-or-nothing OWNERSHIP_BARRIER) |

The `!P && !I && S` case is deliberately a candidacy, not a promotion: if
the duty controller is in fact alive behind a network partition it still
owns the outputs, the acquisition fails, and redundancy is lost with an
alarm rather than risk two owners. Loss of only one heartbeat channel is
degraded transport, not redundancy loss.

## Invariants

```text
CAN_EXECUTE_OUTPUTS = (role == DUTY) && owns_all_required_io

partial ownership != duty
epoch dominates generation
no verified OWNERSHIP_BARRIER  => no promotion
restart | pair loss | epoch discontinuity | unclean shutdown
                             => RESERVE_UNSYNCED (never DUTY)
REDUNDANCY_LOST              => terminal; manual repair only
```

A controller holding an older epoch never commands physical outputs, even
if it claims a numerically larger state generation. A healthy duty
controller never waits on reserve acknowledgement inside its scan.

## Heartbeat

One logical heartbeat packet is sent on **both** channels: port 1 (pair
link, alongside state replication) and port 2 (routed through the whole
I/O daisy-chain, proving chain traversability). Fields:

```text
┌──────────────────────────────────────────────────────────┐
│ pair_id          │ redundant pair (domain) identity      │
│ role             │ sender's FSM role/state               │
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
| Duty | Input Only | Exclusive Owner |
| Reserve | Input Only (observer) | Input Only (observer) |

The reserve never uses Listen Only: a Listen Only connection depends on
an existing owner and would mask duty loss, defeating the `I` signal.
Input Only stands alone. v1 uses standard Exclusive Owner + Input Only;
some vendors offer output modules arbitrating between two owner
connections; out of v1 scope.

## Established industry practice

Redundant duty/reserve controller pairs are long-established industrial
practice, and the concepts this FSM uses are common across vendors: one
application per pair; runtime roles rather than configured ones; a passive
reserve that replicates state from the duty controller; a readiness
lifecycle (unsynced → syncing → ready) governed by a readiness policy
(off / on-link-change / continuous); and program/task synchronization
points. One known approach to ownership arbitration is a dedicated
hardware module in the chassis that manages the pair and runs state
replication in hardware; this design instead does both in software — the
OWNERSHIP_BARRIER at the I/O target plus the two-channel heartbeat.

What we honestly lose versus hardware-module arbitration: failover is
deterministic but **not bumpless**. Takeover time is

```text
T = detection + old-connection timeout + Forward_Open + validation
    + scan boundary
```

and each term is an open parameter below. A hardware module does state
replication and failover in dedicated hardware; we do both in software on
the control network. What we keep: one application per pair, runtime
roles, passive reserve, state replication, program/task synchronization
points, the unsynced → syncing → ready lifecycle, and the three-mode
readiness policy.

## Open Parameters

All values in this section are **open** (roadmap: still open); the FSM is
parameterized over them and none are decided here:

- Heartbeat period and timeout, per channel
- Detection time budget across N adapters on the daisy-chain
- Readiness policy default (off / on-link-change / continuous)
- State replication sizing: segment layout, bandwidth budget on port 1
- Epoch persistence in NV storage (surviving power loss vs. deliberate
  readiness re-establishment)
- Verification that target I/O firmware allows multiple concurrent Input
  Only originators
- Mid-chain break policy: duty continues degraded with partial I/O;
  standby fencing then fails and redundancy is lost

## Out of Scope

- Commit Certificate / quorum protocol internals, ownership lease wire
  formats (the arbitration/quorum design precedes this layer)
- Generation replication, edit replication, and distributed hot change
- External-protocol side effects and replay semantics
- DLR ring operation details; the ring is optional media, not logic
- Dual-owner arbitrating output modules (v2)
