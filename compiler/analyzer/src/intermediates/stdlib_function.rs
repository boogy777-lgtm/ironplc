//! Standard library function definitions for IEC 61131-3.
//!
//! This module defines the standard library functions specified in
//! IEC 61131-3 Section 2.5.1, including:
//! - Type conversion functions (INT_TO_REAL, REAL_TO_INT, etc.)
//! - Selection functions (MIN, MAX, LIMIT, SEL, MUX); the numerical functions
//!   (ABS, SQRT, ...) are in `stdlib_numeric_function`
//!
//! These functions are automatically available in the function environment
//! and do not need to be declared by the user.

use ironplc_dsl::common::TypeName;
use ironplc_dsl::core::Id;

use crate::function_environment::FunctionSignature;
use crate::intermediate_type::IntermediateFunctionParameter;
use crate::intermediates::operator_function_form;
use crate::intermediates::stdlib_numeric_function;
use crate::intermediates::stdlib_time_function;

/// Helper to create an input parameter.
pub(super) fn input_param(name: &str, param_type_name: &str) -> IntermediateFunctionParameter {
    IntermediateFunctionParameter {
        name: Id::from(name),
        param_type: TypeName::from(param_type_name),
        is_input: true,
        is_output: false,
        is_inout: false,
        is_reference: false,
    }
}

/// Creates a type conversion function signature.
///
/// Type conversion functions follow the naming convention `<SOURCE>_TO_<TARGET>`
/// and take a single input parameter of the source type, returning the target type.
fn build_conversion_function(source_name: &str, target_name: &str) -> FunctionSignature {
    let name = format!("{}_TO_{}", source_name, target_name);
    FunctionSignature::stdlib(
        &name,
        TypeName::from(target_name),
        vec![input_param("IN", source_name)],
    )
}

// =============================================================================
// Type Conversion Function Definitions (IEC 61131-3 Section 2.5.1.5)
// =============================================================================

/// Signed integer type names for conversion functions.
const SIGNED_INT_TYPES: &[&str] = &["SINT", "INT", "DINT", "LINT"];

/// Unsigned integer type names for conversion functions.
const UNSIGNED_INT_TYPES: &[&str] = &["USINT", "UINT", "UDINT", "ULINT"];

/// Real (floating-point) type names for conversion functions.
const REAL_TYPES: &[&str] = &["REAL", "LREAL"];

/// All integer type names (signed + unsigned) for BOOL conversion targets.
const ALL_INT_TYPES: &[&str] = &[
    "SINT", "INT", "DINT", "LINT", "USINT", "UINT", "UDINT", "ULINT",
];

/// Bit string type names (excluding BOOL) for conversion functions.
const BIT_STRING_TYPES: &[&str] = &["BYTE", "WORD", "DWORD", "LWORD"];

/// All time and date type names (including short aliases) for conversion functions.
///
/// Includes both canonical names (TIME_OF_DAY, DATE_AND_TIME) and their
/// short aliases (TOD, DT) since function lookup is by exact name.
const ALL_TIME_DATE_TYPES: &[&str] = &[
    "TIME",
    "LTIME",
    "DATE",
    "LDATE",
    "TOD",
    "TIME_OF_DAY",
    "LTOD",
    "LTIME_OF_DAY",
    "DT",
    "DATE_AND_TIME",
    "LDT",
    "LDATE_AND_TIME",
];

/// Integer, real, and bit string type names that time/date types convert to/from.
const TIME_DATE_TARGETS: &[&str] = &[
    "SINT", "INT", "DINT", "LINT", "USINT", "UINT", "UDINT", "ULINT", "REAL", "LREAL", "BYTE",
    "WORD", "DWORD", "LWORD",
];

