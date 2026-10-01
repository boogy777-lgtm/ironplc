# CODESYS ST Surface Syntax

> **Status:** implemented; the special operators are typed by the analyzer and
> refused by code generation (see "Semantic resolution" and "Code generation").
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

Semantic resolution is described in the next section; this section is the
surface syntax only.

**REQ-CS-parser-008** `__NEW(T)` and `__TYPEOF(T)` parse as a call named
after the operator with the type argument recorded; `__DELETE(x)`,
`__ISVALIDREF(x)` and `__XADD(a, b)` parse as ordinary calls with their
argument lists; `__CURRENTTASK` parses as a named variable reference;
`__SYSTEM.field` and `__POOL.field` parse as structured variable references;
and an identifier that merely contains an operator name (`__NEW_ITEM`,
`__XADD2`) stays an identifier.
**REQ-CS-plc2plc-004** The special operators render in the spelling they were
written with and re-parse to the same AST.

`__NEW(T, n)` also parses: the type is followed by the element count of the
array to create, recorded as a second positional argument.

**REQ-CS-parser-009** `__NEW(T, n)` parses as a call named `__NEW` with the type
argument followed by the count expression; `__TYPEOF(T, n)` does not parse.

### Dialect gating

The operators are recognised by the analyzer only where the dialect enables
`--allow-special-operators`. Their surface syntax parses everywhere.

**REQ-CS-parser-010** The `codesys` dialect preset enables `allow_special_operators`;
no other preset does.

### Semantic resolution

With `allow_special_operators` on, `__NEW`, `__DELETE`, `__TYPEOF` and `__XADD`
are registered in the function environment like `SIZEOF`, so the declared-call
(P4017), argument-count (P4018) and argument-type (P4026) rules apply to them.
What a signature cannot say is checked by `rule_special_operator`, and reported
as P4073. The typing is that of the reference compiler
(`Codesys/decompiled/Compiler35220.plugin`, `NewExpression`,
`SimpleTypeChecker`, `TypeCheckerVisitor`):

| Operator | Operands | Type of the call |
|---|---|---|
| `__NEW(T)` / `__NEW(T, n)` | `T` a type; `n` an integer, and `T` elementary when `n` is given; the call is the value of an assignment | `POINTER TO T` |
| `__DELETE(p)` | `p` a pointer | `BOOL` |
| `__TYPEOF(x)` | a type, or an expression | `INT` |
| `__XADD(p, v)` | `p : POINTER TO DINT`, `v` an integer | `DINT` |

Assigning the value of `__NEW(T)` to a reference whose target is not `T` is
P2032, as for `REF(x)`, and is suppressed by the same
`allow_ref_type_punning` (which the `codesys` preset enables).

**REQ-CS-analyzer-003** With `allow_special_operators` on, `__NEW(T)` and
`__NEW(T, n)` have the type `POINTER TO T`, so `p := __NEW(INT)` is accepted for
`p : POINTER TO INT` and assigning it to a variable that is not a reference is
P2032.

**REQ-CS-analyzer-004** `__NEW` is rejected with P4073 when its first operand is
not a type, when its count is not an integer, when a count is given for a
user-defined type, or when its result is not the value of an assignment.

**REQ-CS-analyzer-005** `__DELETE(p)` has the type `BOOL` and is rejected with
P4073 when `p` is not a pointer.

**REQ-CS-analyzer-006** `__TYPEOF(x)` has the type `INT` and accepts a type name
or an expression.

**REQ-CS-analyzer-007** `__XADD(p, v)` has the type `DINT` and is rejected with
P4073 when `p` is not a `POINTER TO DINT`.

`__CURRENTTASK` is a `POINTER TO __SYSTEM.__TaskSpecificInfo`, and `__SYSTEM.x`
and `__POOL.x` look `x` up in the target's system library and global pool. That
structure and library are not in the reference material available here and are
not provided by this compiler, so typing them would be a guess. They are
recognised and rejected, never reported as undeclared.

**REQ-CS-analyzer-008** With `allow_special_operators` on, `__CURRENTTASK`,
`__SYSTEM` and `__POOL` (including `__SYSTEM.x` and `__POOL.x`) are rejected
with P4074, not P4007.

**REQ-CS-analyzer-009** With `allow_special_operators` off, `__NEW`,
`__DELETE`, `__TYPEOF` and `__XADD` are undeclared functions (P4017) and
`__CURRENTTASK`, `__SYSTEM` and `__POOL` are undefined variables (P4007).

`__ISVALIDREF` is unchanged: it is lowered to `r <> NULL` under
`allow_reference_to` (reference-to-twincat.md).

### Code generation

The four operators have no runtime behaviour in this compiler: there is no heap
for `__NEW` and `__DELETE`, no type-class values for `__TYPEOF`, and no atomic
memory access for `__XADD`. Code generation refuses a call to any of them with
P9999 (not implemented) at the operator and emits no code for it.

**REQ-CS-codegen-002** Compiling a program that calls `__NEW`, `__DELETE`,
`__TYPEOF` or `__XADD` fails with P9999 located at the operator name.
