//! Tests for transforming data type declarations (enumerations, arrays, structures).

use super::*;
use ironplc_test::cast;

#[test]
fn transform_when_enum_type_then_creates_enumeration_declaration() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes>
      <dataType name="TrafficLight">
        <baseType>
          <enum>
            <values>
              <value name="Red"/>
              <value name="Yellow"/>
              <value name="Green"/>
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
    let library =
        transform_project(&project, &test_file_id(), &CompilerOptions::default()).unwrap();

    assert_eq!(library.elements.len(), 1);
    let decl = cast!(
        &library.elements[0],
        LibraryElementKind::DataTypeDeclaration
    );
    let enum_decl = cast!(decl, DataTypeDeclarationKind::Enumeration);
    assert_eq!(enum_decl.type_name.to_string(), "TrafficLight");
}

#[test]
fn transform_when_array_type_then_creates_array_declaration() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes>
      <dataType name="IntArray">
        <baseType>
          <array>
            <dimension lower="0" upper="9"/>
            <baseType><INT/></baseType>
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

    assert_eq!(library.elements.len(), 1);
    let decl = cast!(
        &library.elements[0],
        LibraryElementKind::DataTypeDeclaration
    );
    let array_decl = cast!(decl, DataTypeDeclarationKind::Array);
    assert_eq!(array_decl.type_name.to_string(), "IntArray");
}

#[test]
fn transform_when_struct_type_then_creates_structure_declaration() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes>
      <dataType name="Point">
        <baseType>
          <struct>
            <variable name="X">
              <type><REAL/></type>
            </variable>
            <variable name="Y">
              <type><REAL/></type>
            </variable>
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

    assert_eq!(library.elements.len(), 1);
    let decl = cast!(
        &library.elements[0],
        LibraryElementKind::DataTypeDeclaration
    );
    let struct_decl = cast!(decl, DataTypeDeclarationKind::Structure);
    assert_eq!(struct_decl.type_name.to_string(), "Point");
    assert_eq!(struct_decl.elements.len(), 2);
}
