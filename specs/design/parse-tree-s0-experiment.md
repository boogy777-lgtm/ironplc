# S0 Experiment: rowan/Salsa Spike, Parser Choice, Parse Baseline

date: 2026-10-01
branch: `lint-fences` (S0 evidence complete)

S0 of the approved [Parse-Tree Architecture](parse-tree-architecture.md)
("integration experiment"). This document records what was built, what was
measured, and the single parser recommendation; it claims no stage of the
architecture as implemented. S0 is experiment and evidence, **not** a second
production parser: the spike crate is test-only, no production crate depends
on it, and it is deleted at the S1 cutover (design section 4).

Preprocessing/provenance audit and the evidence list this experiment answers:
[Parse-Tree S0 Audit](parse-tree-s0-audit.md).

## 0. Deliverables and reproduction

| Deliverable | Location |
|---|---|
| Spike crate (rowan CST + Salsa adapter + baseline harness) | `compiler/s0-spike/` |
| Performance baseline numbers | Section 3 of this document |
| Parser recommendation | Section 2 of this document |

Commands (from `compiler/`):

```
cargo test -p ironplc-s0-spike                                    # 30 tests
cargo run --release -p ironplc-s0-spike --bin parse_baseline -- 50
```

The spike crate is package `ironplc-s0-spike` version `0.0.0`,
`publish = false`. `cargo tree --workspace -i ironplc-s0-spike` resolves only
to the crate itself: no production crate (codegen, CLI, LSP, MCP, build)
depends on it, and nothing here is wired into codegen or the CLI.

## 1. rowan/Salsa adapter validation

### 1.1 Spike shape

| Module | Lines | What it does |
|---|---|---|
| `src/lexer.rs` | 434 | Byte-exact ST tokenizer over the **original** text: trivia kept, unmatched bytes become `ErrorToken`s so tokens tile the source; `check_coverage` proves tiling/text-slice equality |
| `src/syntax.rs` | 188 | `SyntaxKind` (5 trivia kinds, 6 token kinds, 19 node kinds) and the `rowan::Language` binding |
| `src/parser.rs` | 693 | Recursive-descent POU/statement parser + Pratt expressions; event stream (`Start`/`Token`/`Finish`, forward-parent hoisting); error nodes, sync sets, depth guard (96) |
| `src/cst.rs` | 201 | Event sink into `rowan::GreenNodeBuilder` (trivia re-inserted in source position), red-view helpers, S-expression dump, tree stats |
| `src/db.rs` | 235 | Salsa input/tracked/consumer queries with an event log |
| `src/baseline.rs` | 400 | Production parse-path measurement (section 3) |
| `src/bin/parse_baseline.rs` | 25 | Harness entry point |
| `tests/cst_lossless.rs` | 250 | 15 losslessness/recovery/construct tests |
| `tests/salsa_recompute.rs` | 109 | 4 recomputation tests |
| `tests/baseline_smoke.rs` | 38 | 1 end-to-end harness test (same global allocator as the bin) |

The grammar slice is: one POU form (`PROGRAM` / `FUNCTION_BLOCK` /
`FUNCTION`) with `VAR*` blocks (including `CONSTANT`/`RETAIN`/`PERSISTENT`
qualifiers and `AT` direct variables), and the statements assignment, call,
`IF`/`ELSIF`/`ELSE`, and empty; expressions cover `NOT`, unary `+`/`-`,
`OR`/`XOR`/`AND`, comparisons, additive, multiplicative (`*`, `/`, `MOD`),
calls, parentheses, indexing, and `^` dereference. This is a slice, not the
language.

### 1.2 Lossless CST evidence (design section 3.1)

`cargo test -p ironplc-s0-spike` — **33 passed, 0 failed**
(13 unit, 15 `cst_lossless`, 4 `salsa_recompute`, 1 `baseline_smoke`):

- **Byte equality.** `tree.text() == source` for a POU with CRLF line
  endings, tabs, a block comment, a `{attribute 'strict'}` pragma, and an
  `IF` body; for a source with a non-ASCII comment/string
  (`Wärmebild`); and for every prefix of the sample text (the prefix sweep
  also proves the parser terminates on partial input, including partial
  tokens and partial constructs).
- **Trivia retention.** Newline tokens keep `"\r\n"` spelling; the pragma
  token is present in the tree as trivia (counted), comments are retained;
  the sink re-inserts trivia between significant tokens in source position.
- **Error regions (audit F2).** `x := 1 ? 2;` yields a `SyntaxKind::ErrorToken`
  whose text is `"?"`; reconstruction is exact and no byte is unowned.
