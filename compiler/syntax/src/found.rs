//! What a syntax error says was found.
//!
//! An "expected ..." error also says what stood at its position instead: for
//! the parser the source text of the first significant (not trivia) token
//! there, for the lowering the source text of the whole node it rejects, as
//! written, or the end of input. It is data taken from the token stream, never
//! a string a caller words: a [`Found`] is built only from the tokens the lexer
//! made ([`Found::lexed`], [`Found::lexed_run`]) or from the tokens of the
//! syntax tree ([`Found::node`], [`Found::syntax_run`]).
//!
//! The text is shown in code quotes. A text that holds a line break (a string
//! literal or a node that spans lines) is shown up to its first line break,
//! followed by `...`, inside the quotes: `'abc...`.

use crate::lexer::Token;
use crate::syntax_kind::{SyntaxNode, SyntaxToken};
use core::fmt;
use rowan::{NodeOrToken, TextSize};

/// The input at the position of an error.
#[derive(Debug, Clone, PartialEq, Eq)]
pub(crate) struct Found(Repr);

#[derive(Debug, Clone, PartialEq, Eq)]
enum Repr {
    /// The source text of the token, or of the run of adjacent tokens.
    Source(String),
    /// Nothing stands there: the input has ended.
    EndOfInput,
}

impl Found {
    /// The input has ended.
    pub(crate) fn end_of_input() -> Found {
        Found(Repr::EndOfInput)
    }

    /// The lexed `token`, or the end of input when there is none.
    pub(crate) fn lexed(token: Option<&Token<'_>>) -> Found {
        token.map_or_else(Found::end_of_input, |token| {
            Found(Repr::Source(token.text.to_string()))
        })
    }

    /// The run of lexed `tokens` as written, the trivia between them included;
    /// `None` for an empty run, which found nothing to speak of.
    pub(crate) fn lexed_run(tokens: &[Token<'_>]) -> Option<Found> {
        run(tokens.iter().map(|token| token.text))
    }

    /// The tokens of the tree `tokens` as written; `None` for an empty run.
    pub(crate) fn syntax_run(tokens: &[SyntaxToken]) -> Option<Found> {
        run(tokens.iter().map(SyntaxToken::text))
    }

    /// What stands at `node`: its source text from its first to its last
    /// significant token, trivia around it left out; for a node without one,
    /// the first significant token after it; at the end of the input, that.
    pub(crate) fn node(node: &SyntaxNode) -> Found {
        let mut significant = node
            .descendants_with_tokens()
            .filter_map(NodeOrToken::into_token)
            .filter(|token| !token.kind().is_trivia());
        let first = significant.next();
        let last = significant.last().or_else(|| first.clone());
        if let (Some(first), Some(last)) = (first, last) {
            let start = first.text_range().start() - node.text_range().start();
            let end = last.text_range().end() - node.text_range().start();
            let text = node.text().slice(rowan::TextRange::new(start, end));
            return Found(Repr::Source(text.to_string()));
        }
        first_significant_from(node, node.text_range().end())
            .map_or_else(Found::end_of_input, |token| {
                Found(Repr::Source(token.text().to_string()))
            })
    }

    /// The found of the first significant token of `source`, for tests.
    #[cfg(test)]
    pub(crate) fn first_in(source: &str) -> Found {
        let (tokens, _) = crate::lexer::lex(source);
        Found::lexed(tokens.iter().find(|token| !token.kind.is_trivia()))
    }
}

/// The text of `pieces` joined, as a found; `None` when there are none.
fn run<'a>(pieces: impl Iterator<Item = &'a str>) -> Option<Found> {
    let text: String = pieces.collect();
    (!text.is_empty()).then_some(Found(Repr::Source(text)))
}

