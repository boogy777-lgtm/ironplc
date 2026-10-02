//! Declared names.
//!
//! A name becomes an [`Id`] built the way the legacy grammar builds one:
//! `Id::from` on the token's text as written (an escaped name keeps its
//! backticks, a keyword the dialect has not enabled is an ordinary name) and
//! the token's own range as its position. The lower-casing is `Id`'s own.

use super::tree::significant_tokens;
use super::LowerCx;
use crate::syntax_kind::{SyntaxKind, SyntaxNode, SyntaxToken};
use ironplc_dsl::core::Id;
use ironplc_dsl::diagnostic::Diagnostic;

/// The identifier a name token spells.
pub fn lower_id(cx: &LowerCx, token: &SyntaxToken) -> Id {
    Id::from(token.text()).with_position(cx.token_span(token))
}

/// Lowers a declared name: a [`SyntaxKind::Name`] node holding one token.
pub fn lower_name(cx: &LowerCx, node: &SyntaxNode) -> Result<Id, Diagnostic> {
    if node.kind() != SyntaxKind::Name {
        return Err(cx.unsupported(node));
    }
    match significant_tokens(node).first() {
        Some(token) => Ok(lower_id(cx, token)),
        None => Err(cx.internal_error(node.text_range(), "a name holds no token")),
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::lower::INTERNAL_ERROR;
    use crate::{parse_source_file, ParseOptions};
    use ironplc_dsl::core::FileId;

    fn names_of(source: &str, options: &ParseOptions, file: &FileId) -> Vec<Id> {
        let cx = LowerCx::new(file.clone());
        let parse = parse_source_file(source, options);
        parse
            .root
            .descendants()
            .filter(|node| node.kind() == SyntaxKind::Name)
            .filter_map(|node| lower_name(&cx, &node).ok())
            .collect()
    }

    #[test]
    fn lower_name_when_declared_name_then_original_spelling_lower_case_and_span() {
        let file = FileId::from_string("a.st");
        let ids = names_of(
            "PROGRAM Speed_Ctl\nEND_PROGRAM",
            &ParseOptions::default(),
            &file,
        );
        assert_eq!(ids.len(), 1);
        assert_eq!(ids[0].original(), "Speed_Ctl");
        assert_eq!(ids[0].lower_case(), "speed_ctl");
        assert_eq!(
            (ids[0].span.start, ids[0].span.end, &ids[0].span.file_id),
            (8, 17, &file)
        );
    }

    #[test]
    fn lower_name_when_escaped_name_then_backticks_kept() {
        let options = ParseOptions {
            allow_escaped_identifiers: true,
            ..ParseOptions::default()
        };
        let ids = names_of(
            "PROGRAM `my prog`\nEND_PROGRAM",
            &options,
            &FileId::default(),
        );
        assert_eq!(ids[0].original(), "`my prog`");
    }

    #[test]
    fn lower_name_when_node_is_not_a_name_then_internal_error() {
        let cx = LowerCx::new(FileId::default());
        let parse = crate::parse_expression("5", &ParseOptions::default());
        let literal = parse.root.first_child();
        let diagnostic = literal.and_then(|node| lower_name(&cx, &node).err());
        assert_eq!(diagnostic.map(|d| d.code), Some(INTERNAL_ERROR.to_string()));
    }

    #[test]
    fn lower_name_when_name_node_is_empty_then_internal_error() {
        // A name node holds one token for any tree the parser builds; an empty
        // one is reported, not assumed away.
        let green = rowan::GreenNode::new(
            rowan::SyntaxKind(SyntaxKind::Name as u16),
            std::iter::empty(),
        );
        let node = SyntaxNode::new_root(green);
        let diagnostic = lower_name(&LowerCx::new(FileId::default()), &node).err();
        assert_eq!(diagnostic.map(|d| d.code), Some(INTERNAL_ERROR.to_string()));
    }
}
