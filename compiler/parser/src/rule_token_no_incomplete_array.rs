//! Validation rule: reject the incomplete array type `ARRAY[*] OF T` unless
//! the `allow_incomplete_array` flag is set.
//!
//! IEC 61131-3 declares array bounds as one or more index ranges
//! (`ARRAY [ 1..3 ] OF INT`). CODESYS adds the incomplete form, where `*`
//! stands in for the range list and the caller supplies the bounds; which
//! dialects enable it is defined by the dialect mapping in `options.rs`, not
//! restated here.
//!
//! The grammar accepts `*` unconditionally (see `array_bounds()`), so this
//! token-stream check is what enforces the flag. `ARRAY` is a keyword, so an
//! `ARRAY` token followed (ignoring trivia) by `[` and then `*` is
//! unambiguously the incomplete form: no other construct places `ARRAY` in
//! front of a subscript. The set of tokens skipped here must match the
//! grammar's whitespace rule `_ = (whitespace() / comment() / pragma())*`
//! exactly, or the gate would under-enforce.

use dsl::diagnostic::{Diagnostic, Label};

use crate::{
    options::CompilerOptions,
    token::{Token, TokenType},
};

fn is_trivia(t: &TokenType) -> bool {
    matches!(
        t,
        TokenType::Whitespace | TokenType::Newline | TokenType::Comment | TokenType::Pragma
    )
}

pub fn apply(tokens: &[Token], options: &CompilerOptions) -> Result<(), Vec<Diagnostic>> {
    if options.allow_incomplete_array {
        return Ok(());
    }

    let mut errors: Vec<Diagnostic> = vec![];
    for (i, tok) in tokens.iter().enumerate() {
        if tok.token_type != TokenType::Array {
            continue;
        }
        let mut significant = tokens[i + 1..].iter().filter(|t| !is_trivia(&t.token_type));
        let bracket = significant.next();
        if !matches!(bracket.map(|t| &t.token_type), Some(TokenType::LeftBracket)) {
            continue;
        }
        if let Some(star) = significant.next() {
            if star.token_type == TokenType::Star {
                errors.push(Diagnostic::problem(
                    ironplc_problems::Problem::IncompleteArrayNotAllowed,
                    Label::span(star.span.clone(), "incomplete array bounds"),
                ));
            }
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
        rule_token_no_incomplete_array::apply,
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

    fn incomplete_array_tokens() -> Vec<Token> {
        vec![
            mk_token(TokenType::Array, "ARRAY"),
            mk_token(TokenType::LeftBracket, "["),
            mk_token(TokenType::Star, "*"),
            mk_token(TokenType::RightBracket, "]"),
        ]
    }

    #[test]
    fn apply_when_incomplete_array_and_flag_off_then_error() {
        let options = CompilerOptions::default();
        let result = apply(&incomplete_array_tokens(), &options);
        assert!(result.is_err());
    }

    #[test]
    fn apply_when_incomplete_array_and_flag_on_then_ok() {
        let options = CompilerOptions {
            allow_incomplete_array: true,
            ..CompilerOptions::default()
        };
        let result = apply(&incomplete_array_tokens(), &options);
        assert!(result.is_ok());
    }

    #[test]
    fn apply_when_explicit_ranges_then_ok() {
        let options = CompilerOptions::default();
        let tokens = vec![
            mk_token(TokenType::Array, "ARRAY"),
            mk_token(TokenType::LeftBracket, "["),
            mk_token(TokenType::Digits, "1"),
            mk_token(TokenType::Range, ".."),
            mk_token(TokenType::Digits, "3"),
            mk_token(TokenType::RightBracket, "]"),
        ];
        assert!(apply(&tokens, &options).is_ok());
    }

    #[test]
    fn apply_when_whitespace_between_then_still_error() {
        // The grammar's `_` skips whitespace, comments and pragmas, so the
        // gate must look through the same trivia or it would under-enforce.
        let options = CompilerOptions::default();
        let tokens = vec![
            mk_token(TokenType::Array, "ARRAY"),
            mk_token(TokenType::Whitespace, " "),
            mk_token(TokenType::LeftBracket, "["),
            mk_token(TokenType::Comment, "(* bounds *)"),
            mk_token(TokenType::Star, "*"),
            mk_token(TokenType::RightBracket, "]"),
        ];
        assert!(apply(&tokens, &options).is_err());
    }
}
