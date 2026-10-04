//! The parser state and the primitives every grammar rule is written with.
//!
//! The parser sees the token stream through a token source that skips trivia
//! for lookahead while the sink, not the parser, re-attaches every trivia
//! token to the tree. Rules are written against [`Parser`]: lookahead
//! (`at`, `nth`), consumption (`bump`, `eat`, `expect`), markers for building
//! nodes, error reporting, and the depth guard. Recovery built from these
//! primitives lives in `recovery`.

use super::event::{CompletedMarker, Event, Marker};
use super::options::ParseOptions;
use crate::error::{nesting_message, ErrorKind, SyntaxError};
use crate::lexer::Token;
use crate::syntax_kind::SyntaxKind;
use rowan::{TextRange, TextSize};

/// The deepest syntax tree the parser builds, counting the root: no path from
/// the root down holds more nodes. What would be deeper is not opened; see
/// [`Parser::open`]. Every stage that walks the tree recurses as deep as the
/// tree is, so this number is what bounds their stack use, and a chain of
/// `a + b + c ...` or `a.b.c ...` counts like any other nesting, because the
/// tree nests it.
///
/// It is not chosen here: it is the depth the stack budget of the platform
/// holds, derived by one rule in [`ironplc_dsl::stack`], which is also what
/// gives every stage that budget. The reference accepts more: CODESYS 3.5.22
/// stops at 2000 levels of expression nesting in its parser and 5000 in its
/// statement checker, and reports both as message 584, "Maximum nesting depth
/// exceeded". The test
/// `compile_when_nesting_is_as_deep_as_allowed_then_every_stage_fits_the_stack_budget`
/// of `tests/tree_depth.rs` proves the value, running every stage over every
/// nesting at the limit on a thread of exactly the budget.
pub use ironplc_dsl::stack::MAX_DEPTH;

/// Keywords the legacy lexer matches only in upper case. The lexer here is
/// case-insensitive for every keyword, so the parser treats `mod` and `not`
/// as ordinary names, as the legacy pipeline does.
const UPPER_CASE_ONLY: &[(SyntaxKind, &str)] =
    &[(SyntaxKind::Mod, "MOD"), (SyntaxKind::Not, "NOT")];

/// The CODESYS special operators the legacy token pipeline promotes out of
/// identifiers. They are not available as ordinary names.
const SPECIAL_OPERATORS: &[&str] = &[
    "__NEW",
    "__DELETE",
    "__ISVALIDREF",
    "__TYPEOF",
    "__CURRENTTASK",
    "__XADD",
];

/// True when `text` spells one of the special operators (any case).
pub(crate) fn is_special_operator(text: &str) -> bool {
    SPECIAL_OPERATORS
        .iter()
        .any(|name| name.eq_ignore_ascii_case(text))
}

/// Parser state over one token vector.
pub(crate) struct Parser<'t, 's> {
    tokens: &'t [Token<'s>],
    /// Indices into `tokens` of the tokens that are not trivia.
    significant: Vec<usize>,
    pos: usize,
    events: Vec<Event>,
    errors: Vec<SyntaxError>,
    pub(super) options: ParseOptions,
    /// One entry per node that is open, outermost first: the height of the
    /// tallest node completed directly inside it so far.
    levels: Vec<u32>,
    /// How many `CASE` statements enclose the current position. Inside one,
    /// `name :` selects the next branch instead of labelling a statement.
    pub(super) case_depth: u32,
    end: TextSize,
}

