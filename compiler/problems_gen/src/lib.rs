//! Build-script helper of `ironplc-problems`: checks `problem-texts.csv` and
//! generates the module `text` (one function for each row), and checks the
//! `Severity` and `Codesys` columns of `problem-codes.csv`.
//!
//! It does no file access: `problems/build.rs` reads the files and writes the
//! result, so every check has a unit test here (Cargo does not run the tests of
//! a build script).

// Problems whose "location" is the compiler's own source rather than the
// IEC 61131-3 program. These must be constructed through the dedicated
// `Diagnostic` constructors (`todo*`/`not_implemented`/`internal_error*`)
// so that the compiler `file#Lline` is recorded and the P9xxx telemetry
// dashboards can rank them. The variants are marked `#[deprecated]` and the
// workspace denies the `deprecated` lint, so any direct use of the variant
// fails to compile.
const COMPILER_LOCATED: &[(&str, &str)] = &[
    (
        "NotSupported",
        "construct via Diagnostic::not_supported so the compiler file/line is recorded for the P9xxx dashboards",
    ),
    (
        "NotImplemented",
        "construct via Diagnostic::not_implemented (or todo*) so the compiler file/line is recorded for the P9xxx dashboards",
    ),
    (
        "InternalError",
        "construct via Diagnostic::internal_error / internal_error_at so the compiler file/line is recorded for the P9xxx dashboards",
    ),
];

pub fn deprecation_note(name: &str) -> Option<&'static str> {
    COMPILER_LOCATED
        .iter()
        .find(|(n, _)| *n == name)
        .map(|(_, note)| *note)
}

/// The header `problem-codes.csv` must have. `problems/build.rs` reads the
/// columns by position, so the order is part of the format.
pub const PROBLEM_CODES_HEADER: [&str; 5] = ["Code", "Name", "Message", "Severity", "Codesys"];

/// One category of a message, with every fact the compiler asks of it.
struct Level {
    /// The variant of `Severity` and the value of the `Severity` column.
    name: &'static str,
    /// The text of the category where a program reads it, such as the
    /// `severity` of a diagnostic in a JSON response.
    wire: &'static str,
    /// The value of the category in the CODESYS `Severity` flag set
    /// (`Severity.cs`), which a program reads as `severity_value` and combines
    /// into a mask of categories.
    flag: u32,
    /// The header word of a person's form, taken from the CODESYS Messages view,
    /// or `None` for a category that has no header: a text is a line without a
    /// word, and a suppressed category is not shown.
    word: Option<&'static str>,
    doc: &'static str,
    /// Whether a row of `problem-codes.csv` may name the category. A suppressed
    /// category is only the result of a suppression, never a declaration.
    declarable: bool,
    /// Whether a diagnostic of the category stops the build.
    stops_build: bool,
    /// Whether a diagnostic of the category is shown.
    shown: bool,
    /// The category a diagnostic of this one becomes when it is suppressed. A
    /// category that cannot be suppressed names itself.
    suppressed: &'static str,
}

/// The seven message categories of CODESYS (`Severity.cs`), in its order. This
/// is the one list: `problems/build.rs` generates `enum Severity` and the
/// match of each question from it, and a row of `problem-codes.csv` may name
/// the declarable ones. A level added here does not compile in a consumer that
/// maps `Severity` with an exhaustive `match` until the consumer handles it.
const LEVELS: &[Level] = &[
    Level {
        name: "FatalError",
        wire: "fatal-error",
        flag: 1,
        word: Some("Error"),
        doc: "Stops the build and the stages after it, and the run keeps only the fatal diagnostics.",
        declarable: true,
        stops_build: true,
        shown: true,
        suppressed: "FatalError",
    },
    Level {
        name: "Error",
        wire: "error",
        flag: 2,
        word: Some("Error"),
        doc: "The program is not valid. It stops the build.",
        declarable: true,
        stops_build: true,
        shown: true,
        suppressed: "Error",
    },
    Level {
        name: "Warning",
        wire: "warning",
        flag: 4,
        word: Some("Warning"),
        doc: "The program is valid but probably not what was meant. It is shown and does not stop the build.",
        declarable: true,
        stops_build: false,
        shown: true,
        suppressed: "SuppressedWarning",
    },
    Level {
        name: "Information",
        wire: "information",
        flag: 8,
        word: Some("Information"),
        doc: "A note about a valid program. It is shown and does not stop the build.",
        declarable: true,
        stops_build: false,
        shown: true,
        suppressed: "SuppressedInformation",
    },
    Level {
        name: "Text",
        wire: "text",
        flag: 16,
        word: None,
        doc: "A line of text about the build, such as progress, with no code. It does not stop the build.",
        declarable: true,
        stops_build: false,
        shown: true,
        suppressed: "Text",
    },
    Level {
        name: "SuppressedWarning",
        wire: "suppressed-warning",
        flag: 32,
        word: None,
        doc: "A warning that is disabled. It is not shown and does not stop the build.",
        declarable: false,
        stops_build: false,
        shown: false,
        suppressed: "SuppressedWarning",
    },
    Level {
        name: "SuppressedInformation",
        wire: "suppressed-information",
        flag: 64,
        word: None,
        doc: "An information message that is disabled. It is not shown and does not stop the build.",
        declarable: false,
        stops_build: false,
        shown: false,
        suppressed: "SuppressedInformation",
    },
];

