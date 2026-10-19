//! Tests for the hot-edit tool.

use super::*;
use crate::tools::test_support::{ed2_options, source, COUNTER_PROGRAM, SYNTAX_ERROR_PROGRAM};
use ironplc_container::VarIndex;
use ironplc_runtime::HostMode;

fn make_state() -> (Mutex<ContainerCache>, Mutex<HotEditSession>) {
    (
        Mutex::new(ContainerCache::new(64, 64 * 1024 * 1024)),
        Mutex::new(HotEditSession::default()),
    )
}

/// `COUNTER_PROGRAM` with the scan body replaced — the declarations are
/// untouched, so two compiles share a layout and differ only in logic.
fn counter_variant(body: &str) -> String {
    format!(
        "PROGRAM Main
VAR
  Counter : INT;
END_VAR
  {body}
END_PROGRAM"
    )
}

/// Compiles `source` through the project pipeline and round-trips the
/// wire format, mirroring `runtime/tests/common`.
fn compile_container(source: &str) -> Container {
    compile_container_with_ids(source, &[])
}

/// [`compile_container`] with an engineering-side stable variable ID
/// table (ADR-0053), so a declaration edit stages as a migration
/// candidate. The tool surface has no way to supply IDs — they arrive
/// with PLCopen UID storage (roadmap Phase 3+) — so the schema-edit
/// scenario establishes its session directly instead of through
/// `build_accept_response`.
fn compile_container_with_ids(source: &str, ids: &[(&str, u64)]) -> Container {
    use ironplc_parser::options::CompilerOptions;
    use ironplc_project::{compile, MemoryBackedProject};

    let options = CompilerOptions::default();
    let mut project = MemoryBackedProject::new(options);
    project.add_source(
        ironplc_dsl::core::FileId::from_string("main.st"),
        source.to_owned(),
    );
    if !ids.is_empty() {
        project.set_stable_var_ids(
            ids.iter()
                .map(|(name, uid)| (ironplc_project::SidecarKey::new("main", name), *uid))
                .collect(),
        );
    }

    let output = compile(
        &mut project,
        &options,
        &ironplc_codegen::EmptyLookup,
        vec![],
    );
    assert!(
        output.diagnostics.is_empty(),
        "fixture must compile: {:?}",
        output.diagnostics
    );

    let container = output.container.unwrap();
    let mut bytes = Vec::new();
    container.write_to(&mut bytes).unwrap();
    Container::read_from(&mut Cursor::new(&bytes)).unwrap()
}

/// Finds a variable's table index through the debug section.
fn variable_index(container: &Container, name: &str) -> VarIndex {
    container
        .debug_section
        .as_ref()
        .unwrap()
        .var_names
        .iter()
        .find(|entry| entry.name.eq_ignore_ascii_case(name))
        .unwrap()
        .var_index
}

/// Establishes a session from `COUNTER_PROGRAM`, leaving no candidate.
fn establish_counter_session(cache: &Mutex<ContainerCache>, session: &Mutex<HotEditSession>) {
    let resp = build_accept_response(&source(COUNTER_PROGRAM), &ed2_options(), cache, session);
    assert!(resp.ok, "diagnostics: {:?}", resp.diagnostics);
    assert_eq!(resp.result.as_deref(), Some("established"));
}

/// Establishes a session and stages `body` as a logic-only candidate.
fn session_with_staged_candidate(
    cache: &Mutex<ContainerCache>,
    session: &Mutex<HotEditSession>,
    body: &str,
) {
    establish_counter_session(cache, session);
    let edit = counter_variant(body);
    let resp = build_accept_response(&source(&edit), &ed2_options(), cache, session);
    assert!(
        resp.ok,
        "v_code: {:?} message: {:?}",
        resp.v_code, resp.message
    );
}

