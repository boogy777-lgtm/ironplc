//! Dialect gates on lexical forms and token sequences.
//!
//! The lexer recognises every spelling in every dialect, so a form the
//! dialect has not enabled still lands in the tree, byte for byte. Each gate
//! here names such a form, the flag that enables it, and the error reported
//! when the flag is off. This is the CST counterpart of the legacy token
//! check rules and of the comment and pragma transforms (design: parse-tree
//! architecture, section 3.1): the syntax is kept and flagged rather than
//! rewritten or dropped.
//!
//! A gate looks at one token and, when the form is a short run of tokens
//! (`ARRAY [ *`, `STRING (`, `VAR END_VAR`), at the significant tokens after
//! it. The grammar accepts the form and the gate is what enforces the flag,
//! exactly as the legacy token rules do. A rule that is about the literal's
//! own spelling and not a dialect flag (an undefined `$` escape) is in the
//! same table with an `enabled` that is never true.
//!
//! Gates on keywords are not here: a disabled keyword is an ordinary name,
//! see [`ParseOptions::keyword_enabled`].

use super::options::ParseOptions;
use super::recovery::VAR_OPENERS;
use crate::error::SyntaxError;
use crate::lexer::escapes::invalid_escapes;
use crate::lexer::Token;
use crate::syntax_kind::SyntaxKind;
use rowan::{TextRange, TextSize};

/// The token a gate is asked about, with the tokens after it.
struct Site<'a, 't> {
    tokens: &'a [Token<'t>],
    index: usize,
    options: &'a ParseOptions,
}

impl<'t> Site<'_, 't> {
    fn token(&self) -> Option<&Token<'t>> {
        self.tokens.get(self.index)
    }

    /// The significant tokens after this one, trivia skipped.
    fn following(&self) -> impl Iterator<Item = &Token<'t>> {
        self.tokens
            .get(self.index + 1..)
            .unwrap_or_default()
            .iter()
            .filter(|token| !token.kind.is_trivia())
    }
}

/// One gated form.
struct Gate {
    /// The ranges, at this site, of the gated form: empty when it is not here.
    find: fn(&Site) -> Vec<TextRange>,
    /// True when the dialect enables the form.
    enabled: fn(&ParseOptions) -> bool,
    message: &'static str,
}

