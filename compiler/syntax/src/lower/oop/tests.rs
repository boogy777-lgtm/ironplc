//! Tests of the members: a row of `QUALIFIERS`, `ACCESSORS` and `MEMBERS` for
//! each thing a function block may declare besides its body, and what each
//! makes of its parts.

use super::*;
use crate::lower::{disposition, lower_library, Disposition, INTERNAL_ERROR};
use crate::{parse_source_file, ParseOptions};
use ironplc_dsl::common::{FunctionBlockDeclaration, LibraryElementKind, StringType, VariableType};
use ironplc_dsl::core::FileId;

fn file() -> FileId {
    FileId::from_string("t.st")
}

/// The function block of `source`, lowered with every keyword enabled.
fn function_block(source: &str) -> FunctionBlockDeclaration {
    let parse = parse_source_file(source, &ParseOptions::all());
    assert!(parse.is_ok(), "{source}: {:?}", parse.errors);
    let library = lower_library(&parse, &file())
        .map_err(|error| format!("{source}: {}", error.primary.message))
        .expect("the source lowers");
    match library.elements.into_iter().next() {
        Some(LibraryElementKind::FunctionBlockDeclaration(block)) => Some(block),
        _ => None,
    }
    .expect("a function block")
}

/// The one method of a function block written as `FUNCTION_BLOCK fb {} END_FUNCTION_BLOCK`.
fn method(text: &str) -> MethodDeclaration {
    function_block(&format!("FUNCTION_BLOCK fb {text} END_FUNCTION_BLOCK"))
        .methods
        .remove(0)
}

fn property(text: &str) -> PropertyDeclaration {
    function_block(&format!("FUNCTION_BLOCK fb {text} END_FUNCTION_BLOCK"))
        .properties
        .remove(0)
}

fn kinds(qualifiers: &MemberQualifiers) -> Vec<MemberQualifierKind> {
    qualifiers.iter().map(|qualifier| qualifier.kind).collect()
}

// ---- Qualifiers.

#[test]
fn lower_qualifiers_when_each_word_then_the_kind_of_its_row_in_any_case() {
    for (word, kind) in QUALIFIERS {
        for spelling in [word.to_string(), word.to_lowercase()] {
            let lowered = method(&format!("METHOD {spelling} m END_METHOD")).qualifiers;
            assert_eq!(kinds(&lowered), [*kind], "{spelling}");
        }
    }
}

#[test]
fn qualifiers_when_each_row_then_the_word_is_the_keyword_of_its_kind() {
    for (word, kind) in QUALIFIERS {
        assert_eq!(kind.keyword(), *word);
    }
    // Every kind of qualifier has a row.
    assert_eq!(QUALIFIERS.len(), 8);
}

#[test]
fn lower_qualifiers_when_several_then_in_the_order_written_each_at_its_word() {
    let lowered = method("METHOD PUBLIC FINAL m END_METHOD");
    assert_eq!(
        kinds(&lowered.qualifiers),
        [
            MemberQualifierKind::Access(AccessSpecifier::Public),
            MemberQualifierKind::Final
        ]
    );
    let spans: Vec<(usize, usize)> = lowered
        .qualifiers
        .iter()
        .map(|qualifier| (qualifier.span.start, qualifier.span.end))
        .collect();
    assert_eq!(spans, [(25, 31), (32, 37)]);
}

#[test]
fn lower_qualifiers_when_word_is_the_name_then_no_qualifier() {
    let lowered = method("METHOD Override : BOOL END_METHOD");
    assert!(lowered.qualifiers.is_empty());
    assert_eq!(lowered.name.original(), "Override");
}

// ---- The facet of a function block.

#[test]
fn lower_oop_when_nothing_written_then_none() {
    assert!(function_block("FUNCTION_BLOCK fb END_FUNCTION_BLOCK")
        .oop
        .is_none());
}

#[test]
fn lower_oop_when_only_a_qualifier_then_a_facet_without_a_base_or_interfaces() {
    let oop = function_block("FUNCTION_BLOCK ABSTRACT fb END_FUNCTION_BLOCK")
        .oop
        .expect("a facet");
    assert!(oop.base.is_none() && oop.implements.is_empty());
    assert!(oop.qualifiers.is_abstract());
    assert_eq!((oop.span.start, oop.span.end), (15, 23));
}

#[test]
fn lower_oop_when_extends_and_implements_then_the_base_the_interfaces_and_the_range_from_first_to_last(
) {
    let oop =
        function_block("FUNCTION_BLOCK FINAL fb EXTENDS base IMPLEMENTS a, b END_FUNCTION_BLOCK")
            .oop
            .expect("a facet");
    assert_eq!(
        oop.base
            .as_ref()
            .map(|name| name.name.original().to_string()),
        Some("base".to_string())
    );
    let implements: Vec<String> = oop
        .implements
        .iter()
        .map(|name| name.name.original().to_string())
        .collect();
    assert_eq!(implements, ["a", "b"]);
    assert_eq!(kinds(&oop.qualifiers), [MemberQualifierKind::Final]);
    // From `FINAL` through the last interface.
    assert_eq!((oop.span.start, oop.span.end), (15, 52));
}

#[test]
fn lower_oop_when_extends_alone_then_the_range_is_the_clause() {
    let oop = function_block("FUNCTION_BLOCK fb EXTENDS base END_FUNCTION_BLOCK")
        .oop
        .expect("a facet");
    assert_eq!((oop.span.start, oop.span.end), (18, 30));
    assert!(oop.qualifiers.is_empty());
}

// ---- Methods.

