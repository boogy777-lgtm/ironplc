//! The analysis of an initial value that the lowering of the syntax tree
//! builds.
//!
//! The lowering is not the production parse path yet, so a declaration it
//! lowers is placed in a library the legacy parser built (with a placeholder
//! initial value) and the whole library is analysed. What matters is that the
//! analysis answers every lowered shape with a value or a diagnostic and never
//! with a panic, whatever the legacy grammar would have accepted.

use crate::stages::analyze;
use ironplc_dsl::common::{
    ConstantKind, InitialValueAssignmentKind, Library, LibraryElementKind, StringType, VarDecl,
};
use ironplc_dsl::core::FileId;
use ironplc_parser::options::{CompilerOptions, Dialect};
use ironplc_parser::parse_program;
use ironplc_syntax::lower::initializers::lower_initial_value;
use ironplc_syntax::lower::LowerCx;
use ironplc_syntax::{parse_source_file, ParseOptions, SyntaxKind};

/// What `x : <text>;` in a variable block makes of its type and value, as the
/// lowering of the tree builds it.
fn lowered(text: &str) -> InitialValueAssignmentKind {
    let source = format!("PROGRAM p VAR x : {text}; END_VAR END_PROGRAM");
    let parse = parse_source_file(&source, &ParseOptions::all());
    assert!(parse.is_ok(), "{text}: {:?}", parse.errors);
    let declaration = parse
        .root
        .descendants()
        .find(|node| node.kind() == SyntaxKind::VarDecl)
        .expect("a declaration");
    lower_initial_value(&LowerCx::new(FileId::default()), &declaration)
        .expect("the declaration lowers")
}

/// The legacy parser's library for `source`, whose program `p` declares `x :
/// INT` first, with the initial value of `x` replaced by `initial`.
fn library_with(source: &str, initial: InitialValueAssignmentKind) -> Library {
    let mut library = parse_program(source, &FileId::default(), &CompilerOptions::default())
        .expect("the template parses");
    let variable = library
        .elements
        .iter_mut()
        .find_map(|element| match element {
            LibraryElementKind::ProgramDeclaration(program) => program.variables.first_mut(),
            _ => None,
        })
        .expect("a variable");
    variable.initializer = initial;
    library
}

/// The codes of the diagnostics the analysis reports, and the initial value of
/// `x` after it.
fn analyse(
    source: &str,
    initial: InitialValueAssignmentKind,
    dialect: Dialect,
) -> (Vec<String>, Option<VarDecl>) {
    let library = library_with(source, initial);
    let options = CompilerOptions::from_dialect(dialect);
    let (analysed, context) = analyze(&[&library], &options).expect("the analysis completes");
    let codes = context
        .diagnostics()
        .iter()
        .map(|diagnostic| diagnostic.code.clone())
        .collect();
    let variable = analysed.elements.iter().find_map(|element| match element {
        LibraryElementKind::ProgramDeclaration(program) => program.variables.first().cloned(),
        _ => None,
    });
    (codes, variable)
}

const CONSTANT: &str =
    "VAR_GLOBAL CONSTANT limit : INT := 5; END_VAR PROGRAM p VAR x : INT; END_VAR END_PROGRAM";
const VARIABLE: &str = "VAR_GLOBAL g : INT; END_VAR PROGRAM p VAR x : INT; END_VAR END_PROGRAM";
const NOTHING: &str = "PROGRAM p VAR x : INT; END_VAR END_PROGRAM";

#[test]
fn analyze_when_elementary_variable_initialised_with_a_named_constant_then_its_value() {
    let (codes, variable) = analyse(CONSTANT, lowered("INT := limit"), Dialect::Rusty);
    assert!(codes.is_empty(), "{codes:?}");
    let initial = variable.map(|variable| variable.initializer);
    let value = match initial {
        Some(InitialValueAssignmentKind::Simple(simple)) => simple.initial_value,
        _ => None,
    };
    let value = match value {
        Some(ConstantKind::IntegerLiteral(literal)) => Some(literal.value.value.value),
        _ => None,
    };
    assert_eq!(value, Some(5));
}

