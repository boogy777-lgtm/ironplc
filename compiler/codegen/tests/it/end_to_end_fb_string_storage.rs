//! End-to-end tests for the storage of the STRING variables of a function
//! block: every instance owns its own copy, whichever section declares it and
//! however its value is given.
//!
//! A string is observed through `LEN`, which reads the stored value back, so
//! two instances that shared storage would disagree with what was written to
//! each. Result variables are declared first in every program, so they take
//! the slots 0, 1, ...

use ironplc_parser::options::CompilerOptions;

use crate::common::assert_run_i32_with;

fn methods_enabled() -> CompilerOptions {
    CompilerOptions {
        allow_fb_inheritance: true,
        ..CompilerOptions::default()
    }
}

// One instance writes its VAR string and reads it back.
e2e_i32!(
    end_to_end_when_fb_writes_var_string_then_reads_it_back,
    "
FUNCTION_BLOCK F
  VAR_INPUT n : DINT; END_VAR
  VAR_OUTPUT len : DINT; END_VAR
  VAR s : STRING; END_VAR
  IF n = 1 THEN s := 'abcde'; END_IF;
  len := LEN(s);
END_FUNCTION_BLOCK

PROGRAM main
  VAR la : DINT; a : F; END_VAR
  a(n := 1);
  a(n := 0);
  la := a.len;
END_PROGRAM
",
    &[(0, 5)],
);

// Two instances of one block write different strings into a VAR string; each
// reads back what it wrote, however the calls interleave.
e2e_i32!(
    end_to_end_when_two_fb_instances_write_var_string_then_each_keeps_its_own,
    "
FUNCTION_BLOCK F
  VAR_INPUT n : DINT; END_VAR
  VAR_OUTPUT len : DINT; END_VAR
  VAR s : STRING; END_VAR
  IF n = 1 THEN s := 'abcde'; ELSIF n = 2 THEN s := 'xy'; END_IF;
  len := LEN(s);
END_FUNCTION_BLOCK

PROGRAM main
  VAR la : DINT; lb : DINT; a : F; b : F; END_VAR
  a(n := 1);
  b(n := 2);
  a(n := 0);
  b(n := 0);
  la := a.len;
  lb := b.len;
END_PROGRAM
",
    &[(0, 5), (1, 2)],
);

// The same for a WSTRING, whose run is wider.
e2e_i32!(
    end_to_end_when_two_fb_instances_write_var_wstring_then_each_keeps_its_own,
    "
FUNCTION_BLOCK F
  VAR_INPUT n : DINT; END_VAR
  VAR_OUTPUT len : DINT; END_VAR
  VAR s : WSTRING; END_VAR
  IF n = 1 THEN s := \"abcde\"; ELSIF n = 2 THEN s := \"xy\"; END_IF;
  len := LEN(s);
END_FUNCTION_BLOCK

PROGRAM main
  VAR la : DINT; lb : DINT; a : F; b : F; END_VAR
  a(n := 1);
  b(n := 2);
  a(n := 0);
  b(n := 0);
  la := a.len;
  lb := b.len;
END_PROGRAM
",
    &[(0, 5), (1, 2)],
);

// A string a block declares with a value starts at that value in every
// instance, before any call writes it.
e2e_i32!(
    end_to_end_when_fb_var_string_has_declared_value_then_every_instance_starts_with_it,
    "
FUNCTION_BLOCK F
  VAR_OUTPUT len : DINT; END_VAR
  VAR s : STRING := 'abc'; END_VAR
  len := LEN(s);
END_FUNCTION_BLOCK

PROGRAM main
  VAR la : DINT; lb : DINT; a : F; b : F; END_VAR
  a();
  b();
  la := a.len;
  lb := b.len;
END_PROGRAM
",
    &[(0, 3), (1, 3)],
);

// A write to one instance does not reach the declared value of the other.
e2e_i32!(
    end_to_end_when_one_instance_overwrites_declared_string_then_other_keeps_declared_value,
    "
FUNCTION_BLOCK F
  VAR_INPUT n : DINT; END_VAR
  VAR_OUTPUT len : DINT; END_VAR
  VAR s : STRING := 'abc'; END_VAR
  IF n = 1 THEN s := 'abcdefg'; END_IF;
  len := LEN(s);
END_FUNCTION_BLOCK

PROGRAM main
  VAR la : DINT; lb : DINT; a : F; b : F; END_VAR
  a(n := 1);
  b(n := 0);
  la := a.len;
  lb := b.len;
END_PROGRAM
",
    &[(0, 7), (1, 3)],
);

// An instance's own initializer states a value for the string, over the one
// the block declares.
e2e_i32!(
    end_to_end_when_instance_initializer_states_string_then_each_instance_has_its_own,
    "
FUNCTION_BLOCK F
  VAR_OUTPUT len : DINT; END_VAR
  VAR s : STRING := 'abc'; END_VAR
  len := LEN(s);
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    la : DINT; lb : DINT; lc : DINT;
    a : F := (s := 'x');
    b : F := (s := 'yyyy');
    c : F;
  END_VAR
  a();
  b();
  c();
  la := a.len;
  lb := b.len;
  lc := c.len;
END_PROGRAM
",
    &[(0, 1), (1, 4), (2, 3)],
);

