//! Pragmas: what the text of a `{ ... }` says ([`kind`]), and the
//! conditional-compilation pragmas `{IF}`, `{ELSIF}`, `{ELSE}`, `{END_IF}`,
//! `{DEFINE}` and `{UNDEFINE}` ([`Conditionals`]).
//!
//! Pure functions over the text of a pragma, and the state machine that
//! decides which branch of an `{IF}` is live. The caller owns the token or
//! tree representation; this module is the only place that reads pragma text,
//! so that no second reader of the braces and the keyword exists.
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

use crate::error::nesting_message;
use crate::parser::MAX_DEPTH;

/// A pragma that could not be honoured.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum Fault {
    /// A condition or define name this compiler does not understand.
    UnexpectedValue,
    /// An `{IF}` without its `{END_IF}`, or a stray `{ELSIF}`, `{ELSE}` or
    /// `{END_IF}`.
    Unmatched,
    /// A condition nested deeper than [`MAX_DEPTH`], the limit the tree has.
    NestingTooDeep,
}

impl Fault {
    /// The problem the fault is reported as.
    pub fn problem(self) -> Problem {
        match self {
            Fault::Unmatched => Problem::PragmaIfUnmatched,
            Fault::UnexpectedValue => Problem::PragmaValueExpected,
            Fault::NestingTooDeep => Problem::MaxNestingDepthExceeded,
        }
    }

    /// What is wrong with the pragma.
    pub fn message(self) -> String {
        match self {
            Fault::Unmatched => "Unmatched {IF} pragma".to_string(),
            Fault::UnexpectedValue => {
                "Expected a pragma condition such as `defined(name)`".to_string()
            }
            Fault::NestingTooDeep => nesting_message(),
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
        match condition
            .map_err(|()| Fault::UnexpectedValue)
            .and_then(|text| eval(&text, &self.defines))
        {
            Ok(value) => value,
            Err(found) => {
                *fault = Some(found);
                false
            }
        }
    }

    /// Advances over the pragma `text` found at `origin`.
    pub fn step(&mut self, text: &str, origin: S) -> Step {
        let mut fault = None;
        let keep = match kind(text) {
            Kind::Directive(directive) => self.directive(directive, origin, &mut fault),
            // Every pragma that is not a directive (`{attribute ...}`,
            // `{warning ...}`, any other) is trivia, kept or dropped with the
            // branch it sits in. A kind added to `Kind` is trivia until a
            // reader of it says otherwise.
            _ => self.is_active(),
        };
        Step { keep, fault }
    }

    /// Applies one conditional directive. Whether the directive token stays
    /// in the output is always false: a directive is consumed.
    fn directive(&mut self, directive: Directive, origin: S, fault: &mut Option<Fault>) -> bool {
        match directive {
            Directive::If(condition) => {
                let active = self.condition(condition, fault);
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
                        let active = self.condition(condition, fault);
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
                    None => *fault = Some(Fault::Unmatched),
                }
                false
            }
            Directive::Else => {
                match self.frames.last_mut() {
                    Some(frame) => {
                        frame.active = !frame.taken;
                        frame.taken = true;
                    }
                    None => *fault = Some(Fault::Unmatched),
                }
                false
            }
            Directive::EndIf => {
                if self.frames.pop().is_none() {
                    *fault = Some(Fault::Unmatched);
                }
                false
            }
            Directive::Define(name) => {
                self.change_define(name, fault, |defines, name| {
                    defines.insert(name);
                });
                false
            }
            Directive::Undefine(name) => {
                self.change_define(name, fault, |defines, name| {
                    defines.remove(&name);
                });
                false
            }
        }
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

/// A conditional directive: a pragma the state machine ([`Conditionals`])
/// consumes.
#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Directive {
    If(Result<String, ()>),
    Elsif(Result<String, ()>),
    Else,
    EndIf,
    Define(Option<String>),
    Undefine(Option<String>),
}

/// How a `{text}`, `{info}`, `{warning}` or `{error}` pragma asks to be shown.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Level {
    Text,
    Info,
    Warning,
    Error,
}

