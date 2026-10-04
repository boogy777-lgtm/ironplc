//! End-to-end tests for the initial values declared in types and taken by the
//! storage locations that name them: a structure alias with values, a
//! non-literal value in a type or a structure member, and the locals of every
//! kind of program organization unit.
//!
//! Result variables are declared first, after any global constant, so their
//! slots follow it. A string local of a method is not covered: a method does
//! not declare string locals yet.

use crate::common::parse_and_run;
use ironplc_parser::options::CompilerOptions;

fn with_constants() -> CompilerOptions {
    CompilerOptions {
        allow_top_level_var_global: true,
        allow_constant_initializer_expressions: true,
        ..CompilerOptions::default()
    }
}

// ---------------------------------------------------------------------------
// A structure alias keeps its base structure and the values it states.
// ---------------------------------------------------------------------------

e2e_i32!(
    end_to_end_when_variable_of_struct_alias_with_values_then_members_read_the_values,
    "
TYPE
  Pt : STRUCT x : INT; y : INT; END_STRUCT;
  P0 : Pt := (x := 1, y := 2);
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    v : P0;
  END_VAR
  r1 := v.x;
  r2 := v.y;
END_PROGRAM
",
    &[(0, 1), (1, 2)],
);

e2e_i32!(
    end_to_end_when_variable_of_struct_alias_states_a_member_then_the_alias_values_fill_the_rest,
    "
TYPE
  Pt : STRUCT x : INT; y : INT; END_STRUCT;
  P0 : Pt := (x := 1, y := 2);
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    v : P0 := (y := 7);
  END_VAR
  r1 := v.x;
  r2 := v.y;
END_PROGRAM
",
    &[(0, 1), (1, 7)],
);

e2e_i32!(
    end_to_end_when_struct_alias_values_override_member_defaults_then_alias_values_win,
    "
TYPE
  Pt : STRUCT x : INT := 10; y : INT := 20; z : INT := 30; END_STRUCT;
  P0 : Pt := (x := 1);
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    r3 : INT;
    v : P0;
  END_VAR
  r1 := v.x;
  r2 := v.y;
  r3 := v.z;
END_PROGRAM
",
    &[(0, 1), (1, 20), (2, 30)],
);

e2e_i32!(
    end_to_end_when_struct_alias_of_alias_states_members_then_each_alias_adds_its_values,
    "
TYPE
  Pt : STRUCT x : INT; y : INT; z : INT; END_STRUCT;
  P0 : Pt := (x := 1, y := 2);
  P1 : P0 := (y := 5, z := 6);
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    r3 : INT;
    v : P1;
  END_VAR
  r1 := v.x;
  r2 := v.y;
  r3 := v.z;
END_PROGRAM
",
    &[(0, 1), (1, 5), (2, 6)],
);

e2e_i32!(
    end_to_end_when_struct_member_is_struct_alias_with_values_then_member_reads_the_alias_values,
    "
TYPE
  Pt : STRUCT x : INT; y : INT; END_STRUCT;
  P0 : Pt := (x := 1, y := 2);
  Holder : STRUCT p : P0; k : INT := 3; END_STRUCT;
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    r3 : INT;
    h : Holder;
  END_VAR
  r1 := h.p.x;
  r2 := h.p.y;
  r3 := h.k;
END_PROGRAM
",
    &[(0, 1), (1, 2), (2, 3)],
);

e2e_i32!(
    end_to_end_when_two_instances_of_function_block_with_defaults_then_each_starts_at_the_defaults,
    "
FUNCTION_BLOCK fb
  VAR_OUTPUT o : INT; END_VAR
  VAR n : INT := 4; END_VAR
  n := n + 1;
  o := n;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    a : fb;
    b : fb;
  END_VAR
  a();
  a();
  b();
  r1 := a.o;
  r2 := b.o;
END_PROGRAM
",
    &[(0, 6), (1, 5)],
);

e2e_i32!(
    end_to_end_when_function_block_instance_states_a_field_then_it_overrides_the_default,
    "
FUNCTION_BLOCK fb
  VAR_OUTPUT o : INT; p : INT; END_VAR
  VAR n : INT := 4; m : INT := 10; END_VAR
  o := n;
  p := m;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    a : fb := (n := 9);
  END_VAR
  a();
  r1 := a.o;
  r2 := a.p;
END_PROGRAM
",
    &[(0, 9), (1, 10)],
);

