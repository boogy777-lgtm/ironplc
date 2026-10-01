//! The token-transform order in `tokenize_program` is observable.
//!
//! Each test builds the smallest source whose token stream differs if two
//! adjacent transforms were swapped, or if the dialect gate that guards one of
//! them changed. They document the order recorded next to the pipeline in
//! `lib.rs` (see `specs/design/parse-tree-s0-audit.md`, findings F6 and F7).

use super::common::*;
use crate::token::TokenType;

/// The tokens of `source` after the whole pipeline, as `(type, text)` pairs
/// with whitespace, newlines and comments dropped. Inserted terminators have
/// empty text.
fn stream(source: &str, options: &CompilerOptions) -> Vec<(TokenType, String)> {
    let (tokens, _) = crate::tokenize_program(source, &FileId::default(), options, 0, 0);
    tokens
        .into_iter()
        .filter(|t| {
            !matches!(
                t.token_type,
                TokenType::Whitespace | TokenType::Newline | TokenType::Comment
            )
        })
        .map(|t| (t.token_type, t.text))
        .collect()
}

fn types(source: &str, options: &CompilerOptions) -> Vec<TokenType> {
    stream(source, options)
        .into_iter()
        .map(|(token_type, _)| token_type)
        .collect()
}

/// The number of terminators the pipeline inserted (empty-text semicolons).
fn inserted_terminators(source: &str, options: &CompilerOptions) -> usize {
    stream(source, options)
        .iter()
        .filter(|(token_type, text)| *token_type == TokenType::Semicolon && text.is_empty())
        .count()
}

fn missing_semicolon() -> CompilerOptions {
    CompilerOptions {
        allow_missing_semicolon: true,
        ..CompilerOptions::default()
    }
}

#[test]
fn tokenize_program_when_endtry_demoted_then_terminator_still_inserted() {
    // `__ENDTRY` is a terminator keyword only while it is still `EndTry`.
    // Terminators are inserted before demotion, so the keyword that is then
    // demoted to an identifier still gets its semicolon.
    let source = "__ENDTRY\nEND_PROGRAM";
    let demoted = missing_semicolon();
    assert_eq!(
        types(source, &demoted),
        [
            TokenType::Identifier,
            TokenType::Semicolon,
            TokenType::EndProgram
        ]
    );

    let kept = CompilerOptions {
        allow_try_catch: true,
        ..missing_semicolon()
    };
    assert_eq!(
        types(source, &kept),
        [
            TokenType::EndTry,
            TokenType::Semicolon,
            TokenType::EndProgram
        ]
    );
}

#[test]
fn tokenize_program_when_continue_demoted_then_case_branch_still_sees_statement() {
    // `CONTINUE` starts a statement for the CASE-branch fixup because that
    // fixup sees the token before demotion. Were demotion first, the branch
    // would look empty and get a `;` in front of the identifier.
    let source = "CASE x OF 1: CONTINUE END_CASE END_PROGRAM";
    let demoted = types(source, &missing_semicolon());
    assert_eq!(
        demoted,
        [
            TokenType::Case,
            TokenType::Identifier,
            TokenType::Of,
            TokenType::Digits,
            TokenType::Colon,
            TokenType::Identifier,
            TokenType::EndCase,
            TokenType::Semicolon,
            TokenType::EndProgram,
        ]
    );

    let kept = CompilerOptions {
        allow_continue: true,
        ..missing_semicolon()
    };
    let kept = types(source, &kept);
    assert_eq!(kept[5], TokenType::Continue);
    assert_eq!(kept.len(), demoted.len());
}

#[test]
fn tokenize_program_when_flag_off_then_no_terminators_inserted() {
    // The terminator transform is gated by `allow_missing_semicolon` alone.
    let source = "__ENDTRY\nEND_PROGRAM";
    assert_eq!(inserted_terminators(source, &CompilerOptions::default()), 0);
    assert_eq!(inserted_terminators(source, &missing_semicolon()), 1);
}

