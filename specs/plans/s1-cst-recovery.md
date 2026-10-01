# S1: Lossless rowan CST with Recovery

**Issue:** [#1978](https://github.com/ironplc/ironplc/issues/1978)

Implements stage S1 ("CST and recovery") of
[Parse-Tree Architecture](../design/parse-tree-architecture.md) §5, using the
S0 decisions recorded in [Parse-Tree S0 Experiment](../design/parse-tree-s0-experiment.md)
(parser choice, dependency versions, benchmark baseline) and
[Parse-Tree S0 Audit](../design/parse-tree-s0-audit.md) (preprocessing and
provenance findings F1–F11, prefactoring candidates). S0 is closed; this plan
covers S1 only. The work spans multiple PRs and is tracked by issue #1978;
this plan is a plan PR that is never merged, per the development standards.

## Goal

The frontend gains one lossless CST built from the original decoded ST text
**before** any destructive preprocessing: a hand-written in-tree
recursive-descent parser with Pratt expressions (S0 experiment §2.5, Option B)
that produces a rowan green tree with red navigation views, parses full files,
and recovers from malformed input, with syntax diagnostics whose ranges are
complete.

S1 delivers the grammar capability and its evidence. It does **not** wire the
CST into semantic consumers: replacing the semantic parse entry with lowering
is S2 (design §5). No CLI/LSP/MCP/build path gains a second parser in S1; all
comparisons against the legacy PEG path are test-only, and the S0 spike crate
is deleted at S1 cutover (design §4, "No dual production parser").

## Architecture

- **New crate `compiler/syntax` (`ironplc-syntax`).** Owns `SyntaxKind` and the
  rowan `Language` binding, the lexer over the original text, the event/sink
  parser, red-view helpers, and the syntax error type. This is the seam change
  S0 selected; `ironplc-parser` keeps the legacy PEG path and the dsl AST until
  S2 replaces the entry. The manifest follows workspace conventions (version
  managed by the release tooling).
- **Lossless lexer.** Byte spans over the original text; trivia (whitespace,
  newlines including CRLF spelling, tabs, comments, pragmas) retained;
  unmatched bytes become error tokens so every byte is owned (audit F2).
  Tiling and text-equals-slice assertions guard the property.
- **Parser.** Recursive descent for declarations and statements plus Pratt
  expressions, ported from the spike's proven core (693 lines for a slice),
  extended to the accepted grammar of this tree; dialect gating is an input
  (`CompilerOptions`), the spike's error nodes, synchronization sets and depth
  guard (96) provide recovery, progress and termination (audit F9).
- **Recorded decision (S0 experiment §4.1):** the new frontend lexes the
  original text itself, in `compiler/syntax`, rather than splitting
  `tokenize_program` in `ironplc-parser`. The legacy transforms stay as they
  are until S2 moves their meaning behind lowering with provenance.
- **Diagnostics.** A syntax-layer error type (message + byte range) rendered
  into existing problem codes: `P0002` SyntaxError for parse errors, and the
  token-check rules (audit §3.9) move to CST-based diagnostics with unchanged
  codes (`P0004`, `P0011`, `P4033`, …). Any new code follows the 4-tuple rule
  (CSV + docs + code + test). The conversion into `dsl::Diagnostic` is settled
  together with the P-code catalog (S0 experiment §4.6).
- **Comparison path is test-only.** A parity harness runs the 48-file corpus
  under every dialect preset and asserts the same accept/reject behavior as
  `parse_program` (baseline counts in S0 experiment §3.2), plus reconstruction
  and recovery properties. It is removed at the S2 cutover when the legacy
  path is deleted — not before.

## Prefactoring

Behavior-preserving reshapes that land (each as its own PR) before the S1 core
change. They come from the S0 audit §7 and experiment §4; each makes the CST
change drop in instead of accreting branches in the legacy pipeline.

1. **Coordinate semantics made explicit.** `SourceSpan`'s doc says "character"
   positions (`dsl/src/core.rs:97-105`) while every consumer indexes bytes, and
   columns are UTF-16 code units (`lexer.rs:60-70`). Fix the wording and pin it
   with tests before the CST adds another consumer of the same coordinates
   (audit F10, §7.1).
2. **Pipeline order and dialect gating pinned as documented semantics.**
   Findings F6/F7 show the transform order is observable (terminators inserted
   before keyword demotion; `TIME` demotion depends on neighbors). Add tests
   that pin today's order and behavior, then document the order in
   `lib.rs:69-76`; no behavior change (experiment §4.5).
3. **Zero-width synthetic terminators.** Replace the borrowed-span
   `semicolon_like` tokens (`xform_tokens.rs:6-14`, TODO `144-146`) with
   zero-width markers anchored between real tokens, including the CASE-branch
   fixups that currently emit tokens out of span order (audit F3). This lets
   the legacy stream satisfy the coverage invariant while the PEG parser still
   runs; AST and diagnostic behavior unchanged, tests updated where they
   asserted the borrowed span.
4. **Coverage guard for the legacy pipeline.** Port the spike's
   tiling/text-slice helper into `ironplc-parser` tests over the corpus and the
   OSCAT/Unicode/pragma snippets, with the known F1 (`oscat.st`) and F3
   exceptions pinned explicitly, so any new provenance regression fails loudly
   while the legacy pipeline still runs (experiment §4.3).
5. **Parse-only benchmark.** Fold the spike harness into
   `compiler/benchmarks`: per-file cold/warm tokenize and full-parse timings
   plus allocation counts over the corpus, so S1 has a production baseline and
   the spike bin can be deleted (experiment §4.7).
6. **One-time init cost (optional).** ~990 allocations currently attach to the
   first parse in a process (experiment §4.2); remove or make explicit if the
   new benchmark shows it inside a measured call.

Deliberately not prefactored in S1: the OSCAT blanking pass and the other
destructive transforms stay untouched, because the CST path does not run them.
Representing that meaning through lowering with provenance is S2 (design §3.1,
§5 S2 row).

## Design doc reference

- [Parse-Tree Architecture](../design/parse-tree-architecture.md) §3.1
  (lossless CST), §4 (evidence-driven parser choice, no dual production
  parser, behavior-preserving migration), §5 S1 row (exit evidence).
- [Parse-Tree S0 Audit](../design/parse-tree-s0-audit.md) §3-§4 (findings),
  §7 (prefactoring candidates).
- [Parse-Tree S0 Experiment](../design/parse-tree-s0-experiment.md) §1 (spike
  evidence), §2.5 (parser recommendation), §3 (baseline numbers).

## File map

New:

- `compiler/syntax/` — `Cargo.toml`; `src/lib.rs`; lexer over the original text
  with error tokens; `SyntaxKind` and the rowan `Language`; parser modules
  (declarations, statements, Pratt expressions, recovery); CST sink and red
  views; syntax error type. Tests for losslessness, recovery, dialect gating
  and prefix termination; fixtures under `tests/fixtures/standard|codesys|malformed/`.
- Corpus additions (under the crate's fixtures): the five CODESYS fixtures
  promoted from `compiler/s0-spike/fixtures/codesys/`, expanded per the
  [syntax-gap analysis](../../Codesys/LEXER-GAP-ANALYSIS.md) §14.4 proposal;
  renewed CRLF/tabs/Unicode/comment/pragma cases; a malformed corpus (audit §6
  records the current one as thin).

Modified:

- `compiler/Cargo.toml` — add the `syntax` workspace member;
  `compiler/Cargo.lock` — rowan 0.16.1 (already locked by the spike).
- `compiler/benchmarks/` — parse-only benchmark (prefactor 5).
- `compiler/parser/src/xform_tokens.rs`, `compiler/parser/src/lib.rs`,
  `compiler/parser/src/tests/` — prefactors 2-4.
- `compiler/dsl/src/core.rs`, `compiler/parser/src/lexer.rs` — prefactor 1
  (doc wording and tests).

Deleted:

- `compiler/s0-spike/` — after the parity harness and benchmark fold cover its
  role; remove the workspace member and `Cargo.lock` entries.

## Tasks

Plan (this PR, never merged):

- [ ] Issue #1978 and this plan reviewed and approved.

Prefactor PRs — each behavior-preserving, branched from `main`:

- [ ] Fix `SourceSpan` byte-vs-character wording, state UTF-16 columns, add
      coordinate tests.
- [ ] Pin transform order and dialect gating with tests; document the order in
      `lib.rs:69-76`.
- [ ] Zero-width synthetic terminators; test the CASE fixup ordering.
- [ ] Coverage guard (tiling/text-slice) in `ironplc-parser` tests with the
      known exceptions pinned.
- [ ] Parse-only benchmark in `compiler/benchmarks`; record the S1 baseline.
- [ ] (If measured) remove or make explicit the one-time init allocation cost.

Core change PRs:

- [ ] `compiler/syntax` crate skeleton: `SyntaxKind`, rowan binding, lossless
      lexer with error tokens, coverage assertions, red views.
- [ ] CST sink from the event stream; trivia re-inserted in source position;
      `tree.text() == source` tests.
- [ ] Declarations and statements parser for the accepted grammar, with dialect
      gating through `CompilerOptions`.
- [ ] Pratt expression parser; node-shape tests for precedence and nesting.
- [ ] Error nodes, synchronization sets, depth guard; syntax diagnostics
      rendered into problem codes; token-check rules on the CST with unchanged
      codes.
- [ ] Parity harness: corpus × dialect accept/reject against `parse_program`;
      reconstruction over valid, malformed and prefix inputs; termination
      bounds.
- [ ] Promote CODESYS/malformed/CRLF/Unicode/pragma fixtures and carry the S0
      benchmark numbers as the comparison baseline.
- [ ] Delete `compiler/s0-spike/`; remove the workspace member; re-run
      benchmarks.
- [ ] `cd compiler && just` green on every PR.

## Exit evidence (design §5, S1 row)

- [ ] Byte-identical valid/malformed reconstruction (`tree.text() == source`).
- [ ] CRLF/LF spelling, tabs, Unicode, comments and pragmas retained.
- [ ] Complete ranges — every byte owned by a token or error region.
- [ ] Recovery progress and termination, including partial/truncated input.
- [ ] Existing dialect tests preserved (accepted syntax and gating unchanged).

## Non-goals

- No CST-to-dsl lowering and no change to the semantic parse entry (S2).
- No Salsa queries or tracked analysis (S3); salsa 0.28.5 stays locked by the
  spike until then.
- No snapshot API or consumer wiring (S4); no LSP/CLI/MCP integration.
- No local subtree reparsing (S5); full-file parsing is the S1 correctness
  baseline.
- No change to accepted syntax, dialect semantics, diagnostics for valid
  programs, or backend contracts.
