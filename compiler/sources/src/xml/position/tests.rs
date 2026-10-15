//! Tests for the PLCopen XML parser.

use super::*;
use ironplc_test::cast;

fn test_file_id() -> FileId {
    FileId::from_string("test.xml")
}

#[test]
fn parse_when_minimal_project_then_succeeds() {
    let xml = r#"<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://www.plcopen.org/xml/tc6_0201">
  <fileHeader companyName="Test" productName="Test" productVersion="1.0" creationDateTime="2024-01-01T00:00:00"/>
  <contentHeader name="TestProject">
    <coordinateInfo>
      <fbd><scaling x="1" y="1"/></fbd>
      <ld><scaling x="1" y="1"/></ld>
      <sfc><scaling x="1" y="1"/></sfc>
    </coordinateInfo>
  </contentHeader>
  <types>
    <dataTypes/>
    <pous/>
  </types>
</project>"#;

    let project = parse_plcopen_xml(xml, &test_file_id()).unwrap();

    assert_eq!(project.file_header.company_name, "Test");
    assert_eq!(project.content_header.name, "TestProject");
    assert!(project.types.pous.pou.is_empty());
}

#[test]
fn parse_when_function_block_with_variables_then_extracts_correctly() {
    let xml = r#"<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://www.plcopen.org/xml/tc6_0201">
  <fileHeader companyName="Test" productName="Test" productVersion="1.0" creationDateTime="2024-01-01T00:00:00"/>
  <contentHeader name="TestProject">
    <coordinateInfo>
      <fbd><scaling x="1" y="1"/></fbd>
      <ld><scaling x="1" y="1"/></ld>
      <sfc><scaling x="1" y="1"/></sfc>
    </coordinateInfo>
  </contentHeader>
  <types>
    <dataTypes/>
    <pous>
      <pou name="Counter" pouType="functionBlock">
        <interface>
          <inputVars>
            <variable name="Reset">
              <type><BOOL/></type>
            </variable>
          </inputVars>
          <outputVars>
            <variable name="Count">
              <type><INT/></type>
            </variable>
          </outputVars>
        </interface>
        <body>
          <ST>
            <xhtml xmlns="http://www.w3.org/1999/xhtml">
IF Reset THEN
  Count := 0;
END_IF;
            </xhtml>
          </ST>
        </body>
      </pou>
    </pous>
  </types>
</project>"#;

    let project = parse_plcopen_xml(xml, &test_file_id()).unwrap();

    assert_eq!(project.types.pous.pou.len(), 1);
    let pou = &project.types.pous.pou[0];
    assert_eq!(pou.name, "Counter");
    assert_eq!(pou.pou_type, PouType::FunctionBlock);

    let interface = pou.interface.as_ref().unwrap();
    assert_eq!(interface.input_vars.len(), 1);
    assert_eq!(interface.input_vars[0].variable.len(), 1);
    assert_eq!(interface.input_vars[0].variable[0].name, "Reset");
    assert!(matches!(
        interface.input_vars[0].variable[0].var_type,
        DataType::Bool
    ));

    let body = pou.body.as_ref().unwrap();
    assert!(body.is_st());
    assert!(body.st_text().unwrap().contains("IF Reset THEN"));
}

#[test]
fn parse_when_enumeration_type_then_extracts_values() {
    let xml = r#"<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://www.plcopen.org/xml/tc6_0201">
  <fileHeader companyName="Test" productName="Test" productVersion="1.0" creationDateTime="2024-01-01T00:00:00"/>
  <contentHeader name="TestProject">
    <coordinateInfo>
      <fbd><scaling x="1" y="1"/></fbd>
      <ld><scaling x="1" y="1"/></ld>
      <sfc><scaling x="1" y="1"/></sfc>
    </coordinateInfo>
  </contentHeader>
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
</project>"#;

    let project = parse_plcopen_xml(xml, &test_file_id()).unwrap();

    assert_eq!(project.types.data_types.data_type.len(), 1);
    let dt = &project.types.data_types.data_type[0];
    assert_eq!(dt.name, "TrafficLight");

    let enum_type = cast!(&dt.base_type, DataType::Enum);
    assert_eq!(enum_type.values.value.len(), 3);
    assert_eq!(enum_type.values.value[0].name, "Red");
    assert_eq!(enum_type.values.value[1].name, "Yellow");
    assert_eq!(enum_type.values.value[2].name, "Green");
}

