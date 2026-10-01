// Allow large errors because this is a compiler - we expect large errors.
#![allow(clippy::result_large_err)]
extern crate ironplc_dsl as dsl;

pub mod declarations;
mod legacy;
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
    legacy::tokenize_program(source, file_id, options, line_offset, col_offset)
}

/// Parse a full IEC 61131 program.
pub fn parse_program(
    source: &str,
    file_id: &FileId,
    options: &CompilerOptions,
) -> Result<Library, Diagnostic> {
    legacy::parse_program(source, file_id, options)
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
    legacy::parse_st_statements(source, file_id, options, line_offset, col_offset)
}
