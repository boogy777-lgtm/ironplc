# Download Is the Replace Mode of the Same Pipeline

status: accepted
date: 2026-11-06
amended: 2026-11-07 (stage 5d: the editor's Download, see "The editor's Download")

## Context and Problem Statement

An engineer changes a running application in two ways. An *online change* keeps
the application running and carries its state over to the edited one
([ADR-0064](0064-online-change-on-a-redundant-pair.md) for a body edit,
[ADR-0061](0061-engineer-decided-migration-for-out-of-policy-type-changes.md) for a declaration edit). A *Download*
replaces the application with another one. Until now the second had no path on a
running device: a candidate whose layout, schedule or process-image sizes
differed was refused with V4007, V4008 or V4009 and the engineer stopped the
device and loaded the new container, which lost every value.

The reference is CODESYS, in the behaviour it shows the user. A Download
re-initialises the application, and it keeps the variables declared
`PERSISTENT` whose declaration is equal; it does not keep `RETAIN` variables
([ADR-0085](0085-retained-variables-in-the-container.md) recorded the two kinds
for this day). The owner decided: Download is the `replace` mode of the same
pipeline as every other edit. At the boundary the application is replaced with a
re-initialisation, the machinery of the migration swap, and the keep set holds
only the `PERSISTENT` variables whose declaration is equal. The risk the rule
accepts, as CODESYS does: a variable whose meaning changes without a change of
its declaration keeps its old value.

## Decision Drivers

* One pipeline. A candidate reaches the running application by accept, test and
  assemble, whatever it is. The client does not learn a second protocol.
* One swap. Every way a candidate becomes the running application ends in the
  same boundary swap of the host; the part of the old state that survives is a
  function of the way ([class K2](#the-class)).
* One comparison. The type data of a variable is compared by the code the
  migration planner uses, not by a second copy.
* A reset is never silent. The engineer is told which `PERSISTENT` variables
  start again, and why.
* An additive wire. The session protocol stays 2 ([ADR-0063](0063-engineering-connection-transport.md)).

## Considered Options

* **D2a.** A replace candidate goes through the same accept, test and assemble
  stages as any candidate; test runs the new application live; untest and
  cancel under Test are refused (V4011) as for a migration.
* **D2b.** A replace is applied at the next boundary in one step
  (accept-and-assemble), because a re-initialisation cannot be tested and
  untested.
* A separate command for a Download.

## Decision Outcome

Chosen: **D2a, with the mode as a field of the accept**, because the code shows
it needs no second swap mechanism.

The test of a migration candidate already builds the candidate's buffers fresh,
runs its init image, copies a planned set of values over them and drops the old
buffers. A replace is that swap with another set of values to copy. The table of
hot-edit commands ([ADR-0087](0087-edit-view-and-allowed-commands.md)) already
says what follows: accept only while no candidate is staged (so a replace is
allowed in every state except while a candidate is staged or under Test), test
runs the candidate, untest and cancel under Test are refused because the old
buffers are not kept, and assemble is the way out. D2b would add a command that
skips the test, which ADR-0064 requires for every candidate ("a commit that never
ran is unverified code promoted to canonical"), and would still have to share the
swap. The cost of D2a is that the engineer cannot revert a replace once it runs,
which is the nature of a re-initialisation and the same as for a migration.

### The class

The ways a candidate becomes the running application: `change` (layout equal,
logic only), `migrate` ([ADR-0061](0061-engineer-decided-migration-for-out-of-policy-type-changes.md)) and `replace`.
`CandidateKind` (`compiler/runtime/src/candidate.rs`). A near case outside the
class: the apply of a replicated snapshot on the standby of a redundant pair
(`apply_state_snapshot`), which receives the state and does not decide it.

**Invariant.** Every kind ends in the one boundary swap, `RuntimeHost::apply_test_swap`,
an exhaustive match over the kinds. What survives of the old state is a function of
the kind: the whole persistent part for a change, the planned copy per stable
variable ID for a migration, the `PERSISTENT` variables with an equal stable ID,
qualifier and type data for a replace. Whether the candidate can be reverted is
also a function of the kind (`CandidateKind::reverts`), which the transition table
asks; an exhaustive match checks both, and `edit_guard_table` and `edit_view_walk`
walk a real host through every kind.

**Next case.** A new kind, for instance "keep the retained image too", is one
variant of `CandidateKind`, one arm of `Carry`, one arm of the swap, its row in
`reverts`, and a stage name in the edit view. The compiler names each place.

### D1. The wire

`acceptEdits` gains the optional field `mode`, `"change"` (the default) or
`"replace"`. `migrate` is not a mode a client chooses: the host selects it inside
`change` from the layout hash, as before, so the wire offers only what the client
decides. The field is absent from the line when it is the default. An unknown value
is refused by the codec.

The status carries the optional `candidateMode` (`change`, `migrate` or `replace`)
while a candidate is staged. The edit view of ADR-0087 has the new stage
`testingReplace`, with untest and cancel refused with V4011, as `testingMigration`
has. A client that does not know the stage reads the commands and reasons, which
are the contract.

The accept of a replace is answered by a new response, not by `ack`:

```json
{"response":"accepted","reinitialised":[
  {"uid":2,"name":"Retyped","reason":"typeChanged"},
  {"uid":3,"name":"Gone","reason":"notInCandidate"},
  {"reason":"noStableId"}]}
```

A new response variant, and not a field of `ack`, because only a client that
sends `replace` can receive it, so a client of the protocol as it was is never
confronted with it. Every client that waits for an acknowledgment asks
`Response::is_acknowledgment`. The protocol version stays 2.

### D3. The keep rule

`replace_keep_plan(running, candidate) -> Result<KeepPlan, MigrationError>`
(`compiler/runtime/src/migration/keep.rs`) is a pure function of the two
containers. It reads the slots of the *persistent* retained runs of the running
application ([ADR-0085](0085-retained-variables-in-the-container.md)), and for each
decides:

| `PERSISTENT` variable of the running application | Outcome |
|---|---|
| same stable ID, `PERSISTENT` in the candidate, equal type data | kept |
| no stable ID in the running application, or the candidate carries none at all | reinitialised, `noStableId` |
| its ID is not in the candidate | reinitialised, `notInCandidate` |
| the candidate declares it without `PERSISTENT` | reinitialised, `qualifierChanged` |
| type, string length, array bounds or block layout differ | reinitialised, `typeChanged` |
| a type whose copy the runtime cannot yet size (a standard-library block instance, a block with a string field) | reinitialised, `unsupportedType` |

A `RETAIN` or unqualified variable is not kept and is not listed: it was never
promised across a download. A variable only in the candidate has its initial value.

"Equal type data" reuses the migration planner: the variable table entries have to
agree in type and flags (and in `extra` unless the variable is an array, whose
`extra` is a descriptor index that two builds number alike only by chance), and the
copy is then sized by the planner's own `copy_action`, which compares the array
descriptors, and by its function block rules (`same_instance_layout`). The kept
copies are a `StateMigrationPlan`, applied by `StateMigrationPlan::apply`; the
replace has no copy code of its own.

The planner failed one case that a replace meets at once: the copy of a string or an
aggregate also copied the *slot* of the old buffers over the candidate's, which
aimed the candidate at the old offset while the bytes went to the candidate's. A
migration that left every offset in place never showed it; a replace moves them. The
copy now leaves the candidate's slot alone (its init image aimed it at its own
region), and a test pins it for the migration planner as well.

### D4. The retained file after a replace

The kept values are in the new buffers when the replace is assembled, and the device
writes the retained file at an assemble from the buffers of the application it just
committed (`Device::assembled`, REQ-VC-vm-cli-038). Nothing was added: a restart
after a Download boots the replacement from the slot store, finds a retained file
tagged with its content hash, and applies it. The kept `PERSISTENT` values survive
the restart.

### D5. Where the stable IDs come from

`ironplcc compile` reads the sidecar `<stem>.uids.json` of the project
(`create_project` in `compiler/ironplc-cli/src/cli.rs` calls
`FileBackedProject::load_uid_sidecar`), and `ironplc_project::compile` splits it
(`split_var_uids`) into the `stable_var_ids` and `fb_field_uids` of the codegen
options, so a container built from a project with a sidecar carries its
`stable_vars`. That part exists and is unchanged.

The *allocation* is `ironplcc refactor sync-uids` (`sync_uids`), which assigns
fresh IDs, keeps the existing ones and saves the sidecar; the extension's "Sync
variable IDs" command runs it. Neither `ironplcc compile` nor the build of the
extension runs it. A project that has never been synced therefore builds a
container with no `stable_vars`, and a Download of it keeps nothing: every
`PERSISTENT` variable is listed as `noStableId`. That is the honest outcome, listed
and not silent, and it is why the answer exists. The decision here is *not* to make
`compile` write the sidecar: a command that reads would write beside the project,
and a rename that the sync cannot decide needs the engineer. The remedy is to sync
before the build; the extension's Download (a later stage) is the place to ask for
it.

### The accepted risk

A variable whose meaning changes without a change of its declaration keeps its old
value. The documentation of the Download says so.

### Where a Download is recorded

The client writes the download record (`ironplcc record-download`,
[ADR-0083](0083-build-and-download-records.md)) after the device acknowledged the
assemble (`acknowledgeAssemble` in `integrations/vscode/src/buildCommands.ts`, which
calls `connection.recordDownload`). A replace follows the same ladder, so the record
is written after the replace is assembled and not before. Nothing changes there.

### The editor's Download

The command `ironplc.download` ("IronPLC: Download to Controller",
`integrations/vscode/src/downloadLogic.ts`, wired in `buildCommands.ts`) is the
client action on the replace mode. It needs an open connection and, in this order:

1. syncs the stable variable IDs (`ironplcc refactor sync-uids` with its
   resolution dialogs); rename or swap candidates left open stop the Download
   before anything is sent, because D5 says a project without IDs keeps nothing;
2. compiles once, with the compile path of the Build commands;
3. sends `acceptEdits` with `"mode": "replace"`;
4. shows a modal confirmation that lists every entry of `reinitialised` with the
   reason in plain words, says that the replacement cannot be undone once it
   runs, and adds that whatever was changed on the controller is lost when the
   device is not the one the project last downloaded to (a Download is the way
   to fix that state, so it does not refuse with E0015 as Build does). Cancel
   sends `cancelEdits`, which the view allows before Test;
5. follows the edit view: it sends the `next` command while a candidate is staged
   (test, then assemble), reads the status after each, never sends a command the
   view disables (it shows the view's reason), is bounded by the steps of
   `finalizeAll`, and on a refusal cancels the candidate when, and only when, the
   view allows it; it shows the code of the device and the stage it reports;
6. after the device acknowledged the assemble, writes the download record and
   passes `--keep-container` to `ironplcc record-download`, which keeps a copy of
   the container that was sent as `<app>.<deviceId>.download.iplc` beside the
   record (`kept_container_path` in `compiler/project/src/download_record.rs`, the
   one place that names it). The extension never derives the name.

The actions are the members of one table (`DOWNLOAD_ACTIONS`); the Download that
keeps the application running, chosen in the go-online comparison, is a later
entry. A device that answers an accept in the `replace` mode with `ack` does not
know the mode and has staged an ordinary candidate: the client cancels it and
says that the device does not support a Download.

## Consequences

* V4007, V4008 and V4009 are not the end of the road for a Download: the engineer
  accepts in `replace` mode. They still refuse a candidate in `change` mode.
* A replace is not a hot change: the application re-initialises at the test
  boundary and runs its init image. The candidate lives in RAM and can be cancelled
  until it runs; after that, assemble is the way out.
* `HostStatus` gains `kind`; `migration` still means a migration. The redundancy
  crossload asks the kind and not the flag.
* The edit marker file (`<file>.edit`) may hold the stage `testingReplace`.

### Limits

* **`PERSISTENT` fields of an instance that is not itself `PERSISTENT`.** Such a
  field has a retained run of bytes and no variable of its own in the table, so the
  keep rule does not offer it and it starts again without a line in the answer. A
  `PERSISTENT` instance is kept whole. A later class member (a field UID table with
  run ownership) would close it.
* **A redundant pair.** The offer to the standby carries the wire bytes of the
  candidate and not its mode. The standby stages it by the rule of a change; a
  replace whose layout differs is refused there unless both applications carry
  stable IDs, and the pair then does not report itself ready. Where it is staged,
  the replicated snapshot overwrites the state, so the pair agrees. Carrying the
  mode needs a new frame version of the crossload and is a stage of its own.
* **The stable IDs.** See D5.

## Confirmation

* `compiler/runtime/src/migration/tests/keep.rs`: kept (also when the variable moves
  slots), retyped, string length, no ID on either side, removed, `RETAIN`, dropped
  qualifier, a string whose region moved, an array whose descriptor index differs
  and one whose size differs, a standard block, a block with equal and with changed
  fields, a duplicated ID, nothing persistent.
* `compiler/runtime/tests/replace_acceptance.rs`: two compiled programs of different
  layouts through the command layer: the answer, the values after the swap, the
  assemble, the status, V4011 for untest and cancel under Test, cancel before the
  test, V4013 for a second accept, and a running application with no IDs.
* `compiler/runtime/tests/edit_guard_table.rs` and `edit_view_walk.rs` walk a host
  into every state of every kind; `resources/test/edit_view_golden.json` has the
  replace states.
* `compiler/vm-cli/src/serve_replace_tests.rs`: the device, with a restart over the
  same files. REQ-VC-vm-cli-062 to -064.
* `compiler/mcp`: the `mode` of `hot_edit_accept` and the `reinitialised` list.

## More Information

* [ADR-0053](0053-stable-variable-ids-for-declaration-level-hot-edit.md), [ADR-0061](0061-engineer-decided-migration-for-out-of-policy-type-changes.md),
  [ADR-0064](0064-online-change-on-a-redundant-pair.md),
  [ADR-0073](0073-state-layout-is-the-persistent-part.md),
  [ADR-0085](0085-retained-variables-in-the-container.md),
  [ADR-0086](0086-device-scan-loop.md),
  [ADR-0087](0087-edit-view-and-allowed-commands.md).
* [IronPLC Virtual Machine Runtime](../../docs/reference/runtime/ironplcvm.rst) and
  [Retention](../../docs/reference/language/variables/retention.rst).
