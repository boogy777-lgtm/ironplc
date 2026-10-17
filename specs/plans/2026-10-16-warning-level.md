# Message Parity with CODESYS

## Goal

IronPLC's messages behave as the CODESYS message system does, item by item. CODESYS is the reference. The concepts, the reference behaviour and the parity checklist (`MSG-NNN`) are defined in [Message Ontology](../design/message-ontology.md). This plan orders the work into one PR per concept. Each PR:
- turns its checklist items into requirements with tests;
- sets their state to "done" in the design document.

## Architecture

- **The registries.** Each concept is one class whose members declare themselves:
  - categories in the `LEVELS` list of `problems_gen`;
  - problem codes in `problem-codes.csv`;
  - areas as members of an area class;
  - suppression sources as members of their own mechanism.

  A consumer maps each class with an exhaustive `match`, so the next member does not compile until every consumer handles it.
- **Two kinds of suppression source, never mixed:**
  - settings (options given outside the source text, readable by a program);
  - source text (pragmas and attributes, read by the parser and the analysis).
- **Owner's deviation from CODESYS:** the warning limit is 500, like the error limit (MSG-050).

## Prefactoring

PR 1 (#131, merged) is the prefactor: every diagnostic has one of the seven categories, taken from its code, and every existing code is `Error` (MSG-001 to MSG-005).

## Design doc reference

- [Message Ontology](../design/message-ontology.md): concepts and checklist.
- [Diagnostic Severity](../design/diagnostic-severity.md): the categories.
- [ADR-0081](../adrs/0081-a-diagnostic-takes-its-category-from-its-problem-code.md).

## File map

Per PR below. Shared by most PRs:
- `compiler/problems/resources/problem-codes.csv`, `problem-texts.csv`;
- `compiler/problems_gen/`;
- `compiler/dsl/src/diagnostic.rs`;
- `compiler/ironplc-cli/src/{cli,lsp_project,lsp_runner}.rs`;
- `compiler/mcp/src/tools/`;
- `compiler/playground/`;
- `docs/reference/compiler/problems/`.

## Tasks

### PR 2: message forms (MSG-041, MSG-042, MSG-090 to MSG-093)

- [ ] **The person form.** Our own header renderer, with the Messages view words `Error`, `Warning`, `Information`. A `Text` has no word and no code. codespan stays for the source snippet.
- [ ] **The program form.**
  - The `ScriptMessage` field names and the scripting category names, in MCP and in `--format json`.
  - Suppressed messages are included.
  - A query that returns message objects, as `system.get_message_objects` does, beside one that returns the texts alone, as `system.get_messages` does.
- [ ] **The `CODESYS` column** in `problem-codes.csv`.
  - Fill it from the `exact` rows of the code study; the `close` rows only after review.
  - It is shown in both forms and on each problem page.
- [ ] **VS Code.**
  - `Text` goes to the Output channel through `window/logMessage`.
  - The Problems mapping is as in the design doc.
  - A suppressed message is not published.

### PR 3: the first warning

- [ ] A comparison of two different enumerations, one of them strict, is a `Warning`. It is the CODESYS counterpart of C0354.
- [ ] Tests: each operator; strict and non-strict pairs; an alias.
- [ ] End to end: `check` exits 0 and shows the warning, and `compile` writes the container.

### PR 4: codeless messages, limits and summary lines (MSG-040, MSG-050 to MSG-052, MSG-070 to MSG-074)

- [ ] A message may have no code.
- [ ] The 500 limits, with the CODESYS limit lines.
- [ ] Related positions are dropped after the error limit.
- [ ] The build start line and the "Build complete" and "Compile complete" summaries, with the counts of the run.
- [ ] Production order is kept; duplicates are dropped, the first one wins.

### PR 5: message areas (MSG-020 to MSG-022, MSG-053)

- [ ] An area class with the members `Build` and `Precompile`; each message carries the areas it is shown in.
  - `check` and `compile` write `Build`.
  - The language server's analysis while editing writes `Precompile`.
- [ ] The editor `source` per area.
- [ ] The 950 cap of `Precompile`.

### PR 6: handling and settings (MSG-030, MSG-031, MSG-036, MSG-037)

- [ ] `Report` / `Suppress` / `Ignore` per message.
- [ ] The options listing disabled warnings and warnings as errors.
  - Raising beats disabling.
  - Only warning codes can be listed.

### PR 7: suppression in the source text (MSG-032, MSG-034, MSG-035)

- [ ] `{warning disable Cnnnn}` / `{warning restore Cnnnn}`, using the code of the `CODESYS` column.
- [ ] The attributes `suppress_warning_N` and `suppress_messages`: registry rows move from Waiting to Honoured.
- [ ] MSG-033 is decided by a live check first.

### PR 8: user messages (MSG-080 to MSG-082)

- [ ] `{error}`, `{fatalerror}`, `{warning}`, `{info}` and `{text}`, with their options.
- [ ] MSG-083 is decided by a live check first.

### PR 9: related positions (MSG-060, MSG-061)

- [ ] Secondary labels are given as `Information` messages in the program form, in CODESYS order.

### Live checks in CODESYS (owner)

MSG-033, MSG-075 and MSG-083.
