//! End-to-end tests for typed string literals: `UTF8#'…'`, `UCHAR#'…'` and
//! `__XSTRING#"…"`. The prefix names an encoding, so the literal denotes the
//! decoded characters of its quoted text and the width its delimiter spells.
//! See `specs/design/string-literals.md` (REQ-SL-parser-021, 022).

// s is at variable slot 0, n is at variable slot 1.
e2e_i32!(
    end_to_end_when_utf8_literal_then_length_counts_decoded_characters,
    "
PROGRAM main
  VAR
    s : STRING;
    n : INT;
  END_VAR
  s := UTF8#'a$41b';
  n := LEN(s);
END_PROGRAM
",
    &[(1, 3)],
);

e2e_i32!(
    end_to_end_when_uchar_literal_then_length_is_one,
    "
PROGRAM main
  VAR
    s : STRING;
    n : INT;
  END_VAR
  s := UCHAR#'A';
  n := LEN(s);
END_PROGRAM
",
    &[(1, 1)],
);

// w is at variable slot 0, n is at variable slot 1.
e2e_i32!(
    end_to_end_when_xstring_literal_then_wide_length_counts_characters,
    "
PROGRAM main
  VAR
    w : WSTRING;
    n : INT;
  END_VAR
  w := __XSTRING#\"a$0041b\";
  n := LEN(w);
END_PROGRAM
",
    &[(1, 3)],
);
