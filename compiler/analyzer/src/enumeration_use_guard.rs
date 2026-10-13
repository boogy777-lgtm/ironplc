//! Guard: an enumeration is refused as the input of every standard function
//! that computes, and accepted as the input of every one that holds, whatever
//! functions the environment holds.
//!
//! The walk is derived from the function environment, so a standard function
//! added later is in it with no edit here. For each function it builds a call
//! that gives an enumeration variable to every input an `INT` satisfies and a
//! variable of a fitting type to every other, and reads the expected outcome
//! from the function's [`ValueUse`]: a function that computes refuses the call
//! when it has an enumeration input, and a function that holds accepts it.
//! The refusal is the problem code of the check that owns the input,
//! [`Problem::FunctionCallArgTypeMismatch`] or
//! [`Problem::OperatorOperandTypeMismatch`].
//!
//! A function that cannot be called this way is named in [`UNCALLABLE`] with
//! the reason, and the list is asserted: a function that becomes callable
//! fails the guard until it is taken off the list.

use ironplc_dsl::common::TypeName;
use ironplc_dsl::core::FileId;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_parser::options::{CompilerOptions, Dialect};
use ironplc_parser::parse_program;
use ironplc_problems::Problem;

use crate::function_environment::FunctionSignature;
use crate::stages::analyze;
use crate::type_compat::are_types_compatible;
use crate::value_type::ValueUse;

/// The functions no call can be built for, with the reason. The list is
/// compared with the functions the walk found uncallable.
const UNCALLABLE: [(&str, &str); 0] = [];

/// The types a variable can be declared with to satisfy an input that an
/// `INT` does not.
const CANDIDATES: [&str; 25] = [
    "BOOL",
    "BYTE",
    "WORD",
    "DWORD",
    "LWORD",
    "SINT",
    "INT",
    "DINT",
    "LINT",
    "USINT",
    "UINT",
    "UDINT",
    "ULINT",
    "REAL",
    "LREAL",
    "TIME",
    "LTIME",
    "DATE",
    "LDATE",
    "TIME_OF_DAY",
    "LTIME_OF_DAY",
    "DATE_AND_TIME",
    "LDATE_AND_TIME",
    "STRING",
    "WSTRING",
];

fn options() -> CompilerOptions {
    CompilerOptions {
        allow_sizeof: true,
        allow_special_operators: true,
        ..CompilerOptions::from_dialect(Dialect::Iec61131_3Ed3)
    }
}

/// A call built for one function.
struct Call {
    function: String,
    value_use: ValueUse,
    /// Whether any input is an enumeration.
    has_enumeration: bool,
    /// The statement that makes the call.
    statement: String,
}

/// The variable of a type that satisfies an input declared `required`: the
/// enumeration when an `INT` satisfies it, else the first candidate type that
/// does. `None` when no variable can.
fn argument_for(required: &TypeName, options: &CompilerOptions) -> Option<(String, bool)> {
    if are_types_compatible(required, &TypeName::from("INT"), options) {
        return Some(("e".to_owned(), true));
    }
    CANDIDATES
        .iter()
        .find(|candidate| are_types_compatible(required, &TypeName::from(candidate), options))
        .map(|candidate| (format!("v_{candidate}"), false))
}

/// The call of `signature` that gives each input a fitting variable, or
/// `None` when an input has none.
fn call_of(signature: &FunctionSignature, options: &CompilerOptions) -> Option<Call> {
    let arguments: Option<Vec<(String, bool)>> = signature
        .parameters
        .iter()
        .filter(|param| param.is_input_compatible())
        .map(|param| argument_for(&param.param_type, options))
        .collect();
    let arguments = arguments?;
    let names: Vec<&str> = arguments.iter().map(|(name, _)| name.as_str()).collect();
    Some(Call {
        function: signature.name.original().to_string(),
        value_use: signature.value_use,
        has_enumeration: arguments.iter().any(|(_, enumeration)| *enumeration),
        statement: format!(
            "sink := {}({});",
            signature.name.original(),
            names.join(", ")
        ),
    })
}

