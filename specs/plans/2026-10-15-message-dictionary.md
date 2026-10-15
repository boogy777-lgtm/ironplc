# Message Dictionary: Diagnostic Texts as Checked Templates

## Goal

Move the detailed text of each diagnostic label out of the call sites and into a
dictionary next to `problem-codes.csv`, the way CODESYS keeps message texts in
resource tables. Unlike a resource table, every template is checked when the
compiler is built: a call that passes a missing, extra or misspelled parameter
does not compile.

Today the dictionary holds only the code, the internal name and the short
message (`problem-codes.csv`). The text the user reads in the label is written
with `format!` at ~251 `Diagnostic::problem(` call sites in 114 files.

## Architecture

- **New file** `compiler/problems/resources/problem-texts.csv` with the columns
  `Code,Key,Template`. One row is one label text. A code may have several rows
  (one per wording, for example "its value is '{value}'" and "it has no value").
  A separate file keeps the three-column `problem-codes.csv` unchanged, so its
  other readers (`mcp/build.rs`, `docs/extensions/ironplc_problemcode.py`) are
  untouched.
- **Template syntax**: plain text with named placeholders `{name}`
  (`[a-z][a-z0-9_]*`); `{{` and `}}` are literal braces. No format specs.
- **`problems/build.rs`** reads both files and fails the build when
  - a row names a code that is not in `problem-codes.csv`;
  - a `Key` is not snake_case or is used twice;
  - a placeholder is malformed.
- **Generated API** (in `ironplc-problems`):
  - `pub struct Text` holding the `Problem` and the rendered `String`, with
    accessors; it can only be made by the generated functions.
  - module `text` with one function per row, named by `Key`, taking one
    `impl std::fmt::Display` parameter per distinct placeholder in order of
    first appearance, returning `Text`. The body is
    `format!("<template>")` with inline captured names, so rustc itself checks
    that every placeholder has a parameter.
- **`dsl::Diagnostic`** gets one constructor that takes a span and a `Text`, and
  builds the same diagnostic as `Diagnostic::problem(text.problem(),
  Label::span(span, text))`. The code and the label text come from one row, so a
  text cannot be paired with the wrong code. A secondary label of a migrated
  code also comes from a row of that code.
- **Sample migration**: P4076, P4077, P4078 in `analyzer/src/rule_attribute.rs`.
  The rendered texts stay byte-identical; existing tests do not change.

## N+1

Adding a text = one row + one call of the generated function. No list to keep in
step by hand: the function is derived from the row, and its parameters from the
template.

Declared gap, closed by the last migration PR: until every code is migrated,
nothing stops new code from building a label with `format!`. The steering rule
(`problem-code-management.md`) says new texts go into the dictionary; when the
last call site is migrated, `Diagnostic::problem(Problem, Label)` stops being
public, which turns the rule into a compile error.

## Prefactoring

None needed: the change adds a file, a generated module and one constructor;
`rule_attribute.rs::diagnostic()` already isolates the three texts in one
function.

## Design doc reference

None. An ADR records the decision (dictionary file, compile-time check, binding
of text to code) in the mechanism PR.

## File map

- `compiler/problems/resources/problem-texts.csv` — new
- `compiler/problems/build.rs` — read, validate, generate `Text` and `text`
- `compiler/problems/src/lib.rs` — tests of the generated functions
- `compiler/dsl/src/diagnostic.rs` — constructor from `Text`
- `compiler/analyzer/src/rule_attribute.rs` — P4076–P4078 use the dictionary
- `specs/steering/problem-code-management.md` — how to add a text
- `specs/adrs/NNNN-message-texts-in-a-checked-dictionary.md` — the decision

## Tasks

### Core change PR 1: mechanism + P4076–P4078

- [ ] `problem-texts.csv` with the rows for P4076, P4077 (found / missing),
      P4078 (repeated / first written here)
- [ ] `build.rs` validation and generation
- [ ] `Diagnostic` constructor from `Text`
- [ ] migrate `rule_attribute.rs::diagnostic()`; tests unchanged and passing
- [ ] tests: generated text equals the template with arguments substituted;
      `{{`/`}}` escapes; a build-script unit test per validation failure
- [ ] steering + ADR
- [ ] `cd compiler && just` passes

### Later PRs (one crate or one family of codes per PR)

- [ ] migrate the remaining `Diagnostic::problem(` call sites
- [ ] `.with_context(` descriptions, if they are user-facing text
- [ ] make `Diagnostic::problem(Problem, Label)` non-public
- [ ] optional, owner's decision: a translation column
