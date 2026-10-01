//! VAR_TEMP and mixed located / non-located variable blocks, plus the
//! CODESYS/TwinCAT sections VAR_STAT, VAR_INST and VAR_GENERIC
//! (specs/design/st-declaration-extensions.md).

use super::common::*;
use crate::token::TokenType;
use dsl::core::Located;
use spec_test_macro::spec_test;

#[test]
fn parse_when_function_with_var_temp_then_succeeds() {
    let lib = parse_text(
        "FUNCTION my_func : DINT
VAR_INPUT
    a : DINT;
END_VAR
VAR_TEMP
    temp : DINT;
END_VAR
    temp := a * 2;
    my_func := temp;
END_FUNCTION",
    );
    let func = cast!(&lib.elements[0], LibraryElementKind::FunctionDeclaration);
    assert_eq!(func.variables.len(), 2);
    assert_eq!(func.variables[0].var_type, VariableType::Input);
    assert_eq!(func.variables[1].var_type, VariableType::VarTemp);
}

#[test]
fn parse_when_function_block_with_var_temp_then_succeeds() {
    let lib = parse_text(
        "FUNCTION_BLOCK my_fb
VAR_TEMP
    t : INT;
END_VAR
    t := 42;
END_FUNCTION_BLOCK",
    );
    let fb = cast!(
        &lib.elements[0],
        LibraryElementKind::FunctionBlockDeclaration
    );
    assert_eq!(fb.variables.len(), 1);
    assert_eq!(fb.variables[0].var_type, VariableType::VarTemp);
}

#[test]
fn parse_when_program_with_var_temp_then_fails() {
    let source = "PROGRAM main
VAR_TEMP
    t : INT;
END_VAR
    t := 42;
END_PROGRAM";
    let result = parse_program(source, &FileId::default(), &CompilerOptions::default());
    assert!(result.is_err());
}

#[test]
fn parse_when_program_mixed_located_and_non_located_vars_then_ok() {
    let lib = parse_text(
        "PROGRAM main
VAR
    Motor : BOOL;
    xStart AT %IX0.0 : BOOL;
    xStop AT %IX0.1 : BOOL;
END_VAR
END_PROGRAM",
    );
    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    assert_eq!(prog.variables.len(), 3);
    assert!(matches!(
        &prog.variables[0].identifier,
        VariableIdentifier::Symbol(_)
    ));
    assert!(matches!(
        &prog.variables[1].identifier,
        VariableIdentifier::Direct(_)
    ));
    assert!(matches!(
        &prog.variables[2].identifier,
        VariableIdentifier::Direct(_)
    ));
}

#[test]
fn parse_when_located_var_has_name_then_identifier_span_is_the_name() {
    let source = "PROGRAM main
VAR
    Motor : BOOL;
    xStart AT %IX0.0 : BOOL;
END_VAR
END_PROGRAM";
    let lib = parse_text(source);

    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    let span = prog.variables[1].identifier.span();
    assert_eq!(&source[span.start..span.end], "xStart");
}

#[test]
fn parse_when_located_var_has_no_name_then_identifier_span_is_the_address() {
    let source = "PROGRAM main
VAR
    AT %IX0.0 : BOOL;
END_VAR
END_PROGRAM";
    let lib = parse_text(source);

    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    let span = prog.variables[0].identifier.span();
    assert_eq!(&source[span.start..span.end], "%IX0.0");
}

#[test]
fn parse_when_incomplete_located_var_then_identifier_span_is_the_name() {
    let source = "FUNCTION_BLOCK FB_Example
VAR
    tempSensor AT %I* : INT;
END_VAR
END_FUNCTION_BLOCK";
    let lib = parse_text(source);

    let fb = cast!(
        &lib.elements[0],
        LibraryElementKind::FunctionBlockDeclaration
    );
    let span = fb.variables[0].identifier.span();
    assert_eq!(&source[span.start..span.end], "tempSensor");
}