#[test]
fn parse_when_array_type_then_extracts_dimensions() {
    let xml = r#"<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://www.plcopen.org/xml/tc6_0201">
  <fileHeader companyName="Test" productName="Test" productVersion="1.0" creationDateTime="2024-01-01T00:00:00"/>
  <contentHeader name="TestProject">
    <coordinateInfo>
      <fbd><scaling x="1" y="1"/></fbd>
      <ld><scaling x="1" y="1"/></ld>
      <sfc><scaling x="1" y="1"/></sfc>
    </coordinateInfo>
  </contentHeader>
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
</project>"#;

    let project = parse_plcopen_xml(xml, &test_file_id()).unwrap();

    let dt = &project.types.data_types.data_type[0];
    assert_eq!(dt.name, "IntArray");

    let array = cast!(&dt.base_type, DataType::Array);
    assert_eq!(array.dimension.len(), 1);
    assert_eq!(array.dimension[0].lower, "0");
    assert_eq!(array.dimension[0].upper, "9");
    assert!(matches!(array.base_type, DataType::Int));
}

#[test]
fn parse_when_st_body_then_captures_position() {
    let xml = r#"<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://www.plcopen.org/xml/tc6_0201">
  <fileHeader companyName="Test" productName="Test" productVersion="1.0" creationDateTime="2024-01-01T00:00:00"/>
  <contentHeader name="TestProject">
    <coordinateInfo>
      <fbd><scaling x="1" y="1"/></fbd>
      <ld><scaling x="1" y="1"/></ld>
      <sfc><scaling x="1" y="1"/></sfc>
    </coordinateInfo>
  </contentHeader>
  <types>
    <dataTypes/>
    <pous>
      <pou name="Counter" pouType="functionBlock">
        <interface/>
        <body>
          <ST>
            <xhtml xmlns="http://www.w3.org/1999/xhtml">IF Reset THEN
  Count := 0;
END_IF;</xhtml>
          </ST>
        </body>
      </pou>
    </pous>
  </types>
</project>"#;

    let project = parse_plcopen_xml(xml, &test_file_id()).unwrap();
    let pou = &project.types.pous.pou[0];
    let body = pou.body.as_ref().unwrap();
    let st_body = body.st_body().unwrap();

    // The ST body text should start at a valid line position
    assert!(st_body.line_offset > 0, "Expected line > 0");
    assert!(st_body.text.contains("IF Reset THEN"));
}

#[test]
fn parse_when_multiple_pous_then_returns_all() {
    let xml = r#"<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://www.plcopen.org/xml/tc6_0201">
  <fileHeader companyName="Test" productName="Test" productVersion="1.0" creationDateTime="2024-01-01T00:00:00"/>
  <contentHeader name="TestProject">
    <coordinateInfo>
      <fbd><scaling x="1" y="1"/></fbd>
      <ld><scaling x="1" y="1"/></ld>
      <sfc><scaling x="1" y="1"/></sfc>
    </coordinateInfo>
  </contentHeader>
  <types>
    <dataTypes/>
    <pous>
      <pou name="POU1" pouType="functionBlock">
        <interface/>
        <body>
          <ST>
            <xhtml xmlns="http://www.w3.org/1999/xhtml">x := 1;</xhtml>
          </ST>
        </body>
      </pou>
      <pou name="POU2" pouType="functionBlock">
        <interface/>
        <body>
          <ST>
            <xhtml xmlns="http://www.w3.org/1999/xhtml">y := 2;</xhtml>
          </ST>
        </body>
      </pou>
    </pous>
  </types>
</project>"#;

    let project = parse_plcopen_xml(xml, &test_file_id()).unwrap();

    assert_eq!(project.types.pous.pou.len(), 2);
    assert_eq!(project.types.pous.pou[0].name, "POU1");
    assert_eq!(project.types.pous.pou[1].name, "POU2");

    // POU2 should be on a later line than POU1
    let pos1 = project.types.pous.pou[0]
        .body
        .as_ref()
        .unwrap()
        .st_body()
        .unwrap()
        .line_offset;
    let pos2 = project.types.pous.pou[1]
        .body
        .as_ref()
        .unwrap()
        .st_body()
        .unwrap()
        .line_offset;
    assert!(pos2 > pos1);
}

