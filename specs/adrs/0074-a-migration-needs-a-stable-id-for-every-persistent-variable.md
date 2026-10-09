# A Migration Needs a Stable ID for Every Persistent Variable

status: accepted
date: 2026-10-09

## Context and Problem Statement

A candidate whose state layout differs from the running application's is
accepted as a migration when both containers carry stable variable IDs
([ADR-0053](0053-stable-variable-ids-for-declaration-level-hot-edit.md),
[ADR-0054](0054-state-migration-across-declaration-level-edits.md)). The planner
copies a value by ID. A persistent variable that has no ID on one side or on
both is not copied: the candidate's init image gives it its declared initial
value. Nothing refuses the edit and nothing warns. A counter that had run for
hours starts from 5 again, and the engineer is told the change was accepted.

The test `run_when_persistent_variable_has_no_stable_id_then_migration_returns_it_to_its_initial_value`
pinned this: a program with `A` (ID 1) and `B := 5` (no ID); a variable `C` is
declared; `B` was 35 and is 15 after the swap and one scan.

IDs live in a sidecar `<stem>.uids.json` beside the project, keyed by
`(scope, name)` ([ADR-0057](0057-stable-variable-uid-sidecar-persistence.md)). A
build only reads the sidecar; `refactor sync-uids` assigns. A variable has no ID
when:

1. the sidecar is missing or malformed, or the project is memory-backed and no
   table was set: no variable has an ID, and the host already refuses a layout
   change (V4007);
2. the variable was added, renamed, or moved to another program (or its program
   was renamed) after the last sync;
3. the last sync reported rename or swap candidates and therefore did not save,
   so every new variable has no ID;
4. the kind of declaration is never keyed. `declared_var_keys` keyed program
   variables, top-level `VAR_GLOBAL` and function block fields. It did not key a
   `VAR_GLOBAL` inside `CONFIGURATION`, which code generation lays out, the
   system uptime globals, or the hidden variables of an edge input
   (`x$prev`, `x$edge`).

Causes 2 to 4 leave a container with IDs on both sides and a persistent variable
without one.

## Decision Drivers

- **A value is never lost silently.** A migration either carries a persistent
  value or the candidate is refused with the cause.
- **The runtime never guesses** (ADR-0054): it does not decide that a variable
  without an ID is "probably the same one".
- **One rule, one place.** Program variables, globals and function block fields
  are rows of one rule, not three checks.
- **A variable that code generation lays out can have an ID.** A refusal that no
  synchronization can remove is a permanent block, not a rule.
- **Two lines of defence.** The workstation should not send a candidate the
  controller will refuse; the controller refuses it anyway.

## Considered Options

1. **Refuse (chosen).** A candidate that would go through a migration while a
   persistent variable has no ID is refused, naming the variables.
2. **Accept, and report the variables that returned to their initial value.**
   The controller has already replaced the state; the report comes after the
   loss.
3. **Ask the engineer** to decide per variable, as ADR-0061 does for a type
   change. A decision for a variable whose identity is unknown is a decision
   about which bytes to copy where, which is the guess ADR-0054 forbids.

## Decision Outcome

Chosen option: **refuse**, because it is the only option that keeps the rule
that nothing is lost without being said, and it needs no decision from the
engineer: the remedy is a synchronization or a full download, which the message
names.

### The rule

A migration needs an ID for every persistent variable of both containers. Where
a container has none for a persistent variable, the host refuses the candidate
with the variables named, before any plan is built.

The rule is evaluated by the planner, once, in
`StateMigrationPlan::build_with_decisions` (`runtime/src/migration/identity.rs`).
The host calls the planner at `stage`, so the active application is untouched by
a refusal.

### Which variables, on each side

The persistent variables of a container are those the container itself declares
as persistent (ADR-0073); the rule reads nothing else:

| Row | Persistent variable | Identity it needs |
|---|---|---|
| Variable | each slot of the persistent extent (`Container::persistent_extents`): globals, program variables, system and hidden slots | an entry of the stable variable table with a UID other than 0 |
| Function block field | each field of each user function block type descriptor (`fb_types`) | an entry of the field UID table with a UID other than 0 |

The rule applies to the active container and to the candidate. A variable
without an ID on either side is a refusal, including a variable that exists on
one side only and one that the candidate removes.

A variable the candidate adds, which the active application does not have, has
an ID on the candidate side that the active side does not carry. That is not a
refusal: the variable is new and starts from its initial value, which is
correct. The same holds for a variable that the candidate removes and whose ID
the active side carries.

ADR-0059 already refused a function block field without a UID when the field
layout of the type changed (`FbLayoutUnsupported`). That check is the same
rule: the field row above is evaluated by the identity rule for every user
function block type, and the check in the function block planner stays only as a
guard for a descriptor and a UID table that disagree.

### What the refusal names

