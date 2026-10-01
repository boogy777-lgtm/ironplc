# CODESYS Literal Syntax (P0-9 … P0-12)

## Problem

The CODESYS surface scan in `Codesys/README.md` lists literal forms that
IronPLC does not lex or parse:

- **P0-9** typed string literals `__XSTRING#"…"`, `UTF8#'…'`, `UCHAR#'…'`.
- **P0-10** the `$U` escape (exactly eight hex digits) and the Windows-1252
  mapping of `$80`–`$FF` local codepoints.
- **P0-11** the `10#` radix, `BOOL#1`/`BOOL#0`, the `BIT` type and `BIT#0`/`BIT#1`.
- **P0-12** the `us`/`ns` duration units, the `LT#`/`LD#` prefixes, and
  `TOD#hh:mm` without seconds.

Reference: `Codesys/LEXER-GAP-ANALYSIS.md` sections 6.1, 6.3, 6.4 and
`Codesys/tables/string_escapes.csv`; per-item verification method in
sections 13 and 14.3 of the same document.

## Approach

Follow `specs/steering/syntax-support-guide.md` and ADR-0040: the PEG grammar
stays maximal and option-free; dialect policy is decided outside it.

- **String escapes (P0-10)** live in the single table `dsl::string_escape`,
  which the option-free parser calls through `unquote`; the table therefore
  recognises `$U` and cp1252 in every dialect. The literal's decoding is a
  property of the character set, not of a dialect — `$'` and `$"` already
  stand for themselves in both widths without a flag. The spec documents both.
- **Typed string prefixes (P0-9), `10#` (P0-11), `us`/`ns`, `LT#`/`LD#` and
  `TOD#hh:mm` (P0-12)** are spellings that no identifier can be confused
  with (an identifier does not begin with digits, and the prefix is adjacent
  to its literal), so the grammar recognises them directly and every dialect
  accepts them, the same permissive direction the existing `STRING#` /
  `WSTRING#` prefixes and `**` operator already take. No new flag.
- **`BIT` (P0-11)** can be a legal variable name (`bit : BOOL;` is valid
  Edition 2), so it is a keyword token demoted to `Identifier` unless
  `--allow-bit-type` is set, exactly the demotion mechanism ADR-0040 rule 3
  prescribes for a keyword whose spelling is also a valid identifier. The
  flag is enabled by the `rusty`, `codesys` and `twincat` dialects.

## Changes

- `compiler/dsl/src/string_escape.rs` — `$U` + 8 hex; cp1252 table for
  values 0x80–0xFF in both widths; `encode` emits `$U` for a control
  character the local codepoint cannot spell.
- `compiler/parser/src/rule_token_string_escape.rs` — help text names `$U`.
- `compiler/parser/src/parser.rs` — typed string prefixes; `decimal_based_integer`;
  `BOOL#0/1` matched as `Digits`; `BIT#0/1`; `LT`/`LD` prefixes;
  `us`/`ns` units; optional seconds in `daytime()`.
- `compiler/parser/src/token.rs` — `Bit` keyword token.
- `compiler/parser/src/xform_demote_keywords.rs` — demote `Bit`.
- `compiler/parser/src/xform_split_duration_units.rs` — `LT` is a duration prefix.
- `compiler/dsl/src/common.rs`, `compiler/dsl/src/time.rs` — `BIT` elementary
  type; `microseconds`/`nanoseconds` duration units.
- `compiler/analyzer/src/type_environment.rs` — `BIT` is the 1-bit boolean.
- `specs/design/string-literals.md`, `specs/design/time-literals.md` — new
  requirements; `compiler/parser/build.rs` registers the time-literals doc.
- Tests: parser unit tests per feature, plc2plc round trips, codegen
  end-to-end cases, spec-conformance tests.
- `compiler/problems/resources/problem-codes.csv` + docs page — one code for
  a disabled `BIT` type.

## Out of scope

`__XSTRING(n)` as a declared type (the literal only), `SAFE*` types, the
`__XINT`/`__XWORD`/`__UXINT` portability markers, and vector types.
