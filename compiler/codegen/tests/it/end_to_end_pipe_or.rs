//! End-to-end execution of the `|` spelling of logical OR.

e2e_i32!(
    end_to_end_when_pipe_or_both_false_then_zero,
    "
PROGRAM main
  VAR
    a : BOOL;
    b : BOOL;
    x : DINT;
  END_VAR
  a := FALSE;
  b := FALSE;
  x := a | b;
END_PROGRAM
",
    &[(0, 0), (1, 0), (2, 0)],
);

e2e_i32!(
    end_to_end_when_pipe_or_one_true_then_one,
    "
PROGRAM main
  VAR
    a : BOOL;
    b : BOOL;
    x : DINT;
  END_VAR
  a := TRUE;
  b := FALSE;
  x := a | b;
END_PROGRAM
",
    &[(0, 1), (1, 0), (2, 1)],
);

e2e_i32!(
    end_to_end_when_pipe_or_mixed_with_and_then_precedence_applies,
    "
PROGRAM main
  VAR
    a : BOOL;
    b : BOOL;
    c : BOOL;
    x : DINT;
  END_VAR
  a := FALSE;
  b := TRUE;
  c := FALSE;
  x := a | b & c;
END_PROGRAM
",
    &[(0, 0), (1, 1), (2, 0), (3, 0)],
);
