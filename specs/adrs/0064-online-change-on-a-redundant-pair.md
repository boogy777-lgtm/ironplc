# Online Change on a Redundant Pair (Rockwell-Adapted Lifecycle)

status: accepted
date: 2026-09-20

## Context and Problem Statement

Online change works on a standalone controller: the runtime host stages,
tests, untests, assembles and cancels a candidate at a scan boundary
(ADR-0052), exposed to clients as one typed command layer (ADR-0055), with
state migration behind accept (ADR-0060, ADR-0061). The redundancy design
([HA Redundancy FSM](../design/ha-redundancy-fsm.md)) left online change on
a coupled pair as an open decision, and the engineering connection design
([Mechanism 3](../design/engineering-connection.md)) wires Build onto the
existing commands without deciding the commit policies. Two gaps remain:
the host today allows `assemble` straight from Accepted without the
candidate ever running under Test (`compiler/runtime/src/host.rs:227-245`),
and nothing defines how accept, test, untest, assemble and cancel behave
when the controller is one unit of a redundant pair. What is the session
lifecycle, what must precede assemble, and how does the pipeline run across
PRIMARY and SECONDARY without fusing online edit into the redundancy
statechart?

## Decision Drivers

* The Rockwell Studio 5000 online-edit model is the reference vocabulary:
  Pending Local edits, Accept, Test Edits, Untest, Cancel, Assemble,
  Finalize All Edits. Adapting it keeps the semantics familiar and proven.
* The host model already matches the reference internal model: `(normal,
  candidate, active_is_candidate, pending)` in
  `compiler/runtime/src/host.rs` is `(original, candidate, execution:
  ORIGINAL|CANDIDATE, lifecycle: CLEAN|STAGED)`, plus the migration plan
  the reference model omits (ADR-0054, ADR-0060, ADR-0061).
* A commit that never ran under Test is unverified code promoted to
  canonical; assemble-from-Accepted is a hole, not a shortcut.
* Edit, candidate delivery, execution selection, and commit are four
  separate operations — this separation keeps online edit and redundancy
  from fusing into one giant FSM.
* The pair must never disagree on the canonical generation: neither side
  may hold a different canonical application than its peer.
* State that has moved under a candidate cannot silently revert: untest
  after a schema-changing test is already forbidden (V4011), so a takeover
  that reverts to Original under Test is an untest in disguise.

## Considered Options

* **Fuse online change into the redundancy statechart.** Rejected: one
  giant FSM mixing edit staging with fencing and takeover. The four
  operations stay separate; the redundancy layer carries them, it does not
  absorb them.
* **Build & Trial standalone-only; online change forbidden on a pair.**
  Rejected: the pair is the primary deployment target for Phase 5; a
  forbidden online change there pushes engineers to offline downloads.
