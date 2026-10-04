//! Lists that a type needs at least one item of, and the initial values the
//! grammar leaves for later.
//!
//! CODESYS 3.5 is the reference for both. It reads the members of an
//! enumeration with `EnumListParser`, which reports `Identifier expected
//! instead of ')'` at the first token that is not a name, so `()` and a
//! trailing comma are rejected where they are written
//! (`Codesys/Parser35220.plugin/CODESYS/Parser35220/Declaration/EnumListParser.cs`,
//! lines 118-121 and 261-273). It reads the bounds of an array with the
//! expression parser, which reports `Expression expected instead of ']'` at
//! the `]` of `ARRAY[]` (`Declaration/TypeParser.cs`, lines 615-625, and
//! `Expressions/OperandParser.cs`, lines 481-486). It reads the initial value
//! of a variable as any expression, so `x : INT := name` is not a syntax error
//! (`Declaration/VariableDeclarationParser.cs`, line 180, and
//! `Expressions/InitializationParser.cs`, lines 70-92).

use ironplc_syntax::{parse_source_file, ParseOptions};

/// The message and the text under the range of the first error of `source`.
fn first_error(source: &str) -> Option<(String, String)> {
    let parse = parse_source_file(source, &ParseOptions::all());
    let error = parse.errors.first()?;
    let range = usize::from(error.range.start())..usize::from(error.range.end());
    Some((error.message.clone(), source[range].to_string()))
}

#[test]
fn parse_source_file_when_enumeration_has_no_value_then_error_at_the_closing_parenthesis() {
    for source in [
        "TYPE t : (); END_TYPE",
        "TYPE t : STRUCT a : (); END_STRUCT; END_TYPE",
        "PROGRAM p VAR x : (); END_VAR END_PROGRAM",
        "PROGRAM p VAR_IN_OUT x : (); END_VAR END_PROGRAM",
    ] {
        assert_eq!(
            first_error(source),
            Some(("expected a name".to_string(), ")".to_string())),
            "{source}"
        );
    }
}

#[test]
fn parse_source_file_when_enumeration_ends_in_a_comma_then_error_at_the_closing_parenthesis() {
    for source in [
        "TYPE t : (A, ); END_TYPE",
        "PROGRAM p VAR x : (A, B, ); END_VAR END_PROGRAM",
    ] {
        assert_eq!(
            first_error(source),
            Some(("expected a name".to_string(), ")".to_string())),
            "{source}"
        );
    }
}

#[test]
fn parse_source_file_when_array_has_no_range_then_error_at_the_closing_bracket() {
    for source in [
        "TYPE t : ARRAY[] OF INT; END_TYPE",
        "TYPE t : STRUCT a : ARRAY[] OF INT; END_STRUCT; END_TYPE",
        "PROGRAM p VAR x : ARRAY[] OF INT; END_VAR END_PROGRAM",
        "PROGRAM p VAR x : REF_TO ARRAY[] OF INT; END_VAR END_PROGRAM",
        "PROGRAM p VAR_IN_OUT x : ARRAY[] OF INT; END_VAR END_PROGRAM",
    ] {
        assert_eq!(
            first_error(source),
            Some((
                "expected the lower bound of a range".to_string(),
                "]".to_string()
            )),
            "{source}"
        );
    }
}

#[test]
fn parse_source_file_when_array_ends_in_a_comma_then_error_at_the_closing_bracket() {
    assert_eq!(
        first_error("TYPE t : ARRAY[1..2, ] OF INT; END_TYPE"),
        Some((
            "expected the lower bound of a range".to_string(),
            "]".to_string()
        ))
    );
}

#[test]
fn parse_source_file_when_variable_is_initialised_with_an_expression_then_no_syntax_error() {
    for source in [
        "PROGRAM p VAR x : INT := name; END_VAR END_PROGRAM",
        "PROGRAM p VAR x : INT := a.b; END_VAR END_PROGRAM",
        "PROGRAM p VAR x : INT := limit * 2 + 1; END_VAR END_PROGRAM",
        "PROGRAM p VAR x : MyType := name; END_VAR END_PROGRAM",
    ] {
        assert_eq!(first_error(source), None, "{source}");
    }
}
