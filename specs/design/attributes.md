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
code is not part of this document, except for the attributes of the family
Layout (see [Layout](#layout)) and `strict` (see [Enumeration](#enumeration));
no other part of IronPLC acts on an attribute yet. The names the compiler knows, the places a known attribute may stand and
the values it takes are in scope (see [The registry](#the-registry)); an
attribute with no row is accepted and kept as written.

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

**REQ-ATTR-syntax-015** For every source, every attribute pragma of its tree
that describes a declaration is attached to a declaration or reported, never
both and never neither. An attribute pragma that marks the edge of a region is
neither (see [Region markers](#region-markers)).

**REQ-ATTR-syntax-016** The lowering of a statement list reports an attribute
pragma in the list as P0043 like the lowering of a file.

## Region markers

A row can say that its attribute marks the edge of a region of declarations
instead of describing the declaration it stands before (`Info::region`, set in
the declaration list by `region: true`). `naming` is the one such attribute:
`{attribute 'naming' := 'off'}` and `{attribute 'naming' := 'on'}` bracket the
declarations that CODESYS leaves out of the naming-convention check, and
`'omit'` leaves out the next line only. The closing pragma stands before
`END_VAR`, `END_STRUCT`, `END_TYPE` or another closing keyword, where no
declaration follows. IronPLC has no naming-convention check, so the row is
`Waiting`; the pragma is read, its value is checked, and it does nothing else.

Only `naming` is a region attribute, so there is no more general mechanism than
the one flag of the row; a second attribute of the kind is one more entry with
the flag. Attachment reads the flag and no name.

**REQ-ATTR-syntax-040** An attribute pragma of a row with `region` set may stand
anywhere an attribute pragma may stand, including before `END_VAR`,
`END_STRUCT`, `END_TYPE` and at the end of the file, and is not reported as
P0043, whether or not a declaration follows it.

**REQ-ATTR-syntax-041** An attribute pragma of a row with `region` set is not an
attribute of the declaration that follows it: the declaration carries the other
attributes before it and not this one.

**REQ-ATTR-syntax-042** The value of such a pragma is checked where it stands,
by the check of the registry (`region_fault`), and a value its row does not read
(or none) is reported as P4077 at the pragma, with the message the analysis
gives for the same fault of an attribute of a declaration.

**REQ-ATTR-syntax-043** The lowering of a file and of a statement list reports
the earlier in the source of a P0043 and a P4077 of a region marker.

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
a library of each kind with its attributes, and, for a function, a function
block, a program and a method, the variables it holds with the kind of block
each was written in.

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
- the kinds of variable block a declaration with the attribute **forbids**
  (none for most rows; `subsequent` forbids `VAR_TEMP`);
- the **family**, which groups the rows on the reference page by what the
  attribute concerns in CODESYS;
- the **summary**: one line saying what the attribute does in CODESYS;
- the **state**: `Waiting` when no part of IronPLC acts on the attribute yet,
  so the compiler reads and checks it and does nothing else with it;
  `Honoured` when a part of IronPLC acts on it. Every row is `Waiting`
  except `pack_mode`, `subsequent` and `strict`, which are `Honoured`.

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
| `RuleList` | `RuleNumbers(Vec<u32>)` | it is one or more items separated by commas, each a minus sign and decimal digits, with white space around an item ignored |

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

`registry::declared_faults(&declared)` is the one check. It takes a declaration
of the census (its kind, its attributes and the variables it holds) and returns
the faults, each with the name of the attribute as written and its span. It holds
no logic about a row: it asks the row where it may stand, whether it takes the
value and which blocks of variables it forbids. The part of the compiler that
reports a fault turns it into its own diagnostic and holds no logic about a row
either. About its own place and value, one attribute has at most one fault, found
in this order (`registry::faults(kind, &attributes)` finds these three):

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

Then, about what the declaration holds:

4. **Forbids block**: the attribute is known, stands where its row allows, and
   the declaration holds a variable written in a kind of block that its row
   forbids. The fault is at the first such variable and carries the span of the
   attribute. An attribute that is written again has this fault once, for the
   first.

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

**REQ-ATTR-dsl-034** A known attribute that stands where its row allows, before
a declaration that holds a variable in a kind of block the row forbids, has the
fault `ForbidsBlock`, at the first such variable, with the row, the kind of
block and the span of the attribute; a declaration that holds no such variable
has none; an attribute that is not allowed there has only `NotAllowedHere`; an
attribute written twice has the fault once.

**REQ-ATTR-dsl-050** The row `naming` is the only row with `region` set; it is in
the family Static analysis, is `Waiting`, and reads `off`, `on` and `omit` without
regard to case and to the white space around them.

**REQ-ATTR-dsl-051** `region_fault` gives the fault `Value` of an attribute of a
row with `region` set whose value the row does not read, at the span of the
attribute, and nothing for any other attribute; `Fault::diagnostic` is the one
turning of a fault into its diagnostic, for the analysis and for the lowering.

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
| Forbids block | P4079 | at the first variable of the forbidden block: names the keyword of the block and the attribute | at the attribute: where it is written |

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

**REQ-ATTR-analyzer-015** A declaration with an attribute and a variable in a
kind of block the attribute forbids is reported as P4079 at the first such
variable, with the keyword of the block and the name of the attribute, and a
secondary label at the span of the attribute; the same declaration without the
attribute, and the attribute without such a variable, are not reported.

## Layout

The family Layout holds the attributes that decide where the data of a
declaration stands in memory: `pack_mode` for the members of a structure and
`subsequent` for the variables of a program or a global variable list. Both rows
are `Honoured`. The decision that `pack_mode` describes the byte image and not
the storage of the virtual machine is
[ADR-0078](../adrs/0078-pack-mode-describes-the-byte-image-not-the-vm-slot-storage.md).

### The byte image and the storage

The **byte image** of a structure is the layout the CODESYS help describes: each
member at an offset, the structure with a size. `SIZEOF` and the byte offsets
describe it. The **storage** of a structure in the virtual machine is whole
8-byte slots, one or more for each member, and no attribute changes it. The code
generated for the members is the same with every value of `pack_mode`.

### The mode

`pack_mode` takes 0, 1, 2, 4 or 8; the mode 0 is the mode 1. The mode is an
upper bound on the alignment of the members of the structure or union that it
stands before:

- the offset of a member is the next multiple of the smaller of the alignment
  of the member and the mode after the end of the member before it;
- the alignment of the structure is the smaller of the largest alignment of its
  members (under the same bound) and the mode, and 1 when it has no member;
- a structure without `pack_mode` has no bound: each member is aligned to its
  own alignment. Whether the default of CODESYS without the attribute is a
  bound of 4 or of 8 is not known yet.

**REQ-ATTR-analyzer-020** The offsets of the members of a structure and of a
union follow the mode of the attribute `pack_mode` before it: for the structures
of the three examples of the CODESYS help, in each of the modes 0, 1, 2, 4 and
8, the offsets and the size are those of the help.

**REQ-ATTR-analyzer-021** The alignment of a structure is the smaller of the
largest alignment of its members and the mode; a structure without the
attribute has the largest alignment of its members.

**REQ-ATTR-analyzer-022** The offsets of the members of a structure, a union and
a function block are answered by one function of the members' types and the
bound on their alignment; a function block has no bound.

**REQ-ATTR-analyzer-023** `pack_mode` before an array type is accepted and has
no effect, and before a global variable list it is reported as P4076.

### The size

A structure has two sizes. Its **byte image size**, the size `SIZEOF` answers, is
the end of its last member, without padding to its alignment (the structure of
the third example of the help in the mode 8 is 28 bytes, not 32). Its
**stride** is the byte image size padded up to its alignment: it is the space an
element of an array occupies and the space a member that is a structure
occupies in the structure that holds it. The size of an array is its number of
elements times the stride of an element.

The padding at the end of the byte image is a provisional decision, to be
checked on a live CODESYS. It is made in one function (`Layout::byte_image_size`),
and the opposite decision is a change of that function alone.

**REQ-ATTR-codegen-001** `SIZEOF` of a variable of a structure type is the byte
image size of the structure, in each mode of `pack_mode`.

**REQ-ATTR-codegen-002** `SIZEOF` of an array of structures is the number of
elements times the stride of the structure, and `SIZEOF` of a structure with a
member that is a structure counts the stride of that member.

`SIZEOF` takes a variable; it does not take the name of a type.

### subsequent

`subsequent` asks for the variables of a program or of a global variable list to
be allocated one after another. The compiler allocates the variables of a
program and of a global variable list in declaration order, in consecutive
entries of the variable table, so the attribute is honoured without a change of
the allocation.

**REQ-ATTR-codegen-003** The variables of a program and of a global variable
list with the attribute `subsequent` have consecutive indices in the variable
table, in the order they are declared.

CODESYS reports a `VAR_TEMP` block in a program with the attribute as an error.
The registry row of `subsequent` forbids the block, and the analysis reports a
program that holds a `VAR_TEMP` variable and has the attribute as P4079
(REQ-ATTR-analyzer-015). A program accepts `VAR_TEMP` without the attribute:
its variables are working memory of one scan, laid out after the persistent
part and started again at every scan, so they are not among the variables the
attribute allocates.

Not done: starting a new memory area when the list changes, and the memory for
retained variables. The compiler has no such memory, so a variable of the list
qualified `RETAIN` is not stored apart from the other variables.

## Enumeration

The family Enumeration holds the attributes of an enumeration declaration:
`strict` and `to_string`. Only `strict` is `Honoured`. It decides whether an enumeration
is a number to the places that compute with one or store a number into one.
Whether the attribute is read at all is a behavior policy
([behavior-policies.md](behavior-policies.md#second-application-which-enumerations-are-strict),
[ADR-0079](../adrs/0079-a-policy-may-select-what-the-analysis-accepts.md)).

### Strict and not strict

An enumeration is **strict** when its declaration carries `{attribute 'strict'}`
and the policy `with-attribute` is selected (`codesys`, `twincat`), or when the
policy is `every-enumeration` (the default, and every IEC 61131-3 dialect):
IEC 61131-3 gives the values of an enumeration no arithmetic. An enumeration
that is not strict is its **base type** (`INT` unless the declaration names
another, `(A, B) BYTE`) in every place of the class below. The attribute is read
at one place, where the type of the enumeration is resolved, and the answer is a
field of the type (`IntermediateType::Enumeration::strict`), so an alias of the
enumeration, a variable of it and a member of a structure see it. Every check
asks the type (`IntermediateType::is_strict_enumeration`), never the attribute
or the policy.

The class is every place that treats an enumeration differently from its base
type:

- an arithmetic operator and the function form of one (P4049), a numerical
  standard function and the time overloads that take a number (P4026), and the
  control variable of `FOR` (P4049): these *compute* from the value
  (`ValueUse::Computes`);
- a selection function (`MIN`, `MAX`, `LIMIT`, `SEL`, `MUX`) whose inputs are
  not all one strict enumeration returns a number, and a number stored into a
  strict enumeration is refused (P4027 where the value is the result of a
  call, P4035 for an assignment, P4026 for an argument);
- a value of an integer type, or of another enumeration, stored into a strict
  enumeration is refused as any other number is, and an integer constant that
  names none of its values is refused (P2006).

Comparison (`x = 2`, `x = z`), the conversion of an enumeration to an integer,
and an enumeration where an `INT` is expected are not in the class: they accept
the enumeration whether it is strict or not.

An enumeration that is not strict is its base type there: arithmetic on it is
accepted and has the type of the base type (`operand_type_name` knows it by the
name of the base type), a number is stored into it as into a variable of the
base type, the numerical functions, `FOR` and the selections accept it, and an
integer constant that names none of its values is accepted. An enumeration
written inline in a variable declaration (`w : (U, V)`) has no declaration to
carry the attribute, so under `with-attribute` it is not strict; an alias
(`TYPE E3 : E1`) is the enumeration it names, and an attribute before the alias
declaration is accepted and has no effect.

**REQ-ATTR-analyzer-030** An enumeration is strict when the policy
`policy_enumeration_strictness` is `every-enumeration`, or is `with-attribute`
and the declaration of the enumeration carries the attribute `strict`.

**REQ-ATTR-analyzer-031** A strict enumeration is refused as an operand of an
arithmetic operator or of its function form, as the input of a numerical
function and as the control variable of `FOR`.

**REQ-ATTR-analyzer-032** A value of an integer type, a value of another
enumeration, the result of a selection that is not one strict enumeration and
an integer constant that names none of its values are refused where a strict
enumeration is stored.

**REQ-ATTR-analyzer-033** An enumeration that is not strict is accepted in every
place of REQ-ATTR-analyzer-031 and REQ-ATTR-analyzer-032, as its base type is.

**REQ-ATTR-analyzer-034** An alias of an enumeration is strict as the
enumeration it names is, and an enumeration written inline in a variable
declaration is strict only by the policy.

### Code generation

An enumeration that is not strict is held as its base type in a variable, a
parameter and a member of a structure, so that a program the analysis accepts
because of it runs as the same program on the base type does. A strict
enumeration is held as before (a `DINT` in a variable, the underlying type in a
member).

**REQ-ATTR-codegen-010** A program that uses an enumeration that is not strict
as an operand of arithmetic, as a numerical function input, as the control
variable of `FOR` and as the target of a selection, an integer or a constant
that names no value runs and leaves the numbers of its base type.

**REQ-ATTR-codegen-011** A number stored into an enumeration that is not strict
in a variable or a member of a structure is held at the width of its base type
(a sum wraps as it does in a variable of the base type).

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
