# Stable Variable UID Sidecar Persistence

status: accepted
date: 2026-09-15

## Context and Problem Statement

ADR-0053 gives persistent variables stable UIDs in the container but defers
the engineering-side half of the contract: "project-model and IDE plumbing
that allocates and persists UIDs follows." Phase 3 delivers that plumbing so
declaration-level hot edit survives real editing sessions. The open questions
are where the UID store lives on disk, how entries are keyed and allocated,
how a compilation learns the table, and what happens when the store is
damaged or the sources were edited by hand between sessions. The roadmap
constraint is fixed: IEC sources and PLCopen XML stay untouched, and PLCopen
UID storage stays deferred until after Phase 4.

## Decision Drivers

* The UID store must degrade to the safe no-migration rejection ADR-0053
  describes, never to silent guessing: a project that loses or corrupts its
  store must compile as if no UIDs ever existed, not as if some did.
* The runtime never guesses at declaration intent (ADR-0054): a rename, name
  swap, reorder, add, or remove that raw text cannot disambiguate is
  resolved explicitly by the user; an unresolved candidate is rejected.
* Diffs stay clean: the store is hand-inspectable, deterministic, and free of
  timestamps and environment-specific data.
* One store per project, addressed the same way by compilation, the CLI, and
  the IDE.
* In-memory hosts (MCP, playground) supply source text directly and have no
  path to address; they are not silently given filesystem behavior.

## Considered Options

* **Store UIDs in IEC source comments or PLCopen XML.** Rejected by the
  roadmap: sources and XML stay untouched, and a comment parser is a second
  UID reader to keep in agreement with the first.
* **Persist only through an explicit CLI step, with no auto-load.** Rejected:
  a compile would silently emit a container without the `stable_vars` table,
  so the hot-edit accept flow would not benefit until someone remembered to
  sync; auto-load makes the stored table the default.
* **Auto-load for every `Project` implementation.** Rejected: only
  `FileBackedProject` initializes from a path a sidecar can be derived from;
  `MemoryBackedProject` would need a fabricated path or silently shared
  state, both worse than an explicit `set_stable_var_ids`.

## Decision Outcome

Chosen option: **a deterministic JSON sidecar `<stem>.uids.json` stored next
to the project**, the only UID store, holding the persistent prefix's
`(scope, name) -> uid` mapping.

* **Format.** `{"version": 1, "variables": [{"scope", "name", "uid"}]}`.
  Entries are sorted by `(scope, name)` with fixed field order, so equal
  tables serialize byte-identically; there are no timestamps. The scope of a
  program variable is the program name; top-level `VAR_GLOBAL` declarations
  use the scope `global`. This is the same population codegen records in the
  container's `stable_vars` table.
* **Case-insensitive keying.** Both key parts are IEC 61131-3 identifiers,
  compared and ordered case-insensitively, so a declaration matches its
  entry regardless of source casing.
* **Drop-on-removal.** A key no longer declared is dropped from the table;
  a later re-add is a new entity with initialization semantics, not the
  revival of the old one.
* **Monotonic `max + 1` allocation.** New keys receive `max + 1` over the
  table as loaded, so a removed key's UID is never reused within or across
  sessions. UID `0` is reserved and never assigned.
* **Malformed files recover empty.** A missing or malformed file (invalid
  JSON, wrong version, unexpected shape, duplicate keys, reserved UID `0`)
  loads as an empty table, and the next sync assigns fresh UIDs — the safe
  no-migration degradation ADR-0053 requires. Only a genuine I/O failure on
  an existing file is a diagnostic (P6013).
* **`FileBackedProject`-only auto-load.** `initialize`/`initialize_many`
  resolve the sidecar from the initialization path and inject the table into
  codegen options; `MemoryBackedProject` is unchanged and keeps its explicit
  `set_stable_var_ids` API.
* **Candidate-report sync leaves the sidecar unsaved.** `refactor sync-uids`
  persists the reconciled table only when the report carries no rename or
  swap candidates; a report with candidates keeps the removed keys on disk so
  `refactor map-uid` can still move their UIDs. Persisting an ambiguous
  report would destroy exactly the information the user needs to resolve it.
* **Sidecar path rule.** A directory input addresses the sidecar by the
  directory name; a file input addresses it by the file stem; in both cases
  the sidecar is the sibling `<stem>.uids.json`. Compilation and both
  refactor commands resolve the path through the one function, so they always
  address the same file.

Rename and swap candidates (exactly one removed and one added key; exactly
two of each) are heuristics reported for the user to resolve with
`refactor map-uid`; the sidecar never applies them on its own, keeping
ADR-0054's no-guessing rule on the engineering side.

### Consequences

* Good, because a project that loses its sidecar degrades to safe rejection
  instead of silent guessing, exactly as ADR-0053 promises.
* Good, because deterministic, sorted, timestamp-free JSON keeps project
  diffs clean and the file hand-inspectable.
* Good, because a hand-edited rename is recoverable: `sync-uids` reports the
  candidate and `map-uid` records the resolution, so the UID — and the
  value it identifies — survives.
* Bad, because the sidecar is one more file per project to keep in version
  control; a project copied without it silently loses declaration identity.
* Neutral, because PLCopen UID storage remains a follow-up: when it lands,
  the sidecar's role shrinks but its format does not change.

## More Information

* ADR-0053 — the stable variable ID table whose allocation and persistence
  this sidecar implements.
* ADR-0054 — the no-guessing rule the candidate-report/`map-uid` flow
  upholds on the engineering side.
* `compiler/project/src/sidecar.rs` — the format, sync, and map rules;
  `compiler/project/src/project.rs` — auto-load; `compiler/ironplc-cli/src/cli.rs` —
  the `refactor sync-uids`/`map-uid` commands;
  `integrations/vscode/src/syncUidsLogic.ts` — the IDE resolution flow.
