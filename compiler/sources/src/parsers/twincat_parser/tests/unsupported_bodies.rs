//! Tests that graphical implementation bodies (FBD, LD, IL, SFC) are rejected as unsupported.

use super::*;

#[test]
fn parse_when_fbd_implementation_then_returns_p9003() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="MAIN" Id="{00000000-0000-0000-0000-000000000000}">
    <Declaration><![CDATA[PROGRAM MAIN
VAR
    myVar : INT;
END_VAR]]></Declaration>
    <Implementation>
      <FBD/>
    </Implementation>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &CompilerOptions::default());
    assert!(result.is_err());

    let diagnostic = result.unwrap_err();
    assert_eq!(diagnostic.code, Problem::XmlBodyTypeNotSupported.code());
    assert!(diagnostic.primary.message.contains("FBD"));
}

#[test]
fn parse_when_ld_implementation_then_returns_unsupported() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="MAIN" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[PROGRAM MAIN
VAR
    myVar : INT;
END_VAR]]></Declaration>
    <Implementation>
      <LD/>
    </Implementation>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &CompilerOptions::default());
    assert!(result.is_err());
    let diag = result.unwrap_err();
    assert_eq!(diag.code, Problem::XmlBodyTypeNotSupported.code());
    assert!(diag.primary.message.contains("LD"));
}

#[test]
fn parse_when_il_implementation_then_returns_unsupported() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="MAIN" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[PROGRAM MAIN
VAR
    myVar : INT;
END_VAR]]></Declaration>
    <Implementation>
      <IL/>
    </Implementation>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &CompilerOptions::default());
    assert!(result.is_err());
    let diag = result.unwrap_err();
    assert_eq!(diag.code, Problem::XmlBodyTypeNotSupported.code());
    assert!(diag.primary.message.contains("IL"));
}

#[test]
fn parse_when_sfc_implementation_then_returns_unsupported() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="MAIN" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[PROGRAM MAIN
VAR
    myVar : INT;
END_VAR]]></Declaration>
    <Implementation>
      <SFC/>
    </Implementation>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &CompilerOptions::default());
    assert!(result.is_err());
    let diag = result.unwrap_err();
    assert_eq!(diag.code, Problem::XmlBodyTypeNotSupported.code());
    assert!(diag.primary.message.contains("SFC"));
}
