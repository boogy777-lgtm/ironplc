# Online change core: assemble guard, pending record, A/B slot persistence (ADR-0064)

Date: 2026-09-20
Status: draft
Branch: feature/adr0064-online-change-core

## Goal

Land the ADR-0064 core in one PR: (1) `assemble` requires Test — refused from
Accepted with new V4017 `AssembleWithoutTest`; (2) the RAM-only
`PendingEditRecord` on `RuntimeHost`, written at Accept, cleared at
Assemble/Cancel, exposed as an additive `pendingEdit` status block;
(3) assemble persists the committed artifact to flash via an A/B `SlotStore`
in vm-cli `serve` only (the amendment, including the crash-table adoption
algorithm and V6012 `SlotCommitPersist` on the wire); (4) tests at every
layer.

## Architecture

- **Host owns WHEN, shell owns HOW** (ADR-0064 amendment item 4). The host
  keeps the candidate's exact `acceptEdits` wire bytes beside the parsed
  `Container`; `assemble` moves them to a `committed_wire` latch;
  `take_committed_wire()` drains it (state-snapshot seam shape, beside
  `data_region()` / `read_variable()`). No re-serialization.
- **Assemble guard** (ADR-0064 decision 2): one check in `RuntimeHost::assemble`
  — `!active_is_candidate` → `OnlineChangeError::AssembleWithoutTest` → V4017
  via the runtime CSV + build.rs codegen. Fix all callers/tests; the rule is
  not weakened. Assemble stays allowed from Testing (incl. migration
  candidates under Test).
- **Pending record** (rockwell-parity-audit debt closure, RAM-only by owner
  decision): plain `Option<PendingEditRecord>` field — `name`, `origin`,
  `acceptedAt`, `baseline {normalGeneration, contentHash}` — written in
  `stage_with_decisions`, cleared exactly where the candidate dies
  (`assemble`, `cancel`). `AcceptEdits` gains optional `edit: {name, origin}`
  (serde optional); `StatusPayload` gains optional `pendingEdit`, skipped
  when `None`. No persistence, no tombstone, no new refusal path.
- **SlotStore** (vm-cli, `std::fs` only, no new deps): `<file>.slot-a`,
  `<file>.slot-b`, `<file>.marker` (`{slot, seq}`), `<file>.tmp`,
  `<file>.marker.tmp`. Commit = ordered steps: tmp write + fsync → verify via
  the existing `Container::read_from` (same call boot uses) → write
  `marker.tmp` + fsync **before** the slot swap so the crash table's
  during-4/5 rows resolve by seq (marker residue always names in-flight
  bytes; the ADR's literal sub-order leaves during-5 indistinguishable from
  steady state) → delete inactive slot (Windows rename never overwrites;
  the marker-named slot is never the delete target) → rename tmp into the
  inactive slot → delete old marker → rename `marker.tmp` into marker.
  Boot = read marker then `marker.tmp`; adopt the highest-seq record whose
  slot verifies; heal a missing marker; else newest verifiable bytes (other
  slot, then verifying tmp); else, with no store at all, seed from the given
  file (slot-a + marker, the rollback anchor from the first commit on);
  nothing verifies with residue present → honest error. `seq` is the age
  authority; verified bytes are the durability point; never both-invalid.
- **Wire honesty**: serve persists after the host's assemble ack and the
  driven boundary round, before the response line renders. Persist failure
  answers `V6012 SlotCommitPersist` instead of the ack; the RAM promotion
  stands. `run`/`benchmark`/DAP/MCP unchanged (no persist logic, no store).

## File map

- `compiler/runtime/src/host.rs` — guard; `AcceptedEdit` param on
  `stage_with_decisions`; `candidate_wire` / `committed_wire` /
  `pending_edit` fields; `take_committed_wire()`; `status()` exposes record.
- `compiler/runtime/src/error.rs` — `AssembleWithoutTest` variant + Display.
- `compiler/runtime/src/commands.rs` — `edit` field on `AcceptEdits`,
  `EditSpec`, `pendingEdit` on `StatusPayload`, V4017 mapping.
- `compiler/runtime/resources/problem-codes.csv` — V4017 row.
- `compiler/runtime/src/lib.rs` — export the new public types.
- `compiler/mcp/src/tools/hot_edit.rs` — constructor/caller fixes only.
- `compiler/vm-cli/src/slot_store.rs` — new `SlotStore` module.
- `compiler/vm-cli/src/serve.rs` — compose store in `serve` only;
  `serve_session` gains `Option<&mut SlotStore>`; assemble persist + V6012.
- `compiler/vm-cli/resources/problem-codes.csv` — V6012 row.
- `docs/reference/runtime/problems/V4017.rst`, `V6012.rst` — pages (summary
  auto-generated from the CSVs by the Sphinx extension).
- Tests: `compiler/runtime/tests/{acceptance,commands_acceptance}.rs`,
  `compiler/vm-cli/src/serve.rs` (unit), `compiler/vm-cli/src/slot_store.rs`
  (unit, one per crash row), `compiler/vm-cli/tests/cli.rs` (scripted e2e:
  commit files, simulated reboot loads the committed artifact, crash cases,
  V6012 on wire).

## Tasks

### T1 — runtime guard + record

Host guard, `AcceptedEdit`, record lifecycle, wire bytes latch + accessor,
V4017 CSV row, error/commands mapping, `edit`/`pendingEdit` wire fields,
lib exports. Update runtime + mcp callers and tests for the new rule and
shape (accept → test → assemble everywhere).

### T2 — vm-cli SlotStore + serve

New module per the amendment (boot adoption + seeding, ordered commit,
V6012 mapping); serve composes it; persist on assemble before the response
line; `serve_session` store param; run/benchmark untouched.

### T3 — docs + tests

V4017/V6012 rst pages; host record-lifecycle unit tests; commands
round-trip (acceptEdits with `edit` → status `pendingEdit` → assemble/cancel
→ gone, V4017 refusal); SlotStore unit tests per crash row + byte equality;
serve scripted e2e (assert_cmd pattern): commit writes files, reboot loads
the committed artifact (trapper + untest trick proves identity on the wire),
temp/marker/stale-marker adoption, V6012 on wire.

### T4 — Finalize

Gates (`cd compiler && just` — compile incl. no-std container gate,
coverage ≥ 85 %, clippy, fmt, dupes), plan deletion, ff-merge to
`lint-fences`, push fork.

## Rules

No new external deps; no unsafe/panic/unwrap/expect/todo in non-test code;
modules ≤ 1000 lines; KISS — reuse `stage_with_decisions`,
`Container::read_from`, the existing serve ack path; code comments cite
ADR-0064 only, never plan files; English.
