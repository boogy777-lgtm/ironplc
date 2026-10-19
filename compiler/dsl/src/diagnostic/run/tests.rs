use super::*;
use crate::core::{FileId, SourceSpan};
use crate::diagnostic::Label;
use ironplc_problems::text;
use spec_test_macro::spec_test;

const LINE_OF_ERRORS: &str = "More than 500 errors occured: Skipping all further error messages";
const LINE_OF_WARNINGS: &str =
    "More than 500 warnings occured: Skipping all further warning messages";

fn at_in(file: &str, at: usize) -> SourceSpan {
    SourceSpan::range(at, at + 1).with_file_id(&FileId::from_string(file))
}

/// An error at the byte `at` of the file `main.st`. Every call with a different
/// `at` is a different message.
fn error_at(at: usize) -> Diagnostic {
    Diagnostic::text(at_in("main.st", at), text::variable_undefined())
}

/// A warning at the byte `at` of the file `main.st`.
fn warning_at(at: usize) -> Diagnostic {
    Diagnostic::text(
        at_in("main.st", at),
        text::enumeration_types_compared("E1", "E2"),
    )
}

/// The diagnostic in a category no problem code declares, which only a test
/// reaches (the field is private to the module of `Diagnostic`).
fn in_category(mut diagnostic: Diagnostic, severity: Severity) -> Diagnostic {
    diagnostic.severity = severity;
    diagnostic
}

fn errors(count: usize) -> Vec<Diagnostic> {
    (0..count).map(error_at).collect()
}

fn warnings(count: usize) -> Vec<Diagnostic> {
    (0..count).map(warning_at).collect()
}

/// The category and the text of each message: what a person reads in a list.
fn lines(diagnostics: &[Diagnostic]) -> Vec<(Severity, String)> {
    diagnostics
        .iter()
        .map(|d| (d.severity(), d.person_text()))
        .collect()
}

fn texts_of(diagnostics: &[Diagnostic], severity: Severity) -> Vec<String> {
    diagnostics
        .iter()
        .filter(|d| d.severity() == severity)
        .map(Diagnostic::person_text)
        .collect()
}

#[spec_test(REQ_DS_dsl_015)]
fn kept_when_messages_of_mixed_categories_then_the_order_they_were_produced_in() {
    // Produced out of order of position, of category and of code.
    let produced = vec![
        warning_at(30),
        error_at(20),
        warning_at(10),
        error_at(0),
        error_at(40),
    ];
    let expected: Vec<(Severity, usize)> = produced
        .iter()
        .map(|d| (d.severity(), d.primary.location.start))
        .collect();

    let stays = kept(produced);

    let actual: Vec<(Severity, usize)> = stays
        .iter()
        .map(|d| (d.severity(), d.primary.location.start))
        .collect();
    assert_eq!(actual, expected);
}

#[spec_test(REQ_DS_dsl_016)]
fn kept_when_message_repeats_an_earlier_one_then_the_first_stays() {
    let produced = vec![error_at(5), error_at(9), error_at(5)];

    let stays = kept(produced);

    assert_eq!(stays.len(), 2);
    assert_eq!(stays[0].primary.location.start, 5);
    assert_eq!(stays[1].primary.location.start, 9);
}

#[spec_test(REQ_DS_dsl_016)]
fn kept_when_same_text_at_another_place_or_in_another_category_then_not_a_repeat() {
    let in_other_category = in_category(error_at(5), Severity::Warning);
    let in_other_file = Diagnostic::text(at_in("other.st", 5), text::variable_undefined());

    let stays = kept(vec![
        error_at(5),
        error_at(6),
        in_other_category,
        in_other_file,
    ]);

    assert_eq!(stays.len(), 4);
}

#[spec_test(REQ_DS_dsl_017)]
fn kept_when_500_errors_then_all_stay_and_no_line() {
    let stays = kept(errors(500));

    assert_eq!(stays.len(), 500);
    assert!(stays.iter().all(Diagnostic::has_code));
}

#[spec_test(REQ_DS_dsl_017)]
fn kept_when_501_errors_then_500_and_the_line_of_the_limit() {
    let stays = kept(errors(501));

    assert_eq!(stays.len(), 501);
    assert_eq!(stays[499].primary.location.start, 499);
    let line = &stays[500];
    assert!(!line.has_code());
    assert_eq!(line.severity(), Severity::Error);
    assert_eq!(line.person_text(), LINE_OF_ERRORS);
}

#[spec_test(REQ_DS_dsl_017)]
fn kept_when_more_than_501_errors_then_one_line_and_no_later_error() {
    let mut produced = errors(700);
    // A message of another kind after the limit stays.
    produced.push(warning_at(9000));

    let stays = kept(produced);

    assert_eq!(stays.len(), 502);
    assert_eq!(texts_of(&stays, Severity::Error).len(), 501);
    assert_eq!(
        texts_of(&stays, Severity::Error)
            .iter()
            .filter(|text| *text == LINE_OF_ERRORS)
            .count(),
        1
    );
    assert_eq!(stays[501].severity(), Severity::Warning);
}

