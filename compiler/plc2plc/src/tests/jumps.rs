//! CODESYS jump statements: `JMP` with `label:` statement labels, `CALC` and
//! `__WAIT`.
//!
//! See `specs/design/codesys-jump-and-pragmas.md`.

use super::common::*;
use spec_test_macro::spec_test;

/// REQ-JMP-plc2plc-001: `Jump`, `LabelStatement`, `ConditionalCall` and
/// `Wait` render as `JMP`/`label:`/`CALC`/`__WAIT` and the rendering parses
/// back to an equal AST under the same options. The source covers both jump
/// spellings, a label, `CALC`, and `__WAIT` with and without a condition.
#[spec_test(REQ_JMP_plc2plc_001)]
fn write_to_string_when_jump_statements_then_round_trips() {
    let source = "
PROGRAM main
VAR
    x : INT;
    b : BOOL;
    fb : Fb;
END_VAR
JMP finished;
retry:
JMP (b) finished;
CALC(b, fb(a := 1));
__WAIT;
__WAIT(b);
finished:
x := 1;
END_PROGRAM

FUNCTION_BLOCK Fb
VAR_INPUT
    a : INT;
END_VAR
END_FUNCTION_BLOCK
";
    let options = CompilerOptions::from_dialect(Dialect::Codesys);

    // The round trip proves the keywords: a rendering that dropped the
    // spelled-out forms would not re-parse as the same statements.
    assert_round_trips(source, &options);
}
