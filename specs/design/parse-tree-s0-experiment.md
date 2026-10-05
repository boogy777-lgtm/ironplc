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
| Spike crate (rowan CST + Salsa adapter + baseline harness) | `compiler/s0-spike/` (removed at the S1 cutover; see below) |
| Performance baseline numbers | Section 3 of this document |
| Parser recommendation | Section 2 of this document |

The spike crate was removed at the S1 cutover. The results recorded below
stay as measured. The CST it validated lives on in `compiler/syntax`, the
parse baseline is reproduced by `cargo bench -p ironplc-benchmarks --bench
parse_baseline` (section 3.4), and the five CODESYS fixtures moved to
`compiler/syntax/tests/fixtures/codesys/`.

Commands as they ran at S0 (from `compiler/`):

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

**Withdrawn by the owner on 2026-10-01.** Salsa and query-based tracking
are no longer part of the plan (design §3.3, §5 S3 — recompute per
snapshot; reuse follows the CODESYS style). This section is retained as
the experiment record only.

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

The spike does not exercise cancellation or cycle recovery; those
Salsa-specific behaviors are no longer plan criteria (S3 withdrawn, design
§5). The CODESYS-style reuse path has its own acceptance criteria when a
measured stage lands.

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
cargo bench -p ironplc-benchmarks --bench parse_baseline > baseline.txt
```

At S0 this ran as `cargo run --release -p ironplc-s0-spike --bin
parse_baseline -- 50`; the benchmark above is its successor and measures the
48-file shared corpus.

`dialect_acceptance` in the same binary prints the per-dialect counts shown
in 3.2 (also asserted by a unit test).

### 3.5 S1 baseline (2026-10-01)

The permanent parse-only benchmark now lives in `compiler/benchmarks` and no
longer depends on the spike. It measures the production entry points
`ironplc_parser::tokenize_program` and `parse_program` over the 60 files of
`compiler/resources/test/` (26,203 bytes) under `CompilerOptions::default()`.
The five CODESYS fixtures stay spike-only, so the corpus differs from 3.1 (60
files instead of 53) and the figures below are not comparable file-for-file
with 3.2.

- **Environment:** same machine as 3.1 (Intel Core i5-9300H, Windows 11 Pro
  10.0.26200), rustc 1.98.1 (48a229cea, 2026-09-01), release (`bench`)
  profile, base commit `c1fe7f981` plus the benchmark change.
- **Warm timings (Criterion):** `parse_benchmark` registers
  `parse_tokenize/<file>` and `parse_full/<file>` per file.
- **Cold/warm timings and allocations:** `parse_baseline` prints cold (first
  call for the file) and warm-median (50 repeats) microseconds plus
  `stats_alloc` allocation counts for both entry points.

Three consecutive `parse_baseline` runs:

| quantity | run 1 | run 2 | run 3 |
|---|---|---|---|
| parse, cold, one corpus pass (ms) | 4.822 | 4.776 | 4.962 |
| parse, warm-median sum (ms) | 4.649 | 4.696 | 4.678 |
| parse, cold allocations (one pass) | 31,213 | 31,213 | 31,213 |
| parse, cold KiB allocated (one pass) | 4,419.3 | 4,419.3 | 4,419.3 |

Allocation counts are identical across runs and per file; microseconds vary a
few percent in the warm sums and roughly 5-10 % for cold passes, and
individual cold calls are noisier (desktop, no pinning). Tokenize alone
accounts for 9,289 allocations and about 1.0 ms warm-median over the corpus,
so the PEG parse is about four fifths of the parse time. Representative rows
(run 3): `first_steps.st` (2,719 bytes) 557.0 us cold / 588.9 us warm / 3,255
allocations, tokenize 153.9 / 140.7 us; `var_decl.st` 228.4 / 182.7 us / 1,389
allocations; `strings.st` (53 bytes) 6.1 / 5.6 us / 36 allocations.

**One-time init cost (section 4.2).** It does land inside a measured call, and
it is input-dependent rather than "the first parse": the first
`parse_program` of a program with located variables (`AT %IX0.0`) allocates
989 more than any later call (1,094 vs 105 allocations on the probe input, and
roughly 570-780 us vs 32 us), because the direct-variable address regexes in
`ironplc-dsl` are compiled lazily on first use. A plain program and
`tokenize_program` carry no init (30 vs 30 and 119 vs 119 allocations). The
benchmark therefore measures the init explicitly, on tiny inputs before the
corpus loop, and prints it as its own table (`one-time init`); the per-file
cold figures exclude it. Production parser code is unchanged.

Reproduce:

```
cd compiler
cargo bench -p ironplc-benchmarks --bench parse_baseline          # tables above
cargo bench -p ironplc-benchmarks --bench parse_baseline -- 100   # 100 warm repeats
cargo bench -p ironplc-benchmarks --bench parse_benchmark         # Criterion warm timings
```

### 3.6 CST parse rows (2026-10-02)

`parse_baseline` and `parse_benchmark` now also measure the lossless CST path,
`ironplc_syntax::lex` and `parse_source_file` under `ParseOptions::default()`,
with the same corpus, helpers (`measure`, `measure_row`, 50 warm repeats) and
output format as the legacy rows. `parse_baseline` prints the CST columns,
per-file ratios and a `cst parse` totals line; `parse_benchmark` registers
`parse_cst/<file>`. The legacy rows are unchanged. The corpus is the 60 files
of `compiler/resources/test/`, now 26,229 bytes (the corpus grew by one small
file since 3.5, so the legacy totals moved slightly: 31,253 allocations,
4,422.8 KiB).

- **Environment:** as 3.5 (Intel Core i5-9300H, Windows 11 Pro 10.0.26200,
  rustc 1.98.1 (48a229cea, 2026-09-01), release `bench` profile, desktop, no
  pinning), base commit `5e31a45da` plus the benchmark change.
- **Scope of the comparison:** the CST rows are parse-only: lex, CST parse,
  tree build. They exclude the lowering to the DSL that production needs, so
  they bound the CST cost from below and are not a like-for-like replacement
  for `parse_program`, which returns the DSL. 21 corpus files report syntax
  errors under default options (the legacy path rejects 20); both paths are
  timed on every file, including the dialect fixtures that fail.

Three consecutive `parse_baseline` runs (one corpus pass):

| quantity | legacy run 1 / 2 / 3 | CST run 1 / 2 / 3 | CST / legacy |
|---|---|---|---|
| parse, cold (ms) | 5.839 / 6.638 / 6.657 | 3.258 / 3.385 / 3.223 | 0.50-0.56 |
| parse, warm-median sum (ms) | 4.755 / 4.810 / 4.708 | 2.260 / 2.304 / 2.279 | 0.47-0.49 |
| parse, cold allocations | 31,253 (all runs) | 9,066 (all runs) | 0.29 |
| parse, cold KiB allocated | 4,422.8 | 1,165.6 | 0.26 |
| tokenize / lex, warm-median sum (ms) | 0.911 / 0.944 / 0.946 | 0.238 / 0.253 / 0.238 | 0.25-0.27 |
| tokenize / lex, cold allocations | 9,303 | 60 | 0.006 |

Allocation counts are exact and identical across runs; times are
roughly +-10 % (the cold figures vary more, 5.8-6.7 ms legacy). The CST
`lex` allocates once per file (the token vector); the CST parse allocates
about 9,000 times, so the tree build, not lexing, is where its allocations
go. The one-time init probe shows no lazy initialization in the CST path
(65 vs 65 allocations on the located-variable input; legacy 989 extra).

Reproduce: the commands in 3.5; the CST columns are in the same output.

### 3.7 Both front ends side by side (2026-10-05)

`parse_baseline` and `parse_benchmark` now measure every path from one table
(`compiler/benchmarks/src/paths.rs`). A path is one row: its name, the inputs it
runs over, the call, and optionally the path it is compared with. The totals,
the ratios, the per-input table, the one-time init probe and the Criterion
groups iterate that table, so one more path is one more row. The rows of 3.5
and 3.6 keep their names and meaning; the allocation totals of `tokenize`,
`parse`, `cst lex`, `cst parse` and `cst parse + lower` are identical before and
after the change (9,448 / 32,121 / 61 / 9,590 / 23,865 on the 61-file corpus).

At the time of these measurements the public functions of `ironplc-parser` were
rows like the others, and they ran the front end the crate was built with. The
two front ends were therefore two runs of the same table: default features (the
legacy pipeline), then the `cst-frontend` feature of `ironplc-parser` (the
pipeline on the lossless tree: tokens, regions, gates, grammar, lowering, and
the token view). The output header said which one the public functions ran,
read from what they do (the legacy transforms split the duration literal
`T#1m30s` into more tokens than the lexer of the tree gives it). The rows that
call `ironplc-syntax` directly were in both runs.