- **Recovery and progress (audit F9).** `x := 1 + ;` produces a diagnostic
  and an `ErrorNode`, then resumes: both assignments in the input are parsed
  as `AssignStmt`. Junk between POUs recovers and still parses both POUs.
  An unterminated block comment yields a lexer diagnostic with exact
  reconstruction. `check_coverage` reports zero gaps and zero text/slice
  mismatches for every source exercised in the tests. 400 nested parentheses terminate through the depth guard
  with a diagnostic, still reconstructing exactly.
- **Tree shape.** `x := 1 + 2 * 3;` builds
  `(BinaryExpr (LiteralExpr 1) + (BinaryExpr (LiteralExpr 2) * (LiteralExpr 3)))`,
  i.e. Pratt precedence and the forward-parent hoisting are correct; red
  navigation (`descendants`, `children_with_tokens`) is the only tree
  consumer.

### 1.3 Salsa adapter evidence (design section 3.3)

Version exercised: **salsa 0.28.5** (Apache-2.0 OR MIT, MSRV 1.88;
selected and locked by this experiment). One input (`SourceFile { text }`),
one tracked query `declaration_summary` (lexes the text with the spike
lexer and extracts POU kind/name and variable name/type pairs), one consumer
`interface_fingerprint`. Re-execution is observed through Salsa's own
events (`EventKind::WillExecute`, `EventKind::DidValidateMemoizedValue`),
recorded by the database's `Storage::new(Some(...))` callback:

- **Memoization.** A second call in the same revision executes nothing
  (`WillExecute = 0`); Salsa returns the memo directly.
- **Recomputation on change / backdating.** After a whitespace+comment-only
  edit (`set_text`), the summary query re-executes exactly once
  (`WillExecute = 1`) and returns an **equal** value; the consumer query is
  then validated from cache without re-execution
  (`WillExecute = 0`, `DidValidateMemoizedValue ≥ 1`) — the design's
  "equal semantic summaries stop propagation" behavior, of the section 3.3
  dependency table's first row.
- **Propagation on meaning change.** Changing a variable's type changes the
  fingerprint and re-executes the consumer (`WillExecute ≥ 1`); renaming the
  POU is reflected in the new summary.

API facts recorded for S1 (from the pinned 0.28.5 sources):

| Concern | Salsa 0.28.5 surface |
|---|---|
| Value equality policy | Backdating compares old/new results with `PartialEq` (`lib.rs` "Tracked functions and memoized values"); `no_eq` disables it. Reuse is authorized by equality, not by hashes, so no collision policy is needed for memo reuse itself |
| Return modes | `ref` (default), `clone`, `copy`, `deref`, `as_ref`, `as_deref`; default `ref` ties results to the database borrow, so snapshot-like callers use `clone`/`copy` or clone at the boundary |
| Cancellation | `Database::unwind_if_revision_cancelled`, `trigger_cancellation`, `cancellation_token`; exported `CancellationToken`; `WillCheckCancellation` event |
| Cycles | Tracked-function options `cycle_initial` / `cycle_recovery` (fixpoint iteration; `WillIterateCycle` / `DidFinalizeCycle` events with iteration counts) and the `salsa::Cycle` type |
| Snapshots | No `Snapshot` type in 0.28.5: isolation is the immutable `&db` borrow plus `Storage: Clone` for keeping a second handle |
| Custom values | A tracked function returning a custom struct needs `salsa::SalsaValue` (derive) and `PartialEq`; the spike's summary derives it |

The spike does not exercise cancellation or cycle recovery; S1's acceptance
criteria (cycles, deletion/rename, cancellation of stale revisions)
still need tests when the analyzer is wrapped.

### 1.4 What the spike does not establish

- No full grammar, no dialect gating, no dsl lowering: the spike proves the
  adapter mechanisms, not language parity.
- No incremental *syntax* reuse (S5): text change re-lexes and re-parses the
  whole file; Salsa only reuses semantic results whose dependencies did not
  change.
- No performance claims about rowan construction beyond the numbers in
  section 3 (and those measure the current PEG path, not the spike).
- No LSP/CLI integration: existing token consumers were not changed.

## 2. Parser-choice assessment

Inputs: audit section 5, `Codesys/docs/01_LEXER_PARSER.md` (char-level
scanner; recursive-descent `InternalParser` with sub-parsers and an
`ErrorHandler`), `Codesys/grammar/ST_GRAMMAR.ebnf` (401 lines of recovered
CODESYS grammar), and the spike in section 1.

### 2.1 Option A — extend the in-tree PEG seam

Measured facts on this tree:

- `compiler/parser/src/parser.rs` is 2,296 lines; `grep -oE '\brule [a-z_0-9]+' | sort -u` counts **319 unique rule names**; the token model has
  177 `TokenType` variants (`token.rs`, 1,061 lines, 721 non-test).
