//! Tests that each supported TwinCAT element kind (POU, GVL, DUT, interface) parses into the expected library element.

use super::*;

#[test]
fn parse_when_pou_with_declaration_and_st_then_succeeds() {
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

    let result = parse(xml, &test_file_id(), &CompilerOptions::default());
    assert!(result.is_ok(), "Expected Ok, got: {:?}", result.err());
    let library = result.unwrap();
    assert_eq!(library.elements.len(), 1);
}

#[test]
fn parse_when_function_block_pou_then_succeeds() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="FB_Counter" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[FUNCTION_BLOCK FB_Counter
VAR
    count : INT := 0;
END_VAR]]></Declaration>
    <Implementation>
      <ST><![CDATA[count := count + 1;]]></ST>
    </Implementation>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &CompilerOptions::default());
    assert!(result.is_ok(), "Expected Ok, got: {:?}", result.err());
    let library = result.unwrap();
    assert_eq!(library.elements.len(), 1);
}

#[test]
fn parse_when_gvl_then_succeeds() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <GVL Name="GVL_Main" Id="{00000000-0000-0000-0000-000000000000}">
    <Declaration><![CDATA[VAR_GLOBAL
    gCounter : INT := 0;
    gRunning : BOOL := FALSE;
END_VAR]]></Declaration>
  </GVL>
</TcPlcObject>"#;

    let file_id = FileId::from_string("test.TcGVL");
    let result = parse(xml, &file_id, &CompilerOptions::default());
    assert!(result.is_ok(), "Expected Ok, got: {:?}", result.err());
}

#[test]
fn parse_when_dut_then_succeeds() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <DUT Name="ST_MyStruct" Id="{00000000-0000-0000-0000-000000000000}">
    <Declaration><![CDATA[TYPE ST_MyStruct :
STRUCT
    value : INT;
    name : STRING;
END_STRUCT;
END_TYPE]]></Declaration>
  </DUT>
</TcPlcObject>"#;

    let file_id = FileId::from_string("test.TcDUT");
    let result = parse(xml, &file_id, &CompilerOptions::default());
    assert!(result.is_ok(), "Expected Ok, got: {:?}", result.err());
}

#[test]
fn parse_when_pou_with_no_implementation_then_succeeds() {
    // POU with declaration only, no Implementation element
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="MAIN" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[PROGRAM MAIN
VAR
    myVar : INT;
END_VAR]]></Declaration>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &CompilerOptions::default());
    assert!(result.is_ok(), "Expected Ok, got: {:?}", result.err());
    let library = result.unwrap();
    assert_eq!(library.elements.len(), 1);
}

#[test]
fn parse_when_itf_bare_interface_then_succeeds() {
    // Modeled on a real .TcIO file's structure (Beckhoff TwinCAT
    // interface, no base) — see
    // specs/design/beckhoff-twincat-dialect.md §1.3.
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <Itf Name="I_Drivable" Id="{00000000-0000-0000-0000-000000000000}">
    <Declaration><![CDATA[INTERFACE I_Drivable
]]></Declaration>
  </Itf>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &opts_with_fb_inheritance());
    assert!(result.is_ok(), "Expected Ok, got: {:?}", result.err());
    let library = result.unwrap();
    assert_eq!(library.elements.len(), 1);
    assert!(matches!(
        library.elements[0],
        LibraryElementKind::InterfaceDeclaration(_)
    ));
}

#[test]
fn parse_when_itf_extends_base_interface_then_succeeds() {
    // Modeled on a real .TcIO file's structure (e.g. `I_Focus.TcIO`
    // extending `I_BaseAxis`).
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <Itf Name="I_Focus" Id="{00000000-0000-0000-0000-000000000000}">
    <Declaration><![CDATA[INTERFACE I_Focus EXTENDS I_BaseAxis]]></Declaration>
  </Itf>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &opts_with_fb_inheritance());
    assert!(result.is_ok(), "Expected Ok, got: {:?}", result.err());
    let library = result.unwrap();
    let interface = cast!(
        &library.elements[0],
        LibraryElementKind::InterfaceDeclaration
    );
    assert_eq!(interface.extends, vec![TypeName::from("I_BaseAxis")]);
}

#[test]
fn parse_when_itf_and_default_dialect_then_err() {
    // Without allow_fb_inheritance, INTERFACE is just an identifier.
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <Itf Name="I_Drivable" Id="{00000000-0000-0000-0000-000000000000}">
    <Declaration><![CDATA[INTERFACE I_Drivable
]]></Declaration>
  </Itf>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &CompilerOptions::default());
    assert!(result.is_err());
}

#[test]
fn parse_when_pou_with_empty_implementation_then_succeeds() {
    // Implementation exists but has no ST or other language child
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="MAIN" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[PROGRAM MAIN
VAR
    myVar : INT;
END_VAR]]></Declaration>
    <Implementation>
    </Implementation>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &CompilerOptions::default());
    assert!(result.is_ok(), "Expected Ok, got: {:?}", result.err());
}

#[test]
fn parse_when_pou_with_function_declaration_then_succeeds() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="FC_Add" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[FUNCTION FC_Add : INT
VAR_INPUT
    a : INT;
    b : INT;
END_VAR]]></Declaration>
    <Implementation>
      <ST><![CDATA[FC_Add := a + b;]]></ST>
    </Implementation>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &CompilerOptions::default());
    assert!(result.is_ok(), "Expected Ok, got: {:?}", result.err());
    let library = result.unwrap();
    assert_eq!(library.elements.len(), 1);
}
