//! Conditional-compilation pragmas: `{IF}`, `{ELSIF}`, `{ELSE}`, `{END_IF}`,
//! `{DEFINE}` and `{UNDEFINE}`.
//!
//! The token stream this runs on is the one `xform_collapse_pragmas` produced,
//! so every `{ ... }` is a single [`TokenType::Pragma`] token whose text
//! includes the braces. The reference evaluates these in its parser
//! (`PragmaIfStatementParser` and its operand parsers); the token level is the
//! natural home here, because a branch that is not taken must not reach the
//! grammar at all: with the pragmas treated as trivia, both branches of an
//! `{IF}` would otherwise be compiled.
//!
//! The condition language is the subset the reference accepts that can be
//! answered without a project database:
//!
//! ```text
//! condition = operand { "OR" operand } ;
//! operand   = operand { "AND" operand } ;
//! operand   = "(" condition ")" | "NOT" operand | "defined" "(" name ")" | "TRUE" | "FALSE" ;
//! ```
//!
//! `{DEFINE name}` adds a name, `{UNDEFINE name}` removes it, and both take
//! effect for the conditions that follow them, in source order. Names are
//! compared case-insensitively, as identifiers are.
//!
//! Anything else — `{IF COMPILERVERSION >= 3.5}`, `{IF hastype(x)}` — is
//! reported as P0024 and evaluates to false. An `{IF}` without its `{END_IF}`,
//! or a stray `{ELSIF}`/`{ELSE}`/`{END_IF}`, is reported as P0023.
//!
//! Only runs when `options.allow_pragma_if` is set; without it the pragmas stay
//! trivia and both branches reach the parser, exactly as before.

use std::collections::HashSet;

use dsl::core::SourceSpan;
use dsl::diagnostic::{Diagnostic, Label};

use crate::options::CompilerOptions;
use crate::token::{Token, TokenType};

pub fn apply(tokens: Vec<Token>, options: &CompilerOptions) -> (Vec<Token>, Vec<Diagnostic>) {
    if !options.allow_pragma_if {
        return (tokens, Vec::new());
    }

    let mut output = Vec::with_capacity(tokens.len());
    let mut diagnostics = Vec::new();
    let mut defines: HashSet<String> = HashSet::new();
    let mut frames: Vec<Frame> = Vec::new();

    for tok in tokens {
        if tok.token_type != TokenType::Pragma {
            if frames.iter().all(|frame| frame.active) {
                output.push(tok);
            }
            continue;
        }

        let span = tok.span.clone();
        match directive(&tok.text) {
            Directive::If(condition) => {
                let active = match &condition {
                    Ok(condition) => eval(condition, &defines).unwrap_or_else(|_| {
                        diagnostics.push(unexpected_value(&span));
                        false
                    }),
                    Err(()) => {
                        diagnostics.push(unexpected_value(&span));
                        false
                    }
                };
                frames.push(Frame {
                    active,
                    taken: active,
                    span: span.clone(),
                });
            }
            Directive::Elsif(condition) => match frames.last_mut() {
                Some(frame) if !frame.taken => {
                    frame.active = match &condition {
                        Ok(condition) => eval(condition, &defines).unwrap_or_else(|_| {
                            diagnostics.push(unexpected_value(&span));
                            false
                        }),
                        Err(()) => {
                            diagnostics.push(unexpected_value(&span));
                            false
                        }
                    };
                    frame.taken = frame.active;
                }
                Some(frame) => frame.active = false,
                None => diagnostics.push(unmatched(&span)),
            },
            Directive::Else => match frames.last_mut() {
                Some(frame) => {
                    frame.active = !frame.taken;
                    frame.taken = true;
                }
                None => diagnostics.push(unmatched(&span)),
            },
            Directive::EndIf => {
                if frames.pop().is_none() {
                    diagnostics.push(unmatched(&span));
                }
            }
            Directive::Define(name) => {
                if let Some(name) = name {
                    if frames.iter().all(|frame| frame.active) {
                        defines.insert(name);
                    }
                } else {
                    diagnostics.push(unexpected_value(&span));
                }
            }
            Directive::Undefine(name) => {
                if let Some(name) = name {
                    if frames.iter().all(|frame| frame.active) {
                        defines.remove(&name);
                    }
                } else {
                    diagnostics.push(unexpected_value(&span));
                }
            }
            // Any other pragma (`{attribute ...}`) is trivia, kept or dropped
            // with the branch it sits in.
            Directive::Other => {
                if frames.iter().all(|frame| frame.active) {
                    output.push(tok);
                }
            }
        }

    }

    // An `{IF}` that never met its `{END_IF}` is reported once, at its own
    // span: everything after it was evaluated as though it were still inside
    // the branch.
    if let Some(frame) = frames.first() {
        diagnostics.push(unmatched(&frame.span));
    }

    (output, diagnostics)
}

/// One `{IF}` nesting level.
struct Frame {
    /// Whether the branch currently being scanned is emitted.
    active: bool,
    /// Whether any branch of this `{IF}` has been taken already, so `{ELSIF}`
    /// and `{ELSE}` are skipped.
    taken: bool,
    /// The `{IF}` pragma's span, for an unmatched-`{IF}` diagnostic.
    span: SourceSpan,
}

/// What a pragma token asks the transform to do.
enum Directive {
    If(Result<String, ()>),
    Elsif(Result<String, ()>),
    Else,
    EndIf,
    Define(Option<String>),
    Undefine(Option<String>),
    Other,
}

