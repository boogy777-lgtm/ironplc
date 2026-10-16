//! End-to-end tests for the VAR_TEMP of a function block and of a method.
//!
//! A `VAR_TEMP` belongs to one call: it starts at its declared initial value
//! (or the default of its type) at every call, and it is no field of the
//! instance. The tests drive three scans and read the results the program
//! copies out of the instance after each of them, as the tests of the `VAR_TEMP`
//! of a program and of a function do (`end_to_end_var_temp`).

use ironplc_container::VarIndex;
use ironplc_parser::options::{CompilerOptions, Dialect};

use crate::common::{parse_and_run_rounds, try_parse_and_compile};
use crate::end_to_end_var_temp::{assert_every_scan, assert_instance_every_scan, COUNTER_BLOCK};

/// A program that calls the block `Blk` once and copies its output `y` to the
/// variable at index 0.
const CALL_BLK: &str = "
PROGRAM main
  VAR x : INT; f : Blk; END_VAR
  f();
  x := f.y;
END_PROGRAM
";

/// The block `Blk`, whose declarations are `declarations` and whose body is
/// `body`, and the program that calls it.
fn blk(declarations: &str, body: &str) -> String {
    format!(
        "FUNCTION_BLOCK Blk
  VAR_OUTPUT y : INT; END_VAR
  {declarations}
  {body}
END_FUNCTION_BLOCK
{CALL_BLK}"
    )
}

#[test]
fn end_to_end_when_function_block_var_temp_has_initial_value_then_it_is_restored_every_call() {
    assert_every_scan(
        &blk("VAR_TEMP t : INT := 5; END_VAR", "t := t + 1; y := t;"),
        6,
    );
}

#[test]
fn end_to_end_when_function_block_var_temp_has_no_initial_value_then_it_is_zero_every_call() {
    assert_every_scan(&blk("VAR_TEMP t : INT; END_VAR", "t := t + 2; y := t;"), 2);
}

#[test]
fn end_to_end_when_function_block_var_temp_is_a_string_then_it_is_restored_every_call() {
    assert_every_scan(
        &blk(
            "VAR_TEMP s : STRING[10] := 'ab'; END_VAR",
            "s := CONCAT(s, 'c'); y := LEN(s);",
        ),
        3,
    );
}

#[test]
fn end_to_end_when_function_block_var_temp_is_an_array_then_it_is_restored_every_call() {
    assert_every_scan(
        &blk(
            "VAR_TEMP arr : ARRAY[1..3] OF INT := [1, 2, 3]; END_VAR",
            "arr[1] := arr[1] + 10; y := arr[1];",
        ),
        11,
    );
}

#[test]
fn end_to_end_when_function_block_var_temp_is_a_structure_then_it_is_restored_every_call() {
    let source = format!(
        "TYPE Pair : STRUCT a : INT := 7; b : INT; END_STRUCT; END_TYPE
{}",
        blk(
            "VAR_TEMP p : Pair; END_VAR",
            "p.a := p.a + 1; p.b := p.b + 1; y := p.a * 100 + p.b;"
        )
    );

    assert_every_scan(&source, 801);
}

#[test]
fn end_to_end_when_function_block_var_and_var_temp_both_count_then_only_var_keeps_its_value() {
    // Index 0 is `x`, the program's copy of `y`; the instance keeps `k`.
    let source = blk(
        "VAR k : INT; END_VAR VAR_TEMP t : INT; END_VAR",
        "k := k + 1; t := t + 1; y := k * 100 + t;",
    );

    parse_and_run_rounds(&source, &CompilerOptions::default(), |vm| {
        for scan in 1..=3 {
            vm.run_round(0).unwrap();
            assert_eq!(
                vm.read_variable(VarIndex::new(0)).unwrap(),
                scan * 100 + 1,
                "after scan {scan}"
            );
        }
    });
}

#[test]
fn end_to_end_when_function_block_is_called_twice_in_a_scan_then_var_temp_is_restored_at_each_call()
{
    // `t` is 5 at both calls: a value kept from the first call would make the
    // second read 7. The call count `k` is state of the instance.
    let source = "
FUNCTION_BLOCK Blk
  VAR_OUTPUT y : INT; k : INT; END_VAR
  VAR_TEMP t : INT := 5; END_VAR
  t := t + 1;
  y := t;
  k := k + 1;
END_FUNCTION_BLOCK

PROGRAM main
  VAR x : INT; n : INT; f : Blk; END_VAR
  f();
  f();
  x := f.y;
  n := f.k;
END_PROGRAM
";

    parse_and_run_rounds(source, &CompilerOptions::default(), |vm| {
        for scan in 1..=3 {
            vm.run_round(0).unwrap();
            assert_eq!(vm.read_variable(VarIndex::new(0)).unwrap(), 6);
            assert_eq!(vm.read_variable(VarIndex::new(1)).unwrap(), 2 * scan);
        }
    });
}

#[test]
fn end_to_end_when_two_instances_of_one_block_are_called_in_turn_then_they_share_no_var_temp() {
    // Each call adds its input to a temporary that starts at zero, so the
    // outputs are the inputs, wherever the other instance has run in between.
    let source = "
FUNCTION_BLOCK Acc
  VAR_INPUT n : INT; END_VAR
  VAR_OUTPUT y : INT; END_VAR
  VAR_TEMP t : INT; END_VAR
  t := t + n;
  y := t;
END_FUNCTION_BLOCK

PROGRAM main
  VAR ra : INT; rb : INT; rc : INT; f1 : Acc; f2 : Acc; END_VAR
  f1(n := 1);
  ra := f1.y;
  f2(n := 10);
  rb := f2.y;
  f1(n := 2);
  rc := f1.y;
END_PROGRAM
";

    assert_instance_every_scan(source, [1, 10, 2]);
}

