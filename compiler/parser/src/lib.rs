// Allow large errors because this is a compiler - we expect large errors.
#![allow(clippy::result_large_err)]
extern crate ironplc_dsl as dsl;

pub mod declarations;
mod lexer;
pub mod options;
mod parser;
mod preprocessor;
mod rule_no_empty_var_blocks;
mod rule_token_identifier;
mod rule_token_no_c_style_comment;
mod rule_token_no_incomplete_array;
mod rule_token_no_paren_string_length;
mod rule_token_no_partial_access_syntax;
mod rule_token_string_escape;
mod vars;
mod xform_assign_file_id;
mod xform_collapse_pragmas;
mod xform_demote_keywords;
mod xform_nested_comments;
mod xform_pragma_if;
mod xform_promote_special_operators;
mod xform_split_duration_units;
mod xform_statement_labels;
mod xform_tokens;

use crate::parser::{parse_library, parse_statements};
use dsl::{core::FileId, diagnostic::Diagnostic};
use ironplc_dsl::common::Library;
use ironplc_dsl::textual::StmtKind;
use lexer::tokenize;
use options::CompilerOptions;
use preprocessor::preprocess;
use token::Token;
use xform_tokens::insert_keyword_statement_terminators;

#[cfg(test)]
mod tests;

// Spec conformance testing infrastructure (test-only).
#[cfg(test)]
mod spec_requirements {
    include!(concat!(env!("OUT_DIR"), "/spec_requirements.rs"));
}
#[cfg(test)]
mod spec_conformance;
#[cfg(test)]
mod spec_conformance_adr;
#[cfg(test)]
mod spec_conformance_behavior_policies;
#[cfg(test)]
mod spec_conformance_pointer_to;
#[cfg(test)]
mod spec_conformance_string_literals;
pub mod token;

/// Tokenize a IEC 61131 program.
///
/// Returns a list of tokens and a list of diagnostics. This does not return a result
/// because we usually continue with parsing even if there are token errors because
/// that will give the context of what was wrong in the location with the error.
///
/// # Transform order
///
/// The token transforms run in a fixed order and the order is observable: a
/// later transform sees the token types an earlier one left behind. The order
/// is pinned by the tests in `tests/pipeline_order.rs` (see
/// `specs/design/parse-tree-s0-audit.md`, findings F6 and F7):
///
/// 1. `preprocess` rewrites the text, then the lexer produces tokens.
/// 2. `xform_collapse_pragmas` makes each `{ ... }` one `Pragma` token. This
///    precedes `xform_pragma_if`, which only understands collapsed pragmas.
/// 3. `xform_pragma_if` drops the branches that are not taken, so nothing
///    after it has to know a branch was ever there.
/// 4. `xform_nested_comments` merges a nested comment into one `Comment`
///    token. This precedes terminator insertion, which would otherwise fire on
///    the tokens inside the comment.
/// 5. `xform_split_duration_units` splits duration literals. It still sees
///    `TIME`, `LTIME` and `T` prefixes as the lexer typed them.
/// 6. `insert_keyword_statement_terminators` adds missing `;` tokens. It runs
///    before demotion, so its decisions (`END_TRY` as a terminator,
///    `CONTINUE` as the start of a CASE-branch statement) use the keyword
///    types, whether or not the dialect later demotes those keywords.
/// 7. `xform_statement_labels` marks `name :` in statement position as
///    `Label`. It runs before demotion for the same reason: a keyword that a
///    dialect demotes (`VAR_STAT`, `METHOD`) still opens a declaration for
///    this pass, and a demoted keyword never becomes a label. It runs before
///    promotion, which only touches identifiers.
/// 8. `xform_demote_keywords` turns dialect-gated keywords back into
///    identifiers. Classification is destructive: it depends on the options
///    and, for `TIME`, on the neighbouring tokens, so it cannot be recomputed
///    from the finished stream alone.
/// 9. `xform_promote_special_operators` promotes the `__NEW` family. It is
///    not gated by any dialect flag.
/// 10. The token check rules run over the finished stream.
///
/// Each dialect-gated transform returns its input unchanged when its flag is
/// off, so a dialect only changes which of these steps do any work, never
/// their order.
///
/// The offset parameters allow tokenizing embedded content (like ST body from XML)
/// where the content doesn't start at the beginning of the file:
/// - `line_offset`: The line number (0-based) where this content starts
/// - `col_offset`: The column number (0-based) where this content starts
pub fn tokenize_program(
    source: &str,
    file_id: &FileId,
    options: &CompilerOptions,
    line_offset: usize,
    col_offset: usize,
) -> (Vec<Token>, Vec<Diagnostic>) {
    let source = preprocess(source);
    let (tokens, mut errors) = tokenize(&source, file_id, line_offset, col_offset);

    let tokens = xform_collapse_pragmas::apply(tokens, options);
    // Conditional pragmas drop the branches that are not taken, so they run
    // first: nothing below has to know a branch was ever there.
    let (tokens, mut pragma_errors) = xform_pragma_if::apply(tokens, options);
    let tokens = xform_nested_comments::apply(tokens, options);
    let tokens = xform_split_duration_units::apply(tokens);
    let mut tokens = insert_keyword_statement_terminators(tokens, file_id, options);
    xform_statement_labels::apply(&mut tokens, options);
    xform_demote_keywords::apply(&mut tokens, options);
    xform_promote_special_operators::apply(&mut tokens);
    let result = check_tokens(&tokens, options);
    match result {
        Ok(_) => {}
        Err(mut diagnostics) => errors.append(&mut diagnostics),
    }
    errors.append(&mut pragma_errors);

    (tokens, errors)
}

