# Design: Lossless CST and Recomputing Semantic Analysis

status: partially implemented
date: 2026-10-01

## Overview

The target frontend has one lossless concrete syntax tree (CST) and one
semantic authority, the existing analyzer. The owner approved this direction
after reviewing CODESYS's editor tree, precompile model and selective
compilation. This replaces the previous white/CST → red → green staircase.
The status is **partially implemented**: S0 is delivered — the
preprocessing/provenance [audit](parse-tree-s0-audit.md), the rowan/Salsa
[experiment](parse-tree-s0-experiment.md) with the parser choice and
dependency versions, and the parse benchmark baseline. S1–S4 are
**not implemented**, and the production frontend remains the PEG path.

On 2026-10-01 the owner **withdrew the query-based tracking mechanism**:
Salsa and dependency-tracked query caches are not part of the plan. Semantic
results are produced by the existing analyzer **recomputing the requested
snapshot**; the baseline is a full recomputation, always available. Reuse
optimizations, if ever justified by measurements, follow the **CODESYS style**
recorded in section 2 — a precompile model of declaration and body summaries
with checksums, selective typification along caller/referencer/declarer
dependencies, and explicit invalidation — never a query framework.

Use rowan for the CST's immutable green storage and derived red navigation
views. Green and red represent the same syntax, not separate grammars or
semantic models. The existing dsl AST and analyzer remain the semantic
authority. Derive their input from the CST; do not add a second type checker.

Full-file parsing is the initial correctness baseline. Incremental reuse is
a later, measured optimization in the CODESYS style. Local subtree reparsing
is a later optimization justified by measurements. Neither rowan nor a cache
makes the parser incremental automatically.

Section 5 records future stage acceptance criteria. Implementation changes
add requirement IDs and real conformance tests together, following
[Spec Conformance Testing](spec-conformance-testing.md). This approved
design does not claim those future criteria already pass.

## Design Goals

1. **One grammar authority.** The production parser constructs one CST;
   semantic AST construction lowers it rather than parsing ST again.
2. **One semantic authority.** Reuse dsl and analyzer rules for CLI, LSP,
   MCP and compilation. Syntax views have no resolved types or names.
3. **One recomputation baseline.** Consumers request results for a chosen
   snapshot from one analysis service; the analyzer recomputes it. Reuse
   optimizations, measured before adoption, follow the CODESYS style of
   explicit dependencies and invalidation, without private query graphs.
4. **Correctness before optimization.** Incremental results equal a clean
   rebuild of the same snapshot. Full recomputation remains available when
   dependencies cannot be established safely.
5. **Bounded evolution.** Preserve existing language behavior, diagnostics
   and backend contracts while replacing the frontend seam in stages.

## 1. Current Architecture

At baseline bfa5d9fd7cb30a2f7ec83fc10bbaf95121fa0d2e, IronPLC uses:

- **compiler/parser/src/lib.rs:** tokenize_program preprocesses text, runs
  the logos lexer, applies token transforms/checks, then parse_program calls
  the PEG parse_library grammar.
- **compiler/parser/src/preprocessor.rs:** OSCAT ranged-comment contents
  become whitespace. This semantic-input transformation is not lossless
  storage of the original text.
- **compiler/parser/src/token.rs and parser.rs:** tokens include trivia;
  the PEG grammar skips trivia and constructs the dsl Library AST.
- **compiler/analyzer/src/:** named transformation passes and semantic
  rules resolve the program using existing environments and context.
- **compiler/plc2plc/:** AST rendering produces canonical text. Existing
  round trips prove AST equivalence and formatting fixed points, not
  recovery of original source bytes.

The AST is a Rust data model, not evidence that all names and types are
resolved immediately after parsing. Codegen consumes analyzed program data;
the VM consumes compiled artifacts, not the CST or AST. Existing spans
provide provenance. This design does not assume every AST node has a span
or that current token transforms preserve original text.

## 2. What the CODESYS Sources Establish

The reference is static decompilation of CODESYS 3.5.22.10 in Codesys/.
These mechanisms are visible in source; performance and complete editor
behavior have not been measured.

