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
  crossload, generation replicator, edit replicator, qualification, takeover.
- **Decided 2026-09-15:** network technology = EtherNet/IP; media topology =
  daisy-chain on embedded 2-port switches, optionally closed as a ring
  (DLR if the ring nodes support it). Owner constraint: 4x 10/100 Ethernet
  ports, no add-on redundancy module. Port map: 1 = redundancy link
  (crossload/heartbeat/commit replication between the pair), 2 = I/O ring
  side (daisy/ring), 3/4 = engineering, uplink, witness path.
- **Decided 2026-09-16:** no hardware arbiter. The redundancy layer is a
  software layer above the runtime; I/O ownership is pinned to the primary
  (only it opens EtherNet/IP I/O connections). Standby observes the primary
  on two independent channels: the redundancy link (port 1) and the I/O
  daisy-chain (port 2, seen from the far side). Standby auto-promotes only
  when the primary is silent on BOTH channels; a link break between the
  PLCs destroys the redundant pair (alarm + manual repair), it is never a
  failover. Fencing is protocol-level I/O ownership; standby monitors the
  chain without asserting ownership (exclusive EtherNet/IP connections).
  Cluster clock = logical epochs bumped on promotion/commit, NTP for
  diagnostics only. No TSN on bridged 10/100.
- **Still open:** failover timing/heartbeat thresholds, qualification
  policy, crossload sizing.

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
