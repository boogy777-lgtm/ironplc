//! A recursive cycle, and a construct the sort does not support, are the error
//! of the declarations they name: reported, entered as declarations with an
//! error, and nothing else is stopped.

use super::*;
use crate::test_helpers::parse_only;
use ironplc_problems::Problem;

fn sorted(program: &str) -> (Sorted, TypeEnvironment) {
    let mut environment = TypeEnvironment::new();
    let sorted = super::apply(parse_only(program), &mut environment);
    (sorted, environment)
}

fn names_of_types(library: &Library) -> Vec<String> {
    library
        .elements
        .iter()
        .filter_map(|element| match element {
            LibraryElementKind::DataTypeDeclaration(decl) => {
                Some(data_type_name(decl).to_string())
            }
            _ => None,
        })
        .collect()
}

#[test]
fn apply_when_two_types_hold_each_other_then_one_diagnostic_naming_both() {
    let (sorted, _) = sorted(
        "
TYPE A : STRUCT b : B; END_STRUCT; END_TYPE
TYPE B : STRUCT a : A; END_STRUCT; END_TYPE",
    );

    assert_eq!(sorted.diagnostics.len(), 1);
    let diagnostic = &sorted.diagnostics[0];
    assert_eq!(diagnostic.code, Problem::RecursiveCycle.code());
    assert_eq!(diagnostic.primary.message, "Cycle");
    assert_eq!(diagnostic.secondary.len(), 1);
    assert!(diagnostic.description().contains("A, B"));
}

#[test]
fn apply_when_cycle_then_members_are_declarations_with_an_error_and_others_are_not() {
    let (sorted, environment) = sorted(
        "
TYPE A : STRUCT b : B; END_STRUCT; END_TYPE
TYPE B : STRUCT a : A; END_STRUCT; END_TYPE
TYPE C : STRUCT x : INT; END_STRUCT; END_TYPE
TYPE D : STRUCT a : A; END_STRUCT; END_TYPE",
    );

    assert!(environment.is_error(environment.id_of(&TypeName::from("A")).unwrap()));
    assert!(environment.is_error(environment.id_of(&TypeName::from("B")).unwrap()));
    assert!(environment.id_of(&TypeName::from("C")).is_none());
    assert!(environment.id_of(&TypeName::from("D")).is_none());
    assert_eq!(names_of_types(&sorted.library).len(), 4);
}

#[test]
fn apply_when_cycle_then_other_declarations_are_ordered_before_the_members() {
    let (sorted, _) = sorted(
        "
TYPE A : STRUCT b : B; END_STRUCT; END_TYPE
TYPE B : STRUCT a : A; END_STRUCT; END_TYPE
TYPE D : STRUCT c : C; END_STRUCT; END_TYPE
TYPE C : STRUCT x : INT; END_STRUCT; END_TYPE",
    );

    assert_eq!(names_of_types(&sorted.library), vec!["C", "D", "A", "B"]);
}

#[test]
fn apply_when_type_holds_itself_then_one_diagnostic_for_one_member() {
    let (sorted, environment) = sorted("TYPE S : STRUCT s : S; END_STRUCT; END_TYPE");

    assert_eq!(sorted.diagnostics.len(), 1);
    assert!(sorted.diagnostics[0].description().contains("members=S"));
    assert!(environment.is_error(environment.id_of(&TypeName::from("S")).unwrap()));
}

#[test]
fn apply_when_two_separate_cycles_then_a_diagnostic_for_each() {
    let (sorted, _) = sorted(
        "
TYPE A : STRUCT b : B; END_STRUCT; END_TYPE
TYPE B : STRUCT a : A; END_STRUCT; END_TYPE
TYPE C : STRUCT d : D; END_STRUCT; END_TYPE
TYPE D : STRUCT c : C; END_STRUCT; END_TYPE",
    );

    assert_eq!(sorted.diagnostics.len(), 2);
}

