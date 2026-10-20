use super::*;
use crate::core::SourceSpan;
use ironplc_problems::text;
use spec_test_macro::spec_test;

/// A diagnostic in a category no problem code declares, which only a test
/// reaches (the field is private to the module of `Diagnostic`).
fn in_category(severity: Severity) -> Diagnostic {
    let mut diagnostic = Diagnostic::text(
        SourceSpan::default(),
        text::syntax_error_a_task_needs_a_priority(),
    );
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

/// What a handling does to a message: it stays as it is, it stays in another
/// category, or it is dropped.
#[derive(Debug, PartialEq, Eq)]
enum Outcome {
    Stays,
    StaysAs(Severity),
    Dropped,
}

/// The table of the design, a `match` over the handling and the category with no
/// wildcard arm: a handling or a category added to the lists does not compile
/// here until the test knows what it does.
fn expected(handling: MessageHandling, severity: Severity) -> Outcome {
    match handling {
        MessageHandling::Report => match severity {
            Severity::FatalError
            | Severity::Error
            | Severity::Warning
            | Severity::Information
            | Severity::Text
            | Severity::SuppressedWarning
            | Severity::SuppressedInformation => Outcome::Stays,
        },
        MessageHandling::Suppress => match severity {
            Severity::FatalError | Severity::Error => Outcome::Dropped,
            Severity::Warning => Outcome::StaysAs(Severity::SuppressedWarning),
            Severity::Information => Outcome::StaysAs(Severity::SuppressedInformation),
            Severity::Text | Severity::SuppressedWarning | Severity::SuppressedInformation => {
                Outcome::Stays
            }
        },
        MessageHandling::Ignore => match severity {
            Severity::FatalError
            | Severity::Error
            | Severity::Warning
            | Severity::Information
            | Severity::Text
            | Severity::SuppressedWarning
            | Severity::SuppressedInformation => Outcome::Dropped,
        },
    }
}

#[spec_test(REQ_DS_dsl_029)]
fn handle_when_each_handling_and_each_category_then_the_result_of_the_table() {
    for handling in MessageHandling::ALL {
        for severity in ALL {
            let handled = in_category(severity).handle(handling);

            match expected(handling, severity) {
                Outcome::Dropped => assert!(handled.is_none(), "{handling:?} {severity:?}"),
                Outcome::Stays => assert_eq!(
                    handled.map(|d| d.severity()),
                    Some(severity),
                    "{handling:?} {severity:?}"
                ),
                Outcome::StaysAs(other) => assert_eq!(
                    handled.map(|d| d.severity()),
                    Some(other),
                    "{handling:?} {severity:?}"
                ),
            }
        }
    }
}

#[spec_test(REQ_DS_dsl_029)]
fn handle_when_kept_then_nothing_but_the_category_changes() {
    for handling in MessageHandling::ALL {
        for severity in ALL {
            let before = in_category(severity);
            let Some(after) = before.clone().handle(handling) else {
                continue;
            };

            assert_eq!(after.code, before.code);
            assert_eq!(after.description(), before.description());
            assert_eq!(after.primary.message, before.primary.message);
        }
    }
}

#[spec_test(REQ_DS_dsl_029)]
fn handle_when_suppress_then_it_is_the_change_of_category_suppress_makes() {
    for severity in ALL {
        let suppressed = in_category(severity).suppress().severity();

        match in_category(severity).handle(MessageHandling::Suppress) {
            Some(handled) => assert_eq!(handled.severity(), suppressed, "{severity:?}"),
            None => assert!(severity.stops_build(), "{severity:?}"),
        }
    }
}

#[test]
fn all_when_listed_then_each_handling_once() {
    assert_eq!(MessageHandling::ALL.len(), 3);
    for handling in MessageHandling::ALL {
        assert_eq!(
            MessageHandling::ALL
                .iter()
                .filter(|other| **other == handling)
                .count(),
            1
        );
    }
}
