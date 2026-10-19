use super::*;
use crate::core::{FileId, SourceSpan};
use crate::diagnostic::Severity;
use ironplc_problems::text;
use spec_test_macro::spec_test;

/// A diagnostic of a problem that has a CODESYS counterpart (P4007, C0046).
fn undeclared(span: SourceSpan) -> Diagnostic {
    Diagnostic::text(span, text::variable_undefined_undefined_variable())
}

/// A diagnostic of a problem that has none (P0002).
fn syntax_error(span: SourceSpan) -> Diagnostic {
    Diagnostic::text(span, text::syntax_error_a_task_needs_a_priority())
}

/// The diagnostic in a category no problem declares yet.
fn in_category(diagnostic: Diagnostic, severity: Severity) -> Diagnostic {
    let mut diagnostic = diagnostic;
    diagnostic.severity = severity;
    diagnostic
}

const ALL: [Severity; 7] = [
    Severity::FatalError,
    Severity::Error,
    Severity::Warning,
    Severity::Information,
    Severity::Text,
    Severity::SuppressedWarning,
    Severity::SuppressedInformation,
];

#[spec_test(REQ_DS_dsl_006)]
fn codesys_when_made_by_each_constructor_then_the_codesys_code_of_its_problem() {
    let span = SourceSpan::default();
    let primary = || crate::diagnostic::Label::span(SourceSpan::default(), "primary");

    assert_eq!(undeclared(span.clone()).codesys(), Some("C0046"));
    assert_eq!(
        Diagnostic::text_in_file(
            FileId::from_string("f"),
            text::variable_undefined_undefined_variable()
        )
        .codesys(),
        Some("C0046")
    );
    assert_eq!(syntax_error(span.clone()).codesys(), None);
    assert_eq!(Diagnostic::todo_with_span(span).codesys(), None);
    assert_eq!(Diagnostic::internal_error_at(primary()).codesys(), None);
}

#[spec_test(REQ_DS_dsl_007)]
fn person_header_when_category_has_a_word_then_word_code_and_the_codesys_code() {
    let with = undeclared(SourceSpan::default());
    let without = syntax_error(SourceSpan::default());

    assert_eq!(
        with.person_header().as_deref(),
        Some("Error[P4007] (CODESYS C0046)")
    );
    assert_eq!(without.person_header().as_deref(), Some("Error[P0002]"));
    assert_eq!(
        in_category(with.clone(), Severity::FatalError)
            .person_header()
            .as_deref(),
        Some("Error[P4007] (CODESYS C0046)")
    );
    assert_eq!(
        in_category(with.clone(), Severity::Warning)
            .person_header()
            .as_deref(),
        Some("Warning[P4007] (CODESYS C0046)")
    );
    assert_eq!(
        in_category(without, Severity::Information)
            .person_header()
            .as_deref(),
        Some("Information[P0002]")
    );
}

#[spec_test(REQ_DS_dsl_007)]
fn person_header_when_text_or_suppressed_then_none() {
    let base = undeclared(SourceSpan::default());

    for severity in [
        Severity::Text,
        Severity::SuppressedWarning,
        Severity::SuppressedInformation,
    ] {
        assert_eq!(
            in_category(base.clone(), severity).person_header(),
            None,
            "{severity:?}"
        );
    }
}

#[spec_test(REQ_DS_dsl_008)]
fn person_text_when_label_then_the_label_and_otherwise_the_description() {
    use crate::diagnostic::Label;

    let labelled = syntax_error(SourceSpan::default());
    assert_eq!(labelled.person_text(), "a task needs a `PRIORITY`");

    let mut unlabelled = labelled.clone();
    unlabelled.primary = Label::span(SourceSpan::default(), "");
    assert_eq!(unlabelled.person_text(), unlabelled.description());
}

