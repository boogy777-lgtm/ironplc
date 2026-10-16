# Message Categories for Diagnostics

## Goal

Give every diagnostic one of the seven message categories of CODESYS, which is the reference. Each category behaves as it does in CODESYS:
- whether it stops the build;
- whether it is shown;
- how it is suppressed.

The first new message is a warning on a comparison of two values of different strict enumerations (`a = b` where `a : Color`, `b : Mode`). CODESYS reports this as warning C0354 and still compiles.

Today every diagnostic is an error:
- `project/src/compile.rs` skips code generation when any diagnostic exists;
- `ironplc-cli/src/cli.rs` `finish` fails on any diagnostic;
- the language server sets `DiagnosticSeverity::ERROR` on all of them (`ironplc-cli/src/lsp_project.rs:467`);
- the MCP tools write `"severity": "error"` (`mcp/src/tools/common.rs:228`).

## The reference: CODESYS

**The categories.** `_3S.CoDeSys.Core.Messages.Severity` (decompiled `MessageStorage/Severity.cs:9-32`) is a flag set with seven values:

| Value | Category | What CODESYS does with it (`Compiler35220.plugin/-/-.352.cs:113-160`) | What CODESYS uses it for |
|---|---|---|---|
| 1 | `FatalError` | Clears the messages already stored for the compile, marks the compile as aborted and stores this message alone. It stops the build. | Internal failures that carry no message id (`-/-.294.cs:145`) |
| 2 | `Error` | Stored until the error limit (500) is reached. It stops the build. | 426 compiler messages `Err_*` |
| 4 | `Warning` | Stored until the warning limit is reached. It does not stop the build. | 77 compiler messages `Wrn_*`, for example C0354 |
| 8 | `Information` | Stored; `Inf_RelatedPosition` is capped at 500. It does not stop the build. | The related position of another message (a recursive call, the stack use); "use XSIZEOF"; the code, data and total memory sizes after a build (`-/-.349.cs:52-73`) |
| 16 | `Text` | Stored. It does not stop the build. It has no message id. | Progress and timing lines of the build (`-/-.12.cs:125`, `-/-.340.cs:23`), and `Txt_ParentContextNotUpToDate` |
| 32 | `SuppressedWarning` | Not stored, so not shown. It does not stop the build. | A warning whose id is disabled: `{warning disable Cxxxx}`, or "Not displayed" in the project's compiler warnings (`-/-.14.cs:27`, `-/-.135.cs:246`, `-/-.278.cs:720`) |
| 64 | `SuppressedInformation` | Not stored, so not shown. It does not stop the build. | An information message that is disabled |

Facts from the decompiled code:
- **What stops the build.** Every check of failure in the compiler is `Severity == Error || Severity == FatalError`, for example `-/-.164.cs:137`, `-/-.281.cs:161` and `-/-.321.cs:270`.
- **How suppression works.** Suppression is not a category a message is declared with. A warning becomes `SuppressedWarning` when its id is disabled. `MessageHandling` has three values:
  - `Report` keeps it a warning;
  - `Suppress` makes it `SuppressedWarning`;
  - `Ignore` drops it entirely.
- **Settings in the CODESYS help.** The project setting "Compiler Warnings" sets each warning to one of:
  - "Not displayed" (suppressed);
  - "Warning";
  - "Error", which blocks the download.

  The help says that `{warning disable Cxxxx}` / `{warning restore Cxxxx}` can be used for every compiler warning. Nothing documents how to disable an error.
- **Other tools agree.** RuSTy (PLC-lang) has `Error`, `Warning`, `Info` and `Ignore`, set per code by an `--error-config` file. The IEC 61131-3 texts we could read define no message categories.

## Architecture

- **One type.** `Severity` has the seven CODESYS categories, in the same order. An exhaustive `match` on it answers each question, and there is no other list:
  - `stops_build()`: `FatalError` and `Error`;
  - `is_shown()`: every category except the two suppressed ones;
  - `suppressed()`: `Warning → SuppressedWarning`, `Information → SuppressedInformation`. The others cannot be suppressed and stay as they are, as in CODESYS.
- **Where a code's category is declared.** `problems/resources/problem-codes.csv` gains a `Severity` column. A code may be declared `FatalError`, `Error`, `Warning`, `Information` or `Text`. The two suppressed categories are never declared; they are only the result of a suppression. `problems_gen` generates `Problem::severity()` and fails the build on a missing or invalid value. No list of warnings is kept anywhere else: each code declares its own category in its own row.
- **How a diagnostic gets it.** `Diagnostic` (`dsl/src/diagnostic.rs`) takes its category from its problem, in the private constructor `Diagnostic::problem`. No call site sets it. `Diagnostic::suppress()` is the one way to change it later. The suppression sources, the pragma and per-code settings, come with the Messages family of attributes.
- **A fatal error, as CODESYS.** When a stage produces a `FatalError`, analysis and code generation stop. The diagnostics of the run are only the fatal ones. Which codes are fatal is a separate decision, made per code in its row; PR 1 declares none.
- **What decides failure.** Every place that now asks whether diagnostics are empty asks `stops_build()` instead:
  - code generation;
  - the exit status of the CLI;
  - `SemanticContext` callers that gate on failure;
  - the hot-edit and benchmark paths.
- **What each consumer shows.** Every consumer maps `Severity` with an exhaustive `match`, so a new category does not compile until every consumer handles it:

| Category | CLI | Language server | MCP `severity` | Playground |
|---|---|---|---|---|
| `FatalError` | `fatal error[P####]` | `ERROR` | `"fatal-error"` | error |
| `Error` | `error[P####]` | `ERROR` | `"error"` | error |
| `Warning` | `warning[P####]` | `WARNING` | `"warning"` | warning |
| `Information` | `info[P####]` | `INFORMATION` | `"information"` | information |
| `Text` | the text alone, without a code | `window/logMessage`, not a diagnostic | `"text"` | the output pane |
| `SuppressedWarning`, `SuppressedInformation` | not shown | not shown | returned, with `"suppressed-warning"` / `"suppressed-information"`, so a tool can see what was hidden | not shown |

- **Problem documentation.** Each `docs/reference/compiler/problems/P####.rst` says its category, through one include per category. No sentence is duplicated.
- **The first warning.** A new rule reports `a = b`, `a <> b` and the other comparisons when the operands are values of two different enumerations and at least one of them is strict. Strictness is read through `IntermediateType::is_strict_enumeration`, the one existing answer (ADR-0079).

## Prefactoring

- **PR 1 (no behaviour change).**
  - The seven-value `Severity`, with `stops_build`, `is_shown` and `suppressed`.
  - The `Severity` column, with every existing row `Error`.
  - `Problem::severity()`, `Diagnostic::severity()` and `Diagnostic::suppress()`.
  - Every failure gate asks `stops_build()`; every consumer maps all seven categories.

  No code has another category yet, so no output changes: the gate, `analysis_pins` and the parity tests must not move. Record the decision in ADR-0081; 0080 is taken by an unmerged branch.

## Design doc reference

- None for diagnostics today.
- PR 1 adds `specs/design/diagnostic-severity.md`, with requirement IDs.

## File map

- `compiler/problems/resources/problem-codes.csv`: the `Severity` column.
- `compiler/problems_gen/`: parse the column, generate `severity()`, and fail on a bad value.
- `compiler/dsl/src/diagnostic.rs`, or the problems crate: `Severity`, `severity()`, `suppress()`.
- `compiler/project/src/compile.rs`, `compiler/project/src/project.rs`: gate on `stops_build()`; stop at a fatal error.
- `compiler/ironplc-cli/src/cli.rs`: rendering and exit status.
- `compiler/ironplc-cli/src/lsp_project.rs`, `lsp_runner.rs`: the category mapping, log messages and gating.
- `compiler/mcp/src/tools/*.rs`: the `severity` field and the "any error" checks (`pou_lineage.rs`, `pou_scope.rs`, `project_io.rs`, `project_manifest.rs`, `symbols.rs`, `types_all.rs`).
- `compiler/playground/src/lib.rs`, `playground/`, `integrations/vscode/`: display.
- `compiler/benchmarks/src/{lib,project}.rs`: count the categories apart.
- `compiler/analyzer/src/rule_enumeration_comparison.rs` (new), `semantic_rules.rs`: the first warning.
- `docs/reference/compiler/problems/P####.rst` (new); one include per category in `docs/includes/`.
- `specs/adrs/0081-a-diagnostic-takes-its-category-from-its-problem-code.md`, `specs/design/diagnostic-severity.md`.

## Tasks

### PR 1: prefactor, seven categories with every code an error

- [ ] `Severity` with seven values and its three questions; unit tests for every value.
- [ ] The `Severity` column (`FatalError`, `Error`, `Warning`, `Information`, `Text`), the `problems_gen` check, `Problem::severity()`.
- [ ] `Diagnostic::severity()`, `suppress()`; unit tests.
- [ ] Every failure gate asks `stops_build()`; a fatal error stops the later stages. The stop is tested through a test-only path, because no code is fatal yet.
- [ ] Every consumer maps all seven categories with an exhaustive match. Test the mapping functions directly.
- [ ] ADR-0081 and `specs/design/diagnostic-severity.md` with requirement IDs.
- [ ] Gate: no output moves.

### PR 2: the first warning

- [ ] A rule for comparisons of two different enumerations where one is strict, with a new code in the analyzer range whose row says `Warning`.
- [ ] Tests covering:
  - each comparison operator;
  - strict against strict, strict against non-strict, non-strict against non-strict;
  - an alias;
  - a `CASE` with a selector of another enumeration, if the analysis has a type there; otherwise listed.
- [ ] End to end, a program with the warning:
  - `check` exits 0 and prints the warning;
  - `compile` writes the container;
  - the VM runs it.
- [ ] The language server reports `WARNING`; MCP reports `"warning"`.
- [ ] The problem documentation, and the enumeration page says what is reported.

### Later, each its own decision

- **Which codes are `FatalError`.** Candidates: an internal error (P9998), stack budget exhausted, a source that cannot be read.
- **`Information` messages.** Candidates: the related position of a recursion or of a stack use; the code and data sizes after `compile`, as CODESYS does.
- **`Text` messages.** Candidates: progress and timing lines of `compile`.
- **Suppression sources** (Messages family):
  - `{warning disable Cxxxx}` / `{warning restore Cxxxx}`;
  - per-code project settings "not displayed / warning / error";
  - the `{info}`, `{warning}`, `{error}` and `{text}` message pragmas;
  - the 500-message limits.