The refusal is `MigrationError::UnidentifiedVariables`, a cause of
`OnlineChangeError::MigrationUnsupported`. It lists, per side, the persistent
variables without an ID, in table order. A variable is named by the debug
section when the container carries one: its name, and for a function block field
`block.field` when the debug section names the block. A container without names
is named by position: `variable <index>` and
`field <ordinal> of function block type <type id>`. The command layer maps the
cause to V4020, which tells the engineer to synchronize the IDs
(`ironplcc refactor sync-uids`) and resolve reported rename candidates
(`ironplcc refactor map-uid`), or to load the program with a full download.

### Relation to the other rules

- **The runtime never guesses** (ADR-0054, ADR-0057): the rule is its
  consequence for an identity that is missing, where the earlier text covered
  an identity that is ambiguous.
- **Engineer decisions** (ADR-0061): a decision resolves a type change of an
  entity whose identity is known. It cannot stand in for a missing ID, and the
  rule is evaluated before decisions are read.
- **V4007** stays the refusal for a container with no IDs at all.

### What a variable's key is

The sidecar key is `(scope, name)`. One definition of the persistent
declarations of a library is read by code generation, which lays them out, and by
`declared_var_keys`, which keys them (`ironplc_codegen::persistent`):

| Declaration | Scope | Name |
|---|---|---|
| top-level `VAR_GLOBAL`, `VAR_GLOBAL` of the `CONFIGURATION` that code generation compiles, the system uptime globals | `global` | the variable |
| program variable (not `VAR_EXTERNAL`) | the program | the variable |
| function block field | the function block type | the field |
| hidden variable of an edge input | the scope of the edge input | `<input>$prev`, `<input>$edge` |

`global` is the scope of every global whatever block declares it. Code generation
matches persistent declarations by name only and lays all globals out in one
prefix, so the namespace is one; a scope named after the configuration would
change the key, and with it the identity, when the configuration is renamed, and
nothing in the layout changes. A `VAR_GLOBAL` of a `RESOURCE` is not laid out by
code generation and has no key.

A guard test compiles the test corpus with a full key set and fails when a slot
of a persistent extent or a field of a function block type has no ID. A kind of
persistent declaration that is laid out but not keyed fails it, whichever kind
it is.

### The workstation

A function of `ironplc-project` (`id_difference`, `project/src/id_agreement.rs`)
compares the declared persistent variables of a project with the sidecar: it
returns the keys declared without an ID and the keys with an ID that nothing
declares. The compile pipeline attaches the verdict to its output
(`CompileOutput::id_difference`); a build ignores it. A sender of a candidate
reads it and does not send when the table is non-empty and the two differ; it
names the difference and tells the engineer to synchronize. An empty table is
not compared: no variable has an ID then, and the controller refuses a layout
change itself (V4007). The controller applies the rule regardless of what the
workstation did.

Senders today: the MCP hot edit tool reads the verdict. It has no way to set a
table, so the verdict is empty there. The VS Code extension compiles with
`ironplcc` and sends the bytes; it does not read the verdict, and until it does
the refusal of the controller (V4020) is the only line of defence on that path.

### Consequences

- Good, because a migration cannot return a persistent value to its initial
  value without the engineer being told which variable and what to do.
- Good, because program variables, globals and function block fields are one
  rule with one evaluation.
- Good, because every persistent variable that code generation lays out can
  have an ID, so the refusal can always be removed by a synchronization.
- Bad, because an edit that removes a variable that never had an ID is refused
  until the program is loaded by a full download: the active container cannot
  be synchronized after the fact.
- Bad, because a project whose sidecar is out of date now stops at the
  workstation for a body-only edit as well, until the sidecar is synchronized.
- Neutral: V4020 is a new code; V4010 keeps its meaning and its decision
  payload.

### Confirmation

- `compiler/runtime/tests/migration_acceptance.rs`: the pinned example, now a
  refusal naming `B` and `C`, the active program unchanged.
- `compiler/test/src/edit_classes.rs`: the rows of a changed layout with partial
  IDs, read by `runtime/tests/body_edit_swap.rs` and
  `codegen/tests/it/layout_hash.rs`.
- `compiler/runtime/src/migration/identity.rs` tests: each row of the rule on
  each side.
- `compiler/project/tests/persistent_keys.rs`: the guard of the key definition
  against code generation.
- `compiler/project/src/id_agreement.rs` and `compile.rs` tests: the comparison
  and the verdict the pipeline attaches; `compiler/mcp/src/tools/hot_edit.rs`:
  a candidate is not sent while the verdict is set.

## More Information

- [ADR-0054](0054-state-migration-across-declaration-level-edits.md): the
  planner; "containers without stable IDs keep the stage 1 rejection".
- [ADR-0057](0057-stable-variable-uid-sidecar-persistence.md): the sidecar and
  its scope rule; the scope of a global in a `CONFIGURATION` is stated here.
- [ADR-0059](0059-fb-field-stable-ids.md): function block field UIDs.
- [ADR-0061](0061-engineer-decided-migration-for-out-of-policy-type-changes.md):
  decisions, which do not replace an ID.
- [State Layout and Body Edits](../design/state-layout-and-body-edits.md),
  "Before Implementation", item 5.
