//! CASE statement round-tripping.

use super::common::*;

#[test]
fn write_to_string_when_case_branch_empty_then_round_trips() {
    let source = "
FUNCTION_BLOCK FB_Example
VAR
    x : INT;
    y : INT;
END_VAR
CASE x OF
    1: y := 1;
    5: (* no statement here *)
    10: y := 3;
END_CASE;
END_FUNCTION_BLOCK
";
    // The source's dropped `;` needs `--allow-missing-semicolon` to parse at
    // all, so the round trip runs under that flag.
    let options = CompilerOptions {
        allow_missing_semicolon: true,
        ..CompilerOptions::default()
    };
    let rendered = assert_round_trips(source, &options);

    // The renderer always writes an explicit `(* empty *) ;` for an empty
    // branch, so the rendering is strict-grammar-valid and needs no flag.
    parse_program(&rendered, &FileId::default(), &CompilerOptions::default())
        .expect("rendered empty CASE branch must parse without allow_missing_semicolon");
}

#[test]
fn write_to_string_when_case_label_is_hex_and_binary_literal_then_round_trips() {
    // Bit-string literals already render decimalized everywhere in this
    // codebase (confirmed: even an ordinary `x : DWORD := 16#D012;` VAR
    // initializer renders as `53266`, not the original hex spelling) --
    // pre-existing behavior, not something CASE-label support introduces
    // or is expected to fix. A real, likewise pre-existing consequence:
    // re-parsing the decimalized `53266:` label resolves to
    // SignedInteger, not BitStringLiteral again -- same numeric value,
    // different variant. So this asserts render *idempotency* (parse ->
    // render -> reparse -> render again, same text) rather than AST
    // equality, matching the pattern already established for REFERENCE
    // TO/POINTER TO's analogous "normalizes to a different spelling" case.
    let source = "
FUNCTION_BLOCK FB_Example
VAR
    x : DINT;
    y : INT;
END_VAR
CASE x OF
    16#D012: y := 1;
    2#1010: y := 2;
END_CASE;
END_FUNCTION_BLOCK
";
    let rendered = assert_round_trips_idempotently(source, &CompilerOptions::default());

    assert!(rendered.contains("\n      53266 :\n"));
    assert!(rendered.contains("\n      10 :\n"));
}

#[test]
fn write_to_string_when_case_group_has_two_values_then_one_colon_after_the_list() {
    let source = "
PROGRAM main
VAR
    s : INT;
    x : INT;
END_VAR
CASE s OF
    1, 2: x := 1;
    3..5: x := 2;
    ELSE x := 3;
END_CASE;
END_PROGRAM
";
    let rendered = assert_round_trips(source, &CompilerOptions::default());

    assert!(rendered.contains("\n   1 , 2 :\n"));
    assert_eq!(rendered.matches(" :\n").count(), 2);
}

#[test]
fn write_to_string_when_case_group_mixes_value_range_and_enumeration_value_then_round_trips() {
    let source = "
TYPE
E_State : (Idle, Run, Stop);
END_TYPE
PROGRAM main
VAR
    s : E_State;
    n : INT;
    x : INT;
END_VAR
CASE n OF
    1, 3..5, 9: x := 1;
END_CASE;
CASE s OF
    Idle, E_State#Run: x := 2;
    Stop: x := 3;
END_CASE;
END_PROGRAM
";
    let rendered = assert_round_trips(source, &CompilerOptions::default());

    assert!(rendered.contains("\n   1 , 3.. 5 , 9 :\n"));
    assert!(rendered.contains("\n   Idle , E_State#Run :\n"));
}