#[test]
fn apply_when_function_block_holds_itself_then_entered_as_a_failed_function_block() {
    let (sorted, mut environment) = sorted(
        "
FUNCTION_BLOCK F VAR i : F; END_VAR END_FUNCTION_BLOCK
FUNCTION_BLOCK G VAR x : INT; END_VAR END_FUNCTION_BLOCK",
    );

    assert_eq!(sorted.diagnostics.len(), 1);
    assert!(environment.is_error(environment.id_of(&TypeName::from("F")).unwrap()));
    assert!(environment.id_of(&TypeName::from("G")).is_none());
    // A function block is a program organization unit: a second
    // declaration of the name is P4013.
    environment.insert_failed_function_block(&TypeName::from("F"));
    assert_eq!(
        environment.take_duplicates()[0].code,
        Problem::PouDeclNameDuplicated.code()
    );
}

#[test]
fn apply_when_functions_call_each_other_then_reported_and_functions_stay_callable() {
    let (sorted, environment) = sorted(
        "
FUNCTION F : INT VAR_INPUT a : INT; END_VAR F := G(a); END_FUNCTION
FUNCTION G : INT VAR_INPUT a : INT; END_VAR G := F(a); END_FUNCTION
FUNCTION H : INT VAR_INPUT a : INT; END_VAR H := a; END_FUNCTION",
    );

    assert_eq!(sorted.diagnostics.len(), 1);
    assert_eq!(sorted.library.elements.len(), 3);
    // A function is not a type, and its signature does not depend on the
    // order of the calls: nothing is entered for it.
    assert!(environment.id_of(&TypeName::from("F")).is_none());
}

#[test]
fn apply_when_cycle_then_programs_that_use_it_are_kept() {
    let (sorted, _) = sorted(
        "
TYPE A : STRUCT b : B; END_STRUCT; END_TYPE
TYPE B : STRUCT a : A; END_STRUCT; END_TYPE
PROGRAM main VAR v : A; END_VAR END_PROGRAM",
    );

    assert_eq!(sorted.library.elements.len(), 3);
    assert!(sorted.reachable.contains(&Id::from("main")));
}

#[test]
fn visit_function_when_no_declaration_is_being_visited_then_reported_for_its_unit_and_walk_goes_on() {
    let mut visitor = RuleGraphReferenceableElements::new();
    visitor.unit = Some(Id::from("T"));
    let call = ironplc_dsl::textual::Function {
        name: Id::from("F"),
        param_assignment: vec![],
    };

    let Ok(()) = visitor.visit_function(&call);

    assert_eq!(visitor.diagnostics.len(), 1);
    assert_eq!(visitor.diagnostics[0].code, "P9999");
    assert_eq!(visitor.unsupported_in, vec![Id::from("T")]);
}

#[test]
fn visit_function_block_initial_value_assignment_when_no_declaration_is_being_visited_then_reported_for_its_unit(
) {
    let mut visitor = RuleGraphReferenceableElements::new();
    visitor.unit = Some(Id::from("T"));
    let init = FunctionBlockInitialValueAssignment {
        type_name: TypeName::from("FB"),
        init: vec![],
    };

    let Ok(()) = visitor.visit_function_block_initial_value_assignment(&init);

    assert_eq!(visitor.diagnostics.len(), 1);
    assert_eq!(visitor.unsupported_in, vec![Id::from("T")]);
}

#[test]
fn declares_enter_failed_when_function_or_unit_then_nothing_is_entered() {
    let mut environment = TypeEnvironment::new();

    Declares::Function.enter_failed(&Id::from("F"), &mut environment);
    Declares::Unit.enter_failed(&Id::from("P"), &mut environment);

    assert!(environment.id_of(&TypeName::from("F")).is_none());
    assert!(environment.id_of(&TypeName::from("P")).is_none());
}

#[test]
fn enter_when_the_environment_is_made_again_then_the_members_are_declarations_with_an_error() {
    let (sorted, _) = sorted(
        "
TYPE A : STRUCT b : B; END_STRUCT; END_TYPE
TYPE B : STRUCT a : A; END_STRUCT; END_TYPE
TYPE C : STRUCT x : INT; END_STRUCT; END_TYPE",
    );
    let mut again = TypeEnvironment::new();

    sorted.failed.enter(&mut again);

    assert!(again.is_error(again.id_of(&TypeName::from("A")).unwrap()));
    assert!(again.is_error(again.id_of(&TypeName::from("B")).unwrap()));
    assert!(again.id_of(&TypeName::from("C")).is_none());
}
