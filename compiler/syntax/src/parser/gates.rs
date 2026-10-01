//! Dialect gates on lexical forms.
//!
//! The lexer recognises every spelling in every dialect, so a form the
//! dialect has not enabled still lands in the tree, byte for byte. Each gate
//! here names such a form, the flag that enables it, and the error reported
//! when the flag is off. This is the CST counterpart of the legacy token
//! check rules and of the comment and pragma transforms (design: parse-tree
//! architecture, section 3.1): the syntax is kept and flagged rather than
//! rewritten or dropped.
//!
//! Gates on keywords are not here: a disabled keyword is an ordinary name,
//! see [`ParseOptions::keyword_enabled`].

use super::options::ParseOptions;
use crate::error::SyntaxError;
use crate::lexer::Token;
use crate::syntax_kind::SyntaxKind;

/// One gated lexical form.
struct Gate {
    /// True when the token is the gated form.
    applies: fn(&Token<'_>) -> bool,
    /// True when the dialect enables the form.
    enabled: fn(&ParseOptions) -> bool,
    message: &'static str,
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

const GATES: &[Gate] = &[
    Gate {
        applies: is_c_style_comment,
        enabled: |options| options.allow_c_style_comments,
        message: "C-style comments are not enabled in this dialect",
    },
    Gate {
        applies: is_nested_comment,
        enabled: |options| options.allow_nested_comments,
        message: "nested comments are not enabled in this dialect",
    },
    Gate {
        applies: is_nested_c_comment,
        enabled: |_| false,
        message: "`/* */` comments do not nest",
    },
    Gate {
        applies: |token| token.kind == SyntaxKind::Pragma,
        enabled: |options| options.allow_pragmas,
        message: "pragmas are not enabled in this dialect",
    },
    Gate {
        applies: is_partial_access,
        enabled: |options| options.allow_partial_access_syntax,
        message: "partial-access syntax is not enabled in this dialect",
    },
    Gate {
        applies: |token| token.kind == SyntaxKind::EscapedIdent,
        enabled: |options| options.allow_escaped_identifiers,
        message: "escaped identifiers are not enabled in this dialect",
    },
    Gate {
        applies: is_non_ascii_identifier,
        enabled: |options| options.allow_unicode_identifiers,
        message: "identifiers with letters outside ASCII are not enabled in this dialect",
    },
    Gate {
        applies: has_repeated_underscores,
        enabled: |options| options.allow_multiple_underscores,
        message: "consecutive underscores in an identifier are not enabled in this dialect",
    },
];

/// The errors for every gated form in `tokens` that `options` leaves
/// disabled, in source order.
pub(crate) fn gate_errors(tokens: &[Token<'_>], options: &ParseOptions) -> Vec<SyntaxError> {
    tokens
        .iter()
        .flat_map(|token| {
            GATES
                .iter()
                .filter(|gate| (gate.applies)(token) && !(gate.enabled)(options))
                .map(|gate| SyntaxError::new(gate.message, token.range))
        })
        .collect()
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
}
