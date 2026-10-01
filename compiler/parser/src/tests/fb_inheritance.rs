//! OOP extensions: EXTENDS/IMPLEMENTS/INTERFACE and the qualifier words.
//! See specs/design/beckhoff-twincat-dialect.md §1.3-1.4 and
//! specs/design/st-declaration-extensions.md (OVERLOAD).

use super::common::*;
use dsl::member_qualifier::{AccessSpecifier, MemberQualifierKind};
use spec_test_macro::spec_test;

/// Proves that EXTENDS/IMPLEMENTS/INTERFACE/END_INTERFACE remain valid
/// identifiers in standard IEC 61131-3 mode. If keyword demotion for the
/// OOP extensions is missing or leaks into standard parsing, this test
/// fails. This must exist and pass before any OOP keyword tokens are
/// added (mirrors the same prerequisite in the Beckhoff TwinCAT dialect
/// design doc's Phase 0).
#[test]
fn parse_when_standard_mode_then_oop_keywords_are_valid_identifiers() {
    let program = "
FUNCTION_BLOCK FB_ALL_OOP_KEYWORDS_AS_VARS
VAR
    EXTENDS : INT;
    IMPLEMENTS : INT;
    INTERFACE : INT;
    END_INTERFACE : INT;
    ABSTRACT : INT;
END_VAR

EXTENDS := 1;
IMPLEMENTS := 2;
INTERFACE := 3;
END_INTERFACE := 4;
ABSTRACT := 5;
END_FUNCTION_BLOCK
";
    let result = parse_program(program, &FileId::default(), &CompilerOptions::default());
    assert!(
        result.is_ok(),
        "OOP keywords must remain valid identifiers in standard mode: {:?}",
        result.err()
    );
}

#[test]
fn parse_when_extends_only_then_ok_and_extends_set() {
    let source = "
FUNCTION_BLOCK FB_AdvancedMotor EXTENDS FB_Motor
VAR
    bRunning : BOOL;
END_VAR
END_FUNCTION_BLOCK";
    let library = parse_program(source, &FileId::default(), &opts_with_fb_inheritance()).unwrap();
    let fb = extract_fb(&library);
    let oop = fb.oop.as_ref().unwrap();
    assert_eq!(oop.base, Some(TypeName::from("FB_Motor")));
    assert!(oop.implements.is_empty());
}

#[test]
fn parse_when_implements_only_then_ok_and_implements_set() {
    let source = "
FUNCTION_BLOCK FB_AdvancedMotor IMPLEMENTS I_Drivable
VAR
    bRunning : BOOL;
END_VAR
END_FUNCTION_BLOCK";
    let library = parse_program(source, &FileId::default(), &opts_with_fb_inheritance()).unwrap();
    let fb = extract_fb(&library);
    let oop = fb.oop.as_ref().unwrap();
    assert_eq!(oop.base, None);
    assert_eq!(oop.implements, vec![TypeName::from("I_Drivable")]);
}

#[test]
fn parse_when_extends_and_implements_then_ok() {
    let source = "
FUNCTION_BLOCK FB_AdvancedMotor EXTENDS FB_Motor IMPLEMENTS I_Drivable
VAR
    bRunning : BOOL;
END_VAR
END_FUNCTION_BLOCK";
    let library = parse_program(source, &FileId::default(), &opts_with_fb_inheritance()).unwrap();
    let fb = extract_fb(&library);
    let oop = fb.oop.as_ref().unwrap();
    assert_eq!(oop.base, Some(TypeName::from("FB_Motor")));
    assert_eq!(oop.implements, vec![TypeName::from("I_Drivable")]);
}

#[test]
fn parse_when_implements_multiple_interfaces_then_ok() {
    // Real-world example: `IMPLEMENTS I_Hydraulics, I_Brake`.
    let source = "
FUNCTION_BLOCK FB_AdvancedMotor IMPLEMENTS I_Hydraulics, I_Brake
VAR
    bRunning : BOOL;
END_VAR
END_FUNCTION_BLOCK";
    let library = parse_program(source, &FileId::default(), &opts_with_fb_inheritance()).unwrap();
    let fb = extract_fb(&library);
    let oop = fb.oop.as_ref().unwrap();
    assert_eq!(
        oop.implements,
        vec![TypeName::from("I_Hydraulics"), TypeName::from("I_Brake")]
    );
}

