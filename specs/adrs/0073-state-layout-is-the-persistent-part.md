# State Layout Is the Persistent Part

status: proposed
date: 2026-10-08

## Context and Problem Statement

An online change is accepted when the running application and the candidate
agree on the layout of their state; the code may differ. Today "the layout" is
the whole variable table. The container holds, in one flat table and in this
order: the global variables, the variables of the program, the parameters,
locals and return value of every compiled function, and the working slots of
function block bodies and methods. Code generation compiles only the functions
that a body reaches, so the first call of a user function adds slots to the
table and the removal of the last call takes them away.

Three comparisons then refuse the edit, each over the whole table: the layout
hash (it covers the count of variables and every entry), the count of
variables in the header, and the variable count of the program entry in the
task table. None asks whether a persistent value is affected, and none is: a
function sets its locals and its return value at every call, and a function
block copies its fields in from the data region at a call and out at the
return. Nothing outside the globals, the program variables and the instance
data is read after the scan that wrote it. The check is stricter than the copy
it guards.

With stable variable IDs the same edit is accepted as a migration, which has no
untest. With a function block instance in the program it is refused as
`FbLayoutUnsupported`, because the planner reads the position of a function
block's working slots (`var_offset`) as part of the identity of the block.

The design note [State Layout and Body Edits](../design/state-layout-and-body-edits.md)
compares the options. This record states the decision.

## Decision Drivers

- **A body edit is an ordinary online change.** Accepted, testable, revertible,
  every persistent value kept, with and without stable variable IDs.
- **A changed declaration of persistent state stays a changed layout.** Adding,
  removing or retyping a global or program variable, changing an array bound,
  adding or retyping a function block field.
- **One definition of what is persistent and one of layout identity.** The load
  check, the host, the swap, the migration planner and the redundancy snapshot
  all read them; none recomputes either.
- **No change to the VM.** Dispatch, calls, frames and variable addressing stay
  as they are.

## Considered Options

1. **The layout is the persistent part; working slots stay in the table after
   it.**
2. **Working slots leave the variable table.**
3. **Compile every declared function.**
4. **The host compares the persistent parts of the two containers it holds, and
   the hash stays over the whole table.**

## Decision Outcome

Chosen option: **the layout is the persistent part**, because it is the only
option that makes the decision once, in the format, where every reader finds
it, and that leaves the VM alone.

### What is persistent

The persistent part is the global variables, the variables of the program, the
arrays they use, and the fields of function blocks. The parameters, locals and
return value of functions and the working slots of function block bodies and
methods stay in the variable table, after the persistent part, and are outside
the layout.

### The persistent extent

The type section declares the persistent part as data: one row per program
instance, naming the instance and two runs.

| Field | Meaning |
|---|---|
| `instance_id` | The program instance that owns the row |
| `var_start`, `var_count` | The run of the variable table that is persistent |
| `data_start`, `data_len` | The run of the data region that is persistent |

Code generation records a row at the one point where it has assigned the last
persistent variable: the variable count and the data region offset at that
moment are the extent. The data region before that point holds every string,
array, structure and function block instance of the globals and the program;
everything allocated afterwards (function locals, function block working
slots, string temporaries) is working memory. A container that declares no row
has no working slots, and its extent is the whole table and the whole data
region; that default is applied in the one accessor that returns the rows
(`Container::persistent_extents`), nowhere else.

A later second program instance is one more row. Nothing in the format or in
its readers assumes one.

### The function block type table

The type section already declares a function block type table (`fb_types`: a
type ID and the type and extra of each field) that nothing filled. Code
generation now writes one descriptor for every user function block. It is the
field layout of the type, read from there by the hash, the load check and the
migration planner. The position of a block's working slots (`var_offset` of the
user function block descriptor) is an address for the VM and is no longer read
as identity.

### What the layout hash covers

`compute_layout_hash` is BLAKE3 over, in this order:

1. the number of extent rows, then each row's `var_start`, `var_count`,
   `data_start` and `data_len`;
