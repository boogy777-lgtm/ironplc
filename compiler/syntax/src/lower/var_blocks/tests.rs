//! Tests of the variable blocks: a row of `BLOCKS`, `QUALIFIERS`, `EDGES` and
//! `INSTANCES` for each thing a block may say, and what a block makes of what it
//! declares.

use super::*;
use crate::lower::{disposition, lower_library, Area, Disposition, INTERNAL_ERROR};
use crate::{parse_source_file, ParseOptions};
use ironplc_dsl::common::{
    AddressAssignment, ArrayBounds, ConstantKind, InitialValueAssignmentKind, LibraryElementKind,
    LocationPrefix, SignedIntegerRef, SizePrefix, SpecificationKind, StringType,
};
use ironplc_dsl::core::{FileId, Located};
use ironplc_dsl::textual::SymbolicVariableKind;
use ironplc_problems::Problem;

fn file() -> FileId {
    FileId::from_string("t.st")
}

/// Declarations that hold blocks, with the place for them marked `{}`.
const PROGRAM: &str = "PROGRAM p {} END_PROGRAM";
const FUNCTION: &str = "FUNCTION f : INT {} f := 1; END_FUNCTION";
const FUNCTION_BLOCK: &str = "FUNCTION_BLOCK f {} END_FUNCTION_BLOCK";
const METHOD: &str = "FUNCTION_BLOCK f METHOD m {} END_METHOD END_FUNCTION_BLOCK";
const FILE: &str = "{}";
const CONFIGURATION: &str =
    "CONFIGURATION c RESOURCE r ON t PROGRAM p : q; END_RESOURCE {} END_CONFIGURATION";

fn parsed(wrapper: &str, text: &str) -> crate::Parse {
    let source = wrapper.replace("{}", text);
    let parse = parse_source_file(&source, &ParseOptions::all());
    assert!(parse.is_ok(), "{source}: {:?}", parse.errors);
    parse
}

/// The result of lowering each block in `text` as written in `wrapper`.
fn lower_all(wrapper: &str, text: &str) -> Vec<Result<Block, Diagnostic>> {
    let parse = parsed(wrapper, text);
    let cx = LowerCx::new(file());
    parse
        .root
        .descendants()
        .filter(|node| node.kind() == K::VarBlock)
        .map(|block| lower_var_block(&cx, &block))
        .collect()
}

/// The blocks in `text` as written in `wrapper`.
fn blocks(wrapper: &str, text: &str) -> Vec<Block> {
    lower_all(wrapper, text)
        .into_iter()
        .map(|block| block.expect("the block lowers"))
        .collect()
}

fn block(wrapper: &str, text: &str) -> Block {
    blocks(wrapper, text).remove(0)
}

/// The variables of the one block `text`, written in a program.
fn variables(text: &str) -> Vec<VarDecl> {
    block(PROGRAM, text).variables
}

fn name_of(variable: &VarDecl) -> String {
    variable
        .identifier
        .symbolic_id()
        .map(|id| id.original().to_string())
        .unwrap_or_default()
}

fn address_of(variable: &VarDecl) -> Option<&AddressAssignment> {
    match &variable.identifier {
        VariableIdentifier::Direct(direct) => Some(&direct.address_assignment),
        VariableIdentifier::Symbol(_) => None,
    }
}

macro_rules! variant {
    ($value:expr, $pattern:path) => {
        match $value {
            $pattern(inner) => Some(inner),
            _ => None,
        }
        .expect(concat!("a ", stringify!($pattern)))
    };
}

// ---- The tables.