// ---------------------------------------------------------------------------
// A value that is an expression of constants, in a type or a member.
// ---------------------------------------------------------------------------

e2e_i32_with!(
    end_to_end_when_type_value_names_a_constant_then_variable_starts_at_the_constant,
    with_constants(),
    "
VAR_GLOBAL CONSTANT
  N : INT := 5;
END_VAR
TYPE Level : INT := N; END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    l : Level;
  END_VAR
  r1 := l;
END_PROGRAM
",
    &[(1, 5)],
);

e2e_i32_with!(
    end_to_end_when_type_value_is_an_expression_then_variable_starts_at_its_value,
    with_constants(),
    "
VAR_GLOBAL CONSTANT
  N : INT := 5;
END_VAR
TYPE Level : INT := N * 2 + 1; END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    l : Level;
  END_VAR
  r1 := l;
END_PROGRAM
",
    &[(1, 11)],
);

e2e_i32_with!(
    end_to_end_when_struct_member_value_names_a_constant_then_member_reads_the_constant,
    with_constants(),
    "
VAR_GLOBAL CONSTANT
  N : INT := 5;
END_VAR
TYPE S : STRUCT
  a : INT := N;
  b : INT := 2 + 3;
  c : INT := N * 2;
END_STRUCT;
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    r3 : INT;
    s : S;
  END_VAR
  r1 := s.a;
  r2 := s.b;
  r3 := s.c;
END_PROGRAM
",
    &[(1, 5), (2, 5), (3, 10)],
);

// ---------------------------------------------------------------------------
// A string array member with a default.
// ---------------------------------------------------------------------------

e2e_i32!(
    end_to_end_when_struct_member_is_string_array_with_default_then_elements_read_default,
    "
TYPE S : STRUCT
  names : ARRAY[1..2] OF STRING[8] := ['ab', 'cde'];
  n : INT := 4;
END_STRUCT;
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    r3 : INT;
    s : S;
  END_VAR
  r1 := LEN(s.names[1]);
  r2 := LEN(s.names[2]);
  r3 := s.n;
END_PROGRAM
",
    &[(0, 2), (1, 3), (2, 4)],
);

// ---------------------------------------------------------------------------
// The locals of every kind of program organization unit start at what they
// declare: an elementary value, a subrange's lower bound, an enumeration's
// first value, the value a type gives, and a string value.
// ---------------------------------------------------------------------------

const LOCAL_TYPES: &str = "
TYPE
  Color : (Red, Green, Blue);
  Level : INT := 5;
  Range : INT(3..9);
  RangeD : INT(3..9) := 6;
END_TYPE
";

e2e_i32!(
    end_to_end_when_program_locals_declared_then_each_starts_at_its_declared_value,
    "
TYPE
  Color : (Red, Green, Blue);
  Level : INT := 5;
  Range : INT(3..9);
  RangeD : INT(3..9) := 6;
END_TYPE

PROGRAM main
  VAR
    e : INT := 4;
    sub : Range;
    sub2 : RangeD;
    sub3 : Range := 8;
    sub4 : RangeD := 7;
    col : Color;
    col2 : Color := Blue;
    al : Level;
    s : STRING[8] := 'abc';
    r : INT;
  END_VAR
  r := LEN(s);
END_PROGRAM
",
    &[
        (0, 4),
        (1, 3),
        (2, 6),
        (3, 8),
        (4, 7),
        (5, 0),
        (6, 2),
        (7, 5)
    ],
);

#[test]
fn end_to_end_when_function_locals_declared_then_each_starts_at_its_declared_value() {
    let source = format!(
        "{LOCAL_TYPES}
FUNCTION f : INT
  VAR_INPUT sel : INT; END_VAR
  VAR
    e : INT := 4;
    sub : Range;
    sub2 : RangeD;
    sub3 : Range := 8;
    sub4 : RangeD := 7;
    col : Color;
    col2 : Color := Blue;
    al : Level;
    s : STRING[8] := 'abc';
    bare : INT;
  END_VAR
  CASE sel OF
    0: f := e;
    1: f := sub;
    2: f := sub2;
    3: f := sub3;
    4: f := sub4;
    5: IF col = Red THEN f := 100; END_IF;
    6: IF col2 = Blue THEN f := 200; END_IF;
    7: f := al;
    8: f := LEN(s);
    9: f := bare;
  END_CASE;
  e := 99; sub := 9; sub2 := 9; al := 9; s := 'z'; bare := 9;
END_FUNCTION

PROGRAM main
  VAR
    r0 : INT; r1 : INT; r2 : INT; r3 : INT; r4 : INT;
    r5 : INT; r6 : INT; r7 : INT; r8 : INT; r9 : INT;
    again : INT;
  END_VAR
  r0 := f(0); r1 := f(1); r2 := f(2); r3 := f(3); r4 := f(4);
  r5 := f(5); r6 := f(6); r7 := f(7); r8 := f(8); r9 := f(9);
  again := f(1);
END_PROGRAM
"
    );
    let (_c, bufs) = parse_and_run(&source, &CompilerOptions::default());
    let got: Vec<i32> = (0..11).map(|i| bufs.vars[i].as_i32()).collect();
    assert_eq!(got, vec![4, 3, 6, 8, 7, 100, 200, 5, 3, 0, 3]);
}

