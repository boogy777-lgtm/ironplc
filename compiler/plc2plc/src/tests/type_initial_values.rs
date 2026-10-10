//! Round-tripping of the initial values a type declares: a structure alias
//! with values (both names kept), and an expression as the value of a type or
//! of a structure member.

use super::common::*;

fn constants_enabled() -> CompilerOptions {
    CompilerOptions {
        allow_top_level_var_global: true,
        allow_constant_initializer_expressions: true,
        ..CompilerOptions::default()
    }
}

#[test]
fn write_to_string_when_structure_alias_with_values_then_keeps_both_names() {
    let source = "TYPE
    Pt : STRUCT x : INT; y : INT; END_STRUCT;
    P0 : Pt := (x := 1, y := 2);
END_TYPE
";
    let rendered = assert_round_trips(source, &CompilerOptions::default());
    assert!(rendered.contains("P0 : Pt := ("), "rendered:\n{rendered}");
}

#[test]
fn write_to_string_when_structure_alias_of_alias_then_keeps_each_base() {
    let source = "TYPE
    Pt : STRUCT x : INT; y : INT; END_STRUCT;
    P0 : Pt := (x := 1);
    P1 : P0 := (y := 2);
END_TYPE
";
    let rendered = assert_round_trips(source, &CompilerOptions::default());
    assert!(rendered.contains("P1 : P0 := ("), "rendered:\n{rendered}");
}

#[test]
fn write_to_string_when_type_value_is_a_constant_expression_then_round_trips() {
    let source = "
VAR_GLOBAL CONSTANT
    N : INT := 5;
END_VAR
TYPE
    Level : INT := N;
    Wide : INT := N * 2 + 1;
END_TYPE
";
    assert_round_trips(source, &constants_enabled());
}

#[test]
fn write_to_string_when_struct_member_value_is_a_constant_expression_then_round_trips() {
    let source = "
VAR_GLOBAL CONSTANT
    N : INT := 5;
END_VAR
TYPE S : STRUCT
    a : INT := N;
    b : INT := 2 + 3;
END_STRUCT;
END_TYPE
";
    assert_round_trips(source, &constants_enabled());
}

#[test]
fn write_to_string_when_enumeration_default_is_a_number_then_round_trips() {
    let source = "TYPE
    Level : (Low, Mid, High) := 1;
    Offset : (Down, Up) := -1;
END_TYPE
";
    let rendered = assert_round_trips(source, &CompilerOptions::default());
    assert!(
        rendered.contains(":= 1"),
        "rendered:
{rendered}"
    );
}
