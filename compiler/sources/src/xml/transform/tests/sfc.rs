//! Tests for transforming sequential function chart bodies.

use super::*;
use ironplc_dsl::sfc::ElementKind;
use ironplc_test::cast;

// ========================================================================
// SFC transform tests
// ========================================================================

#[test]
fn transform_when_sfc_body_with_steps_then_creates_sfc() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes/>
    <pous>
      <pou name="SfcProgram" pouType="program">
        <interface>
          <localVars>
            <variable name="x">
              <type><INT/></type>
            </variable>
          </localVars>
        </interface>
        <body>
          <SFC>
            <step localId="1" name="Init" initialStep="true"/>
            <step localId="2" name="Running"/>
          </SFC>
        </body>
        <actions>
          <action name="StartMotor">
            <body>
              <ST>
                <xhtml xmlns="http://www.w3.org/1999/xhtml">x := 1;</xhtml>
              </ST>
            </body>
          </action>
        </actions>
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
    assert_eq!(prog_decl.name.to_string(), "SfcProgram");

    // Verify the body is an SFC
    let sfc = cast!(&prog_decl.body, FunctionBlockBodyKind::Sfc);
    assert_eq!(sfc.networks.len(), 1);
    assert_eq!(sfc.networks[0].initial_step.name.to_string(), "Init");
}

#[test]
fn transform_when_sfc_with_transition_then_creates_transition() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes/>
    <pous>
      <pou name="SfcProgram" pouType="program">
        <interface/>
        <body>
          <SFC>
            <step localId="1" name="Step1" initialStep="true"/>
            <step localId="2" name="Step2"/>
            <transition localId="3" name="T1" priority="1">
              <condition>
                <inline>
                  <ST>
                    <xhtml xmlns="http://www.w3.org/1999/xhtml">TRUE</xhtml>
                  </ST>
                </inline>
              </condition>
            </transition>
          </SFC>
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

    let prog_decl = cast!(&library.elements[0], LibraryElementKind::ProgramDeclaration);

    let sfc = cast!(&prog_decl.body, FunctionBlockBodyKind::Sfc);

    // Should have initial step + 1 other step + 1 transition in elements
    let transition_count = sfc.networks[0]
        .elements
        .iter()
        .filter(|e| matches!(e, ElementKind::Transition(_)))
        .count();
    assert_eq!(transition_count, 1);
}

#[test]
fn transform_when_sfc_with_action_then_creates_action() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes/>
    <pous>
      <pou name="SfcProgram" pouType="program">
        <interface/>
        <body>
          <SFC>
            <step localId="1" name="Step1" initialStep="true"/>
          </SFC>
        </body>
        <actions>
          <action name="DoSomething">
            <body>
              <ST>
                <xhtml xmlns="http://www.w3.org/1999/xhtml">;</xhtml>
              </ST>
            </body>
          </action>
        </actions>
      </pou>
    </pous>
  </types>
</project>"#,
        minimal_project_header()
    );

    let project = parse_project(&xml);
    let library =
        transform_project(&project, &test_file_id(), &CompilerOptions::default()).unwrap();

    let prog_decl = cast!(&library.elements[0], LibraryElementKind::ProgramDeclaration);

    let sfc = cast!(&prog_decl.body, FunctionBlockBodyKind::Sfc);

    // Should have the action in elements
    let action_count = sfc.networks[0]
        .elements
        .iter()
        .filter(|e| matches!(e, ElementKind::Action(_)))
        .count();
    assert_eq!(action_count, 1);
}

#[test]
fn transform_when_sfc_missing_initial_step_then_returns_error() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes/>
    <pous>
      <pou name="SfcProgram" pouType="program">
        <interface/>
        <body>
          <SFC>
            <step localId="1" name="Step1"/>
            <step localId="2" name="Step2"/>
          </SFC>
        </body>
      </pou>
    </pous>
  </types>
</project>"#,
        minimal_project_header()
    );

    let project = parse_project(&xml);
    let result = transform_project(&project, &test_file_id(), &CompilerOptions::default());

    assert!(result.is_err());
    let error = result.unwrap_err();
    assert_eq!(error.code, Problem::SfcMissingInitialStep.code());
}
