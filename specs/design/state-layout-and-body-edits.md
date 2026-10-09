# Design: State Layout and Body Edits

status: partially implemented
date: 2026-10-08

## Overview

An edit of the body of a program must be an ordinary online change: accepted,
testable, revertible, with every value kept. Before the change the first call
of a user function, and the removal of the last call, was refused. This note
states why, what the options were, and which one was chosen.

**The owner decided option O1** (see [Options](#options)), recorded in
[ADR-0073](../adrs/0073-state-layout-is-the-persistent-part.md). This note is
the research the decision was made from, followed by what was implemented and
what was found ([What Was Implemented](#what-was-implemented)). An agent read
the code path of an online change end to end, the decompiled CODESYS compiler,
and the design documents; nothing was built or run for it. The sections up to
[Recommendation](#recommendation) describe the code before the change, with the
line numbers of that time. Paths are under `compiler/`.

## What Happened Before the Change

### The path of an online change

1. The workstation compiles the whole project to one container and sends its
   bytes (`runtime/src/commands.rs:406-430`).
2. The runtime host reads the container, which verifies the content hash and
   recomputes the layout hash (`container/src/load_verify.rs:289-292`).
3. The host decides what the candidate is (`runtime/src/host.rs:265-278`):
   - layout hash equal: `validate_candidate`, then a swap of code;
   - layout hash different and both sides carry stable variable IDs: a
     migration by ID;
   - otherwise: refused, `LayoutIncompatible` (V4007).
4. Test and untest record a pending swap; the swap happens before the next
   scan (`host.rs:629-631`, `:708-736`).
5. Assemble makes the candidate the normal container and drops the old one
   (`host.rs:348-372`).

### What exists in two copies

The host holds two containers while a candidate is staged, the normal one and
the candidate (`host.rs:151-167`): code, constants, type section, task table.
**It holds one set of data buffers.** The candidate gets no buffers until the
swap. At the swap a new buffer set is built from the destination container,
the values are copied into it, and the old set is dropped
(`runtime/src/online_change.rs:168-184`).

So "the controller holds two copies while the new one is loaded" is true of
the program and is not true of the data.

### What the swap does

`swap_buffers` copies `vars[..min]` and `data_region[..min]` from the old
buffers to the new ones by position, and leaves the stack, the frames and the
temporary buffers fresh (`online_change.rs:168-184`). It accepts buffers of
different sizes. An untest calls the same function with the normal container.

### Where the variables of a function live

One flat variable table holds, in this order (`codegen/src/compile.rs:941`,
`:967`, `:971-997`, `:999-1040`):

1. global variables;
2. the variables of the program;
3. the parameters, locals and return value of every compiled function;
4. the working slots of function block bodies and of methods.

Parts 1 and 2 are the persistent state. Their extent is already recorded in
the task table as `shared_globals_size` (`compile.rs:1219`). Parts 3 and 4 are
working memory: a function sets its locals and its return value at every call
(`codegen/src/compile_fn.rs:327-339`), and a function block copies its fields
in from the data region at a call and out at the return
(`vm/src/vm.rs:2561-2573`, `:2971`). Nothing in parts 3 and 4 is read after
the scan that wrote it.

Code generation compiles only the functions that some body reaches
(`compile.rs:336`, `:350`). The first call of a function adds its slots to
part 3; the removal of the last call takes them away. Parts 1 and 2 do not
move.

## Why a First Call Is Refused

Three comparisons refuse it, and each is made over the whole table:

| Comparison | Where |
|---|---|
| layout hash, which covers the count of variables and every entry of the table | `container/src/container.rs:39-68`, `host.rs:265` |
| count of variables | `online_change.rs:31` |
| count of variables of the program, which is the count of the whole table | `online_change.rs:153`, `container/src/builder.rs:432` |

None of them asks whether a persistent value is affected, and none is. The
swap would carry the values correctly. **The check is stricter than the copy.**

With stable variable IDs the same edit is accepted as a migration, and a
migration cannot be untested (`host.rs:330`). With a function block instance
in the program it is refused as `FbLayoutUnsupported`: the function slots
shift the working slots of the function block, and the migration planner reads
that position as part of the identity of the function block
(`runtime/src/migration/fb.rs:70`, `:99-104`, `:293-302`).

## What the References Do

**CODESYS** (decompiled compiler, 3.5.22.10). A variable has a storage class:
relative to the stack for the variables of functions and methods, relative to
the instance for the members of function blocks, absolute for globals
(`SignatureChangesInspector.cs:166-195`). An added program unit or an added
variable does not refuse an online change; it selects the path that copies
values (`DetailedIsUpToDateStrategy.cs:62-79`, `:597-608`). The identity of
the data changes when an interface changes, and the identity of the code when
code changes (`CompilerPhase5_Codegenerator.cs:664-694`). What the controller
holds during a download was not found in these sources.

**Rockwell Logix** (Logix 5000 Controllers Design Considerations, publication
1756-RM094N-EN-P, September 2025; the table "Comparison of Partial
Import/Export and Add-On Instructions"). An import into a running controller
adds programs, routines and Add-On Instructions, replaces existing programs
and routines, and creates tags and data types; "the data values in the
controller are maintained and new tags have their values initialized from the
import file". The definition of an existing Add-On Instruction is edited
offline only, and such an edit keeps the values of members that are inserted,
deleted, moved, renamed or retyped, but not of a member renamed and moved in
one operation: values follow the identity of the member. Parameters of a
subroutine are passed by value and local tags belong to the program, so logic
owns no state of its own. Where the controller keeps temporaries, and whether
it holds data in two copies during an edit, the manual does not say.

Both references agree on the behaviour this note proposes: new code on a
running controller keeps every value, and a change of the shape of existing
data is a separate, stricter case.

## Options

| | What changes | Size | What the engineer sees |
|---|---|---|---|
| **O1** The layout is the persistent part: the hash and the three comparisons cover globals, program variables, the arrays they use and the fields of function blocks. Working slots stay in the table, after the persistent part | The hash function in one place; code generation fills the function block type table, which the format declares and nothing fills; the three comparisons and the planner read the persistent extent. Format version 7 to 8. No change of the VM | medium | A call added or removed is an ordinary online change with test and untest. A changed declaration of a persistent variable is as today |
| **O2** Working slots leave the variable table: a buffer of their own, or addresses relative to the frame | As O1, and the VM. The second form reverses the decision of one flat table and reaches the debugger | medium to large | The same as O1 |
| **O3** Compile every declared function | One filter removed | small | A call added is accepted. A function added, even one nothing calls, is refused. Unused library code is loaded on the controller, and a construct that code generation does not support fails the build where today it is skipped |
| **O4a** The host compares the persistent parts of the two containers it holds; the hash stays over the whole table | One new comparison in the host | small | The same as O1. Two definitions of "the layout" exist and can disagree |
| **O4b** Every candidate with a different hash goes through a migration, and the old buffers are kept for untest | Stable IDs for every persistent variable; a reverse migration | large | A call added is accepted. A variable without an ID returns to its initial value |
| **O4c** The buffers of the candidate are built at accept and both sets are held | Allocation at accept | small | Nothing changes: the refusal stays, because the comparison is unchanged |

O4 is the direction "change where the candidate is held, not what the layout
is". The candidate is already in memory at accept, which is why O4a is
possible at all. O4a and O1 make the same decision; O1 makes it once, in the
format, where the load check, the host, the redundancy snapshot and the
planner all read it. O4c shows that holding more copies does not remove the
refusal: the refusal is a comparison and not a lack of storage.

## Recommendation

O1. It gives the behaviour of an ordinary online change with no change of the
VM, on an extent the task table already holds, and it removes the migration
without untest for this edit because the hash becomes equal. It costs one
format version.

O1 separates state by storage class, persistent against working. A change of
the locals of a function is then a changed declaration for the analysis and an
unchanged layout for the runtime. The split "declaration changed" against
"body changed" stays one definition only if the declaration checksum of the
analysis is also partitioned by storage class, as CODESYS partitions it.

## Before Implementation

These were the open items when the decision was made. Each is answered in
[What Was Implemented](#what-was-implemented).

1. The data region has no persistent extent, only a total size. Define it.
2. The fields of a function block are covered by the hash only through the
   working slots. Move them to the function block type table, and have the
   planner and the load check read them there.
3. The order of functions and function blocks comes from the dependency sort
   and not from their names, so a body edit that adds a dependency may reorder
   them. Add a guard row; if the order moves, give the type ID of a function
   block a stable key.
4. A project of more than one program needs an extent per program.
5. A migration returns a persistent variable without an ID to its initial
   value (`runtime/src/migration.rs:331`, `host.rs:753-758`). Decide whether
   that is refused.
6. `copy_persistent` copies the data region with a function that requires
   equal lengths (`host.rs:565`); in the redundancy untest path the two
   lengths can differ. Not run.
7. The format document disagrees with the code on the version (6 against 7),
   on whether the hash covers the element stride, and on the order rule.

Items 5 and 6 were found beside the question and are defects on their own.

## What Was Implemented

Option O1, on format version 8. The VM did not change.

1. **The persistent extent is data of the container.** The type section has a
   seventh sub-table, one row per program instance, with a run of the variable
   table and a run of the data region
   ([Persistent Extent](bytecode-container-format.md#persistent-extent)). Code
   generation writes the row at the one point where it has assigned the last
   persistent variable; `Container::persistent_extents` is the one accessor.
   A container that declares no row has the whole tables as its extent. A
   second program instance is one more row; the readers assume no count, and
   code generation still takes one program.
2. **The function block type table is filled.** Code generation writes one
   descriptor per user function block, from the variable table entries of the
   block's field slots. The hash, the load check and the migration planner read
   the fields there; the planner no longer reads the position of the working
   slots as identity. It reads that position only for a debug name in a
   decision prompt.
3. **The hash and the gates.** The layout hash covers the extent, the variables
   in it, the arrays they name (by content, including the element stride) and
   the function block type table
   ([Layout Hash and Online Change](bytecode-container-format.md#layout-hash-and-online-change)).
   The host compares the hash and the extent. The count of variables in the
   header and the variable count of the program entry are no longer compared.
   The swap carries the extent and starts the working part fresh. The
   redundancy snapshot carries the extent only.
4. **The order channel.** A guard row for a body that starts to call a function
   declared before it found one: the type ID of a function block was its
   position in the container, which comes from the dependency sort, and the new
   edge moved two other blocks and swapped their IDs. The type ID is now the
   rank of the block's name among the compiled blocks. The IDs of functions are
   positions still, and no hash reads them.
5. **The guard.** One table of edit classes,
   `ironplc_test::edit_classes`, drives the layout guard of code generation and
   the swap guard of the runtime, with and without stable variable IDs.
6. **The document of the format** is true to the code (item 7 above): the
   version, the hash formula including the element stride, and the order rules.

Item 4 of the list above (more than one program) is not implemented: only the
shape that makes it one more row. Item 5 (a variable without an ID returned to its
initial value in a migration) was decided afterwards: the planner refuses a
candidate in which a persistent variable of either container has no ID, and the
refusal names it (V4020, [ADR-0074](../adrs/0074-a-migration-needs-a-stable-id-for-every-persistent-variable.md)).
The rows of the table of edit classes that carry partial IDs say so as data.

## What Was Found

- A replicated untest could panic. `copy_persistent` copied the data region
  with `copy_from_slice`; a standby that held buffers a candidate had sized
  larger than the reverting primary's image panicked. The image now carries the
  persistent runs and applies to the same runs of any buffers.
- The crossload codec dropped every snapshot with a data region of eight bytes
  or more, because it split the payload with `as_chunks::<8>()` and took the
  remainder as the data region. A program with a STRING variable could not
  replicate its state. Fixed.
- The hash did not cover `element_stride`, which the format document said it
  did. A change of a structure that moves the strided STRING fields of an array
  of structures changed the data layout and not the hash. It is covered now.
- The user function block descriptors were written from a hash map, in an order
  that is not stable between compilations. They are not part of the hash and
  the planner no longer compares them; the order was left as it is.
- The format document gave the version as 6 and ordered by sorted name; the
  code was at 7 and ordered by declaration and dependency. Both are corrected.

## Guard

`ironplc_test::edit_classes` holds the rows. `codegen/tests/it/layout_hash.rs`
asserts for each row that the state layout is unchanged or changed as the row
says; `runtime/tests/body_edit_swap.rs` asserts that a host swaps and
reverts an unchanged layout as an ordinary online change, keeps the observed
persistent values and runs the edited code, and that it refuses a changed
layout without stable variable IDs and as the row says with them. The rows are
the ones that were true before (logic, constant inference, string temporaries
and call chains), the first call and the last call of a user function in a
program body and in a function block body, a function nothing calls, a body that
adds a dependency, and the declaration rows: a variable added, removed or
retyped, an array bound, a function block field added, and a function block
field retyped.