/// The program that makes every call in `calls`, and the byte range of each
/// call's statement in it.
fn program_of(calls: &[Call]) -> (String, Vec<(usize, usize)>) {
    let variables: String = CANDIDATES
        .iter()
        .map(|candidate| format!("v_{candidate} : {candidate};"))
        .collect::<Vec<_>>()
        .join(" ");
    let mut text = format!(
        "TYPE E1 : (A, B, C); END_TYPE\nPROGRAM main\nVAR e : E1; sink : DINT; {variables} END_VAR\n"
    );
    let mut ranges = Vec::new();
    for call in calls {
        let start = text.len();
        text.push_str(&call.statement);
        ranges.push((start, text.len()));
        text.push('\n');
    }
    text.push_str("END_PROGRAM\n");
    (text, ranges)
}

/// Whether `diagnostic` is the refusal of an input of the call at `range`.
fn refuses_input(diagnostic: &Diagnostic, range: (usize, usize)) -> bool {
    let at = diagnostic.primary.location.start;
    let refusal = [
        Problem::FunctionCallArgTypeMismatch.code(),
        Problem::OperatorOperandTypeMismatch.code(),
    ];
    refusal.contains(&diagnostic.code.as_str()) && range.0 <= at && at < range.1
}

/// The calls built for the function environment, the functions no call could
/// be built for, and the diagnostics of the analysis of the calls.
struct Walk {
    callable: Vec<Call>,
    uncallable: Vec<String>,
    diagnostics: Vec<Diagnostic>,
    ranges: Vec<(usize, usize)>,
}

fn walk() -> Walk {
    let options = options();
    let library = parse_program(
        "TYPE E1 : (A, B, C); END_TYPE PROGRAM main VAR e : E1; END_VAR END_PROGRAM",
        &FileId::default(),
        &options,
    )
    .unwrap();
    let (_library, context) = analyze(&[&library], &options).unwrap();
    let mut callable = Vec::new();
    let mut uncallable = Vec::new();
    for (_, signature) in context.functions().iter() {
        match call_of(signature, &options) {
            Some(call) => callable.push(call),
            None => uncallable.push(signature.name.original().to_string()),
        }
    }
    let (text, ranges) = program_of(&callable);
    let library = parse_program(&text, &FileId::default(), &options).unwrap();
    let (_library, context) = analyze(&[&library], &options).unwrap();
    Walk {
        callable,
        uncallable,
        diagnostics: context.diagnostics().to_vec(),
        ranges,
    }
}

#[test]
fn function_environment_when_function_given_an_enumeration_then_refused_exactly_when_it_computes() {
    let walk = walk();

    let mismatches: Vec<String> = walk
        .callable
        .iter()
        .zip(&walk.ranges)
        .filter_map(|(call, range)| {
            let refused = walk.diagnostics.iter().any(|d| refuses_input(d, *range));
            let expected = call.value_use == ValueUse::Computes && call.has_enumeration;
            (refused != expected).then(|| {
                format!(
                    "{}: expected {}, got {}",
                    call.function,
                    if expected { "refused" } else { "accepted" },
                    if refused { "refused" } else { "accepted" },
                )
            })
        })
        .collect();

    assert_eq!(mismatches, Vec::<String>::new());
}

#[test]
fn function_environment_when_walked_then_the_uncallable_functions_are_the_listed_ones() {
    let walk = walk();

    let listed: Vec<String> = UNCALLABLE
        .iter()
        .map(|(name, _)| (*name).to_owned())
        .collect();
    assert_eq!(walk.uncallable, listed);
    assert!(walk.callable.len() > 500);
}

#[test]
fn function_environment_when_walked_then_functions_that_compute_are_given_an_enumeration() {
    let walk = walk();

    let computing_with_enumeration = walk
        .callable
        .iter()
        .filter(|call| call.value_use == ValueUse::Computes && call.has_enumeration)
        .count();
    assert!(computing_with_enumeration >= 10);
}
