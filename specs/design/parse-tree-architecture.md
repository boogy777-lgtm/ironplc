# Design: Lossless CST and Dependency-Tracked Semantic Analysis

status: approved
date: 2026-10-01

## Overview

The target frontend has one lossless concrete syntax tree (CST), one
semantic authority, and one mechanism for dependency-tracked computation.
The owner approved this direction after reviewing CODESYS's editor tree,
precompile model and selective compilation. This replaces the previous
white/CST → red → green staircase. The architecture is approved; the CST
and tracked analysis are **not implemented** by this documentation change.

Use rowan for the CST's immutable green storage and derived red navigation
views. Green and red represent the same syntax, not separate grammars or
semantic models. The existing dsl AST and analyzer remain the semantic
authority. Derive their input from the CST and adapt analysis into tracked
results progressively; do not add a second type checker.

Full-file parsing is the initial correctness baseline. Dependency-tracked
semantic recomputation is a separate milestone. Local subtree reparsing is
a later optimization justified by measurements. Neither rowan nor a query
cache makes the parser incremental automatically.

Section 5 records future stage acceptance criteria. Implementation changes
add requirement IDs and real conformance tests together, following
[Spec Conformance Testing](spec-conformance-testing.md). This approved
design does not claim those future criteria already pass.

## Design Goals

1. **One grammar authority.** The production parser constructs one CST;
   semantic AST construction lowers it rather than parsing ST again.
2. **One semantic authority.** Reuse dsl and analyzer rules for CLI, LSP,
   MCP and compilation. Syntax views have no resolved types or names.
3. **One tracking mechanism.** Consumers request results from one analysis
   service, without private symbol databases or invalidation engines.
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
| Analyzer and tracked results | Scopes, resolution, types, validation and result dependencies | UI policy, controller admission or migration |
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
different snapshot. Initially a query may wrap whole-project analysis.
Split it as dependency boundaries become explicit and tested. A cached
result is a product of the same analyzer, not another HIR/resolver.

### 3.3 Dependency-Tracked Computation

Use one query mechanism. Salsa is the preferred candidate for S0's
integration experiment, which records a version and checks snapshots,
cancellation, cycle handling and compatibility with the analyzer. Do not
combine copied CODESYS dirty flags with a second independent query graph.
No SQL or disk database is required.

Inputs include source text, effective CompilerOptions/dialect, project
membership, imports/library interfaces, and target settings that affect
types or codegen. Results depend on every input that affects them.
Lookup depends on a relevant scope even when the name is absent, so a new
declaration invalidates an earlier unresolved lookup.

Start with declaration/interface summaries, POU bodies, scopes and type
dependencies. Separate body-derived meaning from current source locations.
Text edits refresh syntax and location projections; equal meaning-bearing
results stop propagation to consumers that do not read the changed text.
Pragmas, documentation attributes and other observable metadata retain their
dependencies; never discard all trivia as semantically irrelevant.

| Change | Expected dependency behavior |
|---|---|
| Ordinary whitespace/non-semantic comment | Refresh syntax and locations; equal semantic summaries stop propagation |
| Body with unchanged declaration | Recheck body and its consumers; declaration-only callers can reuse results |
| Signature/exported name/declaration | Refresh dependent resolution/typing, including missing-name lookups |
| Type structure/constant/initializer/codegen attribute | Invalidate readers of the affected value, layout or behavior |
| Dialect/target/library/project membership | Invalidate dependent results; full recomputation if tracking is incomplete |

These are future acceptance criteria. Body-only edits may affect inlining
or other body-reading consumers, which must record that dependency.
Queries have no controller/UI side effects. Recursive dependencies use an
explicit cycle/SCC policy that preserves supported recursion and reports
forbidden cycles through existing diagnostics, not stale results or panics.

Result equality accounts for every observable property. Hashes may speed
comparison; define a collision policy that cannot silently authorize stale
reuse. Syntax identity and checksums do not prove StableStateId equality
or bumpless hot edit. Runtime admission and migration keep their owners.

### 3.4 N+1 Check