#[test]
fn lower_var_block_when_each_block_keyword_then_the_kind_of_variable_it_declares() {
    let rows = [
        (FUNCTION_BLOCK, "VAR", VariableType::Var),
        (FUNCTION_BLOCK, "VAR_INPUT", VariableType::Input),
        (FUNCTION_BLOCK, "VAR_OUTPUT", VariableType::Output),
        (FUNCTION_BLOCK, "VAR_IN_OUT", VariableType::InOut),
        (FUNCTION_BLOCK, "VAR_TEMP", VariableType::VarTemp),
        (FUNCTION_BLOCK, "VAR_EXTERNAL", VariableType::External),
        (FILE, "VAR_GLOBAL", VariableType::Global),
        (FUNCTION_BLOCK, "VAR_STAT", VariableType::VarStat),
        (METHOD, "VAR_INST", VariableType::VarInst),
        (FUNCTION_BLOCK, "VAR_GENERIC", VariableType::VarGeneric),
    ];
    for (wrapper, keyword, var_type) in &rows {
        let lowered = block(wrapper, &format!("{keyword} a : INT; END_VAR"));
        assert_eq!(lowered.variables.len(), 1, "{keyword}");
        assert_eq!(&lowered.variables[0].var_type, var_type, "{keyword}");
    }
    // A block keyword added to the table is a row here as well.
    let declaring = BLOCKS
        .iter()
        .filter(|(_, items)| matches!(items, Declarations(_)))
        .count();
    assert_eq!(declaring, rows.len());
}

#[test]
fn lower_var_block_when_every_block_keyword_then_the_table_has_a_row() {
    let openers = [
        K::Var,
        K::VarInput,
        K::VarOutput,
        K::VarInOut,
        K::VarTemp,
        K::VarExternal,
        K::VarAccess,
        K::VarConfig,
        K::VarGlobal,
        K::VarStat,
        K::VarInst,
        K::VarGeneric,
    ];
    assert_eq!(BLOCKS.len(), openers.len());
    for opener in openers {
        assert!(BLOCKS.iter().any(|(kind, _)| *kind == opener), "{opener:?}");
    }
}

#[test]
fn lower_var_block_when_each_qualifier_then_every_declaration_of_the_block_takes_it() {
    let rows = [
        ("CONSTANT", DeclarationQualifier::Constant),
        ("RETAIN", DeclarationQualifier::Retain),
        ("NON_RETAIN", DeclarationQualifier::NonRetain),
        ("PERSISTENT", DeclarationQualifier::Persistent),
    ];
    for (word, qualifier) in &rows {
        let lowered = variables(&format!("VAR {word} a : INT; b : BOOL; END_VAR"));
        assert_eq!(lowered.len(), 2, "{word}");
        for variable in &lowered {
            assert_eq!(&variable.qualifier, qualifier, "{word}");
        }
    }
    assert_eq!(rows.len(), QUALIFIERS.len());
    for variable in variables("VAR a : INT; END_VAR") {
        assert_eq!(variable.qualifier, DeclarationQualifier::Unspecified);
    }
}

#[test]
fn lower_var_block_when_qualifier_on_a_block_with_edges_then_the_edges_take_it_too() {
    let lowered = block(
        PROGRAM,
        "VAR_INPUT RETAIN a : INT; b : BOOL R_EDGE; END_VAR",
    );
    assert_eq!(lowered.variables[0].qualifier, DeclarationQualifier::Retain);
    assert_eq!(lowered.edges[0].qualifier, DeclarationQualifier::Retain);
}

#[test]
fn lower_var_block_when_each_edge_then_one_edge_variable_for_each_name_and_no_variable() {
    let lowered = block(
        PROGRAM,
        "VAR_INPUT a, b : BOOL R_EDGE; c : BOOL F_EDGE; END_VAR",
    );
    assert!(lowered.variables.is_empty());
    let edges: Vec<(String, EdgeDirection)> = lowered
        .edges
        .iter()
        .map(|edge| {
            (
                edge.identifier.original().to_string(),
                edge.direction.clone(),
            )
        })
        .collect();
    assert_eq!(
        edges,
        vec![
            ("a".to_string(), EdgeDirection::Rising),
            ("b".to_string(), EdgeDirection::Rising),
            ("c".to_string(), EdgeDirection::Falling),
        ]
    );
    assert_eq!(EDGES.len(), 2);
}

#[test]
fn lower_var_block_when_edges_beside_variables_then_each_in_its_list_and_the_variables_share_a_block(
) {
    let lowered = block(
        PROGRAM,
        "VAR_INPUT a : INT; e : BOOL R_EDGE; b : INT; END_VAR",
    );
    assert_eq!(
        lowered.variables.iter().map(name_of).collect::<Vec<_>>(),
        vec!["a", "b"]
    );
    assert_eq!(lowered.edges.len(), 1);
    assert_eq!(lowered.variables[0].block, lowered.variables[1].block);
}

