//! Tests that comments and pragmas before the POU keyword neither break parsing nor misplace positions.

use super::*;

#[test]
fn parse_when_pou_declaration_starts_with_comment_then_succeeds() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="FB_Test" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[// header comment
(* block comment *)
FUNCTION_BLOCK FB_Test
VAR
    x : INT;
END_VAR]]></Declaration>
    <Implementation>
      <ST><![CDATA[x := 1;]]></ST>
    </Implementation>
  </POU>
</TcPlcObject>"#;

    let options = CompilerOptions::from_dialect(Dialect::TwinCat);
    let result = parse(xml, &test_file_id(), &options);
    assert!(result.is_ok(), "Expected Ok, got: {:?}", result.err());
    assert_eq!(result.unwrap().elements.len(), 1);
}

const LEADING_COMMENT: &str = "// header comment\n(* block comment *)\n";

/// A function block POU whose declaration starts with `prefix`.
fn fb_pou_xml(prefix: &str, declaration_vars: &str, body: &str) -> String {
    format!(
        r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="FB_Test" Id="{{00000000-0000-0000-0000-000000000000}}" SpecialFunc="None">
    <Declaration><![CDATA[{prefix}FUNCTION_BLOCK FB_Test
VAR
{declaration_vars}
END_VAR]]></Declaration>
    <Implementation>
      <ST><![CDATA[{body}]]></ST>
    </Implementation>
  </POU>
</TcPlcObject>"#
    )
}

/// Parses the same POU with and without a leading comment and returns the
/// start of the diagnostic each reports.
fn error_starts_with_and_without_comment(declaration_vars: &str, body: &str) -> (usize, usize) {
    let options = CompilerOptions::from_dialect(Dialect::TwinCat);
    let start = |prefix: &str| {
        let xml = fb_pou_xml(prefix, declaration_vars, body);
        parse(&xml, &test_file_id(), &options)
            .unwrap_err()
            .primary
            .location
            .start
    };
    (start(LEADING_COMMENT), start(""))
}

/// The leading comment only changes which closing keyword is appended; the
/// declaration text, comment included, is passed on unchanged, so every
/// position after it moves by exactly the comment's length.
#[test]
fn parse_when_leading_comment_and_body_syntax_error_then_position_shifts_by_comment_length() {
    let (with_comment, without) = error_starts_with_and_without_comment("    x : INT;", "x := ;");
    assert_eq!(with_comment, without + LEADING_COMMENT.len());
}

#[test]
fn parse_when_leading_comment_and_declaration_syntax_error_then_position_shifts_by_comment_length()
{
    let (with_comment, without) =
        error_starts_with_and_without_comment("    x : INT :=;", "x := 1;");
    assert_eq!(with_comment, without + LEADING_COMMENT.len());
}

#[test]
fn parse_when_leading_comment_then_function_block_name_span_points_into_xml() {
    let xml = fb_pou_xml(LEADING_COMMENT, "    x : INT;", "x := 1;");
    let options = CompilerOptions::from_dialect(Dialect::TwinCat);
    let fb = only_function_block(parse(&xml, &test_file_id(), &options).unwrap());

    let span = fb.name.span();
    assert_eq!(&xml[span.start..span.end], "FB_Test");
    assert!(span.start > xml.find("FUNCTION_BLOCK").unwrap());
}
