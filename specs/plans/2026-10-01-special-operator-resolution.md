# Plan: Semantic Resolution of the CODESYS Special Operators

## Goal

The CODESYS ST-visible special operators (`__NEW`, `__DELETE`, `__TYPEOF`,
`__XADD`, `__CURRENTTASK`, `__SYSTEM.`, `__POOL.`) parse today
(`codesys-st-surface-syntax.md`, REQ-CS-parser-008) but the analyzer does not
resolve them: `ironplcc check --dialect codesys` reports P4017 (undeclared
function) for the first four and P4007 (undeclared variable) for the last
three. Both are misleading, because the names are recognised.

After this change, under the dialect that accepts the operators, a program
using them either passes semantic analysis with the typing the CODESYS
reference gives, or is rejected with a diagnostic that says what is wrong with
it. Runtime behaviour (heap allocation, task information, atomic memory
access) is out of scope; code generation refuses what it cannot implement.

## Reference findings

From the decompiled reference compiler (`Codesys/decompiled/Compiler35220.plugin`)
and `Codesys/docs/03_ERROR_CATALOG.md`:

| Operator | Reference typing | Source |
|---|---|---|
| `__NEW(T[, n])` | value of type `POINTER TO T` whatever `n` is; `T` must be a type; result must be assigned; `n` on a user-defined type is an error (Err 249); a type is required (Err 248) | `NewExpression._CompiledType`, `ErrNewNeedsType`, `ErrNewArrayOnUserdefNotAllowed`, `ErrNewPositionNotOK` |
| `__DELETE(p)` | `BOOL`; the operand must be a pointer (Err 242) | `SimpleTypeChecker`, `ErrDeleteNeedsPointer` |
| `__TYPEOF(x)` | `INT`; the operand is a type or an expression | `SimpleTypeChecker` |
| `__XADD(p, v)` | `DINT`; `p` must be `POINTER TO DINT`, `v` converts to `DINT` | `TypeCheckerVisitor` (`__XADD` check) |
| `__CURRENTTASK` | `POINTER TO __SYSTEM.__TaskSpecificInfo`; the structure is defined by the target's system library | `TypifierAndCrossReferenceCollector`, `OperatorCurrentTaskInfo.xml` |
| `__SYSTEM.x`, `__POOL.x` | scope prefixes: `x` is looked up in the system library / the global pool | `ScopeExpressionParser`, `docs/05` |

The reference material does not contain `__TaskSpecificInfo`'s field list or
the `__SYSTEM` library, so `__CURRENTTASK`, `__SYSTEM.` and `__POOL.` cannot
be typed without guessing. They are rejected with a dedicated "recognised but
unsupported" problem code.

## Architecture

Extend the existing mechanism for dialect-gated extension functions
(`SIZEOF`, `ADR`): a flag registers the operators' signatures in the function
environment (`stages.rs`, next to `get_sizeof_function`), so the declared-call
and argument-count rules apply unchanged. What a signature cannot express is
data on one descriptor table (`intermediates/special_operator.rs`), read by
the three places that need it:

* result typing in `xform_resolve_expr_types` (`__NEW` is a pointer to its type
  argument; the others have fixed result types);
* one new semantic rule, `rule_special_operator`, that checks operand shape
  (type argument, pointer operand, position of `__NEW`) and reports the
  recognised-but-unsupported names;
* `rule_use_declared_symbolic_var`, which must not report a type argument or a
  recognised name as an undeclared variable.

New surface, so a new flag: `--allow-special-operators`, enabled by the
`codesys` dialect. Without it nothing is registered and the operators remain
undeclared, exactly as today.

Two problem codes, one per distinct meaning, shared by every operator
(same class, no per-operator codes):

* P4073 `SpecialOperatorOperandInvalid` - the operands do not fit the operator.
* P4074 `SpecialOperatorNotSupported` - the operator is recognised but this
  compiler does not support it.

Parser: `__NEW(T, n)` (the element-count form) is accepted by extending the
existing `special_operator_type_expression` rule.

Code generation refuses the four function-shaped operators with the existing
`NotImplemented` diagnostic (P9999) before compiling any operand; it never
emits code for them.

## Prefactoring

None is needed. The new behaviour is a data table plus one rule; it does not
add a `match` arm to a place that already branches on operator names. `ADR` and
`__ISVALIDREF` lower to AST nodes that already exist (`Ref`, a comparison with
`NULL`), which is why they are transforms; these operators have no such node,
so they are typed calls. Folding `ADR`, `SIZEOF` and `__ISVALIDREF` into the
descriptor table would be a rewrite larger than the feature with no caller
asking for it, so it is left out.

## Design doc reference

`specs/design/codesys-st-surface-syntax.md`, section "ST-visible special
operators" (updated by the core change: new REQ IDs, replaces the "not
implemented" paragraph).

## File map

Create:

* `compiler/analyzer/src/intermediates/special_operator.rs`
* `compiler/analyzer/src/rule_special_operator.rs`
* `docs/reference/compiler/problems/P4073.rst`, `P4074.rst`
* `docs/reference/extension-library/functions/special-operators.rst`

Modify:

* `compiler/parser/src/options.rs` (flag), `parser.rs` (`__NEW(T, n)`)
* `compiler/analyzer/src/stages.rs`, `xform_resolve_expr_types.rs`,
  `rule_use_declared_symbolic_var.rs`
* `compiler/codegen/src/compile_call.rs`
* `compiler/problems/resources/problem-codes.csv`
* CLI/LSP/MCP flag plumbing (`ironplc-cli`, `mcp`, `playground`), docs flag
  tables
* `specs/design/codesys-st-surface-syntax.md`
* `Codesys/LEXER-GAP-ANALYSIS.md` (P0-16 status, run-3 note)

## Tasks

Single core change PR (no prefactor):

- [ ] Flag `allow_special_operators` (`codesys` preset) and its plumbing
- [ ] Parser: `__NEW(T, n)`
- [ ] Descriptor table and signature registration
- [ ] Result typing in `xform_resolve_expr_types`
- [ ] `rule_special_operator` and the `rule_use_declared_symbolic_var` exemption
- [ ] P4073 and P4074 (CSV, docs, emitters, tests)
- [ ] Codegen refusal (P9999) and test
- [ ] Analyzer tests per operator (accept and reject), dialect-gating tests
- [ ] REQ IDs in the design doc with `spec_test` tests
- [ ] User docs and the gap-analysis journal (run 3)
- [ ] Gate: `cd compiler && just`, the four specs checks