#[test]
fn build_accept_response_when_no_session_then_establishes_running_application() {
    let (cache, session) = make_state();

    let resp = build_accept_response(&source(COUNTER_PROGRAM), &ed2_options(), &cache, &session);

    assert!(resp.ok, "diagnostics: {:?}", resp.diagnostics);
    assert_eq!(resp.result.as_deref(), Some("established"));
    let status = resp.status.unwrap();
    assert_eq!(status.mode, HostMode::Normal);
    assert_eq!(status.active, 1);
    assert_eq!(status.normal, 1);
    assert_eq!(status.candidate, None);
    assert_eq!(status.rounds, 0);
}

#[test]
fn send_candidate_when_ids_are_out_of_sync_then_not_sent_and_no_session() {
    let (_, session) = make_state();
    let program = {
        let container = compile_container(COUNTER_PROGRAM);
        let mut bytes = Vec::new();
        container.write_to(&mut bytes).unwrap();
        bytes
    };

    let resp = send_candidate(
        program,
        Some("the stable variable IDs are out of sync with the declarations".to_string()),
        &BTreeMap::new(),
        &session,
    );

    assert!(!resp.ok);
    assert!(resp.v_code.is_none());
    assert_eq!(
        resp.message.as_deref(),
        Some("the stable variable IDs are out of sync with the declarations")
    );
    assert!(session.lock().unwrap().host.is_none());
}
#[test]
fn build_accept_response_when_session_running_then_stages_candidate() {
    let (cache, session) = make_state();
    establish_counter_session(&cache, &session);

    let edit = counter_variant("Counter := Counter + 10;");
    let resp = build_accept_response(&source(&edit), &ed2_options(), &cache, &session);

    assert!(resp.ok);
    assert_eq!(resp.result.as_deref(), Some("ack"));
    let status = resp.status.unwrap();
    assert_eq!(status.mode, HostMode::Normal);
    assert_eq!(status.candidate, Some(2));
}

#[test]
fn build_accept_response_when_syntax_error_then_compile_diagnostics_and_no_session() {
    let (cache, session) = make_state();

    let resp = build_accept_response(
        &source(SYNTAX_ERROR_PROGRAM),
        &ed2_options(),
        &cache,
        &session,
    );

    assert!(!resp.ok);
    assert!(resp.v_code.is_none());
    assert!(!resp.diagnostics.is_empty());
    assert!(resp.diagnostics.iter().all(|d| d["prefix"] == "P"));
    // Nothing was established: the next status call still finds no session.
    let status = build_status_response(&session);
    assert!(!status.ok);
    assert!(status.diagnostics.iter().any(|d| d["number"] == 8001));
}

#[test]
fn build_accept_response_when_layout_change_without_stable_ids_then_v4007() {
    let (cache, session) = make_state();
    establish_counter_session(&cache, &session);

    // Adding a variable is a declaration edit: without stable variable
    // IDs the host cannot justify carrying state over, so the stage is
    // refused (with IDs it would stage as a migration candidate).
    let schema_edit = "PROGRAM Main
VAR
  Counter : INT;
  Extra : INT;
END_VAR
  Counter := Counter + 1;
END_PROGRAM";
    let resp = build_accept_response(&source(schema_edit), &ed2_options(), &cache, &session);

    assert!(!resp.ok);
    assert_eq!(resp.v_code.as_deref(), Some("V4007"));
    assert!(resp.message.unwrap().contains("layout"));
}

#[test]
fn build_accept_response_when_candidate_already_staged_then_v4013() {
    let (cache, session) = make_state();
    session_with_staged_candidate(&cache, &session, "Counter := Counter + 10;");

    let second = counter_variant("Counter := Counter + 20;");
    let resp = build_accept_response(&source(&second), &ed2_options(), &cache, &session);

    assert!(!resp.ok);
    assert_eq!(resp.v_code.as_deref(), Some("V4013"));
    // The first candidate is still the one that is staged.
    let status = build_status_response(&session);
    assert_eq!(status.status.unwrap().candidate, Some(2));
}

