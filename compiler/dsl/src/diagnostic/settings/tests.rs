use super::*;
use crate::core::{FileId, SourceSpan};
use crate::diagnostic::{counts, Counts, CHECK_SUMMARY};
use ironplc_problems::text;
use spec_test_macro::spec_test;

/// The only warning today (P4083, CODESYS C0354) at the byte `at`; every call
/// with a different `at` is a different message.
fn warning_at(at: usize) -> Diagnostic {
    Diagnostic::text(
        SourceSpan::range(at, at + 1).with_file_id(&FileId::from_string("main.st")),
        text::enumeration_types_compared("E1", "E2"),
    )
}

fn error_at(at: usize) -> Diagnostic {
    Diagnostic::text(
        SourceSpan::range(at, at + 1).with_file_id(&FileId::from_string("main.st")),
        text::variable_undefined(),
    )
}

/// The diagnostic in a category no problem code declares.
fn in_category(mut diagnostic: Diagnostic, severity: Severity) -> Diagnostic {
    diagnostic.severity = severity;
    diagnostic
}

const ALL: [Severity; 7] = [
    Severity::FatalError,
    Severity::Error,
    Severity::Warning,
    Severity::Information,
    Severity::Text,
    Severity::SuppressedWarning,
    Severity::SuppressedInformation,
];

/// The settings with the code in the list.
fn with(list: WarningList, code: &str) -> MessageSettings {
    let mut settings = MessageSettings::default();
    let refused = settings.add_codes(list, [code]);
    assert!(refused.is_empty(), "{refused:?}");
    settings
}

fn disabled() -> MessageSettings {
    with(WarningList::Disabled, "P4083")
}

fn as_error() -> MessageSettings {
    with(WarningList::AsError, "P4083")
}

fn both() -> MessageSettings {
    let mut settings = disabled();
    settings.add(
        WarningList::AsError,
        WarningSet::from_code("C0354").unwrap(),
    );
    settings
}

#[spec_test(REQ_DS_dsl_030)]
fn keep_when_build_and_warning_disabled_then_it_is_not_kept_and_not_counted() {
    let kept = Area::Build.keep(&disabled(), vec![warning_at(1), error_at(2), warning_at(3)]);

    assert_eq!(kept.len(), 1);
    assert_eq!(kept[0].severity(), Severity::Error);
    assert_eq!(
        counts(&kept),
        Counts {
            errors: 1,
            warnings: 0
        }
    );
}

#[spec_test(REQ_DS_dsl_030)]
fn keep_when_build_and_warning_disabled_then_the_closing_line_does_not_count_it() {
    let closed = CHECK_SUMMARY.close("app", &disabled(), vec![warning_at(1), warning_at(2)]);

    assert_eq!(closed.len(), 1);
    assert_eq!(
        closed[0].person_text(),
        "Compile complete -- 0 errors, 0 warnings"
    );
}

#[spec_test(REQ_DS_dsl_030)]
fn keep_when_build_and_warning_raised_then_an_error_with_the_same_code_counted_as_an_error() {
    let before = warning_at(1);

    let kept = Area::Build.keep(&as_error(), vec![before.clone()]);

    assert_eq!(kept.len(), 1);
    assert_eq!(kept[0].severity(), Severity::Error);
    assert!(kept[0].stops_build());
    assert_eq!(kept[0].code, "P4083");
    assert_eq!(kept[0].description(), before.description());
    assert_eq!(kept[0].primary.message, before.primary.message);
    assert_eq!(
        counts(&kept),
        Counts {
            errors: 1,
            warnings: 0
        }
    );
}

#[spec_test(REQ_DS_dsl_030)]
fn keep_when_build_and_warning_raised_then_the_closing_line_counts_an_error() {
    let closed = CHECK_SUMMARY.close("app", &as_error(), vec![warning_at(1), warning_at(2)]);

    assert_eq!(
        closed[closed.len() - 1].person_text(),
        "Compile complete -- 2 errors, 0 warnings"
    );
}

#[spec_test(REQ_DS_dsl_030)]
fn keep_when_build_and_501_warnings_raised_then_they_pass_the_error_limit() {
    let produced: Vec<Diagnostic> = (0..501).map(warning_at).collect();

    let kept = Area::Build.keep(&as_error(), produced);

    // 500 errors and the line of the error limit, which is counted.
    assert_eq!(kept.len(), 501);
    assert_eq!(
        counts(&kept),
        Counts {
            errors: 501,
            warnings: 0
        }
    );
}

#[spec_test(REQ_DS_dsl_030)]
fn keep_when_build_and_warning_in_both_lists_then_raised_beats_disabled() {
    let kept = Area::Build.keep(&both(), vec![warning_at(1)]);

    assert_eq!(kept.len(), 1);
    assert_eq!(kept[0].severity(), Severity::Error);
}

#[spec_test(REQ_DS_dsl_030)]
fn keep_when_build_and_no_list_names_the_warning_then_every_message_stays_as_it_is() {
    let produced = vec![warning_at(1), error_at(2)];

    let kept = Area::Build.keep(&MessageSettings::default(), produced);

    assert_eq!(
        kept.iter().map(Diagnostic::severity).collect::<Vec<_>>(),
        vec![Severity::Warning, Severity::Error]
    );
}