/// What the text of a pragma says. The one reading of pragma text: every
/// reader asks [`kind`] and none reads the braces or the keyword itself.
#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Kind {
    /// A conditional directive (`{IF}`, `{ELSIF}`, `{ELSE}`, `{END_IF}`,
    /// `{DEFINE}`, `{UNDEFINE}`).
    Directive(Directive),
    /// `{attribute 'name'}` or `{attribute 'name' := 'value'}`. The name is
    /// not empty and holds letters, digits, `-`, `.` and `_`; the value is the
    /// text between its quotes, as written.
    Attribute { name: String, value: Option<String> },
    /// `{text '..'}`, `{info '..'}`, `{warning '..'}` or `{error '..'}`: the
    /// text between the quotes, as written.
    Message { level: Level, text: String },
    /// Any other pragma, including a known keyword whose form is malformed
    /// (`{attribute}`, `{attribute ''}`, `{warning disable C0195}`). Kept as it
    /// is.
    Other,
}

/// True when the pragma `text` is a conditional directive (`{IF}`, `{ELSIF}`,
/// `{ELSE}`, `{END_IF}`, `{DEFINE}`, `{UNDEFINE}`), which the state machine
/// consumes ([`Step::keep`] is false for it) wherever it stands.
pub fn is_directive(text: &str) -> bool {
    matches!(kind(text), Kind::Directive(_))
}

/// Reads the leading keyword of a `{ ... }` pragma, without regard to case,
/// and the rest of its text.
pub fn kind(text: &str) -> Kind {
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
        "IF" => Kind::Directive(Directive::If(non_empty(rest))),
        "ELSIF" => Kind::Directive(Directive::Elsif(non_empty(rest))),
        "ELSE" => Kind::Directive(Directive::Else),
        "END_IF" => Kind::Directive(Directive::EndIf),
        "DEFINE" => Kind::Directive(Directive::Define(symbol(rest))),
        "UNDEFINE" => Kind::Directive(Directive::Undefine(symbol(rest))),
        "ATTRIBUTE" => attribute(rest),
        "TEXT" => message(Level::Text, rest),
        "INFO" => message(Level::Info, rest),
        "WARNING" => message(Level::Warning, rest),
        "ERROR" => message(Level::Error, rest),
        _ => Kind::Other,
    }
}

/// Reads what follows `attribute`: `'name'`, then optionally `:=` and
/// `'value'`, with white space between the parts.
fn attribute(rest: &str) -> Kind {
    let Some((name, after)) = quoted(rest) else {
        return Kind::Other;
    };
    let name_is_valid = !name.is_empty()
        && name
            .chars()
            .all(|c| c.is_ascii_alphanumeric() || matches!(c, '-' | '.' | '_'));
    if !name_is_valid {
        return Kind::Other;
    }

    let after = after.trim_start();
    if after.is_empty() {
        return Kind::Attribute {
            name: name.to_string(),
            value: None,
        };
    }
    let Some(value_text) = after.strip_prefix(":=") else {
        return Kind::Other;
    };
    match quoted(value_text.trim_start()) {
        Some((value, tail)) if tail.trim().is_empty() => Kind::Attribute {
            name: name.to_string(),
            value: Some(value.to_string()),
        },
        _ => Kind::Other,
    }
}

/// Reads what follows `text`, `info`, `warning` or `error`: the quoted text
/// and nothing after it.
fn message(level: Level, rest: &str) -> Kind {
    match quoted(rest) {
        Some((text, tail)) if tail.trim().is_empty() => Kind::Message {
            level,
            text: text.to_string(),
        },
        _ => Kind::Other,
    }
}

