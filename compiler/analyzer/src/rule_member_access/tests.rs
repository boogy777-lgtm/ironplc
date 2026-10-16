//! Tests of `rule_member_access`: one row per variable kind and per way the
//! member is accessed, from each place the code can be in, against the same
//! declarations.

use ironplc_parser::options::{CompilerOptions, Dialect};
use ironplc_problems::Problem;
use rstest::rstest;

use super::apply;
use crate::test_helpers::parse_and_resolve_types_with_options;

/// Declarations every case uses. `FB1` has a variable of every kind that an
/// instance holds, and a handle on another instance of its own type, which is
/// how its own code reaches an instance of it. `Derived` extends `FB1`.
const DECLARATIONS: &str = "
TYPE S : STRUCT m : INT; END_STRUCT; END_TYPE

FUNCTION INC : INT
VAR_INPUT step : INT; END_VAR
VAR_IN_OUT data : INT; END_VAR
    INC := data + step;
END_FUNCTION

FUNCTION_BLOCK Gen
VAR_INPUT i : INT; END_VAR
VAR_OUTPUT q : INT; END_VAR
VAR_IN_OUT io2 : INT; END_VAR
METHOD Meth : INT
VAR_IN_OUT mio : INT; END_VAR
    Meth := mio;
END_METHOD
END_FUNCTION_BLOCK

FUNCTION_BLOCK FB1
VAR_INPUT a : INT; END_VAR
VAR_OUTPUT y : INT; END_VAR
VAR_IN_OUT io : INT; END_VAR
VAR loc : INT; list : ARRAY[1..2] OF INT; rec : S; END_VAR
VAR_STAT st : INT; END_VAR
VAR_TEMP t : INT; END_VAR
END_FUNCTION_BLOCK

FUNCTION_BLOCK Derived EXTENDS FB1
VAR_OUTPUT own_out : INT; END_VAR
END_FUNCTION_BLOCK

FUNCTION_BLOCK Holder
VAR_INPUT in_inner : FB1; END_VAR
VAR_OUTPUT inner : FB1; END_VAR
END_FUNCTION_BLOCK
";

/// The instances the statements use, declared in `VAR` of the code that
/// outside code is written in.
const INSTANCES: &str = "
    f : FB1; g : Gen; fbs : ARRAY[1..2] OF FB1; h : Holder; t1 : TON; s : S;
    d : Derived; x : INT; r : INT;";

fn options() -> CompilerOptions {
    CompilerOptions::from_dialect(Dialect::Codesys)
}

/// The rows the rule reports for `source`, as `P#### member`, in order.
fn reported_in(source: &str) -> Vec<String> {
    let options = options();
    let (library, context) = parse_and_resolve_types_with_options(source, &options);
    match apply(&library, &context, &options) {
        Ok(()) => Vec::new(),
        Err(diagnostics) => diagnostics
            .iter()
            .map(|d| {
                let member = d
                    .described
                    .iter()
                    .find_map(|item| item.strip_prefix("member="))
                    .unwrap_or("?");
                format!("{} {member}", d.code)
            })
            .collect(),
    }
}

/// Code outside every function block: the body of a program.
fn from_program(statements: &str) -> Vec<String> {
    reported_in(&format!(
        "{DECLARATIONS}\nPROGRAM main VAR {INSTANCES} END_VAR\n{statements}\nEND_PROGRAM"
    ))
}

const OUTPUT: &str = "P4081";
const IN_OUT: &str = "P4082";

fn one(code: &str, member: &str) -> Vec<String> {
    vec![format!("{code} {member}")]
}