| Mechanism | Evidence | Consequence for IronPLC |
|---|---|---|
| Editor retains comments, whitespace, line endings, pragmas and erroneous tokens | TokenStream options and WhiteTreeParser recovery | Preserve original text and incomplete input |
| Compiler and editor have distinct parsers | Parser35220 and WhiteTreeParser | Borrow syntax behavior; keep one production grammar |
| Compact compiler trees expand to working red ASTs on demand; weak red cache | StandardParseTreeProvider and GreenTreeContext | Materialize derived data on demand; CODESYS red AST is not a rowan view |
| Precompile model stores signatures and POU bodies, compares checksums and emits changes | PreCompileContext | Separate declaration and body results |
| Selective typification follows caller/referencer/declarer relationships; compilation has its own selection | ObjectsToTypifyDetector and ObjectsToCompileDetector | Track semantic dependencies, beyond syntax reuse |
| Background workers check queued signatures and POU code | PrecompileChecksWindows | Schedule analysis outside the UI |
| Old/new expressions map breakpoint positions | ExpressionComparer and SourceInformationSynchronizer | Separate locations from semantic and executable identity |

WhiteTreeParser's ParseStImplementation and ParsePOUSyntax create fresh
token streams for the supplied text. These entries do not establish local
subtree reparsing. ObjectsToTypifyDetector selects full typification outside
its eligible Online Change path and for several project-wide changes.
Do not generalize these paths into an always-minimal editor guarantee.
The visible implementation uses explicit dependencies and invalidation;
it is not evidence of a Salsa-like framework.

### 2.1 Direct Source References

- [TokenStream](../../Codesys/decompiled/WhiteParsetrees.plugin/CODESYS/WhiteParseTrees/Parser/TokenStream.cs)
  and [WhiteTreeParser](../../Codesys/decompiled/WhiteParsetrees.plugin/CODESYS/WhiteParseTrees/Parser/WhiteTreeParser.cs)
- [InfixOperationParser](../../Codesys/Parser35220.plugin/CODESYS/Parser35220/Expressions/InfixOperationParser.cs)
  — recursive descent through operator-precedence levels
- [StandardParseTreeProvider](../../Codesys/decompiled/LanguageModelManager.plugin/_3S/CoDeSys/LanguageModelManager/StandardParseTreeProvider.cs)
  and [GreenTreeContext](../../Codesys/decompiled/LanguageModelManager.plugin/_3S/CoDeSys/LanguageModelManager/GreenTrees/GreenTreeContext.cs)
- [PreCompileContext](../../Codesys/decompiled/LanguageModelManager.plugin/_3S/CoDeSys/LanguageModelManager/PreCompileContext.cs)
  and [PrecompileChecksWindows](../../Codesys/decompiled/Compiler35220.plugin/_3S/CoDeSys/Compiler35220/PreCompile/PrecompileChecksWindows.cs)
- [ObjectsToTypifyDetector](../../Codesys/decompiled/Compiler35220.plugin/_3S/CoDeSys/Compiler35220/Phase1_Typification/ObjectsToTypifyDetector.cs)
  and [ObjectsToCompileDetector](../../Codesys/decompiled/Compiler35220.plugin/_3S/CoDeSys/Compiler35220/Phase2_AfterTypification/ObjectsToCompileDetector.cs)
- [ExpressionComparer](../../Codesys/decompiled/LanguageModelManager.plugin/_3S/CoDeSys/LanguageModelManager/ExpressionComparer.cs)
  and [SourceInformationSynchronizer](../../Codesys/decompiled/LanguageModelManager.plugin/_3S/CoDeSys/LanguageModelManager/SourceInformationSynchronizer.cs)

Reports [07](../../Codesys/docs/07_AST_RED_TREE_CONSTRUCTION.md) and
[16](../../Codesys/docs/16_WHITE_PARSE_TREES.md) remain CODESYS research.
Their tree-port suggestions are not the IronPLC implementation contract;
this design owns that contract.

## 3. Target Architecture and Ownership

| Part | Owns | Does not own |
|---|---|---|
| Source snapshot | Original text, source identity, revision and effective configuration | Resolved meaning or runtime state |
| Parser and rowan CST | Tokens, trivia, syntax, recovery; green storage and red views | Name resolution, types or deployment identity |
| CST-to-dsl lowering | Semantic AST input and original-source provenance | Another grammar or type checker |
| Analyzer and computed results | Scopes, resolution, types, validation and result dependencies | UI policy, controller admission or migration |
| CLI/LSP/MCP/build | Requests and projections for a chosen snapshot | Private parsers, resolvers or invalidation rules |
| Codegen/runtime | Existing artifact and execution contracts | Persistent state identity derived from CST pointers |

### 3.1 Lossless CST

Parse the original decoded ST text before destructive preprocessing. Every
byte of that UTF-8 text belongs to a token or error region. Retain comments,
CRLF/LF spelling, tabs, pragmas, inactive regions and malformed fragments.
Synthetic recovery tokens have no source text and do not alter reconstruction.
Container decoding/XML escaping remain source-adapter concerns; CST
fidelity concerns the extracted ST text.

