//! The front ends of the parser and the one place that chooses between them.
//!
//! A front end turns text into tokens, a library, or a list of statements.
//! There are two: the legacy pipeline (the logos lexer, the token transforms
//! and the PEG grammar, [`crate::legacy`]) and the one built on the lossless
//! tree of `ironplc_syntax` (the lexer, the regions, the gates, the grammar and
//! the lowering). Each is one row of data, [`LEGACY`] and [`CST`], with the same
//! three entries, and [`SELECTED`] names the row every public function of the
//! crate calls. The `cst-frontend` feature, off by default, is read here and
//! nowhere else in the crate's code: adding a front end is one more row and one
//! more name for the selection, and no public function changes.
//!
//! Both rows are compiled whichever one is selected, so the one that is not
//! selected cannot rot, and a test can run any row. The row that is not
//! selected is never called by the public functions.

use crate::legacy;
use crate::options::CompilerOptions;
use crate::token::Token;
use crate::tokens::project;
use dsl::{core::FileId, diagnostic::Diagnostic};
use ironplc_dsl::common::Library;
use ironplc_dsl::stack::within_stack_budget;
use ironplc_dsl::textual::StmtKind;
use ironplc_syntax::lower::{lower_library, lower_statements};
use ironplc_syntax::{parse_source_file, parse_statements, tokenize, ParseOptions};

/// The entry that tokenizes a text that starts at a line and a column of its
/// document: the tokens, and the diagnostics of the tokenizer.
pub(crate) type TokenizeFn =
    fn(&str, &FileId, &CompilerOptions, usize, usize) -> (Vec<Token>, Vec<Diagnostic>);

/// The entry that reads a statement fragment that starts at a line and a column
/// of its document.
pub(crate) type StatementsFn =
    fn(&str, &FileId, &CompilerOptions, usize, usize) -> Result<Vec<StmtKind>, Diagnostic>;

/// One front end: its three entries, which are those of the
/// public functions of the crate (`tokenize_program`, `parse_program`,
/// `parse_st_statements`) and mean what they mean there.
pub(crate) struct Frontend {
    pub tokenize_program: TokenizeFn,
    pub parse_program: fn(&str, &FileId, &CompilerOptions) -> Result<Library, Diagnostic>,
    pub parse_st_statements: StatementsFn,
}

/// The legacy pipeline.
pub(crate) static LEGACY: Frontend = Frontend {
    tokenize_program: legacy::tokenize_program,
    parse_program: legacy::parse_program,
    parse_st_statements: legacy::parse_st_statements,
};

/// The pipeline built on the lossless tree.
pub(crate) static CST: Frontend = Frontend {
    tokenize_program: cst_tokenize_program,
    parse_program: cst_parse_program,
    parse_st_statements: cst_parse_st_statements,
};

/// Every front end, for the tests that run them all.
#[cfg(test)]
pub(crate) static FRONTENDS: [&Frontend; 2] = [&LEGACY, &CST];

/// The front end the public functions of the crate call: the legacy pipeline,
/// or the one on the lossless tree where the `cst-frontend` feature is on.
pub(crate) static SELECTED: &Frontend = if cfg!(feature = "cst-frontend") {
    &CST
} else {
    &LEGACY
};

/// The dialect flags of the lossless tree for the options of the compiler,
/// read flag by flag by name ([`ParseOptions::FLAG_KEYS`]). A flag the tree
/// reads that the compiler options do not have would read as off; the test
/// `parse_options_when_flag_key_then_the_compiler_options_have_that_flag`
/// fails instead, so a renamed or removed flag is not silently off.
pub(crate) fn parse_options(options: &CompilerOptions) -> ParseOptions {
    let mut parse = ParseOptions::default();
    for key in ParseOptions::FLAG_KEYS {
        parse.set_flag_by_key(key, options.get_flag_by_key(key).unwrap_or(false));
    }
    parse
}

/// The text of a statement fragment as the parser reads it, and where it
/// starts: leading whitespace is skipped and the line and column of the start
/// are advanced over it, trailing whitespace is dropped, and a fragment that is
/// all whitespace has no text (`None`). The offsets of the result are those of
/// the first character of the text.
///
/// Both front ends read a fragment through this, so the bytes they position
/// are the bytes of the trimmed text and not of the fragment as given.
pub(crate) fn statement_fragment(
    source: &str,
    line_offset: usize,
    col_offset: usize,
) -> Option<(&str, usize, usize)> {
    if source.trim().is_empty() {
        return None;
    }

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

    Some((source.get(start_idx..)?.trim_end(), line, col))
}

