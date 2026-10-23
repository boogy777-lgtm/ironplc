# Online Model: Durable Device, Cyclic Device, Exits of an Edit, Go Online

## Goal

The engineer connects to a controller, sees what it holds against the project,
and changes the running application through one standard pipeline, with the
exits of every state shown and the retained values kept across a restart. This
is the "online model" that the discovery plan
(`2026-10-21-plc-discovery.md`, sections 8 and 9) left as its input.

- **Durable device (K3).** RETAIN and PERSISTENT variables survive a restart of
  the device, and the device says which application it runs and which one it
  holds on disk.
- **Cyclic device (K4).** `ironplcvm serve` executes the scan cycle by itself,
  as `ironplcvm run` does, and handles session commands between rounds. Run and
  Stop are a mechanism of their own and compile nothing.
- **Exits of an edit (K1).** For every state of the device the runtime says
  which commands are allowed, with the reason for each one that is not. The
  clients only show it.
- **Deploy modes (K2).** An offline edit reaches the controller through Build and
  the standard pipeline (accept, test, assemble, swap at the next scan
  boundary). Download is the same pipeline in a "replace" mode. "No application"
  is an empty container.
- **The dialog.** A VS Code QuickPick that stays open through all stages
  replaces the modal Trial dialog.
- **Go online.** Connect never refuses because the controller is a different
  one. A function of (device state, records, project) says what the controller
  is, what it holds, what the project has, and which actions follow.

## Decided by the owner

These are fixed. The plan does not reopen them.

1. **What edits go through.** Offline edits go through Build and the standard
   pipeline: accept, test, assemble, with the swap at the next scan. Loading the
   difference is the engineer's responsibility (the CODESYS choice; confirmed
   explicitly where the process may change unpredictably).
2. **Download is the "replace" mode** of the same pipeline: at the boundary the
   application is replaced with a re-initialisation, with an empty "keep" set
   (the machinery of the migration swap).
3. **No application is an empty container** (no ST files, so no programs and no
   tasks). It adds no state to the host's state machine.
4. **Test ownership.** A candidate under Test stays under Test when the session
   drops. The side that runs the test owns it, which is the controller.
5. **Audit defects are fixed inside class K1**, not as patches. K1: the runtime
   gives the allowed commands for each state, and the clients only show them.
6. **Order.** K3 (retained store) and K4 (the cyclic scan in `serve`) come
   first. K1 comes after them.
7. **Stage model, after Rockwell.**
   - Accept loads the edit and does not execute it.
   - Test executes the new logic live, outputs follow it, and there is no
     automatic pass or fail check.
   - Untest goes back to the original.
   - Assemble is permanent and cannot be undone.
   - "Finalize all" means all three.
8. **Output-off test** (the SLC 500 Remote Test) is a later class, with I/O.
9. **The online-edit dialog is a VS Code QuickPick (owner chose B).**
   - It stays open through all stages.
   - The title shows the stage, plus `step`/`totalSteps`. The `busy` line shows
     while a stage runs.
   - Items: Next (its label is the next stage: Build, Accept, Test, Assemble);
     "Finalize all", a button item and not a toggle; Cancel, the exit of the
     current stage. When Cancel is unavailable, the reason is in its description
     and it does nothing.
   - Stages are marked ✔ (done), ● (current) and ○ (to do), with no colour.
   - Labels and enablement come from the runtime's K1 function. The dialog
     replaces the modal Trial dialog.
10. **Go online.**
    - Connect never refuses because the controller is a different one.
    - The comparison shows: what the controller is, what it holds, what the
      project has, and which actions follow.
    - When the controller is not the one last chosen, that is an information row.
    - The truth table is section 8 of the discovery plan, reconciled below.
11. **Run/Stop is a separate mechanism from Build.** Run/Stop starts and stops
    the scan; no run path compiles.
12. **VS Code version.** The owner targets the current VS Code. `engines.vscode`
    (now `^1.75.0`) is raised only when a feature needs it.
13. **Later, after this plan:** a source archive downloaded into the PLC with the
    application, Upload creating a project from it, and project templates (see
    "Not included").

## Current state (sweep of `main` a48a8eb65)

Facts, with the code location. The audit of the discovery plan (section 9) is
re-checked; the rows that changed are marked.

**Device side**