/// Generates all integer-to-integer conversion functions.
///
/// Creates functions like INT_TO_DINT, DINT_TO_INT, SINT_TO_LINT, etc.
fn get_int_to_int_conversions() -> Vec<FunctionSignature> {
    let mut functions = Vec::new();

    // All signed integer types
    for source_name in SIGNED_INT_TYPES {
        for target_name in SIGNED_INT_TYPES {
            if source_name != target_name {
                functions.push(build_conversion_function(source_name, target_name));
            }
        }
    }

    // All unsigned integer types
    for source_name in UNSIGNED_INT_TYPES {
        for target_name in UNSIGNED_INT_TYPES {
            if source_name != target_name {
                functions.push(build_conversion_function(source_name, target_name));
            }
        }
    }

    // Signed to unsigned conversions
    for source_name in SIGNED_INT_TYPES {
        for target_name in UNSIGNED_INT_TYPES {
            functions.push(build_conversion_function(source_name, target_name));
        }
    }

    // Unsigned to signed conversions
    for source_name in UNSIGNED_INT_TYPES {
        for target_name in SIGNED_INT_TYPES {
            functions.push(build_conversion_function(source_name, target_name));
        }
    }

    functions
}

/// Generates all integer-to-real conversion functions.
///
/// Creates functions like INT_TO_REAL, DINT_TO_LREAL, UINT_TO_REAL, etc.
fn get_int_to_real_conversions() -> Vec<FunctionSignature> {
    let mut functions = Vec::new();

    // Signed integer to real
    for source_name in SIGNED_INT_TYPES {
        for target_name in REAL_TYPES {
            functions.push(build_conversion_function(source_name, target_name));
        }
    }

    // Unsigned integer to real
    for source_name in UNSIGNED_INT_TYPES {
        for target_name in REAL_TYPES {
            functions.push(build_conversion_function(source_name, target_name));
        }
    }

    functions
}

/// Generates all real-to-integer conversion functions.
///
/// Creates functions like REAL_TO_INT, LREAL_TO_DINT, REAL_TO_UINT, etc.
fn get_real_to_int_conversions() -> Vec<FunctionSignature> {
    let mut functions = Vec::new();

    // Real to signed integer
    for source_name in REAL_TYPES {
        for target_name in SIGNED_INT_TYPES {
            functions.push(build_conversion_function(source_name, target_name));
        }
    }

    // Real to unsigned integer
    for source_name in REAL_TYPES {
        for target_name in UNSIGNED_INT_TYPES {
            functions.push(build_conversion_function(source_name, target_name));
        }
    }

    functions
}

/// Generates all real-to-real conversion functions.
///
/// Creates functions like REAL_TO_LREAL, LREAL_TO_REAL.
fn get_real_to_real_conversions() -> Vec<FunctionSignature> {
    let mut functions = Vec::new();

    for source_name in REAL_TYPES {
        for target_name in REAL_TYPES {
            if source_name != target_name {
                functions.push(build_conversion_function(source_name, target_name));
            }
        }
    }

    functions
}

/// Generates BOOL-to-integer conversion functions.
///
/// Creates functions like BOOL_TO_SINT, BOOL_TO_INT, BOOL_TO_DINT, etc.
/// FALSE converts to 0, TRUE converts to 1.
fn get_bool_to_int_conversions() -> Vec<FunctionSignature> {
    ALL_INT_TYPES
        .iter()
        .map(|target| build_conversion_function("BOOL", target))
        .collect()
}

/// Generates integer-to-BOOL conversion functions.
///
/// Creates functions like SINT_TO_BOOL, INT_TO_BOOL, DINT_TO_BOOL, etc.
/// 0 converts to FALSE, any non-zero value converts to TRUE.
fn get_int_to_bool_conversions() -> Vec<FunctionSignature> {
    ALL_INT_TYPES
        .iter()
        .map(|source| build_conversion_function(source, "BOOL"))
        .collect()
}

// =============================================================================
// Bit String Type Conversion Functions (IEC 61131-3 Section 2.5.1.5)
// =============================================================================

