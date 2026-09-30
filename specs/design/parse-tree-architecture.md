# Design: Parse-Tree Architecture — Current Model and Future White/Green/Red Layers

status: approved
date: 2026-09-30

## Overview

This document records two things: the parse/semantic pipeline IronPLC has
today, and the owner-decided evolution path toward a CODESYS/Roslyn-style
multi-tree architecture. The path is documented and bounded, but it is **not
built now**. `status: approved` records that the direction is decided; it
schedules no work. Section 5 gates each future layer on the obligation that
justifies it, and this document's first rule is negative: do not build a
layer ahead of its obligation.

The trigger era is in-process, incremental editing in the IDE
([W32](../implementation/dcs-platform/tasks/W32-ide.md)): format-preserving
edits, incremental reparse, and multi-consumer mutations. Until that
obligation is real, one typed AST with source spans is complete and minimal —
the prior analysis: M = 1 now is elegant, and a tree ported ahead of its
obligation is a crutch.

This design builds on:

- [Spec Conformance Testing](spec-conformance-testing.md) — how each future
  stage earns requirement IDs and conformance tests when its work lands
- [Online Editing UX](online-editing-ux.md) — the editor contract in which
  the incremental-editing trigger appears
- [W32](../implementation/dcs-platform/tasks/W32-ide.md) — the IDE work
  package of the trigger era

## Design Goals

1. **One semantic authority.** The `dsl` AST remains the only model the
   analyzer, codegen, VM and `plc2plc` consume. Tree layers are presentation
   and editing; they carry no types, no resolved names, no runtime meaning.
2. **One grammar authority.** A CST, when it exists, is emitted by the same
   parser that emits the AST — never by a second, independently drifting ST
   parser (the liability CODESYS carries; see Section 2).
3. **Append-only seams.** Adding a layer must not rewrite the passes below
   it: `parser → CST → AST` derivation is additive, and when the AST is
   unchanged the analyzer, codegen and renderer tests stay untouched.
4. **Evidence before layers.** Each stage in Section 5 names the trigger that
   justifies it and the evidence that closes it. A layer with no obligation
   is a crutch, not architecture.

## 1. Current Architecture

One pass, no intermediate tree:

```
source text
  → preprocess()          comments → whitespace; {IF} pragma trivia stays opaque
                          compiler/parser/src/preprocessor.rs
  → tokenize() + xforms   logos lexer; the token stream KEEPS trivia:
                          Newline, Whitespace, Comment and the collapsed Pragma
                          compiler/parser/src/lexer.rs, compiler/parser/src/token.rs:63
                          xform_collapse_pragmas, xform_split_duration_units,
                          insert_keyword_statement_terminators, xform_demote_keywords,
                          rule_token_* checks
                          compiler/parser/src/lib.rs:62
  → parse_library()       PEG grammar; rules map 1:1 to IEC 61131-3 Appendix B
                          compiler/parser/src/parser.rs:11, rule `_` at :373
  → dsl AST (Library)     typed; every node carries SourceSpan / Located
                          compiler/dsl/src
  → xform passes          resolve, fold, toposort, … (16 passes)
                          compiler/analyzer/src/xform_*.rs
  → consumers             analyzer rules → codegen → VM; plc2plc renderer
```

The seams that matter for this document:

- **Token tables** — `TokenType` (`compiler/parser/src/token.rs`) already
  produces the trivia tokens; the PEG rule `_`
  (`compiler/parser/src/parser.rs:373`) skips them, and the token-level
  xforms collapse and annotate tokens before parsing. The raw material for a
  CST is therefore present in the stream; no model retains it.
- **PEG grammar** — `parser.rs` rules generally map 1:1 to the formal
  specification (Appendix B), with the two documented naming exceptions
  (`compiler/parser/src/parser.rs:11-15`). Grammar identity is held by a
  single source of truth, and
  [`grammar/AST_MAPPING.md`](../../Codesys/grammar/AST_MAPPING.md) keeps that
  grammar aligned with the CODESYS syntax surface.
- **`dsl` AST** — the single semantic authority: one typed tree for the whole
  pipeline; all mutation is confined to the named xform passes.
