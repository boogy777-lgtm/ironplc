//! Tests for the standard library function definitions and their registration.

use super::*;
use ironplc_dsl::common::FunctionReturnType;
use rstest::rstest;

#[rstest]
#[case::sint("STRING_TO_SINT", "SINT")]
#[case::int("STRING_TO_INT", "INT")]
#[case::dint("STRING_TO_DINT", "DINT")]
#[case::usint("STRING_TO_USINT", "USINT")]
#[case::uint("STRING_TO_UINT", "UINT")]
#[case::udint("STRING_TO_UDINT", "UDINT")]
#[case::byte("STRING_TO_BYTE", "BYTE")]
#[case::word("STRING_TO_WORD", "WORD")]
#[case::dword("STRING_TO_DWORD", "DWORD")]
#[case::lint("STRING_TO_LINT", "LINT")]
#[case::ulint("STRING_TO_ULINT", "ULINT")]
#[case::lword("STRING_TO_LWORD", "LWORD")]
#[case::real("STRING_TO_REAL", "REAL")]
#[case::lreal("STRING_TO_LREAL", "LREAL")]
fn get_string_conversion_functions_when_string_to_integer_then_registered_with_target_return(
    #[case] name: &str,
    #[case] target: &str,
) {
    let functions = get_string_conversion_functions();
    let sig = functions
        .iter()
        .find(|f| f.name.original() == name)
        .unwrap();
    assert_eq!(sig.parameters.len(), 1);
    assert_eq!(sig.parameters[0].param_type, TypeName::from("STRING"));
    assert_eq!(
        sig.return_type,
        Some(FunctionReturnType::Named(TypeName::from(target)))
    );
}

#[test]
fn build_conversion_function_when_called_then_has_correct_signature() {
    let sig = build_conversion_function("INT", "REAL");

    assert_eq!(sig.name.original(), "INT_TO_REAL");
    assert!(sig.is_stdlib());
    assert_eq!(sig.parameters.len(), 1);
    assert_eq!(sig.parameters[0].name.original(), "IN");
    assert!(sig.parameters[0].is_input);
    // Parameter type is now TypeName, not IntermediateType
    assert_eq!(sig.parameters[0].param_type, TypeName::from("INT"));
    // Return type is now TypeName, not IntermediateType
    assert_eq!(
        sig.return_type,
        Some(FunctionReturnType::Named(TypeName::from("REAL")))
    );
}

#[test]
fn get_int_to_int_conversions_contains_expected_functions() {
    let functions = get_int_to_int_conversions();

    // Check some specific conversions exist
    assert!(functions.iter().any(|f| f.name.original() == "INT_TO_DINT"));
    assert!(functions.iter().any(|f| f.name.original() == "DINT_TO_INT"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "SINT_TO_LINT"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "UINT_TO_UDINT"));
    assert!(functions.iter().any(|f| f.name.original() == "INT_TO_UINT"));
    assert!(functions.iter().any(|f| f.name.original() == "UINT_TO_INT"));

    // Self-conversions should not exist
    assert!(!functions.iter().any(|f| f.name.original() == "INT_TO_INT"));
    assert!(!functions
        .iter()
        .any(|f| f.name.original() == "DINT_TO_DINT"));
}

#[test]
fn get_int_to_real_conversions_contains_expected_functions() {
    let functions = get_int_to_real_conversions();

    assert!(functions.iter().any(|f| f.name.original() == "INT_TO_REAL"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "INT_TO_LREAL"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "DINT_TO_REAL"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "UINT_TO_REAL"));
}

#[test]
fn get_real_to_int_conversions_contains_expected_functions() {
    let functions = get_real_to_int_conversions();

    assert!(functions.iter().any(|f| f.name.original() == "REAL_TO_INT"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "LREAL_TO_INT"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "REAL_TO_DINT"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "LREAL_TO_UINT"));
}

#[test]
fn get_real_to_real_conversions_contains_expected_functions() {
    let functions = get_real_to_real_conversions();

    assert_eq!(functions.len(), 2);
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "REAL_TO_LREAL"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "LREAL_TO_REAL"));
}

#[test]
fn get_selection_functions_when_min_then_has_two_inputs() {
    let functions = get_selection_functions();
    let min = functions
        .iter()
        .find(|f| f.name.original() == "MIN")
        .unwrap();

    assert_eq!(min.input_parameter_count(), 2);
    assert_eq!(min.parameters[0].name.original(), "IN1");
    assert_eq!(min.parameters[1].name.original(), "IN2");
    assert!(min.is_stdlib());
}

