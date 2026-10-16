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
    let message = refused("P0001,open_comment_fine,A\nP7777,unknown_code,B");

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
    let message = refused("P0001,open_comment_keyword_placeholder,\"Value {type}\"");

    assert!(message.contains("{type}"), "{message}");
    assert!(message.contains("keyword"), "{message}");
}

#[test]
fn generate_texts_when_key_repeated_then_error_names_the_second_row() {
    let message = refused("P0001,open_comment_same,A\nP0001,open_comment_same,B");

    assert!(message.contains("line 3"), "{message}");
    assert!(message.contains("P0001, 'open_comment_same'"), "{message}");
    assert!(message.contains("used by an earlier row"), "{message}");
}

#[test]
fn generate_texts_when_placeholder_malformed_then_error() {
    for template in ["{}", "{0}", "{X}", "{a b}", "{_a}", "{a-b}"] {
        let message = refused(&format!("P0001,open_comment_bad,\"{template}\""));

        assert!(
            message.contains("P0001, 'open_comment_bad'"),
            "'{template}': {message}"
        );
        assert!(message.contains(template), "'{template}': {message}");
    }
}

#[test]
fn generate_texts_when_placeholder_has_format_spec_then_error() {
    let message = refused("P0001,open_comment_bad,\"Value {x:?}\"");

    assert!(message.contains("{x:?}"), "{message}");
}

#[test]
fn generate_texts_when_open_brace_not_closed_then_error() {
    for template in ["open { here", "{a", "{a{b}}", "{{{a", "{"] {
        let message = refused(&format!("P0001,open_comment_bad,\"{template}\""));

        assert!(message.contains("not closed"), "'{template}': {message}");
    }
}

#[test]
fn generate_texts_when_close_brace_alone_then_error() {
    for template in ["close } here", "a}", "a}}}", "}"] {
        let message = refused(&format!("P0001,open_comment_bad,\"{template}\""));

        assert!(
            message.contains("does not close a placeholder"),
            "'{template}': {message}"
        );
    }
}

#[test]
fn generate_texts_when_code_is_compiler_located_then_error() {
    let message = refused("P9999,not_implemented_here,A");

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
    let source = generated("P0001,open_comment_two_names,\"{b} and {a} and {b}\"");

    assert!(
        source.contains(
            "pub fn open_comment_two_names(b: impl std::fmt::Display, a: impl std::fmt::Display) -> Text"
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
    let source = generated("P0001,open_comment_braces,\"Set {{{name}}}\"");

    assert!(
        source.contains("pub fn open_comment_braces(name: impl std::fmt::Display) -> Text"),
        "{source}"
    );
    assert!(source.contains("format!(\"Set {{{name}}}\")"), "{source}");
}

#[test]
fn generate_texts_when_no_placeholder_then_no_parameter_and_braces_resolved() {
    let source = generated("P0001,open_comment_plain,\"Keep {{this}}\"");

    assert!(
        source.contains(
            "pub fn open_comment_plain() -> Text {\n        Text::new(Problem::OpenComment, \"Keep {this}\".to_string())"
        ),
        "{source}"
    );
}

#[test]
fn generate_texts_when_two_rows_for_one_code_then_a_function_for_each_of_that_problem() {
    let source = generated("P0001,open_comment_first,A\nP0001,open_comment_second,B");

    assert!(source.contains("pub fn open_comment_first()"), "{source}");
    assert!(source.contains("pub fn open_comment_second()"), "{source}");
    assert_eq!(
        source.matches("Problem::OpenComment").count(),
        2,
        "{source}"
    );
}

#[test]
fn generate_texts_when_template_has_quote_and_backslash_then_the_literal_is_escaped() {
    let source = generated("P0001,open_comment_quoted,\"Say \"\"hi\"\" \\ now\"");

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

#[test]
fn generate_texts_when_key_lacks_the_name_of_its_problem_then_error_names_the_prefix() {
    let message = refused("P0002,open_comment_wrong_problem,A");

    assert!(message.contains("line 2"), "{message}");
    assert!(
        message.contains("P0002, 'open_comment_wrong_problem'"),
        "{message}"
    );
    assert!(message.contains("syntax_error"), "{message}");
}

#[test]
fn generate_texts_when_key_is_the_snake_case_of_the_name_then_accepted() {
    let source = generated("P0001,open_comment,A");

    assert!(source.contains("pub fn open_comment()"), "{source}");
}

#[test]
fn generate_texts_when_key_is_the_name_and_a_word_then_accepted() {
    let source = generated("P0001,open_comment_x,A");

    assert!(source.contains("pub fn open_comment_x()"), "{source}");
}

#[test]
fn generate_texts_when_key_continues_the_name_without_underscore_then_error() {
    let message = refused("P0001,open_commentx,A");

    assert!(message.contains("open_comment"), "{message}");
}

#[test]
fn snake_case_of_name_when_camel_case_acronym_and_digits_then_words_split_once() {
    assert_eq!(snake_case_of_name("OpenComment"), "open_comment");
    assert_eq!(snake_case_of_name("Utf8Bom"), "utf8_bom");
    assert_eq!(snake_case_of_name("XmlIoError"), "xml_io_error");
    assert_eq!(snake_case_of_name("Simple"), "simple");
}

#[test]
fn severity_variant_when_each_level_then_its_variant() {
    assert_eq!(severity_variant("P0001", "Error"), Ok("Error"));
    assert_eq!(severity_variant("P0001", "Warning"), Ok("Warning"));
    assert_eq!(severity_variant("P0001", "Info"), Ok("Info"));
}

#[test]
fn severity_variant_when_missing_or_unknown_then_error_names_code_and_value() {
    for value in ["", "error", "Warn", "Information", " Error"] {
        let message = severity_variant("P0042", value).expect_err("the value should be refused");

        assert!(message.contains("P0042"), "'{value}': {message}");
        assert!(message.contains(&format!("'{value}'")), "'{value}': {message}");
        assert!(message.contains("Error, Warning, Info"), "'{value}': {message}");
    }
}

#[test]
fn check_problem_codes_header_when_severity_column_present_then_accepted() {
    assert_eq!(
        check_problem_codes_header(&["Code", "Name", "Message", "Severity"]),
        Ok(())
    );
}

#[test]
fn check_problem_codes_header_when_column_missing_or_moved_then_error() {
    for header in [
        vec!["Code", "Name", "Message"],
        vec!["Code", "Name", "Severity", "Message"],
        vec!["Code", "Name", "Message", "Level"],
    ] {
        let message = check_problem_codes_header(&header).expect_err("the header should be refused");

        assert!(message.contains("Code,Name,Message,Severity"), "{message}");
    }
}

#[test]
fn render_severity_enum_when_called_then_one_variant_for_each_level() {
    let source = render_severity_enum();

    assert!(source.contains("pub enum Severity {"), "{source}");
    for level in ["Error", "Warning", "Info"] {
        assert!(source.contains(&format!("    {level},
")), "{source}");
    }
}