- **Positions** — `SourceSpan` on every token and AST node; diagnostics, LSP
  and the debug pipeline read spans, not a separate position map.
- **`plc2plc` printer + round-trip tests** — the formatting contract (Section
  1.1).

### 1.1 The formatting contract today

Formatting is canonicalization, not preservation: `plc2plc` renders the AST
to text with its own layout, and tests prove the rendering is valid input for
the same dialect:

- `assert_round_trips` — parse → render → re-parse, same AST
  (`compiler/plc2plc/src/tests/common.rs:69`),
- `assert_round_trips_idempotently` — renderings that deliberately normalize
  to a different spelling must be fixed points (`…/common.rs:101`),
- `assert_resource_renders_to` — the same round trip pinned against a golden
  file (`…/common.rs:130`).

Consequently trivia is not part of any model: comments and the original
whitespace are consumed by the parser and lost. A text-to-text round trip
(`text → tree → text`, byte-identical) does not exist, and no consumer needs
one today.

## 2. Deliberate Differences from CODESYS

CODESYS is the reference implementation for the syntax surface, but its
three-tree organization is not the target of our current model:

| Aspect | CODESYS | IronPLC today (deliberate) |
|---|---|---|
| Parsers | Two independent ST parsers: green (`Parser35220`) and white (`WhiteTreeParser`), with grammar and operator precedence duplicated | One PEG grammar (`compiler/parser/src/parser.rs`), rules 1:1 Appendix B |
| Green/red split | Parser emits green `_IExprement`; `RedTreeBuilder` visits green to build the mutable typed red tree, transferring positions and duplicating types | No intermediate tree: the parser emits the typed `dsl` AST directly; types and resolution live in analyzer xform passes |
| White tree | Independent editor service with its own parser, `Leading`/`Trailing` trivia chains, `SourcePositionMap` and a formatter; not an input to the compiler | No CST; trivia is skipped by PEG `_`; positions come from spans; formatting is canonical print + AST-equality round trip |
| Consequence | White tree needed only for editor services: format preservation, round-trip, caret↔offset maps (docs 16 §8) | None of those obligations existed, so the layer was deferred (Section 5) |

Sources: [`07_AST_RED_TREE_CONSTRUCTION.md`](../../Codesys/docs/07_AST_RED_TREE_CONSTRUCTION.md)
(the three-tree model; green→red via `RedTreeBuilder`; the white tree's
editor-only role) and
[`16_WHITE_PARSE_TREES.md`](../../Codesys/docs/16_WHITE_PARSE_TREES.md)
(the white tree is a standalone editor service, shares the lexer but not the
parser, and is not required for a syntax/AST port). We inherit CODESYS's
grammar, not its tree topology: one parser parses the surface once, and any
future CST must be emitted by that parser rather than by a second one.

Scope note: [`LEXER-GAP-ANALYSIS.md`](../../Codesys/LEXER-GAP-ANALYSIS.md)
§12 keeps non-lexical gaps in a register outside the token/grammar backlog,
and §13 marks out-of-scope work "ВНЕ SCOPE (зафиксировать в README/доке)" —
durable documentation, not parked code. This document is that durable record
for the tree architecture.

## 3. Future: Three Layers, Three Invariants, Three Mechanisms

A layer is justified only when a *distinct invariant* appears that no
existing mechanism can carry. The three candidate layers each own one:

| Layer | Invariant it alone guarantees | Obligation it serves |
|---|---|---|
| White tree (CST) | Lossless text: trivia retained in the tree; `text → CST → text` byte-identical | Editing, formatting, incremental reparse |
| Green tree | Immutability without positions, so structurally shared versions stay valid | Many versions/consumers alive at once (measured scale) |
| Red view | Derived-view consistency: parent links, absolute offsets, caches and mutations always agree with the layer beneath | Consumers that navigate parents or mutate the tree |

### 3.1 White tree (CST) — the lossless layer