#[rstest]
// Inputs: free to read and write.
#[case::input_read("x := f.a;", vec![])]
#[case::input_write("f.a := 1;", vec![])]
// Outputs: read, not write.
#[case::output_read("x := f.y;", vec![])]
#[case::output_write("f.y := 1;", one(OUTPUT, "y"))]
// Internal variables: read, not write.
#[case::local_read("x := f.loc;", vec![])]
#[case::local_write("f.loc := 1;", one(OUTPUT, "loc"))]
#[case::static_read("x := f.st;", vec![])]
#[case::static_write("f.st := 1;", one(OUTPUT, "st"))]
// VAR_IN_OUT: neither.
#[case::in_out_read("x := f.io;", one(IN_OUT, "io"))]
#[case::in_out_write("f.io := 1;", one(IN_OUT, "io"))]
// VAR_TEMP is no member at all, which is P4080 and not this rule.
#[case::temporary_read("x := f.t;", vec![])]
#[case::temporary_write("f.t := 1;", vec![])]
// A part of a variable is a write to the variable.
#[case::array_element_of_local_written("f.list[1] := 1;", one(OUTPUT, "list"))]
#[case::array_element_of_local_read("x := f.list[1];", vec![])]
#[case::structure_element_of_local_written("f.rec.m := 1;", one(OUTPUT, "rec"))]
#[case::structure_element_of_local_read("x := f.rec.m;", vec![])]
#[case::bit_of_output_written("f.y.3 := TRUE;", one(OUTPUT, "y"))]
// An instance reached through another.
#[case::nested_output_written("h.inner.y := 1;", one(OUTPUT, "y"))]
#[case::nested_input_written("h.inner.a := 1;", vec![])]
#[case::nested_in_out_read("x := h.inner.io;", one(IN_OUT, "io"))]
#[case::nested_input_of_an_output_of_the_holder("h.in_inner.a := 1;", vec![])]
#[case::element_of_an_array_of_instances_written("fbs[1].loc := 1;", one(OUTPUT, "loc"))]
#[case::element_of_an_array_of_instances_read("x := fbs[2].loc;", vec![])]
// A structure is not a function block.
#[case::structure_element_written("s.m := 1;", vec![])]
// A standard function block.
#[case::standard_input_written("t1.PT := T#1s;", vec![])]
#[case::standard_output_written("t1.Q := TRUE;", one(OUTPUT, "Q"))]
#[case::standard_output_read("IF t1.Q THEN x := 1; END_IF;", vec![])]
// Inherited variables are members, declared in the block that extends.
#[case::inherited_output_written("d.y := 1;", one(OUTPUT, "y"))]
#[case::inherited_input_written("d.a := 1;", vec![])]
#[case::inherited_in_out_read("x := d.io;", one(IN_OUT, "io"))]
#[case::own_output_written("d.own_out := 1;", one(OUTPUT, "own_out"))]
// Two accesses are two diagnostics, and one access is one.
#[case::two_accesses("f.y := f.io;", vec![format!("{OUTPUT} y"), format!("{IN_OUT} io")])]
#[case::in_out_assigned_to_itself_is_two_accesses("f.io := f.io;", vec![format!("{IN_OUT} io"), format!("{IN_OUT} io")])]
fn apply_when_member_is_accessed_from_a_program_then_only_what_codesys_refuses_is_reported(
    #[case] statements: &str,
    #[case] expected: Vec<String>,
) {
    assert_eq!(from_program(statements), expected, "{statements}");
}

#[rstest]
// Calling a block is the way to give it an in-out variable and to take its outputs.
#[case::call_binds_in_out_to_a_variable("g(i := 1, io2 := x);", vec![])]
#[case::call_binds_input("g(i := f.a);", vec![])]
#[case::call_reads_output_into_a_variable("g(q => x);", vec![])]
#[case::call_of_the_block_with_its_own_in_out("f(io := x);", vec![])]
// Output binding targets are writes.
#[case::output_bound_to_an_input("g(q => f.a);", vec![])]
#[case::output_bound_to_an_output("g(q => f.y);", one(OUTPUT, "y"))]
#[case::output_bound_to_a_local("g(q => f.loc);", one(OUTPUT, "loc"))]
#[case::output_bound_to_an_in_out("g(q => f.io);", one(IN_OUT, "io"))]
// A VAR_IN_OUT argument writes.
#[case::block_call_in_out_argument_local("g(i := 1, io2 := f.loc);", one(OUTPUT, "loc"))]
#[case::block_call_in_out_argument_input("g(i := 1, io2 := f.a);", vec![])]
#[case::block_call_in_out_argument_in_out("g(i := 1, io2 := f.io);", one(IN_OUT, "io"))]
#[case::block_call_input_argument_local("g(i := f.loc);", vec![])]
#[case::block_call_input_argument_in_out("g(i := f.io);", one(IN_OUT, "io"))]
#[case::method_call_in_out_argument_local("x := g.Meth(mio := f.loc);", one(OUTPUT, "loc"))]
#[case::method_call_in_out_argument_input("x := g.Meth(mio := f.a);", vec![])]
#[case::function_call_in_out_argument_output("r := INC(1, f.y);", one(OUTPUT, "y"))]
#[case::function_call_in_out_argument_input("r := INC(1, f.a);", vec![])]
#[case::function_call_input_argument_output("r := INC(f.y, x);", vec![])]
#[case::function_call_in_out_argument_part_of_a_local(
    "r := INC(1, f.list[1]);",
    one(OUTPUT, "list")
)]
fn apply_when_member_is_a_call_argument_then_only_a_write_is_refused(
    #[case] statements: &str,
    #[case] expected: Vec<String>,
) {
    assert_eq!(from_program(statements), expected, "{statements}");
}