#[test]
fn build_status_response_when_no_session_then_p8001_diagnostic() {
    let (_cache, session) = make_state();

    let resp = build_status_response(&session);

    assert!(!resp.ok);
    assert!(resp.diagnostics.iter().any(|d| d["number"] == 8001));
}

#[test]
fn build_test_response_when_no_session_then_p8001_diagnostic() {
    let (_cache, session) = make_state();

    let resp = build_test_response(&session);

    assert!(!resp.ok);
    assert!(resp.diagnostics.iter().any(|d| d["number"] == 8001));
}

#[test]
fn build_test_response_when_no_candidate_staged_then_v4012() {
    let (cache, session) = make_state();
    establish_counter_session(&cache, &session);

    let resp = build_test_response(&session);

    assert!(!resp.ok);
    assert_eq!(resp.v_code.as_deref(), Some("V4012"));
    assert_eq!(resp.message.unwrap(), "no candidate is staged");
}

#[test]
fn build_test_response_when_candidate_staged_then_swap_applies_at_scan_boundary() {
    let (cache, session) = make_state();
    session_with_staged_candidate(&cache, &session, "Counter := Counter + 10;");

    let resp = build_test_response(&session);

    assert!(resp.ok);
    assert_eq!(resp.result.as_deref(), Some("ack"));
    let status = resp.status.unwrap();
    assert_eq!(status.mode, HostMode::Testing);
    assert_eq!(status.active, 2);
    assert_eq!(status.candidate, Some(2));
    // The tool drove exactly one scan round: the swap was applied and the
    // candidate's logic executed (Counter went 0 -> 10).
    assert_eq!(status.rounds, 1);
    let guard = session.lock().unwrap();
    let container = compile_container(&counter_variant("Counter := Counter + 10;"));
    let index = variable_index(&container, "Counter");
    assert_eq!(
        guard.host.as_ref().unwrap().read_variable(index).unwrap(),
        10
    );
}

#[test]
fn build_untest_response_when_test_in_progress_then_reverts_code_and_keeps_state() {
    let (cache, session) = make_state();
    session_with_staged_candidate(&cache, &session, "Counter := Counter + 10;");
    let tested = build_test_response(&session);
    assert!(tested.ok);

    let resp = build_untest_response(&session);

    assert!(resp.ok);
    let status = resp.status.unwrap();
    assert_eq!(status.mode, HostMode::Normal);
    assert_eq!(status.active, 1);
    // The second driven round: untest swapped back and the base artifact
    // executed once more on top of the candidate's state (10 -> 11).
    assert_eq!(status.rounds, 2);
    let guard = session.lock().unwrap();
    let container = compile_container(COUNTER_PROGRAM);
    let index = variable_index(&container, "Counter");
    assert_eq!(
        guard.host.as_ref().unwrap().read_variable(index).unwrap(),
        11
    );
}

#[test]
fn build_untest_response_when_no_test_in_progress_then_v4014() {
    let (cache, session) = make_state();
    session_with_staged_candidate(&cache, &session, "Counter := Counter + 10;");

    let resp = build_untest_response(&session);

    assert!(!resp.ok);
    assert_eq!(resp.v_code.as_deref(), Some("V4014"));
    assert_eq!(resp.message.unwrap(), "no test is in progress");
}