Count independent mechanisms, not representations or Rust structs.
Green storage, red navigation and syntax wrappers implement one syntax
mechanism. Lossless syntax and semantic analysis have different invariants;
the analyzer remains a separate authority. Tracking manages result reuse,
not language meaning.

A new language extension adds grammar/lowering/rules within these owners.
An editor consumer reads the same syntax and analysis service. A tracked
result uses the same query mechanism. If any introduces another parser,
resolver or invalidation engine, reconsider the abstraction first.

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

The approved foundation is S0–S4; none is delivered by this document.
S5 is conditional optimization. Syntax-gap work continues against one
grammar; S0 settles that seam before a new frontend path is introduced.

| Stage | Work and boundary | Exit evidence |
|---|---|---|
| **S0 — integration experiment** | Audit preprocessing/provenance; assess PEG extension versus scoped replacement; validate rowan/Salsa adapter; record prefactoring | Standard/CODESYS/malformed corpus, OSCAT/Unicode/pragmas; parser choice, dependency versions, file map and benchmark baseline; no dual production parser |
| **S1 — CST and recovery** | Original-text CST with rowan green storage/red views; full-file parsing and syntax diagnostics | Byte-identical valid/malformed reconstruction; CRLF/tabs/Unicode/comments/pragmas; complete ranges, recovery progress/termination; existing dialect tests preserved |
| **S2 — CST-to-dsl lowering** | Replace semantic parse entry with lowering; retain preprocessing meaning through provenance; reuse analyzer/codegen | Legacy/new AST/diagnostic comparison; canonical plc2plc and analyzer/codegen regressions pass; one production parse path; losslessness retained |
| **S3 — tracked analysis** | Wrap analyzer, then split declaration summaries/body analysis/locations; track all inputs and missing lookups | After each edit, equality with clean rebuild; execution counters prove unaffected reuse and affected recomputation; deletion/rename/body/signature/type/cycle/dialect/library/target cases |
| **S4 — shared consumers and editing** | CLI/LSP/MCP/build use one snapshot API; syntax edits; bound cache/snapshot retention | Same-snapshot diagnostics agree; revision/cancellation exclude stale publication; untouched text preserved; measured cold/warm latency and memory limits; no private resolver/query graph |
| **S5 — measured local reparse** | Safe boundaries and full-file fallback; reuse unaffected syntax | Local/full trees and diagnostics agree after edit sequences, including delimiter/comment/pragma changes; measured latency/allocation improvement |

Measure cold full analysis separately from warm no-change requests, body,
interface and configuration edits. Record corpus size/POU count, hardware,
toolchain, latency distribution, allocations/retained memory, query
executions and cache reuse. S0 sets numeric budgets from the workload.
Do not claim universal speed superiority from rowan or Salsa.

Implementation stages register requirement IDs and assertions in the actual
owning crates as they land. Do not add placeholder tests or empty crates
to claim a future stage is implemented. Advance the status to **partially
implemented** with named delivered stages, then **implemented** when S1–S4
describe the working frontend. S5 may remain unnecessary.

## 6. References

- [rowan](https://github.com/rust-analyzer/rowan) and
  [rust-analyzer syntax architecture](https://rust-analyzer.github.io/book/contributing/syntax.html)
- [Salsa](https://github.com/salsa-rs/salsa) — memoized computation with
  tracked inputs/dependencies; integration version selected in S0
- [CODESYS source evidence](#21-direct-source-references)
- [Syntax gaps](../../Codesys/LEXER-GAP-ANALYSIS.md) and
  [syntax/AST mapping](../../Codesys/grammar/AST_MAPPING.md)
- [Spec Conformance Testing](spec-conformance-testing.md),
  [Online Editing UX](online-editing-ux.md),
  [W32](../implementation/dcs-platform/tasks/W32-ide.md)
- [Stable variable IDs](../adrs/0053-stable-variable-ids-for-declaration-level-hot-edit.md)
  and [IDE-side UID persistence](../roadmap.md#phase-3---ide-side-uid-persistence)
