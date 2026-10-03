//! A small accessor layer over the tree, so a lowering rule reads the pieces
//! of a node by kind and not by position in a child list.

use crate::syntax_kind::{SyntaxKind, SyntaxNode, SyntaxToken};
use rowan::{NodeOrToken, TextRange};

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

/// The children of `node` that are of `kind`, in source order.
pub fn children_of(node: &SyntaxNode, kind: SyntaxKind) -> impl Iterator<Item = SyntaxNode> {
    node.children().filter(move |child| child.kind() == kind)
}

/// The first child of `node` that is of `kind`.
pub fn child_of(node: &SyntaxNode, kind: SyntaxKind) -> Option<SyntaxNode> {
    children_of(node, kind).next()
}

/// The range of `node` up to its last significant element that is not a
/// trailing `terminator` token.
///
/// A statement's tree node holds the `;` that ends it, and an object that
/// records where the statement is written stops before it.
pub fn range_before(node: &SyntaxNode, terminator: SyntaxKind) -> TextRange {
    let start = node.text_range().start();
    let mut elements = node
        .children_with_tokens()
        .filter(|element| match element {
            NodeOrToken::Node(_) => true,
            NodeOrToken::Token(token) => !token.kind().is_trivia(),
        })
        .collect::<Vec<_>>();
    if matches!(elements.last(), Some(NodeOrToken::Token(token)) if token.kind() == terminator) {
        elements.pop();
    }
    match elements.last() {
        Some(last) => TextRange::new(start, last.text_range().end()),
        None => node.text_range(),
    }
}

/// The chain of nodes that nest through their first child node, from `node`
/// down.
///
/// A left-associative operator and a postfix selector both build their node
/// around the node written before them, so `a + b + c` and `a.b.c` are as deep
/// as they are long. Returned are the nodes whose kind `is_link` accepts,
/// innermost first, and the node under the innermost one: the base the chain
/// is applied to (`None` when a link has no node inside it). A rule folds the
/// links over its lowered base in order, so no rule recurses along a chain.
pub fn left_spine(
    node: &SyntaxNode,
    is_link: impl Fn(SyntaxKind) -> bool,
) -> (Option<SyntaxNode>, Vec<SyntaxNode>) {
    let mut links = Vec::new();
    let mut current = Some(node.clone());
    while let Some(candidate) = current {
        if !is_link(candidate.kind()) {
            links.reverse();
            return (Some(candidate), links);
        }
        current = candidate.first_child();
        links.push(candidate);
    }
    links.reverse();
    (None, links)
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::{parse_expression, parse_statements, ParseOptions};

    #[test]
    fn left_spine_when_nested_links_then_innermost_first_over_the_base() {
        let parse = parse_expression("a + b + c", &ParseOptions::default());
        let top = parse.root.first_child();
        let (base, links) = top
            .map(|node| left_spine(&node, |kind| kind == SyntaxKind::BinaryExpr))
            .unwrap_or_default();
        let texts: Vec<String> = links.iter().map(|link| link.text().to_string()).collect();
        assert_eq!(texts, vec!["a + b", "a + b + c"]);
        assert_eq!(
            base.map(|node| (node.kind(), node.text().to_string())),
            Some((SyntaxKind::NameRef, "a".to_string()))
        );
    }

    #[test]
    fn left_spine_when_node_is_not_a_link_then_it_is_the_base() {
        let parse = parse_expression("a", &ParseOptions::default());
        let node = parse.root.first_child();
        let (base, links) = node
            .map(|node| left_spine(&node, |kind| kind == SyntaxKind::BinaryExpr))
            .unwrap_or_default();
        assert!(links.is_empty());
        assert_eq!(base.map(|node| node.kind()), Some(SyntaxKind::NameRef));
    }

    #[test]
    fn left_spine_when_a_link_holds_no_node_then_no_base() {
        let green = rowan::GreenNode::new(
            rowan::SyntaxKind(SyntaxKind::DerefExpr as u16),
            std::iter::empty(),
        );
        let node = SyntaxNode::new_root(green);
        let (base, links) = left_spine(&node, |kind| kind == SyntaxKind::DerefExpr);
        assert!(base.is_none());
        assert_eq!(links.len(), 1);
    }

    #[test]
    fn children_of_when_kind_then_those_children_in_order_and_child_of_the_first() {
        let parse = parse_statements(
            "IF a THEN x := 1; ELSIF b THEN y := 2; ELSIF c THEN END_IF;",
            &ParseOptions::default(),
        );
        let node = parse
            .root
            .descendants()
            .find(|node| node.kind() == SyntaxKind::IfStmt)
            .expect("an IF");
        let texts: Vec<String> = children_of(&node, SyntaxKind::ElsifClause)
            .map(|clause| clause.text().to_string())
            .collect();
        assert_eq!(texts, vec!["ELSIF b THEN y := 2;", "ELSIF c THEN "]);
        assert_eq!(
            child_of(&node, SyntaxKind::ElsifClause).map(|clause| clause.text().to_string()),
            Some("ELSIF b THEN y := 2;".to_string())
        );
        assert!(child_of(&node, SyntaxKind::ElseClause).is_none());
    }

    #[test]
    fn range_before_when_node_ends_in_the_terminator_then_the_range_stops_before_it() {
        let parse = parse_statements("  IF a THEN END_IF  ;\nx := 1;", &ParseOptions::default());
        let ranges: Vec<(usize, usize)> = parse
            .root
            .descendants()
            .filter(|node| matches!(node.kind(), SyntaxKind::IfStmt | SyntaxKind::AssignStmt))
            .map(|node| {
                let range = range_before(&node, SyntaxKind::Semicolon);
                (usize::from(range.start()), usize::from(range.end()))
            })
            .collect();
        // The `IF` through `END_IF`, and the assignment through its value.
        assert_eq!(ranges, vec![(2, 18), (22, 28)]);
    }

    #[test]
    fn range_before_when_node_has_no_terminator_then_its_own_range() {
        let parse = parse_statements("IF a THEN END_IF", &ParseOptions::all());
        let node = parse
            .root
            .descendants()
            .find(|node| node.kind() == SyntaxKind::IfStmt)
            .expect("an IF");
        assert_eq!(
            range_before(&node, SyntaxKind::Semicolon),
            node.text_range()
        );
        let empty = SyntaxNode::new_root(rowan::GreenNode::new(
            rowan::SyntaxKind(SyntaxKind::IfStmt as u16),
            std::iter::empty(),
        ));
        assert_eq!(
            range_before(&empty, SyntaxKind::Semicolon),
            empty.text_range()
        );
    }

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
