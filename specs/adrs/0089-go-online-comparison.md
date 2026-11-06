# The Go-Online Comparison Is One Function, and Connect Never Refuses

status: accepted
date: 2026-11-07

## Context and Problem Statement

An engineer goes online to a controller that the project may or may not have
been loaded to. What follows depends on what the controller holds against what
the project has: watch it, change it online, replace it, or look for another
controller. Until now the editor decided part of this by itself. `baselineLogic.ts`
compared the content hash of the controller with the download record
([ADR-0083](0083-build-and-download-records.md)) and, on a difference, refused
with E0015. That is the wrong shape for three reasons.

- **It refused.** The project is the master and a controller is any controller
  the engineer chooses ([ADR-0084](0084-plc-discovery-and-controller-selection.md)):
  nothing refuses a controller for being a different one. A refusal also left the
  engineer without the one thing they might want, which is to watch the
  controller.
- **It saw one case.** The comparison knew "match", "mismatch" and "no record".
  It did not know that the controller holds no application, that the project's
  sources changed since the last build, that the project does not build, that an
  edit is unfinished on the controller, or that the controller refuses the
  container format.
- **It lived in one client.** The decision was TypeScript in the extension. The
  MCP server and the command line had no way to ask it, and each client that
  needed an answer would have written a second copy.

CODESYS and Rockwell show the shape to follow: after the login the engineer sees
what the controller holds against the project and chooses the action
("Connected To Go Online" in Studio 5000, the login choices of CODESYS). The
vendors show that prompt only when the two differ.

What does the engineering side say about a controller that has answered, and who
says it?

## Decision Drivers

* **Connect never refuses.** The comparison informs; the engineer decides. A
  controller that is not the one last chosen is an information row.
* **Content, not counters.** The `contentHash` is the only comparison. The
  generation counters of a controller restart when it boots and are never read.
* **One decision, many clients.** The editor, the command line and the MCP server
  render one answer; none of them computes what an action is worth.
* **The actions are a class.** Monitor, online change, download, upload, search,
  choose another, retry and resolve each have an enablement rule and a refusal
  text. A new action is one row, and the dialog reads the same rows as the
  comparison.
* **Every state of the controller has a place.** Stopped, faulted, not persisted,
  a lost candidate, an edit in progress and a refused container format appear
  beside whatever the headline is.

## Considered Options

* **Keep the TypeScript comparison and add rows to it.** The MCP server and the
  command line stay without an answer, and a second copy is the next step.
* **One pure function in the engineering-side crate, with entry points for the
  command line and MCP (chosen).** `ironplc-project` already holds the records
  and the up-to-date check that the function reads.
* **Put the comparison on the controller.** The controller does not know the
  project, its sources, its build record or its download records.

## Decision Outcome

Chosen: **`ironplc_project::online_comparison::compare`**, a pure function.

```
compare(attempt, records, project) -> Comparison
```

### The inputs

* **`attempt`**, the answer to the attempt to connect:
  `Ok(device)`, `Unreachable`, `Busy` (V6014), `Protected` and `ConnectionLost`.
  The device is the payload of `Ok`, so a comparison of a controller that was not
  reached cannot have one. `ConnectionLost` is the session that ended, which is
  not the same row as a connect that never opened. `Protected` is the flag of the
  discovery reply; the credentials are not part of this decision.
* **The device** (`DeviceFacts`), read from the `identity` answer:
  `deviceId`, `runtimeVersion`, the `contentHash` C and `layoutHash` Lc of
  `application`, `persistedContentHash`, `runState`, `fault`, `containerFormat`,
  `edit` (the stage, and the refusal of the `accept` command when the controller
  gives one), the boot note `retained` and `candidateLost`. `ironplc-project` does
  not depend on the runtime, so the reader works on the JSON of the answer and
  names only the fields it uses; a field that is present and not in its form
  makes the answer unreadable, and a field that is absent is absent (a device that
  predates it). The editor passes the JSON it received to `ironplcc online-compare`.
  The MCP server serializes the `IdentityPayload` it received, so the wire form is
  the one input of both, and a test in `ironplc-mcp` reads each stage and run state
  of the runtime through it.
* **`records`**: the download record R of the connected controller (looked up by
  the `deviceId` the controller reports; a record of another device is ignored),
  whether the container that was sent is kept beside it, and the path record
  (the `deviceId` last chosen).
* **`project`** (`ProjectState`): the last build (P and Lp, from the build record)
  and one of the answers of the up-to-date check of ADR-0083 run without
  compiling: `UpToDate`, `ChangedSinceBuild`, `BuildFails`, and `NeverBuilt` for a
  project with no usable build record. The fourth answer is added to the three of
  the check because a project that was never built has no P, and an absent P is
  not an up-to-date or a changed one.

