//! Statement bodies lifted from the legacy parser's own tests.
//!
//! The legacy tests wrap their snippets in a minimal POU. This reads those
//! test sources as text, finds the string literals, and cuts the statement
//! body out of each literal that holds exactly one POU. The result is a
//! table the legacy authors wrote, not one written to suit the new parser.

use ironplc_syntax::lexer::lex;
use ironplc_syntax::SyntaxKind;
use std::path::PathBuf;

/// Every Rust string literal in `source`, decoded. Comments and character
/// literals are skipped so a quote inside either is not read as a string.
pub fn string_literals(source: &str) -> Vec<String> {
    let chars: Vec<char> = source.chars().collect();
    let mut literals = Vec::new();
    let mut at = 0;
    while at < chars.len() {
        match chars[at] {
            '/' if chars.get(at + 1) == Some(&'/') => {
                while at < chars.len() && chars[at] != '\n' {
                    at += 1;
                }
            }
            '/' if chars.get(at + 1) == Some(&'*') => {
                while at < chars.len() && !(chars[at] == '*' && chars.get(at + 1) == Some(&'/')) {
                    at += 1;
                }
                at += 2;
            }
            'r' if raw_start(&chars, at).is_some() => {
                let (hashes, body_start) = raw_start(&chars, at).unwrap_or((0, at));
                let closing: Vec<char> = std::iter::once('"')
                    .chain(std::iter::repeat_n('#', hashes))
                    .collect();
                let mut end = body_start;
                while end < chars.len()
                    && chars[end..].iter().take(closing.len()).ne(closing.iter())
                {
                    end += 1;
                }
                literals.push(chars[body_start..end.min(chars.len())].iter().collect());
                at = end + closing.len();
            }
            '"' => {
                let (text, next) = quoted(&chars, at + 1);
                literals.push(text);
                at = next;
            }
            '\'' => at += char_literal_len(&chars, at),
            _ => at += 1,
        }
    }
    literals
}

/// `r"`, `r#"`, `r##"`: the number of hashes and where the body starts. A
/// bare `r` that is part of a longer word is not a raw string.
fn raw_start(chars: &[char], at: usize) -> Option<(usize, usize)> {
    let preceded_by_word = at
        .checked_sub(1)
        .and_then(|before| chars.get(before))
        .is_some_and(|c| c.is_alphanumeric() || *c == '_');
    if preceded_by_word {
        return None;
    }
    let mut hashes = 0;
    let mut cursor = at + 1;
    while chars.get(cursor) == Some(&'#') {
        hashes += 1;
        cursor += 1;
    }
    (chars.get(cursor) == Some(&'"')).then_some((hashes, cursor + 1))
}

/// A `'x'` or `'\x'` character literal is skipped whole; a lifetime is one
/// character.
fn char_literal_len(chars: &[char], at: usize) -> usize {
    match (chars.get(at + 1), chars.get(at + 2)) {
        (Some('\\'), _) => {
            let mut length = 3;
            while chars.get(at + length).is_some_and(|c| *c != '\'') {
                length += 1;
            }
            length + 1
        }
        (Some(_), Some('\'')) => 3,
        _ => 1,
    }
}

/// The decoded body of a normal string literal starting after its opening
/// quote, and the index after the closing quote.
fn quoted(chars: &[char], mut at: usize) -> (String, usize) {
    let mut text = String::new();
    while at < chars.len() {
        match chars[at] {
            '"' => return (text, at + 1),
            '\\' => {
                at += 1;
                match chars.get(at) {
                    Some('n') => text.push('\n'),
                    Some('r') => text.push('\r'),
                    Some('t') => text.push('\t'),
                    Some('0') => text.push('\0'),
                    Some('\n') => {
                        while chars.get(at + 1).is_some_and(|c| c.is_whitespace()) {
                            at += 1;
                        }
                    }
                    Some('u') => {
                        let mut digits = String::new();
                        at += 2;
                        while chars.get(at).is_some_and(|c| *c != '}') {
                            digits.push(chars[at]);
                            at += 1;
                        }
                        let decoded = u32::from_str_radix(&digits, 16)
                            .ok()
                            .and_then(char::from_u32);
                        text.extend(decoded);
                    }
                    Some(other) => text.push(*other),
                    None => {}
                }
                at += 1;
            }
            c => {
                text.push(c);
                at += 1;
            }
        }
    }
    (text, at)
}

