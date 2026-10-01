//! Tree construction: the sink that turns parser events into a rowan tree.
//!
//! The parser records only significant tokens and node boundaries. The sink
//! walks the events together with the full token vector and re-attaches every
//! trivia token in source position, so the tree's text is the source for any
//! input (design: parse-tree architecture, section 3.1). It is the only place
//! that builds green nodes.
//!
//! Placement rule: trivia before a node's first token sits in the enclosing
//! node, before the node; trivia after a node's last token stays in the
//! enclosing node, after it. Leading trivia of the whole input and anything
//! after the last token sit directly in the root. Tokens the parser did not
//! consume are appended to the root, so no input is ever dropped.

use crate::lexer::Token;
use crate::parser::event::Event;
use crate::syntax_kind::{StLanguage, SyntaxKind};
use rowan::{GreenNode, GreenNodeBuilder, Language};
use std::mem;

fn raw(kind: SyntaxKind) -> rowan::SyntaxKind {
    StLanguage::kind_to_raw(kind)
}

/// Builds the green tree for `events` over `tokens`.
pub(crate) fn build_green(tokens: &[Token<'_>], mut events: Vec<Event>) -> GreenNode {
    let mut sink = Sink {
        builder: GreenNodeBuilder::new(),
        tokens,
        cursor: 0,
        open: 0,
        roots: 0,
    };
    let last = events.len().saturating_sub(1);
    for position in 0..events.len() {
        let event = take(&mut events, position);
        match event {
            Event::Start {
                kind,
                forward_parent,
            } => {
                let kinds = start_chain(&mut events, kind, position, forward_parent);
                if position != 0 {
                    sink.eat_trivia();
                }
                for kind in kinds.into_iter().rev() {
                    sink.start(kind);
                }
            }
            Event::Token { index } => sink.token_through(usize::try_from(index).unwrap_or(0)),
            Event::Finish => {
                if position == last {
                    sink.flush_all();
                }
                sink.finish_node();
            }
            Event::Tombstone => {}
        }
    }
    if sink.open > 0 {
        sink.flush_all();
    }
    while sink.open > 0 {
        sink.finish_node();
    }
    if sink.roots == 1 {
        sink.builder.finish()
    } else {
        flat_green(tokens)
    }
}

/// A root holding every token flat: the fallback when the events do not form
/// exactly one tree, so that a parser bug degrades the shape and never the
/// text.
fn flat_green(tokens: &[Token<'_>]) -> GreenNode {
    let mut builder = GreenNodeBuilder::new();
    builder.start_node(raw(SyntaxKind::SourceFile));
    for token in tokens {
        builder.token(raw(token.kind), token.text);
    }
    builder.finish_node();
    builder.finish()
}

fn take(events: &mut [Event], position: usize) -> Event {
    events.get_mut(position).map_or(Event::Tombstone, |slot| {
        mem::replace(slot, Event::Tombstone)
    })
}

/// The node kinds to open at one position, innermost first: the node itself
/// then each node that was declared to wrap it, by forward parent.
fn start_chain(
    events: &mut [Event],
    kind: SyntaxKind,
    position: usize,
    mut forward_parent: Option<u32>,
) -> Vec<SyntaxKind> {
    let mut kinds = vec![kind];
    let mut at = position;
    while let Some(offset) = forward_parent {
        at += offset as usize;
        match take(events, at) {
            Event::Start {
                kind,
                forward_parent: next,
            } => {
                kinds.push(kind);
                forward_parent = next;
            }
            _ => break,
        }
    }
    kinds
}

struct Sink<'t, 's> {
    builder: GreenNodeBuilder<'static>,
    tokens: &'t [Token<'s>],
    /// The next token to emit.
    cursor: usize,
    open: usize,
    roots: usize,
}

impl Sink<'_, '_> {
    fn start(&mut self, kind: SyntaxKind) {
        if self.open == 0 {
            self.roots += 1;
        }
        self.builder.start_node(raw(kind));
        self.open += 1;
    }

    fn finish_node(&mut self) {
        if self.open > 0 {
            self.builder.finish_node();
            self.open -= 1;
        }
    }

    fn emit_next(&mut self) {
        if let Some(token) = self.tokens.get(self.cursor) {
            self.builder.token(raw(token.kind), token.text);
            self.cursor += 1;
        }
    }

    /// Emits the trivia tokens that precede the next significant token.
    fn eat_trivia(&mut self) {
        while self
            .tokens
            .get(self.cursor)
            .is_some_and(|token| token.kind.is_trivia())
        {
            self.emit_next();
        }
    }

    /// Emits every token up to and including the one at `index`: the trivia
    /// before it, then the token itself.
    fn token_through(&mut self, index: usize) {
        while self.cursor <= index && self.cursor < self.tokens.len() {
            self.emit_next();
        }
    }

    fn flush_all(&mut self) {
        while self.cursor < self.tokens.len() {
            self.emit_next();
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::lexer::lex;
    use crate::syntax_kind::SyntaxNode;

    fn tree(source: &str, shape: impl FnOnce(&[Token]) -> Vec<Event>) -> SyntaxNode {
        let (tokens, _) = lex(source);
        let events = shape(&tokens);
        SyntaxNode::new_root(build_green(&tokens, events))
    }

    fn start(kind: SyntaxKind) -> Event {
        Event::Start {
            kind,
            forward_parent: None,
        }
    }

    #[test]
    fn build_green_when_no_events_then_root_holds_all_tokens() {
        let root = tree(" a ", |_| Vec::new());
        assert_eq!(root.kind(), SyntaxKind::SourceFile);
        assert_eq!(root.text().to_string(), " a ");
    }

    #[test]
    fn build_green_when_trivia_around_node_then_it_stays_outside_the_node() {
        // tokens: ws a ws b ; the inner node owns only `a`.
        let root = tree(" a b", |_| {
            vec![
                start(SyntaxKind::SourceFile),
                start(SyntaxKind::NameRef),
                Event::Token { index: 1 },
                Event::Finish,
                Event::Token { index: 3 },
                Event::Finish,
            ]
        });
        let name = root.first_child().map(|node| node.text().to_string());
        assert_eq!(name.as_deref(), Some("a"));
        assert_eq!(root.text().to_string(), " a b");
    }

    #[test]
    fn build_green_when_forward_parent_then_wrapper_encloses_earlier_node() {
        let root = tree("a+b", |_| {
            vec![
                start(SyntaxKind::SourceFile),
                Event::Start {
                    kind: SyntaxKind::NameRef,
                    forward_parent: Some(3),
                },
                Event::Token { index: 0 },
                Event::Finish,
                Event::Start {
                    kind: SyntaxKind::BinaryExpr,
                    forward_parent: None,
                },
                Event::Token { index: 1 },
                Event::Token { index: 2 },
                Event::Finish,
                Event::Finish,
            ]
        });
        let binary = root.first_child();
        assert_eq!(
            binary.as_ref().map(|node| node.kind()),
            Some(SyntaxKind::BinaryExpr)
        );
        assert_eq!(
            binary
                .and_then(|node| node.first_child())
                .map(|node| node.kind()),
            Some(SyntaxKind::NameRef)
        );
    }

    #[test]
    fn build_green_when_parser_left_tokens_unconsumed_then_root_keeps_them() {
        let root = tree("a b c", |_| {
            vec![
                start(SyntaxKind::SourceFile),
                Event::Token { index: 0 },
                Event::Finish,
            ]
        });
        assert_eq!(root.text().to_string(), "a b c");
    }

    #[test]
    fn build_green_when_unbalanced_events_then_tree_still_complete() {
        let root = tree("a", |_| {
            vec![
                start(SyntaxKind::SourceFile),
                start(SyntaxKind::NameRef),
                Event::Token { index: 0 },
            ]
        });
        assert_eq!(root.text().to_string(), "a");
        let stray = tree("a", |_| vec![Event::Finish, Event::Finish]);
        assert_eq!(stray.text().to_string(), "a");
    }
}
