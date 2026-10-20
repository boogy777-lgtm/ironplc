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
fn severity_variant_when_declarable_level_then_its_variant() {
    for level in ["FatalError", "Error", "Warning", "Information", "Text"] {
        assert_eq!(severity_variant("P0001", level), Ok(level));
    }
}

#[test]
fn severity_variant_when_missing_unknown_or_suppressed_then_error_names_code_and_value() {
    for value in [
        "",
        "error",
        "Warn",
        "Info",
        " Error",
        "SuppressedWarning",
        "SuppressedInformation",
    ] {
        let message = severity_variant("P0042", value).expect_err("the value should be refused");

        assert!(message.contains("P0042"), "'{value}': {message}");
        assert!(
            message.contains(&format!("'{value}'")),
            "'{value}': {message}"
        );
        assert!(
            message.contains("FatalError, Error, Warning, Information, Text"),
            "'{value}': {message}"
        );
    }
}

#[test]
fn check_problem_codes_header_when_codesys_column_present_then_accepted() {
    assert_eq!(
        check_problem_codes_header(&["Code", "Name", "Message", "Severity", "Codesys"]),
        Ok(())
    );
}

#[test]
fn check_problem_codes_header_when_column_missing_or_moved_then_error() {
    for header in [
        vec!["Code", "Name", "Message"],
        vec!["Code", "Name", "Message", "Severity"],
        vec!["Code", "Name", "Severity", "Message", "Codesys"],
        vec!["Code", "Name", "Message", "Severity", "Counterpart"],
    ] {
        let message =
            check_problem_codes_header(&header).expect_err("the header should be refused");

        assert!(
            message.contains("Code,Name,Message,Severity,Codesys"),
            "{message}"
        );
    }
}

#[test]
fn codesys_code_when_empty_then_none() {
    assert_eq!(codesys_code("P0001", ""), Ok(None));
}

#[test]
fn codesys_code_when_c_and_four_digits_then_that_code() {
    assert_eq!(
        codesys_code("P4007", "C0046"),
        Ok(Some("C0046".to_string()))
    );
}

#[test]
fn codesys_code_when_malformed_then_error_names_code_and_value() {
    for value in [
        "C046", "C00460", "c0046", "P0046", "C00x6", " C0046", "0046", "C",
    ] {
        let message = codesys_code("P0042", value).expect_err("the value should be refused");

        assert!(message.contains("P0042"), "'{value}': {message}");
        assert!(
            message.contains(&format!("'{value}'")),
            "'{value}': {message}"
        );
    }
}

#[test]
fn render_severity_enum_when_called_then_the_seven_categories_in_the_order_of_codesys() {
    let source = render_severity_enum();

    let positions: Vec<usize> = [
        "FatalError",
        "Error",
        "Warning",
        "Information",
        "Text",
        "SuppressedWarning",
        "SuppressedInformation",
    ]
    .iter()
    .map(|name| {
        source
            .find(&format!("    {name},\n"))
            .unwrap_or_else(|| panic!("no variant {name}: {source}"))
    })
    .collect();
    assert!(
        positions.windows(2).all(|pair| pair[0] < pair[1]),
        "{source}"
    );
}

#[test]
fn render_severity_enum_when_called_then_each_question_is_an_exhaustive_match_without_wildcard() {
    let source = render_severity_enum();

    for function in [
        "script_name",
        "script_value",
        "person_word",
        "name",
        "stops_build",
        "is_shown",
        "suppressed",
        "raised",
    ] {
        assert!(
            source.contains(&format!("pub fn {function}(&self)")),
            "{source}"
        );
    }
    assert!(!source.contains("_ =>"), "{source}");
    assert_eq!(source.matches("match self {").count(), 8, "{source}");
    // Seven arms for each of the eight questions.
    assert_eq!(
        source.matches("            Severity::").count(),
        56,
        "{source}"
    );
}

#[test]
fn render_severity_enum_when_called_then_the_facts_of_each_category() {
    let source = render_severity_enum();

    for line in [
        "Severity::FatalError => \"fatal-error\",",
        "Severity::SuppressedInformation => \"suppressed-information\",",
        "Severity::FatalError => true,",
        "Severity::Error => true,",
        "Severity::Warning => false,",
        "Severity::SuppressedWarning => false,",
        "Severity::Warning => Severity::SuppressedWarning,",
        "Severity::Information => Severity::SuppressedInformation,",
        "Severity::Warning => Severity::Error,",
        "Severity::Text => Severity::Text,",
        "Severity::Error => Severity::Error,",
        "Severity::FatalError => \"FatalError\",",
        "Severity::SuppressedInformation => \"SuppressedInformation\",",
        "Severity::FatalError => 1,",
        "Severity::Error => 2,",
        "Severity::Warning => 4,",
        "Severity::Information => 8,",
        "Severity::Text => 16,",
        "Severity::SuppressedWarning => 32,",
        "Severity::SuppressedInformation => 64,",
        "Severity::FatalError => Some(\"Error\"),",
        "Severity::Error => Some(\"Error\"),",
        "Severity::Warning => Some(\"Warning\"),",
        "Severity::Information => Some(\"Information\"),",
        "Severity::Text => None,",
        "Severity::SuppressedWarning => None,",
    ] {
        assert!(source.contains(line), "{line}: {source}");
    }
}

