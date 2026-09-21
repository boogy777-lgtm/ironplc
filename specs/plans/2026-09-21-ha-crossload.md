# HA Phase 5 slice 3: crossload

Date: 2026-09-21
Status: draft
Branch: feature/ha-crossload

## Goal

Land the crossload milestone of the Phase 5 redundancy layer in one PR:
(1) runtime seam 3 — a typed bulk state snapshot read from the host's
persistent buffers (`vars` + `data_region`, the exact carry-over
vocabulary of `swap_buffers`); (2) runtime seam 4 — an apply-while-idle
path that writes a replicated snapshot into an idle host's buffers with
fail-closed layout checks (coded refusal, never a guess); (3) the ADR-0064
redundant-pair online-change pipeline — at Accept the Primary packages
{candidate wire bytes + state snapshot + CandidateGenerationId + epoch}
over the pair link; the Secondary validates (`Container::read_from`),
stages through the existing `stage_with_decisions`, applies the snapshot
while idle, and raises the Slice-2 `CrossloadReadiness` signal; the same
candidate generation on both sides is the admission contract (ADR-0064(c))
and a Secondary that cannot accept latches an alarm instead of pretending
redundancy-ready; (4) takeover during Testing (ADR-0064(e)): the survivor
promotes executing the CANDIDATE — never a revert to Original (that is an
untest in disguise, V4011) — with untest/cancel pair semantics. No
fencing/claim/barrier/ARM, no calibration, no UI: out of scope.

## Architecture

- **Snapshot (seam 3, mechanism inside `RuntimeHost`):** `StateSnapshot`
  is `{layout_hash, num_variables, data_region_bytes, vars: Vec<u64>,
  data_region: Vec<u8>}` — the layout identity tuple of
  `validate_candidate` plus the two persistent regions, slots exported
  through the existing `Slot::as_u64` / restored with `Slot::from_u64`.
  No new serialization dependency; the wire codec lives in the
  redundancy crate beside the ping/pong codec (a codec detail, not a
  framework).
- **Apply (seam 4, same authority as `apply_migration_swap`):** one host
  method, four fail-closed rules, all refusals in the existing
  `OnlineChangeError` vocabulary (single error authority, command-layer
  V-code mapping reused):
  - snapshot internally inconsistent (lengths ≠ declared layout) →
    `SnapshotCorrupt` (new V4019);
  - snapshot layout == active layout → copy both persistent regions
    wholesale (the steady-state replication of a monitoring peer);
  - snapshot layout == staged candidate's layout while Original is
    active → the mirrored Test advance for a migration candidate:
    rebuild the buffers from the candidate's init image (exactly
    `apply_migration_swap`'s first half), overwrite every persistent byte
    with the replicated image, flip the selector to the candidate, keep
    the migration marker (V4011 stays armed; the replicated image is
    authoritative over a local plan application);
  - snapshot layout == normal's layout while the candidate is active →
    the mirrored Untest, refused on a migration candidate
    (`UntestUnsupported`, V4011), otherwise flip back with the carried
    state;
  - anything else → `LayoutIncompatible` (V4007). A coded refusal never
    applies a byte. "Idle" is a borrow-checker property: the method takes
    `&mut self`, so no scan session can be live.
- **Takeover during Testing (ADR-0064(e)):** `takeover_testing` flips the
  execution selector to the staged candidate *without* a buffer swap,
  because the caller certifies the state already moved under the
  candidate. Refused (fail-closed) for a migration candidate whose state
  has not advanced on this host — executing unmigrated state would be a
  guess. Idempotent once Testing.
- **Idle boundary:** `apply_pending_at_boundary` lets a monitor-mode host
  (which drives no rounds — every idle moment is its boundary) complete a
  `test`/`untest` it recorded, reusing `apply_pending_swap` unchanged;
  this is how the Secondary mirrors an exact-match Untest (the snapshot
  layout is identical on both sides, so the retreat is not visible in the
  replication stream).
