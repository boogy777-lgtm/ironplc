# CODESYS ST Surface Syntax

> **Status:** implemented for the syntax; the semantics named "not implemented"
> below stay open.
> **Date:** 2026-10-01

## Overview

This design covers five pieces of the CODESYS/TwinCAT Structured Text surface
that IronPLC accepts on top of IEC 61131-3:

1. `|` as the symbol spelling of the logical OR operator.
2. The incomplete array type `ARRAY[*] OF T`.
3. The parameter-list data type `PARAMS(n) OF T`.
4. Escaped (backtick) identifiers, letters outside ASCII, and the
   consecutive-underscore restriction.
5. The ST-visible CODESYS special operators (`__NEW`, `__DELETE`,
   `__ISVALIDREF`, `__TYPEOF`, `__CURRENTTASK`, `__XADD`) and the
   `__SYSTEM` / `__POOL` scope prefixes.

The reference is the CODESYS parser table set (`Codesys/tables/*.csv`,
`Codesys/grammar/ST_GRAMMAR.ebnf`) recorded in
`Codesys/LEXER-GAP-ANALYSIS.md`, items P0-5, P0-6, P0-13, P0-14 and P0-16.

The pipeline is the usual one: tokens (`parser/src/token.rs`), a token
transform or validation rule where a flag gates the syntax, grammar rules in
`parser/src/parser.rs`, AST nodes in `dsl`, rendering in `plc2plc`, and the
semantics the construct needs in `analyzer` and `codegen`.

## Operator symbol `|`

`operator_symbols.csv` lists `|` with the `Operator|StructuredText` flag,
exactly as `&` spells AND. IronPLC already accepts `&` unconditionally -- it
cannot be confused with an identifier or any other token -- so `|` is added
to the same token (`TokenType::Or`) and needs no dialect gate. The AST is the
`CompareOp::Or` node `OR` produces, and the renderer writes the canonical
keyword spelling, so the two spellings are interchangeable on input.

**REQ-CS-parser-001** The lexer tokenizes `|` as the `Or` token, and `a | b`
parses to a `CompareOp::Or` node.
**REQ-CS-parser-002** `|` and `&` group by the same precedence as `OR` and
`AND`: `a | b & c` parses as `Or(a, And(b, c))`, and `a | b` equals the AST
of `a OR b`.
**REQ-CS-plc2plc-001** A source using `|` renders back as the canonical `OR`
spelling and re-parses to the same AST.

## Incomplete array type `ARRAY[*] OF T`

`arrayType = "ARRAY" "[" ("*" | indexRange {"," indexRange}) "]" "OF"
dataType`. The two spellings are alternatives, so the AST states them as
alternatives too: `ArraySubranges::bounds` is an `ArrayBounds`, either
`Ranges(Vec<Subrange>)` or `Incomplete(SourceSpan)` -- a range list plus an
"is incomplete" flag could represent a state no source can spell.

The grammar accepts `*` in every dialect (the parser has no options), so the
flag is enforced by the token-stream rule `rule_token_no_incomplete_array`,
which reports P4070. The analyzer resolves the incomplete form to an array
with no dimensions, the intermediate model's existing representation of an
array whose size is not known at declaration time.

**REQ-CS-parser-003** `ARRAY[*] OF T` parses to `ArrayBounds::Incomplete`
with the element type recorded, and the form produces P4070
(`IncompleteArrayNotAllowed`) when `--allow-incomplete-array` is off.
**REQ-CS-analyzer-001** An `ARRAY[*] OF T` declaration resolves to an array
type with no dimensions and the declared element type.
**REQ-CS-plc2plc-002** An `ARRAY[*] OF T` declaration renders with the star
and re-parses to the same AST.

## Parameter-list type `PARAMS(n) OF T`

`paramsType = "PARAMS" "(" expression ")" "OF" dataType`. `params` is a
common variable name, so `PARAMS` is a keyword token demoted to `Identifier`
unless `--allow-params-of` (the demotion lives in
`xform_demote_keywords.rs`). The count is read through the same `integer_ref`
the STRING length and the array bounds use, so a literal or a named constant
is accepted.