/// Generates bit-string-to-bit-string conversion functions.
///
/// Creates functions like BYTE_TO_WORD, WORD_TO_DWORD, DWORD_TO_LWORD, etc.
fn get_bit_string_to_bit_string_conversions() -> Vec<FunctionSignature> {
    let mut functions = Vec::new();

    for source_name in BIT_STRING_TYPES {
        for target_name in BIT_STRING_TYPES {
            if source_name != target_name {
                functions.push(build_conversion_function(source_name, target_name));
            }
        }
    }

    functions
}

/// Generates bit-string-to-integer conversion functions.
///
/// Creates functions like BYTE_TO_INT, WORD_TO_DINT, DWORD_TO_LINT, etc.
fn get_bit_string_to_int_conversions() -> Vec<FunctionSignature> {
    let mut functions = Vec::new();

    for source_name in BIT_STRING_TYPES {
        for target_name in ALL_INT_TYPES {
            functions.push(build_conversion_function(source_name, target_name));
        }
    }

    functions
}

/// Generates integer-to-bit-string conversion functions.
///
/// Creates functions like INT_TO_BYTE, DINT_TO_WORD, LINT_TO_DWORD, etc.
fn get_int_to_bit_string_conversions() -> Vec<FunctionSignature> {
    let mut functions = Vec::new();

    for source_name in ALL_INT_TYPES {
        for target_name in BIT_STRING_TYPES {
            functions.push(build_conversion_function(source_name, target_name));
        }
    }

    functions
}

/// Generates BOOL-to-bit-string and bit-string-to-BOOL conversion functions.
///
/// Creates functions like BOOL_TO_BYTE, BYTE_TO_BOOL, etc.
fn get_bool_bit_string_conversions() -> Vec<FunctionSignature> {
    let mut functions = Vec::new();

    for bit_type in BIT_STRING_TYPES {
        functions.push(build_conversion_function("BOOL", bit_type));
        functions.push(build_conversion_function(bit_type, "BOOL"));
    }

    functions
}

/// Generates bit-string-to-real and real-to-bit-string conversion functions.
///
/// Creates functions like BYTE_TO_REAL, REAL_TO_BYTE, etc.
fn get_bit_string_real_conversions() -> Vec<FunctionSignature> {
    let mut functions = Vec::new();

    for bit_type in BIT_STRING_TYPES {
        for real_type in REAL_TYPES {
            functions.push(build_conversion_function(bit_type, real_type));
            functions.push(build_conversion_function(real_type, bit_type));
        }
    }

    functions
}

// =============================================================================
// Time/Date Type Conversion Functions (IEC 61131-3 Section 2.5.1.5)
// =============================================================================

/// Generates time/date type conversion functions.
///
/// Creates bidirectional conversions between all time/date types (TIME, DATE,
/// TOD, DT and their long and alias forms) and integer, real, and bit string
/// types. For example: TIME_TO_DWORD, DWORD_TO_TIME, DATE_TO_UDINT, etc.
fn get_time_date_conversions() -> Vec<FunctionSignature> {
    let mut functions = Vec::new();

    for time_type in ALL_TIME_DATE_TYPES {
        for target_type in TIME_DATE_TARGETS {
            functions.push(build_conversion_function(time_type, target_type));
            functions.push(build_conversion_function(target_type, time_type));
        }
    }

    functions
}

// =============================================================================
// String Conversion Function Definitions (IEC 61131-3 Section 2.5.1.5)
// =============================================================================

/// Numeric types that convert to STRING (W32 signed).
const SIGNED_INT_TO_STRING_TYPES: &[&str] = &["SINT", "INT", "DINT"];

/// Numeric types that convert to STRING (W32 unsigned / bit-string).
const UNSIGNED_INT_TO_STRING_TYPES: &[&str] = &["USINT", "UINT", "UDINT", "BYTE", "WORD", "DWORD"];

/// Integer and bit-string types that can be parsed from STRING. A bit-string
/// target converts as the unsigned integer of its width.
const STRING_TO_INT_TYPES: &[&str] = &[
    "SINT", "INT", "DINT", "LINT", "USINT", "UINT", "UDINT", "ULINT", "BYTE", "WORD", "DWORD",
    "LWORD",
];

