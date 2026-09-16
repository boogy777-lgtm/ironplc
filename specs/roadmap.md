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
  (state replication/heartbeat/commit replication between the pair), 2 = I/O ring
  side (daisy/ring), 3/4 = engineering, uplink, witness path.
- **Decided 2026-09-16:** no hardware arbiter; redundancy is a software
  layer above the runtime. Quorum (five layers):
  1. two-channel observation — the SAME logical heartbeat (pair id, role,
     epoch, generations, seqs, io_owner_state, crc) on port 1 (pair link)
     and port 2 (through the whole I/O daisy-chain); it proves runtime and
     Ethernet-stack life plus chain traversability, not mere PHY link;
  2. logical epochs for ordering and stale-state rejection (NTP for
     diagnostics; optional future CIP Sync / IEEE-1588; no TSN needed);
  3. fencing at the target — silence on BOTH channels only makes the
     standby to CLAIMING; it must acquire Exclusive Owner on ALL
     required outputs before running the application
     (CAN_EXECUTE_OUTPUTS = ACTIVE && owns_all_required_io); the I/O
     target is the last fence;
  4. all-or-nothing ownership barrier — any failed acquisition releases
     everything, REDUNDANCY_LOST; partial ownership never means duty
     (no functional split-brain);
  5. readiness re-establishment — after restart / pair loss / epoch discontinuity /
     unclean shutdown a controller boots deSYNC and becomes ready
     (SYNC_READY); zombie-ACTIVE re-entry is forbidden.
  Connection roles: duty = Input Only (inputs) + Exclusive Owner
  (outputs); reserve = Input Only observer, never Listen Only (it
  depends on an existing owner). v1 scope: standard Exclusive Owner +
  Input Only; Rockwell-style Redundant Owner is a v2 reference. Failover
  is deterministic but not bumpless: T = detection + old-connection
  timeout + Forward_Open + validation + scan boundary.
- **Still open:** failover timing/heartbeat thresholds and the detection
  time budget across N adapters; readiness policy; state replication sizing;
  epoch persistence in NV storage; verify target firmware allows multiple
  concurrent Input Only originators; mid-chain break policy (duty
  continues with partial I/O, degraded — standby fencing fails, redundancy
  lost).

Autonomy: design proposals are autonomous; implementation starts only after
the decisions above.

## Deferred

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
