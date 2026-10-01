# Plan: CODESYS ST Declaration Extensions (P0-3, P0-4, P0-7, P0-8)

## Context

The CODESYS gap analysis (`Codesys/LEXER-GAP-ANALYSIS.md` §13) lists four P0
items whose syntax the reference CODESYS 3.5.22 parser accepts and IronPLC
rejects with P0002:

- **P0-3** `UNION ... END_UNION` type declarations (CODESYS error 171: only
  allowed inside a `TYPE` DUT).
- **P0-4** `VAR_STAT`, `VAR_INST`, `VAR_GENERIC` declaration sections.
- **P0-7** `NAMESPACE ... END_NAMESPACE` (CODESYS/TwinCAT) and the
  `__BEGIN_IMPLEMENTATION` implementation marker.
- **P0-8** remainder: `OVERLOAD` as a method qualifier; the other access
  modifiers landed in #1899 as contextual qualifier words.

The work is lexer + parser + AST plus round-trip rendering. Memory placement
(VAR_STAT/VAR_INST storage class, UNION layout) and namespace resolution are
P1 semantics and stay out of scope here; `specs/design/st-declaration-extensions.md`
records the delivered subset.

## Approach

Follow `specs/steering/syntax-support-guide.md` for each item:

1. **Reference table first** — `Codesys/tables/st_keywords.csv`,
   `oo_keywords.csv`, and the decompiled parser
   (`TypeDeclarationParser.ReadKindOfDeclarationReturnDone`,
   `WhiteTreeParser.ParseDeclarationFunctionBlock`/`ParseDeclarationNamespace`,
   `PossibleAccessSpecifiers`).
2. **Tokens + demotion** — add the keyword tokens to `parser/src/token.rs`
   and one gate + one `match` arm per keyword group in
   `xform_demote_keywords::apply`. Keywords that may be identifiers in
   Edition 2 (`NAMESPACE`, `UNION`, `VAR_STAT`, ...) must be demoted when
   their flag is off.
3. **Flags** — one `--allow-*` flag per keyword family in
   `define_compiler_options!` (`parser/src/options.rs`), plus the CLI
   (`ironplc-cli/bin/main.rs`), LSP (`ironplc-cli/src/lsp.rs`), MCP fixture
   (`mcp/src/feature_flag_conformance.rs`) and the two docs pages.
4. **AST** — new nodes in `dsl`: `UnionDeclaration`,
   `LibraryElementKind::NamespaceDeclaration`,
   `StmtKind::BeginImplementation`, `VariableType::{Static, Instance,
   Generic}`, `MemberQualifierKind::Overload`.
5. **Parser rules** in `parser/src/parser.rs`, mirroring the existing
   struct/variable-section rules.
6. **Analyzer** — only the exhaustive-match arms a new AST variant forces,
   plus namespace flattening in `xform_toposort_declarations` (the semantic
   model stays flat; the nesting lives in the AST and the renderer).
7. **Tests** — parser test per feature (AST shape), plc2plc round trip per
   feature, a whitespace row for the new `_` gaps, and one REQ per pinned
   behavior in `specs/design/st-declaration-extensions.md`.

## Prefactoring

The UNION rule and the STRUCT rule differ only in the enclosing keyword pair:
extract `structure_element_declaration` reuse rather than copying the struct
rule (already shared), and take the union alternative through the same
`type_declaration()` choice list. The variable-section rules share
`semisep_or_empty(<var_init_decl()>)`, so `VAR_STAT`/`VAR_INST` are two more
callers of it, not a new grammar.

## Steps

1. Design doc with REQ IDs; register it in `parser/build.rs` (and
   `plc2plc/build.rs` for the round-trip requirement).
2. P0-8: `OVERLOAD` qualifier (grammar arm, `MemberQualifierKind::Overload`,
   `rule_member_qualifier_invalid` update, tests).
3. P0-4: `VAR_STAT`/`VAR_INST`/`VAR_GENERIC` tokens, flags, AST, rules.
4. P0-3: `UNION`/`END_UNION` tokens, flag, AST, rules, renderer.
5. P0-7: `NAMESPACE`/`END_NAMESPACE` + `__BEGIN_IMPLEMENTATION`.
6. Round-trip resources + tests, whitespace rows, docs.
7. Gates: `cargo test -p ironplc-parser`, `cargo check --workspace
   --all-targets`, `cargo fmt`.

## Verification

Each item is done when: the parser test asserts the AST shape, the plc2plc
resource re-parses to the same AST, the MCP fixture flips reject→accept, and
the new dialect flags are wired through CLI/LSP/docs.