- The grammar consumes the **post-transform token stream**
  (`parser.rs:338-339`), and the transforms are the destructive ones the
  audit records (F1–F7): OSCAT blanking before lexing, pragma collapsing
  (re-materialized text), duration splitting, synthetic semicolons with
  borrowed spans, dialect keyword demotion. The architecture requires the
  CST to be built from the original decoded text **before** preprocessing
  (design 3.1), so this is not only a grammar change: the pipeline order and
  every transform's provenance must move behind lowering first.
- Trivia are skipped by one rule (`parser.rs:394`); retaining them for a CST
  means every rule's actions must weave trivia tokens into the tree.
- Spans are captured by hand per rule shape (`position!()` +
  `span_of_tokens`), and most rules carry none.
- Failure handling is one `peg::error::ParseError` → one diagnostic
  (`lib.rs:116-119`): fail-fast, no error productions, no resynchronization,
  nothing in the peg crate to build recovery on.
- Tooling: `peg` 0.8.6 (MIT) is stable and in-tree; `debug`/`trace`
  features exist. The grammar is not generated by `build.rs`.

Effort for the S1 target is therefore: rework the pipeline provenance,
rewrite all 319 rules' actions for CST emission + trivia, and add a recovery
layer outside the grammar (statement-level resynchronization like the old
CODESYS parser) — i.e. most of Option B's work, inside a macro DSL with less
control over recovery and node boundaries.

### 2.2 Option B — scoped recursive-descent/Pratt replacement

