//! Guard: an enumeration is a number to the places that refuse it only when it
//! is strict. An enumeration is strict when its declaration carries
//! `{attribute 'strict'}` or when the policy says every enumeration is; one
//! that is not strict is its base type (`INT` unless the declaration names
//! another) in every place of the class: arithmetic, the numerical functions,
//! the counter of a `FOR`, a selection stored into an enumeration, a number
//! stored into one, and a constant that names none of its values.
//!
//! Each table is rows against configurations. A cell compiles a program the
//! way `ironplcc compile` does and, when it is accepted, runs one scan and
//! reads the number the program leaves in `r`; it is compared with what the
//! table states: that number, or the problem code of the first diagnostic that
//! refuses the program.

use ironplc_parser::options::{CompilerOptions, Dialect, EnumerationStrictness};

use spec_test_macro::spec_test;

use super::end_to_end_enumeration_integer::{store_program, PLACES};
use crate::common::checked::try_check_and_run;

/// How a compilation is set up: the options, and whether the enumerations the
/// program declares carry `{attribute 'strict'}`.
struct Config {
    name: &'static str,
    options: CompilerOptions,
    attribute: bool,
    /// Whether the enumerations are strict under this configuration.
    strict: bool,
}

fn configs() -> Vec<Config> {
    let with_attribute = |dialect| CompilerOptions {
        policy_enumeration_strictness: EnumerationStrictness::WithAttribute,
        ..CompilerOptions::from_dialect(dialect)
    };
    vec![
        Config {
            name: "IEC 61131-3 Ed. 2 without the attribute",
            options: CompilerOptions::from_dialect(Dialect::Iec61131_3Ed2),
            attribute: false,
            strict: true,
        },
        Config {
            name: "IEC 61131-3 Ed. 3 without the attribute",
            options: CompilerOptions::from_dialect(Dialect::Iec61131_3Ed3),
            attribute: false,
            strict: true,
        },
        Config {
            name: "CODESYS with the attribute",
            options: CompilerOptions::from_dialect(Dialect::Codesys),
            attribute: true,
            strict: true,
        },
        Config {
            name: "CODESYS without the attribute",
            options: CompilerOptions::from_dialect(Dialect::Codesys),
            attribute: false,
            strict: false,
        },
        Config {
            name: "TwinCAT with the attribute",
            options: CompilerOptions::from_dialect(Dialect::TwinCat),
            attribute: true,
            strict: true,
        },
        Config {
            name: "TwinCAT without the attribute",
            options: CompilerOptions::from_dialect(Dialect::TwinCat),
            attribute: false,
            strict: false,
        },
        Config {
            name: "CODESYS without the attribute, every enumeration strict",
            options: CompilerOptions {
                policy_enumeration_strictness: EnumerationStrictness::EveryEnumeration,
                ..CompilerOptions::from_dialect(Dialect::Codesys)
            },
            attribute: false,
            strict: true,
        },
        Config {
            name: "IEC 61131-3 Ed. 2 without the attribute, strict with the attribute",
            options: with_attribute(Dialect::Iec61131_3Ed2),
            attribute: false,
            strict: false,
        },
    ]
}

/// What a program does under `config`: the number `r` holds, or the code of
/// the first diagnostic. Every `TYPE E<n>` declaration of the source carries
/// the attribute when the configuration says so.
fn outcome_of(source: &str, config: &Config) -> String {
    let source = if config.attribute {
        source.replace("TYPE E", "{attribute 'strict'} TYPE E")
    } else {
        source.to_owned()
    };
    match try_check_and_run(&source, &config.options) {
        Ok((_, bufs)) => bufs.vars[0].as_i32().to_string(),
        Err(diagnostic) => diagnostic.code,
    }
}

/// The program that declares `E1` and `E2`, a function that takes an
/// enumeration, `declarations`, and the variables
/// `r : DINT`, `x : E1 := C`, `y : E1`, `z : E2 := Q`, `i : INT`, `g : BOOL`
/// and `t : TIME`, and runs `statements`.
fn program_of(declarations: &str, statements: &str) -> String {
    format!(
        "TYPE E1 : (A, B, C); END_TYPE TYPE E2 : (P, Q, R); END_TYPE \
         FUNCTION take : DINT VAR_INPUT n : E1; END_VAR take := n; END_FUNCTION \
         {declarations} \
         PROGRAM main VAR r : DINT; x : E1 := C; y : E1; z : E2 := Q; i : INT; \
         g : BOOL; t : TIME := T#10ms; END_VAR {statements} END_PROGRAM"
    )
}