#[test]
fn end_to_end_when_block_calls_another_block_then_each_keeps_its_own_var_temp() {
    // Both blocks name their temporary `t`; the outer one adds the inner's
    // result to its own: 100 + (5 + 1).
    let source = "
FUNCTION_BLOCK Inner
  VAR_OUTPUT y : INT; END_VAR
  VAR_TEMP t : INT := 5; END_VAR
  t := t + 1;
  y := t;
END_FUNCTION_BLOCK

FUNCTION_BLOCK Outer
  VAR_OUTPUT y : INT; END_VAR
  VAR_TEMP t : INT := 100; i : Inner; END_VAR
  i();
  t := t + i.y;
  y := t;
END_FUNCTION_BLOCK

PROGRAM main
  VAR x : INT; f : Outer; END_VAR
  f();
  x := f.y;
END_PROGRAM
";

    assert_every_scan(source, 106);
}

/// A program that calls the block `Outer` and copies its three outputs to the
/// variables at index 0, 1 and 2.
const CALL_OUTER: &str = "
PROGRAM main
  VAR a : INT; b : INT; d : INT; f : Outer; END_VAR
  f();
  a := f.rn;
  b := f.rk;
  d := f.ls;
END_PROGRAM
";

/// The block `Outer`, which declares `temporary` in a `VAR_TEMP` block and
/// calls the instance `c` it names, and the program that calls it.
fn outer_with_temporary_instance(temporary: &str) -> String {
    format!(
        "{COUNTER_BLOCK}
FUNCTION_BLOCK Outer
  VAR_OUTPUT rn : INT; rk : INT; ls : INT; END_VAR
  VAR_TEMP {temporary} END_VAR
  c();
  rn := c.n;
  rk := c.k;
  ls := LEN(c.s);
END_FUNCTION_BLOCK
{CALL_OUTER}"
    )
}

#[test]
fn end_to_end_when_function_block_var_temp_is_a_function_block_instance_then_it_is_new_at_every_call(
) {
    assert_instance_every_scan(&outer_with_temporary_instance("c : Cnt;"), [1, 11, 2]);
}

#[test]
fn end_to_end_when_function_block_var_temp_instance_states_a_member_then_it_starts_from_it_every_call(
) {
    assert_instance_every_scan(
        &outer_with_temporary_instance("c : Cnt := (k := 20);"),
        [1, 21, 2],
    );
}

#[test]
fn end_to_end_when_method_var_temp_is_declared_then_it_is_restored_at_every_call() {
    // The method is called twice a scan and reads 6 both times; the field `k`
    // counts the calls.
    let source = "
FUNCTION_BLOCK Blk
  VAR k : INT; END_VAR
  METHOD Bump : INT
    VAR_TEMP t : INT := 5; END_VAR
    t := t + 1;
    k := k + 1;
    Bump := t;
  END_METHOD
END_FUNCTION_BLOCK

PROGRAM main
  VAR a : INT; b : INT; n : INT; f : Blk; END_VAR
  a := f.Bump();
  b := f.Bump();
  n := f.k;
END_PROGRAM
";
    let options = CompilerOptions::from_dialect(Dialect::Codesys);

    parse_and_run_rounds(source, &options, |vm| {
        for scan in 1..=3 {
            vm.run_round(0).unwrap();
            assert_eq!(vm.read_variable(VarIndex::new(0)).unwrap(), 6);
            assert_eq!(vm.read_variable(VarIndex::new(1)).unwrap(), 6);
            assert_eq!(vm.read_variable(VarIndex::new(2)).unwrap(), 2 * scan);
        }
    });
}

// The VAR_TEMP of a block is not a field of the instance: no program reaches
// it as `f.t`, and no method of the block sees it. The analysis does not check
// the members of an instance, so code generation refuses these: a read or a
// write of `f.t` as an unknown field (P9999, whose message names a property as
// the likely meaning), and a method's use of `t` as an undefined variable
// (P4007).

#[test]
fn compile_when_program_reads_function_block_var_temp_then_unknown_field() {
    let source = blk("VAR_TEMP t : INT := 5; END_VAR", "y := t;").replace("x := f.y;", "x := f.t;");

    let diagnostic = try_parse_and_compile(&source, &CompilerOptions::default()).unwrap_err();

    assert_eq!(diagnostic.code, "P9999");
    let start = source.find("f.t;").unwrap() + "f.".len();
    assert_eq!(diagnostic.primary.location.start, start);
}

#[test]
fn compile_when_program_writes_function_block_var_temp_then_unknown_field() {
    let source = blk("VAR_TEMP t : INT := 5; END_VAR", "y := t;").replace("f();", "f.t := 3;");

    let diagnostic = try_parse_and_compile(&source, &CompilerOptions::default()).unwrap_err();

    assert_eq!(diagnostic.code, "P9999");
}

#[test]
fn compile_when_method_uses_function_block_var_temp_then_variable_not_defined() {
    let source = "
FUNCTION_BLOCK Blk
  VAR_TEMP t : INT := 5; END_VAR
  METHOD Get : INT
    Get := t;
  END_METHOD
END_FUNCTION_BLOCK

PROGRAM main
  VAR f : Blk; x : INT; END_VAR
  x := f.Get();
END_PROGRAM
";
    let options = CompilerOptions::from_dialect(Dialect::Codesys);

    let diagnostic = try_parse_and_compile(source, &options).unwrap_err();

    assert_eq!(diagnostic.code, "P4007");
}