Coverage starts behind (319 rules vs the spike's one-POU slice) and must be
rebuilt against the corpus, dialect-flag tests, and the P0 gap backlog
(`Codesys/LEXER-GAP-ANALYSIS.md` section 13). The other five criteria are
structurally better:

- **Provenance:** lexer over original text; every token consumed exactly once
  into the tree; error tokens for unmatched bytes. Demonstrated end-to-end by
  the spike (byte-exact reconstruction including CRLF/tabs/Unicode/errors).
- **Recovery:** error nodes with explicit sync sets and a depth bound are a
  property of the parser, not an add-on. The spike demonstrates progress and
  termination on malformed input and every prefix.
- **Maintenance:** more lines but no code-generation macro layer; the
  CODESYS reference is itself RD (`InfixOperationParser` is an
  operator-precedence expression parser), and the spike's Pratt core is
  ~120 lines.
- **Licensing:** own code; no new obligations.
- **API cost:** the seam changes (new crate/module), but the token-consuming
  consumers (LSP semantic tokens, CLI `tokenize`) can be served from the CST
  token stream; dsl stays the semantic authority via lowering.

### 2.3 truST (`trust-syntax`) facts — verified 2026-10-01

The audit's Option B record must be corrected; the crate named
`trust-syntax` on crates.io is **not** the IEC 61131-3 project:

- crates.io `trust-syntax` 0.2.2 is "Parsing and re-emission for the Trust
  dialect" — a strict **Rust** dialect by `briannadoubt/trust`; its
  dependencies are `syn`, `quote`, `prettyplease`, `proc-macro2`, and all
  four published versions (0.1.0, 0.2.0, 0.2.1, 0.2.2) are that project.
  `trust-hir`, `trust-ide`, `trust-plcopen` do not exist on crates.io at all
  (`trust-lsp` 0.2.2 is the same Rust-dialect project).
- The real truST platform is the GitHub repository
  `johannesPettersson80/trust-platform` (verified by shallow clone at
  commit `be8d81a4a7ab16ca7554b8be0f4723161ec1a47b`): workspace version
  0.24.69, `license = "MIT OR Apache-2.0"` with `LICENSE-MIT` and
  `LICENSE-APACHE` in the tree, MSRV/Rust 1.95. `crates/trust-syntax` is
  6,393 lines using `logos` 0.16 + `rowan` 0.16 + `text-size` + `drop_bomb`,
  with an event/sink parser (`parser/event.rs`, `parser/sink.rs`) and error
  recovery (`recover_to_sync_point`, `recover_statement`, bounded recovery).
  It covers `REFERENCE`/`POINTER`, `PROPERTY`, pragmas, `CONTINUE`,
  `VAR_IN_OUT`.
- Consequence: **license permits reuse** (MIT OR Apache-2.0, both texts
  present; vendoring would carry Apache-2.0 NOTICE obligations), but there
  is **no crates.io dependency to take** — adoption means a git dependency
  or vendoring a pre-1.0 crate from a fast-moving workspace, plus an adapter
  from truST kinds/diagnostics/options to IronPLC's `CompilerOptions`,
  `Diagnostic`, spans, and dsl lowering. The design already forbids
  attaching truST HIR/IDE/LSP, so only the syntax layer could be reused.

### 2.4 Criteria comparison

| Criterion | Extend PEG | RD/Pratt replacement (in-tree) | Evidence this experiment adds |
|---|---|---|---|
| Coverage | Best today: all currently accepted language | Must be rebuilt; target defined by 48-file corpus, dialect-flag tests, 17-item P0 backlog | Standard corpus parse acceptance with `parse_program`: ed2 36/48, ed3 40/48, rusty 44/48, codesys 47/48, twincat 46/48 — the 12 ed2 misses are dialect-flag fixtures (e.g. `adr.st`, `pointer_to.st`, `oop.st`, `if.st`) plus one intentionally malformed file, so coverage targets are pinned per dialect |
| Provenance | Requires moving destructive transforms behind lowering **and** threading trivia through 319 rule actions | Direct; demonstrated byte-exact over original text, error regions included | Spike section 1.2; audit F1–F7 |
| Recovery | None in peg; must be built outside the grammar | Error nodes + sync sets + depth bound are parser properties | Spike recovery + prefix-termination tests |
| Maintenance | 319 rules in a macro DSL; hand spans; peg stable/MIT | More code, explicit control; CODESYS/truST show the RD shape | Spike parser is 675 lines for the slice |
| Licensing | MIT (no change) | Own code (no change); truST reuse license-OK but no crates.io dependency exists and it is pre-1.0 | Section 2.3 |
| API cost | Grammar-wide rework; consumers keep token/AST APIs | New frontend seam; token consumers served from CST; dsl via lowering | Design 3.2/4; spike keeps token consumers untouched |

### 2.5 Recommendation

**Option B: replace the frontend parse seam with an in-tree, hand-written
recursive-descent/Pratt parser that produces the rowan CST from the original
text, delivered in S1 as the single production grammar.**

Reasons, in priority order:

1. **Recovery is not optional in this design** (S1 exit evidence: complete
   ranges, progress, termination; S2/S4 error nodes feed editor services),
   and `peg` provides no error productions or resynchronization. Option A
   would still require building recovery outside the grammar.
2. **Provenance is the core invariant** (lossless original-text CST before
   destructive preprocessing), and the PEG grammar consumes the
   already-transformed token stream. Restoring provenance under PEG means
   reworking the pipeline *and* all rule actions; under RD the lexer over
   original text plus one token→tree pass is the natural shape the spike
   already demonstrates.
3. **The CST is a rewrite either way.** Every one of the 319 rules must emit
   nodes and retain trivia; doing that in RD gives direct control of node
   boundaries and error ranges instead of working around a grammar DSL.
4. **Bounded coverage risk with a fixed target.** The failing-parse counts
   above are dialect gating in fixtures, not grammar defects; the corpus,
   dialect tests, and P0 backlog give S1 a measurable parity checklist, and
   S2's AST/diagnostic comparison is the safety net.

**Do not adopt `trust-syntax` as a dependency.** The license is compatible,
but the crate is not published under its name on crates.io (that name is an
unrelated Rust-dialect project), it is pre-1.0 with MSRV 1.95, and it would
still require a dialect/adapter layer. Reuse it as a behavioral reference
(its event/sink split and recovery helpers are the same shape the spike
uses); revisit only if vendoring is accepted and a differential harness
shows it covers IronPLC's dialect surface — the CODESYS syntax-gap backlog
remains applicable either way.

**No dual production parser** (design section 4): the spike is the only
comparison path, it is test-only, and it is removed at cutover. S1 replaces
the grammar; it does not run beside the PEG parser in production.

### 2.6 Counterarguments

- *Coverage risk of a hand-written parser.* Real, and the reason the
  recommendation is "scoped" and staged: S1 lands CST + recovery with the
  existing dialect tests preserved, S2 replaces the semantic parse entry,
  and each stage compares against the legacy path (test-only) before the
  legacy path is deleted.
- *Cost of owning a parser.* Already owned: the 319-rule PEG grammar plus
  the five transform modules are in-tree and must change for S1 regardless.
- *Pratt expressions vs ST operators.* ST's infix operator table is small
  (~30 operators per the CODESYS table); the spike's binding-power table is
  explicit and testable, and `Codesys/grammar/ST_GRAMMAR.ebnf` documents the
  operator levels for the port.

## 3. Benchmark baseline

### 3.1 Method and environment

- **Workload:** the full-file parse of the shared corpus
  `compiler/resources/test/*.st` (48 files) plus 5 CODESYS-dialect fixtures
  in `compiler/s0-spike/fixtures/codesys/` — 53 files, 23,241 bytes.
  Standard files use `CompilerOptions::from_dialect(Dialect::Iec61131_3Ed2)`
  (identical to `CompilerOptions::default()`, which is what the parser's own
  corpus tests use); fixtures use `Dialect::Codesys`.
- **Path:** `ironplc_parser::parse_program` — preprocessing, logos lexer,
  token transforms, token checks, PEG parse, file-id stamping. The
  tokenize-only columns measure `tokenize_program` (the same path minus the
  PEG parse) to show the split.
- **Cold:** the first call for that file in the process; the first row of
  the table is therefore also the process-cold call. **Warm:** median of 50
  calls after the cold call for the same file.
- **Allocations:** `stats_alloc` 0.1.10 as the harness's global allocator;
  counts are allocations performed inside the timed call (the returned AST
  is dropped after stats are captured).
- **Environment:** AMD64 desktop, Intel Core i5-9300H @ 2.40 GHz (4 cores /
  8 threads), 31.8 GiB RAM, Acer Nitro AN517-51, Windows 11 Pro
  10.0.26200 (build 26200). Toolchain: rustc 1.98.1 (48a229cea,
  2026-09-01), cargo 1.98.1, host `x86_64-pc-windows-msvc`, release profile
  (workspace `[profile.dev]`/`[profile.test]` overrides do not apply).
  Single-threaded, other processes at normal desktop load.

### 3.2 Numbers

Release run on 2026-10-01 (one of three; see 3.3 for variance):

```
files: 53, bytes: 23241, warm repeats per file: 50
totals (cold, one pass): 11.700 ms, 28625 allocations, 4645.9 KiB allocated
standard corpus parse acceptance (parse_program): iec61131-3-ed2 36/48, iec61131-3-ed3 40/48, rusty 44/48, codesys 47/48, twincat 46/48
```

| file | dialect | bytes | tokens | cold us | cold allocs | cold KiB | warm med us | warm allocs | tok cold us | tok cold allocs | tok warm med us | status |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| codesys/direct_variables.st | codesys | 167 | 56 | 1721.7 | 1180 | 560.3 | 48.9 | 190 | 79.4 | 62 | 13.0 | ok |
| codesys/pragmas.st | codesys | 108 | 31 | 84.5 | 139 | 33.5 | 35.9 | 139 | 11.4 | 43 | 8.9 | ok |
| codesys/references_and_pointers.st | codesys | 189 | 71 | 149.7 | 296 | 59.3 | 67.3 | 296 | 17.4 | 77 | 12.3 | ok |
| codesys/strings_and_comments.st | codesys | 183 | 50 | 83.3 | 144 | 37.0 | 33.1 | 144 | 11.6 | 58 | 10.6 | ok |
| codesys/time_literals.st | codesys | 158 | 73 | 166.3 | 323 | 67.0 | 76.2 | 323 | 23.7 | 93 | 17.5 | ok |
| std/adr.st | iec61131-3-ed2 | 157 | 71 | 197.7 | 603 | 49.4 | 118.7 | 603 | 13.9 | 75 | 10.4 | err[P0002] |
| std/array.st | iec61131-3-ed2 | 116 | 51 | 182.1 | 226 | 32.8 | 45.3 | 226 | 10.5 | 55 | 6.7 | ok |
| std/array_in_function_var.st | iec61131-3-ed2 | 163 | 67 | 123.8 | 241 | 46.4 | 62.3 | 241 | 11.3 | 71 | 9.4 | ok |
| std/array_of_string.st | iec61131-3-ed2 | 103 | 41 | 61.5 | 112 | 25.0 | 29.7 | 112 | 7.4 | 45 | 6.1 | ok |
| std/bit_access.st | iec61131-3-ed2 | 142 | 49 | 141.9 | 232 | 32.3 | 50.7 | 232 | 9.8 | 53 | 6.8 | ok |
| std/bit_string_arithmetic.st | iec61131-3-ed2 | 211 | 34 | 61.9 | 134 | 28.3 | 33.7 | 134 | 6.7 | 38 | 5.6 | ok |
| std/c_style_block_comment.st | iec61131-3-ed2 | 127 | 33 | 33.0 | 45 | 10.5 | 9.3 | 45 | 8.0 | 45 | 7.6 | err[P0004] |
| std/comment.st | iec61131-3-ed2 | 108 | 18 | 47.2 | 51 | 14.3 | 15.8 | 51 | 4.5 | 22 | 3.8 | ok |
| std/conditional.st | iec61131-3-ed2 | 154 | 53 | 91.4 | 153 | 39.8 | 47.5 | 153 | 10.8 | 57 | 7.8 | ok |
| std/configuration.st | iec61131-3-ed2 | 456 | 148 | 262.9 | 322 | 62.3 | 82.0 | 322 | 48.1 | 155 | 22.3 | ok |
| std/empty_var_block.st | iec61131-3-ed2 | 95 | 34 | 15.4 | 44 | 10.4 | 8.2 | 44 | 7.3 | 44 | 6.9 | err[P0011] |
| std/expressions.st | iec61131-3-ed2 | 289 | 157 | 201.8 | 441 | 90.0 | 140.4 | 441 | 42.8 | 165 | 24.1 | ok |
| std/first_steps.st | iec61131-3-ed2 | 2719 | 890 | 1132.5 | 3142 | 441.8 | 948.7 | 3142 | 251.8 | 897 | 222.5 | ok |
| std/first_steps_configuration.st | iec61131-3-ed2 | 259 | 75 | 103.6 | 126 | 26.1 | 31.1 | 126 | 29.0 | 82 | 12.6 | ok |
| std/first_steps_data_type_decl.st | iec61131-3-ed2 | 69 | 27 | 36.9 | 123 | 10.0 | 23.1 | 123 | 5.6 | 31 | 4.4 | ok |
| std/first_steps_func_avg_val.st | iec61131-3-ed2 | 267 | 95 | 152.5 | 542 | 67.5 | 108.8 | 542 | 19.0 | 99 | 14.0 | ok |
| std/first_steps_function_block_counter_fbd.st | iec61131-3-ed2 | 321 | 102 | 147.2 | 451 | 61.9 | 100.0 | 451 | 17.6 | 106 | 15.2 | ok |
| std/first_steps_function_block_counter_ld.st | iec61131-3-ed2 | 417 | 133 | 216.5 | 581 | 79.2 | 136.9 | 581 | 37.0 | 137 | 20.1 | ok |
| std/first_steps_function_block_counter_sfc.st | iec61131-3-ed2 | 942 | 286 | 213.9 | 703 | 133.7 | 160.9 | 703 | 58.0 | 290 | 42.0 | ok |
| std/first_steps_function_block_logger.st | iec61131-3-ed2 | 219 | 79 | 129.1 | 381 | 48.4 | 79.6 | 381 | 14.3 | 83 | 13.0 | ok |
| std/first_steps_program.st | iec61131-3-ed2 | 770 | 264 | 403.9 | 1568 | 180.0 | 382.3 | 1568 | 58.6 | 268 | 39.0 | ok |
| std/first_steps_semantic_error.st | iec61131-3-ed2 | 2719 | 890 | 913.7 | 3142 | 441.8 | 860.2 | 3142 | 187.6 | 897 | 138.0 | ok |
| std/first_steps_syntax_error.st | iec61131-3-ed2 | 2718 | 889 | 1454.3 | 4728 | 633.6 | 1365.8 | 4728 | 216.8 | 896 | 221.7 | err[P0002] |
| std/if.st | iec61131-3-ed2 | 142 | 42 | 67.1 | 106 | 33.0 | 37.6 | 106 | 7.7 | 46 | 6.8 | err[P0002] |
| std/inout_var_decl.st | iec61131-3-ed2 | 183 | 73 | 134.4 | 229 | 41.1 | 54.3 | 229 | 13.1 | 77 | 11.1 | ok |
| std/input_var_decl.st | iec61131-3-ed2 | 150 | 54 | 77.9 | 215 | 31.8 | 49.7 | 215 | 9.2 | 58 | 8.7 | ok |
| std/literal.st | iec61131-3-ed2 | 383 | 102 | 144.4 | 385 | 56.5 | 88.1 | 385 | 17.3 | 106 | 15.5 | ok |
| std/main.st | iec61131-3-ed2 | 354 | 105 | 109.2 | 207 | 40.2 | 52.2 | 207 | 21.2 | 112 | 16.4 | ok |
| std/nested.st | iec61131-3-ed2 | 108 | 39 | 83.0 | 184 | 29.7 | 44.9 | 184 | 7.7 | 43 | 6.6 | ok |
| std/oop.st | iec61131-3-ed2 | 985 | 297 | 138.7 | 311 | 74.1 | 62.1 | 311 | 45.4 | 301 | 46.6 | err[P0002] |
| std/oscat.st | iec61131-3-ed2 | 315 | 38 | 58.5 | 91 | 15.0 | 19.5 | 91 | 8.1 | 44 | 7.5 | ok |
| std/partial_access_bit.st | iec61131-3-ed2 | 146 | 49 | 33.4 | 62 | 11.8 | 12.7 | 62 | 11.6 | 62 | 10.1 | err[P4033] |
| std/partial_access_multi.st | iec61131-3-ed2 | 210 | 106 | 48.2 | 128 | 23.0 | 22.0 | 128 | 21.8 | 128 | 18.8 | err[P4033] |
| std/pointer_to.st | iec61131-3-ed2 | 254 | 116 | 75.9 | 326 | 27.8 | 56.7 | 326 | 17.4 | 120 | 17.1 | err[P0002] |
| std/program.st | iec61131-3-ed2 | 208 | 67 | 86.4 | 158 | 36.0 | 39.4 | 158 | 12.0 | 71 | 11.8 | ok |
| std/ref.st | iec61131-3-ed2 | 589 | 251 | 117.0 | 459 | 49.8 | 81.2 | 459 | 55.7 | 255 | 36.1 | err[P0002] |
| std/reference_to.st | iec61131-3-ed2 | 291 | 130 | 79.3 | 340 | 41.0 | 65.4 | 340 | 19.7 | 134 | 18.7 | err[P0002] |
| std/sfc.st | iec61131-3-ed2 | 349 | 121 | 124.9 | 350 | 53.0 | 78.8 | 350 | 19.6 | 125 | 17.0 | ok |
| std/sized_string_contexts.st | iec61131-3-ed2 | 602 | 189 | 187.3 | 496 | 96.8 | 112.8 | 496 | 31.6 | 195 | 27.3 | ok |
| std/sizeof.st | iec61131-3-ed2 | 158 | 84 | 149.4 | 399 | 51.3 | 87.6 | 399 | 15.8 | 88 | 12.7 | ok |
| std/steel_thread.st | iec61131-3-ed2 | 93 | 48 | 73.4 | 217 | 32.7 | 47.3 | 217 | 8.2 | 52 | 7.5 | ok |
| std/strings.st | iec61131-3-ed2 | 53 | 19 | 22.7 | 36 | 8.0 | 8.3 | 36 | 4.4 | 24 | 3.7 | ok |
| std/textual.st | iec61131-3-ed2 | 599 | 228 | 328.4 | 660 | 138.1 | 187.3 | 660 | 42.7 | 232 | 33.8 | ok |
| std/time_function_decl.st | iec61131-3-ed2 | 113 | 48 | 32.2 | 65 | 10.8 | 12.7 | 65 | 9.9 | 55 | 8.1 | err[P0002] |
| std/type_decl.st | iec61131-3-ed2 | 784 | 292 | 199.4 | 800 | 96.3 | 157.0 | 800 | 45.7 | 298 | 44.8 | ok |
| std/var_decl.st | iec61131-3-ed2 | 1293 | 476 | 452.0 | 1376 | 193.3 | 301.0 | 1367 | 67.9 | 486 | 68.5 | ok |
| std/var_temp.st | iec61131-3-ed2 | 233 | 87 | 124.2 | 323 | 55.8 | 73.5 | 323 | 14.6 | 91 | 13.3 | ok |
| std/wstring_ops.st | iec61131-3-ed2 | 300 | 141 | 222.5 | 529 | 76.3 | 117.0 | 529 | 26.5 | 149 | 20.4 | ok |

Notes: `err[...]` files are dialect-flag fixtures parsed under strict ed2
options on purpose (e.g. `adr.st`, `pointer_to.st`, `ref.st`,
`reference_to.st`, `oop.st`, `if.st` need `--allow-*` flags;
`c_style_block_comment.st` P0004; `empty_var_block.st` P0011;
`partial_access_*.st` P4033 from the token-check rules;
`first_steps_syntax_error.st` is intentionally malformed). Their timings
measure the fail-fast path. All 5 CODESYS fixtures parse under the Codesys
preset.

### 3.3 Observations

- **Allocation counts are deterministic; wall time is not.** Two
  consecutive release runs produced identical totals (28,625 allocations,
  4,645.9 KiB per corpus pass) and identical per-file allocation columns,
  while cold-pass wall time varied 9.7 / 11.7 / 17.8 ms across three runs
  and warm medians by a few percent (desktop machine, no pinning). Treat
  the allocation numbers as exact and the microsecond medians as ±10–20 %.
- **Process-cold vs per-file cold.** The first parse in a process costs an
  extra ~990 allocations of one-time initialization: the process-first file
  (`codesys/direct_variables.st`, 167 bytes) reports 1,721.7 µs / 1,180
  allocations / 560.3 KiB allocated cold vs 48.9 µs / 190 allocations warm.
  Every later file's cold allocations equal its warm allocations.
- **Where the time goes.** For accepted files the tokenize+transform path is
  roughly a fifth to a third of the full parse (`first_steps.st`: 222.5 of
  948.7 µs warm; `var_decl.st`: 68.5 of 301.0 µs; `configuration.st`: 22.3
  of 82.0 µs). The PEG parse dominates; error-path files
  (`first_steps_syntax_error.st`: 221.7 of 1,365.8 µs) fail after
  tokenization and spend most of their time in the transforms/checks.
- **Scale.** The largest corpus file is 2.7 KB; full parse of the whole
  23 KB corpus is ~12 ms cold, ~9 ms warm. These are small-input numbers;
  S5's local-reparse budgets must be set from larger projects, and this
  corpus cannot justify incremental syntax work by itself.

### 3.4 Reproduce

```
cd compiler
cargo run --release -p ironplc-s0-spike --bin parse_baseline -- 50 > baseline.txt
```

`dialect_acceptance` in the same binary prints the per-dialect counts shown
in 3.2 (also asserted by a unit test).

## 4. Prefactoring candidates (observed while spiking)

1. **Lossless token source (new, blocking for S1).** `ironplc-parser`
   exposes only `tokenize_program`, which applies the destructive transforms,
   and `lexer` is private. The spike had to write its own tokenizer to get
   original-text tokens; S1 needs a sanctioned lossless entry (either lex
   original text in the new frontend, or split raw tokenization from
   transforms). Decide before S1 code lands.
2. **One-time init cost is attributed to the first parse (new).** ~990
   allocations attach to whichever input parses first; a long-lived process
   pays it once, a CLI invocation pays it per run. Worth removing or making
   explicit (lazy statics in the parse path) when S1 touches the pipeline.
3. **Port the tiling/coverage invariant into parser tests (new).** The
   spike's `check_coverage` (tokens tile the source; token text equals its
   slice) is the guard the audit's F1–F4 need while the legacy pipeline
   still runs. Small, test-only, and it makes transform regressions fail
   loudly.
