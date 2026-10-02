//! Conditional-compilation pragmas: `{IF}`, `{ELSIF}`, `{ELSE}`, `{END_IF}`,
//! `{DEFINE}` and `{UNDEFINE}`.
//!
//! Pure functions over the text of a `{ ... }` pragma, and the state machine
//! that decides which branch of an `{IF}` is live. The caller owns the token
//! or tree representation; this module only reads pragma text.
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
//! Anything else — `{IF COMPILERVERSION >= 3.5}`, `{IF hastype(x)}` — is a
//! [`Fault::UnexpectedValue`] and evaluates to false. An `{IF}` without its
//! `{END_IF}`, or a stray `{ELSIF}`/`{ELSE}`/`{END_IF}`, is a
//! [`Fault::Unmatched`].

use std::collections::HashSet;

use ironplc_dsl::core::SourceSpan;
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_problems::Problem;

/// A pragma that could not be honoured.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum Fault {
    /// A condition or define name this compiler does not understand.
    UnexpectedValue,
    /// An `{IF}` without its `{END_IF}`, or a stray `{ELSIF}`, `{ELSE}` or
    /// `{END_IF}`.
    Unmatched,
}

impl Fault {
    /// The problem the fault is reported as.
    pub fn problem(self) -> Problem {
        match self {
            Fault::Unmatched => Problem::PragmaIfUnmatched,
            Fault::UnexpectedValue => Problem::PragmaValueExpected,
        }
    }

    /// What is wrong with the pragma.
    pub fn message(self) -> &'static str {
        match self {
            Fault::Unmatched => "Unmatched {IF} pragma",
            Fault::UnexpectedValue => "Expected a pragma condition such as `defined(name)`",
        }
    }

    /// Renders the fault as the compiler diagnostic for the pragma at `span`.
    pub fn diagnostic(self, span: &SourceSpan) -> Diagnostic {
        Diagnostic::problem(self.problem(), Label::span(span.clone(), self.message()))
    }
}

/// What the state machine decided about one pragma.
#[derive(Debug, PartialEq, Eq)]
pub struct Step {
    /// Whether the pragma token itself stays in the output. Conditional
    /// directives are consumed; any other pragma stays only when the branch
    /// it sits in is live.
    pub keep: bool,
    pub fault: Option<Fault>,
}

/// One `{IF}` nesting level.
struct Frame<S> {
    /// Whether the branch currently being scanned is emitted.
    active: bool,
    /// Whether any branch of this `{IF}` has been taken already, so `{ELSIF}`
    /// and `{ELSE}` are skipped.
    taken: bool,
    /// The `{IF}` pragma's origin, for an unmatched-`{IF}` report.
    origin: S,
}

/// The define set and the `{IF}` nesting, advanced one pragma at a time in
/// source order. `S` is whatever the caller uses to say where a pragma is.
pub struct Conditionals<S> {
    defines: HashSet<String>,
    frames: Vec<Frame<S>>,
}

impl<S> Default for Conditionals<S> {
    fn default() -> Self {
        Self {
            defines: HashSet::new(),
            frames: Vec::new(),
        }
    }
}

impl<S> Conditionals<S> {
    pub fn new() -> Self {
        Self::default()
    }

    /// Whether text at this point is emitted: every enclosing branch is live.
    pub fn is_active(&self) -> bool {
        self.frames.iter().all(|frame| frame.active)
    }

    /// Evaluates a condition, recording a fault when it is not understood.
    fn condition(&self, condition: Result<String, ()>, fault: &mut Option<Fault>) -> bool {
        match condition.and_then(|text| eval(&text, &self.defines)) {
            Ok(value) => value,
            Err(()) => {
                *fault = Some(Fault::UnexpectedValue);
                false
            }
        }
    }

