# Plan: VS Code UI Surfaces (Status Bar, Menus, Redundant Pair Dashboard)

## Goal

Extend the IronPLC VS Code extension beyond the left activity-bar list: a
session-aware status bar item, context menus that surface existing commands
where they apply, and a Redundant Pair Dashboard webview that renders the
HA engineering contract (`specs/design/ha-engineering-ui.md`) with an honest
"no pair connected" state plus an explicit, removable demo mode.

## Architecture

Three layers, each following the existing pattern: pure logic in a
vscode-free module, unit-tested; a thin TS file that wires it to VS Code.

1. **Status bar.** `hotEdit.ts` already owns a status-bar item, so no new
   one is created: its text/tooltip/command/theme state move into
   `hotEditStatusBarLogic.ts` (pure). Idle renders "Start Session"
   (command `ironplc.startHotEditSession`); an active session renders the
   protocol status with the pending count and the existing
   `ironplc.showHotEditStatus` command. A staged candidate sets a
   `ThemeColor` warning background (theme-safe). The same places that
   refresh the session also set the `ironplc.hotEditSessionActive` context
   key, which the palette gating consumes.

2. **Menus (declarative only).** `view/title` on the existing Commands
   view exposes the dashboard and Show Status; `editor/context` and
   `editor/title` expose Run Program and Start Hot Edit Session for
   `61131-3-st`/`twincat-pou`; `explorer/context` does the same for `.st`
   via `resourceExtname`. Palette gating hides session-dependent hot-edit
   commands until a session exists and hides compiler-dependent commands
   while no compiler is found (`ironplc.hasCompiler` context key). No menu
   duplicates a tree item's own click target.

3. **Redundant Pair Dashboard.** Command
   `ironplc.openRedundantPairDashboard` (title-bar action from the left
   view) opens a singleton `WebviewPanel` in the editor area
   (`reveal` if present, `getState`/`setState`, proper dispose, CSP nonce,
   no external resources, theme CSS variables for all colors).
   - `redundantPairLogic.ts` (pure) owns the model: pair state, the
     channel truth table (`!P && I` → observer `REDUNDANCY_LOST`, no
     promotion; `P && !I` → `ACTIVE_DEGRADED`, no promotion; `!P && !I` →
     promotion path only), claim → barrier → ARM → epoch bump; primary /
     secondary death and resurrection with the deSYNC → SYNCING →
     SYNC_READY boot and the zombie rule; partition with a live owner →
     `OWNERSHIP_CONFLICT` claim rejection → both `REDUNDANCY_LOST`; ping/
     pong `+1`/`+1000` counters; calibration/budget derivations. Plus
     `buildDashboardViewModel` for the five contract sections.
   - The data source is a single explicitly marked seam: real mode returns
     the "no redundant pair connected" empty state; DEMO MODE renders a
     banner and enables the scenario controls. Removing the demo seam when
     the HA runtime surface lands touches one module and one switch.
4. **Tests.** Unit tests for both logic modules (idle/active/pending,
   truth table, sequences, view model, empty state). `check-invariants`
   needs the new command id referenced from a test; the dashboard command
   constant lives in the logic module and the test pins it.

## Prefactoring

None: `hotEdit.ts`'s inline status-bar formatting is moved into the new
logic module as part of the change (one caller, same behavior shape).

## File Map

| File | Change |
|------|--------|
| `integrations/vscode/src/hotEditStatusBarLogic.ts` | New pure status-bar state logic |
| `integrations/vscode/src/redundantPairLogic.ts` | New pure HA dashboard model |
| `integrations/vscode/src/redundantPairDashboard.ts` | New WebviewPanel provider + HTML |
| `integrations/vscode/src/hotEdit.ts` | Use the logic module; idle/active item; context key |
| `integrations/vscode/src/extension.ts` | Register dashboard; set context keys |
| `integrations/vscode/src/test/unit/hotEditStatusBarLogic.test.ts` | New unit tests |
| `integrations/vscode/src/test/unit/redundantPairLogic.test.ts` | New unit tests |
| `integrations/vscode/package.json` | Dashboard command + menus + palette gating |

## Tasks

- [ ] Write plan (this commit)
- [ ] Status bar logic module + tests; wire `hotEdit.ts`
- [ ] Dashboard logic module (truth table, sequences, view model) + tests
- [ ] Webview provider + command registration + menus
- [ ] Gates: `npm run compile && npm run lint && npm run test:unit`
      (zero new failures; coverage >= 80%)
- [ ] `npx vsce package`; report the VSIX
- [ ] Delete plan, fast-forward merge into `lint-fences`, push fork
