# S0 Audit: Preprocessing and Provenance

Audit of `compiler/parser` text preprocessing and token transforms against the
lossless-CST requirements of [Parse-Tree Architecture](parse-tree-architecture.md)
section 3.1, plus the parser-choice evidence list its section 4 / S0 row requires.
Date: 2026-10-01; branch `lint-fences`. This is audit output only: it changes no
compiler code and claims no stage of the architecture as implemented.

Method: code read of the pipeline (file:line below) plus token dumps produced by a
scratch binary compiled against `ironplc-parser` 0.247.0 from this working tree,
calling `tokenize_program(source, FileId::default(), options, 0, 0)` and comparing
each token's `text` against `source[span.start..span.end]`. Every quoted result is
reproducible from the snippet shown.

## 1. Pipeline and file map

| # | Stage | Code | Consumes | Emits |
|---|---|---|---|---|
| 1 | `preprocess` | `preprocessor.rs:11-66`, called `lib.rs:69` | whole source `&str` | `String` with OSCAT ranged-comment bodies blanked (newlines kept) |
| 2 | logos lexer `tokenize` | `lexer.rs:24-96`; token defs `token.rs:64-536`; called `lib.rs:70` | text | `Vec<Token>` (byte span, line, UTF-16 col, owned text) + diagnostics for unmatched bytes |
| 3 | `xform_collapse_pragmas::apply` | `xform_collapse_pragmas.rs:21-71`; called `lib.rs:72` | token stream | stream with `{...}` runs replaced by one `Pragma` token (gate `allow_pragmas`, `options.rs:450`) |
| 4 | `xform_split_duration_units::apply` | `xform_split_duration_units.rs:18-67`; called `lib.rs:73` | stream | duration-literal identifiers split into letter/digit/`_` runs; `d . d` merged into `FixedPoint` |
| 5 | `insert_keyword_statement_terminators` | `xform_tokens.rs:32-159`; called `lib.rs:74` | stream | stream plus empty-text `Semicolon` tokens (gate `allow_missing_semicolon`, `options.rs:337-340`) |
| 6 | `xform_demote_keywords::apply` | `xform_demote_keywords.rs:48-155`; called `lib.rs:75` | stream (in place) | keyword `TokenType` changed to `Identifier` per dialect gate; `TIME` context rules |
| 7 | `check_tokens` | `lib.rs:86-108`; `rule_*.rs` | `&[Token]` (read-only) | diagnostics only |
| 8 | PEG parse | `parser.rs:172-198`; grammar `parser.rs:338-2296` | `Vec<Token>` | dsl AST; trivia skipped by `parser.rs:394`; spans joined from token indices by `span_of_tokens` (`parser.rs:245-251`) |
| 9 | `xform_assign_file_id::apply` | `xform_assign_file_id.rs:11-28`; called `lib.rs:125` | AST | AST with `FileId` stamped on every span |
| 10 | Fragment entry `parse_st_statements` | `lib.rs:140-167` + `skip_leading_whitespace` `lib.rs:172-197` | ST body text + line/col offset | statements parsed from the trimmed text, spans rebased to it; called by the PLCopen XML adapter `sources/src/xml/transform.rs:1111` (position extraction `sources/src/xml/position.rs:542-580`) |

Consumers of the raw token stream: LSP semantic tokens (`ironplc-cli/src/semantic_tokens.rs:12-14`,
`lsp_project.rs:158`) and the CLI `tokenize` command (`project/src/tokenizer.rs:29-43`).

## 2. Provenance contract today

- A token is `span` (byte offsets from logos, `lexer.rs:41-47`), `line`, `col`
  (UTF-16 code units, `lexer.rs:60-70`), and an owned `text` copy (`token.rs:10-27`, `lexer.rs:50`).
  `SourceSpan` is `{start, end, file_id}` (`dsl/src/core.rs:96-107`); `join` at `core.rs:110-116`.
- The transforms rewrite the stream in place; spans plus text are the only provenance
  carried from lexer to parser. There is no retained original-text buffer.
