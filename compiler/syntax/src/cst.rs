//! Lossless tree construction over rowan green storage.
//!
//! This is the minimal builder: it wraps a token stream into a flat
//! [`SyntaxKind::SourceFile`] root whose text equals the source. The
//! event-driven parser that gives the tree its structure replaces the flat
//! shape later; the root and the token leaves stay as they are here.

use crate::error::SyntaxError;
use crate::lexer::{lex, Token};
use crate::syntax_kind::{StLanguage, SyntaxKind, SyntaxNode};
use rowan::{GreenNode, GreenNodeBuilder, Language};

/// A lexed source file as a flat tree plus the lexical errors.
#[derive(Debug)]
pub struct FlatParse {
    pub root: SyntaxNode,
    pub errors: Vec<SyntaxError>,
}

/// Wraps `tokens` into a `SourceFile` green node, one leaf per token.
pub fn flat_green(tokens: &[Token<'_>]) -> GreenNode {
    let mut builder = GreenNodeBuilder::new();
    builder.start_node(StLanguage::kind_to_raw(SyntaxKind::SourceFile));
    for token in tokens {
        builder.token(StLanguage::kind_to_raw(token.kind), token.text);
    }
    builder.finish_node();
    builder.finish()
}

/// Lexes `source` and builds the flat tree; `root.text()` equals `source`.
pub fn build_flat_tree(source: &str) -> FlatParse {
    let (tokens, errors) = lex(source);
    FlatParse {
        root: SyntaxNode::new_root(flat_green(&tokens)),
        errors,
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn build_flat_tree_when_source_then_root_is_source_file_with_same_text() {
        let source = "x := 1; (* c *)\r\n";
        let parsed = build_flat_tree(source);
        assert_eq!(parsed.root.kind(), SyntaxKind::SourceFile);
        assert_eq!(parsed.root.text().to_string(), source);
        assert!(parsed.errors.is_empty());
    }

    #[test]
    fn build_flat_tree_when_empty_then_empty_root() {
        let parsed = build_flat_tree("");
        assert_eq!(parsed.root.kind(), SyntaxKind::SourceFile);
        assert_eq!(parsed.root.children_with_tokens().count(), 0);
    }
}
