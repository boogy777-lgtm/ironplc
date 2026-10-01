//! CODESYS `UNION ... END_UNION` type declarations (standardized in
//! IEC 61131-3:2013). See specs/design/st-declaration-extensions.md.

use super::common::*;
use crate::token::TokenType;
use spec_test_macro::spec_test;

fn token_types(source: &str, options: &CompilerOptions) -> Vec<TokenType> {
    let (tokens, _) = crate::tokenize_program(source, &FileId::default(), options, 0, 0);
    tokens.iter().map(|t| t.token_type.clone()).collect()
}

/// REQ-STX-parser-001: `UNION` and `END_UNION` lex as `Union`/`EndUnion`,
/// case-insensitively, and demote to `Identifier` unless `allow_union_type`
/// is set.
#[spec_test(REQ_STX_parser_001)]
fn lexer_spec_req_stx_001_union_tokens_and_demotion() {
    let enabled = token_types("UNION END_UNION", &opts_with_union());
    assert!(enabled.contains(&TokenType::Union), "{enabled:?}");
    assert!(enabled.contains(&TokenType::EndUnion), "{enabled:?}");

    let lower = token_types("union end_union", &opts_with_union());
    assert!(lower.contains(&TokenType::Union), "{lower:?}");
    assert!(lower.contains(&TokenType::EndUnion), "{lower:?}");

    let disabled = token_types("UNION", &CompilerOptions::default());
    assert!(disabled.contains(&TokenType::Identifier), "{disabled:?}");
    assert!(!disabled.contains(&TokenType::Union));
}

/// REQ-STX-parser-010: `TYPE U : UNION ... END_UNION; END_TYPE` parses to
/// `DataTypeDeclarationKind::Union`, carrying the declared name and one
/// element per member.
#[spec_test(REQ_STX_parser_010)]
fn parser_spec_req_stx_010_union_declaration_ast_shape() {
    let source = "TYPE
U_Data :
UNION
    intVal : INT;
    realVal : REAL;
END_UNION;
END_TYPE";
    let library = parse_program(source, &FileId::default(), &opts_with_union()).unwrap();
    let declaration = cast!(
        library.elements[0].clone(),
        LibraryElementKind::DataTypeDeclaration
    );
    let union = cast!(&declaration, DataTypeDeclarationKind::Union);
    assert_eq!(union.type_name, TypeName::from("U_Data"));
    assert_eq!(union.elements.len(), 2);
    assert_eq!(union.elements[0].name, Id::from("intVal"));
    assert_eq!(union.elements[1].name, Id::from("realVal"));
    let first = cast!(&union.elements[0].init, InitialValueAssignmentKind::Simple);
    assert_eq!(first.type_name, TypeName::from("INT"));
}

/// REQ-STX-parser-011: union members use the structure element grammar, so
/// an initializer, an array, a string and a nested type name parse as they
/// do inside `STRUCT ... END_STRUCT`.
#[spec_test(REQ_STX_parser_011)]
fn parser_spec_req_stx_011_union_members_use_struct_element_grammar() {
    let source = "TYPE
U_Data :
UNION
    flag : BOOL := TRUE;
    bits : ARRAY[0..31] OF BOOL;
    name : STRING[20];
END_UNION;
END_TYPE";
    let library = parse_program(source, &FileId::default(), &opts_with_union()).unwrap();
    let declaration = cast!(
        library.elements[0].clone(),
        LibraryElementKind::DataTypeDeclaration
    );
    let union = cast!(&declaration, DataTypeDeclarationKind::Union);
    assert_eq!(union.elements.len(), 3);
    assert_eq!(union.elements[0].name, Id::from("flag"));
    let array = cast!(&union.elements[1].init, InitialValueAssignmentKind::Array);
    let subranges = cast!(&array.spec, SpecificationKind::Inline);
    assert_eq!(
        subranges.type_name,
        ArrayElementType::Named(TypeName::from("BOOL"))
    );
    let string = cast!(&union.elements[2].init, InitialValueAssignmentKind::String);
    assert_eq!(string.width, StringType::String);
    assert!(string.length.is_some());
}

/// A union declaration is a syntax error without `allow_union_type`: both
/// words are plain identifiers then, so a `TYPE` body of
/// `U_Data : UNION ...` cannot parse.
#[test]
fn parse_when_union_and_default_dialect_then_err() {
    let source = "TYPE
U_Data :
UNION
    intVal : INT;
END_UNION;
END_TYPE";
    let result = parse_program(source, &FileId::default(), &CompilerOptions::default());
    assert!(result.is_err());
}