impl<'t, 's> Parser<'t, 's> {
    pub(crate) fn new(tokens: &'t [Token<'s>], options: ParseOptions) -> Self {
        let significant = tokens
            .iter()
            .enumerate()
            .filter(|(_, token)| !token.kind.is_trivia())
            .map(|(index, _)| index)
            .collect();
        let end = tokens
            .last()
            .map_or_else(|| TextSize::from(0), |token| token.range.end());
        Parser {
            tokens,
            significant,
            pos: 0,
            events: Vec::new(),
            errors: Vec::new(),
            options,
            levels: Vec::new(),
            case_depth: 0,
            end,
        }
    }

    /// The events and errors recorded so far.
    pub(crate) fn finish(self) -> (Vec<Event>, Vec<SyntaxError>) {
        debug_assert!(self.levels.is_empty(), "a node was opened and never closed");
        (self.events, self.errors)
    }

    // ----- lookahead ------------------------------------------------------

    fn token(&self, n: usize) -> Option<&Token<'s>> {
        let index = *self.significant.get(self.pos + n)?;
        self.tokens.get(index)
    }

    /// The kind of the `n`th significant token ahead, as the lexer typed it.
    pub(crate) fn nth(&self, n: usize) -> Option<SyntaxKind> {
        self.token(n).map(|token| token.kind)
    }

    /// The source text of the `n`th significant token ahead.
    pub(crate) fn nth_text(&self, n: usize) -> &'s str {
        self.token(n).map_or("", |token| token.text)
    }

    pub(crate) fn at_eof(&self) -> bool {
        self.token(0).is_none()
    }

    /// True when the next token is the keyword or token `kind`. A keyword the
    /// dialect leaves disabled is never matched; it is an ordinary name.
    pub(crate) fn at(&self, kind: SyntaxKind) -> bool {
        self.nth_at(0, kind)
    }

    pub(crate) fn nth_at(&self, n: usize, kind: SyntaxKind) -> bool {
        self.nth(n) == Some(kind) && self.keyword_active(n, kind)
    }

    /// True when the `n`th token, of keyword kind `kind`, is a keyword here: its
    /// dialect flag is on, and it is spelled the way the language requires.
    fn keyword_active(&self, n: usize, kind: SyntaxKind) -> bool {
        self.options.keyword_enabled(kind)
            && UPPER_CASE_ONLY
                .iter()
                .all(|(only, spelling)| *only != kind || self.nth_text(n) == *spelling)
    }

    pub(crate) fn at_any(&self, kinds: &[SyntaxKind]) -> bool {
        kinds.iter().any(|kind| self.at(*kind))
    }

    /// True when the `n`th token ahead is the word `word`, ignoring case, as an
    /// ordinary word: an identifier, or a keyword the dialect has not enabled
    /// (`S`, `REF`, `T`, `UTF8`).
    pub(crate) fn nth_is_word(&self, n: usize, word: &str) -> bool {
        let plain = match self.nth(n) {
            Some(SyntaxKind::Ident) => true,
            Some(kind) => kind.is_keyword() && !self.keyword_active(n, kind),
            None => false,
        };
        plain && self.nth_text(n).eq_ignore_ascii_case(word)
    }

    /// True when no trivia separates the `n`th and the following token, which
    /// is what the literal forms (`T#5s`, `16#FF`, `-5`) require.
    pub(crate) fn adjacent(&self, n: usize) -> bool {
        match (
            self.significant.get(self.pos + n),
            self.significant.get(self.pos + n + 1),
        ) {
            (Some(first), Some(second)) => *second == *first + 1,
            _ => false,
        }
    }

    /// True when the `n`th token ahead can be used as an identifier: an
    /// identifier token that is not a special operator, an escaped
    /// identifier, a keyword the dialect has not enabled, or `TIME` where the
    /// dialect lets it name a function.
    pub(crate) fn name_at(&self, n: usize) -> bool {
        let Some(kind) = self.nth(n) else {
            return false;
        };
        match kind {
            SyntaxKind::Ident => !is_special_operator(self.nth_text(n)),
            SyntaxKind::EscapedIdent => true,
            SyntaxKind::Time => self.options.time_is_name(self.nth(n + 1)),
            _ => kind.is_keyword() && !self.keyword_active(n, kind),
        }
    }

    /// True when the previous token and the next one touch, with no trivia
    /// between them.
    pub(crate) fn touches_previous(&self) -> bool {
        match self
            .pos
            .checked_sub(1)
            .and_then(|prev| self.significant.get(prev))
        {
            Some(prev) => self.significant.get(self.pos) == Some(&(*prev + 1)),
            None => false,
        }
    }

    /// The value of the `n`th token ahead when it is a plain decimal number,
    /// ignoring digit separators; `None` when it does not fit.
    pub(crate) fn nth_number(&self, n: usize) -> Option<u64> {
        let digits: String = self.nth_text(n).chars().filter(|c| *c != '_').collect();
        digits.parse().ok()
    }

    /// Like [`Parser::name_at`] but also the words legal as variable names
    /// although they are keywords elsewhere.
    pub(crate) fn variable_name_at(&self, n: usize) -> bool {
        self.name_at(n)
            || matches!(
                self.nth(n),
                Some(SyntaxKind::Step | SyntaxKind::On | SyntaxKind::REdge | SyntaxKind::FEdge)
            )
    }

    // ----- consumption ----------------------------------------------------

    /// Consumes the next token into the current node.
    pub(crate) fn bump(&mut self) {
        if let Some(index) = self.significant.get(self.pos) {
            self.events.push(Event::Token {
                index: u32::try_from(*index).unwrap_or(u32::MAX),
            });
            self.pos += 1;
        }
    }

    pub(crate) fn bump_n(&mut self, count: usize) {
        for _ in 0..count {
            self.bump();
        }
    }

    pub(crate) fn eat(&mut self, kind: SyntaxKind) -> bool {
        let found = self.at(kind);
        if found {
            self.bump();
        }
        found
    }

    /// Consumes `kind` or reports that `what` was expected, consuming nothing.
    pub(crate) fn expect(&mut self, kind: SyntaxKind, what: &str) -> bool {
        let found = self.eat(kind);
        if !found {
            self.error(&format!("expected {what}"));
        }
        found
    }

    /// A position in the stream, used to prove a rule made progress.
    pub(crate) fn position(&self) -> usize {
        self.pos
    }

    // ----- nodes ----------------------------------------------------------

    pub(crate) fn start(&mut self) -> Marker {
        self.open(None)
    }

    /// Starts a node that will wrap `completed` and what follows it.
    pub(crate) fn precede(&mut self, completed: CompletedMarker) -> Marker {
        self.open(Some(completed))
    }

    /// The one place a node is opened, whether it nests around what comes
    /// after it (`start`) or around what is already complete (`precede`,
    /// which is how a left-associative chain grows). The node would sit below
    /// every node that is open and above everything it wraps; when that makes
    /// the tree deeper than [`MAX_DEPTH`] it is not opened. The marker is
    /// absorbed instead: what the node would have held joins the node that is
    /// open, so the text is all kept, and one error is reported.
    fn open(&mut self, wraps: Option<CompletedMarker>) -> Marker {
        let below = wraps.map_or(0, |completed| completed.height);
        if self.levels.len() + 1 + below as usize > MAX_DEPTH {
            self.report_nesting();
            let pos = self.reserve();
            return Marker::absorbed(pos, wraps.unwrap_or(CompletedMarker::none_at(pos)));
        }
        self.levels.push(below);
        match wraps {
            Some(completed) => completed.precede(&mut self.events),
            None => Marker::at(self.reserve()),
        }
    }

    /// Reserves a slot for a `Start` and returns where it is.
    fn reserve(&mut self) -> usize {
        self.events.push(Event::Tombstone);
        self.events.len() - 1
    }

    pub(crate) fn complete(&mut self, marker: Marker, kind: SyntaxKind) -> CompletedMarker {
        let height = if marker.is_absorbed() {
            0
        } else {
            self.close_level(true)
        };
        marker.complete(&mut self.events, kind, height)
    }

    pub(crate) fn abandon(&mut self, marker: Marker) {
        if !marker.is_absorbed() {
            self.close_level(false);
        }
        marker.abandon(&mut self.events);
    }

    /// Closes the innermost open node and returns its height: one more than
    /// its tallest child when it is a node, and the tallest child's when it
    /// is abandoned, whose children go to its parent. The parent learns it.
    fn close_level(&mut self, node: bool) -> u32 {
        let height = self.levels.pop().unwrap_or(0) + u32::from(node);
        if let Some(parent) = self.levels.last_mut() {
            *parent = (*parent).max(height);
        }
        height
    }

    // ----- errors ---------------------------------------------------------

    /// The range of the next token, or the empty range at the end of input.
    pub(crate) fn current_range(&self) -> TextRange {
        self.token(0)
            .map_or_else(|| TextRange::empty(self.end), |token| token.range)
    }

    /// Records an error over an explicit range of the source.
    pub(crate) fn error_at(&mut self, range: TextRange, message: &str) {
        self.errors.push(SyntaxError::new(message, range));
    }

    /// The range of the `n`th token ahead, or the empty range at the end.
    pub(crate) fn nth_range(&self, n: usize) -> TextRange {
        self.token(n)
            .map_or_else(|| TextRange::empty(self.end), |token| token.range)
    }

    /// Records an error where the previous token ends, as an empty range: for
    /// something missing after it, such as the closing keyword of a declaration
    /// that ends where the next one begins.
    pub(crate) fn error_after_previous(&mut self, message: &str) {
        let end = self
            .pos
            .checked_sub(1)
            .and_then(|previous| self.significant.get(previous))
            .and_then(|index| self.tokens.get(*index))
            .map_or_else(|| TextSize::from(0), |token| token.range.end());
        self.errors
            .push(SyntaxError::new(message, TextRange::empty(end)));
    }

    /// Records an error at the next token. A lexical error token already has
    /// its own error, so none is added for it.
    pub(crate) fn error(&mut self, message: &str) {
        if self.nth(0) == Some(SyntaxKind::ErrorToken) {
            return;
        }
        self.errors
            .push(SyntaxError::new(message, self.current_range()));
    }

    // ----- depth guard ----------------------------------------------------

    /// Records that the tree has no room for another node, unless the error
    /// just before it is that one already: a chain over the limit would
    /// otherwise report once per link.
    fn report_nesting(&mut self) {
        if self
            .errors
            .last()
            .is_some_and(|error| error.kind == ErrorKind::NestingTooDeep)
        {
            return;
        }
        self.errors.push(
            SyntaxError::new(nesting_message(), self.current_range())
                .with_kind(ErrorKind::NestingTooDeep),
        );
    }

    /// Runs `body`, or `degraded` when the tree is too deep to recurse
    /// further. `degraded` must consume what `body` would have, as an error
    /// node, so the enclosing loop still makes progress. The recursion of a
    /// rule is as deep as the tree it builds, so the room left in the tree is
    /// the bound on the stack the parser itself uses. One level is kept free
    /// for the error node `degraded` opens.
    pub(crate) fn guarded<T>(
        &mut self,
        body: impl FnOnce(&mut Self) -> T,
        degraded: impl FnOnce(&mut Self) -> T,
    ) -> T {
        if self.levels.len() + 1 >= MAX_DEPTH {
            self.report_nesting();
            return degraded(self);
        }
        body(self)
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::lexer::lex;

    fn parser_over<T>(source: &str, options: ParseOptions, check: impl FnOnce(Parser) -> T) -> T {
        let (tokens, _) = lex(source);
        check(Parser::new(&tokens, options))
    }

    #[test]
    fn nth_when_trivia_between_tokens_then_lookahead_skips_it() {
        parser_over("a (* c *)  :=\n 1", ParseOptions::default(), |p| {
            assert_eq!(p.nth(0), Some(SyntaxKind::Ident));
            assert_eq!(p.nth(1), Some(SyntaxKind::Assignment));
            assert_eq!(p.nth(2), Some(SyntaxKind::IntegerLit));
            assert_eq!(p.nth(3), None);
        });
    }

    #[test]
    fn adjacent_when_trivia_between_then_false_and_when_touching_then_true() {
        parser_over("T#5 s", ParseOptions::default(), |p| {
            assert!(p.adjacent(0));
            assert!(p.adjacent(1));
            assert!(!p.adjacent(2));
        });
    }

    #[test]
    fn at_when_gated_keyword_disabled_then_false() {
        parser_over("CONTINUE", ParseOptions::default(), |p| {
            assert!(!p.at(SyntaxKind::Continue));
            assert!(p.name_at(0));
        });
        parser_over("CONTINUE", ParseOptions::all(), |p| {
            assert!(p.at(SyntaxKind::Continue));
            assert!(!p.name_at(0));
        });
    }

    #[test]
    fn name_at_when_special_operator_then_not_a_name() {
        parser_over("__new __NEW_ITEM", ParseOptions::default(), |p| {
            assert!(!p.name_at(0));
            assert!(p.name_at(1));
        });
    }

    #[test]
    fn name_at_when_time_then_name_only_before_call_or_assignment_and_flag() {
        let flag = ParseOptions {
            allow_time_as_function_name: true,
            ..ParseOptions::default()
        };
        parser_over("TIME()", flag, |p| assert!(p.name_at(0)));
        parser_over("TIME := 1", flag, |p| assert!(p.name_at(0)));
        parser_over("TIME#5s", flag, |p| assert!(!p.name_at(0)));
        parser_over(
            "TIME()",
            ParseOptions::default(),
            |p| assert!(!p.name_at(0)),
        );
    }

    #[test]
    fn variable_name_at_when_step_keyword_then_name() {
        parser_over("STEP", ParseOptions::default(), |p| {
            assert!(!p.name_at(0));
            assert!(p.variable_name_at(0));
        });
    }

    #[test]
    fn error_when_at_end_then_empty_range_at_source_end() {
        let (tokens, _) = lex("x ");
        let mut parser = Parser::new(&tokens, ParseOptions::default());
        parser.bump();
        parser.error("boom");
        let (_, errors) = parser.finish();
        assert_eq!(errors.len(), 1);
        assert_eq!(errors[0].range, TextRange::empty(TextSize::from(2)));
    }

    #[test]
    fn error_when_next_is_error_token_then_not_reported_twice() {
        let (tokens, _) = lex("?");
        let mut parser = Parser::new(&tokens, ParseOptions::default());
        parser.error("boom");
        assert!(parser.finish().1.is_empty());
    }

    #[test]
    fn guarded_when_tree_is_full_then_degraded_runs_and_levels_are_restored() {
        let (tokens, _) = lex("x");
        let mut parser = Parser::new(&tokens, ParseOptions::default());
        fn recurse(parser: &mut Parser) {
            let node = parser.start();
            parser.guarded(recurse, |_| ());
            parser.complete(node, SyntaxKind::ParenExpr);
        }
        recurse(&mut parser);
        assert!(parser.levels.is_empty());
        let (_, errors) = parser.finish();
        assert_eq!(errors.len(), 1);
        assert_eq!(errors[0].kind, ErrorKind::NestingTooDeep);
    }

    #[test]
    fn start_when_tree_is_full_then_node_is_absorbed_and_nothing_is_lost() {
        let (tokens, _) = lex("x x");
        let mut parser = Parser::new(&tokens, ParseOptions::default());
        let mut open = Vec::new();
        for _ in 0..MAX_DEPTH {
            open.push(parser.start());
        }
        assert_eq!(parser.levels.len(), MAX_DEPTH);
        let beyond = parser.start();
        assert_eq!(parser.levels.len(), MAX_DEPTH);
        parser.bump();
        parser.complete(beyond, SyntaxKind::ParenExpr);
        parser.bump();
        while let Some(node) = open.pop() {
            parser.complete(node, SyntaxKind::ParenExpr);
        }
        let (events, errors) = parser.finish();
        assert_eq!(errors.len(), 1);
        let starts = events
            .iter()
            .filter(|event| matches!(event, Event::Start { .. }))
            .count();
        assert_eq!(starts, MAX_DEPTH);
    }

    #[test]
    fn precede_when_wrapper_would_pass_the_limit_then_it_is_absorbed() {
        let (tokens, _) = lex("a ^ ^");
        let mut parser = Parser::new(&tokens, ParseOptions::default());
        let root = parser.start();
        let node = parser.start();
        parser.bump();
        let mut lhs = parser.complete(node, SyntaxKind::NameRef);
        for _ in 0..MAX_DEPTH + 1 {
            let wrapper = parser.precede(lhs);
            lhs = parser.complete(wrapper, SyntaxKind::DerefExpr);
        }
        assert_eq!(lhs.height as usize, MAX_DEPTH - 1);
        parser.complete(root, SyntaxKind::SourceFile);
        let (_, errors) = parser.finish();
        assert_eq!(errors.len(), 1);
    }
}