#[test]
fn lower_method_when_header_blocks_and_body_then_each_part() {
    let lowered = method(
        "METHOD PRIVATE m : BOOL
VAR_INPUT a : INT; END_VAR
VAR_INST c : INT; END_VAR
m := TRUE;
END_METHOD",
    );
    assert_eq!(lowered.name.original(), "m");
    assert!(
        matches!(&lowered.return_type, Some(FunctionReturnType::Named(name)) if name.name.original() == "BOOL")
    );
    assert_eq!(lowered.variables.len(), 2);
    assert_eq!(lowered.variables[1].var_type, VariableType::VarInst);
    assert_eq!(lowered.body.len(), 1);
    assert_eq!(
        kinds(&lowered.qualifiers),
        [MemberQualifierKind::Access(AccessSpecifier::Private)]
    );
}

#[test]
fn lower_method_when_no_return_type_and_no_body_then_none_and_an_empty_body() {
    let lowered = method("METHOD ABSTRACT m END_METHOD");
    assert!(lowered.return_type.is_none());
    assert!(lowered.body.is_empty() && lowered.variables.is_empty());
}

#[test]
fn lower_method_when_string_return_type_then_the_specification() {
    let lowered = method("METHOD m : STRING[8] END_METHOD");
    assert!(matches!(
        &lowered.return_type,
        Some(FunctionReturnType::String(specification)) if specification.width == StringType::String
    ));
}

#[test]
fn lower_method_when_edge_variable_then_kept_apart_from_the_variables() {
    let lowered = method("METHOD m VAR_INPUT e : BOOL R_EDGE; END_VAR END_METHOD");
    assert!(lowered.variables.is_empty());
    assert_eq!(lowered.edge_variables.len(), 1);
}

#[test]
fn lower_method_when_declared_then_the_span_covers_from_its_keyword_to_its_closer() {
    let lowered = method("METHOD m END_METHOD");
    assert_eq!((lowered.span.start, lowered.span.end), (18, 37));
}

// ---- Properties.

#[test]
fn lower_property_when_both_accessors_then_a_method_each_named_after_the_property() {
    let lowered = property(
        "PROPERTY PUBLIC p : INT
GET p := 1; END_GET
SET VAR t : INT; END_VAR END_SET
END_PROPERTY",
    );
    assert_eq!(lowered.name.original(), "p");
    assert_eq!(
        kinds(&lowered.qualifiers),
        [MemberQualifierKind::Access(AccessSpecifier::Public)]
    );
    let get = lowered.get.as_ref().expect("a getter");
    assert_eq!(get.name.original(), "p");
    assert!(get.return_type.is_some());
    assert_eq!(get.body.len(), 1);
    let set = lowered.set.as_ref().expect("a setter");
    assert!(set.return_type.is_none());
    // The implicit input holding the assigned value comes first.
    assert_eq!(set.variables.len(), 2);
    assert_eq!(set.variables[0].var_type, VariableType::Input);
    assert_eq!(lowered.set_declared_variables().len(), 1);
}

#[test]
fn lower_property_when_one_accessor_then_the_other_is_none() {
    let getter = property("PROPERTY p : INT GET END_GET END_PROPERTY");
    assert!(getter.get.is_some() && getter.set.is_none());
    let setter = property("PROPERTY p : INT SET END_SET END_PROPERTY");
    assert!(setter.get.is_none() && setter.set.is_some());
    let neither = property("PROPERTY ABSTRACT p : INT END_PROPERTY");
    assert!(neither.get.is_none() && neither.set.is_none());
    assert!(neither.qualifiers.is_abstract());
}

#[test]
fn lower_property_when_declared_then_each_accessor_is_at_its_own_keywords() {
    let lowered = property("PROPERTY p : INT GET END_GET SET END_SET END_PROPERTY");
    let get = lowered.get.expect("a getter");
    let set = lowered.set.expect("a setter");
    assert_eq!((get.span.start, get.span.end), (35, 46));
    assert_eq!((set.span.start, set.span.end), (47, 58));
    assert_eq!((lowered.span.start, lowered.span.end), (18, 71));
}

#[test]
fn lower_property_when_string_type_then_the_accessors_carry_it() {
    let lowered = property("PROPERTY p : STRING[4] GET END_GET SET END_SET END_PROPERTY");
    assert!(matches!(
        lowered.property_type,
        FunctionReturnType::String(_)
    ));
    assert!(matches!(
        lowered
            .get
            .as_ref()
            .and_then(|get| get.return_type.as_ref()),
        Some(FunctionReturnType::String(_))
    ));
}

// ---- Members.

#[test]
fn lower_member_when_not_a_member_then_an_internal_error() {
    let parse = parse_source_file("PROGRAM p END_PROGRAM", &ParseOptions::all());
    let cx = LowerCx::new(file());
    let error = parse
        .root
        .first_child()
        .map(|node| lower_member(&cx, &node))
        .and_then(Result::err);
    assert_eq!(
        error.map(|error| error.code),
        Some(INTERNAL_ERROR.to_string())
    );
}

#[test]
fn disposition_when_member_kind_then_the_member_area_and_the_parts_structural() {
    assert_eq!(
        disposition(K::MethodDecl),
        Disposition::Lowered(crate::lower::Area::Member)
    );
    assert_eq!(disposition(K::MemberQualifier), Disposition::Structural);
}

#[test]
fn lower_member_when_abstract_qualifier_after_the_keyword_then_it_is_a_qualifier_and_not_a_label() {
    // The legacy grammar reads the declared name after `ABSTRACT` as a
    // statement label and rejects these; the tree reads the name by its
    // place in the declaration.
    let method = method("METHOD ABSTRACT m : INT END_METHOD");
    assert!(method.qualifiers.is_abstract());
    assert_eq!(method.name.original(), "m");
    let property = property("PROPERTY ABSTRACT p : INT END_PROPERTY");
    assert!(property.qualifiers.is_abstract());
    assert_eq!(property.name.original(), "p");
}
