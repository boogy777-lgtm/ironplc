//! Validation rules for identifier spellings: escaped identifiers, letters
//! outside ASCII, and consecutive underscores.
//!
//! The lexer accepts all three spellings in every dialect; each is gated by
//! its own flag here, following the recognize-then-gate pattern the
//! partial-access syntax uses. Which dialects enable which flag is defined by
//! the dialect mapping in `options.rs`, not restated here.
//!
//! * **Escaped identifiers** (`` `my name` ``) — CODESYS "non-compliant
//!   identifiers", off unless `allow_escaped_identifiers`.
//! * **Unicode identifiers** (letters outside ASCII) — a CODESYS scanner
//!   option that is off there by default too, so it stays opt-in
//!   (`allow_unicode_identifiers`).
//! * **Consecutive underscores** — CODESYS/TwinCAT reject them
//!   (`AllowMultipleUnderlines=false`), so they need
//!   `allow_multiple_underscores`. The leading `__` is exempt: it is the
//!   compiler's own reserved namespace (`__SYSTEM_UP_TIME`, `__TRUNC`,
//!   `__NEW`), which every dialect relies on.
//!
//! When an escaped identifier is rejected the token is reported once: the
//! other two checks would report the same span again for the same spelling
//! decision. When it is accepted, the other two checks do not run on it at
//! all: inside the escape every character is legal, which is the whole point
//! of the spelling (the reference scanner likewise skips its underline check
//! while inside an escape).

use dsl::diagnostic::{Diagnostic, Label};

use crate::{
    options::CompilerOptions,
    token::{Token, TokenType},
};

/// True when `text` contains `__` anywhere except in the reserved leading
/// `__` prefix.
///
/// The prefix is examined only once, so `__x__y` (a reserved-prefix name that
/// also contains a later pair) is still rejected, while `___x` is accepted:
/// everything from the first two underscores onward belongs to the reserved
/// namespace.
fn has_inner_double_underscore(text: &str) -> bool {
    match text.find("__") {
        Some(0) => text[2..].contains("__"),
        Some(_) => true,
        None => false,
    }
}

fn check_identifier_spelling(tok: &Token, options: &CompilerOptions, errors: &mut Vec<Diagnostic>) {
    if !options.allow_unicode_identifiers && !tok.text.is_ascii() {
        errors.push(Diagnostic::problem(
            ironplc_problems::Problem::UnicodeIdentifierNotAllowed,
            Label::span(tok.span.clone(), "identifier"),
        ));
    }
    if !options.allow_multiple_underscores && has_inner_double_underscore(&tok.text) {
        errors.push(Diagnostic::problem(
            ironplc_problems::Problem::MultipleUnderscoresNotAllowed,
            Label::span(tok.span.clone(), "identifier"),
        ));
    }
}

pub fn apply(tokens: &[Token], options: &CompilerOptions) -> Result<(), Vec<Diagnostic>> {
    let mut errors: Vec<Diagnostic> = vec![];
    for tok in tokens {
        match tok.token_type {
            TokenType::EscapedIdentifier => {
                if !options.allow_escaped_identifiers {
                    errors.push(Diagnostic::problem(
                        ironplc_problems::Problem::EscapedIdentifierNotAllowed,
                        Label::span(tok.span.clone(), "escaped identifier"),
                    ));
                }
                // Nothing further: an accepted escape admits any character.
            }
            TokenType::Identifier => check_identifier_spelling(tok, options, &mut errors),
            _ => {}
        }
    }

    if errors.is_empty() {
        Ok(())
    } else {
        Err(errors)
    }
}

#[cfg(test)]
mod test {
    use dsl::core::SourceSpan;

    use crate::{
        options::CompilerOptions,
        rule_token_identifier::apply,
        token::{Token, TokenType},
    };

    fn mk_token(token_type: TokenType, text: &str) -> Token {
        Token {
            token_type,
            span: SourceSpan::default(),
            line: 1,
            col: 1,
            text: text.to_string(),
        }
    }

    fn opts() -> CompilerOptions {
        CompilerOptions::default()
    }

    #[test]
    fn apply_when_escaped_identifier_and_flag_off_then_error() {
        let tokens = vec![mk_token(TokenType::EscapedIdentifier, "`my var`")];
        assert!(apply(&tokens, &opts()).is_err());
    }

    #[test]
    fn apply_when_escaped_identifier_and_flag_on_then_ok() {
        let options = CompilerOptions {
            allow_escaped_identifiers: true,
            ..CompilerOptions::default()
        };
        let tokens = vec![mk_token(TokenType::EscapedIdentifier, "`my var`")];
        assert!(apply(&tokens, &options).is_ok());
    }

    #[test]
    fn apply_when_unicode_identifier_and_flag_off_then_error() {
        let tokens = vec![mk_token(TokenType::Identifier, "caf\u{e9}")];
        assert!(apply(&tokens, &opts()).is_err());
    }

    #[test]
    fn apply_when_unicode_identifier_and_flag_on_then_ok() {
        let options = CompilerOptions {
            allow_unicode_identifiers: true,
            ..CompilerOptions::default()
        };
        let tokens = vec![mk_token(TokenType::Identifier, "caf\u{e9}")];
        assert!(apply(&tokens, &options).is_ok());
    }

    #[test]
    fn apply_when_ascii_identifier_then_ok() {
        let tokens = vec![mk_token(TokenType::Identifier, "plain_name")];
        assert!(apply(&tokens, &opts()).is_ok());
    }

    #[test]
    fn apply_when_inner_double_underscore_and_flag_off_then_error() {
        let tokens = vec![mk_token(TokenType::Identifier, "my__var")];
        assert!(apply(&tokens, &opts()).is_err());
    }

    #[test]
    fn apply_when_inner_double_underscore_and_flag_on_then_ok() {
        let options = CompilerOptions {
            allow_multiple_underscores: true,
            ..CompilerOptions::default()
        };
        let tokens = vec![mk_token(TokenType::Identifier, "my__var")];
        assert!(apply(&tokens, &options).is_ok());
    }

    #[test]
    fn apply_when_reserved_prefix_then_ok() {
        // The compiler's own namespace is always available: seeding the
        // implicit `__SYSTEM_UP_TIME` global and calling the `__TRUNC`
        // intrinsic must not require an option.
        let tokens = vec![
            mk_token(TokenType::Identifier, "__SYSTEM_UP_TIME"),
            mk_token(TokenType::Identifier, "__TRUNC"),
        ];
        assert!(apply(&tokens, &opts()).is_ok());
    }

    #[test]
    fn apply_when_reserved_prefix_with_later_pair_then_error() {
        let tokens = vec![mk_token(TokenType::Identifier, "__x__y")];
        assert!(apply(&tokens, &opts()).is_err());
    }
}