#[test]
fn get_selection_functions_when_max_then_has_two_inputs() {
    let functions = get_selection_functions();
    let max = functions
        .iter()
        .find(|f| f.name.original() == "MAX")
        .unwrap();

    assert_eq!(max.input_parameter_count(), 2);
    assert_eq!(max.parameters[0].name.original(), "IN1");
    assert_eq!(max.parameters[1].name.original(), "IN2");
    assert!(max.is_stdlib());
}

#[test]
fn get_selection_functions_when_limit_then_has_three_inputs() {
    let functions = get_selection_functions();
    let limit = functions
        .iter()
        .find(|f| f.name.original() == "LIMIT")
        .unwrap();

    assert_eq!(limit.input_parameter_count(), 3);
    assert_eq!(limit.parameters[0].name.original(), "MN");
    assert_eq!(limit.parameters[1].name.original(), "IN");
    assert_eq!(limit.parameters[2].name.original(), "MX");
    assert!(limit.is_stdlib());
}

#[test]
fn get_selection_functions_when_sel_then_has_three_inputs() {
    let functions = get_selection_functions();
    let sel = functions
        .iter()
        .find(|f| f.name.original() == "SEL")
        .unwrap();

    assert_eq!(sel.input_parameter_count(), 3);
    assert_eq!(sel.parameters[0].name.original(), "G");
    assert_eq!(sel.parameters[1].name.original(), "IN0");
    assert_eq!(sel.parameters[2].name.original(), "IN1");
    assert!(sel.is_stdlib());
}

#[test]
fn get_selection_functions_when_called_then_min_and_max_compare_and_the_others_hold() {
    use crate::value_type::ValueUse;
    let functions = get_selection_functions();

    for f in &functions {
        let expected = match f.name.original().as_str() {
            "MIN" | "MAX" => ValueUse::Compares,
            _ => ValueUse::Holds,
        };
        assert_eq!(f.value_use, expected, "{}", f.name);
    }
}

#[test]
fn get_selection_functions_when_called_then_contains_every_selection_function() {
    let functions = get_selection_functions();

    assert_eq!(functions.len(), 5);
    assert!(["MIN", "MAX", "LIMIT", "SEL", "MUX"]
        .iter()
        .all(|name| functions.iter().any(|f| f.name.original() == *name)));
}

#[test]
fn get_selection_functions_when_mux_then_has_three_minimum_inputs() {
    let functions = get_selection_functions();
    let mux = functions
        .iter()
        .find(|f| f.name.original() == "MUX")
        .unwrap();

    assert_eq!(mux.input_parameter_count(), 3);
    assert_eq!(mux.parameters[0].name.original(), "K");
    assert_eq!(mux.parameters[1].name.original(), "IN0");
    assert_eq!(mux.parameters[2].name.original(), "IN1");
    assert!(mux.is_stdlib());
    assert!(mux.is_extensible);
}

#[test]
fn get_bool_to_int_conversions_when_called_then_contains_all_targets() {
    let functions = get_bool_to_int_conversions();

    assert_eq!(functions.len(), 8);
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "BOOL_TO_SINT"));
    assert!(functions.iter().any(|f| f.name.original() == "BOOL_TO_INT"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "BOOL_TO_DINT"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "BOOL_TO_LINT"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "BOOL_TO_USINT"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "BOOL_TO_UINT"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "BOOL_TO_UDINT"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "BOOL_TO_ULINT"));
}

#[test]
fn get_bool_to_int_conversions_when_called_then_has_correct_signature() {
    let functions = get_bool_to_int_conversions();
    let bool_to_int = functions
        .iter()
        .find(|f| f.name.original() == "BOOL_TO_INT")
        .unwrap();

    assert_eq!(bool_to_int.input_parameter_count(), 1);
    assert_eq!(bool_to_int.parameters[0].name.original(), "IN");
    assert_eq!(bool_to_int.parameters[0].param_type, TypeName::from("BOOL"));
    assert_eq!(
        bool_to_int.return_type,
        Some(FunctionReturnType::Named(TypeName::from("INT")))
    );
    assert!(bool_to_int.is_stdlib());
}

#[test]
fn get_int_to_bool_conversions_when_called_then_contains_all_sources() {
    let functions = get_int_to_bool_conversions();

    assert_eq!(functions.len(), 8);
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "SINT_TO_BOOL"));
    assert!(functions.iter().any(|f| f.name.original() == "INT_TO_BOOL"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "DINT_TO_BOOL"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "LINT_TO_BOOL"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "USINT_TO_BOOL"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "UINT_TO_BOOL"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "UDINT_TO_BOOL"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "ULINT_TO_BOOL"));
}