Audit provenance for OSCAT processing, collapsed pragmas, split duration
tokens, demoted keywords and inserted terminators. Semantic preprocessing
selects or interprets original syntax; it must not overwrite stored source.
Transforms must lower correctly without inventing or losing source bytes.

Use rowan green storage from the first CST stage. Red navigation and typed
syntax wrappers are views over it. Edits create new snapshots. Views and
shared nodes add no semantic authority. Effective structural reuse is
measured independently; it does not justify another bespoke tree.

Keep plc2plc's canonical AST-rendering contract. CST reconstruction has
separate byte-equality tests. Format-preserving edits prove untouched text
unchanged; a canonical formatter need not reproduce the original file.

### 3.2 One Semantic Model

Lower valid CST constructs to the existing dsl AST and reuse analyzer rules
and backend interfaces. Error nodes support editor services and diagnostics;
invalid or incomplete programs cannot reach executable code generation as
though valid.

Immutable source/analysis snapshots isolate readers. Cache owned, versioned
results; analyzer transformations must not mutate AST data shared with a
different snapshot. Initially one analysis entry point may cover the whole
project; split it as reuse stages land and their boundaries are tested. A
cached result is a product of the same analyzer, not another HIR/resolver.

### 3.3 Semantic Recomputation

The baseline mechanism is a full recomputation: consumers request analysis
for a chosen source snapshot, and the existing analyzer produces every
result from the CST, the effective `CompilerOptions`/dialect, project
membership, imports/library interfaces and target settings. No query
framework, no Salsa: the owner withdrew that mechanism on 2026-10-01, after
the S0 experiment showed the CODESYS sources build reuse from explicit
dependencies and invalidation instead (section 2).

Reuse optimizations, if measurements ever justify them, follow the CODESYS
style, each landing as its own measured stage:

- **Precompile model.** Separate declaration/interface summaries and POU
  bodies with checksums, compared before re-analysis — the role
  `PreCompileContext` plays in CODESYS — so an unchanged signature reuses
  its declaration results.
- **Selective typification.** Follow caller/referencer/declarer dependencies
  (`ObjectsToTypifyDetector`) to re-analyze affected POU bodies only; a
  missing-name lookup still depends on its scope, so a new declaration
  invalidates earlier unresolved lookups.
- **Explicit invalidation.** Inputs and summaries are invalidated by named
  dependencies (source text, dialect, library interfaces, target settings);
  never a second query graph, never copied dirty flags mixed with one.

Until such a stage lands, the recompute baseline is the only behavior:
correctness before optimization, and a clean rebuild of the same snapshot
stays available even where dependencies cannot be established safely.
Result reuse never changes diagnostics or backend contracts; runtime
admission and migration keep their owners.

### 3.4 N+1 Check

Count independent mechanisms, not representations or Rust structs.
Green storage, red navigation and syntax wrappers implement one syntax
mechanism. Lossless syntax and semantic analysis have different invariants;
the analyzer remains a separate authority. Reuse optimizations manage result
reuse, not language meaning.

A new language extension adds grammar/lowering/rules within these owners.
An editor consumer reads the same syntax and analysis service. A reuse
optimization extends the same recompute baseline. If any introduces another
parser, resolver or invalidation engine, reconsider the abstraction first.

## 4. Boundaries and Constraints

- **Evidence-driven parser choice.** Assess extending the IronPLC PEG seam
  for CST emission/recovery. A recursive-descent/Pratt replacement, including
  scoped trust-syntax reuse, is an alternative experiment, not a parallel
  production parser. Compare coverage, provenance, recovery, maintenance,
  licensing and API cost.
- **No wholesale truST transplant.** Do not attach truST HIR/IDE/LSP as
  another semantic backend. Reused syntax feeds the same CST and analyzer.
  The CODESYS syntax-gap backlog remains applicable.
- **Behavior-preserving migration.** Keep accepted syntax, dialect gating,
  AST meaning, diagnostics and backend behavior except specified fixes.
  Temporary comparison paths are test-only and removed at cutover.
- **Snapshot-bound positions.** Derive spans from CST provenance and source
  mappings. Publish diagnostics only for the requested revision. Syntax
  pointers are not durable IDs; retain the existing UID/refactor contract.
- **Engineering-side tracking.** POU analysis does not change deployment
  units, container format, hot-edit FSM or runtime authority. Executable
  artifact reuse per POU is separate backend work.
- **W32 is a consumer.** Its command/session work proceeds independently.
  The compiler frontend does not wait for a controller connection or an
  in-process IDE compiler.

## 5. Evolution Steps

