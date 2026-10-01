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
   Per the Amendment below (2026-09-20), assemble is also the single
   commit point that persists the canonical artifact to flash on each
   unit.
   (i) Session close: both confirm identical canonical generation and
   schema version; the pair returns to synchronized redundancy.

### Consequences

* Good, because assemble can no longer promote code that never ran: every
  online commit is code that executed under Test, standalone or on a pair.
* Good, because the committed artifact survives a reboot: assemble is the
  only flash write, after all checks, so what boots is always the last
  verified commit — while an unassembled (staged/Testing) candidate stays
  RAM-only and honestly dies on reboot (see the Amendment below).
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

## Amendment — Assemble persists to flash via A/B slots (2026-09-20)

Owner-confirmed 2026-09-20; devil's-advocate design review folded in the
same date (see the review section at the end of this ADR). The decision
above left persistence unspecified: `assemble` promoted the candidate in
RAM (`compiler/runtime/src/host.rs:227-246`), and `serve`/`run` loaded
the container file once at startup (`compiler/vm-cli/src/serve.rs:31-33`),
so a reboot always returned to whatever file was on disk. This amendment
decides that **the commit persists to flash**: assemble is the single
commit point, and the committed artifact survives reboot. Commit
persistence is an accepted implementation item (Size S), not deferred
debt.

**Layout.** Three memory areas: flash slot A = active artifact (last
known good); flash slot B = the packed candidate; RAM = the hot-edit
workspace (`RuntimeHost`: normal + candidate + buffers). In vm-cli the
store is file-backed beside the served container: `<file>.slot-a`,
`<file>.slot-b`, `<file>.marker`, plus `<file>.tmp` and
`<file>.marker.tmp` during commits. The marker is a tiny `{slot, seq}`
record; `seq` increments per commit and is the age authority at boot.

1. **The wire bytes, not a re-serialization.** Slot contents are exactly
   the candidate's wire bytes from `acceptEdits` — the bytes the command
   layer already parses at `compiler/runtime/src/commands.rs:302`, passed
   through `stage_with_decisions` and kept beside the parsed `Container`.
   `assemble` moves them to a `committed_wire` latch;
   `take_committed_wire()` drains it and returns `None` when nothing
   committed since the last drain. No re-serialization: a byte-identical
   Container→bytes round-trip is not a tested property
   (`compiler/container/src/container.rs:188-265` recomputes offsets and
   hashes, and a read may discard the debug section non-fatally,
   `:233-251`), and pinning one would cost more than the retained copy.
2. **Flash is written only at Assemble** — the single commit point,
   after all checks (stage validation + mandatory Test per this ADR).
   Test/Untest/Cancel never touch flash. Ordered steps, per unit:
   1. Drain `take_committed_wire()`; empty is an internal error, never
      a silent skip.
   2. Write the bytes to `<file>.tmp`; flush and fsync the file.
   3. Verify the temp by parsing it with the same call boot uses,
      `Container::read_from`
      (`compiler/container/src/container.rs:188` — content-hash check
      plus load-time verification in one call).
   4. Delete the inactive slot if present. Required because
      `std::fs::rename` does not overwrite on Windows; this is the one
      window where a slot file is absent, and the active slot is never
      the delete target.
   5. Rename tmp into the inactive slot.
   6. Write `<file>.marker.tmp` = `{slot: inactive, seq: n+1}`; fsync;
      delete the old marker; rename marker.tmp into the marker (same
      delete-before-rename constraint; the marker tmp is the recovery
      residue for exactly this window).

   Crash windows and boot resolutions:

   | Crash point | On-disk state at boot | Resolution |
   |---|---|---|
   | during 2–3 | tmp partial or unverified; marker and active intact | boot the marker's generation; discard tmp |
   | during 4 | inactive absent; active intact | boot the active (previous) generation |
   | during 5 | inactive holds verified new bytes; marker names active | adopt the verified inactive |
   | during 6 | marker or marker.tmp names the new slot | boot the newest verifiable bytes the marker residue names |

   **Durability invariant:** bytes reach a slot only as a verified tmp
   plus rename, and the marker-named slot is never a write target — so a
   crash always leaves at least one verified generation (old active or
   new inactive); never both-invalid. The marker is a boot hint; the
   verifiable bytes are the durability point.
3. **Boot-time adoption.** `serve` boots as: (a) read `marker`, then
   `marker.tmp`, parsing `{slot, seq}` from each; (b) boot the
   highest-seq record whose slot verifies (`read_from`); heal a missing
   marker; (c) if no marker record names a verifiable slot (missing,
   corrupt, or stale marker), boot the newest verifiable bytes — the
   other slot first, then a verifying tmp (residue of a pre-rename
   crash); (d) nothing verifies: empty device — boot with no application
   and say so; the next serve seeds the store. First serve of a plain
   file with no marker seeds: the served file's bytes become the active
   slot and the marker is written, so the previous-active rollback
   anchor exists from the first commit on. `std` offers no portable
   directory fsync; the file fsync is specified, directory-entry
   durability on Windows is best-effort and noted as the residual risk
   the adoption algorithm above exists to heal.
