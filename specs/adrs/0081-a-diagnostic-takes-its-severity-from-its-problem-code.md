# A Diagnostic Takes Its Severity from Its Problem Code

status: accepted
date: 2026-10-16

## Context and Problem Statement

Every diagnostic is an error. Code generation skips when any diagnostic
exists, a command fails when it collected any, the language server publishes
every one with `DiagnosticSeverity::ERROR`, and the MCP tools write
`"severity": "error"` for every one. A program that is valid but probably not
what was meant, such as the comparison of two values of different enumerations,
can therefore only be accepted silently or rejected.

CODESYS has three message categories: errors, warnings and information. A
warning is shown and the build goes on; the comparison above is a warning in
CODESYS, and the program compiles. IronPLC needs the same three levels, and
needs them in a form that a new level or a new warning code adds without a
search for every place that treats a diagnostic as a failure.

## Decision Drivers

- **One declaration of a fact.** Whether a problem stops the build is a property
  of the problem code, as its message is; a second list of warnings would drift
  from the first.
- **A level that a consumer cannot forget.** A consumer that shows a severity
  must handle every level, and one that a later level is added to must not
  compile until it does.
- **One test for failure.** Every place that decides that a program is rejected
  asks the same question, so that none keeps asking whether there are any
  diagnostics.
- **No change of behavior to introduce it.** Every code is an error today.

## Considered Options

1. **A `Severity` column of `problem-codes.csv` (chosen).** `problems/build.rs`
   generates `enum Severity` and `Problem::severity()`. `Diagnostic` reads its
   severity from its problem in the one private constructor.
2. **A severity chosen where the diagnostic is made.** A constructor for a
   warning beside the one for an error.
3. **A list of warning codes kept in the code that consumes them.**

## Decision Outcome

Chosen option: **a `Severity` column of `problem-codes.csv`**.

- The column holds `Error`, `Warning` or `Info`. The build of the problems crate
  fails on a missing or an unknown value and on a header other than
  `Code,Name,Message,Severity`. The enumeration `Severity` is generated from the one list of levels
  that the check reads, so a level is a row of that list.
- `Diagnostic::severity()` is the severity of its problem, set in
  `Diagnostic::problem` and nowhere else; no call site sets it.
  `Diagnostic::is_error()` is the one test for failure, and
  `ironplc_dsl::diagnostic::has_errors` and `error_count` apply it to a list.
- Every gate that stops a build asks `has_errors`: code generation in
  `ironplc-project`, the exit status of the command line, the editor's
  run command, the `ok` of the MCP tools and the benchmark fixtures.
- Every display maps `Severity` with a `match` that has no wildcard arm: the
  command line prints `error`, `warning` and `note` (the codespan category of
  an information message), and the language server uses `ERROR`, `WARNING` and
  `INFORMATION`. The MCP tools and the playground write the text of the level,
  `"error"`, `"warning"` or `"info"`, which `Severity::name()` returns; it is
  generated with the enumeration, from the same list of levels. A fourth level
  does not compile until the command line and the language server handle it.
- An analysis pass that collects diagnostics and returns `Err` to stop that pass
  is not a gate and is unchanged; the diagnostics it returns reach the gates
  above through the semantic context.

### Consequences

- Good, because a new warning is one row of the CSV with `Warning`, and no
  consumer changes.
- Good, because a new level is one entry of the list of levels, and every
  consumer fails to compile until it shows the level.
- Good, because no output changes: every existing row is `Error`.
- Bad, because the severity of a code is fixed for every use of the code. Turning
  a warning off or making it an error is a later decision, and when it is made
  it is a setting that is read where a diagnostic is shown or gated, not a
  second severity on the diagnostic.
- Bad, because a consumer that counts diagnostics for display (a status line
  that says "3 problems") counts warnings with errors until it is changed; the
  command line and the benchmarks count errors where they decide failure.

### Confirmation

- `specs/design/diagnostic-severity.md`: REQ-DS-problems-001 to 003,
  REQ-DS-dsl-001 to 003, REQ-DS-project-001, REQ-DS-mcp-001 and 002,
  REQ-DS-playground-001.
- `compiler/ironplc-cli/src/cli/tests.rs` and `compiler/ironplc-cli/src/lsp_project/tests.rs`:
  the mapping of every level for the command line and the language server (the
  crate has no conformance listing, so these carry no requirement identifier).