The approved foundation is S0–S4; only S0's evidence is delivered so far
(audit, experiment, baseline — linked above), and S1, S2 and S4 remain not
implemented. S3 is withdrawn (owner decision, 2026-10-01 — see §3.3). S5 is
conditional optimization. Syntax-gap work continues against one grammar; S0
settled that seam before a new frontend path is introduced.

| Stage | Work and boundary | Exit evidence |
|---|---|---|
| **S0 — integration experiment** | Audit preprocessing/provenance; assess PEG extension versus scoped replacement; validate the rowan adapter (the Salsa part of the spike was withdrawn afterwards, §3.3); record prefactoring | Standard/CODESYS/malformed corpus, OSCAT/Unicode/pragmas; parser choice, dependency versions, file map and benchmark baseline; no dual production parser |
| **S1 — CST and recovery** | Original-text CST with rowan green storage/red views; full-file parsing and syntax diagnostics | Byte-identical valid/malformed reconstruction; CRLF/tabs/Unicode/comments/pragmas; complete ranges, recovery progress/termination; existing dialect tests preserved |
| **S2 — CST-to-dsl lowering** | Replace semantic parse entry with lowering; retain preprocessing meaning through provenance; reuse analyzer/codegen | Legacy/new AST/diagnostic comparison; canonical plc2plc and analyzer/codegen regressions pass; one production parse path; losslessness retained |
| **S3 — tracked analysis** | **Withdrawn (2026-10-01, owner decision):** recompute per snapshot; reuse in the CODESYS style (precompile model, selective typification, explicit invalidation) only as measured stages, §3.3 | — |
| **S4 — shared consumers and editing** | CLI/LSP/MCP/build use one snapshot API; syntax edits; bound cache/snapshot retention | Same-snapshot diagnostics agree; revision/cancellation exclude stale publication; untouched text preserved; measured cold/warm latency and memory limits; no private resolver/query graph |
| **S5 — measured local reparse** | Safe boundaries and full-file fallback; reuse unaffected syntax | Local/full trees and diagnostics agree after edit sequences, including delimiter/comment/pragma changes; measured latency/allocation improvement |

Measure cold full analysis separately from warm no-change requests, body,
interface and configuration edits. Record corpus size/POU count, hardware,
toolchain, latency distribution, allocations/retained memory, and reuse
counters for any measured reuse stage. S0 sets numeric budgets from the
workload. Do not claim universal speed superiority from rowan or any cache.

Implementation stages register requirement IDs and assertions in the actual
owning crates as they land. Do not add placeholder tests or empty crates
to claim a future stage is implemented. Advance the status to **partially
implemented** with named delivered stages, then **implemented** when S1, S2
and S4 describe the working frontend. S5 may remain unnecessary.

### 5.1 Requirements of the Syntax Crate

`ironplc-syntax` owns the lossless tree and the lowering of that tree to the
`ironplc_dsl` objects. The requirements below are tested there, over the
corpus the repository shares (every `.st` source of the repository, each read
in LF, CRLF and tab-indented spelling) and every option set the claim names. The
requirements with the slug `parser` are tested in `ironplc-parser`, over the same
corpus.

**REQ-PT-syntax-001** For every file of the corpus, in LF, CRLF and
tab-indented spelling, under every option set, the text of the tree equals the
text parsed, whether or not the parse reports errors.

**REQ-PT-syntax-002** An OSCAT ranged-comment pair, and with `allow_pragma_if`
each branch of an `{IF}` that is not taken, is one trivia token of the tree
holding the bytes it covers; the grammar does not read what is inside.

**REQ-PT-syntax-003** Lowering is total over the text the parser accepts: for
every file of the corpus and every prefix of it, the parse and the lowering
return, and a lowering that fails reports a problem other than an internal
error (`P9998`) or a capability that is not implemented (`P9999`).

**REQ-PT-syntax-004** Lowering a parse that reported errors fails with its
primary error (REQ-PT-syntax-009), positioned in the file given, and builds no
object.

**REQ-PT-syntax-005** Every kind of node that a lowering rule owns occurs in a
file of the corpus that lowers, so a construct added to the grammar has a rule
and a case before the corpus passes. The disposition table that assigns each
kind to a rule, to its parent's rule or to trivia has no wildcard arm, so a
kind added without a decision does not compile.

**REQ-PT-syntax-006** Every span of a lowered library carries the file the
text came from and lies inside the text, on character boundaries.

**REQ-PT-syntax-007** Spelling the trivia differently (CRLF line ends, tabs for
indentation) does not change what is lowered: a file and a spelling of it with
the same significant tokens lower to equal libraries.

