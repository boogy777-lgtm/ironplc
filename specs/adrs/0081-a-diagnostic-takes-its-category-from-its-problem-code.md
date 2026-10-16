# A Diagnostic Takes Its Category from Its Problem Code

status: accepted
date: 2026-10-16

## Context and Problem Statement

Every diagnostic is an error. Code generation skips when any diagnostic
exists, a command fails when it collected any, the language server publishes
every one with `DiagnosticSeverity::ERROR`, and the MCP tools write
`"severity": "error"` for every one. A program that is valid but probably not
what was meant, such as the comparison of two values of different enumerations,
can therefore only be accepted silently or rejected.

CODESYS, the reference for this behavior, gives every message one of seven
categories (`Severity.cs`): `FatalError`, `Error`, `Warning`, `Information`,
`Text`, `SuppressedWarning` and `SuppressedInformation`. Each behaves in a
fixed way:

- a fatal error and an error stop the build, and every check of failure in the
  CODESYS compiler is "error or fatal error";
- a fatal error also clears the messages stored so far, so a run that ends in
  one reports only the fatal messages, and no later stage runs;
- a warning, an information message and a text do not stop the build;
- a suppressed warning and a suppressed information message are not shown and
  do not stop the build;
- suppression is not a category a message is declared with: a warning becomes a
  suppressed warning when its id is disabled, and a category that is not a
  warning or an information message cannot be suppressed.

IronPLC needs the same seven, in a form that a new category or a new warning
code adds without a search for every place that treats a diagnostic as a
failure.

## Decision Drivers

- **One declaration of a fact.** The category of a message is a property of the
  problem code, as its message is; a second list of warnings would drift from
  the first.
- **A category that a consumer cannot forget.** A consumer that shows a category
  must handle every one, and one that a later category is added to must not
  compile until it does.
- **One test for failure.** Every place that decides that a program is rejected
  asks the same question, so that none keeps asking whether there are any
  diagnostics.
- **No change of behavior to introduce it.** Every code is an error today.

## Considered Options

1. **A `Severity` column of `problem-codes.csv` and a seven-valued enumeration
   (chosen).** `problems/build.rs` generates `enum Severity`, its questions and
   `Problem::severity()` from one list of categories. `Diagnostic` reads its
   category from its problem in the one private constructor.
2. **Three categories (error, warning, information).** Smaller, but it leaves
   out the fatal error, the text and the suppression that CODESYS has and the
   owner wants.
3. **A category chosen where the diagnostic is made.** A constructor for a
   warning beside the one for an error.
4. **A list of warning codes kept in the code that consumes them.**

## Decision Outcome

Chosen option: **a `Severity` column and a seven-valued enumeration**.

- The column holds `FatalError`, `Error`, `Warning`, `Information` or `Text`.
  The two suppressed categories are never declared. The build of the problems
  crate fails on a missing value, an unknown one or a suppressed one, and on a
  header other than `Code,Name,Message,Severity`.
- `enum Severity`, the three questions `stops_build()`, `is_shown()` and
  `suppressed()`, and the text `name()` are generated as exhaustive matches from
  the one list of categories that the check reads.
- `Diagnostic::severity()` is the category of its problem, set in
  `Diagnostic::problem` and nowhere else. `Diagnostic::suppress()` is the one
  way to change it afterwards; the sources of a suppression (a pragma, a
  setting per code) are a later decision, and nothing but tests calls it.
- Every gate that stops a build asks `stops_build`: `Diagnostic::stops_build()`
  and, for a list, `ironplc_dsl::diagnostic::any_stops_build` and
  `stops_build_count`. The gates are code generation in `ironplc-project`, the
  exit status of the command line, the editor's run command, the `ok` of the MCP
  tools and the benchmark fixtures.
- A fatal error ends the run as in CODESYS. `ironplc_project::compile` runs no
  analysis after a seeded fatal error and no code generation after a fatal error
  of the analysis, and `stop_at_fatal` leaves only the fatal diagnostics. No
  code is fatal yet; which codes are is decided per code, in its row.
- Every display maps `Severity` with a `match` that has no wildcard arm. The
  command line prints a diagnostic of each category with the category codespan
  has for it, prints a text without a code, and prints no suppressed diagnostic;
  the language server uses `ERROR`, `WARNING` and `INFORMATION` and publishes no
  suppressed diagnostic; the MCP tools write `Severity::name()` for every
  diagnostic, suppressed ones included, so that a tool can see what was hidden;
  the playground writes `Severity::name()` and drops the diagnostics that are
  not shown. A category added to the list does not compile until the command
  line and the language server handle it.
- An analysis pass that collects diagnostics and returns `Err` to stop that pass
  is not a gate and is unchanged; the diagnostics it returns reach the gates
  above through the semantic context.

### Consequences

- Good, because a new warning is one row of the CSV with `Warning`, and no
  consumer changes.
- Good, because a new category is one entry of the list, and the command line
  and the language server fail to compile until they show it.
- Good, because no output changes: every existing row is `Error`.
- Bad, because the category of a code is fixed for every use of the code. Turning
  a warning off or making it an error is a later decision, and when it is made
  it goes through `suppress()` or a setting read where a diagnostic is shown or
  gated, not a second category on the diagnostic.
- Bad, because codespan prints its own header: `error`, `warning` and `note`. A
  fatal error is printed as `error` and an information message as `note`, not as
  `fatal error` and `info`.
- Bad, because a text is published by the language server as an information
  message, with its code, and not through `window/logMessage`; and because a
  consumer that counts diagnostics for display (a status line that says "3
  problems") counts warnings with errors until it is changed.

### Confirmation

- `specs/design/diagnostic-severity.md`: REQ-DS-problems-001 to 005,
  REQ-DS-dsl-001 to 005, REQ-DS-project-001 and 002, REQ-DS-mcp-001 to 003,
  REQ-DS-playground-001 and 002.
- `compiler/ironplc-cli/src/cli/tests.rs` and `compiler/ironplc-cli/src/lsp_project/tests.rs`:
  the mapping of every category for the command line and the language server
  (the crate has no conformance listing, so these carry no requirement
  identifier).