#[test]
fn analyze_when_elementary_variable_initialised_with_a_name_that_is_not_constant_then_a_diagnostic()
{
    for source in [VARIABLE, NOTHING] {
        let (codes, _) = analyse(source, lowered("INT := other"), Dialect::Rusty);
        assert!(
            codes.iter().any(|code| code == "P4038"),
            "{source}: {codes:?}"
        );
    }
    let (codes, _) = analyse(VARIABLE, lowered("INT := g"), Dialect::Rusty);
    assert!(codes.iter().any(|code| code == "P4038"), "{codes:?}");
}

#[test]
fn analyze_when_dialect_disallows_constant_expressions_then_the_named_initial_value_is_reported() {
    let (codes, _) = analyse(CONSTANT, lowered("INT := limit"), Dialect::Iec61131_3Ed2);
    assert!(codes.iter().any(|code| code == "P4037"), "{codes:?}");
}

#[test]
fn analyze_when_expression_initial_value_then_a_value_or_a_diagnostic() {
    let (codes, variable) = analyse(CONSTANT, lowered("INT := limit * 2 + 1"), Dialect::Rusty);
    assert!(codes.is_empty(), "{codes:?}");
    let simple = match variable.map(|variable| variable.initializer) {
        Some(InitialValueAssignmentKind::Simple(simple)) => simple.initial_value,
        _ => None,
    };
    assert!(matches!(simple, Some(ConstantKind::IntegerLiteral(_))));
    let (codes, _) = analyse(NOTHING, lowered("INT := 1 / 0"), Dialect::Rusty);
    assert!(!codes.is_empty());
}

#[test]
fn analyze_when_string_value_has_the_other_delimiter_then_it_is_a_value_of_the_declared_width() {
    let template = "PROGRAM p VAR x : INT; END_VAR END_PROGRAM";
    let (codes, variable) = analyse(template, lowered("STRING[10] := \"ab\""), Dialect::Rusty);
    assert!(codes.is_empty(), "{codes:?}");
    let initial = match variable.map(|variable| variable.initializer) {
        Some(InitialValueAssignmentKind::String(initial)) => initial.initial_value,
        _ => None,
    };
    assert_eq!(
        initial.map(|value| (value.width, value.value)),
        Some((StringType::String, vec!['a', 'b']))
    );
}

#[test]
fn analyze_when_string_value_does_not_fit_the_declared_width_then_a_diagnostic() {
    // A character that a string of single-byte characters cannot hold.
    let template = "PROGRAM p VAR x : INT; END_VAR END_PROGRAM";
    let (codes, _) = analyse(
        template,
        lowered("STRING[10] := \"\u{65e5}\""),
        Dialect::Rusty,
    );
    assert!(codes.iter().any(|code| code == "P4052"), "{codes:?}");
}

#[test]
fn analyze_when_initial_value_is_any_expression_then_a_value_or_a_diagnostic_and_never_a_panic() {
    for text in [
        "INT := NULL",
        "INT := a.b",
        "INT := a[1]",
        "INT := p^",
        "INT := f(1)",
        "INT := -name",
        "INT := name + other",
        "INT := name = other",
        "INT := NOT name",
        "BOOL := name",
        "TIME := name",
        "STRING[5] := 'a'",
        "REAL := limit / 2.0",
        "MyType := name",
        "MyType := a.b",
        "MyType := 1 + 2",
        "MyArray := [1, 2]",
        "(A, B) := A",
        "(A, B) := T#B",
        "MyType := (a := 1, b := name)",
    ] {
        for dialect in [Dialect::Rusty, Dialect::Iec61131_3Ed2] {
            let (codes, variable) = analyse(CONSTANT, lowered(text), dialect);
            // The analysis answers: with diagnostics, or with the declaration
            // it analysed.
            assert!(!codes.is_empty() || variable.is_some(), "{text}");
        }
    }
}