**Invariant.** Every byte of the source is recoverable from the tree: trivia
(whitespace, newlines, comments, pragma trivia) is attached to nodes/tokens
rather than skipped. The established shape is a non-syntactic token chain —
CODESYS `Leading`/`Trailing`
([`16_WHITE_PARSE_TREES.md`](../../Codesys/docs/16_WHITE_PARSE_TREES.md) §3.2)
— or Roslyn-style syntax trivia; either way the syntax nodes hold only
syntactic tokens plus children.

**Obligations it serves.** Format-preserving edits (change one region without
reflowing the rest); incremental reparse (reuse the unaffected regions);
format-preserving merge/undo; caret↔offset mapping read from the tree rather
than recomputed.

**Trigger.** The first real format-preserving editing obligation — the IDE
W32 era. An editor (or formatter) that must not rewrite untouched text is the
evidence; "we might have an IDE one day" is not.

**Why not now.** The current contract is canonicalization plus AST equality
(Section 1.1); no consumer reads trivia, and no requirement can validate the
invariant without the consumer that needs it.

### 3.2 Green tree — immutable, position-free structural data

**Invariant.** Immutable, position-free nodes, compared and shared by
structure, so many versions of the same program coexist cheaply. CODESYS's
green tree is the analogous raw immutable layer
([`07_AST_RED_TREE_CONSTRUCTION.md`](../../Codesys/docs/07_AST_RED_TREE_CONSTRUCTION.md)),
though it exists to feed red construction rather than for structural sharing.

**Obligations it serves.** Version-heavy consumers: incremental reparse that
keeps many revisions alive, stable node identity across edits, structural
sharing at measured scale.

**Trigger.** A measured pressure point — per-keystroke reparse/retention
showing memory or allocation cost that a CST plus a red view cannot meet, or
multiple consumers each keeping an independent parse. The trigger is numbers,
not anticipation.

**Why it is last.** A red view can be built directly over the CST. Green
earns its existence only when sharing scale demands a position-free immutable
substrate, which is why the staged order is CST → red → green (Section 5).

### 3.3 Red view — consumer view over the tree

**Invariant.** Derived-view consistency: parent links, absolute offsets,
caches and any mutation the tree exposes never disagree with the structure
beneath. Precedent: CODESYS builds the unit of statement/expression nodes on
which consumers navigate
([`07_AST_RED_TREE_CONSTRUCTION.md`](../../Codesys/docs/07_AST_RED_TREE_CONSTRUCTION.md) §1).

**Obligations it serves.** Consumer navigation (parent, enclosing node),
mutations expressed against a stable tree (editor refactors), position
queries without rescanning.

**Trigger.** The first consumer that needs parenting or mutation — the
Roslyn rationale: red exists because editors navigate upward and mutate; a
compiler that only walks downward does not need it. A workaround growing in
a consumer (hand-built parent maps, offset recomputation) is the signal.

**Why not first.** The AST already supports downward traversal with spans,
and parent links are derivable. Building red now would add a second way to
reach the same nodes with no consumer to exercise it.

**Relationship to `dsl`.** Red is a *view* — never the semantic authority.
Resolved types, names and environments stay in the AST and analyzer state.

### 3.4 N+1: when does M grow?

Today M = 1: one mechanism (the typed `dsl` AST, with trivia skipped and
spans carried) covers every obligation the compiler and the IDE have. The
three candidate layers are not refinements of that obligation — each is a
*genuinely different invariant*, which is exactly why each qualifies as a new
mechanism at all:

- a CST is required only by the lossless-text invariant, which the AST cannot
  carry without duplicating semantics;
- a red view is required only by the parenting/mutation invariant, which the
  AST deliberately does not offer;
- green sharing is required only by the structural-sharing invariant at
  scale.

The order is a staircase, not a batch: **M grows 1→2→3 with matching
obligations** — CST first, red view when a consumer exists, green only at
measured scale. If an obligation never arrives, its stage never lands and
nothing is missing; a shorter staircase is success, not debt. Building ahead
of the obligation is the anti-pattern this document exists to prevent: an
unexercised tree is a crutch, and its invariant cannot be validated without
the consumer that needs it.

## 4. Boundaries and Constraints

