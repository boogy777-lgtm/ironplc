# Attributes of Declarations

## Overview

An attribute pragma — `{attribute 'name'}` or `{attribute 'name' := 'value'}` —
is data of the declaration it stands before. CODESYS, TwinCAT and the other
implementations built on the CODESYS V3 runtime write them before types,
variables and program organisation units (`{attribute 'pack_mode' := '1'}`,
`{attribute 'hide'}`, `{attribute 'qualified_only'}`). This document specifies
how the compiler attaches each attribute pragma to the declaration it stands
before, what it reports when a pragma stands before no such declaration, and
how the attributes travel through the compiler.

The attribute data of a declaration is a field of the declaration's node in
the library (`attributes: Attributes`, `ironplc_dsl::attribute`). The lowering
of the syntax tree fills it; the analysis keeps it; the renderer writes it
back as the pragmas it came from.

**Out of scope.** Which attribute names are known, where a known attribute may
stand, which values it takes, and what it does to the analysis or the
generated code are not part of this document. An attribute is carried as
written; no rule and no code generation reads one yet.

The conditional pragmas (`{IF}`, `{DEFINE}`, ...) stay in
[codesys-jump-and-pragmas.md](codesys-jump-and-pragmas.md); the decision to
make an attribute a field of the node is
[ADR-0075](../adrs/0075-an-attribute-is-data-of-its-declaration.md).

## Terms

- An **attribute pragma** is a pragma whose text reads as an attribute
  (`ironplc_syntax::pragma::kind` answers `Kind::Attribute`): a name that is
  not empty and holds letters, digits, `-`, `.` and `_`, and optionally a
  value. Pragmas that read as a message (`{text}`, `{info}`, `{warning}`,
  `{error}`), as a conditional directive, or as anything else are not
  attribute pragmas.
- A **target** is a declaration that has the field `attributes`. There are
  nineteen kinds: the ten kinds of type declaration (enumeration, subrange,
  simple, array, structure, union, string, reference, parameter list and
  late-bound), the member of a structure or a union, a variable, a function, a
  function block, a program, an interface, a method, a property and an action.
- A **group** is a node that only holds declarations: a `TYPE ... END_TYPE`
  block and a `VAR ... END_VAR` block of any kind.
- A pragma **follows** a node when the node is the first node after the
  pragma in source order, with only trivia (white space, comments and other
  pragmas) between the pragma and the first token of the node.

The syntax tree keeps every pragma as a trivia token and places the trivia that
precedes a node in the node that holds it, before the node. So the pragmas that
stand before a node are the run of trivia tokens just before it among its
siblings.

## The rule

An attribute pragma is attached to what it stands before:

- when it stands before a target, it is attached to the target;
- when it stands before a group, it is attached to every declaration the group
  holds that is a target;
- when it stands before anything else, it is attached to nothing.

**REQ-ATTR-syntax-001** An attribute pragma that stands before a target is an
attribute of the target, with the name and the value as written and the span
of the pragma, for every kind of target.

**REQ-ATTR-syntax-002** Trivia between an attribute pragma and the target —
white space, comments, and pragmas of other kinds — does not prevent the
attachment.

**REQ-ATTR-syntax-003** Several attribute pragmas before one target are all
attributes of it, in source order.

**REQ-ATTR-syntax-004** An attribute pragma before a `TYPE` block is an
attribute of every type declaration of the block, and one before a variable
block of any kind is an attribute of every variable of the block. The
attributes a group passes down come before the declaration's own, in source
order.

**REQ-ATTR-syntax-005** `a, b : INT;` is one declaration in the source and two
variables in the library: the attributes before it are attributes of both.

One function over the tree answers which attributes are attached to a node
(`ironplc_syntax::lower::attributes::attributes_of`); the lowering of a target
asks it and reads no trivia itself.

**REQ-ATTR-syntax-006** Whether an attribute can stand before a node kind is
answered in one place, by a match over every node kind without a wildcard arm
(`node_role`): a node kind added to the syntax declaration does not compile
until it says. Every kind of node that is a target has an attribute attached to
a node of it in the test program of the nineteen kinds, so a target whose
lowering does not ask for its attributes fails a test.

## The library

**REQ-ATTR-dsl-001** Attributes are part of the equality of a declaration: two
declarations that differ only in their attributes are not equal.

**REQ-ATTR-dsl-002** `DataTypeDeclarationKind::with_attributes` sets the
attributes of every kind of type declaration, and `Attributed` answers them
back.

**REQ-ATTR-dsl-003** The kinds of target are named once (`TargetKind`) and the
census (`ironplc_dsl::attribute::census::declared`) lists every declaration of
a library of each kind with its attributes.