#[test]
fn parse_when_program_mixed_vars_with_retain_qualifier_then_ok() {
    let lib = parse_text(
        "PROGRAM main
VAR RETAIN
    counter : INT;
    saved AT %MW0 : INT;
END_VAR
END_PROGRAM",
    );
    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    assert_eq!(prog.variables.len(), 2);
    assert_eq!(prog.variables[0].qualifier, DeclarationQualifier::Retain);
    assert_eq!(prog.variables[1].qualifier, DeclarationQualifier::Retain);
}

/// Beckhoff TwinCAT/CODESYS `PERSISTENT` qualifier on a top-level
/// `VAR_GLOBAL` block (the common case: a GVL's global persistent
/// variables).
#[test]
fn parse_when_var_global_persistent_and_flag_on_then_ok() {
    let options = CompilerOptions {
        allow_persistent_var: true,
        allow_top_level_var_global: true,
        ..CompilerOptions::default()
    };
    let lib = parse_program(
        "VAR_GLOBAL PERSISTENT
    nCounter : DINT;
END_VAR",
        &FileId::default(),
        &options,
    )
    .unwrap();
    let vars = cast!(&lib.elements[0], LibraryElementKind::GlobalVarDeclarations);
    assert_eq!(vars.len(), 1);
    assert_eq!(vars[0].qualifier, DeclarationQualifier::Persistent);
}

/// `PERSISTENT` on a `PROGRAM`'s own `VAR` block.
#[test]
fn parse_when_program_var_persistent_and_flag_on_then_ok() {
    let options = CompilerOptions {
        allow_persistent_var: true,
        ..CompilerOptions::default()
    };
    let lib = parse_program(
        "PROGRAM main
VAR PERSISTENT
    nCounter : DINT;
END_VAR
END_PROGRAM",
        &FileId::default(),
        &options,
    )
    .unwrap();
    let prog = cast!(&lib.elements[0], LibraryElementKind::ProgramDeclaration);
    assert_eq!(prog.variables.len(), 1);
    assert_eq!(
        prog.variables[0].qualifier,
        DeclarationQualifier::Persistent
    );
}

/// `PERSISTENT` on a `FUNCTION_BLOCK`'s own `VAR` block.
#[test]
fn parse_when_function_block_var_persistent_and_flag_on_then_ok() {
    let options = CompilerOptions {
        allow_persistent_var: true,
        ..CompilerOptions::default()
    };
    let lib = parse_program(
        "FUNCTION_BLOCK FB_Example
VAR PERSISTENT
    nCounter : DINT;
END_VAR
END_FUNCTION_BLOCK",
        &FileId::default(),
        &options,
    )
    .unwrap();
    let fb = cast!(
        &lib.elements[0],
        LibraryElementKind::FunctionBlockDeclaration
    );
    assert_eq!(fb.variables.len(), 1);
    assert_eq!(fb.variables[0].qualifier, DeclarationQualifier::Persistent);
}

#[test]
fn parse_when_program_motor_control_style_then_ok() {
    let lib = parse_text(
        "TYPE
  MotorState : (STOPPED, RUNNING, FAULTED);
END_TYPE

FUNCTION_BLOCK FB_MotorControl
  VAR_INPUT
    START_PB : BOOL;
    STOP_PB : BOOL;
    OL_CONTACT : BOOL;
    FAULT_RESET : BOOL;
  END_VAR
  VAR_OUTPUT
    CONTACTOR : BOOL;
    RUN_LAMP : BOOL;
    FAULT_LAMP : BOOL;
  END_VAR
  VAR
    Seal : BOOL;
  END_VAR

  IF NOT OL_CONTACT THEN
    Seal := FALSE;
  ELSE
    IF START_PB AND STOP_PB THEN
      Seal := TRUE;
    END_IF;
    IF NOT STOP_PB THEN
      Seal := FALSE;
    END_IF;
  END_IF;

  CONTACTOR := Seal;
  RUN_LAMP := CONTACTOR;
  FAULT_LAMP := NOT OL_CONTACT;
END_FUNCTION_BLOCK

PROGRAM PLC_PRG
  VAR
    Motor : FB_MotorControl;
    xStart AT %IX0.0 : BOOL;
    xStop AT %IX0.1 : BOOL;
    xOverload AT %IX0.2 : BOOL;
    xReset AT %IX0.3 : BOOL;
    yContactor AT %QX0.0 : BOOL;
    yRunLamp AT %QX0.1 : BOOL;
    yFaultLamp AT %QX0.2 : BOOL;
  END_VAR

  Motor(
    START_PB := xStart,
    STOP_PB := xStop,
    OL_CONTACT := xOverload,
    FAULT_RESET := xReset,
    CONTACTOR => yContactor,
    RUN_LAMP => yRunLamp,
    FAULT_LAMP => yFaultLamp
  );
END_PROGRAM",
    );
    let prog = cast!(&lib.elements[2], LibraryElementKind::ProgramDeclaration);
    assert_eq!(prog.variables.len(), 8);
    assert!(matches!(
        &prog.variables[0].identifier,
        VariableIdentifier::Symbol(_)
    ));
    assert!(matches!(
        &prog.variables[1].identifier,
        VariableIdentifier::Direct(_)
    ));
}

