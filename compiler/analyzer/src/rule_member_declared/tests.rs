//! Tests of `rule_member_declared`: one row per kind of owner and per way the
//! member is written, each against the same declarations.

use ironplc_parser::options::{CompilerOptions, Dialect};
use ironplc_problems::Problem;
use rstest::rstest;

use super::apply;
use crate::test_helpers::parse_and_resolve_types_with_options;

/// Declarations every case uses. `Leaf` inherits from `Middle`, which inherits
/// from `Base`.
const DECLARATIONS: &str = "
TYPE S : STRUCT m : INT; END_STRUCT; END_TYPE
TYPE B : STRUCT c : INT; END_STRUCT; END_TYPE
TYPE A : STRUCT b : B; END_STRUCT; END_TYPE
TYPE RefS : REF_TO S; END_TYPE
TYPE U : UNION m : INT; n : BYTE; END_UNION; END_TYPE

FUNCTION_BLOCK FB1
VAR_INPUT a : INT; END_VAR
VAR_OUTPUT y : INT; inner : FBIN; END_VAR
VAR_IN_OUT io : INT; END_VAR
VAR loc : INT; text : STRING; list : ARRAY[1..2] OF INT; rec : A; END_VAR
VAR_TEMP t : INT; END_VAR
METHOD M : INT
    M := 1;
END_METHOD
PROPERTY Prop : INT
GET
    Prop := 1;
END_GET
END_PROPERTY
END_FUNCTION_BLOCK

FUNCTION_BLOCK FBIN
VAR_OUTPUT z : INT; END_VAR
END_FUNCTION_BLOCK

FUNCTION_BLOCK Base
VAR_OUTPUT base_out : INT; END_VAR
METHOD BaseMethod
END_METHOD
END_FUNCTION_BLOCK

FUNCTION_BLOCK Middle EXTENDS Base
VAR_OUTPUT middle_out : INT; END_VAR
END_FUNCTION_BLOCK

FUNCTION_BLOCK Leaf EXTENDS Middle
VAR_OUTPUT leaf_out : INT; END_VAR
END_FUNCTION_BLOCK
";

/// Declarations of the variables the statements use.
const VARIABLES: &str = "
VAR
    f : FB1; s : S; u : U; n : A; arr : ARRAY[1..3] OF S; fbs : ARRAY[1..3] OF FB1;
    leaf : Leaf; base : Base; t1 : TON; r : RefS; i : INT; x : INT;
END_VAR";

fn options() -> CompilerOptions {
    CompilerOptions {
        allow_union_type: true,
        allow_ref_to: true,
        ..CompilerOptions::from_dialect(Dialect::Codesys)
    }
}

/// The names that the rule reports, in the order it reports them, for a
/// program with `statements` as the body of `main`. Every diagnostic must be
/// `P4080`.
fn reported(statements: &str) -> Vec<String> {
    let program = format!("{DECLARATIONS}\nPROGRAM main\n{VARIABLES}\n{statements}\nEND_PROGRAM");
    let options = options();
    let (library, context) = parse_and_resolve_types_with_options(&program, &options);
    match apply(&library, &context, &options) {
        Ok(()) => Vec::new(),
        Err(diagnostics) => {
            assert!(
                diagnostics
                    .iter()
                    .all(|d| d.code == Problem::MemberNotDeclared.code()),
                "{diagnostics:?}"
            );
            diagnostics
                .iter()
                .map(|d| {
                    d.described
                        .iter()
                        .find_map(|item| item.strip_prefix("member="))
                        .unwrap_or("?")
                        .to_owned()
                })
                .collect()
        }
    }
}

#[rstest]
#[case::input_read("x := f.a;")]
#[case::input_write("f.a := 1;")]
#[case::output_read("x := f.y;")]
#[case::in_out_read("x := f.io;")]
#[case::local_read("x := f.loc;")]
#[case::local_write("f.loc := 1;")]
#[case::string_variable_of_the_block("x := LEN(f.text);")]
#[case::array_variable_of_the_block("x := f.list[1];")]
#[case::property_read("x := f.Prop;")]
#[case::property_write("f.Prop := 1;")]
#[case::method_call("x := f.M();")]
#[case::nested_block("x := f.inner.z;")]
#[case::structure_in_block("x := f.rec.b.c;")]
#[case::structure_read("x := s.m;")]
#[case::structure_write("s.m := 1;")]
#[case::union_read("x := u.m;")]
#[case::union_write("u.n := 1;")]
#[case::nested_structure("x := n.b.c;")]
#[case::array_element("x := arr[1].m;")]
#[case::array_of_blocks("x := fbs[1].y;")]
#[case::dereferenced_reference("x := r^.m;")]
#[case::standard_block("t1(IN := TRUE, PT := T#1s); IF t1.Q THEN x := 1; END_IF;")]
#[case::inherited_variable_of_the_base("x := leaf.base_out;")]
#[case::inherited_variable_of_the_parent("x := leaf.middle_out;")]
#[case::inherited_variable_written("leaf.base_out := 1;")]
#[case::inherited_method("leaf.BaseMethod();")]
#[case::own_variable_of_the_derived("x := leaf.leaf_out;")]
#[case::bit_access_is_not_a_member("x := i.3;")]
#[case::variable_that_is_not_declared("x := nowhere.foo;")]
fn apply_when_member_is_declared_then_ok(#[case] statements: &str) {
    assert_eq!(reported(statements), Vec::<String>::new());
}

