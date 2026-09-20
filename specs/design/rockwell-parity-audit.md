# Spec: Rockwell Online-Editing Parity Audit

## Overview

This spec records a parity audit of Rockwell (Studio 5000 / Logix) online
editing against the IronPLC online-change architecture: which Rockwell
capabilities exist today, which do not, what each gap costs to close, and at
which parity level each closure lands. It answers two questions directly:

1. **What must change to reach parity with the Rockwell online-editing
   model, and at what level?** The feature table below names the change per
   gap with a rough size; the parity levels L0–L3 group those changes into
   increments.
2. **Which level is the target now?** The recommendation section picks the
   target and defers the rest, with the mechanism-vs-convention reasoning
   the repo doctrine requires.

It is an analysis, not a component design: it changes no architecture and
introduces no requirements. Its inputs are the committed specs and ADRs, the
repository sources they describe, and the Rockwell reference notes in
`docs/reference/Rnd_Rockwell/`.

This spec builds on:

- **[ADR-0052](../adrs/0052-online-change-performed-by-the-runtime-host.md)**
  and
  **[ADR-0055](../adrs/0055-hot-edit-command-layer-in-ironplc-runtime.md)**:
  the host-level online-change protocol and its typed command layer
- **[ADR-0064](../adrs/0064-online-change-on-a-redundant-pair.md)**: the
  Rockwell-adapted session lifecycle — assemble requires Test, the
  finalize-equivalent, the redundant-pair pipeline
- **[Engineering Connection](engineering-connection.md)**: the session
  protocol, the connection state machine, the baseline check, and the
  single-session limits
- **[Online Editing UX](online-editing-ux.md)**: the monitoring-first editor
  and the PENDING_LOCAL gate
- **[Per-POU Code Artifacts](per-pou-code-artifacts.md)**: the granularity
  seam — build/transfer granularity today, execution granularity never
- **[HA Redundancy FSM](ha-redundancy-fsm.md)** and
  **[HA Redundancy Layer Architecture](ha-redundancy-layer-architecture.md)**:
  the pair model, the open online-change item, and the runtime seams
- **[HA Architecture Readiness](ha-architecture-readiness.md)**: the reuse
  and gap inventory this audit extends to multi-user editing
- **Rockwell reference notes** (`docs/reference/Rnd_Rockwell/`, read-only):
  `ironplc_hot_edit_redundancy_architecture.md` (the frozen baseline),
  `ironplc_hot_edit_poc_handoff.md` (generations, ActiveManifest),
  `ironplc_ha_handoff.md` (hot change as a distributed transaction)

## The Two Models

**Rockwell model.** Pending rung/program edits live *on the controller*:
an engineer starts pending edits on a rung, the edits are stored
controller-side, other online workstations see the pending edits in
progress, and a rung under edit is locked against a second editor. An
explicit gate ("Start Pending Edits") enters edit mode; Test Edits must
precede Assemble; pending edits survive a workstation disconnect.

**Our model (verified).** The edit unit is one atomic compiled container
candidate. PENDING_LOCAL is workstation-side: a shadow buffer in the IDE,
entered through a diff-triggered confirmation or the Start Pending Edits
command ([Online Editing UX](online-editing-ux.md), Editing Modes and
Safety Property); the controller never sees it. `acceptEdits` delivers one
whole compiled candidate
(`compiler/runtime/src/commands.rs:36`), which the host validates and
stages in its single candidate slot
(`compiler/runtime/src/host.rs:90`,
`compiler/runtime/src/host.rs:152`); one session per profile, one active
connection per workspace
([Engineering Connection](engineering-connection.md), invariants and Out
of Scope); `getStatus` reports generations, mode, and migration
(`compiler/runtime/src/commands.rs:136`).

**The foundational divergence is a frozen decision, not an oversight.**
The accepted baseline froze "Pending exists only in the engineering
environment" (`ironplc_hot_edit_redundancy_architecture.md`, §10.1 and
frozen decision 12) and ADR-0064 kept it ("PENDING_LOCAL (IDE-only edits;
the controller never sees them)"). The owner decision of 2026-09-20
([ADR-0065](../adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md))
kept it for parity: the L1 controller-side record is deferred as debt, the
L2 observer/lock design below is superseded by the one-session model, and
L3 is excluded. The rest of the lifecycle (Accept → Test → Assemble,
Untest, Cancel) stays exactly as ADR-0064 froze it.

