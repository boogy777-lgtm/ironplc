//! A value written at every position that expects an enumeration, for each
//! outcome the rule distinguishes, under both policies of enumeration
//! strictness.
//!
//! Each row is a position (see `position::Position`) and the codes reported for
//! the five values written there:
//!
//! 1. a value of the enumeration the position expects: accepted;
//! 2. the same value written with its type: accepted;
//! 3. a value of another enumeration: P2006;
//! 4. a value of another enumeration written with its type: P4035 (P4026 for
//!    the argument of a function, which types its operands);
//! 5. a name no enumeration declares: P4007 (P2006 where a declaration states
//!    the value, which looks the name up among the values of the enumeration).
//!
//! Under the policy where an enumeration is not strict (the base type, see
//! `value_type::ValueUse`), the rows differ in one place only: a position whose
//! mismatch of types is reported by the check of the operands accepts a value
//! of another enumeration written with its type, as it accepts an `INT`. The
//! positions the rule reports itself do not depend on the policy.
//!
//! Arrays of enumerations pass `check` and stop in code generation, which has
//! no array of enumerations yet; the rows of arrays state what `check` says.

use ironplc_parser::options::{CompilerOptions, EnumerationStrictness};
use rstest::rstest;

use crate::stages::analyze;
use ironplc_dsl::core::FileId;
use ironplc_parser::parse_program;

const NONE: &[&str] = &[];
const P2006: &[&str] = &["P2006"];
const P4007: &[&str] = &["P4007"];
const P4026: &[&str] = &["P4026"];
const P4035: &[&str] = &["P4035"];
/// An unknown name in an initializer of a structure or a function block is a
/// general expression there, which a dialect gate refuses (P4043) beside the
/// name that is not declared (P4007).
const P4007_P4043: &[&str] = &["P4007", "P4043"];
/// The call-style initializer of a function block is not supported by code
/// generation yet.
const P9999: &[&str] = &["P9999"];
const P2006_P9999: &[&str] = &["P2006", "P9999"];
const P4035_P9999: &[&str] = &["P4035", "P9999"];
const P4007_P9999: &[&str] = &["P4007", "P9999"];

/// The values written: the first enumeration's own value, the same with its
/// type, a value of the other enumeration, the same with its type, and a name
/// no enumeration declares.
const VALUES: [&str; 5] = ["A", "E1#A", "C", "E2#C", "Nope"];

#[derive(Clone, Copy, Debug)]
enum Policy {
    Strict,
    NotStrict,
}

impl Policy {
    fn options(self) -> CompilerOptions {
        let mut options = CompilerOptions {
            allow_fb_inheritance: true,
            allow_fb_instance_arrays: true,
            ..CompilerOptions::default()
        };
        if matches!(self, Policy::NotStrict) {
            options.policy_enumeration_strictness = EnumerationStrictness::WithAttribute;
        }
        options
    }
}

/// The codes reported for the five values, under each policy.
#[derive(Clone, Copy)]
struct Expect {
    strict: [&'static [&'static str]; 5],
    not_strict: [&'static [&'static str]; 5],
}

impl Expect {
    const fn same(codes: [&'static [&'static str]; 5]) -> Self {
        Expect {
            strict: codes,
            not_strict: codes,
        }
    }

    fn of(self, policy: Policy, outcome: usize) -> &'static [&'static str] {
        match policy {
            Policy::Strict => self.strict[outcome],
            Policy::NotStrict => self.not_strict[outcome],
        }
    }
}

/// Reported by this rule at every position that expects the first enumeration.
const HERE: Expect = Expect::same([NONE, NONE, P2006, P4035, P4007]);
/// The same for a position that expects the second enumeration: `A` is the
/// other enumeration's value there.
const HERE_SECOND: Expect = Expect::same([P2006, P4035, NONE, NONE, P4007]);
/// The stored value of a declaration is looked up among the values of the
/// enumeration, so a name that is no value of it is P2006.
const DECLARED: Expect = Expect::same([NONE, NONE, P2006, P4035, P2006]);
const DECLARED_SECOND: Expect = Expect::same([P2006, P4035, NONE, NONE, P2006]);
/// A member of a structure or function block that is given a name which turns
/// out to be a general expression.
const MEMBER: Expect = Expect::same([NONE, NONE, P2006, P4035, P4007_P4043]);
/// An assignment to a variable: the check of the types compares a value
/// written with its type.
const ASSIGNED: Expect = Expect {
    strict: [NONE, NONE, P2006, P4035, P4007],
    not_strict: [NONE, NONE, P2006, NONE, P4007],
};
/// An argument of a function, whose parameters the check of the arguments
/// types.
const ARGUMENT: Expect = Expect {
    strict: [NONE, NONE, P2006, P4026, P4007],
    not_strict: [NONE, NONE, P2006, NONE, P4007],
};
/// An assignment to a variable of the second enumeration.
const ASSIGNED_SECOND: Expect = Expect {
    strict: [P2006, P4035, NONE, NONE, P4007],
    not_strict: [P2006, NONE, NONE, NONE, P4007],
};
/// A member of the second enumeration that is given a name which turns out to
/// be a general expression.
const MEMBER_SECOND: Expect = Expect::same([P2006, P4035, NONE, NONE, P4007_P4043]);
const CALL_STYLE: Expect = Expect::same([P9999, P9999, P2006_P9999, P4035_P9999, P4007_P9999]);

