use super::*;
use crate::core::{FileId, SourceSpan};
use crate::diagnostic::Severity;
use ironplc_problems::{codeless, text};
use spec_test_macro::spec_test;

fn error_at(at: usize) -> Diagnostic {
    Diagnostic::text(
        SourceSpan::range(at, at + 1).with_file_id(&FileId::from_string("main.st")),
        text::variable_undefined(),
    )
}

fn warning_at(at: usize) -> Diagnostic {
    Diagnostic::text(
        SourceSpan::range(at, at + 1).with_file_id(&FileId::from_string("main.st")),
        text::enumeration_types_compared("E1", "E2"),
    )
}

fn errors(count: usize) -> Vec<Diagnostic> {
    (0..count).map(error_at).collect()
}

fn starts(diagnostics: &[Diagnostic]) -> Vec<usize> {
    diagnostics
        .iter()
        .map(|d| d.primary.location.start)
        .collect()
}

/// Every member has an arm here, without a wildcard: a member added to `Area`
/// does not compile until the tests below are told about it.
fn is_declared(area: Area) -> bool {
    match area {
        Area::Build | Area::Precompile => {
            !area.name().is_empty() && !area.source().is_empty() && !area.description().is_empty()
        }
    }
}

#[spec_test(REQ_DS_dsl_024)]
fn all_when_listed_then_every_member_once_at_its_index() {
    for (position, area) in Area::ALL.iter().enumerate() {
        assert_eq!(area.index(), position, "{area:?}");
    }
    // The indexes are 0..n, so no member is missing from the list: the next
    // member of the enum gets the index n, which `index` forces it to declare.
    let highest = Area::ALL.iter().map(|area| area.index()).max().unwrap();
    assert_eq!(highest + 1, Area::ALL.len());
}

#[spec_test(REQ_DS_dsl_024)]
fn name_and_source_when_each_area_then_declared_and_distinct() {
    for area in Area::ALL {
        assert!(is_declared(area), "{area:?}");
    }
    let names: Vec<_> = Area::ALL.iter().map(|area| area.name()).collect();
    let sources: Vec<_> = Area::ALL.iter().map(|area| area.source()).collect();
    assert_eq!(names, ["Build", "Precompile"]);
    assert_eq!(sources, ["IronPLC Build", "IronPLC Precompile"]);
}

#[spec_test(REQ_DS_dsl_025)]
fn shown_in_when_message_has_a_position_then_every_area() {
    let diagnostic = error_at(0);

    for area in Area::ALL {
        assert!(diagnostic.shown_in(area), "{area:?}");
    }
}

#[spec_test(REQ_DS_dsl_025)]
fn shown_in_when_message_has_no_position_then_the_build_area_alone() {
    for diagnostic in [
        Diagnostic::codeless(codeless::error_limit_exceeded(500)),
        Diagnostic::codeless(codeless::compile_complete_ok(0, 0)),
    ] {
        assert!(diagnostic.shown_in(Area::Build));
        assert!(!diagnostic.shown_in(Area::Precompile));
    }
}

#[spec_test(REQ_DS_dsl_026)]
fn keep_when_build_area_then_the_rule_of_the_build() {
    let mut produced = errors(501);
    produced.push(error_at(3));

    let stays = Area::Build.keep(&MessageSettings::default(), produced);

    // 500 errors, the line of the limit, and the repeat of the error at 3 gone.
    assert_eq!(stays.len(), 501);
    assert!(!stays[500].has_code());
    assert!(stays[500].shown_in(Area::Build));
}

#[spec_test(REQ_DS_dsl_027)]
fn keep_when_precompile_area_and_message_without_a_position_then_dropped() {
    let produced = vec![
        error_at(1),
        Diagnostic::codeless(codeless::error_limit_exceeded(500)),
        Diagnostic::codeless(codeless::compile_complete_errors(1, 0)),
        error_at(2),
    ];

    let stays = Area::Precompile.keep(&MessageSettings::default(), produced);

    assert_eq!(starts(&stays), [1, 2]);
    assert!(stays.iter().all(Diagnostic::has_code));
}

#[spec_test(REQ_DS_dsl_027)]
fn keep_when_precompile_area_and_suppressed_message_then_dropped() {
    let produced = vec![error_at(1), warning_at(2).suppress(), warning_at(3)];

    let stays = Area::Precompile.keep(&MessageSettings::default(), produced);

    assert_eq!(starts(&stays), [1, 3]);
    assert!(stays.iter().all(|d| d.severity().is_shown()));
}