/// A row of the table of the class: the statements of the program, the code
/// that refuses them when the enumeration is strict, and the number `r` holds
/// when it is not.
struct Site {
    name: &'static str,
    /// Declarations before the program, besides the common ones.
    declarations: &'static str,
    statements: &'static str,
    strict: &'static str,
    not_strict: &'static str,
}

const SITES: [Site; 32] = [
    Site {
        name: "sum into an enumeration",
        declarations: "",
        statements: "y := x + 1; r := y;",
        strict: "P4049",
        not_strict: "3",
    },
    Site {
        name: "sum into an integer",
        declarations: "",
        statements: "i := x + 1; r := i;",
        strict: "P4049",
        not_strict: "3",
    },
    Site {
        name: "product",
        declarations: "",
        statements: "i := x * 2; r := i;",
        strict: "P4049",
        not_strict: "4",
    },
    Site {
        name: "difference of two enumerations",
        declarations: "",
        statements: "i := x - z; r := i;",
        strict: "P4049",
        not_strict: "1",
    },
    Site {
        name: "remainder",
        declarations: "",
        statements: "i := x MOD 2; r := i;",
        strict: "P4049",
        not_strict: "0",
    },
    Site {
        name: "power",
        declarations: "",
        statements: "i := x ** 2; r := i;",
        strict: "P4049",
        not_strict: "4",
    },
    Site {
        name: "negation",
        declarations: "",
        statements: "i := -x; r := i;",
        strict: "P4049",
        not_strict: "-2",
    },
    Site {
        name: "sum function",
        declarations: "",
        statements: "i := ADD(x, 1); r := i;",
        strict: "P4049",
        not_strict: "3",
    },
    Site {
        name: "product function",
        declarations: "",
        statements: "i := MUL(x, 2); r := i;",
        strict: "P4049",
        not_strict: "4",
    },
    Site {
        name: "numerical function into an integer",
        declarations: "",
        statements: "i := ABS(x); r := i;",
        strict: "P4026",
        not_strict: "2",
    },
    Site {
        name: "numerical function into an enumeration",
        declarations: "",
        statements: "y := ABS(x); r := y;",
        strict: "P4026",
        not_strict: "2",
    },
    Site {
        name: "power function",
        declarations: "",
        statements: "i := EXPT(x, 2); r := i;",
        strict: "P4026",
        not_strict: "4",
    },
    Site {
        name: "remainder function",
        declarations: "",
        statements: "i := MOD(x, 2); r := i;",
        strict: "P4026",
        not_strict: "0",
    },
    Site {
        name: "time overload",
        declarations: "",
        statements: "t := MUL_TIME(t, x); r := TIME_TO_DINT(t);",
        strict: "P4026",
        not_strict: "20",
    },
    Site {
        name: "counter of a FOR",
        declarations: "",
        statements: "FOR x := A TO C DO r := r + 1; END_FOR;",
        strict: "P4049",
        not_strict: "3",
    },
    Site {
        name: "counter of a FOR with a step",
        declarations: "",
        statements: "FOR x := A TO C BY 2 DO r := r + 1; END_FOR;",
        strict: "P4049",
        not_strict: "2",
    },
    Site {
        name: "selection of a number and an enumeration",
        declarations: "",
        statements: "y := MAX(x, 1); r := y;",
        strict: "P4027",
        not_strict: "2",
    },
    Site {
        name: "minimum of a number and an enumeration",
        declarations: "",
        statements: "y := MIN(x, 1); r := y;",
        strict: "P4027",
        not_strict: "1",
    },
    Site {
        name: "minimum of two enumerations",
        declarations: "",
        statements: "y := MIN(x, z); r := y;",
        strict: "P4027",
        not_strict: "1",
    },
    Site {
        name: "binary selection of two enumerations",
        declarations: "",
        statements: "y := SEL(g, x, z); r := y;",
        strict: "P4027",
        not_strict: "2",
    },
    Site {
        name: "multiplexer of two enumerations",
        declarations: "",
        statements: "y := MUX(1, x, z); r := y;",
        strict: "P4027",
        not_strict: "1",
    },
    Site {
        name: "integer variable assigned",
        declarations: "",
        statements: "i := 1; y := i; r := y;",
        strict: "P4035",
        not_strict: "1",
    },
    Site {
        name: "integer expression assigned",
        declarations: "",
        statements: "i := 1; y := i + 1; r := y;",
        strict: "P4035",
        not_strict: "2",
    },
    Site {
        name: "value of another enumeration assigned",
        declarations: "",
        statements: "y := z; r := y;",
        strict: "P4035",
        not_strict: "1",
    },
    Site {
        name: "integer variable passed",
        declarations: "",
        statements: "i := 1; r := take(i);",
        strict: "P4026",
        not_strict: "1",
    },
    Site {
        name: "integer returned",
        declarations: "FUNCTION give : E1 VAR_INPUT n : INT; END_VAR give := n; END_FUNCTION",
        statements: "y := give(2); r := y;",
        strict: "P4035",
        not_strict: "2",
    },
    Site {
        name: "constant that names no value assigned",
        declarations: "",
        statements: "y := 7; r := y;",
        strict: "P2006",
        not_strict: "7",
    },
    Site {
        name: "negative constant assigned",
        declarations: "",
        statements: "y := -1; r := y;",
        strict: "P2006",
        not_strict: "-1",
    },
    Site {
        name: "constant that names no value passed",
        declarations: "",
        statements: "r := take(7);",
        strict: "P2006",
        not_strict: "7",
    },
    Site {
        name: "sum wrapping as an INT does",
        declarations: "",
        statements: "y := 32000 + 0; y := y + 1000; r := y;",
        strict: "P4049",
        not_strict: "-32536",
    },
    Site {
        name: "difference below the first value",
        declarations: "",
        statements: "y := x - 5; r := y;",
        strict: "P4049",
        not_strict: "-3",
    },
    Site {
        name: "enumeration assigned to itself and to an integer",
        declarations: "",
        statements: "y := x; i := y; r := i;",
        strict: "2",
        not_strict: "2",
    },
];

