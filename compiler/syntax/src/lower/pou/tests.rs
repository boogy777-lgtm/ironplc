//! Tests of the units: a row of `SECTIONS`, `BODIES` and `RETURN_TYPES` for
//! each thing a unit may hold, and what each unit makes of its parts.

use super::*;
use crate::lower::{disposition, lower_library, Area, Disposition, INTERNAL_ERROR};
use crate::{parse_source_file, ParseOptions};
use ironplc_dsl::common::{
    DeclarationQualifier, IntegerRef, LibraryElementKind, StringSpecification, VariableType,
};
use ironplc_dsl::core::{FileId, Located};

fn file() -> FileId {
    FileId::from_string("t.st")
}

/// The elements of `source`, lowered with every keyword enabled.
fn elements(source: &str) -> Vec<LibraryElementKind> {
    let parse = parse_source_file(source, &ParseOptions::all());
    assert!(parse.is_ok(), "{source}: {:?}", parse.errors);
    lower_library(&parse, &file())
        .map_err(|error| format!("{source}: {}", error.primary.message))
        .expect("the source lowers")
        .elements
}

fn program(source: &str) -> ProgramDeclaration {
    match elements(source).remove(0) {
        LibraryElementKind::ProgramDeclaration(program) => Some(program),
        _ => None,
    }
    .expect("a program")
}

fn function(source: &str) -> FunctionDeclaration {
    match elements(source).remove(0) {
        LibraryElementKind::FunctionDeclaration(function) => Some(function),
        _ => None,
    }
    .expect("a function")
}

fn function_block(source: &str) -> FunctionBlockDeclaration {
    match elements(source).remove(0) {
        LibraryElementKind::FunctionBlockDeclaration(block) => Some(block),
        _ => None,
    }
    .expect("a function block")
}

fn namespace_of(element: &LibraryElementKind) -> &NamespaceDeclaration {
    match element {
        LibraryElementKind::NamespaceDeclaration(namespace) => Some(namespace),
        _ => None,
    }
    .expect("a namespace")
}

fn names(variables: &[VarDecl]) -> Vec<String> {
    variables
        .iter()
        .filter_map(|variable| variable.identifier.symbolic_id())
        .map(|id| id.original().to_string())
        .collect()
}

// ---- Dispositions.

#[test]
fn disposition_when_unit_kind_then_lowered_by_the_unit_area() {
    for kind in [
        K::ProgramDecl,
        K::FunctionDecl,
        K::FunctionBlockDecl,
        K::InterfaceDecl,
        K::NamespaceDecl,
    ] {
        assert_eq!(
            disposition(kind),
            Disposition::Lowered(Area::Unit),
            "{kind:?}"
        );
    }
}

#[test]
fn disposition_when_part_of_a_declaration_then_structural_and_when_a_member_then_the_member_area() {
    for kind in [
        K::GetAccessor,
        K::SetAccessor,
        K::MemberQualifier,
        K::ExtendsClause,
        K::ImplementsClause,
    ] {
        assert_eq!(disposition(kind), Disposition::Structural, "{kind:?}");
    }
    for kind in [K::MethodDecl, K::PropertyDecl] {
        assert_eq!(
            disposition(kind),
            Disposition::Lowered(Area::Member),
            "{kind:?}"
        );
    }
}

#[test]
fn header_when_each_kind_then_it_is_not_a_section() {
    for kind in HEADER {
        assert!(
            !SECTIONS.iter().any(|(section, _)| section == kind),
            "{kind:?}"
        );
    }
}

#[test]
fn lower_body_when_nothing_is_written_then_an_empty_body_and_a_list_without_text_is_none() {
    let cx = LowerCx::new(file());
    let source = "PROGRAM p INITIAL_STEP s: END_STEP ACTION a: END_ACTION ACTION b: ; END_ACTION END_PROGRAM";
    let parse = parse_source_file(source, &ParseOptions::all());
    let actions: Vec<_> = parse
        .root
        .descendants()
        .filter(|node| node.kind() == K::ActionDecl)
        .collect();
    assert_eq!(actions.len(), 2);
    // A list that holds no text is no body; one that holds a `;` is a list of
    // no statements, as for a unit.
    assert!(matches!(
        lower_body(&cx, &actions[0]),
        Ok(FunctionBlockBodyKind::Empty)
    ));
    assert!(matches!(
        lower_body(&cx, &actions[1]),
        Ok(FunctionBlockBodyKind::Statements(statements)) if statements.body.is_empty()
    ));
}

// ---- Programs.