// ---- What a block declares.

#[test]
fn lower_var_block_when_several_names_then_a_variable_for_each_with_one_type_and_one_block() {
    let lowered = variables("VAR a, b : INT := 5; c : BOOL; END_VAR");
    assert_eq!(
        lowered.iter().map(name_of).collect::<Vec<_>>(),
        vec!["a", "b", "c"]
    );
    assert_eq!(lowered[0].initializer, lowered[1].initializer);
    assert_ne!(lowered[0].initializer, lowered[2].initializer);
    assert!(lowered
        .iter()
        .all(|variable| variable.block == lowered[0].block));
    assert!(lowered
        .iter()
        .all(|variable| variable.var_type == VariableType::Var));
}

#[test]
fn lower_var_block_when_two_blocks_then_the_declarations_of_each_share_a_block_and_the_blocks_differ(
) {
    let two = blocks(PROGRAM, "VAR a, b : INT; END_VAR VAR c : INT; END_VAR");
    let (first, second) = (&two[0].variables, &two[1].variables);
    assert_eq!(first[0].block, first[1].block);
    assert_ne!(first[0].block, second[0].block);
    // Blocks of different kinds are blocks as well.
    let kinds = blocks(
        FUNCTION_BLOCK,
        "VAR_INPUT a : INT; END_VAR VAR_OUTPUT b : INT; END_VAR",
    );
    assert_ne!(kinds[0].variables[0].block, kinds[1].variables[0].block);
}

#[test]
fn lower_var_block_when_no_declaration_then_nothing_is_declared() {
    for text in ["VAR END_VAR", "VAR ; END_VAR"] {
        let lowered = block(PROGRAM, text);
        assert_eq!(lowered, Block::default(), "{text}");
    }
}

#[test]
fn lower_var_block_when_declarations_then_in_the_order_written_whatever_they_declare() {
    let lowered = variables("VAR a : INT; b AT %IX0.0 : BOOL; c : INT; d AT %I* : INT; END_VAR");
    assert_eq!(
        lowered.iter().map(name_of).collect::<Vec<_>>(),
        vec!["a", "b", "c", "d"]
    );
}

#[test]
fn lower_var_block_when_variable_then_positioned_at_its_name() {
    let lowered = variables("VAR  ab : INT; END_VAR");
    let span = lowered[0].identifier.span();
    assert_eq!((span.start, span.end), (15, 17));
    assert_eq!(span.file_id, file());
}

// ---- Locations.

#[test]
fn lower_var_block_when_complete_location_then_a_direct_variable_that_shares_the_block() {
    let lowered = variables("VAR a : INT; b AT %IX0.1 : BOOL; END_VAR");
    let address = address_of(&lowered[1]).expect("a located variable");
    assert_eq!(address.location, LocationPrefix::I);
    assert_eq!(address.size, SizePrefix::X);
    assert_eq!(address.address, vec![0, 1]);
    assert_eq!(name_of(&lowered[1]), "b");
    // Which of the declarations of one block are located is known from the
    // block they share.
    assert_eq!(lowered[0].block, lowered[1].block);
    assert_eq!(
        ironplc_dsl::common::mixed_located_var_decls(&lowered).count(),
        1
    );
}

#[test]
fn lower_var_block_when_incomplete_location_then_the_address_is_left_to_the_program() {
    let lowered = variables("VAR a AT %Q* : INT; END_VAR");
    let address = address_of(&lowered[0]).expect("a located variable");
    assert_eq!(address.location, LocationPrefix::Q);
    assert_eq!(address.size, SizePrefix::Unspecified);
    assert!(address.address.is_empty());
}

#[test]
fn lower_var_block_when_location_then_positioned_at_the_address_alone() {
    let lowered = variables("VAR a AT %MW3 : WORD; END_VAR");
    let address = address_of(&lowered[0]).expect("a located variable");
    assert_eq!(
        (address.position.start, address.position.end),
        (19, 23),
        "the `AT` is not part of it"
    );
}

#[test]
fn lower_var_block_when_global_with_name_and_location_then_a_located_variable_with_the_name() {
    let lowered = block(FILE, "VAR_GLOBAL g AT %MW0 : INT; END_VAR").variables;
    assert_eq!(name_of(&lowered[0]), "g");
    let address = address_of(&lowered[0]).expect("a located variable");
    assert_eq!(address.location, LocationPrefix::M);
    assert_eq!(address.size, SizePrefix::W);
    assert_eq!(lowered[0].var_type, VariableType::Global);
}