#[test]
fn parse_when_no_extends_or_implements_then_oop_none() {
    let source = "
FUNCTION_BLOCK FB_Motor
VAR
    bRunning : BOOL;
END_VAR
END_FUNCTION_BLOCK";
    let library = parse_program(source, &FileId::default(), &opts_with_fb_inheritance()).unwrap();
    let fb = extract_fb(&library);
    assert!(fb.oop.is_none());
}

#[test]
fn parse_when_extends_and_default_dialect_then_err() {
    // Without allow_fb_inheritance, EXTENDS is just an identifier, so
    // this is a parse error (two consecutive identifiers).
    let source = "
FUNCTION_BLOCK FB_AdvancedMotor EXTENDS FB_Motor
VAR
    bRunning : BOOL;
END_VAR
END_FUNCTION_BLOCK";
    let result = parse_program(source, &FileId::default(), &CompilerOptions::default());
    assert!(result.is_err());
}

#[test]
fn parse_when_abstract_only_then_ok_and_is_abstract_set() {
    let source = "
FUNCTION_BLOCK ABSTRACT FB_BaseAxis
VAR
    bEnabled : BOOL;
END_VAR
END_FUNCTION_BLOCK";
    let library = parse_program(source, &FileId::default(), &opts_with_fb_inheritance()).unwrap();
    let fb = extract_fb(&library);
    let oop = fb.oop.as_ref().unwrap();
    assert!(oop.qualifiers.is_abstract());
    assert_eq!(oop.base, None);
    assert!(oop.implements.is_empty());
}

#[test]
fn parse_when_abstract_and_extends_and_implements_then_ok() {
    // Real-world shape: `ABSTRACT` combined with `EXTENDS`/`IMPLEMENTS`.
    let source = "
FUNCTION_BLOCK ABSTRACT FB_AxisControl EXTENDS FB_BaseAxis IMPLEMENTS I_Axis
VAR
    bEnabled : BOOL;
END_VAR
END_FUNCTION_BLOCK";
    let library = parse_program(source, &FileId::default(), &opts_with_fb_inheritance()).unwrap();
    let fb = extract_fb(&library);
    let oop = fb.oop.as_ref().unwrap();
    assert!(oop.qualifiers.is_abstract());
    assert_eq!(oop.base, Some(TypeName::from("FB_BaseAxis")));
    assert_eq!(oop.implements, vec![TypeName::from("I_Axis")]);
}

#[test]
fn parse_when_no_abstract_then_oop_none() {
    let source = "
FUNCTION_BLOCK FB_Motor
VAR
    bRunning : BOOL;
END_VAR
END_FUNCTION_BLOCK";
    let library = parse_program(source, &FileId::default(), &opts_with_fb_inheritance()).unwrap();
    let fb = extract_fb(&library);
    assert!(fb.oop.is_none());
}

#[test]
fn parse_when_abstract_and_default_dialect_then_ok_as_identifier() {
    // Without allow_fb_inheritance, ABSTRACT demotes to an ordinary
    // identifier, so `FUNCTION_BLOCK ABSTRACT` is parsed as a function
    // block literally named "ABSTRACT" -- matching how EXTENDS/IMPLEMENTS
    // behave when the flag is off.
    let source = "
FUNCTION_BLOCK ABSTRACT
VAR
    bRunning : BOOL;
END_VAR
END_FUNCTION_BLOCK";
    let library = parse_program(source, &FileId::default(), &CompilerOptions::default()).unwrap();
    let fb = extract_fb(&library);
    assert_eq!(fb.name, TypeName::from("ABSTRACT"));
    assert!(fb.oop.is_none());
}

/// Object-oriented programming is the headline addition of IEC 61131-3:2013,
/// so the `iec61131-3-ed3` dialect preset must parse it without any vendor
/// dialect or explicit `--allow-fb-inheritance`. Before #1427 the preset
/// demoted every OOP keyword to an identifier and reported a syntax error on
/// each of these sources.
#[rstest]
#[case::extends(
    "
FUNCTION_BLOCK FB_Base
END_FUNCTION_BLOCK

