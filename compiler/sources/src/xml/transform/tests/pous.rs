//! Tests for transforming program and function block declarations and their bodies.

use super::*;
use ironplc_test::cast;

#[test]
fn transform_when_function_block_then_creates_declaration() {
    let xml = format!(
        r#"{}
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
IF Reset THEN Count := 0; END_IF;
            </xhtml>
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

    assert_eq!(library.elements.len(), 1);
    let fb_decl = cast!(
        &library.elements[0],
        LibraryElementKind::FunctionBlockDeclaration
    );
    assert_eq!(fb_decl.name.to_string(), "Counter");
    assert_eq!(fb_decl.variables.len(), 2);
}

#[test]
fn transform_when_program_then_creates_declaration() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes/>
    <pous>
      <pou name="Main" pouType="program">
        <interface>
          <localVars>
            <variable name="x">
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
    let library =
        transform_project(&project, &test_file_id(), &CompilerOptions::default()).unwrap();

    assert_eq!(library.elements.len(), 1);
    let prog_decl = cast!(&library.elements[0], LibraryElementKind::ProgramDeclaration);
    assert_eq!(prog_decl.name.to_string(), "Main");
    assert_eq!(prog_decl.variables.len(), 1);
}

#[test]
fn transform_when_function_block_with_st_body_then_parses_statements() {
    let xml = format!(
        r#"{}
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
</project>"#,
        minimal_project_header()
    );

    let project = parse_project(&xml);
    let library =
        transform_project(&project, &test_file_id(), &CompilerOptions::default()).unwrap();

    assert_eq!(library.elements.len(), 1);
    let fb_decl = cast!(
        &library.elements[0],
        LibraryElementKind::FunctionBlockDeclaration
    );

    // Verify the body has statements
    let stmts = cast!(&fb_decl.body, FunctionBlockBodyKind::Statements);
    assert!(!stmts.body.is_empty(), "Expected parsed statements");
}