- The AST holds spans only on selected nodes (idents `parser.rs:426-427`, literals
  `parser.rs:453-464`, statements `parser.rs:2226-2294`); trivia never reach it
  (`parser.rs:387-394`). `grep -ri pragma` finds no match in
  `compiler/analyzer/src`, `compiler/dsl/src`, `compiler/codegen/src`,
  `compiler/plc2plc/src`: comments and pragmas carry no semantics today.
- Parsing is fail-fast: `parse_program` returns only the first diagnostic
  (`lib.rs:116-119`), likewise `parse_st_statements` (`lib.rs:162-164`); a PEG failure is
  one expected-token diagnostic (`parser.rs:146-169`, `172-180`).
- `SourceSpan` compares equal unconditionally (`core.rs:160-167`), so AST comparisons cannot
  detect span changes; existing span tests slice the source explicitly
  (`tests/expression_spans.rs:52-58`).
- Corpus check with this tree's parser: `first_steps.st`, `strings.st`, `expressions.st`,
  `configuration.st`, `comment.st` tile their source (no gaps, every token text equals its
  slice). `oscat.st` does not: 4 whitespace tokens mismatch (section 3.1).

## 3. Transform audit

### 3.1 OSCAT ranged comments: original text destroyed; byte offsets shift on non-ASCII

- Consumes the whole source string; finds the first `(*@KEY@:NAME*)`, then
  `(*@KEY@:END_NAME*)`, and replaces the text between them with spaces (newlines
  kept) so character positions survive (`preprocessor.rs:19-66`, loop `51-65`).
- Lossy in three ways:
  1. The body text is gone before lexing. In the shipped `compiler/resources/test/oscat.st`
     the tokens over the ranged region are `Whitespace` with text `"                        "`
     for span `132..156` where the file holds `version 1.0\t1. jan. 2000` (likewise spans
     `157..175`, `176..192`, `194..210`). No token anywhere carries those bytes.
  2. The substitution is per `char`, not per byte (`preprocessor.rs:56-62`): a multi-byte
     character becomes one 1-byte space, so every later byte offset shifts. Snippet
     `(*@KEY@:DESCRIPTION*)\nWärmebild\n(*@KEY@:END_DESCRIPTION*)\nTYPE\n T : INT;\nEND_TYPE\n`
     yields `Comment text="(*@KEY@:END_DESCRIPTION*)" span=32..57 orig_slice="\n(*@KEY@:END_DESCRIPTION*"`
     and a final coverage gap `(83, 84)` against the original text. All spans after the
     block are wrong by the bytes removed; slicing diagnostics or LSP ranges can produce
     wrong text or split a UTF-8 sequence.
  3. Only the first pair is processed: the function returns after one replacement
     (`preprocessor.rs:23-49`), and a first marker whose name starts with `END_`
     (`37-39`) leaves every pair untouched. Snippet with two pairs leaves
     `second body` as `Identifier` tokens while the first pair is blanked.
- The markers themselves remain as `Comment` tokens (observed above); the OSCAT
  `(*@PROPERTIES_EX@ ... *)` header in `oscat.st` is an ordinary comment and survives.
- CST need (design 3.1): store the original text; represent the ranged region as
  syntax the CST holds intact and only the semantic layer interprets. A blanking pass
  cannot be part of CST construction at all; if any pre-pass survives during migration it
  must be byte-length preserving and return its own mapping.

### 3.2 Lexer error regions: unmatched bytes have no token

- On `Err(_)` the lexer emits a `UnexpectedToken` diagnostic with the span and never a
  token; logos advances past the bad byte(s) (`lexer.rs:74-91`).
- Snippet `a := 1 ? 2;` lexes with a one-byte hole at the `?` (byte `7`): the tokens
  cover `0..7` and `8..11`, and the intervening byte has no token and no error region
  in the stream (1 diagnostic).
- CST need (design 3.1, S1): "every byte ... belongs to a token or error region".
  Error/unexpected tokens (or an error-node kind) must cover unmatched bytes so
  reconstruction and ranges are complete and recovery can make progress.

### 3.3 Pragma collapsing: re-materialized text, inner structure discarded

