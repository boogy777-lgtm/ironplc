//! Tests that `<Method>` elements of a function block are parsed, kept in order and positioned in the XML.

use super::*;

/// The `<Method>` shape TwinCAT writes into a `.TcPOU`: the method's own
/// `<Declaration>` already carries the `METHOD` keyword and the signature,
/// and the body lives in a sibling `<Implementation><ST>`.
const FB_WITH_METHOD: &str = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="FB_Motor" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[FUNCTION_BLOCK FB_Motor
VAR
    speed : REAL;
END_VAR]]></Declaration>
    <Implementation>
      <ST><![CDATA[speed := 0.0;]]></ST>
    </Implementation>
    <Method Name="SetSpeed" Id="{00000000-0000-0000-0000-000000000001}">
      <Declaration><![CDATA[METHOD SetSpeed
VAR_INPUT
    value : REAL;
END_VAR]]></Declaration>
      <Implementation>
        <ST><![CDATA[speed := value;]]></ST>
      </Implementation>
    </Method>
  </POU>
</TcPlcObject>"#;

/// Collect every source span in a library.
fn collect_spans(library: Library) -> Vec<SourceSpan> {
    struct SpanCollector<'a> {
        spans: &'a mut Vec<SourceSpan>,
    }
    impl Fold<()> for SpanCollector<'_> {
        fn fold_source_span(&mut self, node: SourceSpan) -> Result<SourceSpan, ()> {
            self.spans.push(node.clone());
            Ok(node)
        }
    }

    let mut spans = Vec::new();
    let mut collector = SpanCollector { spans: &mut spans };
    collector
        .fold_library(library)
        .expect("the collector cannot fail");
    spans
}

#[test]
fn parse_when_pou_with_method_element_then_method_is_declared() {
    let result = parse(FB_WITH_METHOD, &test_file_id(), &opts_with_fb_inheritance());
    assert!(result.is_ok(), "Expected Ok, got: {:?}", result.err());

    let function_block = only_function_block(result.unwrap());
    assert_eq!(function_block.methods.len(), 1);
    assert_eq!(function_block.methods[0].name, Id::from("SetSpeed"));
    // VAR_INPUT of the method, not of the enclosing function block.
    assert_eq!(function_block.methods[0].variables.len(), 1);
    assert_eq!(function_block.variables.len(), 1);
}

#[test]
fn parse_when_pou_with_multiple_method_elements_then_all_kept_in_document_order() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="FB_Motor" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[FUNCTION_BLOCK FB_Motor
VAR
    speed : REAL;
END_VAR]]></Declaration>
    <Implementation>
      <ST><![CDATA[speed := 0.0;]]></ST>
    </Implementation>
    <Method Name="Start" Id="{00000000-0000-0000-0000-000000000001}">
      <Declaration><![CDATA[METHOD Start]]></Declaration>
      <Implementation>
        <ST><![CDATA[speed := 1.0;]]></ST>
      </Implementation>
    </Method>
    <Method Name="Stop" Id="{00000000-0000-0000-0000-000000000002}">
      <Declaration><![CDATA[METHOD Stop]]></Declaration>
      <Implementation>
        <ST><![CDATA[speed := 0.0;]]></ST>
      </Implementation>
    </Method>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &opts_with_fb_inheritance());
    assert!(result.is_ok(), "Expected Ok, got: {:?}", result.err());

    let function_block = only_function_block(result.unwrap());
    let names: Vec<&Id> = function_block.methods.iter().map(|m| &m.name).collect();
    assert_eq!(names, vec![&Id::from("Start"), &Id::from("Stop")]);
}

#[test]
fn parse_when_method_declares_return_type_then_return_type_is_kept() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="FB_Motor" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[FUNCTION_BLOCK FB_Motor
VAR
    speed : REAL;
END_VAR]]></Declaration>
    <Implementation>
      <ST><![CDATA[speed := 0.0;]]></ST>
    </Implementation>
    <Method Name="IsRunning" Id="{00000000-0000-0000-0000-000000000001}">
      <Declaration><![CDATA[METHOD IsRunning : BOOL]]></Declaration>
      <Implementation>
        <ST><![CDATA[IsRunning := speed > 0.0;]]></ST>
      </Implementation>
    </Method>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &opts_with_fb_inheritance());
    assert!(result.is_ok(), "Expected Ok, got: {:?}", result.err());

    let function_block = only_function_block(result.unwrap());
    assert_eq!(function_block.methods.len(), 1);
    assert!(function_block.methods[0].return_type.is_some());
}

