//! Provides compiler error definitions.
//!
//! Two files in `resources/` are read when this crate is built:
//!
//! - `problem-codes.csv` makes the [`Problem`] enumeration: the code, the name,
//!   the one generic message and the [`Severity`] of each problem. A row
//!   without a valid severity fails the build.
//! - `problem-texts.csv` makes the module [`text`]: one function for each row,
//!   which makes the text a label shows from the arguments the row's template
//!   names. A call with a missing, an extra or a misspelled argument does not
//!   compile.

include!(concat!(env!("OUT_DIR"), "/problems.rs"));

include!(concat!(env!("OUT_DIR"), "/texts.rs"));

// Spec conformance testing infrastructure (test-only).
#[cfg(test)]
mod spec_requirements {
    include!(concat!(env!("OUT_DIR"), "/spec_requirements.rs"));
}

/// The text of a label together with the problem it describes, made by a
/// function of the module [`text`] from one row of `problem-texts.csv`.
///
/// The row names the code, so a text cannot be paired with the wrong problem.
#[derive(Clone, PartialEq, Eq)]
pub struct Text {
    problem: Problem,
    message: String,
}

impl Text {
    /// The only way to make a text is a function of the module [`text`], which
    /// is a child of this module and so sees this function.
    fn new(problem: Problem, message: String) -> Self {
        Text { problem, message }
    }

    /// The problem the row belongs to.
    pub fn problem(&self) -> &Problem {
        &self.problem
    }

    /// The text with the arguments in place of the placeholders.
    pub fn message(&self) -> &str {
        &self.message
    }

    /// Splits the text into its problem and its message.
    pub fn into_parts(self) -> (Problem, String) {
        (self.problem, self.message)
    }
}

impl From<Text> for String {
    fn from(text: Text) -> Self {
        text.message
    }
}

impl std::fmt::Debug for Text {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("Text")
            .field("code", &self.problem.code())
            .field("message", &self.message)
            .finish()
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use spec_test_macro::spec_test;

    #[test]
    fn text_when_row_with_placeholders_then_the_problem_of_the_row_and_the_arguments_in_place() {
        let text = text::attribute_value_invalid_found("pack_mode", "one of 0, 1, 2, 4, 8", 3);

        assert_eq!(text.problem().code(), "P4077");
        assert_eq!(
            text.message(),
            "The attribute 'pack_mode' takes one of 0, 1, 2, 4, 8, but its value is '3'"
        );
    }

    #[test]
    fn text_when_row_without_placeholder_then_the_template_as_written() {
        let text = text::attribute_repeated_first_written_here();

        assert_eq!(text.problem().code(), "P4078");
        assert_eq!(text.message(), "First written here");
    }

    #[test]
    fn text_when_argument_is_not_a_string_then_its_display_is_used() {
        let text = text::attribute_repeated(String::from("a"));

        assert_eq!(
            text.message(),
            "The attribute 'a' is written again for the same declaration"
        );
    }

    #[test]
    fn into_parts_when_text_then_problem_and_message() {
        let (problem, message) = text::attribute_repeated_first_written_here().into_parts();

        assert_eq!(problem.code(), "P4078");
        assert_eq!(message, "First written here");
    }

    #[test]
    fn from_text_when_string_wanted_then_the_message() {
        let message: String = text::attribute_repeated_first_written_here().into();

        assert_eq!(message, "First written here");
    }

    #[test]
    fn debug_when_text_then_code_and_message() {
        let shown = format!("{:?}", text::attribute_repeated_first_written_here());

        assert!(shown.contains("P4078") && shown.contains("First written here"));
    }

    #[test]
    fn all_spec_requirements_have_tests() {
        assert!(
            crate::spec_requirements::UNTESTED.is_empty(),
            "Requirements in spec with no conformance test: {:?}",
            crate::spec_requirements::UNTESTED
        );
    }

    /// The seven categories in the order of CODESYS, each with the answers to
    /// the questions asked of it: its name, whether it stops the build, whether
    /// it is shown and what it becomes when suppressed. The `match` has no
    /// wildcard: a category added to the list does not compile here until the
    /// test knows it.
    fn expected(severity: Severity) -> (&'static str, bool, bool, Severity) {
        match severity {
            Severity::FatalError => ("fatal-error", true, true, Severity::FatalError),
            Severity::Error => ("error", true, true, Severity::Error),
            Severity::Warning => ("warning", false, true, Severity::SuppressedWarning),
            Severity::Information => ("information", false, true, Severity::SuppressedInformation),
            Severity::Text => ("text", false, true, Severity::Text),
            Severity::SuppressedWarning => (
                "suppressed-warning",
                false,
                false,
                Severity::SuppressedWarning,
            ),
            Severity::SuppressedInformation => (
                "suppressed-information",
                false,
                false,
                Severity::SuppressedInformation,
            ),
        }
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

    #[spec_test(REQ_DS_problems_001)]
    fn severity_when_problem_of_the_csv_then_the_category_of_its_row() {
        assert_eq!(Problem::SyntaxError.severity(), Severity::Error);
        assert_eq!(Problem::AttributeRepeated.severity(), Severity::Error);
        assert_eq!(Problem::UnexpectedToken.severity(), Severity::Error);
    }

    #[spec_test(REQ_DS_problems_002)]
    fn severity_name_when_each_category_then_its_text() {
        for severity in ALL {
            assert_eq!(severity.name(), expected(severity).0);
        }
    }

    #[spec_test(REQ_DS_problems_003)]
    fn severity_questions_when_each_category_then_the_answers_of_codesys() {
        for severity in ALL {
            let (_, stops_build, is_shown, suppressed) = expected(severity);
            assert_eq!(severity.stops_build(), stops_build, "{severity:?}");
            assert_eq!(severity.is_shown(), is_shown, "{severity:?}");
            assert_eq!(severity.suppressed(), suppressed, "{severity:?}");
            // A suppressed category cannot be suppressed again.
            assert_eq!(
                severity.suppressed().suppressed(),
                severity.suppressed(),
                "{severity:?}"
            );
        }
    }

    #[spec_test(REQ_DS_problems_004)]
    fn severity_variant_when_row_has_no_declarable_category_then_error_names_code_and_value() {
        for severity in ALL {
            let declarable =
                ironplc_problems_gen::severity_variant("P0001", &format!("{severity:?}"));
            let suppressed_result = matches!(
                severity,
                Severity::SuppressedWarning | Severity::SuppressedInformation
            );
            assert_eq!(declarable.is_ok(), !suppressed_result, "{severity:?}");
        }
        for value in ["", "error", "Warn", "Info", "Fatal"] {
            let message = ironplc_problems_gen::severity_variant("P0042", value)
                .expect_err("the value should be refused");

            assert!(message.contains("P0042"), "'{value}': {message}");
            assert!(
                message.contains(&format!("'{value}'")),
                "'{value}': {message}"
            );
        }
    }

    #[spec_test(REQ_DS_problems_005)]
    fn check_problem_codes_header_when_not_the_four_columns_then_error() {
        assert_eq!(
            ironplc_problems_gen::check_problem_codes_header(&[
                "Code", "Name", "Message", "Severity"
            ]),
            Ok(())
        );
        for header in [
            vec!["Code", "Name", "Message"],
            vec!["Code", "Name", "Severity", "Message"],
            vec!["Code", "Name", "Message", "Level"],
        ] {
            assert!(ironplc_problems_gen::check_problem_codes_header(&header).is_err());
        }
    }
}