### The output

`Comparison`: the headline `row` with its number, a one-line `summary` for the
status bar, `connected`, `atOnce` (the editor goes online with no dialog: only the
`upToDate` row), the `controller` row (what it is and what it holds: nothing, an
application by its hashes, or unknown), the `project` row, the `lastDownload`, the
`information` rows, every `action` with its `enabled` state and `reason`, and
`hotEdit`. The JSON is camelCase and is the same object from the command line,
the MCP tool and the library.

### The headline and the information rows

The go-online table has 15 situations. They are not alternatives: some say how
the controller's content relates to the project, and some stand beside any such
relation. The headline (`Row`) is the first kind, one per comparison; the
information rows (`NoteKind`) are the second kind, any number. The table, with
the form each row takes as built:

| # | Situation | Form | Kind or row |
|---|---|---|---|
| 1 | A unreachable | headline | `unreachable` |
| 2 | A busy (V6014) | headline | `busy` |
| 3 | A protected | headline | `protected`: shown, and nothing more |
| 4 | C is the empty application | headline | `noApplication` |
| 5 | C = P, up to date | headline | `upToDate`: online at once, no dialog |
| 6 | C = P, sources changed | headline | `sourcesChanged` |
| 7 | C = R, C is not P, Lc = Lp | headline | `previousDownload` |
| 8 | C = R, Lc is not Lp | headline | `previousDownloadOtherLayout` |
| 9 | C is neither P nor R | headline | `otherApplication` |
| 10 | An edit is unfinished on the controller | information | `editInProgress` |
| 11 | P does not build | information | `buildFails` |
| 12 | The `deviceId` is not the one last chosen | information | `otherController` |
| 13 | The connection was lost | headline | `connectionLost` |
| 14 | The controller restarted | information | `notPersisted`, `candidateLost`, `retainedDiscarded` |
| 15 | The container format is refused | information | `containerFormatRefused` |

Plus the conditions the table adds as information rows: `stopped`, `faulted`,
`noDeviceId` (the controller reports no id, so no record is kept for it) and
`contentNotReported` (a device that predates the hash).

The headline is chosen in this order. `attempt` first. Then C against the empty
application (row 4), against P (rows 5 and 6: up to date only when the project is
`UpToDate`; a project that changed or does not build is `sourcesChanged`), against
R (rows 7 and 8; the layout is Lc of the controller, or the record's when the
controller reports none, against Lp; with no P the layout is the project's to
decide and the row is `previousDownload`), and otherwise row 9. A controller that
reports no content hash is row 9 with `contentNotReported`: it is compared with
nothing, as the E0015 check treated it.

A restart (row 14) changes no comparison, because the counters are not read. What
it shows is what the controller says about it: that the running code differs from
the code it would boot (`persistedContentHash`), that a restart lost an unfinished
edit (the boot note `candidateLost`), and that the stored retained values were
discarded at boot (the `retained` note). A controller that is faulted, or stopped,
is information too: edit commands are answered, and applied at the boundary
without executing the logic (ADR-0086).

### The contents hash is the only comparison

C, P, R and the empty application are compared by `contentHash`; Lc and Lp, by
`layoutHash`, only to choose between rows 7 and 8. The generation counters are
not inputs, so a restarted controller compares as before.

### The actions

`ActionKind` is the class of the actions. Each member has one enablement rule that
gives the refusal and its text, and the comparison lists every action, enabled or
not, so a dialog shows a disabled one with its reason and reads nothing else. The
class is declared once (a member names itself and its rule; a member without a
rule does not compile), as `NoteKind` is.

| Action | Enabled when | Otherwise |
|---|---|---|
| `monitor` | the attempt reached the controller | `notConnected` |
| `onlineChange` | reached; the container format is accepted; the project builds (not `buildFails`); the controller holds the last build or the last download (rows 5 to 8); the controller accepts an edit | `notConnected`, `containerFormat`, `projectDoesNotBuild`, `noApplicationToChange` (row 4), `otherApplication` (row 9), or the controller's own refusal with its V-code |
| `download` | reached; the container format is accepted; the project builds; the controller accepts an edit | as above, except that the content of the controller does not matter |
| `upload` | never: Upload into a new project is not built | `notAvailable` |
| `search`, `chooseAnother` | always | |
| `retry` | `unreachable`, `busy`, `connectionLost` | `alreadyConnected`, or `protected` for a protected controller |
| `resolve` | the controller reports an edit stage other than idle | `noPendingEdit`, `notConnected` |

