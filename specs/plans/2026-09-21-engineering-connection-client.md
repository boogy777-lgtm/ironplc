# Engineering connection client: profiles, connection manager, device panel, build wiring (ADR-0063/0064/0065)

Date: 2026-09-21
Status: draft
Branch: feature/engineering-connection-client

## Goal

Land the VS Code client slice of the engineering connection in one PR:
(1) the E0010–E0015 client-side problem codes in the extension CSV registry
with generated `problems.ts` rendering and documentation pages; (2) the
connection profile model persisted in settings (`ironplc.connections`,
`ironplc.activeConnection`) with client-side validation → E0010/E0011/E0012
and credentials resolved only through `SecretStorage`; (3) the
`ironplc.connect` / `ironplc.disconnect` commands composing the existing
`HotEditSession` over the existing stdio/TCP transports behind the spec's
connection state machine (Disconnected → Connecting → Connected →
Reconnecting, bounded retries with backoff+jitter, timeouts, heartbeat via
`getStatus`) with E0013/E0014 and an `IronPLC Device` panel fed by the
`identity` handshake; (4) the baseline check against the last verified-equal
state → E0015 StaleBaseline; (5) the `ironplc.build` (Build & Commit) and
`ironplc.buildTrial` (Build & Trial) commands driving compile → acceptEdits
(with edit name/origin) → mandatory testEdits → assembleEdits with
device-panel phases. Authorities:
`specs/design/engineering-connection.md` (Mechanisms 1–3),
`specs/design/online-editing-ux.md` (status vocabulary),
ADRs 0063/0064/0065, `specs/design/extension-error-code-consolidation.md`.

## Architecture

- **Reuse, no parallel session.** The connection manager composes the
  existing `HotEditSession` (`src/hotEditSession.ts:258`) over the existing
  transports — `StdioLineTransport` (`src/hotEdit.ts:460`, exported) for
  spawned `ironplcvm serve <file>`, `TcpLineTransport`
  (`src/tcpTransport.ts:33`, `connectTcpLineTransport` with its 5 s connect
  timeout) for `address:port`. `hotEditSession.ts` gains the `identity`
  command (`IdentityInfo` parse, protocol > 1 refused), the optional
  `pendingEdit` block on `HotEditStatus`, and the optional ADR-0064 `edit`
  `{name, origin}` block on `acceptEdits` — additive extensions of the one
  session, not a second implementation.
- **Vscode-free logic, thin glue** (the extension-standards pattern):
  `connectionProfiles.ts` (profile validation matrix), `connectionState.ts`
  (the FSM + reconnect/timeout policy with injected clock/transport
  factory), `baselineLogic.ts` (capture/compare of the verified-equal
  snapshot), `devicePanelLogic.ts` (panel rows), `buildLogic.ts`
  (accept → test → assemble sequencing and trial exits). The glue
  (`connection.ts`, `devicePanel.ts`, `buildCommands.ts`) owns vscode
  objects (status bar, QuickPick, tree provider, `workspaceState`,
  `SecretStorage`) and renders what the logic decides.
