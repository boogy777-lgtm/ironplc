# Design: State Layout and Body Edits

status: proposed
date: 2026-10-08

## Overview

An edit of the body of a program must be an ordinary online change: accepted,
testable, revertible, with every value kept. Today the first call of a user
function, and the removal of the last call, is refused. This note states why,
what the options are, and which one is recommended.

**The decision is the owner's and is not made.** This note is the research the
decision is made from. An agent read the code path of an online change end to
end, the decompiled CODESYS compiler, and the design documents; nothing was
built or run for it. The three comparisons of "Why a first call is refused",
the swap of "What the swap does", and the absence of `add_fb_type` in code
generation were read a second time by the coordinator. Every other line number
is as the research reported it. Paths are under `compiler/`.

## What Happens Today

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

**Rockwell Logix.** No primary source on the memory layout was found on the
machine of the research.

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

## Guard

The table of body edits in `codegen/tests/it/layout_hash.rs` and
`runtime/tests/body_edit_swap.rs` holds the rows that are true today. The rows
"first call of a user function" and "last call removed", in a program and in a
function block body, and the dependency order row join it with the
implementation. The declaration rows stay and gain "a field of a function
block is retyped", which O1 must still refuse.