4. **Zero-width synthetic terminators (audit F3, confirmed).** The spike's
   zero-width `ErrorNode` for a missing `;` is the shape the legacy
   `xform_tokens` fix should take (the audit's TODO "remove the span and
   line/col").
5. **Pipeline order and dialect gating as documented semantics (audit
   F6/F7, confirmed).** The spike's dialect-agnostic lexer plus a summary
   that recomputes from text show the shape when transforms move behind
   lowering; the legacy order (terminators before demotion) remains
   observable and should be pinned by tests before it moves.
6. **Coordinated diagnostic type (new).** The spike carries
   `ParseError { message, range }`; production diagnostics are
   `dsl::Diagnostic` with codes, labels, and notes. S1 needs the syntax
   layer's error type and its conversion into problem codes (`P0002`,
   `P4033`, …) decided together with the P-code catalog.
7. **A permanent parse-only benchmark (new).** `compiler/benchmarks` covers
   compile→VM only; this spike's harness is the shape of the missing
   parse-only bench (per-file cold/warm, allocation counts). Fold a
   production version into `compiler/benchmarks` at S1 instead of keeping
   this bin.
8. **Audit section 7 items re-verified.** Coordinate semantics wording
   (byte offsets vs character/UTF-16), OSCAT non-destructive
   representation, and byte-invariant tests were all confirmed as relevant
   while building the spike's coverage checks.

