# Plan: Attribute Mechanism

**Status of this file:** plan PR, never merged (development standards). The
core change PRs are listed in the tracking issue named at the end.

## Goal

Every pragma CODESYS V3.5 documents does in IronPLC what it does in CODESYS.

```
{attribute 'name' := 'value'}   -> data of the declaration it stands before
a known attribute                -> one row: name, where it applies, what value it takes
what the attribute states        -> read by the part of the system it concerns
```

Today a pragma in braces is one trivia token that the lowering does not read:
it is accepted under `allow_pragmas` and has no effect. Only the conditional
pragmas (`{IF}`, `{DEFINE}`) are evaluated.

### What "done" means

1. The mechanism (Parts 1 and 2 below): an attribute is attached to its
   declaration, survives every transform, is rendered back by `plc2plc`, and a
   known attribute in a place or with a value its row does not allow is
   reported.
2. Every attribute of the table in "Attributes and what they concern" has a row
   and is honoured by every part of IronPLC it concerns.
3. An attribute that concerns a part IronPLC does not have is still a row, is
   kept on the declaration, and is listed in the reference with the part it
   waits for. Nothing reports such an attribute as honoured.

## Where we are (verified 2026-10-13, `main` = `0bef073a0`)

| Fact | Evidence |
|---|---|
| A pragma is one `SyntaxKind::Pragma` trivia token; trivia before a node sits in the enclosing node, before the node | `compiler/syntax/src/cst.rs` (placement rule), `syntax_kind.rs:144` |
| The lowering reads no trivia | `compiler/syntax/src/lower/` |
| Pragma text is read in one place, for the conditional directives only | `compiler/syntax/src/pragma.rs` (`is_directive`, `Conditionals`) |
| Conditions that need a project (`hasattribute`, `hastype`, `hasvalue`) are a fault | `pragma.rs` module comment, `Fault::UnexpectedValue` |
| A declaration node already carries data that is not syntax (`VarDecl::block`, `VarDecl::type_id`) | `compiler/dsl/src/common.rs:2181` |
| `{attribute 'strict'}`, `{attribute 'hide'}`, `{attribute 'pack_mode' := '1'}` compile under `--dialect codesys` and change nothing; `--dump-vars` shows a hidden variable | run |
| The byte layout of a structure (offsets, C-style alignment, size) is computed by the analyzer; the VM stores each field in an 8-byte slot; `SIZEOF(structure)` is P9999 | `analyzer/src/intermediates/structure.rs`, `intermediate_type.rs:301`, run |
| Every diagnostic is an error; there is no warning or message level | `dsl/src/diagnostic.rs` |
| An enumeration is strict always; there is no mode where it is its base type | `end_to_end_enumeration_integer.rs` |

CODESYS facts come from the online help (`content.helpme-codesys.com`,
"Reference, Programming → Pragmas"), read on 2026-10-13. Not documented there:
what CODESYS does with a known attribute in a place it does not apply to, with
several attributes before one declaration, and whether names are compared by
case. These are decided below and marked as decisions of this project.

## Architecture

Terms (one meaning each):

- **pragma** — any `{ ... }`. Its **kind** is one of: conditional directive,
  attribute, message, other.
- **attribute** — a name and an optional value, as written.
- **target** — the declaration an attribute is attached to.
- **row** — the single declaration of an attribute the compiler knows.
- **observer** — the part of the system an attribute concerns (the analysis of
  enumerations, the layout of structures, the list of variables a debugger
  shows, ...).

### Part 1 — the attribute is data of the declaration

1. **One reader of pragma text.** `ironplc_syntax::pragma` gives the kind of a
   pragma and, for an attribute, its name and value. The conditional
   evaluation and the attribute attachment both ask it; nothing else reads the
   text of a pragma.
2. **One attachment rule, stated once.** An attribute pragma is attached to the
   node that follows it. A node that only groups declarations (`TYPE ...
   END_TYPE`, a `VAR` block) passes the attributes before it to each declaration
   it holds. An attribute pragma that no target follows is reported. The rule
   is one function over the tree; the lowering of a target asks it and does not
   walk trivia itself.
3. **One field.** `dsl::attribute::Attributes` (the attributes in source order,
   each with its span) is a field of every target node: a program, function,
   function block, interface, method, property and action; every type
   declaration; a structure member; a variable declaration; a statement. A
   table names the target kinds; a test fails when a declaration node has no
   such field or the lowering does not fill it.
4. **Round trip.** `plc2plc` renders the attributes of a node before it.

### Part 2 — a known attribute is a row

5. **The registry.** One table of rows. A row states: the name and its
   aliases (`noinit`, `no_init`, `no-init`), the target kinds it applies to,
   the values it takes, and the observers it concerns. A typed reader per row
   (`PACK_MODE.of(&node.attributes) -> Option<PackMode>`) is the only way an
   observer reads an attribute; no observer compares a name.
6. **One validation rule.** For every attribute of every target: a name with
   no row is a user-defined attribute and is accepted (CODESYS documents
   them); a row whose targets do not include the target, or whose values do
   not include the value, is reported (new problem codes).
