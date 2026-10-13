//! Tests of the attributes of declarations: which declaration each attribute
//! pragma is attached to, and which pragma is reported because it is attached
//! to none.

use super::*;
use crate::lower::lower_elements;
use crate::{parse_source_file, ParseOptions};
use ironplc_dsl::attribute::census::{declared, Declared, TargetKind};
use ironplc_dsl::common::Library;
use ironplc_dsl::core::FileId;
use ironplc_test::fixtures::ATTRIBUTE_PROGRAM;
use rstest::rstest;
use spec_test_macro::spec_test;

pub(super) fn file() -> FileId {
    FileId::from_string("t.st")
}

/// The library of a source with every dialect enabled, lowered without the
/// check for pragmas that are attached to nothing.
pub(super) fn elements_of(source: &str) -> (crate::Parse, Library) {
    let parse = parse_source_file(source, &ParseOptions::all());
    assert!(parse.is_ok(), "{source}: {:?}", parse.errors);
    let cx = LowerCx::new(file()).with_options(parse.options);
    let elements = lower_elements(&cx, &parse.root).expect("the source lowers");
    (parse, Library { elements })
}

/// The names of the attributes of each declaration, `(kind, name, attributes)`.
fn attributed(source: &str) -> Vec<(TargetKind, String, Vec<String>)> {
    let (_, library) = elements_of(source);
    declared(&library)
        .into_iter()
        .map(|d| {
            let names = d.attributes.iter().map(|a| a.name.clone()).collect();
            (d.kind, d.name, names)
        })
        .collect()
}

/// The names of the attributes of the declaration of `kind` named `name`.
fn attributes_of_named(source: &str, kind: TargetKind, name: &str) -> Vec<String> {
    attributed(source)
        .into_iter()
        .find(|(k, n, _)| *k == kind && n.eq_ignore_ascii_case(name))
        .map(|(_, _, names)| names)
        .unwrap_or_else(|| panic!("no {kind:?} named {name} in {source}"))
}

/// The attribute that stands before each kind of target in
/// [`ATTRIBUTE_PROGRAM`], and the name of the declaration it stands before.
const BEFORE_EACH_KIND: &[(TargetKind, &str, &str)] = &[
    (TargetKind::Enumeration, "enumeration", "Color"),
    (TargetKind::Subrange, "subrange", "Percent"),
    (TargetKind::Simple, "simple", "Count"),
    (TargetKind::Array, "array", "Buffer"),
    (TargetKind::Structure, "structure", "Point"),
    (TargetKind::Union, "union", "Overlay"),
    (TargetKind::String, "string", "Label"),
    (TargetKind::Reference, "reference", "IntRef"),
    (TargetKind::Params, "params", "Pair"),
    (TargetKind::LateBound, "late_bound", "Location"),
    (TargetKind::Member, "member", "x"),
    (TargetKind::Variable, "variable", "value"),
    (TargetKind::Function, "function", "Twice"),
    (TargetKind::FunctionBlock, "function_block", "Counter"),
    (TargetKind::Program, "program", "Main"),
    (TargetKind::Interface, "interface", "Shape"),
    (TargetKind::Method, "method", "Reset"),
    (TargetKind::Property, "property", "Current"),
    (TargetKind::Action, "action", "Light"),
];

#[spec_test(REQ_ATTR_syntax_001)]
fn attributes_of_when_attribute_stands_before_each_kind_of_target_then_the_target_has_it() {
    for kind in TargetKind::ALL {
        assert!(
            BEFORE_EACH_KIND.iter().any(|(k, _, _)| k == kind),
            "{kind:?} has no case"
        );
    }
    for (kind, attribute, name) in BEFORE_EACH_KIND {
        assert_eq!(
            attributes_of_named(ATTRIBUTE_PROGRAM, *kind, name),
            vec![attribute.to_string()],
            "{kind:?} {name}"
        );
    }
    // No declaration holds an attribute that stands before another one.
    let total: usize = attributed(ATTRIBUTE_PROGRAM)
        .iter()
        .map(|(_, _, names)| names.len())
        .sum();
    assert_eq!(total, BEFORE_EACH_KIND.len());
}

#[spec_test(REQ_ATTR_syntax_001)]
fn attributes_of_when_attribute_has_a_value_then_name_value_and_span_as_written() {
    let source = "{attribute 'pack_mode' := '2'}\nPROGRAM p END_PROGRAM";
    let (_, library) = elements_of(source);

    let found: Vec<Declared> = declared(&library);

    let attribute = found[0].attributes.iter().next().expect("an attribute");
    assert_eq!(attribute.name, "pack_mode");
    assert_eq!(attribute.value.as_deref(), Some("2"));
    assert_eq!(
        (attribute.span.start, attribute.span.end),
        (0, "{attribute 'pack_mode' := '2'}".len())
    );
    assert_eq!(attribute.span.file_id, file());
}

#[spec_test(REQ_ATTR_syntax_001)]
fn attributes_of_when_attribute_has_no_value_then_none_and_case_of_the_name_is_kept() {
    let (_, library) = elements_of("{ATTRIBUTE 'Hide'}\nPROGRAM p END_PROGRAM");

    let found = declared(&library);

    let attribute = found[0].attributes.iter().next().expect("an attribute");
    assert_eq!(
        (attribute.name.as_str(), attribute.value.as_ref()),
        ("Hide", None)
    );
}

#[spec_test(REQ_ATTR_syntax_002)]
fn attributes_of_when_trivia_between_pragma_and_target_then_still_attached() {
    let source = "PROGRAM p
VAR
    {attribute 'a'} (* note *) {info 'm'} // end of line
    x : INT;
END_VAR
END_PROGRAM";

    assert_eq!(
        attributes_of_named(source, TargetKind::Variable, "x"),
        vec!["a".to_string()]
    );
}