## Feature Table

| Rockwell feature | Current state (evidence) | Gap | Change needed | Size |
|---|---|---|---|---|
| Controller-side pending edits | None below the staged candidate. PENDING_LOCAL is an IDE shadow buffer (online-editing-ux.md, Editing Modes); the controller first hears of an edit as a full compiled container in `acceptEdits` (`compiler/runtime/src/commands.rs:36`), staged into one candidate slot (`compiler/runtime/src/host.rs:90`) | No controller-side pending state; nothing survives a workstation loss; nothing for other users to see | A controller-side pending-edit record with identity (`EditTxnId`, content hash) beside the candidate slot; granular zones at L3 | XL (L1 shrinks it to the identity record) |
| Multi-user visibility of pending edits | Single session per profile, one active connection per workspace (engineering-connection.md, invariants and Out of Scope); `serve` is one host over one stdio stream (`compiler/vm-cli/src/serve.rs:31`); StatusPayload carries mode/generations/migration only (`compiler/runtime/src/commands.rs:136`) | A second workstation cannot connect, and there is no pending state to publish | Multi-session serving plus additive status fields (pending state, holder identity); poll on the existing heartbeat | L (blocked on the session model) |
| Rung/POU-level edit granularity + edit locks | Swap unit is the whole linked container at a scan boundary (per-pou-code-artifacts.md, Hot-Edit Interaction; "No partial swap" is a stated non-goal); per-POU artifacts are build/transfer granularity only; no locks — V4013 is a single-candidate latch (`compiler/runtime/src/host.rs:157`) | Rockwell edits and locks per rung; disjoint concurrent edits are normal; we refuse globally | Per-POU pending zones and a host-owned lock table; commit stays one atomic manifest swap (edit granularity ≠ commit granularity; the ActiveManifest model of the POC handoff) | XL |
| Explicit edit-mode gate | **Have it.** Diff-triggered confirmation plus explicit Start Pending Edits (online-editing-ux.md, Diff-triggered edit-mode entry), enforced by the mode, not by discipline (Safety Property) | Gate is client-side only: the host answers `acceptEdits` from any session at any time | L1: nothing. L2+: bind mutation commands to the lock holder at the host — mechanism, not convention | S (L1) / M (server-side, L2) |
| Test-before-Assemble | Decided in ADR-0064 (assemble requires Test; the "Program does not have Test Edits" refusal); the host still allows assemble straight from Accepted (`compiler/runtime/src/host.rs:227` — the hole ADR-0064 names); the V-code (`AssembleWithoutTest`, runtime CSV) is pending | Implementation pending; design accepted | One guard on `assemble`, one CSV row (next free V4017), conformance tests | S |
| Offline edits while connected | Monitoring-first: prospective edits accumulate in the shadow buffer while Connected (online-editing-ux.md, Editing Modes); disconnect-edit-reconnect is the fail-closed baseline check (engineering-connection.md, "Offline edits, online deploy"; E0015 StaleBaseline) | The edits are local-only — invisible and lost with the session; that is the controller-side-pending gap in disguise | None separately; subsumed by controller-side pending at L2/L3. The baseline check stays as the offline-path guard | S (existing) |
| Edit conflict resolution | Fail-closed stale-baseline refusal E0015 (engineering-connection.md, "Mismatch → coded refusal"); single-candidate V4013 (`compiler/runtime/src/host.rs:157`); no merge anywhere | Rockwell *prevents* conflicts spatially (per-rung locks) instead of refusing globally | L2: one single-writer lock (prevention). L3: disjoint per-zone locks make same-zone conflicts unreachable; no merge machinery ever | M (L2) / L (L3) |
| Session model | One open session per profile; one active connection per workspace (engineering-connection.md, invariants, Out of Scope); one host per served process (`compiler/vm-cli/src/serve.rs:31`, MCP per ADR-0056); the readiness doc records "the session model is single-host" (ha-architecture-readiness.md, Engineering backend surface) | Rockwell controllers serve many concurrent online workstations | TCP listener (Mechanism 2 framing is already designed) accepting N sessions; a session registry; read-only observer sessions beside one editor session | M |
| Status / visibility commands | `getStatus` → StatusPayload (`compiler/runtime/src/commands.rs:136`); `identity` handshake with additive-field extensibility (engineering-connection.md, "Extensibility is structural"); `haStatus` pattern (ha-engineering-ui.md); no push channel (deferred in both specs) | No pending-zone, lock, or editor-identity fields; a second user learns nothing | Additive payload fields only; visibility rides the 5 s `getStatus` heartbeat; push stays deferred | S per increment |
| HA pair implications for multi-user | ADR-0064 pair pipeline: one session, the candidate validated on PRIMARY and delivered to SECONDARY, pair STAGED/SYNC, takeover during Testing executes the CANDIDATE, assemble is one transaction over the HA epoch; the FSM's open item is resolved by ADR-0064 (ha-redundancy-fsm.md, Open Parameters) | Multi-user × two units is unaddressed: pair-consistent pending visibility, session binding to the pair rather than a unit, commands landing mid-takeover | Replicate pending state with the candidate (the EditReplicator seam of the reference baseline, Part II); a pair-scoped lock reasserted by epoch after switchover | M (after L2) |