**After the switch.** The compiler has one front end, the tree, and the legacy
pipeline is compiled for tests only, so it is not reachable from the benchmark
crate. The facade rows (`tokenize`, `parse`, `statements`) now measure the tree;
the feature, the header and the second run are gone, and no row has the legacy
pipeline as its baseline. The figures in this section that compare the two front
ends, and the ratios they give, are the recorded history of the comparison that
the budget of the architecture document (section 5.2) was checked against; they
were measured before the switch and are not re-measured, and none of them
becomes false by it.

- **Environment:** Intel Core i5-9300H @ 2.40 GHz (4 cores / 8 threads),
  Windows 11 Pro 10.0.26200, rustc 1.98.1 (48a229cea, 2026-09-01), cargo 1.98.1,
  release `bench` profile, desktop at normal load, no pinning, no other build
  running during a run. Base commit `66b7c230e` plus the benchmark change.
- **Corpus:** 61 files of `compiler/resources/test/`, 26,638 bytes, under
  `CompilerOptions::default()` (the corpus grew by one file since 3.6, so the
  figures of 3.6 are not comparable file for file). 20 files are rejected by
  both front ends under default options (`ok` for 41 of 61 in both): they
  measure the path that rejects.
- **Method:** as 3.5: cold is the first call for the input after one unmeasured
  call, warm is the median of 50 calls, allocations are `stats_alloc` counts
  inside the call, the result is dropped after the counters are read.
- **Three consecutive runs** per feature set, run 1 / 2 / 3 below. No warm sum
  differed by more than 15 % between runs of one set (the widest spread is the
  legacy `parse` warm sum, 10.818-11.517 ms, 6.5 %), so no run was repeated.
  Allocation counts are exact and equal in every run.