- **Hot-edit FSM.** `RuntimeHost` (`compiler/runtime/src/host.rs`) has `stage`,
  `test`, `untest`, `assemble`, `cancel` (lines 236-395). Each command carries
  its own guard inline, as separate `if` chains. `HostMode` is `Normal` or
  `Testing`. The pending-edit record (`PendingEditRecord`, line 99) is RAM only.
- **The commands** (`runtime/src/commands.rs:29-63`): `Identity`, `GetStatus`,
  `AcceptEdits`, `TestEdits`, `UntestEdits`, `AssembleEdits`, `CancelEdits`,
  `SetDeviceSettings`. There is no start, stop, or allowed-commands query.
- **Status** (`StatusPayload`, `commands.rs:176`) holds mode, generations,
  `migration`, `rounds`, `pendingEdit`, `contentHash`, `layoutHash`. The
  identity (`IdentityPayload`, `commands.rs:314`) wraps the device block
  (`deviceId`, name, model, modification, `runtimeVersion`), the status, and the
  optional redundancy block. The session protocol is version 2.
- **Audit row 7 (still a defect).** `cancel()` refuses while the candidate is
  active (`host.rs:382-396`); the extension's `resolveTrial` sends it directly
  (`buildLogic.ts:90-102`).
- **Audit row 8 (still a gap).** `untest()` refuses a migration candidate
  (`host.rs:330-348`, V4011).
- **Audit row 17 (still a defect).** The occupied flag is released by a plain
  store after the session (`vm-cli/src/tcp.rs:182`), with no drop guard. A panic
  in the session thread leaves the device answering V6014 until restart.
- **Audit row 18 (still the fundamental gap, K4).**
  - `ironplcvm run` loops (`vm-cli/src/cli.rs:46-103`): `run_round(uptime)`, then
    sleeps until `next_due_us()`, with a stop flag.
  - `serve` runs one round at clock 0 only after `testEdits`, `untestEdits` or
    `assembleEdits` (`vm-cli/src/serve.rs:573`, `drive_scan_round`). Between
    commands the device does not execute.
  - The TCP session thread takes the host, shell, device and store locks for the
    whole session (`tcp.rs:150-170`).
  - `run` drives `Vm` directly. `serve` drives `RuntimeHost::run_with_commit`
    (`host.rs:631`), which loads the VM once per session of rounds.
  - The host has no pacing: `next_due_us` is on `Vm` (`vm/src/vm.rs:734`) and
    not on the host.
- **Audit rows 11 and 12 (still the fundamental gap, K3).**
  - The language and the DSL carry `RETAIN`, `NON_RETAIN` and `PERSISTENT`
    (`dsl/src/common/variable.rs:633-645`), but the container has no retention
    flag (`specs/design/bytecode-instruction-set.md:1236`) and the compiler has
    no memory for retained variables (`specs/design/attributes.md:495`).
  - The word "persistent" in the code (`container/src/persistent_extent.rs`,
    `runtime/src/persistent_state.rs`, ADR-0073) means the state layout that
    survives a body edit. It is **not** the IEC qualifier. The plan says
    "retained" for the qualifier and keeps the ADR-0073 word as it is.
  - The slot store (`vm-cli/src/slot_store.rs`, `commit` line 199, `boot` line
    120) keeps two slots and a marker and writes with fsync; it stores code only.
  - V6012 (`vm-cli/resources/problem-codes.csv:13`): the swap is promoted in RAM
    and the slot is not written. After a restart the old code returns silently.
    The status has no field for the persisted identity.
- **Candidate loss on restart.** The candidate and its record are RAM only
  (`host.rs:158-183`). A reboot loses them without a trace.
- **No empty application.** `serve <FILE>` needs a container
  (`vm-cli/src/serve.rs:99-103`, `SlotStore::boot`).
- **No replace mode.** `AcceptEdits` runs `validate_candidate` or
  `validate_migration_candidate` (`runtime/src/online_change.rs`). A change of
  schedule, I/O sizes or flags (V4008, V4009, V4007) has no deploy path.

**Engineering side**

- **Build and Commit** (`integrations/vscode/src/buildLogic.ts`): accept, test,
  assemble. **Build and Trial** stops under Test and shows a modal message
  (`buildCommands.ts:150-186`). Esc leaves the candidate under Test, and the next
  build fails with V4013, which does not name the edit (audit rows 5, 9, 10).
- **The baseline check** (`baselineLogic.ts`, `connection.ts:177`) compares the
  device's `contentHash` with the download record, in TypeScript, and the build
  refuses with E0015 on `mismatch` (`buildCommands.ts:75`). The refusal has no
  remedy command (audit row 15).