// ---------------------------------------------------------------------
// CODESYS/TwinCAT FB-instance call-style initializer (distinct node).
// ---------------------------------------------------------------------

#[test]
fn parse_when_fb_call_style_init_named_and_positional_then_parses_call_node() {
    // Both named (retries := 3) and positional (THIS) arguments in the same
    // call-style initializer, parsed into the distinct FunctionBlockCall node.
    let source = "
FUNCTION_BLOCK FB_Comm
VAR_INPUT
    retries : INT;
END_VAR
END_FUNCTION_BLOCK

FUNCTION_BLOCK FB_Example
VAR
    comm : FB_Comm(retries := 3, THIS);
END_VAR
END_FUNCTION_BLOCK";
    let library = parse_text(source);

    let fb = cast!(
        &library.elements[1],
        LibraryElementKind::FunctionBlockDeclaration
    );
    assert_eq!(fb.variables.len(), 1);
    let fb_call = cast!(
        &fb.variables[0].initializer,
        InitialValueAssignmentKind::FunctionBlockCall
    );
    assert_eq!(fb_call.type_name.to_string(), "FB_Comm");
    assert_eq!(fb_call.params.len(), 2);
    assert!(matches!(
        fb_call.params[0],
        ParamAssignmentKind::NamedInput(_)
    ));
    assert!(matches!(
        fb_call.params[1],
        ParamAssignmentKind::PositionalInput(_)
    ));
}

#[test]
fn parse_when_fb_call_style_init_empty_parens_then_parses_call_node() {
    let source = "
FUNCTION_BLOCK FB_Comm
END_FUNCTION_BLOCK

FUNCTION_BLOCK FB_Example
VAR
    comm : FB_Comm();
END_VAR
END_FUNCTION_BLOCK";
    let library = parse_text(source);

    let fb = cast!(
        &library.elements[1],
        LibraryElementKind::FunctionBlockDeclaration
    );
    let fb_call = cast!(
        &fb.variables[0].initializer,
        InitialValueAssignmentKind::FunctionBlockCall
    );
    assert!(fb_call.params.is_empty());
}

#[test]
fn parse_when_fb_bare_decl_then_not_call_node() {
    // Regression: a bare FB instance declaration (no initializer) flows
    // through late-bound resolution, not the call-style rule.
    let source = "
FUNCTION_BLOCK FB_Comm
END_FUNCTION_BLOCK

FUNCTION_BLOCK FB_Example
VAR
    comm : FB_Comm;
END_VAR
END_FUNCTION_BLOCK";
    let library = parse_text(source);

    let fb = cast!(
        &library.elements[1],
        LibraryElementKind::FunctionBlockDeclaration
    );
    assert!(matches!(
        &fb.variables[0].initializer,
        InitialValueAssignmentKind::LateResolvedType(_)
    ));
}

