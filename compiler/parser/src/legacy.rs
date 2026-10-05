//! The legacy parse pipeline: logos lexer, token transforms, token checks and
//! the PEG grammar.
//!
//! The public functions in the crate root reach it through `frontend`, the one
//! place that selects a front end. This module is crate-private, so it is
//! reachable only from this crate (including its tests) and never becomes
//! public API.

use crate::frontend::statement_fragment;
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
    let Some((trimmed_source, adjusted_line, adjusted_col)) =
        statement_fragment(source, line_offset, col_offset)
    else {
        return Ok(vec![]);
    };

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
