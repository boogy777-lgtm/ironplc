use super::*;
use spec_test_macro::spec_test;

/// A diagnostic of the given severity. The severity is the problem's and no
/// call site sets it, so a test that needs a level no problem has yet
/// builds the diagnostic here, where it can see the field.
fn diagnostic_of(severity: Severity) -> Diagnostic {
    let mut diagnostic = Diagnostic::problem(
        Problem::SyntaxError,
        Label::span(SourceSpan::default(), "primary"),
    );
    diagnostic.severity = severity;
    diagnostic
}

#[spec_test(REQ_DS_dsl_001)]
fn severity_when_made_by_each_constructor_then_the_severity_of_its_problem() {
    let span = SourceSpan::default();
    let id = Id::from("a");
    let primary = || Label::span(SourceSpan::default(), "primary");
    let made = [
        (
            Diagnostic::text(
                span.clone(),
                ironplc_problems::text::attribute_repeated("pack_mode"),
            ),
            "P4078",
        ),
        (
            Diagnostic::text_in_file(
                FileId::from_string("f"),
                ironplc_problems::text::attribute_repeated("pack_mode"),
            ),
            "P4078",
        ),
        (Diagnostic::todo_with_id(&id), "P9999"),
        (Diagnostic::todo_with_span(span), "P9999"),
        (Diagnostic::not_implemented(primary()), "P9999"),
        (Diagnostic::not_supported(primary()), "P9997"),
        (Diagnostic::internal_error(), "P9998"),
        (Diagnostic::internal_error_at(primary()), "P9998"),
    ];

    for (diagnostic, code) in made {
        assert_eq!(diagnostic.code, code);
        // Every existing problem is an error; the first two rows also
        // compare with the severity the problem itself declares.
        assert_eq!(diagnostic.severity(), Severity::Error, "{code}");
        assert!(diagnostic.is_error(), "{code}");
    }
    assert_eq!(
        Diagnostic::text(
            SourceSpan::default(),
            ironplc_problems::text::attribute_repeated("pack_mode")
        )
        .severity(),
        Problem::AttributeRepeated.severity()
    );
}

#[spec_test(REQ_DS_dsl_002)]
fn is_error_when_each_severity_then_true_only_for_error() {
    assert!(diagnostic_of(Severity::Error).is_error());
    assert!(!diagnostic_of(Severity::Warning).is_error());
    assert!(!diagnostic_of(Severity::Info).is_error());
    assert_eq!(
        diagnostic_of(Severity::Warning).severity(),
        Severity::Warning
    );
    assert_eq!(diagnostic_of(Severity::Info).severity(), Severity::Info);
}

#[spec_test(REQ_DS_dsl_003)]
fn has_errors_when_list_then_true_only_with_an_error_and_error_count_counts_them() {
    let none: [Diagnostic; 0] = [];
    let not_errors = [
        diagnostic_of(Severity::Warning),
        diagnostic_of(Severity::Info),
    ];
    let mixed = [
        diagnostic_of(Severity::Warning),
        diagnostic_of(Severity::Error),
        diagnostic_of(Severity::Info),
        diagnostic_of(Severity::Error),
    ];

    assert!(!has_errors(&none));
    assert_eq!(error_count(&none), 0);
    assert!(!has_errors(&not_errors));
    assert_eq!(error_count(&not_errors), 0);
    assert!(has_errors(&mixed));
    assert_eq!(error_count(&mixed), 2);
}

#[test]
fn text_when_row_then_same_diagnostic_as_problem_with_span_label() {
    let span = SourceSpan::default();

    let from_text = Diagnostic::text(
        span.clone(),
        ironplc_problems::text::attribute_repeated("pack_mode"),
    );
    let from_problem = Diagnostic::problem(
        Problem::AttributeRepeated,
        Label::span(
            span,
            "The attribute 'pack_mode' is written again for the same declaration",
        ),
    );

    assert_eq!(from_text.code, from_problem.code);
    assert_eq!(from_text.description, from_problem.description);
    assert_eq!(from_text.primary.message, from_problem.primary.message);
    assert_eq!(from_text.primary.file_id, from_problem.primary.file_id);
    assert_eq!(
        from_text.primary.location.start,
        from_problem.primary.location.start
    );
    assert_eq!(
        from_text.primary.location.end,
        from_problem.primary.location.end
    );
    assert!(from_text.secondary.is_empty() && from_text.help.is_empty());
    assert!(from_text.source_file.is_none() && from_text.source_line.is_none());
}