/// One question the compiler asks of a category: the name of the function, its
/// documentation, its return type and the arm for a level.
struct Question {
    function: &'static str,
    doc: &'static str,
    returns: &'static str,
    arm: fn(&Level) -> String,
}

const QUESTIONS: &[Question] = &[
    Question {
        function: "script_name",
        doc: "The name of the category in the CODESYS scripting model (`ScriptSystem.Severity`),
    /// which a program reads as the `severity` of a message.",
        returns: "&'static str",
        arm: |level| format!("\"{}\"", level.name),
    },
    Question {
        function: "script_value",
        doc: "The value of the category in the CODESYS `Severity` flag set: 1, 2, 4, 8, 16, 32 and
    /// 64, which a program reads as the `severity_value` of a message.",
        returns: "u32",
        arm: |level| level.flag.to_string(),
    },
    Question {
        function: "person_word",
        doc: "The header word of a person's form, as in the CODESYS Messages view: `Error` for a
    /// fatal error and an error, `Warning` and `Information`. A text and a suppressed
    /// category have none.",
        returns: "Option<&'static str>",
        arm: |level| level.word.map_or("None".to_string(), |word| format!("Some(\"{word}\")")),
    },
    Question {
        function: "name",
        doc: "The text of the category where a program reads it, such as the `severity` of a\n    /// diagnostic in a JSON response.",
        returns: "&'static str",
        arm: |level| format!("\"{}\"", level.wire),
    },
    Question {
        function: "stops_build",
        doc: "Whether a diagnostic of the category stops the build.",
        returns: "bool",
        arm: |level| level.stops_build.to_string(),
    },
    Question {
        function: "is_shown",
        doc: "Whether a diagnostic of the category is shown.",
        returns: "bool",
        arm: |level| level.shown.to_string(),
    },
    Question {
        function: "suppressed",
        doc: "The category a diagnostic of this one becomes when it is suppressed: a warning and\n    /// an information message have a suppressed category and every other category stays.",
        returns: "Severity",
        arm: |level| format!("Severity::{}", level.suppressed),
    },
];

/// The source of `enum Severity` and of its questions, each an exhaustive
/// `match` with an arm for every category.
pub fn render_severity_enum() -> String {
    let mut out = String::from(concat!(
        "/// The message category of a diagnostic, the seven of CODESYS. Every problem\n",
        "/// code declares its category in the `Severity` column of `problem-codes.csv`.\n",
        "#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]\n",
        "pub enum Severity {\n",
    ));
    for level in LEVELS {
        out.push_str(&format!("    /// {}\n    {},\n", level.doc, level.name));
    }
    out.push_str("}\n\nimpl Severity {\n");
    for (index, question) in QUESTIONS.iter().enumerate() {
        if index > 0 {
            out.push('\n');
        }
        out.push_str(&format!(
            "    /// {}\n    pub fn {}(&self) -> {} {{\n        match self {{\n",
            question.doc, question.function, question.returns
        ));
        for level in LEVELS {
            out.push_str(&format!(
                "            Severity::{} => {},\n",
                level.name,
                (question.arm)(level)
            ));
        }
        out.push_str("        }\n    }\n");
    }
    out.push_str("}\n\n");
    out
}

/// The name of the `Severity` variant a row names, or the error that names the
/// row. The build fails on a missing value, an unknown one or a suppressed
/// category, so a code cannot be added without deciding whether it stops the
/// build.
pub fn severity_variant(code: &str, value: &str) -> Result<&'static str, String> {
    declarable_severity(value).map_err(|e| format!("problem-codes.csv {code}: {e}"))
}

/// The name of the `Severity` variant a declaration names, or the error that
/// lists the categories a declaration may name. Both tables that declare a
/// category (`problem-codes.csv` and `codeless-texts.csv`) ask this.
fn declarable_severity(value: &str) -> Result<&'static str, String> {
    LEVELS
        .iter()
        .find(|level| level.declarable && level.name == value)
        .map(|level| level.name)
        .ok_or_else(|| {
            let allowed: Vec<&str> = LEVELS
                .iter()
                .filter(|level| level.declarable)
                .map(|level| level.name)
                .collect();
            format!(
                "the severity '{value}' is not one of {}",
                allowed.join(", ")
            )
        })
}

