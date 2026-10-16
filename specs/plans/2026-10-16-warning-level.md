# Warning Level for Diagnostics

## Goal

Give every diagnostic a severity. A warning is shown in the CLI, the language server, the MCP tools and the playground, but it does not stop `check` or `compile`. The first warning reports a comparison of two values of different strict enumerations (`a = b` where `a : Color`, `b : Mode`). CODESYS reports this case as warning C0354 and still compiles.

Today every diagnostic is an error. `project/src/compile.rs` skips code generation when any diagnostic exists. `ironplc-cli/src/cli.rs` `finish` fails on any diagnostic. The language server sets `DiagnosticSeverity::ERROR` on all of them (`ironplc-cli/src/lsp_project.rs:467`). The MCP tools write `"severity": "error"` (`mcp/src/tools/common.rs:228`).

## Architecture

- **Where severity is declared.** Severity belongs to the problem code. `problems/resources/problem-codes.csv` gains a `Severity` column, `Error` or `Warning`. The `problems_gen` build step generates `Problem::severity()` from it, and a row without a valid severity fails the build. No list of warnings is kept anywhere else: each code declares its own severity in its own row.
- **How a diagnostic gets it.** `Diagnostic` (`dsl/src/diagnostic.rs`) reads its severity from its problem through the private constructor `Diagnostic::problem`. No call site sets it.
- **Who decides what stops the build.** `Diagnostic::is_error()` is the one test. Every place that now asks whether diagnostics are empty asks whether any is an error:
  - code generation in `project/src/compile.rs`;
  - the exit status of the CLI;
  - `SemanticContext::has_diagnostics` callers that gate on failure;
  - the hot-edit and benchmark paths.
- **What each consumer shows.** Each consumer maps `Severity` with an exhaustive `match`, so a third level added later fails to compile until every consumer handles it:
  - the CLI prints `warning[P####]`;
  - the language server uses `DiagnosticSeverity::WARNING`;
  - MCP writes `"severity": "warning"`;
  - the playground and the VS Code extension show the severity they receive.
- **Problem documentation.** `docs/reference/compiler/problems/P####.rst` says whether the code is a warning. The text comes from one include, so it is not duplicated.
- **The first warning.** A new rule reports `a = b`, `a <> b` and the other comparisons when the operands are values of two different enumerations and at least one of them is strict. Strictness is read through `IntermediateType::is_strict_enumeration`, the one existing answer (ADR-0079). This applies under both policies:
  - `every-enumeration`: the default; IEC dialects;
  - `with-attribute`: an enumeration with `{attribute 'strict'}`, in the CODESYS and TwinCAT dialects.

  Two non-strict enumerations compare as their base integers and are not reported, as in CODESYS.

## Not in this change

These come later, with the Messages family of attributes:
- turning a warning off or making it an error, per code;
- the CODESYS `{warning disable C0354}` pragma and its project settings;
- a CLI `--deny-warnings` option.

The severity of every existing code stays `Error`.

## Prefactoring

- **PR 1 (no behaviour change).** Add the `Severity` column, with every existing row `Error`. Add `Problem::severity()`, `Diagnostic::severity()` and `Diagnostic::is_error()`. Switch every "any diagnostic means failure" test to `is_error()`. Make every consumer map `Severity` with an exhaustive match. Because no code is a warning yet, every output stays the same: the gate, `analysis_pins` and the parity tests must not move. Record the decision in a new ADR. It takes number 0081, because 0080 is taken by an unmerged branch.

## Design doc reference

- None for diagnostics today.
- PR 1 adds `specs/design/diagnostic-severity.md`, with requirement IDs.

## File map

- `compiler/problems/resources/problem-codes.csv`: `Severity` column.
- `compiler/problems_gen/`: parse and generate `severity()`; fail the build on a bad value.
- `compiler/dsl/src/diagnostic.rs`: `Severity`, `severity()`, `is_error()`.
- `compiler/project/src/compile.rs`, `compiler/project/src/project.rs`: gate code generation on errors.
- `compiler/ironplc-cli/src/cli.rs`: rendering and exit status.
- `compiler/ironplc-cli/src/lsp_project.rs`, `lsp_runner.rs`: severity mapping, gating.
- `compiler/mcp/src/tools/*.rs`: the `severity` field and the `any error` checks (`pou_lineage.rs`, `pou_scope.rs`, `project_io.rs`, `project_manifest.rs`, `symbols.rs`, `types_all.rs`).
- `compiler/playground/src/lib.rs`, `playground/`, `integrations/vscode/`: display.
- `compiler/benchmarks/src/{lib,project}.rs`: count errors and warnings apart.
- `compiler/analyzer/src/rule_enumeration_comparison.rs` (new), `semantic_rules.rs`: the first warning.
- `docs/reference/compiler/problems/P####.rst` (new) and an include for "this code is a warning".
- `specs/adrs/0081-a-diagnostic-takes-its-severity-from-its-problem-code.md`, `specs/design/diagnostic-severity.md`.

## Tasks

### PR 1: prefactor, severity with every code an error

- [ ] `Severity` column, `problems_gen` check, `Problem::severity()`.
- [ ] `Diagnostic::severity()`, `is_error()`; unit tests.
- [ ] Every failure gate asks `is_error()`. Find them by searching for `is_empty()` and `has_diagnostics()` on diagnostics outside tests.
- [ ] Every consumer maps `Severity` with an exhaustive match.
- [ ] ADR-0081 and `specs/design/diagnostic-severity.md` with requirement IDs.
- [ ] Gate: no output moves.

### PR 2: the first warning

- [ ] A rule for comparisons of two different enumerations where one is strict, in the analyzer range, with a new code whose row says `Warning`.
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
- [ ] Gate, Sphinx, specs.