#[test]
fn get_int_to_bool_conversions_when_called_then_has_correct_signature() {
    let functions = get_int_to_bool_conversions();
    let int_to_bool = functions
        .iter()
        .find(|f| f.name.original() == "INT_TO_BOOL")
        .unwrap();

    assert_eq!(int_to_bool.input_parameter_count(), 1);
    assert_eq!(int_to_bool.parameters[0].name.original(), "IN");
    assert_eq!(int_to_bool.parameters[0].param_type, TypeName::from("INT"));
    assert_eq!(
        int_to_bool.return_type,
        Some(FunctionReturnType::Named(TypeName::from("BOOL")))
    );
    assert!(int_to_bool.is_stdlib());
}

#[test]
fn get_bit_string_to_bit_string_conversions_when_called_then_contains_expected() {
    let functions = get_bit_string_to_bit_string_conversions();

    assert_eq!(functions.len(), 12);
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "BYTE_TO_WORD"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "WORD_TO_BYTE"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "DWORD_TO_LWORD"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "LWORD_TO_BYTE"));
}

#[test]
fn get_bit_string_to_bit_string_conversions_when_called_then_has_correct_signature() {
    let functions = get_bit_string_to_bit_string_conversions();
    let byte_to_word = functions
        .iter()
        .find(|f| f.name.original() == "BYTE_TO_WORD")
        .unwrap();

    assert_eq!(byte_to_word.input_parameter_count(), 1);
    assert_eq!(byte_to_word.parameters[0].name.original(), "IN");
    assert_eq!(
        byte_to_word.parameters[0].param_type,
        TypeName::from("BYTE")
    );
    assert_eq!(
        byte_to_word.return_type,
        Some(FunctionReturnType::Named(TypeName::from("WORD")))
    );
    assert!(byte_to_word.is_stdlib());
}

#[test]
fn get_bit_string_to_int_conversions_when_called_then_contains_expected() {
    let functions = get_bit_string_to_int_conversions();

    assert_eq!(functions.len(), 32);
    assert!(functions.iter().any(|f| f.name.original() == "BYTE_TO_INT"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "WORD_TO_DINT"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "DWORD_TO_UINT"));
}

#[test]
fn get_int_to_bit_string_conversions_when_called_then_contains_expected() {
    let functions = get_int_to_bit_string_conversions();

    assert_eq!(functions.len(), 32);
    assert!(functions.iter().any(|f| f.name.original() == "INT_TO_BYTE"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "DINT_TO_WORD"));
    assert!(functions
        .iter()
        .any(|f| f.name.original() == "UINT_TO_DWORD"));
}

#[test]
fn get_assignment_functions_when_move_then_has_one_input() {
    let functions = get_assignment_functions();
    let move_fn = functions
        .iter()
        .find(|f| f.name.original() == "MOVE")
        .unwrap();

    assert_eq!(move_fn.input_parameter_count(), 1);
    assert_eq!(move_fn.parameters[0].name.original(), "IN");
    assert!(move_fn.is_stdlib());
}

#[test]
fn stdlib_functions_have_builtin_span() {
    for func in get_all_stdlib_functions() {
        assert!(
            func.is_stdlib(),
            "Expected builtin span for stdlib function {}",
            func.name.original()
        );
    }
}

#[test]
fn get_compiler_intrinsic_functions_when_called_then_any_real_signatures() {
    let functions = get_compiler_intrinsic_functions();

    let trunc = functions
        .iter()
        .find(|f| f.name.original() == "__TRUNC")
        .unwrap();
    assert!(trunc.is_stdlib());
    assert_eq!(trunc.input_parameter_count(), 1);
    assert_eq!(trunc.parameters[0].param_type, TypeName::from("ANY_REAL"));
    assert_eq!(
        trunc.return_type,
        Some(FunctionReturnType::Named(TypeName::from("ANY_REAL")))
    );

    let fmod = functions
        .iter()
        .find(|f| f.name.original() == "__MOD")
        .unwrap();
    assert!(fmod.is_stdlib());
    assert_eq!(fmod.input_parameter_count(), 2);
    assert_eq!(fmod.parameters[0].param_type, TypeName::from("ANY_REAL"));
    assert_eq!(fmod.parameters[1].param_type, TypeName::from("ANY_REAL"));
    assert_eq!(
        fmod.return_type,
        Some(FunctionReturnType::Named(TypeName::from("ANY_REAL")))
    );
}

#[test]
fn get_all_stdlib_functions_when_called_then_includes_compiler_intrinsics() {
    let functions = get_all_stdlib_functions();
    assert!(functions.iter().any(|f| f.name.original() == "__TRUNC"));
    assert!(functions.iter().any(|f| f.name.original() == "__MOD"));
}

#[test]
fn get_all_stdlib_functions_when_called_then_registers_every_operator_form() {
    let names: Vec<Id> = get_all_stdlib_functions()
        .into_iter()
        .map(|f| f.name)
        .collect();
    for name in ["ADD", "GT", "AND", "NOT"] {
        assert!(names.contains(&Id::from(name)), "{name} missing");
    }
}