/// The CODESYS code a row names, `None` for a row that names none, or the error
/// that names the row. A CODESYS code is `C` and four digits, the prefix and the
/// number of a `MessageId` (`C0046`); the build fails on any other value, so a
/// misspelled code cannot reach the output of a person or a program.
pub fn codesys_code(code: &str, value: &str) -> Result<Option<String>, String> {
    if value.is_empty() {
        return Ok(None);
    }
    let digits = value.strip_prefix('C').unwrap_or_default();
    if digits.len() == 4 && digits.chars().all(|c| c.is_ascii_digit()) {
        Ok(Some(value.to_string()))
    } else {
        Err(format!(
            "problem-codes.csv {code}: the CODESYS code '{value}' is not C and four digits (such as C0046), or empty"
        ))
    }
}

/// Checks the header of `problem-codes.csv`.
pub fn check_problem_codes_header(header: &[&str]) -> Result<(), String> {
    if header == PROBLEM_CODES_HEADER {
        Ok(())
    } else {
        Err(format!(
            "the header of problem-codes.csv is {header:?}, but it must be {}",
            PROBLEM_CODES_HEADER.join(",")
        ))
    }
}

/// Words a placeholder or a key cannot be: each becomes a parameter or a
/// function name of the generated code.
const RUST_KEYWORDS: &[&str] = &[
    "abstract", "as", "async", "await", "become", "box", "break", "const", "continue", "crate",
    "do", "dyn", "else", "enum", "extern", "false", "final", "fn", "for", "gen", "if", "impl",
    "in", "let", "loop", "macro", "match", "mod", "move", "mut", "override", "priv", "pub", "ref",
    "return", "self", "static", "struct", "super", "trait", "true", "try", "type", "typeof",
    "unsafe", "unsized", "use", "virtual", "where", "while", "yield",
];

/// One row of `problem-texts.csv`, checked.
struct TextDef {
    /// The code the text belongs to.
    code: String,
    /// The `Problem` variant of that code.
    name: String,
    /// The function that makes the text.
    key: String,
    /// The template as written in the row.
    template: String,
    /// The template with the escaped braces resolved: the text itself when the
    /// template has no placeholder.
    literal: String,
    /// The distinct placeholders in order of first appearance.
    params: Vec<String>,
}

/// Whether `name` is `[a-z][a-z0-9]*(_[a-z0-9]+)*` and not a Rust keyword.
fn is_snake_case(name: &str) -> bool {
    name.split('_').all(|part| {
        !part.is_empty()
            && part
                .chars()
                .all(|c| c.is_ascii_lowercase() || c.is_ascii_digit())
    }) && name.starts_with(|c: char| c.is_ascii_lowercase())
        && !RUST_KEYWORDS.contains(&name)
}

/// The snake_case of an enum variant name: a word starts at an uppercase letter
/// that follows a lowercase letter or a digit, and at the last uppercase letter
/// of a run that is followed by a lowercase letter (`XmlIoError` is `xml_io_error`).
/// A digit continues the word before it (`Utf8Bom` is `utf8_bom`).
fn snake_case_of_name(name: &str) -> String {
    let chars: Vec<char> = name.chars().collect();
    let mut out = String::new();
    for (i, c) in chars.iter().enumerate() {
        if c.is_ascii_uppercase() && i > 0 {
            let after_lower_or_digit =
                chars[i - 1].is_ascii_lowercase() || chars[i - 1].is_ascii_digit();
            let ends_acronym = chars[i - 1].is_ascii_uppercase()
                && chars
                    .get(i + 1)
                    .is_some_and(|next| next.is_ascii_lowercase());
            if after_lower_or_digit || ends_acronym {
                out.push('_');
            }
        }
        out.push(c.to_ascii_lowercase());
    }
    out
}

/// Whether `name` is a placeholder name: `[a-z][a-z0-9_]*`, not a Rust keyword.
fn is_placeholder_name(name: &str) -> bool {
    name.starts_with(|c: char| c.is_ascii_lowercase())
        && name
            .chars()
            .all(|c| c.is_ascii_lowercase() || c.is_ascii_digit() || c == '_')
        && !RUST_KEYWORDS.contains(&name)
}