#[test]
fn lower_var_block_when_global_with_location_only_then_a_located_variable_without_a_name() {
    let lowered = block(FILE, "VAR_GLOBAL AT %MW1 : INT; END_VAR").variables;
    let variable = &lowered[0];
    let direct = variant!(&variable.identifier, VariableIdentifier::Direct);
    assert!(direct.name.is_none());
    assert_eq!(direct.address_assignment.address, vec![1]);
}

#[test]
fn lower_var_block_when_global_stops_after_the_colon_then_no_type_and_no_value_at_the_colon() {
    let lowered = block(FILE, "VAR_GLOBAL g :; END_VAR").variables;
    let span = variant!(&lowered[0].initializer, InitialValueAssignmentKind::None);
    assert_eq!((span.start, span.end), (13, 14));
}

// ---- Types and values.

#[test]
fn lower_var_block_when_a_type_and_a_value_then_what_the_table_of_initial_values_builds() {
    let lowered = variables(
        "VAR a : INT := 5; s : STRING[4] := 'ab'; t : my_type; r : REF_TO INT := NULL; END_VAR",
    );
    let simple = variant!(&lowered[0].initializer, InitialValueAssignmentKind::Simple);
    assert!(matches!(
        simple.initial_value,
        Some(ConstantKind::IntegerLiteral(_))
    ));
    let string = variant!(&lowered[1].initializer, InitialValueAssignmentKind::String);
    assert_eq!(string.width, StringType::String);
    assert!(string.initial_value.is_some());
    assert!(matches!(
        lowered[2].initializer,
        InitialValueAssignmentKind::LateResolvedType(_)
    ));
    assert!(matches!(
        lowered[3].initializer,
        InitialValueAssignmentKind::Reference(_)
    ));
}

#[test]
fn lower_var_block_when_value_does_not_fit_the_type_then_initializer_mismatch_at_the_value() {
    let results = lower_all(FILE, "VAR_GLOBAL s : STRING := 5; END_VAR");
    let error = results[0].as_ref().expect_err("a mismatch");
    assert_eq!(error.code, Problem::InitializerTypeMismatch.code());
    assert_eq!(
        (error.primary.location.start, error.primary.location.end),
        (25, 26)
    );
}

#[test]
fn lower_var_block_when_inline_subrange_in_any_block_then_a_subrange_with_its_default() {
    let texts = [
        (PROGRAM, "VAR x : INT(3..9) := 5; END_VAR"),
        (FUNCTION, "VAR_TEMP x : INT(3..9) := 5; END_VAR"),
        (FUNCTION_BLOCK, "VAR_OUTPUT x : INT(3..9) := 5; END_VAR"),
        (FUNCTION_BLOCK, "VAR_STAT x : INT(3..9) := 5; END_VAR"),
        (FILE, "VAR_GLOBAL x : INT(3..9) := 5; END_VAR"),
        (PROGRAM, "VAR x AT %MW0 : INT(3..9) := 5; END_VAR"),
        (PROGRAM, "VAR x AT %M* : INT(3..9) := 5; END_VAR"),
    ];
    for (wrapper, text) in texts {
        let lowered = block(wrapper, text).variables;
        let subrange = variant!(
            &lowered[0].initializer,
            InitialValueAssignmentKind::Subrange
        );
        let spec = variant!(&subrange.spec, SpecificationKind::Inline);
        assert_eq!(spec.type_name.to_string(), "INT", "{text}");
        assert!(matches!(spec.subrange.start, SignedIntegerRef::Literal(_)));
        assert_eq!(
            subrange
                .initial_value
                .as_ref()
                .map(|value| value.value.value),
            Some(5),
            "{text}"
        );
    }
}

#[test]
fn lower_var_block_when_external_subrange_then_a_subrange_that_takes_no_value() {
    let lowered = block(PROGRAM, "VAR_EXTERNAL x : INT(3..9); END_VAR").variables;
    let subrange = variant!(
        &lowered[0].initializer,
        InitialValueAssignmentKind::Subrange
    );
    assert!(subrange.initial_value.is_none());
}