`hotEdit.allowed` is the answer of `onlineChange`, evaluated once, so the Build
item of the dialog and the action cannot disagree. This replaces E0015: the
refusal is no longer made by the check but by a row, and it comes with the other
row it needs, which is `download`. The controller's own refusal to accept an edit
(a candidate is staged) is passed through as given, with its V-code and text, so
the comparison has no second copy of the table of ADR-0087.

Monitor says what it can show. When the controller holds the last build, the
container with its debug information is the build on disk, and the variables are
shown by name. When it holds the last download and the container that was sent is
kept beside the record (ADR-0088), the same. Otherwise, as in row 9, the
monitor is at the level of the device.

Every action is a client action: each is a sequence (the edit dialog, a build and
a download, a search of the network), so the name of the action is the identifier
the client runs. No action is one session command.

### What the comparison does not do

* **Upload** is a member of the class with no rule that enables it. The class has
  the place for it, and adding it is one rule.
* **Credentials.** Row 3 shows "protected"; a protected controller cannot be
  connected until credentials exist (ADR-0063 stays deferred).
* **It does not connect.** The caller makes the attempt and says how it went.
* **It does not decide for the engineer.** An information row never turns an
  action off. Only a fact that makes an action unable to succeed does (the
  format the controller refuses, a project that does not build, an edit in
  progress, a content that an online change cannot continue).

### The entry points

* **Library**: `online_comparison::compare`, for the MCP server and any later
  client.
* **Command line**: `ironplcc online-compare` takes the sources and the
  application container as `compile` does, `--attempt` (`ok`, `unreachable`,
  `busy`, `protected`, `connection-lost`) and `--identity FILE` (`-` for standard
  input), and prints the `Comparison` as JSON. The state of the project is the
  up-to-date check of ADR-0083 run as a query: the sources are read and compared
  with the build record without building. Only when they differ are they analyzed,
  as `check` analyzes them, to find whether they build; code generation is not
  run, so a construct only the code generator refuses shows only when the build
  runs. The `compile` command asks the same query to decide whether to skip.
* **MCP**: the tool `online_compare` takes `device` and, as the records of the
  editor are not available to a server that reads no file of the project
  (REQ-STL-mcp-006), the project as `sources` and `options`. The compiled
  container is the build, up to date by construction. It has no download record
  and no path record: a controller that holds anything but the sources is
  `otherApplication`, and rows 7, 8 and 12 do not occur.

### Differences from the TypeScript comparison

`baselineLogic.ts` answers "no record: no check". The comparison does not. A
controller that holds neither the last build nor the last download is row 9 and
an online change is not offered, whether or not a record exists, because without
a record nothing says that this workstation knows what the controller holds.
Download is offered. The first online change over a controller that this
workstation has not loaded is therefore a Download, which is also what the
download record then makes the next online change possible from.

## Consequences

* Good, because one function decides, and the editor, the command line and the
  MCP server render the same answer.
* Good, because connect never refuses, and the engineer can always watch the
  controller.
* Good, because the states that had no place (empty controller, project that
  does not build, an edit in progress, a refused container format, a restart that
  lost a candidate) have rows, and a state added later is a row.
* Good, because the editor loses a copy of the decision, and E0015 with it.
* Bad, because a controller that holds neither the last build nor the last
  download, with no record at all, no longer takes an online change, where the old
  check let it through. Download is the way in, and it writes the record.
* Bad, because the MCP server compares without records: an agent cannot see
  "previous download" or "other controller" through it.
* Bad, because `DeviceFacts` reads the identity in a second place from the
  runtime's `IdentityPayload`. The wire form is the contract, and the test over
  the runtime's stages and run states in `ironplc-mcp` keeps the two from drifting.
* Neutral: "up to date" is the content of the last build and its container, as
  the build record states it. A change to a library on disk that the record does
  not see is the limit that ADR-0083 states for the skip.

## Confirmation

* `compiler/project/src/online_comparison/tests.rs`: one case for each row of the
  table, named after the row (the three rows of 14 separately), the states the
  table adds (stopped, faulted, no device id, content not reported, never built,
  a record of another device), the property over every combination of what the
  controller holds, the project state, a download record, a staged edit and a
  refused format that an attempt that reached the controller always connects with
  Monitor enabled, the serialization, and the reading of the identity (a full
  answer, a device that predates the fields, a field present and wrong).
* `compiler/ironplc-cli/tests/cli_online_compare.rs`: the command as the editor
  runs it, for each state of the project (up to date, changed, never built, does
  not build), the records, standard input, the attempts that did not reach the
  controller, and the errors.
* `compiler/mcp/src/tools/online_compare/tests.rs`: the tool on a device that
  runs the compiled sources, other code, no sources, sources that do not build,
  nothing listening, a busy device, back-to-back calls, an unresolved device, and
  the mirror of the runtime's stages and run states.
