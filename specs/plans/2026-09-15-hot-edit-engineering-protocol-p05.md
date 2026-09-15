# Hot Edit Engineering Protocol (P0.5)

Date: 2026-09-15
Status: draft
Branch: feature/hot-edit-p05-protocol

## Goal

Expose the P0 online change protocol (ADR-0052, ADR-0054) to external
clients behind a typed, serializable command layer in `ironplc-runtime`,
then drive it from three thin clients: the `ironplcvm` CLI (a served
session), the MCP server (tools), and the VS Code extension (commands).

## Architecture

1. **Command layer in `ironplc-runtime`** (new `commands.rs`): plain
   serde-serializable request/response types plus a line-delimited JSON
   codec (one command per line, one response per line). Commands:
   `GetStatus`, `AcceptEdits { program }`, `TestEdits`, `UntestEdits`,
   `AssembleEdits`, `CancelEdits`. The payload is the compiled container
   as bytes (JSON array); compilation stays the client's job through
   existing calls, so the runtime crate keeps its current dependency
   surface (vm + container).
2. **User-facing codes**: online-change and migration errors get stable
   V-codes starting at V4007, following the existing problem-code
   conventions (CSV, docs pages under `docs/reference/runtime/problems/`,
   `Vxxxx - ...` message formatting). ADR-0055 records the protocol
   surface decision.
3. **CLI client**: `ironplcvm serve <program>` — loads and starts the
   program through the existing run pipeline, then serves newline-
   delimited JSON commands on stdin/stdout. Suits scripted demos and the
   VS Code client.
4. **MCP tools**: `hot_edit_status`, `hot_edit_accept` (takes IEC
   source, compiles via the existing compile call, then stages),
   `hot_edit_test`, `hot_edit_untest`, `hot_edit_assemble`,
   `hot_edit_cancel`. The server only serializes; logic stays in the
   command layer.
5. **VS Code thin client**: package.json contributions plus a small
   client that compiles through the existing CLI and drives a `serve`
   session; the extension standards apply.

## File map

| Task | Files |
|------|-------|
| T1 command layer + codes | `compiler/runtime/src/commands.rs`, `error.rs`, `lib.rs`; problem-codes CSV per existing convention; `docs/reference/runtime/problems/V40xx.rst`; `specs/adrs/0055-*.md` |
| T2 CLI | `compiler/vm-cli/src/cli.rs`, new `serve.rs`, `error.rs`; vm-cli tests |
| T3 MCP | `compiler/mcp/src/tools/hot_edit.rs`, `tools/mod.rs`; mcp tests |
| T4 VS Code | `integrations/vscode/package.json`, client source |
| T5 gates | coordinator: full `just`, dupes, specs gates, merge to `lint-fences`, push |

## Tasks

1. T1: command layer, V-codes, ADR-0055.
2. T2: `serve` subcommand wired to the command layer over stdio.
3. T3: MCP tools wired to the command layer.
4. T4: VS Code thin client.
5. T5: full gates; duplicate-map cleanup if needed; merge; push.

## Test plan

Unit tests beside the new code; acceptance tests mirroring the runtime
acceptance matrix for the command layer (accept / test / untest /
assemble / cancel, untest blocked after a schema edit, status payloads);
a CLI stdio session test; MCP tool tests. End gate: `cd compiler && just`
(coverage, clippy, fmt, dupes) plus the specs gates.

## Out of scope

TCP transport, authentication, PLCopen UID storage (roadmap Phase 3+),
rename tracking, HA (Phase 5).