#[test]
fn parse_when_method_declaration_has_access_specifier_then_qualifier_is_kept() {
    // The most common qualifier in the TwinCAT corpus (#1199). The
    // implementation starts with an assignment, so the qualifier must not
    // swallow the method name.
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="FB_Motor" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[FUNCTION_BLOCK FB_Motor
VAR
    speed : REAL;
END_VAR]]></Declaration>
    <Implementation>
      <ST><![CDATA[speed := 0.0;]]></ST>
    </Implementation>
    <Method Name="Reset" Id="{00000000-0000-0000-0000-000000000001}">
      <Declaration><![CDATA[METHOD PRIVATE Reset]]></Declaration>
      <Implementation>
        <ST><![CDATA[speed := 0.0;]]></ST>
      </Implementation>
    </Method>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &opts_with_fb_inheritance());
    assert!(result.is_ok(), "Expected Ok, got: {:?}", result.err());

    let function_block = only_function_block(result.unwrap());
    let method = &function_block.methods[0];
    assert_eq!(method.name, Id::from("Reset"));
    assert_eq!(method.qualifiers.access(), Some(AccessSpecifier::Private));
    assert_eq!(method.body.len(), 1);
}

#[test]
fn parse_when_method_has_no_implementation_then_method_has_empty_body() {
    // TwinCAT writes a method with an empty body as a `<Method>` with no
    // `<Implementation>` child at all.
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="FB_Motor" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[FUNCTION_BLOCK FB_Motor
VAR
    speed : REAL;
END_VAR]]></Declaration>
    <Implementation>
      <ST><![CDATA[speed := 0.0;]]></ST>
    </Implementation>
    <Method Name="Reset" Id="{00000000-0000-0000-0000-000000000001}">
      <Declaration><![CDATA[METHOD Reset]]></Declaration>
    </Method>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &opts_with_fb_inheritance());
    assert!(result.is_ok(), "Expected Ok, got: {:?}", result.err());

    let function_block = only_function_block(result.unwrap());
    assert_eq!(function_block.methods.len(), 1);
    assert!(function_block.methods[0].body.is_empty());
}

#[test]
fn parse_when_pou_with_method_then_method_spans_point_into_method_cdata() {
    let result = parse(FB_WITH_METHOD, &test_file_id(), &opts_with_fb_inheritance());
    // Restrict to the method's own spans by measuring against the XML
    // region the `<Method>` element occupies.
    let method_element_start = FB_WITH_METHOD.find("<Method ").unwrap();
    let method_spans: Vec<SourceSpan> = collect_spans(result.unwrap())
        .into_iter()
        .filter(|span| span.start >= method_element_start)
        .collect();

    assert!(
        !method_spans.is_empty(),
        "expected spans inside the <Method> element"
    );

    // `value` is declared only inside the method, so its span must fall
    // inside the method's declaration CDATA rather than the POU's.
    let value_offset = FB_WITH_METHOD.find("value : REAL").unwrap();
    assert!(
        method_spans.iter().any(|span| span.start == value_offset),
        "expected a span at the method's `value` declaration ({value_offset})"
    );

    // The method body assignment must map into the method's own ST CDATA.
    let body_offset = FB_WITH_METHOD.find("speed := value;").unwrap();
    assert!(
        method_spans.iter().any(|span| span.start == body_offset),
        "expected a span at the method's body ({body_offset})"
    );
}

#[test]
fn parse_when_method_body_has_syntax_error_then_position_points_into_method_cdata() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="FB_Motor" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[FUNCTION_BLOCK FB_Motor
VAR
    speed : REAL;
END_VAR]]></Declaration>
    <Implementation>
      <ST><![CDATA[speed := 0.0;]]></ST>
    </Implementation>
    <Method Name="SetSpeed" Id="{00000000-0000-0000-0000-000000000001}">
      <Declaration><![CDATA[METHOD SetSpeed]]></Declaration>
      <Implementation>
        <ST><![CDATA[INVALID SYNTAX HERE !!!]]></ST>
      </Implementation>
    </Method>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &opts_with_fb_inheritance());
    assert!(result.is_err());

    let diagnostic = result.unwrap_err();
    let method_body_start = xml.find("INVALID").unwrap();
    assert!(
        diagnostic.primary.location.start >= method_body_start,
        "Error position {} should be >= method body start {} (pointing into the method's CDATA)",
        diagnostic.primary.location.start,
        method_body_start
    );
}

