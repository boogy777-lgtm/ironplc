//! Guard: integers and enumerations meet as they do in CODESYS with
//! `{attribute 'strict'}`. An integer constant is accepted where an
//! enumeration is stored when it is the number of one of its values, in every
//! place a value is stored; any other integer is refused; an enumeration is
//! accepted where an integer is expected; arithmetic on one is refused.
//!
//! Each table is rows against places. A cell compiles a program the way
//! `ironplcc compile` does and, when it is accepted, runs one scan and reads
//! the number the program leaves in `r`; it is compared with what the table
//! states: that number, or the problem code of the first diagnostic that
//! refuses the program.

use crate::common::checked::try_check_and_run;
use ironplc_parser::options::CompilerOptions;

/// What a program does: the number `r` holds, or the code of the first
/// diagnostic.
fn outcome_of(source: &str) -> String {
    let options = CompilerOptions {
        allow_constant_initializer_expressions: true,
        allow_struct_initializer_expressions: true,
        allow_enum_explicit_values: true,
        ..CompilerOptions::default()
    };
    match try_check_and_run(source, &options) {
        Ok((_, bufs)) => bufs.vars[0].as_i32().to_string(),
        Err(diagnostic) => diagnostic.code,
    }
}

/// A place a value is stored into an enumeration, as a program that stores
/// `{V}` there and then leaves the number the enumeration holds in `r`.
struct Place {
    name: &'static str,
    /// Declarations before the program.
    types: &'static str,
    /// Declarations in the `VAR` block of the program, after `r`.
    decl: &'static str,
    /// The statements of the program, after the value is stored.
    body: &'static str,
}

const PLACES: [Place; 9] = [
    Place {
        name: "assignment",
        types: "",
        decl: "y : E1;",
        body: "y := {V}; r := y;",
    },
    Place {
        name: "variable initial value",
        types: "",
        decl: "y : E1 := {V};",
        body: "r := y;",
    },
    Place {
        name: "type default",
        types: "TYPE E2 : {ENUM} := {V}; END_TYPE",
        decl: "y : E2;",
        body: "r := y;",
    },
    Place {
        name: "structure initializer",
        types: "TYPE S : STRUCT m : E1; END_STRUCT; END_TYPE",
        decl: "s : S := (m := {V});",
        body: "r := s.m;",
    },
    Place {
        name: "structure member default",
        types: "TYPE S : STRUCT m : E1 := {V}; END_STRUCT; END_TYPE",
        decl: "s : S;",
        body: "r := s.m;",
    },
    Place {
        name: "array initializer",
        types: "TYPE S : STRUCT m : ARRAY[1..2] OF E1 := [{V}, {V}]; END_STRUCT; END_TYPE",
        decl: "s : S;",
        body: "r := s.m[2];",
    },
    Place {
        name: "function argument",
        types: "FUNCTION f : DINT VAR_INPUT n : E1; END_VAR f := n; END_FUNCTION",
        decl: "",
        body: "r := f({V});",
    },
    Place {
        name: "function block input",
        types: "FUNCTION_BLOCK B VAR_INPUT n : E1; END_VAR END_FUNCTION_BLOCK",
        decl: "b : B;",
        body: "b(n := {V}); r := b.n;",
    },
    Place {
        name: "function result",
        types: "FUNCTION f : E1 f := {V}; END_FUNCTION",
        decl: "",
        body: "r := f();",
    },
];

/// The program that stores `value` into the enumeration `E1`, declared as
/// `values`, at `place`.
fn store_program(place: &Place, values: &str, value: &str) -> String {
    let render = |text: &str| text.replace("{V}", value).replace("{ENUM}", values);
    format!(
        "TYPE E1 : {values}; END_TYPE {} PROGRAM main VAR r : DINT; {} END_VAR {} END_PROGRAM",
        render(place.types),
        render(place.decl),
        render(place.body)
    )
}

