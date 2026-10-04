//! End-to-end tests for a negated output assignment in a function block call
//! (`NOT out => target`): the target receives the complement of the output.

e2e_i32!(
    end_to_end_when_user_fb_output_negated_and_output_true_then_target_is_false,
    "
FUNCTION_BLOCK FLAG
  VAR_INPUT x : BOOL; END_VAR
  VAR_OUTPUT q : BOOL; END_VAR
  q := x;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    fb : FLAG;
    result : BOOL := TRUE;
  END_VAR
  fb(x := TRUE, NOT q => result);
END_PROGRAM
",
    &[(1, 0)],
);

e2e_i32!(
    end_to_end_when_user_fb_output_negated_and_output_false_then_target_is_true,
    "
FUNCTION_BLOCK FLAG
  VAR_INPUT x : BOOL; END_VAR
  VAR_OUTPUT q : BOOL; END_VAR
  q := x;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    fb : FLAG;
    result : BOOL;
  END_VAR
  fb(x := FALSE, NOT q => result);
END_PROGRAM
",
    &[(1, 1)],
);

e2e_i32!(
    end_to_end_when_user_fb_output_not_negated_and_output_true_then_target_is_true,
    "
FUNCTION_BLOCK FLAG
  VAR_INPUT x : BOOL; END_VAR
  VAR_OUTPUT q : BOOL; END_VAR
  q := x;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    fb : FLAG;
    result : BOOL;
  END_VAR
  fb(x := TRUE, q => result);
END_PROGRAM
",
    &[(1, 1)],
);

// A negated and a plain binding of the same output in one call each store
// their own value.
e2e_i32!(
    end_to_end_when_user_fb_output_bound_plain_and_negated_then_targets_differ,
    "
FUNCTION_BLOCK FLAG
  VAR_INPUT x : BOOL; END_VAR
  VAR_OUTPUT q : BOOL; END_VAR
  q := x;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    fb : FLAG;
    plain : BOOL;
    inverted : BOOL;
  END_VAR
  fb(x := TRUE, q => plain, NOT q => inverted);
END_PROGRAM
",
    &[(1, 1), (2, 0)],
);

// A standard function block goes through the same store.
e2e_i32!(
    end_to_end_when_standard_fb_output_negated_then_target_is_complement,
    "
PROGRAM main
  VAR
    latch : RS;
    result : BOOL;
  END_VAR
  latch(S := TRUE, R1 := FALSE, NOT Q1 => result);
END_PROGRAM
",
    &[(1, 0)],
);