- **Crossload protocol (`ironplc-redundancy::crossload`):** a thin,
  CRC-guarded frame codec (demuxed from the 38-byte ping/pong frames by
  magic + length) carrying `CrossloadOffer { pair_id, epoch,
  generation, candidate_wire, snapshot }` and a response/refusal. Free
  functions per module convention: `package_offer` (Primary: snapshot +
  status' candidate generation + caller's wire bytes), `accept_offer`
  (Secondary: pair check → `Container::read_from` →
  `stage_with_decisions` with the edit provenance → generation contract
  check → `apply_state_snapshot`; a failed apply cancels the half-staged
  candidate back to a clean base). A `CrossloadReceiver` owns the
  readiness/alarm latch — the mechanism that makes "a secondary that
  cannot accept must NOT pretend redundancy-ready" unforgeable — and
  mirrors cancel/untest notices onto the host. New crate CSV rows
  V4102 (candidate rejected) / V4103 (snapshot rejected) / V4104
  (transfer interrupted) / V4105 (generation mismatch); the refusal
  response frame notifies the Primary.
- **No new abstraction layers:** the per-unit driver stays in the
  integration tests as test support (Slice 2's precedent); runtime types
  stay redundancy-free (no epoch/pair in `StateSnapshot` — those ride in
  the offer envelope).

## File map

- `compiler/runtime/src/snapshot.rs` — `StateSnapshot` (new module).
- `compiler/runtime/src/host.rs` — `state_snapshot`,
  `apply_state_snapshot`, `takeover_testing`,
  `apply_pending_at_boundary`.
- `compiler/runtime/src/error.rs` — `OnlineChangeError::SnapshotCorrupt`.
- `compiler/runtime/src/commands.rs` — V4019 mapping + test case.
- `compiler/runtime/resources/problem-codes.csv` — V4019 row.
- `compiler/runtime/tests/state_snapshot.rs` — round-trip, migration
  carry-over equivalence, fail-closed refusals, takeover, idle boundary.
- `compiler/ironplc-redundancy/src/crossload.rs` — offer/response codec,
  `package_offer`, `accept_offer`, `CrossloadReceiver`, refusals.
- `compiler/ironplc-redundancy/src/liveness.rs` — `crc32` crate-visible.
- `compiler/ironplc-redundancy/resources/problem-codes.csv` — V4102–V4105.
- `compiler/ironplc-redundancy/src/lib.rs` — module + curated exports,
  crate doc slice-3 paragraph.
- `compiler/ironplc-redundancy/Cargo.toml` — pipeline dev-deps for
  compiled fixtures (mirrors the runtime test support).
- `compiler/ironplc-redundancy/tests/crossload.rs` — two-instance
  loopback scenarios (Accept → SYNC_READY with identical generation and
  snapshot bytes; snapshot round-trip integrity; takeover mid-Test →
  survivor executes the candidate with V4011 armed; garbled/interrupted
  crossload → deSYNC + alarm + Primary notified; cancel/untest pair
  semantics).
- `docs/reference/runtime/problems/V4019.rst`, `V4102.rst`…`V4105.rst`.
- `specs/roadmap.md` — Phase 5 delivered line.

## Tasks

1. Runtime: `StateSnapshot` + `state_snapshot` + unit round-trip test.
2. Runtime: `apply_state_snapshot` (four fail-closed rules) +
   `SnapshotCorrupt`/V4019 (CSV, mapping, docs page).
3. Runtime: `takeover_testing` + `apply_pending_at_boundary` + tests.
4. Redundancy: crossload module (codec, package/accept, receiver,
   V4102–V4105 + docs pages).
5. Two-instance loopback scenarios incl. takeover mid-Test and garbled
   crossload.
6. Docs: roadmap line; full gates (`cd compiler && just`, specs gates
   via Git Bash); remove this plan before merge.

## Authorities

- ADR-0064 (the core contract: (c) same CandidateGenerationId on both
  units, (d) test barrier with no state copy, (e) takeover during
  Testing executes the CANDIDATE — reverting equals untest which
  migration candidates forbid (V4011), (f) untest keeps the candidate,
  (g) cancel from exec=Original only, (h) assemble one transaction)
- `specs/design/ha-redundancy-layer-architecture.md` (Minimal Seams 3
  and 4 — this slice; crossload module decomposition; simulator binding)
- `specs/design/external-fsm-review.md` (state snapshot accessors seam;
  takeover/revert policy ALREADY-HAVE row; readiness verdicts)
- `specs/design/ha-redundancy-fsm.md` (SYNC chart crossload expectations;
  "takeover during Testing executes the CANDIDATE")
- `specs/design/ha-architecture-readiness.md` (VmBuffers as the
  replication payload; stable-UID addressing; `read_from` as the peer
  verification call)
- Slice 2 code state (`host.rs` permit/seams, `statechart.rs`
  `CrossloadReadiness`, `liveness.rs` codec, `tests/pair_link.rs` node)
