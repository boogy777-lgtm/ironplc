# Spec: HA Redundancy FSM

## Overview

This spec defines the per-controller role/state machine for a redundant
controller pair: how a controller boots, qualifies as a secondary, is promoted
to primary, and how the pair is destroyed when fencing cannot be proven. It
uses Rockwell ControlLogix redundancy (1756-RM semantics) as the operational
reference and maps that model onto the decided no-hardware-module
architecture: redundancy is a software layer above the runtime, with no
add-on redundancy module and no hardware arbiter.

This spec builds on:

- **[Roadmap, Phase 5 — Redundancy / HA](../roadmap.md)**: the binding
  decisions — EtherNet/IP, daisy-chain media, port map, and the five-layer
  quorum (two-channel observation, logical epochs, fencing at the target,
  all-or-nothing ownership barrier, requalification)
- **[HA Architecture Handoff](../../docs/reference/Rnd_Rockwell/ironplc_ha_handoff.md)**:
  HA coordinate model (`DomainId`, `Epoch`, `Generation`), the rule that
  epoch dominates generation, and the separation of state synchronization
  from physical output publication
- **[Redundancy Architecture Summary](../../docs/reference/Rnd_Rockwell/ironplc_redundancy_architecture_summary.md)**:
  passive secondary, state crossload, auto-synchronization policies, no
  automatic failback
- **[Runtime Execution Model](runtime-execution-model.md)**: the scan cycle
  and process image this FSM gates output publication onto

## Design Goals

1. **No functional split-brain** — two controllers must never command the
   same physical output; partial I/O ownership never means Primary
2. **Deterministic failover, honestly not bumpless** — takeover completes in
   a bounded, computable time; it is not a zero-transfer switchover
3. **Fail-safe on ambiguity** — any situation the FSM cannot resolve
   destroys the pair and asks a human; the FSM never auto-fails-over on
   ambiguous evidence
4. **No zombie Primary** — a restarted, pair-lost, or uncleanly stopped
   controller can never resume the Primary role directly
5. **Primary realtime isolation** — a healthy Primary never misses its
   control deadline because the Secondary or the redundancy link became
   unhealthy; no Secondary-ACK sits in the output path

## Roles and Identity

`PRIMARY` and `SECONDARY` are runtime roles, not configuration. The project
stores `REDUNDANCY_ENABLED` plus redundancy configuration; it never stores
"controller A is primary". One application serves the pair.

Each physical controller has a permanent identity (`PairId`, `ControllerId`,
hardware identity). Roles change after switchover; identity does not. One
unit of the pair is the **designated primary**: the only unit permitted to
enter `PRIMARY` directly from boot, and only with fencing applied at boot.

## State Machine

```
                           ┌────────┐
                           │  BOOT  │
                           └───┬────┘
                               │
              designated unit  │  any other unit / any restart,
              + fencing ok     │  pair loss, unclean shutdown
                               ▼
        ┌──────────────┐   ┌─────────────┐         ┌────────────┐
        │   PRIMARY    │   │ UNQUALIFIED │◄────────│ QUALIFYING │
        └──┬────────┬──┘   └──────┬──────┘ sync    └─────┬──────┘
           │        │             ▲     ▲ lost / epoch   │ crossload
 partial   │ full   │             │     │ discontinuity  │ complete,
 I/O loss  │ fencing│             │     └────────────────┤ epoch aligned
 (policy)  │ loss   │             │                      ▼
           ▼        ▼             │               ┌──────────────────┐
   ┌──────────────┐ ┌────────────┴┐  requalify    │ QUALIFIED_       │
   │  DEGRADED_   │ │             │◄──────────────│ SECONDARY        │
   │  PRIMARY     │ │             │               └───────┬──────────┘
   └──────┬───────┘ │             │                       │ !P && !I && S
          │ full    │             │                       ▼
          │ fencing │             │               ┌──────────────────┐
          │ loss    │             │  fence barrier│ PROMOTION_       │
          └────────►│ PAIR_       │◄──────────────│ CANDIDATE        │
                    │ DESTROYED   │  any failed   └───────┬──────────┘
                    │ (terminal,  │  acquisition:         │ ordered
                    │  manual     │  release all          │ acquisition of
                    │  repair)    │                       │ Exclusive Owner
                    └─────────────┘                       │ on ALL required
                          ▲                               │ outputs, verify,
                          │                               │ epoch bump
                          └───────────────────────────────┘
                              promotion verified ──► PRIMARY
```