/// The gate for a form that is one token: its range when `applies`.
fn on_token(site: &Site, applies: fn(&Token<'_>) -> bool) -> Vec<TextRange> {
    site.token()
        .filter(|token| applies(token))
        .map(|token| token.range)
        .into_iter()
        .collect()
}

fn is_c_style_comment(token: &Token<'_>) -> bool {
    match token.kind {
        SyntaxKind::LineComment | SyntaxKind::DocComment => true,
        SyntaxKind::BlockComment => token.text.starts_with("/*"),
        _ => false,
    }
}

/// A `(* *)` comment that holds an inner `(*`. The lexer always nests; a
/// dialect without nesting ends the comment at the first `*)`.
fn is_nested_comment(token: &Token<'_>) -> bool {
    token.kind == SyntaxKind::BlockComment
        && token.text.starts_with("(*")
        && token.text.get(2..).is_some_and(|rest| rest.contains("(*"))
}

/// A `/* */` comment that holds an inner `/*`. No dialect nests these.
fn is_nested_c_comment(token: &Token<'_>) -> bool {
    token.kind == SyntaxKind::BlockComment
        && token.text.starts_with("/*")
        && token.text.get(2..).is_some_and(|rest| rest.contains("/*"))
}

fn is_partial_access(token: &Token<'_>) -> bool {
    token.kind == SyntaxKind::PartialAccess
}

fn is_non_ascii_identifier(token: &Token<'_>) -> bool {
    token.kind == SyntaxKind::Ident && !token.text.is_ascii()
}

/// A double underscore anywhere but in the reserved leading `__` prefix.
fn has_repeated_underscores(token: &Token<'_>) -> bool {
    if token.kind != SyntaxKind::Ident {
        return false;
    }
    match token.text.find("__") {
        Some(0) => token.text.get(2..).is_some_and(|rest| rest.contains("__")),
        Some(_) => true,
        None => false,
    }
}

/// `STRING (` and `WSTRING (`: the length in parentheses. Neither keyword is
/// callable, so a `(` after one is the length delimiter. The range is the `(`.
fn paren_string_length(site: &Site) -> Vec<TextRange> {
    let is_string = site
        .token()
        .is_some_and(|token| matches!(token.kind, SyntaxKind::String | SyntaxKind::WString));
    if !is_string {
        return Vec::new();
    }
    site.following()
        .next()
        .filter(|next| next.kind == SyntaxKind::LeftParen)
        .map(|next| next.range)
        .into_iter()
        .collect()
}

/// `ARRAY [ *`: the bounds are left to the caller. The range is the `*`.
fn incomplete_array(site: &Site) -> Vec<TextRange> {
    if site.token().map(|token| token.kind) != Some(SyntaxKind::Array) {
        return Vec::new();
    }
    let mut next = site.following();
    match (next.next(), next.next()) {
        (Some(bracket), Some(star))
            if bracket.kind == SyntaxKind::LeftBracket && star.kind == SyntaxKind::Star =>
        {
            vec![star.range]
        }
        _ => Vec::new(),
    }
}

/// A variable block opener that is followed by nothing but its qualifiers and
/// `END_VAR`. The range is the opener. A keyword the dialect leaves disabled
/// opens nothing, and `PERSISTENT` is a qualifier only where it is a keyword.
fn empty_var_block(site: &Site) -> Vec<TextRange> {
    let Some(opener) = site.token() else {
        return Vec::new();
    };
    if !VAR_OPENERS.contains(&opener.kind) || !site.options.keyword_enabled(opener.kind) {
        return Vec::new();
    }
    let qualifiers = [
        SyntaxKind::Constant,
        SyntaxKind::Retain,
        SyntaxKind::NonRetain,
        SyntaxKind::Persistent,
    ];
    let first = site.following().find(|token| {
        !(qualifiers.contains(&token.kind) && site.options.keyword_enabled(token.kind))
    });
    match first {
        Some(token) if token.kind == SyntaxKind::EndVar => vec![opener.range],
        _ => Vec::new(),
    }
}

/// Each `$` escape in a character string that the standard does not define.
/// The ranges are over the literal's source text, delimiters included.
fn invalid_string_escapes(site: &Site) -> Vec<TextRange> {
    let Some(token) = site.token() else {
        return Vec::new();
    };
    let wide = match token.kind {
        SyntaxKind::StringLit => false,
        SyntaxKind::WStringLit => true,
        _ => return Vec::new(),
    };
    let Some(inner) = token.text.get(1..token.text.len().saturating_sub(1)) else {
        return Vec::new();
    };
    let base = u32::from(token.range.start()) + 1;
    invalid_escapes(inner, wide)
        .into_iter()
        .filter_map(|range| {
            let start = base.checked_add(u32::try_from(range.start).ok()?)?;
            let end = base.checked_add(u32::try_from(range.end).ok()?)?;
            Some(TextRange::new(TextSize::from(start), TextSize::from(end)))
        })
        .collect()
}

const GATES: &[Gate] = &[
    Gate {
        find: |site| on_token(site, is_c_style_comment),
        enabled: |options| options.allow_c_style_comments,
        message: "C-style comments are not enabled in this dialect",
    },
    Gate {
        find: |site| on_token(site, is_nested_comment),
        enabled: |options| options.allow_nested_comments,
        message: "nested comments are not enabled in this dialect",
    },
    Gate {
        find: |site| on_token(site, is_nested_c_comment),
        enabled: |_| false,
        message: "`/* */` comments do not nest",
    },
    Gate {
        find: |site| on_token(site, |token| token.kind == SyntaxKind::Pragma),
        enabled: |options| options.allow_pragmas,
        message: "pragmas are not enabled in this dialect",
    },
    Gate {
        find: |site| on_token(site, is_partial_access),
        enabled: |options| options.allow_partial_access_syntax,
        message: "partial-access syntax is not enabled in this dialect",
    },
    Gate {
        find: |site| on_token(site, |token| token.kind == SyntaxKind::EscapedIdent),
        enabled: |options| options.allow_escaped_identifiers,
        message: "escaped identifiers are not enabled in this dialect",
    },
    Gate {
        find: |site| on_token(site, is_non_ascii_identifier),
        enabled: |options| options.allow_unicode_identifiers,
        message: "identifiers with letters outside ASCII are not enabled in this dialect",
    },
    Gate {
        find: |site| on_token(site, has_repeated_underscores),
        enabled: |options| options.allow_multiple_underscores,
        message: "consecutive underscores in an identifier are not enabled in this dialect",
    },
    Gate {
        find: empty_var_block,
        enabled: |options| options.allow_empty_var_blocks,
        message: "empty variable blocks are not enabled in this dialect",
    },
    Gate {
        find: paren_string_length,
        enabled: |options| options.allow_paren_string_length,
        message: "a string length in parentheses is not enabled in this dialect",
    },
    Gate {
        find: incomplete_array,
        enabled: |options| options.allow_incomplete_array,
        message: "incomplete array bounds are not enabled in this dialect",
    },
    Gate {
        find: invalid_string_escapes,
        enabled: |_| false,
        message: "this `$` escape is not defined for character strings",
    },
];

/// The errors for every gated form in `tokens` that `options` leaves
/// disabled, in source order.
pub(crate) fn gate_errors(tokens: &[Token<'_>], options: &ParseOptions) -> Vec<SyntaxError> {
    let mut errors = Vec::new();
    for index in 0..tokens.len() {
        let site = Site {
            tokens,
            index,
            options,
        };
        for gate in GATES.iter().filter(|gate| !(gate.enabled)(options)) {
            errors.extend(
                (gate.find)(&site)
                    .into_iter()
                    .map(|range| SyntaxError::new(gate.message, range)),
            );
        }
    }
    errors
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::lexer::lex;

    fn messages(source: &str, options: &ParseOptions) -> Vec<String> {
        let (tokens, _) = lex(source);
        gate_errors(&tokens, options)
            .into_iter()
            .map(|error| error.message)
            .collect()
    }

    fn ranges(source: &str, options: &ParseOptions) -> Vec<(usize, usize)> {
        let (tokens, _) = lex(source);
        gate_errors(&tokens, options)
            .into_iter()
            .map(|error| {
                (
                    usize::from(error.range.start()),
                    usize::from(error.range.end()),
                )
            })
            .collect()
    }

    #[test]
    fn gate_errors_when_c_style_comment_and_flag_off_then_error_on_the_comment() {
        let (tokens, _) = lex("x // note\n");
        let errors = gate_errors(&tokens, &ParseOptions::default());
        assert_eq!(errors.len(), 1);
        assert_eq!(usize::from(errors[0].range.start()), 2);
        let allowed = ParseOptions {
            allow_c_style_comments: true,
            ..ParseOptions::default()
        };
        assert!(gate_errors(&tokens, &allowed).is_empty());
    }

    #[test]
    fn gate_errors_when_nested_block_comment_then_only_without_the_nesting_flag() {
        let source = "(* a (* b *) c *)";
        assert_eq!(messages(source, &ParseOptions::default()).len(), 1);
        assert!(messages(source, &ParseOptions::all()).is_empty());
    }

    #[test]
    fn gate_errors_when_plain_block_comment_then_no_error() {
        assert!(messages("(* a *)", &ParseOptions::default()).is_empty());
    }

    #[test]
    fn gate_errors_when_nested_c_comment_then_error_in_every_dialect() {
        assert_eq!(messages("/* a /* b */ c */", &ParseOptions::all()).len(), 1);
    }

    #[test]
    fn gate_errors_when_pragma_partial_access_or_escape_then_each_follows_its_flag() {
        for source in ["{attribute 'x'}", "a.%X3", "`a b`"] {
            assert_eq!(
                messages(source, &ParseOptions::default()).len(),
                1,
                "{source}"
            );
            assert!(
                messages(source, &ParseOptions::all()).is_empty(),
                "{source}"
            );
        }
    }

    #[test]
    fn gate_errors_when_identifier_spelling_then_unicode_and_underscores_gated() {
        let none = ParseOptions::default();
        assert_eq!(messages("caf\u{e9}", &none).len(), 1);
        assert_eq!(messages("my__var", &none).len(), 1);
        assert!(messages("__NEW", &none).is_empty());
        assert!(messages("__x", &none).is_empty());
        assert_eq!(messages("__x__y", &none).len(), 1);
        assert!(messages("my__var", &ParseOptions::all()).is_empty());
    }

    #[test]
    fn gate_errors_when_paren_string_length_then_error_on_the_paren_until_flag_on() {
        let none = ParseOptions::default();
        assert_eq!(ranges("x : STRING(80);", &none), vec![(10, 11)]);
        assert_eq!(ranges("x : WSTRING (* c *) (80);", &none), vec![(20, 21)]);
        assert!(ranges("x : STRING[80];", &none).is_empty());
        assert!(ranges("x : STRING;", &none).is_empty());
        assert!(messages("x : STRING(80);", &ParseOptions::all()).is_empty());
    }

    #[test]
    fn gate_errors_when_incomplete_array_then_error_on_the_star_until_flag_on() {
        let none = ParseOptions::default();
        assert_eq!(ranges("x : ARRAY [ * ] OF INT;", &none), vec![(12, 13)]);
        assert!(ranges("x : ARRAY [1..2] OF INT;", &none).is_empty());
        assert!(messages("x : ARRAY[*] OF INT;", &ParseOptions::all()).is_empty());
    }

    #[test]
    fn gate_errors_when_empty_var_block_then_error_on_the_opener_until_flag_on() {
        let none = ParseOptions::default();
        assert_eq!(ranges("VAR END_VAR", &none), vec![(0, 3)]);
        assert_eq!(
            ranges("VAR_INPUT RETAIN (* c *) END_VAR", &none),
            vec![(0, 9)]
        );
        assert!(ranges("VAR x : INT; END_VAR", &none).is_empty());
        assert!(messages("VAR END_VAR", &ParseOptions::all()).is_empty());
    }

    #[test]
    fn gate_errors_when_empty_block_opens_with_a_disabled_keyword_then_not_a_block() {
        // `VAR_STAT` is a name here, so nothing is an empty block; with the
        // keyword on it is one.
        assert!(messages("VAR_STAT END_VAR", &ParseOptions::default()).is_empty());
        let stat = ParseOptions {
            allow_var_stat: true,
            ..ParseOptions::default()
        };
        assert_eq!(messages("VAR_STAT END_VAR", &stat).len(), 1);
    }

    #[test]
    fn gate_errors_when_string_escape_undefined_then_error_on_the_escape_in_every_dialect() {
        assert_eq!(
            ranges("x := 'a$Qb';", &ParseOptions::all()),
            vec![(7, 9)]
        );
        assert_eq!(
            ranges("x := \"a$D800\";", &ParseOptions::all()),
            vec![(7, 9)]
        );
        assert!(ranges("x := 'a$$b$41';", &ParseOptions::all()).is_empty());
    }
}