#[test]
fn build_untest_response_when_schema_edit_tested_then_v4011_and_candidate_keeps_running() {
    // The tool surface cannot supply stable variable IDs (PLCopen UID
    // storage is roadmap Phase 3+), so this scenario establishes the
    // session and stages the migration candidate through the command
    // layer directly — the same calls `build_accept_response` makes
    // internally once the compiled bytes exist.
    let (_cache, session) = make_state();
    let base = compile_container_with_ids(
        "PROGRAM main
  VAR
Counter : DINT;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
        &[("Counter", 1)],
    );
    let schema_edit = compile_container_with_ids(
        "PROGRAM main
  VAR
Counter : DINT;
Extra : DINT := 42;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
        &[("Counter", 1), ("Extra", 2)],
    );
    {
        let mut guard = session.lock().unwrap();
        let mut host = RuntimeHost::new(base).unwrap();
        host.permit_execution();
        guard.host = Some(host);
    }
    let staged = {
        let mut guard = session.lock().unwrap();
        let mut bytes = Vec::new();
        schema_edit.write_to(&mut bytes).unwrap();
        execute(
            Command::AcceptEdits {
                program: bytes,
                edit: None,
                migration: BTreeMap::new(),
            },
            guard.host.as_mut().unwrap(),
            &mcp_device_identity(),
        )
    };
    assert!(matches!(staged, Response::Ack));
    let tested = build_test_response(&session);
    assert!(
        tested.ok,
        "v_code: {:?} message: {:?}",
        tested.v_code, tested.message
    );
    assert!(tested.status.unwrap().migration);

    let resp = build_untest_response(&session);

    assert!(!resp.ok);
    assert_eq!(resp.v_code.as_deref(), Some("V4011"));
    assert!(resp.message.unwrap().contains("schema"));
    // The refusal leaves the candidate active.
    let status = build_status_response(&session);
    let status = status.status.unwrap();
    assert_eq!(status.mode, HostMode::Testing);
    assert_eq!(status.candidate, Some(2));
}

#[test]
fn hot_edit_accept_input_when_migration_map_then_parses_decisions() {
    let input: HotEditAcceptInput = serde_json::from_value(serde_json::json!({
        "sources": [{"name": "main.st", "content": "PROGRAM main END_PROGRAM"}],
        "options": {},
        "migration": {"7": "init", "9": "preserve"},
    }))
    .unwrap();

    assert_eq!(
        input.migration,
        BTreeMap::from([
            (7, MigrationDecisionSpec::Init),
            (9, MigrationDecisionSpec::Preserve),
        ])
    );
}

#[test]
fn migration_schema_when_rendered_then_decision_values_are_discoverable() {
    let schema = migration_schema(&mut schemars::SchemaGenerator::default());

    let value = serde_json::to_value(&schema).unwrap();

    assert_eq!(value["type"], serde_json::json!("object"));
    assert_eq!(
        value["additionalProperties"]["enum"],
        serde_json::json!(["init", "preserve"])
    );
}

#[test]
fn hot_edit_accept_input_when_migration_absent_then_empty() {
    let input: HotEditAcceptInput = serde_json::from_value(serde_json::json!({
        "sources": [{"name": "main.st", "content": "PROGRAM main END_PROGRAM"}],
        "options": {},
    }))
    .unwrap();

    assert!(input.migration.is_empty());
}

#[test]
fn dispatch_when_type_change_without_decisions_then_v4010_and_pairs() {
    let (_cache, session) = make_state();
    let base = compile_container_with_ids(
        "PROGRAM main
  VAR
Counter : DINT;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
        &[("Counter", 1)],
    );
    {
        let mut guard = session.lock().unwrap();
        let mut host = RuntimeHost::new(base).unwrap();
        host.permit_execution();
        guard.host = Some(host);
    }
    let candidate = compile_container_with_ids(
        "PROGRAM main
  VAR
Counter : UDINT;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
        &[("Counter", 1)],
    );
    let mut bytes = Vec::new();
    candidate.write_to(&mut bytes).unwrap();

    let mut guard = session.lock().unwrap();
    let resp = dispatch(
        Command::AcceptEdits {
            program: bytes,
            edit: None,
            migration: BTreeMap::new(),
        },
        &mut guard,
        false,
    );

    assert!(!resp.ok);
    assert_eq!(resp.v_code.as_deref(), Some("V4010"));
    assert_eq!(
        resp.pairs,
        Some(vec![TypeChangeDetail {
            uid: 1,
            name: Some("Counter".to_string()),
            from: "I32".to_string(),
            to: "U32".to_string(),
            size_equal: true,
        }])
    );
    // The refusal staged nothing: the client can resubmit with a decision.
    assert!(guard.host.as_ref().unwrap().status().candidate.is_none());
}