FUNCTION_BLOCK FB_Derived EXTENDS FB_Base
END_FUNCTION_BLOCK"
)]
#[case::implements(
    "
FUNCTION_BLOCK FB_Motor IMPLEMENTS I_Drivable
END_FUNCTION_BLOCK"
)]
#[case::abstract_fb(
    "
FUNCTION_BLOCK ABSTRACT FB_BaseAxis
END_FUNCTION_BLOCK"
)]
#[case::interface(
    "
INTERFACE I_Drivable
END_INTERFACE"
)]
#[case::method(
    "
FUNCTION_BLOCK FB_Motor
VAR
    speed : INT;
END_VAR
METHOD Stop
    speed := 0;
END_METHOD
END_FUNCTION_BLOCK"
)]
#[case::this_and_super(
    "
FUNCTION_BLOCK FB_Motor
VAR
    speed : INT;
END_VAR
THIS^.speed := SUPER^.speed;
END_FUNCTION_BLOCK"
)]
fn parse_when_edition3_dialect_then_oop_syntax_parses(#[case] source: &str) {
    let options = CompilerOptions::from_dialect(Dialect::Iec61131_3Ed3);
    let result = parse_program(source, &FileId::default(), &options);
    assert!(
        result.is_ok(),
        "the Edition 3 dialect must parse object-oriented syntax: {:?}",
        result.err()
    );
}

// ---------------------------------------------------------------------
// `OVERLOAD` — the remaining OO qualifier word. It is not a token: like
// `PUBLIC`/`FINAL`/`OVERRIDE` it is matched by text, only in the qualifier
// slot between `METHOD` and the name, and stays an identifier everywhere
// else. See specs/design/st-declaration-extensions.md.
// ---------------------------------------------------------------------

/// REQ-STX-parser-007: `OVERLOAD` is recognized as a member qualifier on a
/// method and kept, in source order, as `MemberQualifierKind::Overload`; a
/// word is a qualifier only when the method name still follows it.
#[spec_test(REQ_STX_parser_007)]
#[rstest]
#[case::overload_only(
    "METHOD OVERLOAD Reset\n    x := 0;\nEND_METHOD",
    "Reset",
    vec![MemberQualifierKind::Overload]
)]
#[case::lower_case(
    "METHOD overload Reset\n    x := 0;\nEND_METHOD",
    "Reset",
    vec![MemberQualifierKind::Overload]
)]
#[case::public_then_overload(
    "METHOD PUBLIC OVERLOAD Reset : BOOL\n    x := 0;\nEND_METHOD",
    "Reset",
    vec![
        MemberQualifierKind::Access(AccessSpecifier::Public),
        MemberQualifierKind::Overload,
    ]
)]
#[case::overload_then_override(
    "METHOD OVERLOAD OVERRIDE Reset\n    x := 0;\nEND_METHOD",
    "Reset",
    vec![MemberQualifierKind::Overload, MemberQualifierKind::Override]
)]
#[case::overload_is_the_method_name(
    "METHOD Overload : BOOL\n    x := 0;\nEND_METHOD",
    "Overload",
    vec![]
)]
#[case::overload_is_the_method_name_with_body(
    "METHOD Overload x := 1;\nEND_METHOD",
    "Overload",
    vec![]
)]
fn parser_spec_req_stx_007_overload_is_a_member_qualifier(
    #[case] method: &str,
    #[case] expected_name: &str,
    #[case] expected_kinds: Vec<MemberQualifierKind>,
) {
    let source = format!(
        "
FUNCTION_BLOCK ABSTRACT FB_Motor
VAR
    x : INT;
END_VAR
{method}
END_FUNCTION_BLOCK"
    );
    let library = parse_program(&source, &FileId::default(), &opts_with_fb_inheritance())
        .expect("Source did not parse");
    let fb = extract_fb(&library);
    assert_eq!(fb.methods.len(), 1);
    let parsed = &fb.methods[0];
    assert_eq!(parsed.name, Id::from(expected_name));
    let kinds: Vec<MemberQualifierKind> = parsed.qualifiers.iter().map(|q| q.kind).collect();
    assert_eq!(kinds, expected_kinds);
}
