# HA Phase 5 slice 4: fencing client + CONTROL FSM

Date: 2026-09-21
Status: draft
Branch: feature/ha-fencing

## Goal

Land the fencing milestone of the Phase 5 redundancy layer in one PR:
(1) the fencing-client seam — one interface (claim / release /
query-owners / barrier-participate) plus the capability descriptor of the
architecture doc's Protocol Portability section, with the simulator
binding first-class: a module registry where each module carries
owner/ARM state and enforces exclusivity (no real network I/O); (2) the
OwnerLease — minted by the HA supervisor at the scan-commit callback (the
Slice-2 seam), carrying the epoch, its expiry driving the proven-death
promotion case; (3) the CONTROL subchart FSM — the guard table's three
ways into CLAIMING (boot / takeover / commanded swap), claim-all →
CLAIMED_DISARMED → all-or-nothing barrier → ARM → ACTIVE, DEGRADED and
REDUNDANCY_LOST per the P/I truth table, the two promotion cases exactly,
partition → owner-conflict rejection → release-all → REDUNDANCY_LOST
(fail-closed, zero partial ownership); (4) online-change interaction —
manual swap only from SYNC_READY, mid-Test takeover unchanged (ADR-0064(e),
Slice 3), the pair assemble stays the one-transaction commit; (5) the
V4106–V4109 codes with rst pages; (6) two-instance loopback + simulator
registry scenarios. No calibration, no engineering UI beyond internal
diagnostics, no EtherNet/IP binding (a later binding), no new ADR.

## Architecture

