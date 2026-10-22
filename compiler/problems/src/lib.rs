//! Provides compiler error definitions.
//!
//! Three files in `resources/` are read when this crate is built:
//!
//! - `problem-codes.csv` makes the [`Problem`] enumeration: the code, the name,
//!   the one generic message and the [`Severity`] of each problem. A row
//!   without a valid severity fails the build.
//! - `problem-texts.csv` makes the module [`text`]: one function for each row,
//!   which makes the text a label shows from the arguments the row's template
//!   names. A call with a missing, an extra or a misspelled argument does not
//!   compile.
//! - `codeless-texts.csv` makes the module [`codeless`]: the same for a message
//!   that has no problem code (a limit line, a summary line). Its rows declare
//!   the [`Severity`] too, because there is no problem to take it from.
//!
//! The type [`WarningProblem`] is generated from the same table: one value for
//! each problem that declares the category Warning, the only problems a setting
//! can disable or raise. [`WarningSet`] holds them and reads a code written in a
//! setting.

include!(concat!(env!("OUT_DIR"), "/problems.rs"));

include!(concat!(env!("OUT_DIR"), "/texts.rs"));

include!(concat!(env!("OUT_DIR"), "/codeless.rs"));

mod warning_set;

pub use warning_set::WarningSet;

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

/// The code and the message, `P0013: ...`, for a place that has a line of text and
/// no diagnostic: the usage error of a command, the log of the language server.
impl std::fmt::Display for Text {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}: {}", self.problem.code(), self.message)
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

/// The text of a message that has no problem code, together with its category
/// and the key of its row in `codeless-texts.csv`, made by a function of the
/// module [`codeless`].
///
/// The row declares the category, so a codeless message cannot be paired with a
/// category other than its row's; the key is the identity of the message in
/// place of a code.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct CodelessText {
    key: &'static str,
    severity: Severity,
    message: String,
}

impl CodelessText {
    /// The only way to make a text is a function of the module [`codeless`],
    /// which is a child of this module and so sees this function.
    fn new(key: &'static str, severity: Severity, message: String) -> Self {
        CodelessText {
            key,
            severity,
            message,
        }
    }

