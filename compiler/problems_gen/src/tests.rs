use super::*;

/// `(code, name)` of the problems the rows may name. P9999 is compiler-located.
const PROBLEMS: &[(&str, &str)] = &[
    ("P0001", "OpenComment"),
    ("P0002", "SyntaxError"),
    ("P9999", "NotImplemented"),
];

fn generate(rows: &str) -> Result<String, String> {
    generate_texts(&format!("Code,Key,Template\n{rows}"), PROBLEMS)
}

fn generated(rows: &str) -> String {
    generate(rows).expect("the rows should be accepted")
}

fn refused(rows: &str) -> String {
    generate(rows).expect_err("the rows should be refused")
}

#[test]
fn generate_texts_when_code_not_in_problem_codes_then_error_names_line_code_and_key() {
    let message = refused("P0001,fine,A\nP7777,unknown_code,B");

    assert!(message.contains("line 3"), "{message}");
    assert!(message.contains("P7777"), "{message}");
    assert!(message.contains("'unknown_code'"), "{message}");
    assert!(message.contains("not in problem-codes.csv"), "{message}");
}

#[test]
fn generate_texts_when_key_not_snake_case_then_error() {
    for key in [
        "Camel", "has-dash", "_lead", "trail_", "dou__ble", "9lead", "",
    ] {
        let message = refused(&format!("P0001,{key},A"));

        assert!(message.contains("snake_case"), "'{key}': {message}");
    }
}

#[test]
fn generate_texts_when_key_is_a_rust_keyword_then_error() {
    let message = refused("P0001,fn,A");

    assert!(message.contains("snake_case"), "{message}");
}

#[test]
fn generate_texts_when_placeholder_is_a_rust_keyword_then_error() {
    let message = refused("P0001,keyword_placeholder,\"Value {type}\"");

    assert!(message.contains("{type}"), "{message}");
    assert!(message.contains("keyword"), "{message}");
}

#[test]
fn generate_texts_when_key_repeated_then_error_names_the_second_row() {
    let message = refused("P0001,same,A\nP0002,same,B");

    assert!(message.contains("line 3"), "{message}");
    assert!(message.contains("P0002, 'same'"), "{message}");
    assert!(message.contains("used by an earlier row"), "{message}");
}

#[test]
fn generate_texts_when_placeholder_malformed_then_error() {
    for template in ["{}", "{0}", "{X}", "{a b}", "{_a}", "{a-b}"] {
        let message = refused(&format!("P0001,bad,\"{template}\""));

        assert!(message.contains("P0001, 'bad'"), "'{template}': {message}");
        assert!(message.contains(template), "'{template}': {message}");
    }
}

#[test]
fn generate_texts_when_placeholder_has_format_spec_then_error() {
    let message = refused("P0001,bad,\"Value {x:?}\"");

    assert!(message.contains("{x:?}"), "{message}");
}

#[test]
fn generate_texts_when_open_brace_not_closed_then_error() {
    for template in ["open { here", "{a", "{a{b}}", "{{{a", "{"] {
        let message = refused(&format!("P0001,bad,\"{template}\""));

        assert!(message.contains("not closed"), "'{template}': {message}");
    }
}

#[test]
fn generate_texts_when_close_brace_alone_then_error() {
    for template in ["close } here", "a}", "a}}}", "}"] {
        let message = refused(&format!("P0001,bad,\"{template}\""));

        assert!(
            message.contains("does not close a placeholder"),
            "'{template}': {message}"
        );
    }
}

#[test]
fn generate_texts_when_code_is_compiler_located_then_error() {
    let message = refused("P9999,not_here,A");

    assert!(message.contains("NotImplemented"), "{message}");
    assert!(message.contains("compiler-located"), "{message}");
}

#[test]
fn generate_texts_when_header_differs_then_error() {
    let message = generate_texts("Code,Name,Message\nP0001,a,A", PROBLEMS)
        .expect_err("the header should be refused");

    assert!(message.contains("Code,Key,Template"), "{message}");
}

#[test]
fn generate_texts_when_placeholders_then_one_display_parameter_each_in_order_of_first_appearance() {
    let source = generated("P0001,two_names,\"{b} and {a} and {b}\"");

    assert!(
        source.contains(
            "pub fn two_names(b: impl std::fmt::Display, a: impl std::fmt::Display) -> Text"
        ),
        "{source}"
    );
    assert!(
        source.contains("Text::new(Problem::OpenComment, format!(\"{b} and {a} and {b}\"))"),
        "{source}"
    );
}

#[test]
fn generate_texts_when_escaped_braces_and_placeholder_then_braces_stay_escaped_in_the_format() {
    let source = generated("P0001,braces,\"Set {{{name}}}\"");

    assert!(
        source.contains("pub fn braces(name: impl std::fmt::Display) -> Text"),
        "{source}"
    );
    assert!(source.contains("format!(\"Set {{{name}}}\")"), "{source}");
}

#[test]
fn generate_texts_when_no_placeholder_then_no_parameter_and_braces_resolved() {
    let source = generated("P0001,plain,\"Keep {{this}}\"");

    assert!(
        source.contains(
            "pub fn plain() -> Text {\n        Text::new(Problem::OpenComment, \"Keep {this}\".to_string())"
        ),
        "{source}"
    );
}

#[test]
fn generate_texts_when_two_rows_for_one_code_then_a_function_for_each_of_that_problem() {
    let source = generated("P0001,first_wording,A\nP0001,second_wording,B");

    assert!(source.contains("pub fn first_wording()"), "{source}");
    assert!(source.contains("pub fn second_wording()"), "{source}");
    assert_eq!(
        source.matches("Problem::OpenComment").count(),
        2,
        "{source}"
    );
}

#[test]
fn generate_texts_when_template_has_quote_and_backslash_then_the_literal_is_escaped() {
    let source = generated("P0001,quoted,\"Say \"\"hi\"\" \\ now\"");

    assert!(
        source.contains("\"Say \\\"hi\\\" \\\\ now\".to_string()"),
        "{source}"
    );
}

#[test]
fn deprecation_note_when_compiler_located_name_then_note_and_otherwise_none() {
    assert!(deprecation_note("NotImplemented").is_some());
    assert!(deprecation_note("NotSupported").is_some());
    assert!(deprecation_note("InternalError").is_some());
    assert!(deprecation_note("SyntaxError").is_none());
}