**REQ-PT-syntax-008** Lowering a tree as deep as the parser allows, for every
construct that nests, completes on the stack budget and reports no internal
error.

**REQ-PT-syntax-009** A parse reports every error it finds, and one of them is
primary: the error of the earliest stage that finds errors, and the earliest in
the text among the errors of that stage. The stages are one table
(`ironplc_syntax::STAGES`) in the order the established checks run: text that
makes no token, one gated form after another, the conditional pragmas, the
grammar. Every kind of error is in exactly one stage. Lowering runs only on a
tree without errors, so a problem it reports follows every stage.

The syntax crate also owns what the front end that reads the tree needs of
the text itself: the tokens of a text without a parse, and where a byte is.

**REQ-PT-syntax-010** `ironplc_syntax::tokenize` is the first stage of a parse
and nothing besides it: for every file of the corpus, in LF, CRLF and
tab-indented spelling, under every option set, its tokens tile the text, and its
errors are errors of the parse of the same text, with the same kind, message and
range, in source order. A parse takes its tokens and its first errors from this
function (there is no second lexing sequence), and tokenizing needs no thread of
its own: it does not recurse on the input, so it completes on the caller's
stack for input nested far past the depth limit.

**REQ-PT-syntax-011** A position in a text is counted by one mapping,
`ironplc_syntax::line_index::LineIndex`, over one table of line breaks
(`LINE_BREAKS`): `\r\n` is one break, a lone `\r` is a break, and so is a form
feed. A column counts UTF-16 code units. A text that is embedded in a document
starts at the line and the column it is given, and the column offset applies
to the first line only. The lexer reads a newline token with the same table, so
that every newline token of a text ends a line.

The front end that reads the tree is in `ironplc-parser`, which converts to and
from the objects the rest of the compiler reads.

**REQ-PT-parser-012** The token view of the tree (`tokenize_program` of the
front end built on the tree) is the token sequence of the legacy pipeline except
for the differences that are rows of one table, each with its reason: a duration
literal is not split into its parts (`T#1m30s` is `T`, `#`, `1`, `m30s`); no
synthetic empty `;` token is inserted; a region the grammar does not read (an
OSCAT ranged comment, an untaken `{IF}` branch) is one comment token; a lone
carriage return is a line break. A difference that no row names fails, and so
does a row that explains no difference. The kind map is one dispatch over the
token kinds of the tree with no wildcard arm, so a token kind added to the tree
does not compile until the view says what it makes of it.

**REQ-PT-parser-013** The compiler options become the options of the tree flag
by flag, by name, in one function (`parse_options`), driven by the list of the
flags the tree reads: every such flag has a counterpart among the compiler
options, so a flag that is renamed or removed fails a test instead of reading
as off, and for every dialect each flag has the value it has in the compiler
options.

**REQ-PT-parser-014** The public functions of the crate call one row of a table
of front ends, the one named by `frontend::SELECTED`: the legacy pipeline, or
the one built on the tree where the `cst-frontend` feature is on. The feature
is named in no other module of the crate's code, and every row of the table
answers the same questions, so adding a front end is one row.

The old-against-new comparison that held the lowering to the PEG parser it
replaces is test-only and lives with that parser, in `ironplc-parser`. It
compares the objects strictly (their printed form, the sequence of their spans
and the grouping of their variable blocks) over the corpus, the declaration
tables and the declarations lifted from the legacy tests, under every dialect
preset, and the one diagnostic each parser reports for every input both reject:
its problem code and byte range, and what it says. Each remaining difference is a row of a table with its reason, and the
reason is one of three classes: a defect of the old parser that is not ported,
a change the owner decided, or a form the new parser accepts on purpose. A
difference that is none of them is fixed in the new parser. The decisions the
comparison settled are recorded in
[ADR-0072](../adrs/0072-lowering-lives-in-the-syntax-crate.md).

## 6. References

- [rowan](https://github.com/rust-analyzer/rowan) and
  [rust-analyzer syntax architecture](https://rust-analyzer.github.io/book/contributing/syntax.html)
- [CODESYS source evidence](#21-direct-source-references)
- [Syntax gaps](../../Codesys/LEXER-GAP-ANALYSIS.md) and
  [syntax/AST mapping](../../Codesys/grammar/AST_MAPPING.md)
- [Spec Conformance Testing](spec-conformance-testing.md),
  [Online Editing UX](online-editing-ux.md),
  [W32](../implementation/dcs-platform/tasks/W32-ide.md)
- [Stable variable IDs](../adrs/0053-stable-variable-ids-for-declaration-level-hot-edit.md)
  and [IDE-side UID persistence](../roadmap.md#phase-3---ide-side-uid-persistence)
