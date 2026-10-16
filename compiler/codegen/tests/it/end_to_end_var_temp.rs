//! End-to-end integration tests for VAR_TEMP declarations.

use ironplc_container::VarIndex;
use ironplc_parser::options::CompilerOptions;

use crate::common::parse_and_run_rounds;

e2e_i32!(
    end_to_end_when_function_with_var_temp_then_correct,
    "
FUNCTION add_doubled : DINT
  VAR_INPUT
    a : DINT;
    b : DINT;
  END_VAR
  VAR_TEMP
    temp : DINT;
  END_VAR
  temp := a + b;
  add_doubled := temp * 2;
END_FUNCTION

PROGRAM main
  VAR
    result : DINT;
  END_VAR
  result := add_doubled(3, 4);
END_PROGRAM
",
    &[(0, 14)],
);

e2e_i32!(
    end_to_end_when_function_with_multiple_var_temp_then_correct,
    "
FUNCTION compute : DINT
  VAR_INPUT
    x : DINT;
  END_VAR
  VAR_TEMP
    a : DINT;
    b : DINT;
  END_VAR
  a := x + 1;
  b := a * 3;
  compute := b;
END_FUNCTION

PROGRAM main
  VAR
    result : DINT;
  END_VAR
  result := compute(4);
END_PROGRAM
",
    &[(0, 15)],
);

// A program's VAR_TEMP is working memory of one scan: it starts again at its
// declared initial value (or the default of its type) at every scan, and no
// value survives to the next. The tests drive three scans and read `x` after
// each. The persistent variables are laid out first, so `x` is at index 0
// wherever the VAR_TEMP block is written.

/// Runs `source` for three scans and asserts that the variable at index 0
/// reads `expected` after each of them.
fn assert_every_scan(source: &str, expected: i32) {
    parse_and_run_rounds(source, &CompilerOptions::default(), |vm| {
        for scan in 1..=3 {
            vm.run_round(0).unwrap();
            assert_eq!(
                vm.read_variable(VarIndex::new(0)).unwrap(),
                expected,
                "after scan {scan}"
            );
        }
    });
}

#[test]
fn end_to_end_when_program_var_temp_has_initial_value_then_it_is_restored_every_scan() {
    assert_every_scan(
        "
PROGRAM main
  VAR_TEMP
    t : INT := 5;
  END_VAR
  VAR
    x : INT;
  END_VAR
  t := t + 1;
  x := t;
END_PROGRAM
",
        6,
    );
}

#[test]
fn end_to_end_when_program_var_temp_has_no_initial_value_then_it_is_zero_every_scan() {
    assert_every_scan(
        "
PROGRAM main
  VAR
    x : INT;
  END_VAR
  VAR_TEMP
    t : INT;
  END_VAR
  t := t + 2;
  x := t;
END_PROGRAM
",
        2,
    );
}

#[test]
fn end_to_end_when_program_var_and_var_temp_both_count_then_only_var_keeps_its_value() {
    parse_and_run_rounds(
        "
PROGRAM main
  VAR
    kept : INT;
    seen : INT;
  END_VAR
  VAR_TEMP
    lost : INT;
  END_VAR
  kept := kept + 1;
  lost := lost + 1;
  seen := lost;
END_PROGRAM
",
        &CompilerOptions::default(),
        |vm| {
            for scan in 1..=3 {
                vm.run_round(0).unwrap();
                assert_eq!(vm.read_variable(VarIndex::new(0)).unwrap(), scan);
                assert_eq!(vm.read_variable(VarIndex::new(1)).unwrap(), 1);
            }
        },
    );
}

#[test]
fn end_to_end_when_program_var_temp_is_a_string_then_it_is_restored_every_scan() {
    assert_every_scan(
        "
PROGRAM main
  VAR
    x : INT;
  END_VAR
  VAR_TEMP
    s : STRING[10] := 'ab';
  END_VAR
  s := CONCAT(s, 'c');
  x := LEN(s);
END_PROGRAM
",
        3,
    );
}

#[test]
fn end_to_end_when_program_var_temp_is_an_array_then_it_is_restored_every_scan() {
    assert_every_scan(
        "
PROGRAM main
  VAR
    x : INT;
  END_VAR
  VAR_TEMP
    arr : ARRAY[1..3] OF INT := [1, 2, 3];
  END_VAR
  arr[1] := arr[1] + 10;
  x := arr[1];
END_PROGRAM
",
        11,
    );
}

#[test]
fn end_to_end_when_program_var_temp_is_a_structure_then_it_is_restored_every_scan() {
    assert_every_scan(
        "
TYPE Pair : STRUCT a : INT := 7; b : INT; END_STRUCT; END_TYPE

PROGRAM main
  VAR
    x : INT;
  END_VAR
  VAR_TEMP
    p : Pair;
  END_VAR
  p.a := p.a + 1;
  p.b := p.b + 1;
  x := p.a * 100 + p.b;
END_PROGRAM
",
        801,
    );
}

