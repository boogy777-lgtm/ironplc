# Design: ST Declaration Extensions (UNION, VAR_STAT/VAR_INST/VAR_GENERIC, NAMESPACE, __BEGIN_IMPLEMENTATION, OVERLOAD)

## Overview

This document covers the CODESYS/TwinCAT Structured Text declaration syntax
that IronPLC's lexer and parser previously rejected with P0002:

| Construct | Source | Gate |
|-----------|--------|------|
| `UNION ... END_UNION` type declaration | `Codesys/tables/st_keywords.csv` (`UNION` 100, `END_UNION` 79) | `--allow-union-type` |
| `VAR_STAT ... END_VAR` section | `st_keywords.csv` (`VAR_STAT` 114) | `--allow-var-stat` |
| `VAR_INST ... END_VAR` section | `st_keywords.csv` (`VAR_INST` 244) | `--allow-var-inst` |
| `VAR_GENERIC ... END_VAR` section | `st_keywords.csv` (`VAR_GENERIC` 280) | `--allow-var-generic` |
| `NAMESPACE ... END_NAMESPACE` | `st_keywords.csv` (`NAMESPACE` 287, `END_NAMESPACE` 288) | `--allow-namespace` |
| `__BEGIN_IMPLEMENTATION` marker | `st_keywords.csv` (286) | `--allow-begin-implementation` |
| `OVERLOAD` method qualifier | `oo_keywords.csv` (289) | `--allow-fb-inheritance` |

Every construct except `OVERLOAD` is a keyword token that demotes to an
ordinary identifier when its flag is off, so an Edition 2 program may keep
using those words as names. `OVERLOAD` follows the access-modifier design of
`specs/design/beckhoff-twincat-dialect.md` §1.5: a contextual word matched by
text only in the qualifier slot.

### Delivered subset (this design) and its boundaries

The parser and the AST carry every construct end to end, and plc2plc renders
them. The following semantic work is deliberately **not** part of this design
and is tracked as P1 in `Codesys/LEXER-GAP-ANALYSIS.md` §13:

- **Storage placement.** `VAR_STAT` and `VAR_INST` declarations keep the
  IronPLC `VAR` lifetime (per-POU-instance storage). CODESYS "static" and
  "method instance" placement rules are not modeled; a `VAR_INST` method
  variable currently re-initializes on each call.
- **UNION memory layout.** A union behaves like a structure of its members
  for resolution; its members are not overlaid at offset 0.
- **Namespace resolution.** `NAMESPACE` flattens: its declarations become
  ordinary global declarations, and qualified access (`ns#name`) is not
  resolved. `NAMESPACE` is therefore a grouping construct, not a scope.

## Lexer

**REQ-STX-parser-001** `UNION` and `END_UNION` lex as the keyword tokens
`TokenType::Union` / `TokenType::EndUnion` (case-insensitive). With
`allow_union_type` off, both demote to `TokenType::Identifier`.

**REQ-STX-parser-002** `VAR_STAT` lexes as `TokenType::VarStat` and demotes to
`Identifier` unless `allow_var_stat` is set.

**REQ-STX-parser-003** `VAR_INST` lexes as `TokenType::VarInst` and demotes to
`Identifier` unless `allow_var_inst` is set.

**REQ-STX-parser-004** `VAR_GENERIC` lexes as `TokenType::VarGeneric` and
demotes to `Identifier` unless `allow_var_generic` is set.

**REQ-STX-parser-005** `NAMESPACE` and `END_NAMESPACE` lex as
`TokenType::Namespace` / `TokenType::EndNamespace` and demote to `Identifier`
unless `allow_namespace` is set.

**REQ-STX-parser-006** `__BEGIN_IMPLEMENTATION` lexes as
`TokenType::BeginImplementation` and demotes to `Identifier` unless
`allow_begin_implementation` is set.

**REQ-STX-parser-007** `OVERLOAD` is not a token. It is matched by text in the
member-qualifier slot on a method and becomes
`MemberQualifierKind::Overload` in source order; everywhere else the word
remains an ordinary identifier.

**REQ-STX-parser-008** The dialect presets enable the flags: `codesys` and
`twincat` enable all six, and `iec61131-3-ed3` and `rusty` additionally
enable `allow_union_type`.

## Grammar and AST

### UNION

**REQ-STX-parser-010** In a `TYPE` declaration, `Name : UNION ... END_UNION`
parses to `DataTypeDeclarationKind::Union(UnionDeclaration)`, carrying the
declared type name and one `StructureElementDeclaration` per member.

**REQ-STX-parser-011** Union members use the same element grammar as struct
members, so an initializer, an array or an inline enumeration in a member
parses as it does inside `STRUCT ... END_STRUCT`.

### Variable sections

**REQ-STX-parser-012** `VAR_STAT ... END_VAR` and `VAR_INST ... END_VAR` are
accepted wherever the other variable sections are (function, function block,
program, method), and produce declarations with `VariableType::Static` and
`VariableType::Instance` respectively.

**REQ-STX-parser-013** `VAR_GENERIC ... END_VAR` is accepted directly after a
function block's name (before `EXTENDS`/`IMPLEMENTS` and the other sections)
and produces declarations with `VariableType::Generic`; in any other position
the keyword is a syntax error.