#[spec_test(REQ_DS_dsl_018)]
fn kept_when_500_warnings_then_all_stay_and_no_line() {
    let stays = kept(warnings(500));

    assert_eq!(stays.len(), 500);
    assert!(stays.iter().all(Diagnostic::has_code));
}

#[spec_test(REQ_DS_dsl_018)]
fn kept_when_501_warnings_then_500_and_the_line_of_the_limit() {
    let stays = kept(warnings(501));

    assert_eq!(stays.len(), 501);
    let line = &stays[500];
    assert!(!line.has_code());
    assert_eq!(line.severity(), Severity::Warning);
    assert_eq!(line.person_text(), LINE_OF_WARNINGS);
}

#[spec_test(REQ_DS_dsl_018)]
fn kept_when_both_limits_passed_then_each_has_its_line_and_the_other_categories_stay() {
    let information = in_category(error_at(7777), Severity::Information);
    let text_line = in_category(error_at(8888), Severity::Text);
    let mut produced = Vec::new();
    // Interleaved, so that neither limit is passed first by construction.
    for at in 0..600 {
        produced.push(error_at(at));
        produced.push(warning_at(at));
    }
    produced.push(information);
    produced.push(text_line);

    let stays = kept(produced);

    // 500 and the line for each kind, and the two messages of other categories.
    assert_eq!(stays.len(), 500 + 1 + 500 + 1 + 2);
    let texts = lines(&stays);
    assert_eq!(
        texts
            .iter()
            .filter(|(_, text)| text == LINE_OF_ERRORS)
            .count(),
        1
    );
    assert_eq!(
        texts
            .iter()
            .filter(|(_, text)| text == LINE_OF_WARNINGS)
            .count(),
        1
    );
    assert_eq!(stays[stays.len() - 2].severity(), Severity::Information);
    assert_eq!(stays[stays.len() - 1].severity(), Severity::Text);
    assert_eq!(
        counts(&stays),
        Counts {
            errors: 501,
            warnings: 501
        }
    );
}

fn with_related_position(diagnostic: Diagnostic) -> Diagnostic {
    diagnostic.with_secondary(Label::span(SourceSpan::range(1, 2), "Related"))
}

#[spec_test(REQ_DS_dsl_019)]
fn kept_when_error_limit_passed_then_related_positions_of_later_messages_are_dropped() {
    let mut produced: Vec<Diagnostic> = (0..500)
        .map(|at| with_related_position(error_at(at)))
        .collect();
    // The 501st error, and then a warning and an error over the limit.
    produced.push(with_related_position(error_at(500)));
    produced.push(with_related_position(warning_at(501)));
    produced.push(with_related_position(error_at(502)));

    let stays = kept(produced);

    // The first 500 errors keep theirs, the 500th included.
    assert!(stays[..500].iter().all(|d| d.secondary.len() == 1));
    // The line of the limit, then the warning that follows it without any.
    assert!(!stays[500].has_code());
    assert_eq!(stays[501].severity(), Severity::Warning);
    assert!(stays[501].secondary.is_empty());
    assert_eq!(stays.len(), 502);
}

#[spec_test(REQ_DS_dsl_019)]
fn kept_when_error_limit_not_passed_then_related_positions_stay() {
    let produced = vec![
        with_related_position(error_at(1)),
        with_related_position(warning_at(2)),
    ];

    let stays = kept(produced);

    assert!(stays.iter().all(|d| d.secondary.len() == 1));
}

#[spec_test(REQ_DS_dsl_019)]
fn kept_when_only_the_warning_limit_passed_then_related_positions_stay() {
    let mut produced: Vec<Diagnostic> = warnings(501);
    produced.push(with_related_position(error_at(1)));

    let stays = kept(produced);

    assert_eq!(stays.last().map(|d| d.secondary.len()), Some(1));
}

#[spec_test(REQ_DS_dsl_020)]
fn kept_when_applied_to_its_own_result_then_unchanged() {
    let mut produced = errors(600);
    produced.extend(warnings(600));
    let once = kept(produced);

    let twice = kept(once.clone());

    assert_eq!(lines(&twice), lines(&once));
}