| State | Description |
|-------|-------------|
| BOOT | Power-on or restart. Loads the application, validates identity and redundancy configuration, then leaves to UNQUALIFIED (or, designated unit only, to PRIMARY under boot fencing). |
| UNQUALIFIED | The controller executes no application logic and owns no I/O. Covers both the Rockwell "unqualified" and "disqualified" conditions; a disqualification reason is recorded for diagnostics. Requalification follows the qualification policy. |
| QUALIFYING | Obtains the peer's epoch, then crossloads application state, runtime state, and I/O configuration from the Primary. Never takeover-ready. |
| QUALIFIED_SECONDARY | Crossload complete; state and epoch aligned with the Primary. The Secondary executes no standard application logic; it receives state crossload and observes I/O. Takeover-eligible only through PROMOTION_CANDIDATE. |
| PROMOTION_CANDIDATE | Evidence says the Primary is gone and self is qualified. Not yet Primary: the controller must prove fencing by acquiring Exclusive Owner on **all** required outputs. |
| PRIMARY | Executes the application and commands outputs. Holds Exclusive Owner on all required outputs under the current epoch. |
| DEGRADED_PRIMARY | A PRIMARY that lost a policy-defined subset of I/O ownership. Continues executing with the degraded policy; escalation to full fencing loss destroys the pair. |
| PAIR_DESTROYED | Terminal. Alarm raised; both controllers drop all I/O ownership; recovery is manual repair. Entered on any failed promotion acquisition and on full fencing loss. Never auto-recovered. |

### Transition Rules

| From | To | Trigger | Action |
|------|----|---------|--------|
| BOOT | UNQUALIFIED | Boot complete (any non-designated unit; any restart, pair loss, or unclean shutdown) | Start qualification per policy |
| BOOT | PRIMARY | Designated unit, boot fencing succeeds | Fence outputs at boot, establish epoch, acquire Exclusive Owner on all required outputs, begin scanning |
| UNQUALIFIED | QUALIFYING | Peer reachable and qualification policy permits | Fetch peer epoch; begin crossload |
| QUALIFYING | QUALIFIED_SECONDARY | Crossload complete, epochs aligned | Begin observing; report qualified |
| QUALIFIED_SECONDARY | UNQUALIFIED | Sync lost or epoch discontinuity | Record disqualification reason; requalify per policy |
| QUALIFIED_SECONDARY | PROMOTION_CANDIDATE | `!P && !I && S` (see case table) | Start ordered fencing acquisition |
| PROMOTION_CANDIDATE | PRIMARY | Exclusive Owner acquired and verified on ALL required outputs | Bump epoch; execute application from the last committed HA resume point |
| PROMOTION_CANDIDATE | PAIR_DESTROYED | Any acquisition or verification fails | Release everything acquired; raise alarm |
| PRIMARY | DEGRADED_PRIMARY | Partial I/O ownership loss within policy | Apply degraded-output policy subset |
| DEGRADED_PRIMARY | PAIR_DESTROYED | Loss grows to full fencing loss | Release all; raise alarm |
| PRIMARY | PAIR_DESTROYED | Full fencing loss | Release all; raise alarm |

Three rules sit above the table:

- **Zombie-Primary re-entry is forbidden.** Restart, pair loss, epoch
  discontinuity, or unclean shutdown always lands in UNQUALIFIED. A former
  Primary that returns rejoins through qualification and becomes a
  Secondary; there is no automatic failback.
- **Silence makes a candidate, never a Primary.** Losing the Primary on
  both heartbeat channels only promotes the Secondary to
  PROMOTION_CANDIDATE. The I/O target is the last fence.
- **The barrier is all-or-nothing.** PROMOTION_CANDIDATE acquires Exclusive
  Owner connections in a fixed, configured order and verifies each. One
  failure releases everything and destroys the pair. Partial ownership is
  never a Primary.

## Detection Case Table

Signals, from the Secondary's viewpoint, per quorum layers 1 and 3:

- **P** — Primary heartbeat observed: the same logical heartbeat arrives on
  port 1 (pair link) or on port 2 (traversing the I/O daisy-chain). This
  proves peer runtime and Ethernet-stack life, not mere PHY link.
- **I** — I/O evidence of a live Primary: input data keeps changing on the
  Secondary's Input Only connections in a way attributable to the Primary's
  ownership epoch.
- **S** — Self is qualified and healthy: state synchronized, scan watchdog
  sound, own I/O path traversable.