**REQ-STX-parser-014** With the flag off, a demoted keyword is a valid
identifier: `VAR UNION : INT; END_VAR` and the other five words parse as
variable names under `CompilerOptions::default()`.

### NAMESPACE

**REQ-STX-parser-015** `NAMESPACE name { element } END_NAMESPACE` parses to
`LibraryElementKind::NamespaceDeclaration(NamespaceDeclaration)`, holding the
namespace name, its nested library elements and the span of the declaration.
Namespaces nest: a namespace element list may contain another namespace.

### `__BEGIN_IMPLEMENTATION`

**REQ-STX-parser-016** `__BEGIN_IMPLEMENTATION` in a statement list parses to
`StmtKind::BeginImplementation(SourceSpan)`; the statements that follow stay
in the same list.

## Rendering

**REQ-STX-plc2plc-020** The renderer writes back `UNION ... END_UNION` and the
`VAR_STAT`/`VAR_INST`/`VAR_GENERIC` sections, and the rendering re-parses to
the same AST.

**REQ-STX-plc2plc-021** The renderer writes back `NAMESPACE ... END_NAMESPACE`
and `__BEGIN_IMPLEMENTATION`, and the rendering re-parses to the same AST.

## Semantics

**REQ-STX-analyzer-030** Declaration toposort flattens namespaces: the
declarations a namespace contains become ordinary library elements, in source
order, so a program or type declared inside a namespace is analyzed and
compiled as a top-level declaration.

## Requirements → Tests

Each requirement ID carries the slug of the crate that owns its conformance
test (see [cross-crate-spec-conformance.md](./cross-crate-spec-conformance.md)).
The `parser`, `plc2plc` and `analyzer` crates list this document in their
`build.rs`, and every test below is annotated with the matching
`#[spec_test(REQ_STX_...)]`.

| Requirement | Test function | File | Kind |
|-------------|---------------|------|------|
| REQ-STX-parser-001 | `lexer_spec_req_stx_001_union_tokens_and_demotion` | `compiler/parser/src/tests/union.rs` | lexer |
| REQ-STX-parser-002 | `lexer_spec_req_stx_002_var_stat_token_and_demotion` | `compiler/parser/src/tests/var_declarations.rs` | lexer |
| REQ-STX-parser-003 | `lexer_spec_req_stx_003_var_inst_token_and_demotion` | `compiler/parser/src/tests/var_declarations.rs` | lexer |
| REQ-STX-parser-004 | `lexer_spec_req_stx_004_var_generic_token_and_demotion` | `compiler/parser/src/tests/var_declarations.rs` | lexer |
| REQ-STX-parser-005 | `lexer_spec_req_stx_005_namespace_tokens_and_demotion` | `compiler/parser/src/tests/namespaces.rs` | lexer |
| REQ-STX-parser-006 | `lexer_spec_req_stx_006_begin_implementation_token_and_demotion` | `compiler/parser/src/tests/namespaces.rs` | lexer |
| REQ-STX-parser-007 | `parser_spec_req_stx_007_overload_is_a_member_qualifier` | `compiler/parser/src/tests/fb_inheritance.rs` | parser |
| REQ-STX-parser-008 | `options_spec_req_stx_008_dialect_presets_enable_the_flags` | `compiler/parser/src/options.rs` (tests mod) | options |
| REQ-STX-parser-010 | `parser_spec_req_stx_010_union_declaration_ast_shape` | `compiler/parser/src/tests/union.rs` | AST |
| REQ-STX-parser-011 | `parser_spec_req_stx_011_union_members_use_struct_element_grammar` | `compiler/parser/src/tests/union.rs` | AST |
| REQ-STX-parser-012 | `parser_spec_req_stx_012_var_stat_and_var_inst_sections` | `compiler/parser/src/tests/var_declarations.rs` | AST |
| REQ-STX-parser-013 | `parser_spec_req_stx_013_var_generic_only_after_function_block_name` | `compiler/parser/src/tests/var_declarations.rs` | AST |
| REQ-STX-parser-014 | `parser_spec_req_stx_014_demoted_keywords_are_identifiers` | `compiler/parser/src/tests/dialect_flags.rs` | negative → positive |
| REQ-STX-parser-015 | `parser_spec_req_stx_015_namespace_declaration_ast_shape` | `compiler/parser/src/tests/namespaces.rs` | AST |
| REQ-STX-parser-016 | `parser_spec_req_stx_016_begin_implementation_marker` | `compiler/parser/src/tests/namespaces.rs` | AST |
| REQ-STX-plc2plc-020 | `plc2plc_spec_req_stx_020_union_and_var_sections_round_trip` | `compiler/plc2plc/src/tests/union.rs` | round-trip |
| REQ-STX-plc2plc-021 | `plc2plc_spec_req_stx_021_namespace_and_marker_round_trip` | `compiler/plc2plc/src/tests/namespaces.rs` | round-trip |
| REQ-STX-analyzer-030 | `analyzer_spec_req_stx_030_namespaces_flatten_during_toposort` | `compiler/analyzer/src/spec_conformance.rs` | analyzer |
