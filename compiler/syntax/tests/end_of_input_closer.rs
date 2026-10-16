//! A construct that the end of the input cuts off is reported by naming its
//! own closer, the closer of the innermost construct that is open, not the
//! closer of a unit around it.
//!
//! Every construct with a closing keyword is a row of [`CASES`]; a closing
//! keyword that has no row fails [`every_closing_keyword_has_a_case_or_is_unused`],
//! so a construct added to the grammar is tested here or the build says so.

use ironplc_dsl::core::FileId;
use ironplc_syntax::{parse_source_file, ParseOptions, SyntaxKind};
use rstest::rstest;

/// A text that ends inside one construct, and what is reported for it.
struct Case {
    /// The keyword that closes the construct.
    closer: SyntaxKind,
    /// A text that ends inside the construct, with every construct around it
    /// open as well.
    source: &'static str,
    /// What the report says is expected at the end of the input, when it is
    /// not the closer by name.
    expected: Option<&'static str>,
}

const fn closing(closer: SyntaxKind, source: &'static str) -> Case {
    Case {
        closer,
        source,
        expected: None,
    }
}

const fn described(closer: SyntaxKind, source: &'static str, expected: &'static str) -> Case {
    Case {
        closer,
        source,
        expected: Some(expected),
    }
}

const CASES: &[Case] = &[
    closing(
        SyntaxKind::EndIf,
        "PROGRAM p\nVAR x : INT; END_VAR\nIF x < 3 THEN\nx := 2;\n",
    ),
    closing(
        SyntaxKind::EndCase,
        "PROGRAM p\nVAR x : INT; END_VAR\nCASE x OF\n1: x := 2;\n",
    ),
    closing(
        SyntaxKind::EndFor,
        "PROGRAM p\nVAR x : INT; END_VAR\nFOR x := 1 TO 3 DO\nx := 2;\n",
    ),
    closing(
        SyntaxKind::EndWhile,
        "PROGRAM main VAR x : INT; END_VAR\n  WHILE x < 3 DO\n    x := x + 1;\n",
    ),
    closing(
        SyntaxKind::EndRepeat,
        "PROGRAM p\nVAR x : INT; END_VAR\nREPEAT\nx := 2;\nUNTIL x > 3\n",
    ),
    closing(
        SyntaxKind::EndTry,
        "PROGRAM p\nVAR x : INT; END_VAR\n__TRY\nx := 2;\n__CATCH\nx := 3;\n",
    ),
    closing(SyntaxKind::EndVar, "PROGRAM p\nVAR x : INT;\n"),
    described(
        SyntaxKind::EndStruct,
        "TYPE t :\nSTRUCT\na : INT;\n",
        "the end of the structure",
    ),
    described(
        SyntaxKind::EndUnion,
        "TYPE t :\nUNION\na : INT;\n",
        "the end of the structure",
    ),
    closing(SyntaxKind::EndType, "TYPE t : INT;\n"),
    closing(
        SyntaxKind::EndProgram,
        "PROGRAM p\nVAR x : INT; END_VAR\nx := 2;\n",
    ),
    closing(
        SyntaxKind::EndFunction,
        "FUNCTION f : INT\nVAR x : INT; END_VAR\nf := 2;\n",
    ),
    closing(
        SyntaxKind::EndFunctionBlock,
        "FUNCTION_BLOCK fb\nVAR x : INT; END_VAR\nx := 2;\n",
    ),
    closing(
        SyntaxKind::EndMethod,
        "FUNCTION_BLOCK fb\nMETHOD m\nVAR x : INT; END_VAR\nx := 2;\n",
    ),
    closing(
        SyntaxKind::EndProperty,
        "FUNCTION_BLOCK fb\nPROPERTY pr : INT\nGET\npr := 2;\nEND_GET\n",
    ),
    described(
        SyntaxKind::EndGet,
        "FUNCTION_BLOCK fb\nPROPERTY pr : INT\nGET\npr := 2;\n",
        "the end of the accessor",
    ),
    described(
        SyntaxKind::EndSet,
        "FUNCTION_BLOCK fb\nPROPERTY pr : INT\nSET\npr := 2;\n",
        "the end of the accessor",
    ),
    closing(SyntaxKind::EndInterface, "INTERFACE i\n"),
    closing(
        SyntaxKind::EndNamespace,
        "NAMESPACE n\nPROGRAM p\nEND_PROGRAM\n",
    ),
    closing(
        SyntaxKind::EndConfiguration,
        "CONFIGURATION c\nRESOURCE r ON cpu\nPROGRAM i : p;\nEND_RESOURCE\n",
    ),
    closing(
        SyntaxKind::EndResource,
        "CONFIGURATION c\nRESOURCE r ON cpu\nPROGRAM i : p;\n",
    ),
    closing(SyntaxKind::EndStep, "PROGRAM p\nINITIAL_STEP s0 :\na(N);\n"),
    closing(
        SyntaxKind::EndAction,
        "PROGRAM p\nINITIAL_STEP s0 : END_STEP\nACTION a :\nx := 2;\n",
    ),
    closing(
        SyntaxKind::EndTransition,
        "PROGRAM p\nINITIAL_STEP s0 : END_STEP\nTRANSITION FROM s0 TO s0 := TRUE;\n",
    ),
];