- **`dsl` AST remains THE semantic authority.** Analyzer, codegen, VM and
  `plc2plc` keep consuming it. Tree layers are presentation/editing only:
  text fidelity, syntactic structure, positions, mutation. No semantic
  duplication.
- **Append-only parser evolution.** When S1 lands, the parser emits the CST
  first and the AST derives from it (or the two are dual-emitted from one
  parse). Existing parses and tests stay valid; no downstream pass is
  rewritten. Failures in one view must not change the meaning of the other.
- **One grammar authority.** Never introduce a second ST parser to feed a
  second tree (Section 2).
- **Positions stay derived.** `SourceSpan` remains the canonical provenance;
  tree offsets are derived and must agree with it.
- **No layer without its Section 5 trigger**, and no port of a foreign tree
  framework (CODESYS assemblies, Roslyn) as a shortcut to having one.
- **Requirements come with the work.** Each stage, when it lands, adds
  requirement IDs and conformance tests per
  [Spec Conformance Testing](spec-conformance-testing.md); this document
  advances to `partially implemented` naming the stages that exist, and to
  `implemented` only when it describes what the compiler actually does.

## 5. Evolution Steps

Each stage lands only on its trigger, in its own change, with the exit
evidence below; a stage without evidence does not start. Until then the
architecture is the one in Section 1.

| Stage | Trigger (appears when…) | Work | Exit evidence |
|---|---|---|---|
| **S1 — white/CST** | An editor/formatter must preserve untouched text (W32-era incremental editing), or `text → tree → text` must be byte-identical | Parser emits a trivia-preserving tree from the same single grammar; AST derives from it (or dual-emit); trivia attached (Leading/Trailing shape); position map if the editor needs caret↔offset | A real-file corpus round-trips byte-identically, including unusual indentation and comments; current parser tests pass unchanged; `plc2plc` golden tests gain the text-preservation assertion while keeping AST equality; the `dsl` AST shape and all xform/analyzer/codegen tests are untouched |
| **S2 — red view** | A consumer needs parent navigation, mutation through the tree, or cached offsets (Roslyn rationale); a consumer is building parent/offset workarounds | Derived view over the CST: parent links, absolute offsets, caches, mutation facade | Parent/offset/view-consistency invariants property-tested; the consumer moves to the view and its workaround disappears; AST and consumers below it unchanged |
| **S3 — green** | Measured structural-sharing pressure (many live revisions per edit session, memory/alloc budgets) that CST + red cannot meet | Position-free immutable layer under the view, structurally shared | The measured metric (memory/allocations/reuse across versions) improves under the incremental workload; CST and view APIs unchanged for consumers |

Stopping early is allowed by design: if S2 or S3's obligation never appears,
the pipeline stays at the previous stage, and this document records why the
next stage was never needed — the path is bounded, not a backlog.

## 6. References

- [`07_AST_RED_TREE_CONSTRUCTION.md`](../../Codesys/docs/07_AST_RED_TREE_CONSTRUCTION.md)
  — CODESYS three-tree model, green→red building via `RedTreeBuilder`
- [`16_WHITE_PARSE_TREES.md`](../../Codesys/docs/16_WHITE_PARSE_TREES.md)
  — CODESYS white tree/CST: independent editor service, trivia chains,
  position map, when it is (not) needed
- [`grammar/AST_MAPPING.md`](../../Codesys/grammar/AST_MAPPING.md) — EBNF
  rule → parser → builder → AST node mapping that keeps one grammar aligned
  with the CODESYS syntax surface
- [`LEXER-GAP-ANALYSIS.md`](../../Codesys/LEXER-GAP-ANALYSIS.md) §12–§13 —
  the non-lexical gap register and the "record in durable docs" rule this
  document satisfies
- Code seams: `compiler/parser/src/lexer.rs`, `token.rs`, `parser.rs`,
  `compiler/dsl/src/`, `compiler/analyzer/src/xform_*.rs`,
  `compiler/plc2plc/src/tests/common.rs`
- [Spec Conformance Testing](spec-conformance-testing.md),
  [Online Editing UX](online-editing-ux.md),
  [W32](../implementation/dcs-platform/tasks/W32-ide.md)