#[test]
fn tokenize_program_when_label_named_like_demoted_keyword_then_not_a_label() {
    // Labels are marked before demotion: `continue` is still a keyword token
    // when the label pass looks for `Identifier` `:`, and is demoted to a
    // plain identifier afterwards, so it never becomes a label.
    let source = "continue: x := 1;";
    let demoted = CompilerOptions {
        allow_jump_statement: true,
        ..CompilerOptions::default()
    };
    assert_eq!(types(source, &demoted)[0], TokenType::Identifier);

    let kept = CompilerOptions {
        allow_continue: true,
        ..demoted
    };
    assert_eq!(types(source, &kept)[0], TokenType::Continue);

    // An ordinary name is a label.
    assert_eq!(types("here: x := 1;", &demoted)[0], TokenType::Label);
}

#[test]
fn tokenize_program_when_jump_flag_off_then_statement_label_not_marked() {
    assert_eq!(
        types("here: x := 1;", &CompilerOptions::default())[0],
        TokenType::Identifier
    );
}

#[test]
fn tokenize_program_when_var_stat_demoted_then_declaration_not_a_label() {
    // `VAR_STAT` still opens a declaration region when labels are marked, even
    // though demotion then turns it into an identifier. The declared name
    // therefore stays an identifier; with demotion first it would be a label.
    let source = "VAR_STAT x : INT; END_VAR";
    let options = CompilerOptions {
        allow_jump_statement: true,
        ..CompilerOptions::default()
    };
    assert_eq!(
        types(source, &options),
        [
            TokenType::Identifier,
            TokenType::Identifier,
            TokenType::Colon,
            TokenType::Int,
            TokenType::Semicolon,
            TokenType::EndVar,
        ]
    );
}

#[test]
fn tokenize_program_when_method_demoted_then_method_name_not_a_label() {
    // `METHOD` introduces a declared name while labels are marked, then is
    // demoted when inheritance syntax is off.
    let source = "METHOD m : INT";
    let options = CompilerOptions {
        allow_jump_statement: true,
        ..CompilerOptions::default()
    };
    assert_eq!(
        types(source, &options),
        [
            TokenType::Identifier,
            TokenType::Identifier,
            TokenType::Colon,
            TokenType::Int,
        ]
    );
}

#[test]
fn tokenize_program_when_label_named_like_special_operator_then_label_wins() {
    // Labels are marked before special operators are promoted, and promotion
    // only touches identifiers, so a label spelled `__new` stays a label.
    let options = CompilerOptions {
        allow_jump_statement: true,
        ..CompilerOptions::default()
    };
    assert_eq!(types("__new: x := 1;", &options)[0], TokenType::Label);
    // Promotion is not gated by any flag.
    assert_eq!(
        types("__new(x)", &CompilerOptions::default())[0],
        TokenType::SpecialNew
    );
}

#[test]
fn tokenize_program_when_time_neighbours_vary_then_demoted_only_in_function_contexts() {
    let options = CompilerOptions {
        allow_time_as_function_name: true,
        ..CompilerOptions::default()
    };
    // Followed by `(`: a call.
    assert_eq!(types("TIME()", &options)[0], TokenType::Identifier);
    // Whitespace and newlines between the neighbours are skipped.
    assert_eq!(types("TIME \n ()", &options)[0], TokenType::Identifier);
    // Preceded by FUNCTION: the declaration.
    assert_eq!(
        types("FUNCTION TIME : TIME", &options)[1],
        TokenType::Identifier
    );
    // ... and the type position after the colon is not.
    assert_eq!(types("FUNCTION TIME : TIME", &options)[3], TokenType::Time);
    // Followed by `:=`: the return-value assignment.
    assert_eq!(types("TIME := 1;", &options)[0], TokenType::Identifier);
    // Type name and duration prefix keep the keyword.
    assert_eq!(types("x : TIME;", &options)[2], TokenType::Time);
    assert_eq!(types("TIME#5s", &options)[0], TokenType::Time);
}

