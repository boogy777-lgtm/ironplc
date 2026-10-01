//! End-to-end tests for the CODESYS literal forms: the base-10 based integer
//! (`10#`), the typed boolean digits (`BOOL#0`, `BOOL#1`), and the one-bit
//! `BIT` type with its `BIT#0` and `BIT#1` literals. See
//! `specs/design/numeric-literals.md`.

use ironplc_parser::options::CompilerOptions;

use crate::common::parse_and_run;

// x is at variable slot 0.
e2e_i32!(
    end_to_end_when_decimal_based_literal_then_value,
    "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := 10#123;
END_PROGRAM
",
    &[(0, 123)],
);

e2e_i32!(
    end_to_end_when_decimal_based_literal_with_underscore_then_value,
    "
PROGRAM main
  VAR
    x : DINT;
  END_VAR
  x := 10#1_000;
END_PROGRAM
",
    &[(0, 1000)],
);

e2e_i32!(
    end_to_end_when_typed_boolean_one_then_true,
    "
PROGRAM main
  VAR
    b : BOOL;
  END_VAR
  b := BOOL#1;
END_PROGRAM
",
    &[(0, 1)],
);

e2e_i32!(
    end_to_end_when_typed_boolean_zero_then_false,
    "
PROGRAM main
  VAR
    b : BOOL;
  END_VAR
  b := BOOL#0;
END_PROGRAM
",
    &[(0, 0)],
);

// The one-bit type is an alias of BOOL: a BIT variable takes the two BIT
// literals and compares like a BOOL.
#[test]
fn end_to_end_when_bit_type_assigns_and_compares_then_bool_results() {
    let source = "
PROGRAM main
  VAR
    b : BIT;
    one : BOOL;
    two : BOOL;
  END_VAR
  b := BIT#1;
  one := b = BIT#1;
  b := BIT#0;
  two := b = BIT#0;
END_PROGRAM
";
    let options = CompilerOptions {
        allow_bit_type: true,
        ..CompilerOptions::default()
    };
    let (_c, bufs) = parse_and_run(source, &options);
    // `b` is left BIT#0 by the last assignment; `one` and `two` are BOOLs,
    // which read back as 1 and 0 in their slot.
    let vars: Vec<i32> = bufs.vars.iter().map(|v| v.as_i32()).collect();
    assert_eq!(vars, vec![0, 1, 1]);
}