#[test]
fn parse_when_comprehensive_project_then_extracts_all_components() {
    // This test verifies that all major components of a PLCopen XML document
    // are parsed correctly
    let xml = r#"<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://www.plcopen.org/xml/tc6_0201">
  <fileHeader companyName="TestCorp" companyURL="http://test.com" productName="TestProduct" productVersion="2.0" productRelease="beta" creationDateTime="2024-06-15T10:30:00" contentDescription="Test project"/>
  <contentHeader name="ComprehensiveTest" version="1.0" author="Tester" organization="TestOrg" language="en">
    <Comment>Project comment</Comment>
    <coordinateInfo>
      <fbd><scaling x="1" y="1"/></fbd>
      <ld><scaling x="1" y="1"/></ld>
      <sfc><scaling x="1" y="1"/></sfc>
    </coordinateInfo>
  </contentHeader>
  <types>
    <dataTypes>
      <dataType name="MyEnum">
        <baseType>
          <enum>
            <values>
              <value name="Val1"/>
              <value name="Val2"/>
            </values>
          </enum>
        </baseType>
      </dataType>
      <dataType name="MyArray">
        <baseType>
          <array>
            <dimension lower="1" upper="10"/>
            <baseType><INT/></baseType>
          </array>
        </baseType>
      </dataType>
      <dataType name="MyStruct">
        <baseType>
          <struct>
            <variable name="Field1">
              <type><REAL/></type>
            </variable>
            <variable name="Field2">
              <type><BOOL/></type>
            </variable>
          </struct>
        </baseType>
      </dataType>
    </dataTypes>
    <pous>
      <pou name="AddNumbers" pouType="function">
        <interface>
          <returnType><INT/></returnType>
          <inputVars>
            <variable name="A">
              <type><INT/></type>
            </variable>
            <variable name="B">
              <type><INT/></type>
            </variable>
          </inputVars>
        </interface>
        <body>
          <ST>
            <xhtml xmlns="http://www.w3.org/1999/xhtml">AddNumbers := A + B;</xhtml>
          </ST>
        </body>
      </pou>
      <pou name="Counter" pouType="functionBlock">
        <interface>
          <inputVars>
            <variable name="Reset">
              <type><BOOL/></type>
            </variable>
          </inputVars>
          <outputVars>
            <variable name="Count">
              <type><INT/></type>
            </variable>
          </outputVars>
          <localVars>
            <variable name="Internal">
              <type><INT/></type>
            </variable>
          </localVars>
        </interface>
        <body>
          <ST>
            <xhtml xmlns="http://www.w3.org/1999/xhtml">IF Reset THEN Count := 0; END_IF;</xhtml>
          </ST>
        </body>
      </pou>
      <pou name="Main" pouType="program">
        <interface>
          <localVars>
            <variable name="MyCounter">
              <type><derived name="Counter"/></type>
            </variable>
          </localVars>
        </interface>
        <body>
          <ST>
            <xhtml xmlns="http://www.w3.org/1999/xhtml">MyCounter(Reset := FALSE);</xhtml>
          </ST>
        </body>
      </pou>
    </pous>
  </types>
</project>"#;

    let project = parse_plcopen_xml(xml, &test_file_id()).unwrap();

    // Verify file header
    assert_eq!(project.file_header.company_name, "TestCorp");
    assert_eq!(
        project.file_header.company_url,
        Some("http://test.com".to_string())
    );
    assert_eq!(project.file_header.product_name, "TestProduct");
    assert_eq!(project.file_header.product_version, "2.0");
    assert_eq!(
        project.file_header.product_release,
        Some("beta".to_string())
    );
    assert_eq!(
        project.file_header.content_description,
        Some("Test project".to_string())
    );

    // Verify content header
    assert_eq!(project.content_header.name, "ComprehensiveTest");
    assert_eq!(project.content_header.version, Some("1.0".to_string()));
    assert_eq!(project.content_header.author, Some("Tester".to_string()));
    assert_eq!(
        project.content_header.organization,
        Some("TestOrg".to_string())
    );

    // Verify data types
    assert_eq!(project.types.data_types.data_type.len(), 3);

    // Enum type
    let enum_type = &project.types.data_types.data_type[0];
    assert_eq!(enum_type.name, "MyEnum");
    let e = cast!(&enum_type.base_type, DataType::Enum);
    assert_eq!(e.values.value.len(), 2);
    assert_eq!(e.values.value[0].name, "Val1");
    assert_eq!(e.values.value[1].name, "Val2");

    // Array type
    let array_type = &project.types.data_types.data_type[1];
    assert_eq!(array_type.name, "MyArray");
    let a = cast!(&array_type.base_type, DataType::Array);
    assert_eq!(a.dimension.len(), 1);
    assert_eq!(a.dimension[0].lower, "1");
    assert_eq!(a.dimension[0].upper, "10");
    assert!(matches!(a.base_type, DataType::Int));

    // Struct type
    let struct_type = &project.types.data_types.data_type[2];
    assert_eq!(struct_type.name, "MyStruct");
    let s = cast!(&struct_type.base_type, DataType::Struct);
    assert_eq!(s.variable.len(), 2);
    assert_eq!(s.variable[0].name, "Field1");
    assert!(matches!(s.variable[0].member_type, DataType::Real));
    assert_eq!(s.variable[1].name, "Field2");
    assert!(matches!(s.variable[1].member_type, DataType::Bool));

    // Verify POUs
    assert_eq!(project.types.pous.pou.len(), 3);

    // Function
    let func = &project.types.pous.pou[0];
    assert_eq!(func.name, "AddNumbers");
    assert_eq!(func.pou_type, PouType::Function);
    let interface = func.interface.as_ref().unwrap();
    assert!(interface.return_type.is_some());
    assert!(matches!(
        interface.return_type.as_ref().unwrap(),
        DataType::Int
    ));
    assert_eq!(interface.input_vars.len(), 1);
    assert_eq!(interface.input_vars[0].variable.len(), 2);
    assert_eq!(interface.input_vars[0].variable[0].name, "A");
    assert_eq!(interface.input_vars[0].variable[1].name, "B");
    assert!(func.body.as_ref().unwrap().is_st());
    assert!(func
        .body
        .as_ref()
        .unwrap()
        .st_text()
        .unwrap()
        .contains("AddNumbers := A + B"));

    // Function block
    let fb = &project.types.pous.pou[1];
    assert_eq!(fb.name, "Counter");
    assert_eq!(fb.pou_type, PouType::FunctionBlock);
    let fb_interface = fb.interface.as_ref().unwrap();
    assert_eq!(fb_interface.input_vars.len(), 1);
    assert_eq!(fb_interface.output_vars.len(), 1);
    assert_eq!(fb_interface.local_vars.len(), 1);
    assert_eq!(fb_interface.input_vars[0].variable[0].name, "Reset");
    assert_eq!(fb_interface.output_vars[0].variable[0].name, "Count");
    assert_eq!(fb_interface.local_vars[0].variable[0].name, "Internal");

    // Program
    let prog = &project.types.pous.pou[2];
    assert_eq!(prog.name, "Main");
    assert_eq!(prog.pou_type, PouType::Program);
    let prog_interface = prog.interface.as_ref().unwrap();
    assert_eq!(prog_interface.local_vars.len(), 1);
    let local_var = &prog_interface.local_vars[0].variable[0];
    assert_eq!(local_var.name, "MyCounter");
    let d = cast!(&local_var.var_type, DataType::Derived);
    assert_eq!(d.name, "Counter");
}

