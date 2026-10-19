# Design: Message Ontology

status: partially implemented
date: 2026-10-17

## Overview

This document defines what a compiler message is in IronPLC, what properties it
has and which rules govern it. The reference is the message system of CODESYS
V3.5.22. Each concept below states the CODESYS model, with a pointer to the
decompiled source in `Codesys/decompiled/`, and IronPLC's counterpart.

The goal is parity: every observable behaviour of the CODESYS message system has
an item in the [parity checklist](#parity-checklist), with its state in IronPLC.
Where the owner chose to differ from CODESYS, the item says so.

Source names are short. They refer to files under `Codesys/decompiled/`:
- `-.352.cs` is `Compiler35220.plugin/-/-.352.cs`;
- `Messages.cs` and `ErrorVisitor.cs` are in `Compiler35220.plugin/_3S/CoDeSys/Compiler35220/Messaging/`;
- `WarningHelper.cs`, `CompilerMessage.cs` and `PreCompileContext.cs` are in `LanguageModelManager.plugin/_3S/CoDeSys/LanguageModelManager/`.

A name that is not unique is found by searching the decompiled tree for it.

The categories of a message are specified in
[Diagnostic Severity](diagnostic-severity.md) and decided in
[ADR-0081](../adrs/0081-a-diagnostic-takes-its-category-from-its-problem-code.md).
This document places them among the other concepts.

### Requirements and the checklist

Each checklist item has a stable number `MSG-NNN`, which never changes or gets
reused:
- an item that IronPLC implements names the requirement (`REQ-…`) whose test
  proves it;
- an item that is not implemented yet becomes a requirement, with a test, in the
  change that implements it. That change also updates its status here.

A requirement without a test fails `all_spec_requirements_have_tests`. The
checklist therefore holds the unimplemented items, and the requirements hold
only what is built.

## Concepts

| Concept | One line | Section |
|---|---|---|
| Message | One report: where, what, how severe, which code | [Message](#message) |
| Category | How severe a message is; 7 values | [Category](#category) |
| Message area | Which list of messages a message belongs to (Build, Precompile, …) | [Message area](#message-area) |
| Code | The identity of a message: prefix and number | [Code](#code) |
| Handling | What happens to a message: report, suppress, ignore | [Handling](#handling) |
| Suppression source | Who asks for a message to be suppressed | [Suppression sources](#suppression-sources) |
| Raising | A warning reported as an error | [Raising](#raising) |
| Limit | How many messages of a category are kept | [Limits](#limits) |
| Related position | A secondary place attached to a message | [Related positions](#related-positions) |
| Aggregation | The order of messages, deduplication, summary lines | [Aggregation](#aggregation) |
| User message | A message written in the source by a pragma | [User messages](#user-messages) |
| Message forms | How a message is shown to a person and given to a program | [Message forms](#message-forms) |

## Message

A CODESYS message (`_ICompilerMessage`, `MessageStorage/IMessage*.cs`) has these
fields:
- `ProjectHandle`, `ObjectGuid`;
- `Position`, `PositionOffset`, `Length`;
- `Text`;
- `Severity`, the category;
- `MessageId`, which gives `Prefix` and `Number`;
- `ShowAttribute`, the areas in which the message is shown.

A message without a source position has the position -1 and is never shown in
Precompile.

In IronPLC a message is a `Diagnostic` (`compiler/dsl/src/diagnostic.rs`). It has
a primary label (file and span), a code, a category taken from the code,
secondary labels, described values and help notes.

Differences:
- A `Diagnostic` always has a code. CODESYS has messages without one: user
  pragma messages, limit lines, summary lines (MSG-040).
- A `Diagnostic` has no areas yet (MSG-020).

## Category

There are seven categories, the values of the CODESYS `Severity` flag set
(`MessageStorage/Severity.cs`): `FatalError` 1, `Error` 2, `Warning` 4,
`Information` 8, `Text` 16, `SuppressedWarning` 32, `SuppressedInformation` 64.

- An `Error` or a `FatalError` stops the build. The other categories do not, so a
  build can succeed with warnings.
- A `FatalError` clears the messages already stored in its area and drops every
  later message of the run (`Compiler35220.plugin/-/-.352.cs:93-130`).
- The two suppressed categories are never declared. A message gets one only by a
  suppression, and it is not shown.

In CODESYS the category is chosen where the message is raised, not by the key
prefix: `Wrn_LibraryNotInstalled` is raised as `FatalError` (`Messaging/Messages.cs:396`).
In IronPLC each problem code declares its category in `problem-codes.csv`, and a
code that needs another category is another code. The key prefix matters in
CODESYS only for which messages the project settings can change: those named
`Wrn_`. In IronPLC these are the codes declared `Warning`.

The category is implemented in full; see [Diagnostic Severity](diagnostic-severity.md).

## Message area

CODESYS keeps messages in separate lists, its message categories
(`IMessageCategory`). The Messages view has a drop-down that selects one list,
and each list shows only its own messages:

| Area | CODESYS class | Written by |
|---|---|---|
| Build | `CompilerMessageCategory` | a compile or a build, with the build start and summary lines |
| Precompile | `PreCompileMessageCategory` | the background check while editing |
| Compile context | `CompileContextMessageCategory` | loading the compile information |

The areas are an open set: a component adds its own, for example static
analysis. Each message carries `ShowAttribute` (`None`, `Precompile`, `Compile`,
`All`), which says the areas it is shown in.

In IronPLC the areas are a class whose members declare themselves (MSG-020):
- `check` and `compile` write the Build area;
- the language server's analysis while editing writes the Precompile area;
- a new area is one new member.

In the editor, the area is the `source` of a diagnostic (MSG-021).

## Code

A CODESYS code is the prefix `C` and the four-digit number of the `MessageId`, for
example `C0037` (`Messaging/ErrorVisitor.cs:110`). The ids run from 0 to 591;
there are 507 named ones, and 0 is `None`, a message without a code. The key
prefixes `Err_`, `Wrn_`, `Inf_` and `Txt_` are a naming convention.

An IronPLC code is `P` and four digits, declared in `problem-codes.csv`.
- A `Codesys` column records the CODESYS code that reports the same condition,
  where the decompiled checker confirms it (MSG-041). A code that is close but
  not the same stays empty.
- Both message forms show it (MSG-042): the header of a person is
  `Error[P4007] (CODESYS C0046)`, and a program reads it as the field `codesys`.

## Handling

CODESYS decides each message with `MessageHandling`
(`PreCompile/Typification/MessageHandling.cs`):

| Value | Effect on a warning | Effect on an error |
|---|---|---|
| `Report` | stored as a warning | stored as an error |
| `Suppress` | stored as `SuppressedWarning`, not shown | dropped |
| `Ignore` | dropped | dropped |

IronPLC has `Diagnostic::suppress()` (REQ-DS-dsl-004). `Ignore` and the per-message
decision are not built yet (MSG-030).

## Suppression sources

Suppression has two kinds of source, and they are separate mechanisms that never
mix:

1. **Settings,** given outside the source text: the project's compiler-warning
   settings, and the OEM settings.
   - `DisabledWarningIds` and `WarningAsErrorIds` are lists of ids
     (`LanguageModelManager.plugin/.../WarningHelper.cs`).
   - In IronPLC they are command-line and project options, readable by a
     program (MSG-031).
2. **Source text:** the pragmas `{warning disable Cnnnn}` and
   `{warning restore Cnnnn}`, and the attributes `suppress_warning_N` on a
   variable or a POU and `suppress_messages` on a POU (`Messaging/Messages.cs`,
   `Messaging/ErrorVisitor.cs`). They are read by the parser and the analysis
   (MSG-032 to MSG-035).

Neither kind changes the other. Both act only on warnings and information. An
error cannot be suppressed by either.

## Raising

A warning whose id is in `WarningAsErrorIds` is reported as an `Error` with the
same id (`Tools/SeverityExtensions.cs`). Raising beats disabling: an id in both
lists is an error (`WarningHelper.cs:42-51`). Nothing lowers an error. IronPLC:
MSG-036.

## Limits

CODESYS keeps at most 500 errors, a hard-coded limit (`-.352.cs:206-215`). At the
501st it stores "More than 500 errors occured: Skipping all further error
messages" as an `Error` without a code.

Warnings stop at `MaxCompilerWarnings`, default 100, which the user may set to
no limit. At the limit CODESYS stores the same kind of line as a `Warning`.

The Precompile area keeps at most 950 messages per POU, dropping the oldest
first.

**IronPLC differs here, by the owner's decision:** the warning limit is 500, the
same as the error limit (MSG-050). The texts are those of CODESYS, including the
spelling "occured" (MSG-051).

## Related positions

A related position is an `Information` message, "Related position"
(`Inf_RelatedPosition`, C0181), added after the message it belongs to.
Other forms are a call chain, "Called '{0}'.", and a stack use,
"'{0}' using {1} bytes of stack." Related positions are dropped once the error
limit is passed.

IronPLC writes them as secondary labels of the one `Diagnostic`. The program
form gives them as messages of their own, in CODESYS order (MSG-060).

## Aggregation

CODESYS stores messages in the order they are produced, without sorting
(`Messaging/Messages.cs:28-43`). It drops duplicates: the first message with the
same id, position, text, category and object wins.

A build writes "------ Build started: Application: {0} -------" first, and ends
with a summary:
- "Build complete -- {0} errors, {1} warnings : Ready for download", or
  "… : No download possible" when an error stops the build;
- a compile without code generation ends with
  "Compile complete -- {0} errors, {1} warnings".

The counts are those of the run, so the summary shows the state of the build
pipeline as it ends. `FatalError` and `Error` count as errors; only `Warning`
counts as a warning. IronPLC: MSG-070 to MSG-074.

## User messages

A pragma in the source creates a message (`Parser35220.plugin/.../ErrorPragmaParser.cs`):

| Pragma | Category | Code |
|---|---|---|
| `{error 'text'}` | `Error` | none |
| `{fatalerror 'text'}` | `FatalError` | none |
| `{warning 'text'}` | `Warning` | `C0373` (`Wrn_Pragma`) |
| `{info 'text'}` | `Information` | none |
| `{text 'text'}` | `Text` | none |

Options after the text:
- `, Length := n` sets the length of the message;
- `, messageguid 'GUID'` sets its object;
- `show_compile` and `show_precompile` choose its areas.

IronPLC skips these pragmas today: MSG-080 to MSG-083.

## Message forms

A message has two forms, both generated from the one list of categories:

- **For a person:** the command line and the editor. They use the words of the
  CODESYS Messages view:
  - the header is `Error` (for a fatal error too), `Warning` or `Information`;
  - a `Text` line has no word and no code;
  - a suppressed message is not shown.

  In VS Code, the Problems panel filters Errors, Warnings and Infos, the same
  three switches as the CODESYS Messages view. A `Text` goes to the Output
  channel, not to the Problems panel: the language server sends it as a
  `window/logMessage` of type `INFO`.
- **For a program:** MCP and the JSON output of `ironplcc check --format json`.
  They use the CODESYS scripting model:
  - the fields of `ScriptMessage` that a diagnostic has (`severity`,
    `severity_value`, `prefix`, `number`, `text`, `position`, `length`,
    `position_text`, `object`), and `codesys`, which CODESYS does not have;
  - the category names `FatalError` … `SuppressedInformation`, with the flag
    value of each (`severity_value`);
  - suppressed messages are included;
  - two queries, as the scripting API has: `get_message_objects` returns the
    message objects of the categories in a mask, and `get_messages` returns the
    texts alone.

The words, names and values of the categories come from the one list of
categories, not from a literal of a form. MSG-090 to MSG-095.

## Parity checklist

States:
- **done:** implemented and proven by the requirement named;
- **planned:** to be built;
- **owner:** IronPLC differs from CODESYS by decision;
- **live:** needs a check in a running CODESYS;
- **n/a:** no counterpart in IronPLC.

| Item | Behaviour | CODESYS evidence | State |
|---|---|---|---|
| MSG-001 | There are seven categories, in the CODESYS order and with the CODESYS names | `Severity.cs` | done (REQ-DS-problems-002, -003) |
| MSG-002 | `Error` and `FatalError` stop the build; the others do not | `Messages.cs:50` | done (REQ-DS-dsl-002, REQ-DS-project-001) |
| MSG-003 | A `FatalError` clears the earlier messages and drops the later ones | `-.352.cs:93-130` | done (REQ-DS-dsl-005, REQ-DS-project-002) |
| MSG-004 | A suppressed message is never shown to a person | `-.352.cs:148-151` | done (REQ-DS-playground-002) |
| MSG-005 | Each code declares its category in one place | — | done (REQ-DS-problems-001, -004) |
| MSG-020 | Messages belong to areas (Build, Precompile, …), an open class; each area lists only its own messages | `CompilerMessageCategory.cs`, `PreCompileMessageCategory.cs` | planned |
| MSG-021 | The editor shows the area as the `source` of a diagnostic, so the Problems filter can select it | — | planned |
| MSG-022 | A message without a source position is not shown in Precompile | `CompilerMessage.cs:38,63` | planned |
| MSG-030 | Handling `Report` / `Suppress` / `Ignore`; an error is dropped by `Suppress` or `Ignore` | `MessageHandling.cs`, `-.278.cs:98-103` | planned |
| MSG-031 | The settings lists of disabled warnings and warnings as errors are given outside the source, as options | `WarningHelper.cs` | planned |
| MSG-032 | `{warning disable Cnnnn}` makes a matching warning suppressed until `{warning restore Cnnnn}` | `ErrorVisitor.cs:108-115` | planned |
| MSG-033 | The scope of `{warning disable}`, and whether the code is case-sensitive | — | live |
| MSG-034 | `suppress_warning_N := 'Cnnnn'` on a variable or a POU suppresses that warning | `Messages.cs:286-300,529-540` | planned |
| MSG-035 | `suppress_messages` on a POU drops all its messages | `Messages.cs:141` | planned |
| MSG-036 | A warning listed as an error is an `Error` with the same code; this beats a disabled one; nothing lowers an error | `SeverityExtensions.cs`, `WarningHelper.cs:42-51` | planned |
| MSG-037 | Only a warning code (`Warning` category) can be disabled or raised | `WarningHelper.cs:185-200` | planned |
| MSG-040 | A message may have no code (user pragma, limit line, summary line) | `CompilerMessage.cs:285-295` | planned |
| MSG-041 | A problem code records its CODESYS counterpart | — | done (REQ-DS-problems-005, -006, -007) |
| MSG-042 | Both forms show the CODESYS counterpart | — | done (REQ-DS-dsl-006, -007, -011, REQ-DS-cli-001, REQ-DS-mcp-004) |
| MSG-050 | At most 500 errors and 500 warnings are kept (CODESYS: warnings 100 by default) | `-.352.cs:190-215`, `CompileOptions.cs:363` | owner |
| MSG-051 | At the limit, the CODESYS line "More than {0} … occured: Skipping all further … messages" is kept, as `Error` or `Warning`, without a code | `-.352.cs:193-213` | planned |
| MSG-052 | Related positions are dropped once the error limit is passed | `-.352.cs:165` | planned |
| MSG-053 | The Precompile area keeps at most 950 messages per POU, oldest dropped first | `PreCompileContext.cs:2507-2525` | planned |
| MSG-060 | A related position is an `Information` message after its message: "Related position", "Called '{0}'.", "'{0}' using {1} bytes of stack." | `CallTree.cs:324-360` | planned |
| MSG-061 | The related position of a suppressed warning is `SuppressedInformation` | `CallTree.cs:354` | planned |
| MSG-070 | Messages keep the order in which they are produced | `Messages.cs:28-43` | planned |
| MSG-071 | A duplicate message (same code, position, text, category, object) is dropped; the first wins | `-.351.cs:65-75` | planned |
| MSG-072 | A build starts with "------ Build started: Application: {0} -------" | `CompilerServicesInternal.cs:1246` | planned |
| MSG-073 | `compile` ends with "Build complete -- {0} errors, {1} warnings : Ready for download" or "… : No download possible", with the counts of the run | `CompilerPhaseControllerGenerateCode.cs:656-686` | planned |
| MSG-074 | `check` ends with "Compile complete -- {0} errors, {1} warnings"; errors count `FatalError` and `Error`, warnings count `Warning` | `LanguageModelManagerConsolidated.cs:1972-1989` | planned |
| MSG-075 | Whether the counts include the limit lines | — | live |
| MSG-080 | `{error}`, `{fatalerror}`, `{warning}`, `{info}`, `{text}` create a message of their category; `{warning}` has code C0373, the others none | `PragmaStatementParser.cs:145-198`, `ErrorPragmaParser.cs:95-187` | planned |
| MSG-081 | `, Length := n` and `, messageguid 'GUID'` options of a message pragma | `ErrorPragmaParser.cs:102-111,198-212` | planned |
| MSG-082 | `show_compile` / `show_precompile` choose the areas of a pragma message | `ErrorPragmaParser.cs:218-247` | planned |
| MSG-083 | Where a pragma message without a show keyword is shown | `ErrorPragmaParser.cs:243-246` | live |
| MSG-090 | The person form uses the Messages view words `Error`, `Warning`, `Information`; a `Text` has no word | — | done (REQ-DS-problems-009, REQ-DS-dsl-007, -008, REQ-DS-cli-001, -002) |
| MSG-091 | VS Code: `FatalError`/`Error` → Errors, `Warning` → Warnings, `Information` → Infos; `Text` → Output channel; suppressed → not published | — | done (REQ-DS-cli-004) |
| MSG-092 | The program form uses the fields of `ScriptMessage` and the scripting category names, and includes suppressed messages | scripting help | done (REQ-DS-problems-008, REQ-DS-dsl-009, -010, REQ-DS-mcp-001, -003, -004, REQ-DS-cli-003) |
| MSG-093 | The program form has two queries: one that returns message objects (`ScriptMessage`), as `system.get_message_objects` does, and one that returns the texts alone, as `system.get_messages` does | scripting help; a CODESYS Forge script that reads `.severity` from each result | done (REQ-DS-mcp-005, -006) |
| MSG-094 | Message texts are rows of the message dictionary, not literals | — | done (ADR-0077) |
| MSG-095 | The message code is shown as prefix and four digits | `ErrorVisitor.cs:110` | done (`P####`) |
| MSG-100 | Static analysis rules (SA####) with off / warning / error per rule | static analysis help | n/a for now |
| MSG-101 | Library message rules: library warnings hidden, library errors summarized per library | `Messages.cs:60-80,405-408` | n/a until compiled libraries exist |
| MSG-102 | OEM-disabled warnings | `WarningHelper.cs:102-128` | n/a |
