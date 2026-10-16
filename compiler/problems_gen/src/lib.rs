//! Build-script helper of `ironplc-problems`: checks `problem-texts.csv` and
//! generates the module `text` (one function for each row), and checks the
//! `Severity` column of `problem-codes.csv`.
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
pub const PROBLEM_CODES_HEADER: [&str; 4] = ["Code", "Name", "Message", "Severity"];

/// The levels of `Severity`, each with the documentation of its variant. This
/// is the one declaration of a level: `problems/build.rs` generates the enum
/// from it, and a row of `problem-codes.csv` may name exactly these.
const SEVERITIES: &[(&str, &str)] = &[
    (
        "Error",
        "The program is not valid. Compilation stops after the analysis that found it.",
    ),
    (
        "Warning",
        "The program is valid but probably not what was meant. It is shown and does not stop compilation.",
    ),
    (
        "Info",
        "A note about a valid program. It is shown and does not stop compilation.",
    ),
];

/// The source of `enum Severity` and of `Severity::name`, with a variant and an
/// arm for each level.
pub fn render_severity_enum() -> String {
    let mut out = String::from(concat!(
        "/// How serious a diagnostic is. Every problem code declares its level in the\n",
        "/// `Severity` column of `problem-codes.csv`.\n",
        "#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]\n",
        "pub enum Severity {\n",
    ));
    for (name, doc) in SEVERITIES {
        out.push_str(&format!("    /// {doc}\n    {name},\n"));
    }
    out.push_str(concat!(
        "}\n\n",
        "impl Severity {\n",
        "    /// The name the level has where a program reads it as text, such as the\n",
        "    /// `severity` of a diagnostic in a JSON response: the variant in lower case.\n",
        "    pub fn name(&self) -> &'static str {\n",
        "        match self {\n",
    ));
    for (name, _) in SEVERITIES {
        out.push_str(&format!(
            "            Severity::{name} => \"{}\",\n",
            name.to_ascii_lowercase()
        ));
    }
    out.push_str("        }\n    }\n}\n\n");
    out
}

/// The name of the `Severity` variant a row names, or the error that names the
/// row. The build fails on a missing or an unknown value, so a code cannot be
/// added without deciding whether it stops the build.
pub fn severity_variant(code: &str, value: &str) -> Result<&'static str, String> {
    SEVERITIES
        .iter()
        .find(|(name, _)| *name == value)
        .map(|(name, _)| *name)
        .ok_or_else(|| {
            let allowed: Vec<&str> = SEVERITIES.iter().map(|(name, _)| *name).collect();
            format!(
                "problem-codes.csv {code}: the severity '{value}' is not one of {}",
                allowed.join(", ")
            )
        })
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

#[cfg(test)]
mod tests;
