//! The numerical functions of IEC 61131-3 (Section 2.5.1.5.2, Table 23):
//! `ABS`, the roots, logarithms and trigonometric functions, `EXPT` and
//! `TRUNC`.
//!
//! Each of them makes a new number from its inputs, so the group states that
//! once: every row is [`FunctionSignature::computing`], and an enumeration is
//! no number to any of them (see [`crate::value_type::ValueUse`]). A function
//! added to the group computes without a line of its own. The selection
//! functions (`MIN`, `MAX`, `LIMIT`, `SEL`, `MUX`) are not in this group: they
//! choose among their inputs and hold what they choose.

use ironplc_dsl::common::TypeName;

use super::stdlib_function::input_param;
use crate::function_environment::FunctionSignature;

/// Returns the numerical function definitions.
///
/// These functions are defined in IEC 61131-3 as operating on generic
/// type categories (ANY_NUM, ANY_REAL). Parameter and return types use
/// the generic type names so that future type validation can check
/// compatibility via `GenericTypeName::is_compatible_with()`.
pub(super) fn get_numeric_functions() -> Vec<FunctionSignature> {
    vec![
        // ABS: absolute value (ANY_NUM -> ANY_NUM)
        FunctionSignature::stdlib(
            "ABS",
            TypeName::from("ANY_NUM"),
            vec![input_param("IN", "ANY_NUM")],
        ),
        // SQRT: square root (ANY_REAL -> ANY_REAL)
        FunctionSignature::stdlib(
            "SQRT",
            TypeName::from("ANY_REAL"),
            vec![input_param("IN", "ANY_REAL")],
        ),
        // LN: natural logarithm (ANY_REAL -> ANY_REAL)
        FunctionSignature::stdlib(
            "LN",
            TypeName::from("ANY_REAL"),
            vec![input_param("IN", "ANY_REAL")],
        ),
        // LOG: base-10 logarithm (ANY_REAL -> ANY_REAL)
        FunctionSignature::stdlib(
            "LOG",
            TypeName::from("ANY_REAL"),
            vec![input_param("IN", "ANY_REAL")],
        ),
        // EXP: natural exponential (ANY_REAL -> ANY_REAL)
        FunctionSignature::stdlib(
            "EXP",
            TypeName::from("ANY_REAL"),
            vec![input_param("IN", "ANY_REAL")],
        ),
        // SIN: sine (ANY_REAL -> ANY_REAL)
        FunctionSignature::stdlib(
            "SIN",
            TypeName::from("ANY_REAL"),
            vec![input_param("IN", "ANY_REAL")],
        ),
        // COS: cosine (ANY_REAL -> ANY_REAL)
        FunctionSignature::stdlib(
            "COS",
            TypeName::from("ANY_REAL"),
            vec![input_param("IN", "ANY_REAL")],
        ),
        // TAN: tangent (ANY_REAL -> ANY_REAL)
        FunctionSignature::stdlib(
            "TAN",
            TypeName::from("ANY_REAL"),
            vec![input_param("IN", "ANY_REAL")],
        ),
        // ASIN: arc sine (ANY_REAL -> ANY_REAL)
        FunctionSignature::stdlib(
            "ASIN",
            TypeName::from("ANY_REAL"),
            vec![input_param("IN", "ANY_REAL")],
        ),
        // ACOS: arc cosine (ANY_REAL -> ANY_REAL)
        FunctionSignature::stdlib(
            "ACOS",
            TypeName::from("ANY_REAL"),
            vec![input_param("IN", "ANY_REAL")],
        ),
        // ATAN: arc tangent (ANY_REAL -> ANY_REAL)
        FunctionSignature::stdlib(
            "ATAN",
            TypeName::from("ANY_REAL"),
            vec![input_param("IN", "ANY_REAL")],
        ),
        // ATAN2: two-argument arc tangent (ANY_REAL, ANY_REAL -> ANY_REAL)
        FunctionSignature::stdlib(
            "ATAN2",
            TypeName::from("ANY_REAL"),
            vec![
                input_param("IN1", "ANY_REAL"),
                input_param("IN2", "ANY_REAL"),
            ],
        ),
        // EXPT: exponentiation (ANY_NUM, ANY_NUM -> ANY_NUM)
        FunctionSignature::stdlib(
            "EXPT",
            TypeName::from("ANY_NUM"),
            vec![input_param("IN1", "ANY_NUM"), input_param("IN2", "ANY_NUM")],
        ),
        // TRUNC: truncation toward zero (ANY_REAL -> ANY_INT)
        FunctionSignature::stdlib(
            "TRUNC",
            TypeName::from("ANY_INT"),
            vec![input_param("IN", "ANY_REAL")],
        ),
    ]
    .into_iter()
    .map(FunctionSignature::computing)
    .collect()
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::value_type::ValueUse;

    #[test]
    fn get_numeric_functions_when_called_then_contains_all_functions() {
        let functions = get_numeric_functions();

        assert_eq!(functions.len(), 14);

        let names = [
            "ABS", "SQRT", "LN", "LOG", "EXP", "SIN", "COS", "TAN", "ASIN", "ACOS", "ATAN",
            "ATAN2", "EXPT", "TRUNC",
        ];

        assert!(names
            .iter()
            .all(|name| functions.iter().any(|f| f.name.original() == *name)));
    }

    #[test]
    fn get_numeric_functions_when_called_then_every_function_computes() {
        let functions = get_numeric_functions();

        assert!(functions.iter().all(|f| f.value_use == ValueUse::Computes));
    }

    #[test]
    fn get_numeric_functions_when_abs_then_has_one_input() {
        let functions = get_numeric_functions();
        let abs = functions
            .iter()
            .find(|f| f.name.original() == "ABS")
            .unwrap();

        assert_eq!(abs.input_parameter_count(), 1);
        assert_eq!(abs.parameters[0].name.original(), "IN");
        assert!(abs.is_stdlib());
    }

    #[test]
    fn get_numeric_functions_when_sqrt_then_has_one_input() {
        let functions = get_numeric_functions();
        let sqrt = functions
            .iter()
            .find(|f| f.name.original() == "SQRT")
            .unwrap();

        assert_eq!(sqrt.input_parameter_count(), 1);
        assert_eq!(sqrt.parameters[0].name.original(), "IN");
        assert!(sqrt.is_stdlib());
    }
}
