//! End-to-end tests for the declared default of a structure member: a variable
//! of a structure type starts with each member's declared initial value, wherever
//! the structure value lives (program variable, nested member, array element,
//! global, alias of the structure type).
//!
//! Result variables are declared first so their slots are 0, 1, ...; the
//! structure variables follow.

e2e_i32!(
    end_to_end_when_struct_member_has_elementary_default_then_member_reads_default,
    "
TYPE S : STRUCT
  a : INT := 5;
  b : INT := 7;
  c : INT;
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
    &[(0, 5), (1, 7), (2, 0)],
);

e2e_i32!(
    end_to_end_when_struct_member_has_subrange_default_then_member_reads_default,
    "
TYPE S : STRUCT
  a : INT(1..10) := 5;
  b : INT := 7;
END_STRUCT;
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    s : S;
  END_VAR
  r1 := s.a;
  r2 := s.b;
END_PROGRAM
",
    &[(0, 5), (1, 7)],
);

e2e_i32!(
    end_to_end_when_struct_subrange_member_has_no_default_then_member_reads_lower_bound,
    "
TYPE S : STRUCT
  a : INT(3..10);
END_STRUCT;
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    s : S;
  END_VAR
  r1 := s.a;
END_PROGRAM
",
    &[(0, 3)],
);

e2e_i32!(
    end_to_end_when_struct_member_has_enum_default_then_member_reads_default,
    "
TYPE Color : (Red, Green, Blue); END_TYPE
TYPE S : STRUCT
  c : Color := Blue;
  d : Color;
END_STRUCT;
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    s : S;
    vc : Color;
    vd : Color;
  END_VAR
  vc := s.c;
  vd := s.d;
  IF vc = Blue THEN r1 := 1; END_IF;
  IF vd = Red THEN r2 := 1; END_IF;
END_PROGRAM
",
    &[(0, 1), (1, 1)],
);

e2e_i32!(
    end_to_end_when_struct_member_has_string_default_then_member_reads_default,
    "
TYPE S : STRUCT
  name : STRING[8] := 'abc';
  n : INT := 4;
END_STRUCT;
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    s : S;
  END_VAR
  r1 := LEN(s.name);
  r2 := s.n;
END_PROGRAM
",
    &[(0, 3), (1, 4)],
);

e2e_i32!(
    end_to_end_when_struct_member_is_struct_with_defaults_then_inner_defaults_apply,
    "
TYPE Inner : STRUCT
  x : INT := 3;
END_STRUCT;
END_TYPE
TYPE Outer : STRUCT
  i : Inner;
  y : INT := 4;
END_STRUCT;
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    o : Outer;
  END_VAR
  r1 := o.i.x;
  r2 := o.y;
END_PROGRAM
",
    &[(0, 3), (1, 4)],
);

e2e_i32!(
    end_to_end_when_struct_member_struct_has_own_initializer_then_it_overrides_inner_default,
    "
TYPE Inner : STRUCT
  x : INT := 3;
  z : INT := 6;
END_STRUCT;
END_TYPE
TYPE Outer : STRUCT
  i : Inner := (x := 9);
  y : INT := 4;
END_STRUCT;
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    r3 : INT;
    o : Outer;
  END_VAR
  r1 := o.i.x;
  r2 := o.i.z;
  r3 := o.y;
END_PROGRAM
",
    &[(0, 9), (1, 6), (2, 4)],
);

e2e_i32!(
    end_to_end_when_array_of_struct_then_every_element_has_member_defaults,
    "
TYPE S : STRUCT
  a : INT := 5;
  b : INT := 7;
END_STRUCT;
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    r3 : INT;
    arr : ARRAY[1..3] OF S;
  END_VAR
  r1 := arr[1].a;
  r2 := arr[2].b;
  r3 := arr[3].a;
END_PROGRAM
",
    &[(0, 5), (1, 7), (2, 5)],
);

e2e_i32!(
    end_to_end_when_struct_variable_initializer_overrides_some_members_then_rest_keep_defaults,
    "
TYPE S : STRUCT
  a : INT(1..10) := 5;
  b : INT := 7;
END_STRUCT;
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    s : S := (a := 3);
  END_VAR
  r1 := s.a;
  r2 := s.b;
END_PROGRAM
",
    &[(0, 3), (1, 7)],
);

e2e_i32!(
    end_to_end_when_struct_variable_initializer_overrides_other_member_then_first_keeps_default,
    "
TYPE S : STRUCT
  a : INT(1..10) := 5;
  b : INT := 7;
END_STRUCT;
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    s : S := (b := 2);
  END_VAR
  r1 := s.a;
  r2 := s.b;
END_PROGRAM
",
    &[(0, 5), (1, 2)],
);

