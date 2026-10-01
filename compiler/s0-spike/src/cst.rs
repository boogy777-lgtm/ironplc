//! Lossless CST construction over rowan green storage.
//!
//! [`Sink`] replays the parser's event stream into a `rowan::GreenNode`:
//! significant tokens come from the events, and every trivia token between
//! them is emitted in source position, so `root.text()` reproduces the input
//! byte for byte. Red navigation ([`SyntaxNode`]) is derived from the green
//! tree; no second tree is built.

use crate::lexer::Token;
use crate::parser::{Event, Parse, ParseError};
use crate::syntax::{StLanguage, SyntaxKind, SyntaxNode, SyntaxToken};
use rowan::Language as _;
use std::mem;

/// A parsed source file: red root plus diagnostics.
#[derive(Debug)]
pub struct ParsedCst {
    pub root: SyntaxNode,
    pub errors: Vec<ParseError>,
}

/// Parses `source` and materializes the red view over its green storage.
pub fn build_cst(source: &str) -> ParsedCst {
    let Parse { green, errors } = crate::parser::parse(source);
    ParsedCst {
        root: SyntaxNode::new_root(green),
        errors,
    }
}

/// The exact text of the tree; equal to the original input when lossless.
pub fn reconstruct(root: &SyntaxNode) -> String {
    root.text().to_string()
}

/// All red tokens of `kind`, in source order.
pub fn tokens_of_kind(root: &SyntaxNode, kind: SyntaxKind) -> Vec<SyntaxToken> {
    root.descendants_with_tokens()
        .filter_map(|element| element.into_token())
        .filter(|token| token.kind() == kind)
        .collect()
}

/// All interior nodes of `kind`, in source order.
pub fn nodes_of_kind(root: &SyntaxNode, kind: SyntaxKind) -> Vec<SyntaxNode> {
    root.descendants()
        .filter(|node| node.kind() == kind)
        .collect()
}

/// Counts used as experiment evidence.
#[derive(Debug, Default, Clone, Copy, PartialEq, Eq)]
pub struct TreeStats {
    pub nodes: usize,
    pub tokens: usize,
    pub trivia_tokens: usize,
    pub error_nodes: usize,
}

/// Walks the tree once and counts nodes/tokens/trivia/error nodes.
pub fn tree_stats(root: &SyntaxNode) -> TreeStats {
    let mut stats = TreeStats::default();
    for element in root.descendants_with_tokens() {
        match element {
            rowan::NodeOrToken::Node(node) => {
                stats.nodes += 1;
                if node.kind() == SyntaxKind::ErrorNode {
                    stats.error_nodes += 1;
                }
            }
            rowan::NodeOrToken::Token(token) => {
                stats.tokens += 1;
                if token.kind().is_trivia() {
                    stats.trivia_tokens += 1;
                }
            }
        }
    }
    stats
}

/// Renders the tree as an S-expression with trivia elided and token text
/// shown verbatim (e.g. `(BinaryExpr (LiteralExpr 1) + (LiteralExpr 2))`).
pub fn sexpr(root: &SyntaxNode) -> String {
    let mut out = String::new();
    write_sexpr(root, &mut out);
    out
}

fn write_sexpr(node: &SyntaxNode, out: &mut String) {
    out.push('(');
    out.push_str(&format!("{:?}", node.kind()));
    for element in node.children_with_tokens() {
        match element {
            rowan::NodeOrToken::Node(child) => {
                out.push(' ');
                write_sexpr(&child, out);
            }
            rowan::NodeOrToken::Token(token) => {
                if !token.kind().is_trivia() {
                    out.push(' ');
                    out.push_str(token.text());
                }
            }
        }
    }
    out.push(')');
}

/// Replays parser events into a green tree.
pub(crate) struct Sink<'t, 'src> {
    tokens: &'t [Token<'src>],
    cursor: usize,
    builder: rowan::GreenNodeBuilder<'static>,
}

impl<'t, 'src> Sink<'t, 'src> {
    pub(crate) fn build(tokens: &'t [Token<'src>], mut events: Vec<Event>) -> rowan::GreenNode {
        let mut sink = Sink {
            tokens,
            cursor: 0,
            builder: rowan::GreenNodeBuilder::new(),
        };

        for index in 0..events.len() {
            match mem::replace(&mut events[index], Event::Placeholder) {
                Event::Start {
                    kind,
                    forward_parent,
                } => {
                    let mut kinds = vec![kind];
                    let mut parent = forward_parent;
                    let mut idx = index;
                    // Follow the forward-parent chain: each link names a
                    // node that must wrap the already-emitted tokens.
                    while let Some(offset) = parent {
                        idx += offset as usize;
                        let next = match events.get_mut(idx) {
                            Some(slot) => mem::replace(slot, Event::Placeholder),
                            None => Event::Placeholder,
                        };
                        match next {
                            Event::Start {
                                kind,
                                forward_parent,
                            } => {
                                kinds.push(kind);
                                parent = forward_parent;
                            }
                            _ => parent = None,
                        }
                    }
                    for kind in kinds.into_iter().rev() {
                        sink.builder.start_node(StLanguage::kind_to_raw(kind));
                    }
                }
                Event::Token { index } => sink.token(index as usize),
                Event::Finish => {
                    // The root's finish is the last event; end-of-file trivia
                    // belongs inside it, before the tree closes.
                    if index + 1 == events.len() {
                        sink.eat_trivia(usize::MAX);
                    }
                    sink.builder.finish_node();
                }
                Event::Placeholder => {}
            }
        }

        sink.builder.finish()
    }

    fn token(&mut self, index: usize) {
        self.eat_trivia(index);
        if let Some(token) = self.tokens.get(index) {
            self.builder.token(
                StLanguage::kind_to_raw(token.kind.syntax_kind()),
                token.text,
            );
            self.cursor = index + 1;
        }
    }

    /// Emits trivia tokens sitting before `limit`; `usize::MAX` means "all
    /// remaining tokens".
    fn eat_trivia(&mut self, limit: usize) {
        while self.cursor < limit {
            let Some(token) = self.tokens.get(self.cursor) else {
                break;
            };
            if !token.kind.is_trivia() {
                break;
            }
            self.builder.token(
                StLanguage::kind_to_raw(token.kind.syntax_kind()),
                token.text,
            );
            self.cursor += 1;
        }
    }
}