#[test]
fn lower_program_when_blocks_of_every_kind_then_one_list_in_the_order_written() {
    let lowered = program(
        "PROGRAM p
VAR a : INT; END_VAR
VAR_INPUT b : BOOL; END_VAR
VAR_OUTPUT c : BOOL; END_VAR
VAR_IN_OUT d : INT; END_VAR
VAR e AT %IX0.0 : BOOL; END_VAR
VAR_EXTERNAL f : INT; END_VAR
VAR RETAIN g : INT; END_VAR
END_PROGRAM",
    );
    assert_eq!(
        names(&lowered.variables),
        ["a", "b", "c", "d", "e", "f", "g"]
    );
    let kinds: Vec<VariableType> = lowered
        .variables
        .iter()
        .map(|variable| variable.var_type.clone())
        .collect();
    assert_eq!(
        kinds,
        [
            VariableType::Var,
            VariableType::Input,
            VariableType::Output,
            VariableType::InOut,
            VariableType::Var,
            VariableType::External,
            VariableType::Var,
        ]
    );
    assert_eq!(lowered.variables[6].qualifier, DeclarationQualifier::Retain);
}

#[test]
fn lower_program_when_blocks_then_each_block_is_its_own_block_and_a_block_shares_one() {
    let lowered = program("PROGRAM p VAR a, b : INT; END_VAR VAR c : INT; END_VAR END_PROGRAM");
    let blocks: Vec<_> = lowered
        .variables
        .iter()
        .map(|variable| variable.block)
        .collect();
    assert_eq!(blocks[0], blocks[1]);
    assert_ne!(blocks[1], blocks[2]);
}

#[test]
fn lower_program_when_access_block_then_the_access_paths() {
    let lowered = program("PROGRAM p VAR_ACCESS a : x.y : INT READ_ONLY; END_VAR END_PROGRAM");
    assert_eq!(lowered.access_variables.len(), 1);
    assert_eq!(lowered.access_variables[0].access_name.original(), "a");
    assert!(lowered.variables.is_empty());
}

#[test]
fn lower_program_when_no_body_then_an_empty_body_and_when_only_a_comment_then_the_same() {
    for source in [
        "PROGRAM p END_PROGRAM",
        "PROGRAM p VAR a : INT; END_VAR (* nothing *) END_PROGRAM",
    ] {
        assert!(
            matches!(program(source).body, FunctionBlockBodyKind::Empty),
            "{source}"
        );
    }
}

#[test]
fn lower_program_when_only_an_empty_statement_then_a_body_of_no_statements() {
    let body = program("PROGRAM p ; END_PROGRAM").body;
    assert!(
        matches!(body, FunctionBlockBodyKind::Statements(statements) if statements.body.is_empty())
    );
}

#[test]
fn lower_program_when_statements_then_the_body_holds_them() {
    let body = program("PROGRAM p x := 1; y := 2; END_PROGRAM").body;
    assert!(
        matches!(body, FunctionBlockBodyKind::Statements(statements) if statements.body.len() == 2)
    );
}

#[test]
fn lower_program_when_name_then_original_spelling_and_position() {
    let lowered = program("PROGRAM  Speed END_PROGRAM");
    assert_eq!(lowered.name.original(), "Speed");
    assert_eq!(
        (
            lowered.name.span.start,
            lowered.name.span.end,
            lowered.name.span.file_id
        ),
        (9, 14, file())
    );
}

#[test]
fn lower_program_when_sequential_chart_then_a_chart_body_and_never_an_empty_one() {
    let lowered = program("PROGRAM p INITIAL_STEP s: END_STEP END_PROGRAM");
    assert!(
        matches!(&lowered.body, FunctionBlockBodyKind::Sfc(chart) if chart.networks.len() == 1),
        "{:?}",
        lowered.body
    );
}

// ---- Functions.

#[test]
fn lower_function_when_each_form_of_return_type_then_its_row() {
    let named = function("FUNCTION f : my_type f := 1; END_FUNCTION").return_type;
    assert!(matches!(&named, FunctionReturnType::Named(name) if name.name.original() == "my_type"));
    let elementary = function("FUNCTION f : tod f := 1; END_FUNCTION").return_type;
    assert!(
        matches!(&elementary, FunctionReturnType::Named(name) if name.name.original() == "TIME_OF_DAY")
    );
    let sized = function("FUNCTION f : STRING[10] f := 1; END_FUNCTION").return_type;
    assert!(matches!(
        &sized,
        FunctionReturnType::String(StringSpecification {
            width: StringType::String,
            length: Some(IntegerRef::Literal(_)),
            ..
        })
    ));
    let wide = function("FUNCTION f : WSTRING(5) f := 1; END_FUNCTION").return_type;
    assert!(matches!(
        &wide,
        FunctionReturnType::WString(StringSpecification {
            width: StringType::WString,
            length: Some(_),
            ..
        })
    ));
    let bare = function("FUNCTION f : STRING f := 1; END_FUNCTION").return_type;
    assert!(matches!(
        &bare,
        FunctionReturnType::String(StringSpecification { length: None, .. })
    ));
}