#[test]
fn parse_when_fb_struct_init_then_not_call_node() {
    // Regression: the standard `:= (member := value)` form must still parse
    // and must NOT become a FunctionBlockCall node.
    let source = "
FUNCTION_BLOCK FB_Comm
VAR_INPUT
    retries : INT;
END_VAR
END_FUNCTION_BLOCK

FUNCTION_BLOCK FB_Example
VAR
    comm : FB_Comm := (retries := 3);
END_VAR
END_FUNCTION_BLOCK";
    let library = parse_text(source);

    let fb = cast!(
        &library.elements[1],
        LibraryElementKind::FunctionBlockDeclaration
    );
    assert_eq!(fb.variables.len(), 1);
    assert!(!matches!(
        &fb.variables[0].initializer,
        InitialValueAssignmentKind::FunctionBlockCall(_)
    ));
}

// ---------------------------------------------------------------------
// CODESYS/TwinCAT additional variable sections.
// See specs/design/st-declaration-extensions.md.
// ---------------------------------------------------------------------

fn token_types(source: &str, options: &CompilerOptions) -> Vec<TokenType> {
    let (tokens, _) = crate::tokenize_program(source, &FileId::default(), options, 0, 0);
    tokens.iter().map(|t| t.token_type.clone()).collect()
}

/// Where a section's declarations are read from in a test case.
#[derive(Clone, Copy, Debug, PartialEq)]
enum POUKind {
    Function,
    FunctionBlock,
    Program,
    Method,
}

/// REQ-STX-parser-002: `VAR_STAT` lexes as `VarStat` and demotes to
/// `Identifier` unless `allow_var_stat` is set.
#[spec_test(REQ_STX_parser_002)]
fn lexer_spec_req_stx_002_var_stat_token_and_demotion() {
    let enabled = token_types("VAR_STAT", &opts_with_var_stat());
    assert!(enabled.contains(&TokenType::VarStat), "{enabled:?}");
    assert!(!enabled.contains(&TokenType::Identifier));

    let disabled = token_types("VAR_STAT", &CompilerOptions::default());
    assert!(disabled.contains(&TokenType::Identifier), "{disabled:?}");
    assert!(!disabled.contains(&TokenType::VarStat));
}

/// REQ-STX-parser-003: `VAR_INST` lexes as `VarInst` and demotes to
/// `Identifier` unless `allow_var_inst` is set.
#[spec_test(REQ_STX_parser_003)]
fn lexer_spec_req_stx_003_var_inst_token_and_demotion() {
    let enabled = token_types("VAR_INST", &opts_with_var_inst());
    assert!(enabled.contains(&TokenType::VarInst), "{enabled:?}");
    assert!(!enabled.contains(&TokenType::Identifier));

    let disabled = token_types("VAR_INST", &CompilerOptions::default());
    assert!(disabled.contains(&TokenType::Identifier), "{disabled:?}");
    assert!(!disabled.contains(&TokenType::VarInst));
}

/// REQ-STX-parser-004: `VAR_GENERIC` lexes as `VarGeneric` and demotes to
/// `Identifier` unless `allow_var_generic` is set.
#[spec_test(REQ_STX_parser_004)]
fn lexer_spec_req_stx_004_var_generic_token_and_demotion() {
    let enabled = token_types("VAR_GENERIC", &opts_with_var_generic());
    assert!(enabled.contains(&TokenType::VarGeneric), "{enabled:?}");
    assert!(!enabled.contains(&TokenType::Identifier));

    let disabled = token_types("VAR_GENERIC", &CompilerOptions::default());
    assert!(disabled.contains(&TokenType::Identifier), "{disabled:?}");
    assert!(!disabled.contains(&TokenType::VarGeneric));
}

/// REQ-STX-parser-012: `VAR_STAT` sections are accepted in a function, a
/// function block and a program, and `VAR_INST` in a method; each produces
/// declarations tagged with its own `VariableType`.
#[spec_test(REQ_STX_parser_012)]
#[rstest]
#[case::function_var_stat(
    "FUNCTION my_func : DINT
VAR_STAT
    calls : DINT;
END_VAR
    calls := calls + 1;
    my_func := calls;
END_FUNCTION",
    opts_with_var_stat,
    POUKind::Function,
    VariableType::VarStat
)]
#[case::function_block_var_stat(
    "FUNCTION_BLOCK FB_Counter