## Parity Levels

### L0 — Today

Single workstation; workstation-side pending (shadow buffer); one
whole-container candidate; single session; poll-only status; the edit-mode
gate exists client-side; assemble-from-Accepted still open pending the
ADR-0064 tightening. No controller-side pending state, no second user, no
locks.

### L1 — Single-user controller-side pending + status visibility

> **Deferred by
> [ADR-0065](../adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md)
> (2026-09-20):** pending stays IDE-side; the controller-side pending
> record this section describes is a named debt item in the
> [roadmap](../roadmap.md). The ADR-0064 assemble tightening (assemble
> requires Test; V4017) is unaffected and remains the target
> implementation.

**Goal.** What Rockwell gives *one* engineer: pending work lives on the
controller, is named, is visible in status, and assemble requires Test.

Changes per subsystem:

- **Protocol commands.** No new verbs. `StatusPayload` gains the staged
  candidate's identity — `EditTxnId`, content hash — as additive optional
  fields, the established extensibility move (engineering-connection.md,
  "Extensibility is structural"). `acceptEdits` / `testEdits` /
  `untestEdits` / `assembleEdits` / `cancelEdits` are unchanged
  (ADR-0055).
- **Runtime storage model.** The candidate slot
  (`compiler/runtime/src/host.rs:90`) gains a small metadata record
  (`EditTxnId`, image hash). The host is already the store; this names
  what it holds. `EditTxnId` is the identity the ADR-0064 pair pipeline
  and the reference baseline's edit replication need anyway.
- **Session/lock manager.** None: one session makes the single-writer
  rule trivially true. The `EditTxnId` record is the seam the L2 lock
  later latches onto.
- **V/E codes.** V4017 `AssembleWithoutTest` in
  `compiler/runtime/resources/problem-codes.csv` (the ADR-0064 follow-up),
  surfaced through the existing `OnlineChangeError` → `CommandError`
  mapping (`compiler/runtime/src/commands.rs:229`). No E-codes.
- **UI.** The STAGED banner
  (online-editing-ux.md, Visual State Model) renders the candidate
  identity; everything else already exists.

Seams reused: the host candidate slot and its FSM; the StatusPayload;
the ADR-0055 enum + CSV codegen pattern; the serve boundary-round drive
(`compiler/vm-cli/src/serve.rs:78`). **Size: S.** This is the pending
ADR-0064 implementation plus one additive status record.

### L2 — Multi-user READ visibility

> **Superseded by
> [ADR-0065](../adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md)
> (2026-09-20):** L2 is exactly one engineering session. No observer
> sessions exist; further connection attempts are refused, and
> single-writer exclusivity comes from session exclusivity, not from the
> `EditSessionToken`/lock design below. The design below is kept as the
> record of what a multi-user L2 would have required.

