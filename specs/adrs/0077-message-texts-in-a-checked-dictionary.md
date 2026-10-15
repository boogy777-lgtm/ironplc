# Message Texts in a Checked Dictionary

status: accepted
date: 2026-10-15

## Context and Problem Statement

A diagnostic has a code and one generic message in `problem-codes.csv`. The text
the user reads in the label ("The attribute 'pack_mode' takes one of ..., but its
value is '3'") is written with `format!` at the call site, about 250 of them in
114 files. Nothing relates that text to the code it is reported under, nothing
lists the texts of a code, and a text cannot be reviewed, searched or translated
without reading the Rust around it. A call site can pair any text with any code.

CODESYS keeps message texts in resource tables, apart from the code that reports
them. The table is read when a message is displayed: a text whose parameters do
not match what the caller gives is found when that message is shown, not when the
product is built.

## Decision Drivers

- **A text is data.** It is a row next to the code, not a string inside a
  function.
- **A text cannot be paired with the wrong code.** The row names the code.
- **A wrong parameter is a build error.** Missing, extra and misspelled
  parameters are found when the compiler is built, not when the message is first
  shown to a user.
- **Adding a text is one row and one call.** No list is kept in step by hand.
- **`problem-codes.csv` is read by other tools** (`mcp/build.rs`,
  `docs/extensions/ironplc_problemcode.py`) which parse it by position.

## Considered Options

1. **A second file of rows, one generated function for each row (chosen).**
   `problem-texts.csv` has the columns `Code,Key,Template`; `problems/build.rs`
   makes `ironplc_problems::text::<Key>(...)` with one parameter for each
   placeholder.
2. **`format!` at the call site (the state before).** Nothing to build; nothing
   checked.
3. **A runtime resource table, as in CODESYS.** Rows are looked up by key at
   display with named arguments.
4. **A fourth column in `problem-codes.csv`.** One file, one row for each code.

## Decision Outcome

Chosen option: **a second file of rows with one generated function for each
row**, because it is the only option in which the text, its code and its
parameters are one declaration that the compiler checks.

A template is text with named placeholders `{name}` (`[a-z][a-z0-9_]*`); `{{` and
`}}` are the literal braces; a format spec is not allowed. `build.rs` reads both
files and fails, naming the row, when a row names a code that is not in
`problem-codes.csv`, a key is not snake_case, is used twice or is not the name of its problem in
snake_case (or that name and `_` and more), a placeholder is
malformed, a brace is unbalanced, or the code is one of the compiler-located
problems (P9997, P9998, P9999), which have their own constructors on
`Diagnostic`.

For each row `build.rs` writes a function whose parameters are the distinct
placeholders in order of first appearance, each `impl std::fmt::Display`, and
whose body is `format!("<template>")` with the placeholders captured by name.
rustc itself then checks that every placeholder has a parameter and every
parameter a placeholder. The function returns a `Text`, which holds the `Problem`
of the row and the rendered message and has no public constructor.
`Diagnostic::text(span, text)` makes the diagnostic from it, the same as
`Diagnostic::problem(problem, Label::span(span, message))`, with the code and the
label text from one row. A secondary label of a migrated code is also a row of
that code (`Text` converts into the `String` a `Label` takes).

A code with several wordings has a row for each, and the call site chooses the
function. The wording is then never built by a `match` that produces strings.

The columns are in their own file because option 4 changes the shape of
`problem-codes.csv`, which two other readers parse by position, for a column that
only the Rust build reads.

### Consequences

- Good, because adding a text is one row and one call of its function; the
  function and its parameters are derived from the template.
- Good, because a text cannot be paired with another code: `Diagnostic::text`
  takes the code from the row.
- Good, because the texts of every code are in one file that can be read,
  searched, reviewed and, if the owner decides, translated by adding a column.
- Good, compared with a CODESYS resource table, because the parameters are
  checked when the compiler is built, not when the message is shown.
- Bad, because a Rust-side `Display` argument is rendered as the callee's
  `Display` and a template cannot choose the form (no format specs); the call
  passes the value in the form to show.
- Bad, because until every call site is migrated, nothing forces the use of the
  dictionary (the declared gap, below).
- Neutral: the sample migration is P4076, P4077 and P4078 in
  `analyzer/src/rule_attribute.rs`. Their rendered texts are unchanged and the
  tests of the rule are unchanged.

### Declared gap

`Diagnostic::problem(Problem, Label)` is still public, so a new call site can
build its label with `format!`. The steering rule
(`problem-code-management.md`) says new texts are rows; it is not enforced.

The condition that closes the gap: the migration of the last call site makes
`Diagnostic::problem(Problem, Label)` non-public, which turns the rule into a
compile error.

### Confirmation

- `compiler/problems/src/lib.rs`: each generated function returns the `Problem`
  of its row and the message with the arguments in place; the conversions of
  `Text`.
- `compiler/dsl/src/diagnostic.rs`: `Diagnostic::text` builds the diagnostic that
  `Diagnostic::problem` builds from the same problem and a `Label::span`.
- `compiler/analyzer/src/rule_attribute.rs`: the tests of the three codes pass
  unchanged, so the rendered texts are the texts written before.
- `compiler/problems_gen/src/tests.rs`: the validation and the generation of the
  rows live in the build-support crate `ironplc-problems-gen`, which does no file
  access (`problems/build.rs` reads the files and writes the result), because
  Cargo does not run the tests of a build script. One test for each failure
  (unknown code, key not snake_case, key a Rust keyword, placeholder a Rust
  keyword, key repeated, key without the prefix of its problem, malformed placeholder, format spec, unbalanced `{` and
  `}`, compiler-located code, wrong header) and for the generation (parameter
  order, repeated placeholder, `{{`/`}}`, no placeholder).

## Pros and Cons of the Options

### A second file of rows, one generated function for each row

- Good, because the compiler checks the parameters of every text at build time.
- Good, because `problem-codes.csv` and its readers do not change.
- Bad, because there are two files to read to see a code with its texts.

### `format!` at the call site

- Good, because nothing is built and the text is next to the code that has the
  values.
- Bad, because the text, its code and its parameters are not related by anything.
- Bad, because a text is found only by searching the Rust source.

### A runtime resource table, as in CODESYS

- Good, because a text can change without rebuilding.
- Bad, because a text whose parameters do not match is found when the message is
  shown, in front of the user.
- Bad, because the compiler is one binary built from this repository, so a text
  that changes without a build has no use.

### A fourth column in `problem-codes.csv`

- Good, because there is one file.
- Bad, because one row for each code cannot hold several wordings.
- Bad, because `mcp/build.rs` and `docs/extensions/ironplc_problemcode.py` parse
  the file by position and would each need a change.
