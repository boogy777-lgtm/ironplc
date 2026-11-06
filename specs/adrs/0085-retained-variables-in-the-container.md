# Retained Variables Are Declared in the Container

status: accepted
date: 2026-11-06

## Context and Problem Statement

The language carries `RETAIN`, `NON_RETAIN` and `PERSISTENT`
(`DeclarationQualifier` in `compiler/dsl/src/common/variable.rs`), the parser
accepts them on globals and on program variables, and code generation ignores
them: a retained variable is laid out like any other, and the container says
nothing about it. A device that is to keep the value of a retained variable
across a restart has two ways to learn which ones they are: read the source (it
has none), or walk the layout and guess. The device will later save the retained
values to a file and apply them at boot (the stages that follow this record). Both
need the same answer to one question, which slots and which bytes hold a retained
value and of which kind, and they must not compute it each on their own: that is
how the persistent part was a computation in four places before
[ADR-0073](0073-state-layout-is-the-persistent-part.md) wrote it down.

The reference is CODESYS, in the behaviour it shows the user:

- A variable declared `RETAIN` keeps its value across a restart (power off, a warm
  restart) and not across a download.
- A variable declared `PERSISTENT` keeps its value across a restart and, when its
  declaration is equal, across a download.
- A function block instance declared `RETAIN` is stored in retain memory with all
  its data. An instance declared without a qualifier whose type declares a `RETAIN`
  variable is stored in retain memory too, and only that variable is treated as
  retained.
- The locals of a function or a method are working memory and are not state;
  a qualifier on them has no effect.

