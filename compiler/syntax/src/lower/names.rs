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

use super::tree::{left_spine, significant_tokens};
use super::LowerCx;
use crate::syntax_kind::{SyntaxKind, SyntaxNode, SyntaxToken};
use ironplc_dsl::common::{ElementaryTypeName, GenericTypeName, TypeName};
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

/// The names of a dotted path (`a.b.c`): a name, and the member names selected
/// from it, in the order written. `expected` says what the path was to be, for
/// the problem reported when the node is not one.
pub fn lower_path(cx: &LowerCx, node: &SyntaxNode, expected: &str) -> Result<Vec<Id>, Diagnostic> {
    let (base, links) = left_spine(node, |kind| kind == SyntaxKind::FieldExpr);
    let base = base
        .filter(|base| base.kind() == SyntaxKind::NameRef)
        .ok_or_else(|| cx.syntax_error(node.text_range(), format!("expected {expected}")))?;
    let mut path = vec![lower_name(cx, &base)?];
    for link in &links {
        let selected = significant_tokens(link)
            .pop()
            .ok_or_else(|| cx.missing(link, "a name"))?;
        path.push(lower_id(cx, &selected));
    }
    Ok(path)
}

/// The kind of the token that precedes `token`, trivia left out.
fn previous_significant(token: &SyntaxToken) -> Option<SyntaxKind> {
    std::iter::successors(token.prev_token(), SyntaxToken::prev_token)
        .find(|previous| !previous.kind().is_trivia())
        .map(|previous| previous.kind())
}

/// The kind of the token that follows `token`, trivia left out.
fn next_significant(token: &SyntaxToken) -> Option<SyntaxKind> {
    std::iter::successors(token.next_token(), SyntaxToken::next_token)
        .find(|next| !next.kind().is_trivia())
        .map(|next| next.kind())
}

/// True when a keyword token is an ordinary name where it stands: the dialect
/// leaves the word available as a name, or it is `TIME` where the dialect lets
/// it name a function and `FUNCTION`, a call or an assignment is next to it
/// (`ParseOptions::time_is_name`). The parser makes the same decision for the
/// same token (`Parser::name_at`), so a word the parser read as a name is never
/// lowered as a keyword.
fn is_name_word(cx: &LowerCx, token: &SyntaxToken) -> bool {
    !cx.keyword_enabled(token.kind())
        || (token.kind() == SyntaxKind::Time
            && cx.time_is_name(previous_significant(token), next_significant(token)))
}

/// The canonical spelling of the type a token names as a keyword: the first
/// spelling its declaration lists. A token that is not a type keyword has none,
/// and neither has a type keyword that is a name where it stands: it is a user
/// type there, named as written.
pub fn type_keyword(cx: &LowerCx, token: &SyntaxToken) -> Option<&'static str> {
    let (spelling, _) = SyntaxKind::KEYWORDS
        .iter()
        .find(|(_, keyword)| *keyword == token.kind())?;
    let id = Id::from(spelling);
    let names_a_type =
        ElementaryTypeName::try_from(&id).is_ok() || GenericTypeName::try_from(&id).is_ok();
    (names_a_type && !is_name_word(cx, token)).then_some(*spelling)
}

/// The type a token names: a type keyword is its canonical spelling, and any
/// other token is a user type, named as written.
pub fn lower_type_token(cx: &LowerCx, token: &SyntaxToken) -> TypeName {
    let name = match type_keyword(cx, token) {
        Some(spelling) => Id::from(spelling).with_position(cx.token_span(token)),
        None => lower_id(cx, token),
    };
    TypeName { name }
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
    Ok(lower_type_token(cx, token))
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::lower::INTERNAL_ERROR;
    use crate::{parse_expression, parse_source_file, ParseOptions};
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

    /// The type name of the first type reference in `__NEW(<text>)`, lowered
    /// for text parsed under `options`.
    fn type_named(text: &str, options: &ParseOptions) -> Option<String> {
        let parse = parse_expression(&format!("__NEW({text})"), options);
        let node = parse
            .root
            .descendants()
            .find(|node| node.kind() == SyntaxKind::TypeRef)?;
        let cx = LowerCx::new(FileId::default()).with_options(*options);
        lower_type_ref(&cx, &node)
            .ok()
            .map(|name| name.name.original().to_string())
    }

    #[test]
    fn lower_type_ref_when_keyword_is_left_a_name_by_the_dialect_then_the_name_as_written() {
        let strict = ParseOptions::default();
        let all = ParseOptions::all();
        assert_eq!(type_named("bit", &strict).as_deref(), Some("bit"));
        assert_eq!(type_named("bit", &all).as_deref(), Some("BIT"));
        assert_eq!(type_named("ltime", &strict).as_deref(), Some("ltime"));
        assert_eq!(type_named("ltime", &all).as_deref(), Some("LTIME"));
        // A keyword that is no type is a name wherever a type is named.
        assert_eq!(type_named("mod", &all).as_deref(), Some("mod"));
    }

    #[test]
    fn keyword_enabled_when_context_built_with_options_then_those_options_decide() {
        let cx = LowerCx::new(FileId::default());
        assert!(cx.keyword_enabled(SyntaxKind::Bit));
        let strict = cx.with_options(ParseOptions::default());
        assert!(!strict.keyword_enabled(SyntaxKind::Bit));
        assert!(strict.keyword_enabled(SyntaxKind::If));
    }
}
