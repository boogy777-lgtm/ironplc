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

    /// The name of a level, by a `match` without a wildcard: a level added to
    /// the list of levels does not compile here until the test knows it.
    fn level_name(severity: Severity) -> &'static str {
        match severity {
            Severity::Error => "Error",
            Severity::Warning => "Warning",
            Severity::Info => "Info",
        }
    }

    #[spec_test(REQ_DS_problems_001)]
    fn severity_when_problem_of_the_csv_then_the_level_of_its_row() {
        assert_eq!(Problem::SyntaxError.severity(), Severity::Error);
        assert_eq!(Problem::AttributeRepeated.severity(), Severity::Error);
        assert_eq!(Problem::UnexpectedToken.severity(), Severity::Error);
        assert_eq!(level_name(Problem::SyntaxError.severity()), "Error");
        assert_eq!(level_name(Severity::Warning), "Warning");
        assert_eq!(level_name(Severity::Info), "Info");
        assert_eq!(Severity::Error.name(), "error");
        assert_eq!(Severity::Warning.name(), "warning");
        assert_eq!(Severity::Info.name(), "info");
    }

    #[spec_test(REQ_DS_problems_002)]
    fn severity_variant_when_row_has_no_valid_level_then_error_names_code_and_value() {
        for level in ["Error", "Warning", "Info"] {
            assert_eq!(
                ironplc_problems_gen::severity_variant("P0001", level),
                Ok(level)
            );
        }
        for value in ["", "error", "Warn", "Information", "Fatal"] {
            let message = ironplc_problems_gen::severity_variant("P0042", value)
                .expect_err("the value should be refused");

            assert!(message.contains("P0042"), "'{value}': {message}");
            assert!(
                message.contains(&format!("'{value}'")),
                "'{value}': {message}"
            );
        }
    }

    #[spec_test(REQ_DS_problems_003)]
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