    /// The key of the row, which identifies the message.
    pub fn key(&self) -> &'static str {
        self.key
    }

    /// The category the row declares.
    pub fn severity(&self) -> Severity {
        self.severity
    }

    /// The text with the arguments in place of the placeholders.
    pub fn message(&self) -> &str {
        &self.message
    }

    /// Splits the text into its key, its category and its message.
    pub fn into_parts(self) -> (&'static str, Severity, String) {
        (self.key, self.severity, self.message)
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

    /// The name and the value a program reads for each category, in the CODESYS
    /// scripting model (`ScriptSystem.Severity`), and the header word of a
    /// person's form. No wildcard arm, as in `expected`.
    fn forms(severity: Severity) -> (&'static str, u32, Option<&'static str>) {
        match severity {
            Severity::FatalError => ("FatalError", 1, Some("Error")),
            Severity::Error => ("Error", 2, Some("Error")),
            Severity::Warning => ("Warning", 4, Some("Warning")),
            Severity::Information => ("Information", 8, Some("Information")),
            Severity::Text => ("Text", 16, None),
            Severity::SuppressedWarning => ("SuppressedWarning", 32, None),
            Severity::SuppressedInformation => ("SuppressedInformation", 64, None),
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

    #[spec_test(REQ_DS_problems_008)]
    fn severity_script_name_and_value_when_each_category_then_those_of_codesys_scripting() {
        for severity in ALL {
            let (name, value, _) = forms(severity);
            assert_eq!(severity.script_name(), name, "{severity:?}");
            assert_eq!(severity.script_value(), value, "{severity:?}");
        }
    }

    /// What a setting that raises a warning to an error makes of each category.
    /// No wildcard: a category added to the list does not compile here until the
    /// test knows what raising does to it.
    fn raised_to(severity: Severity) -> Severity {
        match severity {
            Severity::FatalError => Severity::FatalError,
            Severity::Error => Severity::Error,
            Severity::Warning => Severity::Error,
            Severity::Information => Severity::Information,
            Severity::Text => Severity::Text,
            Severity::SuppressedWarning => Severity::SuppressedWarning,
            Severity::SuppressedInformation => Severity::SuppressedInformation,
        }
    }

    #[spec_test(REQ_DS_problems_013)]
    fn severity_raised_when_each_category_then_only_a_warning_becomes_an_error() {
        for severity in ALL {
            assert_eq!(severity.raised(), raised_to(severity), "{severity:?}");
            // Raising an error changes nothing, so nothing lowers one.
            assert_eq!(
                severity.raised().raised(),
                severity.raised(),
                "{severity:?}"
            );
        }
        assert!(Severity::Warning.raised().stops_build());
    }

    #[spec_test(REQ_DS_problems_009)]
    fn severity_person_word_when_each_category_then_the_word_of_the_messages_view() {
        for severity in ALL {
            assert_eq!(severity.person_word(), forms(severity).2, "{severity:?}");
        }
    }

    #[spec_test(REQ_DS_problems_010)]
    fn generate_codeless_texts_when_row_is_not_valid_then_error_names_line_and_key() {
        let header = "Key,Severity,Template
";
        for (rows, needle) in [
            ("a_line,,A", "''"),
            ("a_line,error,A", "'error'"),
            ("a_line,SuppressedWarning,A", "'SuppressedWarning'"),
            ("A_Line,Text,A", "snake_case"),
            (
                "a_line,Text,A
a_line,Text,B",
                "used by an earlier row",
            ),
            ("a_line,Text,{X}", "{X}"),
        ] {
            let message = ironplc_msg_gen::generate_codeless_texts(&format!("{header}{rows}"))
                .expect_err("the row should be refused");

            assert!(
                message.contains("'a_line'") || message.contains("A_Line"),
                "{message}"
            );
            assert!(message.contains("line "), "{message}");
            assert!(message.contains(needle), "{rows}: {message}");
        }
        let message = ironplc_msg_gen::generate_codeless_texts(
            "Code,Key,Template
",
        )
        .expect_err("the header should be refused");
        assert!(message.contains("Key,Severity,Template"), "{message}");
        assert!(ironplc_msg_gen::generate_codeless_texts(header).is_ok());
    }

    #[spec_test(REQ_DS_problems_011)]
    fn codeless_when_each_row_then_the_key_the_category_and_the_text_of_codesys() {
        let rows = [
            (
                codeless::error_limit_exceeded(500),
                "error_limit_exceeded",
                Severity::Error,
                "More than 500 errors occured: Skipping all further error messages",
            ),
            (
                codeless::warning_limit_exceeded(500),
                "warning_limit_exceeded",
                Severity::Warning,
                "More than 500 warnings occured: Skipping all further warning messages",
            ),
            (
                codeless::build_started("Main"),
                "build_started",
                Severity::Text,
                "------ Build started: Application: Main -------",
            ),
            (
                codeless::build_complete_ok(0, 3),
                "build_complete_ok",
                Severity::Text,
                "Build complete -- 0 errors, 3 warnings : Ready for download",
            ),
            (
                codeless::build_complete_errors(2, 1),
                "build_complete_errors",
                Severity::Text,
                "Build complete -- 2 errors, 1 warnings : No download possible",
            ),
            (
                codeless::compile_complete_ok(0, 0),
                "compile_complete_ok",
                Severity::Text,
                "Compile complete -- 0 errors, 0 warnings",
            ),
            (
                codeless::compile_complete_errors(4, 5),
                "compile_complete_errors",
                Severity::Text,
                "Compile complete -- 4 errors, 5 warnings",
            ),
        ];

        for (text, key, severity, message) in rows {
            assert_eq!(text.key(), key);
            assert_eq!(text.severity(), severity, "{key}");
            assert_eq!(text.message(), message, "{key}");
        }
    }

    /// The lines about the build record are IronPLC's own, not CODESYS text: the
    /// exact CODESYS wording of "up to date" is not verified.
    #[test]
    fn codeless_when_build_record_rows_then_key_the_category_and_the_text() {
        let rows = [
            (
                codeless::build_up_to_date(),
                "build_up_to_date",
                "The application is up to date",
            ),
            (
                codeless::record_malformed("app.build.iplcinfo"),
                "record_malformed",
                "The record app.build.iplcinfo cannot be used and is rewritten after the build",
            ),
        ];

        for (text, key, message) in rows {
            assert_eq!(text.key(), key);
            assert_eq!(text.severity(), Severity::Text, "{key}");
            assert_eq!(text.message(), message, "{key}");
        }
    }

    #[spec_test(REQ_DS_problems_004)]
    fn severity_variant_when_row_has_no_declarable_category_then_error_names_code_and_value() {
        for severity in ALL {
            let declarable = ironplc_msg_gen::severity_variant("P0001", &format!("{severity:?}"));
            let suppressed_result = matches!(
                severity,
                Severity::SuppressedWarning | Severity::SuppressedInformation
            );
            assert_eq!(declarable.is_ok(), !suppressed_result, "{severity:?}");
        }
        for value in ["", "error", "Warn", "Info", "Fatal"] {
            let message = ironplc_msg_gen::severity_variant("P0042", value)
                .expect_err("the value should be refused");

            assert!(message.contains("P0042"), "'{value}': {message}");
            assert!(
                message.contains(&format!("'{value}'")),
                "'{value}': {message}"
            );
        }
    }

    #[spec_test(REQ_DS_problems_005)]
    fn check_problem_codes_header_when_not_the_five_columns_then_error() {
        assert_eq!(
            ironplc_msg_gen::check_problem_codes_header(&[
                "Code", "Name", "Message", "Severity", "Codesys"
            ]),
            Ok(())
        );
        for header in [
            vec!["Code", "Name", "Message", "Severity"],
            vec!["Code", "Name", "Message", "Severity", "Counterpart"],
            vec!["Code", "Name", "Severity", "Message", "Codesys"],
        ] {
            assert!(ironplc_msg_gen::check_problem_codes_header(&header).is_err());
        }
    }

    #[spec_test(REQ_DS_problems_006)]
    fn codesys_when_problem_of_the_csv_then_the_code_of_its_row() {
        // A row with a confirmed counterpart, one that names two problems of
        // one CODESYS code, and rows with none.
        assert_eq!(Problem::VariableUndefined.codesys(), Some("C0046"));
        assert_eq!(
            Problem::FunctionInvocationMissingInput.codesys(),
            Some("C0037")
        );
        assert_eq!(
            Problem::FunctionCallNamedArgUndeclared.codesys(),
            Problem::FunctionInvocationMissingInput.codesys()
        );
        assert_eq!(Problem::SyntaxError.codesys(), None);
        assert_eq!(Problem::McpInputValidation.codesys(), None);
    }

    #[spec_test(REQ_DS_problems_007)]
    fn codesys_code_when_value_is_not_c_and_four_digits_then_error_names_code_and_value() {
        assert_eq!(ironplc_msg_gen::codesys_code("P0001", ""), Ok(None));
        assert_eq!(
            ironplc_msg_gen::codesys_code("P4007", "C0046"),
            Ok(Some("C0046".to_string()))
        );
        for value in ["C046", "C00460", "c0046", "P0046", "C00x6", "0046"] {
            let message = ironplc_msg_gen::codesys_code("P0042", value)
                .expect_err("the value should be refused");

            assert!(message.contains("P0042"), "'{value}': {message}");
            assert!(
                message.contains(&format!("'{value}'")),
                "'{value}': {message}"
            );
        }
    }
}
