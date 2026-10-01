# CODESYS ST: TRY/CATCH, JMP/labels, CALC, `__WAIT`, nested comments, DocComment, `{IF}`

## Problem

The CODESYS ST surface still misses the two P0 groups from
`Codesys/LEXER-GAP-ANALYSIS.md` section 13:

- **P0-15** — `__TRY` / `__CATCH` / `__FINALLY` / `__ENDTRY` / `__THROW`
  (`tables/special_operators.csv:26-30`, `grammar/ST_GRAMMAR.ebnf:307-311`).
- **P0-17** — `JMP` + statement labels, `CALC`, `__WAIT`, nested `(* … *)`
  comments, `DocComment` (`///`), and the `{IF}` conditional-compilation
  pragma (`ST_GRAMMAR.ebnf:297-311,318-330`, `docs/01_LEXER_PARSER.md` 3.8-3.9).

None of the tokens exist today, so each snippet fails with P0002/P0003 (see
`Codesys/LEXER-GAP-ANALYSIS.md` section 15).

## Approach

Follow the standard pipeline (lexer -> demotion -> transform -> PEG -> AST ->
renderer -> codegen) and the syntax-support guide:

- **Tokens** in `parser/src/token.rs`; each new keyword is **demoted** in
  `parser/src/xform_demote_keywords.rs` unless its flag is on, so standard
  programs keep the words as identifiers.
- **Flags** in `parser/src/options.rs` (`define_compiler_options!`), enabled by
  `codesys` and `twincat`: `allow_try_catch`, `allow_jump_statement`,
  `allow_calc_statement`, `allow_wait_statement`, `allow_nested_comments`,
  `allow_pragma_if`.
- **Token transforms** (the established mechanism for syntax that a stateless
  logos lexer cannot express):
  - statement labels: `Identifier Colon` -> `Label Colon` in statement
    position, reusing the CASE/declaration-context tracking pattern of
    `insert_keyword_statement_terminators`;
  - nested comments: merge the token run a nested `(* … *)` spans into one
    `Comment`;
  - `{IF}`: evaluate the pragma condition against `{DEFINE}`/`{UNDEFINE}`
    symbols and drop the branches not taken.

`JMP (cond) label` and `CALC(cond, call)` are statements; `__WAIT(cond)` is the
busy-wait statement the reference lowers to `WHILE NOT cond DO SynchWait(); END_WHILE`.
`__THROW` is a statement; a thrown value is stored into the `__CATCH (e)`
variable and jumps to that clause, so `__THROW` without an enclosing
`__TRY`/`__CATCH` in the same body is diagnosed.

New problem codes (`Codesys/ERROR-CODES-STUDY.md` section 4.3 recommendations):
P0023 `PragmaIfUnmatched`, P0024 `PragmaValueExpected`, P0027
`JumpDestinationInvalid`, P0042 `DuplicateLabelDefinition`.

## Changes

- `compiler/parser/src/token.rs` — `Try`, `Catch`, `Finally`, `EndTry`,
  `Throw`, `Jmp`, `Calc`, `Wait`, `DocComment`, `Label`.
- `compiler/parser/src/xform_demote_keywords.rs` — demotion arms for the six
  keyword groups.
- `compiler/parser/src/options.rs`, `compiler/ironplc-cli/bin/main.rs`,
  `docs/explanation/enabling-dialects-and-features.rst`,
  `docs/reference/compiler/ironplcc.rst` — the six flags.
- `compiler/parser/src/xform_statement_labels.rs`,
  `compiler/parser/src/xform_nested_comments.rs`,
  `compiler/parser/src/xform_pragma_if.rs` — the three transforms, wired into
  `tokenize_program`.
- `compiler/dsl/src/textual.rs` — `TryCatch`, `Jump`, `Label`, `Calc`, `Wait`
  statement nodes.
- `compiler/parser/src/parser.rs` — grammar rules.
- `compiler/plc2plc/src/renderer.rs` — render the new nodes.
- `compiler/codegen/src/compile_stmt.rs` (+ a small `compile_try_catch`
  module) — control-flow lowering.
- Tests: `parser/src/tests/{try_catch,jumps,pragmas,comments_and_errors}.rs`,
  `plc2plc/src/tests/{try_catch,jumps}.rs` + resources,
  `codegen/tests/it/end_to_end_{try_catch,jumps,calc}.rs`, whitespace rows.
- `specs/design/codesys-try-catch.md`, `specs/design/codesys-jump-and-pragmas.md`
  with `REQ-*` requirements and `#[spec_test]` bindings.
- `docs/reference/compiler/problems/P0023.rst`, `P0024.rst`, `P0027.rst`,
  `P0042.rst`.

## Delivery order

1. P0-15 (tokens, flag, AST, grammar, renderer, codegen, tests, spec).
2. P0-17 (tokens, flags, transforms, AST, grammar, renderer, codegen, tests,
   spec).
