//! Array declarations (of REFERENCE TO and of STRING).

use super::common::*;
use spec_test_macro::spec_test;

#[test]
fn parse_when_array_of_ref_to_then_ok() {
    let lib = parse_text_edition3(
        "PROGRAM main
VAR
    data : ARRAY[0..3] OF REF_TO BYTE;
END_VAR
END_PROGRAM",
    );
    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    assert_eq!(prog.variables.len(), 1);
    let arr = cast!(
        &prog.variables[0].initializer,
        InitialValueAssignmentKind::Array
    );
    let subranges = cast!(&arr.spec, SpecificationKind::Inline);
    assert!(subranges.ref_to.is_some());
    assert_eq!(subranges.type_name.to_type_name().to_string(), "BYTE");
    assert_eq!(subranges.ranges().len(), 1);
}

#[test]
fn parse_when_array_of_ref_to_type_decl_then_ok() {
    let lib = parse_text_edition3("TYPE MyArr : ARRAY[1..5] OF REF_TO INT; END_TYPE");
    let dt = cast!(&lib.elements[0], LibraryElementKind::DataTypeDeclaration);
    let arr = cast!(dt, DataTypeDeclarationKind::Array);
    let subranges = cast!(&arr.spec, SpecificationKind::Inline);
    assert!(subranges.ref_to.is_some());
    assert_eq!(subranges.type_name.to_type_name().to_string(), "INT");
}

#[test]
fn parse_when_array_without_ref_to_then_ref_to_is_false() {
    let lib = parse_text_edition3(
        "PROGRAM main
VAR
    data : ARRAY[0..3] OF BYTE;
END_VAR
END_PROGRAM",
    );
    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    let arr = cast!(
        &prog.variables[0].initializer,
        InitialValueAssignmentKind::Array
    );
    let subranges = cast!(&arr.spec, SpecificationKind::Inline);
    assert!(subranges.ref_to.is_none());
}

#[test]
fn parse_when_array_of_string_with_size_then_ok() {
    let lib = parse_text(
        "PROGRAM main
VAR
    names : ARRAY[1..3] OF STRING[10];
END_VAR
END_PROGRAM",
    );
    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    let arr = cast!(
        &prog.variables[0].initializer,
        InitialValueAssignmentKind::Array
    );
    let subranges = cast!(&arr.spec, SpecificationKind::Inline);
    assert_eq!(subranges.type_name.to_type_name().to_string(), "STRING");
    assert_eq!(subranges.ranges().len(), 1);
    let spec = cast!(&subranges.type_name, ArrayElementType::String);
    assert_eq!(
        spec.length.as_ref().unwrap().as_integer().unwrap().value,
        10
    );
}

#[test]
fn parse_when_array_of_wstring_with_size_then_ok() {
    let lib = parse_text(
        "PROGRAM main
VAR
    names : ARRAY[1..3] OF WSTRING[20];
END_VAR
END_PROGRAM",
    );
    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    let arr = cast!(
        &prog.variables[0].initializer,
        InitialValueAssignmentKind::Array
    );
    let subranges = cast!(&arr.spec, SpecificationKind::Inline);
    assert_eq!(subranges.type_name.to_type_name().to_string(), "WSTRING");
    let spec = cast!(&subranges.type_name, ArrayElementType::WString);
    assert_eq!(
        spec.length.as_ref().unwrap().as_integer().unwrap().value,
        20
    );
}

#[test]
fn parse_when_array_of_string_without_size_then_ok() {
    let lib = parse_text(
        "PROGRAM main
VAR
    names : ARRAY[1..3] OF STRING;
END_VAR
END_PROGRAM",
    );
    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    let arr = cast!(
        &prog.variables[0].initializer,
        InitialValueAssignmentKind::Array
    );
    let subranges = cast!(&arr.spec, SpecificationKind::Inline);
    let spec = cast!(&subranges.type_name, ArrayElementType::String);
    assert!(spec.length.is_none());
}

#[test]
fn parse_when_multidim_array_of_string_with_size_then_ok() {
    let lib = parse_text(
        "PROGRAM main
VAR
    weekdays : ARRAY[1..3, 1..7] OF STRING[10];
END_VAR
END_PROGRAM",
    );
    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    let arr = cast!(
        &prog.variables[0].initializer,
        InitialValueAssignmentKind::Array
    );
    let subranges = cast!(&arr.spec, SpecificationKind::Inline);
    assert_eq!(subranges.ranges().len(), 2);
    let spec = cast!(&subranges.type_name, ArrayElementType::String);
    assert_eq!(
        spec.length.as_ref().unwrap().as_integer().unwrap().value,
        10
    );
}

/// Parses `source` with `allow_incomplete_array` on.
fn parse_text_with_incomplete_array(source: &'static str) -> Library {
    let options = CompilerOptions {
        allow_incomplete_array: true,
        ..CompilerOptions::default()
    };
    let result = parse_program(source, &FileId::default(), &options);
    assert!(result.is_ok(), "parse failed: {:?}", result.err());
    result.unwrap()
}

/// REQ-CS-parser-003: `ARRAY[*] OF T` parses to `ArrayBounds::Incomplete`
/// with the element type recorded, and produces P4070 without the flag.
#[spec_test(REQ_CS_parser_003)]
fn parse_when_array_type_declaration_with_star_then_bounds_incomplete() {
    let lib = parse_text_with_incomplete_array("TYPE MyArr : ARRAY[*] OF INT; END_TYPE");
    let dt = cast!(&lib.elements[0], LibraryElementKind::DataTypeDeclaration);
    let arr = cast!(dt, DataTypeDeclarationKind::Array);
    let subranges = cast!(&arr.spec, SpecificationKind::Inline);
    assert_eq!(
        subranges.bounds,
        ArrayBounds::Incomplete(SourceSpan::default())
    );
    assert!(subranges.incomplete_span().is_some());
    // The star is not an empty range list: there are no ranges at all.
    assert!(subranges.ranges().is_empty());
    assert_eq!(subranges.type_name.to_type_name().to_string(), "INT");
}

#[test]
fn parse_when_array_variable_with_star_then_bounds_incomplete() {
    let lib = parse_text_with_incomplete_array(
        "PROGRAM main
VAR
    data : ARRAY[*] OF INT;
END_VAR
END_PROGRAM",
    );
    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    let arr = cast!(
        &prog.variables[0].initializer,
        InitialValueAssignmentKind::Array
    );
    let subranges = cast!(&arr.spec, SpecificationKind::Inline);
    assert!(subranges.incomplete_span().is_some());
}

#[test]
fn parse_when_incomplete_array_and_flag_off_then_requires_flag() {
    // The grammar accepts `*` in every dialect; the flag gate is a
    // token-stream rule, so the error is P4070 rather than a syntax error.
    let source = "TYPE MyArr : ARRAY[*] OF INT; END_TYPE";
    let result = parse_program(source, &FileId::default(), &CompilerOptions::default());
    assert!(result.is_err(), "expected error, got Ok");
    let d = result.unwrap_err();
    assert_eq!(
        d.code,
        "P4070",
        "expected P4070 IncompleteArrayNotAllowed, got {}: {}",
        d.code,
        d.description(),
    );
}