/// The cells of `SITES` whose outcome is not the one the table states, in each
/// configuration.
fn site_mismatches() -> Vec<String> {
    let mut found = Vec::new();
    for config in configs() {
        for site in &SITES {
            let expected = if config.strict {
                site.strict
            } else {
                site.not_strict
            };
            let got = outcome_of(&program_of(site.declarations, site.statements), &config);
            if got != expected {
                found.push(format!(
                    "{} / {}: expected {expected}, got {got}",
                    config.name, site.name
                ));
            }
        }
    }
    found
}

#[test]
#[spec_test(REQ_ATTR_codegen_010)]
fn enumeration_when_place_of_the_class_then_refused_only_when_strict_and_its_base_type_when_not() {
    assert_eq!(site_mismatches(), Vec::<String>::new());
}

/// A place in which an enumeration is only held, or compared, is the same
/// whether the enumeration is strict or not.
const HELD: [(&str, &str, &str); 8] = [
    (
        "compared with a number",
        "g := x = 2; IF g THEN r := 1; END_IF;",
        "1",
    ),
    (
        "compared with another enumeration",
        "g := x = z; IF g THEN r := 1; END_IF;",
        "0",
    ),
    ("converted", "r := INT_TO_DINT(x);", "2"),
    ("assigned to an integer", "i := x; r := i;", "2"),
    ("passed", "r := take(x);", "2"),
    ("selected", "y := SEL(g, x, A); r := y;", "2"),
    (
        "selected among enumerations",
        "y := MAX(x, y); r := y;",
        "2",
    ),
    ("constant that names a value", "y := 1; r := y;", "1"),
];

#[test]
fn enumeration_when_only_held_or_compared_then_same_whether_strict_or_not() {
    let mut found = Vec::new();
    for config in configs() {
        for (name, statements, expected) in HELD {
            let got = outcome_of(&program_of("", statements), &config);
            if got != expected {
                found.push(format!(
                    "{} / {name}: expected {expected}, got {got}",
                    config.name
                ));
            }
        }
    }
    assert_eq!(found, Vec::<String>::new());
}

/// The places a constant is stored into an enumeration, with a constant that
/// names none of its values: refused when the enumeration is strict, stored
/// when it is not.
#[test]
fn enumeration_when_constant_that_names_no_value_stored_in_every_place_then_refused_only_when_strict(
) {
    let mut found = Vec::new();
    for config in configs() {
        let expected = if config.strict { "P2006" } else { "7" };
        for place in &PLACES {
            let got = outcome_of(&store_program(place, "(A, B, C)", "7"), &config);
            if got != expected {
                found.push(format!(
                    "{} / {}: expected {expected}, got {got}",
                    config.name, place.name
                ));
            }
        }
    }
    assert_eq!(found, Vec::<String>::new());
}