#[test]
fn parse_when_method_uses_fbd_implementation_then_returns_p9003() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="FB_Motor" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[FUNCTION_BLOCK FB_Motor
VAR
    speed : REAL;
END_VAR]]></Declaration>
    <Implementation>
      <ST><![CDATA[speed := 0.0;]]></ST>
    </Implementation>
    <Method Name="SetSpeed" Id="{00000000-0000-0000-0000-000000000001}">
      <Declaration><![CDATA[METHOD SetSpeed]]></Declaration>
      <Implementation>
        <FBD/>
      </Implementation>
    </Method>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &opts_with_fb_inheritance());
    assert!(result.is_err());

    let diagnostic = result.unwrap_err();
    assert_eq!(diagnostic.code, Problem::XmlBodyTypeNotSupported.code());
    assert!(diagnostic.primary.message.contains("FBD"));
}

#[test]
fn parse_when_method_missing_declaration_then_returns_p0009() {
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="FB_Motor" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[FUNCTION_BLOCK FB_Motor
VAR
    speed : REAL;
END_VAR]]></Declaration>
    <Implementation>
      <ST><![CDATA[speed := 0.0;]]></ST>
    </Implementation>
    <Method Name="SetSpeed" Id="{00000000-0000-0000-0000-000000000001}">
      <Implementation>
        <ST><![CDATA[speed := 1.0;]]></ST>
      </Implementation>
    </Method>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &opts_with_fb_inheritance());
    assert!(result.is_err());

    let diagnostic = result.unwrap_err();
    assert_eq!(diagnostic.code, "P0009");
    assert!(diagnostic.primary.message.contains("SetSpeed"));
}

#[test]
fn parse_when_pou_with_method_and_default_dialect_then_err() {
    // Without allow_fb_inheritance, METHOD is demoted to an identifier.
    // The methods are no longer silently dropped, so the file reports the
    // unsupported syntax instead of parsing as a method-less type.
    let result = parse(FB_WITH_METHOD, &test_file_id(), &CompilerOptions::default());
    assert!(result.is_err());
}

#[test]
fn parse_when_program_pou_with_method_element_then_method_is_dropped() {
    // `method_declaration` is reachable only from
    // `function_block_declaration`, so a method on a PROGRAM has nowhere
    // to go in the grammar and is left alone rather than turned into a
    // parse error.
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="MAIN" Id="{00000000-0000-0000-0000-000000000000}" SpecialFunc="None">
    <Declaration><![CDATA[PROGRAM MAIN
VAR
    speed : REAL;
END_VAR]]></Declaration>
    <Implementation>
      <ST><![CDATA[speed := 0.0;]]></ST>
    </Implementation>
    <Method Name="SetSpeed" Id="{00000000-0000-0000-0000-000000000001}">
      <Declaration><![CDATA[METHOD SetSpeed]]></Declaration>
      <Implementation>
        <ST><![CDATA[speed := 1.0;]]></ST>
      </Implementation>
    </Method>
  </POU>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &opts_with_fb_inheritance());
    assert!(result.is_ok(), "Expected Ok, got: {:?}", result.err());
}

#[test]
fn parse_when_itf_with_method_element_then_method_is_dropped() {
    // `interface_declaration` parses the header only, so an interface's
    // `<Method>` children are still ignored. Appending them would turn
    // every real `.TcIO` file into a parse error.
    let xml = r#"<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <Itf Name="I_Drivable" Id="{00000000-0000-0000-0000-000000000000}">
    <Declaration><![CDATA[INTERFACE I_Drivable
]]></Declaration>
    <Method Name="Start" Id="{00000000-0000-0000-0000-000000000001}">
      <Declaration><![CDATA[METHOD Start : BOOL]]></Declaration>
    </Method>
  </Itf>
</TcPlcObject>"#;

    let result = parse(xml, &test_file_id(), &opts_with_fb_inheritance());
    assert!(result.is_ok(), "Expected Ok, got: {:?}", result.err());
    let library = result.unwrap();
    assert!(matches!(
        library.elements[0],
        LibraryElementKind::InterfaceDeclaration(_)
    ));
}