/// The first significant token of the tree of `node` that starts at or after
/// `offset`.
fn first_significant_from(node: &SyntaxNode, offset: TextSize) -> Option<SyntaxToken> {
    let root = node.ancestors().last()?;
    let first = root.token_at_offset(offset).right_biased()?;
    std::iter::successors(Some(first), SyntaxToken::next_token)
        .find(|token| !token.kind().is_trivia() && token.text_range().start() >= offset)
}

impl fmt::Display for Found {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match &self.0 {
            Repr::Source(text) => match text.find(['\n', '\r']) {
                Some(end) => write!(f, "`{}...`", &text[..end]),
                None => write!(f, "`{text}`"),
            },
            Repr::EndOfInput => f.write_str("end of input"),
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::lexer::lex;
    use crate::{parse_expression, ParseOptions};

    #[test]
    fn display_when_token_found_then_its_source_text_in_code_quotes() {
        assert_eq!(Found::first_in("END_IF").to_string(), "`END_IF`");
        assert_eq!(Found::first_in("42").to_string(), "`42`");
    }

    #[test]
    fn display_when_keyword_found_then_it_is_in_the_case_written() {
        assert_eq!(Found::first_in("end_if").to_string(), "`end_if`");
        assert_eq!(Found::first_in("End_If").to_string(), "`End_If`");
    }

    #[test]
    fn display_when_trivia_before_token_then_the_token() {
        assert_eq!(Found::first_in("  (* note *)\n  x := 1").to_string(), "`x`");
    }

    #[test]
    fn display_when_end_of_input_then_it_is_said_without_quotes() {
        assert_eq!(Found::first_in("").to_string(), "end of input");
        assert_eq!(
            Found::first_in(" (* only trivia *) ").to_string(),
            "end of input"
        );
        assert_eq!(Found::end_of_input().to_string(), "end of input");
    }

    #[test]
    fn lexed_run_when_tokens_then_their_text_joined_and_when_none_then_none() {
        let (tokens, _) = lex("5x");
        assert_eq!(
            Found::lexed_run(&tokens).map(|found| found.to_string()),
            Some("`5x`".to_string())
        );
        assert_eq!(Found::lexed_run(&[]), None);
    }

    #[test]
    fn display_when_text_holds_a_line_break_then_it_is_cut_there_and_ends_in_dots() {
        assert_eq!(Found::first_in("'a\nb'").to_string(), "`'a...`");
        assert_eq!(Found::first_in("'a\r\nb'").to_string(), "`'a...`");
        assert_eq!(Found::first_in("'ab'").to_string(), "`'ab'`");
    }

    /// The first node of `kind` in the tree of `source`.
    fn node_of(source: &str, kind: crate::syntax_kind::SyntaxKind) -> SyntaxNode {
        let parse = parse_expression(source, &ParseOptions::default());
        parse
            .root
            .descendants()
            .find(|node| node.kind() == kind)
            .unwrap_or_else(|| panic!("no {kind:?} in {:#?}", parse.root))
    }

    #[test]
    fn node_when_node_has_tokens_then_its_text_without_the_trivia_around_it() {
        let node = node_of(
            "(* c *) a +  (* d *) b  ",
            crate::syntax_kind::SyntaxKind::BinaryExpr,
        );
        assert_eq!(Found::node(&node).to_string(), "`a +  (* d *) b`");
    }

    #[test]
    fn node_when_node_spans_lines_then_it_is_cut_at_the_first_line_break() {
        let node = node_of("a +\n b", crate::syntax_kind::SyntaxKind::BinaryExpr);
        assert_eq!(Found::node(&node).to_string(), "`a +...`");
    }

    #[test]
    fn node_when_node_is_empty_and_nothing_follows_then_end_of_input() {
        let green = rowan::GreenNode::new(
            rowan::SyntaxKind(crate::syntax_kind::SyntaxKind::SourceFile as u16),
            std::iter::empty(),
        );
        assert_eq!(
            Found::node(&SyntaxNode::new_root(green)),
            Found::end_of_input()
        );
    }
}
