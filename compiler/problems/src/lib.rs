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
    fn severity_when_problem_of_the_csv_then_the_level_of_its_row() {
        assert_eq!(Problem::SyntaxError.severity(), Severity::Error);
        assert_eq!(Problem::AttributeRepeated.severity(), Severity::Error);
    }
}