#[test]
fn lower_function_when_blocks_edges_and_body_then_each_in_its_list() {
    let lowered = function(
        "FUNCTION f : INT
VAR_INPUT a : INT; e : BOOL R_EDGE; END_VAR
VAR t : INT; END_VAR
VAR_STAT s : INT; END_VAR
f := a;
END_FUNCTION",
    );
    assert_eq!(names(&lowered.variables), ["a", "t", "s"]);
    assert_eq!(lowered.edge_variables.len(), 1);
    assert_eq!(lowered.edge_variables[0].identifier.original(), "e");
    assert_eq!(lowered.body.len(), 1);
}

#[test]
fn lower_function_when_time_names_the_function_then_the_name_is_time() {
    let options = ParseOptions {
        allow_time_as_function_name: true,
        ..ParseOptions::all()
    };
    let parse = parse_source_file("FUNCTION TIME : INT TIME := 1; END_FUNCTION", &options);
    assert!(parse.is_ok(), "{:?}", parse.errors);
    let library = lower_library(&parse, &file());
    let name = library
        .ok()
        .and_then(|library| match library.elements.first() {
            Some(LibraryElementKind::FunctionDeclaration(function)) => {
                Some(function.name.original().to_string())
            }
            _ => None,
        });
    assert_eq!(name.as_deref(), Some("TIME"));
}

// ---- Function blocks.

#[test]
fn lower_function_block_when_plain_then_no_oop_facet_and_the_name_is_a_type_name() {
    let lowered =
        function_block("FUNCTION_BLOCK fb VAR a : INT; END_VAR a := 1; END_FUNCTION_BLOCK");
    assert!(lowered.oop.is_none());
    assert_eq!(lowered.name.name.original(), "fb");
    assert_eq!(names(&lowered.variables), ["a"]);
    assert!(lowered.methods.is_empty() && lowered.properties.is_empty());
    assert_eq!((lowered.span.start, lowered.span.end), (0, 65));
}

#[test]
fn lower_function_block_when_generic_block_then_it_comes_first_in_the_order_written() {
    let lowered = function_block(
        "FUNCTION_BLOCK fb VAR_GENERIC CONSTANT n : INT := 1; END_VAR VAR a : INT; END_VAR END_FUNCTION_BLOCK",
    );
    assert_eq!(names(&lowered.variables), ["n", "a"]);
    assert_eq!(lowered.variables[0].var_type, VariableType::VarGeneric);
}

#[test]
fn lower_function_block_when_members_then_methods_and_properties_apart_in_the_order_written() {
    let lowered = function_block(
        "FUNCTION_BLOCK fb
x := 1;
METHOD a END_METHOD
PROPERTY p : INT GET END_GET END_PROPERTY
METHOD b END_METHOD
END_FUNCTION_BLOCK",
    );
    let methods: Vec<String> = lowered
        .methods
        .iter()
        .map(|m| m.name.original().to_string())
        .collect();
    assert_eq!(methods, ["a", "b"]);
    assert_eq!(lowered.properties.len(), 1);
    assert!(matches!(lowered.body, FunctionBlockBodyKind::Statements(_)));
}

#[test]
fn lower_function_block_when_sequential_chart_then_a_chart_body() {
    let lowered = function_block("FUNCTION_BLOCK fb INITIAL_STEP s: END_STEP END_FUNCTION_BLOCK");
    assert!(matches!(lowered.body, FunctionBlockBodyKind::Sfc(_)));
}

#[test]
fn lower_function_when_sequential_chart_then_an_internal_error() {
    // The grammar does not read a chart in a function, so a tree that holds
    // one is not the tree the parser builds.
    let parse = parse_source_file(
        "FUNCTION f : INT f := 1; END_FUNCTION",
        &ParseOptions::all(),
    );
    let node = parse.root.first_child().expect("a function");
    let error = statements_of(
        &LowerCx::new(file()),
        &node,
        Some(FunctionBlockBodyKind::sfc(vec![])),
    )
    .expect_err("a chart is not statements");
    assert_eq!(error.code, INTERNAL_ERROR);
}

