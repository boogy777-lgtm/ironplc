//! Problem-code parity with the legacy parser, test-only.
//!
//! The legacy parser reports the first diagnostic it finds; the new parser
//! reports every one. For each input both reject, the legacy problem code must
//! be among the new codes, and a difference must be listed in
//! `CODE_EXCEPTIONS` with its reason. Each token check rule is also compared
//! on a snippet of its own, down to the byte range of the diagnostic.

mod common;
mod parity;

use ironplc_dsl::core::FileId;
use ironplc_parser::options::CompilerOptions;
use ironplc_syntax::{parse_source_file, ParseOptions};
use parity::diagnostics::{compare_codes, CODE_EXCEPTIONS};
use parity::legacy::{convert, presets, rejection};

/// One rule on an input of its own. The legacy problem code and the byte range
/// of the legacy diagnostic are expected from the new parser too.
struct RuleCase {
    rule: &'static str,
    source: &'static str,
    code: &'static str,
    /// True when no flag disables the rule, so it is checked with every
    /// flag on; otherwise it is checked with every flag off.
    always: bool,
}

const fn gated(rule: &'static str, source: &'static str, code: &'static str) -> RuleCase {
    RuleCase {
        rule,
        source,
        code,
        always: false,
    }
}

const RULES: &[RuleCase] = &[
    gated(
        "C-style comment",
        "PROGRAM p\n// note\nEND_PROGRAM\n",
        "P0004",
    ),
    gated(
        "C-style documentation comment",
        "PROGRAM p\n/// note\nEND_PROGRAM\n",
        "P0004",
    ),
    gated(
        "empty variable block",
        "PROGRAM p\nVAR\nEND_VAR\nEND_PROGRAM\n",
        "P0011",
    ),
    gated(
        "partial-access syntax",
        "PROGRAM p\nVAR x : DWORD; END_VAR\nx.%X1 := TRUE;\nEND_PROGRAM\n",
        "P4033",
    ),
    gated(
        "parenthesis string length",
        "PROGRAM p\nVAR x : STRING(10); END_VAR\nEND_PROGRAM\n",
        "P4042",
    ),
    gated(
        "incomplete array",
        "FUNCTION f : INT\nVAR_INPUT a : ARRAY [*] OF INT; END_VAR\nEND_FUNCTION\n",
        "P4070",
    ),
    gated(
        "escaped identifier",
        "PROGRAM p\nVAR `a b` : INT; END_VAR\nEND_PROGRAM\n",
        "P4067",
    ),
    gated(
        "non-ASCII identifier",
        "PROGRAM p\nVAR caf\u{e9} : INT; END_VAR\nEND_PROGRAM\n",
        "P4068",
    ),
    gated(
        "consecutive underscores",
        "PROGRAM p\nVAR a__b : INT; END_VAR\nEND_PROGRAM\n",
        "P4069",
    ),
    RuleCase {
        rule: "undefined string escape",
        source: "PROGRAM p\nVAR s : STRING; END_VAR\ns := 'a$Qb';\nEND_PROGRAM\n",
        code: "P0012",
        always: true,
    },
    RuleCase {
        rule: "unmatched character",
        source: "PROGRAM p\nVAR x : INT; END_VAR\nx := 1 ? 2;\nEND_PROGRAM\n",
        code: "P0003",
        always: true,
    },
    RuleCase {
        rule: "grammar error",
        source: "PROGRAM p\nVAR x : INT; END_VAR\nx := ;\nEND_PROGRAM\n",
        code: "P0002",
        always: true,
    },
];

fn options_for(case: &RuleCase) -> (CompilerOptions, ParseOptions) {
    let mut legacy = CompilerOptions::default();
    if case.always {
        for descriptor in CompilerOptions::FEATURE_DESCRIPTORS {
            legacy.set_flag_by_key(descriptor.option_key, true);
        }
    }
    let new = convert(&legacy);
    (legacy, new)
}

fn found(source: &str, options: &ParseOptions) -> Vec<(String, usize, usize)> {
    parse_source_file(source, options)
        .diagnostics(&FileId::default())
        .into_iter()
        .map(|d| (d.code, d.primary.location.start, d.primary.location.end))
        .collect()
}

#[test]
fn diagnostics_when_token_check_rule_input_then_same_code_and_range_as_legacy() {
    for case in RULES {
        let (legacy_options, new_options) = options_for(case);
        let legacy = rejection(case.source, &legacy_options);
        assert!(
            legacy.is_some(),
            "{}: the legacy parser accepts the input",
            case.rule
        );
        let legacy = legacy.unwrap_or_default();
        assert_eq!(legacy.0, case.code, "{}: legacy code", case.rule);
        let new = found(case.source, &new_options);
        assert!(
            new.contains(&legacy),
            "{}: expected {legacy:?}, got {new:?}",
            case.rule
        );
    }
}

#[test]
fn diagnostics_when_rule_flag_on_then_the_rule_does_not_fire() {
    for case in RULES.iter().filter(|case| !case.always) {
        let codes: Vec<String> = found(case.source, &ParseOptions::all())
            .into_iter()
            .map(|(code, _, _)| code)
            .collect();
        assert!(!codes.contains(&case.code.to_string()), "{}", case.rule);
    }
}

#[test]
fn diagnostics_when_corpus_rejected_by_both_then_legacy_code_is_among_the_new_codes() {
    let files = common::file_variants();
    let report = compare_codes(&files, &presets(), CODE_EXCEPTIONS);
    println!(
        "{} rejected by both, {} with the same code",
        report.both_reject, report.same_code
    );
    assert!(
        report.same_code >= 100,
        "only {} files rejected by both parsers with the same code",
        report.same_code
    );
    assert!(
        report.unexplained.is_empty(),
        "{} unexplained code differences:\n{}",
        report.unexplained.len(),
        report.unexplained.join("\n")
    );
    assert!(
        report.stale.is_empty(),
        "exceptions that no longer differ:\n{}",
        report.stale.join("\n")
    );
}

#[test]
fn diagnostics_when_corpus_parsed_then_every_range_lies_inside_the_source_and_has_a_code() {
    for (name, text) in common::file_variants() {
        for preset in presets() {
            for (code, start, end) in found(&text, &preset.new) {
                assert!(
                    code.starts_with('P') && start <= end && end <= text.len(),
                    "{name} under {}: {code} at {start}..{end}",
                    preset.name
                );
            }
        }
    }
}
