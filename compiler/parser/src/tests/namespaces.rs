//! CODESYS/TwinCAT `NAMESPACE ... END_NAMESPACE` grouping and the
//! `__BEGIN_IMPLEMENTATION` marker. See
//! specs/design/st-declaration-extensions.md.

use super::common::*;
use crate::token::TokenType;
use spec_test_macro::spec_test;

fn token_types(source: &str, options: &CompilerOptions) -> Vec<TokenType> {
    let (tokens, _) = crate::tokenize_program(source, &FileId::default(), options, 0, 0);
    tokens.iter().map(|t| t.token_type.clone()).collect()
}

/// REQ-STX-parser-005: `NAMESPACE` and `END_NAMESPACE` lex as
/// `Namespace`/`EndNamespace` and demote to `Identifier` unless
/// `allow_namespace` is set.
#[spec_test(REQ_STX_parser_005)]
fn lexer_spec_req_stx_005_namespace_tokens_and_demotion() {
    let enabled = token_types("NAMESPACE END_NAMESPACE", &opts_with_namespace());
    assert!(enabled.contains(&TokenType::Namespace), "{enabled:?}");
    assert!(enabled.contains(&TokenType::EndNamespace), "{enabled:?}");

    let lower = token_types("namespace end_namespace", &opts_with_namespace());
    assert!(lower.contains(&TokenType::Namespace), "{lower:?}");
    assert!(lower.contains(&TokenType::EndNamespace), "{lower:?}");

    let disabled = token_types("NAMESPACE END_NAMESPACE", &CompilerOptions::default());
    assert_eq!(
        disabled
            .iter()
            .filter(|t| **t == TokenType::Identifier)
            .count(),
        2,
        "{disabled:?}"
    );
    assert!(!disabled.contains(&TokenType::Namespace));
    assert!(!disabled.contains(&TokenType::EndNamespace));
}

/// REQ-STX-parser-006: `__BEGIN_IMPLEMENTATION` lexes as
/// `BeginImplementation` and demotes to `Identifier` unless
/// `allow_begin_implementation` is set.
#[spec_test(REQ_STX_parser_006)]
fn lexer_spec_req_stx_006_begin_implementation_token_and_demotion() {
    let enabled = token_types("__BEGIN_IMPLEMENTATION", &opts_with_begin_implementation());
    assert!(
        enabled.contains(&TokenType::BeginImplementation),
        "{enabled:?}"
    );

    let lower = token_types("__begin_implementation", &opts_with_begin_implementation());
    assert!(lower.contains(&TokenType::BeginImplementation), "{lower:?}");

    let disabled = token_types("__BEGIN_IMPLEMENTATION", &CompilerOptions::default());
    assert!(disabled.contains(&TokenType::Identifier), "{disabled:?}");
    assert!(!disabled.contains(&TokenType::BeginImplementation));
}

/// REQ-STX-parser-015: `NAMESPACE name ... END_NAMESPACE` parses to
/// `LibraryElementKind::NamespaceDeclaration`, holding the name and the
/// declarations it contains; a namespace may contain another namespace.
#[spec_test(REQ_STX_parser_015)]
fn parser_spec_req_stx_015_namespace_declaration_ast_shape() {
    let source = "NAMESPACE Motor
TYPE Speed : INT; END_TYPE

FUNCTION Blocked : BOOL
    Blocked := TRUE;
END_FUNCTION

NAMESPACE inner
PROGRAM nested
END_PROGRAM
END_NAMESPACE
END_NAMESPACE
";
    let library = parse_program(source, &FileId::default(), &opts_with_namespace()).unwrap();
    assert_eq!(library.elements.len(), 1);
    let namespace = cast!(
        library.elements[0].clone(),
        LibraryElementKind::NamespaceDeclaration
    );
    assert_eq!(namespace.name, Id::from("Motor"));
    assert_eq!(namespace.elements.len(), 3);
    assert!(matches!(
        namespace.elements[0],
        LibraryElementKind::DataTypeDeclaration(_)
    ));
    assert!(matches!(
        namespace.elements[1],
        LibraryElementKind::FunctionDeclaration(_)
    ));
    let inner = cast!(
        namespace.elements[2].clone(),
        LibraryElementKind::NamespaceDeclaration
    );
    assert_eq!(inner.name, Id::from("inner"));
    assert_eq!(inner.elements.len(), 1);
    assert!(matches!(
        inner.elements[0],
        LibraryElementKind::ProgramDeclaration(_)
    ));
}