fn is_pou_end(kind: SyntaxKind) -> bool {
    matches!(
        kind,
        SyntaxKind::EndProgram | SyntaxKind::EndFunction | SyntaxKind::EndFunctionBlock
    )
}

/// The statement body of a literal that holds exactly one POU, or `None`.
///
/// The body runs from after the last `END_VAR` (or, with no variable block,
/// after the POU name) to the POU's closing keyword. A `FUNCTION` header has
/// a return type, so its body is only found after a variable block.
pub fn pou_body(literal: &str) -> Option<String> {
    let (tokens, _) = lex(literal);
    let significant: Vec<_> = tokens.iter().filter(|t| !t.kind.is_trivia()).collect();
    let openers: Vec<usize> = significant
        .iter()
        .enumerate()
        .filter(|(_, t)| {
            matches!(
                t.kind,
                SyntaxKind::Program | SyntaxKind::Function | SyntaxKind::FunctionBlock
            )
        })
        .map(|(index, _)| index)
        .collect();
    let closers: Vec<usize> = significant
        .iter()
        .enumerate()
        .filter(|(_, t)| is_pou_end(t.kind))
        .map(|(index, _)| index)
        .collect();
    let ([opener], [closer]) = (&openers[..], &closers[..]) else {
        return None;
    };
    let last_end_var = significant[..*closer]
        .iter()
        .rposition(|t| t.kind == SyntaxKind::EndVar);
    let body_start = match last_end_var {
        Some(index) => usize::from(significant[index].range.end()),
        None if significant[*opener].kind == SyntaxKind::Function => return None,
        None => usize::from(significant.get(opener + 1)?.range.end()),
    };
    let body_end = usize::from(significant[*closer].range.start());
    literal.get(body_start..body_end).map(str::to_string)
}

/// The test sources of the legacy parser crate.
pub fn legacy_test_sources() -> Vec<(PathBuf, String)> {
    let root = PathBuf::from(env!("CARGO_MANIFEST_DIR")).join("src/tests");
    let mut files: Vec<PathBuf> = std::fs::read_dir(root)
        .map(|entries| {
            entries
                .filter_map(|entry| entry.ok().map(|e| e.path()))
                .collect()
        })
        .unwrap_or_default();
    files.sort();
    files
        .into_iter()
        .filter(|path| path.extension().is_some_and(|ext| ext == "rs"))
        .filter_map(|path| std::fs::read_to_string(&path).ok().map(|text| (path, text)))
        .collect()
}

/// True when `body` holds sequential function chart elements, which are
/// not statements: the legacy POU body rule reads them as a network.
fn is_sfc(body: &str) -> bool {
    lex(body).0.iter().any(|token| {
        matches!(
            token.kind,
            SyntaxKind::InitialStep
                | SyntaxKind::EndStep
                | SyntaxKind::Transition
                | SyntaxKind::EndTransition
                | SyntaxKind::Action
                | SyntaxKind::EndAction
        )
    })
}

/// Every distinct non-blank statement body in the legacy parser tests.
pub fn legacy_test_bodies() -> Vec<String> {
    let mut bodies: Vec<String> = legacy_test_sources()
        .iter()
        .flat_map(|(_, text)| string_literals(text))
        .filter_map(|literal| pou_body(&literal))
        .filter(|body| !body.trim().is_empty() && !is_sfc(body))
        .collect();
    bodies.sort();
    bodies.dedup();
    bodies
}