- Consumes a `LeftBrace ..= RightBrace` token run and emits one `Pragma` token whose
  span joins first and last tokens and whose text is the concatenation of the run's
  token texts (`xform_collapse_pragmas.rs:35-51`, `59-71`, text at `62`, span at `66`);
  line/col come from the opening brace (`67-68`). Gate: `allow_pragmas`
  (`22-24`; on for Rusty/CODESYS/TwinCAT, `options.rs:450`).
- When every byte between the braces is tokenized, text equals the source slice:
  observed `Pragma text="{attribute 'strict'}" span=5..25` for `TYPE {attribute 'strict'}`.
- Text is re-materialized, not sliced, so it breaks exactly where the lexer dropped a
  byte: `TYPE {a ? b}` yields `Pragma text="{a  b}" span=5..12 orig_slice="{a ? b}"`
  (`text != slice`).
- Inner tokens (and any structure inside the pragma), their spans and line/col are
  discarded; the grammar then treats the pragma as skippable trivia (`parser.rs:393`),
  and nothing downstream reads it (section 2). A second collapse pass is not applied,
  and nested braces are deliberately not supported (`xform_collapse_pragmas.rs:8-10`).
- CST need: a pragma node holding either its internal tokens or an exact source slice,
  retained as trivia with dependencies (design 3.3); interpretation stays in lowering.

### 3.4 Duration-token splitting: byte-lossless, original lexeme identity destroyed