## 5. Test and run record

Run on 2026-10-01 on the environment in 3.1, from `compiler/`:

| Command | Result |
|---|---|
| `cargo test -p ironplc-s0-spike` | 33 passed, 0 failed (13 unit + 15 `cst_lossless` + 4 `salsa_recompute` + 1 `baseline_smoke`); ~0.3 s |
| `cargo llvm-cov -p ironplc-s0-spike --summary-only` | 92.46 % lines (1,352 measured; the only fully uncovered target is the bin's 10-line `main`) |
| `cargo run --release -p ironplc-s0-spike --bin parse_baseline -- 50` | 53 files measured; output recorded in 3.2; exit 0 |
| `cargo clippy -p ironplc-s0-spike --all-targets` / `cargo fmt -p ironplc-s0-spike -- --check` | clean under workspace lints (`unsafe_code`, clippy panic family, `warnings = deny`) |
| `cargo tree --workspace -i ironplc-s0-spike` | resolves only itself (no production reverse dependency) |

## 6. Dependency versions selected by this experiment

| Crate | Version | License | MSRV | Used for |
|---|---|---|---|---|
| rowan | 0.16.1 | MIT OR Apache-2.0 | 1.77 | Green storage + red views (CST adapter) |
| salsa | 0.28.5 | Apache-2.0 OR MIT | 1.88 | Tracked queries (declaration summary adapter) |
| stats_alloc | 0.1.10 | MIT | unknown | Allocation counting in the baseline harness |
| peg (existing) | 0.8.6 resolved | MIT | — | Current production grammar (assessment input) |
| logos (existing) | 0.16.1 | MIT OR Apache-2.0 | — | Current production lexer (assessment input) |

These versions are locked in `compiler/Cargo.lock` by the spike being a
workspace member; production selection is S1 work, and the spike crate is
deleted at cutover.

## 7. References

- [Parse-Tree Architecture](parse-tree-architecture.md) (sections 1, 3, 4, 5)
- [Parse-Tree S0 Audit](parse-tree-s0-audit.md)
- [CODESYS lexer/parser map](../../Codesys/docs/01_LEXER_PARSER.md) and
  [recovered ST grammar](../../Codesys/grammar/ST_GRAMMAR.ebnf)
- [Syntax gap analysis](../../Codesys/LEXER-GAP-ANALYSIS.md) (P0 backlog)
- truST: `johannesPettersson80/trust-platform` (shallow clone,
  `be8d81a4a7ab16ca7554b8be0f4723161ec1a47b`); crates.io `trust-syntax`
  0.2.2 is an unrelated Rust-dialect project
- Spike crate: `compiler/s0-spike/`