#[test]
fn parse_when_subrange_value_is_an_expression_then_a_syntax_error() {
    // The object holds a subrange's value as an integer, here as in a `TYPE`.
    for text in [
        "PROGRAM p VAR x : INT(3..9) := n + 1; END_VAR END_PROGRAM",
        "VAR_GLOBAL x : INT(3..9) := n; END_VAR",
    ] {
        let parse = parse_source_file(text, &ParseOptions::all());
        assert!(!parse.is_ok(), "{text}");
    }
}

#[test]
fn lower_var_block_when_array_variable_then_array_bounds_and_values() {
    let lowered = variables("VAR a : ARRAY[1..3] OF INT := [1, 2, 3]; END_VAR");
    let array = variant!(&lowered[0].initializer, InitialValueAssignmentKind::Array);
    let spec = variant!(&array.spec, SpecificationKind::Inline);
    assert!(matches!(&spec.bounds, ArrayBounds::Ranges(ranges) if ranges.len() == 1));
    assert_eq!(array.initial_values.len(), 3);
}

// ---- The other forms of block.

#[test]
fn lower_var_block_when_access_paths_then_name_path_type_and_direction() {
    let lowered = block(
        PROGRAM,
        "VAR_ACCESS a : p.x : INT READ_ONLY; b : y : BOOL READ_WRITE; c : z : INT; END_VAR",
    );
    assert!(lowered.variables.is_empty());
    let access = &lowered.access;
    assert_eq!(access.len(), 3);
    assert_eq!(access[0].access_name.original(), "a");
    assert!(matches!(
        access[0].symbolic_variable,
        SymbolicVariableKind::Structured(_)
    ));
    assert_eq!(access[0].type_name.to_string(), "INT");
    let directions: Vec<Option<Direction>> =
        access.iter().map(|decl| decl.direction.clone()).collect();
    assert_eq!(
        directions,
        vec![Some(Direction::ReadOnly), Some(Direction::ReadWrite), None]
    );
    assert!(matches!(
        access[1].symbolic_variable,
        SymbolicVariableKind::Named(_)
    ));
    assert_eq!(DIRECTIONS.len(), 2);
}

#[test]
fn lower_var_block_when_instance_of_a_function_block_then_its_path_type_and_member_values() {
    let lowered = block(
        CONFIGURATION,
        "VAR_CONFIG r.p.fb : Counter := (start := 1, limit := 9); END_VAR",
    );
    let init = variant!(&lowered.instances[0], InstanceInit::FunctionBlock);
    assert_eq!(init.resource_name.original(), "r");
    assert_eq!(init.program_name.original(), "p");
    assert_eq!(
        init.fb_path
            .iter()
            .map(|id| id.original().to_string())
            .collect::<Vec<_>>(),
        vec!["fb"]
    );
    assert_eq!(init.type_name.to_string(), "Counter");
    assert_eq!(init.initializer.len(), 2);
}

#[test]
fn lower_var_block_when_instance_variable_then_its_path_address_and_value() {
    let lowered = block(
        CONFIGURATION,
        "VAR_CONFIG r.p.fb.x AT %QX0.0 : BOOL := TRUE; r.p.arr : ARRAY[1..2] OF INT := [1, 2]; r.p.v : INT; END_VAR",
    );
    assert_eq!(lowered.instances.len(), 3);
    let first = variant!(&lowered.instances[0], InstanceInit::Located);
    assert_eq!(
        first
            .fb_path
            .iter()
            .map(|id| id.original().to_string())
            .collect::<Vec<_>>(),
        vec!["fb", "x"]
    );
    let address = first.address.as_ref().expect("an address");
    assert_eq!(address.location, LocationPrefix::Q);
    assert!(matches!(
        first.initializer,
        InitialValueAssignmentKind::Simple(_)
    ));
    let second = variant!(&lowered.instances[1], InstanceInit::Located);
    assert!(second.address.is_none());
    assert!(matches!(
        second.initializer,
        InitialValueAssignmentKind::Array(_)
    ));
    let third = variant!(&lowered.instances[2], InstanceInit::Located);
    assert!(matches!(
        third.initializer,
        InitialValueAssignmentKind::Simple(_)
    ));
}

