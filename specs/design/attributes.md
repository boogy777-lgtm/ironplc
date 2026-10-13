# Attributes of Declarations

## Overview

An attribute pragma — `{attribute 'name'}` or `{attribute 'name' := 'value'}` —
is data of the declaration it stands before. CODESYS, TwinCAT and the other
implementations built on the CODESYS V3 runtime write them before types,
variables and program organisation units (`{attribute 'pack_mode' := '1'}`,
`{attribute 'hide'}`, `{attribute 'qualified_only'}`). This document specifies
how the compiler attaches each attribute pragma to the declaration it stands
before, what it reports when a pragma stands before no such declaration, which
attributes the compiler knows and what it reports about a known attribute that
stands in the wrong place, has a value it does not take or is written twice,
and how the attributes travel through the compiler.

The attribute data of a declaration is a field of the declaration's node in
the library (`attributes: Attributes`, `ironplc_dsl::attribute`). The lowering
of the syntax tree fills it; the analysis keeps it; the renderer writes it
back as the pragmas it came from.

**Out of scope.** What an attribute does to the analysis or to the generated
code is not part of this document: no part of IronPLC acts on an attribute yet.
The names the compiler knows, the places a known attribute may stand and the
values it takes are in scope (see [The registry](#the-registry)); an attribute
with no row is accepted and kept as written.

The conditional pragmas (`{IF}`, `{DEFINE}`, ...) stay in
[codesys-jump-and-pragmas.md](codesys-jump-and-pragmas.md); the decision to
make an attribute a field of the node is
[ADR-0075](../adrs/0075-an-attribute-is-data-of-its-declaration.md), and the
decision to make a known attribute a row with a typed reader is
[ADR-0076](../adrs/0076-a-known-attribute-is-a-row-with-a-typed-reader.md).

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
asks it and reads no trivia itself. The same relation is what the check reads,
so a pragma is either attached or reported.

**REQ-ATTR-syntax-006** Whether an attribute can stand before a node kind is
answered in one place, by a match over every node kind without a wildcard arm
(`node_role`): a node kind added to the syntax declaration does not compile
until it says. Every kind of node that is a target has an attribute attached to
a node of it in the test program of the nineteen kinds, so a target whose
lowering does not ask for its attributes fails a test.

## What is reported

An attribute pragma that is attached to nothing is an error (P0043). It would
otherwise be accepted and do nothing, and the purpose of the attributes is that
an accepted attribute is never without effect by accident. The span of the
error is the pragma, and the message names the attribute.

**REQ-ATTR-syntax-010** An attribute pragma before `END_VAR`, `END_TYPE`,
`END_FUNCTION_BLOCK` or another closing keyword is reported as P0043.

**REQ-ATTR-syntax-011** An attribute pragma before a statement, inside an
expression, or at the end of the file is reported as P0043.

**REQ-ATTR-syntax-012** An attribute pragma before a node that is neither a
target nor a group (`CONFIGURATION`, `RESOURCE`, `TASK`, `NAMESPACE`, a type
written after the `:` of a declaration) is reported as P0043.

**REQ-ATTR-syntax-013** An attribute pragma before a variable block that holds
no variable (`VAR_ACCESS`, `VAR_CONFIG`) is reported as P0043: the block has no
declaration to attach it to.

**REQ-ATTR-syntax-014** The span of the P0043 error is the span of the pragma
and its message names the attribute as written.

**REQ-ATTR-syntax-015** For every source, every attribute pragma of its tree is
attached to a declaration or reported, never both and never neither.

**REQ-ATTR-syntax-016** The lowering of a statement list reports an attribute
pragma in the list as P0043 like the lowering of a file.

## What is not an attribute

**REQ-ATTR-syntax-020** A pragma that does not read as an attribute (a
message, a conditional directive, any other pragma, and a malformed
`{attribute}`) stays trivia as it is without the attributes: it is attached to
nothing and reported as nothing, wherever it stands.

## Dialects and conditional text

**REQ-ATTR-syntax-030** Without `allow_pragmas` a `{` is the syntax error it is
without the attributes; with it, the attribute pragmas are read as above.

**REQ-ATTR-syntax-031** An attribute pragma in a branch of an `{IF}` that is
not taken is hidden text: it is attached to nothing and not reported.

## The library

**REQ-ATTR-dsl-001** Attributes are part of the equality of a declaration: two
declarations that differ only in their attributes are not equal.

**REQ-ATTR-dsl-002** `DataTypeDeclarationKind::with_attributes` sets the
attributes of every kind of type declaration, and `Attributed` answers them
back.

**REQ-ATTR-dsl-003** The kinds of target are named once (`TargetKind`) and the
census (`ironplc_dsl::attribute::census::declared`) lists every declaration of
a library of each kind with its attributes.

**REQ-ATTR-analyzer-001** After the full analysis the library still carries the
attributes of every kind of target. A transform that rebuilds a declaration from
another one carries the field over.

## The registry

A **known attribute** is a **row** of the registry
(`ironplc_dsl::attribute::registry`). A row is the one declaration of what the
compiler knows about an attribute:

- the **name**, and the **aliases** that name the same attribute (`noinit`,
  `no_init` and `no-init`);
- the **targets**: the kinds of declaration the attribute may stand before, a
  set of `TargetKind`;
- the **value kind**: what the attribute takes (see [The reader](#the-reader));
- the **family**, which groups the rows on the reference page by what the
  attribute concerns in CODESYS;
- the **summary**: one line saying what the attribute does in CODESYS;
- the **state**: `Waiting` when no part of IronPLC acts on the attribute yet,
  so the compiler reads and checks it and does nothing else with it;
  `Honoured` when a part of IronPLC acts on it. Every row is `Waiting` today.

An attribute that has no row is **user-defined**. It is accepted and kept as
written, and nothing is reported about it unless it is written twice.

The rows are what the CODESYS help documents. An attribute whose help page was
not read has no row, and so has every TwinCAT name.

The registry is one declaration list (`rows!` in `registry/rows.rs`). Each entry
makes the typed constant of the row (`PACK_MODE`) and the place of the row in
`ALL`, the list of all rows without the type of the value. A row cannot exist
outside the list, and no other list of names exists: the reference page, the
check and the tests read `ALL`. Adding an attribute is one entry and the
regenerated table of the reference page (see
[The reference page](#the-reference-page)).

**REQ-ATTR-dsl-010** Every known attribute is a row in one list: a row has a
name, a summary and at least one kind of target, and the typed constant of a
row is the entry of that list.

**REQ-ATTR-dsl-011** A name or an alias finds its row without regard to case;
the names and aliases of the rows are unique across the registry without regard
to case; a name that is no row's name or alias finds no row.

**REQ-ATTR-dsl-012** The kinds a row stands before are a set of `TargetKind`
that a row states as a constant; the ten kinds of type declaration are named
once, and that set is exactly the kinds whose label says "type"; a set names
itself by the labels of its kinds.

## The reader

A part of the compiler that acts on an attribute asks the typed constant of the
row, never the name: `registry::PACK_MODE.of(&declaration.attributes)` gives an
`Option<PackMode>`. The reader finds the first attribute of the declaration
named by the row or an alias, without regard to case, and reads its value. It
gives nothing when the declaration has no such attribute and when its value is
not one the row takes. It reports nothing: the fault of a value is reported by
the check (see [The faults](#the-faults)).

A row names the type of its value, and each type reads its value with one
function (`Value::parse`). The reader returns what that function gives and the
check of the value asks whether it gives anything, so what the check accepts and
what the reader returns cannot differ. A type states what it takes
(`ValueKind`) beside its `parse`, and that statement is what a message and the
reference page say. A value kind added later is a new type, its `parse`, and an
arm in each match over `ValueKind`, which has no wildcard.

| Value kind | Type | A value is valid when |
|---|---|---|
| `Flag` | `Flag` | the attribute has no value |
| `OneOf` | an enumeration named by meaning (`PackMode`) | it is one of the listed texts, without regard to case and to the white space around it |
| `OptionalOneOf` | `Optional<..>` of such an enumeration | the attribute has no value, or as `OneOf` |
| `Number` | `Number(u32)` | it is an unsigned decimal integer that fits `u32`, with white space around it ignored |
| `Text` | `Text` | it is not empty and not only white space |
| `OptionalText` | `OptionalText` | always |

**REQ-ATTR-dsl-020** `ROW.of(&attributes)` gives the typed value of the first
attribute that the name or an alias of the row names, without regard to case,
and nothing when the attribute is absent or its value is not valid.

**REQ-ATTR-dsl-021** The check of a value and the reader use the same reading
of it: for every row, a value of its kind that is valid is accepted and a value
that is not is refused, and every text a `OneOf` or `OptionalOneOf` row lists is
accepted in any case and with white space around it.

**REQ-ATTR-dsl-022** Each value kind reads the values of the table above: valid
and invalid samples of each.

## The faults

`registry::faults(kind, &attributes)` is the one check. It takes the kind of
the declaration and its attributes and returns the faults, each with the name of
the attribute as written and its span. It holds no logic about a row: it asks
the row where it may stand and whether it takes the value. The part of the
compiler that reports a fault turns it into its own diagnostic and holds no
logic about a row either. One attribute has at most one fault, found in this
order:

1. **Repeated**: an attribute of the same declaration before it is the same
   attribute. Two known attributes are the same when they are the same row, so
   `noinit` and `no-init` are the same; two user-defined attributes are the same
   when their names are equal without regard to case. The fault is at the
   later attribute and carries the span of the first.
2. **Not allowed here**: the attribute is known and the declaration is of a kind
   its row does not stand before.
3. **Value**: the attribute is known and its value is not one its row takes: a
   value given to an attribute that takes none, a value missing, a value not
   in the list, a value that is not an integer, an empty text. The fault carries
   the value as written, and `None` when it is missing.

An attribute that is user-defined and not repeated has no fault.

**REQ-ATTR-dsl-030** A known attribute before a declaration of a kind that its
row does not stand before has the fault `NotAllowedHere`, at its span with the
row and the kind; before a kind the row stands before it has none; a name, an
alias, and their other spellings in case give the same result, for every row and
every kind of declaration.

**REQ-ATTR-dsl-031** A known attribute before a declaration of a kind it stands
before, with a value that its row does not read, has the fault `Value`, at its
span with the row and the value as written; with a value the row reads it has
no fault; an attribute that is not allowed there has no `Value` fault.

**REQ-ATTR-dsl-032** An attribute that the same declaration holds earlier has
the fault `Repeated` and no other, at the later attribute with the span of the
first: the same name, a name in another case, a name and an alias, two aliases,
two user-defined names, and an attribute of a group and the same attribute
written on the declaration.

**REQ-ATTR-dsl-033** An attribute that is user-defined and not repeated has no
fault, whatever its value and the kind of declaration; a declaration without
attributes has none.

## What the analysis reports

One semantic rule (`rule_attribute`) walks the declarations of the library (the
census lists them with their kind and attributes), asks the check of the
registry for the faults of each and turns each fault into a diagnostic. It holds
no knowledge of any attribute: a row added to the registry is checked without a
change to the rule. The dialect does not change the check; an attribute pragma is
read only where pragmas are allowed (see [Dialects and conditional
text](#dialects-and-conditional-text)).

| Fault | Problem | The primary label | Other labels |
|---|---|---|---|
| Not allowed here | P4076 | at the attribute: names it as written, the kind of declaration it stands before, and the kinds its row stands before | none |
| Value | P4077 | at the attribute: names it, says what it takes and what was found (the value as written, or no value) | none |
| Repeated | P4078 | at the later attribute: names it | at the first attribute: where it was first written |

An attribute written before a group is attached to each declaration of the
group with one span, so the same fault is found once for each declaration. The
rule reports one diagnostic for each problem code and span of the attribute.

**REQ-ATTR-analyzer-010** A known attribute before a declaration of a kind its
row does not stand before is reported as P4076 at the span of the attribute,
with its name as written, the label of the kind of declaration and the kinds the
row stands before.

**REQ-ATTR-analyzer-011** A known attribute with a value its row does not take
is reported as P4077 at the span of the attribute, with its name, what it takes
and what was found.

**REQ-ATTR-analyzer-012** An attribute written twice before one declaration is
reported as P4078 at the span of the second, with a secondary label at the span
of the first.

**REQ-ATTR-analyzer-013** An attribute before a group that is attached to
several declarations gives one diagnostic for each problem code and span, not
one for each declaration.

**REQ-ATTR-analyzer-014** An attribute that is user-defined is not reported,
and neither is a known attribute where it may stand with a value it takes.

## The reference page

The reference page `docs/reference/language/attributes.rst` says what an attribute
is, what is reported, and lists the rows grouped by family. The list is not
written by hand: it is the file `docs/reference/language/attributes-table.csv`,
generated from the registry (`registry::table`) and read by the page. A test
fails when the file differs from the text the registry generates, and the
failure names the command that writes it. The page says once that the state
`Waiting` means the compiler reads and checks the attribute and does nothing
else with it yet; it does not call a row supported.

**REQ-ATTR-dsl-040** The table of the reference page is generated from the
registry, and the checked-in file is equal to the generated text.

**REQ-ATTR-dsl-041** The generated table has one line for each row, grouped by
family in the order of the families, with the name, the aliases, the kinds of
declaration the attribute stands before, the value, what it concerns in CODESYS
and the state.

## Rendering

The renderer writes the attributes of a declaration before the declaration, one
pragma per line, as `{attribute 'name'}` or `{attribute 'name' := 'value'}`,
with the name and the value as written. It writes each type declaration in a
`TYPE` block of its own and each variable in a variable block of its own, so the
attributes stand before the `TYPE` or the variable block keyword and attach to
the one declaration the block holds; `a, b : INT` is written as two variables,
each with the attributes.

**REQ-ATTR-plc2plc-001** The attributes of a declaration are rendered before it,
one pragma per line, in the forms above.

**REQ-ATTR-plc2plc-002** For every kind of target, parsing the rendered text
gives the same attributes on the same declarations.
