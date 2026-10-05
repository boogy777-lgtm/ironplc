//! Cases pinned apart from the corpus comparison, test-only: the edges of what
//! a duration holds, and declarations whose legacy and lowered objects are
//! compared for one cause each.

use super::ast::compare;
use super::legacy::presets;
use super::literals::{body_tokens, file, literal_tokens, range_of};
use super::{new_parse, Class, Kind, Reason};
use crate::legacy::tokenize_program;
use crate::parser::{parse_constant, parse_type_declaration};
use ironplc_dsl::common::{DataTypeDeclarationKind, InitialValueAssignmentKind, SimpleDeclaration};
use ironplc_dsl::core::SourceSpan;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_syntax::lower::{
    disposition, literals::lower_constant, types::lower_type_declaration, Area, Disposition,
    LowerCx,
};
use ironplc_syntax::SyntaxKind;

/// Durations at and past the longest one a duration holds, with the problem
/// both parsers must report for each (`None` when both accept it). The legacy
/// parser and the lowering build them through the same checked builder, so
/// each must give the same duration, or the same problem over the same bytes.
const DURATION_RANGE_EDGES: &[(&str, Option<&str>)] = &[
    ("T#106751991167300d", None),
    ("T#106751991167301d", Some("P2039")),
    ("T#2562047788015215h", None),
    ("T#2562047788015216h", Some("P2039")),
    ("T#153722867280912930m", None),
    ("T#153722867280912931m", Some("P2039")),
    ("T#9223372036854775807s", None),
    ("T#9223372036854775807.999999999s", None),
    ("T#9223372036854775807s999999999ns", None),
    ("T#9223372036854775807s1000000000ns", Some("P2039")),
    ("T#9223372036854775808s", Some("P2039")),
    ("T#18446744073709551615s", Some("P2039")),
    ("T#18446744073709551615ms", None),
    ("T#18446744073709551615us", None),
    ("T#18446744073709551615ns", None),
    ("T#9223372036854775807d", Some("P2039")),
    ("T#106751991167300d23h", Some("P2039")),
    ("T#-9223372036854775807s", None),
    ("T#-9223372036854775807d", Some("P2039")),
    ("LTIME#9223372036854775807d", Some("P2039")),
    ("LT#106751991167300d", None),
];

const DIGITS_BEYOND_U64: Reason = Reason::new(Class::OwnerDecided, "owner decision: a number of twenty digits or more in a duration is a duration too large for any type, the same class as one that fits a count but not a duration, and so is reported with the same code (P2039); the legacy grammar fails to read the number and reports a syntax error (P0002)");

/// A whole part beyond `u64` is not a number the structure holds, whether or
/// not it has a decimal point (it was read as `0` without one). The legacy
/// parser reports a syntax error where the lowering reports the range problem.
const DURATION_DIGITS_EDGES: &[(&str, Reason)] = &[
    ("T#18446744073709551616s", DIGITS_BEYOND_U64),
    ("T#18446744073709551617ms", DIGITS_BEYOND_U64),
    ("T#99999999999999999999.5s", DIGITS_BEYOND_U64),
    ("LT#99999999999999999999999ns", DIGITS_BEYOND_U64),
];

#[test]
fn parity_when_duration_number_beyond_u64_then_legacy_syntax_error_and_lowering_range_error() {
    let preset = presets()
        .into_iter()
        .find(|preset| preset.name == "all-flags")
        .expect("the preset with every flag");
    for (snippet, reason) in DURATION_DIGITS_EDGES {
        assert!(!reason.text.is_empty());
        let text = format!("x := {snippet};");
        let parse = new_parse(Kind::Statements, &text, &preset.new);
        assert!(parse.is_ok(), "{snippet}");
        let node = parse
            .root
            .descendants()
            .find(|node| disposition(node.kind()) == Disposition::Lowered(Area::Literal))
            .expect("a literal");
        let tokens = tokenize_program(&text, &file(), &preset.legacy, 0, 0).0;
        let legacy = parse_constant(literal_tokens(&tokens, range_of(&node), false));
        let lowered = lower_constant(&LowerCx::new(file()), &node).err();
        assert_eq!(legacy.err().map(|d| d.code), Some("P0002".to_string()));
        let lowered = lowered.expect("the lowering rejects the number");
        assert_eq!(lowered.code, "P2039", "{snippet}");
        assert!(lowered.primary.message.contains(snippet), "{snippet}");
    }
}