4. **Authority split (mechanism, not convention).** The host owns WHEN —
   the commit latch is set only in `assemble` — and exposes the drained
   bytes; the shell owns HOW — vm-cli composes a concrete A/B
   `SlotStore` over `std::fs` in one module (trait extraction waits for
   the second backend, per the workspace prefactor rule). The accessor
   is a read-accessor seam in the style of the state-snapshot seam
   (`ha-redundancy-layer-architecture.md:340-344`, beside
   `data_region()` / `read_variable()`), not the scan-commit callback.
   **Wire honesty:** the `serve` session persists after the host's
   assemble and the driven boundary round, before the response line is
   rendered — a persistence failure answers the wire with a new V6xxx
   code (**V6012 SlotCommitPersist**, next free after V6011 in
   `compiler/vm-cli/resources/problem-codes.csv`) instead of an ack; the
   RAM promotion stands and the client learns the commit is live but not
   durable. A reboot honestly restores the previous committed
   generation.
5. **Boundary preserved.** An UNASSEMBLED (staged/Testing) candidate
   remains RAM-only and dies on reboot — only the committed artifact
   persists. **HA pair:** both units persist identical bytes inside the
   one-transaction commit (Decision Outcome 4(h)): PRIMARY's SlotStore
   commit runs, then SECONDARY's, then the epoch transaction closes and
   the client ack follows both. A half-persisted pair (one unit durable
   at generation n+1, the other at n) is detected at boot by the
   existing one-generation admission check (4(a)) and recovers by
   re-download of the committed artifact — crossload carries state, not
   code. Open item, stated explicitly: whether the epoch needs a
   "durable-pending" substate is left to the HA implementation; the
   standalone SlotStore must not bake in pair assumptions.

**Single owner.** `SlotStore` is composed only by `ironplcvm serve` —
the only file-backed shell with an assemble path. `run` and `benchmark`
keep loading the named file through the shared loader
(`compiler/vm-cli/src/cli.rs:24`); DAP owns no host today
(`compiler/vm-cli/src/dap_main.rs:14-21`); the MCP hot-edit session
holds a host with no file path (`compiler/mcp/src/tools/hot_edit.rs:65`)
and composes no store — its sessions are honestly RAM-only and die with
the process. No transport duplicates persist knowledge: a second
file-backed shell drains the same accessor.

Size: **S** — host: keep wire bytes plus the `take_committed_wire`
latch; vm-cli: one `SlotStore` module and boot adoption in `serve`;
codes: the V6012 CSV row and docs page per the problem-code steering;
tests: commit/adoption round-trips on a temp dir, one per crash-point
row above (a crash is simulated by stopping between steps), byte
equality between the drained wire bytes and the slot contents, and the
V6012-on-wire answer in place of the assemble ack.

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

## Amendment design review (devil's advocate) — 2026-09-20

The amendment above was attacked as architecture, not idea: is the
A/B-slot persistence a mechanism, or a patch; does it obey the
project doctrine (mechanism-not-convention, reuse-existing-calls, no
new abstraction layers) and the referenced seams. Findings below;
REFINE verdicts are folded into the Amendment text itself.

* **F1 — Where the wire bytes live. Verdict: HOLD** (chosen design
  confirmed). Alternative (a) — the shell stashes `acceptEdits` bytes
  and persists on the Assemble ack — is convention plus a wire lie:
  two transports already dispatch commands independently
  (`compiler/vm-cli/src/serve.rs:74-84`,
  `compiler/mcp/src/tools/hot_edit.rs:225-244`), so the stash and the
  "ack means commit" inference would be duplicated knowledge, and an
  ack already rendered cannot be recalled when the disk write fails.
  Alternative (b) — re-serialize the `Container` at assemble — fails
  today: no byte-identical round-trip test exists in
  `compiler/container` (the round-trip tests assert structural
  equality only, `compiler/container/src/container.rs:284-331`), and
  `read_from` discards the debug section non-fatally
  (`compiler/container/src/container.rs:233-251`), so re-serialization
  can emit a smaller, debug-stripped artifact. The host-side latch
  keeps the exact bytes the client sent; the bytes are available at
  the only production caller (`compiler/runtime/src/commands.rs:302`)
  and drop on the floor there today.
* **F2 — RAM cost. Verdict: HOLD.** The retention is one flat
  `Vec<u8>` copy of the code image, held only while a candidate is
  staged and drained at assemble / dropped at cancel. The golden
  containers measure 399–538 bytes
  (`compiler/vm-cli/resources/test/`); the format's counters are u16,
  so realistic images are tens-to-hundreds of KiB — plausible even on
  constrained targets, and cheaper than the parsed `Container`'s
  section Vecs it accompanies. Re-serialization would delete the cost
  but is refuted at F1.