#[test]
fn end_to_end_when_function_var_temp_is_declared_then_it_is_restored_at_every_call() {
    // The function sees `t` at 5 and `z` at 0 in each call; a value kept from a
    // call would make the sum grow.
    assert_every_scan(
        "
FUNCTION bump : INT
  VAR_TEMP
    t : INT := 5;
    z : INT;
  END_VAR
  t := t + 1;
  z := z + 1;
  bump := t + z * 100;
END_FUNCTION

PROGRAM main
  VAR
    x : INT;
  END_VAR
  x := bump();
END_PROGRAM
",
        106,
    );
}

// An instance in a program's VAR_TEMP is a new instance at every scan: each
// field is its declared value or the default of its type, as in an instance
// that has never run. The persistent variables come first, so `rn` is at index
// 0, `rk` at 1 and `ls` at 2.

const COUNTER_BLOCK: &str = "
FUNCTION_BLOCK Cnt
  VAR n : INT; k : INT := 10; s : STRING[8] := 'x'; END_VAR
  n := n + 1;
  k := k + 1;
  s := CONCAT(s, 'y');
END_FUNCTION_BLOCK
";

/// Runs `source` for three scans and asserts that variables 0, 1 and 2 read
/// `expected` after each of them.
fn assert_instance_every_scan(source: &str, expected: [i32; 3]) {
    parse_and_run_rounds(source, &CompilerOptions::default(), |vm| {
        for scan in 1..=3 {
            vm.run_round(0).unwrap();
            for (index, value) in expected.iter().enumerate() {
                assert_eq!(
                    vm.read_variable(VarIndex::new(index as u16)).unwrap(),
                    *value,
                    "variable {index} after scan {scan}"
                );
            }
        }
    });
}

#[test]
fn end_to_end_when_program_var_temp_is_a_function_block_instance_then_it_is_new_at_every_scan() {
    let source = format!(
        "{COUNTER_BLOCK}
PROGRAM main
  VAR rn : INT; rk : INT; ls : INT; END_VAR
  VAR_TEMP c : Cnt; END_VAR
  c();
  rn := c.n;
  rk := c.k;
  ls := LEN(c.s);
END_PROGRAM
"
    );

    assert_instance_every_scan(&source, [1, 11, 2]);
}

#[test]
fn end_to_end_when_program_var_temp_instance_states_a_member_then_it_starts_from_it_every_scan() {
    let source = format!(
        "{COUNTER_BLOCK}
PROGRAM main
  VAR rn : INT; rk : INT; ls : INT; END_VAR
  VAR_TEMP c : Cnt := (k := 20); END_VAR
  c();
  rn := c.n;
  rk := c.k;
  ls := LEN(c.s);
END_PROGRAM
"
    );

    assert_instance_every_scan(&source, [1, 21, 2]);
}

#[test]
fn end_to_end_when_program_var_is_a_function_block_instance_then_it_keeps_its_fields_between_scans()
{
    // The near case: an instance in VAR is initialised once and is state.
    let source = format!(
        "{COUNTER_BLOCK}
PROGRAM main
  VAR rn : INT; rk : INT; ls : INT; c : Cnt; END_VAR
  c();
  rn := c.n;
  rk := c.k;
  ls := LEN(c.s);
END_PROGRAM
"
    );

    parse_and_run_rounds(&source, &CompilerOptions::default(), |vm| {
        for scan in 1..=3 {
            vm.run_round(0).unwrap();
            assert_eq!(vm.read_variable(VarIndex::new(0)).unwrap(), scan);
            assert_eq!(vm.read_variable(VarIndex::new(1)).unwrap(), 10 + scan);
            assert_eq!(vm.read_variable(VarIndex::new(2)).unwrap(), 1 + scan);
        }
    });
}

#[test]
fn end_to_end_when_program_var_temp_is_an_array_of_function_block_instances_then_each_is_new_at_every_scan(
) {
    let source = "
FUNCTION_BLOCK Cnt
  VAR n : INT; k : INT := 10; END_VAR
  n := n + 1;
  k := k + 1;
END_FUNCTION_BLOCK

PROGRAM main
  VAR rn : INT; rk : INT; END_VAR
  VAR_TEMP cs : ARRAY[1..2] OF Cnt; END_VAR
  cs[1]();
  cs[2]();
  rn := cs[2].n;
  rk := cs[2].k;
END_PROGRAM
";
    let options = CompilerOptions::from_dialect(ironplc_parser::options::Dialect::Codesys);

    parse_and_run_rounds(source, &options, |vm| {
        for scan in 1..=3 {
            vm.run_round(0).unwrap();
            assert_eq!(
                vm.read_variable(VarIndex::new(0)).unwrap(),
                1,
                "n, scan {scan}"
            );
            assert_eq!(
                vm.read_variable(VarIndex::new(1)).unwrap(),
                11,
                "k, scan {scan}"
            );
        }
    });
}
