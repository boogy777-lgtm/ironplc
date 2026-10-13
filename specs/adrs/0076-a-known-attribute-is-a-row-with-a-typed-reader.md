# A Known Attribute Is a Row with a Typed Reader

status: accepted
date: 2026-10-13

## Context and Problem Statement

[ADR-0075](0075-an-attribute-is-data-of-its-declaration.md) made an attribute
data of the declaration it stands before. Every attribute was still accepted:
any name, any value, before any declaration. The goal of the work on pragmas is
that each attribute CODESYS documents does in this compiler what it does in
CODESYS, and each of them needs the same three answers before it can do
anything: is this name one the compiler knows, may it stand before this
declaration, and is its value one it takes.

Where those answers live decides what each of the dozens of attributes costs.
There are 49 attributes with a CODESYS help page read so far, and more will
follow. Each has a name, sometimes aliases (`noinit`, `no_init`, `no-init`), a
set of declarations it may stand before, one of a few kinds of value, and a
reader that acts on it (layout reads `pack_mode`, the enumeration analysis reads
`strict`). An accepted attribute that no part of IronPLC acts on must not be
presented as honoured.

## Decision Drivers

- **Adding an attribute is one entry.** Not an entry and a branch, a test
  table and a documentation page edited by hand.
- **A reader asks for what it needs by type.** The code that lays out a structure
  asks for the `pack_mode` of a declaration and gets a value it can match; it
  does not compare a name with a string.
- **What the check accepts and what the reader returns cannot differ.** A second
  table of allowed values drifts from the code that reads them.
- **A wider or narrower row is a wrong refusal or a silent acceptance.** The rows
  come from the CODESYS help.
- **An attribute the compiler does not act on is never presented as honoured.**
  The state of a row says so, in the code and on the reference page.
- **A user-defined attribute stays legal.** A name with no row is accepted and
  kept.

## Considered Options

1. **A row with a typed reader (chosen).** The registry is one declaration list;
   each entry makes the typed constant of the row (`PACK_MODE`) and its place in
   the type-erased list of all rows. The reader is `ROW.of(&attributes)`. One
   check reads the list.
2. **Each observer compares names.** The code that acts on an attribute finds it
   in the attributes of the declaration by comparing the name with a string, and
   parses the value itself.
3. **A field per attribute on the node, filled by the lowering.** The
   declaration nodes get a typed field for each known attribute (`pack_mode:
   Option<PackMode>`); the lowering reads the pragmas and fills them.

## Decision Outcome

Chosen option: **a row with a typed reader**, because it is the only option in
which a new attribute is one entry and nothing else, and in which the check and
the reader are one reading of the value.

A row (`ironplc_dsl::attribute::registry`) is the name, the aliases, the set of
`TargetKind` the attribute may stand before, the kind of its value, the family
(what it concerns in CODESYS), one line of summary and the state: `Waiting` when
no part of IronPLC acts on the attribute yet, `Honoured` when a part does. One
declaration list (`rows!`) makes both the typed constant of each row and the list
`ALL`, so a row cannot exist outside the list and no second list of names
exists.

Each type of value has one function that reads it (`Value::parse`) and states
what it takes (`ValueKind`). The reader returns what `parse` gives; the check of
a value asks whether it gives anything. A value kind added later is a type, its
`parse`, and an arm in each match over `ValueKind`, which has no wildcard, so the
build fails until every one answers.

One check, `faults(kind, &attributes)`, returns data: an attribute that stands
before a kind of declaration its row does not stand before, one whose value its
row does not read, one written twice (a name and its alias are the same
attribute). An attribute with no row has no fault unless it is repeated. One
semantic rule turns the faults into three problem codes (P4076, P4077, P4078)
and holds no logic about a row. The reference page's table of rows is
generated from the list into a checked-in file, and a test fails when the file
differs.

### Consequences

- Good, because adding the 50th attribute is one entry in the declaration list
  (and its value type if the value is of a new kind) and the regenerated table
  file; no branch, no list and no page is edited by hand.
- Good, because an observer asks `PACK_MODE.of(&declaration.attributes)` and
  gets an `Option<PackMode>`: no string comparison outside the registry.
- Good, because the check and the reader cannot disagree on a value.
- Good, because the state of a row is data: the reference page cannot call an
  attribute honoured that no part of IronPLC acts on.
- Bad, because an attribute before a block is attached to each declaration of
  the block, so a block that holds declarations of different kinds gets the
  attribute reported for the kinds it does not apply to.
- Bad, because a program that CODESYS accepts and that has a known attribute in a
  place the row does not allow is now refused. The rows are as wide as the CODESYS
  help says; a row that is too narrow is a wrong refusal and is fixed in the
  row.
- Neutral: the rows are read from the help pages. An attribute whose page was not
  read has no row and is user-defined until it has.

### Confirmation

- `compiler/dsl/src/attribute/registry/tests.rs`: the names and aliases are
  unique without regard to case, every name is found in any case, and the
  readers.
- `compiler/dsl/src/attribute/registry/faults/tests.rs`: for every row and every
  kind of declaration, the faults.
- `compiler/dsl/src/attribute/registry/table/tests.rs`: the checked-in table is
  what the registry generates.
- `compiler/analyzer/src/rule_attribute.rs`: one test from source text for each
  problem code, the attribute before a block, and the user-defined attribute.

## Pros and Cons of the Options

### A row with a typed reader

- Good, because one entry declares everything about an attribute, and the
  declaration is the only place that names it.
- Good, because the typed constant is the only handle an observer needs.
- Bad, because the registry is a crate-wide list the dsl crate owns, and a
  row that is wrong is wrong everywhere at once.

### Each observer compares names

- Good, because nothing is built before the first observer needs it.
- Bad, because each observer repeats the name, its aliases, its case rule and
  its value parsing, and the repetitions drift.
- Bad, because nothing checks the attributes that no observer reads: a wrong
  place or a wrong value is accepted until an observer exists, and then reported
  in whichever way that observer chooses.
- Bad, because the reference page would be a hand-written list that no code
  contradicts.

### A field per attribute on the node, filled by the lowering

- Good, because an observer reads a typed field.
- Bad, because every attribute adds a field to the nodes it may stand before and
  a line to the lowering, and the lowering must know the places and the values.
- Bad, because the lowering of the syntax tree would hold the CODESYS attribute
  catalogue, and the attribute as written (an unknown name, a bad value) must
  still be kept to be reported.

## More Information

- [Attributes of Declarations](../design/attributes.md): the registry, the
  reader, the faults and what the analysis reports.
- [ADR-0075](0075-an-attribute-is-data-of-its-declaration.md): the attribute as
  data of the declaration.
