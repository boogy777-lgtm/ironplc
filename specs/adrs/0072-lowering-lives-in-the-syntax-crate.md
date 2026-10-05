# Lowering Lives in the Syntax Crate; the Parser Crate Is the Compatibility Facade

status: accepted
date: 2026-10-05
amended: 2026-10-05 (Confirmation records the facade and the switch, which have landed)

## Context and Problem Statement

The production frontend turns text into the `ironplc_dsl` objects with a PEG
grammar over a token stream that nine transforms have rewritten
(`ironplc-parser`). The replacement is a lossless tree (`ironplc-syntax`) and a
lowering from that tree to the same objects, so the compiler, the language
server and the tools read one parse that keeps every byte of the text
([parse-tree architecture](../design/parse-tree-architecture.md), sections 3.1
and 3.2).

Where the lowering lives decides how many crates a language construct touches,
how the grammar and the lowering are kept in agreement about the shape of the
tree, and what the existing consumers (the analyzer, code generation, the
formatter, the CLI, the language server, the MCP server and the playground)
have to change. Several decisions the work settled also need a place that
outlives the work: how a difference from the old parser is recorded, what
"total" means for the lowering, and which of the old parser's behaviours are
not carried over.

## Decision Drivers

- **One mechanism per class of behaviour.** A language extension adds a row to a
  table, not a branch or a crate.
- **Grammar and lowering agree about the tree.** The shape of a node is a
  contract between the function that builds it and the rule that reads it; drift
  should fail in one crate's tests, not across a crate boundary.
- **No change for consumers.** Hundreds of call sites use `parse_program`,
  `parse_st_statements`, `tokenize_program` and `CompilerOptions`.
- **The old parser is a temporary oracle.** It is deleted at cutover, so what
  holds the new path to it must not become part of the new path.
- **CODESYS is the reference** for what is accepted and what is reported where
  the standard is silent or the old parser disagrees with it.

## Considered Options

1. **The lowering in `ironplc-syntax`; `ironplc-parser` a thin facade.**
2. **A new `ironplc-lower` crate.**
3. **The lowering inside `ironplc-parser`.**

## Decision Outcome

Chosen option: **the lowering lives in `ironplc-syntax`** (`lower`, one module
tree with one module per area), and `ironplc-parser` remains the public
facade with today's signatures: it converts its options, selects one
diagnostic, and projects the tree's tokens to the token view the editor
consumes. Option 1 is the only one that leaves the grammar, the dialect gates,
the knowledge of node shapes and the lowering in one crate with no dependency
edge added (`ironplc-syntax` already depends on `ironplc-dsl` and
`ironplc-problems`), and the only one that lets a single test module assert the
contract between a grammar rule and the rule that reads its node.

The decisions below are properties of the lowering that the comparison with the
old parser established and that the code relies on.

### One disposition table with no wildcard

`disposition` says, for every kind a tree can hold, whether a rule of an area
lowers it, whether its parent's rule reads it, or whether it is trivia. It
matches every node kind without a wildcard arm, so a kind added to the grammar
does not compile until it has a decision, and a test requires every kind with a
rule of its own to occur in a corpus file that lowers. A new construct is one
kind, one grammar function, one disposition, one lowering arm and one corpus
case.

### Totality without panics

Lowering is defined on a parse that reported no error; given one that did, it
returns the first error positioned in the file and builds nothing. On a parse
that did not, it returns the library or a diagnostic about the text (a number
out of range, a form the object cannot represent), never an internal error and
never a capability that is not implemented. A tree that is not shaped as the
grammar guarantees is an internal error carrying the node and the missing part,
not a panic. Chains the grammar builds iteratively are lowered iteratively, and
a tree as deep as the parser allows lowers on the stack budget.

### One place where a range becomes a span

Every span of a lowered object is built from a range of the tree by the
lowering context, which carries the file. No later pass stamps the file onto
the library, so a position the lowering builds without a file is a defect: even
the empty placeholder name of a function block initialisation is positioned
(at the start of the file, in the file).

### A difference from the old parser is a row with a reason of one of three classes

While the old parser exists as the oracle, every difference between what it
builds or accepts and what the lowering builds or accepts is a row of a table,
and its reason is one of:

- **legacy defect not ported**: the old parser is wrong (it positions a label
  name nowhere, reads a parenthesised list of three steps as two, reads
  `METHOD ABSTRACT m` as a label, rejects what it accepts inside a program only
  because of its fragment entry);