7. **The reference page.** The docs page of attributes is checked against the
   registry: a row without an entry fails a test.

### Part 3 — observers

Each family of the table below is a core change of its own: the observer reads
the typed fact of the row. Adding an attribute is one row and the read in its
observer; the lexer, the parser, the lowering, the node types and the
validation rule do not change.

### Decisions of this project (CODESYS does not document them)

| Question | Decision | Why |
|---|---|---|
| A known attribute before a declaration it does not apply to (`{attribute 'pack_mode' := '1'}` before a variable) | error | a silently ignored attribute is the defect this plan removes |
| A value outside the row (`pack_mode := '3'`) | error | same |
| Case of the name (`'Hide'`) | compared without case, as identifiers are | a misspelled case would turn a known attribute into a user-defined one without a word |
| Several attributes before one declaration | all attach, in source order; the same attribute twice is an error | the documentation shows one per line and names no limit |
| An attribute whose observer IronPLC does not have | accepted, kept, listed in the reference as waiting | see "done", item 3 |

## Attributes and what they concern

Every row was read from the CODESYS page of the attribute on 2026-10-13
(`content.helpme-codesys.com`, or its ABB mirror where that host refused),
except where the table says otherwise. "IronPLC today" is by run where it says
so, otherwise by the presence of the feature in the code, to be confirmed by
run before the core change of the family.

| Family | Attributes | Concerns (CODESYS) | IronPLC today | Needs |
|---|---|---|---|---|
| Enumeration | `strict`, `qualified_only` (enumeration), `to_string` | strict type test; a value is written with the type name; `TO_STRING` gives the name of the value | always strict (run); `E.V` is not read, P4007 (run); no `TO_STRING` of an enumeration | a not-strict mode; the qualified form; `TO_STRING` |
| Name scope | `qualified_only` (global variable list) | a variable of the list is addressed only with the list name | a global list has no name in text sources | named global variable lists |
| Layout | `pack_mode` (`0`,`1`,`2`,`4`,`8`), `subsequent` | alignment of the members of a structure (default 4 or 8 by device; an array of structures is padded; a packed structure must not hold an unpacked one); consecutive allocation of the variables of a program or list | natural alignment, not observable: `SIZEOF(structure)` is P9999 (run); the VM stores a field in an 8-byte slot | the layout rule per mode; `SIZEOF` of a structure; a decision on what "memory" a pack mode describes in a slot VM |
| Visibility | `hide`, `hide_all_locals`, `conditionalshow`, `conditionalshow_all_locals` | the variable is not shown in monitoring, debugging, input assistance, the library manager, the symbol configuration; it stays usable by name | `--dump-vars` shows it (run); the debugger and the language server show everything | one "is shown" answer read by every list |
| Display | `displaymode` (`bin`,`dec`,`hex`), `monitoring` (`variable`,`call`), `monitoring_display`, `monitoring_encoding` (`UTF8`) | how a value is shown while monitoring; monitoring of a property | decimal only | a display format in the debug information; property monitoring |
| Messages | `{text}`, `{info}`, `{warning}`, `{error}`, `obsolete`, `suppress_warning`, `{warning disable/restore <id>}`, `no_assign_warning`, `estimated-stack-usage` | messages of a compile; a warning at each use of a type; suppression by id; the stack check of a recursive method | errors only (code) | PF3: a warning and a message level |
| Compile checks | `no_assign`, `no_instance_in_retain`, `enable_dynamic_creation` | assigning an instance to an instance is an error; an instance in retain memory is an error; `__NEW` of a function block needs the attribute | `__NEW` and `RETAIN` are in the compiler (code) | three rules reading the row |
| Initialisation | `noinit` (`no_init`, `no-init`), `initialize_on_call`, `global_init_slot`, `call_after_init`, `call_after_global_init_slot`, `call_before_global_exit_slot`, `no-exit`, `init_namespace`, `instance-path`, `is_connected`, `reflection` | a variable keeps its value on reset; inputs are initialised on each call; order of initialisation (default slot 50000 for a POU, 49990 for a list); a method runs after `FB_Init`; functions run after global initialisation and before exit; `FB_Exit` is not called for an instance; a string holds the library namespace or the instance path; a flag tells whether an input was assigned | `FB_Init` is in the compiler (code); no `FB_Exit`, no reset kinds, no initialisation order (code) | reset kinds; an initialisation order; `FB_Exit`; libraries with a namespace |
| Online change | `init_on_onlchange`, `no_copy`, `call_after_online_change_slot`, `call_on_type_change` | the variable is initialised, not copied, by an online change; functions and methods run after one | the migration of a hot edit copies every variable that keeps its identity (ADR-0073, ADR-0074) | the migration reads the row; a call after the change |
| Code generation | `linkalways`, `no_check`, `const_replaced`, `const_non_replaced` | an unused POU is still compiled in; the implicit check functions (`CheckBounds`, `CheckDiv*`, `CheckRange*`, `CheckPointer`) are not called for a POU; a constant is or is not replaced by its value | every POU is compiled; no implicit check functions (code) | removal of unused POUs; the check functions |
| Inheritance | `no_virtual_actions` | the actions of an SFC base block are not overridden by a derived block | to check | to check |
| Symbols | `symbol` (`none`,`read`,`write`,`readwrite`) | which variables are exported as symbols | no symbol export | a symbol export |
| I/O mapping | `io_function_block`, `io_function_block_mapping` | the block is offered for a channel in the device configuration | no device configuration | waits for one |
| Graphical editors | `pingroup`, `pin_presentation_order_inputs`, `pin_presentation_order_outputs`, `dataflow`, `ProcessValue`, `ExpandFully` | pins and data flow of a block in FBD, LD, CFC; members shown in a visualisation | no graphical editor, no visualisation | waits for one |
| Conditions | `hasattribute`, `hastype`, `hasvalue`, `hasconstantvalue`, `hasconstanttype`, `defined(variable: ...)`, `defined(type: ...)`, `defined(pou: ...)` in `{IF}` | conditional compilation over the project | a fault, P0024 (code) | conditions evaluated after the declarations are known |

