# The Runtime Tells Which Edit Commands Are Allowed, and Why Not

status: accepted
date: 2026-11-06
amended: 2026-11-06 (stages 4b-4d landed: the defects named in "What is not in this stage" are fixed, see "Exits fixed in class K1"; the replace kind is added, see ADR-0088)

## Context and Problem Statement

The hot-edit state machine of the host has an exhaustive transition table
(`edit_guard` in `compiler/runtime/src/edit_guard.rs`): for each pair of an edit
state and an edit command it says whether the host accepts the command and, if
not, which refusal it gives. The host consults the table first in each of its
five edit operations (accept, test, untest, assemble, cancel).

A client that offers these commands to an engineer has to know which ones are
allowed. Until now it had no way to ask: the status carries the mode, the
candidate and the migration flag, and a client that wants to enable a button has
to rebuild the table from them. A second copy of a rule drifts, and the audit of
the edit flow found defects of exactly this class: commands that are offered and
then refused, and refusals whose V-code does not say what to do. The owner
decided that such defects are fixed in the class, not at the sites: the runtime
gives the allowed commands for each state, and the clients only show them. The
dialog of the extension (a later stage) and the MCP server read the same answer,
so no client computes enablement.

## Decision Drivers

* One rule. The set of commands the view reports must be the set the host
  accepts, by construction and not by a test that notices a difference.
* One text. A refusal has a V-code and a text already
  (`CommandError::from(OnlineChangeError)`); a view must not invent a second one.
* A new command, or a new state, is a row of the table and one label. The view,
  the wire and the golden file follow without another edit.
* An additive field keeps the session protocol version (ADR-0063).

## Considered Options

* Each client derives enablement from the mode, the candidate and the migration
  flag of the status.
* A view computed from the host fields with its own conditions.
* A view computed from the guard table and the edit state of the status
  (chosen).

## Decision Outcome

**The view is a function of the edit state and the table, and of nothing else.**
`edit_view(&HostStatus) -> EditView` in `compiler/runtime/src/edit_view.rs` reads
`HostStatus::edit_state`, which the host derives once with the function that its
guards use (`RuntimeHost::edit_state`). A command is enabled when
`edit_guard(state, command)` answers `Ok`. Its reason is the refusal the table
gives, as the V-code and the text that the command layer would answer with
(`CommandError`). The view has no condition on a host field.

**Class.** The pairs of an edit state and an edit command of the hot-edit state
machine. A near case outside the class: `setDeviceSettings`, `startScan` and
`stopScan` are allowed in every edit state and are not edit commands, so they
are not in the view.

**The wire.** The view is the additive optional field `edit` of the status
payload (`getStatus`, and the `application` block of `identity`). The protocol
version stays 2: a client that does not know the field ignores it, and a device
that predates it omits it.

```json
{
  "stage": "accepted",
  "commands": {
    "accept":   { "enabled": false, "label": "Accept",
                  "reason": { "code": "V4013", "text": "a candidate is already staged" } },
    "test":     { "enabled": true,  "label": "Test" },
    "untest":   { "enabled": false, "label": "Untest",
                  "reason": { "code": "V4014", "text": "no test is in progress" } },
    "assemble": { "enabled": false, "label": "Assemble",
                  "reason": { "code": "V4017", "text": "assemble requires the candidate to have executed under Test" } },
    "cancel":   { "enabled": true,  "label": "Cancel" },
    "finalizeAll": { "enabled": false, "label": "Finalize all",
                  "reason": { "code": "V4013", "text": "a candidate is already staged" },
                  "steps": ["accept", "test", "assemble"] }
  },
  "next": { "command": "test", "label": "Test" }
}
```

* **Stage.** `idle` (no candidate), `accepted` (candidate staged, the normal
  artifact executes), `testing` (the candidate executes), `testingMigration`
  (the candidate of a state migration executes, so untest is not possible) and
  the transient `swapPending` (a test or an untest is requested and waits for
  the next scan boundary). A staged migration candidate that has not run is
  `accepted`: the difference matters only once it runs. The mapping is an
  exhaustive match on the edit state, so a new state does not compile until it
  is named.