- **`fencing` module — the client seam.** `FencingClient`: one interface,
  no more (`ha-redundancy-layer-architecture.md`, "Protocol Portability":
  the fencing authority is the one genuinely protocol-bound concern, and
  it binds behind a declared seam): `capabilities()`,
  `claim(module, owner, epoch)` (exclusive acquire; a conflicting live
  owner rejects with `OwnerConflict`), `release(module, owner)`,
  `owners()` (the query-owners view: per-module `ModuleState` — unowned /
  claimed-disarmed / armed, each stamped with owner + epoch),
  `barrier_participate(owner, modules, epoch)` (the all-or-nothing
  barrier: verify every listed module is claimed-disarmed by this owner
  in this epoch, then ARM all — any mismatch arms nothing). The
  `FencingCapabilities` descriptor records the guarantee level:
  ownership mode (single-exclusive / redundant-paired), observer
  capability (independent Input Only / Listen Only / none), staged-claim
  support, explicit-ARM support, epoch support, cyclic owner status, and
  protocol-specific degradation. Two free functions compose the barrier
  procedure over the four ops (the architecture doc's fencing module
  owns "ordered claim, CLAIMED_DISARMED verification, ARM, barrier
  rollback"): `claim_in_order` + `release_all` + `ownership_barrier` —
  any acquisition or verification failure releases everything acquired
  before the error returns. `FencingError` is the refusal vocabulary;
  `ControlAlarm` maps it (plus the driver-level alarms) to V-codes.
- **`simulator` module — the first-class binding.** `ModuleRegistry`:
  the I/O target's admission truth. Each module carries `ModuleState`
  plus `last_commit` (abstract time the armed owner's outputs last
  changed) and an online flag; exclusivity is enforced at claim. The
  registry is shared between the two units of a test through
  `Rc<RefCell<..>>` client handles (the `LoopbackPort` precedent) — one
  registry is the shared I/O network both units fence against.
  `commit_outputs(owner)` stamps the armed modules (the ACTIVE unit's
  scan-commit callback calls it — the outputs-change truth behind the
  `I` signal); `outputs_changing(owner, now, window)` is the Input-Only
  observation; `age_out(owner)` models the target's old-connection
  timeout (the failover formula's `T_old-connection-timeout` term);
  `yank(module)` models a module fault (barrier verification failure).
  The EtherNet/IP binding later replaces this module; the FSM never
  changes.
- **`lease` module.** `OwnerLease { epoch, expires_at }`, minted only by
  the HA supervisor: renewed at every scan commit over the Slice-2
  `run_with_commit` callback, and minted once at the promotion barrier
  (the barrier-pass is a supervisor-owned authority point in the scan
  loop, never the network task — ADR-0062's producer rule is about *who*
  mints). `is_expired(now)` gates the proven-death case; the survivor
  derives the peer's authority window from the last confirmed peer
  frame: `now - peer_last_seen >= lease_ttl` (`T_claim-start =
  max(peer-detection, lease-expiry)`, both proofs must agree). The TTL is
  an open parameter placeholder on `RedundancyConfig`
  (`with_lease_ttl`), like the link-timing counts.
- **`statechart` module — the CONTROL chart.** Pure table-driven chart
  beside the SYNC chart (the architecture doc: one `statechart` module
  owns the only FSMs): `Idle → Claiming → Active`, `ActiveDegraded`,
  `RedundancyLost`, events consumed only when they name a transition
  (`ClaimGuarded`, `BarrierPassed`, `BarrierFailed`, `ReleaseOwner`,
  `IoLossPartial`, `IoLossFull`, `Repaired`). The guard table is a pure
  function — the only role-dependent transitions in the whole
  statechart: boot barrier (Primary-configured, no live peer), takeover
  barrier (Secondary-configured, SYNC in SYNC_READY, `!P && !I`, peer
  authority expired), commanded-swap barrier (Secondary-configured, pair
  in SYNC_READY, swap commanded); all else role-independent. The
  detection case table is a pure function `(P, I, S) →
  DetectionAction`: `P` → none; `!P && I` → degraded-channel alarm,
  never claim, SYNC to deSYNC; `!P && !I && S` → promotion candidacy
  (silence makes a claimant, never an owner — the target is the last
  fence); `!P && !S` → claim forbidden. `ControlAlarm` is the coded
  alarm latch (V4106–V4109).
- **Promotion = exactly the two cases.** (a) Commanded swap at
  SYNC_READY: old owner releases everything, revokes its permit, goes
  `ACTIVE → IDLE`, SYNC → deSYNC (the sync relationship is dissolved by
  the swap — fed as `SyncLoss`, re-entered via `Paired`); the new
  Primary runs the barrier, and on pass bumps the epoch, mints the
  lease, grants the permit, goes ACTIVE; any failure releases everything
  and lands in REDUNDANCY_LOST — never half-done. (b) Proven death:
  liveness dead + no I/O evidence + peer authority expired + SYNC_READY
  → claim → barrier → ARM → ACTIVE. A live Primary behind a partition
  still holds Exclusive Owner: the claim is rejected with
  `OwnerConflict`, everything acquired is released, the unit lands in
  REDUNDANCY_LOST — zero partial ownership.
- **Online change.** Manual swap is refused (V4108) unless the pair is
  in SYNC_READY. Takeover during Testing stays Slice 3's
  `takeover_testing` (the fencing tests compose the permit/host seams,
  not the candidate machinery). The pair assemble stays the ADR-0064(h)
  one transaction — no change.
- **V-codes.** V4106 OwnerConflict, V4107 OwnershipBarrierFailed, V4108
  SwapRefused, V4109 DegradedChannel — CSV rows (build.rs codegen, the
  Slice-2 mechanism) + rst pages mirroring V4101's template; the docs
  problemcode extension already registers the crate CSV, so the pages
  are the registration.

## File map

- `compiler/ironplc-redundancy/src/fencing.rs` — `FencingClient`,
  `FencingCapabilities`, `OwnershipMode`, `ObserverCapability`,
  `ModuleId`, `OwnerId`, `ModuleState`, `ModuleOwnership`,
  `FencingError`, `claim_in_order` / `release_all` /
  `ownership_barrier`.
- `compiler/ironplc-redundancy/src/simulator.rs` — `ModuleRegistry`,
  `RegistryClient` (the `FencingClient` handle), `age_out`, `yank`,
  `commit_outputs`, `outputs_changing`.
- `compiler/ironplc-redundancy/src/lease.rs` — `OwnerLease`.
- `compiler/ironplc-redundancy/src/statechart.rs` — `ControlState`,
  `ControlChart`, `ControlEvent`, `ClaimBarrier`, `claim_barrier`,
  `DetectionAction`, `detect`, `ControlAlarm`.
- `compiler/ironplc-redundancy/src/config.rs` — `lease_ttl` open
  parameter + `with_lease_ttl`.
- `compiler/ironplc-redundancy/src/epoch.rs` — doc comment: the two
  supervisor-owned mint points (scan commit, promotion barrier).
- `compiler/ironplc-redundancy/src/lib.rs` — modules, curated exports,
  crate-doc slice-4 paragraph.
- `compiler/ironplc-redundancy/resources/problem-codes.csv` — V4106–V4109.
- `docs/reference/runtime/problems/V4106.rst` … `V4109.rst`.
- `compiler/ironplc-redundancy/tests/fencing.rs` — two-instance
  loopback + shared-registry scenarios (below).
- `specs/roadmap.md` — the Phase 5 slice-4 delivered line.

## Tasks

1. `fencing` module: seam, descriptor, error, barrier helpers + unit tests.
2. `simulator` module: the registry binding + unit tests (exclusivity,
   arm barrier all-or-nothing, age-out, yank, outputs-changing).
3. `lease` module + config TTL + epoch doc touch-up.
4. CONTROL chart + guard table + detection table + alarms + unit tests.
5. V4106–V4109: CSV rows, alarm v-code mapping, rst pages.
6. `tests/fencing.rs` scenarios: manual swap full sequence
   (RELEASE → claim → barrier → ARM → epoch bump → permit → re-sync,
   zero ownership on the old owner); proven-death takeover via lease
   expiry (dead owner's connections aged out at the target; survivor
   ends ACTIVE, armed, permitted, epoch moved forward); partition with a
   live primary (claim rejected → release-all → REDUNDANCY_LOST, zero
   partial ownership asserted on the registry); `!P && I` →
   degraded-channel alarm, no claim, SYNC to deSYNC; `P && !I` → partial
   fencing loss keeps the owner ACTIVE_DEGRADED (no promotion anywhere),
   full loss → REDUNDANCY_LOST; barrier failure mid-promotion (foreign
   owner on one module) → safe retreat, everything released; swap
   refused when not SYNC_READY (V4108, no claims).
7. Docs: roadmap line; full gates (`cd compiler && just`; specs gates
   via Git Bash); remove this plan before merge.

## Authorities

- `specs/design/ha-redundancy-fsm.md` — THE core contract: CONTROL
  subchart, guard table, promotion exactly two cases, zombie rule,
  detection case table, invariants (`partial ownership != ACTIVE`, no
  automatic RUN, REDUNDANCY_LOST terminal).
- `specs/design/ha-redundancy-layer-architecture.md` — Protocol
  Portability (one fencing-client interface + capability descriptor;
  simulator binding first-class; EtherNet/IP is one later binding),
  module decomposition (`fencing`, `lease`), "the statechart module
  owns the only FSMs".
- `specs/design/external-fsm-review.md` — truth-table rows as explicit
  acceptance tests (T18: `!P && !I` makes a claimant, never an owner);
  the permit latch + scan-commit seams this slice composes.
- ADR-0062 — OwnerLease minted exclusively by the HA supervisor at scan
  commit; `T_claim-start = max(peer-detection, lease-expiry)`; epoch
  anti-stale only.
- ADR-0064 — the pair online-change contract this slice keeps (swap
  guard, mid-Test takeover, one-transaction assemble).
- Slice 2/3 code state (`statechart.rs` SYNC chart, `loopback.rs` binding
  shape, `tests/pair_link.rs` / `tests/crossload.rs` node precedent,
  `run_with_commit` epoch seam, `permit_execution` /
  `revoke_execution_permit`).