/// REQ-STX-parser-016: `__BEGIN_IMPLEMENTATION` in a statement list parses
/// to `StmtKind::BeginImplementation` and the statements after it stay in
/// the same list.
#[spec_test(REQ_STX_parser_016)]
fn parser_spec_req_stx_016_begin_implementation_marker() {
    let source = "PROGRAM main
VAR
    x : INT;
END_VAR
__BEGIN_IMPLEMENTATION
x := 1;
x := x + 1;
END_PROGRAM";
    let library = parse_program(
        source,
        &FileId::default(),
        &opts_with_begin_implementation(),
    )
    .unwrap();
    let program = cast!(&library.elements[0], LibraryElementKind::ProgramDeclaration);
    let statements = cast!(&program.body, FunctionBlockBodyKind::Statements);
    assert_eq!(statements.body.len(), 3);
    assert!(matches!(
        statements.body[0],
        StmtKind::BeginImplementation(_)
    ));
    assert!(matches!(statements.body[1], StmtKind::Assignment(_)));
    assert!(matches!(statements.body[2], StmtKind::Assignment(_)));
}

/// Without the flags both constructs are syntax errors: the words are plain
/// identifiers there, and an isolated identifier is not a statement.
#[test]
fn parse_when_namespace_and_marker_without_flags_then_err() {
    let namespace = "NAMESPACE ns
PROGRAM nested
END_PROGRAM
END_NAMESPACE";
    assert!(
        parse_program(namespace, &FileId::default(), &CompilerOptions::default()).is_err(),
        "NAMESPACE must not parse without allow_namespace"
    );

    let marker = "PROGRAM main
__BEGIN_IMPLEMENTATION
x := 1;
END_PROGRAM";
    assert!(
        parse_program(marker, &FileId::default(), &CompilerOptions::default()).is_err(),
        "__BEGIN_IMPLEMENTATION must not parse without allow_begin_implementation"
    );
}

/// The marker is accepted with and without a `;` after it -- the reference
/// grammar writes none, but a terminator is common in hand-written text --
/// and alone as the whole implementation, which is an empty body.
#[rstest]
#[case::no_terminator("__BEGIN_IMPLEMENTATION\nx := 1;\ny := 2;", 3)]
#[case::terminator("__BEGIN_IMPLEMENTATION;\nx := 1;\ny := 2;", 3)]
#[case::terminator_with_space("__BEGIN_IMPLEMENTATION ;\nx := 1;", 2)]
#[case::alone("__BEGIN_IMPLEMENTATION", 1)]
#[case::alone_with_terminator("__BEGIN_IMPLEMENTATION ;", 1)]
fn parse_when_marker_spelled_variously_then_marker_is_first_statement(
    #[case] body: &str,
    #[case] count: usize,
) {
    let source = format!("PROGRAM main\n{body}\nEND_PROGRAM");
    let library = parse_program(
        &source,
        &FileId::default(),
        &opts_with_begin_implementation(),
    )
    .unwrap_or_else(|e| panic!("Source did not parse: {e:?}\n{source}"));
    let program = cast!(&library.elements[0], LibraryElementKind::ProgramDeclaration);
    let statements = cast!(&program.body, FunctionBlockBodyKind::Statements);
    assert_eq!(statements.body.len(), count);
    assert!(matches!(
        statements.body[0],
        StmtKind::BeginImplementation(_)
    ));
}