#[test]
fn parity_when_duration_at_range_edge_then_same_duration_or_same_problem_and_range() {
    for preset in presets() {
        for (snippet, expected) in DURATION_RANGE_EDGES {
            let text = format!("x := {snippet};");
            let parse = new_parse(Kind::Statements, &text, &preset.new);
            if !parse.is_ok() {
                // `LTIME` and `LT` are keywords of the editions that have them.
                assert!(snippet.starts_with("LT"), "{snippet} under {}", preset.name);
                continue;
            }
            let node = parse
                .root
                .descendants()
                .find(|node| disposition(node.kind()) == Disposition::Lowered(Area::Literal))
                .expect("a literal");
            let range = range_of(&node);
            let tokens = tokenize_program(&text, &file(), &preset.legacy, 0, 0).0;
            let legacy = parse_constant(literal_tokens(&tokens, range, false));
            let lowered = lower_constant(&LowerCx::new(file()), &node);
            let code = |result: &Result<_, ironplc_dsl::diagnostic::Diagnostic>| {
                result.as_ref().err().map(|d| d.code.clone())
            };
            let expected_code = expected.map(str::to_string);
            assert_eq!(
                code(&legacy),
                expected_code,
                "{snippet} under {}: legacy",
                preset.name
            );
            assert_eq!(
                code(&lowered),
                expected_code,
                "{snippet} under {}: lowering",
                preset.name
            );
            match (&legacy, &lowered) {
                (Ok(legacy), Ok(lowered)) => {
                    assert_eq!(compare(legacy, lowered), vec![], "{snippet}");
                }
                // The wording of a syntax error differs by parser; the range
                // and the wording of a range problem do not.
                (Err(legacy), Err(lowered)) if legacy.code == "P2039" => {
                    assert_eq!(legacy.primary.message, lowered.primary.message);
                    assert_eq!(
                        (legacy.primary.location.start, legacy.primary.location.end),
                        (lowered.primary.location.start, lowered.primary.location.end),
                        "{snippet}"
                    );
                }
                _ => {}
            }
        }
    }
}

/// The legacy declaration and the lowered one of the declaration of a type
/// written as `text` (`name : type [:= value]`, without its `;`), under the
/// first preset with every flag on.
fn declaration_both(
    text: &str,
) -> (
    Result<DataTypeDeclarationKind, Diagnostic>,
    Result<DataTypeDeclarationKind, Diagnostic>,
) {
    let preset = presets()
        .into_iter()
        .find(|preset| preset.name == "all-flags")
        .expect("the preset with every flag");
    let source = format!("TYPE {text}; END_TYPE");
    let parse = new_parse(Kind::Declarations, &source, &preset.new);
    let node = parse
        .root
        .descendants()
        .find(|node| node.kind() == SyntaxKind::TypeDecl)
        .expect("a type declaration");
    let tokens = body_tokens(&source, &preset);
    let legacy = parse_type_declaration(literal_tokens(&tokens, range_of(&node), false));
    let lowered = lower_type_declaration(&LowerCx::new(file()).with_options(preset.new), &node);
    (legacy, lowered)
}

#[test]
fn legacy_declaration_when_elementary_type_then_the_type_name_has_no_position_and_the_lowering_has_one(
) {
    // The cause named by `TYPE_NAME_POSITION`.
    let (legacy, lowered) = declaration_both("t : INT := 5");
    let base = |declaration: Result<DataTypeDeclarationKind, Diagnostic>| match declaration {
        Ok(DataTypeDeclarationKind::Simple(SimpleDeclaration {
            spec_and_init: InitialValueAssignmentKind::Simple(initializer),
            ..
        })) => Some(initializer.type_name.name.span),
        _ => None,
    };
    let (legacy, lowered) = (
        base(legacy).expect("legacy"),
        base(lowered).expect("lowered"),
    );
    assert_eq!(legacy, SourceSpan::default());
    assert_eq!(
        (lowered.start, lowered.end, lowered.file_id),
        (9, 12, file())
    );
}

/// The value a structure member declares for itself, whatever the member's
/// type is spelled as.
fn member_default(
    declaration: Result<DataTypeDeclarationKind, Diagnostic>,
) -> Option<Option<ironplc_dsl::common::StructInitialValueAssignmentKind>> {
    match declaration {
        Ok(DataTypeDeclarationKind::Structure(structure)) => structure
            .elements
            .into_iter()
            .next()
            .map(|member| member.init.stated_value()),
        _ => None,
    }
}

#[test]
fn declaration_when_member_type_is_inline_then_both_parsers_keep_the_default() {
    for member in ["a : (X, Y) := X", "a : INT(1..10) := 5"] {
        let (legacy, lowered) = declaration_both(&format!("t : STRUCT {member}; END_STRUCT"));
        let (legacy, lowered) = (member_default(legacy), member_default(lowered));
        assert!(
            matches!(legacy, Some(Some(_))),
            "{member}: legacy {legacy:?}"
        );
        assert_eq!(legacy.is_some(), lowered.is_some(), "{member}");
        assert!(
            matches!(lowered, Some(Some(_))),
            "{member}: lowered {lowered:?}"
        );
    }
}
