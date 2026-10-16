//! Tests of `type_members`: which names each kind of type has.

use ironplc_dsl::core::Id;
use ironplc_parser::options::{CompilerOptions, Dialect};
use rstest::rstest;

use super::*;
use crate::callee_resolution::FunctionBlocks;
use crate::test_helpers::parse_and_resolve_types_with_options;
use crate::variable_type::resolve_initializer;

const SOURCE: &str = "
TYPE S : STRUCT m : INT; END_STRUCT; END_TYPE
FUNCTION_BLOCK Base
VAR_OUTPUT inherited : INT; END_VAR
METHOD BaseMethod
END_METHOD
END_FUNCTION_BLOCK
FUNCTION_BLOCK FB1 EXTENDS Base
VAR_INPUT a : INT; END_VAR
VAR text : STRING; END_VAR
VAR_TEMP t : INT; END_VAR
VAR_EXTERNAL g : INT; END_VAR
PROPERTY Prop : INT
GET
    Prop := 1;
END_GET
END_PROPERTY
END_FUNCTION_BLOCK
FUNCTION_BLOCK Orphan EXTENDS Nowhere
END_FUNCTION_BLOCK
PROGRAM main
VAR f : FB1; s : S; i : INT; t : TON; o : Orphan; END_VAR
END_PROGRAM";

/// The kind of the member `name` of the type of the variable `variable` in
/// `main`: `Err` with the kind of membership when the type is not searched.
fn kind_of(variable: &str, name: &str) -> &'static str {
    let options = CompilerOptions::from_dialect(Dialect::Codesys);
    let (library, context) = parse_and_resolve_types_with_options(SOURCE, &options);
    let members = TypeMembers::new(FunctionBlocks::from_library(&library), context.types());
    let owner = library
        .elements
        .iter()
        .find_map(|element| match element {
            ironplc_dsl::common::LibraryElementKind::ProgramDeclaration(program) => program
                .variables
                .iter()
                .find(|decl| decl.identifier.symbolic_id() == Some(&Id::from(variable)))
                .and_then(|decl| resolve_initializer(&decl.initializer, context.types())),
            _ => None,
        })
        .expect("the variable has a type");
    match members.of(&owner) {
        Membership::NoMembers => "no members",
        Membership::Unresolved => "unresolved",
        Membership::Members(found) => match found.find(&Id::from(name)) {
            None => "absent",
            Some(Member::Field(_)) => "field",
            Some(Member::Variable(_)) => "variable",
            Some(Member::Property(_)) => "property",
            Some(Member::Method(_)) => "method",
        },
    }
}

#[rstest]
#[case::structure_element("s", "m", "field")]
#[case::structure_element_is_case_insensitive("s", "M", "field")]
#[case::structure_without_the_name("s", "n", "absent")]
#[case::own_variable("f", "a", "variable")]
#[case::own_variable_of_a_string_type("f", "text", "variable")]
#[case::inherited_variable("f", "inherited", "variable")]
#[case::temporary_variable_is_no_member("f", "t", "absent")]
#[case::external_variable_is_no_member("f", "g", "absent")]
#[case::property("f", "Prop", "property")]
#[case::inherited_method("f", "BaseMethod", "method")]
#[case::standard_block_field("t", "Q", "field")]
#[case::standard_block_without_the_name("t", "nothere", "absent")]
#[case::elementary_type("i", "m", "no members")]
#[case::block_extending_an_undeclared_name("o", "anything", "unresolved")]
fn of_when_type_then_members_by_declaration(
    #[case] variable: &str,
    #[case] name: &str,
    #[case] expected: &str,
) {
    assert_eq!(kind_of(variable, name), expected);
}
