//! The analysis of the variable blocks that the lowering of the syntax tree
//! builds.
//!
//! The lowering is not the production parse path yet, so the variables a block
//! lowers to are placed in a library the legacy parser built, in place of the
//! variables of its program or of its global block, and the whole library is
//! analysed. What matters is that the analysis answers every shape a block
//! lowers to with a result or a diagnostic and never with a panic: the shapes
//! that differ from the legacy parser's are a located global with a name, one
//! without, a global or external variable of a named type or of a string, and
//! a subrange written in place in any block.

use crate::stages::analyze;
use ironplc_dsl::common::{InitialValueAssignmentKind, Library, LibraryElementKind, VarDecl};
use ironplc_dsl::core::FileId;
use ironplc_parser::options::{CompilerOptions, Dialect};
use ironplc_parser::parse_program;
use ironplc_syntax::lower::var_blocks::lower_var_block;
use ironplc_syntax::lower::LowerCx;
use ironplc_syntax::{parse_source_file, ParseOptions, SyntaxKind};

/// The variables that the block `text` lowers to, written in a program or at
/// the top of a file.
fn lowered(text: &str) -> Vec<VarDecl> {
    let global = text.starts_with("VAR_GLOBAL");
    let source = if global {
        text.to_string()
    } else {
        format!("PROGRAM p {text} END_PROGRAM")
    };
    let parse = parse_source_file(&source, &ParseOptions::all());
    assert!(parse.is_ok(), "{text}: {:?}", parse.errors);
    let block = parse
        .root
        .descendants()
        .find(|node| node.kind() == SyntaxKind::VarBlock)
        .expect("a block");
    lower_var_block(&LowerCx::new(FileId::default()), &block)
        .expect("the block lowers")
        .variables
}

/// The legacy parser's library for `source`, with the variables of the first
/// global block (when `global`) or of the program `p` replaced by `variables`.
fn library_with(source: &str, global: bool, variables: Vec<VarDecl>) -> Library {
    let mut library = parse_program(
        source,
        &FileId::default(),
        &CompilerOptions::from_dialect(Dialect::Rusty),
    )
    .expect("the template parses");
    for element in library.elements.iter_mut() {
        match element {
            LibraryElementKind::GlobalVarDeclarations(declared) if global => {
                *declared = variables.clone();
            }
            LibraryElementKind::ProgramDeclaration(program) if !global => {
                program.variables = variables.clone();
            }
            _ => {}
        }
    }
    library
}

/// The codes of the diagnostics the analysis reports for the block.
fn analyse(source: &str, block: &str) -> Vec<String> {
    let library = library_with(source, block.starts_with("VAR_GLOBAL"), lowered(block));
    let options = CompilerOptions::from_dialect(Dialect::Rusty);
    let (_, context) = analyze(&[&library], &options).expect("the analysis completes");
    context
        .diagnostics()
        .iter()
        .map(|diagnostic| diagnostic.code.clone())
        .collect()
}

const WITH_GLOBAL: &str =
    "VAR_GLOBAL g : INT; END_VAR PROGRAM p VAR x : INT; END_VAR x := g; END_PROGRAM";
const PROGRAM: &str = "PROGRAM p VAR x : INT; END_VAR END_PROGRAM";

#[test]
fn analyze_when_subrange_written_in_place_then_no_diagnostic() {
    for block in [
        "VAR x : INT(3..9) := 5; END_VAR",
        "VAR x : INT(3..9); END_VAR",
        "VAR x : UINT(0..10) := 1; y : INT(-5..5); END_VAR",
    ] {
        let codes = analyse(PROGRAM, block);
        assert!(codes.is_empty(), "{block}: {codes:?}");
    }
}

#[test]
fn analyze_when_global_block_then_every_shape_it_lowers_to_is_answered() {
    for block in [
        "VAR_GLOBAL g : INT; END_VAR",
        "VAR_GLOBAL g, h : INT := 5; END_VAR",
        "VAR_GLOBAL g AT %MW0 : INT; END_VAR",
        "VAR_GLOBAL g AT %I* : INT; END_VAR",
        "VAR_GLOBAL AT %MW1 : INT; END_VAR",
        "VAR_GLOBAL g :; END_VAR",
        "VAR_GLOBAL g : STRING := 'ab'; END_VAR",
        "VAR_GLOBAL g : MyType; END_VAR",
        "VAR_GLOBAL g : INT(0..5); END_VAR",
        "VAR_GLOBAL g : ARRAY[1..2] OF INT := [1, 2]; END_VAR",
        "VAR_GLOBAL CONSTANT g : INT := 5; END_VAR",
        "VAR_GLOBAL RETAIN g : INT; END_VAR",
        "VAR_GLOBAL PERSISTENT g : INT; END_VAR",
    ] {
        // The analysis completes, with diagnostics or without.
        let _ = analyse(WITH_GLOBAL, block);
    }
}

#[test]
fn analyze_when_program_block_then_every_shape_it_lowers_to_is_answered() {
    for block in [
        "VAR_EXTERNAL g : INT; END_VAR",
        "VAR_EXTERNAL g : MyType; END_VAR",
        "VAR_EXTERNAL g : STRING; END_VAR",
        "VAR_EXTERNAL CONSTANT g : INT; END_VAR",
        "VAR_EXTERNAL g : INT(0..5); END_VAR",
        "VAR_EXTERNAL g : ARRAY[1..2] OF INT; END_VAR",
        "VAR_IN_OUT g : INT; END_VAR",
        "VAR_IN_OUT g : MyType; END_VAR",
        "VAR_IN_OUT g : INT(0..5); END_VAR",
        "VAR_INPUT a : INT; END_VAR",
        "VAR_OUTPUT RETAIN a : INT; END_VAR",
        "VAR a : INT; b AT %IX0.0 : BOOL; c AT %I* : INT; END_VAR",
        "VAR_STAT s : INT := 1; END_VAR",
        "VAR_STAT CONSTANT s : INT := 1; END_VAR",
    ] {
        let _ = analyse(WITH_GLOBAL, block);
    }
}

#[test]
fn analyze_when_mixed_block_in_a_program_then_the_located_variable_is_reported() {
    let codes = analyse(PROGRAM, "VAR a : INT; b AT %IX0.0 : BOOL; END_VAR");
    // The rule reads the block of each declaration, which the lowering
    // shares between the declarations of one block.
    let strict = {
        let library = library_with(
            PROGRAM,
            false,
            lowered("VAR a : INT; b AT %IX0.0 : BOOL; END_VAR"),
        );
        let options = CompilerOptions::from_dialect(Dialect::Iec61131_3Ed2);
        let (_, context) = analyze(&[&library], &options).expect("the analysis completes");
        context
            .diagnostics()
            .iter()
            .map(|diagnostic| diagnostic.code.clone())
            .collect::<Vec<_>>()
    };
    assert!(strict.iter().any(|code| code == "P4036"), "{strict:?}");
    assert!(!codes.iter().any(|code| code == "P4036"), "{codes:?}");
}

#[test]
fn analyze_when_lowered_initializer_is_none_then_not_a_panic() {
    let none = InitialValueAssignmentKind::None(Default::default());
    let mut variables = lowered("VAR_GLOBAL g :; END_VAR");
    assert_eq!(variables[0].initializer, none);
    variables[0].initializer = none;
    let library = library_with(WITH_GLOBAL, true, variables);
    let options = CompilerOptions::from_dialect(Dialect::Rusty);
    assert!(analyze(&[&library], &options).is_ok());
}
