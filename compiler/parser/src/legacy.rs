//! The legacy parse pipeline: logos lexer, token transforms, token checks and
//! the PEG grammar.
//!
//! The public functions in the crate root delegate here through a single
//! selection point. This module is crate-private, so it is reachable only from
//! this crate (including its tests) and never becomes public API.

use crate::lexer::tokenize;
use crate::options::CompilerOptions;
use crate::parser::{parse_library, parse_statements};
use crate::preprocessor::preprocess;
use crate::token::Token;
use crate::xform_tokens::insert_keyword_statement_terminators;
use crate::{
    rule_no_empty_var_blocks, rule_token_identifier, rule_token_no_c_style_comment,
    rule_token_no_incomplete_array, rule_token_no_paren_string_length,
    rule_token_no_partial_access_syntax, rule_token_string_escape, xform_assign_file_id,
    xform_collapse_pragmas, xform_demote_keywords, xform_nested_comments, xform_pragma_if,
    xform_promote_special_operators, xform_split_duration_units, xform_statement_labels,
};
use dsl::{core::FileId, diagnostic::Diagnostic};
use ironplc_dsl::common::Library;
use ironplc_dsl::textual::StmtKind;

pub(crate) fn tokenize_program(
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

pub(crate) fn parse_program(
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

pub(crate) fn parse_st_statements(
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

#[cfg(test)]
mod test {
    use super::*;
    use dsl::core::FileId;

    const SOURCE: &str = "PROGRAM main\nVAR x : INT; END_VAR\nx := 1;\nEND_PROGRAM";

    #[test]
    fn parse_program_when_valid_then_facade_matches_legacy() {
        let options = CompilerOptions::default();
        let id = FileId::default();
        let facade = crate::parse_program(SOURCE, &id, &options).unwrap();
        let legacy = parse_program(SOURCE, &id, &options).unwrap();
        assert_eq!(facade, legacy);
    }

    #[test]
    fn tokenize_program_when_valid_then_facade_matches_legacy() {
        let options = CompilerOptions::default();
        let id = FileId::default();
        let facade = crate::tokenize_program(SOURCE, &id, &options, 0, 0);
        let legacy = tokenize_program(SOURCE, &id, &options, 0, 0);
        assert_eq!(facade.0.len(), legacy.0.len());
        assert_eq!(facade.1.len(), legacy.1.len());
    }

    #[test]
    fn parse_st_statements_when_valid_then_facade_matches_legacy() {
        let options = CompilerOptions::default();
        let id = FileId::default();
        let facade = crate::parse_st_statements("  x := 1;", &id, &options, 0, 0).unwrap();
        let legacy = parse_st_statements("  x := 1;", &id, &options, 0, 0).unwrap();
        assert_eq!(facade, legacy);
    }
}
