//! Salsa adapter checks for design section 3.3: memoization, recomputation on
//! input change, and backdating of equal summaries so consumers stop
//! propagating unchanged meaning.
//!
//! Re-execution is observed through Salsa's own events
//! (`WillExecute` / `DidValidateMemoizedValue`), recorded by the spike
//! database, not by instrumenting the queries.

use ironplc_s0_spike::db::{declaration_summary, interface_fingerprint, Database};

const V1: &str = "PROGRAM P\nVAR\n  speed : REAL; (* target speed *)\n  mode : INT;\nEND_VAR\nspeed := 1.0;\nEND_PROGRAM\n";

/// Same declarations; whitespace/comment-only change.
const V2_LAYOUT: &str = "PROGRAM P\n\nVAR\n\tspeed : REAL;   (* reworded comment *)\n\tmode : INT;\nEND_VAR\n\nspeed := 2.0;\nEND_PROGRAM\n";

/// A meaning change: `mode` gets a different type.
const V3_TYPE_CHANGE: &str =
    "PROGRAM P\nVAR\n  speed : REAL;\n  mode : DINT;\nEND_VAR\nspeed := 1.0;\nEND_PROGRAM\n";

#[test]
fn salsa_when_input_unchanged_then_query_is_memoized() {
    let db = Database::default();
    let file = db.add_source(V1);

    let first = declaration_summary(&db, file);
    let _ = db.take_events();

    let second = declaration_summary(&db, file);
    assert_eq!(first, second);
    assert_eq!(
        db.count_events("WillExecute"),
        0,
        "memoized query must not re-execute"
    );
    // No revision has advanced, so Salsa returns the memo directly; the
    // validation event appears when a later revision is checked (see the
    // whitespace-change test).
}

#[test]
fn salsa_when_whitespace_changes_then_summary_recomputes_but_stops_propagation() {
    let mut db = Database::default();
    let file = db.add_source(V1);

    let summary_before = declaration_summary(&db, file);
    let fingerprint_before = interface_fingerprint(&db, file);
    let _ = db.take_events();

    db.set_source(file, V2_LAYOUT);
    let summary_after = declaration_summary(&db, file);
    assert_eq!(
        db.count_events("WillExecute"),
        1,
        "input change must re-execute the summary query"
    );
    assert_eq!(
        summary_after, summary_before,
        "layout-only change keeps the declaration summary"
    );

    let _ = db.take_events();
    let fingerprint_after = interface_fingerprint(&db, file);
    assert_eq!(fingerprint_after, fingerprint_before);
    assert_eq!(
        db.count_events("WillExecute"),
        0,
        "backdated summary must not re-execute its consumer"
    );
    assert!(
        db.count_events("DidValidateMemoizedValue") >= 1,
        "consumer should be validated from cache"
    );
}

#[test]
fn salsa_when_declaration_changes_then_consumer_recomputes() {
    let mut db = Database::default();
    let file = db.add_source(V1);

    let fingerprint_before = interface_fingerprint(&db, file);
    db.set_source(file, V3_TYPE_CHANGE);
    let _ = db.take_events();

    let fingerprint_after = interface_fingerprint(&db, file);
    assert_ne!(fingerprint_after, fingerprint_before);
    assert!(
        db.count_events("WillExecute") >= 1,
        "a meaning change must re-execute the summary and its consumer"
    );
}

#[test]
fn salsa_when_renamed_pou_then_summary_reports_new_name() {
    let mut db = Database::default();
    let file = db.add_source(V1);
    assert_eq!(
        declaration_summary(&db, file).pou_name.as_deref(),
        Some("P")
    );

    db.set_source(
        file,
        "FUNCTION_BLOCK Mixer\nVAR\n  speed : REAL;\nEND_VAR\nEND_FUNCTION_BLOCK\n",
    );
    let summary = declaration_summary(&db, file);
    assert_eq!(summary.pou_kind.as_deref(), Some("FUNCTION_BLOCK"));
    assert_eq!(summary.pou_name.as_deref(), Some("Mixer"));
    assert_eq!(summary.variables.len(), 1);
}