/// A row of a table of constants: the value, and what each place of `PLACES`
/// does with it.
struct Constant {
    name: &'static str,
    value: &'static str,
    cells: [&'static str; 9],
}

/// The cells of `rows` whose outcome is not the one the table states, for the
/// enumeration declared as `values`.
fn constant_mismatches(values: &str, rows: &[Constant]) -> Vec<String> {
    let mut found = Vec::new();
    for row in rows {
        for (place, expected) in PLACES.iter().zip(row.cells) {
            let got = outcome_of(&store_program(place, values, row.value));
            if got != expected {
                found.push(format!(
                    "{} / {}: expected {expected}, got {got}",
                    row.name, place.name
                ));
            }
        }
    }
    found
}

const ORDINAL_CONSTANTS: [Constant; 5] = [
    Constant {
        name: "the number of a value",
        value: "1",
        cells: ["1", "1", "1", "1", "1", "1", "1", "1", "1"],
    },
    Constant {
        name: "a number that is no value",
        value: "7",
        cells: [
            "P2006", "P2006", "P2006", "P2006", "P2006", "P2006", "P2006", "P2006", "P2006",
        ],
    },
    Constant {
        name: "a negative number",
        value: "-1",
        cells: [
            "P2006", "P2006", "P2006", "P2006", "P2006", "P2006", "P2006", "P2006", "P2006",
        ],
    },
    // A type default is a plain number, as the default of a subrange is.
    Constant {
        name: "a typed literal",
        value: "INT#1",
        cells: ["1", "1", "P4022", "1", "1", "1", "1", "1", "1"],
    },
    // An array initializer lists literals, and a structure initializer holds
    // an expression unfolded, which code generation does not lay out for a
    // member of any type.
    Constant {
        name: "a folded constant expression",
        value: "1 + 1",
        cells: ["2", "2", "P0002", "P9999", "2", "P0002", "2", "2", "2"],
    },
];

#[test]
fn enumeration_when_integer_constant_stored_then_accepted_only_as_the_number_of_a_value() {
    assert_eq!(
        constant_mismatches("(A, B, C)", &ORDINAL_CONSTANTS),
        Vec::<String>::new()
    );
}

const EXPLICIT_CONSTANTS: [Constant; 3] = [
    Constant {
        name: "the number of a value",
        value: "20",
        cells: ["20", "20", "20", "20", "20", "20", "20", "20", "20"],
    },
    Constant {
        name: "the ordinal of a value",
        value: "1",
        cells: [
            "P2006", "P2006", "P2006", "P2006", "P2006", "P2006", "P2006", "P2006", "P2006",
        ],
    },
    Constant {
        name: "a number between two values",
        value: "15",
        cells: [
            "P2006", "P2006", "P2006", "P2006", "P2006", "P2006", "P2006", "P2006", "P2006",
        ],
    },
];

#[test]
fn enumeration_when_values_are_numbered_then_constant_accepted_as_the_number_of_a_value() {
    assert_eq!(
        constant_mismatches("(A := 10, B := 20)", &EXPLICIT_CONSTANTS),
        Vec::<String>::new()
    );
}

/// A place a value that is not a constant is stored into `E1`: the program
/// declares `n : INT`, `z : E2` and stores `{V}`.
struct VariablePlace {
    name: &'static str,
    source: &'static str,
}

const VARIABLE_PLACES: [VariablePlace; 3] = [
    VariablePlace {
        name: "assignment",
        source: "PROGRAM main VAR r : DINT; y : E1; n : INT := 1; z : E2 := Q; END_VAR \
                 y := {V}; r := y; END_PROGRAM",
    },
    VariablePlace {
        name: "function argument",
        source: "FUNCTION f : DINT VAR_INPUT m : E1; END_VAR f := m; END_FUNCTION \
                 PROGRAM main VAR r : DINT; n : INT := 1; z : E2 := Q; END_VAR \
                 r := f({V}); END_PROGRAM",
    },
    VariablePlace {
        name: "function result",
        source: "FUNCTION f : E1 VAR_INPUT n : INT; z : E2; END_VAR f := {V}; END_FUNCTION \
                 PROGRAM main VAR r : DINT; y : E1; z : E2 := Q; END_VAR \
                 y := f(1, z); r := y; END_PROGRAM",
    },
];

struct Variable {
    name: &'static str,
    value: &'static str,
    /// The code that refuses the value at each place of `VARIABLE_PLACES`.
    cells: [&'static str; 3],
}

const VARIABLES: [Variable; 4] = [
    Variable {
        name: "an integer variable",
        value: "n",
        cells: ["P4035", "P4026", "P4035"],
    },
    Variable {
        name: "an integer expression",
        value: "n + 1",
        cells: ["P4035", "P4026", "P4035"],
    },
    Variable {
        name: "a value of another enumeration",
        value: "z",
        cells: ["P4035", "P4026", "P4035"],
    },
    Variable {
        name: "a real constant",
        value: "1.5",
        cells: ["P4035", "P4026", "P4035"],
    },
];

fn variable_mismatches() -> Vec<String> {
    let mut found = Vec::new();
    for row in &VARIABLES {
        for (place, expected) in VARIABLE_PLACES.iter().zip(row.cells) {
            let source = format!(
                "TYPE E1 : (A, B, C); END_TYPE TYPE E2 : (P, Q, R); END_TYPE {}",
                place.source.replace("{V}", row.value)
            );
            let got = outcome_of(&source);
            if got != expected {
                found.push(format!(
                    "{} / {}: expected {expected}, got {got}",
                    row.name, place.name
                ));
            }
        }
    }
    found
}

#[test]
fn enumeration_when_value_that_is_not_a_constant_stored_then_refused_by_the_check_of_the_place() {
    assert_eq!(variable_mismatches(), Vec::<String>::new());
}

/// A place an enumeration `x` (the value `C`, which is 2) is used where an
/// integer type `{T}` is expected, leaving the number in `r` through `{READ}`.
struct IntegerPlace {
    name: &'static str,
    source: &'static str,
}

const INTEGER_PLACES: [IntegerPlace; 3] = [
    IntegerPlace {
        name: "assignment",
        source: "PROGRAM main VAR r : DINT; x : E1 := C; t : {T}; END_VAR \
                 t := x; r := {READ}; END_PROGRAM",
    },
    IntegerPlace {
        name: "function argument",
        source: "FUNCTION f : {T} VAR_INPUT n : {T}; END_VAR f := n; END_FUNCTION \
                 PROGRAM main VAR r : DINT; x : E1 := C; t : {T}; END_VAR \
                 t := f(x); r := {READ}; END_PROGRAM",
    },
    IntegerPlace {
        name: "function result",
        source: "FUNCTION f : {T} VAR_INPUT e : E1; END_VAR f := e; END_FUNCTION \
                 PROGRAM main VAR r : DINT; x : E1 := C; t : {T}; END_VAR \
                 t := f(x); r := {READ}; END_PROGRAM",
    },
];

/// A type an enumeration is used as, how `t` of it is read into a `DINT`, and
/// what each place of `INTEGER_PLACES` does.
struct Integer {
    name: &'static str,
    read: &'static str,
    cells: [&'static str; 3],
}

/// The base type of an enumeration is `INT`, so it is accepted where `INT` is,
/// and where `INT` widens to (ADR-0029), and nowhere narrower.
const INTEGERS: [Integer; 5] = [
    Integer {
        name: "INT",
        read: "INT_TO_DINT(t)",
        cells: ["2", "2", "2"],
    },
    Integer {
        name: "DINT",
        read: "t",
        cells: ["2", "2", "2"],
    },
    Integer {
        name: "LINT",
        read: "LINT_TO_DINT(t)",
        cells: ["2", "2", "2"],
    },
    Integer {
        name: "REAL",
        read: "REAL_TO_DINT(t)",
        cells: ["2", "2", "2"],
    },
    Integer {
        name: "SINT",
        read: "SINT_TO_DINT(t)",
        cells: ["P4035", "P4026", "P4035"],
    },
];

fn integer_mismatches() -> Vec<String> {
    let mut found = Vec::new();
    for row in &INTEGERS {
        for (place, expected) in INTEGER_PLACES.iter().zip(row.cells) {
            let source = format!(
                "TYPE E1 : (A, B, C); END_TYPE {}",
                place
                    .source
                    .replace("{T}", row.name)
                    .replace("{READ}", row.read)
            );
            let got = outcome_of(&source);
            if got != expected {
                found.push(format!(
                    "{} / {}: expected {expected}, got {got}",
                    row.name, place.name
                ));
            }
        }
    }
    found
}

#[test]
fn enumeration_when_used_where_an_integer_is_expected_then_accepted_as_its_base_type_is() {
    assert_eq!(integer_mismatches(), Vec::<String>::new());
}

/// A statement that does arithmetic on the enumeration `x` (declared with
/// `i : INT` and `y : E1`).
const ARITHMETIC: [(&str, &str); 9] = [
    ("sum into an enumeration", "y := x + 1;"),
    ("sum into an integer", "i := x + 1;"),
    ("product", "y := x * 2;"),
    ("difference", "i := x - x;"),
    ("remainder", "i := x MOD 2;"),
    ("power", "i := x ** 2;"),
    ("negation into an enumeration", "y := -x;"),
    ("negation into an integer", "i := -x;"),
    ("the function form", "i := ADD(x, 1);"),
];

fn arithmetic_mismatches() -> Vec<String> {
    let mut found = Vec::new();
    for (name, statement) in ARITHMETIC {
        let source = format!(
            "TYPE E1 : (A, B, C); END_TYPE PROGRAM main VAR r : DINT; x : E1 := C; \
             y : E1; i : INT; END_VAR {statement} END_PROGRAM"
        );
        let got = outcome_of(&source);
        if got != "P4049" {
            found.push(format!("{name}: expected P4049, got {got}"));
        }
    }
    found
}

#[test]
fn enumeration_when_operand_of_arithmetic_then_refused_as_an_operand_of_the_wrong_type() {
    assert_eq!(arithmetic_mismatches(), Vec::<String>::new());
}
