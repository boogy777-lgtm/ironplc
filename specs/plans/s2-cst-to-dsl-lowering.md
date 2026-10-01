# S2: CST-to-dsl Lowering and a Single Production Parse Path

**Issue:** [#16](https://github.com/boogy777-lgtm/ironplc/issues/16)

Implements stage S2 ("CST-to-dsl lowering") of
[Parse-Tree Architecture](../design/parse-tree-architecture.md) §5, on top of
the S1 result in `compiler/syntax` (`ironplc-syntax`, merged at `5e31a45da`),
using the provenance findings of
[Parse-Tree S0 Audit](../design/parse-tree-s0-audit.md) and the baseline of
[Parse-Tree S0 Experiment](../design/parse-tree-s0-experiment.md) §3.5. S3 is
withdrawn (owner decision, 2026-10-01): semantics recompute per snapshot and no
query framework is introduced. The work spans multiple PRs and is tracked by
issue [#16](https://github.com/boogy777-lgtm/ironplc/issues/16); this is a plan
PR that is never merged, per the development standards. The file name follows
the S1 plan's `sN-name.md` form rather than the `YYYY-MM-DD-` prefix the
development standards ask for.

## Goal

Replace the legacy production parse path in `compiler/parser` (logos lexer,
nine token transforms, the OSCAT pre-pass, seven token-check rules, the
2,608-line PEG grammar) with **one** path: the `ironplc-syntax` CST, then a
**lowering** from that CST to the existing `ironplc_dsl` AST. The semantic
meaning the legacy transforms carried (OSCAT ranged comments, conditional
`{IF}` pragmas, duration splitting, statement labels, special operators, keyword
demotion, optional terminators) is retained **through provenance in the tree**,
never by rewriting the text or the token stream. After cutover, analyzer,
codegen, plc2plc, CLI, LSP, MCP, sources and the playground see no API change:
`ironplc-parser` stays the public facade (`parse_program`,
`parse_st_statements`, `tokenize_program`, `CompilerOptions`, `Dialect`).

The stage ends with the legacy lexer, PEG grammar, transforms and the `peg` and
`logos` dependencies deleted, and with every difference between the old and new
behaviour either removed or listed, reasoned and approved (see
[Parity exceptions](#parity-exceptions-and-their-resolution)).

## Architecture

### Where the lowering lives (decision)

**In `ironplc-syntax`, as a `lower` module tree. `ironplc-parser` becomes a thin
compatibility facade.** Alternatives considered:

| Option | Callers | Mechanisms after S2 | Verdict |
|---|---|---|---|
| A. `ironplc-syntax::lower` + `ironplc-parser` facade | Unchanged (223 uses of `CompilerOptions`, 99 of `parse_program`, tokenizer/LSP of `tokenize_program` all keep their imports) | One frontend crate holds grammar, gates, shape knowledge and lowering; one literal decoder shared by validation and lowering | **Chosen** |
| B. New `ironplc-lower` crate | Unchanged | Same, plus a third crate, its release/version wiring, and a crate boundary between the grammar and the code that must agree with its node shapes | Rejected: a boundary with no second consumer (extract it if S4 ever needs one) |
| C. Lowering inside `ironplc-parser` | Unchanged | Literal checks would be shared only via `ironplc-dsl`; harness and legacy oracle would sit with the lowering but grammar-shape contracts would cross a crate boundary | Rejected: shape drift between grammar and lowering is then caught only across crates |

Why A leaves one mechanism and minimal churn:

- `ironplc-syntax` already depends on `ironplc-dsl` and `ironplc-problems`
  (`compiler/syntax/Cargo.toml`), so lowering adds no dependency edge. The
  parity harness is moved into `ironplc-parser` (PF2), which removes the
  existing `ironplc-parser` dev-dependency of `ironplc-syntax`: no
  dependency cycle, even a dev-only one, and the oracle and its harness live
  and die together.
- The grammar (`compiler/syntax/src/parser/grammar/`, 3,839 lines) and the
  lowering must agree on node shapes. Co-locating them lets one test module
  assert the contract (the disposition table below) with no cross-crate
  plumbing.
- The facade keeps today's signatures, so none of the production callers
  changes: `sources/src/parsers/st_parser.rs:12`,
  `sources/src/libraries/mod.rs:338`, `sources/src/xml/transform.rs:1111`
  (`parse_st_statements`), `mcp/src/tools/parse.rs:53`,
  `project/src/project.rs:333,480` and `project/src/tokenizer.rs:31,121`
  (`tokenize_program`), `ironplc-cli/src/semantic_tokens.rs` (`Token`,
  `TokenType`), and the benchmarks.

### The path after S2

```
source text ──► ironplc_syntax::lexer (lossless tokens)
            ──► region pass   (OSCAT ranged comments, {IF} branches → one trivia token each)
            ──► gates         (dialect gates, string escapes → SyntaxError)
            ──► parser        (rowan CST, recovery)            parse.errors
            ──► lower         (CST → ironplc_dsl::Library | Vec<StmtKind>)
ironplc_parser (facade): CompilerOptions → ParseOptions, pick one Diagnostic, trim fragments,
                         project CST tokens to the legacy Token view
```

Only the first two arrows are new CST-side work; the lexer, gates and parser
exist from S1. `Parse::diagnostics` keeps returning every error (for S4); the
facade reduces them to the one `Diagnostic` the existing `Result<Library,
Diagnostic>` signature allows.

### The lowering mechanism

The principle applied throughout: **one mechanism per class of behaviour;
decisions are rows in tables; a language extension adds a row, not a branch.**

1. **Context.** `LowerCx { file_id, diagnostics, block_ids }` is threaded
   through every rule. `file_id` replaces the post-pass
   `xform_assign_file_id` (`compiler/parser/src/xform_assign_file_id.rs`, a
   whole-AST fold run at the end of `parse_program`, `lib.rs:178`): every `SourceSpan` is built with it
   directly. `block_ids` hands one `next_block_id()` (`dsl/src/common.rs:2129`)
   per `VarBlock` node, which is how the legacy grammar groups the
   declarations of one block.
2. **One disposition table.** `fn disposition(kind: SyntaxKind) -> Disposition`
   has **no wildcard arm**, so adding a `SyntaxKind` (a language extension)
   fails to compile until someone decides its disposition:
   `Lowered(area)`, `Structural` (consumed by its parent's rule),
   `Trivia`, or `Pending`. `Pending` exists only while slices C2-C8 land and
   a test requires it empty from C9. This is the N+1 guard: a new construct
   is one kind, one grammar function, one disposition, one lowering arm, one
   test row.
3. **One lowering function per sum type, dispatching on `SyntaxKind`.**
   `lower_library_element`, `lower_stmt`, `lower_expr`, `lower_type_decl`, …
   each `match` the node kind once. Children are read through a small
   accessor layer (`child(kind)`, `children(kind)`, `token(kind)`,
   `significant_tokens()`), not through ~110 hand-written typed wrappers.
4. **Dsl values are built only with the constructors the legacy grammar uses**
   (`Id::from(..).with_position(..)`, `TypeName::from`, `Integer::new`,
   `SignedInteger::positive`, `Expr::compare`/`binary`/`unary`,
   `Expr::with_span`, `DurationLiteral::seconds`, …). Struct literals are used
   only where no constructor exists. This keeps normalisation (lower-casing in
   `Id`, integer parsing) identical and keeps drift out of the harness.
5. **Decision tables where the legacy PEG encoded a decision in ordered
   choice.** The AST variant is chosen by shape, not by a bag of `if`s:
   - operator class → `ExprKind::Compare` (`OR`, `OR_ELSE`, `XOR`, `AND`,
     `AND_THEN`, `=`, `<>`, `<`, `>`, `<=`, `>=`) or `ExprKind::BinaryOp`
     (`+ - * / MOD **`), from the same level table the grammar uses
     (`compiler/syntax/src/parser/grammar/expressions.rs`, `infix_power`);
   - `(type specification class, initializer class)` →
     `InitialValueAssignmentKind` (`Simple`, `SimpleExpr`, `String`,
     `EnumeratedValues`, `EnumeratedType`, `Subrange`, `Array`, `Structure`,
     `FunctionBlock`, `LateResolvedType`), replacing the ordered alternatives
     of `simple_or_enumerated_or_subrange_ambiguous_struct_spec_init`
     (`compiler/parser/src/parser.rs:1083-1138`);
   - `TypeDecl` shape → `DataTypeDeclarationKind` (`String`, `Array`,
     `Subrange`, `Structure`, `Union`, `Enumeration`, `Simple`, `Reference`,
     …), replacing `type_declaration()` (`parser.rs:759-`);
   - bare name in expression position → `LateBound` or `Variable` by the
     follow set `( [ . ^` (`parser.rs:2269`).
   Each table row gets one test row against the legacy output.
6. **Left-associative chains are lowered iteratively.** The Pratt parser builds
   a left-deep `BinaryExpr` spine without recursion; a naive recursive lowering
   would overflow the stack on a 10,000-term sum. The spine is walked once and
   folded (one helper used for every binary level), with a test.
7. **Totality without panics.** The workspace denies `unwrap`, `expect`,
   `panic`, `todo`, `unimplemented`. A malformed tree (a missing child that
   the grammar guarantees) yields `Diagnostic::internal_error_at(..)`; a
   `Pending` kind yields `Diagnostic::not_implemented(..)`. Lowering is only
   run on a parse with no errors, mirroring the legacy "first tokenisation
   or grammar error ends the parse" contract.
8. **Spans come from CST ranges.** `span(node) = SourceSpan { start, end,
   file_id }` from `SyntaxNode::text_range()`. The S1 sink places trivia
   outside a node before its first token and after its last token
   (`compiler/syntax/src/cst.rs`, header), so a node range equals the legacy
   `span_of_tokens` join of its first and last significant token
   (`parser.rs:273-281`). Spans the legacy grammar sets from a single token
   (unary operator, caret, parentheses) come from `SyntaxToken::text_range()`.

### How each legacy preprocessing step keeps its meaning

| Legacy step (`compiler/parser/src`) | Meaning carried into the AST path | Where it lives after S2 | Slice |
|---|---|---|---|
| `preprocess` (`preprocessor.rs:11-66`, called `lib.rs:113`) | text between `(*@KEY@:NAME*)` and `(*@KEY@:END_NAME*)` is not code | **Region trivia**: the marker pair and its body form retained trivia; the parser never sees the body | C1 |
| lexer errors (`lexer.rs`) | `P0003` | `ErrorToken` + `UnexpectedCharacter` (done in S1) | - |
| `xform_collapse_pragmas` (`allow_pragmas`) | `{…}` is one skippable item | `Pragma` trivia (done in S1) | - |
| `xform_pragma_if` (359 lines, `allow_pragma_if`; `P0023`, `P0024`) | untaken branches never reach the grammar; `{DEFINE}`/`{UNDEFINE}`; unbalanced/unknown conditions are errors | **Region trivia** (`InactiveRegion`), computed by the same region pass from the directive evaluator moved in PF4; errors rendered as `P0023`/`P0024` | C1 |
| `xform_nested_comments` | nested `(* *)` | `LexOptions::nested_comments` (done in S1) | - |
| `xform_split_duration_units` | `T#1m30s` is `T # 1 m 30 s` | adjacent tokens consumed by `DurationLiteral` (S1); value decoded by the shared builder (PF3) | C2 |
| `insert_keyword_statement_terminators` (`allow_missing_semicolon`) | `;` optional after `END_*`, empty `CASE` branches | parser makes `;` optional (S1); lowering ignores terminators; an empty branch lowers to an empty statement list | C4 |
| `xform_statement_labels` (`allow_jump_statement`) | `name :` in statement position is a label | `LabelStmt` (S1); lowering maps it | C4 |
| `xform_demote_keywords` | dialect keywords become identifiers | `ParseOptions::keyword_enabled` (S1), the one table | - |
| `xform_promote_special_operators` | `__NEW` family | `SpecialOpExpr`; `is_special_operator` (S1) | C3 |
| `check_tokens` + 7 `rule_*.rs` | `P0004 P0011 P4033 P4042 P4070 P4067-P4069 P0012` | `GATES` table (`parser/gates.rs:182`) (S1); the facade ranks them (C10) | C10 |
| PEG action values (`Integer::new`, durations, dates, strings) | numeric/temporal/string values | one shared builder in `ironplc-dsl` used by both the S1 validators and the lowering | PF3, C2 |
| `xform_assign_file_id` | `FileId` on every span | `LowerCx.file_id` | C2 |
| `skip_leading_whitespace` (`lib.rs:225`) + trim (`parse_st_statements`) | fragment spans are relative to the trimmed text (audit F8) | the facade trims identically, so spans are byte-identical; the mapping fix is S4 | C11 |

Decisions for the region pass (one mechanism, a two-row rule table):

- It runs between `lex_with` and `gate_errors` in `parse_with`
  (`compiler/syntax/src/parser/mod.rs:71`), so gates and the parser see the
  region as a single trivia token and nothing inside it. Tiling and
  `tree.text() == source` hold by construction (the region token's text is the
  source slice).
- A rule is `{ open, close/evaluate, lexical_errors_inside }`. OSCAT: marker
  pair, lexical errors inside **dropped** (the legacy pre-pass blanked the body
  before lexing, so it never reported them). `{IF}`: directive sequence
  evaluated with the options and the `{DEFINE}` set, lexical errors inside
  **kept** (the legacy lexer ran before the transform and reported them).
- `ParseOptions` gains `allow_pragma_if`: the legacy transform reads it
  (`xform_pragma_if.rs`, `apply`) and the S1 mirror has no such flag.
- Deviation to confirm (open question 3): the legacy pre-pass handles only the
  **first** marker pair of a file and a first marker named `END_*` suppresses all
  (audit F1.3). The region rule applies to every well-formed pair. Matching is
  on tokens, not on raw text, so a marker inside a string literal is not a
  marker.

### The facade (`ironplc-parser`)

- **`legacy` seam (PF1).** Today's bodies of `tokenize_program`
  (`lib.rs:106`), `check_tokens` (`:137`), `parse_program` (`:164`),
  `parse_st_statements` (`:193`) and `skip_leading_whitespace` (`:225`) move to
  a crate-private `legacy` module; the public functions call one selection
  point. The cutover flips that point; the deletion PR removes `legacy`.
  Because `legacy` is `pub(crate)`, only `#[cfg(test)]` code can reach it: the
  oracle never becomes public API and production code has no second path.
- **Options.** `parse_options(&CompilerOptions) -> ParseOptions` is driven by
  `ParseOptions::FLAG_KEYS` and `CompilerOptions::get_flag_by_key` (already used
  by the parity `convert`, `compiler/syntax/tests/parity/legacy.rs`); a test
  asserts every key exists, so a renamed flag cannot silently read as off.
- **One diagnostic.** `Parse::primary_diagnostic(file_id)` ranks by a table over
  `ErrorKind`, mirroring the legacy order (unmatched character; the seven
  rules in `check_tokens` order; `P0023`/`P0024`; grammar), then by byte
  offset. The legacy order is observable today: `tokenize_program` appends
  lexer errors, rule errors, then pragma errors, and `parse_program` returns
  the first (`lib.rs:164-176`). Rows are tested; deviations become entries in
  `CODE_EXCEPTIONS`.
- **Token view.** `tokenize_program` is served from the CST token stream:
  `ironplc_syntax::tokenize(source, options)` (the first half of `parse_with`,
  lex + regions + gates, no parsing) and one `SyntaxKind → TokenType` table.
  `Token.line`/`col` are derived by one position walk (UTF-16 columns, `\r\n`
  as one break, the `line_offset`/`col_offset` arguments applied as today),
  reusing the counting of `lexer.rs:36-70`. The legacy `Token`/`TokenType`
  stay as a projection DTO (`semantic_tokens.rs` is unchanged: S4 may switch it
  to `SyntaxKind` and delete the projection). Disabled keywords project to
  `TokenType::Identifier` through `keyword_enabled`, which keeps the
  `REQ-RTO`/`REQ-PTR` token tests of `parser/src/spec_conformance*.rs` valid.
  Intended differences from the legacy stream are listed in
  [Intended behaviour changes](#intended-behaviour-changes-need-owner-approval)
  (T1-T4).

### How equivalence is proved

1. **Strict AST comparison**, legacy `legacy::parse_program` against
   `lower`, in `compiler/parser/src/tests/parity/`. `Library` derives
   `PartialEq` but that ignores spans (`SourceSpan::eq` is always true,
   `dsl/src/core.rs:166`), ignores the spelling of identifiers (`Id::eq` compares
   `lower_case`, `core.rs:230`), and ignores `VarDecl.block` and `type_id`
   (`dsl/src/common.rs:2164`). The comparator therefore compares (a) the
   `{:#?}` dump, which prints `Id` by `original` spelling and derived spans, (b)
   the full sequence of every `SourceSpan` collected by a `Visitor` overriding
   `visit_source_span` (the visitor already has that hook,
   `dsl/src/visitor.rs:129`; it reaches `Id` spans the dump omits), and (c)
   the **same-block partition** of `VarDecl.block` (ids renumbered by first
   appearance).
2. **Corpus and tables.** The 60 files of `compiler/resources/test`, the
   `compiler/syntax/tests/fixtures` (24), plus the other `.st` sources in the
   repository (`compiler/plc2plc/resources/test`, `compiler/sources/resources/{libs,test}`,
   `compiler/ironplc-cli/resources/test`, `tests/e2e/library`,
   `examples/getting_started.st`) × six presets (the five dialects and
   all-flags, `parity/legacy.rs:presets`) × LF/CRLF/tab variants
   (`common::file_variants`); the statement/expression/declaration snippet
   tables and the bodies and declarations lifted from the legacy unit tests
   (`parity/extract.rs`).
3. **Ratchet.** An input whose tree still contains a `Pending` kind is skipped
   and counted. Each slice raises a `MIN_COMPARED` floor; C9 asserts zero
   skipped. This gives every slice an objective exit condition without waiting
   for the others.
4. **Span classes.** Differences found by the span sequence are classified in a
   `SPAN_EXCEPTIONS` table with the same "unlisted fails, stale fails"
   discipline as the accept/reject tables. Known class: the legacy
   `SourceSpan::default()` for partial-access selectors
   (`parser.rs:1187-1191`); the lowering records the real range. Other classes
   can only be found by the first run (see risks).
5. **Diagnostics.** For every input both paths reject: same problem code and
   same primary byte range for the ranked first diagnostic; message text is
   **not** compared (see risk R4). For inputs one path rejects: the existing
   exception tables.
6. **Whole-workspace dual run.** From C11 a cargo feature `cst-frontend` on
   `ironplc-parser` (off by default, never enabled in release builds) makes the
   facade select the CST path, and `cd compiler && cargo test --workspace
   --features ironplc-parser/cst-frontend` runs the unchanged analyzer (90
   files), codegen (143 files), plc2plc, sources, MCP and CLI suites on the new
   path before any default changes. It is a temporary, test-only switch and is
   deleted with the cutover.
7. After the cutover: `cd compiler && just`, the plc2plc round trips
   (`plc2plc/resources/test/*.st` against the `.rendered` files), the root e2e
   library test, and the docs example build, all unchanged.

### AST ↔ CST mapping

`ironplc_dsl` has 177 public types (`common.rs` 84, `textual.rs` 47,
`configuration.rs` 19, `sfc.rs` 10, `time.rs` 6, `oop.rs` 4,
`member_qualifier.rs` 4, `core.rs` 3); `SyntaxKind` has 113 node kinds. The
mapping is 1:1 for the declaration skeleton and non-1:1 where the legacy
grammar made a decision. Some dsl types are analyzer outputs (`type_id`,
`expr_type`) and have no CST source.

| Area | CST kinds | dsl | Shape of the mapping |
|---|---|---|---|
| Library | `SourceFile`, `TypeBlock`/`TypeDecl`, `NamespaceDecl`, top-level `VarBlock` | `Library`, `LibraryElementKind` | 1:N: one `TYPE` block yields one `DataTypeDeclaration` per `TypeDecl` (`parser.rs:447-455, 759`); namespaces nest and flatten blocks |
| POUs | `ProgramDecl`, `FunctionDecl`, `FunctionBlockDecl`, `InterfaceDecl`, `MethodDecl`, `PropertyDecl`, `GetAccessor`, `SetAccessor`, `MemberQualifier`, `ExtendsClause`, `ImplementsClause`, `ImplementationMarker` | `ProgramDeclaration`, `FunctionDeclaration`, `FunctionBlockDeclaration`, `InterfaceDeclaration`, `oop.rs`, `member_qualifier.rs` | 1:1 |
| Variables | `VarBlock`, `VarDecl`, `Location`, `EdgeSpec`, `AccessDecl`, `InstanceInit` | `VarDecl` (+ `VariableType`, `DeclarationQualifier`, `block`), `AddressAssignment`, `ProgramAccessDecl`, `InstanceInit` | N:1 per block: names of one CST declaration become one `VarDecl` each, all sharing one `BlockId` (to confirm against `vars.rs` in C6); `VAR_STAT`/`VAR_INST`/`VAR_GENERIC` are rows of the block table |
| Types | `TypeDecl`, `ArrayType`, `Subrange`, `SubrangeType`, `StringType`, `RefType`, `ParamsType`, `EnumType`, `EnumValue`, `StructType`, `UnionType`, `StructMember`, `TypeRef` | `DataTypeDeclarationKind`, `SpecificationKind`, `VariableSpecificationKind`, `*Declaration` | decision: shape → kind (table 5 above) |
| Initial values | `Initializer`, `ArrayInit`, `RepeatedInit`, `StructInit`, `StructInitElement`, `EnumValueRef` | `InitialValueAssignmentKind`, `ArrayInitialValueAssignment`, `StructureInitializationDeclaration`, `StructInitialValueAssignmentKind`, `EnumeratedValue`, `LateBound` | decision: `(spec, initializer)` → variant (table 5); literal collapse `negate_literal_constant`/`resolve_initializer_expr` (`parser.rs:61-147`) |
| Expressions | `BinaryExpr`, `UnaryExpr`, `ParenExpr`, `NameRef`, `FieldExpr`, `IndexExpr`, `BitAccessExpr`, `PartialAccessExpr`, `DerefExpr`, `SelfRefExpr`, `DirectAddressExpr`, `CallExpr`, `ArgList`, `PositionalArg`, `NamedArg`, `OutputArg`, `RefExpr`, `NullLiteral`, `SpecialOpExpr` | `ExprKind` (`Compare`, `BinaryOp`, `UnaryOp`, `Expression`, `Const`, `EnumeratedValue`, `Variable`, `Function`, `MethodCall`, `LateBound`, `Ref`, `Deref`, `Null`), `Variable`, `SymbolicVariableKind`, `ParamAssignmentKind` | the CST is uniform (a name is a `NameRef`); the dsl splits by context: variable vs `LateBound` vs function vs method call |
| Literals | `IntLiteral`, `RealLiteral`, `BitStringLiteral`, `BoolLiteral`, `StringLiteral`, `DurationLiteral`, `TimeOfDayLiteral`, `DateLiteral`, `DateTimeLiteral` | `ConstantKind`, `IntegerLiteral`, `RealLiteral`, `CharacterStringLiteral`, `time.rs` | the CST keeps text; the lowering decodes it (value, width, escapes) with the shared builders |
| Statements | `StatementList`, `EmptyStmt`, `AssignStmt`, `AssignOp`, `CallStmt`, `IfStmt`, `ElsifClause`, `ElseClause`, `CaseStmt`, `CaseBranch`, `CaseLabel`, `ForStmt`, `WhileStmt`, `RepeatStmt`, `ExitStmt`, `ContinueStmt`, `ReturnStmt`, `TryStmt`, `CatchClause`, `FinallyClause`, `ThrowStmt`, `JmpStmt`, `LabelStmt`, `CalcStmt`, `WaitStmt` | `StmtKind` (`Assignment`, `FbCall`, `MethodCall`, `If`, `Case`, `For`, `While`, `Repeat`, `Return`, `Exit`, `Continue`, `BeginImplementation`, …) | mostly 1:1; `EmptyStmt` disappears (the legacy `flatten_statements` drops it) |
| SFC | `SfcBody`, `InitialStepDecl`, `StepDecl`, `ActionAssociation`, `ActionQualifier`, `ActionDecl`, `TransitionDecl`, `TransitionPriority`, `StepList`, `TransitionCondition` | `sfc.rs` | 1:1 |
| Configuration | `ConfigurationDecl`, `ResourceDecl`, `TaskDecl`, `TaskInit`, `TaskInitItem`, `ProgramConfig`, `TaskBinding`, `ProgramConnection` | `configuration.rs` | 1:1; `INTERVAL` and `PRIORITY` values are validated (`parser.rs:2086-2092`) |

### Parity exceptions and their resolution

The S1 harness holds **45** exception entries (statement 21, expression 0,
bodies 5, files 3, legacy declarations 6, declarations 10, problem codes 0;
`compiler/syntax/tests/parity/tables.rs:605-806`,
`parity/diagnostics.rs:29`). C1 removes 11; the other 34 are kept **as
recorded behaviour changes** (BC-n) and survive the deletion as permanent
regression tests (C14).

| Entry (count) | Cause | Resolution |
|---|---|---|
| `OSCAT_RANGED_COMMENT` files: `oscat.st` LF/CRLF/tabs (3) | legacy pre-pass blanks the body | **Resolved in C1** by region trivia; entries deleted. This is the one whole-file exception (oscat.st) |
| `PRAGMA_IF` bodies (4) and legacy declarations (4) | `{IF}`/`{END_IF}` evaluated by a separate pass | **Resolved in C1** (region pass, `P0023`/`P0024`); entries deleted |
| `NEEDS_ONE_ITEM` (1), `NO_TRAILING_TRIVIA` (6), `NO_LEADING_TRIVIA` (1), `NO_TERMINATOR_AT_END` (9): all `FragmentEntry` basis | the legacy `parse_st_statements` fragment entry rejects what the legacy parser accepts inside a POU (a comment-only body, trailing comment/pragma, leading pragma, block statement last without `;`) | **Kept, BC-1**: the facade fragment entry follows the in-POU behaviour. PLCopen XML bodies commonly carry comments, so the in-POU behaviour is the intended one |
| `MARKER_AFTER_STATEMENT` (1) | new accepts `x := 1; __BEGIN_IMPLEMENTATION y := 2;` | **Kept, BC-2**; lowers to `StmtKind::BeginImplementation` |
| `SECOND_END_KEYWORD` (3) | with `allow_missing_semicolon`, `;` is optional after every `END_*` | **Kept, BC-3**; dialect-flag only |
| `LONE_CR` (1 body + 1 declaration) | a lone CR is a line break (old Mac files); legacy lexer rejects | **Kept, BC-4** |
| `PRAGMA_CONTENT` (1) | legacy tokenises the inside of a pragma and rejects `{a ? b}` | **Kept, BC-5**: pragma content is not examined except the directive keyword of an `{IF}` family pragma |
| `LEGACY_ABSTRACT_LABEL` (3) | legacy statement-label transform reads `PROPERTY/METHOD ABSTRACT p : T` as a label (a legacy bug) | **Fixed, BC-6**: legacy cannot be the oracle; C7 adds expected-AST tests built by hand |
| `LEGACY_BARE_NAME_INITIAL` (1) | `x : INT := name;` | **Accepted, BC-7**: lowers to `SimpleExpr` with `ExprKind::LateBound`, the existing shape for non-literal initializers; C5 checks the analyzer answers with a diagnostic or a value, never a panic, and otherwise falls back to a lowering-time `P0002` (the legacy verdict) |
| `LEGACY_STRING_WIDTH` (1) | either delimiter accepted for a `STRING[n]` default | **Accepted, BC-8**: the declared width governs (as `parser.rs:1136-1150`); `rule_string_encoding_compat` reports mismatches |
| `LEGACY_EMPTY_LIST` (2) | new rejects `x : ();` and `ARRAY[] OF INT`; legacy accepted them | **Kept stricter, BC-9** (tightening; owner decision, open question 2) |
| `LEGACY_NAMED_GLOBAL_LOCATION` (2) | `VAR_GLOBAL g AT %MW0 : INT;` (standard syntax) rejected by legacy | **Accepted, BC-10**: lowers to a located `VarDecl` |
| `LEGACY_DEMOTED_REF_TO` (1) | without `REF_TO` as a keyword the legacy grammar reads an enumeration named `REF_TO` | **Kept, BC-11**: the CST rejects; dialect gating is the correct reading |
| `CODE_EXCEPTIONS` (0), `EXPRESSION_EXCEPTIONS` (0) | - | nothing to resolve; C10 may add entries only with a reason |

### Intended behaviour changes (need owner approval)

BC-1 to BC-11 above. Token view (`tokenize_program`) differences:

- **T1** a duration lexeme is no longer split: `T#1m30s` projects as `T # 1 m30s`
  (the identifier `m30s` stays one token). Only colouring granularity changes.
- **T2** no synthetic empty `;` tokens (they were zero-width since the S1
  prefactor and are not coloured).
- **T3** a region (OSCAT body, untaken `{IF}` branch) projects as one `Comment`
  token. Legacy projected a blanked OSCAT body as whitespace and dropped an
  untaken branch entirely, so the editor now shows both as comments.
- **T4** a lone CR is a line break (BC-4).

## Prefactoring

Each item is a separate behaviour-preserving PR from `main`; the existing tests
pass unchanged (`development-standards.md#prefactoring`). They come from the
signals the S1 code and the audit show: a duplicated escape/duration/date
decision (grammar-validation in `ironplc-syntax`, value building in the legacy
PEG), a legacy `{IF}` evaluator that a second copy would duplicate, and a
legacy entry point that must stay callable as an oracle after the facade flips.

- **PF1 Legacy seam and dead dependencies.** Move the legacy pipeline into
  `compiler/parser/src/legacy.rs` (`pub(crate)`), keep the three public
  functions as one-line delegations to a single selection point. Remove
  `phf` from `compiler/parser/Cargo.toml` (no use in `parser/src`) and `logos`
  from `compiler/dsl/Cargo.toml` (no use in `dsl/src`); no `cargo-udeps` is run
  by the project, so confirm with `cargo build`. Also move `ironplc-test` to
  `[dev-dependencies]` only if no non-test code uses it (`parser/src` uses it in
  tests only) - otherwise leave it.
- **PF2 Move the S1 parity harness into `ironplc-parser`.** Move
  `compiler/syntax/tests/{parity,parity_*.rs,legacy_vocabulary.rs}` and the
  corpus helpers they use into `compiler/parser/src/tests/parity/`
  (`#[cfg(test)]`, next to the oracle, reaching `legacy` without making it
  public). `ironplc-syntax` drops its `ironplc-parser` dev-dependency;
  `ironplc-parser` gains `ironplc-syntax` as a dev-dependency (promoted to a
  normal dependency in PF4). The tests move unchanged; the harness-only
  exception tables stay as they are.
- **PF3 One literal-value builder.** Move the pure value-building helpers out
  of `compiler/parser/src/parser.rs` into `ironplc-dsl` (new module, e.g.
  `compiler/dsl/src/construct.rs`): the duration combiner and `DurationUnit`
  (`parser.rs:284-345`), time-of-day and date construction (`parser.rs:685-718`),
  `unquote` (`:284`), `negate_literal_constant`, `late_resolved_*`,
  `literal_value_of`, `resolve_initializer_expr` (`:61-147`) and
  `special_operator_type_call` (`:149`). The PEG actions call them. In
  `ironplc-syntax`, `lexer/escapes.rs` (which "mirrors the escape table of the
  legacy pipeline") calls `dsl::string_escape::decode(..).invalid` instead of
  re-implementing the table, and `grammar/temporal.rs`
  (`validate_interval`, `daytime_value`, `date_value`, `days_in_month`) calls
  the shared checks while keeping its own range selection. After this, each
  literal rule exists once. If the PR outgrows one session, split by
  literal class (strings and escapes; temporal; initializer shaping).
- **PF4 `{IF}` directive parser and evaluator into `ironplc-syntax`.** Extract
  `directive`, the condition grammar, `eval`, `symbol` and the define-set logic
  from `compiler/parser/src/xform_pragma_if.rs` into
  `compiler/syntax/src/pragma.rs`, pure functions over pragma text; the legacy
  transform calls them. `ironplc-parser` gets `ironplc-syntax` as a normal
  dependency. The `pragmas.rs` and `pipeline_order.rs` tests pass unchanged.
- **PF5 CST parse rows in the benchmark.** In `compiler/benchmarks`
  (`benches/parse_baseline.rs`, `benches/parse_benchmark.rs`) add
  `ironplc_syntax::parse_source_file` parse-only rows (cold, warm-median,
  allocations, same corpus, same method as experiment §3.5) so the budget
  comparison in C12 has its CST column. The legacy rows are untouched.

Deliberately **not** prefactored: the legacy transforms stay as they are until
the deletion PR; the grammar-side S1 work already moved their meaning to gates,
keyword tables and trivia.

## Design doc reference

- [Parse-Tree Architecture](../design/parse-tree-architecture.md) §3.1
  (lossless CST, provenance), §3.2 (one semantic model, lowering), §4
  (behaviour-preserving migration, no dual production parser), §5 S2 row
  (exit evidence). Updated in C9 (requirements), C13 and C14 (status).
- [Parse-Tree S0 Audit](../design/parse-tree-s0-audit.md) §3 (findings F1-F11),
  §7 (candidates); [Parse-Tree S0 Experiment](../design/parse-tree-s0-experiment.md)
  §2.2 (token consumers served from the CST), §3.5 (benchmark baseline).
- Durable output of this plan, landed in the PRs that earn it (never cited
  back to this file): an ADR recording "lowering lives in `ironplc-syntax`,
  `ironplc-parser` is the compatibility facade" (next unused ADR number),
  `REQ-PT-syntax-NNN` requirements with `#[spec_test]` tests in the design doc
  (C9), the behaviour-change catalogue as permanent tests (C14), and rewrites of
  `specs/steering/syntax-support-guide.md` and
  `specs/design/dialect-token-transforms.md` (C13/C14).

## File map

New:

- `compiler/syntax/src/lower/` (C2-C9) — `mod.rs` (entry points `lower_library`,
  `lower_statements`; `LowerCx`; `disposition`), `names.rs`, `literals.rs`,
  `expressions.rs`, `variables.rs`, `statements.rs`, `types.rs`,
  `initializers.rs`, `var_blocks.rs`, `pou.rs`, `oop.rs`, `sfc.rs`,
  `configuration.rs`; each under 1,000 lines. Unit tests with hand-built
  expected ASTs for forms the legacy path rejects.
- `compiler/syntax/src/lexer/regions.rs` (C1) — the region pass and its rule
  table.
- `compiler/syntax/src/pragma.rs` (PF4) — `{IF}` directive parsing and
  evaluation.
- `compiler/syntax/src/line_index.rs` (C11) — one byte-offset to
  (line, UTF-16 column) mapping.
- `compiler/syntax/build.rs` (C9) — registers the design doc for `REQ-PT-syntax-*`
  (same helper as `compiler/parser/build.rs`).
- `compiler/dsl/src/construct.rs` (PF3) — shared literal and shaping builders.
- `compiler/parser/src/legacy.rs` (PF1; deleted in C14), `frontend.rs` (C11:
  options conversion, selection point), `tokens.rs` (C11: token projection),
  `tests/parity/` (PF2; deleted in C14).

Modified:

- `compiler/syntax/src/syntax_kind.rs` (C1: one trivia kind `InactiveRegion`),
  `error.rs` (C1: `PragmaIfUnmatched`, `PragmaValueExpected`), `diagnostic.rs`
  (C1 mapping to `P0023`/`P0024`; C10 ranking and `primary_diagnostic`),
  `parser/mod.rs` (`parse_with`: region pass; `tokenize` entry), `parser/options.rs`
  (C1: `allow_pragma_if`), `lexer/escapes.rs` and `parser/grammar/temporal.rs`
  (PF3), `lib.rs` (docs: no longer "not used by any production consumer").
- `compiler/parser/src/lib.rs`, `options.rs` (flag key accessor if missing),
  `token.rs` (C14: Logos derive removed, projection DTO kept),
  `tests/comments_and_errors.rs` (C13: the one test pinning legacy message
  text, `parse_program_when_bad_name_then_err`), `Cargo.toml`
  (PF1, PF4, C11, C14), `compiler/dsl/Cargo.toml` (PF1).
- `compiler/benchmarks/` (PF5, C12), `compiler/Cargo.lock`.
- `specs/design/parse-tree-architecture.md` (status and requirements),
  `specs/steering/syntax-support-guide.md`,
  `specs/steering/compiler-architecture.md` (the `xform_*` lines),
  `specs/design/dialect-token-transforms.md`.

Deleted in C14: `compiler/parser/src/{lexer.rs, parser.rs (2,608),
preprocessor.rs, vars.rs, legacy.rs}`, the nine `xform_*.rs` (2,573 lines), the
seven `rule_*.rs` (1,122 lines), `tests/pipeline_order.rs`,
`tests/token_provenance.rs`, `tests/parity/`, the `peg`/`logos` dependencies
and the `debug`/`trace` features that forward to `peg/trace`
(`ironplc-cli/Cargo.toml:14`, `project/Cargo.toml:14` also forward `trace`:
drop or retarget in the same PR).

## Tasks

Plan (this PR, never merged):

- [ ] Issue [#16](https://github.com/boogy777-lgtm/ironplc/issues/16) and this plan reviewed; open questions answered; budgets agreed.

Order: PF1-PF5 in any order except PF2 before PF4 (dependency edge) and PF3
before C2; C1 and C2 can proceed in parallel; C3 → C4 → C5 → C6 → C7 → C8 →
C9; C10 after C2; C11 after C1, C9, C10; C12 after C11 and PF5; C13 after C12;
C14 after C13. Every PR is green under `cd compiler && just` (including
`cargo dupes`) and branched from `main` (a core PR may be stacked on its
unmerged prefactor, per the process). A slice that outgrows one session is
split along the table halves named in it, never merged with a neighbour.

Prefactor PRs:

- [ ] **PF1** legacy seam; remove `phf` (parser) and `logos` (dsl). *Tests:* none edited; one new test that the facade and `legacy` agree. *Exit:* the diff moves code and deletes two unused dependencies.
- [ ] **PF2** move the parity harness to `compiler/parser/src/tests/parity/`. *Exit:* the same tests, same counts (`same_code >= 100`, `files > 180`), `ironplc-syntax` has no `ironplc-parser` dependency of any kind.
- [ ] **PF3** shared value builders; S1 validators call them. *Tests:* legacy `duration.rs`, `literals.rs`, `daytime_fraction.rs`, `late_resolved_initializers.rs`, S1 `temporal` and `escapes` unit tests pass unchanged. *Exit:* `grep` finds each escape/duration/date rule once.
- [ ] **PF4** `{IF}` evaluator into `ironplc-syntax`; parser depends on syntax. *Exit:* `pragmas.rs` tests unchanged; no copy of the evaluator remains in `parser`.
- [ ] **PF5** CST parse rows in the parse benchmark. *Exit:* `cargo bench -p ironplc-benchmarks --bench parse_baseline` prints legacy and CST columns; `cargo bench ... parse_benchmark` compiles under `just lint`.

Core change PRs:

- [ ] **C1 Region trivia** (`compiler/syntax`). Scope: `lexer/regions.rs`, new trivia kind `InactiveRegion`, `allow_pragma_if` in `ParseOptions`, `ErrorKind::{PragmaIfUnmatched, PragmaValueExpected}` rendered as `P0023`/`P0024`, region pass wired into `parse_with` before `gate_errors`. Tests: byte-exact reconstruction and tiling for every region form (nested `{IF}`, `{ELSIF}`, `{ELSE}`, `{DEFINE}` in an inactive branch ignored, unbalanced, two OSCAT pairs, unterminated, non-ASCII body, markers inside strings), error-range tests, and the legacy `pragmas.rs` and `oscat.st` cases through the parity harness. Exit: `FILE_EXCEPTIONS` (3) and the eight `PRAGMA_IF` entries are deleted from the tables and the harness is green; `oscat.st` is accepted by both paths.
- [ ] **C2 Lowering foundation and literals.** Scope: `lower/mod.rs` (`LowerCx`, `disposition`, entry points, the `Pending` mechanism), `names.rs`, `literals.rs` (numeric, bit-string, bool with the `BOOL#`/`BIT#` forms, string with width and escape decoding, duration, time of day, date, date and time, typed prefixes `INT#`, `STRING#`, `UTF8#`), the strict comparator (dump + span sequence + block partition), the ratchet, and the extended corpus. Tests: literal rows from `EXPRESSIONS` through `x := <literal>;`, the legacy literal tests, hand-built expected values, a test that every `SyntaxKind` has a disposition. Exit: `MIN_COMPARED` set for literal-only inputs, comparator self-tests (it detects a changed span, spelling and block partition).
- [ ] **C3 Expressions and variables.** Scope: `expressions.rs`, `variables.rs`: operator table, unary and literal collapse, parentheses, `NameRef` → `Variable`/`LateBound`, field/index/deref/bit/partial/direct access, `THIS`/`SUPER`, calls with positional, named and output arguments, method calls, `REF`/`NULL`, special operators, iterative left spines. Tests: the whole `EXPRESSIONS` table, `expression_spans.rs`, `method_call_expression.rs`, `special_operators.rs`, `partial_access.rs`, a 10,000-term sum. Exit: expression table compared with zero unlisted differences; ratchet raised.
- [ ] **C4 Statements.** Scope: `statements.rs`: assignments including `S=`, `R=`, `REF=`, calls, `IF`, `CASE` (labels, ranges, empty branches), loops, `EXIT`, `CONTINUE`, `RETURN`, try/catch/finally/throw, `JMP`, labels, `CALC`, `__WAIT`, `__BEGIN_IMPLEMENTATION`; terminators ignored. Tests: the `STATEMENTS` table against the legacy **fragment entry and the in-POU verdict**, the lifted legacy bodies, `case.rs`, `jumps.rs`, `try_catch.rs`, `set_reset_bind.rs`. Exit: statement tables compared with only the BC-1/2/3 entries; `parse_statements` path lowered.
- [ ] **C5 Types and initializers.** Scope: `types.rs`, `initializers.rs`: the decision tables of the mechanism section, enumerations (inline, base type, values), subranges, arrays (incomplete, `PARAMS`), strings (both width forms), structures, unions, `REF_TO`/`REFERENCE TO`/`POINTER TO`, struct and array initialisers, repeated init, late-resolved initialisers. Tests: one test row per table row against the legacy output, `arrays.rs`, `enums.rs`, `struct_init_expressions.rs`, `late_resolved_initializers.rs`, `constant_initializers.rs`, `type_alias.rs`, `union.rs`, `pointer_to.rs`, `reference_to.rs`; BC-7, BC-8 and the analyzer check of BC-7. Exit: type and initializer declaration tables compared; BC-9 and BC-11 entries recorded.
- [ ] **C6 Variable blocks and declarations.** Scope: `var_blocks.rs`: every block row of the S1 table (`VAR`, `_INPUT`, `_OUTPUT`, `_IN_OUT`, `_TEMP`, `_EXTERNAL`, `_GLOBAL`, `_ACCESS`, `_CONFIG`, `VAR_STAT`, `VAR_INST`, `VAR_GENERIC`), qualifiers, edge specs, `AT` locations (complete, incomplete, mixed with plain blocks), one `BlockId` per block, top-level `VAR_GLOBAL`. Tests: `var_declarations.rs`, the `var_decl.st`, `var_sections.st`, `var_temp.st` corpus files, the block-partition comparison, BC-10. Exit: declaration table compared with BC-7..BC-11 only.
- [ ] **C7 POUs, OOP members, interfaces, namespaces.** Scope: `pou.rs`, `oop.rs`: `PROGRAM`, `FUNCTION` (return types), `FUNCTION_BLOCK`, `INTERFACE`, `NAMESPACE`, methods, properties with accessors, member qualifiers, `EXTENDS`/`IMPLEMENTS`, body kinds. Tests: `methods.rs`, `property.rs`, `fb_inheritance.rs`, `member_qualifiers.rs`, `namespaces.rs`, `this_super.rs`, `time_functions.rs`, `types_and_returns.rs`; hand-built expected ASTs for BC-6. Exit: POU and OOP corpus files compared; BC-6 tests pass.
- [ ] **C8 SFC and configuration.** Scope: `sfc.rs`, `configuration.rs`: steps, transitions, actions, qualifiers, priorities; configurations, resources, tasks (`INTERVAL`, `PRIORITY` validation), program configurations, connections, instance init, access declarations. Tests: `sfc.rs`, `tasks.rs`, `configuration` and `first_steps_*` corpus files. Exit: no `Pending` kind remains in `disposition`.
- [ ] **C9 Whole-file parity and exception closure.** Scope: `lower_library` total; whole-file comparison over the full corpus × presets × LF/CRLF/tab; the declaration tables and lifted declarations; the `SPAN_EXCEPTIONS` table from the first full run; `REQ-PT-syntax-*` requirements in the design doc with `#[spec_test]` tests (`compiler/syntax/build.rs`); the ADR. Exit: every difference is in an exception table with a reason, none is stale, `Pending` forbidden by a test, `skipped == 0`.
- [ ] **C10 Single-diagnostic policy and diagnostic parity.** Scope: `Parse::primary_diagnostic`, the `ErrorKind` ranking table, lowering's internal diagnostics. Tests: for every rejected input of the corpus × presets, legacy code and primary byte range equal the ranked first diagnostic; each rule of `check_tokens` in the legacy order (inputs with two rule violations); the message-text difference list (R4). Exit: `CODE_EXCEPTIONS` holds only entries with reasons; the `comments_and_errors.rs` message assertion is identified for C13.
- [ ] **C11 Token projection, options conversion, switch.** Scope: `ironplc_syntax::tokenize`, `line_index.rs`, `parser/src/{frontend.rs,tokens.rs}`, `parse_options`, feature `cst-frontend`. Tests: projection against the legacy stream over the corpus, differences restricted to T1-T4; `semantic_tokens.rs` and `project/src/tokenizer.rs` tests with the feature on; options-key test. Exit: `cargo test --workspace --features ironplc-parser/cst-frontend` is green (results pasted in the PR); default build unchanged.
- [ ] **C12 Measurement.** Scope: add the CST+lower column to `compiler/benchmarks`; run three times; record numbers and environment in the design doc next to experiment §3.5; check the budget. Exit: budget met or the overshoot explained and accepted in review.
- [ ] **C13 Cutover.** Scope: the facade selection point calls the CST path; the `cst-frontend` feature is deleted; `legacy` stays `pub(crate)` for the oracle; update the pinned message test; update `parse-tree-architecture.md` (S2 delivered, status), `syntax-support-guide.md`, `compiler-architecture.md`, `dialect-token-transforms.md`, `ironplc-syntax` crate docs. Tests: `cd compiler && just`, plc2plc corpus round trips unchanged, root e2e library test, docs example build. Exit: no production code path reaches `legacy`; the full workspace suite is green with no test edited other than the message pin.
- [ ] **C14 Deletion.** Scope: delete the files listed in the file map and the dependencies; convert the parity tables into permanent tests (BC-1..BC-11 and T1-T4 as accept/reject and expected-AST cases in `compiler/parser/src/tests/`, the OSCAT/`{IF}` cases in `compiler/syntax/tests/`); port the `spec_conformance*.rs` token assertions that use legacy-only mechanics to the projection or to `parse_program` so every `REQ-*-parser-*` keeps a real test; update the design doc status and the audit/experiment notes. Exit: `peg` and `logos` are absent from `compiler/Cargo.lock`, coverage ≥ 85%, `cd compiler && just` green, the `debug`/`trace` feature chain resolved.

Cleanup:

- [ ] Land any decision worth keeping as an ADR or design update (done inside C9/C13/C14).
- [ ] Open issues for anything this plan describes that was not delivered; close the plan PR unmerged and issue #16.

## Exit evidence (design §5, S2 row)

- [ ] **Legacy/new AST/diagnostic comparison.** Strict AST comparison (dump, span sequence, block partition) and ranked-diagnostic comparison over the corpus × presets × LF/CRLF/tab, differences only in exception tables with reasons (C9, C10).
- [ ] **Canonical plc2plc and analyzer/codegen regressions pass.** Workspace dual run before the flip (C11); the full suite unchanged after it (C13).
- [ ] **One production parse path.** `legacy` unreachable from production at C13; deleted at C14; `peg` and `logos` gone.
- [ ] **Losslessness retained.** `tree.text() == source` including region trivia, for valid, malformed and prefix inputs; the S1 lossless tests pass unchanged (C1, C14).
- [ ] **Performance within budget** (C12).

## Risks and how they are detected

| ID | Risk | Detection |
|---|---|---|
| R1 | AST shape drift in decision areas (initialisers, `LateBound`, literal collapse, `Compare` vs `BinaryOp`) | strict comparator over corpus and tables; one test row per decision-table row; the workspace dual run exercises the several hundred `parse_program` call sites of analyzer, codegen and plc2plc tests that the corpus does not |
| R2 | Parse performance regression (CST + AST allocation; the legacy baseline is 4.65-4.70 ms warm-median per corpus pass, 4.78-4.96 ms cold, 31,213 allocations, 4,419.3 KiB, 60 files, 26,203 bytes, i5-9300H, release, experiment §3.5) | PF5 and C12: `parse_baseline` cold/warm/allocations, three runs; allocation counts are exact, times are +-10 % noise (§3.3). **Proposed budget (not recorded anywhere yet; to agree in review):** warm-median corpus sum and allocation count each within 1.5x the legacy baseline, and anything above 1.25x explained in the design doc |
| R3 | Span differences visible to LSP ranges, diagnostics underlines and span-slicing tests | span sequence comparator, `SPAN_EXCEPTIONS` classes, `tests/expression_spans.rs` on the new path, the dual run |
| R4 | Error message and ordering differences: the legacy message is `Expected {tokens}. Found text '{text}' that matched token {type}` from the PEG expected set (`parser.rs:174-210`); the new messages come from the recursive-descent parser. Only one test pins the text (`parser/src/tests/comments_and_errors.rs`). The legacy order is the first of lexer, then seven rules, then pragma, then grammar | C10 ranking table and tests; grep for message text in CLI/MCP/vscode tests (none found by the plan author outside the one test); the exception list in the C10 PR |
| R5 | Stack overflow on deep left-associative chains or deep nesting | iterative spine fold and a 10,000-term test (C3); the S1 depth guard of 96 (`parser/state.rs:MAX_DEPTH`) bounds statements and expressions; add a deep `NAMESPACE`/POU nesting case |
| R6 | Line/column drift in the token view (UTF-16 columns, CRLF, lone CR, fragment offsets) | projection tests against the legacy stream on the corpus with the T1-T4 allowance; `semantic_tokens.rs` tests |
| R7 | Coverage below 85 % while two engines coexist, or `cargo dupes` (`--max-exact-percent 10 --max-near-percent 5`, `--exclude-tests`, `--min-lines 10`) failing on table-like lowering code | `just` on every PR; PF3/PF4 remove the two duplicated areas before they appear; C14 deletes ~7,200 lines of legacy code and must re-check the threshold |
| R8 | Hidden consumers of legacy-only behaviour (token stream, `Token::describe` output, `trace` features) | the `trace` feature chain is listed for C14; CLI `tokenize` output reviewed in C11; the root e2e test and docs example build at C13 |
| R9 | The playground WASM crate depends on `ironplc-parser` (`compiler/playground/Cargo.toml:15`) and gains rowan | the playground build (`partial_playground.yaml`) is run at C11 and C13; the plan author did not build it |
| R10 | Bounded accepted-language drift: new accepts or rejects not in the 34 recorded entries | whole-corpus harness fails on any unlisted difference; stale entries fail too |
| R11 | Totality: a CST shape the lowering does not expect | `disposition` has no wildcard; `internal_error_at` instead of a panic; prefix-sweep and malformed-corpus lowering tests are not run (lowering needs an error-free parse), so recovered trees are never lowered |

## Open questions for the reviewer

1. Confirm **lowering in `ironplc-syntax`** with `ironplc-parser` as the facade
   (decision above).
2. Approve BC-1..BC-11, in particular BC-9 (tightening: `x : ();` and
   `ARRAY[] OF INT` become errors), BC-7 (the analyzer must handle
   `INT := name`; fallback is a lowering-time `P0002`) and BC-4 (lone CR).
3. OSCAT: all pairs per file (the region rule), or first pair only to match
   the legacy bug bug-for-bug? The plan assumes all pairs.
4. Agree the performance budget (R2); the design doc says S0 sets numeric
   budgets from the workload, but none is recorded.
5. Accept the token-view differences T1-T4.
6. Accept the temporary `cst-frontend` feature (test-only, removed in C13) as
   the "comparison path" the design allows in §4.

## Non-goals

- No snapshot API and no consumer rewiring beyond the entry swap: LSP, CLI, MCP
  and build keep calling the same facade functions (S4).
- No local subtree reparse and no incremental reuse (S5); no Salsa and no query
  framework (S3 withdrawn).
- No new accepted syntax: every difference from the legacy parser is one of
  the listed entries; the fragment-entry, ABSTRACT and named-global-location
  entries are fixes of legacy artifacts, not new language.
- No change to semantic analysis, codegen or the VM; the analyzer sees the same
  `ironplc_dsl` AST (modulo the listed entries). The document-position mapping
  for XML fragments (audit F8) and the byte-versus-UTF-16 consolidation stay
  for S4.
