//! Which enumeration declares a value name: the answer the type environment
//! gives for a bare value name, and for the name an enumeration is known by.

use crate::type_environment::{TypeEnvironment, ValueOwners};
use crate::{
    intermediate_type::{ByteSized, IntermediateType},
    type_attributes::TypeAttributes,
};
use ironplc_dsl::common::TypeName;
use ironplc_dsl::core::{Id, SourceSpan};

fn attributes(representation: IntermediateType) -> TypeAttributes {
    TypeAttributes::new(SourceSpan::default(), representation)
}

/// A name as declared at `start`: two declarations are two places.
fn at(name: &str, start: usize) -> Id {
    Id::from(name).with_position(SourceSpan::range(start, start + name.len()))
}

fn enumeration(values: &[Id]) -> TypeAttributes {
    attributes(IntermediateType::Enumeration {
        underlying_type: Box::new(IntermediateType::Int {
            size: ByteSized::B8,
        }),
    })
    .with_enumerated_values(values.to_vec())
}

/// An environment of the enumerations `declarations` lists, as the name each
/// is declared with and the names of its values.
fn environment(declarations: &[(&str, &[&str])]) -> TypeEnvironment {
    let mut env = TypeEnvironment::new();
    for (position, (name, values)) in declarations.iter().enumerate() {
        let start = position * 100;
        let values: Vec<Id> = values
            .iter()
            .enumerate()
            .map(|(index, value)| at(value, start + 10 + index * 10))
            .collect();
        env.insert_type(&TypeName::from_id(&at(name, start)), enumeration(&values));
    }
    env
}

/// The names of the enumerations that declare `value`, in the order declared.
fn declaring(env: &TypeEnvironment, value: &str) -> Vec<String> {
    match env.enumerations_declaring(&Id::from(value)) {
        ValueOwners::None => vec![],
        ValueOwners::One(owner) => vec![owner.enumeration.to_string()],
        ValueOwners::Several(owners) => owners
            .iter()
            .map(|owner| owner.enumeration.to_string())
            .collect(),
    }
}

#[test]
fn enumerated_values_when_two_enumerations_share_a_value_name_then_each_has_it() {
    let env = environment(&[("E1", &["U1", "U2"]), ("E2", &["U1", "U3"])]);

    assert_eq!(
        env.enumerated_values(&TypeName::from("E1")),
        Some([Id::from("U1"), Id::from("U2")].as_slice())
    );
    assert_eq!(
        env.enumerated_values(&TypeName::from("E2")),
        Some([Id::from("U1"), Id::from("U3")].as_slice())
    );
}

#[test]
fn enumerated_values_when_not_an_enumeration_then_none() {
    let mut env = TypeEnvironment::new();
    env.insert_type(&TypeName::from("FLAG"), attributes(IntermediateType::Bool));

    assert_eq!(env.enumerated_values(&TypeName::from("FLAG")), None);
    assert_eq!(env.enumerated_values(&TypeName::from("MISSING")), None);
}

#[test]
fn enumerations_declaring_when_no_enumeration_declares_the_name_then_none() {
    let env = environment(&[("E1", &["U1", "U2"])]);

    assert_eq!(
        env.enumerations_declaring(&Id::from("U9")),
        ValueOwners::None
    );
}

#[test]
fn enumerations_declaring_when_one_enumeration_declares_the_name_then_that_one() {
    let env = environment(&[("E1", &["U2", "U1"]), ("E2", &["U1", "U3"])]);

    assert_eq!(declaring(&env, "U2"), ["E1"]);
    assert_eq!(declaring(&env, "U3"), ["E2"]);
}

#[test]
fn enumerations_declaring_when_two_enumerations_declare_the_name_then_both_in_order() {
    let env = environment(&[("E1", &["U2", "U1"]), ("E2", &["U1", "U3"])]);

    assert_eq!(declaring(&env, "U1"), ["E1", "E2"]);
}

#[test]
fn enumerations_declaring_when_three_enumerations_declare_the_name_then_all_three() {
    let env = environment(&[
        ("E1", &["U1"]),
        ("E2", &["U1", "U2"]),
        ("E3", &["U3", "U1"]),
    ]);

    assert_eq!(declaring(&env, "U1"), ["E1", "E2", "E3"]);
}

#[test]
fn enumerations_declaring_when_name_differs_only_in_case_then_the_same_name() {
    let env = environment(&[("E1", &["Run"]), ("E2", &["RUN"])]);

    assert_eq!(declaring(&env, "run"), ["E1", "E2"]);
}