#[test]
fn tokenize_program_when_time_gate_off_then_never_demoted() {
    // The gate has inverted polarity: `TIME` is demoted only when
    // `allow_time_as_function_name` is set.
    let options = CompilerOptions::default();
    assert_eq!(types("TIME()", &options)[0], TokenType::Time);
    assert_eq!(types("FUNCTION TIME", &options)[1], TokenType::Time);
    assert_eq!(types("TIME := 1;", &options)[0], TokenType::Time);
}

#[test]
fn tokenize_program_when_comment_between_time_and_paren_then_not_demoted() {
    // Neighbour lookup skips whitespace and newlines but not comments.
    let options = CompilerOptions {
        allow_time_as_function_name: true,
        ..CompilerOptions::default()
    };
    assert_eq!(types("TIME (* c *) ()", &options)[0], TokenType::Time);
}

#[test]
fn tokenize_program_when_ltime_demoted_then_duration_still_split() {
    // The duration split sees `LTIME` before demotion, so the literal's unit
    // suffixes are split even though the prefix becomes an identifier.
    let source = "LTIME#1m30s";
    let demoted = types(source, &CompilerOptions::default());
    assert_eq!(demoted[0], TokenType::Identifier);
    assert_eq!(demoted.len(), 6, "{demoted:?}");

    let kept = CompilerOptions {
        allow_long_time_types: true,
        ..CompilerOptions::default()
    };
    let kept = types(source, &kept);
    assert_eq!(kept[0], TokenType::Ltime);
    assert_eq!(kept.len(), 6, "{kept:?}");
}

#[test]
fn tokenize_program_when_nested_comment_after_end_if_then_terminator_after_comment() {
    // Nested comments are merged before terminators are inserted, so the
    // whole comment is skipped like any comment and the terminator lands on
    // the line break after it. Were the terminator pass first, it would fire
    // on the stray `c` inside the comment, the merge would then swallow that
    // synthetic token, and no terminator would remain.
    let source = "END_IF (* a (* b *) c *)
END_PROGRAM";
    let nested = CompilerOptions {
        allow_nested_comments: true,
        ..missing_semicolon()
    };
    assert_eq!(
        types(source, &nested),
        [
            TokenType::EndIf,
            TokenType::Semicolon,
            TokenType::EndProgram
        ]
    );
    assert_eq!(inserted_terminators(source, &nested), 1);

    // Without the nested-comment gate the comment ends early and its tail
    // reaches the grammar as tokens.
    let flat = stream(source, &missing_semicolon());
    assert!(flat.iter().any(|(_, text)| text == "c"), "{flat:?}");
}

#[test]
fn tokenize_program_when_pragma_if_drops_branch_then_terminator_decision_sees_remainder() {
    // Pragmas are collapsed, then conditionals evaluated, before terminators
    // are inserted. The `{IF}` directives are dropped, so the explicit `;`
    // after them terminates `END_IF` and nothing is inserted.
    let source = "END_IF {IF FALSE} {END_IF};\nEND_PROGRAM";
    let options = CompilerOptions {
        allow_pragmas: true,
        allow_pragma_if: true,
        ..missing_semicolon()
    };
    assert_eq!(inserted_terminators(source, &options), 0);

    // Without the conditional gate the directives are ordinary pragma tokens
    // standing between `END_IF` and the `;`, so a terminator is inserted.
    let no_if = CompilerOptions {
        allow_pragma_if: false,
        ..options
    };
    assert_eq!(inserted_terminators(source, &no_if), 1);
}

#[test]
fn tokenize_program_when_pragma_if_without_pragmas_then_branches_both_kept() {
    // `{IF}` evaluation works on collapsed pragma tokens, so it needs
    // `allow_pragmas` as well; alone it has nothing to evaluate.
    let source = "a {IF FALSE} b {END_IF} c";
    let only_if = CompilerOptions {
        allow_pragma_if: true,
        ..CompilerOptions::default()
    };
    assert!(stream(source, &only_if).iter().any(|(_, text)| text == "b"));

    let both = CompilerOptions {
        allow_pragmas: true,
        allow_pragma_if: true,
        ..CompilerOptions::default()
    };
    let kept = stream(source, &both);
    assert!(kept.iter().all(|(_, text)| text != "b"), "{kept:?}");
}
