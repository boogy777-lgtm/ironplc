# The Runtime Tells Which Edit Commands Are Allowed, and Why Not

status: accepted
date: 2026-11-06

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