#[test]
fn enumerations_declaring_when_several_then_each_owner_points_at_its_own_value() {
    let env = environment(&[("E1", &["U2", "U1"]), ("E2", &["U1", "U3"])]);

    let ValueOwners::Several(owners) = env.enumerations_declaring(&Id::from("U1")) else {
        panic!("expected several owners");
    };

    // E1 declares U1 second (position 20 of its declaration), E2 first.
    assert_eq!(owners[0].value.span.start, 20);
    assert_eq!(owners[1].value.span.start, 110);
}

#[test]
fn enumerations_declaring_when_an_alias_names_the_enumeration_then_not_another_owner() {
    let mut env = environment(&[("E1", &["U1", "U2"]), ("E2", &["U3"])]);
    env.insert_alias(&TypeName::from("EA"), &TypeName::from("E1"))
        .unwrap();

    assert_eq!(declaring(&env, "U1"), ["E1"]);
}

#[test]
fn enumerations_declaring_when_an_alias_of_an_alias_then_still_the_declaration() {
    let mut env = environment(&[("E1", &["U1", "U2"]), ("E2", &["U1"])]);
    env.insert_alias(&TypeName::from("EA"), &TypeName::from("E1"))
        .unwrap();
    env.insert_alias(&TypeName::from("EB"), &TypeName::from("EA"))
        .unwrap();

    assert_eq!(declaring(&env, "U2"), ["E1"]);
    assert_eq!(declaring(&env, "U1"), ["E1", "E2"]);
}

#[test]
fn enumerations_declaring_when_two_declarations_list_the_same_values_then_two_enumerations() {
    let env = environment(&[("E1", &["U1", "U2"]), ("E2", &["U1", "U2"])]);

    assert_eq!(declaring(&env, "U2"), ["E1", "E2"]);
}

#[test]
fn enumerations_declaring_when_the_declaration_has_an_error_then_it_declares_nothing() {
    let mut env = environment(&[("E1", &["U1", "U2"])]);
    env.insert_failed(&TypeName::from("BAD"));

    assert_eq!(declaring(&env, "U1"), ["E1"]);
}

#[test]
fn enumerations_declaring_when_a_name_is_declared_twice_then_the_first_declaration_only() {
    let mut env = environment(&[("E1", &["U1"])]);
    // The repeat is dropped, and with it the values it lists.
    env.insert_type(
        &TypeName::from_id(&at("E1", 500)),
        enumeration(&[at("U1", 510), at("U7", 520)]),
    );

    assert_eq!(declaring(&env, "U1"), ["E1"]);
    assert_eq!(declaring(&env, "U7"), Vec::<String>::new());
}

#[test]
fn enumerations_declaring_when_a_type_is_not_an_enumeration_then_none() {
    let mut env = TypeEnvironment::new();
    env.insert_type(&TypeName::from("FLAG"), attributes(IntermediateType::Bool));

    assert_eq!(declaring(&env, "FLAG"), Vec::<String>::new());
}

#[test]
fn enumeration_declared_as_when_an_alias_then_the_name_of_the_declaration() {
    let mut env = environment(&[("E1", &["U1"])]);
    env.insert_alias(&TypeName::from("EA"), &TypeName::from("E1"))
        .unwrap();
    env.insert_alias(&TypeName::from("EB"), &TypeName::from("EA"))
        .unwrap();

    let declared = |name: &str| {
        env.enumeration_declared_as(&TypeName::from(name))
            .map(|declared| declared.to_string())
    };

    assert_eq!(declared("E1").as_deref(), Some("E1"));
    assert_eq!(declared("EA").as_deref(), Some("E1"));
    assert_eq!(declared("EB").as_deref(), Some("E1"));
}

#[test]
fn enumeration_declared_as_when_not_an_enumeration_then_none() {
    let mut env = environment(&[("E1", &["U1"])]);
    env.insert_type(&TypeName::from("FLAG"), attributes(IntermediateType::Bool));

    assert_eq!(env.enumeration_declared_as(&TypeName::from("FLAG")), None);
    assert_eq!(env.enumeration_declared_as(&TypeName::from("NOPE")), None);
}

#[test]
fn enumerations_declaring_when_entered_out_of_source_order_then_named_in_source_order() {
    let mut env = TypeEnvironment::new();
    // The analysis enters declarations in the order it sorted them.
    env.insert_type(
        &TypeName::from_id(&at("LATE", 500)),
        enumeration(&[at("U1", 510)]),
    );
    env.insert_type(
        &TypeName::from_id(&at("EARLY", 5)),
        enumeration(&[at("U1", 15)]),
    );

    assert_eq!(declaring(&env, "U1"), ["EARLY", "LATE"]);
}
