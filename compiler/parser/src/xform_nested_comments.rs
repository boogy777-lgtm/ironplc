//! Merge the token run a nested `(* ... *)` comment spans into one comment.
//!
//! The logos lexer's comment pattern is a single regex and cannot count
//! nesting, so `(* a (* b *) c *)` lexes as a comment `(* a (* b *)`, an
//! identifier `c` and a stray `*)`. The reference scanner counts depth while
//! `AllowNestedComments` is on (`InternalScanner.ScanComment`), so this
//! transform does the same over the token stream: while a comment token's text
//! leaves an inner `(*` open, the following tokens are folded into it until
//! the depth returns to zero.
//!
//! The merged token is an ordinary [`TokenType::Comment`] whose text and span
//! cover the whole nested comment, so the parser skips it exactly like any
//! other comment. A nested comment that never closes is left as-is: its tokens
//! keep their own types and the parse reports the error it would have
//! reported anyway.
//!
//! Only runs when `options.allow_nested_comments` is set.

use dsl::core::SourceSpan;

use crate::options::CompilerOptions;
use crate::token::{Token, TokenType};

/// Folds the tokens a nested block comment spans into one comment token.
pub fn apply(tokens: Vec<Token>, options: &CompilerOptions) -> Vec<Token> {
    if !options.allow_nested_comments {
        return tokens;
    }

    let mut output = Vec::with_capacity(tokens.len());
    let mut iter = tokens.into_iter().peekable();

    while let Some(tok) = iter.next() {
        if tok.token_type != TokenType::Comment || !tok.text.starts_with("(*") {
            output.push(tok);
            continue;
        }

        if comment_depth(&tok.text) == 0 {
            output.push(tok);
            continue;
        }

        // The comment opened an inner `(*`; consume tokens until every level
        // closes. Depth is counted over the concatenated text because the
        // closing `*)` can be split across a `Star` and a `RightParen`.
        let mut run = vec![tok];
        let mut text = run[0].text.clone();
        let mut closed = false;
        for next in iter.by_ref() {
            text.push_str(&next.text);
            run.push(next);
            if comment_depth(&text) == 0 {
                closed = true;
                break;
            }
        }

        if closed && run.len() > 1 {
            output.push(merge(&run, text));
        } else {
            // Unterminated: hand the tokens back untouched, so the parse
            // reports the error it would have reported without the flag.
            output.extend(run);
        }
    }

    output
}

/// Returns the number of block comments the text leaves open: every `(*`
/// opens one, every `*)` closes the innermost open one.
///
/// Returns 0 for a text with no `(*` at all, so the caller can tell a
/// balanced comment from a nested one.
fn comment_depth(text: &str) -> i32 {
    let bytes = text.as_bytes();
    let mut depth: i32 = 0;
    let mut index = 0;
    while index + 1 < bytes.len() {
        let pair = &bytes[index..index + 2];
        if pair == b"(*" {
            depth += 1;
            index += 2;
        } else if pair == b"*)" {
            depth -= 1;
            index += 2;
        } else {
            index += 1;
        }
    }
    depth.max(0)
}

/// Combines a token run into one comment token covering the whole range.
fn merge(run: &[Token], text: String) -> Token {
    let first = &run[0];
    let last = &run[run.len() - 1];

    Token {
        token_type: TokenType::Comment,
        span: SourceSpan::join(&first.span, &last.span),
        line: first.line,
        col: first.col,
        text,
    }
}

#[cfg(test)]
mod tests {
    use super::apply;
    use crate::options::CompilerOptions;
    use crate::token::TokenType;

    fn opts(allow: bool) -> CompilerOptions {
        CompilerOptions {
            allow_nested_comments: allow,
            ..CompilerOptions::default()
        }
    }

    /// Tokenizes, applies the transform, and returns the comment texts.
    fn comments(source: &str, options: &CompilerOptions) -> Vec<String> {
        let (tokens, diagnostics) =
            crate::lexer::tokenize(source, &dsl::core::FileId::default(), 0, 0);
        assert!(diagnostics.is_empty(), "{diagnostics:?}");
        apply(tokens, options)
            .into_iter()
            .filter(|t| t.token_type == TokenType::Comment)
            .map(|t| t.text)
            .collect()
    }

    #[test]
    fn apply_when_flag_off_then_nested_comment_left_split() {
        let texts = comments("(* a (* b *) c *)", &opts(false));
        assert_eq!(texts, ["(* a (* b *)"]);
    }

    #[test]
    fn apply_when_nested_comment_then_merged_into_one_token() {
        let texts = comments("(* a (* b *) c *)", &opts(true));
        assert_eq!(texts, ["(* a (* b *) c *)"]);
    }

    #[test]
    fn apply_when_deeply_nested_comment_then_merged() {
        let texts = comments("(* a (* b (* c *) d *) e *)", &opts(true));
        assert_eq!(texts, ["(* a (* b (* c *) d *) e *)"]);
    }

    #[test]
    fn apply_when_plain_comment_then_unchanged() {
        let texts = comments("(* plain *)", &opts(true));
        assert_eq!(texts, ["(* plain *)"]);
    }

    #[test]
    fn apply_when_comment_holds_star_then_unchanged() {
        // `*` without a following `)` does not open or close a level.
        let texts = comments("(* a * b *)", &opts(true));
        assert_eq!(texts, ["(* a * b *)"]);
    }

    #[test]
    fn apply_when_two_nested_comments_then_each_merges() {
        let texts = comments("(* a (* b *) *) (* c (* d *) *)", &opts(true));
        assert_eq!(texts, ["(* a (* b *) *)", "(* c (* d *) *)"]);
    }

    #[test]
    fn apply_when_comment_opens_and_never_closes_then_left_as_is() {
        let texts = comments("(* a (* b *) c", &opts(true));
        assert_eq!(texts, ["(* a (* b *)"]);
    }

    #[test]
    fn apply_when_tokens_follow_nested_comment_then_they_are_kept() {
        let (tokens, _) = crate::lexer::tokenize(
            "(* a (* b *) c *) x := 1;",
            &dsl::core::FileId::default(),
            0,
            0,
        );
        let result = apply(tokens, &opts(true));

        let types: Vec<TokenType> = result.iter().map(|t| t.token_type.clone()).collect();
        assert_eq!(
            types.iter().filter(|t| **t == TokenType::Comment).count(),
            1
        );
        assert!(types.contains(&TokenType::Identifier), "{types:?}");
        assert!(types.contains(&TokenType::Assignment), "{types:?}");
    }

    #[test]
    fn apply_when_nested_comment_then_span_covers_the_source() {
        let source = "(* a (* b *) c *)";
        let (tokens, _) = crate::lexer::tokenize(source, &dsl::core::FileId::default(), 0, 0);
        let result = apply(tokens, &opts(true));

        assert_eq!(result.len(), 1);
        assert_eq!(result[0].span.start, 0);
        assert_eq!(result[0].span.end, source.len());
    }
}