- **FSM per the spec table** (`engineering-connection.md` Mechanism 2).
  Connecting: open transport (5 s bound) → `identity` round trip; any open
  failure, timeout, or identity error (including the old server's codeless
  codec refusal — the FSM table's "identity error → E0013") lands in
  Disconnected with E0013 carrying the server V-code when present.
  Connected: 5 s heartbeat (`getStatus`), two consecutive misses are a
  transport fault. TCP faults enter Reconnecting: 5 attempts, full jitter
  `random(0, min(500 ms * 2^(n-1), 8 s))` before attempts 2–5; a coded
  identity refusal ends the sequence immediately; exhaustion lands in
  Disconnected with E0014. stdio faults go straight to E0014 (no respawn —
  the child exiting is definitive). User disconnect from any state cancels
  everything into Disconnected. One manager instance per extension host =
  one active connection per workspace; a second connect is refused with a
  "disconnect first" message.
- **Baseline check (fail-closed).** `workspaceState` keeps the snapshot
  captured at the last verified-equal moment (after our own assemble):
  `{active, application}` counters plus the SHA-256 of the container bytes
  the client itself compiled. On every (re)connect the manager's
  `identity.application` snapshot is compared: equal counters → in sync;
  anything else (another engineer's edit, a reboot that restarted the
  generation counters) → E0015 with re-sync guidance. The connection stays
  up for monitoring; builds are refused while the baseline is stale. No
  stored baseline (initial deploy) → no check.
- **Build is the existing hot-edit FSM** (ADR-0063 "build is reuse"):
  `ironplc.build` = compile (shared `compileArgs` helper,
  `taskProviderLogic.ts:8`) → `acceptEdits` with the ADR-0064 `edit`
  identity and the ADR-0061 V4010 decision flow (reusing
  `acceptEditsWithDecisions`, `hotEditMigrationLogic.ts:87`) → mandatory
  `testEdits` → `assembleEdits` → post-assemble `getStatus`; phases
  compiling → uploading → verifying → running render on the device panel.
  `ironplc.buildTrial` stops after `testEdits` and offers
  assemble / untest / cancel (untest omitted when the status reports
  `migration: true`, the V4011 case). Host refusals render as the existing
  `V#### - message` pattern; compile failure is E0006.
- **Credentials.** A profile stores only `credentialsKey`; on connect the
  glue resolves the secret from `SecretStorage` (prompting once to store it
  when absent). v1 sends nothing over the wire — transport authentication
  stays deferred per ADR-0063.

## Prefactoring

- Extract the compiler runner and compile-to-container helper from the
  `hotEdit.ts` closure into `taskProviderLogic.ts` (the shared compile-arg
  module) so the connection glue's stdio factory is a second caller of the
  same `execFile` path instead of a copy (`hotEdit.ts:357` today).
- Export `StdioLineTransport` (`hotEdit.ts:460`) and the shared
  active-editor/open-dialog resolver (`hotEdit.ts:433`) for reuse by the
  connection and build glue.

## File map

New (vscode-free logic):

- `src/connectionProfiles.ts` — profile shape, defaults (port 49152),
  `validateProfiles` → E0010/E0011/E0012.
- `src/connectionState.ts` — `ConnectionManager` FSM, `backoffDelayMs`,
  injected `open`/`sleep`/`random`/heartbeat/timeout policy.
- `src/baselineLogic.ts` — `captureBaseline` / `baselineState` compare.
- `src/devicePanelLogic.ts` — panel rows from state + identity + status.
- `src/buildLogic.ts` — `runBuildCommit` / `runBuildTrial` / `resolveTrial`.

New (glue):

- `src/connection.ts` — connect/disconnect commands, manager composition,
  status-bar item, credentials, baseline persistence.
- `src/devicePanel.ts` — `ironplc.devicePanel` tree provider (explorer
  container).
- `src/buildCommands.ts` — build/buildTrial commands, trial exits.

Modified:

- `resources/problem-codes.csv`, `src/problems.ts` — E0010–E0015
  (regenerated output; Node is absent on this host so the generator script
  is mirrored by hand, byte-identical to `scripts/generate-problems.js`).
- `src/hotEditSession.ts` — `identity` command, `IdentityInfo`,
  `pendingEdit`, `acceptEdits` edit block.
- `src/hotEdit.ts`, `src/taskProviderLogic.ts` — shared compile helpers.
- `package.json` — four commands, `ironplc.connections` /
  `ironplc.activeConnection` settings, device-panel view, activation event.
- `src/extension.ts` — register the connection support.
- `docs/reference/editor/problems/E0010.rst` … `E0015.rst`,
  `specs/roadmap.md` (Phase 6 delivered lines).

Tests (unit, vscode-free): `connectionProfiles.test.ts`,
`connectionState.test.ts`, `baselineLogic.test.ts`,
`devicePanelLogic.test.ts`, `buildLogic.test.ts`,
`connectionCommands.test.ts` (command-drift guard); extensions of
`hotEditSession.test.ts` and `problems.test.ts`.

## Tasks

1. E-codes: CSV rows, regenerated `problems.ts`, six doc pages.
2. Session extensions + shared compile helpers, with tests.
3. Profiles module + settings schema, with validation-matrix tests.
4. FSM + manager glue + status bar + device panel + baseline, with FSM
   tests (connect ok, handshake fail → E0013, fault → 5 retries → E0014,
   stdio no-respawn, disconnect, one-session refusal, heartbeat miss).
5. Build wiring + trial exits, with sequencing tests (mock session records
   accept → test → assemble; trial stops at test).
6. Roadmap Phase 6 delivered lines; gates (see Risks for the Node absence).
7. Remove this plan before merge.

## Risks

- **Node.js is absent on this host** (no system node, npm, nvm, fnm, volta,
  or VS Code standalone binary): `npm run compile` / `lint` / `test:unit` /
  `generate-problems` cannot run here. The generated file is produced by
  mirroring `scripts/generate-problems.js` output exactly; JSON validity of
  package.json is checked with PowerShell; TypeScript/ESLint conformance is
  by hand against the checked-in style. The vscode unit suite and eslint
  must run in CI.
- **No device-side container hash on the wire**: the baseline compares the
  generation counters (the `StatusPayload` vocabulary) plus the locally
  computed hash of the last assembled bytes; a reboot that restarts
  counters to other values fails closed into E0015.
