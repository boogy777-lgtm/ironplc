//! Lowering against objects built by hand, test-only: the declarations that
//! the legacy parser cannot be the oracle for.
//!
//! The legacy label pass does not count `ABSTRACT` as a qualifier word, so
//! after `METHOD ABSTRACT` or `PROPERTY ABSTRACT` it reads the declared name
//! and the `:` that follows as a statement label and rejects the declaration.
//! The new parser reads the name by its place in the declaration and accepts
//! them, so there is no legacy object to compare. The expected object of each
//! is written out here, with every position, and compared with the same strict
//! comparison as the others: the dump, every span and the block partition.

use super::ast::{compare, explain};
use super::legacy::presets;
use ironplc_dsl::common::{
    FunctionBlockBodyKind, FunctionBlockDeclaration, FunctionReturnType, Integer, IntegerRef,
    Library, LibraryElementKind, MethodDeclaration, PropertyDeclaration, StringSpecification,
    StringType, TypeName,
};
use ironplc_dsl::core::{FileId, Id, SourceSpan};
use ironplc_dsl::member_qualifier::{MemberQualifier, MemberQualifierKind, MemberQualifiers};
use ironplc_syntax::lower::lower_library;
use ironplc_syntax::parse_source_file;

fn file() -> FileId {
    FileId::from_string("hand.st")
}

/// The span of `needle` where it stands in `context`, which stands in
/// `source`.
fn span_in(source: &str, context: &str, needle: &str) -> SourceSpan {
    let start =
        source.find(context).unwrap_or_default() + context.rfind(needle).unwrap_or_default();
    SourceSpan::range(start, start + needle.len()).with_file_id(&file())
}

fn name(source: &str, context: &str, needle: &str) -> Id {
    Id::from(needle).with_position(span_in(source, context, needle))
}

fn abstract_qualifier(source: &str, context: &str) -> MemberQualifiers {
    MemberQualifiers::new(vec![MemberQualifier {
        kind: MemberQualifierKind::Abstract,
        span: span_in(source, context, "ABSTRACT"),
    }])
}

fn type_name(source: &str, context: &str, needle: &str) -> TypeName {
    TypeName {
        name: name(source, context, needle),
    }
}

/// A function block `fb` that declares only what the source declares
/// between its name and its closer.
fn function_block(
    source: &str,
    methods: Vec<MethodDeclaration>,
    properties: Vec<PropertyDeclaration>,
) -> Library {
    Library {
        elements: vec![LibraryElementKind::FunctionBlockDeclaration(
            FunctionBlockDeclaration {
                name: type_name(source, "FUNCTION_BLOCK fb", "fb"),
                variables: vec![],
                edge_variables: vec![],
                body: FunctionBlockBodyKind::empty(),
                span: SourceSpan::range(0, source.len()).with_file_id(&file()),
                oop: None,
                methods,
                properties,
            },
        )],
    }
}

/// The library the lowering builds for `source`, under every flag.
fn lowered(source: &str) -> Library {
    let preset = presets()
        .into_iter()
        .find(|preset| preset.name == "all-flags")
        .expect("the preset with every flag");
    let parse = parse_source_file(source, &preset.new);
    assert!(parse.is_ok(), "{source}: {:?}", parse.errors);
    lower_library(&parse, &file()).expect("the declaration lowers")
}

fn assert_same(source: &str, expected: &Library) {
    let lowered = lowered(source);
    assert_eq!(
        compare(expected, &lowered),
        vec![],
        "{source}: {}",
        explain(expected, &lowered)
    );
}

#[test]
fn lower_library_when_abstract_method_with_a_return_type_then_the_object_written_by_hand() {
    let source = "FUNCTION_BLOCK fb METHOD ABSTRACT m : INT END_METHOD END_FUNCTION_BLOCK";
    let method = MethodDeclaration {
        qualifiers: abstract_qualifier(source, "METHOD ABSTRACT m"),
        name: name(source, "METHOD ABSTRACT m", "m"),
        return_type: Some(FunctionReturnType::Named(type_name(
            source,
            ": INT END",
            "INT",
        ))),
        variables: vec![],
        edge_variables: vec![],
        body: vec![],
        span: span_in(
            source,
            "METHOD ABSTRACT m : INT END_METHOD",
            "METHOD ABSTRACT m : INT END_METHOD",
        ),
    };
    assert_same(source, &function_block(source, vec![method], vec![]));
}

#[test]
fn lower_library_when_abstract_property_then_the_object_written_by_hand() {
    let source = "FUNCTION_BLOCK fb PROPERTY ABSTRACT p : INT END_PROPERTY END_FUNCTION_BLOCK";
    let property = PropertyDeclaration {
        qualifiers: abstract_qualifier(source, "PROPERTY ABSTRACT p"),
        name: name(source, "PROPERTY ABSTRACT p", "p"),
        property_type: FunctionReturnType::Named(type_name(source, ": INT END", "INT")),
        get: None,
        set: None,
        span: span_in(
            source,
            "PROPERTY ABSTRACT p : INT END_PROPERTY",
            "PROPERTY ABSTRACT p : INT END_PROPERTY",
        ),
    };
    assert_same(source, &function_block(source, vec![], vec![property]));
}

#[test]
fn lower_library_when_abstract_property_with_a_string_type_then_the_object_written_by_hand() {
    let source =
        "FUNCTION_BLOCK fb PROPERTY ABSTRACT p : STRING[4] END_PROPERTY END_FUNCTION_BLOCK";
    let length = Integer::new("4", span_in(source, "STRING[4]", "4")).expect("a number");
    let property = PropertyDeclaration {
        qualifiers: abstract_qualifier(source, "PROPERTY ABSTRACT p"),
        name: name(source, "PROPERTY ABSTRACT p", "p"),
        property_type: FunctionReturnType::String(StringSpecification {
            width: StringType::String,
            length: Some(IntegerRef::Literal(length)),
            keyword_span: span_in(source, "STRING[4]", "STRING"),
        }),
        get: None,
        set: None,
        span: span_in(
            source,
            "PROPERTY ABSTRACT p : STRING[4] END_PROPERTY",
            "PROPERTY ABSTRACT p : STRING[4] END_PROPERTY",
        ),
    };
    assert_same(source, &function_block(source, vec![], vec![property]));
}

#[test]
fn compare_when_a_qualifier_is_missing_then_the_hand_built_object_differs() {
    // The comparison is what the three tests above rely on: an object built by
    // hand without the qualifier is not the lowered one.
    let source = "FUNCTION_BLOCK fb METHOD ABSTRACT m : INT END_METHOD END_FUNCTION_BLOCK";
    let method = MethodDeclaration {
        qualifiers: MemberQualifiers::default(),
        name: name(source, "METHOD ABSTRACT m", "m"),
        return_type: Some(FunctionReturnType::Named(type_name(
            source,
            ": INT END",
            "INT",
        ))),
        variables: vec![],
        edge_variables: vec![],
        body: vec![],
        span: span_in(
            source,
            "METHOD ABSTRACT m : INT END_METHOD",
            "METHOD ABSTRACT m : INT END_METHOD",
        ),
    };
    let expected = function_block(source, vec![method], vec![]);
    assert!(!compare(&expected, &lowered(source)).is_empty());
}