e2e_i32!(
    end_to_end_when_variable_type_is_alias_of_struct_then_member_defaults_apply,
    "
TYPE S : STRUCT
  a : INT(1..10) := 5;
  b : INT := 7;
END_STRUCT;
END_TYPE
TYPE T : S; END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    v : T;
  END_VAR
  r1 := v.a;
  r2 := v.b;
END_PROGRAM
",
    &[(0, 5), (1, 7)],
);

e2e_i32!(
    end_to_end_when_global_variable_is_struct_then_member_defaults_apply,
    "
TYPE S : STRUCT
  a : INT(1..10) := 5;
  b : INT := 7;
END_STRUCT;
END_TYPE

CONFIGURATION config
  VAR_GLOBAL
    g : S;
  END_VAR
  RESOURCE res ON PLC
    TASK t(INTERVAL := T#100ms, PRIORITY := 1);
    PROGRAM p WITH t : main;
  END_RESOURCE
END_CONFIGURATION

PROGRAM main
  VAR_EXTERNAL
    g : S;
  END_VAR
  VAR
    r1 : INT;
    r2 : INT;
  END_VAR
  r1 := g.a;
  r2 := g.b;
END_PROGRAM
",
    &[(1, 5), (2, 7)],
);

e2e_i32!(
    end_to_end_when_struct_member_type_is_alias_with_default_then_member_reads_alias_default,
    "
TYPE MyInt : INT := 5; END_TYPE
TYPE R : INT(1..10) := 6; END_TYPE
TYPE S : STRUCT
  c : MyInt;
  d : R;
  e : MyInt := 8;
END_STRUCT;
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    r3 : INT;
    s : S;
  END_VAR
  r1 := s.c;
  r2 := s.d;
  r3 := s.e;
END_PROGRAM
",
    &[(0, 5), (1, 6), (2, 8)],
);

e2e_i32!(
    end_to_end_when_variable_type_is_alias_with_default_then_variable_reads_alias_default,
    "
TYPE MyInt : INT := 5; END_TYPE
TYPE R : INT(1..10) := 6; END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    x : MyInt;
    y : R;
  END_VAR
  r1 := x;
  r2 := y;
END_PROGRAM
",
    &[(0, 5), (1, 6)],
);

// The values of an inline enumeration cannot be compared yet, so the
// default's own value is not observable here; the member beside it is.
e2e_i32!(
    end_to_end_when_struct_member_has_inline_enumeration_default_then_other_members_keep_theirs,
    "
TYPE S : STRUCT
  e : (X, Y, Z) := Z;
  n : INT := 4;
END_STRUCT;
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    s : S;
    v : (A, B, C) := C;
  END_VAR
  r1 := s.n;
END_PROGRAM
",
    &[(0, 4)],
);

e2e_i32!(
    end_to_end_when_array_of_struct_element_has_string_default_then_every_element_reads_it,
    "
TYPE Item : STRUCT
  name : STRING[8] := 'abc';
  n : INT := 4;
END_STRUCT;
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    r3 : INT;
    arr : ARRAY[1..3] OF Item;
  END_VAR
  r1 := LEN(arr[1].name);
  r2 := LEN(arr[3].name);
  r3 := arr[2].n;
END_PROGRAM
",
    &[(0, 3), (1, 3), (2, 4)],
);

e2e_i32!(
    end_to_end_when_struct_holds_array_of_struct_then_elements_have_member_defaults,
    "
TYPE Item : STRUCT
  a : INT := 5;
  b : INT := 7;
END_STRUCT;
END_TYPE
TYPE Holder : STRUCT
  items : ARRAY[1..3] OF Item;
  k : INT := 2;
END_STRUCT;
END_TYPE

PROGRAM main
  VAR
    r1 : INT;
    r2 : INT;
    r3 : INT;
    h : Holder;
  END_VAR
  r1 := h.items[1].a;
  r2 := h.items[3].b;
  r3 := h.k;
END_PROGRAM
",
    &[(0, 5), (1, 7), (2, 2)],
);

e2e_i32!(
    end_to_end_when_struct_member_is_array_with_default_then_elements_read_default,
    "
TYPE S : STRUCT
  a : ARRAY[1..3] OF INT := [10, 20, 30];
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
  r1 := s.a[1];
  r2 := s.a[3];
  r3 := s.n;
END_PROGRAM
",
    &[(0, 10), (1, 30), (2, 4)],
);