#[test]
fn dispatch_when_preserve_decision_then_old_bits_survive_the_retype() {
    let (_cache, session) = make_state();
    let base = compile_container_with_ids(
        "PROGRAM main
  VAR
Counter : DINT;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
        &[("Counter", 1)],
    );
    {
        let mut guard = session.lock().unwrap();
        let mut host = RuntimeHost::new(base).unwrap();
        host.permit_execution();
        guard.host = Some(host);
        guard.host.as_mut().unwrap().run(3, || 0).unwrap();
    }
    let candidate = compile_container_with_ids(
        "PROGRAM main
  VAR
Counter : UDINT;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
        &[("Counter", 1)],
    );
    let counter = variable_index(&candidate, "Counter");
    let mut bytes = Vec::new();
    candidate.write_to(&mut bytes).unwrap();

    let mut guard = session.lock().unwrap();
    let staged = dispatch(
        Command::AcceptEdits {
            program: bytes,
            edit: None,
            migration: BTreeMap::from([(1, MigrationDecisionSpec::Preserve)]),
        },
        &mut guard,
        false,
    );
    assert!(
        staged.ok,
        "v_code: {:?} message: {:?}",
        staged.v_code, staged.message
    );
    let tested = dispatch(Command::TestEdits, &mut guard, true);
    assert!(tested.ok);

    // The DINT 3 (0x3) was preserved bit for bit; the UDINT body added
    // one more.
    let value = guard.host.as_ref().unwrap().read_variable(counter).unwrap();
    assert_eq!(value, 4);
}

#[test]
fn dispatch_when_init_decision_then_candidate_initial_value_stands() {
    let (_cache, session) = make_state();
    let base = compile_container_with_ids(
        "PROGRAM main
  VAR
Counter : DINT;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
        &[("Counter", 1)],
    );
    {
        let mut guard = session.lock().unwrap();
        let mut host = RuntimeHost::new(base).unwrap();
        host.permit_execution();
        guard.host = Some(host);
        guard.host.as_mut().unwrap().run(3, || 0).unwrap();
    }
    let candidate = compile_container_with_ids(
        "PROGRAM main
  VAR
Counter : UDINT := 100;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
",
        &[("Counter", 1)],
    );
    let counter = variable_index(&candidate, "Counter");
    let mut bytes = Vec::new();
    candidate.write_to(&mut bytes).unwrap();

    let mut guard = session.lock().unwrap();
    let staged = dispatch(
        Command::AcceptEdits {
            program: bytes,
            edit: None,
            migration: BTreeMap::from([(1, MigrationDecisionSpec::Init)]),
        },
        &mut guard,
        false,
    );
    assert!(
        staged.ok,
        "v_code: {:?} message: {:?}",
        staged.v_code, staged.message
    );
    let tested = dispatch(Command::TestEdits, &mut guard, true);
    assert!(tested.ok);

    // The old 3 was discarded; the candidate's declared 100 initialized
    // the variable before the candidate body ran.
    let value = guard.host.as_ref().unwrap().read_variable(counter).unwrap();
    assert_eq!(value, 101);
}

#[test]
fn build_assemble_response_when_candidate_staged_then_candidate_becomes_normal() {
    let (cache, session) = make_state();
    session_with_staged_candidate(&cache, &session, "Counter := Counter + 10;");
    // ADR-0064: assemble requires the candidate to have run under Test.
    let tested = build_test_response(&session);
    assert!(tested.ok);

    let resp = build_assemble_response(&session);

    assert!(resp.ok);
    let status = resp.status.unwrap();
    assert_eq!(status.mode, HostMode::Normal);
    assert_eq!(status.normal, 2);
    assert_eq!(status.application, 2);
    assert_eq!(status.candidate, None);
    // The promoted candidate is what runs now: the test and assemble
    // responses each drove one round (0 -> 10 -> 20).
    let guard = session.lock().unwrap();
    let container = compile_container(&counter_variant("Counter := Counter + 10;"));
    let index = variable_index(&container, "Counter");
    assert_eq!(
        guard.host.as_ref().unwrap().read_variable(index).unwrap(),
        20
    );
}

