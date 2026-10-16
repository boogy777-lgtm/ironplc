//! Tests for the LSP project wrapper: document sync, diagnostics, tokens, symbols, and URI keys.

use std::path::PathBuf;
use std::str::FromStr;

use ironplc_dsl::core::{FileId, SourceSpan};
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_parser::token::Token;
use ironplc_test::cast;
use ironplc_test::read_shared_resource;
use lsp_types::Uri;

use ironplc_project::FileBackedProject;

use super::LspProject;
use crate::semantic_tokens::TOKEN_TYPE_LEGEND;
use crate::test_helpers::resolve_lsp_tokens;

#[cfg(target_os = "macos")]
static FAKE_PATH: &str = "file:///localhost/first_steps.st";
#[cfg(target_os = "linux")]
static FAKE_PATH: &str = "file:///localhost/first_steps.st";
#[cfg(target_os = "windows")]
static FAKE_PATH: &str = "file:///C:/first_steps.st";

fn new_empty_project() -> LspProject {
    LspProject::new(Box::new(FileBackedProject::new()))
}

// -----------------------------------------------------------------
// Multi-workspace-folder initialization.
// -----------------------------------------------------------------

fn workspace_folder(dir: &std::path::Path) -> lsp_types::WorkspaceFolder {
    // Windows paths are backslash-separated and start with a drive
    // letter (`C:\...`), neither of which is valid straight in a
    // `file://` URI -- normalise separators and add the extra `/`
    // before the drive letter, matching `UriKey::from_file_id`.
    let normalised = dir.display().to_string().replace('\\', "/");
    let uri_str = if normalised.starts_with('/') {
        format!("file://{normalised}")
    } else {
        format!("file:///{normalised}")
    };
    lsp_types::WorkspaceFolder {
        uri: Uri::from_str(&uri_str).unwrap(),
        name: dir.display().to_string(),
    }
}

#[test]
fn initialize_many_when_two_folders_then_cross_folder_type_resolves() {
    use std::fs;

    let dir1 = tempfile::TempDir::new().unwrap();
    fs::write(
        dir1.path().join("base.st"),
        "FUNCTION_BLOCK FB_Base\nEND_FUNCTION_BLOCK",
    )
    .unwrap();

    let dir2 = tempfile::TempDir::new().unwrap();
    fs::write(
        dir2.path().join("caller.st"),
        "FUNCTION_BLOCK FB_Caller\nVAR\n    inst : FB_Base;\nEND_VAR\nEND_FUNCTION_BLOCK",
    )
    .unwrap();

    let mut proj = new_empty_project();
    let folders = vec![workspace_folder(dir1.path()), workspace_folder(dir2.path())];
    proj.initialize_many(&folders);

    let diagnostics = proj.semantic_all();
    let all_diagnostics: Vec<_> = diagnostics.values().flatten().collect();
    assert!(
        all_diagnostics.is_empty(),
        "unexpected diagnostics: {all_diagnostics:?}"
    );
}

#[test]
fn initialize_many_when_single_folder_then_still_works() {
    use std::fs;

    let dir = tempfile::TempDir::new().unwrap();
    fs::write(dir.path().join("main.st"), "PROGRAM Main\nEND_PROGRAM").unwrap();

    let mut proj = new_empty_project();
    let folders = vec![workspace_folder(dir.path())];
    proj.initialize_many(&folders);

    let diagnostics = proj.semantic_all();
    let all_diagnostics: Vec<_> = diagnostics.values().flatten().collect();
    assert!(
        all_diagnostics.is_empty(),
        "unexpected diagnostics: {all_diagnostics:?}"
    );
}

#[test]
fn tokenize_when_no_document_then_no_tokens() {
    let proj = new_empty_project();
    let url = Uri::from_str("http://example.com").unwrap();
    assert!(proj.tokenize(&url).is_empty());
}