**Goal.** Other online engineers see pending edits in progress; exactly
one editor exists at any moment.

Changes per subsystem:

- **Protocol commands.** Observer sessions get a read-only command subset
  (`identity`, `getStatus`, later `haStatus`). Status carries the pending
  state and the holding session's identity. Mutation commands are bound
  to the editing session.
- **Runtime storage model.** Unchanged from L1: one pending record. The
  new fact is *who holds it*, not what it is.
- **Session/lock manager.** The one real mechanism change: the
  single-writer rule moves from client-side convention ("one active
  connection per workspace") into the host — an `EditSessionToken`
  acquired by the session that stages, checked on every mutation command,
  released by assemble/cancel. The host is the only authority that can
  enforce this once N sessions exist; this is the same policy/mechanism
  split the execution-permit latch establishes
  (ha-redundancy-layer-architecture.md, "Shell, Not Runtime +1"). A
  session registry at the serving layer tracks attached sessions; the
  lock state is readable by all of them.
- **V/E codes.** Runtime V-codes for a mutation from a non-holder
  (e.g. `EditLockHeld`), same CSV codegen; an E-code only if the client
  needs a distinct observer-mode rendering.
- **UI.** Non-holder sessions render "edits in progress by \<session\>"
  as a monitoring overlay over the existing visual model; the holder's
  flow is today's flow unchanged. Visibility rides the existing 5 s
  `getStatus` heartbeat (engineering-connection.md, Timeout policy).
- **HA.** The lock is pair-scoped: held against the pair, reasserted by
  epoch after a switchover; candidate delivery to SECONDARY is ADR-0064(c)
  unchanged.

Seams reused: the Mechanism 2 TCP framing (designed, unimplemented); the
`identity` handshake's session fields; the ADR-0055 command pattern; the
permit-latch precedent for host-side enforcement. **Size: M/L** — the
multi-session transport is the bulk of it; the lock itself is small
because L1 leaves the `EditTxnId` seam.

### L3 — Full parity

> **Excluded by
> [ADR-0065](../adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md)
> (2026-09-20):** concurrent zones and editors cannot exist under the L2
> one-session model, so none of this is pursued for parity.
> [Per-POU Code Artifacts](per-pou-code-artifacts.md) remains independent
> work, not a parity path.

**Goal.** Multi-user edit zones, per-zone locks, granular edits,
conflict prevention by construction.

Changes per subsystem:

- **Runtime storage model.** Pending state per POU (the edit zone is the
  POU body), with an ActiveManifest mapping `LogicId → LogicGeneration`
  and one atomic manifest swap at the boundary — the reference POC
  handoff model (§14–§16). This deliberately reopens per-pou-code-artifacts'
  "No partial swap" non-goal: that spec keeps per-POU granularity out of
  execution; L3 admits per-POU *candidates* while keeping the *commit* a
  whole-manifest atomic transaction. Edit granularity ≠ commit
  granularity.
- **Protocol commands.** Zone-addressed verbs (start/discard pending on a
  POU) plus the unchanged lifecycle verbs operating on the manifest.
- **Session/lock manager.** A per-zone lock table in the host — one
  authority. A same-zone concurrent edit is a lock refusal at the command
  level, not a merge; disjoint zones proceed concurrently and assemble as
  one manifest transaction.
- **Conflict resolution.** Prevention only. The stale-baseline E0015
  stays the offline-path guard; there is no merge machinery at any level.
- **V/E codes.** Per-zone lock and conflict codes in the runtime CSV
  pattern.
- **UI.** Per-POU gutter markers for other users' zones over the existing
  PENDING/STAGED/TESTING rendering.
- **HA.** Pending zones replicate with the candidate set through the
  EditReplicator seam; assemble generalizes from one candidate to one
  manifest as one pair transaction over the HA epoch (ADR-0064(h)).
- **Terminology.** ST has no rungs: Rockwell's "rung lock" maps to a
  POU/routine lock. Sub-POU zones are an open question, not a level.

Seams reused: the per-POU artifact manifest and hashes
(per-pou-code-artifacts.md) as the zone identity and transfer unit; the
stable-UID tables as zone addressing; the ADR-0055 command pattern; the
L2 lock authority. **Size: XL** — it stands on per-POU artifacts shipped
*and* a runtime ActiveManifest, neither of which exists.

## Recommendation

**Decided 2026-09-20 by the owner
([ADR-0065](../adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md));
this section replaces the audit's original "target L1, design L2, defer
L3" recommendation.**

**Target: the ADR-0064 implementation only.** Pending stays in the IDE —
PENDING_LOCAL is the shadow buffer of
[Online Editing UX](online-editing-ux.md) today, and the controller learns
of an edit only at Accept. The L1 controller-side pending record and its
status identity are not built now: controller-side pending edits are
deferred as a named debt item in the [roadmap](../roadmap.md). The
assemble tightening from
[ADR-0064](../adrs/0064-online-change-on-a-redundant-pair.md) (assemble
requires Test; one guard plus V4017) is unaffected and is the parity work
that remains.

**L2 is exactly one engineering session, not multi-user read visibility.**
The controller accepts one engineering session and refuses further
connection attempts. Single-writer exclusivity is therefore a consequence
of session exclusivity, enforced by the one authority that accepts
sessions — no client discipline, no `EditSessionToken`, no observer
sessions, no lock lifetime, no visibility polling. Authentication and
engineer identity are a future option, not v1 scope. The spawned stdio
child is inherently single-client and stays as-is.

**L3 is excluded.** Concurrent zones and editors cannot exist under the
one-session model, so per-POU zones, per-zone locks, and the manifest swap
are not pursued for parity;
[Per-POU Code Artifacts](per-pou-code-artifacts.md) remains independent
work, not a parity path.

**Never build:** merge or conflict-*resolution* machinery. Rockwell
prevents conflicts with locks; the audit found no parity pressure that
locks do not answer, and a merge engine contradicts the fail-closed
posture every existing refusal (V4007–V4016, E0015) already implements.

## Open Questions

1. **Pending vs. validated.** Rockwell's pending edits are pre-accept;
   our candidate is validated at staging. Is controller-side pending a
   pre-validation store, or is "staged and visible" our honest pending
   state? The answer decides whether L1 needs new verbs or none.
   **Resolved by
   [ADR-0065](../adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md):**
   neither is built now — pending stays IDE-side (PENDING_LOCAL/shadow
   buffer) and the controller knows nothing until Accept; the
   controller-side record is deferred as roadmap debt.
2. **Identity without wire auth.** ADR-0063 defers transport
   authentication. Can L2 attribute a lock to an *engineer*, or only to
   an anonymous session token — and is the latter acceptable for the
   visibility display?
   **Resolved by
   [ADR-0065](../adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md):**
   no engineer identity in v1; the single session owns every edit, so
   there is no lock to attribute. Authentication stays a future option.
3. **Lock lifetime across disconnect.** Rockwell pending edits survive a
   workstation loss. Does the L2 lock release on transport death
   (fail-open) or persist (fail-closed), and who may force-release it?
   The connection state machine's Reconnecting path interacts with this.
   **Resolved by
   [ADR-0065](../adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md):**
   no lock exists; exclusivity is session exclusivity, which ends with the
   session. There is nothing to release or force-release.
4. **Zone granularity in ST.** The POU body is the natural zone. Is
   sub-POU (statement-range) addressing ever worth the wire and
   debug-map cost, or is POU the terminal granularity?
   **Resolved by
   [ADR-0065](../adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md):**
   moot under L2 — L3 is excluded, so no zones exist; per-POU artifacts
   remain independent work ([Per-POU Code Artifacts](per-pou-code-artifacts.md)).
5. **stdio transport ceiling.** A spawned child is inherently
   single-client. Is L2 TCP-only, leaving the stdio session permanently
   at L1?
   **Resolved by
   [ADR-0065](../adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md):**
   the stdio child is inherently single-client and stays as-is; TCP
   enforces the same one-session rule by refusing further connections.
6. **Poll cadence.** Is the 5 s `getStatus` heartbeat sufficient for
   edit-visibility, or does L2 force the deferred push-channel decision
   both connection and HA specs currently avoid?
   **Resolved by
   [ADR-0065](../adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md):**
   no observers means nothing to poll for; the heartbeat stays an
   idle-liveness check and the push channel stays deferred.
7. **Migration candidates under multi-user.** A schema-changing zone
   forbids untest (V4011). If two in-flight zones exist at L3 and one is
   a migration candidate, do they assemble as one manifest transaction or
   serialize — and what does untest mean for the mixed pair?
   **Resolved by
   [ADR-0065](../adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md):**
   moot under L2 — L3 is excluded and only the one session's candidate
   exists; the V4011 untest refusal is unchanged.
8. **Pair scope of pending visibility.** Is pending state replicated to
   SECONDARY immediately (both units show it) or only at Accept per
   ADR-0064(c), and what happens to attached observer sessions on a
   takeover mid-Test?
   **Resolved by
   [ADR-0065](../adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md):**
   pending reaches the pair only at Accept (ADR-0064(c) unchanged); no
   observer sessions exist to handle on a takeover.
9. **Observer capacity.** How many concurrent observer sessions must a
   controller target support? This is the same connection-table sizing
   question the I/O firmware notes pose for Input Only connections, and
   belongs in the resource model.
   **Resolved by
   [ADR-0065](../adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md):**
   no observer sessions by decision; there is no capacity to size.

## Out of Scope

- Implementation of any level; this document is analysis and level
  definition only.
- Wire formats for locks, zones, or the session registry; L2/L3 designs
  own those when scheduled.
- Push-channel design and transport authentication (deferred with the
  citing specs).
- Rung/ladder editing concepts; the IEC ST surface has no rungs, and no
  parity level introduces them.
- Any change to production code, the container format, or the host FSM;
  the levels name where such changes would land, nothing more.

## Debt Closure — Controller-Side Pending Edits

> Closure definition for the roadmap debt deferred by
> [ADR-0065](../adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md)
> decision 1 (2026-09-20). The debt entry says "revisit when a driver
> exists"; this section defines the minimal closure so the revisit is a
> scheduling decision, not a redesign. It closes the debt under the
> ADR-0065 constraints (one session, no locks, no merge), not the parity
> gap: the L1 sketch's `EditTxnId` is deliberately not built — the
> existing candidate generation plus `acceptedAt` already identifies the
> record, and a second transaction identity would be a second naming
> authority. Storage is **RAM-only** by owner decision (2026-09-20): the
> record is a plain `Option<PendingEditRecord>` field on `RuntimeHost` —
> no store port, no file backend — and the reboot-diagnostics case is
> consciously dropped (motivation case 1 below).

### 1. Motivation

Under the one-session model the value is named status and pair staging,
not collaboration — no second consumer exists. Three concrete cases, the
first dropped by the RAM-only storage decision:

1. **Reboot during an in-flight edit. (Consciously dropped.)** A staged
   (Accepted or Testing) candidate lives in host memory
   (`compiler/runtime/src/host.rs:90`), and a device reboot silently
   discards it; the stale-baseline check (E0015) only helps a client
   that captured a baseline — it cannot tell a reconnecting tool whether
   the candidate was assembled, cancelled, or lost in the reboot, and a
   migration candidate under Test leaves no trace at all. The RAM-only
   storage decision drops this case: the record does not survive reboot,
   so after one the device honestly reports *no* pending record, and the
   E0015 limitation — it cannot distinguish "assembled then rebooted"
   from "lost then rebooted" — is accepted. Cases 2 and 3 remain the
   motivation.
2. **Device panel and tooling read a named pending state.** `getStatus`
   carries generation counters and the migration flag only
   (`compiler/runtime/src/commands.rs:136-152`). The record lets the
   connected device panel, scripts, and the MCP tools render "edit *X*
   staged at *T* against baseline *H*" beside the STAGED banner
   (online-editing-ux.md:100) instead of a bare candidate generation.
3. **HA pair staging.** Per ADR-0064(c) the pair stages the candidate at
   Accept; the record replicates with that delivery, so both units name
   the same in-flight edit and a takeover mid-Test reports it unchanged
   on the new primary (ADR-0064, decisions (c) and (e)).

Honest assessment: case 3 is real; case 2 is convenience; case 1 was
real but is consciously dropped by the RAM-only decision. Nothing
here serves a second engineer — by decision there is none.

### 2. Scope definition

"Controller-side pending" minimally means: **the device holds and reports
a pending-edit metadata record, in RAM for the edit's lifetime.**

- **Record contents:** `name` (client-supplied label), `origin` (optional
  unauthenticated client label — not engineer identity, ADR-0065
  decision 2), `acceptedAt` (device timestamp at Accept), and `baseline`
  (the normal artifact's identity at Accept: its generation plus
  `content_hash`, `compiler/container/src/header.rs:45`). The candidate
  generation is already reported (`compiler/runtime/src/commands.rs:144`)
  and links the record to the slot.
- **Lifecycle:** written at Accept, deleted at Assemble and Cancel — the
  normal exits (`compiler/runtime/src/host.rs:235-244`, `:256-258`). RAM
  only: a reboot discards the record with the candidate, and the device
  honestly reports no pending record until the next Accept.
- **Accept unchanged:** `acceptEdits` still carries the whole compiled
  container as the only byte transfer
  (`compiler/runtime/src/commands.rs:36`); the record rides as small
  additive JSON fields on the same command. There is no pre-validation
  store: the controller still learns of an edit only at Accept
  (ADR-0065 decision 1 stands).

**Non-scope (explicit):**

- **No edit storage.** The candidate container is not persisted, and
  neither is the record: a plain `Option<PendingEditRecord>` field on
  `RuntimeHost`, no store port, no file backend, no composition choice.
  The workstation-side PENDING_LOCAL shadow buffer remains the only
  pre-Accept store (online-editing-ux.md, Editing Modes).
- **No visibility to other sessions.** One engineering session; reading
  the record requires the session, which only one client can hold
  (ADR-0065 decision 2). No observer fields, no push channel, no polling
  additions — the 5 s `getStatus` heartbeat stays the only carrier.
- **No pre-validation.** The record is written by the existing,
  already-validated stage path (`compiler/runtime/src/host.rs:152-185`);
  it adds no validation surface.
- **No locks or tokens.** V4013 remains the single-candidate latch
  (`compiler/runtime/src/host.rs:157`); the record is metadata, not a
  lock.
- **No merge or conflict resolution.** The ADR-0065 decision 4 guard
  stands unchanged.

### 3. Change map per subsystem

| # | Subsystem | Change (anchors) | Size |
|---|-----------|------------------|------|
| 1 | Protocol / status surface | `StatusPayload` gains an optional `pendingEdit` block — the established additive move ("Extensibility is structural", engineering-connection.md:222-228). `identity` reuses the StatusPayload shape (engineering-connection.md:164-217), so the handshake and the device panel inherit the block with no new command. `Command::AcceptEdits` gains optional `edit` name/origin fields (`compiler/runtime/src/commands.rs:36-48`). No new verbs. | S |
| 2 | Runtime storage | `PendingEditRecord` as a plain `Option<PendingEditRecord>` field beside the candidate slot in `RuntimeHost` (`compiler/runtime/src/host.rs:88-100`) — no store port, no backends, no composition choice. Written in `stage_with_decisions` (`:152-185`) and cleared exactly where the candidate dies: `assemble` (`:235-244`), `cancel` (`:256-258`). The host stays the single authority — mechanism, not a client-side convention. `HostStatus`/`status()` expose it (`compiler/runtime/src/host.rs:69-85`, `:263-281`). | S |
| 3 | VM-CLI session | Unchanged. `serve_session` keeps one response line per command (REQ-VC-vm-cli-019, `compiler/vm-cli/src/serve.rs:67-99`, `:286-326`); the RAM-only record needs nothing at startup. | — |
| 4 | V-codes | None. The record adds no refusal path; V4013 stays the only guard and the pending V4017 (`AssembleWithoutTest`, `compiler/runtime/resources/problem-codes.csv:2-11`) is the unrelated ADR-0064 implementation taking the next free row. | — |
| 5 | Client rendering | `HotEditStatus`/`formatStatusDetail` render the record in the STAGED banner (`integrations/vscode/src/hotEditSession.ts:42`, `:227-249`). No new E-codes; E0015 StaleBaseline is untouched. | S |
| 6 | HA pair | The record rides the ADR-0064(c) candidate delivery at Accept — part of the staged-generation payload, no new pair machinery (ADR-0064, decision (c)). Takeover mid-Test reports the same record on the new primary (decision (e)). | S |
| 7 | Tests | Host unit tests (record lifecycle), command round-trip (pattern at `compiler/runtime/src/commands.rs:415-437`), a scripted serve e2e (pattern at `compiler/vm-cli/src/serve.rs:286-326`), and client unit tests (`integrations/vscode/src/test/unit/hotEditSession.test.ts:46`). | S |

**Dependency check — per-POU code artifacts are not required.** The record
attaches to the one whole-container candidate slot
(`compiler/runtime/src/host.rs:90`; the transfer unit is one container,
`compiler/runtime/src/commands.rs:36`) and carries no per-POU content.
Per-POU artifacts remain independent build/transfer-granularity work with
the swap unit unchanged (per-pou-code-artifacts.md:187-219; ADR-0065
decision 3).

### 4. Sequencing

The closure lands in **Phase 6 (Engineering connection)**, after
Mechanism 2 (the `identity` handshake and session surface) and with the
Build wiring, because every vehicle it needs — the StatusPayload block,
the handshake, the device panel — is a Phase 6 deliverable; building the
record earlier would front-load a status block with no consumer.

- **No dependency on the ADR-0064 implementation.** The assemble
  tightening (one guard plus V4017) and this record touch disjoint code;
  only the CSV's next-free row is shared.
- **No dependency on Phase 5.** Standalone, the record is complete
  without the pair. If the pair pipeline implements first, the record is
  added to the ADR-0064(c) payload in the same change — an S delta, not a
  retrofit.
- **Ordering within Phase 6:** runtime record + status block →
  client rendering; the HA delta ships with the pair
  online-change implementation whenever it lands.

### 5. Decision points still open

1. **Reboot semantics.** Resolved by the RAM-only storage decision
   (owner, 2026-09-20): neither tombstone nor candidate persistence.
   Candidate persistence is edit storage — the declared non-scope — and
   a tombstone is persistence too; with the record in RAM only, the
   reboot-diagnostics case is dropped and the E0015 limitation accepted
   (motivation case 1).
2. **The `origin` field.** Include as optional unauthenticated advisory
   text (recommended; empty by default, documented as not identity) vs
   omit it. Inclusion costs one optional field and serves reconnect
   matching; omission is the stricter KISS reading. Decide when the
   client fields are wired.
3. **Tombstone clearing.** Moot under RAM-only storage: no tombstone
   exists to clear. The record is written at Accept and cleared at
   Assemble/Cancel; overwrite-on-next-Accept needs no command and no
   V-code.
4. **Store placement.** Resolved — RAM-only field, no store port (owner
   decision, 2026-09-20): simplicity over reboot diagnostics. The
   alternatives are moot either way: a host-owned port with a vm-cli
   file backend makes persistence a composition choice, and
   shell-managed file I/O around `execute` duplicates the record's
   lifecycle at a second owner — convention, not mechanism.

### 6. Effort and recommendation

| Step | Size |
|------|------|
| Protocol / status surface (1) | S |
| Runtime storage (2) | S |
| Client rendering (5) | S |
| HA delta (6) | S |
| Tests (7) | S |
| **Total** | **S** — one focused standalone PR plus the client rendering; the HA delta rides the pair work |

**Recommendation: close later — schedule in Phase 6** with the
engineering connection implementation, and fold the record into the pair
online-change change if that lands first.

- **Not now:** the value is thin under one session (a named status
  line plus the HA staging name), and all of its vehicles are Phase 6
  work; closing now builds the record before the surface that reads it.
- **Not never:** the device panel, scripts, and the MCP tools still gain
  the named pending state (case 2), and the HA pipeline needs the record
  to stage a *named* candidate on the pair (case 3); deferring past the
  pair implementation would retrofit it there. The reboot honesty gap
  stays open by the RAM-only decision and is accepted as its limitation.