#[test]
fn build_assemble_response_when_no_candidate_staged_then_v4012() {
    let (cache, session) = make_state();
    establish_counter_session(&cache, &session);

    let resp = build_assemble_response(&session);

    assert!(!resp.ok);
    assert_eq!(resp.v_code.as_deref(), Some("V4012"));
}

#[test]
fn build_cancel_response_when_candidate_staged_then_discards_candidate() {
    let (cache, session) = make_state();
    session_with_staged_candidate(&cache, &session, "Counter := Counter + 10;");

    let resp = build_cancel_response(&session);

    assert!(resp.ok);
    let status = resp.status.unwrap();
    assert_eq!(status.mode, HostMode::Normal);
    assert_eq!(status.normal, 1);
    assert_eq!(status.candidate, None);
}

#[test]
fn build_cancel_response_while_testing_then_v4015() {
    let (cache, session) = make_state();
    session_with_staged_candidate(&cache, &session, "Counter := Counter + 10;");
    let tested = build_test_response(&session);
    assert!(tested.ok);

    let resp = build_cancel_response(&session);

    assert!(!resp.ok);
    assert_eq!(resp.v_code.as_deref(), Some("V4015"));
}

#[test]
fn build_test_response_when_candidate_traps_on_first_scan_then_trap_v_code() {
    let (cache, session) = make_state();
    // `1 / Counter` divides by zero on the first candidate scan. The
    // driven boundary round traps, and the trap's own V-code comes back.
    session_with_staged_candidate(&cache, &session, "Counter := 1 / Counter;");

    let resp = build_test_response(&session);

    assert!(!resp.ok);
    assert_eq!(resp.v_code.as_deref(), Some("V4001"));
    assert!(resp.message.unwrap().contains("divide"));
}

#[test]
fn build_flow_accept_test_assemble_then_candidate_is_the_running_application() {
    let (cache, session) = make_state();

    let established =
        build_accept_response(&source(COUNTER_PROGRAM), &ed2_options(), &cache, &session);
    assert_eq!(established.result.as_deref(), Some("established"));
    let edit = counter_variant("Counter := Counter + 10;");
    let staged = build_accept_response(&source(&edit), &ed2_options(), &cache, &session);
    assert!(staged.ok);
    let tested = build_test_response(&session);
    assert!(tested.ok);
    let assembled = build_assemble_response(&session);

    assert!(assembled.ok);
    let status = assembled.status.unwrap();
    assert_eq!(status.mode, HostMode::Normal);
    assert_eq!(status.normal, 2);
    assert_eq!(status.application, 2);
    assert_eq!(status.candidate, None);
}

#[test]
fn dispatch_when_accept_payload_is_not_a_container_then_v4016() {
    // The accept tool's compile path can never produce invalid bytes, so
    // the command layer's invalid-payload refusal is reached through
    // `dispatch` directly — the same call `build_accept_response` makes
    // for a staged accept.
    let (_cache, session) = make_state();
    {
        let mut guard = session.lock().unwrap();
        let mut host = RuntimeHost::new(compile_container(COUNTER_PROGRAM)).unwrap();
        host.permit_execution();
        guard.host = Some(host);
    }

    let mut guard = session.lock().unwrap();
    let resp = dispatch(
        Command::AcceptEdits {
            program: vec![1, 2, 3],
            edit: None,
            migration: BTreeMap::new(),
        },
        &mut guard,
        false,
    );

    assert!(!resp.ok);
    assert_eq!(resp.v_code.as_deref(), Some("V4016"));
}
