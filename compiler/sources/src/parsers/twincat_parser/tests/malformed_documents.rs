//! Tests that malformed or structurally incomplete TwinCAT XML documents are rejected with the right problem code.

use super::*;

#[test]
fn parse_when_malformed_xml_then_returns_p0006() {
    let xml = "NOT VALID XML <>";

    let result = parse(xml, &test_file_id(), &CompilerOptions::default());
    assert!(result.is_err());

    let diagnostic = result.unwrap_err();
    assert_eq!(diagnostic.code, "P0006");
}

#[test]
fn parse_when_wrong_root_element_then_returns_p0009() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<WrongRoot>
  <POU Name="MAIN">
    <Declaration><![CDATA[PROGRAM MAIN END_PROGRAM]]></Declaration>
  </POU>
</WrongRoot>"#;

    let result = parse(xml, &test_file_id(), &CompilerOptions::default());
    assert!(result.is_err());

    let diagnostic = result.unwrap_err();
    assert_eq!(diagnostic.code, "P0009");
    assert!(diagnostic.primary.message.contains("TcPlcObject"));
}

#[test]
fn parse_when_missing_object_element_then_returns_p0009() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &CompilerOptions::default());
    assert!(result.is_err());

    let diagnostic = result.unwrap_err();
    assert_eq!(diagnostic.code, "P0009");
    assert!(diagnostic.primary.message.contains("POU, GVL, DUT, or Itf"));
}

#[test]
fn parse_when_missing_declaration_then_returns_p0009() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="MAIN" Id="{00000000-0000-0000-0000-000000000000}">
    <Implementation>
      <ST><![CDATA[myVar := 1;]]></ST>
    </Implementation>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &CompilerOptions::default());
    assert!(result.is_err());

    let diagnostic = result.unwrap_err();
    assert_eq!(diagnostic.code, "P0009");
    assert!(diagnostic.primary.message.contains("Declaration"));
}