- Trigger: `TIME`/`LTIME`, or an identifier spelled `T`, followed by `Hash`
  (`xform_split_duration_units.rs:25-31`), optional adjacent `Minus` (`33-37`), then
  adjacent `Digits`/`FixedPoint`/`Identifier`/`Period` (`39-63`). The identifier (the
  lexer's `[A-Za-z_][A-Za-z0-9_]*`, `token.rs:129`, so ASCII) is split into runs of
  letters/digits/`_` with spans `tok.span.start + offset` and text sliced from the
  original token (`120-158`, spans `148-152`); `Period` between digits becomes a
  re-materialized `FixedPoint` (`71-83`).
- Observed `T#1m30s` -> `T` `#` `1` `m` `30` `s`, each part's text equal to its slice;
  spans partition the lexeme exactly. A single-piece split returns the token unchanged
  (`137-139`), and identifiers outside a duration are untouched (test `216-218`).
- The lexer's single token `m30s` no longer exists downstream: grammar, LSP semantic
  tokens and CLI `tokenize` see three synthetic tokens whose types the lexer never
  produced for that lexeme.
- CST need: keep the original lexeme (or record the derivation) and let duration
  interpretation happen in lowering (`specs/design/time-literals.md` REQ-TL-021/022),
  rather than mutating the shared token stream.

### 3.5 Inserted keyword-statement terminators: invented tokens with borrowed spans

- `semicolon_like` (`xform_tokens.rs:6-14`) creates a `Semicolon` with `text: ""` and a
  span cloned from another token. Two insertion sites, both gated on
  `allow_missing_semicolon` (`37-39`):
  - after `EndIf`/`EndStruct`/`EndWhile`/`EndFor`/`EndCase`/`EndRepeat` when the next
    token is neither `Comment` nor `Whitespace` (`141-151`) — a `Newline` counts;
  - for a completely empty `CASE` branch when the next label/`ELSE`/`END_CASE` arrives
    while the branch is still empty (`55-121`; buffer at `116-119`, insertion at
    `71-77`, `79-91`).
- Observed (CODESYS options): `END_IF\nx := 2;` inserts `Semicolon text="" span=63..64`,
  whose original slice is `"\n"` — a real byte range owned by the Newline token.
  `END_IF (* c *)\n;` also inserts one (`span=67..68`) before the explicit `;`, i.e. an
  invented empty statement even where the source was already terminated.
- Observed empty CASE branch `1:\n2: y := 1;`: inserted `Semicolon text="" span=14..15`
  is emitted before the buffered label `Digits span=13..14` — stream spans are
  non-monotonic and span `14..15` is held by both the synthetic semicolon and the real
  colon. The comment at `xform_tokens.rs:144-146` records the known defect ("TODO remove
  the span and line/col").
- CST need: missing terminators as recovery/lowering markers anchored between real
  tokens (zero-width, or a node with an explicit empty range), never sharing a span;
  synthetic tokens must not alter reconstruction (design 3.1).

### 3.6 Demoted keywords: dialect classification applied destructively

- Changes `token_type` only; text, span, line and col are untouched (`xform_demote_keywords.rs:85-88`).
  Flag groups at `50-84` (`LTIME`/`LDATE`/`LTOD`/`LDT`, `REF_TO`/`REF`/`NULL`,
  `REFERENCE`, `POINTER`, OOP words, `AND_THEN`/`OR_ELSE`, `PERSISTENT`, `CONTINUE`);
  the dialect presets in `options.rs:331-470` define which dialects enable each gate.
- `TIME` is context-sensitive: demoted before `(` or `:=`, or after `FUNCTION`, and only
  when `allow_time_as_function_name` is set (`133-155`); neighbor scans skip only
  `Whitespace`/`Newline` (`95-123`), so comments/pragmas between tokens matter.
- Observed: `TYPE R : REF_TO INT;` gives `Identifier` under Ed2 and `RefTo` under Ed3
  with identical text/span.
- CST need: classification is a function of (token text, dialect options), so the CST
  stores the word and the effective options are a snapshot input (design 3.3); consumers
  must be able to recompute keyword vs identifier for any dialect from the same tree.
- Ordering coupling: demotion runs *after* terminator insertion (`lib.rs:74-75`), which
  dispatches on `TokenType::Continue` as an unambiguous statement start
  (`xform_tokens.rs:108-115`) before demotion can hide it. The pipeline order is part of
  current token-stream semantics.

### 3.7 Lexer positions: three coordinate systems, copied text, cloned file ids

- Byte spans (`lexer.rs:41-47`) coexist with UTF-16 columns and line numbers
  (`60-70`); `\r\n` lexes as one `Newline` token (`token.rs:66-69`); multi-byte and
  surrogate-pair characters are covered by tests (`lexer.rs:166-179`).
- `Token.text` is an owned `String` copy of the slice (`token.rs:26`, `lexer.rs:50`);
  `file_id` is cloned per token (`lexer.rs:43-47`, with a performance TODO).
- `SourceSpan`'s fields are documented as "character" positions (`dsl/src/core.rs:97-105`)
  but all consumers index bytes (`tests/expression_spans.rs:52-58`), and section 3.1's
  observed mismatches only make sense byte-wise.
- CST need: one authoritative coordinate system (green tree with byte ranges + text),
  with line/col derived per snapshot for LSP; the byte-vs-character wording should be
  fixed before more code depends on it.

### 3.8 Entry points and adapters

- `parse_program` returns the first diagnostic and stops (`lib.rs:116-119`); there is no
  multi-error recovery today (S1 exit evidence expects recovery progress/termination).
- `parse_st_statements` trims leading whitespace and trailing text (`lib.rs:147-153`,
  `196`), rebases line/col (`172-197`), and parses the trimmed text, so byte spans in
  XML-embedded statements are relative to the trimmed fragment, not the container
  document. The adapter supplies decoded XML text plus row/col offsets
  (`sources/src/xml/position.rs:542-580`).
- `xform_assign_file_id` is post-AST and affects only `file_id` (`xform_assign_file_id.rs:20-28`).
- CST need: an explicit source-mapping boundary for embedded fragments (design 3.1
  "Container decoding/XML escaping remain source-adapter concerns"; section 4
  "Snapshot-bound positions").

### 3.9 Token check rules

- All five rules take `&[Token]` and return diagnostics (`lib.rs:86-108`; e.g.
  `rule_token_no_c_style_comment.rs:8-36`). No provenance impact; they become
  CST-based diagnostics with the same problem codes.

## 4. Findings

| ID | Severity | Finding | Evidence |
|---|---|---|---|
| F1 | blocker | OSCAT ranged-comment bodies are replaced by spaces before lexing; the original bytes never enter the token stream; only the first pair per file is handled; a non-ASCII body shifts every later byte span. | `preprocessor.rs:19-66`; observed `oscat.st` mismatches spans `132..210`; non-ASCII snippet shift + gap `(83,84)` |
| F2 | blocker | Lexer-erroneous bytes have no token or error region, so the stream does not tile the source. | `lexer.rs:74-91`; observed gap `(7,8)` for `a := 1 ? 2;` |
| F3 | blocker | Inserted terminators are synthetic tokens with `text: ""` that clone another token's span; CASE fixups emit them out of span order; insertion fires on `Newline` even when an explicit `;` follows. | `xform_tokens.rs:6-14`, `71-91`, `141-151`, TODO `144-146`; observed spans `63..64`, `67..68`, `14..15` |
| F4 | major | Pragma text is re-materialized from token texts, so it diverges from the source slice when the lexer dropped a byte; inner tokens/positions are discarded and pragmas carry no semantics. | `xform_collapse_pragmas.rs:59-71`; observed `{a ? b}` -> text `"{a  b}"`, span `5..12` |
| F5 | major | Duration splitting is byte-lossless but destroys the lexer's original token; downstream consumers see only synthetic parts. | `xform_split_duration_units.rs:120-158`; observed `T#1m30s` parts |
| F6 | major | Keyword demotion is destructive classification; `TIME` depends on neighbors and options; classification cannot be recomputed from the post-pipeline stream alone. | `xform_demote_keywords.rs:59-88`, `95-123`, `133-155`; observed `REF_TO` Ed2 vs Ed3 |
| F7 | major | Pipeline order couples transforms: terminators are inserted before demotion, so their decisions use pre-demotion token types. | `lib.rs:74-75`; `xform_tokens.rs:108-115` |
| F8 | major | Fragment parsing trims and rebases; spans are relative to the trimmed ST text with no document mapping. | `lib.rs:147-153`, `172-197`; `sources/src/xml/transform.rs:1111` |
| F9 | major | Fail-fast: at most one syntax diagnostic per parse; the PEG grammar has no recovery. | `lib.rs:116-119`, `162-164`; `parser.rs:146-169`, `172-180` |
| F10 | minor | Three coordinate systems (bytes, UTF-16 columns, lines) and a `SourceSpan` doc that says "character" while callers index bytes. | `lexer.rs:41-70`; `dsl/src/core.rs:97-105`; `tests/expression_spans.rs:52-58` |
| F11 | minor | `Token.text` copies every lexeme and `FileId` is cloned per token. | `token.rs:26`; `lexer.rs:43-47`, `50` |

## 5. Parser-choice evidence for S0

### 5.1 Option A: extend the in-tree PEG seam

- `peg = "0.8.5"` in the manifest (`compiler/parser/Cargo.toml:25`), resolved to 0.8.6
  (`compiler/Cargo.lock:1530-1533`), MIT licensed (crates.io metadata, checked 2026-10-01).
- The grammar parses tokens, not text: `parser! { grammar plc_parser<'a>(tokens: &'a [Token]) for SliceByRef<'a, Token> }`
  (`parser.rs:338-339`; element trait `310-336`). 303 rules; 177 `TokenType` variants
  (`token.rs:64-536`).
- Trivia are skipped by `_ = (whitespace() / comment() / pragma())*` (`parser.rs:394`),
  so CST emission would have to retain them explicitly.
- Spans are captured per rule by hand (`position!()` + `span_of_tokens`, `parser.rs:245-251`,
  examples `453-464`, `539-562`, `2045-2092`); most rules carry no span at all.
- Failure handling: one `peg::error::ParseError` (token index + expected set) mapped to a
  single diagnostic (`parser.rs:146-169`, `172-180`, `186-198`); no error productions,
  error nodes or resynchronization in the grammar.
- Tooling: `debug`/`trace` features (`Cargo.toml:13-15`) and pegviz markers
  (`parser.rs:343-354`). `build.rs` does not generate the parser: it emits
  spec-requirement test data only (`compiler/parser/build.rs:1-14`,
  `[build-dependencies]` `compiler/parser/Cargo.toml:27-28`); grammar code lives
  in the `parser!` proc-macro invocation and its actions.

### 5.2 Option B: scoped truST `trust-syntax` reuse

External facts (checked 2026-10-01; to be re-verified in S0):

- crates.io crate `trust-syntax` 0.2.2, "Lexer, parser, and concrete syntax tree for
  IEC 61131-3 Structured Text"; license `MIT OR Apache-2.0`; rust-version 1.85; first
  published 2026-06-11 (4 versions).
- Its dependencies are `logos`, `rowan`, `text-size`, `drop_bomb` — i.e. it already
  produces a rowan CST, the storage this design selects.
- Source organization: GitHub `johannesPettersson80/trust-platform`
  (LICENSE-MIT and LICENSE-APACHE in tree), crates `trust-syntax`, `trust-hir`,
  `trust-ide`, `trust-lsp`, `trust-plcopen`; `crates/trust-syntax/src/` holds
  `lexer/`, `parser/` (event/sink architecture: `parser/event.rs`, `parser/parser.rs`,
  grammar modules `declarations.rs`, `expressions.rs`, `statements.rs`, `pou/`) and
  `syntax/`.
- The design's boundary: reuse is scoped to syntax only; "Do not attach truST
  HIR/IDE/LSP as another semantic backend" (design section 4; `Codesys/README.md`,
  borrowing boundaries).

### 5.3 Assessment criteria (design section 4: coverage, provenance, recovery, maintenance, licensing, API cost)

| Criterion | Option A: extend PEG | Option B: scoped `trust-syntax` | S0 evidence still to produce |
|---|---|---|---|
| Coverage | 303 rules cover today's accepted language; 17-item P0 gap backlog defines the target (`Codesys/LEXER-GAP-ANALYSIS.md` section 13); corpus is 48 `.st` files in `compiler/resources/test/` | Unknown against IronPLC extensions: partial access, `REFERENCE TO`/`POINTER TO`, `PROPERTY` and member qualifiers, `{pragma}`, OSCAT comments, `CONTINUE`, PLCopen XML fragments | Run one corpus (standard + CODESYS dialect + malformed) through both; differential snippets per section 13/14.4; record accept/reject diff counts |
| Provenance | Tokens already carry byte span + text; CST emission needs new rule actions and trivia retention; transform findings F1-F9 must move to lowering | Rowan tree already exists; needs mapping to `SourceSpan` (bytes) and UTF-16 columns, `file_id`, and byte-exact reconstruction tests (CRLF, tabs, Unicode) | Reconstruction harness: `tree.text() == original` over the corpus; coordinate-mapping tests |
| Recovery | None today: single diagnostic, no error productions (`parser.rs:146-169`); S1 expects progress/termination | Event/sink parser suggests error nodes exist | Malformed corpus through both; assert complete ranges, progress and termination |
| Maintenance | 303 rules plus the transform modules are owned in-tree; `peg` is stable and MIT | External pre-1.0 crate (0.2.2, low adoption), version pin or vendoring; its dialect surface must track IronPLC flags independently | Dependency-version record and update policy; measure rule/API churn between releases |
| Licensing | MIT (`compiler/Cargo.toml:32`) | `MIT OR Apache-2.0`; compatible; Apache NOTICE handling if vendored | Confirm crate license file and whether `trust-syntax` is used as a crates.io dep or vendored |
| API cost | Existing consumers keep working: LSP semantic tokens (`ironplc-cli/src/semantic_tokens.rs:12-14`), CLI `tokenize` (`project/src/tokenizer.rs:29-43`), fragment entry (`lib.rs:140-167`); the grammar gains CST actions | Adapter from trust-syntax kinds to the dsl AST and to IronPLC options; keep dsl as the semantic authority (design 3.2); dialect options injection | Prototype the adapter for one POU + one fragment entry; measure code size and public-API delta |

### 5.4 CODESYS material already available

- `Codesys/docs/01_LEXER_PARSER.md`: char-level scanner plus recursive-descent parser
  with sub-parsers and an `ErrorHandler` (sections 2-4); scanner options for
  whitespace/EOL/comments/pragmas (`GetNext` filter, section 3.2); separate pragma
  scanner and pragma grammar in the parser (section 3.9); operator/token tables
  (section 6). Token fields include `SourceOffset`/`Length`/`SourceLine`/`SourceColumn`
  and optional position pragmas (section 2) — direct evidence of a lossless-oriented
  scanner design.
- `Codesys/grammar/AST_MAPPING.md`: grammar-rule -> parser file:line -> builder -> node
  map (sections 1-6) and dispatcher/resynchronization table (section 7, including
  `Utilities/ResynchronizerTables.cs:26`, 23/12 operators). This is the inventory of
  what recovery and statement/declaration dispatch must cover.
- Both are decompiled references, not transplantable code, and explicitly not the
  IronPLC implementation contract (design section 2). `Codesys/LEXER-GAP-ANALYSIS.md`
  section 13 (P0-1..P0-17) and section 14.4 define the coverage target and the
  differential verification method.

### 5.5 Constraints on the decision

- One production parser only; the alternative is a test-only comparison removed at
  cutover (design section 4 and S0 row).
- S0 must land: parser choice with measurements, dependency versions, the file map of
  section 1, and a benchmark baseline.

## 6. S0 exit-evidence status

| Evidence | Status | Where |
|---|---|---|
| Standard corpus | Present: 48 `.st` files | `compiler/resources/test/` |
| CODESYS corpus | Absent: no dialect corpus derived from the CODESYS tables; section 14.4 of the gap analysis proposes one snippet per P0 item | - |
| Malformed corpus | Thin: `first_steps_syntax_error.st`, `c_style_block_comment.st`, `comment.st` | `compiler/resources/test/` |
| OSCAT | Present, ASCII-only: `oscat.st` plus preprocessor unit tests; no non-ASCII ranged-comment case | `preprocessor.rs:68-142` |
| Unicode | Partial: `strings.st`; UTF-16 column tests | `lexer.rs:166-179` |
| Pragmas | Present: 4 tests | `parser/src/tests/pragmas.rs` |
| Parser choice | Decided after this audit: Option B — in-tree recursive-descent/Pratt replacement producing the rowan CST; `trust-syntax` not adopted (S0 experiment §2.5) | [Parse-Tree S0 Experiment](parse-tree-s0-experiment.md) |
| Dependency versions | peg 0.8.6 (`Cargo.lock:1530-1533`), logos 0.16.1; `rowan`, `salsa`, `text-size`, `drop_bomb` absent from `compiler/Cargo.lock` (checked) | `compiler/Cargo.lock` |
| File map | Section 1 | this document |
| Benchmark baseline | Partial: compile -> VM benchmarks only, no tokenize/parse-only measurement | `compiler/benchmarks/src/lib.rs:15-36`, `benches/st_benchmark.rs` |
| No dual production parser | Holds today: one PEG grammar with two entry rules | `parser.rs:172-198` |

## 7. Prefactoring candidates (design S0: "record prefactoring")

1. Make coordinate semantics explicit before anything depends on them: fix the
   `SourceSpan` byte-vs-character doc (`dsl/src/core.rs:97-105`) and state that columns
   are UTF-16 (`lexer.rs:60-70`).
2. Replace borrowed-span synthetic tokens with explicit zero-width markers
   (`xform_tokens.rs:144-146` TODO) so the token stream can satisfy a coverage/
   reconstruction invariant while the PEG parser still runs.
3. Give the OSCAT handling a non-destructive representation (region view or lowering
   rule) instead of a source-rewriting pass (`preprocessor.rs:51-65`).
4. Centralize the pipeline order and dialect gating (`lib.rs:69-76`) as documented
   token-stream semantics, since findings F6/F7 show order is observable.
5. Keep the transform modules' byte-level invariants testable with a coverage helper
   (tokens tile the source; text equals slice) so later refactors cannot regress F1-F4
   silently.

## 8. Scope note

This audit covers preprocessing/provenance and parser-choice evidence only. The S0
integration experiment has since run: rowan/Salsa adapter validation, dependency
selection, and the parse/tokenize benchmark baseline are recorded in
[Parse-Tree S0 Experiment](parse-tree-s0-experiment.md).

Repo note (history): the `specs` plan-citation check failed on `Codesys/README.md`,
which named the implementation-plan directory; the S0 close-out corrected the Codesys
routing document, and the check no longer hits it. Close-out amendment, 2026-10-01:
the §6 parser-choice row and this note.