- **owner-decided change**: a behaviour change the owner decided, by name or
  by the rule that CODESYS is the reference (every OSCAT marker pair is a
  region; an empty enumeration or bound list is a syntax error; a lone CR is a
  line break; the inside of a pragma is not examined; `;` is optional after
  every `END_*`);
- **accepted on purpose**: the new path takes, or builds another shape for,
  what the old parser rejects or builds differently, and the reason says why
  (a variable as an initial value, an inline subrange in any variable block, a
  named global location, either string delimiter for a declared width).

A reason is defined once with its class and named by each row it explains. An
unlisted difference fails, a listed one that no longer differs fails (a stale
row), and one that occurs more often than recorded fails (a growing row). A
difference that is none of the three is fixed in the new path instead of
listed: the grammar's reference element in an array of references no longer
admits an array target, which the object cannot hold, so the form is a syntax
error at the array where the old parser also rejected it.

### One diagnostic is reported, chosen by one table

The old parser reports the first problem it finds, in the order its checks run:
text it cannot read, each token check in turn, the conditional pragmas, the
grammar. The new parser finds every problem, so the choice is made explicitly:
`ironplc_syntax::STAGES` lists the stages in that order, each with the kinds of
error it reports, and `Parse::primary_error` is the error of the earliest stage
and, within a stage, the earliest in the text. A kind of error is a row (or one
more kind of an existing row), not a branch, and lowering follows the last
stage because it runs only on a tree without errors. The same comparison that
holds the objects to the old parser holds the one diagnostic: for every input
both reject, the problem code and the byte range are equal or a row of
`code_exceptions.rs` says why, and what the two say is equal or a row of
`diagnostics_messages.rs` says why. A temporal literal that is wrong in one
part names the whole literal in the message; a number of twenty digits or more
in a duration is a duration out of range (`P2039`), the same class as a count
that fits but whose duration does not.

### The inputs are one corpus in three spellings

Every `.st` source of the repository is read in place and compared as written,
with CRLF line ends and indented with tabs, by one generator shared by every
test that needs the corpus, not by a copy per file. A spelling that differs
only in trivia lowers to an equal library.

## Consequences

- Good, because a language extension touches one crate for the grammar, the
  gate, the disposition and the lowering, and the contract between a grammar
  rule and its lowering rule is tested in that crate.
- Good, because no consumer changes: the facade keeps its signatures, and the
  oracle stays reachable only from test code.
- Good, because a difference from the old parser cannot be recorded without
  saying whether the old parser or the new path is right, and which decision
  made it so.
- Bad, because `ironplc-syntax` grows by the lowering and every consumer of the
  parser links the lowering and the tree library.
- Bad, because until the old parser is deleted two parse paths exist, one of
  them test-only.
- Neutral: the table of differences is deleted with the old parser, and the rows
  that remain true become permanent accept/reject and expected-object tests.

### Confirmation

- `compiler/syntax/src/spec_conformance.rs` holds the tests of
  `REQ-PT-syntax-001` to `REQ-PT-syntax-009`: reconstruction, regions,
  totality over the corpus and its prefixes, refusal of a parse with errors,
  coverage of every node kind with a rule, spans in the file, spelling of
  trivia, nesting as deep as the parser allows, and the one primary error.
- `compiler/parser/src/tests/parity/` runs the strict comparison with the old
  parser over the corpus, the tables of statements, expressions and
  declarations, and the declarations lifted from the old parser's tests, under
  every dialect preset; `differences.rs` and `tables.rs` hold the rows with
  their classes.
- The facade and the switch to the new path have landed. `ironplc-parser`
  reads text through `compiler/parser/src/frontend.rs` only (option conversion,
  the one diagnostic of the ranking, the token view of `tokens.rs`), and the
  old parser is declared under `cfg(test)` in the crate root, so a build without
  it cannot name it; `frontend::tests::legacy_modules_when_declared_in_the_crate_root_then_compiled_for_tests_only`
  fails when a module that is not production code is declared without it.

## Pros and Cons of the Options

### The lowering in `ironplc-syntax`

- Good, because grammar, gates, node shapes and lowering are in one crate.
- Good, because it adds no dependency edge.
- Bad, because the crate that consumers use for tokens now also carries the
  lowering.

### A new `ironplc-lower` crate

- Good, because it separates the tree from the objects.
- Bad, because it adds a crate, its release wiring and a boundary between the
  grammar and the code that must agree with its node shapes, for no second
  consumer.

### The lowering inside `ironplc-parser`

- Good, because the old parser and the new path sit together.
- Bad, because the grammar and the code that reads its tree are then in
  different crates, and the old parser's lifetime is the opposite of the
  lowering's.
