//! Spec conformance tests for the attributes of declarations
//! (plc2plc-owned requirements `REQ-ATTR-plc2plc-NNN`): the renderer writes
//! the attributes of a declaration before it, and the text parses back to the
//! same attributes on the same declarations.
//!
//! See `specs/design/attributes.md`.

use dsl::attribute::census::{declared, TargetKind};
use dsl::common::Library;
use dsl::core::FileId;
use ironplc_parser::options::{CompilerOptions, Dialect};
use ironplc_parser::parse_program;
use ironplc_test::fixtures::ATTRIBUTE_PROGRAM;
use spec_test_macro::spec_test;

use crate::write_to_string;

fn options() -> CompilerOptions {
    CompilerOptions::from_dialect(Dialect::Codesys)
}

fn parsed(source: &str) -> Library {
    parse_program(source, &FileId::default(), &options()).expect("the source parses")
}

fn rendered(source: &str) -> String {
    write_to_string(&parsed(source)).expect("the library renders")
}

/// The trimmed lines of the rendering that follow the pragma line `pragma`
/// directly, skipping other pragma lines.
fn line_after(text: &str, pragma: &str) -> Option<String> {
    let lines: Vec<&str> = text.lines().map(str::trim).collect();
    let at = lines.iter().position(|line| *line == pragma)?;
    lines[at + 1..]
        .iter()
        .find(|line| !line.starts_with("{attribute"))
        .map(|line| line.to_string())
}

#[spec_test(REQ_ATTR_plc2plc_001)]
fn write_to_string_when_declarations_have_attributes_then_a_pragma_line_each_before_the_declaration(
) {
    let text = rendered(ATTRIBUTE_PROGRAM);

    // The first word of the line after the pragma is the keyword that opens
    // the declaration (the members of a structure and the names of a block
    // have the name or the block's keyword).
    let before: [(&str, &str); 19] = [
        ("enumeration", "TYPE"),
        ("subrange", "TYPE"),
        ("simple", "TYPE"),
        ("array", "TYPE"),
        ("structure", "TYPE"),
        ("member", "x : INT ;"),
        ("union", "TYPE"),
        ("string", "TYPE"),
        ("reference", "TYPE"),
        ("params", "TYPE"),
        ("late_bound", "TYPE"),
        ("interface", "INTERFACE Shape"),
        ("function", "FUNCTION Twice : INT"),
        ("variable", "VAR_INPUT"),
        ("function_block", "FUNCTION_BLOCK Counter"),
        ("method", "METHOD Reset"),
        ("property", "PROPERTY Current : INT"),
        ("action", "ACTION Light :"),
        ("program", "PROGRAM Main"),
    ];
    for (attribute, next) in before {
        let pragma = format!("{{attribute '{attribute}'}}");
        assert_eq!(
            line_after(&text, &pragma).as_deref(),
            Some(next),
            "{pragma} in\n{text}"
        );
        assert_eq!(
            text.matches(&pragma).count(),
            1,
            "{pragma} is written once in\n{text}"
        );
    }
}

#[spec_test(REQ_ATTR_plc2plc_001)]
fn write_to_string_when_attribute_has_a_value_then_the_pragma_has_the_value_as_written() {
    let text =
        rendered("{attribute 'obsolete' := 'use q'}\n{attribute 'Hide'}\nPROGRAM p\nEND_PROGRAM");

    let lines: Vec<&str> = text.lines().map(str::trim).collect();

    assert_eq!(
        &lines[..3],
        [
            "{attribute 'obsolete' := 'use q'}",
            "{attribute 'Hide'}",
            "PROGRAM p"
        ]
    );
}

#[spec_test(REQ_ATTR_plc2plc_001)]
fn write_to_string_when_declaration_lists_several_names_then_each_variable_carries_the_attributes()
{
    let text = rendered(
        "PROGRAM p\nVAR\n    {attribute 'a'} {attribute 'b' := '1'}\n    x, y : INT;\nEND_VAR\nEND_PROGRAM",
    );

    assert_eq!(text.matches("{attribute 'a'}").count(), 2, "{text}");
    assert_eq!(text.matches("{attribute 'b' := '1'}").count(), 2, "{text}");
    let reparsed = parse_program(&text, &FileId::default(), &options()).expect("it parses back");
    let variables: Vec<_> = declared(&reparsed)
        .into_iter()
        .filter(|d| d.kind == TargetKind::Variable)
        .collect();
    assert_eq!(variables.len(), 2);
    for variable in variables {
        let names: Vec<&str> = variable
            .attributes
            .iter()
            .map(|a| a.name.as_str())
            .collect();
        assert_eq!(names, ["a", "b"], "{}", variable.name);
    }
}

#[spec_test(REQ_ATTR_plc2plc_002)]
fn write_to_string_when_attribute_before_each_kind_of_target_then_parsing_it_back_gives_the_same_attributes(
) {
    let original = parsed(ATTRIBUTE_PROGRAM);
    let text = write_to_string(&original).expect("the library renders");
    let reparsed = parse_program(&text, &FileId::default(), &options())
        .unwrap_or_else(|e| panic!("the rendering does not parse: {e:?}\n{text}"));

    let (before, after) = (declared(&original), declared(&reparsed));

    for kind in TargetKind::ALL {
        let of_kind = |all: &[dsl::attribute::census::Declared]| {
            all.iter()
                .filter(|d| d.kind == *kind)
                .map(|d| (d.name.to_lowercase(), d.attributes.clone()))
                .collect::<Vec<_>>()
        };
        let expected = of_kind(&before);
        assert!(
            expected
                .iter()
                .any(|(_, attributes)| !attributes.is_empty()),
            "{kind:?} has an attribute before the rendering"
        );
        assert_eq!(expected, of_kind(&after), "{kind:?}");
    }
    assert_eq!(original, reparsed);
}