// ---- Interfaces.

#[test]
fn lower_interface_when_extends_then_the_names_in_order_and_when_not_then_none() {
    let written = elements("INTERFACE i EXTENDS a, b END_INTERFACE INTERFACE j END_INTERFACE");
    let extends: Vec<Vec<String>> = written
        .iter()
        .map(|element| match element {
            LibraryElementKind::InterfaceDeclaration(interface) => interface
                .extends
                .iter()
                .map(|name| name.name.original().to_string())
                .collect(),
            _ => vec![],
        })
        .collect();
    assert_eq!(extends, [vec!["a", "b"], Vec::<&str>::new()]);
}

#[test]
fn lower_interface_when_declared_then_the_span_is_that_of_its_names() {
    let written = elements("INTERFACE i EXTENDS a END_INTERFACE");
    let span = match &written[0] {
        LibraryElementKind::InterfaceDeclaration(interface) => Some(interface.span()),
        _ => None,
    };
    assert_eq!(span.map(|span| (span.start, span.end)), Some((10, 21)));
}

// ---- Namespaces.

#[test]
fn lower_namespace_when_nested_then_the_declarations_nest_and_the_name_is_not_one_of_them() {
    let written = elements(
        "NAMESPACE outer
TYPE t : INT; END_TYPE
NAMESPACE inner PROGRAM p END_PROGRAM END_NAMESPACE
VAR_GLOBAL g : INT; END_VAR
FUNCTION f : INT f := 1; END_FUNCTION
END_NAMESPACE",
    );
    assert_eq!(written.len(), 1);
    let outer = namespace_of(&written[0]);
    assert_eq!(outer.name.original(), "outer");
    assert_eq!(outer.elements.len(), 4);
    assert!(matches!(
        &outer.elements[0],
        LibraryElementKind::DataTypeDeclaration(_)
    ));
    assert!(matches!(
        &outer.elements[2],
        LibraryElementKind::GlobalVarDeclarations(_)
    ));
    assert!(matches!(
        &outer.elements[3],
        LibraryElementKind::FunctionDeclaration(_)
    ));
    let inner = namespace_of(&outer.elements[1]);
    assert_eq!(inner.name.original(), "inner");
    assert!(matches!(
        &inner.elements[0],
        LibraryElementKind::ProgramDeclaration(_)
    ));
}

#[test]
fn lower_namespace_when_empty_then_no_elements_and_the_span_covers_the_declaration() {
    let written = elements("NAMESPACE n END_NAMESPACE");
    let namespace = namespace_of(&written[0]);
    assert!(namespace.elements.is_empty());
    assert_eq!((namespace.span.start, namespace.span.end), (0, 25));
}

#[test]
fn lower_namespace_when_it_holds_a_configuration_then_the_configuration_is_an_element() {
    let written = elements(
        "NAMESPACE n CONFIGURATION c RESOURCE r ON t PROGRAM p : q; END_RESOURCE END_CONFIGURATION END_NAMESPACE",
    );
    assert!(matches!(
        namespace_of(&written[0]).elements[..],
        [LibraryElementKind::ConfigurationDeclaration(_)]
    ));
}

// ---- The file.

#[test]
fn lower_library_when_every_kind_of_unit_then_one_element_each_in_the_order_written() {
    let written = elements(
        "PROGRAM p END_PROGRAM
FUNCTION f : INT f := 1; END_FUNCTION
FUNCTION_BLOCK fb END_FUNCTION_BLOCK
INTERFACE i END_INTERFACE
NAMESPACE n END_NAMESPACE",
    );
    assert!(matches!(
        &written[0],
        LibraryElementKind::ProgramDeclaration(_)
    ));
    assert!(matches!(
        &written[1],
        LibraryElementKind::FunctionDeclaration(_)
    ));
    assert!(matches!(
        &written[2],
        LibraryElementKind::FunctionBlockDeclaration(_)
    ));
    assert!(matches!(
        &written[3],
        LibraryElementKind::InterfaceDeclaration(_)
    ));
    assert!(matches!(
        &written[4],
        LibraryElementKind::NamespaceDeclaration(_)
    ));
}

#[test]
fn lower_library_when_nesting_of_namespaces_is_as_deep_as_the_tree_allows_then_lowered() {
    let depth = 40;
    let source = format!(
        "{}{}",
        "NAMESPACE n ".repeat(depth),
        "END_NAMESPACE ".repeat(depth)
    );
    assert_eq!(elements(&source).len(), 1);
}