#[rstest]
#[case::block_read("x := f.nothere;", &["nothere"])]
#[case::block_write("f.nothere := 1;", &["nothere"])]
#[case::block_temporary_is_no_member("x := f.t;", &["t"])]
#[case::block_temporary_written("f.t := 1;", &["t"])]
#[case::structure_read("x := s.nothere;", &["nothere"])]
#[case::structure_write("s.nothere := 1;", &["nothere"])]
#[case::union_read("x := u.nothere;", &["nothere"])]
#[case::union_write("u.nothere := 1;", &["nothere"])]
#[case::nested_structure_leaf("x := n.b.nothere;", &["nothere"])]
#[case::nested_structure_middle_reported_once("x := n.nothere.c;", &["nothere"])]
#[case::nested_structure_written("n.b.nothere := 1;", &["nothere"])]
#[case::array_element("x := arr[1].nothere;", &["nothere"])]
#[case::array_element_written("arr[2].nothere := 1;", &["nothere"])]
#[case::array_of_blocks("x := fbs[1].nothere;", &["nothere"])]
#[case::dereferenced_reference("x := r^.nothere;", &["nothere"])]
#[case::standard_block("IF t1.nothere THEN x := 1; END_IF;", &["nothere"])]
#[case::nested_block("x := f.inner.nothere;", &["nothere"])]
#[case::structure_in_block("x := f.rec.b.nothere;", &["nothere"])]
#[case::property_that_is_not_declared("x := f.NoProp;", &["NoProp"])]
#[case::variable_of_the_derived_not_in_the_base("x := base.leaf_out;", &["leaf_out"])]
#[case::variable_of_the_parent_not_in_the_base("x := base.middle_out;", &["middle_out"])]
#[case::inherited_name_that_is_not_declared("x := leaf.nothere;", &["nothere"])]
#[case::type_without_members("x := i.foo;", &["foo"])]
#[case::method_selected_as_variable("x := f.M;", &["M"])]
#[case::two_in_one_program("x := f.one; s.two := 1;", &["one", "two"])]
#[case::in_a_condition("IF s.nothere > 1 THEN x := 1; END_IF;", &["nothere"])]
#[case::in_an_argument("x := ABS(s.nothere);", &["nothere"])]
#[case::in_a_loop_bound("FOR x := 0 TO s.nothere DO i := 1; END_FOR;", &["nothere"])]
#[case::in_a_block_call_argument("f(a := s.nothere);", &["nothere"])]
fn apply_when_member_is_not_declared_then_p4080_at_the_member(
    #[case] statements: &str,
    #[case] expected: &[&str],
) {
    assert_eq!(reported(statements), expected);
}

#[test]
fn apply_when_member_is_not_declared_then_the_diagnostic_is_at_the_member_name() {
    let program = "PROGRAM main\nVAR s : S; x : INT; END_VAR\nx := s.nothere;\nEND_PROGRAM";
    let program = format!("TYPE S : STRUCT m : INT; END_STRUCT; END_TYPE\n{program}");
    let options = options();
    let (library, context) = parse_and_resolve_types_with_options(&program, &options);

    let diagnostics = apply(&library, &context, &options).unwrap_err();

    assert_eq!(diagnostics.len(), 1);
    let label = &diagnostics[0].primary;
    assert_eq!(
        &program[label.location.start..label.location.end],
        "nothere"
    );
}

#[test]
fn apply_when_member_name_is_close_to_a_declared_one_then_suggests_it() {
    let diagnostics = diagnostics_of(
        "TYPE S : STRUCT counter : INT; END_STRUCT; END_TYPE
         PROGRAM main VAR s : S; x : INT; END_VAR x := s.conter; END_PROGRAM",
    );

    assert!(diagnostics[0]
        .described
        .contains(&"did you mean=counter".to_owned()));
}

