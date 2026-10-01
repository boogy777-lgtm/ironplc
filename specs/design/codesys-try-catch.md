# CODESYS Exception Handling (`__TRY` / `__CATCH` / `__FINALLY` / `__ENDTRY` / `__THROW`)

## Overview

CODESYS Structured Text adds exception handling on top of IEC 61131-3:

```
__TRY
  <protected statements>
[__CATCH [(e)]
  <handler statements>]
[__FINALLY
  <cleanup statements>]
__ENDTRY
```

and a raise statement, `__THROW`, written in the prefixed-operator form the
reference parser uses (`__THROW(5)`, or a bare `__THROW`).

The syntax is gated behind `--allow-try-catch`, which the `codesys` and
`twincat` dialect presets enable. The five words are demoted to identifiers
when the flag is off, so standard programs may still use `__TRY` (or any of
its siblings) as a variable or POU name.

Reference: `Codesys/tables/special_operators.csv` rows 26-30,
`Codesys/grammar/ST_GRAMMAR.ebnf` (`tryCatchStatement`, `throwStatement`) and
`Codesys/decompiled/Parser35210.plugin/CODESYS/Parser35210/Statements/TryCatchStatementParser.cs`.

## Lexical Grammar

**REQ-TC-parser-001** The lexer recognizes `__TRY`, `__ENDTRY`, `__CATCH`,
`__FINALLY` and `__THROW` as distinct `TokenType` variants, matching them
case-insensitively.

**REQ-TC-parser-002** Each of the five words demotes to `TokenType::Identifier`
when `allow_try_catch` is off, and stays its own token type when the flag is
on. `__ENDTRY` is a longer match than `__TRY`; neither swallows an identifier
that only starts with the same text (`__TRYING` stays an identifier).

## Syntactic Grammar

**REQ-TC-parser-010** `__TRY <statements> __ENDTRY` parses to a `TryCatch`
statement with an empty body allowed, no catch clause and no finally body.

**REQ-TC-parser-011** A `__CATCH` clause may be written bare, as
`__CATCH (e)` with an exception variable, or as `__CATCH ()` with empty
parentheses; the two parenthesized spellings both carry the variable when one
is named.

**REQ-TC-parser-012** A `__FINALLY` clause and a `__CATCH` clause may appear
together, in that order only: `__CATCH` before `__FINALLY`, and `__ENDTRY`
last.

**REQ-TC-parser-013** `__TRY` statements nest; the inner statement is the
outer body's statement.

**REQ-TC-parser-014** `__THROW(expression);` parses to a `Throw` statement
carrying the expression; a bare `__THROW;` carries no value.

## AST Representation

**REQ-TC-parser-020** The three clauses are reachable from `StmtKind::TryCatch`:
the protected body, an optional `CatchClause` (with its optional exception
variable), and the finally body as a separate vector.

## Rendering

**REQ-TC-plc2plc-001** A `TryCatch` statement renders as `__TRY`/`__CATCH`/
`__FINALLY`/`__ENDTRY` and a `Throw` statement as `__THROW(…)`/`__THROW`, and
the rendering parses back to an equal AST under the same options.

## Semantic Analysis

**REQ-TC-codegen-001** A `__THROW` statement with no enclosing `__TRY` that has
a `__CATCH` clause in the same POU body is rejected with problem code P4066,
because the VM has no exception propagation to hand it to.

## Code Generation

**REQ-TC-codegen-010** A `__TRY` that completes without a `__THROW` runs the
protected body and then the `__FINALLY` body, and never runs the `__CATCH`
body.

**REQ-TC-codegen-011** `__THROW(v)` inside the protected body stores `v` into
the `__CATCH (e)` variable, skips the rest of the protected body, runs the
catch body and then the finally body, and goes on after `__ENDTRY`.

**REQ-TC-codegen-012** A `__THROW` inside a `__CATCH` or `__FINALLY` body
belongs to the next enclosing `__TRY`, not to the statement that owns the
body.
