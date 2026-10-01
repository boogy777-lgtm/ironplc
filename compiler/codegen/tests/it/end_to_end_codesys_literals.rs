//! End-to-end tests for the CODESYS literal forms: the base-10 based integer
//! (`10#`), the typed boolean digits (`BOOL#0`, `BOOL#1`), the one-bit `BIT`
//! type with its `BIT#0` and `BIT#1` literals, the sub-millisecond duration
//! units with the abbreviated `LT#` prefix, and a time of day without
//! seconds. See `specs/design/numeric-literals.md` and
//! `specs/design/time-literals.md`.

use ironplc_parser::options::{CompilerOptions, Dialect};

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

// `T#1us` is one thousand nanoseconds; the 32-bit TIME stores milliseconds,
// so it truncates to 0 (ADR-0021).
e2e_i32!(
    end_to_end_when_microsecond_duration_then_time_truncates,
    "
PROGRAM main
  VAR
    t : TIME;
  END_VAR
  t := T#1us;
END_PROGRAM
",
    &[(0, 0)],
);

e2e_i32!(
    end_to_end_when_compound_microsecond_duration_then_time_truncates,
    "
PROGRAM main
  VAR
    t : TIME;
  END_VAR
  t := T#1ms500us;
END_PROGRAM
",
    &[(0, 1)],
);

// `LT#` is the abbreviated `LTIME#`; LTIME stores milliseconds as i64, so
// 1500 microseconds truncate to 1.
e2e_i64_with!(
    end_to_end_when_lt_abbreviation_then_ltime_value,
    CompilerOptions::from_dialect(Dialect::Codesys),
    "
PROGRAM main
  VAR
    t : LTIME;
  END_VAR
  t := LT#1500us;
END_PROGRAM
",
    &[(0, 1)],
);

// A time of day without seconds is the same value with zero seconds:
// 10:00 is 36,000,000 ms past midnight.
e2e_i32!(
    end_to_end_when_time_of_day_omits_seconds_then_zero_seconds,
    "
PROGRAM main
  VAR
    a : TIME_OF_DAY;
    ms : DINT;
  END_VAR
  a := TOD#10:00;
  ms := TOD_TO_DINT(a);
END_PROGRAM
",
    &[(1, 36_000_000)],
);