fn generate_codeless(rows: &str) -> Result<String, String> {
    generate_codeless_texts(&format!("Key,Severity,Template\n{rows}"))
}

fn codeless_refused(rows: &str) -> String {
    generate_codeless(rows).expect_err("the rows should be refused")
}

#[test]
fn generate_codeless_texts_when_rows_then_one_function_for_each_with_its_key_and_category() {
    let out = generate_codeless("limit_line,Error,\"Over {limit}\"\nplain_line,Text,Done")
        .expect("the rows should be accepted");

    assert!(out.contains("pub fn limit_line(limit: impl std::fmt::Display) -> CodelessText"));
    assert!(out.contains("CodelessText::new(\"limit_line\", Severity::Error, format!("));
    assert!(out.contains("pub fn plain_line() -> CodelessText"));
    assert!(out.contains("CodelessText::new(\"plain_line\", Severity::Text, \"Done\".to_string())"));
}

#[test]
fn generate_codeless_texts_when_header_is_not_key_severity_template_then_error() {
    for header in ["Code,Key,Template", "Key,Template,Severity", "Key,Severity"] {
        let message = generate_codeless_texts(&format!("{header}\n")).expect_err("refused");

        assert!(message.contains("Key,Severity,Template"), "{message}");
    }
}

#[test]
fn generate_codeless_texts_when_severity_missing_unknown_or_suppressed_then_error_names_row() {
    for value in [
        "",
        "error",
        "Info",
        "SuppressedWarning",
        "SuppressedInformation",
    ] {
        let message = codeless_refused(&format!("a_line,{value},A"));

        assert!(message.contains("line 2"), "'{value}': {message}");
        assert!(message.contains("'a_line'"), "'{value}': {message}");
        assert!(
            message.contains(&format!("'{value}'")),
            "'{value}': {message}"
        );
    }
}

#[test]
fn generate_codeless_texts_when_key_not_snake_case_or_repeated_then_error() {
    let message = codeless_refused("Camel,Text,A");
    assert!(message.contains("snake_case"), "{message}");

    let message = codeless_refused("a_line,Text,A\na_line,Error,B");
    assert!(message.contains("line 3"), "{message}");
    assert!(message.contains("used by an earlier row"), "{message}");
}

#[test]
fn generate_codeless_texts_when_placeholder_malformed_then_error_names_row() {
    let message = codeless_refused("a_line,Text,\"{X}\"");

    assert!(message.contains("'a_line'"), "{message}");
    assert!(message.contains("{X}"), "{message}");
}

fn warning_problems(rows: &[(&str, &str)]) -> String {
    render_warning_problems(rows)
}

#[test]
fn render_warning_problems_when_rows_then_one_variant_for_each_warning_in_order() {
    let source = warning_problems(&[
        ("SyntaxError", "Error"),
        ("EnumerationTypesCompared", "Warning"),
        ("NotImplemented", "FatalError"),
        ("VariableShadowed", "Warning"),
        ("Note", "Information"),
    ]);

    assert!(
        source.contains(
            "pub enum WarningProblem {\n    EnumerationTypesCompared,\n    VariableShadowed,\n}"
        ),
        "{source}"
    );
    assert!(!source.contains("SyntaxError"), "{source}");
    assert!(!source.contains("NotImplemented"), "{source}");
    assert!(!source.contains("Note"), "{source}");
    assert!(
        source.contains("pub const ALL: [WarningProblem; 2] = ["),
        "{source}"
    );
}

#[test]
fn render_warning_problems_when_rows_then_each_answer_is_an_exhaustive_match_without_wildcard() {
    let source = warning_problems(&[
        ("EnumerationTypesCompared", "Warning"),
        ("VariableShadowed", "Warning"),
    ]);

    assert!(!source.contains("_ =>"), "{source}");
    assert_eq!(source.matches("match self {").count(), 2, "{source}");
    for line in [
        "WarningProblem::EnumerationTypesCompared => Problem::EnumerationTypesCompared,",
        "WarningProblem::VariableShadowed => Problem::VariableShadowed,",
        "WarningProblem::EnumerationTypesCompared => 0,",
        "WarningProblem::VariableShadowed => 1,",
    ] {
        assert!(source.contains(line), "{line}: {source}");
    }
}

#[test]
fn render_warning_problems_when_no_warning_then_an_empty_type() {
    let source = warning_problems(&[("SyntaxError", "Error")]);

    assert!(source.contains("pub enum WarningProblem {\n}"), "{source}");
    assert!(
        source.contains("pub const ALL: [WarningProblem; 0] = ["),
        "{source}"
    );
}