* **Labels.** Plain English strings, defined once, next to the table
  (`EditCommand::label`): Accept, Test, Untest, Assemble, Cancel, and Finalize
  all. The label of the Next item is the label of the command it runs.
* **Reason.** Present when `enabled` is false: the V-code and its text from the
  existing mapping from `OnlineChangeError` to the V-code, never a second text.
  Where two refusals apply, the table has already chosen one (a missing
  candidate first, then a pending swap, then the artifact that is active).
* **Next.** The command that moves the edit one stage on: idle gives accept,
  accepted gives test, testing gives assemble, testing a migration gives
  assemble. While a swap is pending there is none, and the view says so by
  `stage: "swapPending"` and the absence of `next`. The next command is always
  enabled; a test checks it on a real host.
* **Finalize all.** accept, test, assemble in one go. It is a row of the table
  next to the edit commands (`FINALIZE_ALL_STEPS`, with the label) and not a
  host operation: the client runs the steps in order, with the scan boundary
  between them that applies a swap, and the table says which steps. It is
  enabled when its first step is, and its reason is that step's reason. From any
  state with a staged candidate it is therefore disabled with V4013 (a candidate
  is already staged); the engineer finishes such an edit with Next.
* **Commands as a map.** The five edit commands are keyed by their wire names,
  so a new `EditCommand` is a new key and not a new field.

**Replace (ADR-0088).** A candidate accepted in `replace` mode (a Download) is a third
kind of candidate beside a change and a migration. The edit state carries the kind
(`CandidateKind`), the table asks the kind whether the candidate can be reverted
(`reverts`), and the view gains the stage `testingReplace`. The rows are those of a
migration: untest and cancel under Test are refused with V4011, accept is allowed in
every state without a candidate, and assemble ends the edit. The walk test and the
golden file have the three replace states.

**Extension of the class.** A new command is one `EditCommand` variant, its rows
in `edit_guard`, and its label. The view, the wire and the golden file follow.
A new state is one `EditState` variant, its rows, and its entries in the two
exhaustive matches of the view (stage and next); the compiler names every place.

### Consequences

* A client that adopts the field computes no enablement of its own (the dialog
  of stage 6 and the MCP server); a device without the field is read as before.
* `HostStatus` gains the crate-private field `edit_state`. The host derives the
  state in one place for the guards and the status.
* `EditCommand` becomes public (it is part of the view and its wire form).

### What is not in this stage

Stage 4b fixes the defects of the class that this view only makes visible: Cancel
from Testing as a two-step row, the reason of Cancel for a migration, V4013
naming the edit, the occupied-flag guard of a drop, and the false comments about
the table. Stage 4d adds the note that the candidate was lost. Until then the
view reports what the table says, including its known defects, and a change of
the table changes the view and the golden file with it.

## Exits fixed in class K1 (stage 4b-4d)

The audit defects that the view only made visible are fixed in the class, as the
owner decided: the runtime gives the allowed commands for each state, and the
clients only show them. The invariant for every member of the class: every
state with a candidate has at least one exit that the host accepts (cancel or
assemble; a state that waits for a scan boundary has the boundary), the allowed
set of the view equals the host's, no client sequences the commands of an edit
on its own, and the end of a session never changes the edit.

**Cells of the table.**

| State | Command | Before | After |
|---|---|---|---|
| Testing (logic-only) | Cancel | refused, V4015 | allowed, steps untest then cancel |
| Testing (migration) | Cancel | refused, V4015 | refused, V4011 (no revert, the old buffers are not kept) |

The text of V4011 now says that untest and cancel are not supported and that
assemble is the exit. Walk test and golden file follow.

