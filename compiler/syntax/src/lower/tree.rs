//! A small accessor layer over the tree, so a lowering rule reads the pieces
//! of a node by kind and not by position in a child list.

use crate::syntax_kind::{SyntaxKind, SyntaxNode, SyntaxToken};

/// The tokens directly under `node` that are not trivia, in source order.
pub fn significant_tokens(node: &SyntaxNode) -> Vec<SyntaxToken> {
    node.children_with_tokens()
        .filter_map(|element| element.into_token())
        .filter(|token| !token.kind().is_trivia())
        .collect()
}

/// The first token directly under `node` that is of one of `kinds`.
pub fn token_of(node: &SyntaxNode, kinds: &[SyntaxKind]) -> Option<SyntaxToken> {
    significant_tokens(node)
        .into_iter()
        .find(|token| kinds.contains(&token.kind()))
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::{parse_expression, ParseOptions};

    #[test]
    fn significant_tokens_when_literal_then_its_tokens_without_trivia() {
        let parse = parse_expression(" T#5s ", &ParseOptions::default());
        let kinds: Vec<SyntaxKind> = parse
            .root
            .first_child()
            .iter()
            .flat_map(significant_tokens)
            .map(|token| token.kind())
            .collect();
        assert_eq!(
            kinds,
            vec![
                SyntaxKind::Ident,
                SyntaxKind::Hash,
                SyntaxKind::IntegerLit,
                SyntaxKind::Ident
            ]
        );
    }

    #[test]
    fn token_of_when_kind_present_then_first_such_token_and_otherwise_none() {
        let parse = parse_expression("INT#5", &ParseOptions::default());
        let node = parse.root.first_child();
        let hash = node
            .as_ref()
            .and_then(|node| token_of(node, &[SyntaxKind::Hash]));
        assert_eq!(
            hash.map(|token| token.text().to_string()),
            Some("#".to_string())
        );
        let missing = node
            .as_ref()
            .and_then(|node| token_of(node, &[SyntaxKind::Plus]));
        assert!(missing.is_none());
    }
}