/// The places that are outside the function block `FB1`, each with the same
/// access to an instance `f` of it.
#[rstest]
#[case::program("PROGRAM main VAR f : FB1; END_VAR f.y := 1; END_PROGRAM")]
#[case::other_block("FUNCTION_BLOCK Other VAR f : FB1; END_VAR f.y := 1; END_FUNCTION_BLOCK")]
#[case::method_of_other_block(
    "FUNCTION_BLOCK Other
     METHOD Poke VAR f : FB1; END_VAR f.y := 1; END_METHOD
     END_FUNCTION_BLOCK"
)]
#[case::function("FUNCTION F : INT VAR_IN_OUT f : FB1; END_VAR f.y := 1; F := 0; END_FUNCTION")]
fn apply_when_code_is_outside_the_block_then_output_write_is_p4081(#[case] code: &str) {
    let source = format!("{DECLARATIONS}\n{code}");

    assert_eq!(reported_in(&source), one(OUTPUT, "y"), "{code}");
}

/// The places that are inside the function block `FB1`: its body, its methods,
/// and the body and the methods of a block that extends it. Each writes every
/// variable of an instance `f` of `FB1`, and reads it.
const EVERY_ACCESS: &str = "
    f.a := 1; f.y := 1; f.loc := 1; f.st := 1; f.io := 1;
    f.list[1] := 1; f.rec.m := 1; x := f.io; x := f.y;";

#[rstest]
#[case::derived_block_body(
    format!("FUNCTION_BLOCK Inside EXTENDS FB1
             VAR f : FB1; x : INT; END_VAR
             {EVERY_ACCESS}
             END_FUNCTION_BLOCK")
)]
#[case::derived_block_method(
    format!("FUNCTION_BLOCK Inside EXTENDS Derived
             METHOD Poke2 VAR f : FB1; x : INT; END_VAR {EVERY_ACCESS} END_METHOD
             END_FUNCTION_BLOCK")
)]
fn apply_when_code_is_inside_the_block_then_every_access_is_allowed(#[case] code: String) {
    let source = format!(
        "{DECLARATIONS}
{code}"
    );

    assert_eq!(reported_in(&source), Vec::<String>::new(), "{code}");
}

#[test]
fn apply_when_block_reaches_an_instance_of_its_own_type_through_a_reference_then_only_others_are_outside(
) {
    // A block cannot hold an instance of its own type, so its body reaches one
    // through a reference.
    let source = format!(
        "{DECLARATIONS}
FUNCTION_BLOCK Linked
VAR_INPUT next : REF_TO Linked; END_VAR
VAR count : INT; x : INT; END_VAR
next^.count := 1;
x := next^.count;
END_FUNCTION_BLOCK
FUNCTION_BLOCK Outsider
VAR_INPUT first : REF_TO Linked; END_VAR
first^.count := 1;
END_FUNCTION_BLOCK"
    );
    let options = CompilerOptions {
        allow_ref_to: true,
        ..options()
    };

    let (library, context) = parse_and_resolve_types_with_options(&source, &options);
    let diagnostics = apply(&library, &context, &options).unwrap_err();

    assert_eq!(diagnostics.len(), 1);
    assert_eq!(
        diagnostics[0].code,
        Problem::MemberNotAssignableOutside.code()
    );
}

#[test]
fn apply_when_block_extends_the_base_then_variables_of_the_base_are_inside_and_of_a_sibling_derived_are_not(
) {
    // `Sibling` does not extend `Derived` but shares `FB1` with it, so the
    // variables `FB1` declares are inside for it and the ones `Derived`
    // declares are not.
    let source = format!(
        "{DECLARATIONS}
FUNCTION_BLOCK Sibling EXTENDS FB1
VAR_IN_OUT d : Derived; END_VAR
d.loc := 1;
d.own_out := 1;
END_FUNCTION_BLOCK"
    );

    assert_eq!(reported_in(&source), one(OUTPUT, "own_out"));
}