* **F3 — Adoption algorithm. Verdict: REFINE** (folded into item 3).
  The original item named no boot states, no tie-breakers, and no age
  authority; "verified bytes == durable commit, marker is a hint" is
  now the stated invariant, with the marker carrying `seq` and the
  `marker.tmp` residue resolving the marker-flip window, plus the
  empty-device and first-serve seeding cases.
* **F4 — Windows rename semantics. Verdict: REFINE** (folded into
  item 2). The original noted rename-does-not-overwrite but did not
  specify the delete-before-rename window's recovery rule, nor that
  the marker flip obeys the same constraint (hence `marker.tmp` in
  the crash table). The four crash points and their boot resolutions
  are now enumerated; the never-both-invalid argument is stated as
  the durability invariant.
* **F5 — Persist-failure surface. Verdict: REFINE** (folded into
  items 2 and 4). The original named no code and no wire behavior.
  The seeded hypothesis "V6012/V6013 exists" is refuted:
  `compiler/vm-cli/resources/problem-codes.csv` ends at V6011
  (`:12`). The amendment now defines **V6012 SlotCommitPersist** in
  the V6xxx IO range (per `specs/steering/problem-code-management.md`
  the V6xxx registry lives in the vm-cli CSV), raised by the
  SlotStore commit path after the host's assemble and before the
  response line — persistence failure answers error, not ack, and the
  RAM promotion stands honestly.
* **F6 — HA pair persist ordering. Verdict: REFINE** (folded into
  item 5). "Both units persist identical bytes" had no order and no
  half-persisted story. Default now: PRIMARY persists, then
  SECONDARY, the epoch transaction closes after both, the client ack
  follows both; a half-persisted pair is caught by the existing
  one-generation admission check (Decision Outcome 4(a)) and recovers
  by re-download. Whether the epoch needs a "durable-pending"
  substate is an explicit open item left to the HA implementation.
* **F7 — Seam conformance. Verdict: REFINE** (folded into item 4).
  The original cited "the same seam style as the scan-commit
  callback" (`ha-redundancy-layer-architecture.md:332-339`), which is
  a push callback; `take_committed_wire()` is a pull/moving accessor —
  the shape of the same document's state-snapshot seam
  (`:340-344`, beside `data_region()` / `read_variable()`,
  `compiler/runtime/src/host.rs:390`, `:400`). The citation now names
  the right seam; both are established shapes, so no design change.
* **F8 — Single owner. Verdict: REFINE** (folded into the Single
  owner paragraph). The original's "`serve`/`run` boot reads the
  marker" over-scoped. Composition sites audited: `serve`
  (`compiler/vm-cli/src/serve.rs:31-43`) is the only file-backed host
  with an assemble path; `run`/`benchmark` never assemble
  (`compiler/vm-cli/src/cli.rs:46`, `:114`) and keep the shared
  direct-file loader (`:24`); DAP owns no host
  (`compiler/vm-cli/src/dap_main.rs:14-21`); the MCP hot-edit session
  holds a host with no file path (`compiler/mcp/src/tools/hot_edit.rs:65`)
  and composes no store, honestly RAM-only. One owner, no duplicated
  persist logic.
* **F9 — Rollback anchor on first commit. Verdict: REFINE** (folded
  into items 2–3). The original kept "the previous active slot" as
  rollback anchor, but before the first commit that artifact exists
  only as the originally served file — slot A would be absent and the
  anchor illusory. The boot seeding step (served file becomes the
  active slot, marker written) makes the anchor real from the first
  commit on.
* **F10 — The verify call is an existing call. Verdict: HOLD**
  (folded into item 2). "Verify with the existing load-time verifier"
  concretely means `Container::read_from`
  (`compiler/container/src/container.rs:188` — content-hash check at
  `:196`, structural load verification at `:262`), the same call boot
  already uses (`compiler/vm-cli/src/cli.rs:32`). No new verification
  code; the citation names the call rather than the module.
* **F11 — Test surface. Verdict: REFINE** (folded into Size). The
  original's "interrupt round-trips" is now one test per crash-point
  row (a crash is simulated by stopping between steps; true power-cut
  simulation is unavailable), plus byte equality between the drained
  wire bytes and the slot contents, and the V6012-on-wire answer.

Net: no finding required REPLACE. The architecture is a mechanism —
the host's assemble-only latch and drained accessor make "commit
happened" unforgeable and un-staleable at the type level, the SlotStore
is one concrete shell module over `std::fs` (trait extraction deferred
to the second backend per the prefactor rule), and every step reuses
an existing call (`stage_with_decisions`, `Container::read_from`, the
serve ack path). The rockwell-parity-audit's reboot story
(`specs/design/rockwell-parity-audit.md:399-401`, `:450-452`) rests on
this amendment; the refinements above are what make its "a reboot after
assemble boots the committed artifact from flash" claim exactly true
rather than approximately true.