#[test]
fn text_in_file_when_row_then_same_diagnostic_as_problem_with_file_label() {
    let from_text = Diagnostic::text_in_file(
        FileId::from_string("file1"),
        ironplc_problems::text::attribute_repeated("pack_mode"),
    );
    let from_problem = Diagnostic::problem(
        Problem::AttributeRepeated,
        Label::file(
            FileId::from_string("file1"),
            "The attribute 'pack_mode' is written again for the same declaration",
        ),
    );

    assert_eq!(from_text.code, from_problem.code);
    assert_eq!(from_text.description, from_problem.description);
    assert_eq!(from_text.primary.message, from_problem.primary.message);
    assert_eq!(from_text.primary.file_id, from_problem.primary.file_id);
    assert_eq!(from_text.primary.location.start, 0);
    assert_eq!(from_text.primary.location.end, 0);
    assert!(from_text.secondary.is_empty() && from_text.help.is_empty());
    assert!(from_text.source_file.is_none() && from_text.source_line.is_none());
}

#[test]
fn with_secondary_when_row_text_then_label_message_is_the_text() {
    let diag = Diagnostic::text(
        SourceSpan::default(),
        ironplc_problems::text::attribute_repeated("a"),
    )
    .with_secondary(Label::span(
        SourceSpan::default(),
        ironplc_problems::text::attribute_repeated_first_written_here(),
    ));

    assert_eq!(diag.secondary[0].message, "First written here");
}

#[test]
fn todo_with_id_when_called_then_includes_id_location() {
    let id = Id::from("my_var");
    let line = line!() + 1;
    let diag = Diagnostic::todo_with_id(&id);
    assert_eq!(diag.code, "P9999");
    assert!(diag.primary.message.contains(file!()));
    assert_eq!(diag.source_file.as_deref(), Some(file!()));
    assert_eq!(diag.source_line, Some(line));
}

#[test]
fn todo_with_type_when_called_then_includes_type_location() {
    let ty = TypeName::from("MY_TYPE");
    let line = line!() + 1;
    let diag = Diagnostic::todo_with_type(&ty);
    assert_eq!(diag.code, "P9999");
    assert!(diag.primary.message.contains(file!()));
    assert_eq!(diag.source_file.as_deref(), Some(file!()));
    assert_eq!(diag.source_line, Some(line));
}

#[test]
fn todo_with_span_when_called_then_includes_span() {
    let span = SourceSpan::default();
    let line = line!() + 1;
    let diag = Diagnostic::todo_with_span(span);
    assert_eq!(diag.code, "P9999");
    assert!(diag.primary.message.contains(file!()));
    assert_eq!(diag.source_file.as_deref(), Some(file!()));
    assert_eq!(diag.source_line, Some(line));
}

#[test]
fn internal_error_when_called_then_creates_diagnostic() {
    let line = line!() + 1;
    let diag = Diagnostic::internal_error();
    assert_eq!(diag.code, "P9998");
    assert!(diag.primary.message.contains(file!()));
    assert!(diag.primary.message.contains("bug in the compiler"));
    assert_eq!(diag.source_file.as_deref(), Some(file!()));
    assert_eq!(diag.source_line, Some(line));
}

#[test]
fn not_implemented_when_called_then_records_caller_location_and_label() {
    let diag = Diagnostic::not_implemented(Label::span(
        SourceSpan::default(),
        "custom unimplemented message",
    ));
    assert_eq!(diag.code, "P9999");
    // The caller's compiler location is captured automatically.
    assert_eq!(diag.source_file.as_deref(), Some(file!()));
    assert!(diag.source_line.is_some());
    // The caller-supplied label is preserved (unlike todo*()).
    assert_eq!(diag.primary.message, "custom unimplemented message");
}

#[test]
fn internal_error_at_when_called_then_records_caller_location_and_label() {
    let diag = Diagnostic::internal_error_at(Label::span(
        SourceSpan::default(),
        "custom internal error message",
    ));
    assert_eq!(diag.code, "P9998");
    assert_eq!(diag.source_file.as_deref(), Some(file!()));
    assert!(diag.source_line.is_some());
    assert_eq!(diag.primary.message, "custom internal error message");
}

#[test]
fn offset_to_line_column_when_at_start_then_returns_zero_zero() {
    let lc = LineColumn::from_offset("abc\ndef", 0);
    assert_eq!(lc, LineColumn { line: 0, column: 0 });
}

#[test]
fn offset_to_line_column_when_on_first_line_then_returns_line_zero() {
    let lc = LineColumn::from_offset("abc\ndef", 2);
    assert_eq!(lc, LineColumn { line: 0, column: 2 });
}

#[test]
fn offset_to_line_column_when_after_newline_then_advances_line() {
    let lc = LineColumn::from_offset("abc\ndef", 4);
    assert_eq!(lc, LineColumn { line: 1, column: 0 });
}