2. for each row, the variable table entries in the row's run, in table order:
   the type tag and flags, then for an array variable the array descriptor it
   names (element type, total elements, element extra and element stride; not
   the descriptor's index), and for any other variable the entry's `extra`;
3. the function block type table, in ascending type ID order: the type ID, the
   number of fields, and the type tag and extra of each field.

The hash does not cover the count of variables, the entries outside the extent,
array descriptors that no persistent variable names, the user function block
descriptors (function IDs and `var_offset`), the stable variable IDs, the code,
the constants and the debug section.

### Format version 8

The type section gains a seventh sub-table, the extent rows, after the function
block field UIDs. The layout hash changes its definition. The format version
goes from 7 to 8. A container of version 7 is refused when its header is read,
by the existing unsupported-version error; it is never read with the old
meaning.

### Consequences for the readers

- **Load check.** Each row lies inside the variable table and the data region,
  rows do not overlap, each row names an instance of the task table, and each
  user function block descriptor agrees with the field count of its type
  descriptor. The layout hash is recomputed over the definition above.
- **Host gates.** A candidate is a plain swap when its layout hash and its
  extents equal the active container's. The count of variables in the header
  and the variable count of the program entry are no longer compared; they
  describe the whole table, which an edit of a body may change. A candidate
  with a different hash is a migration candidate or is refused, as before.
- **Swap.** The swap copies the extent from the old buffers to the new ones.
  The working part of the new buffers starts fresh, which is what a function or
  a function block body assumes at every call.
- **Migration planner.** The field layout of a function block comes from the
  type descriptor. Two containers lay a block out alike when their descriptors
  list the same fields; a different `var_offset` or function ID no longer
  makes them differ.
- **Redundancy snapshot.** The snapshot carries the persistent runs only and
  identifies its layout by the layout hash and by the counts of persistent
  variables and persistent data bytes. A replica applies exactly those runs,
  whatever the size of the working part on either side.

### Consequence for the declaration checksum of the analysis

The analysis may later keep a checksum of declarations to decide what to
recompute. That checksum must be partitioned by storage class, persistent
against working, to stay one definition with the layout: a change of the locals
of a function is a changed declaration for the analysis and an unchanged layout
for the runtime. A checksum over all declarations without the partition would
be a second definition of layout identity. This record states the requirement
and implements nothing of it.

### Consequences

- Good, because the first call of a user function and the removal of the last
  one are accepted as an ordinary online change, in a program body and in a
  function block body, with test and untest and with or without stable IDs.
- Good, because the decision is made once and every reader derives from it.
- Good, because the VM does not change.
- Bad, because it is a format break: a version 7 container is refused.
- Bad, because a change to the declarations of a function (a local added or
  retyped) is not a changed layout. That is correct for the state and means the
  layout hash is not an identity of the whole program's declarations.
- Neutral: the working slots stay in the variable table, so the table is still
  the address space of the VM and still holds an entry for every slot.

### Confirmation

- `compiler/codegen/tests/it/layout_hash.rs` holds the table of edit classes:
  body edits that leave the layout unchanged (including the first and the last
  call of a user function, in a program body and in a function block body, and
  an edit that adds a dependency between declarations) and declaration edits
  that change it (including a function block field retyped).
- `compiler/runtime/tests/body_edit_swap.rs` swaps each of those edits in,
  with and without stable variable IDs, and checks that the persistent values
  survive the swap and the untest.
- `compiler/container` tests cover the extent rows, the hash and the load
  check.

## Pros and Cons of the Options

### The layout is the persistent part

- Good, because the extent is declared data that every reader shares.
- Good, because no change to the VM is needed.
- Bad, because of the format version.

### Working slots leave the variable table

- Good, because the table would hold persistent state only.
- Bad, because it changes the VM (a buffer of its own, or addresses relative
  to the frame) and reaches the debugger, for the same behaviour the user sees.

### Compile every declared function

- Good, because it is one filter removed.
- Bad, because unused library code is loaded on the controller, a construct
  that code generation does not support fails the build where today it is
  skipped, and adding a function nothing calls is still refused.

### The host compares the persistent parts and the hash stays over the whole table

- Good, because it is small.
- Bad, because two definitions of the layout exist and can disagree: the hash
  and the host's comparison. The load check, the redundancy snapshot and the
  planner would each need a third.

## More Information

- [State Layout and Body Edits](../design/state-layout-and-body-edits.md)
- [Bytecode Container Format](../design/bytecode-container-format.md), "Layout
  Hash and Online Change"
- [ADR-0069](0069-explicit-element-stride-in-array-descriptors.md): the element
  stride is part of the array descriptor and therefore of the layout.