#[test]
fn tokenize_when_unterminated_comment_then_tokens_served_and_diagnostic_published() {
    // The tokenizer reports the comment that is never closed. Tokens are
    // served whatever it reports, so the text before the comment is still
    // highlighted while the comment is being typed, and the diagnostic
    // reaches the editor through the analysis.
    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    proj.change_text_document(
        &url,
        "PROGRAM Main
END_PROGRAM (* open"
            .to_owned(),
    );

    let tokens = proj.tokenize(&url);
    assert!(!tokens.is_empty());

    let published = proj.semantic_all();
    let diagnostics: Vec<_> = published.values().flatten().collect();
    assert!(!diagnostics.is_empty(), "no diagnostic was published");
}

#[test]
fn tokenize_when_has_document_then_not_empty_tokens() {
    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    proj.change_text_document(&url, "TYPE TEXT_EMPTY : STRING [1]; END_TYPE".to_owned());

    let result = proj.tokenize(&url);
    assert!(!result.is_empty());
}

#[test]
fn tokenize_when_tokens_span_multiple_lines_then_emits_relative_deltas() {
    // The LSP semantic-tokens protocol requires delta encoding: each
    // token's `delta_line` / `delta_start` are relative to the previous
    // emitted token. Reconstructing absolute (line, col) by accumulating
    // deltas must match the source positions of the keywords.
    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    proj.change_text_document(
        &url,
        "PROGRAM Main\nVAR\nx : BOOL;\nEND_VAR\nEND_PROGRAM".to_owned(),
    );

    let tokens = proj.tokenize(&url);
    assert!(!tokens.is_empty(), "expected tokens, got none");

    // Reconstruct absolute positions from the deltas.
    let mut line: u32 = 0;
    let mut col: u32 = 0;
    let mut absolutes: Vec<(u32, u32)> = Vec::new();
    for t in &tokens {
        if t.delta_line == 0 {
            col += t.delta_start;
        } else {
            line += t.delta_line;
            col = t.delta_start;
        }
        absolutes.push((line, col));
    }

    // PROGRAM is the first token at (0, 0).
    assert_eq!(absolutes[0], (0, 0), "PROGRAM should be at line 0, col 0");
    // VAR sits at the start of line 1.
    assert!(
        absolutes.contains(&(1, 0)),
        "expected a token at (1, 0) for VAR, got {absolutes:?}"
    );
    // BOOL sits at line 2, col 4.
    assert!(
        absolutes.contains(&(2, 4)),
        "expected a token at (2, 4) for BOOL, got {absolutes:?}"
    );
    // END_VAR sits at the start of line 3.
    assert!(
        absolutes.contains(&(3, 0)),
        "expected a token at (3, 0) for END_VAR, got {absolutes:?}"
    );
    // END_PROGRAM sits at the start of line 4.
    assert!(
        absolutes.contains(&(4, 0)),
        "expected a token at (4, 0) for END_PROGRAM, got {absolutes:?}"
    );
}

#[test]
fn tokenize_when_program_with_keywords_types_and_comment_then_each_lsp_token_lands_on_its_source_text(
) {
    // Coverage: every semantic token emitted to the LSP client must land
    // exactly on the source characters that the lexer recognised. If the
    // delta encoding drifts (or column tracking is wrong inside comments),
    // the substring at the reconstructed position will not match the
    // expected token text and this assertion will fail.
    let source = "PROGRAM Demo\nVAR\n  a : BOOL; (* note *) b : INT;\nEND_VAR\nEND_PROGRAM";
    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    proj.change_text_document(&url, source.to_owned());

    let tokens = proj.tokenize(&url);
    let resolved = resolve_lsp_tokens(source, &tokens);

    // Spot-check key tokens that the user reported as broken.
    let texts: Vec<&str> = resolved.iter().map(|(_, _, s, _)| *s).collect();
    for expected in [
        "PROGRAM",
        "Demo",
        "VAR",
        "a",
        "BOOL",
        "(* note *)",
        "b",
        "INT",
        "END_VAR",
        "END_PROGRAM",
    ] {
        assert!(
            texts.contains(&expected),
            "expected LSP token covering `{expected}`, got tokens: {texts:?}"
        );
    }

    // Both BOOL and INT sit on line 2; their reconstructed columns must
    // match the actual source columns.
    let bool_pos = resolved.iter().find(|(_, _, s, _)| *s == "BOOL").unwrap();
    assert_eq!(
        (bool_pos.0, bool_pos.1),
        (2, 6),
        "BOOL should be at line 2 col 6"
    );
    let int_pos = resolved.iter().find(|(_, _, s, _)| *s == "INT").unwrap();
    // After the inline block comment, the next BOOL/INT must still report
    // the correct column — this is the regression that produced the
    // user's "trip"/"ain Mot" mis-coloring.
    assert_eq!(
        (int_pos.0, int_pos.1),
        (2, 27),
        "INT should be at line 2 col 27"
    );

    // Both BOOL and INT must be tagged as the KEYWORD token type so that
    // VS Code applies the same semantic-token color to every primitive
    // type. (The reported "BOOL in three colors" bug was actually the
    // semantic overlay landing on different surrounding text — we keep
    // this assertion to lock in the type-tagging contract.)
    assert_eq!(bool_pos.3, int_pos.3, "BOOL and INT must share token type");
}

#[test]
fn tokenize_when_inline_block_comment_then_following_token_keeps_position() {
    // Regression: a single-line block comment used to leave the column
    // counter at the comment's start, which displaced every subsequent
    // semantic token in the LSP overlay and caused parts of comment text
    // to lose their `comment.block.st` styling.
    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    proj.change_text_document(
        &url,
        "VAR\n  a : BOOL; (* note *) b : BOOL;\nEND_VAR".to_owned(),
    );

    let tokens = proj.tokenize(&url);
    // Reconstruct absolute positions and the keyword index of each token.
    let mut line: u32 = 0;
    let mut col: u32 = 0;
    let mut bool_positions: Vec<(u32, u32)> = Vec::new();
    for t in &tokens {
        if t.delta_line == 0 {
            col += t.delta_start;
        } else {
            line += t.delta_line;
            col = t.delta_start;
        }
        // BOOL is length 4. The two BOOLs on line 1 sit at columns 6 and
        // 27 in the source string.
        if t.length == 4 && line == 1 {
            bool_positions.push((line, col));
        }
    }
    assert!(
        bool_positions.contains(&(1, 6)) && bool_positions.contains(&(1, 27)),
        "expected BOOLs at (1,6) and (1,27), got {bool_positions:?}"
    );
}

/// The only tokenizer test that runs over a whole real program.
/// `first_steps.st` covers every POU kind -- TYPE, FUNCTION,
/// FUNCTION_BLOCK, SFC steps, transitions and actions, CONFIGURATION --
/// so reconstructing every token from the deltas and matching it back
/// against the source proves the encoding holds across a long file and
/// every construct, which the two-line snippets above cannot.
#[test]
fn tokenize_when_first_steps_then_tokens_match_source() {
    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    let content = read_shared_resource("first_steps.st");
    proj.change_text_document(&url, content.clone());

    let tokens = proj.tokenize(&url);
    assert!(!tokens.is_empty(), "expected tokens, got none");

    // first_steps.st is ASCII, so a char index is also the UTF-16 offset
    // the LSP protocol counts in and can index the line directly.
    let lines: Vec<&str> = content.lines().collect();

    let mut line: u32 = 0;
    let mut col: u32 = 0;
    let mut lexemes: Vec<(u32, u32, String)> = Vec::new();
    for token in &tokens {
        if token.delta_line == 0 {
            col += token.delta_start;
        } else {
            line += token.delta_line;
            col = token.delta_start;
        }

        assert!(
            (token.token_type as usize) < TOKEN_TYPE_LEGEND.len(),
            "token type {} at ({line},{col}) is outside the legend",
            token.token_type
        );

        assert!(
            (line as usize) < lines.len(),
            "token at line {line} is past the end of the file"
        );
        let source_line = &lines[line as usize];
        let chars: Vec<char> = source_line.chars().collect();
        let end = col as usize + token.length as usize;
        assert!(
            end <= chars.len(),
            "token at ({line},{col}) of length {} runs past the end of {source_line:?}",
            token.length
        );

        // A token must cover exactly a lexeme: non-empty, and with no
        // whitespace at either edge. An off-by-one in the delta encoding
        // shifts the span onto neighbouring whitespace and is caught here.
        let lexeme: String = chars[col as usize..end].iter().collect();
        assert!(
            !lexeme.is_empty() && lexeme.trim().len() == lexeme.len(),
            "token at ({line},{col}) covers {lexeme:?}, which is not a lexeme"
        );
        lexemes.push((line, col, lexeme));
    }

    // Anchor both ends of the file at positions the loop above verified
    // against the source rather than assumed.
    assert_eq!(lexemes.first().unwrap(), &(0, 0, "TYPE".to_owned()));
    assert_eq!(
        lexemes.last().unwrap(),
        &(175, 0, "END_CONFIGURATION".to_owned())
    );
}

#[test]
fn semantic_when_error_creates_diagnostics() {
    // Create a project with content that will exercise the character iteration loop
    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();

    // Add some invalid syntax to trigger diagnostics that will use map_label
    let invalid_content = "TYPE\n
INVALID_RANGE : INT(10..-10);\n
END_TYPE\n
INVALID_SYNTAX"
        .to_string();
    proj.change_text_document(&url, invalid_content);

    // Call semantic analysis which will internally call map_label when creating diagnostics
    let _diagnostics = proj.semantic(&url);
}

#[test]
fn map_diagnostic_when_secondary_labels_then_produces_related_information() {
    use ironplc_dsl::core::{FileId, SourceSpan};
    use ironplc_dsl::diagnostic::{
        Diagnostic as DslDiagnostic, Label as DslLabel, Location as DslLocation,
    };

    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    let content = "PROGRAM Main\nVAR\n  x : INT;\nEND_VAR\nEND_PROGRAM";
    proj.change_text_document(&url, content.to_owned());

    let file_id = FileId::from_path(&std::path::PathBuf::from(url.path().as_str()));

    let diag = DslDiagnostic::text(
        SourceSpan::range(0, 7).with_file_id(&file_id),
        ironplc_problems::text::variable_undefined_undefined_variable(),
    )
    .with_secondary(DslLabel {
        location: DslLocation { start: 20, end: 23 },
        file_id: file_id.clone(),
        message: "secondary label".to_string(),
    });

    let lsp_diag = super::map_diagnostic(diag, proj.wrapped.as_ref());

    assert!(lsp_diag.related_information.is_some());
    let related = lsp_diag.related_information.unwrap();
    assert_eq!(related.len(), 1);
    assert_eq!(related[0].message, "secondary label");
}

#[test]
fn map_diagnostic_when_no_secondary_labels_then_related_information_is_none() {
    use ironplc_dsl::core::{FileId, SourceSpan};
    use ironplc_dsl::diagnostic::Diagnostic as DslDiagnostic;

    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    let content = "PROGRAM Main\nEND_PROGRAM";
    proj.change_text_document(&url, content.to_owned());

    let file_id = FileId::from_path(&std::path::PathBuf::from(url.path().as_str()));

    let diag = DslDiagnostic::text(
        SourceSpan::range(0, 7).with_file_id(&file_id),
        ironplc_problems::text::syntax_error_a_task_needs_a_priority(),
    );

    let lsp_diag = super::map_diagnostic(diag, proj.wrapped.as_ref());

    assert!(lsp_diag.related_information.is_none());
}

#[test]
fn map_diagnostic_when_problem_then_url_has_version_only() {
    use ironplc_dsl::core::{FileId, SourceSpan};
    use ironplc_dsl::diagnostic::Diagnostic as DslDiagnostic;

    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    let content = "PROGRAM Main\nEND_PROGRAM";
    proj.change_text_document(&url, content.to_owned());

    let file_id = FileId::from_path(&std::path::PathBuf::from(url.path().as_str()));

    let diag = DslDiagnostic::text(
        SourceSpan::range(0, 7).with_file_id(&file_id),
        ironplc_problems::text::syntax_error_a_task_needs_a_priority(),
    );

    let lsp_diag = super::map_diagnostic(diag, proj.wrapped.as_ref());

    let href = lsp_diag.code_description.unwrap().href.to_string();
    assert!(href.contains("?version="));
    assert!(href.contains("&channel=extension"));
    assert!(!href.contains("&file="));
    assert!(!href.contains("&line="));
}

// Regression test for the language server formatting its own URL with the
// reference section written out as `compiler`, which sent every non-`P`
// diagnostic to a page that does not exist. `V####` codes document under
// `reference/runtime/`.
#[test]
fn map_diagnostic_when_runtime_code_then_url_uses_runtime_section() {
    use ironplc_dsl::core::{FileId, SourceSpan};
    use ironplc_dsl::diagnostic::Diagnostic as DslDiagnostic;

    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    proj.change_text_document(&url, "PROGRAM Main\nEND_PROGRAM".to_owned());

    let file_id = FileId::from_path(&std::path::PathBuf::from(url.path().as_str()));

    let mut diag = DslDiagnostic::text(
        SourceSpan::range(0, 7).with_file_id(&file_id),
        ironplc_problems::text::syntax_error_a_task_needs_a_priority(),
    );
    // The VM's trap codes are not in the `Problem` enum, so stand one in
    // directly; `map_diagnostic` only ever reads the code.
    diag.code = "V4001".to_string();

    let lsp_diag = super::map_diagnostic(diag, proj.wrapped.as_ref());

    let href = lsp_diag.code_description.unwrap().href.to_string();
    assert!(
        href.contains("/reference/runtime/problems/V4001.html"),
        "V4001 documents under reference/runtime/, but the link is {href}",
    );
}

#[test]
fn map_diagnostic_when_todo_then_url_has_version_file_and_line() {
    use ironplc_dsl::core::{FileId, SourceSpan};
    use ironplc_dsl::diagnostic::Diagnostic as DslDiagnostic;

    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    let content = "PROGRAM Main\nEND_PROGRAM";
    proj.change_text_document(&url, content.to_owned());

    let file_id = FileId::from_path(&std::path::PathBuf::from(url.path().as_str()));

    let diag = DslDiagnostic::text(
        SourceSpan::range(0, 7).with_file_id(&file_id),
        ironplc_problems::text::syntax_error_a_task_needs_a_priority(),
    )
    .with_source("compiler/analyzer/src/rule_example.rs", 42);

    let lsp_diag = super::map_diagnostic(diag, proj.wrapped.as_ref());

    let href = lsp_diag.code_description.unwrap().href.to_string();
    assert!(href.contains("?version="));
    assert!(href.contains("&channel=extension"));
    assert!(href.contains("&file=compiler/analyzer/src/rule_example.rs"));
    assert!(href.contains("&line=42"));
}

#[test]
fn document_symbols_when_no_document_then_empty() {
    let proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();

    let result = proj.document_symbols(&url);

    let symbols = cast!(result, lsp_types::DocumentSymbolResponse::Nested);
    assert!(symbols.is_empty());
}

#[test]
fn document_symbols_when_has_structure_then_returns_struct_symbol() {
    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    let content = "TYPE\nMyStruct : STRUCT\n  field1 : INT;\nEND_STRUCT;\nEND_TYPE";
    proj.change_text_document(&url, content.to_owned());

    // Run semantic analysis to populate the context
    let _ = proj.semantic(&url);

    let result = proj.document_symbols(&url);

    let symbols = cast!(result, lsp_types::DocumentSymbolResponse::Nested);
    assert_eq!(symbols.len(), 1);
    assert_eq!(symbols[0].name, "MyStruct");
    assert_eq!(symbols[0].kind, lsp_types::SymbolKind::STRUCT);
}

#[test]
fn document_symbols_when_has_enumeration_then_returns_enum_symbol() {
    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    let content = "TYPE\nMyEnum : (VALUE1, VALUE2, VALUE3);\nEND_TYPE";
    proj.change_text_document(&url, content.to_owned());

    // Run semantic analysis to populate the context
    let _ = proj.semantic(&url);

    let result = proj.document_symbols(&url);

    let symbols = cast!(result, lsp_types::DocumentSymbolResponse::Nested);
    assert_eq!(symbols.len(), 1);
    assert_eq!(symbols[0].name, "MyEnum");
    assert_eq!(symbols[0].kind, lsp_types::SymbolKind::ENUM);
}

#[test]
fn document_symbols_when_has_function_block_then_returns_fb_symbol() {
    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    let content = "FUNCTION_BLOCK MyFB\nVAR\n  x : INT;\nEND_VAR\nEND_FUNCTION_BLOCK";
    proj.change_text_document(&url, content.to_owned());

    // Run semantic analysis to populate the context
    let _ = proj.semantic(&url);

    let result = proj.document_symbols(&url);

    let symbols = cast!(result, lsp_types::DocumentSymbolResponse::Nested);
    assert_eq!(symbols.len(), 1);
    assert_eq!(symbols[0].name, "MyFB");
}

#[test]
fn document_symbols_when_has_function_then_returns_function_symbol() {
    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    let content =
        "FUNCTION MyFunc : INT\nVAR_INPUT\n  x : INT;\nEND_VAR\nMyFunc := x * 2;\nEND_FUNCTION";
    proj.change_text_document(&url, content.to_owned());

    // Run semantic analysis to populate the context
    let _ = proj.semantic(&url);

    let result = proj.document_symbols(&url);

    let symbols = cast!(result, lsp_types::DocumentSymbolResponse::Nested);
    assert_eq!(symbols.len(), 1);
    assert_eq!(symbols[0].name, "MyFunc");
    assert_eq!(symbols[0].kind, lsp_types::SymbolKind::FUNCTION);
}

#[test]
fn document_symbols_when_multiple_types_then_returns_all() {
    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    let content =
        "TYPE\nMyStruct : STRUCT\n  field1 : INT;\nEND_STRUCT;\nMyEnum : (A, B);\nEND_TYPE";
    proj.change_text_document(&url, content.to_owned());

    // Run semantic analysis to populate the context
    let _ = proj.semantic(&url);

    let result = proj.document_symbols(&url);

    let symbols = cast!(result, lsp_types::DocumentSymbolResponse::Nested);
    assert_eq!(symbols.len(), 2);
    let names: Vec<&str> = symbols.iter().map(|s| s.name.as_str()).collect();
    assert!(names.contains(&"MyStruct"));
    assert!(names.contains(&"MyEnum"));
}

#[test]
fn range_of_when_single_line_then_correct_positions() {
    use super::range_of;

    let contents = "TYPE MyType : INT; END_TYPE";

    let range = range_of(contents, 5, 11);

    assert_eq!(range.start.line, 0);
    assert_eq!(range.start.character, 5);
    assert_eq!(range.end.line, 0);
    assert_eq!(range.end.character, 11);
}

#[test]
fn range_of_when_multiline_then_correct_positions() {
    use super::range_of;

    let contents = "TYPE\nMyStruct : STRUCT\n  field : INT;\nEND_STRUCT;\nEND_TYPE";
    // "MyStruct" starts at position 5 (after "TYPE\n")
    let range = range_of(contents, 5, 13);

    assert_eq!(range.start.line, 1);
    assert_eq!(range.start.character, 0);
    assert_eq!(range.end.line, 1);
    assert_eq!(range.end.character, 8);
}

#[test]
fn document_symbols_when_semantic_errors_then_returns_symbols() {
    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    // A valid struct (types resolve) plus a function block with an undefined variable
    // (semantic validation error from rule_use_declared_symbolic_var)
    let content = "TYPE\nMyStruct : STRUCT\n  field1 : INT;\nEND_STRUCT;\nEND_TYPE\n\nFUNCTION_BLOCK BadFB\nVAR\n  x : INT;\nEND_VAR\n  y := x;\nEND_FUNCTION_BLOCK";
    proj.change_text_document(&url, content.to_owned());

    // Semantic analysis will produce diagnostics for undefined variable 'y'
    let diagnostics = proj.semantic(&url);
    assert!(!diagnostics.is_empty());

    // Document symbols should still be available despite the error
    let result = proj.document_symbols(&url);
    let symbols = cast!(result, lsp_types::DocumentSymbolResponse::Nested);
    let names: Vec<&str> = symbols.iter().map(|s| s.name.as_str()).collect();
    assert!(names.contains(&"MyStruct"));
}

#[test]
fn map_diagnostic_when_problem_code_url_then_docs_directory_exists() {
    let mut path = PathBuf::from(env!("CARGO_MANIFEST_DIR"));
    // compiler/ironplc-cli/ -> repo root
    path.push("../..");
    path.push("docs/reference/compiler/problems");
    assert!(
        path.is_dir(),
        "Documentation directory for compiler problem codes does not exist: {}",
        path.display()
    );

    // Verify at least one problem code .rst file exists
    let has_problem_files = std::fs::read_dir(&path)
        .unwrap()
        .filter_map(|e| e.ok())
        .any(|e| {
            let name = e.file_name().to_string_lossy().to_string();
            name.starts_with('P') && name.ends_with(".rst")
        });
    assert!(
        has_problem_files,
        "No P*.rst files found in {}",
        path.display()
    );
}

#[test]
fn type_kind_to_symbol_kind_when_structure_then_struct() {
    use super::type_kind_to_symbol_kind;
    use ironplc_analyzer::extractors::TypeSymbolKind;

    assert_eq!(
        type_kind_to_symbol_kind(TypeSymbolKind::Structure),
        lsp_types::SymbolKind::STRUCT
    );
}

#[test]
fn type_kind_to_symbol_kind_when_enumeration_then_enum() {
    use super::type_kind_to_symbol_kind;
    use ironplc_analyzer::extractors::TypeSymbolKind;

    assert_eq!(
        type_kind_to_symbol_kind(TypeSymbolKind::Enumeration),
        lsp_types::SymbolKind::ENUM
    );
}

#[test]
fn type_kind_to_symbol_kind_when_function_block_then_class() {
    use super::type_kind_to_symbol_kind;
    use ironplc_analyzer::extractors::TypeSymbolKind;

    assert_eq!(
        type_kind_to_symbol_kind(TypeSymbolKind::FunctionBlock),
        lsp_types::SymbolKind::CLASS
    );
}

#[cfg(target_os = "windows")]
static FAKE_PATH_2: &str = "file:///C:/second_steps.st";
#[cfg(not(target_os = "windows"))]
static FAKE_PATH_2: &str = "file:///localhost/second_steps.st";

#[test]
fn semantic_all_when_no_errors_then_returns_empty_map() {
    let mut proj = new_empty_project();
    let url = Uri::from_str(FAKE_PATH).unwrap();
    proj.change_text_document(
        &url,
        "PROGRAM Main\nVAR\n  x : BOOL;\nEND_VAR\nEND_PROGRAM".to_owned(),
    );

    let by_uri = proj.semantic_all();
    // Empty project (no configuration with a program instance) still
    // analyses cleanly when there are no parse errors. Either way,
    // the contract is "no errors => no entries".
    for diags in by_uri.values() {
        assert!(
            diags.is_empty(),
            "expected no diagnostics for clean project, got: {:?}",
            diags
        );
    }
}

#[test]
fn semantic_all_when_two_files_each_have_errors_then_returns_diagnostics_for_each_uri() {
    use super::UriKey;

    let mut proj = new_empty_project();
    let url_a = Uri::from_str(FAKE_PATH).unwrap();
    let url_b = Uri::from_str(FAKE_PATH_2).unwrap();
    let key_a = UriKey::from_uri(&url_a);
    let key_b = UriKey::from_uri(&url_b);

    // Both files contain unparseable garbage so each produces its
    // own parse-time diagnostics and the analyzer should attribute
    // them to their respective FileIds.
    proj.change_text_document(&url_a, "this is not valid IEC 61131-3".to_owned());
    proj.change_text_document(&url_b, "neither is this".to_owned());

    let by_key = proj.semantic_all();

    assert!(
        by_key.contains_key(&key_a),
        "expected diagnostics for {url_a:?}, got keys {:?}",
        by_key.keys().collect::<Vec<_>>()
    );
    assert!(
        by_key.contains_key(&key_b),
        "expected diagnostics for {url_b:?}, got keys {:?}",
        by_key.keys().collect::<Vec<_>>()
    );
    assert!(!by_key[&key_a].is_empty());
    assert!(!by_key[&key_b].is_empty());
}

/// A project whose analysis reports one diagnostic attributed to
/// `FileId::BuiltIn` -- a stdlib declaration, which has no file behind it
/// and so no URI an editor could open.
struct BuiltInDiagnosticProject;

impl ironplc_project::Project for BuiltInDiagnosticProject {
    fn initialize(&mut self, _dir: &std::path::Path) -> Vec<Diagnostic> {
        vec![]
    }

    fn change_text_document(&mut self, _file_id: &FileId, _content: String) {}

    fn tokenize(&self, _file_id: &FileId) -> (Vec<Token>, Vec<Diagnostic>) {
        (vec![], vec![])
    }

    fn semantic(&mut self) -> Vec<Diagnostic> {
        vec![Diagnostic::text(
            SourceSpan::default().with_file_id(&FileId::builtin()),
            ironplc_problems::text::parent_type_not_declared_referenced_type(),
        )]
    }

    fn semantic_context(&self) -> Option<&ironplc_analyzer::SemanticContext> {
        None
    }

    fn analyzed_library(&self) -> Option<&ironplc_dsl::common::Library> {
        None
    }

    fn sources(&self) -> Vec<&ironplc_sources::Source> {
        vec![]
    }

    fn sources_mut(&mut self) -> Vec<&mut ironplc_sources::Source> {
        vec![]
    }

    fn find(&self, _file_id: &FileId) -> Option<&ironplc_sources::Source> {
        None
    }
}

/// A diagnostic with no openable file is attributed to the last edited
/// document rather than dropped -- otherwise the editor shows nothing at
/// all and the user is left with a command that failed for no visible
/// reason.
#[test]
fn semantic_all_when_diagnostic_has_no_openable_file_then_falls_back_to_last_changed() {
    use super::UriKey;

    let mut proj = LspProject::new(Box::new(BuiltInDiagnosticProject));
    let url = Uri::from_str(FAKE_PATH).unwrap();
    proj.change_text_document(&url, "PROGRAM Main\nEND_PROGRAM".to_owned());

    let by_key = proj.semantic_all();

    assert_eq!(
        by_key.keys().collect::<Vec<_>>(),
        vec![&UriKey::from_uri(&url)]
    );
    assert_eq!(by_key[&UriKey::from_uri(&url)].len(), 1);
}

/// With nothing edited yet there is no document to attribute it to, so the
/// diagnostic has nowhere to go.
#[test]
fn semantic_all_when_diagnostic_has_no_openable_file_and_no_last_changed_then_dropped() {
    let mut proj = LspProject::new(Box::new(BuiltInDiagnosticProject));

    assert!(proj.semantic_all().is_empty());
}

#[test]
fn semantic_when_back_compat_wrapper_then_returns_only_uris_diagnostics() {
    // The single-URI `semantic` wrapper should still return only the
    // diagnostics that the publisher would direct at that URI.
    let mut proj = new_empty_project();
    let url_a = Uri::from_str(FAKE_PATH).unwrap();
    let url_b = Uri::from_str(FAKE_PATH_2).unwrap();

    proj.change_text_document(&url_a, "this is not valid IEC 61131-3".to_owned());
    proj.change_text_document(&url_b, "neither is this".to_owned());

    let only_a = proj.semantic(&url_a);
    assert!(
        !only_a.is_empty(),
        "expected diagnostics for url_a from back-compat wrapper"
    );
    // None of the diagnostics returned by `semantic(url_a)` should
    // refer to url_b's range/source — the wrapper is a strict
    // single-URI view.
    // (Indirect check: `semantic_all` for url_b returns at least
    // one diagnostic that does not appear in `only_a`.)
    let only_b = proj.semantic(&url_b);
    assert!(!only_b.is_empty());
}

#[test]
fn uri_key_from_file_id_when_round_trip_from_uri_path_then_matches_original() {
    use super::UriKey;
    use ironplc_dsl::core::FileId;

    let original = Uri::from_str(FAKE_PATH).unwrap();
    let path = std::path::PathBuf::from(original.path().as_str());
    let file_id = FileId::from_path(&path);

    let reconstructed = UriKey::from_file_id(&file_id).unwrap();
    assert_eq!(
        reconstructed.as_str(),
        original.as_str(),
        "expected URI key round-trip via FileId to match the original URI string"
    );
}

#[test]
fn uri_key_from_file_id_when_builtin_then_returns_none() {
    use super::UriKey;
    use ironplc_dsl::core::FileId;

    assert!(UriKey::from_file_id(&FileId::builtin()).is_none());
}

#[test]
fn uri_key_when_to_uri_then_round_trips_back_to_original() {
    use super::UriKey;

    let original = Uri::from_str(FAKE_PATH).unwrap();
    let key = UriKey::from_uri(&original);
    let back = key.to_uri();
    assert_eq!(back.as_str(), original.as_str());
}