#[test]
fn offset_to_line_column_when_on_later_line_then_returns_correct_column() {
    let lc = LineColumn::from_offset("abc\ndef\nghi", 9);
    assert_eq!(lc, LineColumn { line: 2, column: 1 });
}

#[test]
fn offset_to_line_column_when_offset_past_end_then_clamps_to_end() {
    let lc = LineColumn::from_offset("abc", 999);
    assert_eq!(lc, LineColumn { line: 0, column: 3 });
}

#[test]
fn offset_to_line_column_when_empty_source_then_returns_zero_zero() {
    let lc = LineColumn::from_offset("", 0);
    assert_eq!(lc, LineColumn { line: 0, column: 0 });
}

#[test]
fn file_ids_when_has_secondary_then_returns_all_file_ids() {
    let primary_file = FileId::from_string("file1");
    let secondary_file = FileId::from_string("file2");
    let diag = Diagnostic::problem(
        Problem::SyntaxError,
        Label::file(primary_file.clone(), "primary"),
    )
    .with_secondary(Label::file(secondary_file.clone(), "secondary"));

    let ids = diag.file_ids();
    assert_eq!(ids, vec![&primary_file, &secondary_file]);
}

#[test]
fn file_ids_when_secondary_in_primary_file_then_file_once() {
    let file = FileId::from_string("file1");
    let other = FileId::from_string("file2");
    let diag = Diagnostic::problem(Problem::SyntaxError, Label::file(file.clone(), "primary"))
        .with_secondary(Label::file(other.clone(), "second"))
        .with_secondary(Label::file(file.clone(), "third"))
        .with_secondary(Label::file(other.clone(), "fourth"));

    assert_eq!(diag.file_ids(), vec![&file, &other]);
}

#[test]
fn docs_section_when_known_prefix_then_matches_reference_section() {
    assert_eq!(docs_section("P0001"), "compiler");
    assert_eq!(docs_section("V6008"), "runtime");
    assert_eq!(docs_section("E0001"), "editor");
}

#[test]
fn docs_section_when_unknown_prefix_then_unknown() {
    // A future code family must not be silently attributed to an existing
    // section; it falls back to "unknown" (a 404) until mapped.
    assert_eq!(docs_section("D0001"), "unknown");
    assert_eq!(docs_section(""), "unknown");
}

/// The repository root, relative to this crate.
fn repo_root() -> std::path::PathBuf {
    std::path::Path::new(env!("CARGO_MANIFEST_DIR")).join("../..")
}

/// Every documented problem code, paired with the reference section whose
/// `problems/` directory actually holds its page, read from the real docs
/// tree. Shared by the two tests below so the tree walk exists once.
fn documented_codes() -> Vec<(String, String)> {
    let reference = repo_root().join("docs/reference");
    let mut codes = Vec::new();

    for section_entry in std::fs::read_dir(&reference)
        .expect("read docs/reference directory")
        .flatten()
    {
        let section = section_entry.file_name().to_string_lossy().to_string();
        let problems_dir = section_entry.path().join("problems");
        let Ok(files) = std::fs::read_dir(&problems_dir) else {
            continue; // Not every reference section has a problems/ dir.
        };

        for file in files.flatten() {
            let name = file.file_name().to_string_lossy().to_string();
            let Some(code) = name.strip_suffix(".rst") else {
                continue;
            };
            // Problem-code files are <LETTER><digits> (e.g. P0001); skip
            // index.rst and any other prose pages.
            let mut chars = code.chars();
            let is_code = matches!(chars.next(), Some(c) if c.is_ascii_uppercase())
                && !code[1..].is_empty()
                && code[1..].chars().all(|c| c.is_ascii_digit());
            if !is_code {
                continue;
            }

            codes.push((section.clone(), code.to_string()));
        }
    }

    assert!(
        !codes.is_empty(),
        "found no documented problem codes under {} — test wiring is broken",
        reference.display()
    );
    codes
}

// Guards against a new documented code family (a new
// docs/reference/<section>/problems/ directory) slipping past `docs_section`
// and shipping a wrong or 404 problem-code link. Walks the real docs tree
// and asserts every documented code's prefix maps to the section directory
// that actually contains its page. If this fails, add the new prefix to
// `docs_section`.
#[test]
fn docs_section_covers_every_documented_code() {
    for (section, code) in documented_codes() {
        assert_eq!(
            docs_section(&code),
            section,
            "docs_section({code}) should be {section:?} (its page lives in \
             docs/reference/{section}/problems/); a new code family needs a \
             matching arm in docs_section",
        );
    }
}