#[spec_test(REQ_DS_dsl_030)]
fn apply_when_build_and_a_listed_code_in_another_category_then_the_warning_rule_does_not_apply() {
    for settings in [disabled(), as_error(), both()] {
        for severity in ALL {
            if severity == Severity::Warning {
                continue;
            }

            let kept = settings.apply(Area::Build, in_category(warning_at(1), severity));

            assert_eq!(kept.map(|d| d.severity()), Some(severity), "{severity:?}");
        }
    }
}

#[spec_test(REQ_DS_dsl_030)]
fn apply_when_message_has_no_code_then_it_is_not_in_a_list() {
    let line = Diagnostic::codeless(ironplc_problems::codeless::warning_limit_exceeded(500));

    for area in Area::ALL {
        let kept = both().apply(area, line.clone());

        assert_eq!(kept.map(|d| d.severity()), Some(Severity::Warning));
    }
}

#[spec_test(REQ_DS_dsl_031)]
fn keep_when_precompile_and_warning_disabled_then_it_is_removed() {
    let kept = Area::Precompile.keep(&disabled(), vec![warning_at(1), error_at(2)]);

    assert_eq!(kept.len(), 1);
    assert_eq!(kept[0].severity(), Severity::Error);
}

#[spec_test(REQ_DS_dsl_031)]
fn keep_when_precompile_and_warning_raised_then_it_stays_a_warning() {
    let kept = Area::Precompile.keep(&as_error(), vec![warning_at(1)]);

    assert_eq!(kept.len(), 1);
    assert_eq!(kept[0].severity(), Severity::Warning);
}

#[spec_test(REQ_DS_dsl_031)]
fn keep_when_precompile_and_warning_in_both_lists_then_raised_beats_disabled_and_it_stays() {
    let kept = Area::Precompile.keep(&both(), vec![warning_at(1)]);

    assert_eq!(kept.len(), 1);
    assert_eq!(kept[0].severity(), Severity::Warning);
}

#[spec_test(REQ_DS_dsl_032)]
fn apply_when_an_error_has_the_code_of_a_listed_warning_then_no_list_changes_it() {
    for settings in [disabled(), as_error(), both()] {
        for area in Area::ALL {
            for severity in [Severity::Error, Severity::FatalError] {
                let error = in_category(warning_at(1), severity);

                let kept = settings.apply(area, error);

                assert_eq!(kept.map(|d| d.severity()), Some(severity), "{area:?}");
            }
        }
    }
}

#[spec_test(REQ_DS_dsl_032)]
fn keep_when_applied_to_its_own_result_then_nothing_changes() {
    let produced = vec![warning_at(1), error_at(2), warning_at(3)];

    for settings in [MessageSettings::default(), disabled(), as_error(), both()] {
        for area in Area::ALL {
            let once = area.keep(&settings, produced.clone());
            let twice = area.keep(&settings, once.clone());

            assert_eq!(
                once.iter()
                    .map(|d| (d.code.clone(), d.severity()))
                    .collect::<Vec<_>>(),
                twice
                    .iter()
                    .map(|d| (d.code.clone(), d.severity()))
                    .collect::<Vec<_>>(),
                "{area:?}"
            );
        }
    }
}

/// Every member has an arm here, without a wildcard: a list added to
/// `WarningList` does not compile until the tests below are told about it.
fn is_declared(list: WarningList) -> bool {
    match list {
        WarningList::Disabled | WarningList::AsError => {
            !list.option_key().is_empty()
                && !list.cli_flag().is_empty()
                && !list.description().is_empty()
        }
    }
}

#[spec_test(REQ_DS_dsl_033)]
fn all_when_each_list_then_declared_once_with_its_own_key_and_flag() {
    for list in WarningList::ALL {
        assert!(is_declared(list));
        assert_eq!(
            WarningList::ALL
                .iter()
                .filter(|other| **other == list)
                .count(),
            1
        );
        for other in WarningList::ALL {
            if other != list {
                assert_ne!(list.option_key(), other.option_key());
                assert_ne!(list.cli_flag(), other.cli_flag());
            }
        }
    }
}

#[spec_test(REQ_DS_dsl_033)]
fn list_when_added_to_one_list_then_the_other_lists_are_unchanged() {
    let warnings = WarningSet::from_code("P4083").unwrap();

    for list in WarningList::ALL {
        let mut settings = MessageSettings::default();
        settings.add(list, warnings);

        for other in WarningList::ALL {
            let expected = if other == list {
                warnings
            } else {
                WarningSet::default()
            };
            assert_eq!(settings.list(other), expected, "{list:?} {other:?}");
        }
    }
}

#[test]
fn add_codes_when_some_codes_refused_then_the_rest_are_added_and_each_refusal_is_returned() {
    let mut settings = MessageSettings::default();

    let refused = settings.add_codes(WarningList::Disabled, ["C0354", "P4007", "nope"]);

    assert!(settings
        .list(WarningList::Disabled)
        .contains(WarningProblem::EnumerationTypesCompared));
    assert_eq!(refused.len(), 2);
    assert!(refused[0].message().contains("'P4007'"));
    assert!(refused[1].message().contains("'nope'"));
    assert!(settings.list(WarningList::AsError).is_empty());
}
