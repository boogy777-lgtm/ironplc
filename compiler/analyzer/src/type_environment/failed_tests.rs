//! A declaration that has an error is entered as a declaration of its own: the
//! lookup, the error type and the repeated name.

use super::*;
use crate::{intermediate_type::IntermediateType, type_attributes::TypeAttributes};
use ironplc_dsl::core::SourceSpan;

fn named(name: &str, start: usize) -> TypeName {
    TypeName::from_id(
        &ironplc_dsl::core::Id::from(name).with_position(SourceSpan::range(start, start + 4)),
    )
}

#[test]
fn lookup_when_failed_declaration_then_failed_and_get_has_no_type() {
    let mut env = TypeEnvironment::new();
    env.insert_failed(&named("BAD", 0));

    assert!(matches!(env.lookup(&named("BAD", 20)), Resolved::Failed));
    assert!(env.get(&named("BAD", 20)).is_none());
}

#[test]
fn lookup_when_valid_declaration_then_valid() {
    let mut env = TypeEnvironment::new();
    env.insert_type(
        &named("GOOD", 0),
        TypeAttributes::new(SourceSpan::range(0, 4), IntermediateType::Bool),
    );

    assert!(matches!(env.lookup(&named("GOOD", 20)), Resolved::Valid(_)));
}

#[test]
fn lookup_when_not_declared_then_absent() {
    let env = TypeEnvironment::new();

    assert!(matches!(env.lookup(&named("NOPE", 0)), Resolved::Absent));
}

#[test]
fn is_error_when_id_of_failed_declaration_then_true_and_when_valid_then_false() {
    let mut env = TypeEnvironment::new();
    env.insert_failed(&named("BAD", 0));
    env.insert_type(
        &named("GOOD", 10),
        TypeAttributes::new(SourceSpan::range(10, 14), IntermediateType::Bool),
    );

    let bad = env.id_of(&named("BAD", 20)).unwrap();
    let good = env.id_of(&named("GOOD", 20)).unwrap();

    assert!(env.is_error(bad));
    assert!(!env.is_error(good));
    assert!(env.get_by_id(bad).is_none());
}

#[test]
fn insert_failed_when_the_same_declaration_is_entered_again_then_no_duplicate() {
    let mut env = TypeEnvironment::new();
    env.insert_failed(&named("BAD", 0));
    env.insert_failed(&named("BAD", 0));
    env.insert_type(
        &named("BAD", 0),
        TypeAttributes::new(SourceSpan::range(0, 4), IntermediateType::Bool),
    );

    assert!(env.take_duplicates().is_empty());
    assert!(matches!(env.lookup(&named("BAD", 0)), Resolved::Failed));
}

#[test]
fn insert_type_when_declaration_repeats_a_failed_one_then_p2007() {
    let mut env = TypeEnvironment::new();
    env.insert_failed(&named("BAD", 0));
    env.insert_type(
        &named("BAD", 10),
        TypeAttributes::new(SourceSpan::range(10, 14), IntermediateType::Bool),
    );

    let duplicates = env.take_duplicates();
    assert_eq!(duplicates.len(), 1);
    assert_eq!(duplicates[0].code, Problem::TypeDeclNameDuplicated.code());
    assert!(matches!(env.lookup(&named("BAD", 0)), Resolved::Failed));
}

#[test]
fn insert_failed_function_block_when_repeated_by_a_function_block_then_p4013() {
    let mut env = TypeEnvironment::new();
    env.insert_failed_function_block(&named("FB", 0));
    env.insert_failed_function_block(&named("FB", 10));

    let duplicates = env.take_duplicates();
    assert_eq!(duplicates.len(), 1);
    assert_eq!(duplicates[0].code, Problem::PouDeclNameDuplicated.code());
}

#[test]
fn iter_when_failed_declaration_then_not_listed() {
    let mut env = TypeEnvironment::new();
    env.insert_failed(&named("BAD", 0));

    assert_eq!(env.iter().count(), 0);
    assert_eq!(env.iter_ids().count(), 0);
}

#[test]
fn reference_target_when_named_target_is_declared_with_an_error_then_inherited() {
    let mut env = TypeEnvironment::new();
    env.insert_failed(&named("BROKEN", 0));

    let result = env.reference_target(
        &TypeName::from("REF"),
        &ReferenceTarget::Named(named("BROKEN", 20)),
    );

    assert!(matches!(result, Err(Failure::Inherited)));
}
