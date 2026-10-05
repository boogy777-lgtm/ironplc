# Syntax Support Guide

This guide describes everything needed to add support for new syntax in the IronPLC compiler. Follow this guide when adding new language features, extensions, or fixing syntax-related issues.

> **Note**: This covers the full pipeline from lexer through execution. For general compiler architecture, see [compiler-architecture.md](compiler-architecture.md). For IEC 61131-3 compliance rules, see [iec-61131-3-compliance.md](iec-61131-3-compliance.md).

## Quick Checklist

When adding new syntax, ensure every applicable item is complete:

- [ ] **Token / keyword**: Add the spelling to `SyntaxKind` in `syntax/src/syntax_kind.rs` and the token type it becomes in `parser/src/token.rs` and `parser/src/tokens.rs` (if new keywords/operators)
- [ ] **Dialect gating**: Add the flag to `ParseOptions` (`syntax/src/parser/options.rs`) and a `keyword_enabled` row or a `GATES` row (if conditionally enabled)
- [ ] **Grammar**: Add the grammar function in `syntax/src/parser/grammar/` and the node kind (with its `disposition`) in `syntax/src/syntax_kind.rs` and `syntax/src/lower/mod.rs`
- [ ] **Lowering**: Add the lowering rule in `syntax/src/lower/` (the area that owns the node kind)
- [ ] **AST**: Add/modify nodes in the `dsl` crate
- [ ] **Analyzer**: Add semantic validation in `analyzer/`
- [ ] **Codegen**: Add bytecode emission in `codegen/`
- [ ] **plc2plc renderer**: Update `plc2plc/src/renderer.rs` to render the new syntax
- [ ] **plc2plc round-trip test**: Parse → render → **re-parse** (in a focused file under `plc2plc/src/tests/` — see [Test File Organization](#test-file-organization-avoid-merge-conflicts) and [plc2plc round-trip tests](#plc2plc-round-trip-tests-always-re-parse))
- [ ] **End-to-end execution test**: Parse → compile → run → verify variable values
- [ ] **Whitespace invariance**: Every gap a new grammar function permits earns a row in `parser/src/tests/whitespace.rs`, so a function that later stops skipping trivia there fails (see [Which leg asserts what](#which-leg-asserts-what-avoid-duplicate-tests))
- [ ] **Expected-AST test**: A focused file under `parser/src/tests/` that parses through `parse_program` and asserts the AST shape, and a row in the parity exception tables (`parser/src/tests/parity/`) with its reason if the legacy oracle rejects or reads the form differently (see [Parity against the legacy oracle](#parity-against-the-legacy-oracle))
- [ ] **Non-standard gating**: If not standard IEC 61131-3, gate behind `--allow-x` flag
- [ ] **LSP integration**: If a new `--allow-x` flag, add to LSP `extract_compiler_options`
- [ ] **Documentation**: If a new `--allow-x` flag, update `docs/explanation/enabling-dialects-and-features.rst`, `docs/reference/compiler/ironplcc.rst`, and the flag table in this file

Not every syntax change requires all items. A new operator might not need new tokens. A token-level fix might not need codegen changes. Use judgment, but **always** include both round-trip and execution tests when the syntax produces executable code.

Each test leg must assert something the others do not — a parser test that only
checks "it parses" is subsumed by the round-trip test on the same snippet. See
[Which leg asserts what](#which-leg-asserts-what-avoid-duplicate-tests).

## plc2plc round-trip tests: always re-parse

Rendering is only half the job — what the renderer emits has to be text the
parser accepts. A test that only compares the rendering against a golden
`*_rendered.st` file cannot tell correct output from output the parser
rejects: it records the broken spelling as "expected".

So every renderer test **re-parses what it rendered**. A plc2plc test is one
of two shapes, both provided by `plc2plc/src/tests/common.rs`:

1. **Round trip** — `assert_round_trips(source, &options)`: parse → render →
   re-parse, requiring the same AST. This is the default; reach for it first.
2. **Round trip pinned to a golden file** — `assert_resource_renders_to(
   source_name, rendered_name, &options)`: the same round trip, plus an
   equality check of the rendered text against the committed
   `*_rendered.st`. Use it when the exact layout is worth freezing.

Both return the rendered text. Add `assert!(rendered.contains(...))` on top
only for what AST equality cannot see — identifier casing (`Id` compares
case-insensitively) or a spelling the AST does not record, such as
`STRING [ 255 ]` where the DSL keeps no bracket/paren marker. A `contains`
that merely restates something the re-parse already proves is redundant;
drop it. Never assert only `contains`.

When the rendering deliberately normalizes to a *different* AST spelling — a
bit-string literal that decimalizes, a mixed `VAR` block that renders one
block per declaration — use `assert_round_trips_idempotently` and say why at
the call site. It still re-parses; it asserts a second render reproduces
identical text instead of AST equality.

A rendering is re-parsed under the **same** options as its source. A
rendering that needs a laxer dialect than its source did is a renderer bug.
Where a normalization makes the output *stricter*-grammar-valid (`STRING(255)`
→ `STRING [ 255 ]`, `.%X0` → `.0`), add an explicit second parse under
`CompilerOptions::default()` to pin that.

## Test File Organization (avoid merge conflicts)

Tests are split into **small, feature-focused files** so that two feature
branches almost never edit the same lines. Adding tests for a feature area
should mean **a new file plus one `mod` line**, never appending to a large
shared file.

- **Parser** — `parser/src/tests/`: one file per feature area
  (`case.rs`, `enums.rs`, `reference_to.rs`, `duration.rs`, …). Shared
  imports and helpers live in `tests/common.rs`; each file starts with
  `use super::common::*;`. Register new files with a `mod` line in
  `tests/mod.rs` (kept sorted).
- **plc2plc** — `plc2plc/src/tests/`: same structure.
- **codegen** — `codegen/tests/it/`: one file per feature
  (`end_to_end_case.rs`, `compile_add.rs`, …), registered with a `mod` line
  in `tests/it/main.rs`.

Prefer a **new** file for a new sub-feature over appending to an existing
one: appending puts every branch's new tests on the same trailing lines,
which is exactly what causes the recurring conflicts. Only add to an
existing file when the new tests genuinely extend that same narrow feature.

### Which leg asserts what (avoid duplicate tests)

A feature gets tests in several crates, and each leg must assert something
the others do not. A round-trip test already parses the source with the same
options, so a parser test that only asserts `is_ok()` on that same snippet
adds no signal — it is subsumed. Keep the legs distinct:

| Leg | Asserts | Do **not** write |
|---|---|---|
| **Parser** (`parser/src/tests/`) | The AST *shape*: the node variant, its fields, counts, nesting | A bare "it parses" on a snippet a plc2plc round-trip already covers |
| **plc2plc** (`plc2plc/src/tests/`) | Text → AST → text → AST fidelity: the rendering re-parses to the same AST, optionally pinned against a golden file | A render assertion that never re-parses the rendering |
| **Analyzer** (`analyzer/src/rule_*.rs`) | The semantic outcome (accepted, or a specific problem code) | Anything about parse success or failure |
| **codegen `compile_*`** | The emitted instruction sequence / container structure | Run results |
| **codegen `end_to_end_*`** | Run results — the nominal behavior matrix reachable from ST | — |
| **VM** (`vm/tests/it/`) | Traps, overflow/wrap edges, and states codegen cannot emit | A nominal case its codegen twin already runs |
| **Whitespace** (`parser/src/tests/whitespace.rs`) | That the gaps the grammar permits stay permitted, and that adjacencies inside one lexical unit stay rejected | A row duplicating a rejection another file already owns — `REF=` belongs to `parser/src/tests/reference_to.rs` |

Parser tests asserting only `is_ok()` are still right when there is **no**
round-trip counterpart — a dialect-flag rejection, a pragma, or a corpus file
plc2plc does not render. The rule is about the same snippet being asserted
twice at the same strength, not about `is_ok()` itself.

When a VM test and a codegen end-to-end test would cover the same behavior,
the codegen test owns it and the VM file says so in its module header (see
`vm/tests/it/execute_fb_ton.rs` or `execute_string_ops.rs` for the wording).

## The Front End: Tokens, Gating, Grammar, Lowering

The compiler reads text through one front end, the lossless tree of
`ironplc-syntax`. `ironplc-parser` is the facade: `tokenize_program`,
`parse_program` and `parse_st_statements` call `compiler/parser/src/frontend.rs`,
which parses with `ironplc_syntax::parse_source_file` / `parse_statements` and
lowers the tree with `ironplc_syntax::lower`. The tree keeps every byte of the
text and the dialect decisions are made by the parser over the untouched tokens;
nothing rewrites a token stream. The legacy pipeline (the logos lexer, the
`xform_*` token transforms, the `rule_token_*` checks and the PEG grammar in
`parser/src/parser.rs`) is compiled for tests only, as the oracle of the parity
tests; it is not the way to add syntax. For the design, see
[parse-tree-architecture.md](../design/parse-tree-architecture.md).

| Step | Where | What it decides |
|------|-------|-----------------|
| Token or keyword | `syntax/src/syntax_kind.rs` (`syntax_kinds!`) | The spelling, in the `keywords` list: the lexer, `SyntaxKind::KEYWORDS` and `ALL` are generated from it |
| Token view | `parser/src/tokens.rs` (`rows!`), `parser/src/token.rs` | The `TokenType` the token becomes for `tokenize_program`; the match over `TokenKind` has no wildcard arm, so a new kind does not compile until it has a row |
| Dialect flag | `syntax/src/parser/options.rs` (`parse_options!`) | The `allow_*` flag the parser reads; it mirrors the flag of the same name in `CompilerOptions` (`parser/src/options.rs`) |
| Keyword gate | `ParseOptions::keyword_enabled` | Which flag makes a keyword a keyword; a keyword the dialect leaves off is an ordinary name |
| Lexical-form gate | `syntax/src/parser/gates.rs` (`GATES`), `syntax/src/ranking.rs` (`STAGES`) | A form the lexer always reads but a dialect rejects, with the error kind, its message and the stage that ranks it |
| Grammar | `syntax/src/parser/grammar/` | The recursive-descent function for the construct; `positions.rs` is the table of where a type or a value may stand |
| Node kind and disposition | `syntax/src/syntax_kind.rs` (`nodes`), `syntax/src/lower/mod.rs` (`node_disposition`) | Whether lowering turns the node into an object (and in which area), reads it as part of its parent, or ignores it; the match has no wildcard arm |
| Lowering rule | `syntax/src/lower/<area>/` | The `dsl` object the node becomes, with the spans of the source text, and the lowering problems |

### Adding New Tokens

Add a keyword to the `keywords` list in `syntax_kinds!` (`syntax/src/syntax_kind.rs`),
spelled in upper case; the lexer matches every keyword case-insensitively and the
tables generated from the list stay in step:

```rust
keywords {
    // ...
    Limit = ["LIMIT"],
}
```

Then give the kind a row in `rows!` (`parser/src/tokens.rs`), which is the token
view the language server and `ironplcc tokenize` read. A keyword is listed in the
`keywords` arm, which maps `TokenKind::Limit` to `TokenType::Limit` and so needs a
variant of that name in `TokenType` (`parser/src/token.rs`), and the places that
match `TokenType` exhaustively (for example `ironplc-cli/src/semantic_tokens.rs`)
need an arm. Operators and punctuation are tokens of `syntax_kinds!` and a row in
the `plain` arm.

### Keyword Gating Pattern

**When to use**: When a keyword is only valid under certain conditions (e.g.,
Edition 3 mode, or an extension flag) and programs may use that keyword as an
identifier otherwise.

**How it works**: The lexer always produces the keyword token. The parser asks
`ParseOptions::keyword_active(kind, text)` before it treats the token as a
keyword, and a keyword that is not active is an ordinary name everywhere
(including in the token view of `tokenize_program`). One table decides which
keyword is gated by which flag: `ParseOptions::keyword_enabled`.

**Reference implementation**: `ParseOptions::keyword_enabled` in
`syntax/src/parser/options.rs`

To gate a keyword, add the flag to `parse_options!` and a row to the match:

```rust
parse_options! {
    // ...
    /// `LIMIT` is a keyword.
    allow_repeat_limit,
}

// in keyword_enabled:
K::Limit => self.allow_repeat_limit,
```

**Key points**:
- Nothing rewrites tokens: the decision is made where the parser reads the token
- The match ends in a wildcard arm (`_ => true`), so a gated keyword that has no
  row is silently always on: add the row in the same change that adds the flag
- A **context-sensitive** decision (one that depends on the neighbouring tokens,
  like `TIME` as a function name) is a method of `ParseOptions` next to
  `time_is_name`, read by both the parser and the token view

### Lexical-Form Gate Pattern

**When to use**: When the syntax is always recognised by the lexer but rejected
unless a flag is set, because it cannot be confused with an identifier (e.g.,
`//` comments, `{ }` pragmas).

**How it works**: Each gated form is one row of `GATES` in
`syntax/src/parser/gates.rs`: the token kinds it can start at, a function that
finds its ranges at a site, the flag that enables it, the `ErrorKind` (which
decides the problem code) and the message. The error is reported with the other
errors of the parse, and `STAGES` in `syntax/src/ranking.rs` says which stage of
checking the kind belongs to, which decides the one diagnostic a consumer reports.

**Reference implementation**: the `GATES` row for C-style comments in
`syntax/src/parser/gates.rs`

**Key points**:
- A new `ErrorKind` is mapped to its `Problem` in `syntax/src/diagnostic.rs` and
  to exactly one `Stage` in `syntax/src/ranking.rs`; both are exhaustive
- Problem codes come from the shared `ironplc_problems` crate and need the four
  parts of the problem-code rule (CSV row, `docs/reference/compiler/problems/P####.rst`,
  emitter, test)
- A `GATES` row names its trigger kinds; a guard test over the corpus fails a
  trigger set that misses a place where the form occurs
- Always include tests for both allowed and disallowed cases

### Choosing Between Keyword Gating and a Lexical-Form Gate

| Scenario | Use |
|----------|-----|
| New keyword that could conflict with existing identifiers | Keyword gating (`keyword_enabled`) |
| Syntax that is always distinct from standard syntax | Lexical-form gate (`GATES`) |
| Feature controlled by `--dialect` (edition selection) | Keyword gating |
| Feature controlled by `--allow-x` flag | Either, depending on conflict risk |

### Grammar, Node Kinds and Lowering

The grammar is a set of functions over a `Parser` that records events
(`syntax/src/parser/grammar/`); a construct is one function that opens a node of
its kind, reads its tokens (skipping trivia) and closes the node, and the
functions it calls are the places the construct nests. Recovery follows the
tokens that can follow a construct (`syntax/src/parser/recovery.rs`), and the
depth of the tree is bounded at the place a node is opened.

A new construct adds:

1. a node kind to `nodes` in `syntax_kinds!`;
2. a `node_disposition` arm in `syntax/src/lower/mod.rs` (it does not compile
   without one): `Lowered(area)` when a rule turns the node into an object,
   `Structural` when its parent's rule reads it;
3. for a lowered node, one row in the dispatch of its area (for a statement,
   the table in `syntax/src/lower/statements/mod.rs`), whose rule builds the
   `dsl` object from the node and its tokens, takes spans from the source text
   through `LowerCx` and reports a lowering problem as a `Diagnostic`.

Lowering runs only on a tree whose parse reported no error, so a node the
grammar guarantees but the tree lacks is an internal error, not a diagnostic.

### Parity against the legacy oracle

The legacy pipeline stays, compiled for tests only, so that the tree is held to
the language it replaced. The tests in `parser/src/tests/parity/` compare the
objects the lowering builds, the tokens of the token view and the diagnostics
with those of the legacy parser over the corpus and the declaration tables,
under every dialect preset. A difference must be a row of a table with its
reason (a defect of the legacy parser, an owner decision, or a form accepted on
purpose); an unlisted difference fails, and so does a row that no longer
explains one. New syntax that only the tree reads is a row of the verdict
tables (accepted on purpose), and is asserted by an expected-AST test in
`parser/src/tests/`.

## Non-Standard Syntax Gating (`--allow-x` Flags)

> **Not every vendor difference is syntax.** If the difference is in what a
> standard operation *does* at runtime (what `STRING_TO_INT('12abc')`
> returns), it is a [behavior policy](glossary.md#behavior-policy), not an
> extension: it gets a `--policy-*` selection encoded in the bytecode, never
> an `--allow-*` flag. See
> [behavior-policies.md](../design/behavior-policies.md) for how to add one.

**Rule**: Anything not in IEC 61131-3 Edition 2 **must** be gated behind an `--allow-x` flag. That includes Edition 3 syntax, which is gated the same way.

### Before Creating a New Flag

**Always check existing flags first**. Group related extensions under one flag when they represent the same vendor behavior.

The full list of existing flags is intentionally **not** duplicated here — a
mirrored table drifts out of date. Consult these instead:

- **Authoritative (code):** the `define_compiler_options!` macro invocation in
  `compiler/parser/src/options.rs`, exposed at runtime via
  `CompilerOptions::FEATURE_DESCRIPTORS` and the `ironplcc dialects` command.
- **Reader-friendly (docs):**
  [`docs/reference/compiler/ironplcc.rst`](../../docs/reference/compiler/ironplcc.rst)
  documents every `--allow-*` flag with a description, and
  [`docs/explanation/enabling-dialects-and-features.rst`](../../docs/explanation/enabling-dialects-and-features.rst)
  explains which dialects enable each flag.

Scan those before adding a flag to confirm an existing one doesn't already cover
your syntax.

### Dialects

Dialects (`--dialect`) set the base configuration. Individual `--allow-*` flags can override on top.

The dialects are `iec61131-3-ed2` (the default), `iec61131-3-ed3`, `rusty`,
`codesys` and `twincat`.

Which flags each one enables is **not** listed here, for the same reason the
flags themselves are not: a mirrored table drifts. Run `ironplcc dialects`, or
read the per-dialect `**Enables:**` lists in
[`docs/explanation/enabling-dialects-and-features.rst`](../../docs/explanation/enabling-dialects-and-features.rst),
which a build-time check keeps in step with `options.rs`.

### Grouping Guidance

- If the extension is a syntactic variation of something an existing flag covers, add it to that flag
- If the extension is common across multiple vendors and represents the same concept, group under one flag
- If the extension is unique to a specific vendor behavior, create a new flag
- Keep flag names descriptive: `allow_<what_it_allows>`

#### Naming: be specific, avoid umbrella terms

Flag names must describe the **specific syntax** they enable, not a broad
category. Vague umbrella words — `extensions`, `features`, `oop`, `advanced`,
`extra`, `misc` — are ambiguous the moment a second, unrelated extension in the
same category is added: `allow_oop_extensions` gives no hint whether it covers
`EXTENDS`, `METHOD`/`PROPERTY`, `THIS^`/`SUPER^`, or interface dispatch, and a
future OOP flag would have no room left to name itself distinctly.

Prefer names that spell out the construct(s) gated:

| Avoid (ambiguous) | Prefer (specific) |
|-------------------|-------------------|
| `allow_oop_extensions` | `allow_fb_inheritance` (`EXTENDS`/`IMPLEMENTS`/`INTERFACE`/`ABSTRACT` shape) |
| `allow_pointer_features` | `allow_ref_to`, `allow_ref_arithmetic`, … (one flag per construct) |
| `allow_string_extras` | `allow_paren_string_length` |

When one flag genuinely gates several related constructs, name it after the
concept they share (e.g. the `REF_TO` family), not after the vendor or the word
"extension". If you cannot name the flag without a generic umbrella term, that
usually means it should be split into more than one flag.

The same applies to the flag's **description** and doc comments: describe the
*syntax* it gates, not the vendor it came from. A construct is rarely exclusive
to one tool — the same OOP or reference syntax often appears across several —
so "OOP function-block declaration syntax" ages better than "TwinCAT/CODESYS OOP
extensions". Let the [Dialects](#dialects) table express which dialects turn the
flag on; that is where the vendor mapping belongs.

### Adding a New Flag

When no existing flag covers the extension, add a new one. Update these files in order:

#### 1. `CompilerOptions` (`parser/src/options.rs`) and `ParseOptions` (`syntax/src/parser/options.rs`)

Add the flag to the `define_compiler_options!` invocation, and, when the grammar
or the lexical gates read it, a flag of the same name to `parse_options!`:

```rust
parse_options! {
    // ...
    /// Describe the syntax the flag gates.
    allow_my_extension,
}
```

`frontend::parse_options` converts the compiler options to the tree's options flag
by flag, by name; a test fails when the tree reads a flag the compiler options do
not have, so a renamed or missing flag is not silently off.

#### 2. CLI `FileArgs` (`plc2x/bin/main.rs`)

Add the clap argument:

```rust
/// Allow [description of what this enables].
/// This is an extension not part of the IEC 61131-3 standard.
#[arg(long)]
allow_my_extension: bool,
```

The `compiler_options()` method uses `|=` to overlay flags on the dialect preset:

```rust
fn compiler_options(&self) -> CompilerOptions {
    let mut options = CompilerOptions::from_dialect(self.dialect.to_dialect());
    // ... existing overlays ...
    options.allow_my_extension |= self.allow_my_extension;
    options
}
```

**Also add the flag to the relevant dialects** by listing them in the flag's own entry in `define_compiler_options!` (in `parser/src/options.rs`), e.g. `[Codesys, TwinCat]`. `from_dialect()` is generated from those tags — there are no per-dialect arms to edit.

#### 3. LSP extraction (`plc2x/src/lsp.rs`)

Add to `extract_compiler_options()` using the `|=` pattern:

```rust
options.allow_my_extension |= flag("allowMyExtension");  // camelCase for LSP
```

Add a test for the LSP extraction.

#### 4. Playground defaults (`playground/src/lib.rs`)

If the extension should be enabled by default in the playground, set it there.

#### 5. Implement the gating

Use either the keyword gating pattern, the lexical-form gate pattern, or an analyzer-level check (see sections above). Always test both the allowed and disallowed cases.

#### 6. Documentation

Update these files to document the new flag:
- `docs/explanation/enabling-dialects-and-features.rst` — add to the Language Extensions section
- `docs/reference/compiler/ironplcc.rst` — add to the Options section
- Update the flag table in this file (syntax-support-guide.md)

## plc2plc Round-Trip Testing

**Requirement**: Every new syntax feature must have a plc2plc test that proves the compiler can parse the syntax and render it back out correctly.

### How Round-Trip Tests Work

The test pattern parses an `.st` source file, renders it back to text via the `plc2plc` renderer, and compares the output against an expected file.

### File Locations

| What | Where |
|------|-------|
| Shared input `.st` files | `compiler/resources/test/` |
| Expected rendered output | `compiler/plc2plc/resources/test/` |
| Test code | `compiler/plc2plc/src/tests/` (one focused file per feature; helpers in `tests/common.rs`) |
| Renderer implementation | `compiler/plc2plc/src/renderer.rs` |

### Test Pattern

From `plc2plc/src/tests/common.rs`:

```rust
fn parse_and_render_resource(name: &'static str) -> String {
    let source = read_shared_resource(name);
    let library = parse_program(&source, &FileId::default(), &CompilerOptions::default()).unwrap();
    write_to_string(&library).unwrap()
}

#[test]
fn write_to_string_my_feature() {
    let rendered = parse_and_render_resource("my_feature.st");
    let expected = read_resource("my_feature_rendered.st");
    assert_eq!(rendered, expected);
}
```

### Steps to Add a Round-Trip Test

1. **Create the input file**: Add `compiler/resources/test/my_feature.st` with valid IEC 61131-3 source that uses the new syntax
2. **Create the expected output file**: Add `compiler/plc2plc/resources/test/my_feature_rendered.st` with the expected rendered output
3. **Add the test**: Add a test function to the matching focused file under `plc2plc/src/tests/` (or create a new file plus a `mod` line in `tests/mod.rs`), following the pattern above
4. **Update the renderer**: If the new syntax requires new AST nodes, update `plc2plc/src/renderer.rs` to render them

For non-standard syntax that requires a parse option:

```rust
fn parse_and_render_with_options(name: &'static str, options: CompilerOptions) -> String {
    let source = read_shared_resource(name);
    let library = parse_program(&source, &FileId::default(), &options).unwrap();
    write_to_string(&library).unwrap()
}

#[test]
fn write_to_string_my_dialect_extension() {
    let options = CompilerOptions {
        allow_my_extension: true,
        ..CompilerOptions::default()
    };
    let rendered = parse_and_render_with_options("my_extension.st", options);
    let expected = read_resource("my_extension_rendered.st");
    assert_eq!(rendered, expected);
}
```

### What Round-Trip Tests Validate

- The parser correctly understands the syntax structure
- The AST captures all relevant information
- The renderer can reproduce the syntax from the AST
- No information is lost in the parse → AST → render pipeline

## End-to-End Execution Testing

**Requirement**: Every syntax feature that produces executable code must have an end-to-end test that compiles and runs the code, then verifies the results.

### How End-to-End Tests Work

Tests use inline IEC 61131-3 source, run the full pipeline (parse → analyze → compile → VM execute), and inspect the resulting variable buffers.

### File Locations

| What | Where |
|------|-------|
| Test helpers | `compiler/codegen/tests/common/mod.rs` |
| End-to-end tests | `compiler/codegen/tests/end_to_end_*.rs` |

### Test Helpers

From `codegen/tests/common/mod.rs`:

| Helper | Purpose |
|--------|---------|
| `parse_and_run(source)` | Full pipeline, one scan cycle, returns `(Container, VmBuffers)` |
| `parse_and_run_edition3(source)` | Same but with Edition 3 features enabled |
| `parse_and_compile(source)` | Parse + compile without running (for bytecode inspection) |
| `parse_and_try_run(source)` | Returns `Result` so you can test runtime traps |
| `parse_and_run_rounds(source, closure)` | Multi-round execution for stateful tests |

### Test Pattern

From `codegen/tests/end_to_end_if.rs`:

```rust
//! End-to-end integration tests for IF/ELSIF/ELSE statements.

mod common;
use common::parse_and_run;

#[test]
fn end_to_end_when_if_true_then_executes_body() {
    let source = "
PROGRAM main
  VAR
    x : DINT;
    y : DINT;
  END_VAR
  x := 5;
  IF x > 0 THEN
    y := 1;
  END_IF;
END_PROGRAM
";
    let (_c, bufs) = parse_and_run(source);

    assert_eq!(bufs.vars[0].as_i32(), 5);
    assert_eq!(bufs.vars[1].as_i32(), 1);
}
```

### Steps to Add an Execution Test

1. **Create or extend a test file**: Add `compiler/codegen/tests/end_to_end_my_feature.rs` or add tests to an existing file if the feature is closely related
2. **Write inline source**: Use valid IEC 61131-3 source that exercises the new syntax
3. **Run and inspect**: Use `parse_and_run()` and check `bufs.vars[N].as_i32()` (or appropriate type method)
4. **Test both success and edge cases**: Include tests for the happy path and boundary conditions

### Variable Buffer Inspection

Variables appear in `bufs.vars` in declaration order (0-indexed). Use the appropriate type accessor:

- `bufs.vars[N].as_i32()` — for DINT, INT, SINT, etc.
- `bufs.vars[N].as_f32()` — for REAL
- `bufs.vars[N].as_f64()` — for LREAL
- `bufs.vars[N].as_bool()` — for BOOL

### Testing Non-Standard Syntax Execution

If the syntax is behind an `--allow-x` flag, you may need to add a helper in `codegen/tests/common/mod.rs` that enables the flag:

```rust
pub fn parse_with_extension(source: &str) -> (Library, SemanticContext) {
    let options = CompilerOptions {
        allow_my_extension: true,
        ..CompilerOptions::default()
    };
    let library = parse_program(source, &FileId::default(), &options).unwrap();
    let (analyzed, ctx) = ironplc_analyzer::stages::resolve_types(&[&library]).unwrap();
    (analyzed, ctx)
}
```

### What Execution Tests Validate

- The full compiler pipeline works end-to-end for the syntax
- Generated bytecode is correct
- The VM executes the bytecode and produces expected results
- Runtime behavior matches IEC 61131-3 semantics

## Pipeline Integration Points

`parse_program` (`parser/src/lib.rs`) calls `frontend::parse_program`, which runs
on the stack budget (`ironplc_dsl::stack`) these steps in order:

1. **`parse_source_file()`** � the lexer reads the text byte-exactly, the region
   pass sets aside the stretches the grammar must not read (OSCAT ranged
   comments, untaken `{IF}` branches), the gates report the forms the dialect
   rejects, and the recursive-descent parser builds the lossless tree
2. **`lower_library()`** � lowering turns the tree of a file with no error into
   the `Library`; when the tree has errors the one the ranking chooses
   (`syntax/src/ranking.rs`) is the diagnostic

`tokenize_program` is the token view of the same lexer and gates
(`parser/src/tokens.rs`) and does not parse. `parse_st_statements` parses a
statement fragment with `parse_statements` and `lower_statements`.

New syntax goes in the grammar, the gates or the lowering, not in a pass over
tokens: a keyword is gated by a row of `keyword_enabled`, a lexical form by a row
of `GATES`, and a construct by a grammar function with its lowering rule.

## Common Mistakes

- **Forgetting dialect presets**: Every new `--allow-x` flag must list the dialects that enable it in its `define_compiler_options!` entry. Without this, every dialect silently ignores the new feature.
- **Missing LSP wiring**: The flag works on the CLI but not in VS Code because `extract_compiler_options()` in `plc2x/src/lsp.rs` was not updated. Always add LSP extraction for new flags.
- **No round-trip test**: The feature parses but the renderer in `plc2plc` cannot write it back. Always add the round-trip test.
- **No execution test**: The feature parses and analyzes but was never proven to execute correctly. Always add at least one end-to-end test.
- **Creating a flag for standard syntax**: Only extensions get `--allow-x` flags. Standard IEC 61131-3 syntax is always on (or gated by `--dialect`).
- **Context in the lexer**: The lexer reads every keyword the same way in every dialect. Decide context-dependent behavior in the parser (`ParseOptions`), not in lexer rules.
- **Editing the legacy pipeline**: `xform_*`, `rule_token_*` and the PEG grammar are compiled for tests only. A change there changes the oracle, not the compiler.
- **A gated keyword without a row**: `keyword_enabled` ends in a wildcard arm, so a keyword with a flag but no row is always on.
- **A placeholder decision**: A new node kind does not compile until `node_disposition` has an arm for it, and a new token kind until `tokens.rs` has a row; give them real decisions, not a placeholder.

## Step-by-Step Walkthrough

This walkthrough shows the typical sequence for adding a new syntax feature.

### Example: Adding Support for a Hypothetical Language Extension

Suppose a vendor allows `REPEAT ... UNTIL ... END_REPEAT` with an optional `LIMIT` clause (non-standard).

#### Step 1: Check Existing Flags

Review `parser/src/options.rs` — does an existing flag cover this? If not, proceed with a new flag.

#### Step 2: Add the Flag

1. Add `allow_repeat_limit` to `CompilerOptions` dialect fields in the `define_compiler_options!` macro, tagged with the dialects that enable it
2. Add `allow_repeat_limit` to `parse_options!` in `syntax/src/parser/options.rs`
3. Add `--allow-repeat-limit` to CLI `FileArgs`
4. Add `|= self.allow_repeat_limit` in `compiler_options()`
5. Add LSP extraction for `"allowRepeatLimit"`

#### Step 3: Add Tokens (if needed)

If the syntax uses a new keyword like `LIMIT`, add it to `keywords` in `syntax_kinds!` (`syntax/src/syntax_kind.rs`), give it a `TokenType` and a row in `parser/src/tokens.rs`, and gate it so that `LIMIT` is an ordinary name when the flag is off � a row in `ParseOptions::keyword_enabled`:

```rust
K::Limit => self.allow_repeat_limit,
```

#### Step 4: Add Grammar and Lowering

Add the grammar function in `syntax/src/parser/grammar/` (the `REPEAT` statement is read by the function that reads the statements), the node kind and its `node_disposition` arm, and the lowering rule in `syntax/src/lower/statements/`. Add an expected-AST test in `parser/src/tests/` and, if the legacy oracle rejects the form, the row in the parity tables that says so.

#### Step 5: Add AST Nodes

Update the `dsl` crate to represent the new syntax in the AST.

#### Step 6: Add Analyzer Validation

Add semantic checks in `analyzer/` if needed.

#### Step 7: Add Codegen

Add bytecode emission for the new syntax in `codegen/`.

#### Step 8: Update plc2plc Renderer

Update `plc2plc/src/renderer.rs` to render the new AST nodes.

#### Step 9: Add Round-Trip Test

1. Create `compiler/resources/test/repeat_limit.st`
2. Create `compiler/plc2plc/resources/test/repeat_limit_rendered.st`
3. Add test in the matching focused file under `plc2plc/src/tests/`

#### Step 10: Add Execution Test

Create `compiler/codegen/tests/end_to_end_repeat_limit.rs` with tests that compile and run programs using the new syntax, verifying correct variable values.

#### Step 11: Run CI

```bash
cd compiler && just
```

All checks must pass before creating a PR.