/// Returns string ↔ numeric conversion function definitions.
///
/// Covers all W32 numeric types: signed integers (SINT, INT, DINT),
/// unsigned integers and bit-strings (USINT, UINT, UDINT, BYTE, WORD, DWORD),
/// and REAL. Each type gets a *_TO_STRING function, and a subset gets
/// STRING_TO_* functions.
fn get_string_conversion_functions() -> Vec<FunctionSignature> {
    let mut functions = Vec::new();

    // Signed integer → STRING
    for source in SIGNED_INT_TO_STRING_TYPES {
        functions.push(build_conversion_function(source, "STRING"));
    }

    // Unsigned integer / bit-string → STRING
    for source in UNSIGNED_INT_TO_STRING_TYPES {
        functions.push(build_conversion_function(source, "STRING"));
    }

    // REAL → STRING
    functions.push(build_conversion_function("REAL", "STRING"));

    // STRING → integer and bit-string types
    for target in STRING_TO_INT_TYPES {
        functions.push(build_conversion_function("STRING", target));
    }

    // STRING → real types
    for target in REAL_TYPES {
        functions.push(build_conversion_function("STRING", target));
    }

    functions
}

// =============================================================================
// Selection Function Definitions (IEC 61131-3 Section 2.5.1.5.4)
// =============================================================================

/// Returns standard selection function definitions.
///
/// A selection function chooses among its inputs and holds what it chooses, so
/// an enumeration passes through it as the value it is (`ValueUse::Holds`,
/// the default of a row). `MIN`, `MAX`, `LIMIT` and `SEL` are declared here
/// beside `MUX` for that reason, though they take the generic `ANY_NUM`.
///
/// MUX is an extensible multiplexer that selects one of N inputs based on
/// an integer selector K. Unlike SEL (which uses a BOOL selector and exactly
/// 2 inputs), MUX uses an ANY_INT selector and supports 2..16 inputs.
///
/// The declared parameters define the minimum (K + 2 IN values = 3 args).
/// Additional IN arguments are accepted because the signature is extensible.
fn get_selection_functions() -> Vec<FunctionSignature> {
    vec![
        // MIN: minimum of two values (ANY_NUM, ANY_NUM -> ANY_NUM)
        FunctionSignature::stdlib(
            "MIN",
            TypeName::from("ANY_NUM"),
            vec![input_param("IN1", "ANY_NUM"), input_param("IN2", "ANY_NUM")],
        ),
        // MAX: maximum of two values (ANY_NUM, ANY_NUM -> ANY_NUM)
        FunctionSignature::stdlib(
            "MAX",
            TypeName::from("ANY_NUM"),
            vec![input_param("IN1", "ANY_NUM"), input_param("IN2", "ANY_NUM")],
        ),
        // LIMIT: clamp value to range (ANY_NUM, ANY_NUM, ANY_NUM -> ANY_NUM)
        FunctionSignature::stdlib(
            "LIMIT",
            TypeName::from("ANY_NUM"),
            vec![
                input_param("MN", "ANY_NUM"),
                input_param("IN", "ANY_NUM"),
                input_param("MX", "ANY_NUM"),
            ],
        ),
        // SEL: binary selection (BOOL, ANY_NUM, ANY_NUM -> ANY_NUM)
        FunctionSignature::stdlib(
            "SEL",
            TypeName::from("ANY_NUM"),
            vec![
                input_param("G", "BOOL"),
                input_param("IN0", "ANY_NUM"),
                input_param("IN1", "ANY_NUM"),
            ],
        ),
        // MUX: multiplexer (ANY_INT, ANY_NUM, ANY_NUM, ... -> ANY_NUM)
        // MUX supports K + 2..16 IN values = 3..17 total input arguments
        FunctionSignature::stdlib_extensible(
            "MUX",
            TypeName::from("ANY_NUM"),
            vec![
                input_param("K", "ANY_INT"),
                input_param("IN0", "ANY_NUM"),
                input_param("IN1", "ANY_NUM"),
            ],
            Some(17),
        ),
    ]
}