/// The closing keywords that no construct of the grammar is closed by.
const UNUSED: &[SyntaxKind] = &[SyntaxKind::EndTask];

/// What the primary error of `source` says, parsed with every form enabled.
fn said(source: &str) -> String {
    let parse = parse_source_file(source, &ParseOptions::all());
    parse
        .primary_diagnostic(&FileId::default())
        .map(|diagnostic| diagnostic.primary.message)
        .unwrap_or_default()
}

fn expected_said(case: &Case) -> String {
    let wanted = case.expected.map_or_else(
        || format!("`{}`", case.closer.spelling().unwrap_or("")),
        String::from,
    );
    format!("found end of input, expected {wanted}")
}

#[test]
fn primary_error_when_input_ends_inside_a_construct_then_it_names_the_closer_of_that_construct() {
    for case in CASES {
        assert_eq!(said(case.source), expected_said(case), "{:?}", case.source);
    }
}

#[test]
fn every_closing_keyword_has_a_case_or_is_unused() {
    for kind in SyntaxKind::ALL {
        let spelling = kind.spelling().unwrap_or("");
        let closes =
            kind.is_keyword() && (spelling.starts_with("END_") || spelling.starts_with("__END"));
        if closes && !UNUSED.contains(kind) {
            assert!(
                CASES.iter().any(|case| case.closer == *kind),
                "{spelling} closes a construct and has no case"
            );
        }
    }
}

#[rstest]
#[case::two_loops("PROGRAM p\nWHILE a DO\nFOR i := 1 TO 3 DO\nx := 1;\n", "END_FOR")]
#[case::loop_in_branch("PROGRAM p\nIF a THEN\nWHILE b DO\nx := 1;\n", "END_WHILE")]
#[case::branch_in_loop("PROGRAM p\nWHILE a DO\nIF b THEN\nx := 1;\n", "END_IF")]
#[case::method_in_block("FUNCTION_BLOCK fb\nMETHOD m\nIF a THEN\nx := 1;\n", "END_IF")]
#[case::block_after_method(
    "FUNCTION_BLOCK fb\nMETHOD m\nx := 1;\nEND_METHOD\n",
    "END_FUNCTION_BLOCK"
)]
fn primary_error_when_input_ends_inside_nested_constructs_then_the_innermost_closer(
    #[case] source: &str,
    #[case] closer: &str,
) {
    assert_eq!(
        said(source),
        format!("found end of input, expected `{closer}`")
    );
}

#[test]
fn primary_error_when_input_ends_after_trailing_trivia_then_anchored_where_the_last_token_ends() {
    let source = "PROGRAM p\nWHILE a DO\nx := 1;\n\n// last\n";
    let parse = parse_source_file(source, &ParseOptions::all());
    let range = parse.primary_error().map(|error| error.range);
    let end = source.find(';').map_or(0, |at| at + 1);
    assert_eq!(
        range.map(|r| (usize::from(r.start()), usize::from(r.end()))),
        Some((end, end))
    );
}