#[test]
fn lower_var_block_when_instance_value_does_not_fit_then_mismatch() {
    let results = lower_all(CONFIGURATION, "VAR_CONFIG r.p.x : STRING := 5; END_VAR");
    let error = results[0].as_ref().expect_err("a mismatch");
    assert_eq!(error.code, Problem::InitializerTypeMismatch.code());
}

#[test]
fn lower_var_block_when_instance_string_or_subrange_then_the_value_the_type_builds() {
    let lowered = block(
        CONFIGURATION,
        "VAR_CONFIG r.p.s : STRING := 'ab'; r.p.n : INT(1..5) := 2; END_VAR",
    );
    let string = variant!(&lowered.instances[0], InstanceInit::Located);
    assert!(matches!(
        string.initializer,
        InitialValueAssignmentKind::String(_)
    ));
    let subrange = variant!(&lowered.instances[1], InstanceInit::Located);
    assert!(matches!(
        subrange.initializer,
        InitialValueAssignmentKind::Subrange(_)
    ));
}

#[test]
fn lower_var_block_when_instance_path_is_not_a_name_path_then_syntax_error() {
    let parse = parse_source_file(
        "CONFIGURATION c RESOURCE r ON t PROGRAM p : q; END_RESOURCE VAR_CONFIG r.p.a[1] : INT; END_VAR END_CONFIGURATION",
        &ParseOptions::all(),
    );
    assert!(parse.is_ok(), "{:?}", parse.errors);
    let cx = LowerCx::new(file());
    let node = parse
        .root
        .descendants()
        .find(|node| node.kind() == K::VarBlock)
        .expect("a block");
    let error = lower_var_block(&cx, &node).expect_err("not a path of names");
    assert_eq!(error.code, Problem::SyntaxError.code());
}

// ---- Entry points and the disposition of the kinds.

#[test]
fn lower_var_block_when_not_a_block_then_internal_error() {
    let parse = parsed(PROGRAM, "VAR a : INT; END_VAR");
    let cx = LowerCx::new(file());
    let declaration = parse
        .root
        .descendants()
        .find(|node| node.kind() == K::VarDecl)
        .expect("a declaration");
    let error = lower_var_block(&cx, &declaration).expect_err("not a block");
    assert_eq!(error.code, INTERNAL_ERROR);
}

#[test]
fn lower_library_when_global_blocks_at_the_top_then_one_element_for_each_in_order() {
    let source =
        "TYPE t : INT; END_TYPE\nVAR_GLOBAL a, b : INT; END_VAR\nVAR_GLOBAL CONSTANT c : INT := 1; END_VAR\n";
    let parse = parse_source_file(source, &ParseOptions::all());
    let library = lower_library(&parse, &file()).expect("the library lowers");
    assert_eq!(library.elements.len(), 3);
    let first = variant!(
        &library.elements[1],
        LibraryElementKind::GlobalVarDeclarations
    );
    assert_eq!(
        first.iter().map(name_of).collect::<Vec<_>>(),
        vec!["a", "b"]
    );
    let second = variant!(
        &library.elements[2],
        LibraryElementKind::GlobalVarDeclarations
    );
    assert_eq!(second[0].qualifier, DeclarationQualifier::Constant);
    assert_ne!(first[0].block, second[0].block);
}

#[test]
fn lower_library_when_block_in_a_namespace_or_pou_then_not_implemented_there() {
    let parse = parse_source_file(
        "PROGRAM p VAR a : INT; END_VAR END_PROGRAM",
        &ParseOptions::all(),
    );
    let error = lower_library(&parse, &file()).expect_err("no rule for the program yet");
    assert_eq!(error.code, crate::lower::NOT_IMPLEMENTED);
}

#[test]
fn disposition_when_variable_block_nodes_then_lowered_by_the_block_area_and_their_parts_structural()
{
    for kind in [K::VarBlock, K::VarDecl, K::AccessDecl, K::InstanceInit] {
        assert_eq!(
            disposition(kind),
            Disposition::Lowered(Area::Block),
            "{kind:?}"
        );
    }
    for kind in [K::Location, K::EdgeSpec] {
        assert_eq!(disposition(kind), Disposition::Structural, "{kind:?}");
    }
}