VAR_STAT
    count : DINT;
END_VAR
    count := count + 1;
END_FUNCTION_BLOCK",
    opts_with_var_stat,
    POUKind::FunctionBlock,
    VariableType::VarStat
)]
#[case::program_var_stat(
    "PROGRAM main
VAR_STAT
    scans : DINT;
END_VAR
    scans := scans + 1;
END_PROGRAM",
    opts_with_var_stat,
    POUKind::Program,
    VariableType::VarStat
)]
#[case::method_var_inst(
    "FUNCTION_BLOCK FB_Motor
VAR
    speed : INT;
END_VAR
METHOD DoWork : BOOL
VAR_INST
    callCount : INT;
END_VAR
    callCount := callCount + 1;
    DoWork := TRUE;
END_METHOD
END_FUNCTION_BLOCK",
    opts_with_var_inst,
    POUKind::Method,
    VariableType::VarInst
)]
fn parser_spec_req_stx_012_var_stat_and_var_inst_sections(
    #[case] source: &str,
    #[case] options: fn() -> CompilerOptions,
    #[case] pou: POUKind,
    #[case] expected: VariableType,
) {
    let library = parse_program(source, &FileId::default(), &options())
        .unwrap_or_else(|e| panic!("Source did not parse: {e:?}\n{source}"));
    let variables = match pou {
        POUKind::Function => {
            let f = cast!(
                &library.elements[0],
                LibraryElementKind::FunctionDeclaration
            );
            f.variables.clone()
        }
        POUKind::FunctionBlock => extract_fb(&library).variables.clone(),
        POUKind::Program => {
            let p = cast!(&library.elements[0], LibraryElementKind::ProgramDeclaration);
            p.variables.clone()
        }
        POUKind::Method => extract_fb(&library).methods[0].variables.clone(),
    };
    assert_eq!(variables.len(), 1, "{variables:?}");
    assert_eq!(variables[0].var_type, expected);
    assert!(variables[0].identifier.symbolic_id().is_some());
}

/// REQ-STX-parser-013: `VAR_GENERIC` is only accepted directly after a
/// function block's name -- before `EXTENDS` and the other sections -- and
/// nowhere else.
#[spec_test(REQ_STX_parser_013)]
fn parser_spec_req_stx_013_var_generic_only_after_function_block_name() {
    let source = "FUNCTION_BLOCK FB_Scale
VAR_GENERIC CONSTANT
    maxValue : INT := 100;
END_VAR
EXTENDS FB_Base
VAR
    value : INT;
END_VAR
END_FUNCTION_BLOCK";
    let options = CompilerOptions {
        allow_union_type: false,
        ..opts_with_var_generic()
    };
    let library = parse_program(source, &FileId::default(), &options).unwrap();
    let fb = extract_fb(&library);
    assert_eq!(fb.variables.len(), 2);
    assert_eq!(fb.variables[0].var_type, VariableType::VarGeneric);
    assert_eq!(fb.variables[0].qualifier, DeclarationQualifier::Constant);
    assert_eq!(
        fb.variables[0].identifier.symbolic_id(),
        Some(&Id::from("maxValue"))
    );
    assert_eq!(fb.variables[1].var_type, VariableType::Var);
    assert!(fb.oop.as_ref().is_some_and(|oop| oop.base.is_some()));

    // After another section the keyword is not a section opener, so the
    // declaration is a syntax error.
    let late = "FUNCTION_BLOCK FB_Scale
VAR
    value : INT;
END_VAR
VAR_GENERIC
    maxValue : INT := 100;
END_VAR
END_FUNCTION_BLOCK";
    assert!(
        parse_program(late, &FileId::default(), &opts_with_var_generic()).is_err(),
        "VAR_GENERIC after a VAR section must not parse"
    );

    // And not in a program, where there is no function block to parameterize.
    let in_program = "PROGRAM main
VAR_GENERIC
    g : INT;
END_VAR
END_PROGRAM";
    assert!(
        parse_program(in_program, &FileId::default(), &opts_with_var_generic()).is_err(),
        "VAR_GENERIC outside a function block must not parse"
    );
}