/// Reads the leading keyword of a `{ ... }` pragma and the rest of its text.
fn directive(text: &str) -> Directive {
    let body = text
        .strip_prefix('{')
        .and_then(|t| t.strip_suffix('}'))
        .unwrap_or(text)
        .trim();
    let (keyword, rest) = match body.find(char::is_whitespace) {
        Some(index) => (&body[..index], body[index..].trim()),
        None => (body, ""),
    };

    match keyword.to_ascii_uppercase().as_str() {
        "IF" => Directive::If(non_empty(rest)),
        "ELSIF" => Directive::Elsif(non_empty(rest)),
        "ELSE" => Directive::Else,
        "END_IF" => Directive::EndIf,
        "DEFINE" => Directive::Define(symbol(rest)),
        "UNDEFINE" => Directive::Undefine(symbol(rest)),
        _ => Directive::Other,
    }
}

fn non_empty(text: &str) -> Result<String, ()> {
    if text.is_empty() {
        Err(())
    } else {
        Ok(text.to_string())
    }
}

/// Reads a define's name. The reference's `{DEFINE}` takes the name alone
/// (a second token is a value, which this compiler does not model yet), so
/// only a single identifier or dotted name is accepted.
fn symbol(text: &str) -> Option<String> {
    let name = text.trim();
    if name.is_empty() {
        return None;
    }
    let valid = name
        .split('.')
        .all(|part| !part.is_empty() && part.chars().all(|c| c.is_ascii_alphanumeric() || c == '_'));
    valid.then(|| name.to_ascii_lowercase())
}

fn unmatched(span: &SourceSpan) -> Diagnostic {
    Diagnostic::problem(
        ironplc_problems::Problem::PragmaIfUnmatched,
        Label::span(span.clone(), "Unmatched {IF} pragma"),
    )
}

fn unexpected_value(span: &SourceSpan) -> Diagnostic {
    Diagnostic::problem(
        ironplc_problems::Problem::PragmaValueExpected,
        Label::span(
            span.clone(),
            "Expected a pragma condition such as `defined(name)`",
        ),
    )
}

/// Evaluates a pragma condition against the defines in scope. `Err(())` means
/// the text is not a condition this compiler understands.
fn eval(condition: &str, defines: &HashSet<String>) -> Result<bool, ()> {
    let mut parser = ConditionParser {
        text: condition,
        pos: 0,
        defines,
    };
    let value = parser.or_expression()?;
    parser.skip_whitespace();
    if parser.pos == parser.text.len() {
        Ok(value)
    } else {
        Err(())
    }
}

struct ConditionParser<'a> {
    text: &'a str,
    pos: usize,
    defines: &'a HashSet<String>,
}

impl ConditionParser<'_> {
    fn skip_whitespace(&mut self) {
        while let Some(c) = self.text[self.pos..].chars().next() {
            if c.is_whitespace() {
                self.pos += c.len_utf8();
            } else {
                break;
            }
        }
    }

    /// Reads the next word (`defined`, `not`, ...), lowercased.
    fn next_word(&mut self) -> Option<String> {
        self.skip_whitespace();
        let rest = &self.text[self.pos..];
        let len = rest
            .chars()
            .take_while(|c| c.is_ascii_alphanumeric() || *c == '_')
            .map(|c| c.len_utf8())
            .sum();
        if len == 0 {
            return None;
        }
        let word = rest[..len].to_ascii_lowercase();
        self.pos += len;
        Some(word)
    }

    fn eat(&mut self, expected: char) -> bool {
        self.skip_whitespace();
        if self.text[self.pos..].starts_with(expected) {
            self.pos += expected.len_utf8();
            true
        } else {
            false
        }
    }

    fn or_expression(&mut self) -> Result<bool, ()> {
        let mut value = self.and_expression()?;
        loop {
            self.skip_whitespace();
            let start = self.pos;
            match self.next_word().as_deref() {
                Some("or") => value |= self.and_expression()?,
                _ => {
                    self.pos = start;
                    return Ok(value);
                }
            }
        }
    }

    fn and_expression(&mut self) -> Result<bool, ()> {
        let mut value = self.operand()?;
        loop {
            self.skip_whitespace();
            let start = self.pos;
            match self.next_word().as_deref() {
                Some("and") => value &= self.operand()?,
                _ => {
                    self.pos = start;
                    return Ok(value);
                }
            }
        }
    }

    fn operand(&mut self) -> Result<bool, ()> {
        self.skip_whitespace();
        if self.eat('(') {
            let value = self.or_expression()?;
            if !self.eat(')') {
                return Err(());
            }
            return Ok(value);
        }

        let start = self.pos;
        match self.next_word().as_deref() {
            Some("not") => Ok(!self.operand()?),
            Some("true") => Ok(true),
            Some("false") => Ok(false),
            Some("defined") => {
                if !self.eat('(') {
                    return Err(());
                }
                let Some(mut name) = self.next_word() else {
                    return Err(());
                };
                while self.eat('.') {
                    let Some(part) = self.next_word() else {
                        return Err(());
                    };
                    name.push('.');
                    name.push_str(&part);
                }
                if !self.eat(')') {
                    return Err(());
                }
                Ok(self.defines.contains(&name))
            }
            _ => {
                self.pos = start;
                Err(())
            }
        }
    }
}