// =============================================================================
// Assignment Function (IEC 61131-3 Section 2.5.1.5.4)
// =============================================================================

/// Returns the MOVE standard function definition.
///
/// MOVE copies the input value to the output, equivalent to assignment.
/// IEC 61131-3 defines MOVE as operating on ANY type, but since the codegen
/// currently supports numeric types we use ANY_NUM here.
fn get_assignment_functions() -> Vec<FunctionSignature> {
    vec![
        // MOVE: assignment (ANY_NUM -> ANY_NUM)
        FunctionSignature::stdlib(
            "MOVE",
            TypeName::from("ANY_NUM"),
            vec![input_param("IN", "ANY_NUM")],
        ),
    ]
}

// =============================================================================
// BCD Conversion Functions (IEC 61131-3 Section 2.5.1.5)
// =============================================================================

/// Returns BCD conversion function definitions.
///
/// BCD_TO_INT converts a BCD-encoded bit string to an integer.
/// INT_TO_BCD converts an integer to a BCD-encoded bit string.
fn get_bcd_functions() -> Vec<FunctionSignature> {
    vec![
        FunctionSignature::stdlib(
            "BCD_TO_INT",
            TypeName::from("ANY_INT"),
            vec![input_param("IN", "ANY_BIT")],
        ),
        FunctionSignature::stdlib(
            "INT_TO_BCD",
            TypeName::from("ANY_BIT"),
            vec![input_param("IN", "ANY_INT")],
        ),
    ]
}

// =============================================================================
// Bit shift and rotate functions
// =============================================================================

/// Returns standard bit shift and rotate function definitions.
///
/// IEC 61131-3 defines SHL, SHR, ROL, ROR as standard functions operating
/// on ANY_BIT types with an ANY_INT shift count. The return type matches
/// the input type.
fn get_bitshift_functions() -> Vec<FunctionSignature> {
    vec![
        FunctionSignature::stdlib(
            "SHL",
            TypeName::from("ANY_BIT"),
            vec![input_param("IN", "ANY_BIT"), input_param("N", "ANY_INT")],
        ),
        FunctionSignature::stdlib(
            "SHR",
            TypeName::from("ANY_BIT"),
            vec![input_param("IN", "ANY_BIT"), input_param("N", "ANY_INT")],
        ),
        FunctionSignature::stdlib(
            "ROL",
            TypeName::from("ANY_BIT"),
            vec![input_param("IN", "ANY_BIT"), input_param("N", "ANY_INT")],
        ),
        FunctionSignature::stdlib(
            "ROR",
            TypeName::from("ANY_BIT"),
            vec![input_param("IN", "ANY_BIT"), input_param("N", "ANY_INT")],
        ),
    ]
}

