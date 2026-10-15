//! Tests for transforming edge cases such as empty and nested projects.

use super::*;

#[test]
fn transform_when_empty_project_then_returns_empty_library() {
    let xml = format!(
        "{}\n  <types>\n    <dataTypes/>\n    <pous/>\n  </types>\n</project>",
        minimal_project_header()
    );
    let project = parse_project(&xml);
    let library =
        transform_project(&project, &test_file_id(), &CompilerOptions::default()).unwrap();

    assert_eq!(library.elements.len(), 0);
}

// ========================================================================
// Edge case tests
// ========================================================================

#[test]
fn transform_when_project_with_only_types_then_succeeds() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes>
      <dataType name="MyInt">
        <baseType><INT/></baseType>
      </dataType>
      <dataType name="MyStruct">
        <baseType>
          <struct>
            <variable name="field1"><type><INT/></type></variable>
            <variable name="field2"><type><BOOL/></type></variable>
          </struct>
        </baseType>
      </dataType>
    </dataTypes>
    <pous/>
  </types>
</project>"#,
        minimal_project_header()
    );

    let project = parse_project(&xml);
    let library =
        transform_project(&project, &test_file_id(), &CompilerOptions::default()).unwrap();

    // Should have 2 type declarations
    assert_eq!(library.elements.len(), 2);
}

#[test]
fn transform_when_project_with_only_pous_then_succeeds() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes/>
    <pous>
      <pou name="Prog1" pouType="program">
        <interface/>
        <body>
          <ST>
            <xhtml xmlns="http://www.w3.org/1999/xhtml">;</xhtml>
          </ST>
        </body>
      </pou>
      <pou name="FB1" pouType="functionBlock">
        <interface/>
        <body>
          <ST>
            <xhtml xmlns="http://www.w3.org/1999/xhtml">;</xhtml>
          </ST>
        </body>
      </pou>
    </pous>
  </types>
</project>"#,
        minimal_project_header()
    );

    let project = parse_project(&xml);
    let library =
        transform_project(&project, &test_file_id(), &CompilerOptions::default()).unwrap();

    // Should have 2 POU declarations
    assert_eq!(library.elements.len(), 2);
}

#[test]
fn transform_when_deeply_nested_structure_then_succeeds() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes>
      <dataType name="Inner">
        <baseType>
          <struct>
            <variable name="value"><type><INT/></type></variable>
          </struct>
        </baseType>
      </dataType>
      <dataType name="Middle">
        <baseType>
          <struct>
            <variable name="inner"><type><derived name="Inner"/></type></variable>
            <variable name="count"><type><INT/></type></variable>
          </struct>
        </baseType>
      </dataType>
      <dataType name="Outer">
        <baseType>
          <struct>
            <variable name="middle"><type><derived name="Middle"/></type></variable>
            <variable name="flag"><type><BOOL/></type></variable>
          </struct>
        </baseType>
      </dataType>
    </dataTypes>
    <pous>
      <pou name="TestProg" pouType="program">
        <interface>
          <localVars>
            <variable name="data"><type><derived name="Outer"/></type></variable>
          </localVars>
        </interface>
        <body>
          <ST>
            <xhtml xmlns="http://www.w3.org/1999/xhtml">;</xhtml>
          </ST>
        </body>
      </pou>
    </pous>
  </types>
</project>"#,
        minimal_project_header()
    );

    let project = parse_project(&xml);
    let library =
        transform_project(&project, &test_file_id(), &CompilerOptions::default()).unwrap();

    // Should have 3 type declarations + 1 program
    assert_eq!(library.elements.len(), 4);
}

#[test]
fn transform_when_array_of_struct_then_succeeds() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes>
      <dataType name="Point">
        <baseType>
          <struct>
            <variable name="x"><type><INT/></type></variable>
            <variable name="y"><type><INT/></type></variable>
          </struct>
        </baseType>
      </dataType>
      <dataType name="PointArray">
        <baseType>
          <array>
            <dimension lower="0" upper="9"/>
            <baseType><derived name="Point"/></baseType>
          </array>
        </baseType>
      </dataType>
    </dataTypes>
    <pous/>
  </types>
</project>"#,
        minimal_project_header()
    );

    let project = parse_project(&xml);
    let library =
        transform_project(&project, &test_file_id(), &CompilerOptions::default()).unwrap();

    // Should have 2 type declarations
    assert_eq!(library.elements.len(), 2);
}
