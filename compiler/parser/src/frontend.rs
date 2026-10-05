//! The front end of the parser: text to tokens, a library, or a list of
//! statements, through the lossless tree of `ironplc_syntax` (the lexer, the
//! regions, the gates, the grammar and the lowering).
//!
//! The public functions of the crate call the three entries of this module and
//! nothing else reads text. There is one front end in production, so there is
//! nothing to select; the legacy pipeline (`legacy`) is compiled for
//! the tests only, as the oracle of the parity tests.

use crate::options::CompilerOptions;
use crate::token::Token;
use crate::tokens::project;
use dsl::{core::FileId, diagnostic::Diagnostic};
use ironplc_dsl::common::Library;
use ironplc_dsl::stack::within_stack_budget;
use ironplc_dsl::textual::StmtKind;
use ironplc_syntax::lower::{lower_library, lower_statements};
use ironplc_syntax::{parse_source_file, parse_statements, tokenize, ParseOptions};

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
pub(crate) fn tokenize_program(
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
pub(crate) fn parse_program(
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

/// A statement fragment. The line and column offsets say where the fragment starts in its
/// document. Only a token carries a line and a column, and a diagnostic names a
/// byte range, so the offsets change nothing that this entry returns: the
/// spans are byte offsets into the trimmed text, as the legacy pipeline gave
/// them.
pub(crate) fn parse_st_statements(
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

    const SOURCE: &str = "PROGRAM main
VAR x : INT; END_VAR
x := 1;
END_PROGRAM";

    #[spec_test(REQ_PT_parser_014)]
    fn parse_program_when_valid_then_the_public_function_answers_with_the_lowered_tree() {
        let options = CompilerOptions::default();
        let id = FileId::default();
        let public = crate::parse_program(SOURCE, &id, &options).unwrap();
        let tree = parse_source_file(SOURCE, &parse_options(&options));
        assert_eq!(public, lower_library(&tree, &id).unwrap());
    }

    #[spec_test(REQ_PT_parser_014)]
    fn tokenize_program_when_valid_then_the_public_function_answers_with_the_token_view_of_the_tree(
    ) {
        let options = CompilerOptions::default();
        let id = FileId::default();
        let (tokens, diagnostics) = crate::tokenize_program(SOURCE, &id, &options, 2, 3);
        let parse = parse_options(&options);
        let (tree, errors) = tokenize(SOURCE, &parse);
        assert_eq!(
            format!("{tokens:?}"),
            format!("{:?}", project(SOURCE, &tree, &parse, &id, 2, 3))
        );
        assert_eq!(diagnostics.len(), errors.len());
        let first = tokens.first().map(|token| (token.line, token.col));
        assert_eq!(first, Some((2, 3)));
    }

    #[spec_test(REQ_PT_parser_014)]
    fn parse_st_statements_when_valid_then_the_public_function_answers_with_the_lowered_tree() {
        let options = CompilerOptions::default();
        let id = FileId::default();
        let public = crate::parse_st_statements("  x := 1;", &id, &options, 0, 0).unwrap();
        let tree = parse_statements("x := 1;", &parse_options(&options));
        assert_eq!(public, lower_statements(&tree, &id).unwrap());
    }

    #[spec_test(REQ_PT_parser_014)]
    fn legacy_modules_when_declared_in_the_crate_root_then_compiled_for_tests_only() {
        // The modules of the crate root that a production build compiles are
        // the ones named here. Every other module is declared under
        // `cfg(test)`, so a build without it cannot name the legacy pipeline:
        // the compiler, not a checklist, says production does not reach it.
        const PRODUCTION: [&str; 5] = ["declarations", "frontend", "options", "token", "tokens"];
        let root = std::path::Path::new(env!("CARGO_MANIFEST_DIR")).join("src/lib.rs");
        let text = std::fs::read_to_string(root).unwrap();
        let lines: Vec<&str> = text.lines().map(str::trim).collect();
        let mut production = Vec::new();
        for (index, line) in lines.iter().enumerate() {
            let declared = line
                .strip_prefix("pub mod ")
                .or_else(|| line.strip_prefix("mod "))
                .and_then(|rest| rest.strip_suffix(';'));
            let Some(name) = declared else { continue };
            let for_tests = index > 0 && lines[index - 1] == "#[cfg(test)]";
            if !for_tests {
                production.push(name);
            }
        }
        production.sort_unstable();
        assert_eq!(production, PRODUCTION);
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

    #[test]
    fn parse_st_statements_when_fragment_is_embedded_then_the_offsets_change_no_span_in_the_tree_or_the_oracle(
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
        // The legacy pipeline is the oracle: it read a fragment the same way.
        type Reader =
            fn(&str, &FileId, &CompilerOptions, usize, usize) -> Result<Vec<StmtKind>, Diagnostic>;
        let readers: [Reader; 2] = [crate::legacy::parse_st_statements, parse_st_statements];
        let mut by_front_end = Vec::new();
        for read in readers {
            let at_start = read(fragment, &id, &options, 0, 0).unwrap();
            let embedded = read(fragment, &id, &options, 7, 11).unwrap();
            assert_eq!(format!("{at_start:?}"), format!("{embedded:?}"));
            let first = format!("{:?}", at_start.first());
            assert!(first.contains("start: 5, end: 6"), "{first}");
            by_front_end.push(format!("{at_start:?}"));
        }
        assert_eq!(by_front_end[0], by_front_end[1]);
    }
}