#[test]
fn parse_when_variable_with_array_initial_value_then_extracts_elements() {
    let xml = r#"<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://www.plcopen.org/xml/tc6_0201">
  <fileHeader companyName="Test" productName="Test" productVersion="1.0" creationDateTime="2024-01-01T00:00:00"/>
  <contentHeader name="TestProject">
    <coordinateInfo>
      <fbd><scaling x="1" y="1"/></fbd>
      <ld><scaling x="1" y="1"/></ld>
      <sfc><scaling x="1" y="1"/></sfc>
    </coordinateInfo>
  </contentHeader>
  <types>
    <dataTypes/>
    <pous>
      <pou name="TestPou" pouType="program">
        <interface>
          <localVars>
            <variable name="MyArray">
              <type>
                <array>
                  <dimension lower="0" upper="2"/>
                  <baseType><INT/></baseType>
                </array>
              </type>
              <initialValue>
                <arrayValue>
                  <value><simpleValue value="1"/></value>
                  <value><simpleValue value="2"/></value>
                  <value repetitionValue="3"><simpleValue value="0"/></value>
                </arrayValue>
              </initialValue>
            </variable>
          </localVars>
        </interface>
      </pou>
    </pous>
  </types>
</project>"#;

    let project = parse_plcopen_xml(xml, &test_file_id()).unwrap();
    let pou = &project.types.pous.pou[0];
    let interface = pou.interface.as_ref().unwrap();
    let var = &interface.local_vars[0].variable[0];

    assert_eq!(var.name, "MyArray");
    let init = var.initial_value.as_ref().unwrap();
    let arr = init.array_value.as_ref().unwrap();
    assert_eq!(arr.value.len(), 3);

    // First element: simpleValue "1"
    assert_eq!(
        arr.value[0].value.simple_value.as_ref().unwrap().value,
        Some("1".to_string())
    );
    assert!(arr.value[0].repetition_value.is_none());

    // Second element: simpleValue "2"
    assert_eq!(
        arr.value[1].value.simple_value.as_ref().unwrap().value,
        Some("2".to_string())
    );

    // Third element: repeated 3 times, simpleValue "0"
    assert_eq!(arr.value[2].repetition_value, Some("3".to_string()));
    assert_eq!(
        arr.value[2].value.simple_value.as_ref().unwrap().value,
        Some("0".to_string())
    );
}