#[spec_test(REQ_DS_dsl_009)]
fn script_message_when_diagnostic_then_the_fields_of_script_message() {
    let source = "PROGRAM main\nVAR END_VAR\n  x := 1;\nEND_PROGRAM\n";
    let start = source.find("x :=").unwrap();
    let file = FileId::from_string("main.st");
    let diagnostic = undeclared(SourceSpan {
        start,
        end: start + 1,
        file_id: file,
    });

    let message = diagnostic.script_message(Some(source));

    assert_eq!(message.severity, "Error");
    assert_eq!(message.severity_value, 2);
    assert_eq!(message.prefix.as_deref(), Some("P"));
    assert_eq!(message.number, Some(4007));
    assert_eq!(message.position, start);
    assert_eq!(message.length, 1);
    assert_eq!(message.position_text.as_deref(), Some("main.st:3:3"));
    assert_eq!(message.object, "main.st");
    assert!(message.text.contains(&diagnostic.description()));
}

#[spec_test(REQ_DS_dsl_009)]
fn script_message_when_source_unknown_then_no_position_text() {
    let message = undeclared(SourceSpan::default()).script_message(None);

    assert_eq!(message.position_text, None);
    assert_eq!(message.length, 0);
}

#[spec_test(REQ_DS_dsl_009)]
fn script_message_when_label_not_in_description_then_the_text_has_both() {
    let message = syntax_error(SourceSpan::default()).script_message(None);

    assert!(message.text.starts_with("Syntax error"), "{}", message.text);
    assert!(message.text.contains(": "), "{}", message.text);
}

#[spec_test(REQ_DS_dsl_010)]
fn script_message_when_each_category_then_its_scripting_name_and_value() {
    let expected = [
        (Severity::FatalError, "FatalError", 1),
        (Severity::Error, "Error", 2),
        (Severity::Warning, "Warning", 4),
        (Severity::Information, "Information", 8),
        (Severity::Text, "Text", 16),
        (Severity::SuppressedWarning, "SuppressedWarning", 32),
        (Severity::SuppressedInformation, "SuppressedInformation", 64),
    ];
    assert_eq!(expected.len(), ALL.len());

    for (severity, name, value) in expected {
        let message = in_category(undeclared(SourceSpan::default()), severity).script_message(None);

        assert_eq!(message.severity, name, "{severity:?}");
        assert_eq!(message.severity_value, value, "{severity:?}");
    }
}

#[spec_test(REQ_DS_dsl_010)]
fn is_in_when_mask_then_true_only_for_the_categories_in_it() {
    let warning =
        in_category(undeclared(SourceSpan::default()), Severity::Warning).script_message(None);

    assert!(warning.is_in(0xFFFF_FFFF));
    assert!(warning.is_in(4));
    assert!(warning.is_in(2 | 4));
    assert!(!warning.is_in(2));
    assert!(!warning.is_in(0));
}

#[spec_test(REQ_DS_dsl_011)]
fn script_message_when_codesys_counterpart_then_codesys_field_else_null() {
    let with = undeclared(SourceSpan::default()).script_message(None);
    let without = syntax_error(SourceSpan::default()).script_message(None);

    assert_eq!(with.codesys, Some("C0046"));
    assert_eq!(without.codesys, None);
    let json = serde_json::to_value(&with).unwrap();
    assert_eq!(json["codesys"], "C0046");
    assert!(serde_json::to_value(&without).unwrap()["codesys"].is_null());
}

#[test]
fn script_message_when_offset_inside_a_character_then_position_text_uses_the_character() {
    let source = "é = 1;";
    let diagnostic = undeclared(SourceSpan {
        start: 1,
        end: 2,
        file_id: FileId::from_string("f.st"),
    });

    let message = diagnostic.script_message(Some(source));

    assert_eq!(message.position_text.as_deref(), Some("f.st:1:1"));
}

#[test]
fn split_code_when_letter_and_digits_then_prefix_and_number() {
    assert_eq!(split_code("P0042"), Some(("P".to_string(), 42)));
    assert_eq!(split_code("P"), None);
    assert_eq!(split_code("4007"), None);
    assert_eq!(split_code(""), None);
}
