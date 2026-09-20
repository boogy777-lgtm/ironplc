# Spec: Online Editing UX Gate

## Overview

This spec defines how the IDE editor behaves while an engineering
connection is live: a monitoring-first default, an explicit confirmation
gate into editing, and a visual state model that mirrors the ADR-0064
online-change lifecycle. It is a UX contract only — no implementation, no
rendering technology.

The reference client is the VS Code extension.

This spec builds on:

- **[Engineering Connection](engineering-connection.md)**: the Mechanism 2
  connection state machine — its Connected state is the signal that drives
  the editing mode
- **[ADR-0064](../adrs/0064-online-change-on-a-redundant-pair.md)**: the
  session lifecycle whose PENDING_LOCAL state this gate produces and whose
  STAGED / TESTING / CLEAN progression the visuals mirror
- **[HA Engineering UI Contract](ha-engineering-ui.md)**: the established
  "one vocabulary, thin clients" pattern the monitoring overlays follow,
  and the `haStatus` feed the HA state comes from

## Design Goals

1. **No accidental edits in online mode** — while the controller is live,
   no input path changes code without passing one explicit confirmation
2. **Monitoring is the default** — Connected means watching, not editing;
   editing is the exception and looks like one
3. **The screen always answers "is the controller running my edits?"** —
   the visual state mirrors the ADR-0064 lifecycle one-to-one
4. **Offline is free** — with no live equipment there is nothing to
   protect: no read-only, no confirmations

## Editing Modes

The mode derives from the Mechanism 2 connection state machine, not from a
separate toggle the engineer must remember to set:

| Mode | When | Editing |
|------|------|---------|
| Offline | Any state other than Connected (Disconnected, Connecting, Reconnecting) | Free editing, no confirmations |
| Monitoring | Connected, no pending local edits | Read-only |
| Pending Local | Connected, after explicit entry | Local edits; the controller is unchanged until Accept |

### Monitoring-first

While Connected, the editor is a monitoring surface:

- **Live values and overlays** — variable values rendered in place,
  refreshed from the session's polling (the `getStatus` heartbeat cadence
  of Mechanism 2 is the slow poll).
- **Application state** — mode and generation counters from `identity` /
  `getStatus` (`compiler/runtime/src/commands.rs:136`), rendered on the
  device panel and status bar.
- **HA state** — pair state from `haStatus`
  ([HA Engineering UI Contract](ha-engineering-ui.md)) when the redundancy
  block is present.

Mouse interaction is navigation only: clicks move the cursor and change
the selection. Typing is blocked. **A click is never a code change.**

### Explicit edit-mode entry

Any modification attempt while Connected — a keystroke, a paste, a
refactor command — does **not** change code. It opens a diff-backed
confirmation over the change the attempt would have produced:

> **Enter code-change mode?** Changes stay local (Pending Local) until
> Accept.

- **Yes** → the editor enters PENDING_LOCAL; the attempted change applies
  as the first local edit.
- **No** → the input is discarded; the editor stays in monitoring.

The deliberate path is an explicit toolbar / status-bar command **Start
Pending Edits**, which enters PENDING_LOCAL directly — the same gate, used
before rather than at the first keystroke.

Offline (not Connected) there is no gate: editing is free and no
confirmation ever appears, because there is no live equipment to protect.

## Visual State Model

The visual state mirrors the ADR-0064 lifecycle so the engineer can always
read whether the controller is running edited code:

| Lifecycle state | Entered when | Visual affordances |
|-----------------|--------------|--------------------|
| MONITORING (clean) | Connected, no local edits | Live overlays; status bar shows the connection and the running generation; no edit affordances |
| PENDING LOCAL | Confirmation "Yes" or Start Pending Edits | Banner "edits are local, controller unchanged"; gutter markers on changed blocks; status-bar Pending Local indicator |
| STAGED | Accept delivered and validated the candidate | Banner shows the candidate is staged; the controller still runs the original; the pending markers stay |
| TESTING | The candidate executes under Test | Status-bar Testing indicator; live values are the candidate's values; the banner shows the exits (untest / cancel / assemble) |
| CLEAN | Assemble promoted the candidate | Returns to the MONITORING-clean rendering at the new generation |

Transitions that leave the happy path render as their lifecycle outcome:
discarding local edits returns to MONITORING; untest returns from TESTING
to STAGED; cancel drops the candidate and re-syncs the project from the
device, returning to MONITORING.

## Safety Property

In online mode there is exactly one path from input to code change: the
confirmation. The Connected state of the Mechanism 2 state machine is the
latch — modification attempts are intercepted before they reach the
buffer, so "typing is blocked" is enforced by the mode, not by reviewer
discipline. The confirmation is the only bridge into PENDING_LOCAL, and
PENDING_LOCAL is the only state in which edits accumulate.

## Client Mapping

The VS Code extension is the reference client:

- The read-only monitoring surface follows the thin-client pattern of
  `specs/steering/extension-standards.md`: the gate decision lives in
  unit-testable, vscode-free logic; the editor only renders it.
- The confirmation, the Start Pending Edits command, and the PENDING LOCAL
  banner are extension UI over that decision; they hold no session logic.
- The monitoring overlays consume the same feeds the connection spec
  already defines — `identity` / `getStatus` for application state,
  `haStatus` for HA state — and add no new wire machinery.

## Out of Scope

- Implementation of the gate, the overlays, or the confirmation UI.
- Rendering technology and widget choices.
- The host-side online-change protocol itself (owned by ADR-0052,
  ADR-0055, and ADR-0064); this spec only decides what the editor shows
  and when it accepts input.
- Offline editing behavior beyond "free, ungated".