/// The tokens of the lossless tree, as the token view, and every error of the
/// tokenizer as a diagnostic, in source order. The tokenizer needs no stack
/// budget: it does not recurse on the input.
fn cst_tokenize_program(
    source: &str,
    file_id: &FileId,
    options: &CompilerOptions,
    line_offset: usize,
    col_offset: usize,
) -> (Vec<Token>, Vec<Diagnostic>) {
    let parse_options = parse_options(options);
    let (tokens, errors) = tokenize(source, &parse_options);
    let view = project(
        source,
        &tokens,
        &parse_options,
        file_id,
        line_offset,
        col_offset,
    );
    let diagnostics = errors
        .iter()
        .map(|error| error.to_diagnostic(file_id))
        .collect();
    (view, diagnostics)
}

/// A whole file: parse and lower on the stack budget. The tree is not `Send`,
/// so it is built and dropped inside the budget; only the library, or the one
/// diagnostic, leaves it.
fn cst_parse_program(
    source: &str,
    file_id: &FileId,
    options: &CompilerOptions,
) -> Result<Library, Diagnostic> {
    let parse_options = parse_options(options);
    within_stack_budget(|| {
        let parse = parse_source_file(source, &parse_options);
        lower_library(&parse, file_id)
    })
}

/// A statement fragment, read as `parse_st_statements` of the legacy pipeline
/// reads it. The line and column offsets say where the fragment starts in its
/// document. Only a token carries a line and a column, and a diagnostic names a
/// byte range, so the offsets change nothing that this entry returns: the
/// spans are byte offsets into the trimmed text, as the legacy pipeline gives
/// them.
fn cst_parse_st_statements(
    source: &str,
    file_id: &FileId,
    options: &CompilerOptions,
    line_offset: usize,
    col_offset: usize,
) -> Result<Vec<StmtKind>, Diagnostic> {
    let Some((text, _, _)) = statement_fragment(source, line_offset, col_offset) else {
        return Ok(vec![]);
    };
    let parse_options = parse_options(options);
    within_stack_budget(|| {
        let parse = parse_statements(text, &parse_options);
        lower_statements(&parse, file_id)
    })
}

#[cfg(test)]
mod tests {
    use super::*;
    use spec_test_macro::spec_test;

    #[spec_test(REQ_PT_parser_013)]
    fn parse_options_when_flag_key_then_the_compiler_options_have_that_flag() {
        let options = CompilerOptions::default();
        for key in ParseOptions::FLAG_KEYS {
            assert!(
                options.get_flag_by_key(key).is_some(),
                "the compiler options have no flag {key}"
            );
        }
    }

    #[spec_test(REQ_PT_parser_013)]
    fn parse_options_when_one_flag_on_then_exactly_that_flag_is_on() {
        for key in ParseOptions::FLAG_KEYS {
            let mut options = CompilerOptions::default();
            assert!(options.set_flag_by_key(key, true), "{key}");
            let parse = parse_options(&options);
            for other in ParseOptions::FLAG_KEYS {
                assert_eq!(
                    parse.flag_by_key(other),
                    Some(other == key),
                    "{key}/{other}"
                );
            }
        }
    }

    #[spec_test(REQ_PT_parser_013)]
    fn parse_options_when_dialect_then_the_flags_of_the_dialect() {
        for dialect in crate::options::Dialect::ALL {
            let options = CompilerOptions::from_dialect(*dialect);
            let parse = parse_options(&options);
            for key in ParseOptions::FLAG_KEYS {
                assert_eq!(
                    parse.flag_by_key(key),
                    options.get_flag_by_key(key),
                    "{} {key}",
                    dialect.cli_name()
                );
            }
        }
    }

    #[spec_test(REQ_PT_parser_014)]
    fn selected_when_feature_then_the_row_the_feature_names() {
        let expected = if cfg!(feature = "cst-frontend") {
            &CST
        } else {
            &LEGACY
        };
        assert!(std::ptr::eq(SELECTED, expected));
        assert!(FRONTENDS.iter().any(|row| std::ptr::eq(*row, SELECTED)));
    }

