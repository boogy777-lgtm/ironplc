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
//!
//! A gate also names the kinds of token at which it can start (its
//! `triggers`). One pass over the tokens hands a token only to the gates that
//! name its kind and that the dialect leaves inactive, through an index from a
//! kind to its gates that is derived from the table, so the cost of a pass is
//! the tokens a gate can report at, not every token times every gate. A test
//! holds the triggers to the gates: a gate reports nothing at a token whose
//! kind it does not name.

use super::options::ParseOptions;
use super::recovery::VAR_OPENERS;
use crate::error::{ErrorKind, SyntaxError};
use crate::lexer::escapes::invalid_escapes;
use crate::lexer::Token;
use crate::syntax_kind::SyntaxKind;
use rowan::{TextRange, TextSize};
use std::sync::OnceLock;

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

    /// The significant tokens after this one, trivia skipped. A pragma the
    /// dialect has not enabled is not trivia: it is a token in the way, so a
    /// form it interrupts (`STRING {attr} (10)`, `VAR {attr} END_VAR`) is not
    /// the form, and the pragma is what is reported.
    fn following(&self) -> impl Iterator<Item = &Token<'t>> {
        let pragmas = self.options.allow_pragmas;
        self.tokens
            .get(self.index + 1..)
            .unwrap_or_default()
            .iter()
            .filter(move |token| {
                !token.kind.is_trivia() || (token.kind == SyntaxKind::Pragma && !pragmas)
            })
    }
}