**What changed against 3.6.** Every `parse_program` and `parse_st_statements`
call now runs on one thread of the stack budget (`ironplc_dsl::stack`), spawned
and joined per call. 3.5 and 3.6 measured before that existed. It is 43-45 % of
the `parse` sum on this corpus (Q4), and it is why the legacy `parse` sum is
about 10.8 ms here and was 4.7 ms in 3.5.

#### Q1: whole file, `parse_program`

The legacy column is the default run; the cst column is the run with the feature
on, where `parse_program` is parse plus lowering on one budget thread (the
public function wraps it once and the front end's own wrap, and the syntax
crate's, reuse that thread). 61 files.

| quantity | legacy run 1 / 2 / 3 | cst run 1 / 2 / 3 | cst / legacy run 1 / 2 / 3 |
|---|---|---|---|
| cold, one pass (ms) | 12.974 / 12.313 / 12.075 | 12.358 / 12.214 / 12.338 | 0.95 / 0.99 / 1.02 |
| warm-median sum (ms) | 11.517 / 10.818 / 10.907 | 10.373 / 10.887 / 10.420 | 0.90 / 1.01 / 0.96 |
| cold allocations | 32,121 | 23,949 | 0.75 |
| cold KiB allocated | 4,544.6 | 2,800.8 | 0.62 |

The warm allocation sums are 32,037 (legacy) and 23,865 (cst): each front end's
cold sum is 84 higher, in `configuration.st` and `var_decl.st` (first use of a
construct in the process); the ratio is 0.75 either way.

Per file, the warm allocation ratio is exact: worst is
`first_steps_configuration.st` 1.91 (248 against 130 allocations), then
`strings.st` 1.82 (73 / 40), `main.st` 1.64, `configuration.st` 1.62. The
worst warm-time ratio moves between runs, as 100 us medians do: `oop.st` 1.37 /
1.80 / 1.32 (run 3: `var_decl.st` 1.33 is the worst), and 3 / 7 / 5 of 61 files
are above 1.25, 0 / 1 / 0 above 1.5. Why, from the same rows:

- `oop.st`, `c_style_block_comment.st` and the other rejected files: the legacy
  grammar stops at its first error (`oop.st` 137 us); the tree parses the whole
  file with recovery (19 errors) and ranks them (247 us).
- `var_decl.st`, `type_decl.st`, `configuration.st`, `main.st`,
  `first_steps_configuration.st`: lowering-bound. For `var_decl.st` the parse
  alone is 447 allocations and parse plus lowering 1,563; the lowering allocates
  per name and per node and the tree parse does not.
- `strings.st` (53 bytes): 40 against 73 allocations is the fixed part of the
  tree builder and the lowering on a file with six tokens; in time both sides
  are the budget thread (Q4).
- The largest bodies are at parity or better on the tree: `first_steps.st`
  631.8-707.5 us against 641.8-720.5 us warm, 2,685 against 3,259 allocations;
  `first_steps_program.st` 267.8 against 326.8 us (run 2).

#### Q2: token view, `tokenize_program`

Legacy: the default run. cst: the run with the feature on, which is
`ironplc_syntax::tokenize` followed by the projection to `Token` (what the
language server's semantic tokens and `ironplcc tokenize` call).

| quantity | legacy run 1 / 2 / 3 | cst run 1 / 2 / 3 | cst / legacy run 1 / 2 / 3 |
|---|---|---|---|
| cold, one pass (ms) | 1.717 / 1.683 / 1.690 | 3.230 / 3.169 / 3.129 | 1.88 / 1.88 / 1.85 |
| warm-median sum (ms) | 0.925 / 0.917 / 0.925 | 2.212 / 2.212 / 2.254 | 2.39 / 2.41 / 2.44 |
| cold allocations | 9,448 | 9,430 | 1.00 |
| cold KiB allocated | 1,753.3 | 1,407.8 | 0.80 |

Every one of the 61 files is above 1.5 times the legacy warm time (smallest
1.55 / 1.73 / 1.57 in run 1 / 2 / 3; largest `incomplete_array.st` 3.58 in run 1
on 5 us medians, else `var_decl.st` 2.64, 112.6 against 42.6 us), so the cost is
per token and not one file.

Where it goes, from the direct rows (default run, 3 below) and from two ad-hoc
timings that are not in the tree: `lex` alone is 0.258-0.261 ms; `tokenize`
(lexer, regions, gates) is 1.240-1.252 ms; the projection and the options
conversion are the 0.96 ms that remain of 2.21. Splitting the facade with
temporary timers gave 1.43 ms for `tokenize` and 1.03 ms for the projection per
pass; and with the gates skipped (temporary switch) `tokenize` went from 1.27 to
0.37 ms, so the gates are about 0.9 ms. The gate pass calls every disabled gate
on every token, 11 function-pointer calls that each build a `Vec`, for about
9,400 tokens (`compiler/syntax/src/parser/gates.rs`, `gate_errors`); hoisting
the `enabled` test out of the token loop did not change the time. The
projection is a string per token (as many allocations as the legacy path: 9,430
against 9,448), a position per token and the option test per token.

#### Q3: statement fragments, `parse_st_statements`

Input: the statement body of every unit of the corpus that has a variable
block, the text between its last `END_VAR` and its `END_...` word, trimmed (75
bodies, 5,551 bytes; `corpus::statement_bodies`). These are the text a PLCopen
XML document hands to `parse_st_statements`, but they are cut from the ST corpus
and not from XML documents (the repository has no XML corpus with ST bodies of
this kind), and a body is 74 bytes on average: **indicative**. Both front ends
accept 59 of the 75.

| quantity | legacy run 1 / 2 / 3 | cst run 1 / 2 / 3 | cst / legacy run 1 / 2 / 3 |
|---|---|---|---|
| cold, one pass (ms) | 8.950 / 8.768 / 8.741 | 8.256 / 8.453 / 8.275 | 0.92 / 0.96 / 0.95 |
| warm-median sum (ms) | 8.367 / 8.179 / 8.215 | 7.789 / 8.338 / 8.014 | 0.93 / 1.02 / 0.98 |
| cold allocations | 7,744 | 8,306 | 1.07 |
| cold KiB allocated | 1,247.8 | 962.3 | 0.77 |

No body is above 1.25 in warm time in the runs (worst 1.20); the worst
allocation ratio is `c_style_block_comment.st#1` 1.76 (44 against 25), a tiny
rejected body. The bodies are dominated by the budget thread (Q4): the median
call is 107 us, of which the thread is about 75.

#### Q4: the stack budget thread

`within_stack_budget` (`compiler/dsl/src/stack.rs:106`) spawns a thread with a
64 MiB stack and joins it, except where the caller is already on one
(`ON_BUDGET`, `stack.rs:107-109`). Measured as the path `stack budget, empty
work` (one spawn and join for work that returns the length of the input), over
the 61 corpus files:

| quantity | default run 1 / 2 / 3 | cst run 1 / 2 / 3 |
|---|---|---|
| one call, warm median over the files (us) | 82.3 / 74.0 / 78.2 | 73.5 / 79.5 / 73.6 |
| one call, min-max over the files, run 1 (us) | 77.1-106.8 | 72.2-93.9 |
| warm-median sum over 61 calls (ms) | 5.095 / 4.627 / 4.719 | 4.608 / 4.928 / 4.540 |
| allocations per call | 4 | 4 |
| share of the `parse` warm sum | 0.44 / 0.43 / 0.43 | 0.44 / 0.45 / 0.44 |
| share of one `parse` of `strings.st` (53 bytes) | 0.82 / 0.92 / 0.88 | 0.76 / 0.77 / 0.70 |
| share of one `parse` of `first_steps.st` (2,719 bytes) | 0.12 / 0.12 / 0.12 | 0.12 / 0.12 / 0.12 |

A tiny parse is the thread: `strings.st` takes 94.0-103.3 us (legacy) and
95.5-105.0 us (cst) against 72.9-86.7 us for the thread alone, which is one
spawn in each, not two. Threads per public call, from the code:

| public call | legacy | cst | evidence |
|---|---|---|---|
| `tokenize_program` | 0 | 0 | `parser/src/lib.rs:118` has no budget; `frontend.rs` `cst_tokenize_program` has none; `ironplc_syntax::tokenize` runs on the caller's stack (`syntax/src/parser/mod.rs:93`) |
| `parse_program` | 1 | 1 | `parser/src/lib.rs:137` spawns; the front end's own `within_stack_budget` (`frontend.rs`, `cst_parse_program`) and `parse_with` (`syntax/src/parser/mod.rs:111`) run on that thread (`stack.rs:107-109`) |
| `parse_st_statements` | 1 | 1 | `parser/src/lib.rs:159`, nested the same way |

The tree adds no thread to the facade. Called directly, `parse_source_file`
spawns its own (`mod.rs:111`) and `lower_library` then runs on the caller's
stack; the row `cst parse + lower (budget)` is the facade's shape, on one thread.
A `parse_st_statements` call per body (`sources/src/xml/transform.rs:1110`) is
one thread per body: for a PLCopen XML file with N bodies, N threads.

#### Q5: the language server, per edit

Calls per text change (`DidOpenTextDocument` / `DidChangeTextDocument`,
`ironplc-cli/src/lsp.rs:451-483`): `change_text_document` stores the text and
parses nothing (`lsp_project.rs:147`, `sources/src/project.rs:99`);
`publish_workspace_diagnostics` (`lsp.rs:229-233`) calls `semantic_all`
(`lsp_project.rs:195`), which runs `run_semantic_analysis`
(`project/src/project.rs:33`): each activated compatibility library is parsed
again with `parse_program` (`sources/src/libraries/mod.rs:338`, not cached; none
unless discovery activated one), `Source::library()` parses the changed source
only (`project.rs:63`, `sources/src/source.rs:91`; the other sources keep their
cached library) and `analyze` runs (`project.rs:93`, `analyzer/src/stages.rs:64`,
on the budget). Per semantic-token request (`lsp.rs:324-346`): one
`tokenize_program` (`lsp_project.rs:158-173`, `project.rs:333`).

Threads per edit: 1 (`parse_program` of the edited file) + 1 (`analyze`) + 1 per
bundled library file of each activated library, the same for both front ends.
Threads per semantic-token request: 0 for both. Measured, the parse side for
`first_steps.st` (2,719 bytes, run 1 / 2 / 3, warm median):

| call | legacy (us) | cst (us) | allocations legacy / cst |
|---|---|---|---|
| `tokenize_program`, per semantic-token request | 130.0 / 125.7 / 128.0 | 214.6 / 217.5 / 256.1 | 897 / 894 |
| `tokenize_program`, cold | 144.5 / 224.3 / 173.8 | 277.9 / 325.0 / 285.1 | |
| `parse_program`, per edit | 720.5 / 696.1 / 641.8 | 631.8 / 707.5 / 655.3 | 3,259 / 2,685 |
| `parse_program`, cold | 751.1 / 833.1 / 700.7 | 764.6 / 748.5 / 890.9 | |
| thread of one budget call (Q4), per edit twice | 89.7 / 81.0 / 73.9 | 72.9 / 82.3 / 79.1 | 4 |

So one edit pays about two budget threads (150-165 us) on top of the parse and
the analysis; the thread is 12 % of the parse of this file and the parse is the
same cost in both front ends within noise. A semantic-token request is 126-130
us on the legacy path and 215-256 us on the tree, the Q2 overshoot at the scale
of one typical file. The analysis itself and the end-to-end edit were not
measured.

#### Q6: one-time initialization

The init probe of 3.5, run for the facade under each front end (tiny inputs,
before the corpus loop; allocations are exact):

| call | legacy first / second / init | cst first / second / init |
|---|---|---|
| `tokenize`, plain | 30 / 30 / 0 | 30 / 30 / 0 |
| `tokenize`, located variables | 46 / 46 / 0 | 46 / 46 / 0 |
| `parse`, plain | 123 / 123 / 0 | 124 / 124 / 0 |
| `parse`, located variables | 1,115 / 184 / 931 | 1,194 / 263 / 931 |

Time of the first and second `parse` call on the located input (run 1 / 2 / 3):
legacy 746.1 / 782.1 / 753.4 us then 200.9 / 245.2 / 250.6 us; cst 885.8 / 774.7 /
710.4 us then 314.6 / 319.6 / 206.6 us. Both front ends pay the same 931
allocations once, on the first program with a located variable, for the
direct-variable address regexes of `ironplc-dsl`; neither pays anything else on
a plain program or on the token view. The direct rows come after the facade in
the order of the table, so they inherit what it initialized. One difference is
visible there and its cause was not investigated: the located input lowered on
the caller's thread is 263 allocations the first time and 188 after
(`cst parse + lower, located`), and 263 every time on a fresh budget thread (the
`(budget)` row, and the second call of the cst facade above), where the legacy
facade's second call is 184. It reads as per-thread state, 75 allocations, that a
thread made for the call has to build again.

#### Direct rows of the syntax crate (default run, run 1 / 2 / 3)

| path | warm-median sum (ms) | cold one pass (ms) | cold allocations | cold KiB |
|---|---|---|---|---|
| `cst lex` | 0.258 / 0.258 / 0.261 | 0.735 / 0.708 / 0.698 | 61 | 387.0 |
| `cst tokenize` | 1.252 / 1.240 / 1.240 | 1.490 / 1.499 / 1.478 | 186 | 393.9 |
| `cst parse` | 8.512 / 7.913 / 7.968 | 10.100 / 9.342 / 9.554 | 9,590 | 1,199.6 |
| `cst parse + lower` | 10.627 / 10.032 / 10.165 | 11.119 / 10.400 / 10.484 | 23,865 | 2,759.6 |
| `cst parse + lower (budget)` | 10.985 / 10.403 / 10.489 | 11.376 / 10.984 / 10.954 | 23,865 | 2,770.0 |
| `stack budget, empty work` | 5.095 / 4.627 / 4.719 | 5.541 / 5.166 / 5.158 | 244 | 9.5 |
| `statements` (legacy) | 8.367 / 8.179 / 8.215 | 8.950 / 8.768 / 8.741 | 7,744 | 1,247.8 |
| `cst statements (budget)` | 8.211 / 7.920 / 7.988 | 8.921 / 8.803 / 8.801 | 8,306 | 962.3 |

#### Budget and verdict

The owner-approved budget: the warm-median corpus sum and the allocation count
of the tree-based front end are each at most 1.5 times the legacy front end's;
anything above 1.25 is explained here.

| quantity | warm-median sum | allocations | verdict |
|---|---|---|---|
| Q1 whole file | 0.90 / 1.01 / 0.96 | 0.75 | met; nothing above 1.25 |
| Q2 token view | 2.39 / 2.41 / 2.44 | 1.00 | **warm time over the budget** |
| Q3 fragments (indicative) | 0.93 / 1.02 / 0.98 | 1.07 | met |

Q2 overshoots in time and not in allocations (the follow-on at the end of this
section records the change made for it and the figures after). The cause is the two stages in
front of the view, both per token and both measured above: the gate pass of the
syntax tokenizer (about 0.9 ms of 2.2) and the projection (about 1.0 ms). It is
not a defect of one place, so it is not fixed in this change. The fix the
numbers point at is in the gates: each gate declares the token kinds it looks
at and one pass hands a token only to the gates of its kind (and a test that a
gate reports nothing at any other kind), which removes the 11 calls per token;
whether that alone brings 2.2 ms under 1.39 ms (1.5 times 0.925) is not
established, the projection would need to give back about 0.2 ms with it. The
absolute cost is 126-130 us against 215-256 us for the tokens of `first_steps.st`
per semantic-token request.

Also visible in these numbers: on this corpus the budget thread is 43-45 % of
`parse` and about 70 % of a statement body. It is the same in both front ends, so
it does not move the ratios, but a language server that parses per edit and an
XML file with many bodies pay it (Q4, Q5).

#### Not measured

The analysis, code generation and the end-to-end edit in the language server;
files larger than 2.7 KB (the largest input of the corpus; the thread is a fixed
cost, so larger inputs shift the share to the parse and the Q2 per-token cost to
the whole); PLCopen XML documents (Q3 is cut from ST); Linux and `wasm32` (which
has no thread to spawn); memory retained after a call (only allocations during
it); Criterion warm timings (`parse_benchmark` registers the same paths and was
run in test mode only); the activated-library parse per edit.

Reproduce (the tree figures; the legacy figures were measured on the commit
before the switch, where the same command ran the legacy pipeline):

```
cd compiler
cargo bench -p ironplc-benchmarks --bench parse_baseline
cargo bench -p ironplc-benchmarks --bench parse_benchmark      # Criterion
```

A new path is one row of `PATHS` in `compiler/benchmarks/src/paths.rs`.

#### Follow-on: the token view after the gate index (2026-10-05)

The tables above stay as recorded. This follow-on repeats the measurement after
one change at the cause found in Q2, with the same corpus, machine, profile and
method (three consecutive runs per front end, base commit `e50261054` plus the
change).

**What changed.** Each gate of the syntax tokenizer now names the token kinds at
which it can start (its `triggers`, data in its row of `GATES`). The pass over
the tokens derives once an index from a token kind to the gates that name it,
and per call the set of gates the dialect leaves inactive, so a token is handed
to the gates of its kind that are inactive and to no other: the 11 calls per
token are none for most tokens. A test holds every gate to its triggers (a gate
reports nothing at a token whose kind it does not name) over the corpus in LF,
CRLF and tab spelling under the default options, every flag on, and each flag on
alone. A parse starts with the same pass (`tokenize`), so `parse_program` has
the gain too. In the token view, the result is reserved at its final capacity,
the line walker moves by one comparison where positions go forward and counts
ASCII text without decoding it, and the index of line starts skips the bytes no
break starts with. Tokens, diagnostics and positions are unchanged.

**Profile before the change** (ad-hoc timers, not in the tree; best of 300 to
500 passes over the corpus, the drop of the result included, so the figures are
not those of the table below, which excludes the drop): `lex` 308 us,
`lex_regions` 342, `tokenize` 1,263 (the gates are 0.92 ms of it; with the
gate pass skipped, 372), the facade 2,541, so the options conversion and the
projection together 1,278. Pieces of the projection measured apart: one string
per token with its release 501 us, the index of line starts 102 and the walk 158,
a `FileId` clone per token 120, the keyword test 37. Skipping one part of the
projection at a time: the text of the token -560 us, the position -182, the
token type -74, the `FileId` -0.

**Profile after** (the same timers, the drop excluded as in the benchmark):
`tokenize` 447 us (`lex` 304, regions 43, gates 94), the facade 1,222, so the
projection is about 775: the string per token 318, the token type 94, the
position 83, the rest (index of line starts 42, the vector and the `Token`
fields, the `FileId` clone) about 280.

#### Q2 after: token view, `tokenize_program`

Run 1 / 2 / 3; legacy is the default run, cst the run with the feature on.

| quantity | legacy | cst | cst / legacy |
|---|---|---|---|
| cold, one pass (ms) | 1.705 / 1.710 / 1.641 | 1.709 / 1.688 / 1.678 | 1.00 / 0.99 / 1.02 |
| warm-median sum (ms) | 0.919 / 0.909 / 0.869 | 1.155 / 1.151 / 1.141 | 1.26 / 1.27 / 1.31 |
| cold allocations | 9,448 | 9,430 | 1.00 |
| cold KiB allocated | 1,753.3 | 1,151.3 | 0.66 |

The warm-median sum was 2.39 / 2.41 / 2.44 times the legacy one before the
change. The legacy sums moved by 5.5 % between runs and the cst sums by 1.2 %, so
the ratio's spread is the legacy side's. Per file the ratio is 1.0-1.5 (smallest
0.78 / 0.83 / 1.09, 49 / 49 / 58 of 61 files above 1.25; the largest is
`bit_string_arithmetic.st` 1.45-1.47 on 4-6 us medians, and `namespace.st` 2.19
in run 1 on a 10 us median that the other runs do not repeat).

The direct rows of the syntax crate (default run, run 1 / 2 / 3), before the
change in brackets:

| path | warm-median sum (ms) | cold one pass (ms) |
|---|---|---|
| `cst tokenize` | 0.370 / 0.371 / 0.371 (1.252 / 1.240 / 1.240) | 0.596 / 0.586 / 0.577 (1.490 / 1.499 / 1.478) |
| `cst parse` | 7.228 / 6.999 / 7.013 (8.512 / 7.913 / 7.968) | 8.865 / 8.202 / 8.059 (10.100 / 9.342 / 9.554) |
| `cst parse + lower` | 9.427 / 9.156 / 9.221 (10.627 / 10.032 / 10.165) | 9.706 / 9.486 / 9.690 (11.119 / 10.400 / 10.484) |
| `cst parse + lower (budget)` | 9.849 / 9.594 / 9.559 (10.985 / 10.403 / 10.489) | 10.417 / 9.765 / 9.934 (11.376 / 10.984 / 10.954) |

#### Q1 after: whole file, `parse_program`

| quantity | legacy | cst | cst / legacy |
|---|---|---|---|
| cold, one pass (ms) | 12.582 / 12.256 / 11.754 | 10.647 / 10.795 / 11.101 | 0.85 / 0.88 / 0.94 |
| warm-median sum (ms) | 11.415 / 10.974 / 10.902 | 9.526 / 10.013 / 9.906 | 0.83 / 0.91 / 0.91 |
| cold allocations | 32,121 | 23,949 | 0.75 |
| cold KiB allocated | 4,544.6 | 2,800.8 | 0.62 |

The cst sum was 10.373 / 10.887 / 10.420 ms before the change and the ratio
0.90 / 1.01 / 0.96. Per file, the worst warm ratio is 1.22 / 1.29 / 1.32
(`type_decl.st`, `empty_var_block.st` and `first_steps_configuration.st`,
`oop.st`), 0 / 3 / 2 files above 1.25, none above 1.5; the allocation ratios are
those of Q1 above (worst `first_steps_configuration.st` 1.91).

#### Q5 after: `first_steps.st`, warm median (us), run 1 / 2 / 3

| call | legacy | cst |
|---|---|---|
| `tokenize_program` | 127.3 / 120.8 / 85.9 | 132.6 / 132.1 / 110.0 |
| `parse_program` | 679.0 / 685.5 / 674.2 | 518.1 / 628.7 / 611.2 |

#### Verdict after the change

| quantity | warm-median sum | allocations | verdict |
|---|---|---|---|
| Q1 whole file | 0.83 / 0.91 / 0.91 | 0.75 | met |
| Q2 token view | 1.26 / 1.27 / 1.31 | 1.00 | within the 1.5 budget; between 1.25 and 1.5, explained below |

The token view is within the budget and above the 1.25 line the budget asks to
be explained, by 0.24-0.27 ms on the corpus (`first_steps.st`, 2.7 KB: 110-133 us
against 86-127 us). Its allocation count is the legacy one (a string per token, the
`Token` the compiler reads has to own its text); what remains is non-allocating
work per token that the legacy path does not have: the regions and gates pass
over the tokens (about 0.11-0.13 ms), the token type decided from the tree's options
(about 0.09 ms) and the position mapped from a byte offset (about 0.08 ms), on
top of the same string and `FileId` per token (0.3 ms and 0.1 ms). Each is one
mechanism of the tree front end doing what the legacy transforms did in another
shape; going further means a cheaper `Token` (a borrowed text), which changes
the type the rest of the compiler reads, and is not part of this change.

Not measured again: Q3, Q4 and Q6 (the statement fragments also start with this
pass; their figures stand as recorded above). The figures of this follow-on come
from one build per front end on the same machine.

Reproduce: the commands of 3.7; the tables are the same output.

#### Follow-on: the stack budget at the entry of the run (2026-10-06)

The tables above stay as recorded. Q4 and Q5 counted threads per stage call
(1 per `parse_program`, 1 per `analyze`, N for a PLCopen XML document with N
bodies, about 2 per language-server edit). Production no longer pays that: each
program of the compiler gives itself the budget once, at its entry, and the
stages it calls find it (`ironplc_dsl::stack`: `within_stack_budget` runs on the
caller's thread when that thread already has the budget). The stage entries keep
their own call, so a caller that has no budget (a test, a benchmark, an
embedding program) still gets one from the entry it calls and nothing overflows.

**Where the budget is given.** `ironplcc`: `run` in `ironplc-cli/bin/main.rs`,
for every command, `lsp` included. The language server: `start_with_connection`
in `ironplc-cli/src/lsp.rs`, so the thread that serves requests is the budget
thread for the life of the server. The MCP server: `block_on_budget` in
`mcp/src/lib.rs`, which makes the runtime of one thread and runs it on the
budget, so the thread that runs the tools is the budget thread. `ironplcvm`
and `ironplcvmd` read no source. The playground has no threads (`wasm32`): the
budget is the 1 MiB of the shadow stack and every call is inline, as before.

**Threads per run, counted** (`spawns_by_current_thread`, a per-thread count;
before: the same code with the count and the tests added, the program entry not
yet wrapping):

| run | before | after | pinned by |
|---|---|---|---|
| `ironplcc check`, 3 files | 4 (3 parses, 1 analysis) | 1 | `run_when_check_over_three_files_then_one_thread_for_the_run` |
| `ironplcc echo`, 3 files | 6 (3 parses, 3 prints) | 1 | `run_when_echo_over_three_files_then_one_thread_for_the_run` |
| `ironplcc compile`, 3 files | 5 (3 parses, analysis, code generation) | 1 | `run_when_compile_over_three_files_then_one_thread_for_the_run` |
| language server, one edit | 2 (parse, analysis) | 0 | `start_with_connection_when_edits_are_served_then_no_request_makes_a_thread` |
| language server, three edits and a token request | 6 | 0 after the 1 of the server | `start_with_connection_when_server_runs_then_it_makes_one_thread_for_its_whole_life` |
| MCP `check` call | 2 | 0 | `block_on_budget_when_check_tool_runs_then_the_thread_has_the_budget_and_makes_none` |
| MCP `compile` call | 3 | 0 | `block_on_budget_when_compile_tool_runs_then_the_thread_has_the_budget_and_makes_none` |
| MCP, three `check` calls | 6 | 1 | `block_on_budget_when_server_serves_calls_then_the_whole_run_makes_one_thread` |
| PLCopen XML document, 20 bodies | 20 | 1 for the run (0 more) | `parse_source_when_xml_document_has_many_bodies_and_run_has_the_budget_then_one_thread_for_the_run` |
| 4 bundled library files | 4 | 1 for the run (0 more) | `load_activated_libraries_when_run_has_the_budget_then_one_thread_for_the_run` |

For a command over K files the count is K+1 (`check`), 2K (`echo`) and K+2
(`compile`) before and 1 after; per language-server edit it was 2 plus 1 for
each bundled library file of an activated compatibility library, and is 0. A caller with no budget still pays one thread for each stage entry it calls, as
before: `entries_when_caller_has_no_budget_then_each_makes_one_thread` holds each
stage entry to exactly one, and `entries_when_caller_has_the_budget_then_none_makes_a_thread`
to none on a held budget.

**Measured** with the same harness, corpus, machine, profile and method as 3.7
(release `bench` profile, Intel Core i5-9300H, Windows 11 Pro 10.0.26200, rustc
1.98.1, base commit `c7ac3389c` plus the change; another build ran on the
machine during these runs, so the absolute figures are higher and wider than in
3.7 and the ratios are the evidence). Three consecutive runs, run 1 / 2 / 3.
The rows `parse (held)`, `statements (held)` and `xml document (held)` run the
same call as `parse`, `statements` and `xml document` on a thread that holds the
budget (the attribute `stack` of a row of the table, `Held`); the thread is made
outside the measurement. A row without the attribute is what a program paid
before: each facade call spawned and joined its own thread.

| quantity | caller without the budget (before) | budget held (after) | after / before, run 1 / 2 / 3 |
|---|---|---|---|
| `parse`, 61 files, warm-median sum (ms) | 10.401 / 9.500 / 9.950 | 4.734 / 4.282 / 4.275 | 0.46 / 0.45 / 0.43 |
| `parse`, 61 files, cold, one pass (ms) | 14.661 / 10.392 / 10.804 | 4.736 / 4.335 / 4.416 | 0.32 / 0.42 / 0.41 |
| `parse`, 61 files, cold allocations | 23,874 | 23,621 | 0.99 |
| `statements`, 75 bodies, warm-median sum (ms) | 7.740 / 7.600 / 8.334 | 1.756 / 1.729 / 1.775 | 0.23 / 0.23 / 0.21 |
| `statements`, 75 bodies, cold allocations | 8,306 | 8,006 | 0.96 |
| `xml document`, 59 bodies, 11,399 bytes, warm median (ms) | 6.719 / 6.903 / 7.349 | 1.268 / 1.304 / 1.302 | 0.19 / 0.19 / 0.18 |
| `xml document`, cold allocations | 7,466 | 7,230 | 0.97 |
| `strings.st` (53 bytes), warm median (us) | 119.8 / 95.2 / 100.3 | 21.9 / 17.5 / 17.4 | 0.18 / 0.18 / 0.17 |
| `first_steps.st` (2,719 bytes), warm median (us) | 576.6 / 518.2 / 519.1 | 436.4 / 404.6 / 407.0 | 0.76 / 0.78 / 0.78 |
| `stack budget, empty work`, 61 calls, warm-median sum (ms) | 5.130 / 4.579 / 5.020 | | |

The allocations that go are the 4 of each thread (59 x 4 = 236 for the document,
75 x 4 = 300 for the bodies). The document is the case the thread cost compounds
in: 59 bodies were 59 threads, 5.4-6.0 ms of the 6.7-7.3 ms, and are now none
beyond the thread of the program. The edit of a language-server session loses
its two threads: for `first_steps.st` the `parse` row falls by 0.11-0.14 ms (576.6 to 436.4 us
in run 1), and the analysis loses a thread of the same size, which was not
measured. `parse (held)` of the 53-byte `strings.st` is 17.4-21.9 us, the cost of the parse
without the thread. The figure that Q4 and 5.2 of the architecture
document record (the thread as 43-45 % of the corpus parse sum) is what a stage
call paid when the caller had no budget; with the budget held it is 0 % of the
run, and one thread of the program (about 75-84 us, the mean of the 61 calls of
the empty row) for the whole run.

**Does the tree cross threads?** No path of production builds a tree on one
thread and lowers it on another. The one place that builds a tree and lowers it
is `parser/src/frontend.rs` (`parse_program`, `parse_st_statements`), and it
does both inside one `within_stack_budget`. The other callers of the tree
builder and of the lowering are tests (`analyzer/src/lowered_*.rs`,
compiled for tests only) and this crate's benchmarks.

Reproduce: `cargo bench --package ironplc-benchmarks --bench parse_baseline`
(from `compiler/`); the rows named `(held)` and `xml document` are in the same
output. The counts come from `cargo test`: the tests named above.


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
- Spike crate: `compiler/s0-spike/` (removed at the S1 cutover)