The glossary already states the vocabulary: a
[retained variable](../steering/glossary.md#retained-variable) is about a restart
and the [persistent part](../steering/glossary.md#persistent-part) is about an
online change, and the second contains variables of every qualifier.

## Decision Drivers

- **One declaration, one reader.** The runtime and the device read the same rows
  through one function; none walks the layout.
- **The container is the contract.** Everything the device needs to know about
  retention is in the file it already holds. Nothing is looked up in the source.
- **A qualifier is not a layout.** Changing `RETAIN` to `NON_RETAIN` moves no
  storage, so it is not a layout change for an online change.
- **Every retained byte exactly once.** No byte is covered by two rows, and none
  is retained that the persistent part does not hold.
- **A new kind is one case.** A kind that CODESYS has and this record does not
  (the combination `RETAIN PERSISTENT`, a table of forced variables) is one enum
  variant and its mapping in code generation.

## Considered Options

1. **Runs of the variable table and of the data region, one row type with a space
   (chosen).**
2. **One row per variable.** A row names a variable and its kind, and the reader
   expands it.
3. **One row type that holds a run of slots and a run of bytes together**, like
   the persistent extent row.
4. **A flag in the variable table entry** (`VarEntry.flags`) and nothing for the
   data region.
5. **Nothing in the container;** the device derives retention from the debug
   section.

## Decision Outcome

Chosen option: **runs, in a sub-table of the type section**, because a run is the
unit a device copies, it expresses a field of an instance (which no variable table
entry names), and one reader function answers the question for every consumer.

### D1. Kinds

`RetainKind` has two values, one byte on disk: `Retain` (1) and `Persistent` (2).
Both survive a restart and both are saved at one. Only `Persistent` is also kept
across a download; that rule belongs to the download and to a later stage, not to
this declaration. A kind that CODESYS has beyond these two is one more value of
`RetainKind` and one arm in `retain_kind` (`compiler/codegen/src/retained.rs`),
which is an exhaustive `match` over `DeclarationQualifier`, so a new qualifier
does not compile until it decides whether it retains.

### D2. Granularity: a row is a run of one space

The sub-table holds `RetainedRun` rows. A row is a kind, a space and a run in it.

| Field | Type | Meaning |
|---|---|---|
| `kind` | u8 | 1 = `Retain`, 2 = `Persistent` |
| `space` | u8 | 0 = slots of the variable table, 1 = bytes of the data region |
| `start` | u32 LE | first slot or byte |
| `len` | u32 LE | number of slots or bytes |

A row is 10 bytes. Two row types, or one row with a run in each space as the
persistent extent has, were the other shapes. This record takes one row with one
run and a space byte, because:

- A slot and a byte are in different address spaces, and nothing relates the run
  of slots of a variable to the run of its bytes except the declaration. A row with
  both runs would have to repeat that pairing as a pairing of two ranges, and
  merging adjacent rows would then need both runs adjacent at once; two runs that
  are adjacent in one space and not in the other would stay two rows for no reason
  a reader can see. With one run per row the merge is one comparison, and the
  result is canonical.
- A field of a function block instance has bytes and no slot of its own in the
  variable table (the instance has one slot, which holds its address). A row with
  both runs would carry an empty run on the slot side for every such field; a row
  with one run carries no empty run at all.
- The load check is one function over rows: the run lies inside the persistent
  extent of its space; rows ascend by space and start; none overlap. The
  persistent extent check has two axes and four fields; this has one axis.
- The reader returns rows. A consumer that wants the slots takes the rows of space
  `Slots`; it does not take apart a pair.

The cost is that a variable that owns a slot and bytes (a string, an array, a
structure, a function block instance) is two rows. The writer sorts the rows by
space and start, drops an empty run, and merges adjacent rows of one kind, so the
bytes of the container do not depend on the order in which code generation
declared them (`ContainerBuilder::build`, `canonical_runs`). Rows that overlap are
not merged; they stay as given, so that the load check names the defect of the
producer.

### D3. Scope rules

Code generation records the rows at the one point where it assigns the storage of
a declaration of the persistent part: `assign_persistent_variables` assigns the
declarations of the globals and of the program one at a time and passes each to
`retained::record_declaration` with the run of the data region it reserved. The
working memory of the program (its `VAR_TEMP`), of functions, of methods and of
function block bodies is assigned by other paths and never offered to it.

| Declaration | Rows |
|---|---|
| `VAR_GLOBAL RETAIN` or `PERSISTENT`; program `VAR RETAIN` or `PERSISTENT`: a scalar | its slot |
| the same, a string, an array, a structure or a function block instance (or an array of instances) | its slot, and every byte it reserved in the data region |
| an instance (or an array of instances) of a function block, with no qualifier, whose type declares fields `RETAIN` or `PERSISTENT` | for each instance, the slot of each such field inside the instance data, and for a string field its characters; no slot of the variable table |
| `NON_RETAIN` on anything, including an instance of a type that declares retained fields | none |
| no qualifier, anything else | none |
| a local of a function or a method, with `RETAIN` | none; working memory is not state |
| `VAR_TEMP` of a program or a function block | none |

A function block instance declared `RETAIN` is therefore all of its data, and an
unqualified instance is only the retained fields of its type, which is what
CODESYS treats as retained while keeping the whole instance in retain memory. The
fields of the type are read from the layout of the instance
(`FbLayout::retained`, filled by `layout_instance`, the same routine that places
the slots and the runs of the strings), so the position of a field is never
computed in a second place.

`NON_RETAIN` on the declaration of an instance wins over the qualifiers of the
fields of its type. The qualifier of the declaration is the one the user wrote
last and the one that covers the whole instance, so it decides.

A `RETAIN` local of a function is a syntax error in the parser. One of a method
parses, and a `RETAIN` local of a function can reach code generation from a
PLCopen XML variable list marked `retain`. None of them produces a row (tests in
`compiler/codegen/tests/it/retained_runs.rs`).

### D4. Hashing

The retained rows are not part of `layout_hash`. A qualifier moves no storage,
so changing one is not a layout change and an online change is not refused for
it. The rows are in the type section and so in the content hash like every
sub-table; a change of qualifier changes the content hash.

No `retained_layout_hash` is added to the header. The header stays as it is,
except for the version. The file that stage 2c writes has to say which container
it belongs to; it will tag the file with the content hash and with a hash
computed from the retained rows by the one reader (`Container::retained_runs`),
and nothing outside that function computes either. The header can grow a field
when a reader needs one before it can read the type section, and none does.

### D5. Format version 9

The type section gains an eighth sub-table, the retained rows, after the persistent
extent rows. It is always written; its count is 0 when nothing is retained, and a
type section that ends before it reads as no rows (the rule of the later
sub-tables). A container that declares no row retains nothing: unlike the
persistent extent there is no default that stands for the whole table. The format
version goes from 8 to 9; a container of version 8 is refused by the existing
unsupported-version error and is never read as one that retains nothing.

The kind and the space bytes are decoded when the type section is read, and an
undefined byte is refused with `InvalidRetainKind` or `InvalidRetainSpace`, as an
undefined field type or task type is. The row type therefore cannot hold an
undefined kind. The load check (`verify_load`) refuses the other defects:
`RetainedRunOutOfExtent` for a run that is not inside one run of the persistent
extent in its space, and `RetainedRunsOutOfOrder` for rows that are not ascending
by space and start or that overlap.

The two golden containers of the command line tests
(`compiler/vm-cli/resources/test/steel_thread.iplc` and
`debug_source_file_table.iplc`) are regenerated.

### D6. The version error names both versions

`ContainerError::UnsupportedVersion` carried no data, so a user holding a file
of another version was told only that the version was unsupported. It now carries
the version found and the version supported, and reads `unsupported container
format version 8: this reader supports version 9`. The text of `ContainerError`
is the `Display` of the error type of a `no_std` crate; it is not a row of
`problem-texts.csv` and has no problem code, and no page under
`docs/compiler/problems/` quotes it, so no page changes. The consumers print it
through `Display` (the disassembler reports `Failed to parse container: <text>`).

### Consequences

- Good, because the runtime and the device read one declaration through
  `Container::retained_runs` and never walk the layout.
- Good, because a field of an instance that is retained inside an unqualified
  instance is expressible, which a per-variable declaration is not.
- Good, because the output is canonical: one declaration, one byte stream.
- Good, because a qualifier edit is not a layout change.
- Good, because a new kind or a new qualifier is one enum variant and one arm of
  an exhaustive `match`.
- Bad, because it is a format break: a version 8 container is refused.
- Bad, because a string, an array, a structure or an instance is two rows where
  a pairing would be one.
- Bad, because the tag of the retained file is not in the header and the file's
  writer has to hash the rows (D4).
- Neutral: the VM ignores the rows, like the persistent extent and the stable
  variable IDs.

### Not included

- **The device file.** Saving the retained values to a file, its tag and its
  format are stage 2b and 2c.
- **Applying the retained values at boot.** The runtime reads the rows and copies
  the values in; nothing reads them yet.
- **The download keep rule.** Which retained values a download keeps (`Persistent`
  with an equal declaration) is a later stage. The kind is recorded here so that
  stage has it.
- **An analyzer rule for the locals of functions and methods.** `RETAIN` on such
  a local has no effect and is not reported. The parser already refuses it in
  source text; a rule for the XML route is not added.
- **Retained fields of nested instances.** Code generation does not lay out the
  instance a function block field names (such a field is one slot, and calling
  it is not implemented), so a retained field of a nested instance has no storage
  to describe. When nested instances are laid out, `layout_instance` adds the
  runs of the nested type to the outer layout, and the rows follow.
- **`RETAIN PERSISTENT` as a kind of its own** and the forced-variable table.

## Pros and Cons of the Options

### Runs, one row type with a space

- Good, because rows are in one address space each, so merge, order and the load
  check are one comparison.
- Good, because a field of an instance needs no empty slot run.
- Bad, because a variable with a slot and bytes is two rows.

### One row per variable

- Good, because a row reads like a declaration.
- Bad, because it cannot name the retained field of an unqualified instance (the
  field is not a variable of the table), and a device that copies a whole array
  would copy it one row at a time.

### One row type with a run of slots and a run of bytes

- Good, because a variable is one row, and the shape is the persistent extent's.
- Bad, because the pairing is not meaningful across spaces, merging needs both
  runs adjacent, and a field of an instance carries an empty slot run.

### A flag in the variable table entry

- Good, because no sub-table is added.
- Bad, because the bytes of a string, an array or an instance are not in the
  table at all, and the load check reserves every flag bit but the array bit, so
  a new flag is a format change of its own.

### Nothing in the container

- Good, because the format does not change.
- Bad, because every consumer has to derive retention from the debug section,
  which a stripped container does not carry, and from the layout, which is the
  second computation this record exists to prevent.

## Confirmation

- `compiler/codegen/tests/it/retained_runs.rs` compiles small programs: a global
  `RETAIN` scalar, a global `PERSISTENT`, a program `PERSISTENT` string, a
  `RETAIN` array, a `RETAIN` instance (the whole instance), an unqualified
  instance of a block with `RETAIN` and `PERSISTENT` fields (only those fields,
  in each instance of an array too), a `RETAIN` string field, a `NON_RETAIN`
  instance, a `RETAIN` local of a function and of a method, a `VAR_TEMP`, and a
  qualifier that changes while the layout hash does not.
- `compiler/container` tests cover the row encoding, the canonical order and merge,
  the section size and the load check (one test for each defect).
- The requirements `REQ-CF-container-040` to `044` and `REQ-CF-codegen-029` in
  [Bytecode Container Format](../design/bytecode-container-format.md) each have a
  conformance test.
- `compiler/project/src/disassemble/tests.rs` covers the `retainedRuns` array of
  the disassembly.

## More Information

- [Bytecode Container Format](../design/bytecode-container-format.md), "Retained
  Runs"
- [ADR-0073](0073-state-layout-is-the-persistent-part.md): the persistent part and
  the persistent extent that every retained row lies inside.
- [Glossary](../steering/glossary.md): "Retained variable" and "Persistent part".