**Steps in the table.** `edit_guard` answers with the steps of a command (`Steps`,
empty for a command that is one operation). Cancel in Testing has the steps untest
and cancel; the view lists them in the `steps` of that command, and `finalizeAll`
is a plain command view with its steps (the wire is as before). The two kinds of
steps differ in who runs them: the device runs the steps of an edit command, the
client runs those of `finalizeAll`. The host runs the first step when the command
arrives (it requests a swap) and keeps the rest; the boundary that applies the swap
runs them, each one through the table again in the state the boundary leaves. A
boundary that does not apply the swap (the permit was revoked) drops the rest with
it. The client sends `cancelEdits` once. The device loop (and the in-process MCP
session) answers after the boundary, because any command that leaves a swap pending
waits for it (`RuntimeHost::swap_pending`), so the one acknowledgment means the
edit is over; until then the view says `swapPending`. The mirror of a pair applies
its own boundary after a cancel notice. A new exit with several steps is one row
and its steps; a unit test checks that every row's steps start with a swap request
and never contain accept.

**V4013 names the edit.** The refusal of a second accept says `a candidate is already
staged: edit "fix pump" from "studio on WS-12"` when the accept that staged the
candidate gave a name, an origin or both (`StagedEdit`). The table answers with the
bare refusal; `edit_answer` adds the edit from the pending-edit record, and both the
host and the view read the table through it, so the refusal and the reason are one
text. The code stays V4013.

**The claim of a session is a value.** The occupied flag of the TCP session is held
by `SessionClaim`, whose drop frees the device, so a panic of the session thread
frees it as well and the device cannot refuse every client with V6014 for ever. The
claim is dropped before the socket, as the normal end releases it. A session end
never touches the edit: tests end a session by a clean EOF, by a drop in the middle
of a frame and by a panic, and read the edit as the next client
(REQ-VC-vm-cli-055).

**MCP reads the view.** Every hot-edit response that reached the host carries its
status with the `edit` view, a refusal included (one more round trip on a refusal
only). The code and text of a refusal are the host's and equal the reason of that
command in the view; the tool computes no enablement and sends the command even when
the last status says it is disabled.

**Candidate lost.** A candidate lives in RAM only (owner decision). The device keeps
a note of the unfinished edit in `<file>.edit` beside the slots (the name and origin
of the accept, the stage and the content hash of the candidate), written atomically
when a candidate is accepted, rewritten when the stage changes and removed when the
edit ends. At boot the device reads the note, removes it, and reports
`candidateLost { name?, origin?, stage? }` in the `identity` answer next to
`retained` (`stage` is absent when the note was damaged). It never restores the
candidate. A write of the note that fails is logged and does not change the answer of
the command, so there is no new V-code: the note is advisory, and an accept answered
with an error while the candidate is staged would mislead; the cost is that a
restart does not say the candidate was lost. While a candidate exists the edit view
carries `warning`: `a restart of the device loses this candidate`
(REQ-VC-vm-cli-056, REQ-VC-vm-cli-057).

## Pros and Cons of the Options

### Each client derives enablement from the status

* Bad: a second copy of the table in every client; they drift, and a button is
  enabled that the host then refuses.

### A view computed from the host fields with its own conditions

* Bad: a second rule inside the runtime. A state added to the table and not to
  the conditions is wrong silently.

### A view computed from the guard table and the edit state (chosen)

* Good: the allowed set is the accepted set by construction; one mapping to the
  V-code and the text; the golden file and the walk test cost nothing per
  command.
* Bad: the wire grows by about a kilobyte per status, which a status already
  tolerates (the heartbeat is a `getStatus`).

## Confirmation

* `compiler/runtime/tests/edit_view_walk.rs` drives a real host into every
  state and, for every command, compares the enablement of the view with the
  answer of the host, and the reason with the V-code and the text of the actual
  refusal. It also runs the steps of finalize all in order on a real host.
* The same walk writes the views of all states to
  `compiler/runtime/resources/test/edit_view_golden.json`, which the dialog tests
  of the extension read. The test fails when the file is out of date; run it with
  `IRONPLC_UPDATE_GOLDEN=1` to regenerate it.
* `REQ-VC-vm-cli-054` (`specs/design/vm-cli.md`) states that the status payload
  carries the view; the session test of `ironplcvm serve` checks it.

## More Information

* [ADR-0064](0064-online-change-on-a-redundant-pair.md) defines the edit phases
  and why assemble requires a test.
* [ADR-0063](0063-engineering-connection-transport.md) defines the session protocol
  and the rule for additive fields.
* [Engineering Connection](../design/engineering-connection.md) describes the
  status payload.
