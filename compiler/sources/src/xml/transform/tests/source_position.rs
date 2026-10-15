//! Tests for source positions attached to transformed items.

use super::*;
use ironplc_test::cast;

// ========================================================================
// Source position tests
// ========================================================================

#[test]
fn transform_when_pou_name_then_has_source_position() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes/>
    <pous>
      <pou name="MyFunction" pouType="function">
        <interface>
          <returnType><INT/></returnType>
        </interface>
        <body>
          <ST>
            <xhtml xmlns="http://www.w3.org/1999/xhtml">MyFunction := 42;</xhtml>
          </ST>
        </body>
      </pou>
    </pous>
  </types>
</project>"#,
        minimal_project_header()
    );

    let project = parse_project(&xml);
    let file_id = test_file_id();
    let library = transform_project(&project, &file_id, &CompilerOptions::default()).unwrap();

    let func_decl = cast!(
        &library.elements[0],
        LibraryElementKind::FunctionDeclaration
    );

    // Verify the name has a non-zero span (indicating position was captured)
    let span = &func_decl.name.span;
    assert!(
        span.start < span.end,
        "Expected non-zero span for function name, got start={} end={}",
        span.start,
        span.end
    );

    // Verify the name value is correct
    assert_eq!(func_decl.name.original(), "MyFunction");
}

#[test]
fn transform_when_variable_name_then_has_source_position() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes/>
    <pous>
      <pou name="TestProg" pouType="program">
        <interface>
          <localVars>
            <variable name="MyVariable">
              <type><INT/></type>
            </variable>
          </localVars>
        </interface>
      </pou>
    </pous>
  </types>
</project>"#,
        minimal_project_header()
    );

    let project = parse_project(&xml);
    let file_id = test_file_id();
    let library = transform_project(&project, &file_id, &CompilerOptions::default()).unwrap();

    let prog_decl = cast!(&library.elements[0], LibraryElementKind::ProgramDeclaration);

    // Get the variable identifier
    let var = &prog_decl.variables[0];
    let id = cast!(&var.identifier, VariableIdentifier::Symbol);

    // Verify the variable name has a non-zero span
    let span = &id.span;
    assert!(
        span.start < span.end,
        "Expected non-zero span for variable name, got start={} end={}",
        span.start,
        span.end
    );

    // Verify the name value is correct
    assert_eq!(id.original(), "MyVariable");
}

#[test]
fn transform_when_var_list_persistent_then_qualifier_is_persistent() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes/>
    <pous>
      <pou name="TestProg" pouType="program">
        <interface>
          <localVars persistent="true">
            <variable name="nCounter">
              <type><DINT/></type>
            </variable>
          </localVars>
        </interface>
      </pou>
    </pous>
  </types>
</project>"#,
        minimal_project_header()
    );

    let project = parse_project(&xml);
    let file_id = test_file_id();
    let library = transform_project(&project, &file_id, &CompilerOptions::default()).unwrap();

    let prog_decl = cast!(&library.elements[0], LibraryElementKind::ProgramDeclaration);
    assert_eq!(
        prog_decl.variables[0].qualifier,
        DeclarationQualifier::Persistent
    );
}

#[test]
fn transform_when_data_type_name_then_has_source_position() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes>
      <dataType name="MyEnum">
        <baseType>
          <enum>
            <values>
              <value name="Value1"/>
            </values>
          </enum>
        </baseType>
      </dataType>
    </dataTypes>
    <pous/>
  </types>
</project>"#,
        minimal_project_header()
    );

    let project = parse_project(&xml);
    let file_id = test_file_id();
    let library = transform_project(&project, &file_id, &CompilerOptions::default()).unwrap();

    let decl = cast!(
        &library.elements[0],
        LibraryElementKind::DataTypeDeclaration
    );
    let enum_decl = cast!(decl, DataTypeDeclarationKind::Enumeration);

    // Verify the type name has a non-zero span
    let span = &enum_decl.type_name.name.span;
    assert!(
        span.start < span.end,
        "Expected non-zero span for type name, got start={} end={}",
        span.start,
        span.end
    );

    // Verify the name value is correct
    assert_eq!(enum_decl.type_name.to_string(), "MyEnum");

    // Also verify the enum value name has position
    let spec = cast!(&enum_decl.spec_init.spec, SpecificationKind::Inline);
    let value_span = &spec.values[0].value.span;
    assert!(
        value_span.start < value_span.end,
        "Expected non-zero span for enum value name, got start={} end={}",
        value_span.start,
        value_span.end
    );
}