/// Every Rust source file of the legacy parser crate, tests included, except
/// this harness: its tables are not legacy test input.
pub fn legacy_all_sources() -> Vec<(PathBuf, String)> {
    fn collect(dir: &std::path::Path, files: &mut Vec<PathBuf>) {
        let Ok(entries) = std::fs::read_dir(dir) else {
            return;
        };
        for entry in entries.filter_map(|entry| entry.ok()) {
            let path = entry.path();
            if path.is_dir() && path.file_name().is_some_and(|name| name == "parity") {
                continue;
            } else if path.is_dir() {
                collect(&path, files);
            } else if path.extension().is_some_and(|ext| ext == "rs") {
                files.push(path);
            }
        }
    }
    let mut files = Vec::new();
    collect(
        &PathBuf::from(env!("CARGO_MANIFEST_DIR")).join("src"),
        &mut files,
    );
    files.sort();
    files
        .into_iter()
        .filter_map(|path| std::fs::read_to_string(&path).ok().map(|text| (path, text)))
        .collect()
}

/// The keywords that open a top-level declaration.
fn is_declaration_opener(kind: SyntaxKind) -> bool {
    matches!(
        kind,
        SyntaxKind::Program
            | SyntaxKind::Function
            | SyntaxKind::FunctionBlock
            | SyntaxKind::Type
            | SyntaxKind::Configuration
            | SyntaxKind::Interface
            | SyntaxKind::Namespace
            | SyntaxKind::VarGlobal
    )
}

/// Every distinct string literal in the legacy parser crate whose first
/// token opens a declaration: the whole-file snippets the legacy authors
/// wrote, accepted and rejected alike.
/// The string literals of a Rust source whose first token opens a declaration.
fn declaration_literals(text: &str) -> Vec<String> {
    string_literals(text)
        .into_iter()
        .filter(|literal| {
            let (tokens, _) = lex(literal);
            tokens
                .iter()
                .find(|token| !token.kind.is_trivia())
                .is_some_and(|token| is_declaration_opener(token.kind))
        })
        .collect()
}

pub fn legacy_declaration_snippets() -> Vec<String> {
    let mut snippets: Vec<String> = legacy_all_sources()
        .iter()
        .flat_map(|(_, text)| declaration_literals(text))
        .collect();
    snippets.sort();
    snippets.dedup();
    snippets
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn string_literals_when_mixed_forms_then_decoded() {
        let source = "let a = \"x\\n\\\"y\\\"\"; let b = r#\"raw \"q\"\"#; let c = '\"'; // \"no\"\nlet d = \"tail\";";
        assert_eq!(
            string_literals(source),
            vec!["x\n\"y\"", "raw \"q\"", "tail"]
        );
    }

    #[test]
    fn pou_body_when_program_with_vars_then_text_between_end_var_and_end_program() {
        let body = pou_body("PROGRAM p VAR x : INT; END_VAR x := 1; END_PROGRAM");
        assert_eq!(body.as_deref(), Some(" x := 1; "));
    }

    #[test]
    fn declaration_literals_when_type_and_initializer_test_sources_then_each_contributes_declarations(
    ) {
        let sources = legacy_all_sources();
        for name in [
            "arrays.rs",
            "enums.rs",
            "struct_init_expressions.rs",
            "late_resolved_initializers.rs",
            "constant_initializers.rs",
            "type_alias.rs",
            "union.rs",
            "pointer_to.rs",
            "reference_to.rs",
        ] {
            let count = sources
                .iter()
                .filter(|(path, _)| path.file_name().is_some_and(|file| file == name))
                .map(|(_, text)| declaration_literals(text).len())
                .sum::<usize>();
            assert!(count > 0, "{name} contributes no declaration to the corpus");
        }
    }

    #[test]
    fn pou_body_when_two_pous_or_headed_function_without_vars_then_none() {
        assert_eq!(
            pou_body("PROGRAM a END_PROGRAM PROGRAM b END_PROGRAM"),
            None
        );
        assert_eq!(pou_body("FUNCTION f : INT f := 1; END_FUNCTION"), None);
    }
}