#[test]
fn apply_when_owner_type_is_not_declared_then_no_second_diagnostic() {
    assert_eq!(
        diagnostics_of("PROGRAM main VAR v : T_NOWHERE; x : INT; END_VAR x := v.foo; END_PROGRAM")
            .len(),
        0
    );
}

#[test]
fn apply_when_owner_extends_a_name_that_is_not_declared_then_members_are_not_checked() {
    assert_eq!(
        diagnostics_of(
            "FUNCTION_BLOCK D EXTENDS Nowhere VAR_OUTPUT d : INT; END_VAR END_FUNCTION_BLOCK
             PROGRAM main VAR f : D; x : INT; END_VAR x := f.from_the_base; END_PROGRAM"
        )
        .len(),
        0
    );
}

#[rstest]
#[case::structure_element("s : S := (m := 1);", 0)]
#[case::structure_element_not_declared("s : S := (nothere := 1);", 1)]
#[case::nested_structure_element("n : A := (b := (c := 1));", 0)]
#[case::nested_structure_element_not_declared("n : A := (b := (nothere := 1));", 1)]
#[case::block_input("t1 : TON := (PT := T#1s);", 0)]
#[case::block_member_not_declared("t1 : TON := (nothere := T#1s);", 1)]
#[case::user_block_member("f : FB1 := (a := 1);", 0)]
#[case::user_block_temporary("f : FB1 := (t := 1);", 1)]
fn apply_when_initializer_names_an_element_then_p4080_only_for_one_the_type_lacks(
    #[case] declaration: &str,
    #[case] expected: usize,
) {
    let program = format!("{DECLARATIONS}\nPROGRAM main VAR {declaration} END_VAR END_PROGRAM");
    assert_eq!(diagnostics_of(&program).len(), expected, "{program}");
}

fn diagnostics_of(program: &str) -> Vec<ironplc_dsl::diagnostic::Diagnostic> {
    let options = options();
    let (library, context) = parse_and_resolve_types_with_options(program, &options);
    apply(&library, &context, &options)
        .err()
        .unwrap_or_default()
}

/// The example in `docs/reference/compiler/problems/P4080.rst`; keep the two in
/// step.
#[test]
fn apply_when_documentation_example_then_p4080_for_each_bad_line() {
    let failing = "
TYPE Setpoint : STRUCT
    target : INT;
END_STRUCT; END_TYPE

FUNCTION_BLOCK Motor
VAR_OUTPUT running : BOOL; END_VAR
VAR_TEMP scratch : INT; END_VAR
END_FUNCTION_BLOCK

PROGRAM main
VAR m : Motor; sp : Setpoint; ok : BOOL; v : INT; END_VAR
ok := m.running;
ok := m.stopped;
v := m.scratch;
sp.limit := 10;
END_PROGRAM";
    let fixed = "
TYPE Setpoint : STRUCT
    target : INT;
    limit : INT;
END_STRUCT; END_TYPE

FUNCTION_BLOCK Motor
VAR_OUTPUT running : BOOL; stopped : BOOL; scratch : INT; END_VAR
END_FUNCTION_BLOCK

PROGRAM main
VAR m : Motor; sp : Setpoint; ok : BOOL; v : INT; END_VAR
ok := m.running;
ok := m.stopped;
v := m.scratch;
sp.limit := 10;
END_PROGRAM";

    assert_eq!(diagnostics_of(failing).len(), 3);
    assert_eq!(diagnostics_of(fixed).len(), 0);
}

#[test]
fn apply_when_derived_block_names_an_inherited_structure_then_its_members_are_checked() {
    let diagnostics = diagnostics_of(
        "TYPE S : STRUCT m : INT; END_STRUCT; END_TYPE
         FUNCTION_BLOCK Base VAR_OUTPUT rec : S; END_VAR END_FUNCTION_BLOCK
         FUNCTION_BLOCK Leaf EXTENDS Base
         METHOD Use
             rec.m := 1;
             rec.nothere := 1;
         END_METHOD
         END_FUNCTION_BLOCK",
    );

    assert_eq!(diagnostics.len(), 1);
    assert!(diagnostics[0]
        .described
        .contains(&"member=nothere".to_owned()));
}

#[test]
fn apply_when_value_of_an_enumeration_is_written_qualified_then_not_a_member() {
    assert_eq!(
        diagnostics_of(
            "TYPE E : (A, B); END_TYPE
             PROGRAM main VAR col : E; END_VAR col := E#B; col := E.A; END_PROGRAM"
        )
        .len(),
        0
    );
}