#[test]
fn end_to_end_when_function_block_locals_declared_then_each_starts_at_its_declared_value() {
    let source = format!(
        "{LOCAL_TYPES}
FUNCTION_BLOCK fb
  VAR_OUTPUT
    o_e : INT; o_sub : INT; o_sub2 : INT; o_sub3 : INT; o_sub4 : INT;
    o_col : INT; o_col2 : INT; o_al : INT;
  END_VAR
  VAR
    e : INT := 4;
    sub : Range;
    sub2 : RangeD;
    sub3 : Range := 8;
    sub4 : RangeD := 7;
    col : Color;
    col2 : Color := Blue;
    al : Level;
  END_VAR
  o_e := e; o_sub := sub; o_sub2 := sub2; o_sub3 := sub3; o_sub4 := sub4;
  IF col = Red THEN o_col := 100; END_IF;
  IF col2 = Blue THEN o_col2 := 200; END_IF;
  o_al := al;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    r0 : INT; r1 : INT; r2 : INT; r3 : INT; r4 : INT;
    r5 : INT; r6 : INT; r7 : INT;
    inst : fb;
  END_VAR
  inst();
  r0 := inst.o_e; r1 := inst.o_sub; r2 := inst.o_sub2; r3 := inst.o_sub3;
  r4 := inst.o_sub4; r5 := inst.o_col; r6 := inst.o_col2; r7 := inst.o_al;
END_PROGRAM
"
    );
    let (_c, bufs) = parse_and_run(&source, &CompilerOptions::default());
    let got: Vec<i32> = (0..8).map(|i| bufs.vars[i].as_i32()).collect();
    assert_eq!(got, vec![4, 3, 6, 8, 7, 100, 200, 5]);
}

#[test]
fn end_to_end_when_method_locals_declared_then_each_starts_at_its_declared_value() {
    let source = format!(
        "{LOCAL_TYPES}
FUNCTION_BLOCK fb
  METHOD m : INT
    VAR_INPUT sel : INT; END_VAR
    VAR
      e : INT := 4;
      sub : Range;
      sub2 : RangeD;
      sub3 : Range := 8;
      sub4 : RangeD := 7;
      col : Color;
      col2 : Color := Blue;
      al : Level;
    END_VAR
    CASE sel OF
      0: m := e;
      1: m := sub;
      2: m := sub2;
      3: m := sub3;
      4: m := sub4;
      5: IF col = Red THEN m := 100; END_IF;
      6: IF col2 = Blue THEN m := 200; END_IF;
      7: m := al;
    END_CASE;
    e := 99; sub := 9; sub2 := 9; al := 9;
  END_METHOD
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    r0 : INT; r1 : INT; r2 : INT; r3 : INT; r4 : INT;
    r5 : INT; r6 : INT; r7 : INT; again : INT;
    inst : fb;
  END_VAR
  r0 := inst.m(0); r1 := inst.m(1); r2 := inst.m(2); r3 := inst.m(3);
  r4 := inst.m(4); r5 := inst.m(5); r6 := inst.m(6); r7 := inst.m(7);
  again := inst.m(1);
END_PROGRAM
"
    );
    let (_c, bufs) = parse_and_run(
        &source,
        &CompilerOptions {
            allow_fb_inheritance: true,
            ..CompilerOptions::default()
        },
    );
    let got: Vec<i32> = (0..9).map(|i| bufs.vars[i].as_i32()).collect();
    assert_eq!(got, vec![4, 3, 6, 8, 7, 100, 200, 5, 3]);
}
