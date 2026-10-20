# Shared Snapshot and Editing (Parse-Tree Stages S4 and S5)

## Goal

Stage S4 of [Lossless CST and Recomputing Semantic Analysis](../design/parse-tree-architecture.md#5-evolution-steps):
the CLI, the language server, the MCP server, the build and the playground request
analysis through one snapshot API. Editing produces new snapshots, the parse of the
compatibility libraries is cached under a bound, and a result is published only for
the revision it was computed from. The latency and memory of an editor session are
measured. S5 (local reparse) is decided from those numbers and is not planned
beyond that decision.

The recompute baseline stays: every request analyzes the whole snapshot, with no
query framework and no reuse of analysis results (§3.3). Libraries stay fully
analyzed on every run. Only their parse is cached, keyed by what the parse reads.

## Current state (sweep of `main` 0bb29097b)

| Consumer | Path to analysis | Problem |
|---|---|---|
| CLI `check`/`compile`/`sync-uids` | `FileBackedProject` → `Project::semantic(Build)` (`project/src/project.rs:378`) | — |
| LSP diagnostics | `FileBackedProject` behind `LspProject::semantic_all` (`lsp_project.rs:207`) | Full sync only (`lsp.rs:226`); the version is echoed, never stored or compared (`lsp.rs:280-326`); no `didClose`; analysis on the loop thread for every change, one after another |
| LSP `ironplc/run` | `lsp_runner.rs:223-264`: `parse_source` + `analyze` + codegen of the text the client sends | A private pipeline: no compatibility libraries, no other file of the project, no message settings |
| MCP analysis tools | a new `MemoryBackedProject` per request (`mcp/src/tools/check.rs:105`, `compile.rs:123`) | — (stateless by design) |
| Build | `ironplc_project::compile` (`project/src/compile.rs:79`) | — |
| Playground | parses the library text itself (`playground/src/lib.rs:433-464`), passes `set_preparsed_libraries` | A second library-loading path |

The state shared by all of them:

- `FileBackedProject` and `MemoryBackedProject` repeat `change_text_document`,
  `tokenize` and `semantic` line for line (`project.rs:349-386`, `:503-534`).
- Each stores the last analysis (`semantic_context`, `analyzed_library`).
  `change_text_document` does not clear it, so after an edit the project returns the
  analysis of the old text until `semantic` runs again.
- The bundled compatibility libraries are read from disk and parsed on every
  `semantic` call, so once per keystroke in the editor
  (`sources/src/libraries/mod.rs:200-229`, `:290-324`).
- The rowan tree is dropped after lowering (`parser/src/frontend.rs:102-112`). The
  syntax crate has no edit API.

## Architecture

- **Snapshot.** An immutable value: revision, the sources (text, file type, a
  parse cache filled once), compiler options, activated libraries, UID table. Readers
  share it (`Arc`). An edit returns a new snapshot with the next revision. The
  unchanged sources are shared, and so is their parse.
- **Analysis.** `analyze(&Snapshot, Area) -> Analysis` is the one entry point. An
  `Analysis` owns the diagnostics, context and library, and carries the revision it
  was computed from. Projects no longer store a "last analysis". A consumer that
  holds an `Analysis` holds exactly the snapshot it describes, so the "stale after
  edit" state cannot be written.
- **Source of the snapshot.** A directory, a set of in-memory texts or the editor
  are inputs that build a snapshot. They are not kinds of project. One project type
  replaces the two copies.
- **Library parse cache.** One loader for library text, from the bundled registry or
  from text the host supplies (the playground). The cache key is the library's text
  identity plus the parse options (`parse_options`, REQ-PT-parser-013). The cache
  keeps only the entries of the current snapshot's activated set (bound), so a
  changed option or activation set invalidates the entry by key, explicitly.
- **Editor revisions.** The language server keeps the current snapshot and the
  version of each open document. Before it analyzes, it applies every change already
  queued, so it analyzes only the newest revision. It publishes an analysis only when
  the analysis revision equals the current revision. Incremental sync applies range
  edits through `LineIndex` (REQ-PT-syntax-011). `didClose` returns the file to its
  text on disk.
- **No worker thread and no cancellation token at first.** Coalescing queued changes
  already excludes stale publication on one thread. A worker with cooperative
  cancellation comes only if the measurements show requests (semantic tokens,
  symbols) waiting past the budget behind an analysis.

## Class / Inv / N+1

- **Class "consumer of analysis":** CLI commands, the language-server
  notifications and requests, the MCP tools, the build, the playground. Near case
  outside: syntax-only tools (`tokenize`, `echo`, MCP `parse`), which read the tree
  of a snapshot and no analysis.
  - **Inv:** a production crate gets analysis only through `analyze(&Snapshot, _)`.
  - **Check:** a fence test lists the production call sites of
    `ironplc_analyzer::stages::analyze`, `parse_program` and `parse_source` outside
    `project`, `parser` and `sources`. It fails on a site that is not in its table,
    and each table row has its reason: syntax-only, benchmark, or test.
  - **N+1:** a new consumer either calls `analyze` or adds a row with a reason.
    `ironplc/run` (`lsp_runner.rs:223`) is the existing defect of this class.
- **Class "input of a library parse":** library text, its version, the parse
  options.
  - **Inv:** the cache key is derived from the same flag list that `parse_options`
    reads (REQ-PT-parser-013).
  - **Check:** a test toggles every flag of that list and observes a cache miss, so
    a flag added to the parse cannot be missing from the key.
- **Class "edit of a snapshot":** an LSP incremental change; later, edits that a
  rewrite derives from the tree.
  - **Inv:** text outside the edited ranges is byte-identical, and the tree text
    equals the new text (REQ-PT-syntax-001).
  - **Check:** a property test over the shared corpus with random edit sequences,
    including delimiter, comment and pragma edits.

## Prefactoring

- **PR 1 (prefactor, measurement only).** The bench `editor_session` in
  `compiler/benchmarks`:
  - drives the project path the language server uses;
  - measures cold open, warm no-change, a body edit, an interface edit and a
    configuration edit;
  - reports the median and p95 latency, allocations and retained bytes
    (`stats_alloc`), and splits the time into library parse, source parse and
    analysis.

  Corpora:
  - the shared repository corpus;
  - one generated large project, so the bench needs no private files;
  - the owner's private project, run locally only, with aggregate numbers in the
    record (never in the repository).

  These numbers are the baseline. The owner sets the budgets from them.
- **PR 2 (prefactor, behaviour-preserving).** `Analysis` becomes the owned result of
  `semantic`, and one project type replaces `FileBackedProject` and
  `MemoryBackedProject`. Consumers take what they read from the `Analysis` they
  hold. ΔR < 0: the two copies and the two cache fields are gone, so no project can
  return an analysis of old text.

## Design doc reference

- [parse-tree-architecture.md §3.2, §3.3, §4, §5](../design/parse-tree-architecture.md)
- [parse-tree-s0-experiment.md](../design/parse-tree-s0-experiment.md) (benchmark method)

## File map

- `compiler/project/src/{project,compile}.rs`, a new `snapshot.rs` and `analysis.rs`
- `compiler/sources/src/{source,project}.rs`, `sources/src/libraries/mod.rs`
- `compiler/ironplc-cli/src/{lsp,lsp_project,lsp_runner,cli}.rs`
- `compiler/mcp/src/tools/{check,compile,common}.rs`
- `compiler/playground/src/lib.rs`
- `compiler/benchmarks/` (new bench `editor_session`)
- `compiler/syntax/src/` (an edit type with the text property, PR 5)
- `integrations/vscode/src/runSession.ts` (only if Q2 is "project")
- `specs/design/parse-tree-architecture.md` (S4 status and numbers, S5 decision)

## Tasks

### PR 1: baseline bench `editor_session` (prefactor)
- [ ] The bench, its corpora and the generated large project
- [ ] Three runs recorded in `parse-tree-s0-experiment.md` style; numbers go to the owner for budgets

### PR 2: owned `Analysis`, one project type (prefactor)
- [ ] `analyze(&Snapshot, Area) -> Analysis` with its revision; `semantic` delegates to it
- [ ] One project type; directory and in-memory input build its snapshot
- [ ] CLI, LSP, MCP, playground read from the `Analysis` they hold; all tests unchanged

### PR 3: library parse cache, one loader (core)
- [ ] Cache keyed by text identity + parse-option flags, retention = current activated set
- [ ] Playground passes library text to the same loader (`set_preparsed_libraries` and `parse_activated_libraries` removed)
- [ ] Test: each parse flag toggles a miss; libraries are still analyzed on every run (diagnostics unchanged)

### PR 4: editor revisions (core)
- [ ] Document versions stored; queued changes coalesced; publish only when the revision matches
- [ ] Incremental sync through `LineIndex`; `didClose` returns the file to its disk text
- [ ] Tests: a burst of changes publishes once, for the newest version; a stale analysis is never published; edit-sequence property (untouched text preserved)

### PR 5: one consumer path (core, defect)
- [ ] `ironplc/run` builds from the language server's snapshot (Q2 decides: current document or whole project)
- [ ] Fence test of the analysis consumers (Class above)
- [ ] Same-snapshot agreement test: the CLI `check`, the MCP `check` and the LSP publication of one snapshot, per area, give equal diagnostics

### PR 6: measurement after S4, S5 decision (docs)
- [ ] `editor_session` rerun against the baseline and the owner's budgets
- [ ] Design doc: S4 delivered, status `implemented`; S5 "not needed" with the numbers, or a separate S5 plan if a budget is missed by parse time

S5, if the numbers call for it, gets its own plan PR: safe reparse boundaries, a
full-file fallback, and local and full trees and diagnostics that agree after edit
sequences.

## Questions for the owner

- **Q1.** Is the case rewrite you asked for (`bool` → `BOOL`, the CODESYS-like text
  rewrite) the first rewrite-derived edit in S4? Or does it stay in the message plan
  (PR 7) and use the S4 edit type once that lands?
- **Q2.** `ironplc/run` (Run in VS Code) today compiles only the text of the open
  editor, with no libraries and no other files. After PR 5, should it build the
  whole project (as `Build` does) or the open document, with the project's
  libraries and settings?
- **Q3.** The budgets, once PR 1 gives the baseline, for keystroke-to-diagnostics
  latency (p95) and the retained memory of a session.

## Not included

- Reuse of analysis results (precompile model, selective typification): separate
  measured stages (§3.3), path 2.
- The CODESYS-like library packaging: final stage of the roadmap.
- A worker thread and cancellation: only if PR 6 shows the need.
