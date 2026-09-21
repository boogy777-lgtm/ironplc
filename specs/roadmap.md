# IronPLC Roadmap

Status as of 2026-09-15. This is the durable counterpart to the transient work
plans: decisions that outlive a branch live here or in an ADR, not in a plan
file.

## Done

- **Online change (hot edit) P0.** Container format v5: the type section
  carries the variable table (`layout_hash` input) and stable variable IDs;
  codegen emits both; the host-level `ironplc-runtime` crate stages,
  validates, and swaps code at a scan boundary with every IEC value intact
  (accept / test / untest / assemble / cancel). Decisions:
  [ADR-0052](../adrs/0052-online-change-performed-by-the-runtime-host.md),
  [ADR-0053](../adrs/0053-stable-variable-ids-for-declaration-level-hot-edit.md),
  [ADR-0054](../adrs/0054-state-migration-across-declaration-level-edits.md).
- **Declaration-level migration.** The UID diff planner (copy / init / drop,
  fail-closed) carries values across rename, reorder, add and remove; a
  schema-changing test cannot be untested.

## Phases

### Phase 1 - Documentation hygiene

Refresh the stale "no online change" and out-of-scope claims left in
`specs/design/` (`adr-and-pointer-to.md`, `runtime-execution-model.md`).
Landed with this file.

### Phase 2 - Engineering protocol (P0.5), all clients

Controller commands: `GET_STATUS`, `ACCEPT_EDITS`, `TEST_EDITS`,
`UNTEST_EDITS`, `ASSEMBLE_EDITS`, `CANCEL_EDITS`, driven by `RuntimeHost`.

- Typed, serializable command/response layer in `ironplc-runtime`, plus
  user-facing runtime codes for online-change errors (CSV, docs, tests).
- CLI client (`ironplcvm`): a served session a script can drive.
- MCP tools for the same commands (server serializes; logic stays in the
  runtime layer).
- VS Code thin-client commands.

Autonomy: full.

### Phase 3 - IDE-side UID persistence

- **Decision: sidecar JSON stored with the project**, keyed by
  `(scope path, name) -> uid`, updated by explicit rename/swap refactor
  commands. IEC sources and PLCopen XML stay untouched.
- Rename tracking and swap-names operations; the engineering Pending phase
  resolves ambiguous raw-text edits; the runtime never guesses.

Autonomy: full (the extension follows the extension standards).

### Phase 4 - Hardening

- FB field-level stable IDs and migration (today an FB-instance migration is
  allowed only when the post-prefix layout is identical).
- Type-changing migration policies (DINT -> REAL widening and similar; today
  rejected).
