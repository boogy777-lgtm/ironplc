# Spec: Online Editing UX Gate

## Overview

This spec defines how the IDE editor behaves while an engineering
connection is live: a monitoring-first default, an explicit confirmation
gate into editing, and a visual state model that mirrors the ADR-0064
online-change lifecycle. It is a UX contract only — no implementation, no
rendering technology.

The path from the sources to the code of the controller is the online-edit
dialog, which shows the stages of the lifecycle and offers only what the
controller allows.

The reference client is the VS Code extension.

This spec builds on:

- **[Engineering Connection](engineering-connection.md)**: the Mechanism 2
  connection state machine — its Connected state is the signal that drives
  the editing mode
- **[ADR-0064](../adrs/0064-online-change-on-a-redundant-pair.md)**: the
  session lifecycle whose PENDING_LOCAL state this gate produces and whose
  STAGED / TESTING / CLEAN progression the visuals mirror
- **[ADR-0087](../adrs/0087-edit-view-and-allowed-commands.md)**: the `edit`
  view of the status, from which the dialog takes every label, every
  enablement and every reason
- **[ADR-0088](../adrs/0088-download-is-the-replace-mode.md)**: the replace
  mode (Download), whose candidate the dialog shows as well
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
| Monitoring | Connected, no net code diff | Code read-only; prospective edits held in a shadow buffer |
| Pending Local | Connected, after confirmed net diff or Start Pending Edits | Local edits; the controller is unchanged until Accept |

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
the selection. Typing is captured in a shadow buffer, not in the code.
**A click is never a code change.**

### Diff-triggered edit-mode entry

While Connected, the editor holds prospective edits in a shadow buffer;
the code and the online baseline stay untouched. The client computes a
diff of the shadow buffer against that baseline. The confirmation appears
at the moment the first **net code diff** is detected, showing that diff:

> **Enter code-change mode?** Changes stay local (Pending Local) until
> Accept.

- **Yes** → the editor enters PENDING_LOCAL; the diff applies as the first
  local edit.
- **No** → the shadow buffer is discarded; the editor stays in monitoring.

If the buffer ends up identical to the baseline — an edit reverted, a
no-op keystroke — there is no diff to confirm: no dialog appears and the
editor stays in monitoring. No other trigger ever prompts; clicks, cursor
moves, and commands that change nothing are not diffs.

The deliberate path is an explicit toolbar / status-bar command **Start
Pending Edits**, which enters PENDING_LOCAL directly, without waiting for
a diff.

Offline (not Connected) there is no gate: editing is free and no
confirmation ever appears, because there is no live equipment to protect.

## The Online-Edit Dialog

`IronPLC: Online Edit` opens a Quick Pick that stays open through all stages of
an edit. It needs an open connection.

### Stages

An online edit passes four stages, in this order:

| Stage | What happens | Device command |
|-------|--------------|----------------|
| Build | The editor compiles the project | none (`ironplcc compile`) |
| Accept | The controller loads the candidate and checks it against the running application; the candidate does not execute | `acceptEdits` |
| Test | The candidate executes live; the original is kept | `testEdits` |
| Assemble | The candidate becomes the running application | `assembleEdits` |

The way back is **Cancel**: before Test it discards the candidate; under Test of
a change it first returns to the original program and then discards the
candidate, as one command that the device runs. A candidate that migrates
variables of a changed type, and a candidate sent by Download (the replace
mode), cannot be reverted once it runs: Cancel is disabled with V4011 and
Assemble is the exit.

### What the dialog shows

| Part | Content |
|------|---------|
| Title | The stage of the edit on the controller (no candidate, accepted, testing, testing a migration, testing a download, waiting for the scan boundary) with the step and the total (`step`/`totalSteps` of the widget) |
| Busy line | The widget's progress bar, and the name of the running stage in the prompt line, while a command runs |
| Prompt line | The stages marked `✔` (done), `●` (current) and `○` (to do), without colour |
| Notices | The `warning` of the view while a candidate exists (a restart of the device loses it) and, for a migration or a download, what that kind of candidate means |
| **Next** | Labelled with the next stage: Build, Accept, Test or Assemble |
| **Finalize all** | An item (not a toggle) that runs the remaining stages in order |
| **Cancel** | The exit of the current stage |

An item that the device does not allow stays in the list: its description is the
device's reason (the V-code and its text) and selecting it does nothing.

### Where each part comes from

- The labels, the enablement and the reasons are the `edit` view of the status
  (ADR-0087), copied and never recomputed. Next is the `next` of the view, and
  Finalize all is `finalizeAll` with its `steps`. A device that does not report
  the view gets a dialog with nothing enabled but Build.
- The client holds exactly one fact: **a build of the current sources succeeded
  in this dialog.** Build is not a device command, so the view knows nothing of
  it. While the device is idle and the fact is missing, Next is Build and
  Finalize all starts with Build; in every other case Next is the view's. The
  fact is lost when a source file changes after the build, and when the build
  has been assembled.
- The client sequences commands only through the steps the view names: the
  steps of Finalize all (the client sends them, and reads the view again before
  each one after the first, so a step the view disables is not sent and is
  shown with the view's reason), and the two-step Cancel under Test (the client
  sends one command; the device runs the steps).