#[test]
fn apply_when_member_is_refused_then_the_diagnostic_is_at_the_member_name() {
    let program =
        format!("{DECLARATIONS}\nPROGRAM main VAR f : FB1; END_VAR\nf.loc := 1;\nEND_PROGRAM");
    let options = options();
    let (library, context) = parse_and_resolve_types_with_options(&program, &options);

    let diagnostics = apply(&library, &context, &options).unwrap_err();

    assert_eq!(diagnostics.len(), 1);
    assert_eq!(
        diagnostics[0].code,
        Problem::MemberNotAssignableOutside.code()
    );
    let label = &diagnostics[0].primary;
    assert_eq!(&program[label.location.start..label.location.end], "loc");
}

#[test]
fn apply_when_in_out_is_read_then_the_problem_is_p4082_at_the_member_name() {
    let program = format!(
        "{DECLARATIONS}\nPROGRAM main VAR f : FB1; x : INT; END_VAR\nx := f.io;\nEND_PROGRAM"
    );
    let options = options();
    let (library, context) = parse_and_resolve_types_with_options(&program, &options);

    let diagnostics = apply(&library, &context, &options).unwrap_err();

    assert_eq!(diagnostics.len(), 1);
    assert_eq!(
        diagnostics[0].code,
        Problem::InOutMemberAccessedOutside.code()
    );
    let label = &diagnostics[0].primary;
    assert_eq!(&program[label.location.start..label.location.end], "io");
}

#[test]
fn apply_when_member_is_not_declared_then_this_rule_reports_nothing() {
    // The name is P4080's.
    assert_eq!(
        from_program("f.nothere := 1; x := f.nothere;"),
        Vec::<String>::new()
    );
}

#[test]
fn apply_when_instance_initializer_names_a_member_then_it_is_not_an_access() {
    // An initializer sets members from outside the block, as CODESYS lets it.
    let source = format!(
        "{DECLARATIONS}\nPROGRAM main VAR f : FB1 := (y := 1, loc := 2); END_VAR END_PROGRAM"
    );

    assert_eq!(reported_in(&source), Vec::<String>::new());
}

/// The example in `docs/reference/compiler/problems/P4081.rst`; keep the two in
/// speed.
#[test]
fn apply_when_p4081_documentation_example_then_each_bad_line_is_reported() {
    let failing = "
FUNCTION_BLOCK Counter
VAR_INPUT speed : INT; END_VAR
VAR_OUTPUT total : INT; END_VAR
VAR hidden : INT; END_VAR
END_FUNCTION_BLOCK

PROGRAM main
VAR c : Counter; x : INT; END_VAR
c.speed := 2;
x := c.total;
c.total := 0;
c.hidden := 1;
END_PROGRAM";
    let fixed = "
FUNCTION_BLOCK Counter
VAR_INPUT speed : INT; reset : BOOL; END_VAR
VAR_OUTPUT total : INT; END_VAR
VAR hidden : INT; END_VAR
END_FUNCTION_BLOCK

PROGRAM main
VAR c : Counter; x : INT; END_VAR
c.speed := 2;
x := c.total;
c.reset := TRUE;
END_PROGRAM";

    assert_eq!(
        reported_in(failing),
        vec![format!("{OUTPUT} total"), format!("{OUTPUT} hidden")]
    );
    assert_eq!(reported_in(fixed), Vec::<String>::new());
}

/// The example in `docs/reference/compiler/problems/P4082.rst`; keep the two in
/// step.
#[test]
fn apply_when_p4082_documentation_example_then_each_bad_line_is_reported() {
    let failing = "
FUNCTION_BLOCK Scaler
VAR_INPUT gain : INT; END_VAR
VAR_IN_OUT value : INT; END_VAR
    value := value * gain;
END_FUNCTION_BLOCK

PROGRAM main
VAR s : Scaler; reading : INT; x : INT; END_VAR
s.value := 5;
x := s.value;
s(gain := 2, value := reading);
END_PROGRAM";
    let fixed = "
FUNCTION_BLOCK Scaler
VAR_INPUT gain : INT; END_VAR
VAR_IN_OUT value : INT; END_VAR
    value := value * gain;
END_FUNCTION_BLOCK

PROGRAM main
VAR s : Scaler; reading : INT; x : INT; END_VAR
reading := 5;
s(gain := 2, value := reading);
x := reading;
END_PROGRAM";

    assert_eq!(
        reported_in(failing),
        vec![format!("{IN_OUT} value"), format!("{IN_OUT} value")]
    );
    assert_eq!(reported_in(fixed), Vec::<String>::new());
}
