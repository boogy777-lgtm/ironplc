// Allow large errors because this is a compiler - we expect large errors.
#![allow(clippy::result_large_err)]
extern crate ironplc_dsl as dsl;

pub mod declarations;
mod frontend;
pub mod options;
mod tokens;

// The legacy pipeline (the logos lexer, the token transforms, the token checks
// and the PEG grammar) is compiled for the tests only: it is the oracle that
// the parity tests of this crate compare the front end against. Nothing in a
// build without `cfg(test)` can name it, so the compiler, not a checklist, says
// that production does not reach it.
#[cfg(test)]
mod legacy;
#[cfg(test)]
mod lexer;
#[cfg(test)]
mod parser;
#[cfg(test)]
mod preprocessor;
#[cfg(test)]
mod rule_no_empty_var_blocks;
#[cfg(test)]
mod rule_token_identifier;
#[cfg(test)]
mod rule_token_no_c_style_comment;
#[cfg(test)]
mod rule_token_no_incomplete_array;
#[cfg(test)]
mod rule_token_no_paren_string_length;
#[cfg(test)]
mod rule_token_no_partial_access_syntax;
#[cfg(test)]
mod rule_token_string_escape;
#[cfg(test)]
mod vars;
#[cfg(test)]
mod xform_assign_file_id;
#[cfg(test)]
mod xform_collapse_pragmas;
#[cfg(test)]
mod xform_demote_keywords;
#[cfg(test)]
mod xform_nested_comments;
#[cfg(test)]
mod xform_pragma_if;
#[cfg(test)]
mod xform_promote_special_operators;
#[cfg(test)]
mod xform_split_duration_units;
#[cfg(test)]
mod xform_statement_labels;
#[cfg(test)]
mod xform_tokens;

use dsl::{core::FileId, diagnostic::Diagnostic};
use ironplc_dsl::common::Library;
use ironplc_dsl::textual::StmtKind;
use options::CompilerOptions;
use token::Token;

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
/// The tokens are the token view of the lossless tree of `ironplc-syntax`: the
/// tokens cover every byte of the text, a keyword that the dialect does not
/// enable is a name, a duration lexeme is one token, a stretch that the grammar
/// does not read (a pragma, a comment) is one token, and a lone carriage return
/// is a line break. The diagnostics are the errors of the tokenizer (an
/// unterminated comment, pragma or string; a pragma in a dialect without
/// pragmas), in source order. Nothing here parses, so the tokenizer runs on
/// the stack of the caller.
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
    frontend::tokenize_program(source, file_id, options, line_offset, col_offset)
}

/// Parse a full IEC 61131 program.
///
/// The parse reads the program into the lossless tree and lowers the tree to
/// the library, and reports the one diagnostic that the ranking of
/// `ironplc-syntax` chooses. It runs on the stack budget
/// (`ironplc_dsl::stack`): a program nests as deep as its text does, up to
/// `MAX_DEPTH` nodes of the tree, and deeper is a diagnostic (P0019).
pub fn parse_program(
    source: &str,
    file_id: &FileId,
    options: &CompilerOptions,
) -> Result<Library, Diagnostic> {
    frontend::parse_program(source, file_id, options)
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
    frontend::parse_st_statements(source, file_id, options, line_offset, col_offset)
}