/// Where the value is written.
#[derive(Clone, Copy)]
enum Where {
    /// A statement of the program.
    Statement(&'static str),
    /// A variable declaration of the program.
    Variable(&'static str),
    /// A declaration in `TYPE`.
    Type(&'static str),
    /// The body of a function that returns the first enumeration.
    Result(&'static str),
}

const TYPES: &str = "E1 : (A, B); E2 : (C, D); EA : E1; EB : E2; \
    S : STRUCT m : E1; arr : ARRAY[0..1] OF E1; END_STRUCT; \
    T : STRUCT s : S; END_STRUCT; \
    SA : STRUCT m : EA; END_STRUCT; \
    SB : STRUCT m : EB; END_STRUCT; \
    Row : ARRAY[0..1] OF E1; Grid : ARRAY[0..1] OF Row; Mat : ARRAY[0..1, 0..1] OF E1; \
    AA : ARRAY[0..1] OF EA; AB : ARRAY[0..1] OF EB; AS : ARRAY[0..1] OF S;";

const BLOCKS: &str = "\
    FUNCTION_BLOCK FB VAR_INPUT p : E1; pa : EA; END_VAR VAR m : E1; END_VAR \
    METHOD M VAR_INPUT k : E1; END_VAR END_METHOD END_FUNCTION_BLOCK \
    FUNCTION F : BOOL VAR_INPUT v : E1; END_VAR F := TRUE; END_FUNCTION";

const VARIABLES: &str = "x : E1; xa : EA; xb : EB; s : S; t : T; row : Row; mat : Mat; \
    as_ : AS; g : BOOL; fb1 : FB; fbs : ARRAY[0..2] OF FB;";

fn program(place: Where, value: &str) -> String {
    let (mut type_declaration, mut variable, mut statement, mut function) =
        (String::new(), String::new(), String::new(), String::new());
    match place {
        Where::Statement(text) => statement = text.replace("{}", value),
        Where::Variable(text) => variable = text.replace("{}", value),
        Where::Type(text) => type_declaration = text.replace("{}", value),
        Where::Result(text) => {
            function = format!(
                "FUNCTION RF : E1 VAR_INPUT i : INT; END_VAR {} END_FUNCTION",
                text.replace("{}", value)
            )
        }
    }
    format!(
        "TYPE {TYPES} {type_declaration} END_TYPE {BLOCKS} {function} \
         PROGRAM main VAR {VARIABLES} {variable} END_VAR {statement} END_PROGRAM"
    )
}

fn codes_of(source: &str, options: &CompilerOptions) -> Vec<String> {
    let library = parse_program(source, &FileId::default(), options).unwrap();
    let (_library, context) = analyze(&[&library], options).unwrap();
    let mut codes: Vec<String> = context
        .diagnostics()
        .iter()
        .map(|diagnostic| diagnostic.code.clone())
        .collect();
    codes.sort();
    codes.dedup();
    codes
}

#[rstest]
// The target of an assignment.
#[case::target_variable(Where::Statement("x := {};"), ASSIGNED)]
#[case::target_alias_of_first(Where::Statement("xa := {};"), ASSIGNED)]
#[case::target_alias_of_second(Where::Statement("xb := {};"), ASSIGNED_SECOND)]
#[case::target_member(Where::Statement("s.m := {};"), HERE)]
#[case::target_member_of_member(Where::Statement("t.s.m := {};"), HERE)]
#[case::target_array_in_member(Where::Statement("s.arr[1] := {};"), HERE)]
#[case::target_element(Where::Statement("row[1] := {};"), HERE)]
#[case::target_element_of_matrix(Where::Statement("mat[0, 1] := {};"), HERE)]
#[case::target_member_of_element(Where::Statement("as_[0].m := {};"), HERE)]
#[case::target_input_of_instance(Where::Statement("fb1.p := {};"), HERE)]
#[case::target_function_result(Where::Result("RF := {};"), ASSIGNED)]
// The input of a function.
#[case::function_positional(Where::Statement("g := F({});"), ARGUMENT)]
#[case::function_named(Where::Statement("g := F(v := {});"), ARGUMENT)]
// The input of a function block, and of a method.
#[case::block_named(Where::Statement("fb1(p := {});"), HERE)]
#[case::block_positional(Where::Statement("fb1({}, A);"), HERE)]
#[case::block_named_alias(Where::Statement("fb1(pa := {});"), HERE)]
#[case::block_in_array(Where::Statement("fbs[1](p := {});"), HERE)]
#[case::method_named(Where::Statement("fb1.M(k := {});"), HERE)]
#[case::method_positional(Where::Statement("fb1.M({});"), HERE)]
// A label of a CASE, and an operand of a comparison.
#[case::case_label(Where::Statement("CASE x OF {}: g := TRUE; END_CASE;"), HERE)]
#[case::compared_on_the_right(Where::Statement("g := x = {};"), HERE)]
#[case::compared_on_the_left(Where::Statement("g := {} = x;"), HERE)]
// The initial value of a variable.
#[case::initial_value(Where::Variable("v : E1 := {};"), DECLARED)]
#[case::initial_value_alias_of_second(Where::Variable("v : EB := {};"), DECLARED_SECOND)]
// The member of a structure.
#[case::structure_member(Where::Variable("v : S := (m := {});"), MEMBER)]
#[case::structure_member_alias(Where::Variable("v : SA := (m := {});"), MEMBER)]
#[case::structure_member_alias_of_second(Where::Variable("v : SB := (m := {});"), MEMBER_SECOND)]
#[case::structure_member_nested(Where::Variable("v : T := (s := (m := {}));"), MEMBER)]
#[case::structure_member_array(Where::Variable("v : S := (arr := [{}, A]);"), HERE)]
#[case::structure_member_of_declared_type(Where::Type("V : S := (m := {});"), MEMBER)]
#[case::structure_member_default_in_type(
    Where::Type("V : STRUCT n : E1 := {}; END_STRUCT;"),
    DECLARED
)]
#[case::structure_member_structure_in_type(
    Where::Type("V : STRUCT n : S := (m := {}); END_STRUCT;"),
    MEMBER
)]
#[case::structure_member_array_in_type(
    Where::Type("V : STRUCT n : ARRAY[0..1] OF E1 := [A, {}]; END_STRUCT;"),
    HERE
)]
// The member of a function block instance, and the argument a call-style
// initializer passes to it.
#[case::block_member(Where::Variable("v : FB := (m := {});"), MEMBER)]
#[case::block_call_style(Where::Variable("v : FB(p := {});"), CALL_STYLE)]
// The element of an array, repeated or nested in an array of arrays.
#[case::element(Where::Variable("v : ARRAY[0..1] OF E1 := [{}, A];"), HERE)]
#[case::element_repeated(Where::Variable("v : ARRAY[0..1] OF E1 := [2({})];"), HERE)]
#[case::element_of_named_type(Where::Variable("v : Row := [A, {}];"), HERE)]
#[case::element_of_alias(Where::Variable("v : AA := [A, {}];"), HERE)]
#[case::element_of_alias_of_second(Where::Variable("v : AB := [C, {}];"), HERE_SECOND)]
#[case::element_of_array_of_arrays(Where::Variable("v : Grid := [A, A, {}, A];"), HERE)]
#[case::element_of_matrix(Where::Variable("v : Mat := [A, A, {}, A];"), HERE)]
#[case::element_in_type(Where::Type("V : ARRAY[0..1] OF E1 := [A, {}];"), HERE)]
fn apply_when_value_is_written_at_a_position_then_it_is_a_value_of_the_enumeration_the_position_expects(
    #[case] place: Where,
    #[case] expect: Expect,
    #[values(Policy::Strict, Policy::NotStrict)] policy: Policy,
    #[values(0, 1, 2, 3, 4)] outcome: usize,
) {
    let source = program(place, VALUES[outcome]);

    let codes = codes_of(&source, &policy.options());

    assert_eq!(codes, expect.of(policy, outcome), "{source}");
}
