use std::{
    env,
    error::Error,
    fs::{self, File},
    io::Write,
    path::PathBuf,
    process,
};

struct ProblemDef {
    /// The code that users know this as. This should remain stable
    /// between releases to facilitate consistent documentation.
    code: String,
    /// The internal name that this error is known as. This makes for
    /// easy reading, but we don't promise that this remains consistent
    /// between releases.
    name: String,
    /// A message describing the type of error.
    message: String,
}

// Problems whose "location" is the compiler's own source rather than the
// IEC 61131-3 program. These must be constructed through the dedicated
// `Diagnostic` constructors (`todo*`/`not_implemented`/`internal_error*`)
// so that the compiler `file#Lline` is recorded and the P9xxx telemetry
// dashboards can rank them. The variants are marked `#[deprecated]` and the
// workspace denies the `deprecated` lint, so any direct
// `Diagnostic::problem(Problem::NotImplemented, …)` fails to compile.
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

fn deprecation_note(name: &str) -> Option<&'static str> {
    COMPILER_LOCATED
        .iter()
        .find(|(n, _)| *n == name)
        .map(|(_, note)| *note)
}

fn create_problems() -> Result<(), Box<dyn Error>> {
    // Tell Cargo that if the error definitions change, to rerun this build script.
    println!("cargo:rerun-if-changed=resources/problem-codes.csv");
    println!("cargo:rerun-if-changed=resources/problem-texts.csv");

    // Read the problem-codes.csv into bytes so that we can use it with
    let mut src_path = PathBuf::from(env!("CARGO_MANIFEST_DIR"));
    src_path.push("resources");
    src_path.push("problem-codes.csv");

    let src = fs::read_to_string(src_path).expect("Unable to read 'problem-codes.csv'");
    let src = src.as_bytes();

    // Read the file into the definition (we'll iterate over the structs more than once)
    let mut defs = vec![];
    let mut rdr = csv::Reader::from_reader(src);
    for result in rdr.records() {
        let record = result?;
        let code = record
            .get(0)
            .ok_or_else(|| format!("Record {record:?} is not valid at column 0"))?;
        let name = record
            .get(1)
            .ok_or_else(|| format!("Record {record:?} is not valid at column 1"))?;
        let message = record
            .get(2)
            .ok_or_else(|| format!("Record {record:?} is not valid at column 2"))?;
        defs.push(ProblemDef {
            code: code.to_string(),
            name: name.to_string(),
            message: message.to_string(),
        });
    }

    // Create the output directory and file problems.rs that will have the definitions
    let mut out_path = PathBuf::from(env::var("OUT_DIR")?);
    fs::create_dir_all(out_path.clone())
        .map_err(|e| format!("Unable to create directory 'problems': {e}"))?;

    out_path.push("problems.rs");
    let mut out =
        File::create(out_path).map_err(|e| format!("Unable to create 'problems.rs': {e}"))?;

    // Create the enumeration definition
    out.write_all(b"pub enum Problem {\n")?;
    for def in &defs {
        if let Some(note) = deprecation_note(&def.name) {
            out.write_all(format!("    #[deprecated(note = \"{note}\")]\n").as_bytes())?;
        }
        out.write_all(format!("    {},\n", def.name).as_bytes())?;
    }
    out.write_all(b"}\n\n")?;

    // Create the function to return information about each definition. The impl
    // allows `deprecated` because `code()`/`message()` legitimately match every
    // variant, including the compiler-located ones marked above.
    out.write_all(b"#[expect(deprecated, reason = \"the impl names every variant, including the deprecated compiler-located ones\")]\n")?;
    out.write_all(b"impl Problem {\n")?;

    // Define code()
    out.write_all(b"    /// Returns the code for the particular problem as a string.\n")?;
    out.write_all(b"    pub fn code(&self) -> &str {\n")?;
    out.write_all(b"        match self {\n")?;
    for def in &defs {
        out.write_all(
            format!("            Problem::{} => \"{}\",\n", def.name, def.code).as_bytes(),
        )?;
    }
    out.write_all(b"        }\n")?;
    out.write_all(b"    }\n\n")?;

    // Define message()
    out.write_all(b"    /// Returns the message for the particular problem as a string.\n")?;
    out.write_all(b"    /// The message is constant and does not depend on the particular instance of the problem.\n")?;
    out.write_all(b"    pub fn message(&self) -> &str {\n")?;
    out.write_all(b"        match self {\n")?;
    for def in &defs {
        out.write_all(
            format!(
                "            Problem::{} => \"{}\",\n",
                def.name, def.message
            )
            .as_bytes(),
        )?;
    }
    out.write_all(b"        }\n")?;
    out.write_all(b"    }\n")?;

    out.write_all(b"}\n")?;

    // Finalize the out file
    out.flush()?;

    create_texts(&defs)
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
/// `problem-codes.csv`.
fn read_texts(src: &str, defs: &[ProblemDef]) -> Result<Vec<TextDef>, String> {
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

        let def = defs
            .iter()
            .find(|d| d.code == code)
            .ok_or_else(|| format!("{row}: the code is not in problem-codes.csv"))?;
        if deprecation_note(&def.name).is_some() {
            return Err(format!(
                "{row}: {} is a compiler-located problem and has its own constructors on Diagnostic",
                def.name
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
        let (literal, params) = parse_template(&template).map_err(|e| format!("{row}: {e}"))?;
        texts.push(TextDef {
            code,
            name: def.name.clone(),
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

fn create_texts(defs: &[ProblemDef]) -> Result<(), Box<dyn Error>> {
    let mut src_path = PathBuf::from(env!("CARGO_MANIFEST_DIR"));
    src_path.push("resources");
    src_path.push("problem-texts.csv");
    let src = fs::read_to_string(src_path)
        .map_err(|e| format!("Unable to read 'problem-texts.csv': {e}"))?;

    let texts = read_texts(&src, defs)?;

    let mut out_path = PathBuf::from(env::var("OUT_DIR")?);
    out_path.push("texts.rs");
    fs::write(out_path, render_texts(&texts))
        .map_err(|e| format!("Unable to create 'texts.rs': {e}"))?;
    Ok(())
}

fn main() {
    if let Err(err) = create_problems() {
        eprintln!("problem generating problems.rs: {err}");
        process::exit(1);
    }
}