- After every command the client reads the status again and renders the
  view, so the dialog is always the device's state.
- The download record is written once, when the device acknowledges the assemble
  of a candidate that this dialog staged.

### Closing and reopening

The dialog keeps its place when focus moves to another window. `Esc` hides it and
leaves the candidate where it is. While a candidate exists, a status bar item
(`Online Edit: <stage>`, its tooltip carrying the warning of the view) reopens
the dialog.

### Download

Download (`IronPLC: Download to Controller`) is a separate command with its own
confirmation: it replaces the application and follows the device's view to the
end by itself. If it stops with a candidate staged, the dialog shows that candidate
as the ordinary stages (stage `testingReplace` under Test) with the exits the
view gives it.

## Visual State Model

The visual state mirrors the ADR-0064 lifecycle so the engineer can always
read whether the controller is running edited code:

| Lifecycle state | Entered when | Visual affordances |
|-----------------|--------------|--------------------|
| MONITORING (clean) | Connected, no net code diff | Live overlays; status bar shows the connection and the running generation; no edit affordances |
| PENDING LOCAL | Confirmation "Yes" or Start Pending Edits | Banner "edits are local, controller unchanged"; gutter markers on changed blocks; status-bar Pending Local indicator |
| STAGED | Accept delivered and validated the candidate (edit stage `accepted`) | Status bar item `Online Edit: Accepted`; the controller still runs the original; the dialog marks Test as current |
| TESTING | The candidate executes under Test (`testing`, `testingMigration`, `testingReplace`) | Status bar item `Online Edit: Testing`; live values are the candidate's values; the dialog marks Assemble as current |
| WAITING | A command waits for the next scan boundary (`swapPending`) | The dialog offers nothing until the view changes |
| CLEAN | Assemble promoted the candidate | Returns to the MONITORING-clean rendering at the new generation |

Transitions that leave the happy path render as their lifecycle outcome:
discarding local edits returns to MONITORING; Cancel drops the candidate and
returns to MONITORING. The exits are those of the `edit` view, shown by the
dialog (Cancel under Test is one command; under a migration or a replace
candidate Assemble is the only exit). The dialog shows the `warning` of the view
while a candidate exists: a restart of the device loses it.

## Safety Property

In online mode there is exactly one path from input to code change: the
confirmation, and it opens only on a net code diff. The Connected state
of the Mechanism 2 state machine is the latch — keystrokes land in a
shadow buffer, the diff of that buffer against the online baseline is the
only trigger that can prompt, and PENDING_LOCAL is the only state in
which edits accumulate. Start Pending Edits opens PENDING_LOCAL with a
clean buffer and carries no change across the gate. The gate is enforced
by the mode, not by reviewer discipline.

From the sources to the code of the controller there is likewise one path, the
dialog: Build, then Accept, Test and Assemble, each of which the controller must
allow by its `edit` view. Finalize all runs the same steps and stops at the first
step the view disables or the device refuses; a device whose view is unavailable
offers Build only.

## Client Mapping

The VS Code extension is the reference client:

- The monitoring surface follows the thin-client pattern of
  `specs/steering/extension-standards.md`: the shadow buffer's diff
  against the online baseline and the gate decision live in
  unit-testable, vscode-free logic; the editor only renders it.
- The confirmation, the Start Pending Edits command, and the PENDING LOCAL
  banner are extension UI over that decision; they hold no session logic.
- The dialog follows the thin-client pattern of
  `specs/steering/extension-standards.md`. The model — title, step, busy line,
  stage marks, notices and the three items — is a function of the `edit` view,
  the kind of the candidate and the one Build fact, in unit-testable, vscode-free
  logic (`onlineEditLogic.ts`). The runner that sends the item's command or steps
  and reads the view again is vscode-free as well (`onlineEditRunner.ts`). The
  shell (`onlineEditCommands.ts`) sets the widget properties, runs the selected
  item and reopens the dialog from the status bar item.
- Tests feed every state of the runtime's golden file of edit views
  (`compiler/runtime/resources/test/edit_view_golden.json`) to the model and
  assert that the labels, the enablement and the reasons of the items equal the
  view's, and a guard test fails when the model or the runner reads a status
  field other than `edit` and `candidateMode`.
- A stage is one entry of the table of stages; a new edit command that the
  device adds to the view appears in the dialog as Next, as a step or as Cancel
  without a change to the model, and needs one entry in the table of commands the
  runner can send.
- The monitoring overlays consume the same feeds the connection spec
  already defines — `identity` / `getStatus` for application state,
  `haStatus` for HA state — and add no new wire machinery.

## Out of Scope

- Implementation of the gate, the overlays, or the confirmation UI (the dialog is
  specified above and implemented; the gate and the overlays are not).
- Rendering technology and widget choices.
- The host-side online-change protocol itself (owned by ADR-0052,
  ADR-0055, and ADR-0064); this spec only decides what the editor shows
  and when it accepts input.
- The `candidateLost` note that the `identity` of a restarted device carries
  (ADR-0087): the dialog does not show it.
- Offline editing behavior beyond "free, ungated".