    const SOURCE: &str = "PROGRAM main
VAR x : INT; END_VAR
x := 1;
END_PROGRAM";

    #[test]
    fn parse_program_when_valid_then_the_public_function_answers_as_the_selected_front_end() {
        let options = CompilerOptions::default();
        let id = FileId::default();
        let public = crate::parse_program(SOURCE, &id, &options).unwrap();
        let selected = (SELECTED.parse_program)(SOURCE, &id, &options).unwrap();
        assert_eq!(public, selected);
    }

    #[test]
    fn tokenize_program_when_valid_then_the_public_function_answers_as_the_selected_front_end() {
        let options = CompilerOptions::default();
        let id = FileId::default();
        let public = crate::tokenize_program(SOURCE, &id, &options, 2, 3);
        let selected = (SELECTED.tokenize_program)(SOURCE, &id, &options, 2, 3);
        assert_eq!(public.0.len(), selected.0.len());
        assert_eq!(public.1.len(), selected.1.len());
        let first = public.0.first().map(|token| (token.line, token.col));
        assert_eq!(first, Some((2, 3)));
    }

    #[test]
    fn parse_st_statements_when_valid_then_the_public_function_answers_as_the_selected_front_end() {
        let options = CompilerOptions::default();
        let id = FileId::default();
        let public = crate::parse_st_statements("  x := 1;", &id, &options, 0, 0).unwrap();
        let selected = (SELECTED.parse_st_statements)("  x := 1;", &id, &options, 0, 0).unwrap();
        assert_eq!(public, selected);
    }

    #[test]
    fn statement_fragment_when_all_whitespace_then_none() {
        assert_eq!(statement_fragment("", 3, 4), None);
        assert_eq!(statement_fragment(" \t\r\n ", 3, 4), None);
    }

    #[test]
    fn statement_fragment_when_leading_and_trailing_whitespace_then_trimmed_and_offsets_advanced() {
        assert_eq!(
            statement_fragment("  \n\t x := 1;  \n", 2, 5),
            Some(("x := 1;", 3, 2))
        );
        assert_eq!(statement_fragment("x := 1;", 2, 5), Some(("x := 1;", 2, 5)));
    }

    #[spec_test(REQ_PT_parser_014)]
    fn selected_when_source_of_the_crate_then_the_feature_is_named_in_the_front_end_module_only() {
        // The code of the crate (the modules next to this one; the tests are in
        // `src/tests`) names the feature in one module. A second naming would be a
        // second place that chooses.
        let src = std::path::Path::new(env!("CARGO_MANIFEST_DIR")).join("src");
        let mut naming = Vec::new();
        for entry in std::fs::read_dir(src).unwrap() {
            let path = entry.unwrap().path();
            if path.extension().is_some_and(|ext| ext == "rs") {
                let text = std::fs::read_to_string(&path).unwrap();
                if text.contains("cst-frontend") {
                    naming.push(path.file_name().unwrap().to_string_lossy().to_string());
                }
            }
        }
        assert_eq!(naming, vec!["frontend.rs".to_string()]);
    }

    #[test]
    fn parse_st_statements_when_fragment_is_embedded_then_the_offsets_change_no_span_in_either_front_end(
    ) {
        // The line and column offsets say where the fragment starts in its
        // document. Only a token has a line and a column, so the statements and
        // their spans do not depend on them, and the spans are bytes of the
        // trimmed text: the `1` of `x := 1` is byte 5, not byte 10.
        let options = CompilerOptions::default();
        let id = FileId::default();
        let fragment = "  
	 x := 1;
 IF a THEN b := 2; END_IF;  
";
        let mut by_front_end = Vec::new();
        for row in &FRONTENDS {
            let at_start = (row.parse_st_statements)(fragment, &id, &options, 0, 0).unwrap();
            let embedded = (row.parse_st_statements)(fragment, &id, &options, 7, 11).unwrap();
            assert_eq!(format!("{at_start:?}"), format!("{embedded:?}"));
            let first = format!("{:?}", at_start.first());
            assert!(first.contains("start: 5, end: 6"), "{first}");
            by_front_end.push(format!("{at_start:?}"));
        }
        assert_eq!(by_front_end[0], by_front_end[1]);
    }
}