/// Reads a template: `{name}` is a placeholder, `{{` and `}}` are the literal
/// braces, and any other brace is an error. Returns the template with the
/// escapes resolved and the distinct placeholders in order of first appearance.
fn parse_template(template: &str) -> Result<(String, Vec<String>), String> {
    let mut literal = String::new();
    let mut params: Vec<String> = vec![];
    let mut chars = template.chars().peekable();
    while let Some(c) = chars.next() {
        match c {
            '{' if chars.peek() == Some(&'{') => {
                chars.next();
                literal.push('{');
            }
            '}' if chars.peek() == Some(&'}') => {
                chars.next();
                literal.push('}');
            }
            '{' => {
                let mut name = String::new();
                loop {
                    match chars.next() {
                        Some('}') => break,
                        Some('{') | None => {
                            return Err(format!("the placeholder '{{{name}' is not closed"));
                        }
                        Some(c) => name.push(c),
                    }
                }
                if !is_placeholder_name(&name) {
                    return Err(format!(
                        "the placeholder '{{{name}}}' is not a name [a-z][a-z0-9_]* (a format spec and a Rust keyword are not allowed)"
                    ));
                }
                if !params.contains(&name) {
                    params.push(name);
                }
            }
            '}' => {
                return Err(
                    "a '}' does not close a placeholder; write '}}' for the character".to_string(),
                );
            }
            c => literal.push(c),
        }
    }
    Ok((literal, params))
}

/// Reads and checks the rows of `problem-texts.csv` against the problems of
/// `problem-codes.csv`, given as `(code, name)`.
fn read_texts(src: &str, problems: &[(&str, &str)]) -> Result<Vec<TextDef>, String> {
    let mut reader = csv::Reader::from_reader(src.as_bytes());
    let header = reader.headers().map_err(|e| e.to_string())?.clone();
    if header.iter().collect::<Vec<_>>() != ["Code", "Key", "Template"] {
        return Err(format!(
            "the header of problem-texts.csv is {header:?}, but it must be Code,Key,Template"
        ));
    }

    let mut texts: Vec<TextDef> = vec![];
    for result in reader.records() {
        let record = result.map_err(|e| e.to_string())?;
        let field = |index: usize| record.get(index).unwrap_or_default().to_string();
        let (code, key, template) = (field(0), field(1), field(2));
        let line = record.position().map_or(0, |p| p.line());
        let row = format!("problem-texts.csv line {line} ({code}, '{key}')");

        let (_, name) = problems
            .iter()
            .find(|(problem_code, _)| *problem_code == code)
            .ok_or_else(|| format!("{row}: the code is not in problem-codes.csv"))?;
        if deprecation_note(name).is_some() {
            return Err(format!(
                "{row}: {name} is a compiler-located problem and has its own constructors on Diagnostic"
            ));
        }
        if !is_snake_case(&key) {
            return Err(format!(
                "{row}: the key is not snake_case ([a-z][a-z0-9]*(_[a-z0-9]+)*, not a Rust keyword)"
            ));
        }
        if texts.iter().any(|t| t.key == key) {
            return Err(format!("{row}: the key is used by an earlier row"));
        }
        let prefix = snake_case_of_name(name);
        if key != prefix && !key.starts_with(&format!("{prefix}_")) {
            return Err(format!(
                "{row}: the key must be '{prefix}' (the name {name} in snake_case) or start with '{prefix}_'"
            ));
        }
        let (literal, params) = parse_template(&template).map_err(|e| format!("{row}: {e}"))?;
        texts.push(TextDef {
            code,
            name: (*name).to_string(),
            key,
            template,
            literal,
            params,
        });
    }
    Ok(texts)
}

/// The module `text`: one function for each row.
///
/// With placeholders the body is a `format!` of the template and the names of
/// the placeholders are the parameters, so rustc checks that every placeholder
/// has a parameter and every parameter a placeholder.
fn render_texts(texts: &[TextDef]) -> String {
    let mut out = String::from("pub mod text {\n    use super::{Problem, Text};\n");
    for text in texts {
        out.push_str(&format!(
            "\n    /// {} ({})\n    ///\n    /// ```text\n",
            text.code, text.name
        ));
        for line in text.template.lines() {
            out.push_str(&format!("    /// {line}\n"));
        }
        out.push_str("    /// ```\n");
        let params: Vec<String> = text
            .params
            .iter()
            .map(|p| format!("{p}: impl std::fmt::Display"))
            .collect();
        let message = if text.params.is_empty() {
            format!("{:?}.to_string()", text.literal)
        } else {
            format!("format!({:?})", text.template)
        };
        out.push_str(&format!(
            "    pub fn {}({}) -> Text {{\n        Text::new(Problem::{}, {message})\n    }}\n",
            text.key,
            params.join(", "),
            text.name,
        ));
    }
    out.push_str("}\n");
    out
}

/// The source of the module `text`: one function for each row of `src` (the
/// text of `problem-texts.csv`). `problems` is the `(code, name)` of every row
/// of `problem-codes.csv`. The error names the line, the code and the key of the
/// row that is not valid.
pub fn generate_texts(src: &str, problems: &[(&str, &str)]) -> Result<String, String> {
    read_texts(src, problems).map(|texts| render_texts(&texts))
}

mod codeless;
pub use codeless::{generate_codeless_texts, CODELESS_TEXTS_HEADER};

#[cfg(test)]
mod tests;