#[spec_test(REQ_DS_dsl_027)]
fn keep_when_precompile_area_and_repeated_message_then_the_first_stays() {
    let stays = Area::Precompile.keep(
        &MessageSettings::default(),
        vec![error_at(5), error_at(9), error_at(5)],
    );

    assert_eq!(starts(&stays), [5, 9]);
}

#[spec_test(REQ_DS_dsl_027)]
fn keep_when_precompile_area_and_over_500_errors_then_all_stay_and_no_line() {
    let stays = Area::Precompile.keep(&MessageSettings::default(), errors(501));

    assert_eq!(stays.len(), 501);
    assert!(stays.iter().all(Diagnostic::has_code));
}

#[spec_test(REQ_DS_dsl_027)]
fn keep_when_precompile_area_and_fatal_error_then_the_other_messages_stay() {
    let mut fatal = error_at(0);
    fatal.severity = Severity::FatalError;

    let stays = Area::Precompile.keep(
        &MessageSettings::default(),
        vec![error_at(1), fatal, warning_at(2)],
    );

    assert_eq!(starts(&stays), [1, 0, 2]);
}

#[spec_test(REQ_DS_dsl_027)]
fn keep_when_precompile_area_applied_to_its_own_result_then_unchanged() {
    let once = Area::Precompile.keep(
        &MessageSettings::default(),
        vec![error_at(1), error_at(1), warning_at(2).suppress()],
    );
    let twice = Area::Precompile.keep(&MessageSettings::default(), once.clone());

    assert_eq!(starts(&once), starts(&twice));
}

#[spec_test(REQ_DS_dsl_028)]
fn capacity_when_each_area_then_the_precompile_area_alone_has_one() {
    for area in Area::ALL {
        match area {
            Area::Build => assert_eq!(area.capacity(), None),
            Area::Precompile => assert_eq!(area.capacity(), Some(950)),
        }
    }
}

#[spec_test(REQ_DS_dsl_028)]
fn store_when_an_object_has_more_than_the_capacity_then_its_first_stay() {
    let stored = Area::Precompile.store(vec![("a", (0..1200).collect::<Vec<u32>>())]);

    assert_eq!(stored.len(), 1);
    assert_eq!(stored[0].1.len(), 950);
    assert_eq!(stored[0].1[0], 0);
    assert_eq!(stored[0].1[949], 949);
}

#[spec_test(REQ_DS_dsl_028)]
fn store_when_the_total_is_over_the_capacity_then_the_oldest_are_removed_first() {
    // Written in this order: "a" is the oldest, "c" the newest.
    let written = vec![
        ("a", (0..400).collect::<Vec<u32>>()),
        ("b", (1000..1400).collect::<Vec<u32>>()),
        ("c", (2000..2300).collect::<Vec<u32>>()),
    ];

    let stored = Area::Precompile.store(written);

    // 1100 messages: the 150 oldest, which are the first of "a", go.
    let lengths: Vec<usize> = stored.iter().map(|(_, m)| m.len()).collect();
    assert_eq!(lengths, [250, 400, 300]);
    assert_eq!(stored[0].1[0], 150);
    assert_eq!(stored[2].1.len(), 300);
}

#[spec_test(REQ_DS_dsl_028)]
fn store_when_the_oldest_objects_are_removed_whole_then_the_newer_stay() {
    let written = vec![
        ("a", (0..300).collect::<Vec<u32>>()),
        ("b", (0..300).collect::<Vec<u32>>()),
        ("c", (0..950).collect::<Vec<u32>>()),
    ];

    let stored = Area::Precompile.store(written);

    let lengths: Vec<usize> = stored.iter().map(|(_, m)| m.len()).collect();
    assert_eq!(lengths, [0, 0, 950]);
}

#[spec_test(REQ_DS_dsl_028)]
fn store_when_the_total_is_within_the_capacity_then_unchanged() {
    let stored = Area::Precompile.store(vec![("a", vec![1, 2]), ("b", vec![3])]);

    assert_eq!(stored, [("a", vec![1, 2]), ("b", vec![3])]);
}

#[spec_test(REQ_DS_dsl_028)]
fn store_when_the_area_has_no_capacity_then_everything_is_stored() {
    let stored = Area::Build.store(vec![("a", (0..2000).collect::<Vec<u32>>())]);

    assert_eq!(stored[0].1.len(), 2000);
}
