# An Attribute Is Data of Its Declaration

status: accepted
date: 2026-10-13

## Context and Problem Statement

CODESYS, TwinCAT and the other implementations on the CODESYS V3 runtime
describe a declaration with attribute pragmas written before it:
`{attribute 'pack_mode' := '1'}` before a structure, `{attribute 'hide'}` before
a function block, `{attribute 'qualified_only'}` before a block of variables.
The goal of the work on pragmas is that each pragma CODESYS documents does in
this compiler what it does in CODESYS. Every one of them needs the same first
step: the compiler must know which declaration the pragma describes.

Until now a pragma was one trivia token. The lossless syntax tree keeps it, but
the lowering reads no trivia, so the library the analysis and code generation
read never held an attribute, and nothing could act on one. An attribute
pragma that stood in a place where it describes nothing was accepted like a
comment.

Where the attribute lives decides what every later change to the pragmas costs:
how a rule reads the attributes of a declaration, what a transform that rebuilds
a declaration must remember, and what is said when a pragma stands where it
describes nothing.

## Decision Drivers

- **One rule says which declaration a pragma describes.** The lowering has
  nineteen places that build a declaration; nineteen copies of a walk over
  trivia would be nineteen places to get the rule wrong.
- **A reader of an attribute asks the declaration.** A rule that checks a
  variable, or code generation that lays out a structure, reads what it has in
  hand.
- **An accepted attribute is never without effect by accident.** A pragma that
  describes nothing is an error, not a silent no-op.
- **The attribute survives the analysis.** The field is filled once and read
  after many transforms.
- **The legacy parser stays as it is.** It reads no attribute and is deleted
  with the cutover.

## Considered Options

1. **A field of the declaration node (chosen).** Every declaration that an
   attribute can stand before has the field `attributes`; the lowering fills it
   from the tree.
2. **A side table keyed by span.** The lowering returns the library as it was
   and a table from the span of a declaration to its attributes.
3. **Attributes left as trivia.** The tree keeps the pragmas; a reader that wants
   them walks the tree.

## Decision Outcome

Chosen option: **a field of the declaration node**, because it is the only
option in which the attribute travels with the declaration without anyone being
asked to carry it.

An attribute is `Attribute { name, value, span }`, and a declaration holds
`Attributes`, a list in source order. Nineteen kinds of declaration have the
field (the ten kinds of type declaration, a structure member, a variable, a
function, a function block, a program, an interface, a method, a property and an
action), named once as `TargetKind` and listed by one census of a library.
`DataTypeDeclarationKind::with_attributes` and the trait `Attributed` answer for
every kind of type declaration with one arm each and no wildcard.

The attachment rule is one function over the tree
(`ironplc_syntax::lower::attributes::attributes_of`): a pragma is attached to the
node it stands directly before, past trivia; a pragma before a `TYPE` block or
a variable block is attached to every declaration the block holds. The
lowering of a declaration asks it and reads no trivia itself. The pragmas that
no node claims are reported (P0043) from the same relation, so a pragma is
attached or reported, never both and never neither. The design is in
[Attributes of Declarations](../design/attributes.md).

### Consequences

- Good, because every later rule reads the attributes of the declaration it
  checks, and no rule is handed a table.
- Good, because a pragma that describes nothing is an error with a code, a
  page and a span, and the rule that attaches and the rule that reports are the
  same relation.
- Good, because the equality of a declaration includes its attributes, so a
  transform that drops them is caught by the comparisons that already exist.
- Bad, because every transform that rebuilds a declaration from another must
  carry the field over. The test of the analysis (`REQ-ATTR-analyzer-001`) holds
  an attribute before each of the nineteen kinds and fails if one stops.
  The transforms that rebuild a declaration today already carry it
  (`xform_resolve_type_decl_environment` and `xform_resolve_decl_types`); a
  declaration built by hand or by code generation has none.
- Bad, because the debug text of the library holds the attributes, so the
  analysis pins move when a pinned source has one. None of the pinned sources
  does.
- Bad, because a pragma that the legacy parser accepts and drops, in a place
  where it describes nothing, is now refused. The parity comparison records the
  one fixture of the corpus that shows it.
- Neutral: no attribute has an effect. Which names are known, where a known
  attribute may stand and what it does are later decisions that read this field.

### Confirmation

- `compiler/syntax/src/lower/attributes/tests.rs`: an attribute before each of
  the nineteen kinds, before a group, before several names, and the guard that
  every kind of node that is a target has one.
- `compiler/syntax/src/lower/attributes/reported_tests.rs`: every place where a
  pragma is attached to nothing, the span and the message of the error, the
  pragmas that are not attributes, the dialect gate and the text of a branch of
  `{IF}` that is not taken.
- `compiler/analyzer/src/spec_conformance_attributes.rs`: the analysed library
  holds the attribute of every kind.
- `compiler/plc2plc/src/spec_conformance_attributes.rs`: the rendering and the
  round trip for every kind.

## Pros and Cons of the Options

### A field of the declaration node

- Good, because the attribute is where the reader is, and it follows the node
  through every transform that moves the node.
- Good, because one function attaches it, once, in the lowering.
- Bad, because it adds a field to nineteen nodes and a duty to each transform
  that builds one of them.

### A side table keyed by span

- Good, because no node changes.
- Bad, because every reader must be handed the table, and every analysis stage
  would have to carry it next to the library.
- Bad, because the key is the span of the declaration, and a transform that
  rebuilds a node (the resolution of a late-bound type does, and gives it
  another kind) breaks the key without a word.

### Attributes left as trivia

- Good, because it is what the tree already does.
- Bad, because no analysis reads the tree: the library that the analysis and
  code generation read holds no trivia, so no rule could read an attribute.

## More Information

- [Attributes of Declarations](../design/attributes.md): the rule, what is
  reported and how the field is rendered.
- [CODESYS Jump Statements, Comments and Conditional Pragmas](../design/codesys-jump-and-pragmas.md):
  the conditional pragmas, which stay trivia that the lexer consumes.
- [ADR-0072](0072-lowering-lives-in-the-syntax-crate.md): the lowering that fills
  the field.