Not found in the help (HTTP 403 and 404): `nonvirtual`, `noflow`. They get a
row when a page is found.

TwinCAT documents more attributes (`OPC.UA.DA` and its family, `TcRpcEnable`,
`TcDisplayScale`, `tc_no_symbol`, `TcNcAxis`, `TcLinkTo`, `TcInitSymbol`,
`TcContextName`, `c++_compatible`, `minimal_input_size`, `analysis`,
`naming`). They are outside this plan; until a row exists each is a
user-defined attribute.

## Prefactoring

Three, each behaviour-preserving, each its own PR:

- **PF1 — one reader of pragma text.** `pragma::kind(text)` replaces
  `is_directive`; the conditional evaluation asks it. No pragma is read
  differently.
- **PF2 — the field, empty.** `Attributes` is added to every target node and is
  always empty. The analysis pins are rewritten in this PR and only `library`
  digests move (the procedure in the header of
  `benchmarks/tests/analysis_pins.rs`: this is its second step with an empty
  first step, since the field holds nothing).
- **PF3 — a warning and a message level** is a prefactor of the Messages family
  only, with its own plan: it changes the exit code rules, the language server
  and every place that counts diagnostics.

No prefactoring is needed for the registry: it is new and has no earlier shape.

## Design doc reference

New: `specs/design/attributes.md` (requirement area `ATTR`), landed with the
first core change. It supersedes nothing; `codesys-jump-and-pragmas.md` keeps
the conditional pragmas and gains a pointer.

ADR (with the first core change): "An attribute is data of the declaration it
stands before" — the alternatives are a side table keyed by span (no node
changes, but every observer needs the table handed to it and a transform must
carry it) and attributes kept as trivia (no observer can read them).

## File map

| File | Change |
|---|---|
| `compiler/syntax/src/pragma.rs` | PF1: the kind of a pragma; the attribute form |
| `compiler/dsl/src/attribute.rs` (new) | `Attribute`, `Attributes`, the target kinds |
| `compiler/dsl/src/common.rs`, `oop.rs`, `textual.rs`, `sfc.rs` | PF2: the field on the target nodes |
| `compiler/benchmarks/tests/data/analysis_pins.txt` | PF2: rewritten |
| `compiler/syntax/src/lower/attributes.rs` (new) | the attachment rule |
| `compiler/syntax/src/lower/{pou,types,var_blocks,oop,statements}/` | each target asks the rule |
| `compiler/plc2plc/src/renderer.rs` | renders attributes |
| `compiler/dsl/src/attribute/registry.rs` (new) | rows and typed readers |
| `compiler/analyzer/src/rule_attribute.rs` (new) | the validation rule |
| `compiler/problems/resources/problem-codes.csv`, `docs/reference/compiler/problems/` | new codes: attribute without a target; attribute not allowed here; attribute value |
| `docs/reference/language/` | the attributes reference page |
| `specs/design/attributes.md`, `specs/adrs/0075-*.md` (new) | design and decision |

## Tasks

### PR 1 (prefactor PF1)
- [ ] `pragma::kind`; conditional evaluation asks it; tests unchanged

### PR 2 (prefactor PF2)
- [ ] `dsl::attribute`; the field on every target node, empty
- [ ] guard: every declaration node kind is in the target table
- [ ] pins rewritten; only `library` digests move

### PR 3 (core: Part 1)
- [ ] the attachment rule; targets ask it; an attribute without a target is reported
- [ ] `plc2plc` round trip
- [ ] design doc and ADR

### PR 4 (core: Part 2)
- [ ] the registry with every row of the table; typed readers
- [ ] the validation rule and its problem codes
- [ ] the reference page, checked against the registry

### PR 5.. (core: one per family, in the owner's order)
- [ ] Enumeration · Layout · Visibility · Display · Compile checks ·
      Initialisation · Online change · Code generation · Inheritance
- [ ] Messages (after PF3) · Name scope · Symbols · Conditions
- [ ] I/O mapping · Graphical editors: rows only, until the part they concern exists

Tracking issue: #62.