/// Splits `'text'` and what follows it. No escape is read: the text ends at
/// the next single quote, so a text that holds one is not understood.
fn quoted(text: &str) -> Option<(&str, &str)> {
    const QUOTE: char = '\'';
    text.strip_prefix(QUOTE)?.split_once(QUOTE)
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

/// Evaluates a pragma condition against the defines in scope. The error says
/// why the text is not a condition this compiler evaluates.
fn eval(condition: &str, defines: &HashSet<String>) -> Result<bool, Fault> {
    let mut parser = ConditionParser {
        text: condition,
        pos: 0,
        depth: 0,
        defines,
    };
    let value = parser.or_expression()?;
    parser.skip_whitespace();
    if parser.pos == parser.text.len() {
        Ok(value)
    } else {
        Err(Fault::UnexpectedValue)
    }
}

struct ConditionParser<'a> {
    text: &'a str,
    pos: usize,
    /// How many operands enclose the one being read.
    depth: usize,
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

    fn or_expression(&mut self) -> Result<bool, Fault> {
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

    fn and_expression(&mut self) -> Result<bool, Fault> {
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

    /// An operand. Every nesting of the condition, a parenthesis or a `NOT`,
    /// goes through here, which is where its depth is counted.
    fn operand(&mut self) -> Result<bool, Fault> {
        if self.depth >= MAX_DEPTH {
            return Err(Fault::NestingTooDeep);
        }
        self.depth += 1;
        let value = self.operand_inside();
        self.depth -= 1;
        value
    }

    fn operand_inside(&mut self) -> Result<bool, Fault> {
        self.skip_whitespace();
        if self.eat('(') {
            let value = self.or_expression()?;
            if !self.eat(')') {
                return Err(Fault::UnexpectedValue);
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
                    return Err(Fault::UnexpectedValue);
                }
                let Some(mut name) = self.next_word() else {
                    return Err(Fault::UnexpectedValue);
                };
                while self.eat('.') {
                    let Some(part) = self.next_word() else {
                        return Err(Fault::UnexpectedValue);
                    };
                    name.push('.');
                    name.push_str(&part);
                }
                if !self.eat(')') {
                    return Err(Fault::UnexpectedValue);
                }
                Ok(self.defines.contains(&name))
            }
            _ => {
                self.pos = start;
                Err(Fault::UnexpectedValue)
            }
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use rstest::rstest;

    fn attribute(name: &str, value: Option<&str>) -> Kind {
        Kind::Attribute {
            name: name.to_string(),
            value: value.map(str::to_string),
        }
    }

    fn message(level: Level, text: &str) -> Kind {
        Kind::Message {
            level,
            text: text.to_string(),
        }
    }

    #[rstest]
    #[case::if_directive("{IF defined(X)}", Kind::Directive(Directive::If(Ok("defined(X)".to_string()))))]
    #[case::if_without_condition("{IF}", Kind::Directive(Directive::If(Err(()))))]
    #[case::elsif_directive("{ELSIF true}", Kind::Directive(Directive::Elsif(Ok("true".to_string()))))]
    #[case::else_directive("{ELSE}", Kind::Directive(Directive::Else))]
    #[case::end_if_directive("{END_IF}", Kind::Directive(Directive::EndIf))]
    #[case::define_directive("{DEFINE Foo}", Kind::Directive(Directive::Define(Some("foo".to_string()))))]
    #[case::undefine_directive("{UNDEFINE Foo}", Kind::Directive(Directive::Undefine(Some("foo".to_string()))))]
    #[case::directive_keyword_without_case("{end_if}", Kind::Directive(Directive::EndIf))]
    #[case::attribute_with_name("{attribute 'hide'}", attribute("hide", None))]
    #[case::attribute_with_value(
        "{attribute 'pack_mode' := '2'}",
        attribute("pack_mode", Some("2"))
    )]
    #[case::attribute_with_value_without_white_space(
        "{attribute 'pack_mode':='2'}",
        attribute("pack_mode", Some("2"))
    )]
    #[case::attribute_with_white_space_around_the_parts(
        "{  attribute   'a'  :=  'b'  }",
        attribute("a", Some("b"))
    )]
    #[case::attribute_with_new_lines(
        "{attribute\n\t'a'\r\n:=\n\t'b'\n}",
        attribute("a", Some("b"))
    )]
    #[case::attribute_keyword_without_case("{ATTRIBUTE 'Hide'}", attribute("Hide", None))]
    #[case::attribute_name_with_dash("{attribute 'no-init'}", attribute("no-init", None))]
    #[case::attribute_name_with_dashes(
        "{attribute 'estimated-stack-usage' := '64'}",
        attribute("estimated-stack-usage", Some("64"))
    )]
    #[case::attribute_name_with_dots(
        "{attribute 'OPC.UA.DA' := '1'}",
        attribute("OPC.UA.DA", Some("1"))
    )]
    #[case::attribute_with_empty_value("{attribute 'a' := ''}", attribute("a", Some("")))]
    #[case::attribute_with_value_holding_spaces(
        "{attribute 'a' := 'x y'}",
        attribute("a", Some("x y"))
    )]
    #[case::attribute_without_name("{attribute}", Kind::Other)]
    #[case::attribute_with_empty_name("{attribute ''}", Kind::Other)]
    #[case::attribute_name_without_quotes("{attribute hide}", Kind::Other)]
    #[case::attribute_name_without_closing_quote("{attribute 'hide}", Kind::Other)]
    #[case::attribute_name_with_forbidden_character("{attribute 'a b'}", Kind::Other)]
    #[case::attribute_with_text_after_the_name("{attribute 'a' 'b'}", Kind::Other)]
    #[case::attribute_with_assignment_without_value("{attribute 'a' :=}", Kind::Other)]
    #[case::attribute_with_value_without_quotes("{attribute 'a' := 2}", Kind::Other)]
    #[case::attribute_with_value_without_closing_quote("{attribute 'a' := '2}", Kind::Other)]
    #[case::attribute_with_text_after_the_value("{attribute 'a' := 'b' c}", Kind::Other)]
    #[case::text_message("{text 'hello'}", message(Level::Text, "hello"))]
    #[case::info_message("{info 'hello'}", message(Level::Info, "hello"))]
    #[case::warning_message("{warning 'hello world'}", message(Level::Warning, "hello world"))]
    #[case::error_message("{error 'hello'}", message(Level::Error, "hello"))]
    #[case::message_keyword_without_case("{WARNING 'x'}", message(Level::Warning, "x"))]
    #[case::message_with_white_space("{  warning   'x'  }", message(Level::Warning, "x"))]
    #[case::message_with_empty_text("{info ''}", message(Level::Info, ""))]
    #[case::message_without_text("{warning}", Kind::Other)]
    #[case::message_without_quotes("{warning hello}", Kind::Other)]
    #[case::message_without_closing_quote("{warning 'hello}", Kind::Other)]
    #[case::message_with_text_after_the_quoted_text("{warning 'a' b}", Kind::Other)]
    #[case::warning_disable("{warning disable C0195}", Kind::Other)]
    #[case::warning_restore("{warning restore C0195}", Kind::Other)]
    #[case::unknown_keyword("{flag 'x'}", Kind::Other)]
    #[case::empty_pragma("{}", Kind::Other)]
    fn kind_when_pragma_text_then_the_kind_it_says(#[case] text: &str, #[case] expected: Kind) {
        assert_eq!(kind(text), expected);
    }

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
        assert_eq!(
            eval("hastype(x)", &HashSet::new()),
            Err(Fault::UnexpectedValue)
        );
    }

    #[test]
    fn eval_when_unbalanced_parenthesis_then_err() {
        assert_eq!(eval("(true", &HashSet::new()), Err(Fault::UnexpectedValue));
    }

    #[test]
    fn eval_when_nested_exactly_to_the_limit_then_evaluated() {
        let condition = format!(
            "{}true{}",
            "(".repeat(MAX_DEPTH - 1),
            ")".repeat(MAX_DEPTH - 1)
        );
        assert_eq!(eval(&condition, &HashSet::new()), Ok(true));
        let negations = format!("{}true", "not ".repeat(MAX_DEPTH - 1));
        assert_eq!(eval(&negations, &HashSet::new()), Ok(MAX_DEPTH % 2 == 1));
    }

    #[test]
    fn eval_when_nested_one_past_the_limit_then_nesting_too_deep() {
        let condition = format!("{}true{}", "(".repeat(MAX_DEPTH), ")".repeat(MAX_DEPTH));
        assert_eq!(
            eval(&condition, &HashSet::new()),
            Err(Fault::NestingTooDeep)
        );
        let negations = format!("{}true", "not ".repeat(MAX_DEPTH));
        assert_eq!(
            eval(&negations, &HashSet::new()),
            Err(Fault::NestingTooDeep)
        );
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

    #[test]
    fn is_directive_when_pragma_then_exactly_the_ones_the_state_machine_consumes() {
        for text in [
            "{IF true}",
            "{ELSIF false}",
            "{ELSE}",
            "{END_IF}",
            "{DEFINE a}",
            "{UNDEFINE a}",
            "{attribute x}",
            "{}",
        ] {
            let consumed = !Conditionals::new().step(text, ()).keep;
            assert_eq!(is_directive(text), consumed, "{text}");
        }
    }
}
