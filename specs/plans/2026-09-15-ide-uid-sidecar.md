# IDE-side stable variable ID persistence (Phase 3)

Date: 2026-09-15
Status: draft
Branch: feature/hot-edit-uid-sidecar

## Goal

Make declaration-level hot edit survive real editing sessions: stable
variable IDs persist in a project sidecar, rename / reorder / add / remove
are tracked explicitly, ambiguous raw-text edits are resolved by the user,
and the runtime never guesses (ADR-0054). Roadmap decision: sidecar JSON
stored with the project; IEC sources and PLCopen XML stay untouched;
PLCopen UID storage stays deferred until after Phase 4.

## Architecture

1. **Sidecar module in the `project` crate** (`sidecar.rs`): deterministic
   JSON next to the project file (`<stem>.uids.json`) mapping each declared
   variable `(scope path, name) -> uid`. `Sidecar::load/save`, and
   `sync(declared)`: unchanged keys keep their uid, new keys get
   `max + 1` (monotonic, unique; 0 reserved), removed keys are dropped
   (a later re-add is a new variable with init semantics). Sorted keys,
   no timestamps, so diffs stay clean.
   `FileBackedProject` auto-loads the sidecar and injects
   `stable_var_ids` on compile; `MemoryBackedProject` is unchanged.
2. **CLI** (`ironplc-cli`): `refactor sync-uids <project>` prints the
   report (preserved / assigned / removed) plus rename and swap candidates
   (exactly-one-removed-and-one-added; two-and-two); `refactor map-uid
   <project> <old-scope> <old-name> <new-scope> <new-name>` records an
   explicit rename/swap resolution by moving the old uid to the new key.
3. **VS Code**: "IronPLC Hot Edit: Sync Variable IDs" runs sync, shows the
   report, and when candidates exist offers quick-pick resolution that ends
   in `map-uid`; the existing accept flow benefits automatically because
   compilation injects the sidecar UIDs. Follows extension-standards and
   problem-code-management; thin sections over the CLI calls.
4. **ADR-0057** records the format, keying, drop-on-removal, `max + 1`
   allocation, and the FileBackedProject-only auto-load scope.

## Tasks

1. T1 compiler: sidecar module, FileBackedProject auto-load, refactor
   subcommands, tests.
2. T2 extension: sync command, resolution quick-picks, tests.
3. T3 finalize: ADR-0057, full gates, merge to `lint-fences`, push.

## Test plan

Unit: sidecar round-trip, sync preserve/assign/drop, monotonic uid
allocation, malformed-file handling. Integration: compiling a
FileBackedProject with a sidecar yields stable vars for known keys and
fresh behavior for unknown ones (reuse existing project/codegen test
helpers). CLI: both subcommands incl. candidate reporting. Extension: unit
tests with a mocked CLI transport per existing conventions.

## Out of scope

PLCopen UID storage (deferred past Phase 4), Pending-phase protocol
changes, MCP-side auto-load, parser-backed source rewriting.
