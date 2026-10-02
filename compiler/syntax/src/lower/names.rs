//! Names: what a name token denotes.
//!
//! A name becomes an [`Id`] built the way the legacy grammar builds one:
//! `Id::from` on the token's text as written (an escaped name keeps its
//! backticks, a keyword the dialect has not enabled is an ordinary name) and
//! the token's own range as its position. The lower-casing is `Id`'s own. The
//! same rule reads the name a declaration introduces and the name a use of it
//! spells (`NameRef`).
//!
//! A type name differs in one respect: a type keyword stands for one type
//! however it is spelled (`TOD` and `time_of_day` are both `TIME_OF_DAY`), so
//! it becomes the keyword's canonical spelling, the first one the keyword
//! declaration lists. A user type is the name as written.

use super::tree::significant_tokens;
use super::LowerCx;
use crate::syntax_kind::{SyntaxKind, SyntaxNode, SyntaxToken};
use ironplc_dsl::common::TypeName;
use ironplc_dsl::core::Id;
use ironplc_dsl::diagnostic::Diagnostic;

/// The identifier a name token spells.
pub fn lower_id(cx: &LowerCx, token: &SyntaxToken) -> Id {
    Id::from(token.text()).with_position(cx.token_span(token))
}

/// The kinds of node that hold one name token: the name a declaration
/// introduces, and a use of a name.
const NAME_KINDS: &[SyntaxKind] = &[SyntaxKind::Name, SyntaxKind::NameRef];

/// Lowers a name: a node of one of the [`NAME_KINDS`] holding one token.
pub fn lower_name(cx: &LowerCx, node: &SyntaxNode) -> Result<Id, Diagnostic> {
    if !NAME_KINDS.contains(&node.kind()) {
        return Err(cx.unsupported(node));
    }
    match significant_tokens(node).first() {
        Some(token) => Ok(lower_id(cx, token)),
        None => Err(cx.internal_error(node.text_range(), "a name holds no token")),
    }
}

/// The canonical spelling of a keyword: the first one its declaration lists.
fn canonical_spelling(kind: SyntaxKind) -> Option<&'static str> {
    SyntaxKind::KEYWORDS
        .iter()
        .find(|(_, keyword)| *keyword == kind)
        .map(|(spelling, _)| *spelling)
}

/// Lowers a reference to a type: a [`SyntaxKind::TypeRef`] holding the type's
/// keyword or its name.
pub fn lower_type_ref(cx: &LowerCx, node: &SyntaxNode) -> Result<TypeName, Diagnostic> {
    if node.kind() != SyntaxKind::TypeRef {
        return Err(cx.unsupported(node));
    }
    let tokens = significant_tokens(node);
    let token = tokens
        .first()
        .ok_or_else(|| cx.missing(node, "a type name"))?;
    let name = match canonical_spelling(token.kind()) {
        Some(spelling) => Id::from(spelling).with_position(cx.token_span(token)),
        None => lower_id(cx, token),
    };
    Ok(TypeName { name })
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

    fn type_ref_of(source: &str, options: &ParseOptions) -> Option<SyntaxNode> {
        crate::parse_expression(&format!("__NEW({source})"), options)
            .root
            .descendants()
            .find(|node| node.kind() == SyntaxKind::TypeRef)
    }

    fn type_name(source: &str) -> Option<TypeName> {
        let node = type_ref_of(source, &ParseOptions::all())?;
        lower_type_ref(&LowerCx::new(FileId::from_string("t.st")), &node).ok()
    }

    #[test]
    fn lower_name_when_name_reference_then_the_identifier_at_its_token() {
        let parse = crate::parse_expression("  speed ", &ParseOptions::default());
        let node = parse.root.first_child().expect("a node");
        assert_eq!(node.kind(), SyntaxKind::NameRef);
        let id = lower_name(&LowerCx::new(FileId::default()), &node).expect("a name");
        assert_eq!(id.original(), "speed");
        assert_eq!((id.span.start, id.span.end), (2, 7));
    }

    #[test]
    fn lower_type_ref_when_type_keyword_then_its_canonical_spelling_at_the_keyword() {
        let rows = [
            ("INT", "INT"),
            ("int", "INT"),
            ("Lreal", "LREAL"),
            ("tod", "TIME_OF_DAY"),
            ("TIME_OF_DAY", "TIME_OF_DAY"),
            ("dt", "DATE_AND_TIME"),
            ("ltod", "LTIME_OF_DAY"),
            ("ldt", "LDATE_AND_TIME"),
            ("string", "STRING"),
            ("wstring", "WSTRING"),
            ("any", "ANY"),
            ("any_num", "ANY_NUM"),
        ];
        for (written, canonical) in rows {
            let name = type_name(written).expect("a type name");
            assert_eq!(name.name.original(), canonical, "{written}");
            assert_eq!(
                (
                    name.name.span.start,
                    name.name.span.end,
                    &name.name.span.file_id
                ),
                (6, 6 + written.len(), &FileId::from_string("t.st")),
                "{written} is positioned at its token"
            );
        }
    }

    #[test]
    fn lower_type_ref_when_elementary_type_then_the_name_the_legacy_grammar_gives_it() {
        use ironplc_dsl::common::ElementaryTypeName as E;
        for elementary in [
            E::BOOL,
            E::BIT,
            E::SINT,
            E::INT,
            E::DINT,
            E::LINT,
            E::USINT,
            E::UINT,
            E::UDINT,
            E::ULINT,
            E::REAL,
            E::LREAL,
            E::TIME,
            E::LTIME,
            E::DATE,
            E::LDATE,
            E::TimeOfDay,
            E::LTimeOfDay,
            E::DateAndTime,
            E::LDateAndTime,
            E::STRING,
            E::BYTE,
            E::WORD,
            E::DWORD,
            E::LWORD,
            E::WSTRING,
        ] {
            let expected = elementary.as_id();
            let name = type_name(expected.original()).expect("a type name");
            assert_eq!(name.name.original(), expected.original());
        }
    }

    #[test]
    fn lower_type_ref_when_user_type_then_the_name_as_written() {
        let name = type_name("MyFb").expect("a type name");
        assert_eq!(name.name.original(), "MyFb");
        assert_eq!(name.name.lower_case(), "myfb");
        let options = ParseOptions {
            allow_escaped_identifiers: true,
            ..ParseOptions::all()
        };
        let node = type_ref_of("`my type`", &options).expect("a type reference");
        let escaped = lower_type_ref(&LowerCx::new(FileId::default()), &node).expect("a name");
        assert_eq!(escaped.name.original(), "`my type`");
    }

    #[test]
    fn lower_type_ref_when_node_is_not_a_type_reference_or_holds_nothing_then_internal_error() {
        let cx = LowerCx::new(FileId::default());
        let parse = crate::parse_expression("5", &ParseOptions::default());
        let literal = parse.root.first_child().expect("a node");
        assert_eq!(
            lower_type_ref(&cx, &literal).err().map(|d| d.code),
            Some(INTERNAL_ERROR.to_string())
        );
        let green = rowan::GreenNode::new(
            rowan::SyntaxKind(SyntaxKind::TypeRef as u16),
            std::iter::empty(),
        );
        assert_eq!(
            lower_type_ref(&cx, &SyntaxNode::new_root(green))
                .err()
                .map(|d| d.code),
            Some(INTERNAL_ERROR.to_string())
        );
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