    /// Advances over the pragma `text` found at `origin`.
    pub fn step(&mut self, text: &str, origin: S) -> Step {
        let mut fault = None;
        let keep = match directive(text) {
            Directive::If(condition) => {
                let active = self.condition(condition, &mut fault);
                self.frames.push(Frame {
                    active,
                    taken: active,
                    origin,
                });
                false
            }
            Directive::Elsif(condition) => {
                match self.frames.last().map(|frame| frame.taken) {
                    Some(false) => {
                        let active = self.condition(condition, &mut fault);
                        if let Some(frame) = self.frames.last_mut() {
                            frame.active = active;
                            frame.taken = active;
                        }
                    }
                    Some(true) => {
                        if let Some(frame) = self.frames.last_mut() {
                            frame.active = false;
                        }
                    }
                    None => fault = Some(Fault::Unmatched),
                }
                false
            }
            Directive::Else => {
                match self.frames.last_mut() {
                    Some(frame) => {
                        frame.active = !frame.taken;
                        frame.taken = true;
                    }
                    None => fault = Some(Fault::Unmatched),
                }
                false
            }
            Directive::EndIf => {
                if self.frames.pop().is_none() {
                    fault = Some(Fault::Unmatched);
                }
                false
            }
            Directive::Define(name) => {
                self.change_define(name, &mut fault, |defines, name| {
                    defines.insert(name);
                });
                false
            }
            Directive::Undefine(name) => {
                self.change_define(name, &mut fault, |defines, name| {
                    defines.remove(&name);
                });
                false
            }
            // Any other pragma (`{attribute ...}`) is trivia, kept or dropped
            // with the branch it sits in.
            Directive::Other => self.is_active(),
        };
        Step { keep, fault }
    }

    fn change_define(
        &mut self,
        name: Option<String>,
        fault: &mut Option<Fault>,
        change: impl FnOnce(&mut HashSet<String>, String),
    ) {
        match name {
            Some(name) => {
                if self.is_active() {
                    change(&mut self.defines, name);
                }
            }
            None => *fault = Some(Fault::UnexpectedValue),
        }
    }

    /// The origin of the outermost `{IF}` that never met its `{END_IF}`:
    /// everything after it was evaluated as though it were still inside the
    /// branch. Reported once, by the caller, when the input ends.
    pub fn unclosed(&self) -> Option<&S> {
        self.frames.first().map(|frame| &frame.origin)
    }
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
    let valid = name.split('.').all(|part| {
        !part.is_empty() && part.chars().all(|c| c.is_ascii_alphanumeric() || c == '_')
    });
    valid.then(|| name.to_ascii_lowercase())
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

#[cfg(test)]
mod tests {
    use super::*;

    fn run(pragmas: &[&str]) -> Vec<Step> {
        let mut conditionals = Conditionals::new();
        pragmas
            .iter()
            .map(|text| conditionals.step(text, ()))
            .collect()
    }

    #[test]
    fn eval_when_defined_name_then_true() {
        let defines: HashSet<String> = ["a.b".to_string()].into();
        assert_eq!(eval("defined(A.B) and not false", &defines), Ok(true));
    }

    #[test]
    fn eval_when_unknown_condition_then_err() {
        assert_eq!(eval("hastype(x)", &HashSet::new()), Err(()));
    }

    #[test]
    fn eval_when_unbalanced_parenthesis_then_err() {
        assert_eq!(eval("(true", &HashSet::new()), Err(()));
    }

    #[test]
    fn step_when_define_then_if_defined_is_live() {
        let mut conditionals = Conditionals::new();
        conditionals.step("{DEFINE Foo}", ());
        conditionals.step("{IF defined(foo)}", ());
        assert!(conditionals.is_active());
        conditionals.step("{ELSE}", ());
        assert!(!conditionals.is_active());
        conditionals.step("{END_IF}", ());
        assert!(conditionals.unclosed().is_none());
    }

    #[test]
    fn step_when_stray_end_if_then_unmatched() {
        let steps = run(&["{END_IF}"]);
        assert_eq!(steps[0].fault, Some(Fault::Unmatched));
    }

    #[test]
    fn step_when_unclosed_if_then_reports_origin() {
        let mut conditionals = Conditionals::new();
        conditionals.step("{IF true}", 7);
        assert_eq!(conditionals.unclosed(), Some(&7));
    }

    #[test]
    fn step_when_other_pragma_in_dead_branch_then_dropped() {
        let steps = run(&["{IF false}", "{attribute x}", "{END_IF}", "{attribute y}"]);
        assert!(!steps[1].keep);
        assert!(steps[3].keep);
    }

    #[test]
    fn step_when_define_without_name_then_unexpected_value() {
        let steps = run(&["{DEFINE}"]);
        assert_eq!(steps[0].fault, Some(Fault::UnexpectedValue));
    }
}