#[test]
fn parse_when_variable_with_struct_initial_value_then_extracts_members() {
    let xml = r#"<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://www.plcopen.org/xml/tc6_0201">
  <fileHeader companyName="Test" productName="Test" productVersion="1.0" creationDateTime="2024-01-01T00:00:00"/>
  <contentHeader name="TestProject">
    <coordinateInfo>
      <fbd><scaling x="1" y="1"/></fbd>
      <ld><scaling x="1" y="1"/></ld>
      <sfc><scaling x="1" y="1"/></sfc>
    </coordinateInfo>
  </contentHeader>
  <types>
    <dataTypes>
      <dataType name="Point">
        <baseType>
          <struct>
            <variable name="X"><type><INT/></type></variable>
            <variable name="Y"><type><INT/></type></variable>
          </struct>
        </baseType>
      </dataType>
    </dataTypes>
    <pous>
      <pou name="TestPou" pouType="program">
        <interface>
          <localVars>
            <variable name="MyPoint">
              <type><derived name="Point"/></type>
              <initialValue>
                <structValue>
                  <value member="X"><simpleValue value="10"/></value>
                  <value member="Y"><simpleValue value="20"/></value>
                </structValue>
              </initialValue>
            </variable>
          </localVars>
        </interface>
      </pou>
    </pous>
  </types>
</project>"#;

    let project = parse_plcopen_xml(xml, &test_file_id()).unwrap();
    let pou = &project.types.pous.pou[0];
    let interface = pou.interface.as_ref().unwrap();
    let var = &interface.local_vars[0].variable[0];

    assert_eq!(var.name, "MyPoint");
    let init = var.initial_value.as_ref().unwrap();
    let sv = init.struct_value.as_ref().unwrap();
    assert_eq!(sv.value.len(), 2);

    // First member: X = 10
    assert_eq!(sv.value[0].member, "X");
    assert_eq!(
        sv.value[0].value.simple_value.as_ref().unwrap().value,
        Some("10".to_string())
    );

    // Second member: Y = 20
    assert_eq!(sv.value[1].member, "Y");
    assert_eq!(
        sv.value[1].value.simple_value.as_ref().unwrap().value,
        Some("20".to_string())
    );
}

#[test]
fn parse_when_unsupported_version_then_returns_error() {
    let xml = r#"<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://www.plcopen.org/xml/tc6_0100">
  <fileHeader companyName="Test" productName="Test" productVersion="1.0" creationDateTime="2024-01-01T00:00:00"/>
  <contentHeader name="TestProject">
    <coordinateInfo>
      <fbd><scaling x="1" y="1"/></fbd>
      <ld><scaling x="1" y="1"/></ld>
      <sfc><scaling x="1" y="1"/></sfc>
    </coordinateInfo>
  </contentHeader>
  <types>
    <dataTypes/>
    <pous/>
  </types>
</project>"#;

    let result = parse_plcopen_xml(xml, &test_file_id());
    assert!(result.is_err());
    let error = result.unwrap_err();
    assert_eq!(error.code, Problem::XmlUnsupportedVersion.code());
}

#[test]
fn parse_when_no_namespace_then_succeeds() {
    // Allow files without namespace for flexibility
    let xml = r#"<?xml version="1.0" encoding="UTF-8"?>
<project>
  <fileHeader companyName="Test" productName="Test" productVersion="1.0" creationDateTime="2024-01-01T00:00:00"/>
  <contentHeader name="TestProject">
    <coordinateInfo>
      <fbd><scaling x="1" y="1"/></fbd>
      <ld><scaling x="1" y="1"/></ld>
      <sfc><scaling x="1" y="1"/></sfc>
    </coordinateInfo>
  </contentHeader>
  <types>
    <dataTypes/>
    <pous/>
  </types>
</project>"#;

    let result = parse_plcopen_xml(xml, &test_file_id());
    assert!(result.is_ok());
}