- **Records** (ADR-0083, `compiler/project/src/records.rs`): `Build`
  (`<app>.build.iplcinfo`), `Download` (`<app>.<deviceId>.download.iplcinfo`,
  version 2, keyed by the device id), `Path` (`<app>.path.iplcinfo`: chosen
  `deviceId`, `routeHint`, `chosenAt`), `Uids`.
- **Connect to the chosen controller** (`connectControllerLogic.ts`) builds
  information lines when the answering controller is not the one chosen. It never
  refuses on identity (ADR-0084).
- **Old "IronPLC Hot Edit" commands** (`package.json`: `startHotEditSession`,
  `acceptEdits`, `testEdits`, `untestEdits`, `assembleEdits`, `cancelEdits`,
  `showHotEditStatus`, with `hotEdit.ts`) bypass the checks and write no record
  (audit row 16).
- **MCP** hot-edit tools take a `device` and connect like VS Code (`mcp`, PR
  #181). They call the same session commands, so K1 reaches them too.
- **VS Code API in use.** `engines.vscode` is `^1.75.0`
  (`integrations/vscode/package.json:9`).

## Design doc reference

- `specs/design/engineering-connection.md`: the session, Mechanism 3 (Start
  Build, Trial, baseline check, path record). Sections "Trial-run" and "Offline
  edits, online deploy (baseline check)" are superseded by this plan and are
  rewritten in the PRs that deliver the change.
- `specs/design/online-editing-ux.md`: the UX that the dialog replaces.
- ADR-0064 and ADR-0065 (edit phases and session exclusivity), ADR-0061
  (migration), ADR-0073 (state layout), ADR-0083 (records), ADR-0084 (discovery).
- New ADRs, each in the first PR of its class: the retained store (K3), the
  device scan loop and Run/Stop (K4), the edit view and allowed commands (K1),
  the go-online comparison (stage 7). Each records its owner decisions as taken.

## Architecture

### 1. K3: the retained store

- **The container** declares which variables are retained. A new format version
  (`FORMAT_VERSION` 8 to 9, `container/src/header.rs:20`) adds a retained extent
  table beside the persistent extents of ADR-0073. The writer (codegen) fills it
  from the qualifier; no qualifier means no extent. The version check already
  rejects a container of another format (row 15 of the table).
- **The runtime** reads the retained image of the buffers at a scan boundary and
  applies one at start. It is one more reader of the extent function, next to
  `persistent_state` (`carry`, `read`, `write`), never a second layout walker.
- **The device** keeps the retained image in a file beside the slots, written
  with the same fsync discipline as the slot store, tagged with the `contentHash`
  and the retained-layout hash of the application it belongs to. At boot:
  - tag equal: apply the image (a warm start keeps RETAIN and PERSISTENT);
  - tag different, or file damaged: discard it and record a boot note, which the
    identity reports (never silent).
- **Cadence** (see open question 1) is a device setting, a row in the existing
  table of device settings (`vm-cli/src/device_settings.rs`).
- **Running versus persisted identity (audit row 12).** The identity reports the
  `contentHash` that runs and the `contentHash` of the slot on disk. When they
  differ the comparison shows "not persisted". V6012 keeps its code and gains the
  field that makes it visible.
- **The qualifier semantics** (IEC 61131-3): RETAIN survives a warm restart and
  power loss; PERSISTENT (a CODESYS and TwinCAT extension, already in the DSL)
  survives also a download that keeps the layout. Replace mode (decision 2)
  re-initialises everything, as the owner approved, so PERSISTENT across a
  download is not delivered here (see open question 2).

### 2. K4: the cyclic device

- **One owner of the host.** A device loop owns the `RuntimeHost`, the HA shell
  and the slot store. Session threads (stdio, TCP) send a command with a reply
  channel to the loop; the loop executes it between rounds and replies. This
  replaces the long-held mutexes of `tcp.rs:150-170`. No lock is held across a
  round, and the swap stays at the round boundary.
- **The ack means applied.** A command that requests a swap (`testEdits`,
  `untestEdits`, `assembleEdits`) is answered after the boundary that applied it,
  as `drive_scan_round` makes it today. `drive_scan_round` disappears: the next
  round is the boundary.
- **Pacing in one place.** The deadline computation (`next_due_us`, the sleep,
  the stop flag) is one function. `run` and the device loop both call it. The
  executors differ today (`Vm` in `run`, `RuntimeHost` in `serve`); open question
  3 asks whether `run` also moves onto the host.
- **Run and Stop.** The host gains a run state (`Running`, `Stopped`) that the
  identity reports. Two session commands start and stop the scan. A stopped host
  executes no rounds but still answers every command. The device boots in
  `Running` (what an operator of `serve` expects once the loop exists). Run/Stop
  never compiles: the editor's Run and Stop for a connected device send these
  commands. The debug launch (`debugAdapter.ts`) compiles a source file for an
  in-process debug session; it is a different thing and stays.
- **A trap** stops the scan (state `Faulted` of the VM), as `run` does; the loop
  keeps serving commands and reports the fault in the identity.

### 3. K1: the exits of an unfinished edit

- **One transition table.** Each of the five edit operations (`stage`, `test`,
  `untest`, `assemble`, `cancel`) has its guard today as inline `if` chains
  (`host.rs:236-396`). The guards become one table of rows: command, the host
  states it is allowed in, the error code otherwise. The host methods consult the
  table, and so does the K1 function. The two can never disagree.
- **The K1 function** `edit_view(&HostStatus) -> EditView` returns:
  - `stage`: `idle`, `accepted`, `testing`, `testingMigration` (and the
    transient `swapPending`);
  - `commands`: for each of accept, test, untest, assemble, cancel, and
    "finalize all": `enabled`, a `label`, and a `reason` (the V-code and its
    plain text) when not;
  - `next`: the command that the Next item runs.
- **Cancel from Testing** is Untest, then Cancel at the boundary (audit row 7).
  It is one table row with two steps, not a client sequence. Under a migration
  candidate the Cancel row is disabled with its reason (V4011; decided: the
  dialog shows the reason, and the old buffers are not kept; audit row 8).
- **Finalize all** is accept, test, assemble. It is a row of the table
  (`steps: [accept, test, assemble]`) and is enabled when the first step is. The
  client runs the steps in order; the table says which, so the client has no
  sequence of its own.
- **On the wire** the view is an additive optional field `edit` of the status
  payload. The protocol version stays 2 (the precedent: `deviceId`, ADR-0083).
- **Build is not a device command.** The ladder in the dialog starts with Build
  (compile, engineering side) and the device ladder follows. The client holds
  exactly one fact about Build: "a build of the current sources succeeded in this
  dialog". It is the result of `ironplcc compile`, not a rule. Everything that
  follows comes from `edit`.
- **A session end never touches the edit.** A drop of the session, clean or by
  panic, leaves the host state as it is (decision 4). The occupied flag is
  released by a drop guard (audit row 17). A test covers the three ends.
- **Every failure ends in a state whose exits are shown** (audit row 10): after
  a refused or failed command the client re-reads `edit` and renders it.
- **The comments that disagree with the code** (audit row 21: assemble "requests
  a swap" in `serve.rs`; MCP says assemble works from staged; "needs Test") are
  corrected where the table now states the truth.

### 4. K2: deploy modes

- **Three modes, one pipeline.** `change` (layout equal, logic only), `migrate`
  (ADR-0061) and `replace`. The mode is a field of `AcceptEdits` (additive,
  default `change`; the existing selection between logic and migration stays
  automatic from the layout hash). `replace` skips the layout validation, keeps
  nothing, and the boundary swap builds fresh buffers and runs init. The
  container itself is still verified (format, hashes, bytecode verifier).
- **Retained values in replace.** Replace discards the retained image (decision 2,
  empty keep set). The new application's tag starts a new retained file.
- **No application.** The built-in empty container is produced by the container
  crate's builder, with a constant `contentHash`. `ironplcvm serve` without a file
  and with an empty store boots it. "Controller holds no application" is then
  `contentHash == EMPTY_APPLICATION_HASH`, derived and not stored.
- **A replace is allowed in every state** except while a candidate is staged or
  under Test (the K1 table says so), because a replace would drop the candidate.
- **Download is a client action** on the replace mode and writes the download
  record (`ironplcc record-download`, unchanged).

### 5. The go-online comparison

One pure function in the engineering-side crate `ironplc-project`, next to the
records it reads:

```
compare(attempt, device, records, project) -> Comparison
```

- **Inputs**
  - `attempt` A: `ok`, `unreachable`, `busy` (V6014), `protected`.
  - `device`: from the `identity`: `deviceId`, `runtimeVersion`, `contentHash` C,
    `layoutHash` Lc, `persistedContentHash`, `runState`, `containerFormat`,
    `edit` (K1), the boot note.
  - `records`: the download record R for this `deviceId` (hash, layout hash), the
    path record (the `deviceId` last chosen), the build record.
  - `project`: P and Lp from the build record, and one of `upToDate`,
    `changedSinceBuild`, `buildFails` from the ADR-0083 up-to-date check, run
    without compiling.
- **Output** `Comparison`: the controller row (what it is and what it holds), the
  project row, the information rows, the allowed actions with reasons, and
  whether hot edit is allowed. Table row numbers below.
- **Entry points.** A library function for MCP, and an `ironplcc` subcommand that
  prints the result as JSON for the extension. The extension reads JSON and
  renders it; `baselineLogic.ts` (the TypeScript comparison) and E0015 are
  removed.
- **The contents hash is the only comparison.** Restarted counters never matter.

### 6. The QuickPick dialog

A vscode-free logic module maps `edit` (and the one Build fact) to the QuickPick:
title (stage, `step`/`totalSteps`), `busy`, items (Next, Finalize all, Cancel),
the stage marks in the prompt line. The vscode shell sets the widget properties,
runs the command of the selected item, and re-reads `edit`. `ignoreFocusOut` and
a status bar item reopen it; Esc hides it and leaves the candidate where it is.
The modal Trial dialog, `runBuildTrial`, `resolveTrial` and the old Hot Edit
commands are removed in the same stage.

## The go-online table, reconciled with `main`

The 15 rows of the discovery plan stand. What changes is that most inputs now
exist, and the function gets them from records and the identity, not from the
workspace state.

| # | Situation | Input from | Exists on `main` | Delivered in |
|---|---|---|---|---|
| 1 | A unreachable | transport error | yes (connect failure) | stage 7 maps it to the row |
| 2 | A busy | V6014 refusal, graceful close (#178) | yes | stage 7 |
| 3 | A protected | discovery reply flag (`discovery/src/reply.rs`) | the flag, no credentials | stage 7 shows "protected"; credentials are a later class |
| 4 | B none: C = `EMPTY_APPLICATION_HASH` | identity `contentHash` | no empty container | stage 6 (K2), row in stage 7 |
| 5 | C = P, up to date | build record, identity | both | stage 7. Online at once, no dialog |
| 6 | C = P, sources changed | up-to-date check without compile | the check, as part of `compile` | stage 7 (query form) |
| 7 | C = R, C != P, Lc = Lp | download record v2, build record | both | stage 7 |
| 8 | C = R, Lc != Lp | as 7, plus layout hashes | both | stage 7. Migration decisions use ADR-0061 |
| 9 | C != P and C != R | records | both | stage 7. R is looked up by the connected `deviceId`, so another workstation's change lands here (audit row 14 closed) |
| 10 | E staged or under test | `edit` stage, `pendingEdit` | the record, no `edit` view | K1 (stage 5), row in stage 7 |
| 11 | P build fails | up-to-date check | partial | stage 7 |
| 12 | `deviceId` differs from the chosen | path record, identity | both; `connectControllerLogic.ts` already prints it | stage 7 (information row) |
| 13 | connection lost | session state | client | stage 7 |
| 14 | controller restarted | boot note, `persistedContentHash` | no | K3 (stage 2), row in stage 7 |
| 15 | container format refused | `containerFormat` in identity, `FORMAT_VERSION` | no field | stage 6, row in stage 7 |

New, from the audit: **a controller whose `contentHash` and `persistedContentHash`
differ** shows "running code is not persisted" beside any row (audit row 12). **A
boot note "candidate lost"** shows beside any row (candidate loss on restart).

**Allowed actions** are produced for each row, as the discovery plan lists them:
monitor only (Login without change), Login with online change (the Build then
runs through the dialog; the Build item is enabled only where the table says hot
edit is allowed, which replaces E0015), Download (replace), Upload into a new
project (later), search the network, choose another controller, retry, cancel,
resolve the pending edit (opens the dialog at its stage).

**Monitoring a previous download (rows 7, 9, 11)** needs the debug information of
the container that the controller holds. Only the latest `<app>.iplc` is kept.
The approved proposal of the discovery plan stands: Download keeps a copy of the
sent container beside its download record, per `deviceId`. A controller that
holds an application this workstation never sent (row 9) is monitored at device
level only.

## Class / Inv / N+1

**K1: the exits of an unfinished edit.**

- Class: the set of (host state, command) pairs of the hot-edit FSM. A member is a
  state (idle, accepted, accepted-migration, testing, testing-migration, swap
  pending) with the answer for each command. A near case outside: the settings
  command (`SetDeviceSettings`), which is allowed in every state and does not
  touch the edit.
- Inv: every state has at least one command that leaves the unfinished edit
  (an exit), and the allowed set the clients see is the set the host accepts.
  No client computes enablement.
- Check: (a) the transition table is an exhaustive match, so a state or command
  without a row does not compile; (b) a test walks every state, applies every
  command to a fresh host in that state, and compares the result with `edit_view`;
  (c) the same walk writes a golden file that the TypeScript tests of the dialog
  read, so the extension is tested against the runtime's own answers; (d) a test
  fails if the dialog module reads a field other than `edit` and the Build fact.
- N+1: a new command (for example a forced stop of a test) is one row in the
  table, one variant in the command enum, and one label; the host, the view, the
  wire and the dialog follow. A new state is one row of states and its rows of
  commands. Defect: the other sites of the same bug are the other commands'
  guards (rows 7, 8, 9, 10), which the walk covers, and the recurrence guard is the
  walk itself.

**K2: the deploy modes.**

- Class: the ways a candidate becomes the running application: `change`,
  `migrate`, `replace`. Near case outside: the redundancy snapshot apply
  (`apply_state_snapshot`), which copies the state of a peer and is not an
  engineer's deploy.
- Inv: every mode ends in the one boundary swap, and the part of the old state
  that is kept is a function of the mode (everything persistent for `change`, the
  planned copy for `migrate`, nothing for `replace`).
- Check: a mode enum with an exhaustive match in the swap; one test per mode on
  the same fixture, asserting kept and re-initialised variables.
- N+1: a new mode (for example "keep the retained image") is one variant and its
  keep rule.

**K3: device durability.**

- Class: the data a device keeps across a restart and its tag: slots (code), the
  retained image, the device settings, the device id. Near case outside: the
  engineering records (ADR-0083), which the device never reads.
- Inv: a stored item is applied only when its tag equals the application that is
  booting; a mismatch or a damaged item is discarded and reported in the identity,
  never applied and never silent.
- Check: one boot function with an exhaustive result type (`Applied`,
  `Discarded(reason)`, `Absent`) used by all stored items; a test per item for
  each result; the identity-field class (one builder for the device block, the
  discovery reply and the identity) carries the report.
- N+1: a new stored item (for example the forced-variable table) is one tag rule
  and one result mapping.

**K4: cyclic execution of the device.**

- Class: the drivers of the scan round: the offline `run`, the benchmark, the
  device loop. Near case outside: the debug adapter, which steps rounds on
  request.
- Inv: the deadline and stop policy of a free-running scan is one function; the
  device executes whenever it is `Running`, and a command never waits for more
  than one round.
- Check: a test with a fake clock drives the pacer for `run` and for the loop and
  compares the round times; a loop test sends commands between rounds and checks
  the ack-after-boundary rule.
- N+1: a new session transport (USB) sends commands to the same loop and changes
  nothing in it.

**Class "online action":** the actions that the comparison offers (monitor,
online change, download, upload, choose another, retry, resolve). Inv: every
action is a row with a command or a client action and its refusal text; the
comparison and the dialog read the same rows. Check: an exhaustive match, and a
test over the 15 rows of the table. N+1: Upload is one row.

## Prefactoring

Each is its own PR, branched from `main`, behaviour-preserving, with the existing
tests unchanged. A prefactor that only adds an API the feature uses stays in the
feature PR.

1. **P-A, the transition table of the hot-edit guards** (`host.rs`). The guards
   of `stage`, `test`, `untest`, `assemble`, `cancel` become rows. Needed by K1
   and by K2 (the replace row). Several commands branch on the same state, so
   this is the "more than one place" signal.
2. **P-B, one atomic file write** extracted from `slot_store.rs` (`write_fsync`,
   `discard_tmp`) so that the retained store and the slot store share it. The
   copy-and-edit signal. Needed by K3.
3. **P-C, one pacing function** for `run` and the device loop (`cli.rs:46-103`).
   Needed by K4. Dead code alone is avoided because `run` calls it.
4. **P-D, the device block builder** already exists for `deviceId`; the new
   identity fields (`persistedContentHash`, `runState`, `containerFormat`, boot
   note) go through it. No prefactor, a note for the stage.
5. **P-E, glossary:** "retained", "persistent part", "run state" and "stage"
   defined in `specs/steering/glossary.md`, so the two meanings of "persistent"
   do not meet in code or docs. A docs-only PR.

## File map

| Area | Files | Change |
|---|---|---|
| Container | `compiler/container/src/header.rs`, `persistent_extent.rs`, a new `retained_extent.rs`, the builder | format 9, retained extents, empty container |
| Codegen | `compiler/codegen/src` (variable table writer) | fill retained extents from the qualifier |
| Runtime | `compiler/runtime/src/host.rs`, `commands.rs`, `persistent_state.rs`, a new `edit_view.rs`, a new `retained_state.rs` | table, view, run state, replace mode, new payload fields |
| Device | `compiler/vm-cli/src/serve.rs`, `tcp.rs`, `cli.rs`, `slot_store.rs`, `device_settings.rs`, a new `retained_store.rs`, a new `device_loop.rs` | loop, store, drop guard, empty boot |
| Engineering | `compiler/project/src`, a new `online_comparison.rs`; `compiler/ironplc-cli` (subcommand) | the comparison |
| MCP | `compiler/mcp/src/tools/hot_edit.rs` | read `edit`, start and stop |
| Extension | `integrations/vscode/src/onlineEditLogic.ts`, `onlineEditCommands.ts`; `buildLogic.ts`, `buildCommands.ts`, `baselineLogic.ts`, `hotEdit.ts`, `package.json` | dialog in, Trial and old Hot Edit and E0015 out |
| Docs | `docs/` problem pages for new V codes, `specs/design/engineering-connection.md`, `online-editing-ux.md`, four ADRs, glossary | per stage |

## Stages (each its own PR or PRs, in the owner's order)

Tracking issue: to be opened when this plan is approved (the standards require
one when more than one core change PR follows), and linked here.

### Stage 1: prefactors

- [ ] P-E glossary.
- [ ] P-B one atomic write shared by `slot_store.rs`.
- [ ] P-C one pacing function used by `run`.
- [ ] P-A the transition table of the hot-edit guards.

### Stage 2: K3, the retained store

- [ ] 2a ADR "retained store"; container format 9 with retained extents; codegen
      fills them; verifier rule; docs of the format. Row 15 message names both
      format versions.
- [ ] 2b runtime: read and apply the retained image next to `persistent_state`;
      tests on the swap (retained image carried in `change`).
- [ ] 2c device: the retained file, tag, cadence setting, boot result type and
      boot note; `persistedContentHash` and the boot note in the identity (through
      the one device block builder; discovery stays identity only).
- [ ] 2d V6012 made visible: the comparison field and a docs page update.

### Stage 3: K4, the cyclic device

- [ ] 3a ADR "device scan loop and Run/Stop".
- [ ] 3b the device loop owns the host; session threads send commands; the ack
      after boundary rule; `drive_scan_round` removed; stdio and TCP both.
- [ ] 3c run state in the host, two session commands, `runState` in the
      identity; MCP and the extension get Run and Stop that compile nothing.
- [ ] 3d HA shell composes on the loop (the pair keeps its permit and commit
      callbacks); the HA tests run unchanged.

### Stage 4: K1, the exits, with the protocol for allowed commands

- [ ] 4a ADR "edit view and allowed commands"; `edit_view.rs`; `edit` in the
      status payload; the walk test; the golden file.
- [ ] 4b defects fixed in the class: Cancel from Testing (row 7), migration
      Cancel disabled with reason (row 8), V4013 names the edit (rows 5, 9), the
      drop guard for the occupied flag (row 17), the comments (row 21), session
      end leaves the edit (decision 4) with its test.
- [ ] 4c MCP tools read `edit` and refuse with its reason.

### Stage 5: the QuickPick dialog

- [ ] 5a `onlineEditLogic.ts` and the shell; tests against the golden file.
- [ ] 5b remove the modal Trial dialog, `runBuildTrial`, `resolveTrial` and the
      old Hot Edit commands (audit row 16), each only in its own entries. Raise
      `engines.vscode` only if a used API needs it.
- [ ] 5c docs and the design doc.

### Stage 6: K2, deploy modes (placed before the comparison, see open question 4)

- [ ] 6a the empty container; `serve` without a file boots it.
- [ ] 6b `replace` mode in `AcceptEdits` and the swap; V4007, V4008, V4009 get a
      path.
- [ ] 6c Download as an extension action; the sent container is kept beside the
      download record.
- [ ] 6d `containerFormat` in the identity.

### Stage 7: the go-online comparison

- [ ] 7a ADR "go-online comparison"; `online_comparison.rs`; the table test over
      all 15 rows plus the new rows.
- [ ] 7b the `ironplcc` subcommand; the up-to-date check in query form.
- [ ] 7c the view in the extension; Connect shows it (open question 5); E0015 and
      `baselineLogic.ts` removed; the information row for another controller.
- [ ] 7d MCP uses the library function.
- [ ] 7e close-out: update the design docs, open issues for what is not
      delivered, close this plan PR unmerged.

## Not included / next plans

- **Output-off test** (the SLC 500 and PLC-5 Remote Test, where logic is solved
  and outputs stay off): a later class, with I/O.
- **The source archive downloaded into the PLC with the application, Upload
  creating a project from it, and project templates**: the next plan.
- **Wire authentication and credentials** (row 3): ADR-0063 stays deferred.
- **Cold and warm reset commands, boot mode setting** (run or stop at boot): with
  Run/Stop's next step.
- **HA pair changes** (audit row 19: the peer drops the migration decisions, a
  Test lost to permit revocation is acknowledged): the HA plan. Stage 3d only
  keeps what works.
- **Large containers** (audit row 20: about 5 MB limit by JSON framing): binary
  framing or chunks.
- **Change detection** (Rockwell Audit Value): a later class.
- **Firmware update**: no firmware concept; the version is the runtime version.
- **A match-the-application-to-controller option** (Rockwell's Match Project to
  Controller): later, per application.

## Open questions for the owner

1. **Cadence of the retained write.** Writing at every scan wears the disk and
   costs time; writing only at a controlled stop loses everything on a power cut.
   - A: at a controlled stop and at assemble only.
   - B: A, plus every N seconds when the image changed (N a device setting).
   - C: at every scan boundary.
   - Recommendation: B, default 1 second. A soft PLC on a PC has no hold-up
     power, so a bounded loss is the honest guarantee, and the setting lets a
     hardware target choose.
2. **PERSISTENT across a Download.** Decision 2 makes replace re-initialise
   everything. CODESYS keeps PERSISTENT variables across a download when their
   layout is equal.
   - A: Download re-initialises all, PERSISTENT too (as decided, nothing extra).
   - B: Download keeps variables qualified PERSISTENT whose declaration is equal.
   - Recommendation: A in this plan, with B as a later keep rule of the K2
     class (one variant). B needs the UID table of ADR-0053 to match variables.
3. **Does `run` move onto the host?** The pacer is shared in any case.
   - A: `run` keeps `Vm` and shares only the pacer. Two executors remain.
   - B: `run` is rebuilt on `RuntimeHost`, so there is one loop and one executor,
     as the audit asked.
   - Recommendation: B, if a release benchmark against `main` shows no cost
     (the VM split cost 7 % once); otherwise A, with the reason written at the
     site.
4. **Where K2 sits in the order.** Your order lists K3, K4, K1, the dialog, then
   the comparison, and K2 (replace, empty container) is needed by the comparison's
   Download actions.
   - A: K2 between the dialog and the comparison (this plan).
   - B: K2 right after K1, so the dialog already has its Cancel and Download
     paths.
   - Recommendation: A; the dialog needs no replace mode.
5. **How the comparison is shown on Connect.**
   - A: row 5 connects at once with no dialog. Every other row opens a
     QuickPick, in the same style as the edit dialog, listing the controller,
     what it holds, the project, and the actions.
   - B: a permanent "Online" view (a panel) that always shows the comparison, and
     no dialog.
   - Recommendation: A, with the status bar showing the row's one-line summary at
     all times, so B's information is never more than a click away.
6. **A candidate when the device restarts.** Today it is lost (RAM). After K3 the
   device could keep it.
   - A: the loss is reported (the boot note "candidate lost"), and the
     engineer repeats the edit.
   - B: the candidate and its record are persisted, and the device returns to
     Accepted (never to Testing) after a restart.
   - Recommendation: A. Code that has not been assembled should not boot, and
     persisting it adds a store that the K3 tag rule would have to cover.
