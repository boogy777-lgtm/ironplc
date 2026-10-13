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

Verified = the CODESYS page of the attribute was read. The rows marked
"to read" are completed before the core change of their family.

| Family | Attributes | Concerns | IronPLC today | Needs |
|---|---|---|---|---|
| Enumeration | `strict`, `qualified_only` (enumeration), `to_string` | which programs are accepted; what a name means; `TO_STRING` | always strict; `E.V` is not read (P4007); no `TO_STRING`/`TO_INT` | a not-strict mode; the qualified form; the `TO_<type>` conversions |
| Name scope | `qualified_only` (global variable list) | global variables are addressed with the list name | a global list has no name in text sources | named global variable lists |
| Layout | `pack_mode` (`0`,`1`,`2`,`4`,`8`) | offsets and size of a structure | layout computed with natural alignment; not observable (`SIZEOF(structure)` is P9999) | `SIZEOF` of a structure; the layout rule per mode |
| Visibility | `hide`, `hide_all_locals`, `conditionalshow`, `conditionalshow_all_locals` | lists a user sees: monitoring, debugger, input assistance, symbols | `--dump-vars`, the debugger and the language server show everything | one "is shown" answer read by each list |
| Display | `displaymode`, `monitoring`, `monitoring_display`, `monitoring_encoding` | how a value is shown while monitoring | values shown in decimal | a display format in the debug information |
| Messages | `{text}`, `{info}`, `{warning}`, `{error}`, `obsolete`, `suppress_warning`, `no_assign`, `no_assign_warning`, `{warning disable/restore}` | messages of a compile | errors only | a warning and a message level (also wanted for C0354) |
| Initialisation | `noinit`, `global_init_slot`, `call_after_init`, `call_after_global_init_slot`, `call_before_global_exit_slot`, `initialize_on_call`, `init_namespace`, `no_instance_in_retain` | values after start and reset; order of initialisation; `FB_Init` | to read | to read |
| Online change | `init_on_onlchange`, `no_copy`, `call_after_online_change_slot`, `call_on_type_change` | what a variable holds after an online change | the migration of a hot edit copies every variable that keeps its identity (ADR-0073, ADR-0074) | the migration reads the row |
| Code generation | `linkalways`, `no_check`, `const_replaced`, `const_non_replaced`, `estimated-stack-usage` | what is compiled in; implicit check functions; constants | every POU is compiled; no implicit check functions | to read |
| Dynamic creation | `enable_dynamic_creation` | `__NEW` of a function block | to check | to check |
| Symbols | `symbol` (`none`, `read`, `write`, `readwrite`) | which variables are exported | no symbol export | a symbol export |
| Graphical editors | `pingroup`, `pin_presentation_order_inputs`, `pin_presentation_order_outputs`, `ExpandFully` | pins of a block in FBD/LD | no graphical editor | waits for one |
| To read | `dataflow`, `instance-path`, `io_function_block`, `io_function_block_mapping`, `is_connected`, `no-exit`, `no_virtual_actions`, `ProcessValue`, `reflection`, `subsequent` | to read | — | — |
| Conditions | `hasattribute`, `hastype`, `hasvalue`, `hasconstantvalue`, `defined(variable: ...)` in `{IF}` | conditional compilation over the project | a fault (P0024) | conditions evaluated after the declarations are known |

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
- [ ] Enumeration · Layout · Visibility · Display · Initialisation · Online
      change · Code generation · Dynamic creation
- [ ] Messages (after PF3) · Name scope · Symbols · Conditions

Tracking issue: to be opened with this plan PR.