/// One gated form.
struct Gate {
    /// The kinds of token at which the form can start: [`Gate::find`] reports
    /// nothing at a token of any other kind.
    triggers: &'static [SyntaxKind],
    /// The ranges, at this site, of the gated form: empty when it is not here.
    find: fn(&Site) -> Vec<TextRange>,
    /// True when the dialect enables the form.
    enabled: fn(&ParseOptions) -> bool,
    /// The kind of error, which decides its problem code.
    kind: ErrorKind,
    message: &'static str,
    /// What to do about the form, when it is the same every time.
    help: Option<&'static str>,
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

fn is_partial_access(token: &Token<'_>) -> bool {
    token.kind == SyntaxKind::PartialAccess
}

fn is_non_ascii_identifier(token: &Token<'_>) -> bool {
    token.kind == SyntaxKind::Ident && !token.text.is_ascii()
}

/// A double underscore anywhere but in the reserved leading `__` prefix.
fn has_repeated_underscores(token: &Token<'_>) -> bool {
    // Most names have no underscore, and looking for one byte is cheaper than
    // searching for two.
    if token.kind != SyntaxKind::Ident || !token.text.contains('_') {
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

/// Each `$` escape in a character string literal of kind `literal` (a string or
/// a wide string) that the standard does not define. The ranges are over the
/// literal's source text, delimiters included.
fn invalid_string_escapes(site: &Site, literal: SyntaxKind) -> Vec<TextRange> {
    let Some(token) = site.token().filter(|token| token.kind == literal) else {
        return Vec::new();
    };
    let wide = literal == SyntaxKind::WStringLit;
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
        triggers: &[
            SyntaxKind::LineComment,
            SyntaxKind::DocComment,
            SyntaxKind::BlockComment,
        ],
        find: |site| on_token(site, is_c_style_comment),
        enabled: |options| options.allow_c_style_comments,
        kind: ErrorKind::CStyleComment,
        message: "C-style comments are not enabled in this dialect",
        help: Some(
            "Convert the comment to IEC 61131-3 syntax using `(*` and `*)`, \
             or select a dialect that supports C-style comments.",
        ),
    },
    Gate {
        triggers: &[SyntaxKind::Pragma],
        find: |site| on_token(site, |token| token.kind == SyntaxKind::Pragma),
        enabled: |options| options.allow_pragmas,
        kind: ErrorKind::Syntax,
        message: "pragmas are not enabled in this dialect",
        help: None,
    },
    Gate {
        triggers: &[SyntaxKind::PartialAccess],
        find: |site| on_token(site, is_partial_access),
        enabled: |options| options.allow_partial_access_syntax,
        kind: ErrorKind::PartialAccessSyntaxDisabled,
        message: "partial-access syntax is not enabled in this dialect",
        help: None,
    },
    Gate {
        triggers: &[SyntaxKind::EscapedIdent],
        find: |site| on_token(site, |token| token.kind == SyntaxKind::EscapedIdent),
        enabled: |options| options.allow_escaped_identifiers,
        kind: ErrorKind::EscapedIdentifierNotAllowed,
        message: "escaped identifiers are not enabled in this dialect",
        help: None,
    },
    Gate {
        triggers: &[SyntaxKind::Ident],
        find: |site| on_token(site, is_non_ascii_identifier),
        enabled: |options| options.allow_unicode_identifiers,
        kind: ErrorKind::UnicodeIdentifierNotAllowed,
        message: "identifiers with letters outside ASCII are not enabled in this dialect",
        help: None,
    },
    Gate {
        triggers: &[SyntaxKind::Ident],
        find: |site| on_token(site, has_repeated_underscores),
        enabled: |options| options.allow_multiple_underscores,
        kind: ErrorKind::MultipleUnderscoresNotAllowed,
        message: "consecutive underscores in an identifier are not enabled in this dialect",
        help: None,
    },
    Gate {
        triggers: VAR_OPENERS,
        find: empty_var_block,
        enabled: |options| options.allow_empty_var_blocks,
        kind: ErrorKind::EmptyVarBlock,
        message: "empty variable blocks are not enabled in this dialect",
        help: None,
    },
    Gate {
        triggers: &[SyntaxKind::String, SyntaxKind::WString],
        find: paren_string_length,
        enabled: |options| options.allow_paren_string_length,
        kind: ErrorKind::ParenStringLengthNotAllowed,
        message: "a string length in parentheses is not enabled in this dialect",
        help: None,
    },
    Gate {
        triggers: &[SyntaxKind::Array],
        find: incomplete_array,
        enabled: |options| options.allow_incomplete_array,
        kind: ErrorKind::IncompleteArrayNotAllowed,
        message: "incomplete array bounds are not enabled in this dialect",
        help: None,
    },
    Gate {
        triggers: &[SyntaxKind::StringLit],
        find: |site| invalid_string_escapes(site, SyntaxKind::StringLit),
        enabled: |_| false,
        kind: ErrorKind::InvalidStringEscape,
        message: "this `$` escape is not defined for character strings",
        help: Some(
            "Use $$, $', $L, $N, $P, $R, $T, $ followed by two hex digits, \
             or $U followed by eight hex digits.",
        ),
    },
    Gate {
        triggers: &[SyntaxKind::WStringLit],
        find: |site| invalid_string_escapes(site, SyntaxKind::WStringLit),
        enabled: |_| false,
        kind: ErrorKind::InvalidStringEscape,
        message: "this `$` escape is not defined for character strings",
        help: Some(
            "Use $$, $\", $L, $N, $P, $R, $T, $ followed by four hex digits, \
             or $U followed by eight hex digits.",
        ),
    },
];

/// A set of gates, one bit each: bit `g` is `GATES[g]`.
type GateSet = u64;

const _: () = assert!(GATES.len() <= GateSet::BITS as usize);

/// For each kind of token, the gates that name it as a trigger. Derived from
/// [`GATES`] once, so a gate is registered by its row alone.
fn trigger_index() -> &'static [GateSet] {
    static INDEX: OnceLock<Vec<GateSet>> = OnceLock::new();
    INDEX.get_or_init(|| {
        let mut index = vec![0; SyntaxKind::ALL.len()];
        for (position, gate) in GATES.iter().enumerate() {
            for kind in gate.triggers {
                if let Some(slot) = index.get_mut(*kind as usize) {
                    *slot |= 1 << position;
                }
            }
        }
        index
    })
}

/// The gates that report a form the dialect leaves disabled.
fn inactive_gates(options: &ParseOptions) -> GateSet {
    GATES
        .iter()
        .enumerate()
        .filter(|(_, gate)| !(gate.enabled)(options))
        .fold(0, |set, (position, _)| set | 1 << position)
}

/// The errors for every gated form in `tokens` that `options` leaves
/// disabled, in source order.
pub(crate) fn gate_errors(tokens: &[Token<'_>], options: &ParseOptions) -> Vec<SyntaxError> {
    let mut errors = Vec::new();
    let inactive = inactive_gates(options);
    if inactive == 0 {
        return errors;
    }
    let index = trigger_index();
    for (position, token) in tokens.iter().enumerate() {
        let mut here = index.get(token.kind as usize).copied().unwrap_or(0) & inactive;
        while here != 0 {
            let gate = &GATES[here.trailing_zeros() as usize];
            here &= here - 1;
            let site = Site {
                tokens,
                index: position,
                options,
            };
            errors.extend((gate.find)(&site).into_iter().map(|range| {
                let error = SyntaxError::new(gate.message, range).with_kind(gate.kind);
                match gate.help {
                    Some(help) => error.with_help(help),
                    None => error,
                }
            }));
        }
    }
    errors
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::lexer::lex;
    use crate::parse_source_file;
    use ironplc_dsl::core::FileId;

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
    fn gate_errors_when_gate_has_advice_then_the_error_carries_it() {
        let (tokens, _) = lex("x // note\n 'a$Qb' \"a$Qb\"");
        let errors = gate_errors(&tokens, &ParseOptions::default());
        let help: Vec<Option<&str>> = errors.iter().map(|error| error.help).collect();
        assert_eq!(errors.len(), 3);
        assert!(help.iter().all(Option::is_some), "{help:?}");
        // The two string widths give different advice.
        assert_ne!(help[1], help[2]);
        // A gate without advice gives none.
        let (tokens, _) = lex("a.%X3");
        let errors = gate_errors(&tokens, &ParseOptions::default());
        assert_eq!(errors.len(), 1);
        assert_eq!(errors[0].help, None);
        let diagnostics =
            parse_source_file("x // c", &ParseOptions::default()).diagnostics(&FileId::default());
        assert!(diagnostics.iter().any(|d| !d.help().is_empty()));
    }

    #[test]
    fn gate_errors_when_plain_block_comment_then_no_error() {
        assert!(messages("(* a *)", &ParseOptions::default()).is_empty());
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
    fn gate_errors_when_a_pragma_the_dialect_lacks_interrupts_a_form_then_it_is_not_the_form() {
        // The pragma is a token in the way where the dialect has none, so the
        // form is not there and the pragma is what is reported (by the
        // grammar); where pragmas are enabled it is trivia and the form stands.
        let none = ParseOptions::default();
        let pragmas = ParseOptions {
            allow_pragmas: true,
            ..ParseOptions::default()
        };
        let kinds = |source: &str, options: &ParseOptions| -> Vec<ErrorKind> {
            let (tokens, _) = lex(source);
            gate_errors(&tokens, options)
                .into_iter()
                .map(|error| error.kind)
                .collect()
        };
        // Without pragmas the only error is the pragma's own.
        assert_eq!(
            kinds("x : STRING {a} (10);", &none),
            vec![ErrorKind::Syntax]
        );
        assert_eq!(kinds("VAR {a} END_VAR", &none), vec![ErrorKind::Syntax]);
        assert_eq!(
            kinds("x : STRING {a} (10);", &pragmas),
            vec![ErrorKind::ParenStringLengthNotAllowed]
        );
        assert_eq!(
            kinds("VAR {a} END_VAR", &pragmas),
            vec![ErrorKind::EmptyVarBlock]
        );
    }

    #[test]
    fn gate_errors_when_string_escape_undefined_then_error_on_the_escape_in_every_dialect() {
        assert_eq!(ranges("x := 'a$Qb';", &ParseOptions::all()), vec![(7, 9)]);
        assert_eq!(
            ranges("x := \"a$D800\";", &ParseOptions::all()),
            vec![(7, 9)]
        );
        assert!(ranges("x := 'a$$b$41';", &ParseOptions::all()).is_empty());
    }

    /// Every option set the gates are asked about: the strict defaults, every
    /// flag on, and each flag on alone. The sets differ in which regions and
    /// pragmas the tokens hold.
    fn option_sets() -> Vec<ParseOptions> {
        let mut sets = vec![ParseOptions::default(), ParseOptions::all()];
        for key in ParseOptions::FLAG_KEYS {
            let mut options = ParseOptions::default();
            assert!(options.set_flag_by_key(key, true), "{key}");
            sets.push(options);
        }
        sets
    }

    #[test]
    fn triggers_when_gate_declared_then_named_and_unique_token_kinds() {
        for gate in GATES {
            assert!(!gate.triggers.is_empty(), "{}", gate.message);
            for (position, kind) in gate.triggers.iter().enumerate() {
                assert!(kind.is_token(), "{kind:?}");
                assert!(!gate.triggers[..position].contains(kind), "{kind:?}");
            }
        }
    }

    #[test]
    fn trigger_index_when_built_then_each_kind_holds_exactly_the_gates_that_name_it() {
        let index = trigger_index();
        assert_eq!(index.len(), SyntaxKind::ALL.len());
        for kind in SyntaxKind::ALL {
            for (position, gate) in GATES.iter().enumerate() {
                let set = index[*kind as usize] & (1 << position) != 0;
                assert_eq!(set, gate.triggers.contains(kind), "{kind:?}");
            }
        }
    }

    #[test]
    fn find_when_token_kind_is_not_a_trigger_then_the_gate_reports_nothing() {
        // The guard of the trigger sets: over the corpus in every spelling and
        // every option set, no gate reports at a token whose kind it does not
        // name, so a wrong trigger set fails here and does not lose a diagnostic.
        let mut outside = 0;
        let mut inside = 0;
        for (name, text) in ironplc_test::corpus::variants() {
            for options in option_sets() {
                let (tokens, _) = crate::tokenize(&text, &options);
                for index in 0..tokens.len() {
                    let site = Site {
                        tokens: &tokens,
                        index,
                        options: &options,
                    };
                    for gate in GATES {
                        if gate.triggers.contains(&tokens[index].kind) {
                            inside += 1;
                        } else {
                            outside += 1;
                            assert!(
                                (gate.find)(&site).is_empty(),
                                "{name}: `{}` reports at a {:?} token",
                                gate.message,
                                tokens[index].kind
                            );
                        }
                    }
                }
            }
        }
        assert!(outside > 0 && inside > 0);
    }
}