* **Keep assemble-from-Accepted.** Rejected: it promotes code that never
  executed. The reference model refuses it ("Program does not have Test
  Edits"); we tighten to match.
* **Takeover during Testing reverts to Original** (the reference's default
  `RevertToOriginal` policy). Rejected: the state has already moved under
  the candidate, so reverting to Original equals untest — which migration
  candidates forbid (V4011). The standby takes over executing the
  CANDIDATE.

## Decision Outcome

Chosen option: **the Rockwell-adapted session lifecycle, with assemble
requiring Test, a finalize-equivalent Build & Commit, and a redundant-pair
pipeline that keeps the four operations separate.**

1. **Session lifecycle model.** PENDING_LOCAL (IDE-only edits; the
   controller never sees them) → ACCEPT (candidate staged and validated;
   execution selector stays Original) → TEST (the selector switches
   Original → Candidate at a scan boundary). Exits from TEST: UNTEST
   (selector back to Original at a boundary; the candidate is kept; process
   state is NOT rolled back), CANCEL (requires exec = Original; drops the
   candidate), ASSEMBLE (requires the candidate to have executed under
   Test; the candidate becomes canonical). The internal model is
   `(original, candidate, execution, lifecycle)`; our host already matches
   it (`normal` / `candidate` / `active_is_candidate` / `pending`), plus
   the migration plan the reference model omits.
2. **Assemble requires Test.** All online commits require the candidate to
   have executed under Test; `assembleEdits` from Accepted is refused with
   a new V-code (`AssembleWithoutTest` in the runtime CSV) at
   implementation time. This **supersedes** the "initial deploy onto an
   empty device is `acceptEdits` → `assembleEdits`" detail in
   [engineering-connection.md](../design/engineering-connection.md): an
   initial deploy becomes accept → test → assemble, where the test at the
   barrier is trivial for empty state. This ADR is design only; the host
   tightening and the V-code are the follow-up implementation.
3. **Build & Commit is the finalize-equivalent.** One action executing
   Accept → Test → Assemble automatically — Test is never skipped. **Build
   & Trial** is the full manual sequence with verification checkpoints
   between steps. Guidance: use the manual sequence for safety-critical
   changes; the finalize-equivalent is for non-safety changes.
4. **Redundant-pair online change pipeline.** (a) The engineering session
   connects to the pair and verifies PRIMARY/SECONDARY, SYNC, and one
   application generation. (b) PENDING_LOCAL edits stay IDE-only. (c)
   Accept: the candidate is validated on PRIMARY, then the same candidate
   generation is delivered and confirmed on SECONDARY; both hold Original +
   Candidate with exec = Original and the pair reports STAGED/SYNC. If
   SECONDARY cannot accept, the session is NOT redundancy-ready — alarm,
   no pretending. (d) Test: PRIMARY switches the execution selector at a
   safe barrier (bumpless-compatible: no state copy); SECONDARY holds the
   same candidate and keeps replicating. (e) Takeover during Testing: the
   standby takes over executing the CANDIDATE, because the state has
   already moved under the candidate and reverting to Original equals
   untest, which migration candidates forbid (V4011). (f) Untest: barrier
   switch back; the candidate is kept. (g) Cancel: only from exec =
   Original; drops the candidate on both. (h) Assemble is one transaction
   across the pair: both sides agree the candidate is canonical (reusing
   the HA epoch); neither side may hold a different canonical generation.
   (i) Session close: both confirm identical canonical generation and
   schema version; the pair returns to synchronized redundancy.

### Consequences

* Good, because assemble can no longer promote code that never ran: every
  online commit is code that executed under Test, standalone or on a pair.
* Good, because the host model needs no restructuring — the tightening is
  one guard on `assemble`, and the pair pipeline reuses the existing
  commands and the HA epoch rather than adding a fused FSM.
* Good, because the redundancy open item in ha-redundancy-fsm.md is
  resolved with an explicit takeover policy that is consistent with the
  V4011 no-untest-after-migration rule.
* Neutral, because Build & Commit changes meaning from "accept then
  assemble" to the finalize-equivalent sequence; clients and the Build
  button wiring adopt it at implementation time.
* Bad, because an initial deploy onto an empty device gains a (trivial)
  test step, and the finalize-equivalent is unsuitable for safety-critical
  changes, which must keep the manual checkpoints.

## More Information

* ADR-0052 — the host-level online change protocol and its FSM;
  ADR-0055 — the command vocabulary (`acceptEdits` / `testEdits` /
  `untestEdits` / `assembleEdits` / `cancelEdits`) and the V-code ranges.
* ADR-0060, ADR-0061 — the migration policies and engineer-decided
  migration the reference model omits and our accept carries.
* ADR-0063 — the engineering connection transport the session rides.
* [HA Redundancy FSM](../design/ha-redundancy-fsm.md) — the epoch and
  SYNC/CONTROL machinery the pair pipeline reuses.
* [Engineering Connection](../design/engineering-connection.md),
  Mechanism 3 — Build & Commit / Build & Trial wiring.
* `compiler/runtime/src/host.rs` — `stage_with_decisions` / `test` /
  `untest` / `assemble` / `cancel`; the assemble guard to be added.