// A VAR_INPUT string is set by the call, into the instance's own run.
e2e_i32!(
    end_to_end_when_two_fb_instances_take_string_input_then_each_keeps_its_own,
    "
FUNCTION_BLOCK F
  VAR_INPUT s : STRING; END_VAR
  VAR_OUTPUT len : DINT; END_VAR
  len := LEN(s);
END_FUNCTION_BLOCK

PROGRAM main
  VAR la : DINT; lb : DINT; a : F; b : F; END_VAR
  a(s := 'abcde');
  b(s := 'xy');
  la := a.len;
  lb := b.len;
END_PROGRAM
",
    &[(0, 5), (1, 2)],
);

// A VAR_OUTPUT string is written by the block, read from outside through the
// instance, and bound to a string variable with `=>`.
e2e_i32!(
    end_to_end_when_two_fb_instances_write_output_string_then_each_reads_its_own,
    "
FUNCTION_BLOCK F
  VAR_INPUT n : DINT; END_VAR
  VAR_OUTPUT s : STRING; END_VAR
  IF n = 1 THEN s := 'abcde'; ELSE s := 'xy'; END_IF;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    la : DINT; lb : DINT; lc : DINT; ld : DINT;
    a : F; b : F;
    bound : STRING;
  END_VAR
  a(n := 1);
  b(n := 2, s => bound);
  la := LEN(a.s);
  lb := LEN(b.s);
  lc := LEN(bound);
  ld := LEN(a.s);
END_PROGRAM
",
    &[(0, 5), (1, 2), (2, 2), (3, 5)],
);

// A string field is written from outside by assignment, into the run of the
// instance named, and only that one.
e2e_i32!(
    end_to_end_when_program_assigns_instance_string_then_only_that_instance_changes,
    "
FUNCTION_BLOCK F
  VAR_INPUT s : STRING; END_VAR
  VAR_OUTPUT len : DINT; END_VAR
  len := LEN(s);
END_FUNCTION_BLOCK

PROGRAM main
  VAR la : DINT; lb : DINT; a : F; b : F; END_VAR
  a(s := 'ab');
  b(s := 'cd');
  a.s := 'abcdef';
  a();
  b();
  la := a.len;
  lb := b.len;
END_PROGRAM
",
    &[(0, 6), (1, 2)],
);

// Strings of different capacities in one block each occupy a run of their own
// size, so neither overruns the other.
e2e_i32!(
    end_to_end_when_fb_has_two_strings_of_different_capacity_then_runs_do_not_overlap,
    "
FUNCTION_BLOCK F
  VAR_OUTPUT len1 : DINT; len2 : DINT; END_VAR
  VAR first : STRING[5] := 'abcde'; second : STRING[12] := 'ABCDEFGHIJKL'; END_VAR
  len1 := LEN(first);
  len2 := LEN(second);
  first := 'zzzzzzzz';
END_FUNCTION_BLOCK

PROGRAM main
  VAR la : DINT; lb : DINT; a : F; b : F; END_VAR
  a();
  b();
  la := a.len1 + b.len1;
  lb := a.len2 + b.len2;
END_PROGRAM
",
    &[(0, 10), (1, 24)],
);

// A STRING local of a method starts afresh on every call, whichever instance
// calls it.
#[test]
fn end_to_end_when_method_has_string_local_then_every_call_starts_from_its_value() {
    assert_run_i32_with(
        "
FUNCTION_BLOCK F
  VAR_OUTPUT len : DINT; END_VAR
  METHOD M : DINT
    VAR_INPUT k : DINT; END_VAR
    VAR s : STRING := 'abc'; END_VAR
    IF k = 1 THEN s := 'abcde'; END_IF;
    M := LEN(s);
  END_METHOD
END_FUNCTION_BLOCK

PROGRAM main
  VAR la : DINT; lb : DINT; lc : DINT; a : F; b : F; END_VAR
  la := a.M(k := 1);
  lb := a.M(k := 0);
  lc := b.M(k := 0);
END_PROGRAM
",
        &methods_enabled(),
        &[(0, 5), (1, 3), (2, 3)],
    );
}

// A string local of a method and a string field of the type with the same
// name are two variables: the local hides the field only inside the method.
#[test]
fn end_to_end_when_method_local_string_hides_field_then_field_is_unchanged() {
    assert_run_i32_with(
        "
FUNCTION_BLOCK F
  VAR_OUTPUT len : DINT; END_VAR
  VAR s : STRING := 'abc'; END_VAR
  METHOD M : DINT
    VAR s : STRING := 'abcdefgh'; END_VAR
    M := LEN(s);
  END_METHOD
  METHOD Field : DINT
    Field := LEN(s);
  END_METHOD
END_FUNCTION_BLOCK

PROGRAM main
  VAR la : DINT; lb : DINT; a : F; END_VAR
  la := a.M();
  lb := a.Field();
END_PROGRAM
",
        &methods_enabled(),
        &[(0, 8), (1, 3)],
    );
}