/// Returns standard string function definitions.
///
/// IEC 61131-3 defines string functions operating on ANY_STRING types.
fn get_string_functions() -> Vec<FunctionSignature> {
    vec![
        // LEN: current length of a string (ANY_STRING -> INT)
        FunctionSignature::stdlib(
            "LEN",
            TypeName::from("INT"),
            vec![input_param("IN", "ANY_STRING")],
        ),
        // FIND: find first occurrence of IN2 within IN1 (ANY_STRING, ANY_STRING -> INT)
        FunctionSignature::stdlib(
            "FIND",
            TypeName::from("INT"),
            vec![
                input_param("IN1", "ANY_STRING"),
                input_param("IN2", "ANY_STRING"),
            ],
        ),
        // REPLACE: replace L chars at position P in IN1 with IN2
        // (ANY_STRING, ANY_STRING, ANY_INT, ANY_INT -> ANY_STRING)
        FunctionSignature::stdlib(
            "REPLACE",
            TypeName::from("ANY_STRING"),
            vec![
                input_param("IN1", "ANY_STRING"),
                input_param("IN2", "ANY_STRING"),
                input_param("L", "ANY_INT"),
                input_param("P", "ANY_INT"),
            ],
        ),
        // INSERT: insert IN2 into IN1 after position P
        // (ANY_STRING, ANY_STRING, ANY_INT -> ANY_STRING)
        FunctionSignature::stdlib(
            "INSERT",
            TypeName::from("ANY_STRING"),
            vec![
                input_param("IN1", "ANY_STRING"),
                input_param("IN2", "ANY_STRING"),
                input_param("P", "ANY_INT"),
            ],
        ),
        // DELETE: delete L chars from IN1 starting at position P
        // (ANY_STRING, ANY_INT, ANY_INT -> ANY_STRING)
        FunctionSignature::stdlib(
            "DELETE",
            TypeName::from("ANY_STRING"),
            vec![
                input_param("IN1", "ANY_STRING"),
                input_param("L", "ANY_INT"),
                input_param("P", "ANY_INT"),
            ],
        ),
        // LEFT: return leftmost L characters of IN
        // (ANY_STRING, ANY_INT -> ANY_STRING)
        FunctionSignature::stdlib(
            "LEFT",
            TypeName::from("ANY_STRING"),
            vec![input_param("IN", "ANY_STRING"), input_param("L", "ANY_INT")],
        ),
        // RIGHT: return rightmost L characters of IN
        // (ANY_STRING, ANY_INT -> ANY_STRING)
        FunctionSignature::stdlib(
            "RIGHT",
            TypeName::from("ANY_STRING"),
            vec![input_param("IN", "ANY_STRING"), input_param("L", "ANY_INT")],
        ),
        // MID: return L characters from IN starting at position P
        // (ANY_STRING, ANY_INT, ANY_INT -> ANY_STRING)
        FunctionSignature::stdlib(
            "MID",
            TypeName::from("ANY_STRING"),
            vec![
                input_param("IN", "ANY_STRING"),
                input_param("L", "ANY_INT"),
                input_param("P", "ANY_INT"),
            ],
        ),
        // CONCAT: concatenate IN1 and IN2
        // (ANY_STRING, ANY_STRING -> ANY_STRING)
        FunctionSignature::stdlib(
            "CONCAT",
            TypeName::from("ANY_STRING"),
            vec![
                input_param("IN1", "ANY_STRING"),
                input_param("IN2", "ANY_STRING"),
            ],
        ),
    ]
}

// =============================================================================
// Public API
// =============================================================================

/// Returns all standard library function definitions.
///
/// Each function is returned as a FunctionSignature ready to be inserted
/// into the FunctionEnvironment.
pub fn get_all_stdlib_functions() -> Vec<FunctionSignature> {
    let mut functions = Vec::new();

    // Type conversion functions
    functions.extend(get_int_to_int_conversions());
    functions.extend(get_int_to_real_conversions());
    functions.extend(get_real_to_int_conversions());
    functions.extend(get_real_to_real_conversions());
    functions.extend(get_bool_to_int_conversions());
    functions.extend(get_int_to_bool_conversions());

    // Bit string type conversion functions
    functions.extend(get_bit_string_to_bit_string_conversions());
    functions.extend(get_bit_string_to_int_conversions());
    functions.extend(get_int_to_bit_string_conversions());
    functions.extend(get_bool_bit_string_conversions());
    functions.extend(get_bit_string_real_conversions());

    // Time/date type conversion functions
    functions.extend(get_time_date_conversions());

    // String conversion functions
    functions.extend(get_string_conversion_functions());

    // Numerical functions, which compute (ABS, SQRT, EXPT, TRUNC, ...)
    functions.extend(stdlib_numeric_function::get_numeric_functions());

    // Function forms of operators (+, -, *, /, MOD, comparisons, AND, OR, XOR, NOT)
    functions.extend(operator_function_form::signatures());

    // BCD conversion functions
    functions.extend(get_bcd_functions());

    // Selection functions, which hold what they select (MIN, MAX, LIMIT, SEL, MUX)
    functions.extend(get_selection_functions());

    // Assignment function (MOVE)
    functions.extend(get_assignment_functions());

    // Bit shift and rotate functions
    functions.extend(get_bitshift_functions());

    // String functions
    functions.extend(get_string_functions());

    // Time functions (IEC 61131-3 Section 2.5.1.5.8, Table 35)
    functions.extend(stdlib_time_function::get_time_functions());

    // Compiler intrinsics (reserved `__` namespace)
    functions.extend(get_compiler_intrinsic_functions());

    functions
}