- `content_hash` / `debug_hash` and signatures (issue #1583).
- Load-time verifier consuming the variable table (ADR-0006).
- Per-POU code artifacts (design first; no user-visible FSM change).

Done 2026-09-15 (ADR-0058..0060, container v6). Follow-up, pending
implementation: engineer-decided initialize/preserve for out-of-policy type
changes (ADR-0061).

Autonomy: full.

### Phase 5 - Redundancy / HA

- Design first: Arbitration Quorum + Commit Certificate + Fencing (HA
  handoff, section 25), then the redundancy layer: role manager, state
  replication, generation replicator, edit replicator, readiness, takeover.
- **Decided 2026-09-15:** network technology = EtherNet/IP; media topology =
  daisy-chain on embedded 2-port switches, optionally closed as a ring
  (DLR if the ring nodes support it). Owner constraint: 4x 10/100 Ethernet
  ports, no add-on redundancy module. Port map: 1 = redundancy link
  (state replication/ping-pong liveness/commit replication between the pair), 2 = I/O ring
  side (daisy/ring), 3/4 = engineering, uplink, witness path.
- **Decided 2026-09-16:** no hardware arbiter; redundancy is a software
  layer above the runtime. Quorum (five layers):
  1. two-channel observation — the SAME logical ping/pong packet (pair id,
     role, epoch, generations, ping/pong seqs, io_owner_state, crc) on
     port 1 (pair link)
     and port 2 (through the whole I/O daisy-chain); it proves runtime and
     Ethernet-stack life plus chain traversability, not mere PHY link;
  2. logical epochs for ordering and stale-state rejection (NTP for
     diagnostics; optional future CIP Sync / IEEE-1588; no TSN needed);
  3. fencing at the target — silence on BOTH channels only makes the
     secondary to CLAIMING; it must acquire Exclusive Owner on ALL
     required outputs before running the application
     (CAN_EXECUTE_OUTPUTS = ACTIVE && owns_all_required_io); the I/O
     target is the last fence;
  4. all-or-nothing ownership barrier — any failed acquisition releases
     everything, REDUNDANCY_LOST; partial ownership never means ACTIVE
     (no functional split-brain);
  5. readiness re-establishment — after restart / pair loss / epoch discontinuity /
     unclean shutdown a controller boots deSYNC and becomes ready
     (SYNC_READY); zombie-ACTIVE re-entry is forbidden.
  Connection roles: primary = Input Only (inputs) + Exclusive Owner
  (outputs); secondary = Input Only observer, never Listen Only (it
  depends on an existing owner). v1 scope: standard Exclusive Owner +
  Input Only; dual-owner arbitrating output modules are a v2 reference.
  Failover is deterministic but not bumpless: T = detection +
  old-connection timeout + Forward_Open + validation + scan boundary.
- **Decided 2026-09-16 (owner):** configured role names = Primary /
  Secondary, assigned in the engineering UI (the engineer flashes PLC 1
  as Primary with its IP; the UI registers PLC 2's IP with the Secondary
  role). **Admission before app start:** a PLC does not know a priori
  whether it runs standalone or redundant — the redundancy layer decides
  before the application starts and grants permission (discover the
  neighbor; a live Primary neighbor makes this unit the Secondary; the
  sync pipeline runs per the readiness policy; only then may the app
  start — Secondary in monitor mode). **Manual commanded swap:** an
  engineer-commanded Primary↔Secondary swap while SYNC_READY — the old
  Primary releases ownership to IDLE and re-syncs as Secondary; the new
  Primary passes CLAIMING → ACTIVE; barrier failure rolls back to
  REDUNDANCY_LOST. A Secondary gains I/O control in exactly these two
  cases (commanded swap, proven Primary death per layer 3); a revived
  ex-Primary never re-enters as Primary. Details:
  [HA Redundancy FSM](design/ha-redundancy-fsm.md).
- **Decided 2026-09-20 (owner):** online change on a redundant pair —
  design done; implementation pending. Rockwell-adapted session lifecycle
  (Pending Local → Accept → Test → Untest/Cancel/Assemble), assemble
  tightened to require the candidate ran under Test (a new
  assemble-without-test V-code at implementation time, superseding the
  initial-deploy shortcut), Build & Commit as the finalize-equivalent
  (accept → test → assemble automatically), and the pair pipeline
  (staged delivery to both units, takeover during Testing executes the
   candidate, assemble as one transaction over the HA epoch). Details:
   [ADR-0064](../adrs/0064-online-change-on-a-redundant-pair.md).
- **Decided 2026-09-20 (owner), ADR-0064 amendment:** assemble
  persists to flash via A/B slots — flash slot A = active artifact,
  slot B = the committed candidate's wire bytes, RAM = the hot-edit
  workspace; flash is written only at Assemble (temp + fsync →
  load_verify → rename into the inactive slot → flip the marker), boot
  adopts the active slot and heals crash windows, and an unassembled
  (staged/Testing) candidate stays RAM-only and dies on reboot.
  Accepted implementation item (size S), not debt; both units of the
  pair persist identical bytes at Assemble. Details:
  [ADR-0064 Amendment](../adrs/0064-online-change-on-a-redundant-pair.md).
- **Delivered 2026-09-21 (Phase 5 slice 1):** the execution permit latch
  (runtime seam 1) and the `ironplc-redundancy` crate skeleton — the host
  boots unpermitted and `run()` refuses with V4018, standalone composition
  roots grant at startup, the boundary re-validates a revoked permit and
  cancels the pending swap terminally, and the shell's admission verdict
  (Standalone/Primary/Secondary) owns the grant policy. Details:
  [HA Redundancy Layer Architecture](design/ha-redundancy-layer-architecture.md).
- **Delivered 2026-09-21 (Phase 5 slice 2):** the pair link and SYNC
  subchart — the scan-commit callback on `RuntimeHost::run` (the epoch
  mint point), the `NicPort` transport seam with the loopback simulator
  binding, the ping/pong exchange (+1/+1000 penalty, missing-increment
  silence, restart and peer-death inputs), the anti-stale epoch, and the
  deSYNC → SYNCING → SYNC_READY chart with admission's zombie fence and
  the V4101 foreign-pair refusal. Details:
  [HA Redundancy FSM](design/ha-redundancy-fsm.md).
- **Still open:** failover timing/ping-pong confirmation thresholds and the detection
  time budget across N adapters; readiness policy; state replication sizing;
  epoch persistence in NV storage; verify target firmware allows multiple
  concurrent Input Only originators; mid-chain break policy (primary
  continues with partial I/O, degraded — secondary fencing fails,
  redundancy lost); **per-port NIC drivers + calibration metrics** — the
  `NicPort` seam and the loopback simulator binding exist (slice 2);
  target-side per-port drivers (EtherNet/IP) and the per-port EMA
  calibration collection of ADR-0062 remain;
  **engineering UI backend + tabs** — backend surface (pair status,
  SYNC/CONTROL state, per-channel calibration EMA metrics, ownership
  barrier, takeover readiness) and Studio tab structure are undesigned.
- **I/O firmware contract (AUDIT — decide with the I/O firmware spec):**
  three profiles — GENERIC (Exclusive Owner + observer-if-available;
  takeover via owner expiry + Forward_Open), REDUNDANT_OWNER (standard CIP
  dual connections, COO/ROO-style), IRONPLC_HA_IO (own modules: dual
  persistent connections, OwnerLease, Pending/Committed epoch +
  epoch_floor, staged CLAIMED_DISARMED, explicit ARM, ownership barrier,
  autonomous owner watchdog + safe-state policies
  SAFE_VALUE/HOLD_LAST/RAMP_TO_SAFE, explicit owner state in T->O status,
  per-module timing metrics EMA, PHY counters, PairId/ConfigHash binding,
  1 Exclusive Owner + 3 Input Only connection capacity). Invariants to
  audit: epoch is anti-stale not arbiter; OwnerLease minted only by HA
  supervisor; partial new ownership => physically DISARMED; ownership
  atomicity != simultaneous actuation (ARM_AT_TIME reserved); I/O safety
  never depends on the reserve PLC; generic third-party I/O yields weaker
  guarantees that Studio must surface. Evaluate rack-level ownership domain
  (one guard per remote rack) vs per-module.

Autonomy: design proposals are autonomous; implementation starts only after
the decisions above.

### Phase 6 - Engineering connection

The desktop IDE connection workflow (Owen Logic reference UI: auth block,
connection parameters, connected-device panel, status bar):

- **Connection settings** — profile model (name, transport `stdio`/`tcp`,
  address, port, credentials as a secret-store key), settings persistence,
  E0010+ validation codes. Design done.
  - **Delivered 2026-09-21:** the `ironplc.connections` /
    `ironplc.activeConnection` settings schema, client-side profile
    validation (E0010–E0012) before any transport opens, and credentials
    held only in `SecretStorage` keyed by the profile's secret-store key.
  - **Establish connection** — one session protocol over stdio (spawned
    `ironplcvm serve`) and TCP (length-prefixed JSON lines); the `identity`
    handshake filling the device panel; the client connection state machine
    with bounded reconnect; V6012+ transport codes. Design done.
  - **Delivered 2026-09-21:** the `identity` handshake (runtime) and the TCP
    transport — `ironplcvm serve --listen` with V6013 framing drops and the
    V6014 single-session refusal (ADR-0065), plus the client-side
    `TcpLineTransport`.
  - **Delivered 2026-09-21:** the client connection state machine
    (Disconnected → Connecting → Connected → Reconnecting, bounded retries
    with backoff+jitter, heartbeat via `getStatus`) behind `ironplc.connect`
    / `ironplc.disconnect`, the `IronPLC Device` panel, E0013 ConnectFailed
    / E0014 ConnectionLost, and the fail-closed baseline check (E0015
    StaleBaseline) against the last verified-equal state.
- **Decided 2026-09-20 (owner):** one engineering session at a time — the
  controller accepts exactly one session and refuses further connection
  attempts; single-writer exclusivity comes from session exclusivity, not
  from a lock token. Authentication and engineer identity are a future
  option, not v1. Decision:
  [ADR-0065](adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md).
- **Start build** — reuse of the existing compile → `acceptEdits` (upload
  + verify) → test/assemble pipeline (Build & Commit / Build & Trial
  commit policies); build-status phases on the device panel. The pipeline
  exists today (hot edit).
  - **Delivered 2026-09-21:** `ironplc.build` (Build & Commit) and
    `ironplc.buildTrial` (Build & Trial with the assemble/untest/cancel
    exits) driving the existing session commands with the ADR-0064 edit
    identity and mandatory Test; phases compiling → uploading → verifying →
    running render on the device panel.

Details: [Engineering Connection](design/engineering-connection.md).
Decisions: [ADR-0063](../adrs/0063-engineering-connection-transport.md).

Autonomy: design done autonomously; implementation follows owner review.

## Deferred

- **Debt — controller-side pending edits (parity L1), deferred by owner
  decision 2026-09-20.** Pending stays IDE-side (PENDING_LOCAL shadow
  buffer); the controller learns of an edit only at Accept. The L1 pending
  record and its additive status identity are not built now; revisit when
  a driver exists. Decision:
  [ADR-0065](adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md).
  Closure scope, change map, and sequencing:
  [Rockwell Online-Editing Parity Audit](design/rockwell-parity-audit.md),
  "Debt Closure — Controller-Side Pending Edits". Closure scope reduced
  by owner decision (same date): a RAM-only record — a plain
  `Option<PendingEditRecord>` field on the runtime host, no persistence
  port or file backend — size S, still Phase 6. The reboot-diagnostics
  case (naming the edit a reboot killed) is consciously dropped; after a
  reboot the device honestly reports no pending record. (Candidate
  *bytes* are separate and NOT this debt: they persist only via the
  assemble commit — flash A/B slots per the
  [ADR-0064 amendment](adrs/0064-online-change-on-a-redundant-pair.md),
  same date — which is an accepted Phase 5 implementation item above.)
- **PLCopen XML / project-file storage of stable variable IDs.** The
  `project` crate already reads PLCopen XML, so it is a candidate for
  carrying UIDs; changing that format is a larger, user-visible commitment.
  Revisit after Phase 4; until then the Phase 3 sidecar is the storage.
- HA follow-ups from the reference documents: external-protocol side effects
  and replay semantics, multi-task ExecutionFrontier, pair bootstrap.

## Decisions

- UID storage: project sidecar JSON (default). PLCopen XML deferred until
  after Phase 4.
- P0.5 surface: all clients (CLI, MCP, VS Code).
- Integration: work integrates into the owner's integration branch; the
  release line carries version automation and is not merged into directly.
