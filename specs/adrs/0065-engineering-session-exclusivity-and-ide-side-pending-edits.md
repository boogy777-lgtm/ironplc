# Engineering Session Exclusivity and IDE-Side Pending Edits

status: accepted
date: 2026-09-20

## Context and Problem Statement

The Rockwell online-editing parity audit
([Rockwell Online-Editing Parity Audit](../design/rockwell-parity-audit.md))
defined parity levels L0–L3 and left nine Open Questions for the owner. It
recommended targeting L1 (a controller-side pending record plus a status
identity), designing L2 as multi-user read visibility (observer sessions
beside one editor, with a host-side `EditSessionToken` lock), and deferring
L3. The owner has now decided the parity direction: pending stays in the
IDE, the controller serves exactly one engineering session, and L3 is
excluded. What session model does the parity path require, what happens to
the pending state, and which parts of the audit's L2/L3 designs are
deleted rather than built?

## Decision Drivers

* Mechanism, not convention: single-writer exclusivity must be enforced by
  the one authority that can enforce it, never by client discipline. The
  client-side "one active connection per workspace" convention cannot hold
  once a second client can connect.
* KISS: the audit's L2 surface — lock token, lock lifetime, observer
  registry, pending-visibility fields, engineer attribution — exists only
  to make a second session safe. Excluding the second session deletes that
  surface instead of building it.
* The honest pending state already exists: PENDING_LOCAL is a
  workstation-side shadow buffer and the controller learns of an edit only
  at Accept (ADR-0064;
  [Online Editing UX](../design/online-editing-ux.md)).
* ADR-0063 defers transport authentication; with one session owning every
  edit there is nothing to attribute, so engineer identity buys no parity.
* Deferral must be visible: a deliberately postponed capability is a named
  debt item in the roadmap, not a silence.

## Considered Options

* **Keep the audit's L2 (observer sessions plus a host-side
  `EditSessionToken` lock).** Rejected: the token, its lifetime across
  disconnect, the observer registry, visibility polling, and lock
  attribution all exist to make N sessions safe. One session is already
  the state of the system, so refusing the second session is strictly
  smaller than making N safe.
* **Engineer identity and transport authentication in v1.** Deferred:
  authentication remains a future option; a single session that owns every
  edit needs no attribution, and ADR-0063 already defers the transport
  decision.
* **Controller-side pending state now (the audit's L1 record).**
  Deferred: the controller knows nothing until Accept and that is the
  honest pending state; the record is not built now and is recorded as
  debt in the roadmap.
* **L3 zones, per-zone locks, and the per-POU manifest swap.** Rejected:
  concurrent zones require concurrent editors, which the one-session model
  makes unreachable. Per-POU artifacts remain independent work
  ([Per-POU Code Artifacts](../design/per-pou-code-artifacts.md)), not a
  parity path.

## Decision Outcome

Chosen option: **one engineering session with IDE-side pending edits, and
no observer or multi-zone editing surface.**

1. **L1 pending stays in the IDE.** PENDING_LOCAL remains the
   workstation-side shadow buffer (online-editing-ux.md, Editing Modes);
   the controller first hears of an edit at `acceptEdits`. The audit's
   controller-side pending record is deliberately deferred, not dropped —
   the "staged and visible" reading of Open Question 1 is not built now
   either — and the [roadmap](../roadmap.md) carries the deferral as a
   named debt item. There is no pre-validation store and no
   controller-side pending state now.
2. **L2 is exactly one engineering session.** The controller accepts one
   engineering session; further connection attempts are refused. The
   single-writer rule is therefore a mechanism at the one authority that
   accepts sessions: there is no second session to hold a lock or to
   violate exclusivity, so no `EditSessionToken`, no lock lifetime, no
   observer state, and no pending-visibility fields exist. No engineer
   identity in v1; authentication is a future option. The spawned stdio
   child is inherently single-client and stays as-is.
3. **L3 is excluded.** Concurrent zones and editors cannot exist under the
   one-session model, so per-POU zones, per-zone locks, and the
   ActiveManifest swap are not pursued for parity. The audit's Open
   Questions 4 and 7 close as moot under L2.
4. **Merge and conflict resolution are never built.** Locks (or, here,
   session exclusivity) prevent conflicts; the fail-closed refusals
   (V4007–V4016, E0015) stay the guard. This keeps the audit's rule
   unchanged.

### Consequences

* Good, because exclusivity lands on an existing invariant instead of a
  new abstraction: `one open session per profile`
  ([Engineering Connection](../design/engineering-connection.md),
  Invariants) and one host per served process
  (`compiler/vm-cli/src/serve.rs:31`) already say it; the parity path only
  refuses instead of accepting the second session.
* Good, because the deleted surface is large: no lock V-codes, no lock
  lifetime policy, no observer capacity sizing, no pending-visibility
  polling, and no identity dependency on a future
  transport-authentication decision.
* Good, because the ADR-0064 implementation is untouched: assemble
  requires Test (V4017) remains the pending work, and the pair pipeline
  sees pending only at Accept as ADR-0064(c) froze it.
* Neutral, because parity is knowingly partial: Rockwell serves many
  online workstations, we serve one. A future authorization or observer
  design would be a new ADR, not an amendment here.
* Bad, because nothing survives a workstation loss: pending edits live and
  die with the IDE session; reconnect reconciliation still relies on the
  fail-closed baseline check (E0015 StaleBaseline, ADR-0063).
* Bad, because a second engineer cannot even watch the first; the audit's
  observer UI is not planned.

## More Information

* [Rockwell Online-Editing Parity Audit](../design/rockwell-parity-audit.md)
  — the levels and Open Questions this ADR resolves; its L2 design is
  superseded by decision 2 before implementation.
* [ADR-0064](0064-online-change-on-a-redundant-pair.md) — the
  session lifecycle this ADR keeps (PENDING_LOCAL IDE-only, Accept → Test
  → Assemble); assemble-without-test (V4017) remains the pending
  implementation.
* [ADR-0063](0063-engineering-connection-transport.md) — the
  transport whose authentication stays a future option.
* [Engineering Connection](../design/engineering-connection.md) — the
  one-session invariant and the connection state machine.
* [Online Editing UX](../design/online-editing-ux.md) — the PENDING_LOCAL
  gate that is now the terminal pending state for parity.
* [Roadmap](../roadmap.md) — the controller-side pending-edits debt entry.