#[spec_test(REQ_ATTR_syntax_003)]
fn attributes_of_when_several_pragmas_before_one_target_then_all_in_source_order() {
    let source = "PROGRAM p
VAR
    {attribute 'first'}
    {attribute 'second' := 'v'}
    x : INT;
    {attribute 'third'} y : INT;
END_VAR
END_PROGRAM";

    assert_eq!(
        attributes_of_named(source, TargetKind::Variable, "x"),
        vec!["first".to_string(), "second".to_string()]
    );
    assert_eq!(
        attributes_of_named(source, TargetKind::Variable, "y"),
        vec!["third".to_string()]
    );
}

#[rstest]
#[case::type_block(
    "{attribute 'g'} TYPE {attribute 'own'} A : INT; B : INT; END_TYPE",
    TargetKind::Simple
)]
#[case::global_block(
    "{attribute 'g'} VAR_GLOBAL {attribute 'own'} A : INT; B : INT; END_VAR",
    TargetKind::Variable
)]
#[case::input_block_of_a_unit(
    "FUNCTION_BLOCK f {attribute 'g'} VAR_INPUT {attribute 'own'} A : INT; B : INT; END_VAR END_FUNCTION_BLOCK",
    TargetKind::Variable
)]
#[case::block_of_a_method(
    "FUNCTION_BLOCK f METHOD m {attribute 'g'} VAR_TEMP {attribute 'own'} A : INT; B : INT; END_VAR END_METHOD END_FUNCTION_BLOCK",
    TargetKind::Variable
)]
#[spec_test(REQ_ATTR_syntax_004)]
fn attributes_of_when_pragma_before_a_group_then_every_declaration_of_it_group_first(
    #[case] source: &str,
    #[case] kind: TargetKind,
) {
    assert_eq!(
        attributes_of_named(source, kind, "A"),
        vec!["g".to_string(), "own".to_string()]
    );
    assert_eq!(
        attributes_of_named(source, kind, "B"),
        vec!["g".to_string()]
    );
}

#[spec_test(REQ_ATTR_syntax_004)]
fn attributes_of_when_group_in_a_unit_does_not_pass_its_attributes_to_the_unit() {
    let source = "{attribute 'unit'} FUNCTION_BLOCK f {attribute 'g'} VAR A : INT; END_VAR END_FUNCTION_BLOCK";

    assert_eq!(
        attributes_of_named(source, TargetKind::FunctionBlock, "f"),
        vec!["unit".to_string()]
    );
    assert_eq!(
        attributes_of_named(source, TargetKind::Variable, "A"),
        vec!["g".to_string()]
    );
}

#[spec_test(REQ_ATTR_syntax_005)]
fn attributes_of_when_declaration_lists_several_names_then_every_variable_has_them() {
    let source = "PROGRAM p VAR {attribute 'a'} x, y : INT; z : INT; END_VAR END_PROGRAM";

    for name in ["x", "y"] {
        assert_eq!(
            attributes_of_named(source, TargetKind::Variable, name),
            vec!["a".to_string()],
            "{name}"
        );
    }
    assert!(attributes_of_named(source, TargetKind::Variable, "z").is_empty());
}

#[spec_test(REQ_ATTR_syntax_005)]
fn attributes_of_when_edge_input_declaration_then_every_variable_has_them() {
    let source =
        "FUNCTION_BLOCK f VAR_INPUT {attribute 'a'} x, y : BOOL R_EDGE; END_VAR END_FUNCTION_BLOCK";

    for name in ["x", "y"] {
        assert_eq!(
            attributes_of_named(source, TargetKind::Variable, name),
            vec!["a".to_string()],
            "{name}"
        );
    }
}

#[spec_test(REQ_ATTR_syntax_006)]
fn role_when_node_kind_is_a_target_then_attribute_program_attaches_to_a_node_of_it() {
    // The kinds of node that are targets are the kinds an attribute of
    // `ATTRIBUTE_PROGRAM` is attached to: a kind that is a target and has no
    // lowering that asks for its attributes fails here.
    let parse = parse_source_file(ATTRIBUTE_PROGRAM, &ParseOptions::all());
    let mut attached_kinds: Vec<SyntaxKind> = parse
        .root
        .descendants()
        .filter(|node| !run_before(node).is_empty())
        .map(|node| node.kind())
        .collect();
    attached_kinds.sort();
    attached_kinds.dedup();

    let targets: Vec<SyntaxKind> = SyntaxKind::ALL
        .iter()
        .copied()
        .filter(|kind| role(*kind) == Role::Target)
        .collect();

    for target in &targets {
        assert!(
            attached_kinds.contains(target),
            "{target:?} is a target and no pragma of the program stands before one"
        );
    }
    // The groups are nodes only declarations stand in.
    for group in SyntaxKind::ALL.iter().filter(|k| role(**k) == Role::Group) {
        assert!(matches!(
            group,
            SyntaxKind::TypeBlock | SyntaxKind::VarBlock
        ));
    }
}

#[test]
fn node_role_when_every_node_kind_then_one_role_and_only_declarations_and_their_blocks_have_one() {
    let mut targets = 0;
    let mut groups = 0;
    for kind in SyntaxKind::ALL {
        match role(*kind) {
            Role::Target => targets += 1,
            Role::Group => groups += 1,
            Role::Neither => {}
        }
    }
    // Ten kinds of node write the nineteen kinds of target: a type
    // declaration writes ten of them.
    assert_eq!((targets, groups), (10, 2));
}

#[test]
fn role_when_token_then_neither() {
    assert_eq!(role(SyntaxKind::Pragma), Role::Neither);
    assert_eq!(role(SyntaxKind::Ident), Role::Neither);
}
