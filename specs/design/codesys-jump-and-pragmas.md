# CODESYS Jump Statements, Comments and Conditional Pragmas

## Overview

Three CODESYS Structured Text surfaces that the reference implementation
accepts and IronPLC did not:

- **Jump statements** — the Instruction List mnemonics `JMP` (with `label:`
  statement labels) and `CALC` (a conditional call), plus the `__WAIT`
  statement. `Codesys/grammar/ST_GRAMMAR.ebnf` (`jumpStatement`,
  `conditionalCall`, `waitStatement`), `tables/ilo_operators.csv` (`JMP`,
  `CALC`) and `tables/special_operators.csv` (`__WAIT`).
- **Comments** — nested `(* a (* b *) c *)` block comments
  (`InternalScanner.ScanComment`, `AllowNestedComments`) and `///`
  documentation comments (`ScanSingleLineComment`, `TokenType.DocComment`).
- **Conditional pragmas** — `{IF}`/`{ELSIF}`/`{ELSE}`/`{END_IF}` with
  `{DEFINE}`/`{UNDEFINE}` (`PragmaIfStatementParser` and its operand
  parsers).

Each surface has its own flag, enabled by the `codesys` and `twincat`
dialects: `--allow-jump-statement`, `--allow-calc-statement`,
`--allow-wait-statement`, `--allow-nested-comments` and `--allow-pragma-if`.
The keyword flags demote their words to identifiers when off, so a standard
program keeps using `JMP`, `CALC` or `__WAIT` as names.

## Jump statements

**REQ-JMP-parser-001** `JMP`, `CALC` and `__WAIT` are separate token types
matching case-insensitively, and each demotes to an identifier when its flag
is off.

**REQ-JMP-parser-002** The parser accepts `JMP label;`, where `label` names a
statement label, and `JMP (condition) label;`, where the jump happens only
when the condition is TRUE.

**REQ-JMP-parser-003** A statement label is `name:` in statement position. A
name in a declaration region (`VAR`…`END_VAR`, `TYPE`…`END_TYPE`,
`STRUCT`…`END_STRUCT`), a name directly after a declaration keyword
(`FUNCTION f : INT`), and a name inside a `CASE` statement are *not* labels;
the token transform leaves them as identifiers. The reference statement
parser draws the same line (`TryParseDeclarationOrLabel`, which skips the
label check while in a declaration or a `CASE`).

**REQ-JMP-parser-004** `CALC(condition, call);` carries the condition and the
function block invocation; the reference's optional third operand (an
expected result type) is not modelled.

**REQ-JMP-parser-005** `__WAIT;` and `__WAIT(condition);` both parse; the
condition is optional in the AST.

## Comments

**REQ-JMP-parser-020** With `allow_nested_comments`, the token run a nested
block comment spans merges into one `Comment` token whose text and span cover
the whole comment. An unterminated nested comment is left as it was, so the
parse reports the error it would have reported with the flag off.

**REQ-JMP-parser-021** `///` lexes as `DocComment`, a token type of its own
that the parser skips as trivia exactly like a `Comment`. Without
`allow_c_style_comments` a `///` comment is rejected like a `//` one
(P0004).

## Conditional pragmas

**REQ-JMP-parser-030** With `allow_pragma_if`, an `{IF}` pragma evaluates its
condition and only the taken branch's tokens reach the parser. The condition
language is `OR`, `AND`, `NOT`, parentheses, `TRUE`/`FALSE`, and
`defined(name)`. `{DEFINE name}` adds a name and `{UNDEFINE name}` removes
one, in source order, for the conditions that follow. Name comparison is
case-insensitive. Pragmas that are not branch pragmas (`{attribute …}`) keep
their trivia behavior inside the taken branch.

**REQ-JMP-parser-031** An `{IF}` with no `{END_IF}`, and a branch pragma with
no `{IF}`, report P0023; a condition the compiler does not understand reports
P0024 and evaluates to FALSE.

## Semantic Analysis

**REQ-JMP-analyzer-001** A `JMP` whose target names no label in the same POU
body reports P0027.

**REQ-JMP-analyzer-002** Two labels with the same name in one POU body report
P0042.

## Code Generation

**REQ-JMP-codegen-001** `JMP label;` transfers control to the label, forward
or backward, and execution goes on from there. `JMP (condition) label;`
transfers control only when the condition is TRUE.

**REQ-JMP-codegen-002** `CALC(condition, call);` runs the call only when the
condition is TRUE.

**REQ-JMP-codegen-003** `__WAIT(condition);` goes on only once the condition
is TRUE; with no condition the statement has no runtime meaning this VM can
implement, so it is refused as not implemented.

## Rendering

**REQ-JMP-plc2plc-001** `Jump`, `LabelStatement`, `ConditionalCall` and
`Wait` render as `JMP`/`label:`/`CALC`/`__WAIT` and the rendering parses back
to an equal AST under the same options.
