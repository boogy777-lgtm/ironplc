use dsl::core::{FileId, SourceSpan};

use crate::options::CompilerOptions;
use crate::token::{Token, TokenType};

/// Creates a synthetic `;` anchored immediately before `next`.
///
/// The marker is zero-width (`start == end` at the start of `next`) with empty
/// text, so it owns no source bytes: it sits between the real token that ends
/// where `next` begins and `next` itself, and never shares a span with a real
/// token. Line and column are those of the anchor point.
fn zero_width_semicolon_before(next: &Token) -> Token {
    Token {
        token_type: TokenType::Semicolon,
        span: SourceSpan {
            start: next.span.start,
            end: next.span.start,
            file_id: next.span.file_id.clone(),
        },
        line: next.line,
        col: next.col,
        text: "".to_owned(),
    }
}

/// Adds a semicolon after keyword statements to terminate the statement, and
/// fills in a missing statement for a completely empty `CASE` branch.
///
/// IEC 61131-3 requires a semicolon after each statement but many programs
/// do not have a semicolon after named keywords. This function inserts the
/// semicolon token after keyword statements (when the semicolon does not
/// exist) so that the token stream is valid.
///
/// It also handles a `CASE` branch with no statements at all (a label that
/// falls straight through to the next label, `ELSE`, or `END_CASE` --
/// confirmed against real TwinCAT output). Strict IEC 61131-3 only allows
/// this via an explicit empty statement (`5: ;`); this inserts that `;`
/// when a branch is otherwise completely empty, turning `5:` into the
/// already-legal `5: ;` before the grammar ever sees it.
///
/// Both fixups are only applied when `options.allow_missing_semicolon` is set.
pub fn insert_keyword_statement_terminators(
    input: Vec<Token>,
    _file_id: &FileId,
    options: &CompilerOptions,
) -> Vec<Token> {
    if !options.allow_missing_semicolon {
        return input;
    }

    let mut output = Vec::new();

    let mut in_end_statement = false;

    // Tracks CASE...END_CASE nesting and, within it, whether we're still
    // waiting for the first statement of the current branch (right after a
    // case label's `:`, or after `ELSE`). While waiting, tokens that could
    // still be part of either an empty branch's case list (the next label)
    // or a real statement -- digits, identifiers, `,`, `..`, `#`, sign --
    // are buffered rather than emitted immediately, since we can't tell
    // them apart until we see whichever comes first: an unambiguous
    // statement token (`:=`, `(`, `.`, `[`, `^`, or a statement keyword) or
    // the next branch terminator (another `:`, `ELSE`, `END_CASE`).
    // Reaching a terminator while still waiting means the branch was empty.
    let mut case_depth: u32 = 0;
    let mut awaiting_case_branch_statement = false;
    let mut case_branch_buffer: Vec<Token> = Vec::new();

    for tok in input {
        match tok.token_type {
            TokenType::Case => {
                if awaiting_case_branch_statement {
                    output.append(&mut case_branch_buffer);
                    awaiting_case_branch_statement = false;
                }
                case_depth += 1;
                output.push(tok);
                continue;
            }
            TokenType::Colon if case_depth > 0 => {
                if awaiting_case_branch_statement {
                    // The empty branch sits right after the previous `:`, so
                    // anchor at the first buffered token (or here if none).
                    output.push(zero_width_semicolon_before(
                        case_branch_buffer.first().unwrap_or(&tok),
                    ));
                    output.append(&mut case_branch_buffer);
                }
                awaiting_case_branch_statement = true;
                output.push(tok);
                continue;
            }
            TokenType::EndCase if case_depth > 0 => {
                if awaiting_case_branch_statement {
                    output.push(zero_width_semicolon_before(
                        case_branch_buffer.first().unwrap_or(&tok),
                    ));
                    output.append(&mut case_branch_buffer);
                    awaiting_case_branch_statement = false;
                }
                case_depth -= 1;
            }
            TokenType::Else if awaiting_case_branch_statement => {
                output.push(zero_width_semicolon_before(
                    case_branch_buffer.first().unwrap_or(&tok),
                ));
                output.append(&mut case_branch_buffer);
                awaiting_case_branch_statement = false;
            }
            TokenType::Semicolon if awaiting_case_branch_statement => {
                // The branch already has an explicit empty statement
                // (`5: ;`) -- nothing to insert, just stop waiting.
                output.append(&mut case_branch_buffer);
                awaiting_case_branch_statement = false;
            }
            TokenType::Assignment
            | TokenType::LeftParen
            | TokenType::LeftBracket
            | TokenType::Caret
            | TokenType::Period
            | TokenType::If
            | TokenType::For
            | TokenType::While
            | TokenType::Repeat
            | TokenType::Return
            | TokenType::Exit
            | TokenType::Continue
                if awaiting_case_branch_statement =>
            {
                // Unambiguous start of a real statement.
                output.append(&mut case_branch_buffer);
                awaiting_case_branch_statement = false;
            }
            _ if awaiting_case_branch_statement => {
                case_branch_buffer.push(tok);
                continue;
            }
            _ => {}
        }

        // Insert a semicolon after a keyword statement terminator
        // (END_IF, END_CASE, ...) when the source omitted it.
        if !in_end_statement
            && matches!(
                tok.token_type,
                TokenType::EndIf
                    | TokenType::EndStruct
                    | TokenType::EndWhile
                    | TokenType::EndFor
                    | TokenType::EndCase
                    | TokenType::EndRepeat
                    | TokenType::EndTry
            )
        {
            in_end_statement = true;
        } else if in_end_statement && tok.token_type == TokenType::Semicolon {
            // The source already has the semicolon — no insertion needed.
            in_end_statement = false;
        } else if in_end_statement
            && tok.token_type != TokenType::Comment
            && tok.token_type != TokenType::DocComment
            && tok.token_type != TokenType::Whitespace
        {
            output.push(zero_width_semicolon_before(&tok));
            in_end_statement = false;
        }

        output.push(tok);
    }

    output
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::tokenize_program;

    fn tokens_for(source: &str) -> Vec<Token> {
        let options = CompilerOptions {
            allow_missing_semicolon: true,
            ..CompilerOptions::default()
        };
        let (tokens, _) = tokenize_program(source, &FileId::default(), &options, 0, 0);
        tokens
    }

    /// Synthetic terminators are the only `;` tokens with empty text.
    fn synthetic_terminators(tokens: &[Token]) -> Vec<&Token> {
        tokens
            .iter()
            .filter(|t| t.token_type == TokenType::Semicolon && t.text.is_empty())
            .collect()
    }

    fn assert_non_decreasing_spans(tokens: &[Token]) {
        for pair in tokens.windows(2) {
            assert!(
                pair[0].span.start <= pair[1].span.start && pair[0].span.end <= pair[1].span.end,
                "tokens out of span order: {:?} then {:?}",
                pair[0],
                pair[1]
            );
        }
    }

    const END_IF_SOURCE: &str = "PROGRAM p\nIF TRUE THEN\nx := 1;\nEND_IF\nx := 2;\nEND_PROGRAM\n";
    const EMPTY_BRANCH_SOURCE: &str =
        "PROGRAM p\nCASE x OF\n1:\n2: y := 1;\nELSE\nEND_CASE;\nEND_PROGRAM\n";
    const CASE_ELSE_SOURCE: &str =
        "PROGRAM p\nCASE x OF\n1: y := 1;\n2:\nELSE\nEND_CASE\nEND_PROGRAM\n";
    const LABEL_LIST_SOURCE: &str =
        "PROGRAM p\nCASE x OF\n1,\n2:\n3..5:\n6: y := 1;\nEND_CASE;\nEND_PROGRAM\n";

    #[test]
    fn insert_keyword_statement_terminators_when_end_if_unterminated_then_marker_is_zero_width_and_empty(
    ) {
        let tokens = tokens_for(END_IF_SOURCE);
        let markers = synthetic_terminators(&tokens);
        assert_eq!(markers.len(), 1);
        // The terminator lands right after `END_IF`, before the newline token.
        let anchor = END_IF_SOURCE.find("END_IF").unwrap() + "END_IF".len();
        assert_eq!(markers[0].span.start, anchor);
        assert_eq!(markers[0].span.end, anchor);
        assert_eq!(markers[0].text, "");
    }

    #[test]
    fn insert_keyword_statement_terminators_when_empty_case_branches_then_markers_are_zero_width_and_empty(
    ) {
        for source in [EMPTY_BRANCH_SOURCE, CASE_ELSE_SOURCE, LABEL_LIST_SOURCE] {
            let tokens = tokens_for(source);
            let markers = synthetic_terminators(&tokens);
            assert!(!markers.is_empty(), "no marker inserted for {source:?}");
            for marker in markers {
                assert_eq!(marker.span.start, marker.span.end);
                assert_eq!(marker.text, "");
            }
        }
    }

    #[test]
    fn insert_keyword_statement_terminators_when_empty_case_branch_then_marker_anchored_after_colon(
    ) {
        let tokens = tokens_for(EMPTY_BRANCH_SOURCE);
        let markers = synthetic_terminators(&tokens);
        // `1:` ends just before the newline that precedes `2:`.
        let anchor = EMPTY_BRANCH_SOURCE.find("1:").unwrap() + 2;
        assert_eq!(markers[0].span.start, anchor);
        assert_eq!(markers[0].span.end, anchor);
    }

    #[test]
    fn insert_keyword_statement_terminators_when_any_fixup_then_spans_non_decreasing() {
        for source in [
            END_IF_SOURCE,
            EMPTY_BRANCH_SOURCE,
            CASE_ELSE_SOURCE,
            LABEL_LIST_SOURCE,
        ] {
            assert_non_decreasing_spans(&tokens_for(source));
        }
    }

    #[test]
    fn insert_keyword_statement_terminators_when_end_if_then_marker_does_not_share_span_with_real_token(
    ) {
        let tokens = tokens_for(END_IF_SOURCE);
        for marker in synthetic_terminators(&tokens) {
            for real in tokens.iter().filter(|t| !t.text.is_empty()) {
                assert!(
                    !(marker.span.start > real.span.start && marker.span.start < real.span.end),
                    "marker strictly inside real token {real:?}"
                );
            }
        }
    }
}