The AST keeps the surface spelling: `ParamsSpecification { count, type_name }`
inside a variable declaration (`InitialValueAssignmentKind::Params`) or a type
declaration (`DataTypeDeclarationKind::Params`). The analyzer lowers the list
to the array `ARRAY[0 .. n-1] OF T` -- the elements are addressed by index,
exactly like an array -- so bounds validation, element typing, layout and
code generation are the ordinary array ones. The variadic call-site semantics
of a PARAMS parameter (a call may pass fewer arguments than the list holds,
as CODESYS's `ErrParamsNotAllowed` presumes) are **not implemented**; only
the declaration is.

**REQ-CS-parser-004** `PARAMS(n) OF T` parses to a `ParamsSpecification`
recording the count and the element type, in a variable declaration and in a
type declaration, and `PARAMS` is an ordinary identifier when
`--allow-params-of` is off.
**REQ-CS-analyzer-002** A PARAMS declaration resolves to the array
`ARRAY[0 .. n-1]` of its element type, and a variable of a named PARAMS type
resolves the same way.
**REQ-CS-codegen-001** A PARAMS list is laid out as that array and its
elements read and write through the ordinary array subscript path.
**REQ-CS-plc2plc-003** A PARAMS declaration renders with the `PARAMS(n) OF T`
spelling and re-parses to the same AST.

## Identifier spellings

Three CODESYS scanner settings affect what an identifier may look like. Each
is a separate flag, because the reference treats them as separate options:

* **Escaped identifiers** (`SupportNonCompliantIdentifiers`, on by default in
  CODESYS): a backtick-delimited name, `` `my name` ``, which may contain
  characters an ordinary identifier cannot. The lexer recognizes
  `` `[^`\r\n]*` `` in every dialect and the rule reports P4067 unless
  `--allow-escaped-identifiers`. The backticks stay part of the name, so a
  declaration and every reference spell the name the same way and the
  renderer writes it back verbatim. Inside the escape every character is
  legal, so the Unicode and underscore checks do not apply to it.
* **Unicode identifiers** (`UnicodeIdentifiers`, off by default in CODESYS):
  `IsIdentifierStartCharacter` is an ASCII letter, `_`, or (when enabled) any
  Unicode letter; continuation characters add Unicode digits. The lexer
  accepts `\p{L}` / `\p{Nd}` in every dialect (the pattern replaces the ASCII
  one, they describe the same language for ASCII) and the rule reports P4068
  unless `--allow-unicode-identifiers`, which no dialect enables.
* **Consecutive underscores** (`AllowMultipleUnderlines=false` in CODESYS):
  two underscores in a row make the token an error there. The rule reports
  P4069 unless `--allow-multiple-underscores`. The leading `__` is exempt in
  every dialect: it is the compiler's reserved namespace, which the implicit
  `__SYSTEM_UP_TIME` global and the `__TRUNC` intrinsic rely on.

**REQ-CS-parser-005** A backtick-delimited identifier parses as one
identifier whose name keeps the backticks, a reference to it resolves to the
same name, and the form produces P4067 (`EscapedIdentifierNotAllowed`) when
`--allow-escaped-identifiers` is off.
**REQ-CS-parser-006** Letters outside ASCII are accepted in an identifier --
including non-ASCII digits as continuation characters -- and produce P4068
(`UnicodeIdentifierNotAllowed`) when `--allow-unicode-identifiers` is off.
**REQ-CS-parser-007** An identifier containing `__` outside the leading
reserved prefix produces P4069 (`MultipleUnderscoresNotAllowed`) unless
`--allow-multiple-underscores`; the leading `__` prefix is accepted without
the flag, and a later pair in such a name is still rejected.

## ST-visible special operators

`special_operators.csv` marks these names `Operator|AllLanguages`. The
reference scanner reads a whole identifier and only then looks it up in its
operator table, so `__NEW` is an operator while `__NEW_ITEM` is an ordinary
identifier. A logos token cannot make that distinction -- a
`#[token("__NEW")]` rule would split the longer name -- so the lexer produces
`Identifier` and `xform_promote_special_operators` performs the lookup, which
is the "keyword promotion" transform category of
`dialect-token-transforms.md`. The promotion is not flag-gated: the reference
recognizes these names in every language, and the `__` prefix is IronPLC's
reserved compiler namespace.

Grammar and AST:

* `__NEW(T)` and `__TYPEOF(T)` take a type (`newExpression`); they parse as
  the ordinary call node with the type name recorded as a variable reference,
  which is what the AST, the renderer and the round trip need. Resolving the
  type is semantic work a later pass does.
* `__DELETE(x)`, `__ISVALIDREF(x)` and `__XADD(a, b)` take expression lists
  and parse as ordinary calls (`prefixedOperator`).
* `__CURRENTTASK` is a value (`currentTaskExpression`) and parses as a named
  variable reference, so `__CURRENTTASK` and field access on it work.
* `__SYSTEM` and `__POOL` are scope prefixes, not operators: `__SYSTEM.x`
  is already the ordinary structured variable `__SYSTEM` with field `x`, so
  they stay identifiers and need no token.

The semantics of every one of these operators (allocation, deletion, atomic
exchange, task lookup) are **not implemented**: a call to one of them is an
undeclared function to the analyzer today. Only the surface syntax is in
scope here, as `LEXER-GAP-ANALYSIS.md` records for P0-16.

**REQ-CS-parser-008** `__NEW(T)` and `__TYPEOF(T)` parse as a call named
after the operator with the type argument recorded; `__DELETE(x)`,
`__ISVALIDREF(x)` and `__XADD(a, b)` parse as ordinary calls with their
argument lists; `__CURRENTTASK` parses as a named variable reference;
`__SYSTEM.field` and `__POOL.field` parse as structured variable references;
and an identifier that merely contains an operator name (`__NEW_ITEM`,
`__XADD2`) stays an identifier.
**REQ-CS-plc2plc-004** The special operators render in the spelling they were
written with and re-parse to the same AST.