| P | I | S | Meaning | Secondary action |
|---|---|---|---------|------------------|
| 1 | — | 1 | Normal | Stay QUALIFIED_SECONDARY; keep crossload current |
| 1 | — | 0 | Peer alive, own sync broken | UNQUALIFIED; requalify per policy |
| 0 | 1 | 1 | Heartbeat lost but Primary demonstrably owns I/O (pair-link failure) | Stay Secondary; raise degraded-channel alarm; never promote |
| 0 | 0 | 1 | No heartbeat, no I/O evidence, self qualified | PROMOTION_CANDIDATE; run the fencing barrier |
| 0 | — | 0 | Ambiguous and self not qualified | UNQUALIFIED; promotion forbidden |
| 0 | 0 | 1, acquisition fails | Primary alive behind a partition | PAIR_DESTROYED (all-or-nothing barrier) |

The `!P && !I && S` case is deliberately a candidacy, not a promotion: if
the Primary is in fact alive behind a network partition it still owns the
outputs, the acquisition fails, and the pair is destroyed with an alarm
rather than risk two owners. Loss of only one heartbeat channel is degraded
transport, not redundancy loss.

## Invariants

```text
CAN_EXECUTE_OUTPUTS = (role == PRIMARY) && owns_all_required_io

partial ownership != Primary
epoch dominates generation
no verified fencing barrier  => no promotion
restart | pair loss | epoch discontinuity | unclean shutdown
                             => UNQUALIFIED (never PRIMARY)
PAIR_DESTROYED               => terminal; manual repair only
```

A controller holding an older epoch never commands physical outputs, even
if it claims a numerically larger state generation. A healthy Primary never
waits on Secondary acknowledgement inside its scan.

## Heartbeat

One logical heartbeat packet is sent on **both** channels: port 1 (pair
link, alongside crossload) and port 2 (routed through the whole I/O
daisy-chain, proving chain traversability). Fields:

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
| Primary | Input Only | Exclusive Owner |
| Secondary | Input Only (observer) | Input Only (observer) |

The Secondary never uses Listen Only: a Listen Only connection depends on
an existing owner and would mask Primary loss, defeating the `I` signal.
Input Only stands alone. v1 uses standard Exclusive Owner + Input Only;
Rockwell-style Redundant Owner is a v2 reference (out of scope).

## Rockwell Mapping

| ControlLogix (1756-RM) | IronPLC |
|------------------------|---------|
| Unqualified / Disqualified | UNQUALIFIED (disqualification reason recorded) |
| Qualification + synchronization | QUALIFYING (peer epoch fetch + state/I/O-config crossload) |
| Qualified Secondary | QUALIFIED_SECONDARY |
| Primary | PRIMARY / DEGRADED_PRIMARY |
| Auto-Synchronization: Never / Conditional / Always | Qualification policy (default open, below) |
| RM-module hardware arbitration | Fencing barrier at the I/O target + two-channel heartbeat, in software |
| Redundant Owner (OwnerClaim / OwnerReady / OwnerActive) | v2 reference; explicitly out of v1 scope |
| Chassis-based pair, RM crossload | Two controllers, port-1 pair link crossload |

What we honestly lose versus an RM module: failover is deterministic but
**not bumpless**. Takeover time is

```text
T = detection + old-connection timeout + Forward_Open + validation
    + scan boundary
```

and each term is an open parameter below. The RM does crossload and
switchover in dedicated hardware; we do both in software on the control
network. What we keep: one application per pair, runtime roles, passive
Secondary, state crossload, program/task synchronization points, the
unqualified → qualifying → qualified lifecycle, and the three-mode
synchronization policy.

## Open Parameters

All values in this section are **open** (roadmap: still open); the FSM is
parameterized over them and none are decided here:

- Heartbeat period and timeout, per channel
- Detection time budget across N adapters on the daisy-chain
- Qualification policy default (Never / Conditional / Always)
- Crossload sizing: segment layout, bandwidth budget on port 1
- Epoch persistence in NV storage (surviving power loss vs. deliberate
  requalification)
- Verification that target I/O firmware allows multiple concurrent Input
  Only originators
- Mid-chain break policy: Primary continues degraded with partial I/O;
  standby fencing then fails and the pair is destroyed

## Out of Scope

- Commit Certificate / quorum protocol internals, ownership lease wire
  formats (the arbitration/quorum design precedes this layer)
- Generation replication, edit replication, and distributed hot change
- External-protocol side effects and replay semantics
- DLR ring operation details; the ring is optional media, not logic
- Redundant Owner connection type (v2)