// =============================================================================
// Compiler intrinsics (reserved `__` namespace)
// =============================================================================

/// Returns the `__`-prefixed compiler intrinsic function definitions.
///
/// The `__` prefix is the established compiler-provided namespace (like
/// `__SYSTEM_UP_TIME`): names only the compiler can provide, visibly
/// non-portable, and colliding with no IEC 61131-3 or vendor name. These
/// exist for behavior IEC 61131-3 source cannot express, and their intended
/// callers are bundled compatibility-library bodies (e.g. `Tc2_Math`'s
/// `LTRUNC := __TRUNC(IN);`). They are seeded unconditionally because
/// library bodies are analyzed under the *user's* options, so a flag gate
/// would break every library that uses them.
///
/// - `__TRUNC(IN: ANY_REAL): ANY_REAL` — truncation toward zero that stays
///   in the input's real type (unlike `TRUNC`, whose `ANY_INT` result clamps
///   values beyond the integer range).
/// - `__MOD(IN1, IN2: ANY_REAL): ANY_REAL` — IEEE-754 floating remainder
///   with the sign of the dividend (unlike `MOD`, which is integer-only);
///   `__MOD(x, 0.0)` is NaN, never a runtime error.
///
/// **Why named intrinsics rather than a manifest binding.** The alternative
/// was to let a library manifest bind a vendor name straight to an unnamed
/// VM builtin, which is what [ADR-0042](../../../../specs/adrs/0042-library-functions-over-compiler-intrinsics.md)
/// rule 3 still describes. It was rejected on security grounds: it makes an
/// on-disk data file an input to code *emission*, and nothing structurally
/// guarantees a library's declared signature matches the builtin's stack
/// behaviour — a mismatched binding would corrupt the operand stack. With
/// `__TRUNC`/`__MOD` as ordinary typed intrinsics, every `BUILTIN` emission
/// originates from a compiler-owned table, manifests stay pure metadata, and
/// the signature is type-checked like any other stdlib function, so that
/// mismatch cannot exist. No manifest binding mechanism was ever built.
pub fn get_compiler_intrinsic_functions() -> Vec<FunctionSignature> {
    vec![
        FunctionSignature::stdlib(
            "__TRUNC",
            TypeName::from("ANY_REAL"),
            vec![input_param("IN", "ANY_REAL")],
        ),
        FunctionSignature::stdlib(
            "__MOD",
            TypeName::from("ANY_REAL"),
            vec![
                input_param("IN1", "ANY_REAL"),
                input_param("IN2", "ANY_REAL"),
            ],
        ),
    ]
    .into_iter()
    .map(FunctionSignature::computing)
    .collect()
}

// =============================================================================
// SIZEOF (Language Extension)
// =============================================================================

/// Returns the SIZEOF function definition.
///
/// SIZEOF returns the size in bytes of a variable or type. It is not part of
/// IEC 61131-3 but is a CODESYS/TwinCAT/RuSTy extension used in buffer
/// management functions. It is registered conditionally based on the
/// `allow_sizeof` compiler option.
pub fn get_sizeof_function() -> FunctionSignature {
    FunctionSignature::stdlib(
        "SIZEOF",
        TypeName::from("ANY_INT"),
        vec![input_param("IN", "ANY")],
    )
}

#[cfg(test)]
mod tests;
