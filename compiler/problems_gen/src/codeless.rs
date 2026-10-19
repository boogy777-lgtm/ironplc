//! The table of messages that have no problem code, `codeless-texts.csv`, and
//! the module `codeless` generated from it: one function for each row.
//!
//! A message with a code takes its category from its problem and its text from
//! `problem-texts.csv`. A message without a code has no problem to take either
//! from, so its row declares both: the category and the template.

use super::{declarable_severity, is_snake_case, parse_template};

/// The header `codeless-texts.csv` must have.
pub const CODELESS_TEXTS_HEADER: [&str; 3] = ["Key", "Severity", "Template"];

/// One row of `codeless-texts.csv`, checked.
struct CodelessDef {
    /// The function that makes the text, and the identity of the message.
    key: String,
    /// The name of the `Severity` variant the row declares.
    severity: &'static str,
    /// The template as written in the row.
    template: String,
    /// The template with the escaped braces resolved.
    literal: String,
    /// The distinct placeholders in order of first appearance.
    params: Vec<String>,
}

/// Reads and checks the rows of `codeless-texts.csv`.
fn read_codeless(src: &str) -> Result<Vec<CodelessDef>, String> {
    let mut reader = csv::Reader::from_reader(src.as_bytes());
    let header = reader.headers().map_err(|e| e.to_string())?.clone();
    if header.iter().collect::<Vec<_>>() != CODELESS_TEXTS_HEADER {
        return Err(format!(
            "the header of codeless-texts.csv is {header:?}, but it must be {}",
            CODELESS_TEXTS_HEADER.join(",")
        ));
    }

    let mut defs: Vec<CodelessDef> = vec![];
    for result in reader.records() {
        let record = result.map_err(|e| e.to_string())?;
        let field = |index: usize| record.get(index).unwrap_or_default().to_string();
        let (key, severity, template) = (field(0), field(1), field(2));
        let line = record.position().map_or(0, |p| p.line());
        let row = format!("codeless-texts.csv line {line} ('{key}')");

        if !is_snake_case(&key) {
            return Err(format!(
                "{row}: the key is not snake_case ([a-z][a-z0-9]*(_[a-z0-9]+)*, not a Rust keyword)"
            ));
        }
        if defs.iter().any(|def| def.key == key) {
            return Err(format!("{row}: the key is used by an earlier row"));
        }
        let severity = declarable_severity(&severity).map_err(|e| format!("{row}: {e}"))?;
        let (literal, params) = parse_template(&template).map_err(|e| format!("{row}: {e}"))?;
        defs.push(CodelessDef {
            key,
            severity,
            template,
            literal,
            params,
        });
    }
    Ok(defs)
}

/// The module `codeless`: one function for each row. Like the functions of the
/// module `text`, with placeholders the body is a `format!` of the template, so
/// rustc checks that every placeholder has a parameter and every parameter a
/// placeholder.
fn render_codeless(defs: &[CodelessDef]) -> String {
    let mut out = String::from("pub mod codeless {\n    use super::{CodelessText, Severity};\n");
    for def in defs {
        out.push_str(&format!(
            "\n    /// {}\n    ///\n    /// ```text\n",
            def.severity
        ));
        for line in def.template.lines() {
            out.push_str(&format!("    /// {line}\n"));
        }
        out.push_str("    /// ```\n");
        let params: Vec<String> = def
            .params
            .iter()
            .map(|p| format!("{p}: impl std::fmt::Display"))
            .collect();
        let message = if def.params.is_empty() {
            format!("{:?}.to_string()", def.literal)
        } else {
            format!("format!({:?})", def.template)
        };
        out.push_str(&format!(
            "    pub fn {}({}) -> CodelessText {{\n        CodelessText::new({:?}, Severity::{}, {message})\n    }}\n",
            def.key,
            params.join(", "),
            def.key,
            def.severity,
        ));
    }
    out.push_str("}\n");
    out
}

/// The source of the module `codeless`: one function for each row of `src` (the
/// text of `codeless-texts.csv`). The error names the line and the key of the row
/// that is not valid.
pub fn generate_codeless_texts(src: &str) -> Result<String, String> {
    read_codeless(src).map(|defs| render_codeless(&defs))
}