#[spec_test(REQ_DS_dsl_020)]
fn kept_when_message_added_after_a_limit_line_then_still_over_the_limit_without_a_second_line() {
    let mut produced = kept(errors(501));
    produced.push(error_at(9000));
    produced.push(warning_at(9001));

    let stays = kept(produced);

    assert_eq!(texts_of(&stays, Severity::Error).len(), 501);
    assert_eq!(
        texts_of(&stays, Severity::Error)
            .iter()
            .filter(|text| *text == LINE_OF_ERRORS)
            .count(),
        1
    );
    assert_eq!(texts_of(&stays, Severity::Warning).len(), 1);
}

#[test]
fn kept_when_fatal_error_among_messages_then_only_the_fatal_errors_stay() {
    let fatal = in_category(error_at(3), Severity::FatalError);

    let stays = kept(vec![error_at(1), warning_at(2), fatal]);

    assert_eq!(stays.len(), 1);
    assert_eq!(stays[0].severity(), Severity::FatalError);
}

#[spec_test(REQ_DS_dsl_021)]
fn counts_when_each_category_then_errors_are_fatal_and_error_and_warnings_are_warning_only() {
    let all = [
        Severity::FatalError,
        Severity::Error,
        Severity::Warning,
        Severity::Information,
        Severity::Text,
        Severity::SuppressedWarning,
        Severity::SuppressedInformation,
    ];
    let diagnostics: Vec<Diagnostic> = all
        .iter()
        .enumerate()
        .map(|(at, severity)| in_category(error_at(at), *severity))
        .collect();

    assert_eq!(
        counts(&diagnostics),
        Counts {
            errors: 2,
            warnings: 1
        }
    );
}

#[spec_test(REQ_DS_dsl_021)]
fn counts_when_a_limit_line_then_it_is_counted() {
    let stays = kept(errors(501));

    assert_eq!(
        counts(&stays),
        Counts {
            errors: 501,
            warnings: 0
        }
    );
}

fn text_lines(closed: &[Diagnostic]) -> Vec<String> {
    closed
        .iter()
        .filter(|d| d.severity() == Severity::Text)
        .map(Diagnostic::person_text)
        .collect()
}

#[spec_test(REQ_DS_dsl_022)]
fn check_summary_close_when_counts_then_compile_complete_with_the_counts_last() {
    for (errors_in, warnings_in) in [(0, 0), (0, 3), (2, 0), (2, 3)] {
        let mut run = errors(errors_in);
        run.extend(warnings(warnings_in));

        let closed = CHECK_SUMMARY.close("app", run);

        assert_eq!(closed.len(), errors_in + warnings_in + 1);
        let last = &closed[closed.len() - 1];
        assert_eq!(last.severity(), Severity::Text);
        assert!(!last.has_code());
        assert_eq!(
            last.person_text(),
            format!("Compile complete -- {errors_in} errors, {warnings_in} warnings")
        );
        assert_eq!(text_lines(&closed).len(), 1);
    }
}

#[spec_test(REQ_DS_dsl_022)]
fn check_summary_close_when_limit_passed_then_the_counts_include_the_limit_line() {
    let closed = CHECK_SUMMARY.close("app", errors(501));

    let last = &closed[closed.len() - 1];
    assert_eq!(
        last.person_text(),
        "Compile complete -- 501 errors, 0 warnings"
    );
}

#[spec_test(REQ_DS_dsl_023)]
fn compile_summary_close_when_no_error_then_started_first_and_ready_for_download_last() {
    let closed = COMPILE_SUMMARY.close("Main", warnings(2));

    assert_eq!(closed.len(), 4);
    assert_eq!(closed[0].severity(), Severity::Text);
    assert!(!closed[0].has_code());
    assert_eq!(
        closed[0].person_text(),
        "------ Build started: Application: Main -------"
    );
    assert_eq!(
        closed[3].person_text(),
        "Build complete -- 0 errors, 2 warnings : Ready for download"
    );
}

#[spec_test(REQ_DS_dsl_023)]
fn compile_summary_close_when_clean_then_ready_for_download_with_zero_counts() {
    let closed = COMPILE_SUMMARY.close("Main", vec![]);

    assert_eq!(closed.len(), 2);
    assert_eq!(
        closed[1].person_text(),
        "Build complete -- 0 errors, 0 warnings : Ready for download"
    );
}

#[spec_test(REQ_DS_dsl_023)]
fn compile_summary_close_when_error_then_no_download_possible() {
    let mut run = errors(2);
    run.extend(warnings(1));

    let closed = COMPILE_SUMMARY.close("Main", run);

    assert_eq!(
        closed[closed.len() - 1].person_text(),
        "Build complete -- 2 errors, 1 warnings : No download possible"
    );
}

#[test]
fn close_when_run_has_text_lines_then_they_are_not_counted() {
    let closed = CHECK_SUMMARY.close("app", vec![in_category(error_at(1), Severity::Text)]);

    assert_eq!(
        closed[closed.len() - 1].person_text(),
        "Compile complete -- 0 errors, 0 warnings"
    );
}