#[allow(clippy::type_complexity)]
fn check_tokens(tokens: &[Token], options: &CompilerOptions) -> Result<(), Vec<Diagnostic>> {
    let rules: Vec<fn(&[Token], &CompilerOptions) -> Result<(), Vec<Diagnostic>>> = vec![
        rule_token_no_c_style_comment::apply,
        rule_no_empty_var_blocks::apply,
        rule_token_no_partial_access_syntax::apply,
        rule_token_no_paren_string_length::apply,
        rule_token_no_incomplete_array::apply,
        rule_token_identifier::apply,
        rule_token_string_escape::apply,
    ];

    let mut errors = vec![];
    for rule in rules {
        match rule(tokens, options) {
            Ok(_) => {}
            Err(mut diagnostics) => errors.append(&mut diagnostics),
        };
    }

    if !errors.is_empty() {
        return Err(errors);
    }

    Ok(())
}

/// Parse a full IEC 61131 program.
pub fn parse_program(
    source: &str,
    file_id: &FileId,
    options: &CompilerOptions,
) -> Result<Library, Diagnostic> {
    let mut result = tokenize_program(source, file_id, options, 0, 0);
    if !result.1.is_empty() {
        return Err(result.1.remove(0));
    }

    let library = parse_library(result.0).map(|elements| Library { elements })?;

    // The parser does not know how to assign the file identifier, so transform the input as
    // a post-processing step.
    xform_assign_file_id::apply(library, file_id)
}

/// Parse ST (Structured Text) body content into statements.
///
/// This is useful for parsing ST body content from PLCopen XML files
/// where only the statements (not the full POU declaration) are provided.
///
/// The offset parameters allow parsing embedded content where the content
/// doesn't start at the beginning of the file:
/// - `line_offset`: The line number (0-based) where this content starts
/// - `col_offset`: The column number (0-based) where this content starts
///
/// Leading and trailing whitespace is automatically trimmed, with the offset
/// adjusted accordingly to maintain accurate source positions.
pub fn parse_st_statements(
    source: &str,
    file_id: &FileId,
    options: &CompilerOptions,
    line_offset: usize,
    col_offset: usize,
) -> Result<Vec<StmtKind>, Diagnostic> {
    if source.trim().is_empty() {
        return Ok(vec![]);
    }

    // Calculate adjusted offset after skipping leading whitespace
    let (trimmed_source, adjusted_line, adjusted_col) =
        skip_leading_whitespace(source, line_offset, col_offset);

    let mut result = tokenize_program(
        trimmed_source,
        file_id,
        options,
        adjusted_line,
        adjusted_col,
    );
    if !result.1.is_empty() {
        return Err(result.1.remove(0));
    }

    parse_statements(result.0)
}

/// Skip leading whitespace and calculate the adjusted line/column offset.
///
/// Returns (trimmed_source, adjusted_line_offset, adjusted_col_offset).
fn skip_leading_whitespace(
    source: &str,
    line_offset: usize,
    col_offset: usize,
) -> (&str, usize, usize) {
    let mut line = line_offset;
    let mut col = col_offset;
    let mut start_idx = 0;

    for (idx, ch) in source.char_indices() {
        match ch {
            '\n' => {
                line += 1;
                col = 0;
                start_idx = idx + 1;
            }
            ' ' | '\t' | '\r' => {
                col += 1;
                start_idx = idx + 1;
            }
            _ => break,
        }
    }

    (source[start_idx..].trim_end(), line, col)
}