/// A program that declares enumerations with a base type, a structure with a
/// member of one, and leaves in `r` the number the statements compute. The
/// base type is what bounds the numbers a non-strict enumeration holds, so a
/// sum wraps as it does in a variable of the base type.
fn base_type_program(statements: &str) -> String {
    format!(
        "TYPE ES : (S1, S2) SINT; END_TYPE TYPE EW : (W1, W2) WORD; END_TYPE \
         TYPE EL : (L1, L2) LINT; END_TYPE TYPE E1 : (A, B, C); END_TYPE \
         TYPE S : STRUCT m : E1; END_STRUCT; END_TYPE \
         TYPE SS : STRUCT m : ES; END_STRUCT; END_TYPE \
         PROGRAM main VAR r : DINT; es : ES; ew : EW; el : EL; s : S; ss : SS; \
         l : LINT; END_VAR {statements} END_PROGRAM"
    )
}

const BASE_TYPES: [(&str, &str, &str); 8] = [
    (
        "a SINT enumeration wraps at eight bits",
        "es := 100; es := es + 100; r := SINT_TO_DINT(es);",
        "-56",
    ),
    (
        "a WORD enumeration wraps at sixteen bits",
        "ew := 65500; ew := ew + 100; r := WORD_TO_DINT(ew);",
        "64",
    ),
    (
        "a LINT enumeration keeps sixty-four bits",
        "el := L2; el := el + 5000000000; r := LINT_TO_DINT(el - 4999999990);",
        "11",
    ),
    (
        "a LINT enumeration is stored as a LINT",
        "l := 5000000000; el := l; l := el; r := LINT_TO_DINT(l - 4999999990);",
        "10",
    ),
    (
        "a member of an INT enumeration keeps its number",
        "s.m := 300; r := s.m;",
        "300",
    ),
    (
        "a member of an INT enumeration wraps at sixteen bits",
        "s.m := 30000; s.m := s.m + 5000; r := s.m;",
        "-30536",
    ),
    (
        "a member of a SINT enumeration wraps at eight bits",
        "ss.m := 100; ss.m := ss.m + 100; r := SINT_TO_DINT(ss.m);",
        "-56",
    ),
    ("the sum of a member", "s.m := 3; r := s.m * 2;", "6"),
];

#[test]
#[spec_test(REQ_ATTR_codegen_011)]
fn enumeration_when_not_strict_then_numbers_are_held_as_its_base_type_holds_them() {
    let config = Config {
        name: "CODESYS without the attribute",
        options: CompilerOptions::from_dialect(Dialect::Codesys),
        attribute: false,
        strict: false,
    };
    let mut found = Vec::new();
    for (name, statements, expected) in BASE_TYPES {
        let got = outcome_of(&base_type_program(statements), &config);
        if got != expected {
            found.push(format!("{name}: expected {expected}, got {got}"));
        }
    }
    assert_eq!(found, Vec::<String>::new());
}

#[test]
fn enumeration_when_alias_of_a_strict_one_then_strict_too() {
    let config = Config {
        name: "CODESYS",
        options: CompilerOptions::from_dialect(Dialect::Codesys),
        attribute: false,
        strict: true,
    };
    let source = "{attribute 'strict'} TYPE E1 : (A, B, C); END_TYPE TYPE E3 : E1; END_TYPE \
                  PROGRAM main VAR r : DINT; w : E3 := C; END_VAR r := w + 1; END_PROGRAM";

    assert_eq!(outcome_of(source, &config), "P4049");
}

#[test]
fn enumeration_when_written_inline_in_a_variable_then_not_strict_under_the_attribute_policy() {
    let source = "PROGRAM main VAR r : DINT; w : (P, Q, R) := R; END_VAR r := w + 1; END_PROGRAM";
    let strict_for_everyone = CompilerOptions::from_dialect(Dialect::Iec61131_3Ed2);
    let with_attribute = CompilerOptions::from_dialect(Dialect::Codesys);

    let strict =
        try_check_and_run(source, &strict_for_everyone).map(|(_, bufs)| bufs.vars[0].as_i32());
    let lax = try_check_and_run(source, &with_attribute).map(|(_, bufs)| bufs.vars[0].as_i32());

    assert_eq!(strict.err().map(|d| d.code), Some("P4049".to_owned()));
    assert_eq!(lax.ok(), Some(3));
}
