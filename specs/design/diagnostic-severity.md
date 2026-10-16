# Design: Diagnostic Severity

status: implemented
date: 2026-10-16

## Overview

A diagnostic has a severity: error, warning or information, the three message
categories of CODESYS. The severity belongs to the problem code. A warning and
an information message are shown and do not stop a build; only an error does.
The decision and its alternatives are in
[ADR-0081](../adrs/0081-a-diagnostic-takes-its-severity-from-its-problem-code.md).

Every problem code is an error today, so no output depends on the other two
levels yet. This document specifies the mechanism, so that a code that declares
`Warning` or `Info` is shown and does not stop the build in every consumer.

## Severity of a problem code

`compiler/problems/resources/problem-codes.csv` has the columns
`Code,Name,Message,Severity`. The value of `Severity` is `Error`, `Warning` or
`Info`.

**REQ-DS-problems-001** `Problem::severity()` returns the `Severity` of the
problem's row, and `Severity` has exactly the variants `Error`, `Warning` and
`Info`.

**REQ-DS-problems-002** A row of `problem-codes.csv` whose `Severity` is missing
or is not one of `Error`, `Warning` or `Info` fails the build, and the error
names the code and the value.

**REQ-DS-problems-003** The header of `problem-codes.csv` is
`Code,Name,Message,Severity`; any other header fails the build.

The enumeration `Severity` and the match of `Problem::severity()` are generated
by `problems/build.rs` from the one list of levels in `problems_gen`. The match
names every problem and has no wildcard arm.

## Severity of a diagnostic

**REQ-DS-dsl-001** A `Diagnostic` has the severity of its problem, whichever
constructor made it. No call site sets the severity.

**REQ-DS-dsl-002** `Diagnostic::is_error()` is true for a diagnostic of severity
`Error` and false for one of severity `Warning` or `Info`.

**REQ-DS-dsl-003** `has_errors` is true when at least one diagnostic of a list is
an error, and `error_count` counts the errors of a list; a list of only warnings
and information messages has no errors.

## What stops a build

A place that decides that a program is rejected asks `has_errors`, not whether
there are diagnostics.

**REQ-DS-project-001** `ironplc_project::compile` runs code generation and
returns a container when no diagnostic is an error, and returns no container
when one is, seeded by the caller or found by the analysis.

The other gates are the exit status of every command of the command line
(`finish`), the check that `sync-uids` runs before it writes the sidecar, the
language server's run command, the `ok` of the MCP tools `check`, `parse`,
`pou_lineage`, `pou_scope`, `project_io`, `project_manifest`, `symbols` and
`types_all`, and the fixture guard of the benchmarks. A diagnostic that is not an
error is still printed by the command line and published by the language server.

An analysis pass that collects diagnostics and returns `Err` to stop that pass
(`semantic_rules`, the `xform_*` passes) is not a gate. Its diagnostics are added
to the semantic context, and they reach the gates above.

## What each consumer shows

Each consumer maps `Severity` with a `match` that has no wildcard arm.

| Consumer | Error | Warning | Info |
|----------|-------|---------|------|
| Command line (codespan) | `error` | `warning` | `note` |
| Language server | `DiagnosticSeverity::ERROR` | `WARNING` | `INFORMATION` |
| MCP tools, `severity` | `"error"` | `"warning"` | `"info"` |
| Playground, `severity` | `"error"` | `"warning"` | `"info"` |

**REQ-DS-mcp-001** `serialize_diagnostic` writes `"error"`, `"warning"` or `"info"`
as the `severity` of a diagnostic, by the severity of the diagnostic.

**REQ-DS-mcp-002** The `ok` of `check` is false when a diagnostic is an error and
true when none is. A diagnostic of another severity is listed and does not make
`ok` false, because `ok` asks `has_errors`.

**REQ-DS-playground-001** The playground writes `"error"`, `"warning"` or `"info"`
as the `severity` of a diagnostic, by the severity of the diagnostic.

The command line and the language server are tested by unit tests of the mapping
functions (`codespan_severity`, `lsp_severity`) and of `finish`; `ironplc-cli` has
no conformance listing, so they carry no requirement identifier.
