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
the controller never sees them)"). Parity levels L2 and L3 below therefore
amend that decision locally — controller-side pending *state* — while
keeping the rest of the lifecycle (Accept → Test → Assemble, Untest,
Cancel) exactly as ADR-0064 froze it.

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

**Target L1 now.** It is the already-accepted ADR-0064 implementation
(assemble guard + V4017) plus one additive status record: no new session
machinery, no new storage model, no protocol verbs. It converts a frozen
hole into a small mechanism and names the candidate — the identity every
later level builds on. KISS: nothing here is reopened.

**Design L2 next; implement it when the TCP transport lands.** L2's core
is one mechanism promotion, and the repo doctrine demands it be made
deliberately: today "one editor" is a *convention* enforced by client
configuration (one workspace, one connection). The moment N sessions
exist, no client discipline can enforce it, so the single-writer
invariant belongs at the one authority that owns the candidate slot and
the boundary swap — `RuntimeHost` — behind a token the command layer
checks. That is the execution-permit latch's shape applied to editing,
not a new abstraction layer: the host gains a latch, the redundancy
crate pattern shows where session policy lives. Everything else in L2
(observer sessions, visibility fields) is additive surface on existing
registries.

**Defer L3.** It requires per-POU runtime candidates (today a stated
non-goal) and an ActiveManifest in the runtime; both are Phase-4+/P2
work in their own right. L3 also buys the least per unit of risk: our
editors are textual ST, where whole-POU zones are the honest granularity,
and the single-writer L2 model already covers the overwhelmingly common
case — one engineer editing, others watching. Revisit when per-POU
artifacts have shipped and the manifest swap has a real driver.

**Never build:** merge or conflict-*resolution* machinery. Rockwell
prevents conflicts with locks; the audit found no parity pressure that
locks do not answer, and a merge engine contradicts the fail-closed
posture every existing refusal (V4007–V4016, E0015) already implements.

## Open Questions

1. **Pending vs. validated.** Rockwell's pending edits are pre-accept;
   our candidate is validated at staging. Is controller-side pending a
   pre-validation store, or is "staged and visible" our honest pending
   state? The answer decides whether L1 needs new verbs or none.
2. **Identity without wire auth.** ADR-0063 defers transport
   authentication. Can L2 attribute a lock to an *engineer*, or only to
   an anonymous session token — and is the latter acceptable for the
   visibility display?
3. **Lock lifetime across disconnect.** Rockwell pending edits survive a
   workstation loss. Does the L2 lock release on transport death
   (fail-open) or persist (fail-closed), and who may force-release it?
   The connection state machine's Reconnecting path interacts with this.
4. **Zone granularity in ST.** The POU body is the natural zone. Is
   sub-POU (statement-range) addressing ever worth the wire and
   debug-map cost, or is POU the terminal granularity?
5. **stdio transport ceiling.** A spawned child is inherently
   single-client. Is L2 TCP-only, leaving the stdio session permanently
   at L1?
6. **Poll cadence.** Is the 5 s `getStatus` heartbeat sufficient for
   edit-visibility, or does L2 force the deferred push-channel decision
   both connection and HA specs currently avoid?
7. **Migration candidates under multi-user.** A schema-changing zone
   forbids untest (V4011). If two in-flight zones exist at L3 and one is
   a migration candidate, do they assemble as one manifest transaction or
   serialize — and what does untest mean for the mixed pair?
8. **Pair scope of pending visibility.** Is pending state replicated to
   SECONDARY immediately (both units show it) or only at Accept per
   ADR-0064(c), and what happens to attached observer sessions on a
   takeover mid-Test?
9. **Observer capacity.** How many concurrent observer sessions must a
   controller target support? This is the same connection-table sizing
   question the I/O firmware notes pose for Input Only connections, and
   belongs in the resource model.

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
