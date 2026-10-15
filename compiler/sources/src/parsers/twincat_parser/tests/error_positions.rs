//! Tests that diagnostics and spans from the embedded Structured Text point back into the XML document.

use super::*;

#[test]
fn parse_when_pou_then_positions_point_into_xml() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="MAIN" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[PROGRAM MAIN
VAR
    myVar : INT;
END_VAR]]></Declaration>
    <Implementation>
      <ST><![CDATA[myVar := myVar + 1;]]></ST>
    </Implementation>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &CompilerOptions::default()).unwrap();

    // Collect all spans to verify they point into the CDATA sections
    use ironplc_dsl::fold::Fold;
    let mut spans = Vec::new();
    struct SpanCollector<'a> {
        spans: &'a mut Vec<SourceSpan>,
    }
    impl Fold<()> for SpanCollector<'_> {
        fn fold_source_span(&mut self, node: SourceSpan) -> Result<SourceSpan, ()> {
            self.spans.push(node.clone());
            Ok(node)
        }
    }
    let mut collector = SpanCollector { spans: &mut spans };
    collector
        .fold_library(result)
        .expect("the collector cannot fail");

    // All spans should point to positions within the XML document that
    // fall inside CDATA sections
    let cdata_start = xml.find("<![CDATA[").unwrap() + "<![CDATA[".len();
    for span in &spans {
        assert!(
            span.start >= cdata_start,
            "Span start {} should be >= CDATA start {} (pointing into XML CDATA)",
            span.start,
            cdata_start
        );
    }
}

#[test]
fn parse_when_pou_syntax_error_then_position_points_into_xml() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="MAIN" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[PROGRAM MAIN
VAR
    myVar : INT;
END_VAR]]></Declaration>
    <Implementation>
      <ST><![CDATA[INVALID SYNTAX HERE !!!]]></ST>
    </Implementation>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &CompilerOptions::default());
    assert!(result.is_err());

    let diag = result.unwrap_err();
    // The error position should point into the Implementation CDATA
    let impl_cdata_start = xml.find("INVALID").unwrap();
    assert!(
        diag.primary.location.start >= impl_cdata_start,
        "Error position {} should be >= impl CDATA start {} (pointing into XML)",
        diag.primary.location.start,
        impl_cdata_start
    );
}

#[test]
fn parse_when_dut_with_invalid_syntax_then_returns_error_with_adjusted_position() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <DUT Name="ST_Bad" Id="{00000000-0000-0000-0000-000000000000}">
    <Declaration><![CDATA[TYPE ST_Bad :
INVALID SYNTAX HERE
END_TYPE]]></Declaration>
  </DUT>
</TcPlcObject>"#;

    let file_id = FileId::from_string("test.TcDUT");
    let result = parse(xml, &file_id, &CompilerOptions::default());
    assert!(result.is_err());

    let diag = result.unwrap_err();
    // Position should point into the CDATA section in the XML
    let cdata_start = xml.find("<![CDATA[").unwrap() + "<![CDATA[".len();
    assert!(
        diag.primary.location.start >= cdata_start,
        "Error position {} should be >= CDATA start {}",
        diag.primary.location.start,
        cdata_start
    );
}

#[test]
fn parse_when_pou_syntax_error_in_declaration_then_position_in_declaration() {
    // Syntax error in the declaration section (not implementation)
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="MAIN" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[PROGRAM MAIN
VAR
    BAD SYNTAX : INT;
END_VAR]]></Declaration>
    <Implementation>
      <ST><![CDATA[x := 1;]]></ST>
    </Implementation>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &CompilerOptions::default());
    assert!(result.is_err());

    let diag = result.unwrap_err();
    // Should point into the first CDATA (declaration)
    let decl_cdata_start = xml.find("<![CDATA[").unwrap() + "<![CDATA[".len();
    assert!(
        diag.primary.location.start >= decl_cdata_start,
        "Error position {} should be >= declaration CDATA start {}",
        diag.primary.location.start,
        decl_cdata_start
    );
}