#[test]
fn problem_help_url_when_known_prefix_then_links_to_that_section() {
    assert!(problem_help_url("P0001", "1.0.0", "cli", None, None)
        .starts_with("https://www.ironplc.com/reference/compiler/problems/P0001.html?"));
    assert!(problem_help_url("V6008", "1.0.0", "cli", None, None)
        .starts_with("https://www.ironplc.com/reference/runtime/problems/V6008.html?"));
    assert!(problem_help_url("E0001", "1.0.0", "cli", None, None)
        .starts_with("https://www.ironplc.com/reference/editor/problems/E0001.html?"));
}

#[test]
fn problem_help_url_when_unknown_prefix_then_unknown_section() {
    // An unmapped family must 404 honestly rather than be attributed to an
    // existing section, which would look like a working link to the wrong
    // page.
    assert!(problem_help_url("D0001", "1.0.0", "cli", None, None)
        .contains("/reference/unknown/problems/D0001.html"));
}

#[test]
fn problem_help_url_when_channel_given_then_tagged_with_version_and_channel() {
    let url = problem_help_url("P0001", "1.2.3", "mcp", None, None);
    assert!(url.ends_with("?version=1.2.3&channel=mcp"));
    assert!(!url.contains("&file="));
    assert!(!url.contains("&line="));
}

#[test]
fn problem_help_url_when_source_location_then_appends_file_and_line() {
    let url = problem_help_url(
        "P0001",
        "1.2.3",
        "extension",
        Some("compiler/analyzer/src/rule_example.rs"),
        Some(42),
    );
    assert!(url.contains("&file=compiler/analyzer/src/rule_example.rs"));
    assert!(url.contains("&line=42"));
}

#[test]
fn help_url_when_diagnostic_has_source_then_carries_it() {
    let diag = Diagnostic::problem(
        Problem::SyntaxError,
        Label::span(SourceSpan::default(), "some error".to_string()),
    )
    .with_source("compiler/analyzer/src/rule_example.rs", 42);

    assert_eq!(
        diag.help_url("1.2.3", "cli"),
        problem_help_url(
            &diag.code,
            "1.2.3",
            "cli",
            Some("compiler/analyzer/src/rule_example.rs"),
            Some(42),
        )
    );
}

// The end-to-end guard: for every code that has a documentation page, the
// URL we actually hand a user must point at the directory that page lives
// in. `docs_section_covers_every_documented_code` checks the mapping;
// this checks the thing built from it, so a builder that ignored
// `docs_section` (as the language server once did) fails here.
#[test]
fn problem_help_url_when_every_documented_code_then_points_at_its_page() {
    for (section, code) in documented_codes() {
        let url = problem_help_url(&code, "1.0.0", "cli", None, None);
        assert!(
            url.contains(&format!("/reference/{section}/problems/{code}.html")),
            "problem_help_url({code}) is {url}, but the page lives in \
             docs/reference/{section}/problems/",
        );
    }
}

// Keeps the fix for the language server's hardcoded `compiler` section from
// coming back in a new channel. `problem_help_url` is the only place in the
// compiler allowed to name the documentation host; anything else formatting
// its own URL is free to pick the wrong section, which is exactly the bug
// this consolidation removed. A new channel calls the builder.
#[test]
fn problem_help_url_is_the_only_place_rust_names_the_docs_host() {
    const HOST: &str = "ironplc.com/reference";
    let compiler = repo_root().join("compiler");
    // The module that builds the URL and the file of its tests, which name the
    // host to check the URL.
    let exempt: Vec<_> = ["dsl/src/diagnostic.rs", "dsl/src/diagnostic/tests.rs"]
        .iter()
        .filter_map(|file| compiler.join(file).canonicalize().ok())
        .collect();

    let mut offenders = Vec::new();
    let mut dirs = vec![compiler.clone()];
    while let Some(dir) = dirs.pop() {
        for entry in std::fs::read_dir(&dir)
            .expect("read compiler directory")
            .flatten()
        {
            let path = entry.path();
            if path.is_dir() {
                // Build output is not source; it contains our own strings.
                if entry.file_name() == "target" {
                    continue;
                }
                dirs.push(path);
            } else if path.extension().is_some_and(|e| e == "rs")
                && path
                    .canonicalize()
                    .is_ok_and(|resolved| !exempt.contains(&resolved))
                && std::fs::read_to_string(&path).is_ok_and(|t| t.contains(HOST))
            {
                offenders.push(path.display().to_string());
            }
        }
    }

    assert!(
        offenders.is_empty(),
        "these build a documentation URL instead of calling problem_help_url, so \
         nothing stops them naming the wrong reference section: {}",
        offenders.join(", "),
    );
}
